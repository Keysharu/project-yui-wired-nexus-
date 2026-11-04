using System;
using UnityEngine;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Sistema de Efectos Volumétricos 3D, Ecualizador Holográfico y Rotación Táctica.
    /// Genera barras de ecualizador dinámicas, discos de plasma y aros de energía con shaders URP.
    /// ---
    /// [EN] 3D Volumetric Effects System, Holographic Equalizer, and Tactical Rotation.
    /// Generates dynamic equalizer bars, plasma discs, and revolving energy rings using lightweight URP shaders.
    /// </summary>
    public class CyberHologramEffects3D : MonoBehaviour
    {
        [Header("Anillos Tácticos 3D y Pedestales")]
        public float rotationSpeed = 65f;
        public Transform[] rotatingRings;

        [Header("Ecualizador 3D en Vivo (Radio Chipset)")]
        public float equalizerMaxHeight = 0.32f;
        public float equalizerMinHeight = 0.04f;
        public float equalizerFrequency = 5.5f;
        public Transform[] equalizerBars;

        [Header("Nodos Orbitales 3D")]
        public float orbitalSpeed = 90f;
        public float orbitalRadius = 0.45f;
        public Transform[] orbitalNodes;

        [Header("Rotación Volumétrica 360° (8 Ángulos)")]
        public SpriteRenderer targetSpriteRenderer;
        public Sprite[] turn360Sprites;
        public bool autoTurntablePreview = false;
        public float turntableSpeed = 0.8f;

        private void Update()
        {
            // 1. Girar anillos tácticos 3D en sentidos alternos
            if (rotatingRings != null)
            {
                for (int i = 0; i < rotatingRings.Length; i++)
                {
                    Transform ring = rotatingRings[i];
                    if (ring == null) continue;
                    float dir = (i % 2 == 0) ? 1f : -1.35f;
                    ring.Rotate(0f, rotationSpeed * dir * Time.deltaTime, 0f, Space.Self);
                }
            }

            // 2. Ecualizador 3D reactivo para la consola de Mina y Kira
            if (equalizerBars != null)
            {
                for (int i = 0; i < equalizerBars.Length; i++)
                {
                    Transform bar = equalizerBars[i];
                    if (bar == null) continue;
                    float noise = Mathf.PerlinNoise(Time.time * equalizerFrequency + i * 0.43f, i * 1.17f);
                    float h = Mathf.Lerp(equalizerMinHeight, equalizerMaxHeight, noise);
                    Vector3 sc = bar.localScale;
                    bar.localScale = new Vector3(sc.x, h, sc.z);
                    Vector3 lp = bar.localPosition;
                    bar.localPosition = new Vector3(lp.x, h * 0.5f, lp.z);
                }
            }

            // 3. Nodos de energía 3D orbitando alrededor del núcleo
            if (orbitalNodes != null && orbitalNodes.Length > 0)
            {
                for (int i = 0; i < orbitalNodes.Length; i++)
                {
                    Transform node = orbitalNodes[i];
                    if (node == null) continue;
                    float angleRad = (Time.time * orbitalSpeed + (i * (360f / orbitalNodes.Length))) * Mathf.Deg2Rad;
                    float yWave = Mathf.Sin(Time.time * 3f + i) * 0.08f;
                    node.localPosition = new Vector3(
                        Mathf.Cos(angleRad) * orbitalRadius,
                        0.85f + yWave,
                        Mathf.Sin(angleRad) * orbitalRadius
                    );
                }
            }

            // 4. Selección de ángulo 360° según la posición de la jugadora en el salón
            if (targetSpriteRenderer != null && turn360Sprites != null && turn360Sprites.Length >= 8)
            {
                if (autoTurntablePreview)
                {
                    int idx = Mathf.FloorToInt(Time.time * turntableSpeed) % turn360Sprites.Length;
                    if (turn360Sprites[idx] != null)
                    {
                        targetSpriteRenderer.sprite = turn360Sprites[idx];
                    }
                }
            }
        }
    }
}
