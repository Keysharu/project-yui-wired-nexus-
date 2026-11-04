/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Seguidor Híbrido de Manos y Mandos XR para Meta Quest 3S.
 *      Soporta mandos Touch Plus (6DoF) mediante OpenXR (UnityEngine.XR.InputDevices)
 *      y Meta XR (OVRInput), con fallback ergonómico en PC para simulación con ratón en Editor.
 * ---
 * [EN] Hybrid XR Hands & Controllers Follower for Meta Quest 3S.
 *      Supports Touch Plus controllers (6DoF) via OpenXR (UnityEngine.XR.InputDevices)
 *      and Meta XR (OVRInput), with ergonomic FPS fallback for mouse/keyboard in Editor.
 * =============================================================================
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Controla el seguimiento de manos y mandos de Meta Quest 3S para el arsenal y escudo.
    /// [EN] Controls Meta Quest 3S hand and controller tracking for arsenal and shield.
    /// </summary>
    public class XRHandAnchorFollower : MonoBehaviour
    {
        public enum HandSide
        {
            LeftHand,
            RightHand
        }

        [Header("Configuración de Extremidad XR")]
        [Tooltip("[ES] Lado de la mano / [EN] Hand side")]
        [SerializeField] private HandSide handSide = HandSide.RightHand;

        [Header("Offset de Simulación en Editor / PC")]
        [SerializeField] private Vector3 editorCameraOffset = new Vector3(0.24f, -0.14f, 0.52f);

        [Header("Telemetría de Seguimiento")]
        [SerializeField] private bool isTracked = false;
        [SerializeField] private string activeTrackingDevice = "PC_Simulation";

        private Transform camTransform;
        private Transform trackingSpaceTransform;
        private InputDevice xrDevice;

        public bool IsTracked => isTracked;
        public HandSide Side => handSide;

        private void Start()
        {
            FindTrackingReferences();
            InitializeInputDevice();
        }

        private void InitializeInputDevice()
        {
            XRNode targetNode = handSide == HandSide.RightHand ? XRNode.RightHand : XRNode.LeftHand;
            xrDevice = InputDevices.GetDeviceAtXRNode(targetNode);
        }

        private void FindTrackingReferences()
        {
            if (Camera.main != null)
            {
                camTransform = Camera.main.transform;

                if (camTransform.parent != null)
                {
                    trackingSpaceTransform = camTransform.parent;
                }
            }

            if (trackingSpaceTransform == null)
            {
                GameObject ts = GameObject.Find("Player_Rig_Root");
                if (ts == null) ts = GameObject.Find("TrackingSpace");
                if (ts != null) trackingSpaceTransform = ts.transform;
            }
        }

        private void LateUpdate()
        {
            if (camTransform == null && Camera.main != null)
            {
                FindTrackingReferences();
            }

            // 1. PRIMERA PRIORIDAD: OpenXR Nativo de Unity (UnityEngine.XR.InputDevices)
            // Funciona al 100% en Meta Quest 3S con Unity 6 y controladores Touch Plus
            if (!xrDevice.isValid)
            {
                InitializeInputDevice();
            }

            if (xrDevice.isValid)
            {
                bool gotPos = xrDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 devicePos);
                bool gotRot = xrDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion deviceRot);

                if (gotPos && gotRot)
                {
                    ApplyTrackingTransform(devicePos, deviceRot);
                    isTracked = true;
                    activeTrackingDevice = "OpenXR_TouchPlus";
                    return;
                }
            }

            // 2. SEGUNDA PRIORIDAD: Meta XR SDK OVRInput directo
            OVRInput.Controller controllerMask = handSide == HandSide.RightHand 
                ? OVRInput.Controller.RTouch 
                : OVRInput.Controller.LTouch;

            if (OVRInput.GetControllerPositionTracked(controllerMask))
            {
                Vector3 localPos = OVRInput.GetLocalControllerPosition(controllerMask);
                Quaternion localRot = OVRInput.GetLocalControllerRotation(controllerMask);
                ApplyTrackingTransform(localPos, localRot);
                isTracked = true;
                activeTrackingDevice = "MetaXR_TouchPlus";
                return;
            }

            // 3. TERCERA OPCIÓN: Hand Tracking óptico de Meta Quest (manos desnudas)
            OVRInput.Controller handTrackingMask = handSide == HandSide.RightHand 
                ? OVRInput.Controller.RHand 
                : OVRInput.Controller.LHand;

            if (OVRInput.GetControllerPositionTracked(handTrackingMask))
            {
                Vector3 localPos = OVRInput.GetLocalControllerPosition(handTrackingMask);
                Quaternion localRot = OVRInput.GetLocalControllerRotation(handTrackingMask);
                ApplyTrackingTransform(localPos, localRot);
                isTracked = true;
                activeTrackingDevice = "MetaXR_HandTracking";
                return;
            }

            // 4. FALLBACK EN PC / EDITOR: FPS Anchor relativo a la cámara
            isTracked = false;
            activeTrackingDevice = "Editor_Simulation";
            if (camTransform != null)
            {
                Vector3 currentOffset = editorCameraOffset;
                if (handSide == HandSide.LeftHand && currentOffset.x > 0f)
                {
                    currentOffset.x = -currentOffset.x;
                }

                transform.position = camTransform.TransformPoint(currentOffset);
                transform.rotation = camTransform.rotation;
            }
        }

        private void ApplyTrackingTransform(Vector3 localPos, Quaternion localRot)
        {
            if (trackingSpaceTransform != null)
            {
                transform.position = trackingSpaceTransform.TransformPoint(localPos);
                transform.rotation = trackingSpaceTransform.rotation * localRot;
            }
            else if (transform.parent != null)
            {
                transform.localPosition = localPos;
                transform.localRotation = localRot;
            }
            else if (camTransform != null && camTransform.parent != null)
            {
                transform.position = camTransform.parent.TransformPoint(localPos);
                transform.rotation = camTransform.parent.rotation * localRot;
            }
            else
            {
                transform.position = localPos;
                transform.rotation = localRot;
            }
        }

        public void SetHandSide(HandSide side)
        {
            handSide = side;
            if (side == HandSide.LeftHand && editorCameraOffset.x > 0f)
            {
                editorCameraOffset.x = -editorCameraOffset.x;
            }
            else if (side == HandSide.RightHand && editorCameraOffset.x < 0f)
            {
                editorCameraOffset.x = Mathf.Abs(editorCameraOffset.x);
            }
            InitializeInputDevice();
        }
    }
}
