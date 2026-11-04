using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectNyx.Core
{
    [Serializable]
    public class FloorNode
    {
        public int floorIndex;
        public string floorName;
        public bool isSafeControlZone;
        public bool isUnlocked;
        public Vector3 teleportAnchorPos;
    }

    /// <summary>
    /// [ES] Fase 4 del Plan Maestro: Red de Teletransporte por Nodos y Pisos.
    /// - Registra pisos visitados (Piso 1, 2, 3...).
    /// - Fija Zonas Seguras de Control cada 5 niveles (Piso 1, 5, 10, 15...).
    /// - Permite teletransporte inmediato ("TP") a pisos inferiores ya visitados
    ///   para comerciar, reagrupar al escuadrón o farmear con amigos.
    /// - Bloquea pisos superiores que aún no han sido descubiertos.
    /// Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
    /// ---
    /// [EN] Master Plan Phase 4: Floor Node Teleportation Network.
    /// - Tracks visited tower floors (Floor 1, 2, 3...).
    /// - Designates Safe Control Zones every 5 tiers (Floor 1, 5, 10, 15...).
    /// - Provides instantaneous teleportation ("TP") back to discovered lower floors
    ///   for trading, squad regrouping, or co-op farming.
    /// - Locks undiscovered higher floors until cleared.
    /// Created and developed by Raquel D. Torres (El taller de la gatita tech).
    /// </summary>
    public class FloorTeleportNetwork : MonoBehaviour
    {
        [Header("Configuración de Red de Nodos")]
        [SerializeField] private int maxRegisteredFloors = 20;
        [SerializeField] private int currentFloorIndex = 1;
        [SerializeField] private List<FloorNode> floorNodes = new List<FloorNode>();

        private static FloorTeleportNetwork instance;
        public static FloorTeleportNetwork Instance => instance;

        public int CurrentFloorIndex => currentFloorIndex;
        public IReadOnlyList<FloorNode> FloorNodes => floorNodes;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeFloorNetwork();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeFloorNetwork()
        {
            if (floorNodes.Count == 0)
            {
                for (int i = 1; i <= maxRegisteredFloors; i++)
                {
                    bool isSafe = (i == 1) || (i % 5 == 0);
                    floorNodes.Add(new FloorNode
                    {
                        floorIndex = i,
                        floorName = isSafe ? $"Piso {i}: Zona Segura de Control" : $"Piso {i}: Sector Hostil",
                        isSafeControlZone = isSafe,
                        isUnlocked = (i == 1), // Solo el piso 1 empieza desbloqueado
                        teleportAnchorPos = new Vector3(0f, 0.05f, 0f)
                    });
                }
            }
        }

        public void UnlockFloor(int floorIndex)
        {
            FloorNode node = floorNodes.Find(f => f.floorIndex == floorIndex);
            if (node != null && !node.isUnlocked)
            {
                node.isUnlocked = true;
                Debug.Log($"[FloorTeleportNetwork] ¡Nodo de Piso {floorIndex} registrado y desbloqueado en la red!");
            }
        }

        public bool TryTeleportToFloor(int targetFloor, Transform playerTransform)
        {
            FloorNode node = floorNodes.Find(f => f.floorIndex == targetFloor);
            if (node == null || !node.isUnlocked)
            {
                Debug.LogWarning($"[FloorTeleportNetwork] El Piso {targetFloor} aún está bloqueado. Requiere visita previa.");
                return false;
            }

            currentFloorIndex = targetFloor;
            if (playerTransform != null)
            {
                playerTransform.position = node.teleportAnchorPos;
            }

            Debug.Log($"[FloorTeleportNetwork] Teletransporte táctico exitoso a: {node.floorName}");
            return true;
        }
    }
}
