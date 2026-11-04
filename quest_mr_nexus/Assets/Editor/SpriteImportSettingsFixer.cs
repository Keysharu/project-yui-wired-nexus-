using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectNyx.EditorTools
{
    /// <summary>
    /// [ES] Fase 1 del Montaje XR: Configurador masivo de Sprites 2.5D.
    /// Asegura que todas las texturas de personajes, enemigos y armas se importen
    /// con compresión transparente, filtrado Point/Bilinear y ratio de píxeles óptimo para Quest 3S.
    /// ---
    /// [EN] XR Staging Phase 1: Bulk 2.5D Sprite Configurator.
    /// Ensures all character, enemy, and weapon textures are imported with transparent
    /// compression, optimal Point/Bilinear filtering, and ideal pixel ratio for Meta Quest 3S.
    /// </summary>
    public static class SpriteImportSettingsFixer
    {
        private const string SpritesRootFolder = "Assets/Sprites";
        private const float DefaultPixelsPerUnit = 512f;

        [MenuItem("Wired Nexus MR/1. Configurar Todos los Sprites (2.5D Transparentes)", false, 10)]
        public static void FixAllSpritesMenu()
        {
            int updatedCount = FixAllSprites();
            Debug.Log($"[SpriteImportSettingsFixer] Configuración completada. {updatedCount} texturas actualizadas a Sprite 2.5D.");
        }

        public static int FixAllSprites()
        {
            if (!AssetDatabase.IsValidFolder(SpritesRootFolder))
            {
                Debug.LogWarning($"[SpriteImportSettingsFixer] No se encontró la carpeta {SpritesRootFolder}.");
                return 0;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesRootFolder });
            int modifiedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(assetPath))
                    {
                        continue;
                    }

                    string ext = Path.GetExtension(assetPath).ToLowerInvariant();
                    if (ext != ".png" && ext != ".jpg" && ext != ".jpeg")
                    {
                        continue;
                    }

                    TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    if (importer == null)
                    {
                        continue;
                    }

                    bool needsReimport = false;

                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        needsReimport = true;
                    }

                    if (importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        importer.spriteImportMode = SpriteImportMode.Single;
                        needsReimport = true;
                    }

                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        needsReimport = true;
                    }

                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        needsReimport = true;
                    }

                    if (importer.filterMode != FilterMode.Bilinear)
                    {
                        importer.filterMode = FilterMode.Bilinear;
                        needsReimport = true;
                    }

                    if (Mathf.Abs(importer.spritePixelsPerUnit - DefaultPixelsPerUnit) > 0.1f)
                    {
                        importer.spritePixelsPerUnit = DefaultPixelsPerUnit;
                        needsReimport = true;
                    }

                    if (importer.wrapMode != TextureWrapMode.Clamp)
                    {
                        importer.wrapMode = TextureWrapMode.Clamp;
                        needsReimport = true;
                    }

                    TextureImporterSettings settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);

                    // Pivot centrado para facilitar rotación Billboard 2.5D y anclajes
                    if (settings.spriteAlignment != (int)SpriteAlignment.Center)
                    {
                        settings.spriteAlignment = (int)SpriteAlignment.Center;
                        settings.spritePivot = new Vector2(0.5f, 0.5f);
                        importer.SetTextureSettings(settings);
                        needsReimport = true;
                    }

                    if (needsReimport)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                        modifiedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return modifiedCount;
        }
    }
}
