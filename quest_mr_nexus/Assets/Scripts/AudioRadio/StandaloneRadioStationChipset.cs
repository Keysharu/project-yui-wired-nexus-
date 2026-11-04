// =============================================================================
// Autora / Author: Raquel D. Torres (El taller de la gatita tech)
// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
// Meta Global AI Developer Hackathon 2026
// =============================================================================
// [ES] Emisora de Radio Autónoma 107.7 FM para Project NYX (Solo Audio Local).
//      Emisión musical ininterrumpida con locuciones intercaladas de Kira y Mina,
//      audio ducking suave, espacialización 3D y cero dependencias de red externa.
// ---
// [EN] Autonomous 107.7 FM Radio Station for Project NYX (Local Audio Only).
//      Continuous in-game music broadcasting with Kira & Mina radio presenter capsules,
//      smooth audio ducking, 3D spatialization, and zero external network dependencies.
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectNyx.AudioRadio
{
    /// <summary>
    /// [ES] Emisora de Radio 107.7 FM 100% Interna y Autónoma.
    /// Funciona de forma totalmente independiente del HomeLab o de YouTube,
    /// garantizando música cyberpunk continua y locuciones en directo de Kira y Mina.
    /// ---
    /// [EN] 100% Standalone and Autonomous 107.7 FM Radio Station.
    /// Runs completely independent from HomeLab or YouTube,
    /// guaranteeing continuous cyberpunk music and live presenter broadcasts from Kira & Mina.
    /// </summary>
    public class StandaloneRadioStationChipset : MonoBehaviour
    {
        private static StandaloneRadioStationChipset instance;
        public static StandaloneRadioStationChipset Instance => instance;

        [Header("Fuentes de Audio Internas")]
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource presenterAudioSource;

        [Header("Configuración de la Emisora")]
        [Tooltip("[ES] Nombre comercial de la emisora / [EN] Station callsign")]
        [SerializeField] private string stationName = "Radio Chipset 107.7 FM";

        [Tooltip("[ES] Volumen nominal de la música (0 a 1) / [EN] Normal music volume (0 to 1)")]
        [Range(0f, 1f)] [SerializeField] private float normalMusicVolume = 0.82f;

        [Tooltip("[ES] Volumen atenuado de la música durante locuciones (Ducking) / [EN] Ducked music volume during speech")]
        [Range(0.05f, 0.5f)] [SerializeField] private float duckedMusicVolume = 0.18f;

        [Tooltip("[ES] Velocidad de transición de volumen / [EN] Volume ducking transition speed")]
        [SerializeField] private float duckSpeed = 3.2f;

        [Tooltip("[ES] Intervalo en segundos entre intervenciones automáticas de locutoras / [EN] Interval in seconds between presenter capsules")]
        [SerializeField] private float presenterIntervalSeconds = 48.0f;

        [Tooltip("[ES] Espacialización 3D (0 = 2D global en auriculares, 1 = 3D posicional en transistor) / [EN] 3D spatial blend (0 = global 2D, 1 = 3D positional)")]
        [Range(0f, 1f)] [SerializeField] private float spatialBlend = 0.0f;

        [Header("Playlist Musical Cyberpunk / Synthwave (Suno)")]
        [SerializeField] private List<AudioClip> musicPlaylist = new List<AudioClip>();

        [Header("Cápsulas de Voz de Presentadoras (Kira & Mina)")]
        [SerializeField] private List<AudioClip> presenterClips = new List<AudioClip>();

        [Header("Locuciones Especiales de Combate")]
        [SerializeField] private AudioClip welcomeIntroClip;
        [SerializeField] private AudioClip bossAlertClip;
        [SerializeField] private AudioClip climaxClip;
        [SerializeField] private AudioClip victoryClip;

        private int currentTrackIndex = 0;
        private float targetMusicVolume;
        private bool isPresenterSpeaking = false;
        private Coroutine presenterLoopCoroutine;
        private Coroutine duckingCoroutine;

        public bool IsPresenterSpeaking => isPresenterSpeaking;
        public string StationName => stationName;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                transform.SetParent(null, true);
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Inicializar y configurar canales de audio dedicados
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
            if (presenterAudioSource == null)
            {
                presenterAudioSource = gameObject.AddComponent<AudioSource>();
            }

            // Garantizar la presencia del Corazón de IA de Kira y Mina (emisión puramente sonora)
            if (GetComponent<KiraMinaRadioAI>() == null)
            {
                gameObject.AddComponent<KiraMinaRadioAI>();
            }

            ConfigureAudioSource(musicAudioSource, isLoop: false, priority: 64);
            ConfigureAudioSource(presenterAudioSource, isLoop: false, priority: 16);

            targetMusicVolume = normalMusicVolume;
            musicAudioSource.volume = normalMusicVolume;
        }

        private void Start()
        {
            // Auto-anclar sobre el transistor Radio_Chipset_3D si existe en el salón
            GameObject radioTransistor = GameObject.Find("Radio_Chipset_3D");
            if (radioTransistor != null)
            {
                transform.position = radioTransistor.transform.position;
            }

            // Silenciar BattleMusicManager para evitar colisiones
            if (BattleMusicManager.Instance != null)
            {
                BattleMusicManager.Instance.DisableOriginalMusicForStreaming = true;
                BattleMusicManager.Instance.StopOriginalMusic();
            }

            // Iniciar la programación continua 24/7
            StartStationBroadcast();
        }

        private void Update()
        {
            // Transición suave de Ducking (música baja cuando habla la locutora y sube al terminar)
            if (musicAudioSource != null)
            {
                musicAudioSource.volume = Mathf.MoveTowards(
                    musicAudioSource.volume,
                    targetMusicVolume,
                    Time.deltaTime * duckSpeed
                );

                // Si la canción actual terminó, pasar a la siguiente pista automáticamente
                if (!musicAudioSource.isPlaying && musicPlaylist.Count > 0)
                {
                    PlayNextMusicTrack();
                }
            }
        }

        /// <summary>
        /// [ES] Configura los parámetros 3D y de mezcla de cada canal de audio.
        /// [EN] Configures 3D parameters and mixing profile for each audio channel.
        /// </summary>
        private void ConfigureAudioSource(AudioSource source, bool isLoop, int priority)
        {
            source.playOnAwake = false;
            source.loop = isLoop;
            source.priority = priority;
            source.spatialBlend = spatialBlend;
            source.minDistance = 2.0f;
            source.maxDistance = 28.0f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.dopplerLevel = 0.2f;
        }

        /// <summary>
        /// [ES] Inicia la rotación musical y el bucle de locutoras en vivo.
        /// [EN] Starts music rotation and live presenter loop.
        /// </summary>
        public void StartStationBroadcast()
        {
            if (musicPlaylist.Count > 0)
            {
                PlayNextMusicTrack();
            }

            // Emitir bienvenida inicial de Kira si está disponible
            if (welcomeIntroClip != null)
            {
                PlayPresenterVoice(welcomeIntroClip);
            }

            if (presenterLoopCoroutine != null)
            {
                StopCoroutine(presenterLoopCoroutine);
            }
            presenterLoopCoroutine = StartCoroutine(RoutinePresenterLoop());
        }

        /// <summary>
        /// [ES] Reproduce la siguiente pista de música en rotación.
        /// [EN] Plays next music track in playlist rotation.
        /// </summary>
        public void PlayNextMusicTrack()
        {
            if (musicPlaylist == null || musicPlaylist.Count == 0) return;

            AudioClip clip = musicPlaylist[currentTrackIndex];
            currentTrackIndex = (currentTrackIndex + 1) % musicPlaylist.Count;

            if (clip != null && musicAudioSource != null)
            {
                musicAudioSource.clip = clip;
                musicAudioSource.Play();
                Debug.Log($"[{stationName}] 🎵 Sonando ahora: {clip.name}");
            }
        }

        /// <summary>
        /// [ES] Bucle periódico de animación y locuciones de Kira y Mina sobre la música.
        /// [EN] Periodic loop for Kira & Mina presenter radio capsules over music.
        /// </summary>
        private IEnumerator RoutinePresenterLoop()
        {
            WaitForSeconds wait = new WaitForSeconds(presenterIntervalSeconds);
            while (true)
            {
                yield return wait;

                if (!isPresenterSpeaking && presenterClips.Count > 0)
                {
                    AudioClip randomCapsule = presenterClips[UnityEngine.Random.Range(0, presenterClips.Count)];
                    if (randomCapsule != null)
                    {
                        PlayPresenterVoice(randomCapsule);
                    }
                }
            }
        }

        /// <summary>
        /// [ES] Emite una locución de Kira o Mina con audio ducking dinámico sobre la música.
        /// [EN] Broadcasts a Kira or Mina presenter line with dynamic audio ducking over music.
        /// </summary>
        public void PlayPresenterVoice(AudioClip voiceClip)
        {
            if (voiceClip == null || presenterAudioSource == null) return;

            if (duckingCoroutine != null)
            {
                StopCoroutine(duckingCoroutine);
            }
            duckingCoroutine = StartCoroutine(RoutinePlayVoiceWithDucking(voiceClip));
        }

        private IEnumerator RoutinePlayVoiceWithDucking(AudioClip clip)
        {
            isPresenterSpeaking = true;
            targetMusicVolume = duckedMusicVolume; // [ES] Bajar volumen de música suavemente

            presenterAudioSource.Stop();
            presenterAudioSource.clip = clip;
            presenterAudioSource.volume = 1.0f;
            presenterAudioSource.Play();

            Debug.Log($"[{stationName}] 🎙️ Presentadora al aire: {clip.name} ({clip.length:F1}s)");

            yield return new WaitForSeconds(clip.length + 0.35f);

            targetMusicVolume = normalMusicVolume; // [ES] Restaurar volumen de música
            isPresenterSpeaking = false;
        }

        /// <summary>
        /// [ES] Reproduce en antena la secuencia completa de intervenciones de Kira y Mina con ducking musical continuo.
        /// [EN] Broadcasts the entire dialogue turn sequence between Kira and Mina with continuous music ducking.
        /// </summary>
        public void PlayDialogueSequence(List<DialogueLine> dialogues, AudioClip[] kiraClips, AudioClip[] minaClips, AudioClip[] nyxClips)
        {
            if (dialogues == null || dialogues.Count == 0) return;
            if (presenterAudioSource == null) return;

            if (duckingCoroutine != null)
            {
                StopCoroutine(duckingCoroutine);
            }
            duckingCoroutine = StartCoroutine(RoutinePlayDialogueSequence(dialogues, kiraClips, minaClips, nyxClips));
        }

        private IEnumerator RoutinePlayDialogueSequence(List<DialogueLine> dialogues, AudioClip[] kiraClips, AudioClip[] minaClips, AudioClip[] nyxClips)
        {
            isPresenterSpeaking = true;
            targetMusicVolume = duckedMusicVolume; // [ES] Ducking a 18% para escuchar las voces con claridad

            int kIdx = 0;
            int mIdx = 0;
            int nIdx = 0;

            foreach (var line in dialogues)
            {
                AudioClip clipToPlay = null;
                string sp = (line.speaker ?? "KIRA").ToUpper().Trim();

                if (sp.Contains("KIRA") && kiraClips != null && kiraClips.Length > 0)
                {
                    clipToPlay = kiraClips[kIdx % kiraClips.Length];
                    kIdx++;
                }
                else if (sp.Contains("MINA") && minaClips != null && minaClips.Length > 0)
                {
                    clipToPlay = minaClips[mIdx % minaClips.Length];
                    mIdx++;
                }
                else if (sp.Contains("NYX") && nyxClips != null && nyxClips.Length > 0)
                {
                    clipToPlay = nyxClips[nIdx % nyxClips.Length];
                    nIdx++;
                }
                else if (presenterClips != null && presenterClips.Count > 0)
                {
                    clipToPlay = presenterClips[UnityEngine.Random.Range(0, presenterClips.Count)];
                }

                if (clipToPlay != null && presenterAudioSource != null)
                {
                    presenterAudioSource.Stop();
                    presenterAudioSource.clip = clipToPlay;
                    presenterAudioSource.volume = 1.0f;
                    presenterAudioSource.Play();
                    Debug.Log($"[{stationName}] 🎙️ Voz sintetizada [{sp}]: {clipToPlay.name} ({clipToPlay.length:F1}s)");
                    yield return new WaitForSeconds(clipToPlay.length + 0.30f);
                }
                else
                {
                    yield return new WaitForSeconds(2.0f);
                }
            }

            targetMusicVolume = normalMusicVolume; // [ES] Restaurar música al 82%
            isPresenterSpeaking = false;
        }

        // =========================================================================
        // Disparadores de Eventos de Combate (Kira & Mina reaccionando en directo)
        // Combat Event Triggers (Kira & Mina reacting live on air)
        // =========================================================================

        /// <summary>
        /// [ES] Llamado cuando empieza una oleada de arañas o combate.
        /// [EN] Called when a spider wave or combat encounter begins.
        /// </summary>
        public void NotifyCombatWaveStarted(int waveNumber)
        {
            Debug.Log($"[{stationName}] Alerta de combate en emisión: Oleada {waveNumber}");
            if (KiraMinaRadioAI.Instance != null && waveNumber > 0)
            {
                KiraMinaRadioAI.Instance.OnWaveStarted(waveNumber);
            }

            // Si la locutora no está ocupada, lanzar intervención táctica
            if (!isPresenterSpeaking && presenterClips.Count > 0)
            {
                AudioClip brief = presenterClips[UnityEngine.Random.Range(0, presenterClips.Count)];
                PlayPresenterVoice(brief);
            }
        }

        /// <summary>
        /// [ES] Llamado cuando aparece el Boss Quantum Centurion.
        /// [EN] Called when Quantum Centurion Boss breaches into the room.
        /// </summary>
        public void NotifyBossEncounter()
        {
            if (KiraMinaRadioAI.Instance != null)
            {
                KiraMinaRadioAI.Instance.OnBossAppeared();
            }

            if (bossAlertClip != null)
            {
                PlayPresenterVoice(bossAlertClip);
            }
        }

        /// <summary>
        /// [ES] Llamado cuando el Boss entra en fase crítica (<= 35% de vida).
        /// [EN] Called when Boss enters critical phase (<= 35% health).
        /// </summary>
        public void NotifyClimaxEnergizer()
        {
            if (climaxClip != null)
            {
                PlayPresenterVoice(climaxClip);
            }
        }

        /// <summary>
        /// [ES] Llamado cuando la jugadora derrota a las fuerzas invasoras.
        /// [EN] Called when player defeats invading forces.
        /// </summary>
        public void NotifyVictory()
        {
            if (KiraMinaRadioAI.Instance != null)
            {
                KiraMinaRadioAI.Instance.OnVictoryAchieved();
            }

            if (victoryClip != null)
            {
                PlayPresenterVoice(victoryClip);
            }
        }

        /// <summary>
        /// [ES] Configura la lista de música y voces desde el editor.
        /// [EN] Sets up music and voice playlists from editor setup.
        /// </summary>
        public void SetupPlaylists(List<AudioClip> music, List<AudioClip> presenters, AudioClip welcome, AudioClip bossAlert, AudioClip climax, AudioClip victory)
        {
            musicPlaylist = music ?? new List<AudioClip>();
            presenterClips = presenters ?? new List<AudioClip>();
            welcomeIntroClip = welcome;
            bossAlertClip = bossAlert;
            climaxClip = climax;
            victoryClip = victory;
        }
    }
}
