using System;
using System.IO;
using UnityEngine;
using ProjectNyx.Companion;
using ProjectNyx.Gameplay;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Simulador de Salón en PC + HUD Táctico LainOS y Capturador para el Vídeo Demo.
    /// Permite depurar y grabar el juego en Unity Editor sin visor, simulando entradas y passthrough.
    /// ---
    /// [EN] Desktop Room Simulator + LainOS Tactical HUD & Demo Video Capture Rig.
    /// Enables debugging and recording directly within the Unity Editor without a headset, simulating XR inputs and passthrough.
    /// </summary>
    public class DesktopSimulationHUD : MonoBehaviour
    {
        [Header("Controles de Cámara en PC")]
        [SerializeField] private float moveSpeed = 2.8f;
        [SerializeField] private float lookSensitivity = 2.2f;
        [SerializeField] private GameObject holodeckPreviewEnvironment;

        private Camera mainCam;
        private Transform playerRoot;
        private float yaw;
        private float pitch;
        private QuantumCenturionBossAI bossRef;
        private NyxAdaptationEngine nyxRef;
        private HolographicGauntlet gauntletRef;
        private HoloShieldController shieldRef;
        private MRUKEncounterSpawner spawnerRef;
        private CyberRoomDecorator3D decoratorRef;

        private GUIStyle headerStyle;
        private GUIStyle subHeaderStyle;
        private GUIStyle boxStyle;
        private Texture2D darkTex;
        private Texture2D cyanTex;
        private Texture2D magentaTex;

        private void Awake()
        {
            EnsurePlayerRoot();
        }

        private void EnsurePlayerRoot()
        {
            if (playerRoot != null) { if (playerRoot.GetComponent<XRPlayerLocomotion>() == null) playerRoot.gameObject.AddComponent<XRPlayerLocomotion>(); return; }
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            if (mainCam.transform.parent != null)
            {
                playerRoot = mainCam.transform.parent;
            }
            else
            {
                GameObject rootObj = GameObject.Find("Player_Rig_Root");
                if (rootObj == null)
                {
                    rootObj = new GameObject("Player_Rig_Root");
                    rootObj.transform.position = Vector3.zero;
                    rootObj.transform.rotation = Quaternion.identity;
                }
                playerRoot = rootObj.transform;

                // Emparentar la camara preservando su posicion en el mundo
                mainCam.transform.SetParent(playerRoot, true);

                // Emparentar tambien las manos para que viajen juntas con el jugador
                GameObject rHand = GameObject.Find("RightHand_Arsenal");
                if (rHand != null) rHand.transform.SetParent(playerRoot, true);

                GameObject lHand = GameObject.Find("LeftHand_HoloShield");
                if (lHand != null) lHand.transform.SetParent(playerRoot, true);
            }
        }

        private void Start()
        {
            EnsurePlayerRoot();
            if (mainCam != null)
            {
                Vector3 euler = mainCam.transform.eulerAngles;
                yaw = euler.y;
                pitch = euler.x;
            }

            // En el visor físico Android Quest 3S, ocultamos la rejilla de simulación de PC
            // para que se vea el salón real por Passthrough. En PC Editor la dejamos activa.
            if (holodeckPreviewEnvironment != null)
            {
                bool isQuestDevice = Application.platform == RuntimePlatform.Android && !Application.isEditor;
                holodeckPreviewEnvironment.SetActive(!isQuestDevice);
            }
        }

        private void Update()
        {
            if (mainCam == null) mainCam = Camera.main;

            // Movimiento libre táctico con WASD, Shift para correr, Espacio/C para subir/bajar y ratón en PC
            if (Application.isEditor && mainCam != null)
            {
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");

                if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) v += 1f;
                if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) v -= 1f;

                h = Mathf.Clamp(h, -1f, 1f);
                v = Mathf.Clamp(v, -1f, 1f);

                float upDown = 0f;
                if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E)) upDown += 1f;
                if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl)) upDown -= 1f;

                // Velocidad ágil: 5.5 m/s base, 12.0 m/s con Shift
                float speed = Input.GetKey(KeyCode.LeftShift) ? 12.0f : 5.5f;

                Vector3 forward = mainCam.transform.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f) forward.Normalize();
                else forward = Vector3.forward;

                Vector3 right = mainCam.transform.right;
                right.y = 0f;
                if (right.sqrMagnitude > 0.001f) right.Normalize();
                else right = Vector3.right;

                Vector3 moveDelta = (forward * v + right * h + Vector3.up * upDown) * (speed * Time.deltaTime);

                // Soporte para rueda del ratón (desplazamiento rápido hacia adelante/atrás)
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    moveDelta += mainCam.transform.forward * (scroll * 15f);
                }

                EnsurePlayerRoot();

                // Mover PlayerRoot para que Unity XR no sobreescriba la posicion en el mundo
                if (playerRoot != null)
                {
                    playerRoot.position += moveDelta;
                }
                else
                {
                    mainCam.transform.position += moveDelta;
                }

                // Giro con teclas Q/R
                if (Input.GetKey(KeyCode.Q)) yaw -= 75f * Time.deltaTime;
                if (Input.GetKey(KeyCode.R)) yaw += 75f * Time.deltaTime;

                // Giro con clic derecho o botón central del ratón
                if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
                {
                    yaw += Input.GetAxis("Mouse X") * lookSensitivity * 2.5f;
                    pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * lookSensitivity * 2.5f, -60f, 60f);
                }

                if (playerRoot != null)
                {
                    playerRoot.rotation = Quaternion.Euler(0f, yaw, 0f);
                    mainCam.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
                }
                else
                {
                    mainCam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
                }
            }

            // Captura directa para el Blog / Historial con F12
            if (Input.GetKeyDown(KeyCode.F12))
            {
                CaptureBlogScreenshot();
            }
        }

        public void CaptureBlogScreenshot()
        {
            string dir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Capturas_Blog_Hackathon");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string fileName = $"WiredNexusMR_Blog_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string fullPath = Path.Combine(dir, fileName);
            ScreenCapture.CaptureScreenshot(fullPath, 1);
            Debug.Log($"[DesktopSimulationHUD] ¡Captura para el blog guardada en: {fullPath}!");
        }

        private void InitStyles()
        {
            if (headerStyle != null) return;

            darkTex = MakeColorTex(new Color(0.04f, 0.05f, 0.08f, 0.85f));
            cyanTex = MakeColorTex(new Color(0.0f, 1.0f, 0.8f, 0.9f));
            magentaTex = MakeColorTex(new Color(1.0f, 0.0f, 0.5f, 0.9f));

            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = darkTex;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            headerStyle.normal.textColor = new Color(0f, 1f, 0.8f, 1f);

            subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal
            };
            subHeaderStyle.normal.textColor = new Color(0.78f, 0.84f, 0.90f, 1f);
        }

        private Texture2D MakeColorTex(Color col)
        {
            Texture2D tex = new Texture2D(2, 2);
            Color[] pix = new Color[4];
            for (int i = 0; i < 4; i++) pix[i] = col;
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        private void OnGUI()
        {
            InitStyles();

            if (bossRef == null) bossRef = FindAnyObjectByType<QuantumCenturionBossAI>();
            if (nyxRef == null) nyxRef = FindAnyObjectByType<NyxAdaptationEngine>();
            if (gauntletRef == null) gauntletRef = FindAnyObjectByType<HolographicGauntlet>();
            if (shieldRef == null) shieldRef = FindAnyObjectByType<HoloShieldController>();
            if (spawnerRef == null) spawnerRef = FindAnyObjectByType<MRUKEncounterSpawner>();
            if (decoratorRef == null) decoratorRef = FindAnyObjectByType<CyberRoomDecorator3D>();

            // Panel Superior Izquierdo: Radio Chipset & Telemetría
            GUI.Box(new Rect(16, 16, 370, 74), GUIContent.none, boxStyle);
            GUI.DrawTexture(new Rect(16, 16, 370, 3), cyanTex);
            GUI.Label(new Rect(28, 24, 350, 24), "📻 RADIO CHIPSET 107.7 FM // ON AIR: MINA & KIRA", headerStyle);
            string weaponName = gauntletRef != null ? gauntletRef.CurrentWeapon.ToString().ToUpper() : "BLASTER";
            float shieldPct = shieldRef != null ? Mathf.RoundToInt(shieldRef.EnergyRatio * 100f) : 100f;
            string vrState = (decoratorRef != null && decoratorRef.IsFullVrActive) ? "VR INMERSIVO" : "AR SALON";
            GUI.Label(new Rect(28, 48, 350, 22), $"ARMA: [{weaponName}] | ESCUDO: {shieldPct}% | MODO: {vrState}", subHeaderStyle);
            string nyxInfo = nyxRef != null ? $"NYX: [{nyxRef.ActivityLevel}] ([N] Actividad | [T] 3D)" : "NYX: ACTIVA";
            GUI.Label(new Rect(28, 66, 370, 20), $"IA: BMAX (192.168.0.29) | {nyxInfo}", subHeaderStyle);

            // Panel Superior Central: Barra de Vida del Boss o Fase de la Brecha Interdimensional
            float bw = 460f;
            float bx = (Screen.width - bw) * 0.5f;
            if (bossRef != null && bossRef.gameObject.activeInHierarchy && !bossRef.IsDead)
            {
                GUI.Box(new Rect(bx, 16, bw, 56), GUIContent.none, boxStyle);
                float ratio = Mathf.Clamp01((float)bossRef.CurrentHealth / Mathf.Max(1, bossRef.MaxHealth));
                string climaxTag = ratio <= 0.35f ? " [¡CLIMAX <=35%!]" : "";
                GUI.Label(new Rect(bx + 14, 20, bw - 28, 22), $"⚠️ BOSS: QUANTUM CENTURION ({bossRef.CurrentHealth}/{bossRef.MaxHealth} HP){climaxTag}", headerStyle);

                GUI.DrawTexture(new Rect(bx + 14, 46, bw - 28, 14), darkTex);
                GUI.DrawTexture(new Rect(bx + 14, 46, (bw - 28) * ratio, 14), ratio <= 0.35f ? cyanTex : magentaTex);
            }
            else if (spawnerRef != null)
            {
                GUI.Box(new Rect(bx, 16, bw, 52), GUIContent.none, boxStyle);
                GUI.DrawTexture(new Rect(bx, 16, bw, 3), magentaTex);
                GUI.Label(new Rect(bx + 14, 22, bw - 28, 22), $"⚡ FASE: {spawnerRef.CurrentPhase} (Enemigos: {spawnerRef.ActiveEnemyCount})", headerStyle);
                GUI.Label(new Rect(bx + 14, 42, bw - 28, 20), "Dispara a los Cofres (+) o pulsa [E] -> Brecha -> Oleada 1 -> 3s -> Oleada 2 -> Boss", subHeaderStyle);
            }

            // Panel Inferior Izquierdo: Guía de Controles en PC
            GUI.Box(new Rect(16, Screen.height - 68, 640, 52), GUIContent.none, boxStyle);
            GUI.DrawTexture(new Rect(16, Screen.height - 68, 640, 2), cyanTex);
            GUI.Label(new Rect(28, Screen.height - 62, 620, 22), "CONTROLES: [Clic Der / Q / Grip] APUNTAR (Activa Retícula + Predicción) | [Clic Izq] Disparo", subHeaderStyle);
            GUI.Label(new Rect(28, Screen.height - 42, 620, 22), "[WASD] Mover | [Shift] Correr | [Espacio/C] Subir/Bajar | [Clic Der] Mirar | [Tab] Arma | [B] Boss", subHeaderStyle);
        }
    }
}
