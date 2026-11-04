using System.Collections;



using UnityEngine;



using Meta.XR.MRUtilityKit;



using ProjectNyx.Gameplay;



using ProjectNyx.AudioRadio;







namespace NeoCyber.AR



{



    public enum NeoCyberEnemyType



    {



        Spider, // crawls along the scanned walls and approaches player



        Drone   // hovers and repositions over the open floor



    }







    /// <summary>



    /// [ES] IA Enemiga de Trinchera: Drones y Arañas Cibernéticas sobre Malla MRUK.



    /// Utiliza las superficies físicas escaneadas (paredes y suelo) del salón real para trepar



    /// y maniobrar con fallback automático si MRUK está inicializando, anima sprites 2.5D



    /// de Google Flow con orientación Billboard y ejecuta disparos de plasma y ataques cuerpo a cuerpo.



    /// ---



    /// [EN] Tactical Enemy AI: Cyber Spiders and Drones on MRUK Physical Surfaces.



    /// Traverses scanned room surfaces (walls and floor) in the player's physical space



    /// with automatic fallback during MRUK initialization, animates Google Flow 2.5D sprites



    /// with billboard rotation, and executes ranged plasma shots and close-quarters melee strikes.



    /// </summary>



    [RequireComponent(typeof(Rigidbody))]



    public class NeoCyberSpiderDroneAI : MonoBehaviour



    {



        [Header("Enemy type")]



        [SerializeField] private NeoCyberEnemyType enemyType = NeoCyberEnemyType.Spider;







        [Header("Orientacion y Offset 3D")]
        [Tooltip("[ES] Compensacion Euler de rotacion adicional (Vector3.zero para postura horizontal sobre sus patas) / [EN] Additional Euler rotation offset (Vector3.zero for horizontal stance on legs)")]
        [SerializeField] private Vector3 modelEulerOffset = Vector3.zero;



        [Header("Movement")]



        [SerializeField] private float moveSpeed = 1.4f;



        [SerializeField] private float rotationSpeed = 6f;



        [SerializeField] private float repositionInterval = 4f;



        [SerializeField] private float droneFlightHeight = 1.5f;



        [SerializeField] private float minDistanceToSurfaceEdge = 0.15f;







        [Header("Combat & Animation")]



        [SerializeField] private int maxHealth = 40;



        [SerializeField] private int attackDamage = 8;



        [SerializeField] private float attackInterval = 3.5f;



        [SerializeField] private EnemyCombatSpriteAnimator spriteAnimator;



        [SerializeField] private LineRenderer attackLaserRenderer;



        [SerializeField] private GameObject deathVfxPrefab;



        [SerializeField] private GameObject hitVfxPrefab;







        private int currentHealth;



        private Vector3 targetPosition;



        private Vector3 targetNormal = Vector3.up;



        private Coroutine repositionRoutine;



        private bool hasTarget;



        private bool isDying;



        private float attackTimer;



        private AudioSource enemyAudio;



        private AudioClip hitSfx;



        private AudioClip attackSfx;







        public int CurrentHealth => currentHealth;



        public bool IsDying => isDying;







        private void Awake()



        {



            if (spriteAnimator == null)



            {



                spriteAnimator = GetComponent<EnemyCombatSpriteAnimator>();



            }







            enemyAudio = GetComponent<AudioSource>();



            if (enemyAudio == null)



            {



                enemyAudio = gameObject.AddComponent<AudioSource>();



                enemyAudio.spatialBlend = 1.0f;



                enemyAudio.playOnAwake = false;



            }







            hitSfx = SynthSfxGenerator.CreateEnemyHitClip();



            attackSfx = SynthSfxGenerator.CreatePlasmaBlasterClip();







            // Asegurar que el enemigo use la Malla 3D Mecha pura



            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();



            if (sr != null) sr.enabled = false;







            bool alreadyHasMesh = GetComponentInChildren<MeshRenderer>(true) != null || GetComponentInChildren<SkinnedMeshRenderer>(true) != null;



            if (!alreadyHasMesh && transform.Find("Spider_3D_Mesh_Model") == null)



            {



                GameObject model3D = Resources.Load<GameObject>("Models/SpiderDrone_Mecha_3D");



                if (model3D != null)



                {



                    GameObject instance = Instantiate(model3D, transform);



                    instance.name = "Spider_3D_Mesh_Model";



                    instance.transform.localPosition = Vector3.zero;



                    instance.transform.localRotation = Quaternion.identity;



                    instance.transform.localScale = Vector3.one * 0.75f;







                    // Aplicar Piel Texturizada Mecha PBR y Paleta Completa sin shaders rosas



                    Texture2D texSkin = Resources.Load<Texture2D>("Textures/Enemies/tex_spider_mecha_skin");



                    Material skinMat = ProjectNyx.Core.CyberMaterialFactory.CreatePbrMaterial("Mat_Spider_Skin_Instance", Color.white, 0.65f, 0.75f, texSkin);



                    Material eyeCyanMat = ProjectNyx.Core.CyberMaterialFactory.CreateEmissiveMaterial("Mat_Spider_EyeCyan", new Color(0.0f, 1.0f, 0.88f), 3.5f);



                    Material eyeAmberMat = ProjectNyx.Core.CyberMaterialFactory.CreateEmissiveMaterial("Mat_Spider_EyeAmber", new Color(1.0f, 0.60f, 0.0f), 2.5f);



                    Material steelMat = ProjectNyx.Core.CyberMaterialFactory.CreatePbrMaterial("Mat_Spider_Steel", new Color(0.18f, 0.20f, 0.24f), 0.85f, 0.90f);



                    Material nitrideMat = ProjectNyx.Core.CyberMaterialFactory.CreatePbrMaterial("Mat_Spider_Nitride", new Color(0.85f, 0.65f, 0.15f), 0.90f, 0.85f);







                    ProjectNyx.Core.CyberMaterialFactory.ApplyMechaSpiderPalette(instance, skinMat, eyeCyanMat, eyeAmberMat, steelMat, nitrideMat);



                }



            }



        }







        private void Start()



        {



            currentHealth = maxHealth;



            targetPosition = transform.position;







            if (TryPickNewTarget())



            {



                hasTarget = true;



            }







            repositionRoutine = StartCoroutine(RepositionLoop());



        }







        private void OnDestroy()



        {



            if (repositionRoutine != null)



            {



                StopCoroutine(repositionRoutine);



            }



        }







        private IEnumerator RepositionLoop()



        {



            var wait = new WaitForSeconds(repositionInterval);



            while (!isDying)



            {



                yield return wait;



                if (!isDying)



                {



                    TryPickNewTarget();



                }



            }



        }







        private bool TryPickNewTarget()



        {



            MRUKRoom room = MRUK.Instance != null ? MRUK.Instance.GetCurrentRoom() : null;



            if (room != null)



            {



                Vector3 position;



                Vector3 normal;



                bool found;







                if (enemyType == NeoCyberEnemyType.Spider)



                {



                    found = room.GenerateRandomPositionOnSurface(



                        MRUK.SurfaceType.VERTICAL,



                        minDistanceToSurfaceEdge,



                        LabelFilter.Included(MRUKAnchor.SceneLabels.WALL_FACE),



                        out position,



                        out normal);



                }



                else



                {



                    found = room.GenerateRandomPositionOnSurface(



                        MRUK.SurfaceType.FACING_UP,



                        minDistanceToSurfaceEdge,



                        LabelFilter.Included(MRUKAnchor.SceneLabels.FLOOR),



                        out position,



                        out normal);



                }







                if (found)



                {



                    targetNormal = normal;



                    targetPosition = enemyType == NeoCyberEnemyType.Drone



                        ? position + normal * droneFlightHeight



                        : position + normal * 0.04f;







                    hasTarget = true;



                    return true;



                }



            }







            // Fallback espacial alrededor de la jugadora si la habitación MRUK aún no está cargada



            Transform cam = Camera.main != null ? Camera.main.transform : null;



            Vector3 center = cam != null ? cam.position : Vector3.zero;



            float angle = Random.Range(0f, Mathf.PI * 2f);



            float radius = Random.Range(1.6f, 3.2f);



            float height = enemyType == NeoCyberEnemyType.Spider ? Random.Range(0.2f, 1.7f) : droneFlightHeight;







            targetNormal = Vector3.up;



            targetPosition = new Vector3(



                center.x + Mathf.Cos(angle) * radius,



                height,



                center.z + Mathf.Sin(angle) * radius



            );



            hasTarget = true;



            return true;



        }







        private void Update()



        {



            if (isDying) return;







            if (hasTarget)



            {



                float dist = Vector3.Distance(transform.position, targetPosition);



                bool moving = dist > 0.05f;







                if (moving)



                {



                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);



                }







                if (spriteAnimator != null)



                {



                    spriteAnimator.SetLocomotionState(moving);



                }







                Vector3 toTarget = targetPosition - transform.position;



                Quaternion desiredRotation;







                if (enemyType == NeoCyberEnemyType.Spider && targetNormal != Vector3.up)



                {



                    Vector3 forwardOnWall = Vector3.ProjectOnPlane(toTarget.sqrMagnitude > 0.0001f ? toTarget : transform.forward, targetNormal);



                    if (forwardOnWall.sqrMagnitude < 0.0001f)



                    {



                        forwardOnWall = Vector3.ProjectOnPlane(transform.forward, targetNormal);



                    }



                    desiredRotation = Quaternion.LookRotation(forwardOnWall, targetNormal) * Quaternion.Euler(modelEulerOffset);



                }



                else



                {



                    Vector3 forward = toTarget.sqrMagnitude > 0.0001f ? toTarget : transform.forward;



                    desiredRotation = Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(modelEulerOffset);



                }







                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);



            }







            attackTimer += Time.deltaTime;



            if (attackTimer >= attackInterval)



            {



                attackTimer = 0f;



                PerformCombatAttack();



            }



        }







        private void PerformCombatAttack()



        {



            if (isDying) return;







            if (spriteAnimator != null)



            {



                spriteAnimator.PlayAttackSequence(0.35f, 0.30f, FireSpiderStrike);



            }



            else



            {



                FireSpiderStrike();



            }



        }







        private void FireSpiderStrike()



        {



            if (isDying) return;







            if (enemyAudio != null && attackSfx != null)



            {



                enemyAudio.PlayOneShot(attackSfx, 0.55f);



            }







            Transform cam = Camera.main != null ? Camera.main.transform : null;



            Vector3 targetPos = cam != null ? cam.position + Vector3.down * 0.2f : transform.position + transform.forward * 2f;







            if (attackLaserRenderer != null)



            {



                StartCoroutine(CoShowAttackPulse(transform.position, targetPos));



            }







            HoloShieldController shield = FindAnyObjectByType<HoloShieldController>();



            if (shield != null && shield.IsShieldActive)



            {



                shield.OnProjectileBlocked(attackDamage);



            }



        }







        private IEnumerator CoShowAttackPulse(Vector3 from, Vector3 to)



        {



            attackLaserRenderer.enabled = true;



            attackLaserRenderer.SetPosition(0, from);



            attackLaserRenderer.SetPosition(1, to);



            yield return new WaitForSeconds(0.10f);



            attackLaserRenderer.enabled = false;



        }







        public void ApplyBlasterHit(int damage, Vector3 hitPoint)



        {



            if (isDying || currentHealth <= 0)



            {



                return;



            }







            currentHealth -= damage;







            if (enemyAudio != null && hitSfx != null)



            {



                enemyAudio.PlayOneShot(hitSfx, 0.8f);



            }







            if (hitVfxPrefab != null)



            {



                Instantiate(hitVfxPrefab, hitPoint, Quaternion.identity);



            }







            if (currentHealth <= 0)



            {



                StartCoroutine(CoDieSequence());



                return;



            }







            if (spriteAnimator != null)



            {



                spriteAnimator.PlayHitStagger(0.20f);



            }







            TryPickNewTarget();



        }







        private IEnumerator CoDieSequence()



        {



            isDying = true;



            if (spriteAnimator != null)



            {



                spriteAnimator.PlayDeathCollapse();



            }







            if (deathVfxPrefab != null)



            {



                Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);



            }







            yield return new WaitForSeconds(0.65f);



            Destroy(gameObject);



        }







        private void OnTriggerEnter(Collider other)



        {



            var projectile = other.GetComponentInParent<IBlasterProjectile>();



            if (projectile == null)



            {



                return;



            }







            ApplyBlasterHit(projectile.Damage, other.ClosestPoint(transform.position));



            projectile.OnHitTarget();



        }



    }







    public interface IBlasterProjectile



    {



        int Damage { get; }



        void OnHitTarget();



    }



}



