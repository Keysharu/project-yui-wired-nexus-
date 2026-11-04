using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using ProjectNyx.Core;
using QuestMrNexus.Core;

namespace ProjectNyx.World
{
    // =====================================================================================
    //  TEXTOS LOCALES EN/ES del módulo World.
    //  No tocamos LocalizationManager (otro agente lo amplía en paralelo): este diccionario
    //  consulta GameSettings.Language directamente.
    // =====================================================================================
    public static class WorldText
    {
        private static readonly Dictionary<string, string[]> Table = new Dictionary<string, string[]>
        {
            // clave                      EN                                                                   ES
            { "lobby",              new[] { "Central Lobby",                                                   "Vestibulo central" } },
            { "floor",              new[] { "Floor {0}",                                                       "Piso {0}" } },
            { "major_zone",         new[] { "Major Control World",                                             "Mundo de Control Mayor" } },
            { "control_zone",       new[] { "Control zone",                                                    "Zona de control" } },
            { "locked",             new[] { "LOCKED",                                                          "BLOQUEADO" } },
            { "here",               new[] { "<< YOU ARE HERE",                                                 "<< ESTAS AQUI" } },
            { "title",              new[] { "FLOOR TELEPORT TERMINAL",                                         "TERMINAL DE TELETRANSPORTE" } },
            { "location",           new[] { "Location: {0}",                                                   "Ubicacion: {0}" } },
            { "floors_header",      new[] { "DESTINATIONS (visited floors only)",                              "DESTINOS (solo pisos visitados)" } },
            { "friends_header",     new[] { "FRIENDS",                                                         "AMIGOS" } },
            { "no_friends",         new[] { "No friends connected.",                                           "Sin amigos conectados." } },
            { "friend_row",         new[] { "{0}  -  {1}",                                                     "{0}  -  {1}" } },
            { "btn_tp",             new[] { "TELEPORT to {0}",                                                 "TELETRANSPORTAR a {0}" } },
            { "btn_tp_cd",          new[] { "TELEPORT ({0} s)",                                                "TELETRANSPORTE ({0} s)" } },
            { "btn_tp_none",        new[] { "SELECT A DESTINATION",                                            "ELIGE UN DESTINO" } },
            { "btn_friend",         new[] { "TP",                                                              "TP" } },
            { "btn_close",          new[] { "CLOSE [F5]",                                                      "CERRAR [F5]" } },
            { "warn_not_safe",      new[] { "You are not in a safe zone. Teleport disabled.",                  "No estas en una zona segura. Teletransporte desactivado." } },
            { "warn_combat",        new[] { "Combat active (enemies or boss alive). Teleport disabled.",       "Combate activo (enemigos o jefe vivo). Teletransporte desactivado." } },
            { "warn_cooldown",      new[] { "Teleport recharging: {0} s.",                                     "Teletransporte recargando: {0} s." } },
            { "warn_busy",          new[] { "Teleport sequence in progress.",                                  "Secuencia de teletransporte en curso." } },
            { "msg_locked",         new[] { "Locked: you have not visited that floor yet.",                    "Bloqueado: aun no has visitado ese piso." } },
            { "msg_same_floor",     new[] { "You are already on that floor.",                                  "Ya estas en ese piso." } },
            { "msg_friend_here",    new[] { "You are already on {0}'s floor.",                                 "Ya estas en el piso de {0}." } },
            { "msg_invalid",        new[] { "Invalid destination.",                                            "Destino invalido." } },
            { "msg_arrived",        new[] { "Teleport complete: {0}.",                                         "Teletransporte completado: {0}." } },
            { "msg_loader_failed",  new[] { "Destination failed to load. Teleport cancelled.",                 "El destino no se pudo cargar. Teletransporte cancelado." } },
            { "banner_floor",       new[] { "{0} - Control zone (safe)",                                       "{0} - Zona de control (segura)" } },
            { "banner_lobby",       new[] { "{0} (safe)",                                                      "{0} (seguro)" } },
            { "dbg_hint",           new[] { "[DEBUG] F5 terminal | F6 next floor (Ctrl+F6 reset) | F7 safe zone | F8 Nyx encounter",
                                            "[DEBUG] F5 terminal | F6 siguiente piso (Ctrl+F6 reinicia) | F7 zona segura | F8 encuentro Nyx" } },
            { "dbg_state",          new[] { "Floor: {0} | Safe zone: {1} | Highest: {2}",                      "Piso: {0} | Zona segura: {1} | Mas alto: {2}" } },
        };

        /// <summary>Devuelve el texto en el idioma de GameSettings; si falta la clave devuelve la clave.</summary>
        public static string Get(string key, params object[] args)
        {
            try
            {
                GameSettings.EnsureLoaded();
                string[] pair;
                if (!Table.TryGetValue(key, out pair)) return key;
                int idx = GameSettings.Language == LocalizationManager.Language.English ? 0 : 1;
                string text = pair[idx];
                if (args != null && args.Length > 0) return string.Format(CultureInfo.InvariantCulture, text, args);
                return text;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[WorldText] Error resolviendo texto '" + key + "': " + ex.Message);
                return key;
            }
        }

        /// <summary>Nombre localizado de un destino: 0 = vestíbulo, 1..N = piso.</summary>
        public static string FloorName(int floor)
        {
            return floor == FloorProgressionService.LobbyFloor ? Get("lobby") : Get("floor", floor);
        }
    }

    // =====================================================================================
    //  BACKEND DE PROGRESO (remoto PENDIENTE DE DATOS: Nakama o Colyseus sin elegir)
    // =====================================================================================

    /// <summary>Datos de progreso de pisos que viajan entre cliente y backend.</summary>
    [Serializable]
    public class FloorSaveData
    {
        public int[] visitedFloors = new int[] { 1 };
        public int lastFloor = 0;
    }

    /// <summary>
    /// Contrato de persistencia de progreso de pisos. Implementación remota: PENDIENTE DE DATOS
    /// (depende del backend de cuenta/inventario que se elija). Las llamadas son síncronas y
    /// deben devolver false si fallan; el servicio ya tiene respaldo local.
    /// </summary>
    public interface IFloorBackend
    {
        bool TryLoad(out FloorSaveData data);
        bool TrySave(FloorSaveData data);
    }

    /// <summary>Backend por defecto: PlayerPrefs con clave versionada, checksum y copia de respaldo.</summary>
    public class LocalPlayerPrefsFloorBackend : IFloorBackend
    {
        public const string Key = "nyx.world.floors.v1";
        public const string BackupKey = "nyx.world.floors.v1.bak";
        private const string Version = "v1";

        public bool TryLoad(out FloorSaveData data)
        {
            data = null;
            try
            {
                if (TryParse(PlayerPrefs.GetString(Key, string.Empty), out data)) return true;
                // El principal falta o está corrupto: probamos la copia de respaldo.
                if (TryParse(PlayerPrefs.GetString(BackupKey, string.Empty), out data)) return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LocalPlayerPrefsFloorBackend] Error leyendo progreso: " + ex.Message);
            }
            data = null;
            return false;
        }

        public bool TrySave(FloorSaveData data)
        {
            if (data == null) return false;
            try
            {
                // Escritura segura: primero respaldamos el valor válido anterior, luego escribimos el nuevo.
                string previous = PlayerPrefs.GetString(Key, string.Empty);
                FloorSaveData check;
                if (!string.IsNullOrEmpty(previous) && TryParse(previous, out check))
                {
                    PlayerPrefs.SetString(BackupKey, previous);
                }
                PlayerPrefs.SetString(Key, Serialize(data));
                PlayerPrefs.Save();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LocalPlayerPrefsFloorBackend] Error guardando progreso: " + ex.Message);
                return false;
            }
        }

        private static string Serialize(FloorSaveData data)
        {
            var sb = new StringBuilder();
            int[] floors = data.visitedFloors ?? new int[0];
            for (int i = 0; i < floors.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(floors[i].ToString(CultureInfo.InvariantCulture));
            }
            string body = Version + "|" + data.lastFloor.ToString(CultureInfo.InvariantCulture) + "|" + sb;
            return body + "|" + Checksum(body).ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryParse(string raw, out FloorSaveData data)
        {
            data = null;
            if (string.IsNullOrEmpty(raw)) return false;

            string[] parts = raw.Split('|');
            if (parts.Length != 4 || parts[0] != Version) return false;

            string body = parts[0] + "|" + parts[1] + "|" + parts[2];
            int sum;
            if (!int.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out sum)) return false;
            if (sum != Checksum(body)) return false;

            int last;
            if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out last)) return false;

            var list = new List<int>();
            if (parts[2].Length > 0)
            {
                foreach (string s in parts[2].Split(','))
                {
                    int f;
                    if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out f)) list.Add(f);
                }
            }

            data = new FloorSaveData { visitedFloors = list.ToArray(), lastFloor = last };
            return true;
        }

        private static int Checksum(string s)
        {
            unchecked
            {
                int h = 17;
                for (int i = 0; i < s.Length; i++) h = h * 31 + s[i];
                return h;
            }
        }
    }

    // =====================================================================================
    //  SERVICIO DE PROGRESO DE PISOS
    // =====================================================================================

    /// <summary>
    /// [ES] Estado estático de progresión por pisos (estilo SAO). Piso 1 siempre desbloqueado.
    /// Piso 0 = vestíbulo VR central (no es un piso, nunca es "desbloqueable").
    /// Cada múltiplo de 5 es un Mundo de Control Mayor.
    /// ---
    /// [EN] Static floor progression state (SAO tower style). Floor 1 is always unlocked.
    /// Floor 0 = central VR lobby (hub zone, not a tower combat floor).
    /// Every multiple of 5 is a Major Control World safe checkpoint.
    /// </summary>
    public static class FloorProgressionService
    {
        public const int LobbyFloor = 0;
        public const int DefaultMaxFloors = 20;
        public const int MajorControlInterval = 5;

        private static readonly LocalPlayerPrefsFloorBackend LocalBackend = new LocalPlayerPrefsFloorBackend();
        private static readonly HashSet<int> Visited = new HashSet<int>();

        private static IFloorBackend remoteBackend; // null = solo local. PENDIENTE DE DATOS.
        private static int maxFloors = DefaultMaxFloors;
        private static bool loaded;
        private static int currentFloor = LobbyFloor;
        private static bool inSafeZone = true; // el vestíbulo es zona segura

        /// <summary>Se dispara la primera vez que se visita un piso (argumento: número de piso).</summary>
        public static event Action<int> FloorVisited;
        /// <summary>Se dispara cuando cambia el piso actual (0 = vestíbulo).</summary>
        public static event Action<int> CurrentFloorChanged;
        /// <summary>Se dispara cuando cambia el estado de zona segura.</summary>
        public static event Action<bool> SafeZoneChanged;

        public static int MaxFloors { get { return maxFloors; } }
        public static int CurrentFloor { get { EnsureLoaded(); return currentFloor; } }
        public static bool IsInSafeZone { get { EnsureLoaded(); return inSafeZone; } }

        /// <summary>Último piso guardado en la sesión anterior (informativo; no se usa para reposicionar).</summary>
        public static int LastSavedFloor { get; private set; }

        public static int HighestVisited
        {
            get
            {
                EnsureLoaded();
                int best = 1;
                foreach (int f in Visited) if (f > best) best = f;
                return best;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            // Necesario con "Enter Play Mode Options" (sin recarga de dominio).
            FloorVisited = null;
            CurrentFloorChanged = null;
            SafeZoneChanged = null;
            Visited.Clear();
            remoteBackend = null;
            maxFloors = DefaultMaxFloors;
            loaded = false;
            currentFloor = LobbyFloor;
            inSafeZone = true;
            LastSavedFloor = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureLoaded();
        }

        /// <summary>Configura el número de pisos (N). Mínimo 1.</summary>
        public static void Configure(int floorCount)
        {
            maxFloors = Mathf.Clamp(floorCount, 1, 999);
            EnsureLoaded();
        }

        /// <summary>Instala un backend remoto (PENDIENTE DE DATOS). null vuelve a modo solo local.</summary>
        public static void SetRemoteBackend(IFloorBackend backend)
        {
            remoteBackend = backend;
            if (backend == null) return;

            try
            {
                FloorSaveData remote;
                if (backend.TryLoad(out remote) && remote != null)
                {
                    EnsureLoaded();
                    bool changed = false;
                    // Fusión por unión: nunca perdemos pisos ya visitados en local.
                    foreach (int f in remote.visitedFloors ?? new int[0])
                    {
                        if (f >= 1 && f <= maxFloors && Visited.Add(f))
                        {
                            changed = true;
                            if (FloorVisited != null) FloorVisited(f);
                        }
                    }
                    if (changed) Save();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[FloorProgressionService] Backend remoto fallo al cargar: " + ex.Message);
            }
        }

        public static void EnsureLoaded()
        {
            if (loaded) return;
            loaded = true;

            Visited.Clear();
            Visited.Add(1); // el piso 1 siempre está desbloqueado

            try
            {
                FloorSaveData data;
                if (LocalBackend.TryLoad(out data) && data != null)
                {
                    foreach (int f in data.visitedFloors ?? new int[0])
                    {
                        if (f >= 1 && f <= maxFloors) Visited.Add(f);
                    }
                    LastSavedFloor = Mathf.Clamp(data.lastFloor, 0, maxFloors);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[FloorProgressionService] No se pudo cargar progreso, se usa solo el piso 1: " + ex.Message);
            }
        }

        public static bool IsMajorControlZone(int floor)
        {
            return floor > 0 && floor % MajorControlInterval == 0;
        }

        /// <summary>true si el piso (1..N) está desbloqueado. El vestíbulo (0) devuelve false: no es un piso.</summary>
        public static bool IsUnlocked(int floor)
        {
            EnsureLoaded();
            if (floor < 1 || floor > maxFloors) return false;
            return floor == 1 || Visited.Contains(floor);
        }

        /// <summary>Lista ordenada de pisos visitados.</summary>
        public static List<int> GetVisitedFloors()
        {
            EnsureLoaded();
            var list = new List<int>(Visited);
            list.Sort();
            return list;
        }

        /// <summary>
        /// Marca un piso como visitado (al entrar en su zona de control) y lo convierte en piso actual.
        /// Entrar en una zona de control implica zona segura.
        /// </summary>
        public static void MarkVisited(int floor)
        {
            EnsureLoaded();
            if (floor < 1 || floor > maxFloors)
            {
                Debug.LogWarning("[FloorProgressionService] MarkVisited fuera de rango: " + floor);
                return;
            }

            if (Visited.Add(floor))
            {
                Save();
                if (FloorVisited != null) FloorVisited(floor);
            }
            SetCurrentFloor(floor, true);
        }

        /// <summary>Fija el piso actual (0 = vestíbulo) y el estado de zona segura. Un piso nuevo cuenta como visitado.</summary>
        public static void SetCurrentFloor(int floor, bool safeZone)
        {
            EnsureLoaded();
            floor = Mathf.Clamp(floor, 0, maxFloors);

            if (floor >= 1 && !Visited.Contains(floor))
            {
                Visited.Add(floor);
                if (FloorVisited != null) FloorVisited(floor);
            }

            bool floorChanged = floor != currentFloor;
            currentFloor = floor;
            LastSavedFloor = floor;
            SetSafeZone(safeZone);
            Save();

            if (floorChanged && CurrentFloorChanged != null) CurrentFloorChanged(floor);
        }

        public static void SetSafeZone(bool safe)
        {
            EnsureLoaded();
            if (inSafeZone == safe) return;
            inSafeZone = safe;
            if (SafeZoneChanged != null) SafeZoneChanged(safe);
        }

        /// <summary>Borra el progreso (solo para pruebas). Deja únicamente el piso 1.</summary>
        public static void ResetProgress()
        {
            EnsureLoaded();
            Visited.Clear();
            Visited.Add(1);
            currentFloor = LobbyFloor;
            LastSavedFloor = 0;
            inSafeZone = true;
            Save();
            if (CurrentFloorChanged != null) CurrentFloorChanged(LobbyFloor);
            if (SafeZoneChanged != null) SafeZoneChanged(true);
        }

        private static void Save()
        {
            var data = new FloorSaveData
            {
                visitedFloors = GetVisitedFloors().ToArray(),
                lastFloor = currentFloor
            };

            LocalBackend.TrySave(data); // caché local siempre

            if (remoteBackend != null)
            {
                try
                {
                    remoteBackend.TrySave(data);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[FloorProgressionService] Backend remoto fallo al guardar: " + ex.Message);
                }
            }
        }
    }
}
