# 📐 ARQUITECTURA TÉCNICA DEL ECOSISTEMA // PROJECT YUI
> **Whitepaper Técnico:** El taller de la gatita tech  
> **Ámbito:** Meta Global AI Developer Hackathon 2026  
> **Filosofía:** Arquitectura Soberana Híbrida (Zero-Cost Local Baseline + On-Demand Cloud Superintelligence)

---

## 🏗️ Las 3 Capas del Ecosistema

```text
┌────────────────────────────────────────────────────────────────────────┐
│               CAPA 1: INMERSIÓN ESPACIAL (META QUEST 3S)               │
│  - Passthrough a Color 90 FPS (Snapdragon XR2 Gen 2)                   │
│  - MRUK: Drones y Arácnidos en WALL_FACE y FLOOR                       │
│  - Oclusión Invisible por Depth Mask (EffectMesh + ZWrite On)           │
│  - Guantelete de Nanotecnología: Shaders Aditivos de Luz Sólida (0MB)  │
│  - Tinte Passthrough Dinámico (PulsePassthroughColorTint)              │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ HTTP REST / Wi-Fi Local (JSON)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│             CAPA 2: MOTOR SOBERANO LOCAL (BMAX B8A PRO HOMELAB)        │
│  - Mini PC Bare-Metal (Debian 12 + LGM-OS 3.15.1 + Docker)             │
│  - Ollama Engine: Meta Llama 3.2 3B en http://192.168.0.29:11434      │
│  - Emisora Radio Chipset 24/7 (Kira & Mina con Edge-TTS Dual)           │
│  - Puente de Eventos en Tiempo Real (events_queue.json)                │
│  - Audio Ducking Automático (Atenuación musical en locuciones)         │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ Fallback HTTPS (api.meta.ai/v1)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│            CAPA 3: SUPERINTELIGENCIA EN NUBE (META MODEL API)          │
│  - Meta Muse Spark 1.3 (Entrada Multimodal / Razonamiento Profundo)    │
│  - Generación de Lore complejo y eventos de Boss cósmicos              │
│  - Inferencia de respaldo con cupones subvencionados                   │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Contratos de Datos y Protocolo de Red

### 1. Telemetría de Combate Quest 3S -> BMAX Server
Cuando la jugadora sella una grieta, destruye un módulo del jefe o abate un enjambre de arácnidos, `BmaxBridgeClient.cs` envía una petición HTTP POST asíncrona a la red local:

* **Endpoint:** `POST http://192.168.0.29:11434/api/events/game`
* **Payload JSON:**
  ```json
  {
    "event_type": "FINAL_BOSS_LASER_CHARGE",
    "timestamp": 1793164800,
    "player_id": "Raquel_Quest3S",
    "details": {
      "boss_name": "Mente Colmena Omnipresente",
      "room_status": "Overload_Violet",
      "modules_destroyed": 3,
      "message": "¡La Mente Colmena ha iniciado la sobrecarga dimensional en el salón!"
    }
  }
  ```

### 2. Respuesta de la Radio Pirata *Radio Chipset*
El motor de radio (`events_bridge.py`) captura el evento de la cola, sintetiza el diálogo de Kira y Mina con Edge-TTS en menos de 1.2 segundos y transmite el clip de audio al transistor virtual `RadioReceiver3D.cs` en las gafas:
* **Locución Generada:**  
  *Kira:* «¡Atención en el salón! Detecto una emisión masiva de taquiones en el techo. ¡Buscad cobertura tras el sofá!»  
  *Mina:* «Confirmado, Kira. ¡El blindaje del salón está cayendo, activando música de emergencia!»
* **Acción en el Juego:** La música de combate hace crossfade a `FinalBoss`, y el passthrough se tiñe de violeta.

---

## ⚡ Optimizaciones Clave para Hardware Móvil (Quest 3S)

| Vector de Rendimiento | Problema Típico en VR | Nuestra Solución en *Project Yui* |
| :--- | :--- | :--- |
| **Memoria VRAM (GPU)** | Cargar 20 mallas de armas y texturas 4K satura los 8GB de RAM unificada del visor. | **Guantelete Holográfico + Shader Aditivo:** Una sola malla base en la muñeca; las armas se proyectan con fórmulas matemáticas Fresnel sin texturas (`0 MB` de VRAM). |
| **Draw Calls & CPU** | Animar jefes gigantescos de 100k polígonos hunde la tasa de frames por debajo de 45 FPS. | **Jefe Modular Estático:** Núcleo abstracto fijo al techo con `MRUKAnchor`; solo sus extremidades y módulos activos se procesan de forma independiente. |
| **Oclusión Espacial** | Monstruos virtuales superpuestos sobre muebles reales rompen la ilusión de inmersión. | **Malla de Profundidad Invisible (`ColorMask 0`):** El escaneo de MRUK renderiza la silueta invisible del sofá/tele en la cola `Geometry-1`, tapando a los enemigos reales de forma natural. |
| **Latencia de Red** | Esperar respuestas de APIs cloud en EE.UU. congela el juego o introduce retardos de 4 segundos. | **Inferencia Edge en LAN:** El servidor BMAX responde en <200ms por Wi-Fi local sin depender de la conexión a internet exterior. |

---

## 🛡️ Resiliencia y Fallback Automático (Zero-Downtime)

Si la conexión a Internet cae o Meta aplica bloqueos de IPs de datacenter:
1. El juego **sigue funcionando al 100% de forma autónoma**: los shaders, los arácnidos, el guantelete y el combate en Quest 3S no requieren internet.
2. La radio pirata conmuta automáticamente a **`bmax_local` (Ollama Llama 3.2 3B)**, manteniendo las locuciones y la música de combate vivas en la red local.
3. Cuando la conexión con `api.meta.ai` está disponible y con créditos válidos, el sistema activa el modo superinteligencia para generar lore y misiones expandidas.
