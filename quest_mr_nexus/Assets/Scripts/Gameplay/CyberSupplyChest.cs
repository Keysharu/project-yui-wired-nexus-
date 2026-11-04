using System;
using System.Collections;
using UnityEngine;
using ProjectNyx.AudioRadio;

namespace ProjectNyx.Gameplay
{
    /// <summary>
    /// [ES] Fase 3 del Plan Maestro: Cofre / Caja de Suministros Cyberpunk 3D Interactiva.
    /// Aparece anclada en el suelo del salón o sobre una mesa; se abre con disparo o interacción.
    /// ---
    /// [EN] Master Plan Phase 3: Interactive 3D Cyberpunk Supply Chest.
    /// Spawns anchored to the physical floor or a scanned table; unlocks via blaster fire or proximity interaction.
    /// </summary>
    public class CyberSupplyChest : MonoBehaviour
    {
        [Header("Nodos 3D del Cofre (Spatial Ops Style)")]
        public Transform chestLidTransform;
        public GameObject holographicFlotador;
        public Light chestGlowLight;

        [Header("Parámetros de Animación")]
        public float rotationSpeed = 95f;
        public float bobbingSpeed = 2.4f;
        public float bobbingHeight = 0.05f;
        public float openSpeed = 0.45f;
        public float autoCollectRadius = 1.15f;

        private bool isOpen;
        private float rotationAngle;
        private Vector3 initialHoloLocalPos;
        private AudioSource chestAudio;
        private AudioClip unlockClip;
        private Transform playerHead;

        public bool IsOpen => isOpen;
        public event Action<CyberSupplyChest> OnChestOpened;

        private void Awake()
        {
            if (holographicFlotador != null)
            {
                initialHoloLocalPos = holographicFlotador.transform.localPosition;
            }

            chestAudio = GetComponent<AudioSource>();
            if (chestAudio == null)
            {
                chestAudio = gameObject.AddComponent<AudioSource>();
                chestAudio.spatialBlend = 1.0f;
                chestAudio.playOnAwake = false;
            }

            unlockClip = SynthSfxGenerator.CreateSupplyChestClip();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                playerHead = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (isOpen) return;

            // 1. Rotar y hacer levitar la cruz holográfica '+' (estilo Spatial Ops Imagen 5)
            if (holographicFlotador != null)
            {
                rotationAngle += rotationSpeed * Time.deltaTime;
                holographicFlotador.transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
                float bob = Mathf.Sin(Time.time * bobbingSpeed) * bobbingHeight;
                holographicFlotador.transform.localPosition = initialHoloLocalPos + new Vector3(0f, bob, 0f);
            }

            // 2. Detectar si la jugadora se acerca físicamente al cofre en la habitación
            if (playerHead == null && Camera.main != null)
            {
                playerHead = Camera.main.transform;
            }

            if (playerHead != null)
            {
                Vector2 pFlat = new Vector2(playerHead.position.x, playerHead.position.z);
                Vector2 cFlat = new Vector2(transform.position.x, transform.position.z);
                if (Vector2.Distance(pFlat, cFlat) <= autoCollectRadius)
                {
                    OpenChest();
                }
            }
        }

        /// <summary>
        /// Abre el cofre (llamado al acercarse, al recibir un disparo del Blaster o por el bucle táctico).
        /// </summary>
        public void OpenChest()
        {
            if (isOpen) return;
            isOpen = true;

            if (!gameObject.activeInHierarchy)
            {
                OnChestOpened?.Invoke(this);
                return;
            }

            if (chestAudio != null && unlockClip != null)
            {
                chestAudio.PlayOneShot(unlockClip, 0.9f);
            }

            StartCoroutine(OpenChestRoutine());
            OnChestOpened?.Invoke(this);
        }

        private IEnumerator OpenChestRoutine()
        {
            float startTime = Time.time;
            Quaternion startLidRot = chestLidTransform != null ? chestLidTransform.localRotation : Quaternion.identity;
            Quaternion targetLidRot = Quaternion.Euler(-110f, 0f, 0f);
            Vector3 startHoloScale = holographicFlotador != null ? holographicFlotador.transform.localScale : Vector3.one;

            while (Time.time - startTime < openSpeed)
            {
                float t = (Time.time - startTime) / openSpeed;
                if (chestLidTransform != null)
                {
                    chestLidTransform.localRotation = Quaternion.Slerp(startLidRot, targetLidRot, t);
                }
                if (holographicFlotador != null)
                {
                    holographicFlotador.transform.localPosition = initialHoloLocalPos + Vector3.up * (t * 0.45f);
                    holographicFlotador.transform.localScale = Vector3.Lerp(startHoloScale, Vector3.zero, t);
                }
                yield return null;
            }

            if (holographicFlotador != null)
            {
                holographicFlotador.SetActive(false);
            }

            if (chestGlowLight != null)
            {
                chestGlowLight.color = new Color(0.0f, 1.0f, 0.85f);
                chestGlowLight.intensity = 0.6f;
            }

            Debug.Log($"[CyberSupplyChest] ¡Cofre '{name}' desbloqueado! +Sobrecarga de Plasma y +50 XP.");
        }
    }
}
