/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Configurador Editor para la Emisora de Radio 107.7 FM Autónoma e Interna.
 *      Asigna playlists de música synthwave/cyberpunk y locuciones de Kira y Mina,
 *      configura AudioSources 3D dedicados y garantiza cero dependencias de red.
 * ---
 * [EN] Editor Configurator for Autonomous Internal 107.7 FM Radio Station.
 *      Binds synthwave/cyberpunk music playlists and Kira/Mina presenter voiceovers,
 *      configures dedicated 3D AudioSources, and guarantees zero network dependencies.
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.EditorTools
{
    [InitializeOnLoad]
    public static class SetupStandaloneRadioStation
    {
        private const string RadioGameObjectName = "Radio_Chipset_107_7_FM";

        static SetupStandaloneRadioStation()
        {
            EditorApplication.delayCall += () =>
            {
                if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    ConfigureStationNow();
                }
            };
        }

        [MenuItem("Wired Nexus MR/★ LEVANTAR EMISORA RADIO 107.7 FM AUTÓNOMA (Solo Audio + Kira & Mina)", false, 3)]
        public static void ConfigureStationNow()
        {
            Debug.Log("[SetupRadio] 📻 Configurando Emisora Autónoma Radio Chipset 107.7 FM...");

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded || !scene.name.Contains("MainMRScene"))
            {
                scene = EditorSceneManager.OpenScene("Assets/MainMRScene.unity", OpenSceneMode.Single);
            }

            // 1. Localizar o crear GameObject de la Emisora Autónoma
            GameObject radioObj = GameObject.Find(RadioGameObjectName);
            if (radioObj == null)
            {
                // Buscar si existe el transistor físico en el salón
                GameObject radioPedestal = GameObject.Find("Radio_Chipset_3D");
                if (radioPedestal != null)
                {
                    radioObj = radioPedestal;
                }
                else
                {
                    radioObj = new GameObject(RadioGameObjectName);
                    radioObj.transform.position = new Vector3(0.3f, 1.0f, 1.8f);
                }
            }

            StandaloneRadioStationChipset station = radioObj.GetComponent<StandaloneRadioStationChipset>();
            if (station == null)
            {
                station = radioObj.AddComponent<StandaloneRadioStationChipset>();
            }

            KiraMinaRadioAI radioAi = radioObj.GetComponent<KiraMinaRadioAI>();
            if (radioAi == null)
            {
                radioAi = radioObj.AddComponent<KiraMinaRadioAI>();
            }

            // 2. Cargar pistas de música local (Suno)
            string[] musicPaths = new string[]
            {
                "Assets/Audio/Battle/Normal/Ronin_Code_Focus_Beats.mp3",
                "Assets/Audio/Battle/Normal/Syndicate_Combat.mp3",
                "Assets/Audio/Battle/Normal/Synthwave_Circuit_Board_Drive.mp3",
                "Assets/Audio/Battle/Boss/Fortuna_Empire_Doomsday_Loop.mp3",
                "Assets/Audio/Battle/Boss/Fairy tail 2.5.mp3",
                "Assets/Audio/Battle/FinalBoss/Terminal_Breach.mp3"
            };

            List<AudioClip> musicList = new List<AudioClip>();
            foreach (string p in musicPaths)
            {
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                if (clip != null) musicList.Add(clip);
            }

            // 3. Cargar locuciones diferenciadas de Kira, Mina y Nyx
            AudioClip[] kiraClips = new AudioClip[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_01_kira_welcome_247.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_04_kira_boss_alert.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_06_kira_climax_energizer.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/test_kira.mp3")
            };

            AudioClip[] minaClips = new AudioClip[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_02_mina_tactical_brief.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_07_mina_victory_247.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/test_mina.mp3")
            };

            AudioClip[] nyxClips = new AudioClip[]
            {
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_03_nyx_wave_banter.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_05_nyx_battle_stance.mp3"),
                AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/test_nyx.mp3")
            };

            List<AudioClip> presenterList = new List<AudioClip>();
            foreach (var c in kiraClips) if (c != null && !presenterList.Contains(c)) presenterList.Add(c);
            foreach (var c in minaClips) if (c != null && !presenterList.Contains(c)) presenterList.Add(c);
            foreach (var c in nyxClips) if (c != null && !presenterList.Contains(c)) presenterList.Add(c);

            AudioClip welcomeClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_01_kira_welcome_247.mp3");
            AudioClip bossAlertClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_04_kira_boss_alert.mp3");
            AudioClip climaxClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_06_kira_climax_energizer.mp3");
            AudioClip victoryClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Voices/voice_07_mina_victory_247.mp3");

            station.SetupPlaylists(musicList, presenterList, welcomeClip, bossAlertClip, climaxClip, victoryClip);
            radioAi.ConfigureVoiceClips(kiraClips, minaClips, nyxClips);
            EditorUtility.SetDirty(radioAi);

            // 4. Mute de audio en pantalla de streaming para evitar solapamientos
            HologramCyberCrestDisplay crest = UnityEngine.Object.FindAnyObjectByType<HologramCyberCrestDisplay>();
            if (crest != null)
            {
                AudioSource crestAudio = crest.GetComponent<AudioSource>();
                if (crestAudio != null)
                {
                    crestAudio.volume = 0f;
                    crestAudio.mute = true;
                    EditorUtility.SetDirty(crestAudio);
                }
            }

            // 5. Silenciar BattleMusicManager
            BattleMusicManager bmm = UnityEngine.Object.FindAnyObjectByType<BattleMusicManager>();
            if (bmm != null)
            {
                bmm.DisableOriginalMusicForStreaming = true;
                bmm.StopOriginalMusic();
                EditorUtility.SetDirty(bmm);
            }

            EditorUtility.SetDirty(station);
            EditorUtility.SetDirty(radioObj);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[SetupRadio] ✅ ¡EMISORA RADIO 107.7 FM LISTA! {musicList.Count} canciones + {presenterList.Count} locuciones de Kira y Mina configuradas.");
        }
    }
}
