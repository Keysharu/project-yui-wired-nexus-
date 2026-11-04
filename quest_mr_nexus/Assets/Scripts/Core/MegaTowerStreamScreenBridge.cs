using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Puente de Vídeo Streaming en Directo de MegaTorre hacia Pantalla Flotante MR.
    /// Recibe y decodifica streams de vídeo para proyectarlos dentro del espacio virtual o mixto.
    /// ---
    /// [EN] MegaTower Live Video Streaming Bridge for Mixed Reality Screens.
    /// Receives and decodes live video streams to project them inside the virtual or mixed reality space.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class MegaTowerStreamScreenBridge : MonoBehaviour
    {
        public enum ScreenFeedSource
        {
            YouTubeLiveStream,  // Emisión en directo de YouTube (Radio Chipset 107.7 FM)
            LocalVideoLoop,     // Clips MP4 renderizados en Google Flow (StreamingAssets)
            SpriteFramesLoop,   // Secuencia de sprites/visemas extraídos
            LiveStreamUrl       // Enlace directo a stream HLS / RTMP / MP4
        }

        [Header("Fuente de Señal de la Megatorre")]
        [SerializeField] private ScreenFeedSource feedSource = ScreenFeedSource.YouTubeLiveStream;
        [Tooltip("URL pública de la emisión en vivo de YouTube (Radio Chipset 107.7 FM)")]
        [SerializeField] private string youtubeLiveUrl = "https://www.youtube.com/live/j_bLVMldHsA?is=G6wSXMPCZ4bmVci9";
        [Tooltip("Ruta relativa en StreamingAssets o URL web (fallback / bucle local)")]
        [SerializeField] private string videoClipNameOrUrl = "mits_studio_lofi_100s.mp4";

        [Header("Perspectiva Dinámica Adaptativa (Altura según Observador)")]
        [Tooltip("Activa el ajuste continuo de altura e inclinación según la posición individual del jugador")]
        [SerializeField] private bool enableDynamicPerspectiveHeight = true;
        [Tooltip("Altura base de la pantalla en la estructura de la Megatorre")]
        [SerializeField] private float baseHeightY = 16.0f;
        [Tooltip("Altura mínima cuando el jugador está a los pies de la torre")]
        [SerializeField] private float minHeightY = 7.5f;
        [Tooltip("Altura máxima cuando el jugador se aleja a media distancia")]
        [SerializeField] private float maxHeightY = 26.0f;
        [Tooltip("Velocidad de ajuste suave para evitar mareos")]
        [SerializeField] private float heightSmoothSpeed = 1.8f;
        [Tooltip("Corrección de inclinación automática hacia la mirada del jugador")]
        [SerializeField] private bool enableDynamicTilt = true;

        [Header("Modo Secuencia de Fotogramas (Fallback / Sprites)")]
        [SerializeField] private Texture2D[] presenterFrames;
        [SerializeField] private float frameRate = 12f;

        [Header("Material y Shader Holográfico")]
        [SerializeField] private string textureProperty = "_MainTex";
        [SerializeField] private string emissionProperty = "_EmissionMap";
        [SerializeField] private Color emissionColor = new Color(0f, 1f, 0.88f);
        [SerializeField] private float emissionIntensity = 2.5f;

        [Header("Efectos de Transmisión Cyberpunk")]
        [SerializeField] private bool enableScanlines = true;
        [SerializeField] private bool enableGlitchOnCombat = true;
        [SerializeField] private float scanlineSpeed = 1.8f;

        private MeshRenderer screenRenderer;
        private Material screenMaterial;
        private VideoPlayer videoPlayer;
        private RenderTexture renderTexture;
        private Transform playerCamera;
        private Vector3 initialTowerLocalPos;
        private YouTubeLiveStreamResolver youtubeResolver;
        private float frameTimer;
        private int currentFrameIndex;
        private bool isGlitching;

        private void Awake()
        {
            screenRenderer = GetComponent<MeshRenderer>();
            if (screenRenderer != null)
            {
                screenMaterial = screenRenderer.material;
            }

            initialTowerLocalPos = transform.localPosition;
            SetupVideoPlayer();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }

            ApplyFeedConfiguration();
        }

        private void Update()
        {
            if (playerCamera == null)
            {
                if (Camera.main != null) playerCamera = Camera.main.transform;
            }

            if (enableDynamicPerspectiveHeight && playerCamera != null)
            {
                UpdateDynamicPerspectiveHeight();
            }

            if (feedSource == ScreenFeedSource.SpriteFramesLoop && presenterFrames != null && presenterFrames.Length > 0)
            {
                UpdateSpriteFrames();
            }

            UpdateScanlines();
        }

        /// <summary>
        /// Ajusta dinámicamente la elevación e inclinación de la pantalla en la torre
        /// según la posición tridimensional y altura de ojos de cada usuario individual.
        /// </summary>
        private void UpdateDynamicPerspectiveHeight()
        {
            Vector3 camPos = playerCamera.position;
            Vector3 screenWorldPos = transform.position;

            // Distancia horizontal (XZ) entre el usuario y la torre
            Vector2 camXZ = new Vector2(camPos.x, camPos.z);
            Vector2 screenXZ = new Vector2(screenWorldPos.x, screenWorldPos.z);
            float horizontalDistance = Vector2.Distance(camXZ, screenXZ);

            // Factor de distancia (0 a los pies, 1 a media distancia de visualización cinematográfica ~40m)
            float distFactor = Mathf.Clamp01(horizontalDistance / 35.0f);

            // Altura objetivo: Si está muy cerca, la pantalla baja para no forzar el cuello hacia arriba.
            // Si el jugador está en una azotea o piso elevado, se compensa sumando la altura del observador.
            float targetY = Mathf.Lerp(minHeightY, maxHeightY, distFactor);
            float userEyeOffset = (camPos.y - 1.6f) * 0.5f; // Compensar usuarios sentados o en plataformas altas
            targetY += userEyeOffset;

            // Interpolación suave de altura
            Vector3 newLocalPos = transform.localPosition;
            newLocalPos.y = Mathf.Lerp(newLocalPos.y, targetY, Time.deltaTime * heightSmoothSpeed);
            transform.localPosition = newLocalPos;

            // Inclinación dinámica hacia el jugador
            if (enableDynamicTilt)
            {
                Vector3 toCam = camPos - transform.position;
                if (toCam.sqrMagnitude > 1.0f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(toCam);
                    // Solo inclinamos en X (Pitch) suavemente, manteniendo la orientación de la torre
                    float pitch = Mathf.Clamp(targetRot.eulerAngles.x, 0f, 35f);
                    Quaternion smoothTilt = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, smoothTilt, Time.deltaTime * heightSmoothSpeed);
                }
            }
        }

        private void SetupVideoPlayer()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            youtubeResolver = GetComponent<YouTubeLiveStreamResolver>();
            if (youtubeResolver == null)
            {
                youtubeResolver = gameObject.AddComponent<YouTubeLiveStreamResolver>();
            }

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;

            AudioSource towerAudio = GetComponent<AudioSource>();
            if (towerAudio != null)
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                videoPlayer.EnableAudioTrack(0, true);
                videoPlayer.SetTargetAudioSource(0, towerAudio);
            }
            else
            {
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // El audio lo gestiona el sistema 3D de la radio
            }

            if (renderTexture == null)
            {
                renderTexture = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32);
                renderTexture.name = "RT_MegaTower_BroadcastScreen";
                renderTexture.Create();
            }

            videoPlayer.targetTexture = renderTexture;

            // Asegurar que el material use un shader válido anti-magenta
            if (screenMaterial != null)
            {
                Shader validShader = Shader.Find("Unlit/Texture") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Sprites/Default");
                if (validShader != null && (screenMaterial.shader == null || !screenMaterial.shader.isSupported || screenMaterial.shader.name.Contains("Error")))
                {
                    screenMaterial.shader = validShader;
                }
                screenMaterial.color = Color.white;
                screenMaterial.SetTexture(textureProperty, renderTexture);
            }

            // Iniciar inmediatamente el vídeo de reserva en StreamingAssets para evitar pantalla negra o magenta
            string immediateFallback = System.IO.Path.Combine(Application.streamingAssetsPath, videoClipNameOrUrl);
            if (System.IO.File.Exists(immediateFallback))
            {
                videoPlayer.url = immediateFallback;
                videoPlayer.Play();
                Debug.Log($"[MegaTower] 🎬 Vídeo de emisión local iniciado de inmediato: {videoClipNameOrUrl}");
            }
        }

        /// <summary>
        /// Devuelve el RenderTexture activo donde se está renderizando la emisión de YouTube Live.
        /// Permite que la pantalla flotante de pared en AR comparta la misma textura sin decodificación redundante.
        /// </summary>
        public RenderTexture GetBroadcastRenderTexture()
        {
            return renderTexture;
        }

        public void ApplyFeedConfiguration()
        {
            if (screenMaterial == null) return;

            switch (feedSource)
            {
                case ScreenFeedSource.YouTubeLiveStream:
                    screenMaterial.SetTexture(textureProperty, renderTexture);
                    if (screenMaterial.HasProperty(emissionProperty))
                    {
                        screenMaterial.SetTexture(emissionProperty, renderTexture);
                        screenMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                    }
                    if (youtubeResolver != null)
                    {
                        youtubeResolver.SetYouTubeUrl(youtubeLiveUrl);
                        youtubeResolver.ConnectToLiveStream(videoPlayer, (resolvedUrl) => {
                            Debug.Log($"[MegaTower] 📡 Emisión YouTube Live activa en la Mega Torre: {resolvedUrl}");
                        });
                    }
                    break;

                case ScreenFeedSource.LocalVideoLoop:
                case ScreenFeedSource.LiveStreamUrl:
                    string fullPath = videoClipNameOrUrl;
                    if (!videoClipNameOrUrl.StartsWith("http://") && !videoClipNameOrUrl.StartsWith("https://"))
                    {
                        fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoClipNameOrUrl);
                    }

                    videoPlayer.url = fullPath;
                    screenMaterial.SetTexture(textureProperty, renderTexture);
                    if (screenMaterial.HasProperty(emissionProperty))
                    {
                        screenMaterial.SetTexture(emissionProperty, renderTexture);
                        screenMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                    }
                    videoPlayer.Play();
                    break;

                case ScreenFeedSource.SpriteFramesLoop:
                    if (videoPlayer.isPlaying) videoPlayer.Stop();
                    if (presenterFrames != null && presenterFrames.Length > 0)
                    {
                        SetScreenTexture(presenterFrames[0]);
                    }
                    break;
            }
        }

        private void UpdateSpriteFrames()
        {
            frameTimer += Time.deltaTime;
            float interval = 1f / Mathf.Max(1f, frameRate);

            if (frameTimer >= interval)
            {
                frameTimer -= interval;
                currentFrameIndex = (currentFrameIndex + 1) % presenterFrames.Length;
                SetScreenTexture(presenterFrames[currentFrameIndex]);
            }
        }

        private void SetScreenTexture(Texture2D tex)
        {
            if (screenMaterial == null || tex == null) return;
            screenMaterial.SetTexture(textureProperty, tex);
            if (screenMaterial.HasProperty(emissionProperty))
            {
                screenMaterial.SetTexture(emissionProperty, tex);
                screenMaterial.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            }
        }

        private void UpdateScanlines()
        {
            if (!enableScanlines || screenMaterial == null) return;
            if (screenMaterial.HasProperty("_MainTex_ST"))
            {
                float offset = (Time.time * scanlineSpeed) % 1f;
                Vector4 currentSt = screenMaterial.GetVector("_MainTex_ST");
                currentSt.w = offset;
                screenMaterial.SetVector("_MainTex_ST", currentSt);
            }
        }

        /// <summary>
        /// Dispara una interferencia holográfica cuando ocurre un evento de combate en la calle o salón.
        /// </summary>
        public void TriggerCombatGlitch(float duration = 1.2f)
        {
            if (!isGlitching && gameObject.activeInHierarchy)
            {
                StartCoroutine(GlitchRoutine(duration));
            }
        }

        private IEnumerator GlitchRoutine(float duration)
        {
            isGlitching = true;
            float elapsed = 0f;
            Color originalEmission = emissionColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float flicker = UnityEngine.Random.Range(0.4f, 2.2f);
                Color glitchColor = UnityEngine.Random.value > 0.5f ? new Color(1f, 0f, 0.49f) : new Color(0f, 1f, 0.88f);
                screenMaterial.SetColor("_EmissionColor", glitchColor * (emissionIntensity * flicker));
                yield return new WaitForSeconds(0.06f);
            }

            screenMaterial.SetColor("_EmissionColor", originalEmission * emissionIntensity);
            isGlitching = false;
        }

        /// <summary>
        /// Cambia en tiempo real la URL o clip de vídeo de la pantalla (ej. cambio de bloque de noticias a música de Mits).
        /// </summary>
        public void SwitchBroadcastStream(string newUrlOrFileName)
        {
            videoClipNameOrUrl = newUrlOrFileName;
            ApplyFeedConfiguration();
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
