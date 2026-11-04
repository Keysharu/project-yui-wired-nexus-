using System;

using System.Collections;

using System.Collections.Generic;

using UnityEngine;

using ProjectNyx.AudioRadio;

using ProjectNyx.Companion;

using ProjectNyx.NetworkBridge;



namespace ProjectNyx.Gameplay

{

    public enum EncounterPhase

    {

        LootSupplyChests,

        BreachWarning,

        Wave1_SingleSpider,

        Intermission_Wave1ToWave2,

        Wave2_TwoSpiders,

        Intermission_Wave2ToBoss,

        BossEncounterActive,

        EncounterCleared

    }



    /// <summary>

    /// [ES] Fase 4 del Plan Maestro: Director de Bucle de Juego, Brecha Interdimensional y Oleadas.

    /// Coordina la generación procedural de enemigos sobre paredes y techos físicos escaneados por MRUK.

    /// ---

    /// [EN] Master Plan Phase 4: Game Loop Director, Interdimensional Rift & Wave Spawner.

    /// Coordinates procedural enemy spawning on physical walls and ceilings detected by MRUK.

    /// </summary>

    public class MRUKEncounterSpawner : MonoBehaviour

    {

        [Header("Prefabs y Referencias de Escena")]

        [SerializeField] private GameObject spiderDronePrefab;

        [SerializeField] private DimensionalRift3D dimensionalRift3D;

        [SerializeField] private QuantumCenturionBossAI centurionBossInstance;

        [SerializeField] private CyberSupplyChest[] roomSupplyChests;



        [Header("Voces Audibles en Español (Mina, Kira y Nyx)")]

        [SerializeField] private AudioClip voiceKiraWelcome247;

        [SerializeField] private AudioClip voiceMinaTacticalBrief;

        [SerializeField] private AudioClip voiceNyxWaveBanter;

        [SerializeField] private AudioClip voiceKiraBossAlert;

        [SerializeField] private AudioClip voiceNyxBattleStance;
        public AudioClip VoiceNyxWaveBanter => voiceNyxWaveBanter;
        public AudioClip VoiceNyxBattleStance => voiceNyxBattleStance;

        [SerializeField] private AudioClip voiceKiraClimaxEnergizer;

        [SerializeField] private AudioClip voiceMinaVictory247;



        [Header("Tiempos de la Secuencia Táctica")]

        [SerializeField] private float delayAfterChestsSeconds = 2.0f;

        [SerializeField] private float breachOpeningDurationSeconds = 2.5f;

        [SerializeField] private float delayBetweenWavesSeconds = 3.0f;

        [SerializeField] private float autoOpenChestsFallbackSeconds = 12.0f;

        [SerializeField] private float spawnRadius = 2.3f;



        [Header("Estado en Vivo (Solo Lectura)")]

        [SerializeField] private EncounterPhase currentPhase = EncounterPhase.LootSupplyChests;



        private readonly List<GameObject> activeSpiders = new List<GameObject>();

        private NyxBillboardAnimator nyxAnimator;

        private RadioHologramDisplay radioDisplay;

        private int openedChestsCount;

        private float phaseTimer;

        private bool sequenceStarted;

        private bool victoryBroadcastPlayed;



        public EncounterPhase CurrentPhase => currentPhase;

        public int ActiveEnemyCount => activeSpiders.Count;



        private void Start()

        {

            nyxAnimator = FindAnyObjectByType<NyxBillboardAnimator>();

            radioDisplay = FindAnyObjectByType<RadioHologramDisplay>();



            if (dimensionalRift3D == null)

            {

                dimensionalRift3D = FindAnyObjectByType<DimensionalRift3D>(FindObjectsInactive.Include);

            }



            if (roomSupplyChests == null || roomSupplyChests.Length == 0)

            {

                roomSupplyChests = FindObjectsByType<CyberSupplyChest>(FindObjectsSortMode.None);

            }



            // Auto-enlazar Boss de la escena si no esta asignado en el Inspector

            if (centurionBossInstance == null)

            {

                centurionBossInstance = FindAnyObjectByType<QuantumCenturionBossAI>(FindObjectsInactive.Include);

                if (centurionBossInstance == null)

                {

                    GameObject bossGo = GameObject.Find("QuantumCenturion_Boss_3D");

                    if (bossGo != null)

                    {

                        centurionBossInstance = bossGo.GetComponent<QuantumCenturionBossAI>();

                    }

                }

            }



            // Auto-enlazar Araña de la escena si no esta asignada

            if (spiderDronePrefab == null)

            {

                GameObject spiderGo = GameObject.Find("Meshy_AI_Spider_Mech_VR_1103022049_generate");

                if (spiderGo != null)

                {

                    spiderDronePrefab = spiderGo;

                }

            }



            // Asegurar que el Boss empieza oculto hasta su fase tras las oleadas

            if (centurionBossInstance != null)

            {

                centurionBossInstance.gameObject.SetActive(false);

            }



            // Ocultar la araña plantilla de la escena para que solo aparezcan las oleadas activadas

            if (spiderDronePrefab != null && spiderDronePrefab.scene.isLoaded)

            {

                spiderDronePrefab.SetActive(false);

            }



            // Suscribirse a los cofres 3D de la habitación

            openedChestsCount = 0;

            if (roomSupplyChests != null && roomSupplyChests.Length > 0)

            {

                foreach (var chest in roomSupplyChests)

                {

                    if (chest != null)

                    {

                        chest.OnChestOpened += HandleChestOpened;

                    }

                }

            }



            currentPhase = EncounterPhase.LootSupplyChests;

            phaseTimer = 0f;

            victoryBroadcastPlayed = false;



            // 1. Arrancar emisión musical continua 24/7 en modo Peaceful y locución de bienvenida de Kira + Mina

            if (BattleMusicManager.Instance != null)

            {

                BattleMusicManager.Instance.EnsureRadio247Playing();

                if (voiceKiraWelcome247 != null)

                {

                    BattleMusicManager.Instance.PlayVoiceBroadcast(voiceKiraWelcome247);

                }

                if (voiceMinaTacticalBrief != null)

                {

                    float delay = voiceKiraWelcome247 != null ? voiceKiraWelcome247.length + 0.25f : 0.5f;

                    BattleMusicManager.Instance.QueueVoiceBroadcast(voiceMinaTacticalBrief, delay);

                }

            }



            if (radioDisplay != null)

            {

                radioDisplay.TriggerLiveBroadcastAnimation(9.5f);

            }



            if (nyxAnimator != null)

            {

                nyxAnimator.SetBossBattleStance(false);

                nyxAnimator.SpeakForDuration(3.8f, "Perímetro escaneado, operadora. Dispara o acércate a los Cofres de Suministros con la cruz holográfica para sobrecargar el arsenal.");

            }

        }



        private bool lastVrState = false;



        private void Update()

        {

            // Ocultar cofres de la habitación en modo VR inmersivo para que no estorben en el bulevar

            var decorator = FindAnyObjectByType<ProjectNyx.Core.CyberRoomDecorator3D>();

            if (decorator != null && decorator.IsFullVrActive != lastVrState)

            {

                lastVrState = decorator.IsFullVrActive;

                if (roomSupplyChests != null)

                {

                    foreach (var chest in roomSupplyChests)

                    {

                        if (chest != null) chest.gameObject.SetActive(!lastVrState);

                    }

                }

            }



            // Limpiar referencias de arañas abatidas

            for (int i = activeSpiders.Count - 1; i >= 0; i--)

            {

                if (activeSpiders[i] == null)

                {

                    activeSpiders.RemoveAt(i);

                }

            }



            // Atajos manuales de prueba rápida en PC / Quest

            if (Input.GetKeyDown(KeyCode.E))

            {

                ForceOpenAllChests();

            }

            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch) || Input.GetKeyDown(KeyCode.B))

            {

                StopAllCoroutines();

                StartCoroutine(CoEscalateToBossBreach());

            }

            if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch) || Input.GetKeyDown(KeyCode.N))

            {

                SpawnSpiderWave(1);

            }



            // Máquina de estados de la secuencia de juego

            switch (currentPhase)

            {

                case EncounterPhase.LootSupplyChests:

                    // En modo VR Inmersivo, no forzar la apertura de cofres para permitir explorar la calle tranquilamente

                    var roomDec = FindAnyObjectByType<ProjectNyx.Core.CyberRoomDecorator3D>();

                    if (roomDec != null && roomDec.IsFullVrActive)

                    {

                        phaseTimer = 0f;

                        break;

                    }



                    phaseTimer += Time.deltaTime;

                    if (!sequenceStarted && phaseTimer >= autoOpenChestsFallbackSeconds)

                    {

                        ForceOpenAllChests();

                    }

                    break;



                case EncounterPhase.Wave1_SingleSpider:

                    if (activeSpiders.Count == 0)

                    {

                        currentPhase = EncounterPhase.Intermission_Wave1ToWave2;

                        phaseTimer = 0f;

                        Debug.Log("[MRUKEncounterSpawner] Oleada 1 eliminada. Esperando 3.0 segundos antes de la Oleada 2...");

                        if (nyxAnimator != null)

                        {

                            nyxAnimator.SpeakForDuration(2.5f, "¡Buena puntería! Tres segundos para la segunda oleada, recarga el condensador.");

                        }

                    }

                    break;



                case EncounterPhase.Intermission_Wave1ToWave2:

                    phaseTimer += Time.deltaTime;

                    if (phaseTimer >= delayBetweenWavesSeconds && activeSpiders.Count == 0)

                    {

                        currentPhase = EncounterPhase.Wave2_TwoSpiders;

                        SpawnSpiderWave(2);

                    }

                    break;



                case EncounterPhase.Wave2_TwoSpiders:

                    if (activeSpiders.Count == 0)

                    {

                        currentPhase = EncounterPhase.Intermission_Wave2ToBoss;

                        phaseTimer = 0f;

                        Debug.Log("[MRUKEncounterSpawner] Oleada 2 eliminada. Esperando 3.0 segundos antes de la expansión de Brecha Boss...");

                    }

                    break;



                case EncounterPhase.Intermission_Wave2ToBoss:

                    phaseTimer += Time.deltaTime;

                    if (phaseTimer >= delayBetweenWavesSeconds && activeSpiders.Count == 0)

                    {

                        currentPhase = EncounterPhase.BossEncounterActive;

                        StartCoroutine(CoEscalateToBossBreach());

                    }

                    break;



                case EncounterPhase.BossEncounterActive:

                    if (centurionBossInstance != null && centurionBossInstance.IsDead)

                    {

                        currentPhase = EncounterPhase.EncounterCleared;

                        if (nyxAnimator != null)

                        {

                            nyxAnimator.SetBossBattleStance(false);

                        }

                        if (!victoryBroadcastPlayed)

                        {

                            victoryBroadcastPlayed = true;

                            if (BattleMusicManager.Instance != null && voiceMinaVictory247 != null)

                            {

                                BattleMusicManager.Instance.PlayVoiceBroadcast(voiceMinaVictory247);

                            }

                            if (radioDisplay != null)

                            {

                                radioDisplay.TriggerLiveBroadcastAnimation(6.5f);

                            }

                        }

                    }

                    break;

            }

        }



        private void HandleChestOpened(CyberSupplyChest chest)

        {

            openedChestsCount++;

            int totalChests = (roomSupplyChests != null && roomSupplyChests.Length > 0) ? roomSupplyChests.Length : 1;



            if (!sequenceStarted && openedChestsCount >= Mathf.Min(2, totalChests))

            {

                sequenceStarted = true;

                StartCoroutine(CoBreachWarningAndWave1());

            }

        }



        public void ForceOpenAllChests()

        {

            if (roomSupplyChests != null)

            {

                foreach (var chest in roomSupplyChests)

                {

                    if (chest != null && !chest.IsOpen)

                    {

                        chest.OpenChest();

                    }

                }

            }



            if (!sequenceStarted)

            {

                sequenceStarted = true;

                StartCoroutine(CoBreachWarningAndWave1());

            }

        }



        private IEnumerator CoBreachWarningAndWave1()

        {

            yield return new WaitForSeconds(delayAfterChestsSeconds);



            // 1. ADVERTENCIA DE BRECHA INTERDIMENSIONAL

            currentPhase = EncounterPhase.BreachWarning;

            if (dimensionalRift3D != null)

            {

                dimensionalRift3D.TriggerBreachWarning();

            }



            if (nyxAnimator != null)

            {

                nyxAnimator.TriggerExpression(NyxMoodState.Alert, 2.5f);

                nyxAnimator.SpeakForDuration(2.5f, "¡ADVERTENCIA DE BRECHA! ¡Se está rasgando una Brecha Interdimensional en la pared norte!");

            }



            if (BmaxBridgeClient.Instance != null)

            {

                BmaxBridgeClient.Instance.SendGameEvent("BREACH_WARNING", "¡Alerta sísmica! Se está abriendo una brecha interdimensional en los sectores comerciales de la ciudad.");

            }



            yield return new WaitForSeconds(breachOpeningDurationSeconds);



            // 2. SE ABRE EL PORTAL Y CAMBIA DE RITMO A MÚSICA DE COMBATE NORMAL + OLEADA 1 (1 ARAÑA)

            if (dimensionalRift3D != null)

            {

                dimensionalRift3D.OpenForBasicWaves();

            }



            if (BattleMusicManager.Instance != null)

            {

                BattleMusicManager.Instance.SetBattleTier(BattleTier.NormalBattle);

                if (voiceNyxWaveBanter != null)

                {

                    BattleMusicManager.Instance.PlayVoiceBroadcast(voiceNyxWaveBanter);

                }

            }



            currentPhase = EncounterPhase.Wave1_SingleSpider;

            SpawnSpiderWave(1);

        }



        public void SpawnSpiderWave(int count)

        {

            if (spiderDronePrefab == null)

            {

                GameObject spiderGo = GameObject.Find("Meshy_AI_Spider_Mech_VR_1103022049_generate");

                if (spiderGo != null) spiderDronePrefab = spiderGo;

            }



            if (spiderDronePrefab == null)

            {

                Debug.LogWarning("[MRUKEncounterSpawner] spiderDronePrefab no encontrado.");

                return;

            }



            Vector3 portalOrigin = dimensionalRift3D != null

                ? dimensionalRift3D.transform.position

                : new Vector3(0f, 1.4f, 5.4f);



            for (int i = 0; i < count; i++)

            {

                float lateralOffset = (count == 1) ? 0f : ((i == 0) ? -1.45f : 1.45f);

                Vector3 spawnPos = new Vector3(

                    portalOrigin.x + lateralOffset,

                    0.55f + (i * 0.25f),

                    portalOrigin.z - 0.55f

                );



                Quaternion spawnRot = spiderDronePrefab != null ? spiderDronePrefab.transform.rotation : Quaternion.identity;
                GameObject spider = Instantiate(spiderDronePrefab, spawnPos, spawnRot);

                spider.name = $"CyberSpider_Wave_{count}_Unit_{i + 1}";

                spider.SetActive(true);

                activeSpiders.Add(spider);

            }



            if (nyxAnimator != null)

            {

                string msg = (count == 1)

                    ? "¡Primera oleada saliendo de la brecha! ¡Una Cyber-Spider de reconocimiento, toda tuya Raquel!"

                    : "¡Segunda oleada! ¡Dos Cyber-Spiders armadas saliendo del portal! Yo las distraigo para que tú te lleves la experiencia.";

                nyxAnimator.TriggerExpression(NyxMoodState.Alert, 2.2f);

                nyxAnimator.SpeakForDuration(4.5f, msg);

            }



            if (BmaxBridgeClient.Instance != null)

            {

                BmaxBridgeClient.Instance.SendGameEvent(

                    "WAVE_STARTED",

                    $"¡Oleada de {count} Cyber-Spider(s) emergiendo de la brecha interdimensional!"

                );

            }

        }



        private IEnumerator CoEscalateToBossBreach()

        {

            // 1. La Brecha Interdimensional crece al doble de tamaño

            if (dimensionalRift3D != null)

            {

                dimensionalRift3D.ExpandForBossBreach();

            }



            // 2. Cambia la música a BossLevel y suenan las voces de alerta de Kira y Pose de Batalla de Nyx

            if (BattleMusicManager.Instance != null)

            {

                BattleMusicManager.Instance.SetBattleTier(BattleTier.BossLevel);

                if (voiceKiraBossAlert != null)

                {

                    BattleMusicManager.Instance.PlayVoiceBroadcast(voiceKiraBossAlert);

                }

                if (voiceNyxBattleStance != null)

                {

                    float delay = voiceKiraBossAlert != null ? voiceKiraBossAlert.length + 0.2f : 0.4f;

                    BattleMusicManager.Instance.QueueVoiceBroadcast(voiceNyxBattleStance, delay);

                }

            }



            if (radioDisplay != null)

            {

                radioDisplay.TriggerLiveBroadcastAnimation(7.0f);

            }



            // 3. Nyx adopta Pose de Batalla y cambia a modo Fuego Real

            if (nyxAnimator != null)

            {

                nyxAnimator.SetBossBattleStance(true);

            }



            yield return new WaitForSeconds(1.2f);



            // 4. Emerge el Quantum Centurión desde la Brecha expandida

            SummonQuantumCenturionBoss();

        }



        public void SummonQuantumCenturionBoss()

        {

            if (centurionBossInstance == null)

            {

                centurionBossInstance = FindAnyObjectByType<QuantumCenturionBossAI>(FindObjectsInactive.Include);

                if (centurionBossInstance == null)

                {

                    GameObject bossGo = GameObject.Find("QuantumCenturion_Boss_3D");

                    if (bossGo != null)

                    {

                        centurionBossInstance = bossGo.GetComponent<QuantumCenturionBossAI>();

                    }

                }

            }



            if (centurionBossInstance == null)

            {

                Debug.LogWarning("[MRUKEncounterSpawner] centurionBossInstance no encontrado.");

                return;

            }



            Vector3 spawnPos = dimensionalRift3D != null

                ? new Vector3(dimensionalRift3D.transform.position.x, 0.05f, dimensionalRift3D.transform.position.z - 0.55f)

                : new Vector3(0f, 0.05f, 2.7f);



            centurionBossInstance.transform.position = spawnPos;

            centurionBossInstance.gameObject.SetActive(true);

        }

    }

}

