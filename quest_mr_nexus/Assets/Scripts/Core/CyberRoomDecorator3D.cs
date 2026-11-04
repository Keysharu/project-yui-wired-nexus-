// =============================================================================
// Autora / Author: Raquel D. Torres (El taller de la gatita tech)
// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
// Meta Global AI Developer Hackathon 2026
// =============================================================================
// [ES] Decorador de Habitación Cyberpunk 3D (AR Passthrough + Modo VR Inmersivo Completo).
//      Garantiza el aislamiento absoluto entre el entorno VR (Bulevar) y el entorno AR (Salón físico).
// ---
// [EN] Cyberpunk 3D Room Decorator (AR Passthrough + Full Immersive VR Mode).
//      Guarantees absolute isolation between VR environment (Boulevard) and AR environment (Physical Room).
// =============================================================================

using System;
using UnityEngine;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Controla la alternancia y aislamiento estricto entre el salón físico en AR y el Bulevar en VR.
    /// [EN] Controls toggling and strict isolation between physical room in AR and Boulevard in VR.
    /// </summary>
    public class CyberRoomDecorator3D : MonoBehaviour
    {
        [Header("Nodos de Decoración AR (Salón Híbrido)")]
        [SerializeField] private Transform arWallVinesRoot;
        [SerializeField] private Transform floorDataWellRoot;
        [SerializeField] private Transform openWiredCeilingRing;
        [SerializeField] private Transform tacticalCoverShieldRoot;
        [SerializeField] private GameObject arStageBackdrop;

        [Header("Mundo VR Inmersivo Completo (Bulevar y Megatorre 360)")]
        [SerializeField] private GameObject fullVrSkylineRoot;
        [SerializeField] private Transform vrOrbitalSkyRing;
        [SerializeField] private bool startInFullVrMode = false;

        private bool isFullVrActive;

        public bool IsFullVrActive => isFullVrActive;

        private void Start()
        {
            SetImmersiveVrMode(startInFullVrMode);
        }

        private void Update()
        {
            // [ES] Conmutador táctico AR <-> VR (Tecla V en PC o Botón Y / Click Stick Izquierdo en Meta Quest Touch Plus)
            // [EN] Tactical switch AR <-> VR (Key V on PC or Button Y / Left Stick Click on Meta Quest Touch Plus)
            if (Input.GetKeyDown(KeyCode.V) || 
                OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch) ||
                OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch))
            {
                ToggleVrArMode();
            }

            // Animación del anillo orbital de cielo abierto
            if (openWiredCeilingRing != null)
            {
                openWiredCeilingRing.Rotate(0f, 18f * Time.deltaTime, 0f, Space.World);
            }

            // Animación suave del pozo de datos
            if (floorDataWellRoot != null)
            {
                floorDataWellRoot.Rotate(0f, -12f * Time.deltaTime, 0f, Space.Self);
            }

            // Giro del anillo estelar en modo VR Inmersivo
            if (isFullVrActive && vrOrbitalSkyRing != null)
            {
                vrOrbitalSkyRing.Rotate(0f, 8f * Time.deltaTime, 0f, Space.World);
            }
        }

        public void ToggleVrArMode()
        {
            SetImmersiveVrMode(!isFullVrActive);
        }

        /// <summary>
        /// [ES] Conmuta entre el modo AR (salón físico con Passthrough) y el Modo VR Inmersivo Completo (Bulevar NeoEclipse).
        /// Aísla al 100% las mallas y fondos de ambos mundos para evitar solapamientos en la APK final.
        /// ---
        /// [EN] Toggles between AR mode (physical room via Passthrough) and Full Immersive VR Mode (NeoEclipse Boulevard).
        /// 100% isolates meshes and backgrounds of both worlds preventing any visual bleeding in the final APK.
        /// </summary>
        public void SetImmersiveVrMode(bool enableFullVr)
        {
            isFullVrActive = enableFullVr;

            // 1. Resolver root principal del entorno VR
            if (fullVrSkylineRoot == null)
            {
                fullVrSkylineRoot = GameObject.Find("FullVR_MegacitySkyline_Root");
            }

            if (fullVrSkylineRoot != null)
            {
                fullVrSkylineRoot.SetActive(enableFullVr);
            }

            // 2. Aislamiento Garantizado: Localizar todos los bloques del Bulevar y Megatorre que estén en la escena
            GameObject[] allSceneObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (GameObject obj in allSceneObjects)
            {
                if (obj == null) continue;

                // [ES] Mantener la radio y la pantalla de streaming siempre vivas en combate AR
                // ---
                // [EN] Keep radio and streaming display active during AR combat
                if (obj.GetComponent<HologramCyberCrestDisplay>() != null ||
                    obj.GetComponent<YouTubeLiveStreamResolver>() != null ||
                    obj.GetComponent<RadioReceiver3D>() != null ||
                    obj.name.Contains("CyberCrest") ||
                    obj.name.Contains("Radio_Chipset"))
                {
                    continue;
                }

                string objName = obj.name;

                // Estructuras del Bulevar y Megatorre VR
                if (objName.StartsWith("NeoEclipse_Boulevard") || 
                    objName.StartsWith("VR_Megatower") ||
                    objName.StartsWith("Megatower_Broadcast") ||
                    objName.StartsWith("Megatower_HeaderSign"))
                {
                    obj.SetActive(enableFullVr);
                }

                // Ocultar la habitación simulada en visor Quest físico si estamos en AR
                if (objName.Contains("MR_Holodeck_PreviewRoom"))
                {
                    bool isQuestDevice = Application.platform == RuntimePlatform.Android && !Application.isEditor;
                    if (isQuestDevice)
                    {
                        obj.SetActive(false); // Siempre apagado en Quest real para ver el salón real por Passthrough
                    }
                    else
                    {
                        obj.SetActive(!enableFullVr);
                    }
                }
            }

            // 3. Gestión de props de habitación AR (solo visibles en AR)
            if (arWallVinesRoot != null) arWallVinesRoot.gameObject.SetActive(!enableFullVr);
            if (floorDataWellRoot != null) floorDataWellRoot.gameObject.SetActive(!enableFullVr);
            if (tacticalCoverShieldRoot != null) tacticalCoverShieldRoot.gameObject.SetActive(!enableFullVr);
            if (arStageBackdrop != null) arStageBackdrop.SetActive(!enableFullVr);

            // 4. Notificar al integrador de props 3D
            CyberProps3DIntegration props = GetComponent<CyberProps3DIntegration>();
            if (props != null)
            {
                props.OnModeSwitched(enableFullVr);
            }

            // 5. Configuración de Cámara para Passthrough / Skybox
            if (Camera.main != null)
            {
                Camera.main.clearFlags = enableFullVr ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                if (!enableFullVr)
                {
                    Camera.main.backgroundColor = new Color(0f, 0f, 0f, 0f);
                }
            }

            // 6. Passthrough Manager de Meta Quest 3S
            if (PassthroughManager.Instance != null)
            {
                if (enableFullVr)
                {
                    PassthroughManager.Instance.DisablePassthrough();
                }
                else
                {
                    PassthroughManager.Instance.EnablePassthrough();
                    PassthroughManager.Instance.PulsePassthroughColorTint(new Color(0f, 1f, 0.88f, 1f), 0.8f);
                }
            }

            Debug.Log($"[CyberRoomDecorator3D] ★ MODO VISUAL CAMBIADO: {(enableFullVr ? "VR INMERSIVO COMPLETO (Bulevar NeoEclipse)" : "AR MIXED REALITY (Salón Físico con Passthrough)")}");
        }
    }
}
