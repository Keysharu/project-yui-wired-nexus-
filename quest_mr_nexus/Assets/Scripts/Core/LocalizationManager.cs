using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectNyx.Core;

namespace QuestMrNexus.Core
{
    /// <summary>
    /// [ES] Gestor de localización ligero para Meta Quest 3S + escritorio.
    /// Soporta cambio dinámico de cadenas de texto entre Español e Inglés en tiempo de ejecución.
    /// ---
    /// [EN] Lightweight localization manager for Meta Quest 3S and desktop standalone.
    /// Supports dynamic runtime text localization between Spanish and English.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        public enum Language
        {
            English,
            Spanish
        }

        [Header("Language Settings")]
        [Tooltip("Idioma activo. Se sincroniza con GameSettings.Language al arrancar.")]
        [SerializeField] private Language _currentLanguage = Language.English;

        public Language CurrentLanguage => _currentLanguage;

        public event Action<Language> OnLanguageChanged;

        // Diccionarios estáticos: permiten consultar textos sin instancia en escena.
        private static readonly Dictionary<string, string> DictEn = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // ---- Claves originales ----
            { "WEAPON_BLASTER", "SOLID-LIGHT BLASTER" },
            { "WEAPON_SABER", "PLASMA ENERGY SABER" },
            { "WEAPON_CROSSBOW", "PHOTON CROSSBOW" },
            { "RADIO_STATION", "RADIO CHIPSET // WIRED FM (PIRATE RADIO)" },
            { "SCANNING_ROOM", "SCANNING SPATIAL ENVIRONMENT..." },
            { "ROOM_READY", "ROOM MESH CONFIGURED. OCCLUSION ACTIVE." },
            { "OCCLUSION_STATUS", "FURNITURE DEPTH-MASK: 90 FPS ACTIVE" },
            { "RIFT_DETECTED", "DIMENSIONAL RIFT DETECTED! INCOMING INVASION!" },
            { "RIFT_SEALED", "DIMENSIONAL RIFT STABILIZED." },
            { "BOSS_ALERT", "CRITICAL WARNING: MODULAR TITAN HIVE MIND INCOMING!" },
            { "BOSS_DEFEATED", "HIVE MIND CORE NEUTRALIZED. SECTOR SECURED." },
            { "GAUNTLET_READY", "HOLOGRAPHIC GAUNTLET ONLINE" },
            { "BATTERY_WARNING", "ENERGY MATRIX CRITICAL - RELOAD" },
            { "HOMELAB_CONNECTED", "CONNECTED TO SOVEREIGN HOMELAB (LLAMA 3.2 3B)" },
            { "HOMELAB_DISCONNECTED", "HOMELAB OFFLINE - RUNNING LOCAL EDGE PROTOCOL" },

            // ---- Menú de pausa ----
            { "MENU_TITLE", "PAUSE // SYSTEM MENU" },
            { "MENU_RESUME", "Resume" },
            { "MENU_OPTIONS", "Options" },
            { "MENU_BACK", "Back" },
            { "MENU_SESSION_MODE", "Session mode" },
            { "MENU_MODE_OFFLINE", "OFFLINE" },
            { "MENU_MODE_ONLINE", "ONLINE" },
            { "MENU_MODE_SWITCH_TO_ONLINE", "Switch to ONLINE" },
            { "MENU_MODE_SWITCH_TO_OFFLINE", "Switch to OFFLINE" },
            { "MENU_MODE_WARN_OFFLINE", "OFFLINE: the whole game is frozen while this menu is open." },
            { "MENU_MODE_WARN_ONLINE", "ONLINE: the world keeps running. Only YOUR movement and actions are blocked." },
            { "MENU_MODE_TEST_NOTE", "Testing switch: real networking is not connected yet." },
            { "MENU_QUIT", "Quit to desktop" },
            { "MENU_QUIT_CONFIRM", "Quit the game? Unsaved progress will be lost." },
            { "MENU_YES", "Yes, quit" },
            { "MENU_NO", "Cancel" },
            { "MENU_HINT_NAV", "[Up/Down] Select   [Left/Right] Adjust   [Enter] Accept   [Esc] Back/Close" },
            { "MENU_HINT_VR", "Quest: left stick = navigate, index trigger = accept, grip = back, Menu = close" },

            // ---- Pestañas de opciones ----
            { "TAB_AUDIO", "Audio" },
            { "TAB_LANGUAGE", "Language" },
            { "TAB_MIC", "Microphone" },
            { "TAB_SERVICES", "Services" },

            // ---- Audio ----
            { "VOL_MASTER", "Master volume" },
            { "VOL_MUSIC", "Music" },
            { "VOL_PRESENTERS", "Presenters (Mina & Kira)" },
            { "VOL_NYX", "Nyx" },
            { "VOL_ENV", "Environment & SFX" },
            { "SUBS_ENABLED", "Subtitles" },
            { "TOGGLE_ON", "ON" },
            { "TOGGLE_OFF", "OFF" },

            // ---- Idioma ----
            { "LANG_LABEL", "Game language" },
            { "LANG_ENGLISH", "English" },
            { "LANG_SPANISH", "Español" },
            { "LANG_NOTE", "Applies instantly: voices, subtitles and interface text." },

            // ---- Micrófono ----
            { "MIC_ENABLED", "Microphone enabled" },
            { "MIC_VOICECHAT", "Talk to other players (online)" },
            { "MIC_NYX_CMD", "Talk to Nyx / voice requests" },
            { "MIC_DEVICE", "Input device" },
            { "MIC_DEFAULT_DEVICE", "System default" },
            { "MIC_NO_DEVICES", "No microphone detected" },
            { "MIC_GAIN", "Input gain" },
            { "MIC_MODE", "Activation" },
            { "MIC_MODE_PTT", "Push-to-talk" },
            { "MIC_MODE_OPEN", "Open mic" },
            { "MIC_PTT_KEY", "Push-to-talk key" },
            { "MIC_PTT_REBIND", "Change key" },
            { "MIC_PTT_PRESS", "Press a key... (Esc cancels)" },
            { "MIC_PTT_RESERVED", "That key is reserved by the game." },
            { "MIC_REFRESH", "Refresh devices" },

            // ---- Servicios ----
            { "SVC_TTS_URL", "Local TTS server URL" },
            { "SVC_TTS_APPLY", "Apply URL" },
            { "SVC_TTS_APPLIED", "URL saved." },
            { "SVC_TTS_TEST", "Test voice" },
            { "SVC_TTS_TEST_TEXT", "Nyx online. Voice channel check complete." },
            { "SVC_TTS_NO_DIRECTOR", "Voice system not available yet." },
            { "SVC_TTS_SPEAKING", "Speaking..." },

            // ---- Subtítulos / HUD ----
            { "SPEAKER_KIRA", "Kira" },
            { "SPEAKER_MINA", "Mina" },
            { "SPEAKER_NYX", "Nyx" },
            { "HUD_RADIO_HEADER", "📻 RADIO CHIPSET 107.7 FM // ON AIR: MINA & KIRA" },
            { "HUD_STATUS_LINE", "WEAPON: [{0}] | SHIELD: {1}% | MODE: {2}" },
            { "HUD_MODE_VR", "IMMERSIVE VR" },
            { "HUD_MODE_AR", "AR LIVING ROOM" },
            { "HUD_AI_LINE", "AI: BMAX LOCAL + JENY-CODER (192.168.0.29) [0,00 €]" },
            { "HUD_BOSS", "⚠️ BOSS: QUANTUM CENTURION ({0}/{1} HP){2}" },
            { "HUD_CLIMAX", " [CLIMAX <=35%!]" },
            { "HUD_PHASE", "⚡ PHASE: {0} (Enemies: {1})" },
            { "HUD_PHASE_HINT", "Shoot the Chests (+) or press [E] -> Rift -> Wave 1 -> 3s -> Wave 2 -> Boss" },
            { "HUD_CONTROLS_1", "CONTROLS: [Left Click / Space] Shoot at Cursor | [E] Open Chests | [V] Switch AR <-> VR World" },
            { "HUD_CONTROLS_2", "[WASD] Move | [Q/R or Right Click] Look | [Tab] Switch Weapon | [Shift] Shield | [B] Jump to Boss | [Esc] Pause" },
            { "HUD_SESSION_OFFLINE", "OFFLINE" },
            { "HUD_SESSION_ONLINE", "ONLINE" },
            { "HUD_PAUSED", "PAUSED" },
            { "HUD_PAUSED_INPUT", "INPUT BLOCKED" }
        };

        private static readonly Dictionary<string, string> DictEs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // ---- Claves originales ----
            { "WEAPON_BLASTER", "BLÁSTER DE LUZ SÓLIDA" },
            { "WEAPON_SABER", "SABLE DE ENERGÍA PLASMA" },
            { "WEAPON_CROSSBOW", "BALLESTA DE FOTONES" },
            { "RADIO_STATION", "RADIO CHIPSET // WIRED FM (EMISORA PIRATA)" },
            { "SCANNING_ROOM", "ESCANEANDO ENTORNO ESPACIAL..." },
            { "ROOM_READY", "MALLA DE HABITACIÓN CONFIGURADA. OCLUSIÓN ACTIVA." },
            { "OCCLUSION_STATUS", "MÁSCARA DE PROFUNDIDAD: 90 FPS ACTIVA" },
            { "RIFT_DETECTED", "¡BRECHA DIMENSIONAL DETECTADA! ¡INVASIÓN EN CURSO!" },
            { "RIFT_SEALED", "BRECHA DIMENSIONAL ESTABILIZADA." },
            { "BOSS_ALERT", "¡ADVERTENCIA CRÍTICA: NÚCLEO TITÁN COLMENA ENTRANDO!" },
            { "BOSS_DEFEATED", "NÚCLEO COLMENA NEUTRALIZADO. SECTOR ASEGURADO." },
            { "GAUNTLET_READY", "GUANTELETE HOLOGRÁFICO EN LÍNEA" },
            { "BATTERY_WARNING", "MATRIZ DE ENERGÍA CRÍTICA - RECARGAR" },
            { "HOMELAB_CONNECTED", "CONECTADO A SERVIDOR SOBERANO (LLAMA 3.2 3B)" },
            { "HOMELAB_DISCONNECTED", "SERVIDOR DESCONECTADO - EJECUTANDO PROTOCOLO EDGE" },

            // ---- Menú de pausa ----
            { "MENU_TITLE", "PAUSA // MENÚ DEL SISTEMA" },
            { "MENU_RESUME", "Reanudar" },
            { "MENU_OPTIONS", "Opciones" },
            { "MENU_BACK", "Volver" },
            { "MENU_SESSION_MODE", "Modo de sesión" },
            { "MENU_MODE_OFFLINE", "OFFLINE" },
            { "MENU_MODE_ONLINE", "ONLINE" },
            { "MENU_MODE_SWITCH_TO_ONLINE", "Cambiar a ONLINE" },
            { "MENU_MODE_SWITCH_TO_OFFLINE", "Cambiar a OFFLINE" },
            { "MENU_MODE_WARN_OFFLINE", "OFFLINE: todo el juego queda congelado mientras este menú está abierto." },
            { "MENU_MODE_WARN_ONLINE", "ONLINE: el mundo sigue en marcha. Solo se bloquean TUS movimientos y acciones." },
            { "MENU_MODE_TEST_NOTE", "Conmutador de pruebas: la red real aún no está conectada." },
            { "MENU_QUIT", "Salir al escritorio" },
            { "MENU_QUIT_CONFIRM", "¿Salir del juego? Se perderá el progreso sin guardar." },
            { "MENU_YES", "Sí, salir" },
            { "MENU_NO", "Cancelar" },
            { "MENU_HINT_NAV", "[Arriba/Abajo] Elegir   [Izq/Der] Ajustar   [Enter] Aceptar   [Esc] Volver/Cerrar" },
            { "MENU_HINT_VR", "Quest: stick izquierdo = navegar, gatillo índice = aceptar, grip = volver, Menú = cerrar" },

            // ---- Pestañas de opciones ----
            { "TAB_AUDIO", "Audio" },
            { "TAB_LANGUAGE", "Idioma" },
            { "TAB_MIC", "Micrófono" },
            { "TAB_SERVICES", "Servicios" },

            // ---- Audio ----
            { "VOL_MASTER", "Volumen general" },
            { "VOL_MUSIC", "Música" },
            { "VOL_PRESENTERS", "Locutoras (Mina y Kira)" },
            { "VOL_NYX", "Nyx" },
            { "VOL_ENV", "Entorno y efectos" },
            { "SUBS_ENABLED", "Subtítulos" },
            { "TOGGLE_ON", "SÍ" },
            { "TOGGLE_OFF", "NO" },

            // ---- Idioma ----
            { "LANG_LABEL", "Idioma del juego" },
            { "LANG_ENGLISH", "English" },
            { "LANG_SPANISH", "Español" },
            { "LANG_NOTE", "Se aplica al instante: voces, subtítulos y textos de la interfaz." },

            // ---- Micrófono ----
            { "MIC_ENABLED", "Micrófono activado" },
            { "MIC_VOICECHAT", "Hablar con otros jugadores (online)" },
            { "MIC_NYX_CMD", "Hablar con Nyx / pedirle cosas" },
            { "MIC_DEVICE", "Dispositivo de entrada" },
            { "MIC_DEFAULT_DEVICE", "Predeterminado del sistema" },
            { "MIC_NO_DEVICES", "No se detecta ningún micrófono" },
            { "MIC_GAIN", "Ganancia de entrada" },
            { "MIC_MODE", "Activación" },
            { "MIC_MODE_PTT", "Pulsar para hablar" },
            { "MIC_MODE_OPEN", "Micrófono abierto" },
            { "MIC_PTT_KEY", "Tecla de pulsar para hablar" },
            { "MIC_PTT_REBIND", "Cambiar tecla" },
            { "MIC_PTT_PRESS", "Pulsa una tecla... (Esc cancela)" },
            { "MIC_PTT_RESERVED", "Esa tecla está reservada por el juego." },
            { "MIC_REFRESH", "Refrescar dispositivos" },

            // ---- Servicios ----
            { "SVC_TTS_URL", "URL del servidor TTS local" },
            { "SVC_TTS_APPLY", "Aplicar URL" },
            { "SVC_TTS_APPLIED", "URL guardada." },
            { "SVC_TTS_TEST", "Probar voz" },
            { "SVC_TTS_TEST_TEXT", "Nyx en línea. Comprobación del canal de voz completada." },
            { "SVC_TTS_NO_DIRECTOR", "El sistema de voz aún no está disponible." },
            { "SVC_TTS_SPEAKING", "Hablando..." },

            // ---- Subtítulos / HUD ----
            { "SPEAKER_KIRA", "Kira" },
            { "SPEAKER_MINA", "Mina" },
            { "SPEAKER_NYX", "Nyx" },
            { "HUD_RADIO_HEADER", "📻 RADIO CHIPSET 107.7 FM // EN EL AIRE: MINA Y KIRA" },
            { "HUD_STATUS_LINE", "ARMA: [{0}] | ESCUDO: {1}% | MODO: {2}" },
            { "HUD_MODE_VR", "VR INMERSIVO" },
            { "HUD_MODE_AR", "AR SALON" },
            { "HUD_AI_LINE", "IA: BMAX LOCAL + JENY-CODER (192.168.0.29) [0,00 €]" },
            { "HUD_BOSS", "⚠️ BOSS: QUANTUM CENTURION ({0}/{1} HP){2}" },
            { "HUD_CLIMAX", " [¡CLIMAX <=35%!]" },
            { "HUD_PHASE", "⚡ FASE: {0} (Enemigos: {1})" },
            { "HUD_PHASE_HINT", "Dispara a los Cofres (+) o pulsa [E] -> Brecha -> Oleada 1 -> 3s -> Oleada 2 -> Boss" },
            { "HUD_CONTROLS_1", "CONTROLES: [Clic Izq / Espacio] Disparar al Cursor | [E] Abrir Cofres | [V] Cambiar AR <-> Mundo VR" },
            { "HUD_CONTROLS_2", "[WASD] Mover | [Q/R o Clic Der] Mirar | [Tab] Cambiar Arma | [Shift] Escudo | [B] Saltar a Boss | [Esc] Pausa" },
            { "HUD_SESSION_OFFLINE", "OFFLINE" },
            { "HUD_SESSION_ONLINE", "ONLINE" },
            { "HUD_PAUSED", "PAUSA" },
            { "HUD_PAUSED_INPUT", "ENTRADA BLOQUEADA" }
        };

        // Bootstrap: garantiza que exista un LocalizationManager y que el idioma guardado se aplique al arrancar.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            try
            {
                GameSettings.EnsureLoaded();
                if (Instance == null)
                {
                    var existing = FindAnyObjectByType<LocalizationManager>();
                    if (existing == null)
                    {
                        var go = new GameObject("LocalizationManager (auto)");
                        existing = go.AddComponent<LocalizationManager>();
                    }
                }
                if (Instance != null)
                {
                    Instance.SetLanguage(GameSettings.Language);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LocalizationManager] Error en bootstrap: " + ex.Message);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // El idioma guardado manda sobre el valor serializado en la escena.
            GameSettings.EnsureLoaded();
            _currentLanguage = GameSettings.Language;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Devuelve el texto localizado según GameSettings.Language (fuente de verdad).
        /// Funciona sin instancia en escena. Si falta la clave devuelve el fallback o la propia clave.
        /// </summary>
        public static string Get(string key, string fallback = "")
        {
            GameSettings.EnsureLoaded();
            var dict = GameSettings.Language == Language.Spanish ? DictEs : DictEn;
            if (key != null && dict.TryGetValue(key, out var val))
            {
                return val;
            }
            return string.IsNullOrEmpty(fallback) ? (key ?? string.Empty) : fallback;
        }

        /// <summary>Texto localizado con string.Format ({0}, {1}...). Protegido contra formatos rotos.</summary>
        public static string Format(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch (FormatException)
            {
                return template;
            }
        }

        public void SetLanguage(Language newLanguage)
        {
            if (_currentLanguage != newLanguage)
            {
                _currentLanguage = newLanguage;
                OnLanguageChanged?.Invoke(_currentLanguage);
            }
        }

        public void ToggleLanguage()
        {
            GameSettings.SetLanguage(GameSettings.Language == Language.English ? Language.Spanish : Language.English);
        }
    }
}
