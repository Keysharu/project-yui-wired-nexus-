using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectNyx.Core;
using ProjectNyx.Companion;
using ProjectNyx.AudioRadio;
using ProjectNyx.Gameplay;
using ProjectNyx.NetworkBridge;
using NeoCyber.AR;

namespace ProjectNyx.EditorTools
{
    /// <summary>
    /// [ES] Orquestador Maestro de Escena y Prefabs para Meta Global AI Developer Hackathon 2026.
    /// Monta la escena completa (MRUK, OVRCameraRig, Guantelete, Radio 3D, Mente Colmena,
    /// HUD táctico y visuales) con un solo clic.
    /// ---
    /// [EN] Master Scene & Prefab Assembler for Meta Global AI Developer Hackathon 2026.
    /// Automatically constructs the entire MR scene (MRUK, OVRCameraRig, Holographic Gauntlet,
    /// 3D Radio Transistor, Hive Mind Boss, Tactical HUD, and visual experience) in a single click.
    /// </summary>
    [InitializeOnLoad]
    public static class WiredNexusSceneAssembler
    {
        private const string FlagFilePath = "ProjectSettings/WiredNexusSceneAssembled_v1.flag";
        private const string ReportFilePath = "ProjectSettings/WiredNexusAssemblyReport.txt";
        private const string PrefabsFolder = "Assets/Prefabs";
        private const string MainScenePath = "Assets/MainMRScene.unity";

        static WiredNexusSceneAssembler()
        {
            EditorApplication.delayCall += TryAutoAssembleOnLoad;
        }

        private static void TryAutoAssembleOnLoad()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            {
                return;
            }

            if (!File.Exists(FlagFilePath))
            {
                Debug.Log("[WiredNexusSceneAssembler] Iniciando montaje automático de MainMRScene y Prefabs...");
                AssembleFullProject();
            }
        }

                [MenuItem("Wired Nexus MR/0. Nivelar Vista Horizontal (Horizonte 0 grados)", false, 1)]
        public static void AlignSceneViewToHorizon()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.LookAt(new Vector3(0, 1.65f, 0), Quaternion.Euler(0, 0, 0), 8f);
                sceneView.Repaint();
                Debug.Log("[Wired Nexus MR] Vista de Scene nivelada horizontalmente a 0 grados.");
            }
        }

        [MenuItem("Wired Nexus MR/2. Montar Escena Completa y Prefabs (Fases 1 a 5)", false, 20)]
        public static void AssembleFullProject()
        {
            try
            {
                // FASE 1: Asegurar que todas las imágenes en Assets/Sprites sean Sprite (2D and UI)
                int spritesFixed = SpriteImportSettingsFixer.FixAllSprites();

                if (!AssetDatabase.IsValidFolder(PrefabsFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                }

                // Crear materiales básicos para LineRenderers y Escudo
                Material cyanLaserMat = CreateOrLoadUnlitColorMaterial("Assets/Prefabs/Mat_CyanLaser.mat", new Color(0f, 1f, 0.85f, 1f));
                Material magentaLaserMat = CreateOrLoadUnlitColorMaterial("Assets/Prefabs/Mat_MagentaLaser.mat", new Color(1f, 0.1f, 0.55f, 1f));
                Material holoShieldMat = CreateOrLoadTransparentMaterial("Assets/Prefabs/Mat_HoloShield.mat", new Color(0f, 0.95f, 1f, 0.35f));

                // FASE 2: Crear Prefab de Nyx Billboard Companion y Radio Radio Chipset 3D
                GameObject nyxPrefab = BuildNyxPrefab(cyanLaserMat);
                GameObject radioPrefab = BuildRadioStationPrefab();

                // FASE 4: Crear Prefabs de Spider-Drone y Quantum Centurion Boss
                GameObject spiderPrefab = BuildSpiderDronePrefab(magentaLaserMat);
                GameObject centurionPrefab = BuildQuantumCenturionPrefab(magentaLaserMat);

                // FASE 3 y 5: Ensamblar MainMRScene.unity con Rig XR, Manos, Armas, Nyx, Radio y Director de Oleadas
                AssembleMainMRScene(nyxPrefab, radioPrefab, spiderPrefab, centurionPrefab, cyanLaserMat, holoShieldMat);

                File.WriteAllText(FlagFilePath, DateTime.UtcNow.ToString("O"));
                string report = $"STATUS: OK\nTIMESTAMP: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nSPRITES_UPDATED: {spritesFixed}\nSCENE: {MainScenePath}\nPREFABS_CREATED: Nyx, RadioRadioChipset, SpiderDrone, QuantumCenturion\n";
                File.WriteAllText(ReportFilePath, report);

                Debug.Log("[WiredNexusSceneAssembler] ¡Montaje completo de Wired Nexus MR finalizado con 0 errores!");
            }
            catch (Exception ex)
            {
                string errReport = $"STATUS: ERROR\nMESSAGE: {ex.Message}\nSTACK: {ex.StackTrace}";
                File.WriteAllText(ReportFilePath, errReport);
                Debug.LogError($"[WiredNexusSceneAssembler] Error durante el montaje: {ex}");
            }
        }

        private static Material CreateOrLoadUnlitColorMaterial(string path, Color color)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Universal Render Pipeline/Unlit");
                mat = new Material(shader);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Material CreateOrLoadTransparentMaterial(string path, Color color)
        {
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
                mat = new Material(shader);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        private static Sprite LoadSprite(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static AudioClip LoadAudio(string assetPath)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static GameObject BuildNyxPrefab(Material laserMat)
        {
            GameObject root = new GameObject("Nyx_Billboard_Companion");

            NyxFloatingCompanion floating = root.AddComponent<NyxFloatingCompanion>();
            NyxAdaptationEngine adaptation = root.AddComponent<NyxAdaptationEngine>();
            NyxBillboardAnimator animator = root.AddComponent<NyxBillboardAnimator>();

            GameObject spriteChild = new GameObject("Nyx_Sprite25D");
            spriteChild.transform.SetParent(root.transform, false);
            spriteChild.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            SpriteRenderer sr = spriteChild.AddComponent<SpriteRenderer>();

            Sprite idle = LoadSprite("Assets/Sprites/Nyx/idle.png") ?? LoadSprite("Assets/Sprites/Nyx/view_front.png");
            sr.sprite = idle;

            LineRenderer lr = root.AddComponent<LineRenderer>();
            lr.startWidth = 0.02f;
            lr.endWidth = 0.008f;
            lr.material = laserMat;
            lr.enabled = false;

            SerializedObject soAnim = new SerializedObject(animator);
            soAnim.FindProperty("bodySpriteRenderer").objectReferenceValue = sr;
            soAnim.FindProperty("supportLaserLine").objectReferenceValue = lr;
            soAnim.FindProperty("idleSprite").objectReferenceValue = idle;
            soAnim.FindProperty("blinkSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Nyx/blink.png");
            soAnim.FindProperty("happySprite").objectReferenceValue = LoadSprite("Assets/Sprites/Nyx/happy.png");
            soAnim.FindProperty("surpriseSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Nyx/surprise.png");
            soAnim.FindProperty("thinkSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Nyx/think.png");
            soAnim.FindProperty("viewFrontSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Nyx/view_front.png");
            soAnim.FindProperty("viewSideSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Nyx/view_side.png");
            soAnim.FindProperty("viewBackSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Nyx/view_back.png");

            SetSpriteArray(soAnim.FindProperty("talkSprites"), new[]
            {
                LoadSprite("Assets/Sprites/Nyx/talk_1.png"),
                LoadSprite("Assets/Sprites/Nyx/talk_2.png"),
                LoadSprite("Assets/Sprites/Nyx/talk_3.png"),
                LoadSprite("Assets/Sprites/Nyx/talk_4.png"),
                LoadSprite("Assets/Sprites/Nyx/talk_5.png")
            });

            SetSpriteArray(soAnim.FindProperty("walkSprites"), new[]
            {
                LoadSprite("Assets/Sprites/Nyx/walk_1.png"),
                LoadSprite("Assets/Sprites/Nyx/walk_2.png"),
                LoadSprite("Assets/Sprites/Nyx/walk_3.png"),
                LoadSprite("Assets/Sprites/Nyx/walk_4.png"),
                LoadSprite("Assets/Sprites/Nyx/walk_5.png"),
                LoadSprite("Assets/Sprites/Nyx/walk_6.png"),
                LoadSprite("Assets/Sprites/Nyx/walk_7.png"),
                LoadSprite("Assets/Sprites/Nyx/walk_8.png")
            });
            soAnim.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soAdapt = new SerializedObject(adaptation);
            soAdapt.FindProperty("companionController").objectReferenceValue = floating;
            soAdapt.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{PrefabsFolder}/Nyx_Billboard_Companion.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildRadioStationPrefab()
        {
            GameObject root = new GameObject("Radio_Chipset_3D");

            AudioSource musicSource = root.AddComponent<AudioSource>();
            GameObject voiceChild = new GameObject("VoiceChannel");
            voiceChild.transform.SetParent(root.transform, false);
            AudioSource voiceSource = voiceChild.AddComponent<AudioSource>();
            voiceSource.spatialBlend = 1.0f;

            RadioReceiver3D receiver = root.AddComponent<RadioReceiver3D>();
            RadioHologramDisplay display = root.AddComponent<RadioHologramDisplay>();

            // Pedestal holográfico compacto
            GameObject baseCube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseCube.name = "HoloEmitterPedestal";
            baseCube.transform.SetParent(root.transform, false);
            baseCube.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            baseCube.transform.localScale = new Vector3(0.45f, 0.03f, 0.45f);

            // Holograma de Mina
            GameObject minaObj = new GameObject("Hologram_Mina");
            minaObj.transform.SetParent(root.transform, false);
            minaObj.transform.localPosition = new Vector3(-0.16f, 0.05f, 0f);
            minaObj.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);
            SpriteRenderer minaSr = minaObj.AddComponent<SpriteRenderer>();
            minaSr.sprite = LoadSprite("Assets/Sprites/Mina_Kira/Mina/idle.png");

            // Holograma de Kira
            GameObject kiraObj = new GameObject("Hologram_Kira");
            kiraObj.transform.SetParent(root.transform, false);
            kiraObj.transform.localPosition = new Vector3(0.16f, 0.05f, 0f);
            kiraObj.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);
            SpriteRenderer kiraSr = kiraObj.AddComponent<SpriteRenderer>();
            kiraSr.sprite = LoadSprite("Assets/Sprites/Mina_Kira/Kira/idle.png");

            // Gato Sintetizador del Estudio
            GameObject catObj = new GameObject("Hologram_StudioCat");
            catObj.transform.SetParent(root.transform, false);
            catObj.transform.localPosition = new Vector3(0f, -0.08f, -0.04f);
            catObj.transform.localScale = new Vector3(0.14f, 0.14f, 0.14f);
            SpriteRenderer catSr = catObj.AddComponent<SpriteRenderer>();
            catSr.sprite = LoadSprite("Assets/Sprites/RadioStudio/cyber_cat_console.png");

            SerializedObject soRec = new SerializedObject(receiver);
            soRec.FindProperty("musicAudioSource").objectReferenceValue = musicSource;
            soRec.FindProperty("voiceAudioSource").objectReferenceValue = voiceSource;
            soRec.FindProperty("antennaLed").objectReferenceValue = baseCube.transform;
            soRec.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soDisp = new SerializedObject(display);
            soDisp.FindProperty("minaRenderer").objectReferenceValue = minaSr;
            soDisp.FindProperty("kiraRenderer").objectReferenceValue = kiraSr;
            soDisp.FindProperty("studioCatRenderer").objectReferenceValue = catSr;
            soDisp.FindProperty("minaIdle").objectReferenceValue = LoadSprite("Assets/Sprites/Mina_Kira/Mina/idle.png");
            soDisp.FindProperty("minaHappy").objectReferenceValue = LoadSprite("Assets/Sprites/Mina_Kira/Mina/happy.png");
            soDisp.FindProperty("kiraIdle").objectReferenceValue = LoadSprite("Assets/Sprites/Mina_Kira/Kira/idle.png");
            soDisp.FindProperty("kiraHappy").objectReferenceValue = LoadSprite("Assets/Sprites/Mina_Kira/Kira/happy.png");

            SetSpriteArray(soDisp.FindProperty("minaTalkSprites"), new[]
            {
                LoadSprite("Assets/Sprites/Mina_Kira/Mina/talk_1.png"),
                LoadSprite("Assets/Sprites/Mina_Kira/Mina/talk_2.png"),
                LoadSprite("Assets/Sprites/Mina_Kira/Mina/talk_3.png")
            });

            SetSpriteArray(soDisp.FindProperty("kiraTalkSprites"), new[]
            {
                LoadSprite("Assets/Sprites/Mina_Kira/Kira/talk_1.png"),
                LoadSprite("Assets/Sprites/Mina_Kira/Kira/talk_2.png"),
                LoadSprite("Assets/Sprites/Mina_Kira/Kira/talk_3.png")
            });

            SetSpriteArray(soDisp.FindProperty("studioCatSprites"), new[]
            {
                LoadSprite("Assets/Sprites/RadioStudio/cyber_cat_console.png"),
                LoadSprite("Assets/Sprites/RadioStudio/calico_cat_speaker.png"),
                LoadSprite("Assets/Sprites/RadioStudio/ginger_cat_synth.png"),
                LoadSprite("Assets/Sprites/RadioStudio/white_kitten_floor.png")
            });
            soDisp.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{PrefabsFolder}/Radio_RadioChipset_3D.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildSpiderDronePrefab(Material laserMat)
        {
            GameObject root = new GameObject("SpiderDrone_25D");

            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.size = new Vector3(0.55f, 0.50f, 0.55f);

            EnemyCombatSpriteAnimator anim = root.AddComponent<EnemyCombatSpriteAnimator>();
            NeoCyberSpiderDroneAI ai = root.AddComponent<NeoCyberSpiderDroneAI>();

            GameObject spriteObj = new GameObject("Spider_Sprite25D");
            spriteObj.transform.SetParent(root.transform, false);
            spriteObj.transform.localScale = new Vector3(0.42f, 0.42f, 0.42f);
            SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
            Sprite idle = LoadSprite("Assets/Sprites/Enemies/Spider_drone/combat_idle.png");
            sr.sprite = idle;

            LineRenderer lr = root.AddComponent<LineRenderer>();
            lr.startWidth = 0.025f;
            lr.endWidth = 0.01f;
            lr.material = laserMat;
            lr.enabled = false;

            SerializedObject soAnim = new SerializedObject(anim);
            soAnim.FindProperty("spriteRenderer").objectReferenceValue = sr;
            soAnim.FindProperty("idleSprite").objectReferenceValue = idle;
            soAnim.FindProperty("moveSprite1").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Spider_drone/combat_move_1.png");
            soAnim.FindProperty("moveSprite2").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Spider_drone/combat_move_2.png");
            soAnim.FindProperty("attackChargeSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Spider_drone/combat_attack_charge.png");
            soAnim.FindProperty("attackStrikeSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Spider_drone/combat_attack_strike.png");
            soAnim.FindProperty("hitStaggerSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Spider_drone/combat_hit_stagger.png");
            soAnim.FindProperty("deathCollapseSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Spider_drone/combat_death_collapse.png");
            soAnim.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soAi = new SerializedObject(ai);
            soAi.FindProperty("spriteAnimator").objectReferenceValue = anim;
            soAi.FindProperty("attackLaserRenderer").objectReferenceValue = lr;
            soAi.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{PrefabsFolder}/SpiderDrone_25D.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildQuantumCenturionPrefab(Material laserMat)
        {
            GameObject root = new GameObject("QuantumCenturion_Boss_25D");

            BoxCollider col = root.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.85f, 0f);
            col.size = new Vector3(0.95f, 1.75f, 0.85f);

            EnemyCombatSpriteAnimator anim = root.AddComponent<EnemyCombatSpriteAnimator>();
            QuantumCenturionBossAI bossAi = root.AddComponent<QuantumCenturionBossAI>();

            GameObject spriteObj = new GameObject("Centurion_Sprite25D");
            spriteObj.transform.SetParent(root.transform, false);
            spriteObj.transform.localPosition = new Vector3(0f, 0.88f, 0f);
            spriteObj.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            SpriteRenderer sr = spriteObj.AddComponent<SpriteRenderer>();
            Sprite idle = LoadSprite("Assets/Sprites/Enemies/Quantum_centurion/combat_idle.png");
            sr.sprite = idle;

            LineRenderer lr = root.AddComponent<LineRenderer>();
            lr.startWidth = 0.08f;
            lr.endWidth = 0.03f;
            lr.material = laserMat;
            lr.enabled = false;

            SerializedObject soAnim = new SerializedObject(anim);
            soAnim.FindProperty("spriteRenderer").objectReferenceValue = sr;
            soAnim.FindProperty("idleSprite").objectReferenceValue = idle;
            soAnim.FindProperty("moveSprite1").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Quantum_centurion/combat_move_1.png");
            soAnim.FindProperty("moveSprite2").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Quantum_centurion/combat_move_2.png");
            soAnim.FindProperty("attackChargeSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Quantum_centurion/combat_attack_charge.png");
            soAnim.FindProperty("attackStrikeSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Quantum_centurion/combat_attack_strike.png");
            soAnim.FindProperty("hitStaggerSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Quantum_centurion/combat_hit_stagger.png");
            soAnim.FindProperty("deathCollapseSprite").objectReferenceValue = LoadSprite("Assets/Sprites/Enemies/Quantum_centurion/combat_death_collapse.png");
            soAnim.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject soBoss = new SerializedObject(bossAi);
            soBoss.FindProperty("plasmaBeamRenderer").objectReferenceValue = lr;
            soBoss.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{PrefabsFolder}/QuantumCenturion_Boss_25D.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        private static void AssembleMainMRScene(
            GameObject nyxPrefab,
            GameObject radioPrefab,
            GameObject spiderPrefab,
            GameObject centurionPrefab,
            Material cyanLaserMat,
            Material holoShieldMat)
        {
            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            // Limpiar objetos previos generados para evitar duplicados al re-ejecutar
            string[] managedNames = new[]
            {
                "WiredNexus_SystemsHub",
                "Nyx_Billboard_Companion",
                "Radio_Chipset_3D",
                "RightHand_Arsenal",
                "LeftHand_HoloShield",
                "QuantumCenturion_Boss_25D",
                "MRUK_EncounterDirector"
            };

            foreach (GameObject rootObj in scene.GetRootGameObjects())
            {
                foreach (string mName in managedNames)
                {
                    if (rootObj.name == mName)
                    {
                        UnityEngine.Object.DestroyImmediate(rootObj);
                        break;
                    }
                }
            }

            // 1. Configurar Cámara Principal para Passthrough Transparente en Quest 3S
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = GameObject.Find("Main Camera") ?? new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                mainCam = camObj.GetComponent<Camera>() ?? camObj.AddComponent<Camera>();
                if (camObj.GetComponent<AudioListener>() == null) camObj.AddComponent<AudioListener>();
            }

            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0f, 0f, 0f, 0f);
            mainCam.nearClipPlane = 0.05f;
            mainCam.transform.position = new Vector3(0f, 1.65f, 0f);
            mainCam.transform.rotation = Quaternion.identity;

            // 2. Hub de Sistemas (OVRManager, PassthroughManager, BattleMusicManager, BmaxBridgeClient)
            GameObject systemsHub = new GameObject("WiredNexus_SystemsHub");

            OVRManager ovrManager = systemsHub.AddComponent<OVRManager>();
            ovrManager.isInsightPassthroughEnabled = true;

            OVRPassthroughLayer ptLayer = systemsHub.AddComponent<OVRPassthroughLayer>();
            ptLayer.overlayType = OVROverlay.OverlayType.Underlay;

            PassthroughManager ptManager = systemsHub.AddComponent<PassthroughManager>();
            SerializedObject soPt = new SerializedObject(ptManager);
            soPt.FindProperty("passthroughLayer").objectReferenceValue = ptLayer;
            soPt.ApplyModifiedPropertiesWithoutUndo();

            BmaxBridgeClient bridgeClient = systemsHub.AddComponent<BmaxBridgeClient>();
            SerializedObject soBridge = new SerializedObject(bridgeClient);
            soBridge.FindProperty("serverIp").stringValue = "192.168.0.29";
            soBridge.FindProperty("eventPort").intValue = 5001;
            soBridge.ApplyModifiedPropertiesWithoutUndo();

            BattleMusicManager musicManager = systemsHub.AddComponent<BattleMusicManager>();
            SerializedObject soMusic = new SerializedObject(musicManager);
            SetAudioArray(soMusic.FindProperty("normalBattleTracks"), new[]
            {
                LoadAudio("Assets/Audio/Battle/Normal/Ronin_Code_Focus_Beats.mp3"),
                LoadAudio("Assets/Audio/Battle/Normal/Syndicate_Combat.mp3"),
                LoadAudio("Assets/Audio/Battle/Normal/Synthwave_Circuit_Board_Drive.mp3")
            });
            SetAudioArray(soMusic.FindProperty("bossTracks"), new[]
            {
                LoadAudio("Assets/Audio/Battle/Boss/Fairy tail 2.5.mp3"),
                LoadAudio("Assets/Audio/Battle/Boss/Fortuna_Empire_Doomsday_Loop.mp3")
            });
            SetAudioArray(soMusic.FindProperty("finalBossTracks"), new[]
            {
                LoadAudio("Assets/Audio/Battle/FinalBoss/Terminal_Breach.mp3"),
                LoadAudio("Assets/Audio/Battle/FinalBoss/sinfonía del juicio final 2.3.mp3"),
                LoadAudio("Assets/Audio/Battle/FinalBoss/(Fast Fiddle arpeggio).mp3")
            });
            soMusic.ApplyModifiedPropertiesWithoutUndo();

            // 3. Instanciar a Nyx en el salón (a la derecha de Raquel)
            GameObject nyxInstance = (GameObject)PrefabUtility.InstantiatePrefab(nyxPrefab, scene);
            nyxInstance.name = "Nyx_Billboard_Companion";
            nyxInstance.transform.position = new Vector3(0.45f, 1.72f, 1.35f);

            NyxFloatingCompanion nyxComp = nyxInstance.GetComponent<NyxFloatingCompanion>();
            SerializedObject soNyxComp = new SerializedObject(nyxComp);
            soNyxComp.FindProperty("playerHead").objectReferenceValue = mainCam.transform;
            soNyxComp.ApplyModifiedPropertiesWithoutUndo();

            NyxAdaptationEngine nyxAdapt = nyxInstance.GetComponent<NyxAdaptationEngine>();

            // 4. Instanciar el Sintonizador Holográfico 3D de Radio Chipset (Mina & Kira)
            GameObject radioInstance = (GameObject)PrefabUtility.InstantiatePrefab(radioPrefab, scene);
            radioInstance.name = "Radio_Chipset_3D";
            radioInstance.transform.position = new Vector3(-0.75f, 1.15f, 1.65f);

            // 5. Mano Derecha: Arsenal Holográfico (Blaster + Katana + Guantelete)
            GameObject rightHand = new GameObject("RightHand_Arsenal");
            XRHandAnchorFollower rightFollower = rightHand.AddComponent<XRHandAnchorFollower>();
            SerializedObject soRf = new SerializedObject(rightFollower);
            soRf.FindProperty("hand").enumValueIndex = (int)OVRInput.Controller.RTouch;
            soRf.FindProperty("editorCameraOffset").vector3Value = new Vector3(0.24f, -0.18f, 0.45f);
            soRf.ApplyModifiedPropertiesWithoutUndo();

            GameObject muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(rightHand.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.04f, 0.18f);

            GameObject blasterHolo = CreateWeaponSpriteObject("Blaster_Hologram", rightHand.transform, "Assets/Sprites/Weapons/Blaster/full_blaster.png", 0.25f);
            GameObject katanaHolo = CreateWeaponSpriteObject("Katana_Hologram", rightHand.transform, "Assets/Sprites/Weapons/Katana/full_katana.png", 0.28f);
            GameObject gauntletHolo = CreateWeaponSpriteObject("Gauntlet_Hologram", rightHand.transform, "Assets/Sprites/Weapons/Gauntlet/full_gauntlet_shield.png", 0.25f);

            LineRenderer blasterLaser = rightHand.AddComponent<LineRenderer>();
            blasterLaser.startWidth = 0.025f;
            blasterLaser.endWidth = 0.008f;
            blasterLaser.material = cyanLaserMat;
            blasterLaser.enabled = false;

            EnergyBlaster energyBlaster = rightHand.AddComponent<EnergyBlaster>();
            SerializedObject soBlaster = new SerializedObject(energyBlaster);
            soBlaster.FindProperty("nyxAdaptation").objectReferenceValue = nyxAdapt;
            soBlaster.FindProperty("muzzlePoint").objectReferenceValue = muzzle.transform;
            soBlaster.FindProperty("visualWeaponRoot").objectReferenceValue = blasterHolo.transform;
            soBlaster.FindProperty("laserLineRenderer").objectReferenceValue = blasterLaser;
            soBlaster.ApplyModifiedPropertiesWithoutUndo();

            HolographicGauntlet gauntlet = rightHand.AddComponent<HolographicGauntlet>();
            SerializedObject soGauntlet = new SerializedObject(gauntlet);
            soGauntlet.FindProperty("wristAnchor").objectReferenceValue = rightHand.transform;
            soGauntlet.FindProperty("blasterHologram").objectReferenceValue = blasterHolo;
            soGauntlet.FindProperty("plasmaBladeHologram").objectReferenceValue = katanaHolo;
            soGauntlet.FindProperty("energyCrossbowHologram").objectReferenceValue = gauntletHolo;
            soGauntlet.ApplyModifiedPropertiesWithoutUndo();

            // 6. Mano Izquierda: Guantelete y Holo-Shield
            GameObject leftHand = new GameObject("LeftHand_HoloShield");
            XRHandAnchorFollower leftFollower = leftHand.AddComponent<XRHandAnchorFollower>();
            SerializedObject soLf = new SerializedObject(leftFollower);
            soLf.FindProperty("hand").enumValueIndex = (int)OVRInput.Controller.LTouch;
            soLf.FindProperty("editorCameraOffset").vector3Value = new Vector3(-0.24f, -0.18f, 0.45f);
            soLf.ApplyModifiedPropertiesWithoutUndo();

            GameObject leftGauntletSpriteObj = CreateWeaponSpriteObject("LeftGauntlet_Sprite", leftHand.transform, "Assets/Sprites/Weapons/Gauntlet/full_gauntlet_shield.png", 0.26f);
            SpriteRenderer leftGauntletSr = leftGauntletSpriteObj.GetComponent<SpriteRenderer>();

            GameObject barrierRoot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            barrierRoot.name = "HoloShield_BarrierField";
            barrierRoot.transform.SetParent(leftHand.transform, false);
            barrierRoot.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            barrierRoot.transform.localScale = new Vector3(0.55f, 0.55f, 0.08f);
            MeshRenderer barrierRenderer = barrierRoot.GetComponent<MeshRenderer>();
            if (barrierRenderer != null) barrierRenderer.enabled = false; // El sprite del guantelete ya incluye el escudo hexagonal
            Collider barrierCollider = barrierRoot.GetComponent<Collider>();
            if (barrierCollider != null) barrierCollider.isTrigger = true;

            HoloShieldController shieldCtrl = leftHand.AddComponent<HoloShieldController>();
            SerializedObject soShield = new SerializedObject(shieldCtrl);
            soShield.FindProperty("gauntletSpriteRenderer").objectReferenceValue = leftGauntletSr;
            soShield.FindProperty("activeBarrierRoot").objectReferenceValue = barrierRoot;
            soShield.FindProperty("shieldTriggerCollider").objectReferenceValue = barrierCollider;
            soShield.ApplyModifiedPropertiesWithoutUndo();

            // 7. Instanciar Boss Quantum Centurion (inicialmente en espera hasta limpiar la primera oleada o pulsar B / Botón A)
            GameObject centurionInstance = (GameObject)PrefabUtility.InstantiatePrefab(centurionPrefab, scene);
            centurionInstance.name = "QuantumCenturion_Boss_25D";
            centurionInstance.transform.position = new Vector3(0f, 0.05f, 2.8f);
            centurionInstance.SetActive(false);

            // 8. Director de Encuentros MRUK
            GameObject directorObj = new GameObject("MRUK_EncounterDirector");
            MRUKEncounterSpawner spawner = directorObj.AddComponent<MRUKEncounterSpawner>();
            SerializedObject soSpawner = new SerializedObject(spawner);
            soSpawner.FindProperty("spiderDronePrefab").objectReferenceValue = spiderPrefab;
            soSpawner.FindProperty("centurionBossInstance").objectReferenceValue = centurionInstance.GetComponent<QuantumCenturionBossAI>();
            soSpawner.FindProperty("initialSpiderCount").intValue = 3;
            soSpawner.ApplyModifiedPropertiesWithoutUndo();

            // Guardar escena y registrar en Build Settings
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };

            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateWeaponSpriteObject(string name, Transform parent, string spritePath, float scale)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localScale = new Vector3(scale, scale, scale);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite(spritePath);
            return obj;
        }

        private static void SetSpriteArray(SerializedProperty arrayProp, Sprite[] sprites)
        {
            if (arrayProp == null || sprites == null) return;
            arrayProp.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++)
            {
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
            }
        }

        private static void SetAudioArray(SerializedProperty arrayProp, AudioClip[] clips)
        {
            if (arrayProp == null || clips == null) return;
            arrayProp.arraySize = clips.Length;
            for (int i = 0; i < clips.Length; i++)
            {
                arrayProp.GetArrayElementAtIndex(i).objectReferenceValue = clips[i];
            }
        }
    }
}
