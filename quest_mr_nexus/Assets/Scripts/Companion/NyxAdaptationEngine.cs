using System;
using System.Text;
using UnityEngine;

namespace ProjectNyx.Companion
{
    public enum NyxActivityLevel
    {
        Passive_Quiet,      // Modo discreto/silencioso: Solo interviene en alertas críticas de salud o brechas
        Balanced_Standard,  // Modo equilibrado: Asistencia táctica, comentarios de combate moderados
        Tactical_Proactive  // Modo proactivo/activo: Interacción frecuente, sugerencias tácticas y compañía constante
    }

    /// <summary>
    /// [ES] Motor de Adaptación Emocional, Nivel de Actividad y Personalización Táctica de Nyx.
    /// Evalúa la precisión de disparo, ritmo de combate y afinidad para modular el diálogo y aura.
    /// ---
    /// [EN] Nyx Emotional Adaptation, Combat Pacing, and Tactical Personality Engine.
    /// Evaluates player marksmanship, combat tempo, and bond level to dynamically adapt dialogue and visual aura.
    /// </summary>
    public class NyxAdaptationEngine : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private NyxFloatingCompanion companionController;
        [SerializeField] private Renderer holographicRenderer;

        [Header("Nivel de Actividad e Interacción del Usuario")]
        [Tooltip("Configura la frecuencia y proactividad con la que Nyx interviene e interactúa con el jugador.")]
        [SerializeField] private NyxActivityLevel activityLevel = NyxActivityLevel.Balanced_Standard;

        [Header("Telemetría en Vivo")]
        [SerializeField] private int totalShotsFired = 0;
        [SerializeField] private int totalRiftsSealed = 0;
        [SerializeField] private float physicalMovementDistance = 0f;
        [SerializeField] private int damageTaken = 0;

        [Header("Evolución de Personalidad")]
        [SerializeField] private NyxArchetype currentArchetype = NyxArchetype.TacticalAnalyst;
        [Range(1, 100)] [SerializeField] private int bondLevel = 1;

        [Header("Accesorios Holográficos Únicos")]
        [SerializeField] private GameObject felineAntennasRoot;
        [SerializeField] private GameObject tacticalVisorRoot;
        [SerializeField] private ParticleSystem auraParticles;

        private Vector3 lastHeadPosition;
        private Transform playerHead;
        private float telemetrySampleTimer = 0f;
        private Material runtimeMaterial;

        // Propiedad pública del ADN de Nyx
        public NyxDNA DNA { get; private set; }

        public NyxActivityLevel ActivityLevel => activityLevel;

        private void Awake()
        {
            if (companionController == null)
            {
                companionController = GetComponent<NyxFloatingCompanion>();
            }

            GenerateInitialDNA();
        }

        private void Start()
        {
            if (Camera.main != null)
            {
                playerHead = Camera.main.transform;
                lastHeadPosition = playerHead.position;
            }

            ApplyVisualArchetype(currentArchetype);
        }

        private void Update()
        {
            SamplePlayerTelemetry();

            // Atajo de teclado en Desktop para alternar el nivel de actividad de Nyx rápidamente
            if (Input.GetKeyDown(KeyCode.N))
            {
                CycleNextActivityLevel();
            }
        }

        /// <summary>
        /// Permite alternar o configurar el nivel de interacción de Nyx (Discreto, Equilibrado, Proactivo).
        /// </summary>
        public void SetActivityLevel(NyxActivityLevel newLevel)
        {
            activityLevel = newLevel;
            Debug.Log($"[NyxAdaptationEngine] Nivel de actividad de Nyx cambiado a: {activityLevel}");
        }

        public void CycleNextActivityLevel()
        {
            int next = ((int)activityLevel + 1) % 3;
            SetActivityLevel((NyxActivityLevel)next);
        }

        private void SamplePlayerTelemetry()
        {
            if (playerHead == null) return;

            telemetrySampleTimer += Time.deltaTime;
            if (telemetrySampleTimer >= 1.0f)
            {
                telemetrySampleTimer = 0f;
                float distMoved = Vector3.Distance(playerHead.position, lastHeadPosition);
                physicalMovementDistance += distMoved;
                lastHeadPosition = playerHead.position;

                EvaluateArchetypeShift();
            }
        }

        public void RecordShotFired()
        {
            totalShotsFired++;
            IncreaseBond(1);
        }

        public void RegisterShot() => RecordShotFired();

        public void RecordRiftSealed()
        {
            totalRiftsSealed++;
            IncreaseBond(5);
        }

        public void RegisterRiftSealed() => RecordRiftSealed();

        public void RecordDamageTaken(int amount)
        {
            damageTaken += amount;
            if (activityLevel != NyxActivityLevel.Passive_Quiet)
            {
                // Avisar en niveles moderado o activo
                companionController?.SetMoodState(NyxMoodState.Alert);
            }
        }

        public void RegisterDamageTaken(int amount) => RecordDamageTaken(amount);

        private void EvaluateArchetypeShift()
        {
            NyxArchetype targetArchetype = currentArchetype;

            if (totalShotsFired > 30 && damageTaken < 20)
            {
                targetArchetype = NyxArchetype.TacticalAnalyst;
            }
            else if (damageTaken > 40)
            {
                targetArchetype = NyxArchetype.BerserkerGlitch;
            }
            else if (physicalMovementDistance > 15f)
            {
                targetArchetype = NyxArchetype.GuardianProtector;
            }
            else if (totalRiftsSealed >= 2)
            {
                targetArchetype = NyxArchetype.CuriousMaker;
            }

            if (targetArchetype != currentArchetype)
            {
                currentArchetype = targetArchetype;
                ApplyVisualArchetype(currentArchetype);
                Debug.Log($"[NyxAdaptationEngine] ¡La personalidad de Nyx ha evolucionado a: {currentArchetype}!");
            }
        }

        private void ApplyVisualArchetype(NyxArchetype archetype)
        {
            Color auraColor = Color.cyan;

            switch (archetype)
            {
                case NyxArchetype.TacticalAnalyst:
                    auraColor = new Color(0.0f, 1.0f, 0.88f); // Cian Neón
                    if (tacticalVisorRoot != null) tacticalVisorRoot.SetActive(true);
                    if (felineAntennasRoot != null) felineAntennasRoot.SetActive(false);
                    break;
                case NyxArchetype.BerserkerGlitch:
                    auraColor = new Color(1.0f, 0.0f, 0.5f); // Magenta Glitch
                    if (tacticalVisorRoot != null) tacticalVisorRoot.SetActive(false);
                    if (felineAntennasRoot != null) felineAntennasRoot.SetActive(false);
                    break;
                case NyxArchetype.GuardianProtector:
                    auraColor = new Color(1.0f, 0.84f, 0.0f); // Dorado Neón
                    if (tacticalVisorRoot != null) tacticalVisorRoot.SetActive(false);
                    if (felineAntennasRoot != null) felineAntennasRoot.SetActive(true);
                    break;
                case NyxArchetype.CuriousMaker:
                    auraColor = new Color(0.22f, 1.0f, 0.08f); // Verde Terminal
                    if (tacticalVisorRoot != null) tacticalVisorRoot.SetActive(true);
                    if (felineAntennasRoot != null) felineAntennasRoot.SetActive(true);
                    break;
            }

            if (auraParticles != null)
            {
                var main = auraParticles.main;
                main.startColor = auraColor;
            }
        }

        private void GenerateInitialDNA()
        {
            DNA = new NyxDNA
            {
                id = "NYX-" + UnityEngine.Random.Range(1000, 9999),
                archetype = currentArchetype,
                bondLevel = bondLevel,
                primaryAura = new Color(0.0f, 1.0f, 0.8f),
                secondaryAura = Color.blue,
                hasFelineAntennas = false,
                hasTacticalVisor = false
            };
            Debug.Log($"[NyxAdaptationEngine] Nyx nacida con identidad única de ADN: {DNA.id} | Nivel de Actividad: {activityLevel}");
        }

        public void IncreaseBond(int points)
        {
            bondLevel = Mathf.Clamp(bondLevel + points, 1, 100);
        }

        public void OnPartyNyxDetected(NyxDNA otherNyx)
        {
            Debug.Log($"[Nyx Party Mode] ¡Nyx detectó a otra compañera en el salón! ID: {otherNyx.id} | Arquetipo: {otherNyx.archetype}");

            if (otherNyx.archetype == NyxArchetype.BerserkerGlitch && currentArchetype == NyxArchetype.TacticalAnalyst)
            {
                Debug.Log("[Nyx Party Mode] Combinación perfecta: Analista + Berserker. +20% Buff de daño de equipo.");
            }
        }

        public string ExportPersonalityPromptContext()
        {
            string activityPrompt = activityLevel switch
            {
                NyxActivityLevel.Passive_Quiet => "Nivel de Interacción: SILENCIOSO/PASIVO. Solo hablas si la jugadora te pregunta directamente o si la situación es crítica de vida o muerte. Sé breve, precisa y no interrumpas.",
                NyxActivityLevel.Balanced_Standard => "Nivel de Interacción: EQUILIBRADO/TÁCTICO. Ofreces soporte táctico oportuno, reportes de brechas y comentarios motivadores cuando corresponda.",
                NyxActivityLevel.Tactical_Proactive => "Nivel de Interacción: PROACTIVO/ACTIVO. Eres muy comunicativa, atenta a cada detalle del entorno, compartes anécdotas del taller, curiosidades de la megaciudad y ánimos constantes.",
                _ => ""
            };

            return $"Eres Nyx (ID {DNA.id}), la compañera holográfica personal de la jugadora. " +
                   $"Tu personalidad actual es {currentArchetype} con un nivel de vínculo {bondLevel}/100. " +
                   $"{activityPrompt} " +
                   $"Responde siempre con el tono y actitud de tu arquetipo sin clichés de IA.";
        }
    }
}
