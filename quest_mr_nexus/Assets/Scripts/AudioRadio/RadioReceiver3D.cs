using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectNyx.AudioRadio
{
    /// <summary>
    /// [ES] Pieza 3 del Puzzle: Transistor Holográfico Radio Chipset 3D.
    /// Emite la música de Suno y las locuciones de Kira y Mina con sonido espacializado 3D
    /// y atenuación natural por distancia en el salón real.
    /// ---
    /// [EN] Puzzle Piece 3: 3D Holographic Transistor Radio Chipset.
    /// Broadcasts Suno music and AI presenter dialogue (Kira & Mina) with spatialized 3D audio
    /// and distance attenuation across the physical living room.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class RadioReceiver3D : MonoBehaviour
    {
        [Header("Fuentes de Audio")]
        [SerializeField] private AudioSource musicAudioSource;
        [SerializeField] private AudioSource voiceAudioSource;

        [Header("Modo Streaming Exclusivo (Silenciar Radio Interna)")]
        [Tooltip("[ES] Silencia el receptor de radio interno para que suene unicamente el streaming de Radio Chipset.")]
        [SerializeField] private bool disableInternalRadioForStreaming = true;

        [Header("Configuración de Ducking")]
        [Range(0.0f, 1.0f)] [SerializeField] private float normalMusicVolume = 0.8f;
        [Range(0.0f, 1.0f)] [SerializeField] private float duckedMusicVolume = 0.15f;
        [SerializeField] private float duckSpeed = 3.0f;

        [Header("Visualizador Holográfico")]
        [SerializeField] private Transform antennaLed;
        [SerializeField] private Color onAirColor = Color.cyan;
        [SerializeField] private Color talkColor = Color.magenta;

        private float targetMusicVolume;
        private bool isVoiceActive = false;

        private void Awake()
        {
            if (musicAudioSource == null) musicAudioSource = GetComponent<AudioSource>();
            
            // Configurar sonido 3D puro
            musicAudioSource.spatialBlend = 1.0f; // 1.0 = 3D completo
            musicAudioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            musicAudioSource.minDistance = 0.5f;
            musicAudioSource.maxDistance = 8.0f;
            targetMusicVolume = normalMusicVolume;
            if (disableInternalRadioForStreaming)
            {
                StopInternalRadio();
            }
        }

        private void Update()
        {
            // Transición suave de volumen de música (Ducking)
            musicAudioSource.volume = Mathf.MoveTowards(
                musicAudioSource.volume, 
                targetMusicVolume, 
                Time.deltaTime * duckSpeed
            );
        }

        /// <summary>
        /// Reproduce una locución de Kira o Mina bajando automáticamente la música de fondo.
        /// </summary>
        public void PlayVoiceCapsule(AudioClip voiceClip)
        {
            if (disableInternalRadioForStreaming) return;
            if (voiceClip == null) return;
            StartCoroutine(RoutinePlayVoiceWithDucking(voiceClip));
        }

        private IEnumerator RoutinePlayVoiceWithDucking(AudioClip clip)
        {
            isVoiceActive = true;
            targetMusicVolume = duckedMusicVolume; // Baja la música

            if (voiceAudioSource != null)
            {
                voiceAudioSource.clip = clip;
                voiceAudioSource.Play();
                yield return new WaitForSeconds(clip.length);
            }

            targetMusicVolume = normalMusicVolume; // Sube la música
            isVoiceActive = false;
        }

        /// <summary>
        /// Descarga y reproduce un clip generado dinámicamente desde el servidor BMAX.
        /// </summary>
        public IEnumerator DownloadAndPlayVoice(string audioUrl)
        {
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(audioUrl, AudioType.MPEG))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    PlayVoiceCapsule(clip);
                }
                else
                {
                    Debug.LogWarning($"[RadioReceiver3D] Error al descargar audio de radio: {www.error}");
                }
            }
        }
    
        public bool DisableInternalRadioForStreaming
        {
            get => disableInternalRadioForStreaming;
            set
            {
                disableInternalRadioForStreaming = value;
                if (disableInternalRadioForStreaming)
                {
                    StopInternalRadio();
                }
            }
        }

        public void StopInternalRadio()
        {
            if (musicAudioSource != null)
            {
                musicAudioSource.Stop();
                musicAudioSource.volume = 0f;
            }
            if (voiceAudioSource != null)
            {
                voiceAudioSource.Stop();
                voiceAudioSource.volume = 0f;
            }
            Debug.Log("[RadioReceiver3D] Radio interna silenciada. Modo streaming exclusivo activo.");
        }

    }
}
