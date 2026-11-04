using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using ProjectNyx.Core;

namespace ProjectNyx.EditorTools
{
    /// <summary>
    /// [ES] Configura la Pantalla Gigante de la Mega Torre en MainMRScene para retransmitir
    /// YouTube Live (Radio Chipset 107.7 FM) con respaldo instantáneo del vídeo MP4 de Mits.
    /// Autora: Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    [InitializeOnLoad]
    public static class SetupMegatowerScreen
    {
        [MenuItem("Wired Nexus MR/6. Configurar Pantalla MegaTorre (YouTube Live y Mits)", false, 60)]
        public static void ConfigureScreen()
        {
            Debug.Log("[MegaTowerSetup] Configurando pantalla gigante de la Mega Torre...");

            Scene scene = SceneManager.GetActiveScene();
            GameObject screenObj = GameObject.Find("Megatower_BroadcastScreen");
            if (screenObj == null)
            {
                Debug.LogError("[MegaTowerSetup] No se encontró el objeto 'Megatower_BroadcastScreen' en la escena.");
                return;
            }

            // 1. Quitar FlowRoomBackdropAnimator si existe (evita conflicto con el vídeo)
            FlowRoomBackdropAnimator anim = screenObj.GetComponent<FlowRoomBackdropAnimator>();
            if (anim != null)
            {
                UnityEngine.Object.DestroyImmediate(anim);
                Debug.Log("[MegaTowerSetup] FlowRoomBackdropAnimator retirado de Megatower_BroadcastScreen.");
            }

            // 2. Configurar Material y Shader limpio anti-magenta
            MeshRenderer mr = screenObj.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Shader unlitTex = Shader.Find("Unlit/Texture") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Sprites/Default");
                string matPath = "Assets/Prefabs/Mat_3D_VRLobbyTowerScreen.mat";
                Material screenMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (screenMat == null)
                {
                    screenMat = new Material(unlitTex) { name = "Mat_3D_VRLobbyTowerScreen" };
                    AssetDatabase.CreateAsset(screenMat, matPath);
                }
                else
                {
                    screenMat.shader = unlitTex;
                }
                screenMat.color = Color.white;
                EditorUtility.SetDirty(screenMat);
                mr.sharedMaterial = screenMat;
            }

            // 3. Añadir AudioSource si no existe
            AudioSource audio = screenObj.GetComponent<AudioSource>();
            if (audio == null)
            {
                audio = screenObj.AddComponent<AudioSource>();
                audio.playOnAwake = false;
                audio.spatialBlend = 1.0f; // 3D Audio
                audio.minDistance = 3.0f;
                audio.maxDistance = 45.0f;
                audio.rolloffMode = AudioRolloffMode.Logarithmic;
            }

            // 4. Añadir VideoPlayer si no existe
            VideoPlayer vp = screenObj.GetComponent<VideoPlayer>();
            if (vp == null)
            {
                vp = screenObj.AddComponent<VideoPlayer>();
            }
            vp.playOnAwake = false;
            vp.isLooping = true;
            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
            vp.EnableAudioTrack(0, true);
            vp.SetTargetAudioSource(0, audio);

            // 5. Añadir YouTubeLiveStreamResolver
            YouTubeLiveStreamResolver resolver = screenObj.GetComponent<YouTubeLiveStreamResolver>();
            if (resolver == null)
            {
                resolver = screenObj.AddComponent<YouTubeLiveStreamResolver>();
            }

            // 6. Añadir y configurar MegaTowerStreamScreenBridge
            MegaTowerStreamScreenBridge bridge = screenObj.GetComponent<MegaTowerStreamScreenBridge>();
            if (bridge == null)
            {
                bridge = screenObj.AddComponent<MegaTowerStreamScreenBridge>();
            }

            SerializedObject soBridge = new SerializedObject(bridge);
            soBridge.FindProperty("feedSource").enumValueIndex = (int)MegaTowerStreamScreenBridge.ScreenFeedSource.YouTubeLiveStream;
            soBridge.FindProperty("youtubeLiveUrl").stringValue = "https://www.youtube.com/live/j_bLVMldHsA?is=G6wSXMPCZ4bmVci9";
            soBridge.FindProperty("videoClipNameOrUrl").stringValue = "mits_studio_lofi_100s.mp4";
            soBridge.FindProperty("enableDynamicPerspectiveHeight").boolValue = true;
            soBridge.FindProperty("enableDynamicTilt").boolValue = true;
            soBridge.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(screenObj);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[MegaTowerSetup] ✅ ¡Pantalla de la Mega Torre configurada con éxito con YouTube Live + Mits Lo-Fi!");
        }
    }
}
