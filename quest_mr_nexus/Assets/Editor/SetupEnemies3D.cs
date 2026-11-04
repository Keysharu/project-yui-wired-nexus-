/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Asignador Automático de Scripts y Físicas para Enemigos 3D en Escena.
 *      Configura QuantumCenturion_Boss_3D y Meshy_AI_Spider_Mech_VR_1103022049_generate
 *      con Colliders, Rigidbody, AudioSource e IAs de combate (SpiderDroneAI y BossAI),
 *      y los vincula a MRUK_EncounterDirector para el combate AR / MRUK.
 * ---
 * [EN] Automatic Script and Physics Binder for 3D Scene Enemies.
 *      Configures QuantumCenturion_Boss_3D and Meshy_AI_Spider_Mech_VR_1103022049_generate
 *      with Colliders, Rigidbody, AudioSource, and Combat AI controllers,
 *      linking them to MRUK_EncounterDirector for AR / MRUK combat loops.
 * =============================================================================
 */

using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeoCyber.AR;
using ProjectNyx.Gameplay;

namespace ProjectNyx.EditorTools
{
    [InitializeOnLoad]
    public static class SetupEnemies3D
    {
        static SetupEnemies3D()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    ConfigureEnemiesNow();
                }
            };
        }

        [MenuItem("Wired Nexus MR/★ VINCULAR ENEMIGOS 3D (Spider + Boss)", false, 2)]
        public static void ConfigureEnemiesNow()
        {
            Debug.Log("[SetupEnemies3D] ==========================================================");
            Debug.Log("[SetupEnemies3D] INICIANDO VINCULACIÓN AUTOMÁTICA DE ENEMIGOS 3D...");
            Debug.Log("[SetupEnemies3D] ==========================================================");

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                scene = EditorSceneManager.OpenScene("Assets/MainMRScene.unity", OpenSceneMode.Single);
            }

            // 1. CONFIGURAR LA CYBER-SPIDER 3D
            GameObject spiderObj = GameObject.Find("Meshy_AI_Spider_Mech_VR_1103022049_generate");
            if (spiderObj == null)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name.Contains("Spider_Mech") || root.name.Contains("SpiderDrone"))
                    {
                        spiderObj = root;
                        break;
                    }
                }
            }

            if (spiderObj != null)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(spiderObj))
                {
                    PrefabUtility.UnpackPrefabInstance(spiderObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                // Collider de impacto para disparos de Blaster y tajos de Katana
                BoxCollider boxCol = spiderObj.GetComponent<BoxCollider>();
                if (boxCol == null)
                {
                    boxCol = spiderObj.AddComponent<BoxCollider>();
                }
                boxCol.center = new Vector3(0f, 0.45f, 0f);
                boxCol.size = new Vector3(1.6f, 0.9f, 1.6f);

                // Rigidbody cinemático para registro de impactos
                Rigidbody rb = spiderObj.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = spiderObj.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;
                rb.useGravity = false;

                // AudioSource 3D para chillido y disparo de plasma
                AudioSource audio = spiderObj.GetComponent<AudioSource>();
                if (audio == null)
                {
                    audio = spiderObj.AddComponent<AudioSource>();
                    audio.playOnAwake = false;
                    audio.spatialBlend = 1.0f;
                    audio.minDistance = 1.0f;
                    audio.maxDistance = 20.0f;
                }

                // Asegurar postura erguida sobre sus patas (-90 Euler en X)
                spiderObj.transform.localRotation = Quaternion.identity;

                // IA de desplazamiento y ataque de la Araña sobre paredes/suelo
                NeoCyberSpiderDroneAI spiderAI = spiderObj.GetComponent<NeoCyberSpiderDroneAI>();
                if (spiderAI == null)
                {
                    spiderAI = spiderObj.AddComponent<NeoCyberSpiderDroneAI>();
                }

                EditorUtility.SetDirty(spiderObj);
                Debug.Log("[SetupEnemies3D] ✓ Cyber-Spider 3D configurada con Collider, Rigidbody e IA.");
            }
            else
            {
                Debug.LogWarning("[SetupEnemies3D] No se encontró Meshy_AI_Spider_Mech_VR_1103022049_generate en la escena.");
            }

            // 2. CONFIGURAR EL QUANTUM CENTURION BOSS 3D
            GameObject bossObj = GameObject.Find("QuantumCenturion_Boss_3D");
            if (bossObj == null)
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name.Contains("QuantumCenturion") || root.name.Contains("Centurion_Boss"))
                    {
                        bossObj = root;
                        break;
                    }
                }
            }

            QuantumCenturionBossAI bossAI = null;
            if (bossObj != null)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(bossObj))
                {
                    PrefabUtility.UnpackPrefabInstance(bossObj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                // Collider cilíndrico de cuerpo entero para el Jefe
                CapsuleCollider capCol = bossObj.GetComponent<CapsuleCollider>();
                if (capCol == null)
                {
                    capCol = bossObj.AddComponent<CapsuleCollider>();
                }
                capCol.center = new Vector3(0f, 1.15f, 0f);
                capCol.radius = 0.85f;
                capCol.height = 2.4f;

                // Rigidbody cinemático
                Rigidbody rb = bossObj.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = bossObj.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;
                rb.useGravity = false;

                // AudioSource 3D
                AudioSource audio = bossObj.GetComponent<AudioSource>();
                if (audio == null)
                {
                    audio = bossObj.AddComponent<AudioSource>();
                    audio.playOnAwake = false;
                    audio.spatialBlend = 1.0f;
                    audio.minDistance = 1.5f;
                    audio.maxDistance = 35.0f;
                }

                // IA del Boss
                bossAI = bossObj.GetComponent<QuantumCenturionBossAI>();
                if (bossAI == null)
                {
                    bossAI = bossObj.AddComponent<QuantumCenturionBossAI>();
                }

                EditorUtility.SetDirty(bossObj);
                Debug.Log("[SetupEnemies3D] ✓ Quantum Centurion Boss 3D configurado con Collider, Rigidbody y BossAI.");
            }
            else
            {
                Debug.LogWarning("[SetupEnemies3D] No se encontró QuantumCenturion_Boss_3D en la escena.");
            }

            // 3. VINCULAR AL MRUK_ENCOUNTERDIRECTOR
            GameObject directorObj = GameObject.Find("MRUK_EncounterDirector");
            if (directorObj != null)
            {
                MRUKEncounterSpawner spawner = directorObj.GetComponent<MRUKEncounterSpawner>();
                if (spawner != null)
                {
                    SerializedObject soSpawner = new SerializedObject(spawner);
                    if (bossAI != null)
                    {
                        soSpawner.FindProperty("centurionBossInstance").objectReferenceValue = bossAI;
                    }
                    if (spiderObj != null)
                    {
                        soSpawner.FindProperty("spiderDronePrefab").objectReferenceValue = spiderObj;
                    }
                    soSpawner.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(spawner);
                    Debug.Log("[SetupEnemies3D] ✓ Director de oleadas MRUK enlazado con Spider y Boss.");
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SetupEnemies3D] ★ TODOS LOS ENEMIGOS 3D GUARDADOS Y LISTOS PARA COMBATE.");
        }
    }
}
