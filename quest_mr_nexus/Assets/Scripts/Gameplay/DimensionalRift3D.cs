using System;
using System.Collections;
using UnityEngine;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.Gameplay
{
    public enum RiftStage
    {
        Dormant,
        WarningPulse,
        NormalWavePortal,
        BossMegaPortal
    }

    /// <summary>
    /// [ES] Fase 3 del Plan Maestro: Portal / Brecha Interdimensional 3D Circular en la Pared.
    /// Proyecta una anomalía cuántica flotante en la pared física de la que emergen drones y arañas.
    /// ---
    /// [EN] Master Plan Phase 3: 3D Circular Interdimensional Wall Rift.
    /// Projects a quantum spatial anomaly against the physical room wall from which drones and spiders emerge.
    /// </summary>
    public class DimensionalRift3D : MonoBehaviour
    {
        [Header("Nodos 3D del Portal Circular")]
        [SerializeField] private Transform portalRoot;
        [SerializeField] private Transform outerRing;
        [SerializeField] private Transform innerRing;
        [SerializeField] private Transform voidCoreDisc;
        [SerializeField] private Light riftPointLight;

        [Header("Escalas por Fase")]
        [SerializeField] private float warningScale = 0.45f;
        [SerializeField] private float normalWaveScale = 1.55f;
        [SerializeField] private float bossExpansionScale = 2.75f;
        [SerializeField] private float scaleTransitionSpeed = 3.2f;

        [Header("Velocidades de Giro 3D")]
        [SerializeField] private float outerRingSpeed = 55f;
        [SerializeField] private float innerRingSpeed = -95f;

        private RiftStage currentStage = RiftStage.Dormant;
        private Vector3 targetScaleVec = Vector3.zero;
        private AudioSource riftAudio;
        private AudioClip riftVortexClip;

        public RiftStage CurrentStage => currentStage;

        private void Awake()
        {
            if (portalRoot == null) portalRoot = transform;

            riftAudio = GetComponent<AudioSource>();
            if (riftAudio == null)
            {
                riftAudio = gameObject.AddComponent<AudioSource>();
                riftAudio.spatialBlend = 1.0f;
                riftAudio.playOnAwake = false;
            }

            riftVortexClip = SynthSfxGenerator.CreateDimensionalRiftClip();
        }

        private void Update()
        {
            if (currentStage == RiftStage.Dormant) return;

            float mult = (currentStage == RiftStage.BossMegaPortal) ? 2.2f : 1.0f;

            if (outerRing != null)
            {
                outerRing.Rotate(0f, 0f, outerRingSpeed * mult * Time.deltaTime, Space.Self);
            }

            if (innerRing != null)
            {
                innerRing.Rotate(0f, 0f, innerRingSpeed * mult * Time.deltaTime, Space.Self);
            }

            if (voidCoreDisc != null)
            {
                float pulse = 1.0f + Mathf.Sin(Time.time * 6.0f * mult) * 0.05f;
                voidCoreDisc.localScale = new Vector3(pulse, 0.02f, pulse);
            }

            if (portalRoot != null)
            {
                portalRoot.localScale = Vector3.Lerp(portalRoot.localScale, targetScaleVec, Time.deltaTime * scaleTransitionSpeed);
            }

            if (riftPointLight != null)
            {
                float baseIntensity = (currentStage == RiftStage.BossMegaPortal) ? 5.5f : 2.8f;
                riftPointLight.intensity = baseIntensity + Mathf.Sin(Time.time * 9f) * 0.8f;
            }
        }

        /// <summary>
        /// Activa la advertencia visual y sonora de que se está rasgando el tejido espacial.
        /// </summary>
        public void TriggerBreachWarning()
        {
            gameObject.SetActive(true);
            currentStage = RiftStage.WarningPulse;
            portalRoot.localScale = Vector3.one * 0.1f;
            targetScaleVec = Vector3.one * warningScale;

            if (riftPointLight != null)
            {
                riftPointLight.color = new Color(1.0f, 0.15f, 0.55f); // Alerta Magenta
            }

            if (riftAudio != null && riftVortexClip != null)
            {
                riftAudio.PlayOneShot(riftVortexClip, 0.75f);
            }

            Debug.Log("[DimensionalRift3D] ¡ADVERTENCIA DE BRECHA! Inestabilidad dimensional detectada en el muro norte.");
        }

        /// <summary>
        /// Abre el portal circular a tamaño de incursión de oleadas (1.55m, estilo Spatial Ops).
        /// </summary>
        public void OpenForBasicWaves()
        {
            gameObject.SetActive(true);
            currentStage = RiftStage.NormalWavePortal;
            targetScaleVec = Vector3.one * normalWaveScale;

            if (riftPointLight != null)
            {
                riftPointLight.color = new Color(0.0f, 0.95f, 1.0f); // Azul/Cian Portal Spatial Ops
            }

            if (riftAudio != null && riftVortexClip != null)
            {
                riftAudio.PlayOneShot(riftVortexClip, 0.95f);
            }

            Debug.Log("[DimensionalRift3D] Portal interdimensional abierto para oleadas de Cyber-Spiders.");
        }

        /// <summary>
        /// Expande la brecha al doble de tamaño (2.75m) con sobrecarga magenta/cian para la salida del Boss.
        /// </summary>
        public void ExpandForBossBreach()
        {
            gameObject.SetActive(true);
            currentStage = RiftStage.BossMegaPortal;
            targetScaleVec = Vector3.one * bossExpansionScale;

            if (riftPointLight != null)
            {
                riftPointLight.color = new Color(1.0f, 0.05f, 0.50f); // Sobrecarga Magenta Boss
            }

            if (riftAudio != null && riftVortexClip != null)
            {
                riftAudio.PlayOneShot(riftVortexClip, 1.0f);
            }

            Debug.Log("[DimensionalRift3D] ¡SOBRECARGA DIMENSIONAL! La brecha se ha expandido al doble para el Quantum Centurión.");
        }
    }
}
