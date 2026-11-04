// =============================================================================
// Autora / Author: Raquel D. Torres (El taller de la gatita tech)
// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
// Meta Global AI Developer Hackathon 2026
// =============================================================================
// [ES] Componente de Integracion Tactica para Modelos 3D Procedurales en Realidad Mixta.
//      Asigna mallas FBX, colisionadores convexos y materiales URP a los elementos interactivos del salon.
// ---
// [EN] Tactical Integration Component for Procedural 3D Props in Mixed Reality.
//      Binds FBX meshes, convex physics colliders, and URP materials to interactive physical room objects.
// =============================================================================

using System;
using UnityEngine;
using ProjectNyx.Gameplay;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Integrador de Props 3D y Bulevar NeoEclipse para Realidad Mixta y VR.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] 3D Props and NeoEclipse Boulevard Integrator for Mixed Reality and VR.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    [RequireComponent(typeof(CyberRoomDecorator3D))]
    public class CyberProps3DIntegration : MonoBehaviour
    {
        [Header("Modelos 3D del Entorno AR (Passthrough)")]
        [Tooltip("Malla 3D del parapeto balistico tactico (TacticalCover_BallisticBarrier.fbx)")]
        [SerializeField] private GameObject tacticalCover3DPrefab;

        [Tooltip("Malla 3D del pozo de datos holografico (DataWell_Core3D.fbx)")]
        [SerializeField] private GameObject dataWell3DPrefab;

        [Tooltip("Malla 3D del cofre tactico de suministros (CyberChest_Supply3D.fbx)")]
        [SerializeField] private GameObject cyberChest3DPrefab;

        [Header("Mundo Inmersivo VR Completo")]
        [Tooltip("Malla 3D del bulevar texturizado con megatorre y tiendas (NeoEclipse_Boulevard_Master_VR.fbx)")]
        [SerializeField] private GameObject boulevard3DPrefab;

        [Header("Instancias en Runtime (Solo Lectura)")]
        [SerializeField] private GameObject spawnedTacticalCover;
        [SerializeField] private GameObject spawnedDataWell;
        [SerializeField] private GameObject spawnedBoulevard;

        private CyberRoomDecorator3D roomDecorator;

        private void Awake()
        {
            roomDecorator = GetComponent<CyberRoomDecorator3D>();
            Initialize3DEnvironment();
        }

        /// <summary>
        /// [ES] Inicializa y vincula las mallas 3D de alta fidelidad tactica en la escena.
        /// ---
        /// [EN] Initializes and binds tactical high-fidelity 3D meshes in the scene.
        /// </summary>
        public void Initialize3DEnvironment()
        {
            // 1. Vincular o instanciar el Bulevar Maestro VR
            GameObject existingBoulevard = GameObject.Find("NeoEclipse_Boulevard_Master_VR");
            if (existingBoulevard != null)
            {
                spawnedBoulevard = existingBoulevard;
                spawnedBoulevard.SetActive(true);
            }
            else if (boulevard3DPrefab != null)
            {
                GameObject skylineRoot = GameObject.Find("FullVR_MegacitySkyline_Root");
                Transform parentTransform = skylineRoot != null ? skylineRoot.transform : transform;
                spawnedBoulevard = Instantiate(boulevard3DPrefab, parentTransform);
                spawnedBoulevard.name = "NeoEclipse_Boulevard_Master_VR";
                spawnedBoulevard.transform.localPosition = Vector3.zero;
                spawnedBoulevard.transform.localRotation = Quaternion.identity;
                spawnedBoulevard.transform.localScale = Vector3.one;
                spawnedBoulevard.SetActive(true);
            }

            if (spawnedBoulevard != null)

            {

                ApplyBoulevardTexturesRuntime(spawnedBoulevard);

            }



            // 2. Vincular el pozo de datos AR si existe en la jerarquia
            GameObject existingDataWell = GameObject.Find("DataWell_Core3D");
            if (existingDataWell != null)
            {
                spawnedDataWell = existingDataWell;
            }

            // 3. Vincular el parapeto de cobertura tactica si existe
            GameObject existingCover = GameObject.Find("TacticalCover_BallisticBarrier");
            if (existingCover != null)
            {
                spawnedTacticalCover = existingCover;
            }

            // 4. Vincular cinemática y acompañamiento táctico de Nyx en tiempo real
            InitializeNyxCompanion();
        }

        /// <summary>
        /// [ES] Sincroniza la visibilidad de los props 3D cuando el usuario conmuta entre AR y VR.
        /// ---
        /// [EN] Synchronizes visibility of 3D props when the user switches between AR and VR.
        /// </summary>
        /// <summary>

        /// [ES] Asegura que todos los edificios del Boulevard tengan aplicada la textura de la MegaTorre (MegaTower_Spire).

        /// ---

        /// [EN] Enforces the MegaTower_Spire texture onto all buildings in the Boulevard.

        /// </summary>

        public void ApplyBoulevardTexturesRuntime(GameObject root)

        {

            if (root == null) return;



            Transform junkCube = root.transform.Find("Cube");

            if (junkCube != null) Destroy(junkCube.gameObject);



            Material mat = Resources.Load<Material>("Materials/Mat_Boulevard_MegaTower");

            if (mat == null)

            {

                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Unlit/Texture");

                mat = new Material(shader);

                mat.name = "Mat_Boulevard_Runtime_MegaTower";

            }



            Texture2D texAlbedo = Resources.Load<Texture2D>("Textures/Boulevard/Meshy_AI_MegaTower_Summit_Ques_1103011744_texture");

            Texture2D texMetallic = Resources.Load<Texture2D>("Textures/Boulevard/Meshy_AI_MegaTower_Summit_Ques_1103011744_texture_metallic");



            if (texAlbedo != null)

            {

                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texAlbedo);

                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texAlbedo);

            }

            if (texMetallic != null && mat.HasProperty("_MetallicGlossMap"))

            {

                mat.SetTexture("_MetallicGlossMap", texMetallic);

            }



            foreach (var mr in root.GetComponentsInChildren<MeshRenderer>(true))

            {

                mr.sharedMaterial = mat;

            }

        }



        /// <summary>
        /// [ES] Asegura que Nyx tenga los componentes NyxFloatingCompanion y NyxAdaptationEngine activos.
        /// ---
        /// [EN] Enforces NyxFloatingCompanion and NyxAdaptationEngine on Nyx in the active scene.
        /// </summary>
        private void InitializeNyxCompanion()
        {
            GameObject nyxObj = GameObject.Find("Nyx_Companion_Anime3D");
            if (nyxObj == null) nyxObj = GameObject.Find("Nyx_Billboard_Companion");

            if (nyxObj != null)
            {
                ProjectNyx.Companion.NyxFloatingCompanion companion = nyxObj.GetComponent<ProjectNyx.Companion.NyxFloatingCompanion>();
                if (companion == null)
                {
                    companion = nyxObj.AddComponent<ProjectNyx.Companion.NyxFloatingCompanion>();
                }

                ProjectNyx.Companion.NyxAdaptationEngine adaptation = nyxObj.GetComponent<ProjectNyx.Companion.NyxAdaptationEngine>();
                if (adaptation == null)
                {
                    adaptation = nyxObj.AddComponent<ProjectNyx.Companion.NyxAdaptationEngine>();
                }

                Animator anim = nyxObj.GetComponent<Animator>();
                if (anim != null && anim.runtimeAnimatorController == null)
                {
                    anim.enabled = false;
                }
            }
        }

        public void OnModeSwitched(bool isFullVr)
        {
            if (spawnedBoulevard != null)
            {
                spawnedBoulevard.SetActive(isFullVr);
            }

            if (spawnedDataWell != null)
            {
                spawnedDataWell.SetActive(!isFullVr);
            }

            if (spawnedTacticalCover != null)
            {
                spawnedTacticalCover.SetActive(!isFullVr);
            }
        }
    }
}
