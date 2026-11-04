using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Pantalla Flotante Anclada a la Pared Física en Realidad Mixta (LainOS).
    /// Proyecta vídeo, estado de la red o telemetría sobre una pared real escaneada con MRUK.
    /// ---
    /// [EN] Wall-Mounted Floating Screen for Mixed Reality (LainOS Style).
    /// Projects video feeds, network status, or live telemetry onto a scanned physical wall using MRUK.
    /// </summary>
    public class MRWallFloatingScreen : MonoBehaviour
    {
        [Header("Configuración de Posicionamiento Ergonómico")]
        [Tooltip("Distancia hacia el frente del jugador donde se ancla la pantalla (metros)")]
        [SerializeField] private float wallForwardDistance = 2.8f;
        [Tooltip("Elevación base sobre la altura de los ojos del jugador (metros)")]
        [SerializeField] private float heightAboveEyeLevel = 0.55f;
        [Tooltip("Inclinación suave hacia abajo para visualización natural sin fatiga de cuello")]
        [SerializeField] private float tiltAngleDown = 14f;
        [Tooltip("Velocidad de interpolación suave al reposicionar")]
        [SerializeField] private float positionSmoothSpeed = 2.5f;

        [Header("Dimensiones de Pantalla Panorámica 16:9")]
        [SerializeField] private float screenWidth = 1.9f;
        [SerializeField] private float screenHeight = 1.07f;
        [SerializeField] private float frameBezelThickness = 0.05f;

        [Header("Señal de Vídeo de la Emisora")]
        [Tooltip("Puente de la Mega Torre para compartir la señal de YouTube sin coste de GPU extra")]
        [SerializeField] private MegaTowerStreamScreenBridge streamBridge;
        [Tooltip("URL pública de la emisión en vivo de YouTube (Radio Chipset 107.7 FM)")]
        [SerializeField] private string youtubeLiveUrl = "https://www.youtube.com/live/j_bLVMldHsA?is=G6wSXMPCZ4bmVci9";
        [Tooltip("Vídeo de reserva en StreamingAssets si no hay conexión")]
        [SerializeField] private string offlineFallbackVideo = "mits_studio_lofi_100s.mp4";

        [Header("Estética Cyberpunk LainOS")]
        [SerializeField] private Color bezelColor = new Color(0.04f, 0.05f, 0.07f, 1f);
        [SerializeField] private Color neonCyanAccent = new Color(0f, 1f, 0.88f, 1f);
        [SerializeField] private Color neonMagentaAccent = new Color(1f, 0f, 0.49f, 1f);

        private Transform playerCamera;
        private GameObject screenDisplayQuad;
        private GameObject frameObject;
        private Material screenMaterial;
        private Material frameMaterial;
        private Material neonMaterial;
        private VideoPlayer localVideoPlayer;
        private bool isAnchored;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private void Start()
        {
            // [ES] Desactivar en VR Inmersivo para no tapar el bulevar ni la reticula
            CyberRoomDecorator3D dec = FindAnyObjectByType<CyberRoomDecorator3D>();
            if (dec != null && dec.IsFullVrActive)
            {
                gameObject.SetActive(false);
                return;
            }

            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
            }

            // Buscar puente de streaming existente en la escena
            if (streamBridge == null)
            {
                streamBridge = FindAnyObjectByType<MegaTowerStreamScreenBridge>();
            }

            BuildProceduralScreenMesh();
            SetupVideoDisplay();
            RecalculateWallAnchor(true);
        }

        private void Update()
        {
            if (playerCamera == null)
            {
                if (Camera.main != null) playerCamera = Camera.main.transform;
                else return;
            }

            UpdateDynamicErgonomicTracking();
        }

        /// <summary>
        /// Construye proceduralmente el marco cyberpunk y el panel 16:9 de la pantalla flotante.
        /// </summary>
        private void BuildProceduralScreenMesh()
        {
            // 1. Marco exterior oscuro
            frameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameObject.name = "CyberFrame_Backing";
            frameObject.transform.SetParent(transform, false);
            frameObject.transform.localScale = new Vector3(screenWidth + frameBezelThickness * 2f, screenHeight + frameBezelThickness * 2f, 0.04f);

            // Shader URP o Unlit para el marco
            Shader standardShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Color");
            frameMaterial = new Material(standardShader);
            frameMaterial.color = bezelColor;
            Renderer frameRenderer = frameObject.GetComponent<Renderer>();
            if (frameRenderer != null) frameRenderer.material = frameMaterial;

            // Eliminar collider para no interferir con la física del salón
            Collider frameCol = frameObject.GetComponent<Collider>();
            if (frameCol != null) Destroy(frameCol);

            // 2. Tiras Neón de acento (Borde superior cian, borde inferior magenta)
            CreateNeonStrip("NeonTop_Cyan", new Vector3(0f, (screenHeight / 2f) + (frameBezelThickness / 2f), -0.025f), new Vector3(screenWidth + frameBezelThickness, 0.015f, 0.02f), neonCyanAccent);
            CreateNeonStrip("NeonBottom_Magenta", new Vector3(0f, -(screenHeight / 2f) - (frameBezelThickness / 2f), -0.025f), new Vector3(screenWidth + frameBezelThickness, 0.015f, 0.02f), neonMagentaAccent);

            // 3. Quad 16:9 donde se proyecta la imagen
            screenDisplayQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screenDisplayQuad.name = "BroadcastDisplay_Screen";
            screenDisplayQuad.transform.SetParent(transform, false);
            screenDisplayQuad.transform.localPosition = new Vector3(0f, 0f, -0.022f);
            screenDisplayQuad.transform.localScale = new Vector3(screenWidth, screenHeight, 1f);

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
            screenMaterial = new Material(unlitShader);
            screenMaterial.color = Color.white;
            Renderer quadRenderer = screenDisplayQuad.GetComponent<Renderer>();
            if (quadRenderer != null) quadRenderer.material = screenMaterial;

            Collider quadCol = screenDisplayQuad.GetComponent<Collider>();
            if (quadCol != null) Destroy(quadCol);
        }

        private void CreateNeonStrip(string name, Vector3 localPos, Vector3 localScale, Color emission)
        {
            GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = name;
            strip.transform.SetParent(transform, false);
            strip.transform.localPosition = localPos;
            strip.transform.localScale = localScale;

            Shader unlitColor = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color");
            Material mat = new Material(unlitColor);
            mat.color = emission;
            strip.GetComponent<Renderer>().material = mat;

            Collider c = strip.GetComponent<Collider>();
            if (c != null) Destroy(c);
        }

        private void SetupVideoDisplay()
        {
            // 1. Si el puente de la Mega Torre existe y tiene RenderTexture, compartirlo
            if (streamBridge != null)
            {
                RenderTexture sharedRt = streamBridge.GetBroadcastRenderTexture();
                if (sharedRt != null)
                {
                    screenMaterial.mainTexture = sharedRt;
                    Debug.Log("[MRWallScreen] 📺 Compartiendo RenderTexture de YouTube Live desde la Mega Torre.");
                    return;
                }
            }

            // 2. Si no hay puente activo, levantar VideoPlayer y resolver la emisión de YouTube
            localVideoPlayer = gameObject.GetComponent<VideoPlayer>();
            if (localVideoPlayer == null)
            {
                localVideoPlayer = gameObject.AddComponent<VideoPlayer>();
            }

            localVideoPlayer.playOnAwake = false;
            localVideoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            localVideoPlayer.targetMaterialRenderer = screenDisplayQuad.GetComponent<Renderer>();
            localVideoPlayer.targetMaterialProperty = "_MainTex";

            AudioSource screenAudio = GetComponent<AudioSource>();
            if (screenAudio != null)
            {
                localVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                localVideoPlayer.EnableAudioTrack(0, true);
                localVideoPlayer.SetTargetAudioSource(0, screenAudio);
            }
            else
            {
                localVideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            }

            YouTubeLiveStreamResolver resolver = GetComponent<YouTubeLiveStreamResolver>();
            if (resolver == null)
            {
                resolver = gameObject.AddComponent<YouTubeLiveStreamResolver>();
            }

            resolver.SetYouTubeUrl(youtubeLiveUrl);
            resolver.ConnectToLiveStream(localVideoPlayer, (resolvedUrl) => {
                Debug.Log($"[MRWallScreen] 📡 Pantalla flotante AR conectada a YouTube Live: {resolvedUrl}");
            });
        }

        /// <summary>
        /// Calcula la posición frontal anclada a la pared sobre el jugador según su estatura actual.
        /// </summary>
        public void RecalculateWallAnchor(bool snapImmediately = false)
        {
            if (playerCamera == null) return;

            Vector3 camPos = playerCamera.position;
            Vector3 camForward = playerCamera.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude < 0.01f) camForward = Vector3.forward;
            camForward.Normalize();

            // Intentar detectar colisión con pared real si hay Scene Mesh (MRUK)
            RaycastHit hit;
            float distance = wallForwardDistance;
            if (Physics.Raycast(camPos, camForward, out hit, 6.0f))
            {
                distance = Mathf.Clamp(hit.distance - 0.08f, 1.8f, 5.0f);
            }

            // Altura adaptativa: Altura de ojos del jugador + offset ergonómico
            float targetHeight = camPos.y + heightAboveEyeLevel;
            targetPosition = camPos + (camForward * distance);
            targetPosition.y = targetHeight;

            // Inclinación hacia los ojos del usuario
            Vector3 dirToCam = camPos - targetPosition;
            Quaternion lookRot = Quaternion.LookRotation(dirToCam);
            Vector3 euler = lookRot.eulerAngles;
            // Bloquear rotación en Z y fijar inclinación X natural
            euler.z = 0f;
            targetRotation = Quaternion.Euler(euler.x, euler.y + 180f, 0f);

            if (snapImmediately)
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
                isAnchored = true;
            }
        }

        /// <summary>
        /// Adapta dinámicamente la elevación si el usuario se sienta o se levanta.
        /// </summary>
        private void UpdateDynamicErgonomicTracking()
        {
            if (!isAnchored) return;

            // Comprobar si la altura de los ojos cambió drásticamente (sentarse / levantarse)
            float expectedY = playerCamera.position.y + heightAboveEyeLevel;
            if (Mathf.Abs(targetPosition.y - expectedY) > 0.18f)
            {
                targetPosition.y = expectedY;
            }

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * positionSmoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * positionSmoothSpeed);
        }

        /// <summary>
        /// Cambia el clip de vídeo en pantalla (ej. de noticias a música de Mits).
        /// </summary>
        public void SetVideoClip(string fileName)
        {
            offlineFallbackVideo = fileName;
            if (localVideoPlayer != null)
            {
                string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, offlineFallbackVideo);
                localVideoPlayer.url = videoPath;
                localVideoPlayer.Play();
            }
        }
    }
}
