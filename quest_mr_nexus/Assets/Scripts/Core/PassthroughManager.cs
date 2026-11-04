/**
 * =============================================================================
 * Autora / Author: Raquel D. Torres (El taller de la gatita tech)
 * Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
 * Meta Global AI Developer Hackathon 2026
 * =============================================================================
 * [ES] Gestor del Passthrough a Color para Meta Quest 3S.
 *      Garantiza la transparencia absoluta de la cámara URP y el renderizado
 *      de Passthrough Underlay sin obstrucción del skybox virtual.
 * ---
 * [EN] Color Passthrough Manager for Meta Quest 3S.
 *      Guarantees absolute URP camera transparency and seamless Underlay
 *      passthrough rendering without virtual skybox occlusion.
 * =============================================================================
 */

using System;
using System.Collections;
using UnityEngine;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Gestor del Passthrough en Meta Quest 3S. Configura color passthrough, transparencia y pulsos de tinte.
    /// [EN] Meta Quest 3S Passthrough Manager. Configures full-color passthrough, camera transparency, and room tints.
    /// </summary>
    public class PassthroughManager : MonoBehaviour
    {
        public static PassthroughManager Instance { get; private set; }

        [Header("Configuración Passthrough")]
        [Tooltip("Capa de Passthrough de Meta XR")]
        [SerializeField] private OVRPassthroughLayer passthroughLayer;

        [Tooltip("Activar passthrough automáticamente al iniciar")]
        [SerializeField] private bool autoEnableOnStart = true;

        [Header("Estilo Visual Cyberpunk")]
        [SerializeField] private bool enableEdgeColoring = false;
        [SerializeField] private Color defaultEdgeColor = new Color(0.0f, 1.0f, 0.8f, 1.0f); // Cian neón

        private Coroutine activeTintCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (passthroughLayer == null)
            {
                passthroughLayer = GetComponent<OVRPassthroughLayer>();
                if (passthroughLayer == null)
                {
                    passthroughLayer = gameObject.AddComponent<OVRPassthroughLayer>();
                }
            }
        }

        private void Start()
        {
            if (autoEnableOnStart)
            {
                EnablePassthrough();
            }
        }

        public void EnablePassthrough()
        {
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0f, 0f, 0f, 0f);
            }

            if (passthroughLayer != null)
            {
                passthroughLayer.enabled = true;
                passthroughLayer.overlayType = OVROverlay.OverlayType.Underlay;

                if (enableEdgeColoring)
                {
                    passthroughLayer.edgeRenderingEnabled = true;
                    passthroughLayer.edgeColor = defaultEdgeColor;
                }
                else
                {
                    passthroughLayer.edgeRenderingEnabled = false;
                }
                Debug.Log("[PassthroughManager] Passthrough a color activado en Quest 3S (Fondo transparente).");
            }
        }

        public void DisablePassthrough()
        {
            if (passthroughLayer != null)
            {
                passthroughLayer.enabled = false;
            }

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.Skybox;
            }

            Debug.Log("[PassthroughManager] Passthrough desactivado (modo VR puro).");
        }

        public void TogglePassthrough()
        {
            if (passthroughLayer != null)
            {
                if (passthroughLayer.enabled)
                    DisablePassthrough();
                else
                    EnablePassthrough();
            }
        }

        /// <summary>
        /// Pulso de tinte dinámico en el passthrough (Ej: Tinte violeta cuando el jefe dispara).
        /// </summary>
        public void PulsePassthroughColorTint(Color targetColor, float duration = 2.0f)
        {
            if (passthroughLayer == null || !passthroughLayer.enabled) return;

            if (activeTintCoroutine != null)
                StopCoroutine(activeTintCoroutine);

            activeTintCoroutine = StartCoroutine(CoPulsePassthroughTint(targetColor, duration));
        }

        private IEnumerator CoPulsePassthroughTint(Color targetColor, float duration)
        {
            bool previousEdge = passthroughLayer.edgeRenderingEnabled;
            Color previousColor = passthroughLayer.edgeColor;

            passthroughLayer.edgeRenderingEnabled = true;

            float elapsed = 0f;
            float halfDuration = duration * 0.5f;

            // Subir intensidad
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                passthroughLayer.edgeColor = Color.Lerp(previousColor, targetColor, t);
                yield return null;
            }

            elapsed = 0f;
            // Bajar y restaurar
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / halfDuration);
                passthroughLayer.edgeColor = Color.Lerp(targetColor, previousColor, t);
                yield return null;
            }

            passthroughLayer.edgeRenderingEnabled = previousEdge;
            passthroughLayer.edgeColor = previousColor;
            activeTintCoroutine = null;
        }
    }
}
