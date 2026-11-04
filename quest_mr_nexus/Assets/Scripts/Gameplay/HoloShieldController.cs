using UnityEngine.XR;
using System;
using UnityEngine;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.Gameplay
{
    /// <summary>
    /// [ES] Fase 3 del Montaje XR: Guantelete y Escudo Holográfico (Holo-Shield).
    /// Despliega una barrera de luz sólida para bloquear proyectiles enemigos y disparos de torreta.
    /// ---
    /// [EN] XR Staging Phase 3: Holographic Gauntlet & Solid-Light Shield (Holo-Shield).
    /// Deploys a solid-light barrier on the player's wrist to deflect incoming enemy plasma projectiles.
    /// </summary>
    public class HoloShieldController : MonoBehaviour
    {
        [Header("Renderizadores Visuales y Sprites FPS")]
        [SerializeField] private SpriteRenderer gauntletSpriteRenderer;
        [SerializeField] private Sprite idleNoShieldSprite;
        [SerializeField] private Sprite activeShieldSprite;
        [SerializeField] private GameObject activeBarrierRoot;
        [SerializeField] private Collider shieldTriggerCollider;

        [Header("Energía del Escudo")]
        [SerializeField] private float maxShieldEnergy = 100f;
        [SerializeField] private float drainPerSecond = 18f;
        [SerializeField] private float rechargePerSecond = 25f;

        [Header("Control y Audio")]
        [SerializeField] private OVRInput.Controller controllerHand = OVRInput.Controller.LTouch;
        [SerializeField] private AudioSource shieldAudioSource;
        [SerializeField] private AudioClip deployClip;

        private float currentEnergy;
        private bool isShieldActive;

        public bool IsShieldActive => isShieldActive;
        public float EnergyRatio => maxShieldEnergy > 0f ? currentEnergy / maxShieldEnergy : 0f;

        private void Awake()
        {
            currentEnergy = maxShieldEnergy;
            if (gauntletSpriteRenderer == null)
            {
                gauntletSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (shieldAudioSource == null)
            {
                shieldAudioSource = gameObject.AddComponent<AudioSource>();
                shieldAudioSource.spatialBlend = 0.8f;
                shieldAudioSource.playOnAwake = false;
            }

            if (deployClip == null)
            {
                deployClip = SynthSfxGenerator.CreateShieldPulseClip();
            }

            SetShieldState(false);
        }

        private void Update()
        {
            bool openXrShield = false;
            InputDevice xrDev = InputDevices.GetDeviceAtXRNode(controllerHand == OVRInput.Controller.LTouch ? XRNode.LeftHand : XRNode.RightHand);
            if (xrDev.isValid && ((xrDev.TryGetFeatureValue(CommonUsages.gripButton, out bool gb) && gb) || (xrDev.TryGetFeatureValue(CommonUsages.triggerButton, out bool tb) && tb)))
            {
                openXrShield = true;
            }

            bool wantsShield = !ProjectNyx.Core.GameSession.PlayerInputBlocked &&
                               (openXrShield || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controllerHand) ||
                                OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controllerHand) ||
                                Input.GetMouseButton(1) ||
                                Input.GetKey(KeyCode.LeftShift));

            if (wantsShield && currentEnergy > 5f)
            {
                if (!isShieldActive)
                {
                    SetShieldState(true);
                }

                currentEnergy = Mathf.Max(0f, currentEnergy - drainPerSecond * Time.deltaTime);
                if (currentEnergy <= 0.1f)
                {
                    SetShieldState(false);
                }
            }
            else
            {
                if (isShieldActive)
                {
                    SetShieldState(false);
                }

                currentEnergy = Mathf.Min(maxShieldEnergy, currentEnergy + rechargePerSecond * Time.deltaTime);
            }
        }

        public void SetShieldState(bool active)
        {
            isShieldActive = active;

            if (gauntletSpriteRenderer != null)
            {
                if (active && activeShieldSprite != null)
                {
                    gauntletSpriteRenderer.sprite = activeShieldSprite;
                }
                else if (!active && idleNoShieldSprite != null)
                {
                    gauntletSpriteRenderer.sprite = idleNoShieldSprite;
                }
            }

            if (activeBarrierRoot != null)
            {
                activeBarrierRoot.SetActive(active);
            }

            if (shieldTriggerCollider != null)
            {
                shieldTriggerCollider.enabled = active;
            }

            if (active)
            {
                if (shieldAudioSource != null && deployClip != null && Application.isPlaying)
                {
                    shieldAudioSource.PlayOneShot(deployClip);
                }
                OVRInput.SetControllerVibration(0.25f, 0.35f, controllerHand);
                if (Application.isPlaying)
                {
                    Invoke(nameof(StopHaptics), 0.12f);
                }
            }
            else
            {
                StopHaptics();
            }
        }

        public void OnProjectileBlocked(int incomingDamage)
        {
            currentEnergy = Mathf.Max(0f, currentEnergy - incomingDamage * 0.5f);
            OVRInput.SetControllerVibration(0.6f, 0.8f, controllerHand);
            if (Application.isPlaying)
            {
                Invoke(nameof(StopHaptics), 0.15f);
            }

            if (shieldAudioSource != null && deployClip != null)
            {
                shieldAudioSource.PlayOneShot(deployClip);
            }
        }

        private void StopHaptics()
        {
            OVRInput.SetControllerVibration(0f, 0f, controllerHand);
        }
    }
}
