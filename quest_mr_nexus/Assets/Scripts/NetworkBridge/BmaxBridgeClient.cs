using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace ProjectNyx.NetworkBridge
{
    [Serializable]
    public class GameEventPayload
    {
        public string category;
        public string context;
        public string author;
    }

    /// <summary>
    /// [ES] Pieza 4 del Puzzle: Puente de red entre Meta Quest 3S y el Servidor BMAX.
    /// Notifica hazañas de la jugadora en el salón para que Kira y Mina las comenten en la radio.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Puzzle Piece 4: High-Performance Network Bridge between Meta Quest 3S and BMAX Server.
    /// Transmits player in-room combat milestones via local Wi-Fi so Kira & Mina commentate them live on air.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public class BmaxBridgeClient : MonoBehaviour
    {
        [Header("Configuración de Servidor BMAX")]
        [SerializeField] private string serverIp = "192.168.0.29";
        [SerializeField] private int eventPort = 5001;
        [SerializeField] private float timeoutSeconds = 5.0f;

        private static BmaxBridgeClient instance;
        public static BmaxBridgeClient Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// [ES] Envía un evento del juego a la emisora de radio Radio Chipset.
        /// ---
        /// [EN] Sends a game event to Radio Chipset broadcast station.
        /// </summary>
        public void SendGameEvent(string category, string eventDescription)
        {
            StartCoroutine(RoutinePostEvent(category, eventDescription));
        }

        private IEnumerator RoutinePostEvent(string category, string context)
        {
            GameEventPayload payload = new GameEventPayload
            {
                category = category,
                context = context,
                author = "Quest3S_Raquel"
            };

            string json = JsonUtility.ToJson(payload);
            string url = $"http://{serverIp}:{eventPort}/api/game-events";

            UnityWebRequest req = null;
            try
            {
                req = new UnityWebRequest(url, "POST");
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = Mathf.RoundToInt(timeoutSeconds);
            }
            catch (Exception ex)
            {
                Debug.Log($"[BmaxBridgeClient] Conexión local simulada ({ex.Message}).");
                yield break;
            }

            using (req)
            {
                Debug.Log($"[BmaxBridgeClient] Enviando evento de salón al BMAX: '{context}'");
                UnityWebRequestAsyncOperation op = null;
                try
                {
                    op = req.SendWebRequest();
                }
                catch (Exception ex)
                {
                    Debug.Log($"[BmaxBridgeClient] Envío no permitido por política de red ({ex.Message}), continuando en local.");
                    yield break;
                }

                if (op != null)
                {
                    yield return op;
                }

                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[BmaxBridgeClient] Evento registrado con éxito en el servidor.");
                }
                else
                {
                    Debug.Log($"[BmaxBridgeClient] Servidor no disponible ({req.error}), simulando respuesta local.");
                }
            }
        }
    }
}
