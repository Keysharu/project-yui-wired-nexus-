/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Director Cinemático de Flujo de Juego para Meta Quest 3S.
 *      Inicia de forma predeterminada en AR Passthrough (Salón físico) con
 *      la Brecha Dimensional y Nyx escoltando a la jugadora.
 * ---
 * [EN] Cinematic Game Flow Director for Meta Quest 3S.
 *      Defaults directly into AR Passthrough (Physical room) with the
 *      Dimensional Rift and Nyx escorting the player.
 * =============================================================================
 */

using System;
using System.Collections;
using UnityEngine;
using ProjectNyx.Core;
using ProjectNyx.AudioRadio;
using ProjectNyx.Companion;

namespace ProjectNyx.Gameplay
{
    public enum GameFlowState
    {
        VRLobby_StreetExploration, // Estado 1: Bulevar VR, Megatorre, Tiendas, Radio Chipset 107.7 FM emitiendo noticias
        BreachAlarm_Countdown,     // Estado 2: Sirena, la Megatorre parpadea en alerta, se abre la Brecha en la calle
        ARSaloon_IncursionActive,  // Estado 3: La realidad virtual se disuelve, entra Passthrough AR al salón real con arañas y boss
        EncounterVictory_ReturnVR  // Estado 4: Victoria, opción de teletransporte de vuelta a la Zona Segura del Vestíbulo VR
    }

    /// <summary>
    /// [ES] Director Cinemático: Passthrough AR -> Grieta Dimensional -> Llegada de Nyx -> Combate.
    /// [EN] Cinematic Flow Director: AR Passthrough -> Dimensional Rift -> Nyx Arrival -> Combat.
    /// </summary>
    public class CinematicGameFlowDirector : MonoBehaviour
    {
        [Header("Estado de Flujo Cinemático")]
        [Tooltip("[ES] Iniciar directamente en AR Passthrough para el concurso Meta / [EN] Start directly in AR Passthrough")]
        [SerializeField] private bool startInARPassthrough = true;
        [SerializeField] private GameFlowState currentState = GameFlowState.ARSaloon_IncursionActive;
        [SerializeField] private float lobbyExploreDuration = 6.0f; // Tiempo antes de la brecha si se inicia en VR

        [Header("Referencias de Entornos")]
        [SerializeField] private CyberRoomDecorator3D roomDecorator;
        [SerializeField] private MRUKEncounterSpawner encounterSpawner;
        [SerializeField] private DimensionalRift3D dimensionalRift;

        [Header("Megatorre Holográfica y Pantalla Adaptativa")]
        [SerializeField] private Transform megatowerBillboardRoot;
        [SerializeField] private FlowRoomBackdropAnimator megatowerAnimator;
        [SerializeField] private bool autoAdjustScreenToPlayer = true;

        [Header("Audio y Alarmas")]
        [SerializeField] private AudioClip sirenBreachClip;
        [SerializeField] private AudioSource directorAudioSource;

        private static CinematicGameFlowDirector instance;
        public static CinematicGameFlowDirector Instance => instance;
        public GameFlowState CurrentState => currentState;

        private float stateTimer;
        private bool sequenceTriggered;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (directorAudioSource == null)
            {
                directorAudioSource = gameObject.AddComponent<AudioSource>();
                directorAudioSource.spatialBlend = 0.5f;
            }
        }

        private void Start()
        {
            if (roomDecorator == null) roomDecorator = FindAnyObjectByType<CyberRoomDecorator3D>();
            if (encounterSpawner == null) encounterSpawner = FindAnyObjectByType<MRUKEncounterSpawner>();
            if (dimensionalRift == null) dimensionalRift = FindAnyObjectByType<DimensionalRift3D>();

            if (megatowerBillboardRoot == null)
            {
                GameObject screenObj = GameObject.Find("Megatower_BroadcastScreen");
                if (screenObj != null)
                {
                    megatowerBillboardRoot = screenObj.transform;
                }
            }

            if (startInARPassthrough)
            {
                // [ES] Arranque directo en Mixed Reality (AR Passthrough a color)
                currentState = GameFlowState.ARSaloon_IncursionActive;
                stateTimer = 0f;
                sequenceTriggered = true;

                if (roomDecorator != null)
                {
                    roomDecorator.SetImmersiveVrMode(false);
                }

                if (PassthroughManager.Instance != null)
                {
                    PassthroughManager.Instance.EnablePassthrough();
                }

                if (BattleMusicManager.Instance != null)
                {
                    BattleMusicManager.Instance.EnsureRadio247Playing();
                }

                Debug.Log("[CinematicGameFlowDirector] ¡Arranque directo en AR Passthrough para Meta Quest 3S!");
            }
            else
            {
                // Modo VR con prólogo
                currentState = GameFlowState.VRLobby_StreetExploration;
                stateTimer = 0f;
                sequenceTriggered = false;

                if (roomDecorator != null)
                {
                    roomDecorator.SetImmersiveVrMode(true);
                }

                if (BattleMusicManager.Instance != null)
                {
                    BattleMusicManager.Instance.EnsureRadio247Playing();
                }

                Debug.Log("[CinematicGameFlowDirector] Inicio en modo prólogo VR.");
            }
        }

        private void Update()
        {
            // Auto-ajuste de la pantalla holográfica gigante hacia la perspectiva del jugador
            if (autoAdjustScreenToPlayer && megatowerBillboardRoot != null && Camera.main != null)
            {
                Vector3 lookTarget = Camera.main.transform.position;
                lookTarget.y = megatowerBillboardRoot.position.y;
                Vector3 direction = lookTarget - megatowerBillboardRoot.position;
                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    megatowerBillboardRoot.rotation = Quaternion.Slerp(megatowerBillboardRoot.rotation, targetRot, 2.5f * Time.deltaTime);
                }
            }

            // Atajo de prueba manual para saltar a la brecha con tecla B o botón B en Touch Plus
            if ((Input.GetKeyDown(KeyCode.B) || OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) 
                && currentState == GameFlowState.VRLobby_StreetExploration)
            {
                TriggerBreachIncursionSequence();
            }

            if (currentState == GameFlowState.VRLobby_StreetExploration && !sequenceTriggered)
            {
                stateTimer += Time.deltaTime;
                if (stateTimer >= lobbyExploreDuration)
                {
                    sequenceTriggered = true;
                    TriggerBreachIncursionSequence();
                }
            }
        }

        public void TriggerBreachIncursionSequence()
        {
            StartCoroutine(CoBreachTransitionRoutine());
        }

        private IEnumerator CoBreachTransitionRoutine()
        {
            currentState = GameFlowState.BreachAlarm_Countdown;
            Debug.Log("[CinematicGameFlowDirector] ¡ALERTA SÍSMICA! Abriendo Brecha hacia el Salón físico...");

            if (directorAudioSource != null && sirenBreachClip != null)
            {
                directorAudioSource.PlayOneShot(sirenBreachClip);
            }

            if (dimensionalRift != null)
            {
                dimensionalRift.TriggerBreachWarning();
            }

            if (BattleMusicManager.Instance != null)
            {
                BattleMusicManager.Instance.SetBattleTier(BattleTier.NormalBattle);
            }

            yield return new WaitForSeconds(3.0f);

            // Disolver la matriz VR -> Entrar al salón físico AR (Passthrough)
            currentState = GameFlowState.ARSaloon_IncursionActive;
            if (roomDecorator != null)
            {
                roomDecorator.SetImmersiveVrMode(false);
            }

            if (PassthroughManager.Instance != null)
            {
                PassthroughManager.Instance.PulsePassthroughColorTint(new Color(0f, 1f, 0.88f, 1f), 1.0f);
            }

            Debug.Log("[CinematicGameFlowDirector] ¡SALTO AR COMPLETADO! Ahora estás en tu salón real combatiendo la incursión híbrida.");
        }
    }
}
