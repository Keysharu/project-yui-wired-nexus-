using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Resolvedor de URLs de Streaming en Directo de YouTube para Unity VideoPlayer.
    /// Extrae enlaces directos de reproducción para proyectar retransmisiones en la pantalla virtual.
    /// ---
    /// [EN] YouTube Live Streaming URL Resolver for Unity VideoPlayer.
    /// Resolves direct video playback endpoints to stream live broadcasts onto the in-game floating screen.
    /// </summary>
    public class YouTubeLiveStreamResolver : MonoBehaviour
    {
        [Header("Configuración de Emisión YouTube Live")]
        [Tooltip("URL pública de la emisión en directo de YouTube")]
        [SerializeField] private string youtubeLiveUrl = "https://www.youtube.com/live/j_bLVMldHsA?is=G6wSXMPCZ4bmVci9";
        
        [Tooltip("ID de vídeo de YouTube extraído automáticamente (o manual)")]
        [SerializeField] private string explicitVideoId = "j_bLVMldHsA";

        [Header("Servidor Local BMAX (LAN / Red del Taller)")]
        [Tooltip("Endpoint del resolver en el servidor BMAX para resolución instantánea")]
        [SerializeField] private string bmaxResolverEndpoint = "http://192.168.0.29:8554/live_hls";
        [Tooltip("Tiempo de espera para intentar conectar con BMAX antes de resolver vía web (segundos)")]
        [SerializeField] private float bmaxTimeoutSeconds = 2.0f;

        [Header("Parámetros de Reproducción y Resiliencia")]
        [Tooltip("Intervalo de comprobación y refresco de stream si se pierde la conexión (segundos)")]
        [SerializeField] private float reconnectIntervalSeconds = 12.0f;
        [Tooltip("Archivo de respaldo en StreamingAssets en caso de corte total de conexión")]
        [SerializeField] private string offlineFallbackVideo = "mits_studio_lofi_100s.mp4";

        [Header("Telemetría de Estado")]
        [SerializeField] private bool isStreamActive = false;
        [SerializeField] private string currentStreamUrl = "";
        [SerializeField] private string lastResolutionMethod = "Ninguno";

        private VideoPlayer targetVideoPlayer;
        private Coroutine resolutionCoroutine;
        private Coroutine reconnectMonitorCoroutine;

        public bool IsStreamActive => isStreamActive;
        public string CurrentStreamUrl => currentStreamUrl;
        public string YouTubeLiveUrl => youtubeLiveUrl;

        private void Awake()
        {
            targetVideoPlayer = GetComponent<VideoPlayer>();
        }

        private void Start()
        {
            if (targetVideoPlayer == null)
            {
                targetVideoPlayer = GetComponent<VideoPlayer>();
            }

            if (targetVideoPlayer != null && !isStreamActive && resolutionCoroutine == null)
            {
                ConnectToLiveStream(targetVideoPlayer);
            }
        }

        /// <summary>
        /// Inicia la resolución y conexión de la emisión de YouTube al VideoPlayer indicado.
        /// </summary>
        public void ConnectToLiveStream(VideoPlayer player, Action<string> onConnected = null)
        {
            if (player != null)
            {
                targetVideoPlayer = player;
            }

            if (resolutionCoroutine != null)
            {
                StopCoroutine(resolutionCoroutine);
            }

            resolutionCoroutine = StartCoroutine(ResolveAndPlayRoutine(onConnected));

            if (reconnectMonitorCoroutine == null && gameObject.activeInHierarchy)
            {
                reconnectMonitorCoroutine = StartCoroutine(ReconnectMonitorRoutine());
            }
        }

        /// <summary>
        /// Cambia dinámicamente la URL de YouTube y reconecta la señal.
        /// </summary>
        public void SetYouTubeUrl(string newUrl, bool autoConnect = true)
        {
            if (string.IsNullOrEmpty(newUrl)) return;
            youtubeLiveUrl = newUrl;
            explicitVideoId = ExtractVideoId(newUrl);
            if (autoConnect && targetVideoPlayer != null)
            {
                ConnectToLiveStream(targetVideoPlayer);
            }
        }

        private IEnumerator ResolveAndPlayRoutine(Action<string> onConnected)
        {
            string resolvedUrl = null;
            string videoId = string.IsNullOrEmpty(explicitVideoId) ? ExtractVideoId(youtubeLiveUrl) : explicitVideoId;

            Debug.Log($"[YouTubeResolver] 🛰️ Iniciando sintonización de YouTube Live (VideoID: {videoId})...");

            // --- CAPA 1: Intentar resolver a través del micro-servicio local BMAX ---
            yield return StartCoroutine(TryResolveFromBMaxLAN(bmaxResolverEndpoint, (url) => {
                resolvedUrl = url;
                lastResolutionMethod = "BMAX_LAN";
            }));

            // --- CAPA 2: Si BMAX no respondió o estamos fuera de la LAN, resolver vía web ---
            if (string.IsNullOrEmpty(resolvedUrl))
            {
                Debug.Log("[YouTubeResolver] 🌐 BMAX no disponible en LAN. Intentando extracción web directa...");
                yield return StartCoroutine(TryResolveFromYouTubeWeb(videoId, (url) => {
                    resolvedUrl = url;
                    lastResolutionMethod = "YOUTUBE_WEB_SCRAPE";
                }));
            }

            // --- CAPA 3: Si se resolvió un stream HLS válido, reproducirlo ---
            if (!string.IsNullOrEmpty(resolvedUrl))
            {
                Debug.Log($"[YouTubeResolver] ✅ Señal de YouTube en directo sintonizada vía [{lastResolutionMethod}]!");
                currentStreamUrl = resolvedUrl;
                isStreamActive = true;

                if (targetVideoPlayer != null)
                {
                    targetVideoPlayer.source = VideoSource.Url;
                    targetVideoPlayer.url = resolvedUrl;
                    targetVideoPlayer.isLooping = false; // El stream live no debe loopear
                    targetVideoPlayer.Prepare();

                    while (!targetVideoPlayer.isPrepared)
                    {
                        yield return null;
                    }

                    targetVideoPlayer.Play();
                }

                onConnected?.Invoke(resolvedUrl);
                yield break;
            }

            // --- CAPA 4 (FALLBACK RESILIENTE): Conexión de seguridad para evitar pantalla negra ---
            Debug.LogWarning("[YouTubeResolver] ⚠️ No se pudo resolver la señal en vivo. Activando fallback local resiliente...");
            lastResolutionMethod = "FALLBACK_STREAMING_ASSETS";
            string fallbackPath = System.IO.Path.Combine(Application.streamingAssetsPath, offlineFallbackVideo);

            if (targetVideoPlayer != null)
            {
                targetVideoPlayer.source = VideoSource.Url;
                targetVideoPlayer.url = fallbackPath;
                targetVideoPlayer.isLooping = true;
                targetVideoPlayer.Play();
            }

            onConnected?.Invoke(fallbackPath);
        }

        /// <summary>
        /// Intenta obtener el manifiesto HLS desde el micro-servicio BMAX en LAN.
        /// </summary>
        private IEnumerator TryResolveFromBMaxLAN(string endpoint, Action<string> onResult)
        {
            if (string.IsNullOrEmpty(endpoint)) yield break;

            using (UnityWebRequest req = UnityWebRequest.Get(endpoint))
            {
                req.certificateHandler = new BypassCertificateHandler();
                req.timeout = Mathf.RoundToInt(bmaxTimeoutSeconds);
                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    string text = req.downloadHandler.text;
                    // Puede responder JSON {"hls_url":"https://..."} o directamente la URL
                    if (text.Contains("hls_url"))
                    {
                        Match match = Regex.Match(text, "\"hls_url\"\\s*:\\s*\"([^\"]+)\"");
                        if (match.Success)
                        {
                            onResult?.Invoke(match.Groups[1].Value.Replace("\\/", "/"));
                            yield break;
                        }
                    }
                    else if (text.StartsWith("http") && text.Contains(".m3u8"))
                    {
                        onResult?.Invoke(text.Trim());
                        yield break;
                    }
                }
            }
        }

        /// <summary>
        /// [ES] Extrae el manifiesto HLS (.m3u8) directamente desde los datos públicos de la página de YouTube.
        ///      Usa User-Agent de dispositivo móvil/iPad para que YouTube entregue el playlist HLS oficial.
        /// ---
        /// [EN] Extracts HLS manifest (.m3u8) directly from YouTube public stream data.
        ///      Uses mobile/iPad User-Agent to ensure YouTube provides the official HLS stream playlist.
        /// </summary>
        private IEnumerator TryResolveFromYouTubeWeb(string videoId, Action<string> onResult)
        {
            if (string.IsNullOrEmpty(videoId)) yield break;

            string pageUrl = $"https://www.youtube.com/live/{videoId}";
            
            // User-Agents móviles para garantizar entrega de manifiesto HLS (.m3u8) por parte de Google/YouTube
            string[] mobileUserAgents = new string[]
            {
                "Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
                "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Mobile Safari/537.36"
            };

            foreach (string ua in mobileUserAgents)
            {
                using (UnityWebRequest req = UnityWebRequest.Get(pageUrl))
                {
                    req.certificateHandler = new BypassCertificateHandler();
                    req.SetRequestHeader("User-Agent", ua);
                    req.timeout = 8;
                    yield return req.SendWebRequest();

                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        string html = req.downloadHandler.text;

                        // 1. Buscar hlsManifestUrl en los datos del reproductor
                        Match hlsMatch = Regex.Match(html, @"hlsManifestUrl[\""']?\s*:\s*[\""']([^\""']+)[\""']");
                        if (hlsMatch.Success)
                        {
                            string rawHls = hlsMatch.Groups[1].Value.Replace("\\/", "/").Replace("\\u0026", "&");
                            onResult?.Invoke(rawHls);
                            yield break;
                        }

                        // 2. Buscar URLs directas de manifest .m3u8 en el código
                        Match m3u8Match = Regex.Match(html, @"(https:[^\""'\s\\]+?\.m3u8[^\""'\s\\]*)");
                        if (m3u8Match.Success)
                        {
                            string rawM3u8 = m3u8Match.Groups[1].Value.Replace("\\/", "/").Replace("\\u0026", "&");
                            onResult?.Invoke(rawM3u8);
                            yield break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Monitorea continuamente la salud del stream y reconecta si el buffer se congela.
        /// </summary>
        private IEnumerator ReconnectMonitorRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(reconnectIntervalSeconds);
            while (true)
            {
                yield return wait;

                if (targetVideoPlayer != null && isStreamActive)
                {
                    // Si el VideoPlayer se detuvo inesperadamente o dio error, relanzar
                    if (!targetVideoPlayer.isPlaying && targetVideoPlayer.isPrepared)
                    {
                        Debug.LogWarning("[YouTubeResolver] 🔄 Stream pausado o finalizado. Reintentando sintonización...");
                        ConnectToLiveStream(targetVideoPlayer);
                    }
                }
            }
        }

        /// <summary>
        /// Extrae el ID de 11 caracteres de cualquier formato de URL de YouTube.
        /// </summary>
        public static string ExtractVideoId(string url)
        {
            if (string.IsNullOrEmpty(url)) return "j_bLVMldHsA";

            Match match = Regex.Match(url, @"(?:live\/|v=|youtu\.be\/|embed\/)([a-zA-Z0-9_-]{11})");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return "j_bLVMldHsA";
        }
    }

    /// <summary>
    /// [ES] Validador resiliente de certificados SSL para evitar Curl error 60 en UnityTLS.
    /// [EN] Resilient SSL certificate handler to prevent Curl error 60 in UnityTLS.
    /// </summary>
    public class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
