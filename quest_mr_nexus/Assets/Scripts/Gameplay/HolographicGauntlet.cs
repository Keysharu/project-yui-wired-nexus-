using UnityEngine.XR;
using System;
using System.Collections;
using UnityEngine;

namespace ProjectNyx.Gameplay
{
    public enum SolidLightWeaponType
    {
        Blaster,
        PlasmaBlade,
        EnergyCrossbow
    }

    /// <summary>
    /// [ES] Guantelete Holográfico de Nanotecnología (Solid-Light Projector).
    /// Selector y proyector de armas virtuales acoplado a la muñeca de la jugadora en Meta Quest 3S.
    /// ---
    /// [EN] Nanotech Holographic Gauntlet (Solid-Light Projector).
    /// Wrist-mounted virtual weapon projector and selector for the player's hand in Meta Quest 3S.
    /// </summary>
    public class HolographicGauntlet : MonoBehaviour
    {
        [Header("Referencias del Guantelete")]
        [Tooltip("Transform anclado a la muñeca o mano del jugador")]
        [SerializeField] private Transform wristAnchor;

        [Tooltip("Emisor de partículas holográficas de nanotecnología")]
        [SerializeField] private ParticleSystem projectionVFX;

        [Header("Armas de Luz Sólida (Objetos Hijos Ligeros)")]
        [SerializeField] private GameObject blasterHologram;
        [SerializeField] private GameObject plasmaBladeHologram;
        [SerializeField] private GameObject energyCrossbowHologram;

        [Header("Ajustes de Sonido y Feedback")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip switchSound;
        [SerializeField] private AudioClip bladeHumSound;
        [SerializeField] private AudioClip bowChargeSound;

        [Header("Estado Actual")]
        [SerializeField] private SolidLightWeaponType currentWeapon = SolidLightWeaponType.Blaster;
        private bool hadSecondaryPressed;
        [SerializeField] private OVRInput.Controller controllerHand = OVRInput.Controller.RTouch;

        public SolidLightWeaponType CurrentWeapon => currentWeapon;

        private Coroutine activeHapticCoroutine;

        private void Start()
        {
            EquipWeapon(currentWeapon);
        }

        private void Update()
        {
            // Con el menú de pausa abierto no se cambia de arma
            if (ProjectNyx.Core.GameSession.PlayerInputBlocked) return;

            // Cambio rápido con botón secundario o joystick lateral
            bool openXrSwitch = false;
            InputDevice xrDev = InputDevices.GetDeviceAtXRNode(controllerHand == OVRInput.Controller.RTouch ? XRNode.RightHand : XRNode.LeftHand);
            if (xrDev.isValid && xrDev.TryGetFeatureValue(CommonUsages.secondaryButton, out bool sb) && sb)
            {
                if (!hadSecondaryPressed) { openXrSwitch = true; hadSecondaryPressed = true; }
            }
            else { hadSecondaryPressed = false; }

            if (openXrSwitch || OVRInput.GetDown(OVRInput.Button.Two, controllerHand) || Input.GetKeyDown(KeyCode.Tab))
            {
                CycleNextWeapon();
            }

            // Háptica continua sutil si la espada de plasma está encendida
            if (currentWeapon == SolidLightWeaponType.PlasmaBlade && OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controllerHand))
            {
                OVRInput.SetControllerVibration(0.05f, 0.1f, controllerHand);
            }
        }

        public void CycleNextWeapon()
        {
            int next = ((int)currentWeapon + 1) % Enum.GetValues(typeof(SolidLightWeaponType)).Length;
            EquipWeapon((SolidLightWeaponType)next);
        }

        public void EquipWeapon(SolidLightWeaponType newWeapon)
        {
            currentWeapon = newWeapon;

            // Desactivar todas las proyecciones para mantener consumo mínimo
            if (blasterHologram != null) blasterHologram.SetActive(false);
            if (plasmaBladeHologram != null) plasmaBladeHologram.SetActive(false);
            if (energyCrossbowHologram != null) energyCrossbowHologram.SetActive(false);

            // Activar partículas de proyección
            if (projectionVFX != null)
            {
                projectionVFX.Play();
            }

            if (switchSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(switchSound);
            }

            // Activar la elegida y disparar háptica de cambio
            switch (currentWeapon)
            {
                case SolidLightWeaponType.Blaster:
                    if (blasterHologram != null) blasterHologram.SetActive(true);
                    TriggerHapticPulse(0.3f, 0.4f, 0.15f); // Pulso seco y ágil
                    break;

                case SolidLightWeaponType.PlasmaBlade:
                    if (plasmaBladeHologram != null) plasmaBladeHologram.SetActive(true);
                    TriggerHapticPulse(0.15f, 0.25f, 0.35f); // Zumbido sutil
                    break;

                case SolidLightWeaponType.EnergyCrossbow:
                    if (energyCrossbowHologram != null) energyCrossbowHologram.SetActive(true);
                    TriggerHapticPulse(0.5f, 0.7f, 0.25f); // Tensado contundente
                    break;
            }

            Debug.Log($"[HolographicGauntlet] Proyección sólida activada: {currentWeapon}");
        }

        /// <summary>
        /// Disparo háptico diferenciado por perfil de arma en los mandos Touch Plus.
        /// </summary>
        public void TriggerHapticPulse(float frequency, float amplitude, float duration)
        {
            if (activeHapticCoroutine != null)
                StopCoroutine(activeHapticCoroutine);

            activeHapticCoroutine = StartCoroutine(CoHapticPulse(frequency, amplitude, duration));
        }

        private IEnumerator CoHapticPulse(float frequency, float amplitude, float duration)
        {
            OVRInput.SetControllerVibration(frequency, amplitude, controllerHand);
            yield return new WaitForSeconds(duration);
            OVRInput.SetControllerVibration(0f, 0f, controllerHand);
            activeHapticCoroutine = null;
        }

        private void OnDisable()
        {
            OVRInput.SetControllerVibration(0f, 0f, controllerHand);
        }
    }
}
