using System;
using System.Collections;
using UnityEngine;
using ProjectNyx.AudioRadio;
using ProjectNyx.NetworkBridge;

namespace ProjectNyx.Companion
{
    /// <summary>
    /// [ES] Fase 2 del Montaje XR: Animador Billboard 2.5D y Soporte Táctico para Nyx.
    /// Administra animaciones HD de Nyx, estados de ánimo y rotación hacia el jugador.
    /// ---
    /// [EN] XR Staging Phase 2: 2.5D Billboard Animator and Tactical Support for Nyx.
    /// Drives Nyx HD sprite sequences, emotional moods, and continuous camera-facing alignment.
    /// </summary>
    [RequireComponent(typeof(NyxFloatingCompanion))]
    public class NyxBillboardAnimator : MonoBehaviour
    {
        [Header("Renderizador 2.5D")]
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [SerializeField] private LineRenderer supportLaserLine;

        [Header("Sprites de Expresión y Visemas (Nyx)")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite blinkSprite;
        [SerializeField] private Sprite happySprite;
        [SerializeField] private Sprite surpriseSprite;
        [SerializeField] private Sprite thinkSprite;
        [SerializeField] private Sprite[] talkSprites;
        [SerializeField] private Sprite[] walkSprites;

        [Header("Tríptico Ortogonal 360°")]
        [SerializeField] private Sprite viewFrontSprite;
        [SerializeField] private Sprite viewSideSprite;
        [SerializeField] private Sprite viewBackSprite;
        [Tooltip("8 vistas rotatorias a 45 grados (turn360_0 a turn360_7) para renderizado 2.5D HD")]
        [SerializeField] private Sprite[] turn360Sprites;

        [Header("Parámetros de Animación")]
        [SerializeField] private float blinkIntervalMin = 2.8f;
        [SerializeField] private float blinkIntervalMax = 5.5f;
        [SerializeField] private float blinkDuration = 0.14f;
        [SerializeField] private float talkFrameRate = 10f;
        [SerializeField] private float walkFrameRate = 9f;
        [SerializeField] private float movementSpeedThreshold = 0.35f;

        [Header("Soporte Táctico en Combate (Modo Remolona en Mob vs Fuego Real en Boss)")]
        [SerializeField] private bool enableTacticalSupportFire = true;
        [SerializeField] private float basicMobFireInterval = 6.5f;
        [SerializeField] private int basicMobTickleDamage = 2;
        [SerializeField] private float bossBattleFireInterval = 2.2f;
        [SerializeField] private int bossBattleSupportDamage = 16;
        [SerializeField] private float supportRange = 12f;

        private NyxFloatingCompanion floatingCompanion;
        private NyxAdaptationEngine adaptationEngine;
        private Vector3 lastPosition;
        private float currentVelocity;
        private float nextBlinkTime;
        private float blinkEndTime;
        private bool isBlinking;
        private bool isSpeaking;
        private float speakingTimer;
        private float supportFireTimer;
        private bool climaxCallTriggered;
        private bool isBossBattleMode;
        private bool announcedPlayDumbLine;

        private void Awake()
        {
            floatingCompanion = GetComponent<NyxFloatingCompanion>();
            adaptationEngine = GetComponent<NyxAdaptationEngine>();

            if (bodySpriteRenderer == null)
            {
                bodySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (turn360Sprites == null || turn360Sprites.Length < 8 || turn360Sprites[0] == null)
            {
                turn360Sprites = new Sprite[8];
                for (int i = 0; i < 8; i++)
                {
                    turn360Sprites[i] = Resources.Load<Sprite>("Sprites/Nyx/turn360_" + i);
                }
            }
        }

        private void Start()
        {
            lastPosition = transform.position;
            ScheduleNextBlink();

            if (bodySpriteRenderer != null && idleSprite != null)
            {
                bodySpriteRenderer.sprite = idleSprite;
            }
        }

        private void Update()
        {
            // Calcular velocidad de desplazamiento en el salón para activar sprites de caminata
            float dist = Vector3.Distance(transform.position, lastPosition);
            currentVelocity = dist / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPosition = transform.position;

            UpdateBlinkTimer();
            UpdateSpeakingTimer();
            UpdateSpriteSelection();

            if (enableTacticalSupportFire)
            {
                UpdateTacticalSupport();
            }
        }

        private void UpdateBlinkTimer()
        {
            if (isBlinking)
            {
                if (Time.time >= blinkEndTime)
                {
                    isBlinking = false;
                    ScheduleNextBlink();
                }
            }
            else if (Time.time >= nextBlinkTime)
            {
                isBlinking = true;
                blinkEndTime = Time.time + blinkDuration;
            }
        }

        private void ScheduleNextBlink()
        {
            nextBlinkTime = Time.time + UnityEngine.Random.Range(blinkIntervalMin, blinkIntervalMax);
        }

        private void UpdateSpeakingTimer()
        {
            if (isSpeaking)
            {
                speakingTimer -= Time.deltaTime;
                if (speakingTimer <= 0f)
                {
                    isSpeaking = false;
                    floatingCompanion.SetMood(NyxMoodState.Idle);
                }
            }
        }

        private void UpdateSpriteSelection()
        {
            if (bodySpriteRenderer == null) return;

            // 1. Si Nyx está hablando (visemas activos)
            if (isSpeaking && talkSprites != null && talkSprites.Length > 0)
            {
                int index = Mathf.FloorToInt(Time.time * talkFrameRate) % talkSprites.Length;
                if (talkSprites[index] != null)
                {
                    bodySpriteRenderer.sprite = talkSprites[index];
                    return;
                }
            }

            // 2. Si Nyx se está desplazando rápido por el salón siguiendo a Raquel
            if (currentVelocity > movementSpeedThreshold && walkSprites != null && walkSprites.Length > 0)
            {
                int index = Mathf.FloorToInt(Time.time * walkFrameRate) % walkSprites.Length;
                if (walkSprites[index] != null)
                {
                    bodySpriteRenderer.sprite = walkSprites[index];
                    return;
                }
            }

            // 3. Si está en Modo Batalla contra el Boss, adopta Pose de Combate Táctica
            if (isBossBattleMode)
            {
                if (viewSideSprite != null && Mathf.FloorToInt(Time.time * 2.5f) % 2 == 0)
                {
                    bodySpriteRenderer.sprite = viewSideSprite;
                    return;
                }
                if (surpriseSprite != null)
                {
                    bodySpriteRenderer.sprite = surpriseSprite;
                    return;
                }
            }

            // 4. Parpadeo natural en estado de reposo
            if (isBlinking && blinkSprite != null)
            {
                bodySpriteRenderer.sprite = blinkSprite;
                return;
            }

            // 5. Sistema 2.5D HD de 8 Ángulos Direccionales
            if (turn360Sprites != null && turn360Sprites.Length == 8 && turn360Sprites[0] != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 toCam = cam.transform.position - transform.position;
                    toCam.y = 0;
                    if (toCam.sqrMagnitude > 0.001f)
                    {
                        toCam.Normalize();
                        Vector3 fwd = transform.forward;
                        fwd.y = 0;
                        if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;
                        fwd.Normalize();

                        float signedAngle = Vector3.SignedAngle(fwd, toCam, Vector3.up);
                        float angle = (signedAngle + 360f) % 360f;
                        int sector = Mathf.RoundToInt(angle / 45f) % 8;

                        if (turn360Sprites[sector] != null)
                        {
                            bodySpriteRenderer.sprite = turn360Sprites[sector];
                            return;
                        }
                    }
                }
            }

            // Fallback a idleSprite
            if (idleSprite != null)
            {
                bodySpriteRenderer.sprite = idleSprite;
            }
            else if (viewFrontSprite != null)
            {
                bodySpriteRenderer.sprite = viewFrontSprite;
            }
        }

        /// <summary>
        /// Activa o desactiva la Pose de Batalla Real de Nyx para la fase del Boss.
        /// </summary>
        public void SetBossBattleStance(bool active)
        {
            isBossBattleMode = active;
            supportFireTimer = 0f;

            if (active)
            {
                floatingCompanion.SetMood(NyxMoodState.Alert);
                SpeakForDuration(4.0f, "¡Se acabó el calentamiento, Raquel! ¡Adopto pose de combate sincronizada y abro fuego pesado contra el Centurión!");
                if (bodySpriteRenderer != null)
                {
                    bodySpriteRenderer.color = new Color(1.0f, 0.92f, 0.98f, 1.0f);
                }
            }
            else
            {
                floatingCompanion.SetMood(NyxMoodState.Idle);
                if (bodySpriteRenderer != null)
                {
                    bodySpriteRenderer.color = Color.white;
                }
            }
        }

        /// <summary>
        /// Activa la animación de habla de Nyx durante los segundos indicados.
        /// </summary>
        public void SpeakForDuration(float seconds, string logMessage = null)
        {
            isSpeaking = true;
            speakingTimer = Mathf.Max(seconds, 1.0f);
            floatingCompanion.SetMood(NyxMoodState.Talking);

            if (!string.IsNullOrEmpty(logMessage))
            {
                Debug.Log($"[Nyx Voz Táctica] \"{logMessage}\"");
            }
        }

        /// <summary>
        /// Muestra una expresión puntual (Happy, Surprise, Think).
        /// </summary>
        public void TriggerExpression(NyxMoodState mood, float duration = 2.0f)
        {
            StartCoroutine(CoTemporaryExpression(mood, duration));
        }

        private IEnumerator CoTemporaryExpression(NyxMoodState mood, float duration)
        {
            floatingCompanion.SetMood(mood);
            if (bodySpriteRenderer != null)
            {
                switch (mood)
                {
                    case NyxMoodState.Happy:
                        if (happySprite != null) bodySpriteRenderer.sprite = happySprite;
                        break;
                    case NyxMoodState.Alert:
                        if (surpriseSprite != null) bodySpriteRenderer.sprite = surpriseSprite;
                        break;
                    case NyxMoodState.Thinking:
                        if (thinkSprite != null) bodySpriteRenderer.sprite = thinkSprite;
                        break;
                }
            }
            yield return new WaitForSeconds(duration);
            floatingCompanion.SetMood(NyxMoodState.Idle);
        }

        /// <summary>
        /// Soporte táctico autónomo:
        /// - En oleadas básicas: Nyx se hace un poco la remolona (disparo suave de distracción sin rematar para no robar XP).
        /// - En fase Boss: Nyx adopta pose de batalla y dispara ráfagas pesadas contra el Quantum Centurión.
        /// </summary>
        private void UpdateTacticalSupport()
        {
            float currentInterval = isBossBattleMode ? bossBattleFireInterval : basicMobFireInterval;
            supportFireTimer += Time.deltaTime;
            if (supportFireTimer < currentInterval) return;

            supportFireTimer = 0f;

            // 1. Si estamos en combate contra el Boss, prioridad absoluta de fuego real contra el Centurión
            ProjectNyx.Gameplay.QuantumCenturionBossAI boss = FindAnyObjectByType<ProjectNyx.Gameplay.QuantumCenturionBossAI>();
            if (boss != null && boss.gameObject.activeInHierarchy && !boss.IsDead)
            {
                if (!isBossBattleMode)
                {
                    SetBossBattleStance(true);
                }
                Vector3 hitPos = boss.transform.position + Vector3.up * 0.9f;
                boss.TakeDamage(bossBattleSupportDamage, hitPos);
                if (supportLaserLine != null)
                {
                    StartCoroutine(CoShowSupportLaser(transform.position + Vector3.up * 0.6f, hitPos));
                }
                return;
            }

            // 2. Enemigos básicos (Arañas): Nyx ayuda distrayéndolas pero sin farmear tu XP
            NeoCyber.AR.NeoCyberSpiderDroneAI[] enemies = FindObjectsByType<NeoCyber.AR.NeoCyberSpiderDroneAI>(FindObjectsSortMode.None);
            NeoCyber.AR.NeoCyberSpiderDroneAI closest = null;
            float minDist = supportRange;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                float d = Vector3.Distance(transform.position, enemy.transform.position);
                if (d < minDist)
                {
                    minDist = d;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                // Nunca remata a la araña básica (deja mínimo 6 HP) para que Raquel se lleve el 100% del XP
                if (closest.CurrentHealth > 6)
                {
                    closest.ApplyBlasterHit(basicMobTickleDamage, closest.transform.position);
                }

                if (supportLaserLine != null)
                {
                    StartCoroutine(CoShowSupportLaser(transform.position + Vector3.up * 0.6f, closest.transform.position));
                }

                if (!announcedPlayDumbLine)
                {
                    announcedPlayDumbLine = true;
                    SpeakForDuration(3.2f, "Jeje, solo le doy un toquecito de distracción para no robarte la experiencia de farmeo, ¡remátala tú, Raquel!");
                }
            }
        }

        private IEnumerator CoShowSupportLaser(Vector3 from, Vector3 to)
        {
            supportLaserLine.enabled = true;
            supportLaserLine.SetPosition(0, from);
            supportLaserLine.SetPosition(1, to);
            yield return new WaitForSeconds(0.08f);
            supportLaserLine.enabled = false;
        }

        /// <summary>
        /// FASE 5: Conexión cruzada Nyx -> Mina & Kira (Radio Radio Chipset).
        /// Cuando al Boss le queda poca vida (< 20%), Nyx pide a las presentadoras que suban
        /// la música al máximo (FinalBoss Tier) y lanza el evento al servidor de IA.
        /// </summary>
        public void TriggerBossClimaxRadioCall(string bossName, float healthRatio)
        {
            if (climaxCallTriggered) return;
            climaxCallTriggered = true;

            int percent = Mathf.RoundToInt(healthRatio * 100f);
            string callout = $"¡Mina, Kira! ¡Al {bossName} solo le queda un {percent}% de blindaje! ¡Poned el temazo de combate a tope en Radio Chipset que Raquel lo va a rematar!";

            SpeakForDuration(4.5f, callout);

            // 1. Subir la música a FinalBoss (Drop épico)
            if (BattleMusicManager.Instance != null)
            {
                BattleMusicManager.Instance.SetBattleTier(BattleTier.FinalBoss);
            }

            // 2. Activar visemas de celebración en el holograma de la radio
            RadioHologramDisplay radioDisplay = FindAnyObjectByType<RadioHologramDisplay>();
            if (radioDisplay != null)
            {
                radioDisplay.TriggerLiveBroadcastAnimation(6.0f);
            }

            // 3. Enviar el evento al servidor BMAX / Baseten para que Mina y Kira contesten en vivo
            if (BmaxBridgeClient.Instance != null)
            {
                string personality = adaptationEngine != null ? adaptationEngine.ExportPersonalityPromptContext() : "Nyx Táctica";
                BmaxBridgeClient.Instance.SendGameEvent(
                    "NYX_RADIO_CLIMAX_REQUEST",
                    $"[CONEXIÓN EN DIRECTO DESDE EL SALÓN] {callout} ({personality})"
                );
            }
        }

        public void ResetClimaxCallState()
        {
            climaxCallTriggered = false;
        }
    }
}
