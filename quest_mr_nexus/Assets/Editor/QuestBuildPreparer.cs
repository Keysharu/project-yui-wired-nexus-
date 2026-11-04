// =============================================================================
// Autora / Author: Raquel D. Torres (El taller de la gatita tech)
// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
// Meta Global AI Developer Hackathon 2026
// =============================================================================
// [ES] Asistente de Preparación y Compilación de APK para Meta Quest 3S.
//      Configura automáticamente Package Name, IL2CPP, ARM64 y plataforma Android.
// ---
// [EN] Quest 3S APK Build Preparation Assistant.
//      Automatically configures Package Name, IL2CPP backend, ARM64, and Android platform.
// =============================================================================

using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace ProjectNyx.EditorTools
{
    [InitializeOnLoad]
    public static class QuestBuildPreparer
    {
        static QuestBuildPreparer()
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    ApplySettingsSilently();
                }
            };
        }

        private static void ApplySettingsSilently()
        {
            PlayerSettings.companyName = "El taller de la gatita tech";
            PlayerSettings.productName = "Project NYX - Wired Nexus";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.GatitaTech.ProjectNyx");
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            CleanBurstTempLocks();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Wired Nexus MR/🚀 PREPARAR CONFIGURACIÓN DE APK PARA QUEST 3S", false, 1)]
        public static void PreparePlayerSettingsForQuest()
        {
            Debug.Log("[QuestBuildPreparer] 🔧 Configurando Player Settings para Meta Quest 3S...");

            // 1. Identidad y Nombre de Paquete
            PlayerSettings.companyName = "El taller de la gatita tech";
            PlayerSettings.productName = "Project NYX - Wired Nexus";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.GatitaTech.ProjectNyx");

            // 2. Versión y SDK
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;

            // 3. Arquitectura y Backend exigidos por Meta Quest 3S
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // 4. Orientación y configuración
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            CleanBurstTempLocks();

            // 5. Garantizar sincronización completa de la emisora de radio autónoma (Kira, Mina, Nyx)
            PlayerPrefs.SetString("NYX_BASETEN_API_KEY", "b10_kcxX0oEd.u8Tv3mTeETKiiSjPdoCx1V9H82yxaOXx");
            PlayerPrefs.SetString("NYX_META_API_KEY", "LLM_1165970146086954_iqlTUYcZahQbs3LPx2ORq53PZxw");
            PlayerPrefs.Save();
            SetupStandaloneRadioStation.ConfigureStationNow();

            AssetDatabase.SaveAssets();

            // 5. Auto-resolver checklist de Meta XR Project Setup Tool si está disponible
            try
            {
                var projectSetupType = System.Type.GetType("OVRProjectSetup, Oculus.VR.Editor");
                if (projectSetupType != null)
                {
                    var fixMethod = projectSetupType.GetMethod("FixTasks", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                    if (fixMethod != null)
                    {
                        fixMethod.Invoke(null, new object[] { BuildTargetGroup.Android, null, 1, false, null });
                        Debug.Log("[QuestBuildPreparer] 🥽 Meta XR Project Setup Tool tareas Android auto-corregidas.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[QuestBuildPreparer] Aviso al auto-corregir Meta XR: " + ex.Message);
            }

            Debug.Log("[QuestBuildPreparer] ✅ ¡Configuración de Quest 3S lista! Package: com.GatitaTech.ProjectNyx | Backend: IL2CPP | Arch: ARM64");
            
            // Abrir la ventana de Build Settings directamente para comodidad de Raquel
            EditorWindow.GetWindow(System.Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        }
            [MenuItem("Wired Nexus MR/🧹 Limpiar Bloqueos de Temp y Burst", false, 2)]
        public static void CleanBurstTempLocks()
        {
            try
            {
                foreach (var proc in System.Diagnostics.Process.GetProcessesByName("java"))
                {
                    try { proc.Kill(); proc.WaitForExit(1500); } catch {}
                }
            }
            catch {}

            string projectDir = System.IO.Directory.GetCurrentDirectory();
            string[] pathsToClean = new string[] {
                System.IO.Path.Combine(projectDir, "Temp", "BurstOutput"),
                System.IO.Path.Combine(projectDir, "Library", "Bee", "artifacts", "Android", "AsyncPluginsFromLinker"),
                System.IO.Path.Combine(projectDir, "Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle", "unityLibrary", "build"),
                System.IO.Path.Combine(projectDir, "Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle", "launcher", "build")
            };

            foreach (string p in pathsToClean)
            {
                if (System.IO.Directory.Exists(p))
                {
                    try
                    {
                        var dirInfo = new System.IO.DirectoryInfo(p);
                        foreach (var f in dirInfo.GetFiles("*", System.IO.SearchOption.AllDirectories))
                        {
                            f.Attributes = System.IO.FileAttributes.Normal;
                        }
                        foreach (var d in dirInfo.GetDirectories("*", System.IO.SearchOption.AllDirectories))
                        {
                            d.Attributes = System.IO.FileAttributes.Normal;
                        }
                        System.IO.Directory.Delete(p, true);
                        Debug.Log($"[QuestBuildPreparer] ✅ Carpeta temporal limpiada: {p}");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[QuestBuildPreparer] Aviso al limpiar {p}: {ex.Message}");
                    }
                }
            }
        }
}
}
