using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectNyx.EditorTools
{
    /// <summary>
    /// [ES] Re-importa y conecta limpiamente las texturas oficiales sobre la heroína 3D Nyx.
    /// Autora: Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public static class ForceRefreshAndApplyNyx
    {
        [MenuItem("Wired Nexus MR/5. Diagnosticar y Reparar Texturas Nyx 3D", false, 50)]
        public static void RepairNyxTextures()
        {
            Debug.Log("[NyxRepair] Iniciando sincronización forzada de texturas y materiales de Nyx 3D...");

            // 1. Forzar reimportación de Texturas
            string texDir = "Assets/Resources/Textures/Companion";
            string[] texFiles = Directory.GetFiles(texDir, "*.png");
            foreach (var tf in texFiles)
            {
                string assetPath = tf.Replace('\\', '/');
                TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (ti != null)
                {
                    ti.textureType = TextureImporterType.Default;
                    ti.sRGBTexture = true;
                    ti.isReadable = true;
                    ti.SaveAndReimport();
                }
            }

            // 2. Reimportar el FBX limpio
            string fbxPath = "Assets/Resources/Models/Nyx_Companion_Anime3D.fbx";
            AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);

            // 3. Crear o actualizar materiales oficiales
            Shader pbrShader = Shader.Find("Mobile/Diffuse") ?? Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            Debug.Log($"[NyxRepair] Shader seleccionado para materiales: {pbrShader.name}");

            Material topsMat = CreateOrUpdateMat("Mat_Nyx_Tops_Tactical", "tex_nyx_tops.png", pbrShader);
            Material bottomsMat = CreateOrUpdateMat("Mat_Nyx_Bottoms_Tactical", "tex_nyx_bottoms.png", pbrShader);
            Material shoesMat = CreateOrUpdateMat("Mat_Nyx_Shoes_Tactical", "tex_nyx_shoes.png", pbrShader);
            Material bodySkinMat = CreateOrUpdateMat("Mat_Nyx_Body_Skin", "tex_nyx_body_skin.png", pbrShader);
            Material hairMat = CreateOrUpdateMat("Mat_Nyx_Hair_Crimson", "tex_nyx_hair.png", pbrShader);
            Material faceSkinMat = CreateOrUpdateMat("Mat_Nyx_Face_Skin", "tex_nyx_face_skin.png", pbrShader);
            Material eyesMat = CreateOrUpdateMat("Mat_Nyx_Eyes_Iris", "tex_nyx_eyes_iris.png", pbrShader);
            Material eyeWhiteMat = CreateOrUpdateMat("Mat_Nyx_EyeWhite", "tex_nyx_eyewhite.png", pbrShader);
            Material eyeHighlightMat = CreateOrUpdateMat("Mat_Nyx_EyeHighlight", "tex_nyx_eyehighlight.png", pbrShader);
            Material eyeLineMat = CreateOrUpdateMat("Mat_Nyx_FaceEyeline", "tex_nyx_eyeline.png", pbrShader);
            Material eyeLashMat = CreateOrUpdateMat("Mat_Nyx_FaceEyelash", "tex_nyx_eyelash.png", pbrShader);
            Material browMat = CreateOrUpdateMat("Mat_Nyx_FaceBrow", "tex_nyx_eyebrow.png", pbrShader);
            Material mouthMat = CreateOrUpdateMat("Mat_Nyx_FaceMouth", "tex_nyx_mouth.png", pbrShader);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[NyxRepair] ¡Materiales de Nyx regenerados y guardados con éxito en Assets/Resources/Materials/!");
        }

        private static Material CreateOrUpdateMat(string matName, string texFileName, Shader shader)
        {
            string matPath = $"Assets/Resources/Materials/{matName}.mat";
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

            string texPath = $"Assets/Resources/Textures/Companion/{texFileName}";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            if (tex != null)
            {
                mat.mainTexture = tex;
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                mat.color = Color.white;
                Debug.Log($"[NyxRepair] Asignada textura {texFileName} a {matName}");
            }
            else
            {
                Debug.LogWarning($"[NyxRepair] Textura no encontrada en: {texPath}");
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
