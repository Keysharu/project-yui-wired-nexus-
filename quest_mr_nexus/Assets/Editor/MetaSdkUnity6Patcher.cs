// =============================================================================
// Autora / Author: Raquel D. Torres (El taller de la gatita tech)
// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
// Meta Global AI Developer Hackathon 2026
// =============================================================================
// [ES] Guardián de Compatibilidad para Meta XR SDK en Unity 6.
//      Evita automáticamente el error CS0234 por la eliminación de 'UnityEngine.Experimental.XR'.
// ---
// [EN] Compatibility Guardian for Meta XR SDK on Unity 6.
//      Automatically shields and prevents CS0234 error caused by deprecated 'UnityEngine.Experimental.XR'.
// =============================================================================

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectNyx.EditorTools
{
    [InitializeOnLoad]
    public static class MetaSdkUnity6Patcher
    {
        static MetaSdkUnity6Patcher()
        {
            ApplyCompatibilityPatch();
        }

        [MenuItem("Wired Nexus MR/🛡️ VERIFICAR Y PARCHEAR SDK META XR (Unity 6)", false, 2)]
        public static void ApplyCompatibilityPatch()
        {
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                string packageCacheDir = Path.Combine(projectRoot, "Library", "PackageCache");

                if (!Directory.Exists(packageCacheDir)) return;

                string[] coreDirs = Directory.GetDirectories(packageCacheDir, "com.meta.xr.sdk.core*");
                if (coreDirs.Length == 0) return;

                string scriptsDir = Path.Combine(coreDirs[0], "Scripts");
                if (!Directory.Exists(scriptsDir)) return;

                string[] targetFiles = new string[]
                {
                    "OVRManager.cs",
                    "OVRDisplay.cs",
                    "OVRCommon.cs",
                    "OVRDebugHeadController.cs"
                };

                bool anyPatched = false;

                foreach (string file in targetFiles)
                {
                    string fullPath = Path.Combine(scriptsDir, file);
                    if (File.Exists(fullPath))
                    {
                        string content = File.ReadAllText(fullPath);
                        if (content.Contains("using UnityEngine.Experimental.XR;"))
                        {
                            string patched = content.Replace(
                                "using UnityEngine.Experimental.XR;",
                                "// using UnityEngine.Experimental.XR; // [Unity 6 fix]"
                            );
                            File.WriteAllText(fullPath, patched);
                            anyPatched = true;
                            Debug.Log($"[MetaSdkUnity6Patcher] 🛡️ Parcheado automáticamente para Unity 6: {file}");
                        }
                    }
                }

                if (anyPatched)
                {
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[MetaSdkUnity6Patcher] Nota: {ex.Message}");
            }
        }
    }
}
