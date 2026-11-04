/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Pantalla Holográfica CyberCrest para Radio Chipset 107.7 FM.
 *      Reproduce la emisión en directo de YouTube (ID: j_bLVMldHsA) en el display holográfico
 *      3D con audio espacializado, textura PBR/Unlit y respaldo continuo de streaming.
 * ---
 * [EN] CyberCrest Holographic Display for Radio Chipset 107.7 FM.
 *      Streams live YouTube broadcast (ID: j_bLVMldHsA) onto the 3D holographic display
 *      featuring spatialized 3D audio, PBR/Unlit material rendering, and resilient streaming fallback.
 * =============================================================================
 */

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using ProjectNyx.Core;

namespace ProjectNyx.AudioRadio
{
    /// <summary>
    /// [ES] Controlador de Pantalla Holográfica CyberCrest para Radio Chipset 107.7 FM.
    /// [EN] CyberCrest Holographic Screen Controller for Radio Chipset 107.7 FM.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    public class HologramCyberCrestDisplay : MonoBehaviour
    {
        [Header("Emisión YouTube Live de Radio Chipset 107.7 FM")]
        [Tooltip("[ES] URL de emisión en directo / [EN] Live stream URL")]
        [SerializeField] private string youtubeEmbedUrl = "https://www.youtube.com/embed/j_bLVMldHsA";
        [SerializeField] private string videoId = "j_bLVMldHsA";

        [Header("Componentes Multimedia")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private YouTubeLiveStreamResolver streamResolver;
        [SerializeField] private MeshRenderer screenRenderer;

        [Header("Efectos Holográficos Visuales")]
        [SerializeField] private bool enableHolographicBobbing = true;
        [SerializeField] private float bobbingSpeed = 1.2f;
        [SerializeField] private float bobbingHeight = 0.035f;
        [SerializeField] private bool lookAtPlayer = false;

        [Header("Telemetría de Emisión")]
        [SerializeField] private bool isPlaying = false;
        [SerializeField] private string currentStreamStatus = "Iniciando...";

        private Vector3 initialLocalPos;
        private Transform playerCam;
        private RenderTexture screenRenderTexture;

        public bool IsPlaying => isPlaying;
        public string VideoId => videoId;

        private void Awake()
        {
            // [ES] Desacoplar de jerarquías VR temporales para que la radio suene ininterrumpidamente en combate AR
            // ---
            // [EN] Decouple from temporary VR hierarchies so radio plays uninterrupted during AR combat
            transform.SetParent(null, true);
            initialLocalPos = transform.localPosition;
            if (screenRenderer == null) screenRenderer = GetComponent<MeshRenderer>();
            if (videoPlayer == null) videoPlayer = GetComponent<VideoPlayer>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (streamResolver == null) streamResolver = GetComponent<YouTubeLiveStreamResolver>();

            EnsureComponentsConfigured();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                playerCam = Camera.main.transform;
            }

            // [ES] Si existe el pedestal físico Radio_Chipset_3D en el salón, anclarse flotando sobre él
            // ---
            // [EN] If physical Radio_Chipset_3D pedestal exists in room, anchor floating above it
            GameObject radioPedestal = GameObject.Find("Radio_Chipset_3D");
            if (radioPedestal != null)
            {
                transform.position = radioPedestal.transform.position + new Vector3(0f, 1.40f, 0f);
                initialLocalPos = transform.position;
            }

            // Silenciar cualquier música o radio original para dejar únicamente el streaming en vivo
            if (BattleMusicManager.Instance != null)
            {
                BattleMusicManager.Instance.DisableOriginalMusicForStreaming = true;
                BattleMusicManager.Instance.StopOriginalMusic();
            }
            RadioReceiver3D radio = FindAnyObjectByType<RadioReceiver3D>();
            if (radio != null)
            {
                radio.DisableInternalRadioForStreaming = true;
                radio.StopInternalRadio();
            }

            StartLiveBroadcast();
        }

        private void Update()
        {
            // Efecto suave de flotación holográfica
            if (enableHolographicBobbing)
            {
                float yOffset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y + yOffset, initialLocalPos.z);
            }

            // Orientación suave hacia la jugadora si está activada
            if (lookAtPlayer && playerCam != null)
            {
                Vector3 dir = playerCam.position - transform.position;
                if (dir.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(-dir, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2.5f);
                }
            }

            // Atajo de teclado [M] para silenciar o reanudar audio
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMute();
            }
        }

        /// <summary>
        /// [ES] Asegura que todos los componentes multimedia (VideoPlayer, AudioSource, Resolver)
        /// estén correctamente cableados y configurados.
        /// </summary>
        public void EnsureComponentsConfigured()
        {
            // 1. AudioSource 3D
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 1.0f; // 3D Espacial
            audioSource.playOnAwake = false;
            audioSource.minDistance = 1.5f;
            audioSource.maxDistance = 35.0f;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSource.volume = 0f;
            audioSource.mute = true;

            // 2. VideoPlayer (Audio deshabilitado para evitar duplicar música con la radio del juego)
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.SetDirectAudioMute(0, true);

            // Crear RenderTexture 16:9 con resolución nítida (1280x720)
            if (screenRenderTexture == null)
            {
                screenRenderTexture = new RenderTexture(1280, 720, 0, RenderTextureFormat.ARGB32)
                {
                    name = "RT_CyberCrest_YouTube_Stream",
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear
                };
                screenRenderTexture.Create();
            }

            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = screenRenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetTargetAudioSource(0, audioSource);

            // Asignar el RenderTexture al material del MeshRenderer
            if (screenRenderer != null)
            {
                Material mat = screenRenderer.sharedMaterial;
                if (mat == null)
                {
                    Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
                    mat = new Material(unlitShader) { name = "Mat_Hologram_CyberCrest_Screen" };
                    screenRenderer.sharedMaterial = mat;
                }

                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", screenRenderTexture);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", screenRenderTexture);
                if (mat.HasProperty("_EmissionMap"))
                {
                    mat.SetTexture("_EmissionMap", screenRenderTexture);
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.white);
                }
                mat.color = Color.white;
            }

            // 3. YouTubeLiveStreamResolver
            if (streamResolver == null)
            {
                streamResolver = gameObject.AddComponent<YouTubeLiveStreamResolver>();
            }
        }

        /// <summary>
        /// [ES] Inicia la sintonización y reproducción del directo de YouTube.
        /// </summary>
        public void StartLiveBroadcast()
        {
            if (streamResolver != null && videoPlayer != null)
            {
                currentStreamStatus = "Conectando con Radio Chipset 107.7 FM...";
                Debug.Log($"[HologramCyberCrestDisplay] Sintonizando emisión YouTube Live: {youtubeEmbedUrl} (ID: {videoId})");
                streamResolver.SetYouTubeUrl($"https://www.youtube.com/live/{videoId}", autoConnect: false);
                streamResolver.ConnectToLiveStream(videoPlayer, OnStreamConnected);
            }
        }

        private void OnStreamConnected(string streamUrl)
        {
            isPlaying = true;
            currentStreamStatus = "Transmitiendo en directo // 107.7 FM";
            Debug.Log($"[HologramCyberCrestDisplay] ★ ¡ÉXITO! Emisión conectada en directo: {streamUrl}");

            // El audio de streaming de YouTube queda silenciado permanentemente: la música suena desde el juego (StandaloneRadioStationChipset)
            if (audioSource != null)
            {
                audioSource.mute = true;
                audioSource.volume = 0f;
                audioSource.Stop();
            }
        }

        public void ToggleMute()
        {
            if (audioSource != null)
            {
                audioSource.mute = !audioSource.mute;
                Debug.Log($"[HologramCyberCrestDisplay] Audio de Radio Chipset {(audioSource.mute ? "Silenciado" : "Activo")}.");
            }
        }

        private void OnDestroy()
        {
            if (screenRenderTexture != null)
            {
                screenRenderTexture.Release();
                Destroy(screenRenderTexture);
            }
        }
    }
}
