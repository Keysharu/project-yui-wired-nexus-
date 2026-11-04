using System;
using UnityEngine;
using ProjectNyx.NetworkBridge;
using ProjectNyx.Companion;

namespace ProjectNyx.Gameplay
{
    /// <summary>
    /// [ES] Pieza 5 del Puzzle: Grieta dimensional o anomalía de código anclada a la pared.
    /// Administra partículas de plasma, apertura de vórtice y ciclo de vida de la grieta cuántica.
    /// ---
    /// [EN] Puzzle Piece 5: Dimensional Code Rift Anchored to Physical Wall.
    /// Manages plasma particles, vortex expansion, and quantum rift lifecycle in physical space.
    /// </summary>
    public class DimensionalRift : MonoBehaviour
    {
        [Header("Estadísticas")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private string riftName = "Brecha Cuántica Sector Salón";
        [SerializeField] private int currentHealth;

        [Header("Vínculo con Nyx")]
        [SerializeField] private NyxAdaptationEngine nyxAdaptation;

        [Header("Efectos")]
        [SerializeField] private ParticleSystem collapseParticles;
        [SerializeField] private AudioSource riftAudioSource;
        [SerializeField] private AudioClip defeatClip;

        private bool isDestroyed = false;

        private void Start()
        {
            currentHealth = maxHealth;
            if (nyxAdaptation == null)
            {
                nyxAdaptation = FindAnyObjectByType<NyxAdaptationEngine>();
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDestroyed) return;

            currentHealth -= damage;
            Debug.Log($"[DimensionalRift] '{riftName}' recibió {damage} de daño. Vida restante: {currentHealth}");

            if (currentHealth <= 0)
            {
                DefeatRift();
            }
        }

        private void DefeatRift()
        {
            isDestroyed = true;
            Debug.Log($"[DimensionalRift] ¡'{riftName}' ha sido neutralizada con éxito!");

            // 1. Notificar a Nyx para evolucionar vínculo y arquetipo
            if (nyxAdaptation != null)
            {
                nyxAdaptation.RegisterRiftSealed();
            }

            // 2. Notificar al puente del BMAX para que la radio lo comente en vivo
            if (BmaxBridgeClient.Instance != null)
            {
                BmaxBridgeClient.Instance.SendGameEvent(
                    "game_event",
                    $"La operadora Raquel ha sellado una {riftName} en combate de Realidad Mixta usando su arma de energía."
                );
            }

            // 3. Efectos visuales de colapso
            if (collapseParticles != null)
            {
                collapseParticles.transform.parent = null;
                collapseParticles.Play();
                Destroy(collapseParticles.gameObject, 2.0f);
            }

            // 4. Sonido de victoria
            if (riftAudioSource != null && defeatClip != null)
            {
                AudioSource.PlayClipAtPoint(defeatClip, transform.position);
            }

            Destroy(gameObject, 0.1f);
        }
    }
}
