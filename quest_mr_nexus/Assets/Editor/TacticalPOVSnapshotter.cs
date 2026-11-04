using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectNyx.Core;
using ProjectNyx.Gameplay;
using ProjectNyx.Companion;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.EditorTools
{
    /// <summary>
    /// [ES] Visor Táctico Compartido 3D / HD-2D ("Push Frontier v6: 24/7 Radio + Gauntlet + Flow").
    /// Herramienta de Editor para capturar snapshots de verificación en tiempo real de la escena MR.
    /// ---
    /// [EN] Shared 3D / HD-2D Tactical Viewport ("Push Frontier v6: 24/7 Radio + Gauntlet + Flow").
    /// Unity Editor utility for capturing live tactical POV verification snapshots of the MR scene.
    /// </summary>
    [InitializeOnLoad]
    public static class TacticalPOVSnapshotter
    {
        private const string MainScenePath = "Assets/MainMRScene.unity";
        private const string UpgradeFlagPath = "ProjectSettings/PushFrontier3D_v7_VoiceMenuMelee.flag";

        static TacticalPOVSnapshotter()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !File.Exists(UpgradeFlagPath))
                {
                    CaptureTacticalPOV();
                    File.WriteAllText(UpgradeFlagPath, DateTime.UtcNow.ToString("O"));
                }
            };
        }

        private static Sprite ForceLoadSprite(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                bool changed = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    changed = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    changed = true;
                }
                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    changed = true;
                }
                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        [MenuItem("Wired Nexus MR/4. Capturar Vista Táctica POV (1080p Blog & Ajuste)", false, 40)]
        public static void CaptureTacticalPOV()
        {
            AssetDatabase.Refresh();
            SpriteImportSettingsFixer.FixAllSprites();
            UpdateSpiderPrefabWithFull8Legs();

            Sprite fpsBlasterSprite = ForceLoadSprite("Assets/Sprites/Weapons/Blaster/fps_blaster_forward.png");
            Sprite fpsKatanaSprite = ForceLoadSprite("Assets/Sprites/Weapons/Katana/fps_katana_forward.png");
            Sprite fpsGauntletShieldSprite = ForceLoadSprite("Assets/Sprites/Weapons/Gauntlet/fps_gauntlet_forward.png");
            Sprite fpsGauntletNoShieldSprite = ForceLoadSprite("Assets/Sprites/Weapons/Gauntlet/fps_gauntlet_no_shield.png");
            Sprite fpsGauntletRightSprite = ForceLoadSprite("Assets/Sprites/Weapons/Gauntlet/fps_gauntlet_right_forward.png");

            // Música 24/7 por carpetas reales del proyecto (Assets/Audio/Battle/* y Assets/Audio/Radio/*).
            // Si en el futuro Raquel suelta más mp3 en esas carpetas, se cablean solos.
            AudioClip[] radioFolderTracks = LoadClipsFromFolder("Assets/Audio/Radio");
            AudioClip[] normalFolderTracks = LoadClipsFromFolder("Assets/Audio/Battle/Normal");
            AudioClip[] bossFolderTracks = LoadClipsFromFolder("Assets/Audio/Battle/Boss");
            AudioClip[] finalFolderTracks = LoadClipsFromFolder("Assets/Audio/Battle/FinalBoss");

            // Peaceful (radio 24/7): Ronin_Code_Focus_Beats (calma) + lo que haya en Assets/Audio/Radio
            List<AudioClip> peacefulList = new List<AudioClip>(radioFolderTracks);
            List<AudioClip> normalList = new List<AudioClip>();
            foreach (AudioClip c in normalFolderTracks)
            {
                if (c != null && c.name.StartsWith("Ronin_Code_Focus")) peacefulList.Add(c);
                else if (c != null) normalList.Add(c);
            }
            if (peacefulList.Count == 0 && normalList.Count > 0) peacefulList.Add(normalList[0]);
            Debug.Log($"[TacticalPOVSnapshotter] Música cableada -> Peaceful:{peacefulList.Count} Normal:{normalList.Count} Boss:{bossFolderTracks.Length} Final:{finalFolderTracks.Length}");

            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[TacticalPOVSnapshotter] No se encontró Main Camera en MainMRScene.");
                return;
            }

            Shader unlitShader = Shader.Find("Unlit/Color");
            if (unlitShader == null) unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");

            Material cyanGlowMat = CreateOrUpdateMat("Assets/Prefabs/Mat_3D_CyanGlow.mat", unlitShader, new Color(0f, 1f, 0.88f, 0.92f));
            Material magentaGlowMat = CreateOrUpdateMat("Assets/Prefabs/Mat_3D_MagentaGlow.mat", unlitShader, new Color(1f, 0.08f, 0.58f, 0.92f));
            Material goldGlowMat = CreateOrUpdateMat("Assets/Prefabs/Mat_3D_GoldGlow.mat", unlitShader, new Color(1f, 0.78f, 0.15f, 0.90f));
            Material greenHealMat = CreateOrUpdateMat("Assets/Prefabs/Mat_3D_GreenHeal.mat", unlitShader, new Color(0.22f, 1f, 0.35f, 0.95f));
            Material obsidianMat = CreateOrUpdateMat("Assets/Prefabs/Mat_3D_Obsidian.mat", unlitShader, new Color(0.06f, 0.08f, 0.14f, 1f));
            Material voidPortalMat = CreateOrUpdateMat("Assets/Prefabs/Mat_3D_VoidPortal.mat", unlitShader, new Color(0.02f, 0.04f, 0.11f, 0.96f));

            // 1. Reconstruir el Salón AR/VR (Mythic Realms + Spatial Ops) con Cofres 3D y Portal Circular de Brecha
            GameObject existingRoom = FindInScene(scene, "MR_Holodeck_PreviewRoom");
            if (existingRoom != null)
            {
                UnityEngine.Object.DestroyImmediate(existingRoom);
            }

            DimensionalRift3D riftComponent;
            CyberSupplyChest[] chestsArray;
            GameObject previewRoom = BuildFull3DCyberpunkRoom(
                unlitShader,
                cyanGlowMat,
                magentaGlowMat,
                goldGlowMat,
                greenHealMat,
                obsidianMat,
                voidPortalMat,
                out riftComponent,
                out chestsArray
            );

            // 2. Vincular DesktopSimulationHUD y BattleMusicManager (Emisora 24/7 + 4 Niveles de Música) al Hub de Sistemas
            GameObject systemsHub = FindInScene(scene, "WiredNexus_SystemsHub");
            if (systemsHub != null)
            {
                DesktopSimulationHUD hud = systemsHub.GetComponent<DesktopSimulationHUD>() ?? systemsHub.AddComponent<DesktopSimulationHUD>();
                SerializedObject soHud = new SerializedObject(hud);
                soHud.FindProperty("holodeckPreviewEnvironment").objectReferenceValue = previewRoom;
                soHud.ApplyModifiedPropertiesWithoutUndo();

                // Asegurar PauseMenuController, FloorTeleportNetwork y MicrophoneVoiceBridge en el Hub
                PauseMenuController pauseCtrl = systemsHub.GetComponent<PauseMenuController>() ?? systemsHub.AddComponent<PauseMenuController>();
                FloorTeleportNetwork teleNet = systemsHub.GetComponent<FloorTeleportNetwork>() ?? systemsHub.AddComponent<FloorTeleportNetwork>();
                MicrophoneVoiceBridge micBridge = systemsHub.GetComponent<MicrophoneVoiceBridge>() ?? systemsHub.AddComponent<MicrophoneVoiceBridge>();
                CinematicGameFlowDirector flowDirector = systemsHub.GetComponent<CinematicGameFlowDirector>() ?? systemsHub.AddComponent<CinematicGameFlowDirector>();

                BattleMusicManager musicMgr = systemsHub.GetComponent<BattleMusicManager>() ?? systemsHub.AddComponent<BattleMusicManager>();
                SerializedObject soMusic = new SerializedObject(musicMgr);
                AssignClipArray(soMusic, "peacefulRadioTracks", peacefulList.ToArray());
                AssignClipArray(soMusic, "normalBattleTracks", normalList.ToArray());
                AssignClipArray(soMusic, "bossTracks", bossFolderTracks);
                AssignClipArray(soMusic, "finalBossTracks", finalFolderTracks);
                soMusic.ApplyModifiedPropertiesWithoutUndo();
            }

            // 3. Configurar Cámara Principal a altura de ojos (1.65 m) con gran angular (65°) alineado al Salón 3D de Google Flow
            mainCam.transform.position = new Vector3(0f, 1.65f, -0.25f);
            mainCam.transform.rotation = Quaternion.Euler(2f, 0f, 0f);
            mainCam.fieldOfView = 65f;

            // 4. Mano Derecha: Pistola de Plasma, Katana y Guantelete en Primera Persona Apuntando de Frente
            GameObject rightHand = FindInScene(scene, "RightHand_Arsenal");
            Transform blaster = null;
            Transform katana = null;
            Transform gauntlet = null;
            if (rightHand != null)
            {
                Vector3 rightOffset = new Vector3(0.24f, -0.14f, 0.52f);
                rightHand.transform.position = mainCam.transform.TransformPoint(rightOffset);
                rightHand.transform.rotation = mainCam.transform.rotation;

                XRHandAnchorFollower rFollow = rightHand.GetComponent<XRHandAnchorFollower>();
                if (rFollow != null)
                {
                    SerializedObject soRf = new SerializedObject(rFollow);
                    soRf.FindProperty("editorCameraOffset").vector3Value = rightOffset;
                    soRf.ApplyModifiedPropertiesWithoutUndo();
                }

                blaster = rightHand.transform.Find("Blaster_Hologram");
                katana = rightHand.transform.Find("Katana_Hologram");
                gauntlet = rightHand.transform.Find("Gauntlet_Hologram");

                // Asignar KatanaMeleeController a la Katana para corte físico auténtico
                if (katana != null)
                {
                    KatanaMeleeController kMelee = katana.GetComponent<KatanaMeleeController>() ?? katana.gameObject.AddComponent<KatanaMeleeController>();
                }
                if (blaster != null)
                {
                    blaster.gameObject.SetActive(true);
                    blaster.localPosition = Vector3.zero;
                    blaster.localScale = new Vector3(0.185f, 0.185f, 0.185f);
                    blaster.localRotation = Quaternion.identity;
                    SpriteRenderer bSr = blaster.GetComponent<SpriteRenderer>();
                    if (bSr != null)
                    {
                        bSr.sprite = fpsBlasterSprite != null ? fpsBlasterSprite : AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapons/Blaster/full_blaster.png");
                        bSr.sortingOrder = 50;
                    }
                }
                if (katana != null)
                {
                    katana.gameObject.SetActive(false);
                    katana.localPosition = new Vector3(-0.08f, 0.04f, 0f);
                    katana.localScale = new Vector3(0.21f, 0.21f, 0.21f);
                    katana.localRotation = Quaternion.identity;
                    SpriteRenderer kSr = katana.GetComponent<SpriteRenderer>();
                    if (kSr != null)
                    {
                        kSr.sprite = fpsKatanaSprite != null ? fpsKatanaSprite : AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Weapons/Katana/full_katana.png");
                        kSr.sortingOrder = 50;
                    }
                }
                if (gauntlet != null)
                {
                    gauntlet.gameObject.SetActive(false);
                    gauntlet.localPosition = new Vector3(0.02f, -0.02f, 0f);
                    gauntlet.localScale = new Vector3(0.20f, 0.20f, 0.20f);
                    gauntlet.localRotation = Quaternion.identity;
                    SpriteRenderer gSr = gauntlet.GetComponent<SpriteRenderer>();
                    if (gSr != null)
                    {
                        gSr.sprite = fpsGauntletRightSprite != null ? fpsGauntletRightSprite : fpsGauntletNoShieldSprite;
                        gSr.sortingOrder = 50;
                    }
                }
            }

            // 5. Mano Izquierda: Guantelete sin partículas en reposo (fps_gauntlet_no_shield.png) y con escudo al bloquear (fps_gauntlet_forward.png)
            GameObject leftHand = FindInScene(scene, "LeftHand_HoloShield");
            SpriteRenderer leftGauntletSr = null;
            HoloShieldController shieldCtrl = null;
            if (leftHand != null)
            {
                Vector3 leftOffset = new Vector3(-0.24f, -0.13f, 0.52f);
                leftHand.transform.position = mainCam.transform.TransformPoint(leftOffset);
                leftHand.transform.rotation = mainCam.transform.rotation;

                XRHandAnchorFollower lFollow = leftHand.GetComponent<XRHandAnchorFollower>();
                if (lFollow != null)
                {
                    SerializedObject soLf = new SerializedObject(lFollow);
                    soLf.FindProperty("editorCameraOffset").vector3Value = leftOffset;
                    soLf.ApplyModifiedPropertiesWithoutUndo();
                }

                Transform leftGauntlet = leftHand.transform.Find("LeftGauntlet_Sprite");
                if (leftGauntlet != null)
                {
                    leftGauntlet.localPosition = Vector3.zero;
                    leftGauntlet.localScale = new Vector3(0.185f, 0.185f, 0.185f);
                    leftGauntlet.localRotation = Quaternion.identity;
                    leftGauntletSr = leftGauntlet.GetComponent<SpriteRenderer>();
                    if (leftGauntletSr != null)
                    {
                        leftGauntletSr.sprite = fpsGauntletShieldSprite != null ? fpsGauntletShieldSprite : fpsGauntletNoShieldSprite;
                        leftGauntletSr.sortingOrder = 50;
                    }
                }

                shieldCtrl = leftHand.GetComponent<HoloShieldController>();
                if (shieldCtrl != null)
                {
                    SerializedObject soShield = new SerializedObject(shieldCtrl);
                    if (leftGauntletSr != null) soShield.FindProperty("gauntletSpriteRenderer").objectReferenceValue = leftGauntletSr;
                    if (fpsGauntletNoShieldSprite != null) soShield.FindProperty("idleNoShieldSprite").objectReferenceValue = fpsGauntletNoShieldSprite;
                    if (fpsGauntletShieldSprite != null) soShield.FindProperty("activeShieldSprite").objectReferenceValue = fpsGauntletShieldSprite;
                    soShield.ApplyModifiedPropertiesWithoutUndo();
                }

                Transform barrier = leftHand.transform.Find("HoloShield_BarrierField");
                if (barrier != null)
                {
                    MeshRenderer mr = barrier.GetComponent<MeshRenderer>();
                    if (mr != null) mr.enabled = false;
                }
            }

            // 6. Nyx Companion en el ALA DERECHA AMPLIA A RAS DE SUELO (x = +2.10 m, z = 3.55 m)
            GameObject nyx = FindInScene(scene, "Nyx_Billboard_Companion");
            if (nyx != null)
            {
                nyx.transform.position = new Vector3(2.10f, 0.05f, 3.55f);
                NyxFloatingCompanion floatComp = nyx.GetComponent<NyxFloatingCompanion>();
                if (floatComp != null)
                {
                    SerializedObject soNyx = new SerializedObject(floatComp);
                    soNyx.FindProperty("preferredDistance").floatValue = 3.55f;
                    soNyx.FindProperty("rightFlankOffset").floatValue = 2.10f;
                    soNyx.FindProperty("floorHeight").floatValue = 0.05f;
                    soNyx.ApplyModifiedPropertiesWithoutUndo();
                }

                Transform nyxSprite = nyx.transform.Find("Nyx_Sprite25D");
                SpriteRenderer nyxSr = null;
                if (nyxSprite != null)
                {
                    nyxSprite.localPosition = new Vector3(0f, 0.92f, 0f);
                    nyxSprite.localScale = new Vector3(0.60f, 0.60f, 0.60f);
                    nyxSr = nyxSprite.GetComponent<SpriteRenderer>();
                    if (nyxSr != null) nyxSr.sortingOrder = 10;
                }
                OrientTowardsCamera(nyx.transform, mainCam.transform);
                EnhanceNyxWith3DVolume(nyx, nyxSr, cyanGlowMat, magentaGlowMat);
            }

            // 7. Estación de Radio 3D Radio Chipset en el ALA IZQUIERDA AMPLIA (x = -2.20 m, z = 3.65 m)
            GameObject radio = FindInScene(scene, "Radio_RadioChipset_3D");
            if (radio != null)
            {
                radio.transform.position = new Vector3(-2.20f, 0.95f, 3.65f);
                OrientTowardsCamera(radio.transform, mainCam.transform);
                EnhanceRadioStation3D(radio, obsidianMat, cyanGlowMat, magentaGlowMat);
            }

            // 8. Boss Quantum Centurion emergiendo del Portal Circular del Salón de Google Flow (z = 4.35 m)
            GameObject boss = FindInScene(scene, "QuantumCenturion_Boss_25D");
            QuantumCenturionBossAI bossAi = null;
            if (boss != null)
            {
                boss.SetActive(true);
                boss.transform.position = new Vector3(0f, 0.12f, 4.35f);
                bossAi = boss.GetComponent<QuantumCenturionBossAI>();
                if (bossAi != null)
                {
                    SerializedObject soBoss = new SerializedObject(bossAi);
                    soBoss.FindProperty("climaxThresholdRatio").floatValue = 0.35f;
                    soBoss.FindProperty("preferredCombatDistance").floatValue = 4.1f;
                    soBoss.ApplyModifiedPropertiesWithoutUndo();
                }

                Transform bossSprite = boss.transform.Find("Centurion_Sprite25D");
                SpriteRenderer bossSr = null;
                if (bossSprite != null)
                {
                    bossSprite.localPosition = new Vector3(0f, 1.25f, 0f);
                    bossSprite.localScale = new Vector3(1.18f, 1.18f, 1.18f);
                    OrientTowardsCamera(bossSprite, mainCam.transform);
                    bossSr = bossSprite.GetComponent<SpriteRenderer>();
                    if (bossSr != null) bossSr.sortingOrder = 12;
                }
                EnhanceBossWith3DArena(boss, bossSr, cyanGlowMat, magentaGlowMat, goldGlowMat);
            }

            // 9. Vincular MRUKEncounterSpawner con los Cofres 3D, la Brecha 3D, el Prefab de Araña de 8 patas, el Boss y las 7 Voces
            GameObject encounterDir = FindInScene(scene, "MRUK_EncounterDirector");
            GameObject spiderPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SpiderDrone_25D.prefab");
            if (encounterDir != null)
            {
                MRUKEncounterSpawner spawner = encounterDir.GetComponent<MRUKEncounterSpawner>();
                if (spawner != null)
                {
                    SerializedObject soSpawner = new SerializedObject(spawner);
                    soSpawner.FindProperty("spiderDronePrefab").objectReferenceValue = spiderPrefab;
                    soSpawner.FindProperty("dimensionalRift3D").objectReferenceValue = riftComponent;
                    soSpawner.FindProperty("centurionBossInstance").objectReferenceValue = bossAi;

                    SerializedProperty chestsProp = soSpawner.FindProperty("roomSupplyChests");
                    if (chestsProp != null && chestsArray != null)
                    {
                        chestsProp.arraySize = chestsArray.Length;
                        for (int i = 0; i < chestsArray.Length; i++)
                        {
                            chestsProp.GetArrayElementAtIndex(i).objectReferenceValue = chestsArray[i];
                        }
                    }
                    soSpawner.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // 10. Instanciar temporalmente 2 Cyber-Spiders de 8 patas completas bien espaciadas sobre el suelo del Salón Flow
            GameObject tempSpiderL = null;
            GameObject tempSpiderR = null;
            if (spiderPrefab != null)
            {
                tempSpiderL = CreatePreviewSpiderWith3DRing(spiderPrefab, scene, "Preview_Spider_Left", new Vector3(-1.10f, 0.35f, 3.25f), mainCam.transform, magentaGlowMat);
                tempSpiderR = CreatePreviewSpiderWith3DRing(spiderPrefab, scene, "Preview_Spider_Right", new Vector3(1.10f, 0.35f, 3.25f), mainCam.transform, magentaGlowMat);
            }

            // 11. Renderizar Capturas 1080p para verificación y Blog
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outDir = Path.Combine(Path.GetDirectoryName(projectRoot), "Capturas_Blog_Hackathon");
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }


            CyberRoomDecorator3D dec = previewRoom.GetComponent<CyberRoomDecorator3D>();

            // Modo AR para Capturas 1, 2 y 3 (Salón Híbrido: Pozo de datos, Barricada táctica, Enredaderas y Cofres)
            if (dec != null) dec.SetImmersiveVrMode(false);

            // Captura 1: Combate AR completo con Escudo Hexagonal Activo en Mano Izquierda + Blaster + Boss + Arañas
            if (leftGauntletSr != null && fpsGauntletShieldSprite != null)
            {
                leftGauntletSr.sprite = fpsGauntletShieldSprite;
            }
            string povPath = Path.Combine(outDir, "01_WiredNexus_POV_Combat_Showcase.png");
            RenderCameraToPng(mainCam, 1920, 1080, povPath);

            // Captura 2: Inicio de Partida AR Limpio (Sin Boss, Mano Izquierda SIN partículas del escudo + Blaster)
            if (boss != null) boss.SetActive(false);
            if (leftGauntletSr != null && fpsGauntletNoShieldSprite != null)
            {
                leftGauntletSr.sprite = fpsGauntletNoShieldSprite;
            }
            string stagePath = Path.Combine(outDir, "02_WiredNexus_Radio_MinaKira_Nyx.png");
            RenderCameraToPng(mainCam, 1920, 1080, stagePath);

            // Captura 3: Vista FPS AR con Katana Ciberpunk en Mano Derecha y Mano Izquierda sin escudo
            if (blaster != null) blaster.gameObject.SetActive(false);
            if (katana != null) katana.gameObject.SetActive(true);
            string katanaPath = Path.Combine(outDir, "03_WiredNexus_Katana_NoShield_POV.png");
            RenderCameraToPng(mainCam, 1920, 1080, katanaPath);

            // Captura 4: MUNDO PRINCIPAL VR INMERSIVO (Neo-Eclipse // Bulevar 107 con Tiendas, Cafeterías y Megatorre de Radio Chipset 107.7 FM)
            if (katana != null) katana.gameObject.SetActive(false);
            if (blaster != null) blaster.gameObject.SetActive(true);
            if (dec != null) dec.SetImmersiveVrMode(true);
            string vrLobbyPath = Path.Combine(outDir, "04_WiredNexus_VR_Boulevard_Lobby.png");
            RenderCameraToPng(mainCam, 1920, 1080, vrLobbyPath);

            // Restaurar configuración: Blaster activo, mano limpia, y dejar en VR para el arranque inicial cinemático
            if (leftGauntletSr != null && fpsGauntletNoShieldSprite != null)
            {
                leftGauntletSr.sprite = fpsGauntletNoShieldSprite;
            }
            if (dec != null) dec.SetImmersiveVrMode(true);

            // Eliminar las arañas temporales de foto y DEJAR AL BOSS OCULTO (SetActive(false)) para que el Play Mode empiece en Fase 1 (Cofres -> Brecha -> Oleadas)
            if (tempSpiderL != null) UnityEngine.Object.DestroyImmediate(tempSpiderL);
            if (tempSpiderR != null) UnityEngine.Object.DestroyImmediate(tempSpiderR);
            if (boss != null) boss.SetActive(false);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[TacticalPOVSnapshotter] Escena v6 (Mano Izq Sin Escudo Idle + Katana FPS + Voces Audibles + Radio 24/7) guardada en: {outDir}");
        }

        private static void AssignClipArray(SerializedObject so, string propName, AudioClip[] clips)
        {
            SerializedProperty prop = so.FindProperty(propName);
            if (prop == null || clips == null) return;
            prop.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }
        }

        /// <summary>Carga todos los AudioClip (mp3/wav/ogg) de una carpeta del proyecto, ordenados por nombre.</summary>
        private static AudioClip[] LoadClipsFromFolder(string folder)
        {
            List<AudioClip> result = new List<AudioClip>();
            if (!AssetDatabase.IsValidFolder(folder)) return result.ToArray();

            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
            List<string> paths = new List<string>();
            foreach (string guid in guids) paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            paths.Sort(StringComparer.OrdinalIgnoreCase);

            foreach (string p in paths)
            {
                AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                if (clip != null) result.Add(clip);
            }
            return result.ToArray();
        }

        private static void UpdateSpiderPrefabWithFull8Legs()
        {
            string prefabPath = "Assets/Prefabs/SpiderDrone_25D.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            if (prefabRoot == null) return;

            try
            {
                BoxCollider box = prefabRoot.GetComponent<BoxCollider>();
                if (box == null) box = prefabRoot.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 0.05f, 0f);
                box.size = new Vector3(0.85f, 0.55f, 0.45f);

                Sprite idle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/Spider_drone/combat_idle.png");
                Sprite move1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/Spider_drone/combat_move_1.png");
                Sprite move2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/Spider_drone/combat_move_2.png");
                Sprite charge = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/Spider_drone/combat_attack_charge.png");
                Sprite strike = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/Spider_drone/combat_attack_strike.png");
                Sprite hit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/Spider_drone/combat_hit_stagger.png");
                Sprite death = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemies/Spider_drone/combat_death_collapse.png");

                SpriteRenderer sr = prefabRoot.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    if (idle != null) sr.sprite = idle;
                    sr.transform.localScale = new Vector3(0.36f, 0.36f, 0.36f);
                    sr.sortingOrder = 10;
                }

                EnemyCombatSpriteAnimator anim = prefabRoot.GetComponent<EnemyCombatSpriteAnimator>();
                if (anim != null)
                {
                    SerializedObject soAnim = new SerializedObject(anim);
                    if (sr != null) soAnim.FindProperty("spriteRenderer").objectReferenceValue = sr;
                    if (idle != null) soAnim.FindProperty("idleSprite").objectReferenceValue = idle;
                    if (move1 != null) soAnim.FindProperty("moveSprite1").objectReferenceValue = move1;
                    if (move2 != null) soAnim.FindProperty("moveSprite2").objectReferenceValue = move2;
                    if (charge != null) soAnim.FindProperty("attackChargeSprite").objectReferenceValue = charge;
                    if (strike != null) soAnim.FindProperty("attackStrikeSprite").objectReferenceValue = strike;
                    if (hit != null) soAnim.FindProperty("hitStaggerSprite").objectReferenceValue = hit;
                    if (death != null) soAnim.FindProperty("deathCollapseSprite").objectReferenceValue = death;
                    soAnim.ApplyModifiedPropertiesWithoutUndo();
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static GameObject BuildFull3DCyberpunkRoom(
            Shader unlitShader,
            Material cyanMat,
            Material magentaMat,
            Material goldMat,
            Material greenHealMat,
            Material obsidianMat,
            Material voidPortalMat,
            out DimensionalRift3D riftOut,
            out CyberSupplyChest[] chestsOut)
        {
            GameObject roomRoot = new GameObject("MR_Holodeck_PreviewRoom");

            // 1. Importar y cargar los 12 fotogramas 1080p del Salón 3D generado en Google Flow (Neon_fiber_optic_animation_loop)
            List<Sprite> flowSpriteList = new List<Sprite>();
            List<Texture2D> flowTexList = new List<Texture2D>();
            for (int i = 0; i < 12; i++)
            {
                string fPath = $"Assets/Sprites/Environment/FlowRoom/flow_room_{i:02d}.png";
                AssetDatabase.ImportAsset(fPath, ImportAssetOptions.ForceUpdate);
                Sprite sp = ForceLoadSprite(fPath);
                if (sp != null) flowSpriteList.Add(sp);
                Texture2D tx = AssetDatabase.LoadAssetAtPath<Texture2D>(fPath);
                if (tx != null) flowTexList.Add(tx);
            }

            // Fallback garantizado a cyber_wall_mural.png si algún frame no cargó aún
            AssetDatabase.ImportAsset("Assets/Sprites/Environment/cyber_wall_mural.png", ImportAssetOptions.ForceUpdate);
            Texture2D mainFlowTex = flowTexList.Count > 0
                ? flowTexList[0]
                : AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Environment/cyber_wall_mural.png");

            Shader texShader = Shader.Find("Unlit/Texture");
            if (texShader == null) texShader = unlitShader;
            string flowMatPath = "Assets/Prefabs/Mat_3D_FlowRoomStage.mat";
            Material flowStageMat = AssetDatabase.LoadAssetAtPath<Material>(flowMatPath);
            if (flowStageMat == null)
            {
                flowStageMat = new Material(texShader);
                AssetDatabase.CreateAsset(flowStageMat, flowMatPath);
            }
            else
            {
                flowStageMat.shader = texShader;
            }
            flowStageMat.mainTexture = mainFlowTex;
            flowStageMat.renderQueue = 1990;
            EditorUtility.SetDirty(flowStageMat);

            // 2. Escenario Panorámico 3D 1080p Animado de Google Flow (Quad 3D con Unlit/Texture y FlowRoomBackdropAnimator)
            GameObject flowStageObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            flowStageObj.name = "FlowRoom_3D_StageBackdrop";
            flowStageObj.transform.SetParent(roomRoot.transform, false);
            flowStageObj.transform.localPosition = new Vector3(0f, 1.46f, 5.55f);
            flowStageObj.transform.localRotation = Quaternion.Euler(2f, 0f, 0f);
            // A z = 5.55m (dist = 5.80m desde cámara a z = -0.25m) con FOV = 65°, alto = 7.42m y ancho 16:9 = 13.19m cubre el 100% de la vista
            flowStageObj.transform.localScale = new Vector3(13.25f, 7.46f, 1f);
            UnityEngine.Object.DestroyImmediate(flowStageObj.GetComponent<Collider>());

            MeshRenderer stageMr = flowStageObj.GetComponent<MeshRenderer>();
            stageMr.sharedMaterial = flowStageMat;

            FlowRoomBackdropAnimator flowAnim = flowStageObj.AddComponent<FlowRoomBackdropAnimator>();
            SerializedObject soFlow = new SerializedObject(flowAnim);
            soFlow.FindProperty("targetMeshRenderer").objectReferenceValue = stageMr;
            soFlow.FindProperty("framesPerSecond").floatValue = 8f;
            SerializedProperty txArr = soFlow.FindProperty("flowFrames");
            if (txArr != null)
            {
                txArr.arraySize = flowTexList.Count;
                for (int i = 0; i < flowTexList.Count; i++)
                {
                    txArr.GetArrayElementAtIndex(i).objectReferenceValue = flowTexList[i];
                }
            }
            SerializedProperty spArr = soFlow.FindProperty("flowSprites");
            if (spArr != null)
            {
                spArr.arraySize = flowSpriteList.Count;
                for (int i = 0; i < flowSpriteList.Count; i++)
                {
                    spArr.GetArrayElementAtIndex(i).objectReferenceValue = flowSpriteList[i];
                }
            }
            soFlow.ApplyModifiedPropertiesWithoutUndo();

            // 3. Suelo físico invisible (con Collider activo para físicas/raycasts pero sin tapar el suelo reflectante del vídeo de Google Flow)
            GameObject floorColliderObj = new GameObject("Room_FloorCollider3D");
            floorColliderObj.transform.SetParent(roomRoot.transform, false);
            floorColliderObj.transform.localPosition = new Vector3(0f, -0.05f, 3.5f);
            BoxCollider floorCol = floorColliderObj.AddComponent<BoxCollider>();
            floorCol.size = new Vector3(12f, 0.1f, 10f);

            // 4. Anillo Orbital Superior adicional (sincronizado con los anillos del techo del vídeo de Google Flow)
            Transform ceilingRing = CreateSegmented3DRing("OpenWiredCeiling_OrbitalRing", roomRoot.transform, new Vector3(0f, 3.85f, 3.6f), 2.2f, 24, cyanMat);

            // 5. Portal Circular 3D de Brecha Interdimensional alineado exactamente con el portal circular del fondo del vídeo de Google Flow
            riftOut = BuildSpatialOpsDimensionalRift(roomRoot.transform, new Vector3(0f, 1.52f, 4.95f), voidPortalMat, cyanMat, magentaMat);

            // 6. Tres Cofres de Suministros 3D con Holograma '+' sobre las baldosas del Salón Flow (sin bloques amarillos que tapen la vista)
            Vector3[] chestPositions =
            {
                new Vector3(-1.45f, 0.0f, 2.75f),
                new Vector3(0.0f, 0.0f, 2.55f),
                new Vector3(1.45f, 0.0f, 2.75f)
            };
            chestsOut = new CyberSupplyChest[chestPositions.Length];
            for (int c = 0; c < chestPositions.Length; c++)
            {
                chestsOut[c] = BuildSpatialOpsSupplyChest(roomRoot.transform, $"CyberSupplyChest_{c + 1}", chestPositions[c], obsidianMat, cyanMat, greenHealMat);
            }

            
            // 7. Enredaderas de Fibra Óptica y Nodos de Circuito en paredes y marcos (AR Salón Híbrido)
            GameObject arVinesRoot = new GameObject("AR_WallCircuitVines_Root");
            arVinesRoot.transform.SetParent(roomRoot.transform, false);
            BuildWallCircuitVines(arVinesRoot.transform, cyanMat, magentaMat, goldMat);

            // 8. Pozo de Datos Hexagonal en el suelo ("Data-Well") a los pies del jugador (AR Salón Híbrido)
            GameObject floorDataWell = BuildFloorDataWellPond(roomRoot.transform, new Vector3(0f, 0.01f, 1.35f), obsidianMat, cyanMat, magentaMat);

            // 9. Parapeto / Escudo Balístico vertical de Cobertura Táctica (AR Salón Híbrido)
            GameObject tacticalCoverShield = BuildSpatialOpsCoverShield(roomRoot.transform, new Vector3(-0.95f, 0.0f, 1.85f), obsidianMat, obsidianMat, cyanMat, goldMat);

            // 10. Mundo VR Inmersivo Completo: Bulevar con Tiendas, Cafeterías y Megatorre de Radio Chipset 107.7 FM
            Transform vrSkyRing;
            GameObject fullVrRoot = BuildFullVrCyberpunkMegacity(roomRoot.transform, obsidianMat, cyanMat, magentaMat, goldMat, unlitShader, out vrSkyRing);

            // Configurar el componente CyberRoomDecorator3D
            CyberRoomDecorator3D decorator = roomRoot.AddComponent<CyberRoomDecorator3D>();
            SerializedObject soDec = new SerializedObject(decorator);
            soDec.FindProperty("arWallVinesRoot").objectReferenceValue = arVinesRoot.transform;
            soDec.FindProperty("floorDataWellRoot").objectReferenceValue = floorDataWell.transform;
            soDec.FindProperty("tacticalCoverShieldRoot").objectReferenceValue = tacticalCoverShield.transform;
            soDec.FindProperty("openWiredCeilingRing").objectReferenceValue = ceilingRing;
            soDec.FindProperty("arStageBackdrop").objectReferenceValue = flowStageObj;
            soDec.FindProperty("fullVrSkylineRoot").objectReferenceValue = fullVrRoot;
            soDec.FindProperty("vrOrbitalSkyRing").objectReferenceValue = vrSkyRing;
            soDec.FindProperty("startInFullVrMode").boolValue = true; // Iniciar en el Bulevar VR para el efecto Wow
            soDec.ApplyModifiedPropertiesWithoutUndo();

            return roomRoot;
        }

        /// <summary>
        /// Construye las ramificaciones 3D de fibra óptica y nodos de circuito sobre las paredes ampliadas.
        /// </summary>
        private static void BuildWallCircuitVines(Transform parent, Material cyanMat, Material magentaMat, Material goldMat)
        {
            System.Random rng = new System.Random(2077);
            for (int b = 0; b < 24; b++)
            {
                bool onFrontWall = (b < 14);
                float x = onFrontWall ? (-4.5f + (b * 0.68f)) : ((b % 2 == 0) ? -5.65f : 5.65f);
                float y = 0.75f + (float)rng.NextDouble() * 3.1f;
                float z = onFrontWall ? 6.08f : (1.5f + (b - 14) * 0.45f);
                float angle = onFrontWall ? (-35f + (b % 3) * 35f) : 25f;

                GameObject branch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                branch.name = $"CyberVine_Branch_{b}";
                branch.transform.SetParent(parent, false);
                branch.transform.localPosition = new Vector3(x, y, z);
                branch.transform.localRotation = onFrontWall ? Quaternion.Euler(0f, 0f, angle) : Quaternion.Euler(angle, 0f, 0f);
                branch.transform.localScale = onFrontWall ? new Vector3(0.65f, 0.035f, 0.025f) : new Vector3(0.025f, 0.035f, 0.65f);
                branch.GetComponent<MeshRenderer>().sharedMaterial = (b % 3 == 0) ? magentaMat : cyanMat;
                UnityEngine.Object.DestroyImmediate(branch.GetComponent<Collider>());

                // Nodo luminoso hexagonal/esférico en el extremo de cada rama (como las hojas/cristales de Mythic Realms)
                GameObject leafNode = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leafNode.name = $"CyberVine_Node_{b}";
                leafNode.transform.SetParent(parent, false);
                leafNode.transform.localPosition = new Vector3(x, y + 0.16f, z - (onFrontWall ? 0.03f : 0f));
                leafNode.transform.localScale = new Vector3(0.095f, 0.095f, 0.095f);
                leafNode.GetComponent<MeshRenderer>().sharedMaterial = (b % 4 == 0) ? goldMat : cyanMat;
                UnityEngine.Object.DestroyImmediate(leafNode.GetComponent<Collider>());
            }
        }

        /// <summary>
        /// Construye el Pozo de Datos Hexagonal en el suelo (inspirado en el estanque de rocas y setas de Mythic Realms Imagen 1).
        /// </summary>
        private static GameObject BuildFloorDataWellPond(Transform parent, Vector3 pos, Material obsidianMat, Material cyanMat, Material magentaMat)
        {
            GameObject wellRoot = new GameObject("Floor_DataWell_Root");
            wellRoot.transform.SetParent(parent, false);
            wellRoot.transform.localPosition = pos;

            // Disco de energía líquida cian en el centro
            GameObject poolCore = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            poolCore.name = "LiquidDataCore";
            poolCore.transform.SetParent(wellRoot.transform, false);
            poolCore.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            poolCore.transform.localScale = new Vector3(0.78f, 0.018f, 0.78f);
            poolCore.GetComponent<MeshRenderer>().sharedMaterial = cyanMat;
            UnityEngine.Object.DestroyImmediate(poolCore.GetComponent<Collider>());

            // Bloques perimetrales de blindaje (como las piedras alrededor del estanque en Mythic Realms)
            int segments = 10;
            for (int i = 0; i < segments; i++)
            {
                float ang = i * (360f / segments) * Mathf.Deg2Rad;
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rock.name = $"DataWell_RimBlock_{i}";
                rock.transform.SetParent(wellRoot.transform, false);
                rock.transform.localPosition = new Vector3(Mathf.Cos(ang) * 0.44f, 0.055f, Mathf.Sin(ang) * 0.44f);
                rock.transform.localRotation = Quaternion.Euler(0f, -i * (360f / segments), 0f);
                rock.transform.localScale = new Vector3(0.14f, 0.09f, 0.22f);
                rock.GetComponent<MeshRenderer>().sharedMaterial = (i % 3 == 0) ? magentaMat : obsidianMat;
                UnityEngine.Object.DestroyImmediate(rock.GetComponent<Collider>());
            }

            return wellRoot;
        }

        /// <summary>
        /// Construye el Escudo / Parapeto Balístico vertical de cobertura física (inspirado en Spatial Ops Imagen 5).
        /// </summary>
        private static GameObject BuildSpatialOpsCoverShield(Transform parent, Vector3 pos, Material obsidianMat, Material armorMat, Material cyanMat, Material goldMat)
        {
            GameObject shieldRoot = new GameObject("TacticalCoverShield_Root");
            shieldRoot.transform.SetParent(parent, false);
            shieldRoot.transform.localPosition = pos;
            shieldRoot.transform.localRotation = Quaternion.Euler(0f, -16f, 0f);

            // Placa blindada inferior
            GameObject lowerPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lowerPlate.name = "BallisticPlate_Lower";
            lowerPlate.transform.SetParent(shieldRoot.transform, false);
            lowerPlate.transform.localPosition = new Vector3(0f, 0.42f, 0f);
            lowerPlate.transform.localScale = new Vector3(0.68f, 0.82f, 0.06f);
            lowerPlate.GetComponent<MeshRenderer>().sharedMaterial = armorMat;

            // Bordes de refuerzo con tiras de neón
            GameObject leftEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftEdge.name = "ShieldEdge_Left";
            leftEdge.transform.SetParent(shieldRoot.transform, false);
            leftEdge.transform.localPosition = new Vector3(-0.35f, 0.48f, -0.01f);
            leftEdge.transform.localScale = new Vector3(0.04f, 0.92f, 0.08f);
            leftEdge.GetComponent<MeshRenderer>().sharedMaterial = goldMat;
            UnityEngine.Object.DestroyImmediate(leftEdge.GetComponent<Collider>());

            GameObject rightEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightEdge.name = "ShieldEdge_Right";
            rightEdge.transform.SetParent(shieldRoot.transform, false);
            rightEdge.transform.localPosition = new Vector3(0.35f, 0.48f, -0.01f);
            rightEdge.transform.localScale = new Vector3(0.04f, 0.92f, 0.08f);
            rightEdge.GetComponent<MeshRenderer>().sharedMaterial = goldMat;
            UnityEngine.Object.DestroyImmediate(rightEdge.GetComponent<Collider>());

            // Visor holográfico superior para asomarse y disparar
            GameObject holoSlit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            holoSlit.name = "ShieldHoloVisor";
            holoSlit.transform.SetParent(shieldRoot.transform, false);
            holoSlit.transform.localPosition = new Vector3(0f, 0.92f, 0f);
            holoSlit.transform.localScale = new Vector3(0.58f, 0.14f, 0.02f);
            holoSlit.GetComponent<MeshRenderer>().sharedMaterial = cyanMat;
            UnityEngine.Object.DestroyImmediate(holoSlit.GetComponent<Collider>());

            return shieldRoot;
        }

        /// <summary>
        /// Construye el Gran Portal Circular 3D de Brecha Interdimensional en la pared (inspirado en Spatial Ops Imagen 5).
        /// </summary>
        private static DimensionalRift3D BuildSpatialOpsDimensionalRift(Transform parent, Vector3 pos, Material voidMat, Material cyanMat, Material magentaMat)
        {
            GameObject riftRoot = new GameObject("DimensionalRift_Portal3D");
            riftRoot.transform.SetParent(parent, false);
            riftRoot.transform.localPosition = pos;
            riftRoot.transform.localScale = Vector3.one * 1.45f;

            // Disco interior invisible para dejar ver el portal circular animado del vídeo de Google Flow
            GameObject voidDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            voidDisc.name = "RiftVoidCoreDisc";
            voidDisc.transform.SetParent(riftRoot.transform, false);
            voidDisc.transform.localPosition = Vector3.zero;
            voidDisc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            voidDisc.transform.localScale = new Vector3(1.15f, 0.02f, 1.15f);
            MeshRenderer voidMr = voidDisc.GetComponent<MeshRenderer>();
            if (voidMr != null) voidMr.enabled = false;
            UnityEngine.Object.DestroyImmediate(voidDisc.GetComponent<Collider>());

            // Anillo Circular Exterior Azul/Cian (Spatial Ops Portal Ring)
            Transform outerRing = CreateVerticalPortalRing("RiftOuterRing3D", riftRoot.transform, new Vector3(0f, 0f, -0.03f), 0.62f, 20, cyanMat);

            // Anillo Circular Interior Magenta de Sobrecarga
            Transform innerRing = CreateVerticalPortalRing("RiftInnerRing3D", riftRoot.transform, new Vector3(0f, 0f, -0.05f), 0.46f, 14, magentaMat);

            // Luz puntual del portal
            GameObject lightObj = new GameObject("RiftPortalLight3D");
            lightObj.transform.SetParent(riftRoot.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 0f, -0.35f);
            Light ptLight = lightObj.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(0f, 0.95f, 1f, 1f);
            ptLight.intensity = 3.2f;
            ptLight.range = 4.2f;

            DimensionalRift3D riftComp = riftRoot.AddComponent<DimensionalRift3D>();
            SerializedObject soRift = new SerializedObject(riftComp);
            soRift.FindProperty("portalRoot").objectReferenceValue = riftRoot.transform;
            soRift.FindProperty("outerRing").objectReferenceValue = outerRing;
            soRift.FindProperty("innerRing").objectReferenceValue = innerRing;
            soRift.FindProperty("voidCoreDisc").objectReferenceValue = voidDisc.transform;
            soRift.FindProperty("riftPointLight").objectReferenceValue = ptLight;
            soRift.ApplyModifiedPropertiesWithoutUndo();

            return riftComp;
        }

        private static Transform CreateVerticalPortalRing(string name, Transform parent, Vector3 localPos, float radius, int segments, Material mat)
        {
            GameObject ringRoot = new GameObject(name);
            ringRoot.transform.SetParent(parent, false);
            ringRoot.transform.localPosition = localPos;

            float arcLength = (2f * Mathf.PI * radius) / segments * 0.82f;
            for (int i = 0; i < segments; i++)
            {
                float angleDeg = i * (360f / segments);
                float angleRad = angleDeg * Mathf.Deg2Rad;
                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = $"PortalSeg_{i}";
                seg.transform.SetParent(ringRoot.transform, false);
                seg.transform.localPosition = new Vector3(Mathf.Cos(angleRad) * radius, Mathf.Sin(angleRad) * radius, 0f);
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg + 90f);
                seg.transform.localScale = new Vector3(arcLength, 0.045f, 0.045f);
                seg.GetComponent<MeshRenderer>().sharedMaterial = mat;
                UnityEngine.Object.DestroyImmediate(seg.GetComponent<Collider>());
            }

            return ringRoot.transform;
        }

        /// <summary>
        /// Construye un Cofre / Caja de Suministros 3D con el icono holográfico '+' flotando encima (Spatial Ops Imagen 5).
        /// </summary>
        private static CyberSupplyChest BuildSpatialOpsSupplyChest(Transform parent, string name, Vector3 pos, Material boxMat, Material cyanMat, Material greenCrossMat)
        {
            GameObject chestRoot = new GameObject(name);
            chestRoot.transform.SetParent(parent, false);
            chestRoot.transform.localPosition = pos;

            // Collider para poder abrirlo de un disparo con el Blaster
            BoxCollider col = chestRoot.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.25f, 0f);
            col.size = new Vector3(0.48f, 0.55f, 0.38f);

            // Base de la caja militar cyberpunk
            GameObject baseBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            baseBox.name = "ChestBase3D";
            baseBox.transform.SetParent(chestRoot.transform, false);
            baseBox.transform.localPosition = new Vector3(0f, 0.14f, 0f);
            baseBox.transform.localScale = new Vector3(0.42f, 0.28f, 0.30f);
            baseBox.GetComponent<MeshRenderer>().sharedMaterial = boxMat;
            UnityEngine.Object.DestroyImmediate(baseBox.GetComponent<Collider>());

            // Franja frontal de neón cian
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "ChestNeonTrim";
            trim.transform.SetParent(baseBox.transform, false);
            trim.transform.localPosition = new Vector3(0f, 0.1f, -0.52f);
            trim.transform.localScale = new Vector3(0.85f, 0.12f, 0.05f);
            trim.GetComponent<MeshRenderer>().sharedMaterial = cyanMat;
            UnityEngine.Object.DestroyImmediate(trim.GetComponent<Collider>());

            // Tapa abatible del cofre
            GameObject lidPivot = new GameObject("ChestLidPivot");
            lidPivot.transform.SetParent(chestRoot.transform, false);
            lidPivot.transform.localPosition = new Vector3(0f, 0.28f, 0.15f);

            GameObject lidMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lidMesh.name = "ChestLidMesh";
            lidMesh.transform.SetParent(lidPivot.transform, false);
            lidMesh.transform.localPosition = new Vector3(0f, 0.03f, -0.15f);
            lidMesh.transform.localScale = new Vector3(0.44f, 0.06f, 0.32f);
            lidMesh.GetComponent<MeshRenderer>().sharedMaterial = boxMat;
            UnityEngine.Object.DestroyImmediate(lidMesh.GetComponent<Collider>());

            // Cruz Holográfica Flotante '+' (Exactamente como en la captura de Spatial Ops)
            GameObject holoPlus = new GameObject("HoloPlusCross3D");
            holoPlus.transform.SetParent(chestRoot.transform, false);
            holoPlus.transform.localPosition = new Vector3(0f, 0.54f, 0f);

            GameObject vBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vBar.name = "Plus_V";
            vBar.transform.SetParent(holoPlus.transform, false);
            vBar.transform.localScale = new Vector3(0.055f, 0.20f, 0.035f);
            vBar.GetComponent<MeshRenderer>().sharedMaterial = greenCrossMat;
            UnityEngine.Object.DestroyImmediate(vBar.GetComponent<Collider>());

            GameObject hBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            hBar.name = "Plus_H";
            hBar.transform.SetParent(holoPlus.transform, false);
            hBar.transform.localScale = new Vector3(0.20f, 0.055f, 0.035f);
            hBar.GetComponent<MeshRenderer>().sharedMaterial = greenCrossMat;
            UnityEngine.Object.DestroyImmediate(hBar.GetComponent<Collider>());

            CyberSupplyChest chestComp = chestRoot.AddComponent<CyberSupplyChest>();
            chestComp.chestLidTransform = lidPivot.transform;
            chestComp.holographicFlotador = holoPlus;

            return chestComp;
        }

        /// <summary>
        /// Construye el Bulevar Inmersivo Cyberpunk 360° para el Mundo Principal VR.
        /// Diseñado para El taller de la gatita tech:
        /// - Asfalto y aceras del Bulevar con marcas de tráfico neón.
        /// - Tiendas y cafeterías laterales (CYBER-RAMEN, TECH-FORGE, WIRED CAFE, NEO-PHARMA) con terrazas y toldos neón.
        /// - Farolas cibernéticas iluminando la calle.
        /// - Megatorre monumental de Radio Chipset 107.7 FM al final de la perspectiva con la Pantalla Gigante 16:9
        ///   animada con los fotogramas de Mina y Kira transmitiendo noticias y música.
        /// - Skyline 360° de rascacielos perimetrales y anillo orbital estelar.
        /// </summary>
        private static GameObject BuildFullVrCyberpunkMegacity(Transform parent, Material towerMat, Material cyanMat, Material magentaMat, Material goldMat, Shader unlitShader, out Transform skyRingOut)
        {
            GameObject vrRoot = new GameObject("FullVR_MegacitySkyline_Root");
            vrRoot.transform.SetParent(parent, false);

            // 1. Asfalto y Calzada Central del Bulevar (Calle transitable en VR)
            GameObject street = GameObject.CreatePrimitive(PrimitiveType.Cube);
            street.name = "VR_Boulevard_StreetRoad";
            street.transform.SetParent(vrRoot.transform, false);
            street.transform.localPosition = new Vector3(0f, -0.04f, 8.0f);
            street.transform.localScale = new Vector3(6.8f, 0.06f, 26.0f);
            street.GetComponent<MeshRenderer>().sharedMaterial = towerMat;

            // Línea central de tráfico neón cian
            GameObject centerLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            centerLine.name = "VR_Street_CenterStripe_Cyan";
            centerLine.transform.SetParent(street.transform, false);
            centerLine.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            centerLine.transform.localScale = new Vector3(0.025f, 0.1f, 0.95f);
            centerLine.GetComponent<MeshRenderer>().sharedMaterial = cyanMat;
            UnityEngine.Object.DestroyImmediate(centerLine.GetComponent<Collider>());

            // Líneas de carril neón magenta
            for (int s = -1; s <= 1; s += 2)
            {
                GameObject laneLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
                laneLine.name = $"VR_Street_LaneStripe_{s}";
                laneLine.transform.SetParent(street.transform, false);
                laneLine.transform.localPosition = new Vector3(s * 0.42f, 0.52f, 0f);
                laneLine.transform.localScale = new Vector3(0.015f, 0.1f, 0.95f);
                laneLine.GetComponent<MeshRenderer>().sharedMaterial = magentaMat;
                UnityEngine.Object.DestroyImmediate(laneLine.GetComponent<Collider>());
            }

            // 2. Aceras Peatonales Elevadas a ambos lados
            for (int side = -1; side <= 1; side += 2)
            {
                string sideName = (side < 0) ? "Left" : "Right";
                GameObject sidewalk = GameObject.CreatePrimitive(PrimitiveType.Cube);
                sidewalk.name = $"VR_Sidewalk_{sideName}";
                sidewalk.transform.SetParent(vrRoot.transform, false);
                sidewalk.transform.localPosition = new Vector3(side * 4.6f, 0.03f, 8.0f);
                sidewalk.transform.localScale = new Vector3(2.6f, 0.12f, 26.0f);
                sidewalk.GetComponent<MeshRenderer>().sharedMaterial = towerMat;

                // Tira luminosa en el bordillo de la acera
                GameObject curbNeon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curbNeon.name = "CurbNeonStrip";
                curbNeon.transform.SetParent(sidewalk.transform, false);
                curbNeon.transform.localPosition = new Vector3((side < 0) ? 0.48f : -0.48f, 0.52f, 0f);
                curbNeon.transform.localScale = new Vector3(0.04f, 0.12f, 0.98f);
                curbNeon.GetComponent<MeshRenderer>().sharedMaterial = (side < 0) ? cyanMat : magentaMat;
                UnityEngine.Object.DestroyImmediate(curbNeon.GetComponent<Collider>());
            }

            // 3. Farolas Cyberpunk a lo largo del Bulevar (4 por lado)
            float[] lampZ = { 0.5f, 4.5f, 8.5f, 12.5f };
            for (int side = -1; side <= 1; side += 2)
            {
                string sCode = (side < 0) ? "L" : "R";
                for (int l = 0; l < lampZ.Length; l++)
                {
                    GameObject lampPost = new GameObject($"VR_StreetLamp_{sCode}_{l}");
                    lampPost.transform.SetParent(vrRoot.transform, false);
                    lampPost.transform.localPosition = new Vector3(side * 3.4f, 0f, lampZ[l]);

                    // Poste vertical
                    GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pole.name = "LampPole";
                    pole.transform.SetParent(lampPost.transform, false);
                    pole.transform.localPosition = new Vector3(0f, 1.4f, 0f);
                    pole.transform.localScale = new Vector3(0.09f, 1.4f, 0.09f);
                    pole.GetComponent<MeshRenderer>().sharedMaterial = towerMat;
                    UnityEngine.Object.DestroyImmediate(pole.GetComponent<Collider>());

                    // Brazo horizontal hacia la calle
                    GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    arm.name = "LampArm";
                    arm.transform.SetParent(lampPost.transform, false);
                    arm.transform.localPosition = new Vector3(side * -0.32f, 2.78f, 0f);
                    arm.transform.localScale = new Vector3(0.75f, 0.06f, 0.08f);
                    arm.GetComponent<MeshRenderer>().sharedMaterial = towerMat;
                    UnityEngine.Object.DestroyImmediate(arm.GetComponent<Collider>());

                    // Foco emisor de luz neón
                    GameObject bulb = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bulb.name = "LampBulb";
                    bulb.transform.SetParent(lampPost.transform, false);
                    bulb.transform.localPosition = new Vector3(side * -0.65f, 2.68f, 0f);
                    bulb.transform.localScale = new Vector3(0.24f, 0.09f, 0.24f);
                    bulb.GetComponent<MeshRenderer>().sharedMaterial = (l % 2 == 0) ? cyanMat : magentaMat;
                    UnityEngine.Object.DestroyImmediate(bulb.GetComponent<Collider>());
                }
            }

            // 4. Tiendas y Cafeterías a la Izquierda del Bulevar
            // A) CYBER-RAMEN // 24H (z = 2.5m)
            GameObject ramenShop = BuildVRLobbyStorefront(
                vrRoot.transform,
                "VR_Shop_CyberRamen_24H",
                new Vector3(-5.9f, 2.1f, 2.5f),
                new Vector3(3.8f, 4.2f, 3.8f),
                towerMat, magentaMat, goldMat,
                "RAMEN // 24H", true, false);

            // B) TECH-FORGE // CYBERWARE & WEAPONS (z = 7.5m)
            GameObject techForge = BuildVRLobbyStorefront(
                vrRoot.transform,
                "VR_Shop_TechForge_Cyberware",
                new Vector3(-5.9f, 2.4f, 7.5f),
                new Vector3(3.8f, 4.8f, 4.2f),
                towerMat, cyanMat, goldMat,
                "TECH-FORGE", false, false);

            // 5. Tiendas y Cafeterías a la Derecha del Bulevar
            // C) WIRED CAFE // SYNTH & COFFEE (z = 2.5m)
            GameObject wiredCafe = BuildVRLobbyStorefront(
                vrRoot.transform,
                "VR_Shop_WiredCafe_SynthCoffee",
                new Vector3(5.9f, 2.1f, 2.5f),
                new Vector3(3.8f, 4.2f, 3.8f),
                towerMat, cyanMat, magentaMat,
                "WIRED CAFE", false, true);

            // D) NEO-NEXUS // PHARMACY (z = 7.5m)
            GameObject neoPharma = BuildVRLobbyStorefront(
                vrRoot.transform,
                "VR_Shop_NeoNexus_Pharmacy",
                new Vector3(5.9f, 2.4f, 7.5f),
                new Vector3(3.8f, 4.8f, 4.2f),
                towerMat, cyanMat, goldMat,
                "NEO-PHARMA", false, false);

            // 6. Megatorre Monumental de Radio Chipset 107.7 FM al Fondo (z = 15.0m)
            GameObject megatowerRoot = new GameObject("VR_Megatower_RadioChipset_Root");
            megatowerRoot.transform.SetParent(vrRoot.transform, false);
            megatowerRoot.transform.localPosition = new Vector3(0f, 0f, 15.0f);

            // Cuerpo del Rascacielos Central
            GameObject towerMain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            towerMain.name = "Megatower_MainBuilding";
            towerMain.transform.SetParent(megatowerRoot.transform, false);
            towerMain.transform.localPosition = new Vector3(0f, 13.0f, 1.2f);
            towerMain.transform.localScale = new Vector3(14.0f, 26.0f, 4.5f);
            towerMain.GetComponent<MeshRenderer>().sharedMaterial = towerMat;

            // Torres Aguja Gemelas Flanqueando la Megatorre
            for (int side = -1; side <= 1; side += 2)
            {
                string spName = (side < 0) ? "Left" : "Right";
                GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spire.name = $"Megatower_Spire_{spName}";
                spire.transform.SetParent(megatowerRoot.transform, false);
                spire.transform.localPosition = new Vector3(side * 8.6f, 16.0f, 1.0f);
                spire.transform.localScale = new Vector3(4.5f, 32.0f, 5.0f);
                spire.GetComponent<MeshRenderer>().sharedMaterial = towerMat;

                // Tira luminosa vertical de la aguja
                GameObject spireNeon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                spireNeon.name = "SpireNeon_Strip";
                spireNeon.transform.SetParent(spire.transform, false);
                spireNeon.transform.localPosition = new Vector3(0f, 0f, -0.51f);
                spireNeon.transform.localScale = new Vector3(0.08f, 0.95f, 0.02f);
                spireNeon.GetComponent<MeshRenderer>().sharedMaterial = (side < 0) ? cyanMat : magentaMat;
                UnityEngine.Object.DestroyImmediate(spireNeon.GetComponent<Collider>());
            }

            // Circuitos Gigantes en la fachada de la Megatorre
            for (int c = 0; c < 8; c++)
            {
                float cx = -5.0f + (c * 1.42f);
                GameObject towerCircuit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                towerCircuit.name = $"Megatower_Circuit_{c}";
                towerCircuit.transform.SetParent(towerMain.transform, false);
                towerCircuit.transform.localPosition = new Vector3(cx / 14.0f, 0.15f, -0.51f);
                towerCircuit.transform.localScale = new Vector3(0.02f, 0.65f, 0.015f);
                towerCircuit.GetComponent<MeshRenderer>().sharedMaterial = (c % 2 == 0) ? cyanMat : magentaMat;
                UnityEngine.Object.DestroyImmediate(towerCircuit.GetComponent<Collider>());
            }

            // 7. Pantalla Gigante 16:9 de Radio Chipset 107.7 FM con Mina y Kira
            // Cargar frames de la torre
            List<Sprite> towerSprites = new List<Sprite>();
            List<Texture2D> towerTexs = new List<Texture2D>();
            for (int i = 0; i < 12; i++)
            {
                string tPath = $"Assets/Sprites/Environment/VRLobby/vr_lobby_tower_{i:02d}.png";
                AssetDatabase.ImportAsset(tPath, ImportAssetOptions.ForceUpdate);
                Sprite sp = ForceLoadSprite(tPath);
                if (sp != null) towerSprites.Add(sp);
                Texture2D tx = AssetDatabase.LoadAssetAtPath<Texture2D>(tPath);
                if (tx != null) towerTexs.Add(tx);
            }

            Shader texShader = Shader.Find("Unlit/Texture");
            if (texShader == null) texShader = unlitShader;

            string towerMatPath = "Assets/Prefabs/Mat_3D_VRLobbyTowerScreen.mat";
            Material towerScreenMat = AssetDatabase.LoadAssetAtPath<Material>(towerMatPath);
            if (towerScreenMat == null)
            {
                towerScreenMat = new Material(texShader);
                AssetDatabase.CreateAsset(towerScreenMat, towerMatPath);
            }
            else
            {
                towerScreenMat.shader = texShader;
            }
            if (towerTexs.Count > 0) towerScreenMat.mainTexture = towerTexs[0];
            towerScreenMat.renderQueue = 2000;
            EditorUtility.SetDirty(towerScreenMat);

            GameObject screenObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screenObj.name = "Megatower_BroadcastScreen";
            screenObj.transform.SetParent(megatowerRoot.transform, false);
            screenObj.transform.localPosition = new Vector3(0f, 4.8f, -1.18f);
            screenObj.transform.localScale = new Vector3(9.6f, 5.4f, 1.0f); // 16:9 exacto
            UnityEngine.Object.DestroyImmediate(screenObj.GetComponent<Collider>());

            MeshRenderer screenMr = screenObj.GetComponent<MeshRenderer>();
            screenMr.sharedMaterial = towerScreenMat;

            FlowRoomBackdropAnimator towerAnim = screenObj.AddComponent<FlowRoomBackdropAnimator>();
            SerializedObject soTower = new SerializedObject(towerAnim);
            soTower.FindProperty("targetMeshRenderer").objectReferenceValue = screenMr;
            soTower.FindProperty("framesPerSecond").floatValue = 8f;
            SerializedProperty txArr2 = soTower.FindProperty("flowFrames");
            if (txArr2 != null)
            {
                txArr2.arraySize = towerTexs.Count;
                for (int i = 0; i < towerTexs.Count; i++)
                {
                    txArr2.GetArrayElementAtIndex(i).objectReferenceValue = towerTexs[i];
                }
            }
            SerializedProperty spArr2 = soTower.FindProperty("flowSprites");
            if (spArr2 != null)
            {
                spArr2.arraySize = towerSprites.Count;
                for (int i = 0; i < towerSprites.Count; i++)
                {
                    spArr2.GetArrayElementAtIndex(i).objectReferenceValue = towerSprites[i];
                }
            }
            soTower.ApplyModifiedPropertiesWithoutUndo();

            // Marco 3D de la Pantalla Gigante con Borde Neón
            GameObject screenFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screenFrame.name = "Screen_NeonBezel";
            screenFrame.transform.SetParent(screenObj.transform, false);
            screenFrame.transform.localPosition = new Vector3(0f, 0f, 0.02f);
            screenFrame.transform.localScale = new Vector3(1.05f, 1.08f, 0.05f);
            screenFrame.GetComponent<MeshRenderer>().sharedMaterial = towerMat;
            UnityEngine.Object.DestroyImmediate(screenFrame.GetComponent<Collider>());

            // Rótulo 3D Superior: RADIO CHIPSET // 107.7 FM
            GameObject signObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signObj.name = "Megatower_HeaderSign_RadioChipset";
            signObj.transform.SetParent(screenObj.transform, false);
            signObj.transform.localPosition = new Vector3(0f, 0.58f, -0.05f);
            signObj.transform.localScale = new Vector3(0.85f, 0.12f, 0.08f);
            signObj.GetComponent<MeshRenderer>().sharedMaterial = cyanMat;
            UnityEngine.Object.DestroyImmediate(signObj.GetComponent<Collider>());

            // 8. Rascacielos Perimetrales 360° en el Horizonte (Megaciudad Circundante)
            int towerCount = 28;
            float radius = 18.0f;
            for (int i = 0; i < towerCount; i++)
            {
                float ang = i * (360f / towerCount) * Mathf.Deg2Rad;
                float h = 5.5f + (i % 6) * 2.2f;
                float w = 1.4f + (i % 3) * 0.55f;

                GameObject tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tower.name = $"VR_Skyscraper_{i}";
                tower.transform.SetParent(vrRoot.transform, false);
                tower.transform.localPosition = new Vector3(Mathf.Cos(ang) * radius, h * 0.5f - 0.5f, 8.0f + Mathf.Sin(ang) * radius);
                tower.transform.localRotation = Quaternion.Euler(0f, -i * (360f / towerCount), 0f);
                tower.transform.localScale = new Vector3(w, h, w);
                tower.GetComponent<MeshRenderer>().sharedMaterial = towerMat;
                UnityEngine.Object.DestroyImmediate(tower.GetComponent<Collider>());

                // Tira de neón en la fachada de cada rascacielos
                GameObject neon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                neon.name = $"VR_TowerNeon_{i}";
                neon.transform.SetParent(tower.transform, false);
                neon.transform.localPosition = new Vector3(0f, 0.1f, -0.51f);
                neon.transform.localScale = new Vector3(0.18f, 0.85f, 0.02f);
                neon.GetComponent<MeshRenderer>().sharedMaterial = (i % 3 == 0) ? magentaMat : ((i % 2 == 0) ? cyanMat : goldMat);
                UnityEngine.Object.DestroyImmediate(neon.GetComponent<Collider>());
            }

            // 9. Anillo Orbital Estelar Superior 360°
            skyRingOut = CreateSegmented3DRing("VR_OrbitalSkyRing360", vrRoot.transform, new Vector3(0f, 12.0f, 8.0f), 14.5f, 32, magentaMat);
            vrRoot.SetActive(true); // Activo por defecto para el arranque inicial en VR
            return vrRoot;
        }

        /// <summary>
        /// Construye un bloque de local / tienda comercial para las aceras del bulevar VR.
        /// </summary>
        private static GameObject BuildVRLobbyStorefront(
            Transform parent,
            string name,
            Vector3 localPos,
            Vector3 size,
            Material facadeMat,
            Material neonMat,
            Material accentMat,
            string signText,
            bool hasRamenCounter,
            bool hasCafeTerrace)
        {
            GameObject shopRoot = new GameObject(name);
            shopRoot.transform.SetParent(parent, false);
            shopRoot.transform.localPosition = localPos;

            // Fachada principal del local
            GameObject facade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            facade.name = "BuildingFacade";
            facade.transform.SetParent(shopRoot.transform, false);
            facade.transform.localPosition = Vector3.zero;
            facade.transform.localScale = size;
            facade.GetComponent<MeshRenderer>().sharedMaterial = facadeMat;

            // Toldo / Marquesina voladiza en ángulo
            GameObject awning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            awning.name = "AwningCanopy";
            awning.transform.SetParent(shopRoot.transform, false);
            float facingDir = (localPos.x < 0) ? 1.0f : -1.0f; // hacia la calle
            awning.transform.localPosition = new Vector3(facingDir * (size.x * 0.5f + 0.35f), -0.2f, 0f);
            awning.transform.localRotation = Quaternion.Euler(0f, 0f, facingDir * 18f);
            awning.transform.localScale = new Vector3(0.95f, 0.06f, size.z * 0.92f);
            awning.GetComponent<MeshRenderer>().sharedMaterial = neonMat;
            UnityEngine.Object.DestroyImmediate(awning.GetComponent<Collider>());

            // Rótulo luminoso 3D superior
            GameObject signBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            signBoard.name = $"ShopSign_{signText.Replace(' ', '_')}";
            signBoard.transform.SetParent(shopRoot.transform, false);
            signBoard.transform.localPosition = new Vector3(facingDir * (size.x * 0.5f + 0.12f), size.y * 0.38f, 0f);
            signBoard.transform.localScale = new Vector3(0.14f, 0.42f, size.z * 0.75f);
            signBoard.GetComponent<MeshRenderer>().sharedMaterial = accentMat;
            UnityEngine.Object.DestroyImmediate(signBoard.GetComponent<Collider>());

            // Elementos temáticos
            if (hasRamenCounter)
            {
                // Barra de ramen exterior
                GameObject counter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                counter.name = "RamenCounterBar";
                counter.transform.SetParent(shopRoot.transform, false);
                counter.transform.localPosition = new Vector3(facingDir * (size.x * 0.5f + 0.45f), -size.y * 0.32f, 0f);
                counter.transform.localScale = new Vector3(0.55f, 0.72f, size.z * 0.75f);
                counter.GetComponent<MeshRenderer>().sharedMaterial = facadeMat;

                // Taburetes cilíndricos
                for (int s = -1; s <= 1; s++)
                {
                    GameObject stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stool.name = $"RamenStool_{s}";
                    stool.transform.SetParent(shopRoot.transform, false);
                    stool.transform.localPosition = new Vector3(facingDir * (size.x * 0.5f + 0.95f), -size.y * 0.36f, s * 0.95f);
                    stool.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);
                    stool.GetComponent<MeshRenderer>().sharedMaterial = neonMat;
                    UnityEngine.Object.DestroyImmediate(stool.GetComponent<Collider>());
                }
            }

            if (hasCafeTerrace)
            {
                // Terraza con mesa cyberpunk y asientos
                for (int t = -1; t <= 1; t += 2)
                {
                    GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    table.name = $"CafeTable_{t}";
                    table.transform.SetParent(shopRoot.transform, false);
                    table.transform.localPosition = new Vector3(facingDir * (size.x * 0.5f + 0.85f), -size.y * 0.32f, t * 1.1f);
                    table.transform.localScale = new Vector3(0.55f, 0.38f, 0.55f);
                    table.GetComponent<MeshRenderer>().sharedMaterial = accentMat;
                    UnityEngine.Object.DestroyImmediate(table.GetComponent<Collider>());
                }
            }

            return shopRoot;
        }

        private static void EnhanceRadioStation3D(GameObject radio, Material obsidianMat, Material cyanMat, Material magentaMat)
        {
            Transform oldHolo3D = radio.transform.Find("HoloDeck3D_Structure");
            if (oldHolo3D != null) UnityEngine.Object.DestroyImmediate(oldHolo3D.gameObject);

            Transform oldPedestal = radio.transform.Find("HoloEmitterPedestal");
            if (oldPedestal != null) oldPedestal.gameObject.SetActive(false);

            GameObject holoStruct = new GameObject("HoloDeck3D_Structure");
            holoStruct.transform.SetParent(radio.transform, false);

            GameObject column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            column.name = "TableColumn3D";
            column.transform.SetParent(holoStruct.transform, false);
            column.transform.localPosition = new Vector3(0f, -0.52f, 0f);
            column.transform.localScale = new Vector3(0.32f, 0.48f, 0.32f);
            column.GetComponent<MeshRenderer>().sharedMaterial = obsidianMat;

            GameObject topDisk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            topDisk.name = "HoloProjectorDeck3D";
            topDisk.transform.SetParent(holoStruct.transform, false);
            topDisk.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            topDisk.transform.localScale = new Vector3(0.82f, 0.035f, 0.82f);
            topDisk.GetComponent<MeshRenderer>().sharedMaterial = obsidianMat;

            Transform ringOuter = CreateSegmented3DRing("OuterNeonRing3D", holoStruct.transform, new Vector3(0f, -0.02f, 0f), 0.44f, 12, cyanMat);
            Transform ringInner = CreateSegmented3DRing("InnerNeonRing3D", holoStruct.transform, new Vector3(0f, 0.01f, 0f), 0.32f, 8, magentaMat);

            Transform[] eqBars = new Transform[12];
            for (int i = 0; i < 12; i++)
            {
                GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bar.name = $"EQ_Bar3D_{i}";
                bar.transform.SetParent(holoStruct.transform, false);
                float xOffset = (i - 5.5f) * 0.065f;
                float h = 0.08f + Mathf.Abs(Mathf.Sin(i * 0.7f)) * 0.22f;
                bar.transform.localPosition = new Vector3(xOffset, h * 0.5f, 0.12f);
                bar.transform.localScale = new Vector3(0.035f, h, 0.035f);
                bar.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? cyanMat : magentaMat;
                UnityEngine.Object.DestroyImmediate(bar.GetComponent<Collider>());
                eqBars[i] = bar.transform;
            }

            GameObject lightObj = new GameObject("RadioPointLight3D");
            lightObj.transform.SetParent(holoStruct.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 0.35f, -0.2f);
            Light ptLight = lightObj.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(0f, 0.95f, 1f, 1f);
            ptLight.intensity = 2.2f;
            ptLight.range = 2.5f;

            Transform mina = radio.transform.Find("Hologram_Mina");
            Transform kira = radio.transform.Find("Hologram_Kira");
            Transform cat = radio.transform.Find("Hologram_StudioCat");
            if (mina != null)
            {
                mina.localPosition = new Vector3(-0.22f, 0.36f, -0.02f);
                mina.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            }
            if (kira != null)
            {
                kira.localPosition = new Vector3(0.22f, 0.36f, -0.02f);
                kira.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            }
            if (cat != null) cat.gameObject.SetActive(false);

            CyberHologramEffects3D fx = radio.GetComponent<CyberHologramEffects3D>() ?? radio.AddComponent<CyberHologramEffects3D>();
            fx.rotatingRings = new[] { ringOuter, ringInner };
            fx.equalizerBars = eqBars;
        }

        private static void EnhanceNyxWith3DVolume(GameObject nyx, SpriteRenderer nyxSr, Material cyanMat, Material magentaMat)
        {
            Transform oldRig = nyx.transform.Find("Nyx3D_HoloRig");
            if (oldRig != null) UnityEngine.Object.DestroyImmediate(oldRig.gameObject);

            GameObject rig = new GameObject("Nyx3D_HoloRig");
            rig.transform.SetParent(nyx.transform, false);

            Transform baseRing = CreateSegmented3DRing("NyxAuraRing3D", rig.transform, new Vector3(0f, 0.03f, 0f), 0.35f, 10, cyanMat);

            Transform[] nodes = new Transform[4];
            for (int i = 0; i < 4; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"NyxOrbitalNode3D_{i}";
                sphere.transform.SetParent(rig.transform, false);
                float ang = i * 90f * Mathf.Deg2Rad;
                sphere.transform.localPosition = new Vector3(Mathf.Cos(ang) * 0.38f, 0.65f + (i % 2) * 0.25f, Mathf.Sin(ang) * 0.38f);
                sphere.transform.localScale = new Vector3(0.055f, 0.055f, 0.055f);
                sphere.GetComponent<MeshRenderer>().sharedMaterial = (i % 2 == 0) ? cyanMat : magentaMat;
                UnityEngine.Object.DestroyImmediate(sphere.GetComponent<Collider>());
                nodes[i] = sphere.transform;
            }

            GameObject lightObj = new GameObject("NyxAuraLight3D");
            lightObj.transform.SetParent(rig.transform, false);
            lightObj.transform.localPosition = new Vector3(0f, 0.85f, -0.25f);
            Light l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(0f, 1f, 0.85f, 1f);
            l.intensity = 0.15f;
            l.range = 2.2f;

            CyberHologramEffects3D fx = nyx.GetComponent<CyberHologramEffects3D>() ?? nyx.AddComponent<CyberHologramEffects3D>();
            fx.rotatingRings = new[] { baseRing };
            fx.orbitalNodes = nodes;
            fx.targetSpriteRenderer = nyxSr;
            fx.turn360Sprites = LoadTurn360Sprites("Assets/Sprites/Nyx");
        }

        private static void EnhanceBossWith3DArena(GameObject boss, SpriteRenderer bossSr, Material cyanMat, Material magentaMat, Material goldMat)
        {
            Transform oldArena = boss.transform.Find("Boss3D_ArenaRig");
            if (oldArena != null) UnityEngine.Object.DestroyImmediate(oldArena.gameObject);

            GameObject arena = new GameObject("Boss3D_ArenaRig");
            arena.transform.SetParent(boss.transform, false);

            Transform outerRing = CreateSegmented3DRing("BossOuterRing3D", arena.transform, new Vector3(0f, 0.03f, 0f), 0.95f, 16, magentaMat);
            Transform innerRing = CreateSegmented3DRing("BossInnerRing3D", arena.transform, new Vector3(0f, 0.05f, 0f), 0.68f, 12, goldMat);

            GameObject reactorLightObj = new GameObject("CenturionReactorLight3D");
            reactorLightObj.transform.SetParent(arena.transform, false);
            reactorLightObj.transform.localPosition = new Vector3(0f, 1.35f, -0.35f);
            Light rLight = reactorLightObj.AddComponent<Light>();
            rLight.type = LightType.Point;
            rLight.color = new Color(0f, 0.95f, 1f, 1f);
            rLight.intensity = 3.5f;
            rLight.range = 3.8f;

            CyberHologramEffects3D fx = boss.GetComponent<CyberHologramEffects3D>() ?? boss.AddComponent<CyberHologramEffects3D>();
            fx.rotatingRings = new[] { outerRing, innerRing };
            fx.targetSpriteRenderer = bossSr;
            fx.turn360Sprites = LoadTurn360Sprites("Assets/Sprites/Enemies/Quantum_centurion");
        }

        private static GameObject CreatePreviewSpiderWith3DRing(GameObject prefab, Scene scene, string name, Vector3 pos, Transform cam, Material ringMat)
        {
            GameObject spider = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            spider.name = name;
            spider.transform.position = pos;
            OrientTowardsCamera(spider.transform, cam);

            CreateSegmented3DRing("SpiderTargetRing3D", spider.transform, new Vector3(0f, -0.22f, 0f), 0.34f, 8, ringMat);
            return spider;
        }

        private static Transform CreateSegmented3DRing(string name, Transform parent, Vector3 localPos, float radius, int segments, Material mat)
        {
            GameObject ringRoot = new GameObject(name);
            ringRoot.transform.SetParent(parent, false);
            ringRoot.transform.localPosition = localPos;

            float arcLength = (2f * Mathf.PI * radius) / segments * 0.72f;
            for (int i = 0; i < segments; i++)
            {
                float angleDeg = i * (360f / segments);
                float angleRad = angleDeg * Mathf.Deg2Rad;
                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = $"Seg_{i}";
                seg.transform.SetParent(ringRoot.transform, false);
                seg.transform.localPosition = new Vector3(Mathf.Cos(angleRad) * radius, 0f, Mathf.Sin(angleRad) * radius);
                seg.transform.localRotation = Quaternion.Euler(0f, -angleDeg, 0f);
                seg.transform.localScale = new Vector3(0.035f, 0.025f, arcLength);
                seg.GetComponent<MeshRenderer>().sharedMaterial = mat;
                UnityEngine.Object.DestroyImmediate(seg.GetComponent<Collider>());
            }

            return ringRoot.transform;
        }

        private static Sprite[] LoadTurn360Sprites(string folderPath)
        {
            Sprite[] arr = new Sprite[8];
            for (int i = 0; i < 8; i++)
            {
                arr[i] = AssetDatabase.LoadAssetAtPath<Sprite>($"{folderPath}/turn360_{i}.png");
            }
            return arr;
        }

        private static Material CreateOrUpdateMat(string assetPath, Shader shader, Color col)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (mat == null)
            {
                mat = new Material(shader);
                mat.color = col;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                mat.renderQueue = 2000;
                AssetDatabase.CreateAsset(mat, assetPath);
            }
            else
            {
                mat.shader = shader;
                mat.color = col;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
                mat.renderQueue = 2000;
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static GameObject FindInScene(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
            }
            return GameObject.Find(name);
        }

        private static void OrientTowardsCamera(Transform target, Transform cam)
        {
            Vector3 dir = cam.position - target.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                target.rotation = Quaternion.LookRotation(-dir);
            }
        }

        private static void RenderCameraToPng(Camera cam, int width, int height, string filePath)
        {
            RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            RenderTexture prevActive = RenderTexture.active;
            RenderTexture prevCamTarget = cam.targetTexture;
            CameraClearFlags prevClearFlags = cam.clearFlags;
            Color prevBg = cam.backgroundColor;

            try
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.03f, 0.04f, 0.08f, 1.0f);
                cam.targetTexture = rt;
                cam.Render();

                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex.Apply();

                byte[] pngBytes = tex.EncodeToPNG();
                File.WriteAllBytes(filePath, pngBytes);
                UnityEngine.Object.DestroyImmediate(tex);
            }
            finally
            {
                cam.targetTexture = prevCamTarget;
                cam.clearFlags = prevClearFlags;
                cam.backgroundColor = prevBg;
                RenderTexture.active = prevActive;
                rt.Release();
                UnityEngine.Object.DestroyImmediate(rt);
            }
        }
    }
}
