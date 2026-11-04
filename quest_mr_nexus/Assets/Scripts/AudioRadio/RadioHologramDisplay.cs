// =============================================================================
// Autora / Author: Raquel D. Torres (El taller de la gatita tech)
// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
// Meta Global AI Developer Hackathon 2026
// =============================================================================
// [ES] Visualizador Holografico para la Radio Chipset 107.7 FM.
//      Muestra el dial, volumen y animacion de audio.
// ---
// [EN] Holographic Visualizer for Radio Chipset 107.7 FM.
//      Displays dial frequency, audio volume and live broadcast animation.
// =============================================================================

using System;
using UnityEngine;

namespace ProjectNyx.AudioRadio
{
    /// <summary>
    /// [ES] Fase 2 del Montaje XR: Visualizador para el Sintonizador de Radio y Frecuencias Holograficas.
    /// Renderiza la barra de dial, volumen y espectro de frecuencias en un panel flotante holografico.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] XR Staging Phase 2: Visualizer for Holographic Radio Tuner & Frequencies.
    /// Renders frequency dial bars, volume levels, and audio spectrum on a floating holographic panel.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    [RequireComponent(typeof(RadioReceiver3D))]
    public class RadioHologramDisplay : MonoBehaviour
    {
        [Header("Renderizadores Holograficos de Locutoras (Opcionales / Deprecados)")]
        [SerializeField] private SpriteRenderer minaRenderer;
        [SerializeField] private SpriteRenderer kiraRenderer;
        [SerializeField] private SpriteRenderer studioCatRenderer;

        [Header("Sprites de Mina (Radio Chipset)")]
        [SerializeField] private Sprite minaIdle;
        [SerializeField] private Sprite minaHappy;
        [SerializeField] private Sprite[] minaTalkSprites;

        [Header("Sprites de Kira (Radio Chipset)")]
        [SerializeField] private Sprite kiraIdle;
        [SerializeField] private Sprite kiraHappy;
        [SerializeField] private Sprite[] kiraTalkSprites;

        [Header("Gatos DJ del Estudio")]
        [SerializeField] private Sprite[] studioCatSprites;

        [Header("Parametros de Animacion")]
        [SerializeField] private float visemeFrameRate = 9.5f;
        [SerializeField] private float hologramBobSpeed = 2.0f;
        [SerializeField] private float hologramBobAmplitude = 0.015f;

        [Header("Pantalla Flotante de Pared MR")]
        [SerializeField] private ProjectNyx.Core.MRWallFloatingScreen wallScreen;

        private RadioReceiver3D radioReceiver;
        private Transform playerHead;
        private float broadcastAnimTimer;
        private bool isBroadcastingLive;

        private void Awake()
        {
            radioReceiver = GetComponent<RadioReceiver3D>();

            // Desactivar presentadoras individuales flotando en mitad de la sala AR
            if (minaRenderer != null) minaRenderer.gameObject.SetActive(false);
            if (kiraRenderer != null) kiraRenderer.gameObject.SetActive(false);
            if (studioCatRenderer != null) studioCatRenderer.gameObject.SetActive(false);
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                playerHead = Camera.main.transform;
            }

            // Buscar pantalla flotante solo si ya esta pre-configurada en la escena
            if (wallScreen == null)
            {
                wallScreen = FindAnyObjectByType<ProjectNyx.Core.MRWallFloatingScreen>();
            }

            // Saludo inicial al encender la emisora en el salon
            TriggerLiveBroadcastAnimation(5.0f);
        }

        private void Update()
        {
            if (isBroadcastingLive)
            {
                broadcastAnimTimer -= Time.deltaTime;
                if (broadcastAnimTimer <= 0f)
                {
                    isBroadcastingLive = false;
                }
            }
        }

        /// <summary>
        /// [ES] Senaliza la intervencion en directo de la emisora.
        /// ---
        /// [EN] Signals live radio intervention.
        /// </summary>
        public void TriggerLiveBroadcastAnimation(float durationSeconds = 5.0f)
        {
            isBroadcastingLive = true;
            broadcastAnimTimer = Mathf.Max(durationSeconds, 2.0f);
        }
    }
}
