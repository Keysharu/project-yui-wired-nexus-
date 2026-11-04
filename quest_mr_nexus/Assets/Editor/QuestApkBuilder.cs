using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ProjectNyx.EditorTools
{
    /// <summary>
    /// [ES] Compilador y Desplegador Directo de APK para Meta Quest 3S.
    /// Genera el ejecutable Android XR (WiredNexusMR_Quest3S.apk) y permite instalarlo
    /// por ADB con un solo clic desde el menú superior de Unity.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Direct APK Compiler & Deployer for Meta Quest 3S.
    /// Builds the Android XR executable (WiredNexusMR_Quest3S.apk) and allows one-click ADB installation
    /// from the Unity top menu bar.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public static class QuestApkBuilder
    {
        private const string BuildOutputPath = "Builds/WiredNexusMR_Quest3S.apk";

        [MenuItem("Wired Nexus MR/3. Compilar APK para Meta Quest 3S", false, 30)]
        public static void BuildQuestApk()
        {
            QuestBuildPreparer.CleanBurstTempLocks();
            // WiredNexusSceneAssembler.AssembleFullProject();

            string buildDir = Path.GetDirectoryName(BuildOutputPath);
            if (!string.IsNullOrEmpty(buildDir) && !Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/MainMRScene.unity" },
                locationPathName = BuildOutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log($"[QuestApkBuilder] Iniciando compilación de APK Android XR en: {BuildOutputPath}...");
            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[QuestApkBuilder] ¡APK compilado con éxito! Tamaño: {summary.totalSize / (1024 * 1024)} MB | Ruta: {Path.GetFullPath(BuildOutputPath)}");
            }
            else
            {
                Debug.LogError($"[QuestApkBuilder] Fallo en la compilación del APK: {summary.result} ({summary.totalErrors} errores).");
            }
        }
    }
}
