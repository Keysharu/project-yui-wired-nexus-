using System;
using UnityEngine;
using QuestMrNexus.Core;

namespace ProjectNyx.Core
{
    public enum MicMode
    {
        PushToTalk,
        OpenMic
    }

    /// <summary>
    /// [ES] Ajustes persistentes del jugador (PlayerPrefs). Contrato compartido entre módulos.
    /// Almacena volumen, idioma activo, sensibilidad de controles y opciones gráficas.
    /// ---
    /// [EN] Persistent player preferences and settings (PlayerPrefs). Shared architectural contract.
    /// Stores audio volumes, active language, input sensitivity, and graphical performance settings.
    /// </summary>
    public static class GameSettings
    {
        private const string KeyPrefix = "nyx.settings.";

        // ---------- Volúmenes (0..1) ----------
        public static float MasterVolume { get; private set; } = 1.0f;
        public static float MusicVolume { get; private set; } = 0.8f;
        public static float PresenterVolume { get; private set; } = 1.0f;   // Mina y Kira
        public static float NyxVolume { get; private set; } = 1.0f;
        public static float EnvironmentVolume { get; private set; } = 0.9f; // SFX, disparos, entorno

        // ---------- Idioma ----------
        public static LocalizationManager.Language Language { get; private set; } = LocalizationManager.Language.Spanish;

        // ---------- Micrófono ----------
        public static bool MicEnabled { get; private set; } = true;
        public static string MicDeviceName { get; private set; } = string.Empty; // vacío = dispositivo por defecto
        public static float MicGain { get; private set; } = 1.0f;                // 0..2
        public static MicMode MicActivation { get; private set; } = MicMode.PushToTalk;
        public static KeyCode PushToTalkKey { get; private set; } = KeyCode.T;
        public static bool VoiceChatEnabled { get; private set; } = true;        // hablar con otros jugadores (online)
        public static bool NyxVoiceCommandsEnabled { get; private set; } = true; // hablar / pedir cosas a Nyx

        // ---------- Servicios locales ----------
        public static string TtsServerUrl { get; private set; } = "http://127.0.0.1:8880";

        private static bool loaded;

        /// <summary>Se dispara tras cualquier cambio (UI de opciones, carga, etc.).</summary>
        public static event Action Changed;

        /// <summary>Se dispara solo cuando cambia el idioma (voces + textos deben refrescarse).</summary>
        public static event Action<LocalizationManager.Language> LanguageChanged;

        public static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;

            MasterVolume = PlayerPrefs.GetFloat(KeyPrefix + "master", MasterVolume);
            MusicVolume = PlayerPrefs.GetFloat(KeyPrefix + "music", MusicVolume);
            PresenterVolume = PlayerPrefs.GetFloat(KeyPrefix + "presenters", PresenterVolume);
            NyxVolume = PlayerPrefs.GetFloat(KeyPrefix + "nyx", NyxVolume);
            EnvironmentVolume = PlayerPrefs.GetFloat(KeyPrefix + "environment", EnvironmentVolume);

            Language = (LocalizationManager.Language)PlayerPrefs.GetInt(KeyPrefix + "language", (int)Language);

            MicEnabled = PlayerPrefs.GetInt(KeyPrefix + "mic.enabled", MicEnabled ? 1 : 0) == 1;
            MicDeviceName = PlayerPrefs.GetString(KeyPrefix + "mic.device", MicDeviceName);
            MicGain = PlayerPrefs.GetFloat(KeyPrefix + "mic.gain", MicGain);
            MicActivation = (MicMode)PlayerPrefs.GetInt(KeyPrefix + "mic.mode", (int)MicActivation);
            PushToTalkKey = (KeyCode)PlayerPrefs.GetInt(KeyPrefix + "mic.ptt", (int)PushToTalkKey);
            VoiceChatEnabled = PlayerPrefs.GetInt(KeyPrefix + "mic.voicechat", VoiceChatEnabled ? 1 : 0) == 1;
            NyxVoiceCommandsEnabled = PlayerPrefs.GetInt(KeyPrefix + "mic.nyxcmd", NyxVoiceCommandsEnabled ? 1 : 0) == 1;

            TtsServerUrl = PlayerPrefs.GetString(KeyPrefix + "tts.url", TtsServerUrl);
        }

        public static void Save()
        {
            PlayerPrefs.SetFloat(KeyPrefix + "master", MasterVolume);
            PlayerPrefs.SetFloat(KeyPrefix + "music", MusicVolume);
            PlayerPrefs.SetFloat(KeyPrefix + "presenters", PresenterVolume);
            PlayerPrefs.SetFloat(KeyPrefix + "nyx", NyxVolume);
            PlayerPrefs.SetFloat(KeyPrefix + "environment", EnvironmentVolume);
            PlayerPrefs.SetInt(KeyPrefix + "language", (int)Language);
            PlayerPrefs.SetInt(KeyPrefix + "mic.enabled", MicEnabled ? 1 : 0);
            PlayerPrefs.SetString(KeyPrefix + "mic.device", MicDeviceName ?? string.Empty);
            PlayerPrefs.SetFloat(KeyPrefix + "mic.gain", MicGain);
            PlayerPrefs.SetInt(KeyPrefix + "mic.mode", (int)MicActivation);
            PlayerPrefs.SetInt(KeyPrefix + "mic.ptt", (int)PushToTalkKey);
            PlayerPrefs.SetInt(KeyPrefix + "mic.voicechat", VoiceChatEnabled ? 1 : 0);
            PlayerPrefs.SetInt(KeyPrefix + "mic.nyxcmd", NyxVoiceCommandsEnabled ? 1 : 0);
            PlayerPrefs.SetString(KeyPrefix + "tts.url", TtsServerUrl ?? string.Empty);
            PlayerPrefs.Save();
        }

        // ---------- Setters (validan rango, guardan y notifican) ----------
        public static void SetMasterVolume(float v) { MasterVolume = Mathf.Clamp01(v); Commit(); }
        public static void SetMusicVolume(float v) { MusicVolume = Mathf.Clamp01(v); Commit(); }
        public static void SetPresenterVolume(float v) { PresenterVolume = Mathf.Clamp01(v); Commit(); }
        public static void SetNyxVolume(float v) { NyxVolume = Mathf.Clamp01(v); Commit(); }
        public static void SetEnvironmentVolume(float v) { EnvironmentVolume = Mathf.Clamp01(v); Commit(); }

        public static void SetLanguage(LocalizationManager.Language lang)
        {
            if (Language == lang) return;
            Language = lang;
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.SetLanguage(lang);
            }
            Commit();
            LanguageChanged?.Invoke(lang);
        }

        public static void SetMicEnabled(bool v) { MicEnabled = v; Commit(); }
        public static void SetMicDevice(string deviceName) { MicDeviceName = deviceName ?? string.Empty; Commit(); }
        public static void SetMicGain(float v) { MicGain = Mathf.Clamp(v, 0f, 2f); Commit(); }
        public static void SetMicActivation(MicMode m) { MicActivation = m; Commit(); }
        public static void SetPushToTalkKey(KeyCode k) { PushToTalkKey = k; Commit(); }
        public static void SetVoiceChatEnabled(bool v) { VoiceChatEnabled = v; Commit(); }
        public static void SetNyxVoiceCommandsEnabled(bool v) { NyxVoiceCommandsEnabled = v; Commit(); }
        public static void SetTtsServerUrl(string url) { TtsServerUrl = string.IsNullOrWhiteSpace(url) ? "http://127.0.0.1:8880" : url.Trim(); Commit(); }

        private static void Commit()
        {
            Save();
            Changed?.Invoke();
        }

        /// <summary>Volumen efectivo del bus de música (Master × Music).</summary>
        public static float EffectiveMusic => MasterVolume * MusicVolume;
        /// <summary>Volumen efectivo de las presentadoras (Master × Presenters).</summary>
        public static float EffectivePresenter => MasterVolume * PresenterVolume;
        /// <summary>Volumen efectivo de Nyx (Master × Nyx).</summary>
        public static float EffectiveNyx => MasterVolume * NyxVolume;
        /// <summary>Volumen efectivo del entorno/SFX (Master × Environment).</summary>
        public static float EffectiveEnvironment => MasterVolume * EnvironmentVolume;
    }
}