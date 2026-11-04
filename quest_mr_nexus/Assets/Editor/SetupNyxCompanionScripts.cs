/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Asignador y Limpiador Automático de Scripts y Cinemática para Nyx 3D.
 *      Garantiza que Nyx_Companion_Anime3D tenga ÚNICAMENTE los componentes oficiales
 *      (NyxFloatingCompanion, NyxAdaptationEngine, AudioSource) con postura erguida,
 *      escala táctica (0.55) y visibilidad ergonómica frontal-izquierda.
 * ---
 * [EN] Automatic Script and Kinematics Binder & Cleaner for 3D Nyx.
 *      Ensures Nyx_Companion_Anime3D has EXCLUSIVELY official components
 *      with upright posture, tactical scale (0.55), and ergonomic front-left visibility.
 * =============================================================================
 */

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectNyx.Companion;

namespace ProjectNyx.EditorTools
{
    [InitializeOnLoad]
    public static class SetupNyxCompanionScripts
    {
        [MenuItem("Wired Nexus MR/★ VINCULAR TODOS LOS SCRIPTS DE NYX 3D (Automático)", false, 1)]
        public static void SetupNyxNow()
        {
            Debug.Log("[NyxSetup] ==========================================================");
            Debug.Log("[NyxSetup] INICIANDO CONFIGURACIÓN QUIRÚRGICA DE NYX 3D...");
            Debug.Log("[NyxSetup] ==========================================================");

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene("Assets/MainMRScene.unity", OpenSceneMode.Single);
            }

            GameObject nyxObj = GameObject.Find("Nyx_Companion_Anime3D");
            if (nyxObj == null)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name.Contains("Nyx") && !root.name.Contains("Billboard"))
                    {
                        nyxObj = root;
                        break;
                    }
                }
            }

            if (nyxObj == null)
            {
                Debug.LogError("[NyxSetup] Error crítico: No se encontró Nyx_Companion_Anime3D en la escena.");
                return;
            }

            if (PrefabUtility.IsPartOfPrefabInstance(nyxObj))
            {
                PrefabUtility.UnpackPrefabInstance(nyxObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            // 1. Desactivar Animator sin Controller para liberar el Transform
            Animator anim = nyxObj.GetComponent<Animator>();
            if (anim != null && anim.runtimeAnimatorController == null)
            {
                anim.enabled = false;
            }

            // 2. Limpiar componentes ajenos duplicados que no deben estar en Nyx
            foreach (var comp in nyxObj.GetComponents<MonoBehaviour>())
            {
                if (comp == null) continue;
                string typeName = comp.GetType().Name;
                if (typeName != nameof(NyxFloatingCompanion) &&
                    typeName != nameof(NyxAdaptationEngine))
                {
                    // Remover scripts de salas, radio o música que pertenezcan a otros GameObjects
                    if (typeName.Contains("Decorator") || typeName.Contains("Radio") || 
                        typeName.Contains("BattleMusic") || typeName.Contains("Bridge") || 
                        typeName.Contains("Billboard") || typeName.Contains("Props"))
                    {
                        Debug.LogWarning($"[NyxSetup] Eliminando componente ajeno de Nyx: {typeName}");
                        UnityEngine.Object.DestroyImmediate(comp);
                    }
                }
            }

            // Asegurar un solo NyxFloatingCompanion
            var companions = nyxObj.GetComponents<NyxFloatingCompanion>();
            while (companions.Length > 1)
            {
                UnityEngine.Object.DestroyImmediate(companions[companions.Length - 1]);
                companions = nyxObj.GetComponents<NyxFloatingCompanion>();
            }

            NyxFloatingCompanion companion = nyxObj.GetComponent<NyxFloatingCompanion>();
            if (companion == null)
            {
                companion = nyxObj.AddComponent<NyxFloatingCompanion>();
            }

            // Configurar propiedades ergonómicas visibles
            SerializedObject soComp = new SerializedObject(companion);
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                soComp.FindProperty("playerHead").objectReferenceValue = mainCam.transform;
            }
            soComp.FindProperty("useFull3DModel").boolValue = true;
            soComp.FindProperty("companionScale").floatValue = 0.55f;
            soComp.FindProperty("modelEulerOffset").vector3Value = new Vector3(-90f, 0f, 0f);
            soComp.FindProperty("lookAtPlayer").boolValue = true;
            soComp.FindProperty("invertFacingDirection").boolValue = false;
            soComp.FindProperty("preferredDistance").floatValue = 1.6f;
            soComp.FindProperty("rightFlankOffset").floatValue = -0.55f;
            soComp.FindProperty("hoverHeightOffset").floatValue = -0.18f;
            soComp.FindProperty("hoverAtEyeLevel").boolValue = true;
            soComp.FindProperty("followSpeed").floatValue = 3.5f;
            soComp.FindProperty("rotationSpeed").floatValue = 5.0f;
            soComp.ApplyModifiedPropertiesWithoutUndo();

            // 3. Asegurar un solo NyxAdaptationEngine
            var adaptations = nyxObj.GetComponents<NyxAdaptationEngine>();
            while (adaptations.Length > 1)
            {
                UnityEngine.Object.DestroyImmediate(adaptations[adaptations.Length - 1]);
                adaptations = nyxObj.GetComponents<NyxAdaptationEngine>();
            }

            NyxAdaptationEngine adaptation = nyxObj.GetComponent<NyxAdaptationEngine>();
            if (adaptation == null)
            {
                adaptation = nyxObj.AddComponent<NyxAdaptationEngine>();
            }

            SerializedObject soAdapt = new SerializedObject(adaptation);
            soAdapt.FindProperty("companionController").objectReferenceValue = companion;
            soAdapt.FindProperty("activityLevel").enumValueIndex = (int)NyxActivityLevel.Balanced_Standard;
            soAdapt.ApplyModifiedPropertiesWithoutUndo();

            // 4. AudioSource
            var audios = nyxObj.GetComponents<AudioSource>();
            while (audios.Length > 1)
            {
                UnityEngine.Object.DestroyImmediate(audios[audios.Length - 1]);
                audios = nyxObj.GetComponents<AudioSource>();
            }

            AudioSource audio = nyxObj.GetComponent<AudioSource>();
            if (audio == null)
            {
                audio = nyxObj.AddComponent<AudioSource>();
                audio.playOnAwake = false;
                audio.spatialBlend = 1.0f;
                audio.minDistance = 1.0f;
                audio.maxDistance = 25.0f;
            }

            // 5. Asignar material y Transform erguido
            nyxObj.transform.localScale = new Vector3(55f, 55f, 55f);
            nyxObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            companion.ApplyScale();

            Material matNyx = AssetDatabase.LoadAssetAtPath<Material>("Assets/Models/3d/nix 3d meshy/Materials/Meshy_AI_Anime_Cyberpunk_Femal_1103022234_image-to-3d-texture.mat");
            if (matNyx == null)
            {
                matNyx = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/3D_Meshes/Mat_Nyx_Heroine_3D.mat");
            }

            if (matNyx != null)
            {
                foreach (var mr in nyxObj.GetComponentsInChildren<MeshRenderer>(true))
                {
                    mr.sharedMaterial = matNyx;
                    EditorUtility.SetDirty(mr);
                }
            }

            EditorUtility.SetDirty(nyxObj);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[NyxSetup] ★ NYX 3D CONFIGURADA, LIMPIA Y VISIBLE EN LA ESCENA.");
        }
    }
}
