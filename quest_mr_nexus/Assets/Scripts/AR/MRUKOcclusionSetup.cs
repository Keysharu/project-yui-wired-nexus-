using UnityEngine;
using Meta.XR.MRUtilityKit;

namespace NeoCyber.AR
{
    /// <summary>
    /// [ES] Configuración de Oclusión Espacial MRUK (Máscara de Profundidad Invisible).
    /// Genera una malla de oclusión sobre la habitación escaneada (muebles, paredes, techo)
    /// mediante EffectMesh de MRUK y el shader DepthMask, ocultando físicamente a drones,
    /// arañas y proyectiles cuando pasan detrás de objetos reales como el sofá o la televisión.
    /// El suelo se excluye deliberadamente para permitir la visualización de enemigos en tránsito.
    /// ---
    /// [EN] MRUK Spatial Occlusion Setup (Invisible Depth Mask).
    /// Builds an invisible occlusion mesh over the scanned room (furniture, walls,
    /// ceiling) using MRUK's EffectMesh and DepthMask shader, so drones, spiders and
    /// projectiles are correctly hidden when they pass behind real objects like a TV or couch.
    /// The floor is deliberately excluded to prevent hiding enemies navigating on it.
    /// </summary>
    public class MRUKOcclusionSetup : MonoBehaviour
    {
        [Tooltip("Material using the NeoCyber/MRUK_DepthMaskOccluder shader.")]
        [SerializeField] private Material occluderMaterial;

        [Tooltip("Leave empty to have this script add and configure one at runtime.")]
        [SerializeField] private EffectMesh effectMesh;

        private bool isSetUp;

        private void OnEnable()
        {
            if (MRUK.Instance == null)
            {
                Debug.LogWarning("[MRUKOcclusionSetup] MRUK.Instance is not ready yet. " +
                    "Make sure an MRUK component exists in the scene before this object enables.");
                return;
            }

            MRUK.Instance.RegisterSceneLoadedCallback(SetupOcclusion);

            // Scene data may already be loaded by the time this object enables
            // (e.g. on a scene reload); handle that case directly too.
            if (MRUK.Instance.IsInitialized && MRUK.Instance.GetCurrentRoom() != null)
            {
                SetupOcclusion();
            }
        }

        private void OnDisable()
        {
            if (MRUK.Instance != null)
            {
                MRUK.Instance.SceneLoadedEvent.RemoveListener(SetupOcclusion);
            }
        }

        private void SetupOcclusion()
        {
            if (isSetUp)
            {
                return;
            }

            if (occluderMaterial == null)
            {
                Debug.LogError("[MRUKOcclusionSetup] No occluderMaterial assigned; " +
                    "assign a material using NeoCyber/MRUK_DepthMaskOccluder.");
                return;
            }

            MRUKRoom room = MRUK.Instance.GetCurrentRoom();
            if (room == null)
            {
                Debug.LogWarning("[MRUKOcclusionSetup] No current MRUKRoom found yet.");
                return;
            }

            if (effectMesh == null)
            {
                effectMesh = gameObject.AddComponent<EffectMesh>();
            }

            // Furniture, walls and ceiling occlude; the open floor does not.
            effectMesh.Labels =
                MRUKAnchor.SceneLabels.WALL_FACE |
                MRUKAnchor.SceneLabels.CEILING |
                MRUKAnchor.SceneLabels.TABLE |
                MRUKAnchor.SceneLabels.COUCH |
                MRUKAnchor.SceneLabels.OTHER |
                MRUKAnchor.SceneLabels.DOOR_FRAME |
                MRUKAnchor.SceneLabels.WINDOW_FRAME;

            effectMesh.MeshMaterial = occluderMaterial;
            effectMesh.CreateMesh(room);

            isSetUp = true;

            Debug.Log("[MRUKOcclusionSetup] Occlusion mesh built from the scanned room.");
        }

        // NOTE ON SDK VERSIONS: EffectMesh's public field names (Labels,
        // MeshMaterial, CreateMesh) and the RegisterSceneLoadedCallback /
        // GetCurrentRoom() calls on MRUK.Instance match the MRUK API surface
        // documented at the time this was written. Meta has moved several MRUK
        // features to a native implementation across recent releases, so if
        // this doesn't compile against your installed package, check the
        // EffectMesh.cs source that ships inside your
        // Packages/com.meta.xr.mrutilitykit folder for the current field names
        // -- the logic above (filter by SceneLabels, assign the depth-mask
        // material, build the mesh once the room is loaded) stays the same.
    }
}
