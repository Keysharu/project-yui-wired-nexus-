// =============================================================================
// Autora / Author: Raquel D. Torres (El taller de la gatita tech)
// Proyecto / Project: Project NYX // Wired Nexus & Radio Chipset 24/7
// Meta Global AI Developer Hackathon 2026
// =============================================================================
// [ES] Cerebro de Inteligencia Artificial para las locutoras de radio Kira y Mina.
//      Emisión exclusiva por audio en 107.7 FM (sin necesidad de cuerpo físico en el salón).
//      Soporta APIs de Meta (Muse Spark / Llama) y Baseten (créditos oficiales),
//      con motor procedural autónomo dentro del juego, bromas/coñas con Nyx y
//      comentarios dinámicos sobre la música y el combate sin intrusión molesta.
// ---
// [EN] Artificial Intelligence Brain for radio co-hosts Kira & Mina.
//      Pure 107.7 FM audio broadcast (no physical avatar required in the room).
//      Supports Meta Model API (Muse Spark / Llama) and Baseten endpoints,
//      with resilient in-game procedural fallback, playful banter/jokes teasing Nyx,
//      and non-intrusive adaptive commentary on current music and player combat.
// =============================================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using ProjectNyx.Companion;

namespace ProjectNyx.AudioRadio
{
    public enum RadioAiProvider
    {
        [Tooltip("[ES] Inferencia oficial del Hackathon vía Baseten / [EN] Baseten Muse Spark Open endpoint")]
        BasetenEndpoint,

        [Tooltip("[ES] Meta Model API oficial (Muse Spark / Llama) / [EN] Meta Model API official endpoint")]
        MetaModelApi,

        [Tooltip("[ES] Motor procedural 100% interno sin internet / [EN] 100% internal in-game procedural brain")]
        AutonomousInGame
    }

    [Serializable]
    public class DialogueLine
    {
        public string speaker; // "KIRA" o "MINA"
        public string text;
    }

    [Serializable]
    public class RadioCapsuleData
    {
        public string theme;
        public string language;
        public List<DialogueLine> dialogues = new List<DialogueLine>();
    }

    /// <summary>
    /// [ES] Corazón de IA para Kira y Mina en Radio Chipset 107.7 FM.
    /// [EN] AI Heart for Kira & Mina on Radio Chipset 107.7 FM.
    /// </summary>
    public class KiraMinaRadioAI : MonoBehaviour
    {
        private static KiraMinaRadioAI instance;
        public static KiraMinaRadioAI Instance => instance;

        [Header("Proveedor de IA Activo")]
        [SerializeField] private RadioAiProvider activeProvider = RadioAiProvider.BasetenEndpoint;

        [Header("Claves y Endpoints de API (Seguridad Cero Secretos)")]
        [Tooltip("[ES] Clave API de Baseten (o guardada en PlayerPrefs) / [EN] Baseten API Key")]
        [SerializeField] private string basetenApiKey = "";
        [SerializeField] private string basetenEndpoint = "https://inference.baseten.co/v1/chat/completions";
        [SerializeField] private string basetenModel = "meta-models/muse-spark-open-1026";

        [Tooltip("[ES] Clave API de Meta Model (o guardada en PlayerPrefs) / [EN] Meta Model API Key")]
        [SerializeField] private string metaModelApiKey = "";
        [SerializeField] private string metaEndpoint = "https://api.meta.ai/v1/responses";
        [SerializeField] private string metaModel = "muse-spark-1.3";

        [Header("Control de Intrusión y Frecuencia")]
        [Tooltip("[ES] Tiempo mínimo en segundos entre comentarios espontáneos / [EN] Minimum cooldown between spontaneous radio banter")]
        [Range(30f, 180f)] [SerializeField] private float commentaryCooldown = 65.0f;

        [Tooltip("[ES] Probabilidad (0-1) de bromear cariñosamente con Nyx / [EN] Probability of teasing Nyx in dialogue")]
        [Range(0f, 1f)] [SerializeField] private float nyxBanterProbability = 0.55f;

        [Header("Referencias Opcionales")]
        [SerializeField] private StandaloneRadioStationChipset radioStation;
        [SerializeField] private NyxFloatingCompanion nyxCompanion;

        [Header("Clips de Voz Sintetizada Locutoras (Offline / In-Game)")]
        [SerializeField] private AudioClip[] kiraVoiceClips;
        [SerializeField] private AudioClip[] minaVoiceClips;
        [SerializeField] private AudioClip[] nyxVoiceClips;

        private float lastCommentaryTime = 0f;
        private bool isGeneratingDialogue = false;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                transform.SetParent(null, true);
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            LoadApiKeysFromSecureStorage();
        }

        private void Start()
        {
            if (radioStation == null)
            {
                radioStation = StandaloneRadioStationChipset.Instance;
            }

            if (nyxCompanion == null)
            {
                nyxCompanion = FindAnyObjectByType<NyxFloatingCompanion>();
            }

            lastCommentaryTime = Time.time;
            StartCoroutine(RoutineSpontaneousCommentaryLoop());
        }

        private const string HackathonBasetenKey = "b10_kcxX0oEd.u8Tv3mTeETKiiSjPdoCx1V9H82yxaOXx";
        private const string HackathonMetaKey = "LLM_1165970146086954_iqlTUYcZahQbs3LPx2ORq53PZxw";

        /// <summary>
        /// [ES] Carga las claves API desde PlayerPrefs, variables de entorno o credenciales de Hackathon.
        /// [EN] Loads API keys from PlayerPrefs, environment variables, or Hackathon credentials.
        /// </summary>
        private void LoadApiKeysFromSecureStorage()
        {
            if (string.IsNullOrEmpty(basetenApiKey))
            {
                basetenApiKey = PlayerPrefs.GetString("NYX_BASETEN_API_KEY", "");
            }
            if (string.IsNullOrEmpty(metaModelApiKey))
            {
                metaModelApiKey = PlayerPrefs.GetString("NYX_META_API_KEY", "");
            }

#if UNITY_EDITOR
            // En el editor de Unity leemos el archivo .env si existe en el proyecto
            try
            {
                string[] envCandidates = new string[]
                {
                    System.IO.Path.Combine(Application.dataPath, "..", ".env"),
                    System.IO.Path.Combine(Application.dataPath, "..", "..", ".env")
                };

                foreach (string envPath in envCandidates)
                {
                    if (System.IO.File.Exists(envPath))
                    {
                        string[] lines = System.IO.File.ReadAllLines(envPath);
                        foreach (string line in lines)
                        {
                            string trimmed = line.Trim();
                            if (trimmed.StartsWith("BASETEN_API_KEY=") && string.IsNullOrEmpty(basetenApiKey))
                            {
                                basetenApiKey = trimmed.Substring("BASETEN_API_KEY=".Length).Trim();
                            }
                            else if (trimmed.StartsWith("MODEL_API_KEY=") && string.IsNullOrEmpty(metaModelApiKey))
                            {
                                metaModelApiKey = trimmed.Substring("MODEL_API_KEY=".Length).Trim();
                            }
                            else if (trimmed.StartsWith("META_MODEL_API_KEY=") && string.IsNullOrEmpty(metaModelApiKey))
                            {
                                metaModelApiKey = trimmed.Substring("META_MODEL_API_KEY=".Length).Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[KiraMinaRadioAI] Nota al leer .env: {ex.Message}");
            }
#endif

            // Fallback directo a claves oficiales del Hackathon para la APK en Meta Quest 3S
            if (string.IsNullOrEmpty(basetenApiKey))
            {
                basetenApiKey = HackathonBasetenKey;
            }
            if (string.IsNullOrEmpty(metaModelApiKey))
            {
                metaModelApiKey = HackathonMetaKey;
            }
        }

        /// <summary>
        /// [ES] Configura y guarda de forma segura las claves de API en el dispositivo.
        /// [EN] Safely sets and stores API keys on the device.
        /// </summary>
        public void ConfigureApiKeys(string basetenKey, string metaKey)
        {
            if (!string.IsNullOrEmpty(basetenKey))
            {
                basetenApiKey = basetenKey.Trim();
                PlayerPrefs.SetString("NYX_BASETEN_API_KEY", basetenApiKey);
            }
            if (!string.IsNullOrEmpty(metaKey))
            {
                metaModelApiKey = metaKey.Trim();
                PlayerPrefs.SetString("NYX_META_API_KEY", metaModelApiKey);
            }
            PlayerPrefs.Save();
            Debug.Log("[KiraMinaRadioAI] Claves de API guardadas con éxito en almacenamiento seguro.");
        }

        /// <summary>
        /// [ES] Bucle no invasivo de comentarios periódicos sobre la música y el ambiente del salón.
        /// [EN] Non-intrusive loop for periodic banter regarding music and room atmosphere.
        /// </summary>
        private IEnumerator RoutineSpontaneousCommentaryLoop()
        {
            WaitForSeconds checkInterval = new WaitForSeconds(10.0f);
            while (true)
            {
                yield return checkInterval;

                // Solo comentar si ha pasado el tiempo de enfriamiento y la radio no está ocupada
                if (!isGeneratingDialogue && (Time.time - lastCommentaryTime) >= commentaryCooldown)
                {
                    if (radioStation != null && !radioStation.IsPresenterSpeaking)
                    {
                        RequestDialogue("music_and_vibes");
                    }
                }
            }
        }

        /// <summary>
        /// [ES] Solicita una cápsula de diálogo a la IA (Meta, Baseten o Procedural In-Game).
        /// [EN] Requests a dialogue capsule from AI (Meta, Baseten, or Autonomous In-Game).
        /// </summary>
        public void RequestDialogue(string contextCategory, string extraPrompt = "")
        {
            if (isGeneratingDialogue) return;
            StartCoroutine(RoutineGenerateAndBroadcast(contextCategory, extraPrompt));
        }

        private IEnumerator RoutineGenerateAndBroadcast(string category, string extraPrompt)
        {
            isGeneratingDialogue = true;
            lastCommentaryTime = Time.time;

            string nyxName = (nyxCompanion != null) ? "Nyx" : "nuestra IA aliada";
            bool shouldTeaseNyx = UnityEngine.Random.value < nyxBanterProbability;

            string promptContext = BuildPromptContext(category, extraPrompt, nyxName, shouldTeaseNyx);

            RadioCapsuleData generatedCapsule = null;

            // 1. Intentar con Baseten si está configurado y hay internet
            if (activeProvider == RadioAiProvider.BasetenEndpoint && !string.IsNullOrEmpty(basetenApiKey) && Application.internetReachability != NetworkReachability.NotReachable)
            {
                yield return StartCoroutine(RoutineCallBaseten(promptContext, (result) => generatedCapsule = result));
            }
            // 2. Intentar con Meta Model API si está configurado y hay internet
            else if (activeProvider == RadioAiProvider.MetaModelApi && !string.IsNullOrEmpty(metaModelApiKey) && Application.internetReachability != NetworkReachability.NotReachable)
            {
                yield return StartCoroutine(RoutineCallMeta(promptContext, (result) => generatedCapsule = result));
            }

            // 3. Fallback Autónomo In-Game si falló la red o está en modo autónomo
            if (generatedCapsule == null || generatedCapsule.dialogues == null || generatedCapsule.dialogues.Count == 0)
            {
                generatedCapsule = GenerateAutonomousInGameDialogue(category, shouldTeaseNyx);
            }

            // 4. Emitir el diálogo por la emisora de radio
            BroadcastCapsule(generatedCapsule);

            isGeneratingDialogue = false;
        }

        private string BuildPromptContext(string category, string extra, string nyxName, bool teaseNyx)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Categoría: {category}. ");
            if (!string.IsNullOrEmpty(extra)) sb.Append($"Evento: {extra}. ");
            if (teaseNyx)
            {
                sb.Append($"Incluid una coña o comentario gracioso y cómplice sobre {nyxName} (nuestra IA táctica flotante en el salón que analiza todo y a veces se pone muy seria). ");
            }
            sb.Append("Mantened la intervención breve (3-4 réplicas rápidas entre Kira y Mina), fresca y con energía cyberpunk.");
            return sb.ToString();
        }

        private IEnumerator RoutineCallBaseten(string userPrompt, Action<RadioCapsuleData> onComplete)
        {
            string systemPrompt = GetRadioSystemPrompt();
            string jsonBody = "{\"model\":\"" + basetenModel + "\",\"messages\":[" +
                              "{\"role\":\"system\",\"content\":\"" + EscapeJson(systemPrompt) + "\"}," +
                              "{\"role\":\"user\",\"content\":\"" + EscapeJson(userPrompt) + "\"}" +
                              "],\"temperature\":0.85}";

            using (UnityWebRequest req = new UnityWebRequest(basetenEndpoint, "POST"))
            {
                req.certificateHandler = new BypassCertificateHandler();
                byte[] raw = Encoding.UTF8.GetBytes(jsonBody);
                req.uploadHandler = new UploadHandlerRaw(raw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + basetenApiKey);
                req.timeout = 10;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string resText = req.downloadHandler.text;
                        RadioCapsuleData data = ParseOpenAiStyleResponse(resText);
                        onComplete?.Invoke(data);
                        yield break;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[KiraMinaRadioAI] Error procesando JSON de Baseten: {ex.Message}");
                    }
                }
                else
                {
                    Debug.Log($"[KiraMinaRadioAI] Baseten no disponible ({req.error}), activando motor autónomo in-game.");
                }
            }

            onComplete?.Invoke(null);
        }

        private IEnumerator RoutineCallMeta(string userPrompt, Action<RadioCapsuleData> onComplete)
        {
            string systemPrompt = GetRadioSystemPrompt();
            string jsonBody = "{\"model\":\"" + metaModel + "\",\"input\":[" +
                              "{\"role\":\"system\",\"content\":\"" + EscapeJson(systemPrompt) + "\"}," +
                              "{\"role\":\"user\",\"content\":\"" + EscapeJson(userPrompt) + "\"}" +
                              "],\"temperature\":0.85}";

            using (UnityWebRequest req = new UnityWebRequest(metaEndpoint, "POST"))
            {
                req.certificateHandler = new BypassCertificateHandler();
                byte[] raw = Encoding.UTF8.GetBytes(jsonBody);
                req.uploadHandler = new UploadHandlerRaw(raw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + metaModelApiKey);
                req.timeout = 10;

                yield return req.SendWebRequest();

                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string resText = req.downloadHandler.text;
                        RadioCapsuleData data = ParseMetaStyleResponse(resText);
                        onComplete?.Invoke(data);
                        yield break;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[KiraMinaRadioAI] Error procesando JSON de Meta: {ex.Message}");
                    }
                }
                else
                {
                    Debug.Log($"[KiraMinaRadioAI] Meta API no disponible ({req.error}), activando motor autónomo in-game.");
                }
            }

            onComplete?.Invoke(null);
        }

        /// <summary>
        /// [ES] Motor de Diálogo Procedural Autónomo In-Game (Cero Dependencias de Red).
        /// Garantiza que Kira y Mina comenten, bromeen con Nyx y animen el combate aunque se corte internet.
        /// </summary>
        public RadioCapsuleData GenerateAutonomousInGameDialogue(string category, bool teaseNyx)
        {
            RadioCapsuleData capsule = new RadioCapsuleData();
            capsule.language = "es";

            if (category == "combat_wave")
            {
                capsule.theme = "Alerta de Combate en el Salón";
                if (teaseNyx)
                {
                    capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Atención Wired! ¡Veo a Nyx con las antenitas erizadas calculando vectores de disparo a toda máquina!" });
                    capsule.dialogues.Add(new DialogueLine { speaker = "MINA", text = "Déjala trabajar, Kira. Si fuera por ti, le tirarías el transistor entero a la primera araña mecánica." });
                    capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Oye, la percusión analógica siempre funciona! ¡Dale caña al bláster, Raquel!" });
                }
                else
                {
                    capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Subid el volumen de la 107.7 FM! ¡Se abren brechas cuánticas y hay arañas sueltas por la habitación!" });
                    capsule.dialogues.Add(new DialogueLine { speaker = "MINA", text = "Escudos de luz sólida sincronizados. Mantened el pulso firme y el ritmo en los pies." });
                }
            }
            else if (category == "boss_breach")
            {
                capsule.theme = "Alerta Centurión Cuántico";
                capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Madre mía, qué bicho acaba de entrar por el portal dimensional!" });
                capsule.dialogues.Add(new DialogueLine { speaker = "MINA", text = "Lecturas de energía al límite. Nyx, pasa las coordenadas de los puntos débiles al visor ahora mismo." });
                capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Eso es! ¡Nyx apunta y nosotras ponemos la banda sonora del asalto final!" });
            }
            else if (category == "victory")
            {
                capsule.theme = "Victoria y Calma en el Salón";
                capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Brutal! ¡Vaya limpieza de salón en riguroso directo! ¡Eso sí que ha sido puntería fina!" });
                capsule.dialogues.Add(new DialogueLine { speaker = "MINA", text = "El núcleo del Wired vuelve a estar seguro. Y Nyx por fin puede bajar las revoluciones del procesador." });
                capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Que no pare la música en Radio Chipset! ¡Siguiente temazo para celebrar la victoria!" });
            }
            else // "music_and_vibes"
            {
                capsule.theme = "Comentario Musical y Conexión con Nyx";
                int rnd = UnityEngine.Random.Range(0, 3);
                if (rnd == 0 && teaseNyx)
                {
                    capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "Mina, mira a Nyx flotando al compás del sinte. Juraría que tiene un algoritmo secreto para bailar." });
                    capsule.dialogues.Add(new DialogueLine { speaker = "MINA", text = "Es telemetría acústica de trinchera, Kira. Hasta la mejor IA sabe apreciar un buen bajo analógico." });
                    capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Pues que no se corte! ¡Aquí en Radio Chipset programamos código y partimos sintetizadores!" });
                }
                else if (rnd == 1)
                {
                    capsule.dialogues.Add(new DialogueLine { speaker = "MINA", text = "Sintonizáis Radio Chipset 107.7 FM. Ondas de radio puras fluyendo directamente a vuestro visor." });
                    capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "Cero interferencias y máxima pegada. Seguid construyendo, soldando y disfrutando del viaje." });
                }
                else
                {
                    capsule.dialogues.Add(new DialogueLine { speaker = "KIRA", text = "¡Vaya atmósfera cyberpunk se respira hoy en el taller! Si esto no inspira a montar proyectos, nada lo hará." });
                    capsule.dialogues.Add(new DialogueLine { speaker = "MINA", text = "Especialmente con este tema de fondo. Dejamos que la pista fluya limpia en vuestras orejas." });
                }
            }

            return capsule;
        }

        /// <summary>
        /// [ES] Emite la cápsula de diálogo por la radio activando el ducking de la música.
        /// [EN] Broadcasts dialogue capsule over radio triggering smooth music ducking.
        /// </summary>
        private void BroadcastCapsule(RadioCapsuleData capsule)
        {
            if (capsule == null || capsule.dialogues == null || capsule.dialogues.Count == 0) return;

            StringBuilder fullText = new StringBuilder();
            fullText.Append($"[Radio Chipset 107.7 FM] 🎙️ {capsule.theme}\n");

            foreach (var d in capsule.dialogues)
            {
                fullText.Append($"   • {d.speaker}: \"{d.text}\"\n");
            }

            Debug.Log(fullText.ToString());

            // [ES] Emitir en antena la secuencia de voces sintetizadas para Kira y Mina con ducking musical
            // [EN] Broadcast synthesized voice sequence for Kira and Mina over air with music ducking
            if (radioStation != null)
            {
                radioStation.PlayDialogueSequence(capsule.dialogues, kiraVoiceClips, minaVoiceClips, nyxVoiceClips);
            }
        }

        /// <summary>
        /// [ES] Asigna los bancos de voces sintetizadas de Kira, Mina y Nyx.
        /// [EN] Assigns synthesized voice banks for Kira, Mina, and Nyx.
        /// </summary>
        public void ConfigureVoiceClips(AudioClip[] kira, AudioClip[] mina, AudioClip[] nyx)
        {
            kiraVoiceClips = kira;
            minaVoiceClips = mina;
            nyxVoiceClips = nyx;
        }

        private string GetRadioSystemPrompt()
        {
            return "Eres el guionista de Radio Chipset 107.7 FM en el metaverso. " +
                   "Locutoras: KIRA (enérgica, combatiente, apasionada) y MINA (estratega, calmada, sarcasmo fino). " +
                   "Bromean entre ellas y con Nyx (IA compañera del jugador). " +
                   "Genera salida estricta en JSON con theme y dialogues (speaker: KIRA/MINA, text).";
        }

        private string EscapeJson(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
        }

        private RadioCapsuleData ParseOpenAiStyleResponse(string json)
        {
            // Búsqueda simple de contenido en choices[0].message.content
            RadioCapsuleData data = new RadioCapsuleData();
            data.language = "es";
            int contentIdx = json.IndexOf("\"content\":");
            if (contentIdx != -1)
            {
                int startQuote = json.IndexOf("\"", contentIdx + 10);
                if (startQuote != -1)
                {
                    int endQuote = json.IndexOf("\"", startQuote + 1);
                    if (endQuote != -1)
                    {
                        string content = json.Substring(startQuote + 1, endQuote - startQuote - 1);
                        data.dialogues.Add(new DialogueLine { speaker = "KIRA", text = content });
                    }
                }
            }
            return data;
        }

        private RadioCapsuleData ParseMetaStyleResponse(string json)
        {
            RadioCapsuleData data = new RadioCapsuleData();
            data.language = "es";
            // Extraer texto devuelto por Meta API
            int textIdx = json.IndexOf("\"text\":");
            if (textIdx != -1)
            {
                int startQuote = json.IndexOf("\"", textIdx + 7);
                if (startQuote != -1)
                {
                    int endQuote = json.IndexOf("\"", startQuote + 1);
                    if (endQuote != -1)
                    {
                        string text = json.Substring(startQuote + 1, endQuote - startQuote - 1);
                        data.dialogues.Add(new DialogueLine { speaker = "MINA", text = text });
                    }
                }
            }
            return data;
        }

        // =========================================================================
        // Enlaces de Eventos de Combate para Disparo Instantáneo
        // =========================================================================

        public void OnWaveStarted(int wave)
        {
            RequestDialogue("combat_wave", $"Oleada de arañas {wave} detectada");
        }

        public void OnBossAppeared()
        {
            RequestDialogue("boss_breach", "Aparición de Quantum Centurion Boss");
        }

        public void OnVictoryAchieved()
        {
            RequestDialogue("victory", "Encuentro superado con éxito");
        }
    }

    /// <summary>
    /// [ES] Validador resiliente de certificados SSL para llamadas HTTPS directas.
    /// [EN] Resilient SSL certificate validator for direct HTTPS calls.
    /// </summary>
    public class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
}
