// =============================================================================

// Autora / Author: Raquel D. Torres (El taller de la gatita tech)

// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7

// Meta Global AI Developer Hackathon 2026

// =============================================================================

// [ES] Cinematica y comportamiento de Nyx flotando en el salon o bulevar.

//      Sigue suavemente a la jugadora manteniendo una distancia de confort tactica,

//      con ondulacion de hada y soporte dual: 2.5D HD Billboard y Malla 3D Completa (Anime Heroine).

// ---

// [EN] Flight kinematics and companion behavior for Nyx across the room or boulevard.

//      Smoothly follows the player while preserving a tactical comfort distance,

//      featuring gentle fairy bobbing and dual-rendering support: 2.5D HD Billboard and Full 3D Mesh (Anime Heroine).

// =============================================================================



using System;

using System.Text;

using UnityEngine;



namespace ProjectNyx.Companion

{

    public enum NyxMoodState

    {

        Idle,

        Thinking,

        Talking,

        Alert,

        Happy

    }



    public enum NyxArchetype

    {

        TacticalAnalyst,    // Analitica, fria, probabilidades de combate (Cian Neon)

        BerserkerGlitch,    // Agresiva, adrenalina, fuego en combate (Magenta Neon)

        GuardianProtector,  // Protectora, escudos, soporte emocional (Dorado Neon)

        CuriousMaker        // Manitas, electronica, busca silicio y piezas (Verde Terminal)

    }



    [Serializable]

    public struct NyxDNA

    {

        public string id;

        public NyxArchetype archetype;

        public int bondLevel;

        public Color primaryAura;

        public Color secondaryAura;

        public bool hasFelineAntennas;

        public bool hasTacticalVisor;

    }



    /// <summary>

    /// [ES] Cinematica y comportamiento de Nyx flotando en el salon o bulevar.

    /// Sigue suavemente a la jugadora manteniendo una distancia de confort tactica en su campo de vision.

    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).

    /// ---

    /// [EN] Flight kinematics and companion behavior for Nyx across the room or boulevard.

    /// Smoothly follows the player while preserving an ergonomic tactical comfort distance within FOV.

    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).

    /// </summary>

    public class NyxFloatingCompanion : MonoBehaviour

    {

        [Header("Referencias")]

        [Tooltip("Camara del visor (CenterEyeAnchor de OVRCameraRig o Main Camera)")]

        [SerializeField] private Transform playerHead;



        [Header("Modelo 3D y 2.5D de Nyx")]

        [Tooltip("Prefabs o malla 3D de Nyx (Nyx_Companion_Anime3D.fbx)")]

        [SerializeField] private GameObject nyx3DModelPrefab;

        [SerializeField] private GameObject active3DInstance;

        [SerializeField] private GameObject sprite25DRoot;

        [SerializeField] private bool useFull3DModel = true;



        [Header("Escala y Orientacion de Malla 3D")]

        [Tooltip("[ES] Factor de escala para Nyx (0.55 = tamano de hada tactica anime) / [EN] Scale factor for Nyx (0.55 = anime tactical companion size)")]

        [SerializeField] private float companionScale = 0.55f;



        [Tooltip("[ES] Compensacion Euler para el FBX de Blender (-90 en X para verticalidad) / [EN] Euler compensation for Blender FBX (-90 on X for verticality)")]

        [SerializeField] private Vector3 modelEulerOffset = new Vector3(-90f, 0f, 0f);



        [Tooltip("[ES] Mirar a la jugadora (true) o mirar al frente en patrulla (false) / [EN] Look at player (true) or face forward in patrol (false)")]

        [SerializeField] private bool lookAtPlayer = true;



        [Tooltip("[ES] Invertir 180 grados si el modelo muestra la espalda / [EN] Invert 180 degrees if model faces backward")]

        [SerializeField] private bool invertFacingDirection = false;



        [Header("Distancia y Posicionamiento de Confort Ergonomico")]

        [SerializeField] private float preferredDistance = 1.6f;

        [SerializeField] private float rightFlankOffset = -0.55f;

        [SerializeField] private float hoverHeightOffset = -0.18f;

        [SerializeField] private bool hoverAtEyeLevel = true;

        [SerializeField] private float floorHeight = 0.05f;

        [SerializeField] private float followSpeed = 3.2f;

        [SerializeField] private float rotationSpeed = 4.5f;



        [Header("Fisica de Flotacion (Efecto Hada)")]

        [SerializeField] private float bobbingFrequency = 1.8f;

        [SerializeField] private float bobbingAmplitude = 0.025f;



        [Header("Estado Actual")]

        [SerializeField] private NyxMoodState currentState = NyxMoodState.Idle;



        private Vector3 targetPosition;

        private float bobbingTimer;



        private void Awake()

        {

            if (sprite25DRoot == null)

            {

                Transform spr = transform.Find("Nyx_Sprite25D");

                if (spr != null) sprite25DRoot = spr.gameObject;

            }



            Animator anim = GetComponent<Animator>();

            if (anim != null && anim.runtimeAnimatorController == null)

            {

                anim.enabled = false; // Desactivar animator vacio para no congelar la cinematica

            }



            if (active3DInstance == null)

            {

                Transform ex = transform.Find("Nyx_Anime3D_MeshInstance");

                if (ex != null)

                {

                    active3DInstance = ex.gameObject;

                }

                else if (GetComponentInChildren<SkinnedMeshRenderer>(true) != null || GetComponentInChildren<MeshRenderer>(true) != null)

                {

                    active3DInstance = gameObject;

                }

            }



            if (nyx3DModelPrefab == null)

            {

                nyx3DModelPrefab = Resources.Load<GameObject>("Models/Nyx_Companion_Anime3D");

            }



            // Activar modelo 3D anime por defecto

            if (useFull3DModel)

            {

                TryInitialize3DModel();

            }

            else

            {

                ApplyVisualRepresentationMode();

            }

        }



        private void Start()

        {

            if (playerHead == null && Camera.main != null)

            {

                playerHead = Camera.main.transform;

            }



            ApplyScale();

            TryInitialize3DModel();

        }



        private void Update()

        {

            if (playerHead == null && Camera.main != null)

            {

                playerHead = Camera.main.transform;

            }



            if (playerHead != null)

            {

                UpdateCompanionPosition();

                UpdateCompanionRotation();

            }



            // Toggle entre modo 2.5D HD y 3D Real con la tecla [T]

            if (Input.GetKeyDown(KeyCode.T))

            {

                ToggleVisualMode();

            }



            // Toggle de direccion de mirada (cara a cara vs escolta al frente) con la tecla [L]

            if (Input.GetKeyDown(KeyCode.L))

            {

                lookAtPlayer = !lookAtPlayer;

                Debug.Log($"[NyxFloatingCompanion] Modo de mirada cambiado: {(lookAtPlayer ? "Mirando a Raquel (Cara a cara)" : "Modo Escolta (Mirando al frente)")}");

            }

        }



        /// <summary>

        /// [ES] Inicializa y vincula la instancia de la malla 3D de Nyx con texturas URP completas.

        /// ---

        /// [EN] Initializes and binds Nyx 3D mesh instance with full URP tactical anime materials.

        /// </summary>

        public void TryInitialize3DModel()

        {

            if (active3DInstance == null)

            {

                Transform ex = transform.Find("Nyx_Anime3D_MeshInstance");

                if (ex != null)

                {

                    active3DInstance = ex.gameObject;

                }

                else if (GetComponentInChildren<SkinnedMeshRenderer>(true) != null || GetComponentInChildren<MeshRenderer>(true) != null)

                {

                    active3DInstance = gameObject;

                }

            }



            if (active3DInstance == null)

            {

                if (nyx3DModelPrefab == null)

                {

                    nyx3DModelPrefab = Resources.Load<GameObject>("Models/Nyx_Companion_Anime3D");

                }



                if (nyx3DModelPrefab != null)

                {

                    active3DInstance = Instantiate(nyx3DModelPrefab, transform);

                    active3DInstance.name = "Nyx_Anime3D_MeshInstance";

                    active3DInstance.transform.localPosition = Vector3.zero;

                    active3DInstance.transform.localRotation = Quaternion.identity;

                    active3DInstance.transform.localScale = Vector3.one * 0.85f;

                }

            }



            if (active3DInstance != null)

            {

                active3DInstance.SetActive(true);

                // Aplicar las texturas de alta fidelidad tactica de Nyx (Chaqueta cian neon, cargo pants, botas y cabello)

                ProjectNyx.Core.CyberMaterialFactory.ApplyNyxAnimeHeroineMaterials(active3DInstance);

            }



            ApplyVisualRepresentationMode();

        }



        public void ToggleVisualMode()

        {

            useFull3DModel = !useFull3DModel;

            if (useFull3DModel && active3DInstance == null)

            {

                TryInitialize3DModel();

            }

            ApplyVisualRepresentationMode();

            Debug.Log($"[NyxFloatingCompanion] Modo de visualizacion cambiado: {(useFull3DModel ? "3D Real Anime" : "2.5D HD Billboard")}");

        }



        private void ApplyVisualRepresentationMode()

        {

            if (active3DInstance != null && active3DInstance != gameObject)

            {

                active3DInstance.SetActive(useFull3DModel);

            }



            if (sprite25DRoot != null)

            {

                sprite25DRoot.SetActive(!useFull3DModel || active3DInstance == null);

            }

        }



        private void UpdateCompanionPosition()

        {

            Vector3 flatForward = playerHead.forward;

            flatForward.y = 0f;

            if (flatForward.sqrMagnitude < 0.001f) flatForward = Vector3.forward;

            flatForward.Normalize();



            Vector3 flatRight = Vector3.Cross(Vector3.up, flatForward).normalized;



            Vector3 desiredBasePos = playerHead.position + (flatForward * preferredDistance) + (flatRight * rightFlankOffset);



            // Altura de flotacion ergonomica: cerca del nivel de los ojos / pecho de la jugadora

            if (hoverAtEyeLevel)

            {

                desiredBasePos.y = playerHead.position.y + hoverHeightOffset;

            }

            else

            {

                desiredBasePos.y = floorHeight;

            }



            bobbingTimer += Time.deltaTime * bobbingFrequency;

            float verticalBob = Mathf.Abs(Mathf.Sin(bobbingTimer)) * bobbingAmplitude;

            desiredBasePos.y += verticalBob;



            float distToPlayer = Vector3.Distance(transform.position, desiredBasePos);
            float dynamicFollowSpeed = distToPlayer > 2.5f ? followSpeed * 2.2f : followSpeed;
            if (distToPlayer > 5.5f)
            {
                transform.position = desiredBasePos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, desiredBasePos, Time.deltaTime * dynamicFollowSpeed);
            }

        }



        private void UpdateCompanionRotation()

        {

            Vector3 lookDir;

            if (lookAtPlayer)

            {

                // [ES] Mirar cara a cara a la jugadora

                // [EN] Face-to-face look towards the player

                lookDir = playerHead.position - transform.position;

            }

            else

            {

                // [ES] Modo escolta tactica: mirar en la misma direccion que la jugadora

                // [EN] Tactical escort mode: face forward along player's gaze

                lookDir = playerHead.forward;

            }



            lookDir.y = 0f;



            if (lookDir.sqrMagnitude > 0.001f)

            {

                lookDir.Normalize();

                if (invertFacingDirection)

                {

                    lookDir = -lookDir;

                }



                // [ES] Compensar la inclinacion de 90 grados de Blender en el FBX para que permanezca erguida de pie

                // [EN] Compensate Blender's 90-degree pitch on the FBX so she stays fully upright on her feet

                Quaternion lookYaw = Quaternion.LookRotation(lookDir, Vector3.up);

                Quaternion targetRot = lookYaw * Quaternion.Euler(modelEulerOffset);



                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

            }

        }



        /// <summary>

        /// [ES] Ajusta la escala de Nyx para evitar proporciones gigantescas y mantener el perfil de hada tactica anime.

        /// ---

        /// [EN] Adjusts Nyx scale to prevent giant proportions and maintain anime tactical companion profile.

        /// </summary>

        public void ApplyScale()

        {

            float targetScale = 100f * companionScale;

            if (transform.localScale.x > 10f || transform.localScale.y > 10f)

            {

                transform.localScale = new Vector3(targetScale, targetScale, targetScale);

            }

            else

            {

                float baseSmall = 1.0f * companionScale;

                transform.localScale = new Vector3(baseSmall, baseSmall, baseSmall);

            }



            if (active3DInstance != null && active3DInstance != gameObject)

            {

                if (active3DInstance.transform.localScale.x > 10f)

                {

                    active3DInstance.transform.localScale = new Vector3(targetScale, targetScale, targetScale);

                }

                else

                {

                    float baseSmall = 1.0f * companionScale;

                    active3DInstance.transform.localScale = new Vector3(baseSmall, baseSmall, baseSmall);

                }

            }

        }



        public void SetMood(NyxMoodState newState)

        {

            currentState = newState;

            Debug.Log($"[NyxFloatingCompanion] Estado emocional cambiado a: {newState}");

        }



        public void SetMoodState(NyxMoodState newState)

        {

            SetMood(newState);

        }

    }

}

