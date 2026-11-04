using System;
using System.Collections;
using UnityEngine;

namespace ProjectNyx.Gameplay
{
    public enum EnemyAnimState
    {
        Idle,
        Moving,
        AttackCharge,
        AttackStrike,
        HitStagger,
        DeathCollapse
    }

    /// <summary>
    /// [ES] Fase 4 del Montaje XR: Controlador de Animación 2.5D para Enemigos y Jefes.
    /// Reproduce los 7 fotogramas tácticos extraídos de Google Flow (Idle, Caminata 1-2,
    /// Carga de Ataque, Impacto/Disparo, Stagger por Daño y Colapso de Muerte) manteniendo
    /// orientación Billboard hacia el visor Meta Quest 3S.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] XR Staging Phase 4: 2.5D Combat Animation Controller for Enemies & Bosses.
    /// Plays the 7 tactical frames extracted from Google Flow (Idle, Walk 1-2,
    /// Attack Charge, Strike/Shoot, Damage Stagger, and Death Collapse) while maintaining
    /// Billboard facing orientation toward the Meta Quest 3S headset.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public class EnemyCombatSpriteAnimator : MonoBehaviour
    {
        [Header("Renderizador de Sprite")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Fotogramas de Combate (Google Flow)")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite moveSprite1;
        [SerializeField] private Sprite moveSprite2;
        [SerializeField] private Sprite attackChargeSprite;
        [SerializeField] private Sprite attackStrikeSprite;
        [SerializeField] private Sprite hitStaggerSprite;
        [SerializeField] private Sprite deathCollapseSprite;

        [Header("Opciones de Orientación")]
        [SerializeField] private bool billboardToPlayer = true;
        [SerializeField] private bool lockVerticalAxis = true;
        [SerializeField] private float moveCycleSpeed = 6.5f;

        private EnemyAnimState currentState = EnemyAnimState.Idle;
        private Transform playerHead;
        private Coroutine activeTransientRoutine;
        private Color baseColor = Color.white;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                playerHead = Camera.main.transform;
            }
            ApplyCurrentStateSprite();
        }

        private void Update()
        {
            if (billboardToPlayer)
            {
                UpdateBillboard();
            }

            if (currentState == EnemyAnimState.Moving)
            {
                UpdateMoveCycle();
            }
        }

        private void UpdateBillboard()
        {
            if (playerHead == null)
            {
                if (Camera.main != null) playerHead = Camera.main.transform;
                else return;
            }

            if (spriteRenderer == null) return;

            Vector3 dir = playerHead.position - spriteRenderer.transform.position;
            if (lockVerticalAxis)
            {
                dir.y = 0f;
            }

            if (dir.sqrMagnitude > 0.001f)
            {
                spriteRenderer.transform.rotation = Quaternion.LookRotation(-dir);
            }
        }

        private void UpdateMoveCycle()
        {
            if (spriteRenderer == null) return;

            bool firstFrame = Mathf.FloorToInt(Time.time * moveCycleSpeed) % 2 == 0;
            Sprite chosen = firstFrame ? moveSprite1 : moveSprite2;
            if (chosen != null)
            {
                spriteRenderer.sprite = chosen;
            }
            else if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }

        public void SetLocomotionState(bool isMoving)
        {
            if (currentState == EnemyAnimState.DeathCollapse ||
                currentState == EnemyAnimState.AttackCharge ||
                currentState == EnemyAnimState.AttackStrike ||
                currentState == EnemyAnimState.HitStagger)
            {
                return;
            }

            currentState = isMoving ? EnemyAnimState.Moving : EnemyAnimState.Idle;
            if (!isMoving)
            {
                ApplyCurrentStateSprite();
            }
        }

        public void PlayAttackSequence(float chargeTime, float strikeTime, Action onStrikeCallback = null)
        {
            if (currentState == EnemyAnimState.DeathCollapse) return;

            if (activeTransientRoutine != null)
            {
                StopCoroutine(activeTransientRoutine);
            }
            activeTransientRoutine = StartCoroutine(CoAttackSequence(chargeTime, strikeTime, onStrikeCallback));
        }

        private IEnumerator CoAttackSequence(float chargeTime, float strikeTime, Action onStrikeCallback)
        {
            currentState = EnemyAnimState.AttackCharge;
            ApplyCurrentStateSprite();
            yield return new WaitForSeconds(chargeTime);

            currentState = EnemyAnimState.AttackStrike;
            ApplyCurrentStateSprite();
            onStrikeCallback?.Invoke();
            yield return new WaitForSeconds(strikeTime);

            currentState = EnemyAnimState.Idle;
            ApplyCurrentStateSprite();
            activeTransientRoutine = null;
        }

        public void PlayHitStagger(float staggerDuration = 0.25f)
        {
            if (currentState == EnemyAnimState.DeathCollapse) return;

            if (activeTransientRoutine != null)
            {
                StopCoroutine(activeTransientRoutine);
            }
            activeTransientRoutine = StartCoroutine(CoHitStagger(staggerDuration));
        }

        private IEnumerator CoHitStagger(float staggerDuration)
        {
            currentState = EnemyAnimState.HitStagger;
            ApplyCurrentStateSprite();

            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
            }

            yield return new WaitForSeconds(staggerDuration);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
            }

            currentState = EnemyAnimState.Idle;
            ApplyCurrentStateSprite();
            activeTransientRoutine = null;
        }

        public void PlayDeathCollapse()
        {
            if (activeTransientRoutine != null)
            {
                StopCoroutine(activeTransientRoutine);
                activeTransientRoutine = null;
            }

            currentState = EnemyAnimState.DeathCollapse;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = baseColor;
                if (deathCollapseSprite != null)
                {
                    spriteRenderer.sprite = deathCollapseSprite;
                }
            }
        }

        private void ApplyCurrentStateSprite()
        {
            if (spriteRenderer == null) return;

            switch (currentState)
            {
                case EnemyAnimState.Idle:
                    if (idleSprite != null) spriteRenderer.sprite = idleSprite;
                    break;
                case EnemyAnimState.Moving:
                    if (moveSprite1 != null) spriteRenderer.sprite = moveSprite1;
                    break;
                case EnemyAnimState.AttackCharge:
                    if (attackChargeSprite != null) spriteRenderer.sprite = attackChargeSprite;
                    break;
                case EnemyAnimState.AttackStrike:
                    if (attackStrikeSprite != null) spriteRenderer.sprite = attackStrikeSprite;
                    break;
                case EnemyAnimState.HitStagger:
                    if (hitStaggerSprite != null) spriteRenderer.sprite = hitStaggerSprite;
                    break;
                case EnemyAnimState.DeathCollapse:
                    if (deathCollapseSprite != null) spriteRenderer.sprite = deathCollapseSprite;
                    break;
            }
        }
    }
}
