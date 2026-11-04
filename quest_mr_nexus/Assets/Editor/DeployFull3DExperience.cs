/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Implementación Integral V4 en Unity: Bulevar Maestro 3D (+Z alineado),
 *      Nyx 3D activa, Araña Mecha 3D activa con rig IK y Mecha Boss 3D activo.
 *      Localiza objetos incluso si están desactivados (m_IsActive: 0), limpia
 *      duplicados y primitivas antiguas y alinea la cámara del Editor.
 * ---
 * [EN] Full Unity Implementation V4: Master 3D Boulevard (+Z aligned),
 *      active 3D Nyx, active 3D Spider Mech with IK rig, and active 3D Mecha Boss.
 *      Finds objects even if inactive (m_IsActive: 0), cleans up duplicates and
 *      legacy primitives, and aligns the Editor SceneView camera.
 * =============================================================================
 */

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectNyx.EditorTools
{
    [InitializeOnLoad]
    public static class DeployFull3DExperience
    {
        private const string ScenePath = "Assets/MainMRScene.unity";
        private const string MaterialsFolder = "Assets/Materials/3D_Meshes";
        private const string DeployStateKey = "ProjectNyx_DeployFull3D_V8_MegaTowerSpireTextures_Applied";

        static DeployFull3DExperience()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (!SessionState.GetBool(DeployStateKey, false))
                    {
                        SessionState.SetBool(DeployStateKey, true);
                        Debug.Log("[DeployFull3DExperience V4] Ejecutando despliegue integral automático (Bulevar V3 + Nyx + Araña + Mecha Boss)...");
                        DeployEverything();
                    }
                }
            };
        }

        [MenuItem("Wired Nexus MR/★ IMPLEMENTAR TODO EN UNITY (Bulevar 3D + Nyx + Araña + Mecha Boss)", false, 0)]
        public static void DeployEverything()
        {
            Debug.Log("====================================================================");
            Debug.Log("[DeployFull3DExperience V4] INICIANDO DESPLIEGUE COMPLETO 3D EN UNITY...");
            Debug.Log("====================================================================");

            try
            {
                // 1. Asegurar directorios de materiales
                if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                {
                    AssetDatabase.CreateFolder("Assets", "Materials");
                }
                if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                {
                    AssetDatabase.CreateFolder("Assets/Materials", "3D_Meshes");
                }

                AssetDatabase.Refresh();

                // 2. Obtener shaders para URP
                Shader pbrShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Shader skyShader = Shader.Find("Skybox/Panoramic") ?? Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");

                // 3. Crear o actualizar Materiales PBR con la ruta solicitada por Raquel: MegaTower_Spire
                string spireBaseTex = "Assets/Models/Boulevard_Optimized_VR/MegaTower_Spire/Meshy_AI_MegaTower_Summit_Ques_1103011744_texture.png";
                string spireMetTex = "Assets/Models/Boulevard_Optimized_VR/MegaTower_Spire/Meshy_AI_MegaTower_Summit_Ques_1103011744_texture_metallic.png";

                Material matStreet = CreatePbrMaterial(
                    "Mat_Boulevard_Street_Diorama",
                    spireBaseTex,
                    spireMetTex,
                    null,
                    pbrShader);

                Material matTechForge = CreatePbrMaterial(
                    "Mat_Boulevard_TechForge",
                    spireBaseTex,
                    spireMetTex,
                    null,
                    pbrShader);

                Material matNeoRamen = CreatePbrMaterial(
                    "Mat_Boulevard_NeoRamen",
                    spireBaseTex,
                    spireMetTex,
                    null,
                    pbrShader);

                Material matSpire = CreatePbrMaterial(
                    "Mat_Boulevard_MegaTower",
                    spireBaseTex,
                    spireMetTex,
                    null,
                    pbrShader);

                Material matNyx = CreatePbrMaterial(
                    "Mat_Nyx_Heroine_3D",
                    "Assets/Resources/Textures/Characters/nyx_heroine.png",
                    null,
                    null,
                    pbrShader);

                Material matSpider = CreatePbrMaterial(
                    "Mat_Spider_Mech_3D",
                    "Assets/Resources/Textures/Characters/spider_drone_texture.png",
                    "Assets/Resources/Textures/Characters/spider_drone_metallic.png",
                    "Assets/Resources/Textures/Characters/spider_drone_normal.png",
                    pbrShader);

                Material matBoss = CreatePbrMaterial(
                    "Mat_QuantumCenturion_3D",
                    "Assets/Resources/Textures/Characters/mecha_boss_texture.png",
                    "Assets/Resources/Textures/Characters/mecha_boss_metallic.png",
                    "Assets/Resources/Textures/Characters/mecha_boss_normal.png",
                    pbrShader);

                Material matSkybox = CreateSkyboxMaterial(
                    "Mat_Cyberspace_Skybox_360",
                    "Assets/Resources/Textures/Environment/skybox_cyberspace_360_panorama.png",
                    skyShader);

                if (matSkybox != null)
                {
                    RenderSettings.skybox = matSkybox;
                }

                AssetDatabase.SaveAssets();

                // 4. Abrir la escena principal
                Scene scene = EditorSceneManager.GetActiveScene();
                if (scene.path != ScenePath)
                {
                    scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }

                // 4b. Asegurar Player_Rig_Root para locomoción sin bloqueo de Unity XR
                GameObject playerRoot = FindInScene(scene, "Player_Rig_Root");
                if (playerRoot == null)
                {
                    playerRoot = new GameObject("Player_Rig_Root");
                    playerRoot.transform.position = Vector3.zero;
                    playerRoot.transform.rotation = Quaternion.identity;
                }
                GameObject camObj = FindInScene(scene, "Main Camera");
                if (camObj != null && camObj.transform.parent != playerRoot.transform)
                {
                    camObj.transform.SetParent(playerRoot.transform, true);
                    EditorUtility.SetDirty(camObj);
                }
                GameObject rHandObj = FindInScene(scene, "RightHand_Arsenal");
                if (rHandObj != null && rHandObj.transform.parent != playerRoot.transform)
                {
                    rHandObj.transform.SetParent(playerRoot.transform, true);
                    EditorUtility.SetDirty(rHandObj);
                }
                GameObject lHandObj = FindInScene(scene, "LeftHand_HoloShield");
                if (lHandObj != null && lHandObj.transform.parent != playerRoot.transform)
                {
                    lHandObj.transform.SetParent(playerRoot.transform, true);
                    EditorUtility.SetDirty(lHandObj);
                }

                // -------------------------------------------------------------
                // 5. LIMPIAR PRIMITIVAS Y ANILLOS ANTIGUOS DE MR_Holodeck_PreviewRoom
                // -------------------------------------------------------------
                GameObject previewRoom = FindInScene(scene, "MR_Holodeck_PreviewRoom");
                if (previewRoom != null)
                {
                    previewRoom.SetActive(true);
                    string[] legacyRigNames = new string[] {
                        "Nyx3D_HoloRig", "Boss3D_ArenaRig", "FlowRoom_3D_StageBackdrop",
                        "OpenWiredCeiling_OrbitalRing", "AR_WallCircuitVines_Root",
                        "Floor_DataWell_Root", "TacticalCoverShield_Root",
                        "DimensionalRift_Portal3D", "CyberSupplyChest_1", "CyberSupplyChest_2", "CyberSupplyChest_3"
                    };
                    foreach (string oldName in legacyRigNames)
                    {
                        Transform t = previewRoom.transform.Find(oldName);
                        if (t != null) t.gameObject.SetActive(false);
                    }
                }

                // -------------------------------------------------------------
                // 6. IMPLEMENTAR EL BULEVAR MAESTRO V3 EN FullVR_MegacitySkyline_Root
                // -------------------------------------------------------------
                GameObject vrRoot = FindInScene(scene, "FullVR_MegacitySkyline_Root");
                if (vrRoot == null)
                {
                    vrRoot = new GameObject("FullVR_MegacitySkyline_Root");
                    if (previewRoom != null) vrRoot.transform.SetParent(previewRoom.transform, false);
                }
                vrRoot.SetActive(true);

                // Desactivar primitivas viejas y eliminar instancias previas del bulevar para re-instanciar limpio el V3
                for (int i = vrRoot.transform.childCount - 1; i >= 0; i--)
                {
                    Transform child = vrRoot.transform.GetChild(i);
                    if (child.name == "NeoEclipse_Boulevard_Master_VR" ||
                        child.name == "NeoEclipse_Boulevard_3D_Instance" ||
                        child.name == "CyberCafe_Armory_Interior" ||
                        child.name == "Fontaine_Canals_Root")
                    {
                        UnityEngine.Object.DestroyImmediate(child.gameObject);
                    }
                    else if (child.name != "CyberLights_Boulevard_Group" && child.name != "Celestial_CosmicDome_Instance")
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                GameObject masterFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/NeoEclipse_Boulevard_Master_VR.fbx");
                if (masterFbx != null)
                {
                    GameObject masterBoulevardObj = (GameObject)PrefabUtility.InstantiatePrefab(masterFbx, vrRoot.transform);
                    masterBoulevardObj.name = "NeoEclipse_Boulevard_Master_VR";
                    masterBoulevardObj.transform.localPosition = Vector3.zero;
                    masterBoulevardObj.transform.localRotation = Quaternion.identity;
                    masterBoulevardObj.transform.localScale = Vector3.one;
                    masterBoulevardObj.SetActive(true);

                    ApplyMaterialToChild(masterBoulevardObj, "Boulevard_Street_Diorama", matStreet);
                    ApplyMaterialToChild(masterBoulevardObj, "Boulevard_TechForge_Facade_L", matTechForge);
                    ApplyMaterialToChild(masterBoulevardObj, "Boulevard_NeoRamen_Facade_R", matNeoRamen);
                    ApplyMaterialToChild(masterBoulevardObj, "Boulevard_MegaTower_Spire", matSpire);

                    // Eliminar el cubo por defecto de Meshy AI si existe
                    Transform junkCube = masterBoulevardObj.transform.Find("Cube");
                    if (junkCube != null) UnityEngine.Object.DestroyImmediate(junkCube.gameObject);

                    // Aplicar el material de la MegaTorre a absolutamente todos los edificios del Boulevard
                    foreach (var mr in masterBoulevardObj.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        if (mr.name.Equals("Boulevard_Street_Diorama", StringComparison.OrdinalIgnoreCase)) mr.sharedMaterial = matStreet;
                        else if (mr.name.Equals("Boulevard_TechForge_Facade_L", StringComparison.OrdinalIgnoreCase)) mr.sharedMaterial = matTechForge;
                        else if (mr.name.Equals("Boulevard_NeoRamen_Facade_R", StringComparison.OrdinalIgnoreCase)) mr.sharedMaterial = matNeoRamen;
                        else mr.sharedMaterial = matSpire;
                        EditorUtility.SetDirty(mr);
                    }

                    Debug.Log("[DeployFull3DExperience V4] ¡Bulevar NeoEclipse Maestro V3 instanciado y texturizado!");
                }

                // -------------------------------------------------------------
                // 7. ACTIVAR E IMPLEMENTAR A NYX 3D EN LA ESCENA
                // -------------------------------------------------------------
                GameObject nyxCompanionObj = FindInScene(scene, "Nyx_Companion_Anime3D") ?? FindInScene(scene, "Nyx_Billboard_Companion");
                if (nyxCompanionObj != null)
                {
                    nyxCompanionObj.name = "Nyx_Companion_Anime3D";
                    var comp = nyxCompanionObj.GetComponent<ProjectNyx.Companion.NyxFloatingCompanion>() ?? nyxCompanionObj.AddComponent<ProjectNyx.Companion.NyxFloatingCompanion>();
                    var adapt = nyxCompanionObj.GetComponent<ProjectNyx.Companion.NyxAdaptationEngine>() ?? nyxCompanionObj.AddComponent<ProjectNyx.Companion.NyxAdaptationEngine>();
                    Animator anim = nyxCompanionObj.GetComponent<Animator>();
                    if (anim != null && anim.runtimeAnimatorController == null)
                    {
                        anim.enabled = false;
                    }
                    EditorUtility.SetDirty(nyxCompanionObj);
                }
                if (nyxCompanionObj != null)
                {
                    nyxCompanionObj.SetActive(true);
                    GameObject screen1 = FindInScene(scene, "Megatower_BroadcastScreen");
                    if (screen1 != null) screen1.SetActive(false);
                    GameObject screen2 = FindInScene(scene, "MR_WallFloatingBroadcastScreen");
                    if (screen2 != null) screen2.SetActive(false);
                    nyxCompanionObj.transform.position = new Vector3(-0.75f, 1.35f, 1.75f);
                    nyxCompanionObj.transform.rotation = Quaternion.identity;

                    foreach (var sr in nyxCompanionObj.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        sr.gameObject.SetActive(false);
                    }

                    Transform oldNyx3D = nyxCompanionObj.transform.Find("Nyx_Anime3D_MeshInstance");
                    if (oldNyx3D != null)
                    {
                        UnityEngine.Object.DestroyImmediate(oldNyx3D.gameObject);
                    }

                    GameObject nyxFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Nyx_Companion_Anime3D.fbx");
                    if (nyxFbx != null)
                    {
                        GameObject nyx3D = (GameObject)PrefabUtility.InstantiatePrefab(nyxFbx, nyxCompanionObj.transform);
                        nyx3D.name = "Nyx_Anime3D_MeshInstance";
                        nyx3D.transform.localPosition = Vector3.zero;
                        // Rotar hacia la cámara (jugador en Z = -0.25)
                        nyx3D.transform.localRotation = Quaternion.Euler(0f, 20f, 0f);
                        nyx3D.transform.localScale = Vector3.one * 0.95f;
                        nyx3D.SetActive(true);

                        foreach (var mr in nyx3D.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            if (matNyx != null) mr.sharedMaterial = matNyx;
                            EditorUtility.SetDirty(mr);
                        }
                        foreach (var smr in nyx3D.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        {
                            if (matNyx != null) smr.sharedMaterial = matNyx;
                            EditorUtility.SetDirty(smr);
                        }
                        Debug.Log("[DeployFull3DExperience V4] ¡Nyx 3D Heroína ACTIVADA y texturizada en (-1.35, 0, 2.55)!");
                    }
                }

                // -------------------------------------------------------------
                // 8. ACTIVAR E IMPLEMENTAR EL MECHA BOSS 3D EN LA ESCENA
                // -------------------------------------------------------------
                GameObject bossObj = FindInScene(scene, "QuantumCenturion_Boss_25D");
                if (bossObj != null)
                {
                    bossObj.SetActive(true);
                    bossObj.transform.position = new Vector3(0.0f, 0.0f, 5.20f);
                    bossObj.transform.rotation = Quaternion.identity;

                    foreach (var sr in bossObj.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        sr.gameObject.SetActive(false);
                    }

                    Transform oldBoss1 = bossObj.transform.Find("Centurion_Boss_3D_Model");
                    if (oldBoss1 != null) UnityEngine.Object.DestroyImmediate(oldBoss1.gameObject);
                    Transform oldBoss2 = bossObj.transform.Find("Boss_3D_Mesh_Model");
                    if (oldBoss2 != null) UnityEngine.Object.DestroyImmediate(oldBoss2.gameObject);

                    GameObject bossFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/QuantumCenturion_Boss_3D.fbx");
                    if (bossFbx != null)
                    {
                        GameObject b3D = (GameObject)PrefabUtility.InstantiatePrefab(bossFbx, bossObj.transform);
                        b3D.name = "Centurion_Boss_3D_Model";
                        b3D.transform.localPosition = Vector3.zero;
                        b3D.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                        b3D.transform.localScale = Vector3.one * 1.05f;
                        b3D.SetActive(true);

                        foreach (var smr in b3D.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        {
                            if (matBoss != null) smr.sharedMaterial = matBoss;
                            EditorUtility.SetDirty(smr);
                        }
                        foreach (var mr in b3D.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            if (matBoss != null) mr.sharedMaterial = matBoss;
                            EditorUtility.SetDirty(mr);
                        }
                        Debug.Log("[DeployFull3DExperience V4] ¡Quantum Centurion Mecha Boss 3D ACTIVADO y texturizado en (0, 0, 5.20)!");
                    }
                }

                // -------------------------------------------------------------
                // 9. INSTANCIAR LA ARAÑA MECHA 3D ARTICULADA EN LA ESCENA + PREFAB
                // -------------------------------------------------------------
                GameObject spiderFbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/SpiderDrone_Mecha_3D.fbx");
                if (spiderFbxAsset != null)
                {
                    GameObject existingSpiderScene = FindInScene(scene, "SpiderDrone_Mecha_3D_Active");
                    if (existingSpiderScene != null)
                    {
                        UnityEngine.Object.DestroyImmediate(existingSpiderScene);
                    }

                    GameObject spiderSceneInst = (GameObject)PrefabUtility.InstantiatePrefab(spiderFbxAsset);
                    spiderSceneInst.name = "SpiderDrone_Mecha_3D_Active";
                    spiderSceneInst.transform.position = new Vector3(1.15f, 0.0f, 3.15f);
                    spiderSceneInst.transform.rotation = Quaternion.Euler(0f, -25f, 0f);
                    spiderSceneInst.transform.localScale = Vector3.one * 0.85f;
                    spiderSceneInst.SetActive(true);

                    foreach (var smr in spiderSceneInst.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                    {
                        if (matSpider != null) smr.sharedMaterial = matSpider;
                        EditorUtility.SetDirty(smr);
                    }
                    foreach (var mr in spiderSceneInst.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        if (matSpider != null) mr.sharedMaterial = matSpider;
                        EditorUtility.SetDirty(mr);
                    }
                    Debug.Log("[DeployFull3DExperience V4] ¡Araña Mecha 3D con Rig IK ACTIVADA en (1.15, 0, 3.15)!");
                }

                // Activar también Radio_Chipset_3D si existe
                GameObject radioObj = FindInScene(scene, "Radio_Chipset_3D");
                if (radioObj != null)
                {
                    radioObj.SetActive(true);
                }

                // -------------------------------------------------------------
                // 10. ACTUALIZAR PREFABS DE LA ARAÑA Y NYX
                // -------------------------------------------------------------
                string spiderPrefabPath = "Assets/Prefabs/SpiderDrone_25D.prefab";
                if (File.Exists(spiderPrefabPath) && spiderFbxAsset != null)
                {
                    using (var editScope = new PrefabUtility.EditPrefabContentsScope(spiderPrefabPath))
                    {
                        GameObject prefabRoot = editScope.prefabContentsRoot;
                        Transform sprChild = prefabRoot.transform.Find("Spider_Sprite25D");
                        if (sprChild != null) sprChild.gameObject.SetActive(false);

                        Transform existingMesh = prefabRoot.transform.Find("Spider_3D_Mesh_Model");
                        if (existingMesh != null) UnityEngine.Object.DestroyImmediate(existingMesh.gameObject);

                        GameObject meshInst = (GameObject)PrefabUtility.InstantiatePrefab(spiderFbxAsset, prefabRoot.transform);
                        meshInst.name = "Spider_3D_Mesh_Model";
                        meshInst.transform.localPosition = Vector3.zero;
                        meshInst.transform.localRotation = Quaternion.identity;
                        meshInst.transform.localScale = Vector3.one * 0.75f;

                        foreach (var smr in meshInst.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        {
                            if (matSpider != null) smr.sharedMaterial = matSpider;
                        }
                        foreach (var mr in meshInst.GetComponentsInChildren<MeshRenderer>(true))
                        {
                            if (matSpider != null) mr.sharedMaterial = matSpider;
                        }
                    }
                }

                // -------------------------------------------------------------
                // 11. NIVELAR CÁMARA Y ALINEAR VISTA DEL EDITOR (SCENE VIEW)
                // -------------------------------------------------------------
                GameObject mainCamObj = FindInScene(scene, "Main Camera");
                if (mainCamObj != null)
                {
                    mainCamObj.transform.position = new Vector3(0f, 1.65f, -0.25f);
                    mainCamObj.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                    if (SceneView.lastActiveSceneView != null)
                    {
                        SceneView.lastActiveSceneView.AlignViewToObject(mainCamObj.transform);
                        SceneView.lastActiveSceneView.Repaint();
                    }
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();

                Debug.Log("====================================================================");
                Debug.Log("[DeployFull3DExperience V4] ¡DESPLIEGUE 3D COMPLETADO Y APLICADO EN ESCENA!");
                Debug.Log("====================================================================");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[DeployFull3DExperience V4] Error durante el despliegue: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static GameObject FindInScene(Scene scene, string targetName)
        {
            if (!scene.IsValid()) return null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                {
                    return root;
                }
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.Equals(targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        return t.gameObject;
                    }
                }
            }
            return null;
        }

        private static Material CreatePbrMaterial(string matName, string baseTexPath, string metallicTexPath, string normalTexPath, Shader shader)
        {
            string matPath = $"{MaterialsFolder}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
            }

            mat.color = Color.white;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);

            Texture2D baseTex = AssetDatabase.LoadAssetAtPath<Texture2D>(baseTexPath);
            if (baseTex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", baseTex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", baseTex);
            }

            if (!string.IsNullOrEmpty(metallicTexPath))
            {
                Texture2D metTex = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicTexPath);
                if (metTex != null && mat.HasProperty("_MetallicGlossMap"))
                {
                    mat.SetTexture("_MetallicGlossMap", metTex);
                }
            }

            if (!string.IsNullOrEmpty(normalTexPath))
            {
                Texture2D normTex = AssetDatabase.LoadAssetAtPath<Texture2D>(normalTexPath);
                if (normTex != null && mat.HasProperty("_BumpMap"))
                {
                    mat.SetTexture("_BumpMap", normTex);
                }
            }

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.55f);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", 0.35f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateSkyboxMaterial(string matName, string texPath, Shader shader)
        {
            string matPath = $"{MaterialsFolder}/{matName}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, matPath);
            }

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
            }

            if (mat.HasProperty("_Exposure")) mat.SetFloat("_Exposure", 1.25f);
            if (mat.HasProperty("_Rotation")) mat.SetFloat("_Rotation", 185.0f);

            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void ApplyMaterialToChild(GameObject root, string childName, Material mat)
        {
            if (root == null || mat == null) return;
            foreach (Transform c in root.GetComponentsInChildren<Transform>(true))
            {
                if (c.name.Equals(childName, StringComparison.OrdinalIgnoreCase))
                {
                    var mr = c.GetComponent<MeshRenderer>();
                    if (mr != null)
                    {
                        mr.sharedMaterial = mat;
                        EditorUtility.SetDirty(mr);
                    }
                }
            }
        }
    }
}
