using System;
using UnityEngine;

namespace ProjectNyx.Core
{
    /// <summary>
    /// [ES] Pieza 1 del Puzzle: Anclaje espacial persistente en muebles y superficies del salón.
    /// Utiliza OVRSpatialAnchor para guardar y recuperar la posición de props virtuales en la habitación física.
    /// ---
    /// [EN] Puzzle Piece 1: Persistent Spatial Anchoring on Real Furniture & Room Surfaces.
    /// Employs OVRSpatialAnchor to persist and restore virtual prop coordinates in physical room space.
    /// </summary>
    public class SpatialAnchorController : MonoBehaviour
    {
        [Header("Propiedades del Ancla")]
        [SerializeField] private bool persistAnchorOnSave = true;
        [SerializeField] private string anchorIdentifier = "RadioTerminal_Anchor";

        private OVRSpatialAnchor spatialAnchor;

        private void Awake()
        {
            spatialAnchor = GetComponent<OVRSpatialAnchor>();
            if (spatialAnchor == null)
            {
                spatialAnchor = gameObject.AddComponent<OVRSpatialAnchor>();
            }
        }

        private void Start()
        {
            InitializeAnchor();
        }

        public void InitializeAnchor()
        {
            if (spatialAnchor != null)
            {
                Debug.Log($"[SpatialAnchorController] Ancla '{anchorIdentifier}' inicializada en posición: {transform.position}");
            }
        }

        public void SnapToSurface(Vector3 hitPoint, Vector3 hitNormal)
        {
            transform.position = hitPoint;
            transform.rotation = Quaternion.LookRotation(hitNormal);
            Debug.Log($"[SpatialAnchorController] Objeto fijado a superficie física en: {hitPoint}");
        }
    }
}
