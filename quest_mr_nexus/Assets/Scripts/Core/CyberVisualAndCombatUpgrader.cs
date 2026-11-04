using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NeoCyber.AR;
using ProjectNyx.Gameplay;
using ProjectNyx.Companion;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Componente Maestro del Plan de Choque para el Efecto WOW (Unity 6 MR en Meta Quest 3S).
    /// Orquesta los materiales PBR de alta fidelidad del Bulevar NeoEclipse, Nyx 3D, Araña Mecha 3D
    /// y Quantum Centurión 3D. Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Master Component of the WOW-Factor Impact Plan (Unity 6 MR on Meta Quest 3S).
    /// Orchestrates high-fidelity PBR materials for NeoEclipse Boulevard, 3D Nyx, 3D Spider Mech,
    /// and 3D Quantum Centurion. Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    [ExecuteAlways]
    public class CyberVisualAndCombatUpgrader : MonoBehaviour
    {
        private Shader pbrShader;
        private Shader unlitShader;

        private Material matBoulevardStreet;
        private Material matBoulevardTechForge;
        private Material matBoulevardNeoRamen;
        private Material matBoulevardMegaTower;

        private Material matNyxHeroine;
        private Material matSpiderPbr;
        private Material matBossPbr;

        private GameObject spiderDrone3DModel;
        private GameObject centurionBoss3DModel;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootSystems()
        {
            if (FindAnyObjectByType<CyberVisualAndCombatUpgrader>() == null)
            {
                GameObject bootObj = new GameObject("CyberVisualSystems_Host");
                bootObj.AddComponent<CyberVisualAndCombatUpgrader>();
                DontDestroyOnLoad(bootObj);
                Debug.Log("[CyberVisualAndCombatUpgrader] AutoBootSystems: Inyectado Host de Sistemas Visuales y Combate 3D.");
            }
        }

        private void Awake()
        {
            InitializeShadersAndMaterials();
            Load3DEnemyModels();
        }

        private void OnEnable()
        {
            InitializeShadersAndMaterials();
            Load3DEnemyModels();
            if (Application.isEditor)
            {
                LevelCameraHorizon();
            }
            CleanupArPrimitives();
            UpgradeExistingBossInstance();
            InjectRealWorldLights();
        }

        private void Start()
        {
            if (Application.isEditor)
            {
                LevelCameraHorizon();
            }
            CleanupArPrimitives();
            UpgradeExistingBossInstance();
            InjectRealWorldLights();
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                UpgradeActiveSpiderDrones();
            }
        }

        /// <summary>
        /// [ES] Corrige cualquier inclinación residual de la cámara principal para que el bulevar inicie 100% horizontal.
        /// ---
        /// [EN] Fixes any residual tilt on the main camera so the boulevard starts 100% level.
        /// </summary>
        public void LevelCameraHorizon()
        {
            if (Application.isEditor && Camera.main != null)
            {
                Vector3 euler = Camera.main.transform.localEulerAngles;
                Camera.main.transform.localRotation = Quaternion.Euler(0f, euler.y, 0f);
            }
        }

        private void InitializeShadersAndMaterials()
        {
            pbrShader = CyberMaterialFactory.GetPbrShader();
            unlitShader = CyberMaterialFactory.GetEmissiveShader();

            Texture2D texStreet = Resources.Load<Texture2D>("Textures/Boulevard/Meshy_AI_Cyberpunk_Street_Dior_1102234751_texture");
            Texture2D texTechForge = Resources.Load<Texture2D>("Textures/Boulevard/Meshy_AI_Modular_Cyberpunk_Fac_1103001207_texture");
            Texture2D texNeoRamen = Resources.Load<Texture2D>("Textures/Boulevard/Meshy_AI_Modular_NeoRamen_Faca_1103002322_texture");
            Texture2D texSpire = Resources.Load<Texture2D>("Textures/Boulevard/Meshy_AI_MegaTower_Summit_Ques_1103011744_texture");

            matBoulevardStreet = CreatePbrMat("Mat_Boulevard_Street_Diorama", Color.white, 0.55f, 0.35f, texStreet);
            matBoulevardTechForge = CreatePbrMat("Mat_Boulevard_TechForge", Color.white, 0.50f, 0.35f, texTechForge);
            matBoulevardNeoRamen = CreatePbrMat("Mat_Boulevard_NeoRamen", Color.white, 0.50f, 0.35f, texNeoRamen);
            matBoulevardMegaTower = CreatePbrMat("Mat_Boulevard_MegaTower", Color.white, 0.60f, 0.45f, texSpire);

            Texture2D texNyx = Resources.Load<Texture2D>("Textures/Characters/nyx_heroine");
            Texture2D texSpider = Resources.Load<Texture2D>("Textures/Characters/spider_drone_texture");
            Texture2D texBoss = Resources.Load<Texture2D>("Textures/Characters/mecha_boss_texture");

            matNyxHeroine = CreatePbrMat("Mat_Nyx_Heroine_3D", Color.white, 0.45f, 0.15f, texNyx);
            matSpiderPbr = CreatePbrMat("Mat_Spider_Mech_3D", Color.white, 0.65f, 0.55f, texSpider);
            matBossPbr = CreatePbrMat("Mat_QuantumCenturion_3D", Color.white, 0.70f, 0.60f, texBoss);
        }

        private void Load3DEnemyModels()
        {
            spiderDrone3DModel = Resources.Load<GameObject>("Models/SpiderDrone_Mecha_3D");
            centurionBoss3DModel = Resources.Load<GameObject>("Models/QuantumCenturion_Boss_3D");
        }

        private Material CreatePbrMat(string name, Color baseColor, float smoothness, float metallic, Texture2D tex)
        {
            Material mat = new Material(pbrShader != null ? pbrShader : Shader.Find("Standard"));
            mat.name = name;
            mat.color = baseColor;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            if (tex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
            }
            return mat;
        }

        /// <summary>
        /// [ES] Oculta los cubos primitivos antiguos y activa el Bulevar 3D real con sus texturas PBR de Meshy.
        /// ---
        /// [EN] Hides legacy primitive cubes and activates the real 3D Boulevard with its Meshy PBR textures.
        /// </summary>
        public void EnsureFullBoulevardActive()
        {
            GameObject vrRoot = GameObject.Find("FullVR_MegacitySkyline_Root");
            if (vrRoot == null) return;

            // Desactivar cualquier primitiva vieja o duplicado antiguo
            foreach (Transform child in vrRoot.transform)
            {
                if (child.name == "NeoEclipse_Boulevard_Master_VR")
                {
                    continue;
                }
                if (child.name == "CyberLights_Boulevard_Group" || child.name == "Celestial_CosmicDome_Instance")
                {
                    continue;
                }
                child.gameObject.SetActive(false);
            }

            Transform masterT = vrRoot.transform.Find("NeoEclipse_Boulevard_Master_VR");
            GameObject boulevard = masterT != null ? masterT.gameObject : null;

            if (boulevard == null)
            {
                GameObject prefab = Resources.Load<GameObject>("Models/NeoEclipse_Boulevard_Master_VR");
                if (prefab != null)
                {
                    boulevard = Instantiate(prefab, vrRoot.transform);
                    boulevard.name = "NeoEclipse_Boulevard_Master_VR";
                    boulevard.transform.localPosition = Vector3.zero;
                    boulevard.transform.localRotation = Quaternion.identity;
                    boulevard.transform.localScale = Vector3.one;
                }
            }

            if (boulevard != null)
            {
                boulevard.SetActive(true);
                boulevard.transform.localPosition = Vector3.zero;
                boulevard.transform.localRotation = Quaternion.identity;
                boulevard.transform.localScale = Vector3.one;
                ApplyBoulevardMaterials(boulevard);
            }
        }

        public void CleanupArPrimitives()
        {
            GameObject previewRoom = GameObject.Find("MR_Holodeck_PreviewRoom");
            if (previewRoom != null)
            {
                string[] oldRigs = new string[] {
                    "Nyx3D_HoloRig", "Boss3D_ArenaRig", "FlowRoom_3D_StageBackdrop",
                    "OpenWiredCeiling_OrbitalRing", "AR_WallCircuitVines_Root",
                    "Floor_DataWell_Root", "TacticalCoverShield_Root"
                };
                foreach (string rigName in oldRigs)
                {
                    Transform t = previewRoom.transform.Find(rigName);
                    if (t != null) t.gameObject.SetActive(false);
                }
            }
        }

        private void ApplyBoulevardMaterials(GameObject root)
        {
            if (root == null) return;

            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                string n = r.gameObject.name.ToLower();
                if (n.Contains("diorama") || n.Contains("street"))
                {
                    if (matBoulevardStreet != null) r.sharedMaterial = matBoulevardStreet;
                }
                else if (n.Contains("techforge"))
                {
                    if (matBoulevardTechForge != null) r.sharedMaterial = matBoulevardTechForge;
                }
                else if (n.Contains("neoramen"))
                {
                    if (matBoulevardNeoRamen != null) r.sharedMaterial = matBoulevardNeoRamen;
                }
                else if (n.Contains("spire") || n.Contains("megatower"))
                {
                    if (matBoulevardMegaTower != null) r.sharedMaterial = matBoulevardMegaTower;
                }
            }
        }

        public void InjectRealWorldLights()
        {
            GameObject vrRoot = GameObject.Find("FullVR_MegacitySkyline_Root");
            Transform parentT = vrRoot != null ? vrRoot.transform : null;

            if (GameObject.Find("CyberLights_Boulevard_Group") != null) return;

            GameObject lightGroup = new GameObject("CyberLights_Boulevard_Group");
            if (parentT != null) lightGroup.transform.SetParent(parentT, false);

            GameObject lightChipset = new GameObject("Light_RadioChipset_Billboard");
            lightChipset.transform.SetParent(lightGroup.transform, false);
            lightChipset.transform.localPosition = new Vector3(0f, 12f, 18f);
            Light l1 = lightChipset.AddComponent<Light>();
            l1.type = LightType.Point;
            l1.color = new Color(1.0f, 0.05f, 0.65f);
            l1.range = 32f;
            l1.intensity = 3.5f;

            float[] zPoles = new float[] { 2f, 8f, 16f, 24f };
            for (int i = 0; i < zPoles.Length; i++)
            {
                GameObject poleR = new GameObject($"StreetLight_Pole_R_{i}");
                poleR.transform.SetParent(lightGroup.transform, false);
                poleR.transform.localPosition = new Vector3(4.5f, 4.2f, zPoles[i]);
                Light lpR = poleR.AddComponent<Light>();
                lpR.type = LightType.Point;
                lpR.color = new Color(0.15f, 0.95f, 1.0f);
                lpR.range = 16f;
                lpR.intensity = 2.4f;

                GameObject poleL = new GameObject($"StreetLight_Pole_L_{i}");
                poleL.transform.SetParent(lightGroup.transform, false);
                poleL.transform.localPosition = new Vector3(-4.5f, 4.2f, zPoles[i]);
                Light lpL = poleL.AddComponent<Light>();
                lpL.type = LightType.Point;
                lpL.color = new Color(0.15f, 0.95f, 1.0f);
                lpL.range = 16f;
                lpL.intensity = 2.4f;
            }
        }

        private void UpgradeActiveSpiderDrones()
        {
            if (spiderDrone3DModel == null) return;

            NeoCyberSpiderDroneAI[] spiders = FindObjectsByType<NeoCyberSpiderDroneAI>(FindObjectsSortMode.None);
            foreach (var spider in spiders)
            {
                SpriteRenderer sr = spider.GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null) sr.gameObject.SetActive(false);

                Transform existing = spider.transform.Find("Spider_3D_Mesh_Model");
                GameObject mesh3D = existing != null ? existing.gameObject : null;
                if (mesh3D == null)
                {
                    mesh3D = Instantiate(spiderDrone3DModel, spider.transform);
                    mesh3D.name = "Spider_3D_Mesh_Model";
                    mesh3D.transform.localPosition = Vector3.zero;
                    mesh3D.transform.localRotation = Quaternion.identity;
                    mesh3D.transform.localScale = Vector3.one * 0.75f;
                }

                if (matSpiderPbr != null)
                {
                    foreach (var smr in mesh3D.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        smr.sharedMaterial = matSpiderPbr;
                    foreach (var mr in mesh3D.GetComponentsInChildren<MeshRenderer>(true))
                        mr.sharedMaterial = matSpiderPbr;
                }
            }
        }

        public void UpgradeExistingBossInstance()
        {
            if (centurionBoss3DModel == null) return;

            QuantumCenturionBossAI boss = FindAnyObjectByType<QuantumCenturionBossAI>(FindObjectsInactive.Include);
            if (boss == null) return;

            SpriteRenderer sr = boss.GetComponentInChildren<SpriteRenderer>(true);
            if (sr != null) sr.gameObject.SetActive(false);

            // Limpiar duplicado antiguo si existiera
            Transform oldDup = boss.transform.Find("Boss_3D_Mesh_Model");
            Transform primary3D = boss.transform.Find("Centurion_Boss_3D_Model");
            if (oldDup != null && primary3D != null)
            {
                oldDup.gameObject.SetActive(false);
            }

            Transform targetT = primary3D != null ? primary3D : oldDup;
            GameObject mesh3D = targetT != null ? targetT.gameObject : null;

            if (mesh3D == null)
            {
                mesh3D = Instantiate(centurionBoss3DModel, boss.transform);
                mesh3D.name = "Centurion_Boss_3D_Model";
                mesh3D.transform.localPosition = Vector3.zero;
                mesh3D.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                mesh3D.transform.localScale = Vector3.one * 1.0f;
            }

            if (matBossPbr != null)
            {
                foreach (var smr in mesh3D.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    smr.sharedMaterial = matBossPbr;
                foreach (var mr in mesh3D.GetComponentsInChildren<MeshRenderer>(true))
                    mr.sharedMaterial = matBossPbr;
            }
        }

        public void BuildCyberCafeArmoryInterior()
        {
            // [ES] Retirado: el interior ahora forma parte de las fachadas 3D modulares de Meshy
            // [EN] Retired: interior is now part of the modular Meshy 3D facades
        }
    }
}
