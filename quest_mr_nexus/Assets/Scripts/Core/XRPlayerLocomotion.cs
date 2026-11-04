/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Controlador de Locomoción Cómoda en VR/MR para Meta Quest 3S.
 *      - Joystick Izquierdo: Movimiento suave continuo relativo a la vista (cero mareos).
 *      - Joystick Derecho: Giro por tramos (Snap Turn de 45°) anti-cinetosis.
 *      - Mueve el nodo raíz 'Player_Rig_Root' para que la cámara y el arsenal viajen juntos.
 * ---
 * [EN] Comfortable VR/MR Locomotion Controller for Meta Quest 3S.
 *      - Left Thumbstick: Smooth continuous locomotion relative to head orientation (zero nausea).
 *      - Right Thumbstick: Snap turning (45° intervals) to prevent motion sickness.
 *      - Translates 'Player_Rig_Root' so the camera rig and arsenal travel seamlessly together.
 * =============================================================================
 */

using System;
using UnityEngine;
using UnityEngine.XR;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Controla la locomoción de la jugadora mediante los joysticks analógicos de los mandos Touch Plus.
    /// [EN] Controls player locomotion using Meta Quest Touch Plus analog thumbsticks.
    /// </summary>
    public class XRPlayerLocomotion : MonoBehaviour
    {
        [Header("Configuración de Movimiento Suave (Stick Izquierdo)")]
        [Tooltip("[ES] Velocidad de caminata cómoda en m/s / [EN] Comfortable walking speed in m/s")]
        [SerializeField] private float moveSpeed = 2.2f;

        [Header("Configuración de Giro (Stick Derecho)")]
        [Tooltip("[ES] Ángulo de giro instantáneo / [EN] Snap turn angle")]
        [SerializeField] private float snapTurnAngle = 45.0f;
        [Tooltip("[ES] Umbral del joystick para registrar el giro / [EN] Thumbstick deadzone threshold")]
        [SerializeField] private float snapThreshold = 0.65f;

        [Header("Referencias")]
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Transform headCamera;

        private InputDevice leftController;
        private InputDevice rightController;
        private bool hasTurnedThisFlick = false;

        private void Start()
        {
            if (playerRoot == null)
            {
                playerRoot = transform;
            }

            if (headCamera == null && Camera.main != null)
            {
                headCamera = Camera.main.transform;
            }

            InitializeControllers();
        }

        private void InitializeControllers()
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }

        private void Update()
        {
            if (headCamera == null && Camera.main != null)
            {
                headCamera = Camera.main.transform;
            }

            if (!leftController.isValid || !rightController.isValid)
            {
                InitializeControllers();
            }

            // 1. OBTENER ENTRADA DEL JOYSTICK IZQUIERDO (Locomoción continua)
            Vector2 leftStick = Vector2.zero;

            // Intentar OpenXR primero
            if (leftController.isValid && leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axisLeft))
            {
                leftStick = axisLeft;
            }

            // Fallback a OVRInput
            if (leftStick.sqrMagnitude < 0.01f)
            {
                leftStick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            }

            // 2. APLICAR TRASLACIÓN SUAVE RELATIVA AL HORIZONTE DE LA MIRADA
            if (leftStick.sqrMagnitude > 0.04f && headCamera != null && playerRoot != null)
            {
                Vector3 forward = headCamera.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.001f) forward.Normalize();
                else forward = Vector3.forward;

                Vector3 right = headCamera.right;
                right.y = 0f;
                if (right.sqrMagnitude > 0.001f) right.Normalize();
                else right = Vector3.right;

                Vector3 direction = (forward * leftStick.y + right * leftStick.x);
                if (direction.sqrMagnitude > 1f) direction.Normalize();

                playerRoot.position += direction * (moveSpeed * Time.deltaTime);
            }

            // 3. OBTENER ENTRADA DEL JOYSTICK DERECHO (Giro por tramos / Snap Turn)
            Vector2 rightStick = Vector2.zero;

            if (rightController.isValid && rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axisRight))
            {
                rightStick = axisRight;
            }

            if (rightStick.sqrMagnitude < 0.01f)
            {
                rightStick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            }

            // 4. APLICAR SNAP TURN
            if (Mathf.Abs(rightStick.x) > snapThreshold)
            {
                if (!hasTurnedThisFlick && playerRoot != null && headCamera != null)
                {
                    float angle = rightStick.x > 0 ? snapTurnAngle : -snapTurnAngle;
                    playerRoot.RotateAround(headCamera.position, Vector3.up, angle);
                    hasTurnedThisFlick = true;
                }
            }
            else if (Mathf.Abs(rightStick.x) < 0.2f)
            {
                hasTurnedThisFlick = false;
            }
        }
    }
}
