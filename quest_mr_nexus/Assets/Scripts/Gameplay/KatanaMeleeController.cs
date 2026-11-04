using System;
using System.Collections;
using UnityEngine;
using ProjectNyx.Companion;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.Gameplay
{
    /// <summary>
    /// [ES] Fase 3 del Montaje XR: Controlador de Combate Melee para la Katana Ciberpunk.
    /// Ejecuta cortes físicos frontales sin disparar proyectiles de plasma,
    /// consume Resistencia (Stamina), aplica daño contundente contra enemigos acorazados
    /// y rompe el escudo físico del Centinela Bastión y arañas.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] XR Staging Phase 3: Melee Combat Controller for the Cyberpunk Katana.
    /// Performs physical frontal slashes without consuming plasma ammo,
    /// depletes player Stamina, inflicts blunt kinetic damage on armored enemies,
    /// and breaks physical shield barriers on Centurion Bastions and spiders.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public class KatanaMeleeController : MonoBehaviour
    {
        [Header("Parámetros Melee")]
        [SerializeField] private float attackRange = 2.1f;
        [SerializeField] private float attackRadius = 0.85f;
        [SerializeField] private int baseMeleeDamage = 65;
        [SerializeField] private float attackCooldown = 0.42f;
        [SerializeField] private float staminaCostPerSwing = 15f;

        [Header("Transform y Animación")]
        [SerializeField] private Transform bladeRoot;
        [SerializeField] private TrailRenderer swingTrail;

        [Header("Audio y Háptica")]
        [SerializeField] private AudioSource meleeAudioSource;
        [SerializeField] private AudioClip swingClip;
        [SerializeField] private AudioClip hitMetalClip;
        [SerializeField] private OVRInput.Controller controllerHand = OVRInput.Controller.RTouch;

        [Header("Vínculo con Guantelete")]
        [SerializeField] private HolographicGauntlet parentGauntlet;

        private float nextAttackTime;
        private bool isSwinging;
        private Vector3 initialLocalPos;
        private Quaternion initialLocalRot;

        public bool IsSwinging => isSwinging;

        private void Awake()
        {
            if (bladeRoot == null) bladeRoot = transform;
            initialLocalPos = bladeRoot.localPosition;
            initialLocalRot = bladeRoot.localRotation;

            if (meleeAudioSource == null)
            {
                meleeAudioSource = gameObject.AddComponent<AudioSource>();
                meleeAudioSource.spatialBlend = 0.6f;
                meleeAudioSource.playOnAwake = false;
            }

            if (parentGauntlet == null)
            {
                parentGauntlet = GetComponentInParent<HolographicGauntlet>();
            }

            if (swingTrail != null)
            {
                swingTrail.emitting = false;
            }
        }

        private void Update()
        {
            // Solo procesar ataques si el arma activa en el guantelete es la Katana
            if (parentGauntlet != null && parentGauntlet.CurrentWeapon != SolidLightWeaponType.PlasmaBlade)
            {
                return;
            }

            bool attackTriggered = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controllerHand) ||
                                   Input.GetMouseButtonDown(0) ||
                                   Input.GetKeyDown(KeyCode.Space);

            if (attackTriggered && Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                ExecuteMeleeSlash();
            }
        }

        public void ExecuteMeleeSlash()
        {
            if (isSwinging) return;
            StartCoroutine(CoMeleeSlashRoutine());
        }

        private IEnumerator CoMeleeSlashRoutine()
        {
            isSwinging = true;

            // Reproducir sonido de tajo
            if (meleeAudioSource != null && swingClip != null)
            {
                meleeAudioSource.PlayOneShot(swingClip);
            }

            if (swingTrail != null) swingTrail.emitting = true;

            // Háptica de golpe seco
            OVRInput.SetControllerVibration(0.35f, 0.45f, controllerHand);

            // Detección de impacto física frontal
            Transform camTransform = Camera.main != null ? Camera.main.transform : transform;
            Vector3 center = camTransform.position + camTransform.forward * (attackRange * 0.6f);
            Collider[] hits = Physics.OverlapSphere(center, attackRadius);

            bool hitAnyTarget = false;
            foreach (Collider hit in hits)
            {
                if (hit == null) continue;

                // 1. Cofres de Suministros (abrir de un tajo)
                CyberSupplyChest chest = hit.GetComponentInParent<CyberSupplyChest>();
                if (chest != null)
                {
                    chest.OpenChest();
                    hitAnyTarget = true;
                }

                // 2. Arañas Arácnidas
                NeoCyber.AR.NeoCyberSpiderDroneAI spider = hit.GetComponentInParent<NeoCyber.AR.NeoCyberSpiderDroneAI>();
                if (spider != null)
                {
                    spider.ApplyBlasterHit(baseMeleeDamage, hit.ClosestPoint(center));
                    hitAnyTarget = true;
                }

                // 3. Boss Quantum Centurion (rompe escudos físicos)
                QuantumCenturionBossAI boss = hit.GetComponentInParent<QuantumCenturionBossAI>();
                if (boss != null)
                {
                    boss.TakeDamage(baseMeleeDamage, hit.ClosestPoint(center));
                    hitAnyTarget = true;
                }
            }

            if (hitAnyTarget && meleeAudioSource != null && hitMetalClip != null)
            {
                meleeAudioSource.PlayOneShot(hitMetalClip);
                OVRInput.SetControllerVibration(0.7f, 0.85f, controllerHand);
            }

            // Animación de tajo diagonal rápida
            float elapsed = 0f;
            float duration = 0.22f;
            Vector3 slashOffset = new Vector3(-0.14f, -0.08f, 0.12f);
            Quaternion slashRotation = initialLocalRot * Quaternion.Euler(15f, -35f, 40f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                bladeRoot.localPosition = Vector3.Lerp(initialLocalPos + slashOffset, initialLocalPos, t);
                bladeRoot.localRotation = Quaternion.Slerp(slashRotation, initialLocalRot, t);
                yield return null;
            }

            bladeRoot.localPosition = initialLocalPos;
            bladeRoot.localRotation = initialLocalRot;

            if (swingTrail != null) swingTrail.emitting = false;
            OVRInput.SetControllerVibration(0f, 0f, controllerHand);
            isSwinging = false;
        }
    }
}
