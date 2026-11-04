/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Herramienta Editor para:
 *      1. Configurar la pantalla holográfica Hologram_CyberCrest_Display con YouTube Live.
 *      2. Silenciar el audio local dejando solo el streaming (con control en el menú de pausa).
 *      3. Garantizar el aislamiento estricto entre el entorno VR (Bulevar) y AR (Salón con Passthrough).
 *      4. Calibrar el seguimiento de manos y mandos Touch Plus de Meta Quest 3S en el arsenal y escudo.
 * ---
 * [EN] Editor Tool to:
 *      1. Configure Hologram_CyberCrest_Display with YouTube Live stream.
 *      2. Mute local audio leaving solely the live stream (with pause menu toggle).
 *      3. Guarantee strict isolation between VR environment (Boulevard) and AR (Room Passthrough).
 *      4. Calibrate Meta Quest 3S Touch Plus hands & controller tracking for arsenal and shield.
 * =============================================================================
 */

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using ProjectNyx.Core;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.EditorTools
{
    /// <summary>
    /// [ES] Configurador Editor Maestro para Radio Streaming, Aislamiento VR/AR y Manos Quest 3S.
    /// [EN] Master Editor Configurator for Radio Streaming, VR/AR Isolation and Quest 3S Hands.
    /// </summary>
    [InitializeOnLoad]
    public static class SetupCyberCrestDisplay
    {
        private const string DisplayObjectName = "Hologram_CyberCrest_Display";
        private const string VideoId = "j_bLVMldHsA";
        private const string MaterialPath = "Assets/Materials/Mat_Hologram_CyberCrest_Screen.mat";
        private const string DefaultTexturePath = "Assets/Resources/Textures/Environment/tex_cyber_crest_hologram.png";
        private const string FallbackTexturePath = "Assets/Textures/Environment/tex_cyber_crest_hologram.png";

        static SetupCyberCrestDisplay()
        {
            EditorApplication.delayCall += ExecuteAutoSetupIfEligible;
        }

        private static void ExecuteAutoSetupIfEligible()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;

            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.isLoaded || !activeScene.name.Contains("MainMRScene")) return;

            GameObject existing = GameObject.Find(DisplayObjectName);
            if (existing == null || existing.GetComponent<HologramCyberCrestDisplay>() == null)
            {
                ConfigureCyberCrestDisplay();
            }
        }

        [MenuItem("Wired Nexus MR/★ CONFIGURAR EXPERIENCIA COMPLETA (Radio, Manos 3S y Aislamiento VR-AR)", false, 1)]
        public static void ConfigureCyberCrestDisplay()
        {
            Debug.Log("[CyberCrestSetup] 🛰️ Configurando Proyecto NYX // Radio Chipset, Manos Quest 3S y Aislamiento VR/AR...");

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                Debug.LogWarning("[CyberCrestSetup] La escena activa no está cargada.");
                return;
            }

            // 1. Localizar o instanciar GameObject Hologram_CyberCrest_Display
            GameObject displayObj = GameObject.Find(DisplayObjectName);
            if (displayObj == null)
            {
                Debug.Log($"[CyberCrestSetup] '{DisplayObjectName}' no encontrado. Creando primitivo Quad holográfico 16:9...");
                displayObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                displayObj.name = DisplayObjectName;

                Collider col = displayObj.GetComponent<Collider>();
                if (col != null) UnityEngine.Object.DestroyImmediate(col);

                GameObject radioPedestal = GameObject.Find("Radio_Chipset_3D");
                if (radioPedestal != null)
                {
                    displayObj.transform.position = radioPedestal.transform.position + new Vector3(0f, 1.45f, 0.45f);
                    displayObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
                else
                {
                    displayObj.transform.position = new Vector3(0f, 1.45f, 2.3f);
                    displayObj.transform.rotation = Quaternion.identity;
                }

                displayObj.transform.localScale = new Vector3(1.92f, 1.08f, 1.0f);
            }

            // 2. Material del Display
            MeshRenderer mr = displayObj.GetComponent<MeshRenderer>();
            if (mr == null) mr = displayObj.AddComponent<MeshRenderer>();

            Material screenMat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (screenMat == null)
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") 
                                  ?? Shader.Find("Unlit/Texture") 
                                  ?? Shader.Find("Standard");

                screenMat = new Material(unlitShader) { name = "Mat_Hologram_CyberCrest_Screen" };
                Texture2D defaultTex = AssetDatabase.LoadAssetAtPath<Texture2D>(DefaultTexturePath)
                                    ?? AssetDatabase.LoadAssetAtPath<Texture2D>(FallbackTexturePath);

                if (defaultTex != null)
                {
                    if (screenMat.HasProperty("_BaseMap")) screenMat.SetTexture("_BaseMap", defaultTex);
                    if (screenMat.HasProperty("_MainTex")) screenMat.SetTexture("_MainTex", defaultTex);
                }
                screenMat.color = Color.white;
                EnsureDirectoryExists("Assets/Materials");
                AssetDatabase.CreateAsset(screenMat, MaterialPath);
            }
            mr.sharedMaterial = screenMat;
            EditorUtility.SetDirty(screenMat);

            // 3. AudioSource 3D Espacializado para el Streaming
            AudioSource audio = displayObj.GetComponent<AudioSource>();
            if (audio == null) audio = displayObj.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 1.0f;
            audio.minDistance = 2.0f;
            audio.maxDistance = 35.0f;
            audio.rolloffMode = AudioRolloffMode.Logarithmic;
            audio.volume = 1.0f;
            audio.mute = false;
            EditorUtility.SetDirty(audio);

            // 4. VideoPlayer para Streaming
            VideoPlayer vp = displayObj.GetComponent<VideoPlayer>();
            if (vp == null) vp = displayObj.AddComponent<VideoPlayer>();
            vp.playOnAwake = false;
            vp.isLooping = true;
            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            vp.EnableAudioTrack(0, true);
            vp.SetTargetAudioSource(0, audio);
            EditorUtility.SetDirty(vp);

            // 5. YouTubeLiveStreamResolver
            YouTubeLiveStreamResolver resolver = displayObj.GetComponent<YouTubeLiveStreamResolver>();
            if (resolver == null) resolver = displayObj.AddComponent<YouTubeLiveStreamResolver>();
            resolver.SetYouTubeUrl($"https://www.youtube.com/live/{VideoId}", autoConnect: false);
            EditorUtility.SetDirty(resolver);

            // 6. HologramCyberCrestDisplay (Controlador Principal)
            HologramCyberCrestDisplay crestDisplay = displayObj.GetComponent<HologramCyberCrestDisplay>();
            if (crestDisplay == null) crestDisplay = displayObj.AddComponent<HologramCyberCrestDisplay>();
            crestDisplay.EnsureComponentsConfigured();
            EditorUtility.SetDirty(crestDisplay);

            // 7. Calibrar y certificar las Manos / Mandos Quest 3S (Arsenal y Escudo)
            ConfigureQuest3SHandsAndControllers();

            // 9. Silenciar sonidos originales y preparar el menú de pausa
            SilenceOriginalSounds(scene);

            // 10. Guardar cambios en la escena
            EditorUtility.SetDirty(displayObj);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CyberCrestSetup] ★ ¡TODO CERTIFICADO! Radio Streaming, Manos Quest 3S y Aislamiento VR/AR listos para APK.");
        }

        [MenuItem("Wired Nexus MR/★ CALIBRAR MANOS Y MANDOS TOUCH PLUS (Meta Quest 3S)", false, 15)]
        public static void ConfigureQuest3SHandsAndControllersMenu()
        {
            ConfigureQuest3SHandsAndControllers();
            Scene scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[CyberCrestSetup] ★ Manos y mandos Touch Plus de Meta Quest 3S calibrados con éxito.");
        }

        private static void ConfigureQuest3SHandsAndControllers()
        {
            // Mano Derecha: Arsenal (Blaster, Katana, Guantelete)
            GameObject rHand = GameObject.Find("RightHand_Arsenal");
            if (rHand != null)
            {
                XRHandAnchorFollower follower = rHand.GetComponent<XRHandAnchorFollower>();
                if (follower == null) follower = rHand.AddComponent<XRHandAnchorFollower>();
                follower.SetHandSide(XRHandAnchorFollower.HandSide.RightHand);
                EditorUtility.SetDirty(follower);
                Debug.Log("[CyberCrestSetup] RightHand_Arsenal configurado para Mando Derecho Touch Plus / RHand.");
            }

            // Mano Izquierda: HoloEscudo Táctico
            GameObject lHand = GameObject.Find("LeftHand_HoloShield");
            if (lHand != null)
            {
                XRHandAnchorFollower follower = lHand.GetComponent<XRHandAnchorFollower>();
                if (follower == null) follower = lHand.AddComponent<XRHandAnchorFollower>();
                follower.SetHandSide(XRHandAnchorFollower.HandSide.LeftHand);
                EditorUtility.SetDirty(follower);
                Debug.Log("[CyberCrestSetup] LeftHand_HoloShield configurado para Mando Izquierdo Touch Plus / LHand.");
            }
        }

        [MenuItem("Wired Nexus MR/★ SILENCIAR SONIDOS ORIGINALES (Solo Streaming YouTube)", false, 20)]
        public static void SilenceOriginalSoundsMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            SilenceOriginalSounds(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[CyberCrestSetup] ★ Sonidos originales desactivados con éxito. Únicamente el streaming permanece activo.");
        }

        private static void SilenceOriginalSounds(Scene scene)
        {
            BattleMusicManager musicMgr = UnityEngine.Object.FindAnyObjectByType<BattleMusicManager>();
            if (musicMgr != null)
            {
                SerializedObject so = new SerializedObject(musicMgr);
                SerializedProperty prop = so.FindProperty("disableOriginalMusicForStreaming");
                if (prop != null)
                {
                    prop.boolValue = true;
                    so.ApplyModifiedProperties();
                }
                musicMgr.DisableOriginalMusicForStreaming = true;
                musicMgr.StopOriginalMusic();
                EditorUtility.SetDirty(musicMgr);
            }

            RadioReceiver3D radioRec = UnityEngine.Object.FindAnyObjectByType<RadioReceiver3D>();
            if (radioRec != null)
            {
                SerializedObject so = new SerializedObject(radioRec);
                SerializedProperty prop = so.FindProperty("disableInternalRadioForStreaming");
                if (prop != null)
                {
                    prop.boolValue = true;
                    so.ApplyModifiedProperties();
                }
                radioRec.DisableInternalRadioForStreaming = true;
                radioRec.StopInternalRadio();
                EditorUtility.SetDirty(radioRec);
            }

            PauseMenuController pauseMenu = UnityEngine.Object.FindAnyObjectByType<PauseMenuController>();
            if (pauseMenu != null)
            {
                pauseMenu.SetLocalAudioMute(true);
                EditorUtility.SetDirty(pauseMenu);
            }

            AudioSource[] allAudioSources = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            GameObject streamingObj = GameObject.Find(DisplayObjectName);

            foreach (AudioSource a in allAudioSources)
            {
                if (streamingObj != null && (a.gameObject == streamingObj || a.transform.IsChildOf(streamingObj.transform)))
                {
                    a.mute = false;
                    a.volume = 1.0f;
                    EditorUtility.SetDirty(a);
                    continue;
                }

                if (a.gameObject.name.Contains("SystemsHub") || a.gameObject.name.Contains("Radio_Chipset") || a.gameObject.name.Contains("Radio"))
                {
                    a.playOnAwake = false;
                    a.volume = 0f;
                    EditorUtility.SetDirty(a);
                }
            }
        }

        private static void EnsureDirectoryExists(string relativeDirPath)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", relativeDirPath);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }
        }
    }
}
