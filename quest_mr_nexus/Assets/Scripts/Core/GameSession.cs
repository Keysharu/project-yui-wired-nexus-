using UnityEngine;

namespace ProjectNyx.Core
{
    public enum SessionMode
    {
        Offline,
        Online
    }

    /// <summary>
    /// [ES] Estado global de la sesión de juego (contrato compartido entre módulos).
    /// - Offline: el menú de pausa congela TODO el juego (Time.timeScale = 0).
    /// - Online: el mundo sigue corriendo; el menú solo bloquea TUS movimientos y acciones
    ///   (los demás jugadores y enemigos no se pausan).
    /// Cualquier script que lea entrada del jugador (disparar, escudo, moverse, cambiar de arma)
    /// debe comprobar GameSession.PlayerInputBlocked antes de actuar.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Global game session state (shared architectural contract across modules).
    /// - Offline: the pause menu freezes the ENTIRE game (Time.timeScale = 0).
    /// - Online: the world keeps simulating; the menu only blocks LOCAL player inputs and combat
    ///   (other players and enemies do not pause).
    /// Any script that handles player input (firing, shielding, moving, weapon switching)
    /// must verify GameSession.PlayerInputBlocked before acting.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public static class GameSession
    {
        public static SessionMode Mode { get; set; } = SessionMode.Offline;

        public static bool IsOnline => Mode == SessionMode.Online;

        /// <summary>
        /// [ES] Lo activa/desactiva únicamente PauseMenuController.
        /// [EN] Toggled exclusively by PauseMenuController.
        /// </summary>
        public static bool IsPauseMenuOpen { get; set; }

        /// <summary>
        /// [ES] true mientras el menú está abierto: el jugador no mueve, no dispara, no cambia de arma.
        /// [EN] true while the menu is open: blocks player locomotion, shooting, and weapon swaps.
        /// </summary>
        public static bool PlayerInputBlocked => IsPauseMenuOpen;
    }
}
