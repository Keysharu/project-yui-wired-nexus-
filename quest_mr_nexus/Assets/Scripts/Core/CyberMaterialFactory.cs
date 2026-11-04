using UnityEngine;

using UnityEngine.Rendering;



namespace ProjectNyx.Core

{

    /// <summary>

    /// [ES] Generador de Materiales PBR y Emisivos a prueba de fallos para El taller de la gatita tech.

    /// Crea proceduralmente materiales URP Lit/Unlit optimizados para Quest 3S (Cian neón, Magenta neón, Oro neón).

    /// ---

    /// [EN] Fail-safe PBR & Emissive Material Factory for El taller de la gatita tech.

    /// Procedurally constructs optimized URP Lit/Unlit materials for Meta Quest 3S (Neon Cyan, Neon Magenta, Neon Gold).

    /// </summary>

    public static class CyberMaterialFactory

    {

        private static Shader cachedPbrShader;

        private static Shader cachedEmissiveShader;

        private static bool isInitialized;



        public static bool IsUrpActive

        {

            get

            {

                return GraphicsSettings.currentRenderPipeline != null;

            }

        }



        public static Shader GetPbrShader()

        {

            if (cachedPbrShader != null) return cachedPbrShader;



            if (IsUrpActive)

            {

                cachedPbrShader = Shader.Find("Universal Render Pipeline/Lit");

                if (cachedPbrShader != null) return cachedPbrShader;

            }



            // En Built-in Render Pipeline (o como respaldo seguro):

            cachedPbrShader = Shader.Find("Standard");

            if (cachedPbrShader != null) return cachedPbrShader;



            cachedPbrShader = Shader.Find("Mobile/Diffuse");

            if (cachedPbrShader != null) return cachedPbrShader;



            cachedPbrShader = Shader.Find("Legacy Shaders/Diffuse");

            if (cachedPbrShader != null) return cachedPbrShader;



            cachedPbrShader = Shader.Find("Unlit/Texture");

            if (cachedPbrShader != null) return cachedPbrShader;



            cachedPbrShader = Shader.Find("Sprites/Default");

            return cachedPbrShader;

        }



        public static Shader GetEmissiveShader()

        {

            if (cachedEmissiveShader != null) return cachedEmissiveShader;



            if (IsUrpActive)

            {

                cachedEmissiveShader = Shader.Find("Universal Render Pipeline/Unlit") 

                                       ?? Shader.Find("Universal Render Pipeline/Lit");

                if (cachedEmissiveShader != null) return cachedEmissiveShader;

            }



            // En Built-in: Standard soporta _EmissionColor con _EMISSION

            cachedEmissiveShader = Shader.Find("Standard");

            if (cachedEmissiveShader != null) return cachedEmissiveShader;



            cachedEmissiveShader = Shader.Find("Unlit/Color");

            if (cachedEmissiveShader != null) return cachedEmissiveShader;



            cachedEmissiveShader = Shader.Find("Mobile/Particles/Additive");

            if (cachedEmissiveShader != null) return cachedEmissiveShader;



            cachedEmissiveShader = Shader.Find("Sprites/Default");

            return cachedEmissiveShader;

        }



        public static Material CreatePbrMaterial(string name, Color baseColor, float smoothness, float metallic, Texture2D texture = null)

        {

            Shader shader = GetPbrShader();

            Material mat = new Material(shader);

            mat.name = name;



            // Soporte dual para Built-in (Standard) y URP (Lit)

            mat.color = baseColor;

            if (mat.HasProperty("_Color")) mat.SetColor("_Color", baseColor);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", baseColor);



            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);



            if (texture != null)

            {

                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);

                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);

            }



            return mat;

        }



        public static Material CreateEmissiveMaterial(string name, Color glowColor, float intensity, Texture2D texture = null)

        {

            Shader shader = GetEmissiveShader();

            Material mat = new Material(shader);

            mat.name = name;



            Color finalGlow = glowColor * intensity;

            mat.color = glowColor;



            if (mat.HasProperty("_Color")) mat.SetColor("_Color", glowColor);

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", glowColor);



            if (mat.HasProperty("_EmissionColor"))

            {

                mat.EnableKeyword("_EMISSION");

                mat.SetColor("_EmissionColor", finalGlow);

            }



            if (texture != null)

            {

                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);

                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);

                if (mat.HasProperty("_EmissionMap")) mat.SetTexture("_EmissionMap", texture);

            }



            return mat;

        }



        /// <summary>

        /// Aplica la paleta Mecha completa sobre la araña 3D.

        /// Oculta los renderizadores de UCX (cajas de colisión) para revelar el chasis interior.

        /// </summary>

        public static void ApplyMechaSpiderPalette(GameObject spiderRoot, Material skinMat, Material eyeCyanMat, Material eyeAmberMat, Material steelMat, Material nitrideMat)

        {

            if (spiderRoot == null) return;



            MeshRenderer[] renderers = spiderRoot.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var mr in renderers)

            {

                string n = mr.gameObject.name;



                // 1. Ocultar estrictamente colisionadores UCX importados

                if (n.StartsWith("UCX_"))

                {

                    mr.enabled = false;

                    continue;

                }



                // 2. Núcleos oculares y sensores

                if (n.Contains("CorePupil") || n.Contains("Weakspot") || n.Contains("VentBar"))

                {

                    mr.material = eyeCyanMat;

                }

                else if (n.Contains("OuterIris") || n.Contains("EyeRing"))

                {

                    mr.material = eyeAmberMat;

                }

                else if (n.Contains("Piston"))

                {

                    mr.material = nitrideMat;

                }

                else if (n.Contains("Socket") || n.Contains("Joint") || n.Contains("FootPad") || n.Contains("LowerBelly") || n.Contains("SensorNodule"))

                {

                    mr.material = steelMat;

                }

                else

                {

                    // Chasis, vigas de patas, armadura y power pack

                    mr.material = skinMat;

                }

            }

        }



        /// <summary>

        /// Aplica la paleta pesada Mecha sobre el Quantum Centurion Boss 3D.

        /// Oculta los renderizadores de UCX (cajas de colisión) para que el boss no se vea como un cubo blanco.

        /// </summary>

        public static void ApplyQuantumBossPalette(GameObject bossRoot, Material armorMat, Material reactorCyanMat, Material heatSinkMat, Material visorRedMat, Material exhaustOrangeMat, Material steelMat)

        {

            if (bossRoot == null) return;



            MeshRenderer[] renderers = bossRoot.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var mr in renderers)

            {

                string n = mr.gameObject.name;



                // 1. Ocultar estrictamente colisionadores UCX importados

                if (n.StartsWith("UCX_"))

                {

                    mr.enabled = false;

                    continue;

                }



                // 2. Reactores, visores y disipadores térmicos

                if (n.Contains("Reactor") || n.Contains("Gatling_MuzzleGlow") || n.Contains("Core"))

                {

                    mr.material = reactorCyanMat;

                }

                else if (n.Contains("Visor") || n.Contains("OpticSlit"))

                {

                    mr.material = visorRedMat;

                }

                else if (n.Contains("HeatSink"))

                {

                    mr.material = heatSinkMat;

                }

                else if (n.Contains("Radiator") || n.Contains("Exhaust"))

                {

                    mr.material = exhaustOrangeMat;

                }

                else if (n.Contains("Gatling") || n.Contains("Joint") || n.Contains("FootCleat") || n.Contains("JetNozzle") || n.Contains("Pelvis") || n.Contains("MissileTube") || n.Contains("Arm_"))

                {

                    mr.material = steelMat;

                }

                else

                {

                    // Placas de blindaje cuántico, yelmo, hombreras y escudo balístico

                    mr.material = armorMat;

                }

            }

        }



        /// <summary>

        /// Aplica la paleta Anime Heroine sobre la malla 3D de Nyx y oculta la cápsula UCX.

        /// </summary>

        public static Material CreateCutoutMaterial(string name, Texture2D texture, float cutoff = 0.35f)

        {

            Shader shader = GetPbrShader();

            Material mat = new Material(shader) { name = name };

            if (texture != null)

            {

                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);

                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);

            }

            if (mat.HasProperty("_Mode")) mat.SetFloat("_Mode", 1); // Cutout

            if (mat.HasProperty("_Cutoff")) mat.SetFloat("_Cutoff", cutoff);

            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);

            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);

            mat.SetInt("_ZWrite", 1);

            mat.EnableKeyword("_ALPHATEST_ON");

            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;

            return mat;

        }



        /// <summary>

        /// Aplica las texturas UV individuales de alta fidelidad sobre la heroína anime táctica de Nyx.

        /// </summary>

        public static void ApplyNyxAnimeHeroineMaterials(GameObject nyxRoot)

        {

            if (nyxRoot == null) return;



            // [ES] Si el modelo 3D es la nueva malla optimizada de Meshy, aplicar su material PBR

            // [EN] If the 3D model is the new Meshy optimized mesh, apply its PBR material

            Texture2D nyxPbrTex = Resources.Load<Texture2D>("Textures/Characters/nyx_heroine");

            if (nyxPbrTex != null)

            {

                Material pbrMat = CreatePbrMaterial("Mat_Nyx_Heroine_3D_Runtime", Color.white, 0.45f, 0.15f, nyxPbrTex);

                foreach (var r in nyxRoot.GetComponentsInChildren<Renderer>(true))

                {

                    r.material = pbrMat;

                }

                return;

            }



            Texture2D topsTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_tops");

            Texture2D bottomsTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_bottoms");

            Texture2D shoesTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_shoes");

            Texture2D hairTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_hair");

            Texture2D eyesTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_eyes_iris");

            Texture2D faceTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_face_skin");

            Texture2D bodyTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_body_skin");

            Texture2D eyeWhiteTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_eyewhite");

            Texture2D eyeHighlightTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_eyehighlight");

            Texture2D eyeLineTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_eyeline");

            Texture2D eyeLashTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_eyelash");

            Texture2D browTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_eyebrow");

            Texture2D mouthTex = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_mouth");



            // Materiales de Vestimenta

            Material bodySkinMat = Resources.Load<Material>("Materials/Mat_Nyx_Body_Skin") ?? CreatePbrMaterial("Mat_Nyx_Body_Skin", Color.white, 0.4f, 0.0f, bodyTex);

            

            // Chaqueta Táctica con emisión cian neón en circuitos

            Material topsMat = Resources.Load<Material>("Materials/Mat_Nyx_Tops_Tactical") ?? CreatePbrMaterial("Mat_Nyx_Tops_Tactical", Color.white, 0.4f, 0.15f, topsTex);



            Material bottomsMat = Resources.Load<Material>("Materials/Mat_Nyx_Bottoms_Tactical") ?? CreatePbrMaterial("Mat_Nyx_Bottoms_Tactical", Color.white, 0.3f, 0.1f, bottomsTex);

            Material shoesMat = Resources.Load<Material>("Materials/Mat_Nyx_Shoes_Tactical") ?? CreatePbrMaterial("Mat_Nyx_Shoes_Tactical", Color.white, 0.5f, 0.2f, shoesTex);



            // Materiales de Rostro y Cabello

            Material faceSkinMat = Resources.Load<Material>("Materials/Mat_Nyx_Face_Skin") ?? CreatePbrMaterial("Mat_Nyx_Face_Skin", Color.white, 0.35f, 0.0f, faceTex);

            Material eyesMat = Resources.Load<Material>("Materials/Mat_Nyx_Eyes_Iris") ?? CreatePbrMaterial("Mat_Nyx_Eyes_Iris", Color.white, 0.8f, 0.0f, eyesTex);

            if (eyesTex != null)

            {

                eyesMat.EnableKeyword("_EMISSION");

                if (eyesMat.HasProperty("_EmissionMap")) eyesMat.SetTexture("_EmissionMap", eyesTex);

                if (eyesMat.HasProperty("_EmissionColor")) eyesMat.SetColor("_EmissionColor", new Color(0.3f, 0.1f, 0.5f, 1.0f));

            }



            Material hairMat = Resources.Load<Material>("Materials/Mat_Nyx_Hair_Crimson") ?? CreateCutoutMaterial("Mat_Nyx_Hair_Crimson", hairTex, 0.4f);

            Material browMat = CreateCutoutMaterial("Mat_Nyx_FaceBrow", browTex, 0.2f);

            Material eyeLashMat = CreateCutoutMaterial("Mat_Nyx_FaceEyelash", eyeLashTex, 0.2f);

            Material eyeLineMat = CreateCutoutMaterial("Mat_Nyx_FaceEyeline", eyeLineTex, 0.2f);

            Material mouthMat = CreateCutoutMaterial("Mat_Nyx_FaceMouth", mouthTex, 0.2f);

            Material eyeWhiteMat = CreatePbrMaterial("Mat_Nyx_EyeWhite", Color.white, 0.5f, 0.0f, eyeWhiteTex);

            Material eyeHighlightMat = CreateCutoutMaterial("Mat_Nyx_EyeHighlight", eyeHighlightTex, 0.2f);



            Renderer[] renderers = nyxRoot.GetComponentsInChildren<Renderer>(true);

            foreach (var r in renderers)

            {

                string n = r.gameObject.name;

                if (n.Contains("Body"))

                {

                    r.materials = new Material[] { bodySkinMat, topsMat, bottomsMat, shoesMat };

                }

                else if (n.Contains("Face"))

                {

                    r.materials = new Material[] { mouthMat, eyeWhiteMat, eyeLineMat, eyeLashMat, browMat, eyesMat, eyeHighlightMat, faceSkinMat, eyeWhiteMat };

                }

                else if (n.Contains("Hair"))

                {

                    r.materials = new Material[] { hairMat, hairMat };

                }

            }

        }



        public static void ApplyNyxAnime3DPalette(GameObject nyxRoot)

        {

            if (nyxRoot == null) return;



            // 1. Cargar el material PBR HD oficial de Nyx con textura Atlas

            Material hdMat = Resources.Load<Material>("Materials/Mat_Nyx_Heroine_HD");

            if (hdMat == null)

            {

                Texture2D albedo = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_anime3d_albedo");

                Texture2D emission = Resources.Load<Texture2D>("Textures/Companion/tex_nyx_anime3d_emission");



                Shader shader = GetPbrShader();

                hdMat = new Material(shader) { name = "Mat_Nyx_Heroine_HD_Runtime" };

                hdMat.color = Color.white;

                if (hdMat.HasProperty("_Color")) hdMat.SetColor("_Color", Color.white);

                if (hdMat.HasProperty("_BaseColor")) hdMat.SetColor("_BaseColor", Color.white);

                if (hdMat.HasProperty("_Glossiness")) hdMat.SetFloat("_Glossiness", 0.50f);

                if (hdMat.HasProperty("_Metallic")) hdMat.SetFloat("_Metallic", 0.20f);



                if (albedo != null)

                {

                    if (hdMat.HasProperty("_MainTex")) hdMat.SetTexture("_MainTex", albedo);

                    if (hdMat.HasProperty("_BaseMap")) hdMat.SetTexture("_BaseMap", albedo);

                }



                if (emission != null)

                {

                    hdMat.EnableKeyword("_EMISSION");

                    if (hdMat.HasProperty("_EmissionMap")) hdMat.SetTexture("_EmissionMap", emission);

                    if (hdMat.HasProperty("_EmissionColor")) hdMat.SetColor("_EmissionColor", new Color(1.5f, 1.5f, 1.5f, 1.0f));

                }

            }



            MeshRenderer[] renderers = nyxRoot.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var mr in renderers)

            {

                string n = mr.gameObject.name;

                if (n.StartsWith("UCX_"))

                {

                    mr.enabled = false;

                    continue;

                }



                mr.enabled = true;

                if (hdMat != null)

                {

                    Material[] newMats = new Material[mr.sharedMaterials != null && mr.sharedMaterials.Length > 0 ? mr.sharedMaterials.Length : 1];

                    for (int j = 0; j < newMats.Length; j++)

                    {

                        newMats[j] = hdMat;

                    }

                    mr.materials = newMats;

                }

            }



            SkinnedMeshRenderer[] skinnedRenderers = nyxRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);

            foreach (var smr in skinnedRenderers)

            {

                string n = smr.gameObject.name;

                if (n.StartsWith("UCX_"))

                {

                    smr.enabled = false;

                    continue;

                }



                smr.enabled = true;

                if (hdMat != null)

                {

                    Material[] newMats = new Material[smr.sharedMaterials != null && smr.sharedMaterials.Length > 0 ? smr.sharedMaterials.Length : 1];

                    for (int j = 0; j < newMats.Length; j++)

                    {

                        newMats[j] = hdMat;

                    }

                    smr.materials = newMats;

                }

            }

        }

    }

}

