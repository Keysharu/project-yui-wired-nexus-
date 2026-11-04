using UnityEngine.XR;
using System;
using System.Collections;
using UnityEngine;
using ProjectNyx.Companion;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.Gameplay
{
    /// <summary>
    /// [ES] Pieza 5 del Puzzle: Arma de energía en Realidad Mixta.
    /// Dispara ráfagas de plasma proyectadas desde el guantelete táctico con retroceso y sonido 3D.
    /// ---
    /// [EN] Puzzle Piece 5: Mixed Reality Energy Blaster.
    /// Fires high-energy plasma bolts from the tactical gauntlet with haptic recoil and 3D audio.
    /// </summary>
    public class EnergyBlaster : MonoBehaviour
    {
        [Header("Vínculo con Guantelete y Nyx")]
        [SerializeField] private HolographicGauntlet parentGauntlet;
        [SerializeField] private NyxAdaptationEngine nyxAdaptation;

        [Header("Configuración de Disparo")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private Transform visualWeaponRoot;
        [SerializeField] private float range = 25.0f;
        [SerializeField] private int damagePerShot = 25;
        [SerializeField] private float fireRate = 0.22f;

        [Header("Efectos Acústicos y Visuales")]
        [SerializeField] private AudioSource blasterAudioSource;
        [SerializeField] private AudioClip shotClip;
        [SerializeField] private LineRenderer laserLineRenderer;
        [SerializeField] private float laserDuration = 0.06f;

        [Header("Controles")]
        [SerializeField] private OVRInput.Controller controllerHand = OVRInput.Controller.RTouch;

        private float nextFireTime;
        private bool hadTriggerPressed;
        private Vector3 initialWeaponLocalPos;

        private void Awake()
        {
            if (parentGauntlet == null)
            {
                parentGauntlet = GetComponentInParent<HolographicGauntlet>();
            }

            if (blasterAudioSource == null)
            {
                blasterAudioSource = gameObject.AddComponent<AudioSource>();
                blasterAudioSource.spatialBlend = 0.7f;
                blasterAudioSource.playOnAwake = false;
            }

            if (shotClip == null)
            {
                shotClip = SynthSfxGenerator.CreatePlasmaBlasterClip();
            }

            if (visualWeaponRoot != null)
            {
                initialWeaponLocalPos = visualWeaponRoot.localPosition;
            }
        }

        private void Start()
        {
            if (nyxAdaptation == null)
            {
                nyxAdaptation = FindAnyObjectByType<NyxAdaptationEngine>();
            }
        }

        private void Update()
        {
            // BLOQUEO QUIRÚRGICO: Si el arma activa en el guantelete NO es el Blaster, NO disparar
            if (parentGauntlet != null && parentGauntlet.CurrentWeapon != SolidLightWeaponType.Blaster)
            {
                return;
            }

            bool openXrTrigger = false;
            InputDevice xrDev = InputDevices.GetDeviceAtXRNode(controllerHand == OVRInput.Controller.RTouch ? XRNode.RightHand : XRNode.LeftHand);
            if (xrDev.isValid && xrDev.TryGetFeatureValue(CommonUsages.triggerButton, out bool tb) && tb)
            {
                if (!hadTriggerPressed) { openXrTrigger = true; hadTriggerPressed = true; }
            }
            else { hadTriggerPressed = false; }

            bool firePressed = openXrTrigger || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controllerHand) ||
                               Input.GetMouseButtonDown(0) ||
                               Input.GetKeyDown(KeyCode.Space);

            if (firePressed && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                Shoot();
            }
        }

        public void Shoot()
        {
            Transform originTransform = muzzlePoint != null ? muzzlePoint : (Camera.main != null ? Camera.main.transform : transform);
            Ray ray = new Ray(originTransform.position, originTransform.forward);

            if (Input.GetMouseButtonDown(0) && Camera.main != null)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }

            RaycastHit hit;
            Vector3 targetPoint = ray.origin + (ray.direction * range);

            if (Physics.Raycast(ray, out hit, range))
            {
                targetPoint = hit.point;

                CyberSupplyChest chest = hit.collider.GetComponentInParent<CyberSupplyChest>();
                if (chest != null)
                {
                    chest.OpenChest();
                }

                DimensionalRift rift = hit.collider.GetComponentInParent<DimensionalRift>();
                if (rift != null)
                {
                    rift.TakeDamage(damagePerShot);
                }

                NeoCyber.AR.NeoCyberSpiderDroneAI enemyAI = hit.collider.GetComponentInParent<NeoCyber.AR.NeoCyberSpiderDroneAI>();
                if (enemyAI != null)
                {
                    enemyAI.ApplyBlasterHit(damagePerShot, hit.point);
                }

                QuantumCenturionBossAI centurionBoss = hit.collider.GetComponentInParent<QuantumCenturionBossAI>();
                if (centurionBoss != null)
                {
                    centurionBoss.TakeDamage(damagePerShot, hit.point);
                }

                ModularHiveMindBoss hiveBoss = hit.collider.GetComponentInParent<ModularHiveMindBoss>();
                if (hiveBoss != null)
                {
                    hiveBoss.DamageCore(damagePerShot);
                }
            }

            if (nyxAdaptation != null)
            {
                nyxAdaptation.RegisterShot();
            }

            if (blasterAudioSource != null && shotClip != null)
            {
                blasterAudioSource.PlayOneShot(shotClip);
            }

            if (visualWeaponRoot != null)
            {
                StartCoroutine(CoWeaponRecoil());
            }

            OVRInput.SetControllerVibration(0.4f, 0.4f, controllerHand);
            if (Application.isPlaying)
            {
                Invoke(nameof(StopHaptics), 0.1f);
            }

            if (laserLineRenderer != null)
            {
                StartCoroutine(RoutineShowLaser(ray.origin, targetPoint));
            }
        }

        private IEnumerator CoWeaponRecoil()
        {
            Vector3 recoilPos = initialWeaponLocalPos + new Vector3(0f, 0.015f, -0.04f);
            visualWeaponRoot.localPosition = recoilPos;
            float elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                visualWeaponRoot.localPosition = Vector3.Lerp(recoilPos, initialWeaponLocalPos, elapsed / 0.1f);
                yield return null;
            }
            visualWeaponRoot.localPosition = initialWeaponLocalPos;
        }

        private void StopHaptics()
        {
            OVRInput.SetControllerVibration(0, 0, controllerHand);
        }

        private IEnumerator RoutineShowLaser(Vector3 start, Vector3 end)
        {
            laserLineRenderer.enabled = true;
            laserLineRenderer.SetPosition(0, start);
            laserLineRenderer.SetPosition(1, end);
            yield return new WaitForSeconds(laserDuration);
            laserLineRenderer.enabled = false;
        }
    }
}
