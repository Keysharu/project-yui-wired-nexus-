using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectNyx.Core;
using ProjectNyx.AudioRadio;
using ProjectNyx.NetworkBridge;

namespace ProjectNyx.Gameplay
{
    [System.Serializable]
    public class BossModule
    {
        public string moduleName = "Extremidad_Holografica";
        public GameObject moduleObject;
        public float maxHealth = 100f;
        public float currentHealth = 100f;
        public bool isDestroyed = false;

        public void TakeDamage(float amount)
        {
            if (isDestroyed) return;
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                isDestroyed = true;
                if (moduleObject != null)
                {
                    moduleObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// [ES] Jefe Final Titánico: La Mente Colmena Omnipresente (Neo-Cyberpunk).
    /// En lugar de un modelo completo de 100k polígonos que sature el Quest 3S,
    /// opera como un núcleo abstracto estático anclado a la pared/techo físico
    /// con módulos independientes (escudos, extremidades, torretas) destruibles por separado.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Titanic Final Boss: The Omnipresent Hive Mind (Neo-Cyberpunk).
    /// Instead of a monolithic 100k polygon mesh that strains mobile XR,
    /// functions as a ceiling/wall-anchored abstract core with modular destructible components
    /// (deflector shields, kinetic limbs, energy turrets) that must be tactically neutralized.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public class ModularHiveMindBoss : MonoBehaviour
    {
        [Header("Núcleo Abstracto Central")]
        [Tooltip("Transform anclado al techo o la pared mediante MRUK")]
        [SerializeField] private Transform coreAnchor;
        [SerializeField] private MeshRenderer coreRenderer;
        [SerializeField] private float coreHealth = 500f;

        [Header("Módulos Destruibles Independientes")]
        [SerializeField] private List<BossModule> modules = new List<BossModule>();

        [Header("Efectos Cinemáticos en AR")]
        [Tooltip("Color del láser y del tinte passthrough del salón")]
        [SerializeField] private Color beamColor = new Color(0.85f, 0.1f, 1.0f, 1.0f); // Violeta neón
        [SerializeField] private LineRenderer massiveLaserBeam;

        private bool isBossActive = false;
        private bool isInvulnerable = true;

        private void Start()
        {
            // El núcleo es invulnerable hasta que sus módulos exteriores sean destruidos
            CheckModulesStatus();
        }

        /// <summary>
        /// Activa la batalla contra el jefe final: cambia la música a FinalBoss y avisa al BMAX.
        /// </summary>
        public void SpawnBossEncounter()
        {
            isBossActive = true;

            // 1. Música de combate Final Boss en la radio
            if (BattleMusicManager.Instance != null)
            {
                BattleMusicManager.Instance.SetBattleTier(BattleTier.FinalBoss);
            }

            // 2. Notificación en red al servidor local Homelab
            if (BmaxBridgeClient.Instance != null)
            {
                BmaxBridgeClient.Instance.SendGameEvent(
                    "FINAL_BOSS_SUMMONED",
                    "¡Alerta de distorsión! La Mente Colmena Omnipresente se ha anclado al techo de la habitación."
                );
            }

            // 3. Primer pulso de ambientación en el passthrough
            if (PassthroughManager.Instance != null)
            {
                PassthroughManager.Instance.PulsePassthroughColorTint(beamColor, 3.5f);
            }

            Debug.Log("[ModularHiveMindBoss] ¡Jefe final activado en Realidad Mixta!");
        }

        public void DamageModule(int moduleIndex, float damage)
        {
            if (moduleIndex >= 0 && moduleIndex < modules.Count)
            {
                modules[moduleIndex].TakeDamage(damage);
                CheckModulesStatus();
            }
        }

        public void DamageCore(float damage)
        {
            if (isInvulnerable)
            {
                Debug.Log("[ModularHiveMindBoss] El núcleo está protegido por el escudo de los módulos exteriores.");
                return;
            }

            coreHealth -= damage;
            Debug.Log($"[ModularHiveMindBoss] Daño al núcleo. Vida restante: {coreHealth}");

            if (coreHealth <= 0)
            {
                DefeatBoss();
            }
        }

        private void CheckModulesStatus()
        {
            bool allDestroyed = true;
            foreach (var mod in modules)
            {
                if (!mod.isDestroyed)
                {
                    allDestroyed = false;
                    break;
                }
            }

            isInvulnerable = !allDestroyed;

            if (!isInvulnerable)
            {
                Debug.Log("[ModularHiveMindBoss] ¡Todos los módulos destruidos! El núcleo cósmico ha quedado expuesto.");
                // Pulso de advertencia en passthrough
                if (PassthroughManager.Instance != null)
                {
                    PassthroughManager.Instance.PulsePassthroughColorTint(Color.red, 2.0f);
                }
            }
        }

        /// <summary>
        /// Disparo del láser colosal que tiñe digitalmente toda la habitación real.
        /// </summary>
        public void FireColossalLaser()
        {
            if (PassthroughManager.Instance != null)
            {
                PassthroughManager.Instance.PulsePassthroughColorTint(beamColor, 2.5f);
            }

            if (massiveLaserBeam != null)
            {
                massiveLaserBeam.gameObject.SetActive(true);
                StartCoroutine(CoTurnOffLaser(1.5f));
            }

            if (BmaxBridgeClient.Instance != null)
            {
                BmaxBridgeClient.Instance.SendGameEvent(
                    "BOSS_LASER_ATTACK",
                    "¡Láser orbital violeta proyectado sobre el salón real del jugador!"
                );
            }
        }

        private IEnumerator CoTurnOffLaser(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (massiveLaserBeam != null)
            {
                massiveLaserBeam.gameObject.SetActive(false);
            }
        }

        private void DefeatBoss()
        {
            isBossActive = false;
            Debug.Log("[ModularHiveMindBoss] ¡Jefe final derrotado!");

            // Volver a música normal
            if (BattleMusicManager.Instance != null)
            {
                BattleMusicManager.Instance.SetBattleTier(BattleTier.NormalBattle);
            }

            // Notificar victoria épica a Kira y Mina en la radio
            if (BmaxBridgeClient.Instance != null)
            {
                BmaxBridgeClient.Instance.SendGameEvent(
                    "FINAL_BOSS_DEFEATED",
                    "¡Victoria confirmada! El operador ha purgado la Mente Colmena y restaurado la estabilidad cuántica."
                );
            }

            gameObject.SetActive(false);
        }
    }
}
