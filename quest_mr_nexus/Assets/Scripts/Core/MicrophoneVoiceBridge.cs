/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Puente de Voz y Micrófono Abierto para Meta Quest 3S.
 *      - Solicita permisos de audio en tiempo de ejecución (RECORD_AUDIO).
 *      - Micro abierto con VAD (Voice Activity Detection) continuo en tiempo real.
 *      - Conversación bidireccional con Nyx (IA compañera): Nyx escucha a Raquel,
 *        entra en pose atenta y responde con voz sintetizada, lip-sync y eventos a BMAX.
 *      - Preparado para chat multijugador con canal de audio continuo.
 * ---
 * [EN] Open Microphone & Voice Interaction Bridge for Meta Quest 3S.
 *      - Requests runtime RECORD_AUDIO permissions on Meta Horizon OS / Android.
 *      - Continuous real-time VAD (Voice Activity Detection) on open microphone stream.
 *      - Bidirectional dialogue with Nyx (AI Companion): Nyx listens to Raquel,
 *        switches to attentive stance, and responds with synthesized voice, lip-sync, and BMAX events.
 *      - Multiplayer-ready continuous audio buffer streaming.
 * =============================================================================
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectNyx.Companion;
using ProjectNyx.AudioRadio;
using ProjectNyx.NetworkBridge;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Captura de micro abierto de Meta Quest 3S e interacción conversacional con Nyx.
    /// [EN] Meta Quest 3S open mic streaming and conversational voice interaction with Nyx.
    /// </summary>
    public class MicrophoneVoiceBridge : MonoBehaviour
    {
        public static MicrophoneVoiceBridge Instance { get; private set; }

        [Header("Configuración de Micro y VAD")]
        [Tooltip("[ES] Iniciar captura automáticamente al otorgar permisos / [EN] Auto-start mic on permission grant")]
        [SerializeField] private bool autoStartRecording = true;
        [SerializeField] private int sampleRate = 16000;
        [Tooltip("[ES] Umbral RMS de sensibilidad vocal / [EN] Voice activity RMS threshold")]
        [SerializeField] private float voiceThreshold = 0.018f;
        [Tooltip("[ES] Segundos de silencio para dar por concluida la frase / [EN] Silence duration before sentence end")]
        [SerializeField] private float silenceDurationRequired = 0.75f;

        [Header("Feedback y Respuestas Vocales de Nyx")]
        [SerializeField] private AudioClip[] nyxVoiceResponses;
        [SerializeField] private AudioSource localMicAudioSource;

        private AudioClip micClip;
        private string micDevice;
        private bool isRecording;
        private bool isPlayerSpeaking;
        private float speechDuration;
        private float silenceTimer;
        private float[] sampleBuffer = new float[128];
        private int responseIndex = 0;

        private NyxBillboardAnimator nyxAnimator;
        private NyxFloatingCompanion nyxCompanion;

        public bool IsRecording => isRecording;
        public bool IsPlayerSpeaking => isPlayerSpeaking;
        public float CurrentRmsLevel { get; private set; }
        public string ActiveMicDevice => micDevice;

        public static event Action<float> OnPlayerVoiceLevel;
        public static event Action OnPlayerStartedSpeaking;
        public static event Action OnPlayerFinishedSpeaking;

        private static readonly string[] NyxTacticalReplies = new string[]
        {
            "¡Te escucho alto y claro, Raquel! Mantengo el flanco derecho protegido con el escudo.",
            "¡Recibido, Raquel! Sistemas tácticos en línea, sincronizando disparo de apoyo.",
            "¡Te sigo el ritmo, Raquel! ¡Recarga el Blaster que yo vigilo el perímetro!",
            "¡Afirmativo! Análisis espectral en curso: la matriz de la brecha sigue estable.",
            "¡Compañera, tus órdenes son ley! ¡Desplegando sobrecarga de plasma en el área!"
        };

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this);
                return;
            }
        }

        private void Start()
        {
            nyxAnimator = FindAnyObjectByType<NyxBillboardAnimator>();
            nyxCompanion = FindAnyObjectByType<NyxFloatingCompanion>();

            if (nyxVoiceResponses == null || nyxVoiceResponses.Length == 0)
            {
                AudioSource nyxAudio = nyxCompanion != null ? nyxCompanion.GetComponent<AudioSource>() : null;
                if (nyxAudio != null && nyxAudio.clip != null)
                {
                    nyxVoiceResponses = new AudioClip[] { nyxAudio.clip };
                }
            }

            StartCoroutine(RoutineInitializeMicrophone());
        }

        private IEnumerator RoutineInitializeMicrophone()
        {
            // 1. SOLICITAR PERMISOS DE GRABACIÓN DE AUDIO EN ANDROID / META QUEST 3S
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
            {
                Debug.Log("[MicrophoneVoiceBridge] Solicitando permiso android.permission.RECORD_AUDIO al visor...");
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
                yield return new WaitForSeconds(0.5f);
            }

            float waitTimer = 0f;
            while (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone) && waitTimer < 4.0f)
            {
                waitTimer += Time.deltaTime;
                yield return null;
            }
#endif
            yield return new WaitForSeconds(0.2f);

            // 2. ENUMERAR MICRÓFONOS DISPONIBLES
            if (Microphone.devices.Length > 0)
            {
                micDevice = Microphone.devices[0];
                Debug.Log($"[MicrophoneVoiceBridge] ¡Micrófono físico conectado en Quest 3S!: '{micDevice}'");
                if (autoStartRecording)
                {
                    StartMicCapture();
                }
            }
            else
            {
                Debug.LogWarning("[MicrophoneVoiceBridge] Advertencia: No se enumeró ningún micrófono. Reintentando en 2s...");
                yield return new WaitForSeconds(2.0f);
                if (Microphone.devices.Length > 0)
                {
                    micDevice = Microphone.devices[0];
                    StartMicCapture();
                }
            }
        }

        public void StartMicCapture()
        {
            if (string.IsNullOrEmpty(micDevice))
            {
                if (Microphone.devices.Length > 0) micDevice = Microphone.devices[0];
                else return;
            }

            if (isRecording) StopMicCapture();

            // Búfer circular de 10 segundos a sampleRate (16kHz calidad estándar de voz)
            micClip = Microphone.Start(micDevice, true, 10, sampleRate);
            isRecording = true;
            Debug.Log($"[MicrophoneVoiceBridge] ★ CANAL DE MICRO ABIERTO ACTIVO ({sampleRate} Hz) en Quest 3S.");
        }

        public void StopMicCapture()
        {
            if (!string.IsNullOrEmpty(micDevice) && Microphone.IsRecording(micDevice))
            {
                Microphone.End(micDevice);
                isRecording = false;
            }
        }

        private void Update()
        {
            if (!isRecording || micClip == null) return;

            // 3. VOICE ACTIVITY DETECTION (VAD) EN TIEMPO REAL
            int micPos = Microphone.GetPosition(micDevice);
            if (micPos < sampleBuffer.Length) return;

            // Extraer las últimas muestras para medir amplitud RMS
            micClip.GetData(sampleBuffer, Mathf.Max(0, micPos - sampleBuffer.Length));
            float sumSquares = 0f;
            for (int i = 0; i < sampleBuffer.Length; i++)
            {
                sumSquares += sampleBuffer[i] * sampleBuffer[i];
            }
            CurrentRmsLevel = Mathf.Sqrt(sumSquares / sampleBuffer.Length);
            OnPlayerVoiceLevel?.Invoke(CurrentRmsLevel);

            // Detección de voz de Raquel
            if (CurrentRmsLevel >= voiceThreshold)
            {
                silenceTimer = 0f;
                speechDuration += Time.deltaTime;

                if (!isPlayerSpeaking && speechDuration >= 0.12f)
                {
                    isPlayerSpeaking = true;
                    OnPlayerStartedSpeaking?.Invoke();
                    NotifyNyxPlayerIsSpeaking();
                }
            }
            else
            {
                if (isPlayerSpeaking)
                {
                    silenceTimer += Time.deltaTime;
                    if (silenceTimer >= silenceDurationRequired)
                    {
                        // Raquel terminó de hablar
                        isPlayerSpeaking = false;
                        silenceTimer = 0f;
                        float totalSpeech = speechDuration;
                        speechDuration = 0f;

                        if (totalSpeech >= 0.35f)
                        {
                            OnPlayerFinishedSpeaking?.Invoke();
                            TriggerNyxVocalResponse();
                        }
                    }
                }
                else
                {
                    speechDuration = 0f;
                }
            }
        }

        private void NotifyNyxPlayerIsSpeaking()
        {
            if (nyxCompanion == null) nyxCompanion = FindAnyObjectByType<NyxFloatingCompanion>();
            if (nyxAnimator == null) nyxAnimator = FindAnyObjectByType<NyxBillboardAnimator>();

            if (nyxCompanion != null)
            {
                nyxCompanion.SetMood(NyxMoodState.Alert);
            }

            Debug.Log("[MicrophoneVoiceBridge] 🎙️ Voz de Raquel detectada en el salón. Nyx prestando atención...");
        }

        private void TriggerNyxVocalResponse()
        {
            if (nyxCompanion == null) nyxCompanion = FindAnyObjectByType<NyxFloatingCompanion>();
            if (nyxAnimator == null) nyxAnimator = FindAnyObjectByType<NyxBillboardAnimator>();

            string replyText = NyxTacticalReplies[responseIndex % NyxTacticalReplies.Length];
            responseIndex++;

            if (nyxAnimator != null)
            {
                nyxAnimator.SpeakForDuration(3.5f, replyText);
            }
            else if (nyxCompanion != null)
            {
                nyxCompanion.SetMood(NyxMoodState.Talking);
            }

            // Reproducir clip de voz si existe
            if (nyxVoiceResponses != null && nyxVoiceResponses.Length > 0)
            {
                AudioClip clip = nyxVoiceResponses[UnityEngine.Random.Range(0, nyxVoiceResponses.Length)];
                if (clip != null && BattleMusicManager.Instance != null)
                {
                    BattleMusicManager.Instance.PlayVoiceBroadcast(clip, 0.18f);
                }
            }

            // Notificar evento al servidor local BMAX para que Kira y Mina comenten en radio
            if (BmaxBridgeClient.Instance != null)
            {
                BmaxBridgeClient.Instance.SendGameEvent("PLAYER_VOICE_COMMAND", $"Raquel interactuó por voz con Nyx: \"{replyText}\"");
            }

            Debug.Log($"[MicrophoneVoiceBridge] 💬 Nyx responde a Raquel: \"{replyText}\"");
        }

        private void OnDisable()
        {
            StopMicCapture();
        }
    }
}
