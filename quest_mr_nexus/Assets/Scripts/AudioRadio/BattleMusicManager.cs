using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using ProjectNyx.Core;

namespace ProjectNyx.AudioRadio
{
    public enum BattleTier
    {
        Peaceful,       // Emisión continua 24/7 de Radio Chipset 107.7 FM (Saqueo de cofres y post-victoria)
        NormalBattle,   // Esbirros o grietas menores de código (Oleadas 1 y 2 de Cyber-Spiders)
        BossLevel,      // Jefe de sector / Quantum Centurion (Brecha x2)
        FinalBoss       // Clímax energizante final del evento (Boss <= 35% de salud)
    }

    /// <summary>
    /// [ES] Gestor de Audio Dinámico 24/7 y Locución en Vivo para Radio Chipset.
    /// Administra la música de combate, audio ducking suave y transición de pistas.
    /// ---
    /// [EN] Dynamic 24/7 Audio & Live Radio Broadcast Manager for Radio Chipset.
    /// Manages adaptive combat music transitions, smooth presenter audio ducking, and track crossfading.
    /// </summary>
    public class BattleMusicManager : MonoBehaviour
    {
        [Header("Canales de Audio")]
        [SerializeField] private AudioSource channelA;
        [SerializeField] private AudioSource channelB;
        [SerializeField] private AudioSource voiceBroadcastSource;

        [Header("Playlists de Emisora 24/7 y Combate")]
        [SerializeField] private AudioClip[] peacefulRadioTracks;
        [SerializeField] private AudioClip[] normalBattleTracks;
        [SerializeField] private AudioClip[] bossTracks;
        [SerializeField] private AudioClip[] finalBossTracks;

        [Header("Efecto de Alerta de Boss")]
        [SerializeField] private AudioClip bossAlertSiren;

        [Header("Parámetros de Transición y Ducking")]
        [SerializeField] private float crossfadeDuration = 1.6f;
        [Range(0.0f, 1.0f)] [SerializeField] private float maxMusicVolume = 0.78f;
        [Range(0.05f, 0.6f)] [SerializeField] private float voiceDuckMusicVolume = 0.22f;

        [Header("Modo Streaming Exclusivo (Silenciar Originales)")]
        [Tooltip("[ES] Si esta activo, desactiva la musica original interna para que suene unicamente la emision de streaming de Radio Chipset.")]
        [SerializeField] private bool disableOriginalMusicForStreaming = true;

        [Header("Estado Actual")]
        [SerializeField] private BattleTier currentTier = BattleTier.Peaceful;

        private AudioSource activeSource;
        private AudioSource fadingSource;
        private Coroutine activeCrossfadeCoroutine;
        private Coroutine activeVoiceDuckCoroutine;
        private float currentTargetMusicVolume;

        private static BattleMusicManager instance;
        public static BattleMusicManager Instance => instance;
        public BattleTier CurrentTier => currentTier;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (channelA == null) channelA = gameObject.AddComponent<AudioSource>();
            if (channelB == null) channelB = gameObject.AddComponent<AudioSource>();
            if (voiceBroadcastSource == null) voiceBroadcastSource = gameObject.AddComponent<AudioSource>();

            ConfigureMusicChannel(channelA);
            ConfigureMusicChannel(channelB);
            ConfigureVoiceChannel(voiceBroadcastSource);

            activeSource = channelA;
            fadingSource = channelB;
            currentTargetMusicVolume = maxMusicVolume;
            if (disableOriginalMusicForStreaming)
            {
                StopOriginalMusic();
            }
        }

        private void Start()
        {
            if (disableOriginalMusicForStreaming)
            {
                StopOriginalMusic();
            }
            else
            {
                EnsureRadio247Playing();
            }
        }

        private void Update()
        {
            // [ES] Si el modo streaming exclusivo esta activo, silenciar y detener musica original
            // ---
            // [EN] If exclusive streaming mode is active, keep original music silenced and stopped
            if (disableOriginalMusicForStreaming)
            {
                if (activeSource != null && activeSource.isPlaying)
                {
                    activeSource.Stop();
                    activeSource.volume = 0f;
                }
                if (fadingSource != null && fadingSource.isPlaying)
                {
                    fadingSource.Stop();
                    fadingSource.volume = 0f;
                }
                return;
            }

            // Ajustar volumen master si el controlador de pausa existe
            float masterMult = PauseMenuController.Instance != null ? PauseMenuController.Instance.MasterVolume : 1.0f;
            float musicMult = PauseMenuController.Instance != null ? PauseMenuController.Instance.MusicVolume : 1.0f;

            if (activeSource != null && activeCrossfadeCoroutine == null)
            {
                activeSource.volume = currentTargetMusicVolume * masterMult * musicMult;
            }

            // Garantía 24/7: si se detiene, relanzar automáticamente
            if (activeSource != null && !activeSource.isPlaying && activeCrossfadeCoroutine == null)
            {
                AudioClip clip = SelectClipForTier(currentTier);
                if (clip != null)
                {
                    activeSource.clip = clip;
                    activeSource.volume = currentTargetMusicVolume * masterMult * musicMult;
                    activeSource.Play();
                }
            }
        }

        private void ConfigureMusicChannel(AudioSource source)
        {
            source.loop = true;
            source.playOnAwake = false;
            source.volume = 0f;
            source.spatialBlend = 0f;
        }

        private void ConfigureVoiceChannel(AudioSource source)
        {
            source.loop = false;
            source.playOnAwake = false;
            source.volume = 1.0f;
            source.spatialBlend = 0f;
            source.priority = 16;
        }

        public void EnsureRadio247Playing()
        {
            if (disableOriginalMusicForStreaming) return;
            if (activeSource != null && activeSource.isPlaying) return;

            AudioClip startClip = SelectClipForTier(currentTier);
            if (startClip != null && activeSource != null)
            {
                activeSource.clip = startClip;
                activeSource.volume = currentTargetMusicVolume;
                activeSource.loop = true;
                activeSource.Play();
                Debug.Log($"[BattleMusicManager] Radio Chipset 107.7 FM 24/7 en el aire: {startClip.name}");
            }
        }

        public void SetBattleTier(BattleTier newTier)
        {
            if (currentTier == newTier && activeSource != null && activeSource.isPlaying) return;

            Debug.Log($"[BattleMusicManager] Transición musical de {currentTier} a {newTier}");
            currentTier = newTier;

            // [ES] Notificar a la emisora de radio autónoma 107.7 FM para reacciones en vivo de Kira y Mina
            // ---
            // [EN] Notify standalone 107.7 FM radio station for live Kira & Mina presenter reactions
            if (StandaloneRadioStationChipset.Instance != null)
            {
                if (newTier == BattleTier.NormalBattle) StandaloneRadioStationChipset.Instance.NotifyCombatWaveStarted(1);
                else if (newTier == BattleTier.BossLevel) StandaloneRadioStationChipset.Instance.NotifyBossEncounter();
                else if (newTier == BattleTier.FinalBoss) StandaloneRadioStationChipset.Instance.NotifyClimaxEnergizer();
                else if (newTier == BattleTier.Peaceful) StandaloneRadioStationChipset.Instance.NotifyVictory();
            }

            // [ES] Si la emisora autónoma o streaming están activos, no arrancar música local duplicada
            // ---
            // [EN] If standalone radio or streaming are active, do not start duplicate local music
            if (disableOriginalMusicForStreaming) return;

            AudioClip nextClip = SelectClipForTier(newTier);
            if (nextClip == null) return;

            if ((newTier == BattleTier.BossLevel || newTier == BattleTier.FinalBoss) && bossAlertSiren != null)
            {
                AudioSource.PlayClipAtPoint(bossAlertSiren, Camera.main != null ? Camera.main.transform.position : transform.position);
            }

            StartCrossfade(nextClip);
        }

        public void PlayVoiceBroadcast(AudioClip voiceClip, float customDuckVolume = -1f)
        {
            if (voiceClip == null) return;

            float masterMult = PauseMenuController.Instance != null ? PauseMenuController.Instance.MasterVolume : 1.0f;
            float presMult = PauseMenuController.Instance != null ? PauseMenuController.Instance.PresentersVolume : 1.0f;

            if (voiceBroadcastSource == null)
            {
                voiceBroadcastSource = gameObject.AddComponent<AudioSource>();
                ConfigureVoiceChannel(voiceBroadcastSource);
            }

            voiceBroadcastSource.Stop();
            voiceBroadcastSource.clip = voiceClip;
            voiceBroadcastSource.volume = masterMult * presMult;
            voiceBroadcastSource.Play();

            float duckVol = customDuckVolume > 0f ? customDuckVolume : voiceDuckMusicVolume;
            if (activeVoiceDuckCoroutine != null) StopCoroutine(activeVoiceDuckCoroutine);
            activeVoiceDuckCoroutine = StartCoroutine(RoutineDuckMusicForVoice(voiceClip.length, duckVol));

            Debug.Log($"[Radio Chipset Voz] Emitiendo locución: {voiceClip.name} ({voiceClip.length:F1}s)");
        }

        public void QueueVoiceBroadcast(AudioClip voiceClip, float delaySeconds)
        {
            if (voiceClip == null) return;
            StartCoroutine(RoutineDelayedVoiceBroadcast(voiceClip, delaySeconds));
        }

        private IEnumerator RoutineDelayedVoiceBroadcast(AudioClip voiceClip, float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0.1f, delaySeconds));
            float safetyWait = 0f;
            while (voiceBroadcastSource != null && voiceBroadcastSource.isPlaying && safetyWait < 8.0f)
            {
                safetyWait += 0.15f;
                yield return new WaitForSeconds(0.15f);
            }
            PlayVoiceBroadcast(voiceClip);
        }

        private IEnumerator RoutineDuckMusicForVoice(float voiceDuration, float duckedVolume)
        {
            currentTargetMusicVolume = duckedVolume;
            if (activeSource != null && activeCrossfadeCoroutine == null)
            {
                activeSource.volume = duckedVolume;
            }

            yield return new WaitForSeconds(Mathf.Max(0.5f, voiceDuration + 0.15f));

            float elapsed = 0f;
            float restoreDuration = 0.6f;
            float startVol = activeSource != null ? activeSource.volume : duckedVolume;
            currentTargetMusicVolume = maxMusicVolume;

            while (elapsed < restoreDuration)
            {
                elapsed += Time.deltaTime;
                if (activeSource != null && activeCrossfadeCoroutine == null)
                {
                    activeSource.volume = Mathf.Lerp(startVol, maxMusicVolume, elapsed / restoreDuration);
                }
                yield return null;
            }

            if (activeSource != null && activeCrossfadeCoroutine == null)
            {
                activeSource.volume = maxMusicVolume;
            }
            activeVoiceDuckCoroutine = null;
        }

        private AudioClip SelectClipForTier(BattleTier tier)
        {
            switch (tier)
            {
                case BattleTier.Peaceful:
                    AudioClip peaceful = GetRandomClip(peacefulRadioTracks);
                    if (peaceful != null) return peaceful;
                    return GetRandomClip(normalBattleTracks);

                case BattleTier.NormalBattle:
                    return GetRandomClip(normalBattleTracks);

                case BattleTier.BossLevel:
                    AudioClip boss = GetRandomClip(bossTracks);
                    if (boss != null) return boss;
                    return GetRandomClip(normalBattleTracks);

                case BattleTier.FinalBoss:
                    AudioClip finalClip = GetRandomClip(finalBossTracks);
                    if (finalClip != null) return finalClip;
                    return GetRandomClip(bossTracks);

                default:
                    return GetRandomClip(peacefulRadioTracks);
            }
        }

        private AudioClip GetRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0) return null;
            List<AudioClip> validClips = new List<AudioClip>();
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null) validClips.Add(clips[i]);
            }
            if (validClips.Count == 0) return null;
            return validClips[UnityEngine.Random.Range(0, validClips.Count)];
        }

        private void StartCrossfade(AudioClip newClip)
        {
            if (disableOriginalMusicForStreaming) return;
            if (activeCrossfadeCoroutine != null) StopCoroutine(activeCrossfadeCoroutine);

            AudioSource temp = activeSource;
            activeSource = fadingSource;
            fadingSource = temp;

            activeSource.clip = newClip;
            activeSource.loop = true;
            activeSource.Play();

            activeCrossfadeCoroutine = StartCoroutine(RoutineCrossfade());
        }

        private IEnumerator RoutineCrossfade()
        {
            float elapsed = 0f;
            float startFadeOutVol = fadingSource != null ? fadingSource.volume : 0f;
            float startFadeInVol = activeSource != null ? activeSource.volume : 0f;

            while (elapsed < crossfadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / crossfadeDuration;

                if (fadingSource != null) fadingSource.volume = Mathf.Lerp(startFadeOutVol, 0f, t);
                if (activeSource != null) activeSource.volume = Mathf.Lerp(startFadeInVol, currentTargetMusicVolume, t);

                yield return null;
            }

            if (fadingSource != null)
            {
                fadingSource.volume = 0f;
                fadingSource.Stop();
            }
            if (activeSource != null) activeSource.volume = currentTargetMusicVolume;

            activeCrossfadeCoroutine = null;
        }

        public void FadeOutMusic()
        {
            SetBattleTier(BattleTier.Peaceful);
        }
    
        public bool DisableOriginalMusicForStreaming
        {
            get => disableOriginalMusicForStreaming;
            set
            {
                disableOriginalMusicForStreaming = value;
                if (disableOriginalMusicForStreaming)
                {
                    StopOriginalMusic();
                }
            }
        }

        public void StopOriginalMusic()
        {
            if (activeSource != null)
            {
                activeSource.Stop();
                activeSource.volume = 0f;
            }
            if (fadingSource != null)
            {
                fadingSource.Stop();
                fadingSource.volume = 0f;
            }
            if (channelA != null)
            {
                channelA.Stop();
                channelA.volume = 0f;
            }
            if (channelB != null)
            {
                channelB.Stop();
                channelB.volume = 0f;
            }
            if (voiceBroadcastSource != null)
            {
                voiceBroadcastSource.Stop();
                voiceBroadcastSource.volume = 0f;
            }
            Debug.Log("[BattleMusicManager] Musica original silenciada. Modo streaming exclusivo activo.");
        }

    }
}
