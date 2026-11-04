using UnityEngine;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Animador en tiempo real del Salón 3D Cyberpunk generado en Google Flow.
    /// Controla la transición visual entre la habitación física real y el entorno virtual extendido.
    /// ---
    /// [EN] Real-time Animator for the Cyberpunk 3D Room Environment generated in Google Flow.
    /// Smoothly blends the visual transition between the physical room passthrough and the extended virtual world.
    /// </summary>
    public class FlowRoomBackdropAnimator : MonoBehaviour
    {
        [Header("Renderizador y Fotogramas 1080p de Google Flow")]
        [SerializeField] private MeshRenderer targetMeshRenderer;
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private Texture2D[] flowFrames;
        [SerializeField] private Sprite[] flowSprites;
        [SerializeField] private float framesPerSecond = 8f;

        [Header("Anclaje de Escala de Habitación")]
        [SerializeField] private float baseDistanceFromCamera = 5.80f;
        [SerializeField] private bool maintainPerspectiveCoverage = true;

        private float timer;
        private int currentFrame;
        private Vector3 initialScale;

        private void Awake()
        {
            initialScale = transform.localScale;
        }

        private void Update()
        {
            int count = (flowSprites != null && flowSprites.Length > 0)
                ? flowSprites.Length
                : (flowFrames != null ? flowFrames.Length : 0);

            if (count > 1)
            {
                timer += Time.deltaTime;
                float interval = 1f / Mathf.Max(1f, framesPerSecond);
                if (timer >= interval)
                {
                    timer -= interval;
                    currentFrame = (currentFrame + 1) % count;

                    if (targetSpriteRenderer != null && flowSprites != null && currentFrame < flowSprites.Length)
                    {
                        targetSpriteRenderer.sprite = flowSprites[currentFrame];
                    }

                    if (targetMeshRenderer != null && flowFrames != null && currentFrame < flowFrames.Length && targetMeshRenderer.material != null)
                    {
                        targetMeshRenderer.material.mainTexture = flowFrames[currentFrame];
                    }
                }
            }

            if (maintainPerspectiveCoverage && Camera.main != null && initialScale.x > 0.1f)
            {
                float distZ = Mathf.Max(2.5f, transform.position.z - Camera.main.transform.position.z);
                float scaleFactor = Mathf.Clamp(distZ / Mathf.Max(1f, baseDistanceFromCamera), 0.85f, 3.5f);
                transform.localScale = new Vector3(initialScale.x * scaleFactor, initialScale.y * scaleFactor, 1f);
            }
        }
    }
}
