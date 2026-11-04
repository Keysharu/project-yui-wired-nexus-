using System;

using System.Collections;

using UnityEngine;

using ProjectNyx.Core;

using ProjectNyx.Companion;

using ProjectNyx.AudioRadio;

using ProjectNyx.NetworkBridge;



namespace ProjectNyx.Gameplay

{

    /// <summary>

    /// [ES] Fase 4 y 5 del Montaje XR: Inteligencia Artificial del Boss Quantum Centurion.

    /// Jefe bípedo acorazado con escudo frontal reflectante, teletransporte cuántico y láser orbital.

    /// ---

    /// [EN] XR Staging Phases 4 & 5: Quantum Centurion Boss AI Controller.

    /// Armored bipedal boss featuring reflective front barrier, quantum teleportation, and orbital strike phases.

    /// </summary>

    // [RequireComponent(typeof(EnemyCombatSpriteAnimator))] // Opcional para modelos 3D puros

    public class QuantumCenturionBossAI : MonoBehaviour

    {

        [Header("Estadísticas del Centurión")]

        [SerializeField] private string bossTitle = "Quantum Centurion Mk-IX";

        [SerializeField] private int maxHealth = 300;

        [SerializeField] private int currentHealth = 300;

        [SerializeField] private float climaxThresholdRatio = 0.35f;



        [Header("Movimiento y Combate")]

        [SerializeField] private float moveSpeed = 0.85f;

        [SerializeField] private float preferredCombatDistance = 4.2f;

        [SerializeField] private float attackInterval = 4.2f;

        [SerializeField] private Vector3 modelEulerOffset = Vector3.zero;

        [SerializeField] private int plasmaBeamDamage = 20;

        [SerializeField] private LineRenderer plasmaBeamRenderer;

        [SerializeField] private Color plasmaColor = new Color(1.0f, 0.15f, 0.55f, 1.0f);



        [Header("Audio y Locuciones de Clímax")]

        [SerializeField] private AudioSource bossAudioSource;

        [SerializeField] private AudioClip hitClip;

        [SerializeField] private AudioClip plasmaShotClip;

        [SerializeField] private AudioClip climaxEnergizerVoiceClip;

        [SerializeField] private AudioClip victoryRadioVoiceClip;



        private EnemyCombatSpriteAnimator spriteAnimator;

        private Transform playerHead;

        private NyxBillboardAnimator nyxAnimator;

        private NyxAdaptationEngine nyxAdaptation;

        private HoloShieldController playerShield;

        private float attackTimer;

        private bool isAttacking;

        private bool isDead;

        private bool climaxTriggered;

        private Vector3 patrolTarget;



        public int CurrentHealth => currentHealth;

        public int MaxHealth => maxHealth;

        public bool IsDead => isDead;



        private void Awake()

        {

            spriteAnimator = GetComponent<EnemyCombatSpriteAnimator>();

            currentHealth = maxHealth;



            if (bossAudioSource == null)

            {

                bossAudioSource = gameObject.AddComponent<AudioSource>();

                bossAudioSource.spatialBlend = 1.0f;

                bossAudioSource.playOnAwake = false;

            }



            if (hitClip == null) hitClip = SynthSfxGenerator.CreateEnemyHitClip();

            if (plasmaShotClip == null) plasmaShotClip = SynthSfxGenerator.CreatePlasmaBlasterClip();



            // Desactivar billboard plano si existe y activar Malla 3D del Boss

            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

            if (sr != null) sr.enabled = false;



            bool alreadyHasMesh = GetComponentInChildren<MeshRenderer>(true) != null || GetComponentInChildren<SkinnedMeshRenderer>(true) != null;

            if (!alreadyHasMesh && transform.Find("Centurion_Boss_3D_Model") == null)

            {

                GameObject boss3DPrefab = Resources.Load<GameObject>("Models/QuantumCenturion_Boss_3D");

                if (boss3DPrefab != null)

                {

                    GameObject bossInstance = Instantiate(boss3DPrefab, transform);

                    bossInstance.name = "Centurion_Boss_3D_Model";

                    bossInstance.transform.localPosition = Vector3.zero;

                    bossInstance.transform.localRotation = Quaternion.identity;

                    bossInstance.transform.localScale = Vector3.one * 1.15f;



                    // Aplicar Piel Texturizada Quantum Boss PBR y Paleta Completa sin shaders rosas

                    Texture2D texBossSkin = Resources.Load<Texture2D>("Textures/Enemies/tex_boss_centurion_skin");

                    Material bossArmorMat = ProjectNyx.Core.CyberMaterialFactory.CreatePbrMaterial("Mat_Boss_Armor_Instance", Color.white, 0.70f, 0.85f, texBossSkin);

                    Material reactorCyanMat = ProjectNyx.Core.CyberMaterialFactory.CreateEmissiveMaterial("Mat_Boss_ReactorCyan", new Color(0.0f, 1.0f, 0.95f), 4.5f);

                    Material heatSinkMat = ProjectNyx.Core.CyberMaterialFactory.CreateEmissiveMaterial("Mat_Boss_HeatSink", new Color(1.0f, 0.05f, 0.65f), 3.5f);

                    Material visorRedMat = ProjectNyx.Core.CyberMaterialFactory.CreateEmissiveMaterial("Mat_Boss_VisorRed", new Color(1.0f, 0.10f, 0.20f), 3.5f);

                    Material exhaustOrangeMat = ProjectNyx.Core.CyberMaterialFactory.CreateEmissiveMaterial("Mat_Boss_Exhaust", new Color(1.0f, 0.45f, 0.05f), 2.5f);

                    Material steelMat = ProjectNyx.Core.CyberMaterialFactory.CreatePbrMaterial("Mat_Boss_Steel", new Color(0.12f, 0.14f, 0.18f), 0.75f, 0.85f);



                    ProjectNyx.Core.CyberMaterialFactory.ApplyQuantumBossPalette(bossInstance, bossArmorMat, reactorCyanMat, heatSinkMat, visorRedMat, exhaustOrangeMat, steelMat);

                }

            }

        }



        private void OnEnable()

        {

            currentHealth = maxHealth;

            isDead = false;

            isAttacking = false;

            climaxTriggered = false;

            attackTimer = 1.5f;



            if (Camera.main != null)

            {

                playerHead = Camera.main.transform;

            }



            nyxAnimator = FindAnyObjectByType<NyxBillboardAnimator>();

            nyxAdaptation = FindAnyObjectByType<NyxAdaptationEngine>();

            playerShield = FindAnyObjectByType<HoloShieldController>();



            if (nyxAnimator != null)

            {

                nyxAnimator.ResetClimaxCallState();

            }



            PickNewPatrolPoint();



            // Activar música de Boss en Radio Chipset

            if (BattleMusicManager.Instance != null)

            {

                BattleMusicManager.Instance.SetBattleTier(BattleTier.BossLevel);

            }



            if (BmaxBridgeClient.Instance != null)

            {

                BmaxBridgeClient.Instance.SendGameEvent(

                    "BOSS_SPAWNED",

                    $"¡Alerta de incursión mayor! El {bossTitle} se ha materializado en el salón de Raquel."

                );

            }

        }



        private void Update()

        {

            if (isDead) return;



            if (playerHead == null && Camera.main != null)

            {

                playerHead = Camera.main.transform;

            }



            if (!isAttacking)

            {

                UpdateMovement();

                attackTimer += Time.deltaTime;

                if (attackTimer >= attackInterval)

                {

                    attackTimer = 0f;

                    StartPlasmaAttack();

                }

            }

        }



        private void UpdateMovement()

        {

            // [ES] Rotar hacia la jugadora suavemente para encarar el combate 3D

            // [EN] Smoothly rotate toward player for 3D combat facing

            if (playerHead != null)

            {

                Vector3 lookDir = playerHead.position - transform.position;

                lookDir.y = 0f;

                if (lookDir.sqrMagnitude > 0.001f)

                {

                    Quaternion targetRot = Quaternion.LookRotation(lookDir, Vector3.up) * Quaternion.Euler(modelEulerOffset);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 4.5f);

                }

            }



            float distToTarget = Vector3.Distance(transform.position, patrolTarget);

            if (distToTarget > 0.15f)

            {

                transform.position = Vector3.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);

                if (spriteAnimator != null) spriteAnimator.SetLocomotionState(true);

            }

            else

            {

                if (spriteAnimator != null) spriteAnimator.SetLocomotionState(false);

                PickNewPatrolPoint();

            }

        }



        private void PickNewPatrolPoint()

        {

            Vector3 center = playerHead != null ? playerHead.position : Vector3.zero;

            center.y = 0f;



            float angle = UnityEngine.Random.Range(-65f, 65f) * Mathf.Deg2Rad;

            Vector3 forward = playerHead != null ? playerHead.forward : Vector3.forward;

            forward.y = 0f;

            if (forward.sqrMagnitude < 0.01f) forward = Vector3.forward;

            forward.Normalize();



            Vector3 right = Vector3.Cross(Vector3.up, forward);

            Vector3 dir = (forward * Mathf.Cos(angle) + right * Mathf.Sin(angle)).normalized;



            patrolTarget = center + dir * preferredCombatDistance;

            patrolTarget.y = 0.05f;

        }



        private void StartPlasmaAttack()

        {

            isAttacking = true;

            if (spriteAnimator != null) spriteAnimator.SetLocomotionState(false);



            if (spriteAnimator != null)

            {

                spriteAnimator.PlayAttackSequence(0.65f, 0.45f, ExecutePlasmaBeamShot);

            }

            else

            {

                StartCoroutine(CoDelayedShoot(0.65f));

            }

            StartCoroutine(CoResetAttackFlag(1.15f));

        }



        private IEnumerator CoResetAttackFlag(float delay)

        {

            yield return new WaitForSeconds(delay);

            isAttacking = false;

        }



        

        private IEnumerator CoDelayedShoot(float delay)

        {

            yield return new WaitForSeconds(delay);

            ExecutePlasmaBeamShot();

        }

        private void ExecutePlasmaBeamShot()

        {

            if (isDead) return;



            if (bossAudioSource != null && plasmaShotClip != null)

            {

                bossAudioSource.PlayOneShot(plasmaShotClip);

            }



            Vector3 startPos = transform.position + Vector3.up * 0.9f;

            Vector3 targetPos = playerHead != null ? playerHead.position + Vector3.down * 0.15f : startPos + transform.forward * 3f;



            if (plasmaBeamRenderer != null)

            {

                StartCoroutine(CoRenderPlasmaBeam(startPos, targetPos));

            }



            if (PassthroughManager.Instance != null)

            {

                PassthroughManager.Instance.PulsePassthroughColorTint(plasmaColor, 0.8f);

            }



            // Comprobar si la jugadora ha bloqueado con el Holo-Shield izquierdo

            if (playerShield != null && playerShield.IsShieldActive)

            {

                playerShield.OnProjectileBlocked(plasmaBeamDamage);

                Debug.Log($"[{bossTitle}] ¡Disparo de plasma bloqueado por el Holo-Shield de Raquel!");

            }

            else if (nyxAdaptation != null)

            {

                nyxAdaptation.RegisterDamageTaken(plasmaBeamDamage);

            }

        }



        private IEnumerator CoRenderPlasmaBeam(Vector3 from, Vector3 to)

        {

            plasmaBeamRenderer.enabled = true;

            plasmaBeamRenderer.SetPosition(0, from);

            plasmaBeamRenderer.SetPosition(1, to);

            yield return new WaitForSeconds(0.18f);

            plasmaBeamRenderer.enabled = false;

        }



        public void TakeDamage(int damage, Vector3 hitPoint)

        {

            if (isDead) return;



            currentHealth = Mathf.Max(0, currentHealth - damage);



            if (bossAudioSource != null && hitClip != null)

            {

                bossAudioSource.PlayOneShot(hitClip);

            }



            if (!isAttacking)

            {

                if (spriteAnimator != null) spriteAnimator.PlayHitStagger(0.22f);

            }



            float healthRatio = (float)currentHealth / Mathf.Max(1, maxHealth);

            Debug.Log($"[{bossTitle}] Daño recibido: {damage} | Salud: {currentHealth}/{maxHealth} ({Mathf.RoundToInt(healthRatio * 100f)}%)");



            // FASE 5: Conexión Cruzada Nyx -> Mina & Kira al caer por debajo del 35% de vida

            if (!climaxTriggered && currentHealth > 0 && healthRatio <= climaxThresholdRatio)

            {

                climaxTriggered = true;

                if (nyxAnimator == null) nyxAnimator = FindAnyObjectByType<NyxBillboardAnimator>();

                if (nyxAnimator != null)

                {

                    nyxAnimator.TriggerBossClimaxRadioCall(bossTitle, healthRatio);

                }



                if (BattleMusicManager.Instance != null)

                {

                    BattleMusicManager.Instance.SetBattleTier(BattleTier.FinalBoss);

                    if (climaxEnergizerVoiceClip != null)

                    {

                        BattleMusicManager.Instance.PlayVoiceBroadcast(climaxEnergizerVoiceClip);

                    }

                }

            }



            if (currentHealth <= 0)

            {

                StartCoroutine(CoHandleBossDefeat());

            }

        }



        private IEnumerator CoHandleBossDefeat()

        {

            isDead = true;

            isAttacking = false;

            if (spriteAnimator != null) spriteAnimator.PlayDeathCollapse();



            Debug.Log($"[{bossTitle}] ¡Colapso crítico! Boss neutralizado en Realidad Mixta.");



            if (nyxAnimator != null)

            {

                nyxAnimator.TriggerExpression(NyxMoodState.Happy, 4.0f);

                nyxAnimator.SpeakForDuration(3.5f, "¡Objetivo abatido! ¡Menuda puntería, Mayor! Mina, Kira, dadle los honores por antena.");

            }



            if (BattleMusicManager.Instance != null)

            {

                BattleMusicManager.Instance.SetBattleTier(BattleTier.Peaceful);

                if (victoryRadioVoiceClip != null)

                {

                    BattleMusicManager.Instance.PlayVoiceBroadcast(victoryRadioVoiceClip);

                }

            }



            if (BmaxBridgeClient.Instance != null)

            {

                BmaxBridgeClient.Instance.SendGameEvent(

                    "BOSS_DEFEATED",

                    $"¡Victoria rotunda en el salón! Raquel y Nyx han destruido al {bossTitle}."

                );

            }



            yield return new WaitForSeconds(2.5f);



            gameObject.SetActive(false);

        }

    }

}

