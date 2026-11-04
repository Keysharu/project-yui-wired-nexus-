/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Gestor de Pausa Inteligente y Configuración de Audio, Idioma y Entorno AR/VR.
 *      - Botón directo para silenciar/activar el audio local y priorizar el streaming de YouTube.
 *      - Conmutador directo de entorno (VR Bulevar Inmersivo vs AR Salón con Passthrough).
 *      - Control de volumen por canales e idioma dinámico (Español / Inglés).
 * ---
 * [EN] Smart Pause Manager and Audio, Language, and AR/VR Environment Configuration.
 *      - Direct toggle button to mute/unmute local audio and prioritize YouTube live streaming.
 *      - Direct environment switcher (Immersive VR Boulevard vs Physical Room AR Passthrough).
 *      - Multi-channel volume control and runtime dynamic language switching (Spanish / English).
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.Core
{
    public enum GameLanguage
    {
        Spanish,
        English
    }

    /// <summary>
    /// [ES] Menú de Pausa LainOS con control táctico de audio local, streaming y alternancia AR/VR.
    /// [EN] LainOS Pause Menu with tactical control of local audio, live streaming, and AR/VR toggling.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Ajuste de Modo")]
        [SerializeField] private bool isOnlineSession = false;
        [SerializeField] private bool isPaused = false;

        [Header("Control de Audio Local vs Streaming")]
        [Tooltip("[ES] Si está activo, silencia las pistas locales y deja solo la emisión en vivo de YouTube.")]
        [SerializeField] private bool muteLocalMusicRadio = true;

        [Header("Niveles de Volumen (0.0 a 1.0)")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.85f;
        [Range(0f, 1f)] [SerializeField] private float presentersVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float nyxVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float sfxEnvironmentVolume = 0.9f;

        [Header("Idioma Activo")]
        [SerializeField] private GameLanguage currentLanguage = GameLanguage.Spanish;

        private static PauseMenuController instance;
        public static PauseMenuController Instance => instance;

        public bool IsPaused => isPaused;
        public GameLanguage CurrentLanguage => currentLanguage;
        public bool MuteLocalMusicRadio => muteLocalMusicRadio;

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float PresentersVolume => presentersVolume;
        public float NyxVolume => nyxVolume;
        public float SfxVolume => sfxEnvironmentVolume;

        public event Action<bool> OnPauseStateChanged;
        public event Action<GameLanguage> OnLanguageChanged;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Aplicar el estado inicial de silencio para priorizar el streaming
            SetLocalAudioMute(muteLocalMusicRadio);
        }

        private void Update()
        {
            // Atajo de teclado 'Escape' en PC o botón de menú / Inicio en Touch Plus
            if (Input.GetKeyDown(KeyCode.Escape) || OVRInput.GetDown(OVRInput.Button.Start))
            {
                TogglePause();
            }

            // Atajo directo [M] para alternar el silencio del audio local en cualquier momento
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleLocalAudioMute();
            }
        }

        public void TogglePause()
        {
            SetPause(!isPaused);
        }

        public void SetPause(bool pause)
        {
            isPaused = pause;

            if (!isOnlineSession && Application.isEditor)
            {
                Time.timeScale = pause ? 0f : 1f;
            }

            OnPauseStateChanged?.Invoke(isPaused);
            Debug.Log($"[PauseMenuController] Estado de Pausa: {isPaused} | Online: {isOnlineSession}");
        }

        public void ToggleLocalAudioMute()
        {
            SetLocalAudioMute(!muteLocalMusicRadio);
        }

        /// <summary>
        /// [ES] Silencia o reactiva la música y radio locales para dejar únicamente el streaming en directo.
        /// </summary>
        public void SetLocalAudioMute(bool mute)
        {
            muteLocalMusicRadio = mute;

            if (mute)
            {
                if (BattleMusicManager.Instance != null)
                {
                    BattleMusicManager.Instance.DisableOriginalMusicForStreaming = true;
                    BattleMusicManager.Instance.StopOriginalMusic();
                }

                RadioReceiver3D radio = UnityEngine.Object.FindAnyObjectByType<RadioReceiver3D>();
                if (radio != null)
                {
                    radio.DisableInternalRadioForStreaming = true;
                    radio.StopInternalRadio();
                }
            }
            else
            {
                if (BattleMusicManager.Instance != null)
                {
                    BattleMusicManager.Instance.DisableOriginalMusicForStreaming = false;
                    BattleMusicManager.Instance.EnsureRadio247Playing();
                }

                RadioReceiver3D radio = UnityEngine.Object.FindAnyObjectByType<RadioReceiver3D>();
                if (radio != null)
                {
                    radio.DisableInternalRadioForStreaming = false;
                }
            }

            Debug.Log($"[PauseMenuController] Audio Local: {(mute ? "SILENCIADO (Streaming Exclusivo)" : "REACTIVADO (Música Original)")}");
        }

        public void SetLanguage(GameLanguage newLang)
        {
            currentLanguage = newLang;
            OnLanguageChanged?.Invoke(currentLanguage);
            Debug.Log($"[PauseMenuController] Idioma cambiado a: {currentLanguage}");
        }

        public void SetVolumeChannels(float master, float music, float presenters, float nyx, float sfx)
        {
            masterVolume = Mathf.Clamp01(master);
            musicVolume = Mathf.Clamp01(music);
            presentersVolume = Mathf.Clamp01(presenters);
            nyxVolume = Mathf.Clamp01(nyx);
            sfxEnvironmentVolume = Mathf.Clamp01(sfx);
        }

        private void OnGUI()
        {
            if (!isPaused || !Application.isEditor) return;

            // Renderizado GUI estilo LainOS Cyberpunk
            GUI.color = Color.white;
            int boxWidth = 500;
            int boxHeight = 520;
            int startX = (Screen.width - boxWidth) / 2;
            int startY = (Screen.height - boxHeight) / 2;

            GUI.Box(new Rect(startX, startY, boxWidth, boxHeight), "═══ SISTEMA // MENÚ DE PAUSA TÁCTICO (LAINOS) ═══");

            GUILayout.BeginArea(new Rect(startX + 20, startY + 35, boxWidth - 40, boxHeight - 50));

            GUILayout.Label(isOnlineSession ? "MODO: SESIÓN EN LÍNEA (Mundo Activo)" : "MODO: LOCAL OFFLINE (Simulación Pausada)");
            GUILayout.Space(8);

            // 1. Botón Reanudar
            if (GUILayout.Button("▶ REANUDAR PARTIDA", GUILayout.Height(34)))
            {
                SetPause(false);
            }
            GUILayout.Space(10);

            // 2. Control de Audio Local vs Streaming (La petición clave de Raquel)
            string localAudioStatus = muteLocalMusicRadio 
                ? "🔴 AUDIO LOCAL: SILENCIADO (SOLO STREAMING YOUTUBE)" 
                : "🟢 AUDIO LOCAL: ACTIVO (MÚSICA Y VOCES INTERNAS)";

            if (GUILayout.Button($"📻 {localAudioStatus}", GUILayout.Height(36)))
            {
                ToggleLocalAudioMute();
            }
            GUILayout.Space(8);

            // 3. Alternancia de Entorno AR (Passthrough) vs VR (Bulevar)
            CyberRoomDecorator3D dec = UnityEngine.Object.FindAnyObjectByType<CyberRoomDecorator3D>();
            string envStatus = (dec != null && dec.IsFullVrActive) 
                ? "🌐 ENTORNO: VR BULEVAR INMERSIVO [Pulsar para AR Salón]" 
                : "🏠 ENTORNO: AR SALÓN REAL PASSTHROUGH [Pulsar para VR Bulevar]";

            if (GUILayout.Button(envStatus, GUILayout.Height(34)))
            {
                if (dec != null) dec.ToggleVrArMode();
            }
            GUILayout.Space(12);

            // 4. Sliders de Volumen
            GUILayout.Label($"Volumen General: {Mathf.RoundToInt(masterVolume * 100)}%");
            masterVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f);

            GUILayout.Label($"Música 24/7 (Radio Chipset): {Mathf.RoundToInt(musicVolume * 100)}%");
            musicVolume = GUILayout.HorizontalSlider(musicVolume, 0f, 1f);

            GUILayout.Label($"Locutoras (Mina & Kira): {Mathf.RoundToInt(presentersVolume * 100)}%");
            presentersVolume = GUILayout.HorizontalSlider(presentersVolume, 0f, 1f);

            GUILayout.Label($"Nyx (Compañera IA): {Mathf.RoundToInt(nyxVolume * 100)}%");
            nyxVolume = GUILayout.HorizontalSlider(nyxVolume, 0f, 1f);

            GUILayout.Label($"Efectos y Entorno (SFX): {Mathf.RoundToInt(sfxEnvironmentVolume * 100)}%");
            sfxEnvironmentVolume = GUILayout.HorizontalSlider(sfxEnvironmentVolume, 0f, 1f);

            GUILayout.Space(10);
            GUILayout.Label($"Idioma / Language: [{currentLanguage}]");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Español (ES)")) SetLanguage(GameLanguage.Spanish);
            if (GUILayout.Button("English (EN)")) SetLanguage(GameLanguage.English);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }
    }
}
