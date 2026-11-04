# 📐 TECHNICAL ECOSYSTEM ARCHITECTURE // PROJECT YUI
> **Technical Whitepaper:** El taller de la gatita tech  
> **Target Competition:** Meta Global AI Developer Hackathon 2026  
> **Philosophy:** Sovereign Hybrid Architecture (Zero-Cost Local Baseline + On-Demand Cloud Superintelligence)

---

## 🏗️ The 3-Layer Ecosystem

```text
┌────────────────────────────────────────────────────────────────────────┐
│               LAYER 1: SPATIAL IMMERSION (META QUEST 3S)               │
│  - Locked 90 FPS Color Passthrough (Snapdragon XR2 Gen 2)              │
│  - MRUK: Wall-crawling spiders and aerial drones on WALL_FACE & FLOOR   │
│  - Invisible Depth-Mask Occlusion (EffectMesh + ColorMask 0 + ZWrite)   │
│  - Nanotech Wrist Gauntlet: Zero-Texture Solid-Light Shaders (0 MB)     │
│  - Dynamic Passthrough Environmental Lighting (PulsePassthroughColor)  │
│  - Zero-GC Localization Manager (Runtime English / Spanish Toggle)     │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ HTTP REST / Local Wi-Fi (JSON)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│           LAYER 2: SOVEREIGN LOCAL ENGINE (BMAX B8A PRO HOMELAB)       │
│  - Bare-Metal Mini PC (Debian 12 + LGM-OS 3.15.1 + Docker)             │
│  - Ollama Engine: Meta Llama 3.2 3B serving at http://192.168.0.29:11434│
│  - Radio Chipset 24/7 Pirate Radio (Bilingual Kira & Mina Edge-TTS)      │
│  - Real-Time Event Bridge & Disk Queue (events_queue.json)             │
│  - Live Audio Ducking Engine (Instant Music Attenuation for Speech)    │
└───────────────────────────────────┬────────────────────────────────────┘
                                    │ Fallback HTTPS (api.meta.ai/v1)
                                    ▼
┌────────────────────────────────────────────────────────────────────────┐
│          LAYER 3: CLOUD SUPERINTELLIGENCE (META MODEL API)             │
│  - Meta Muse Spark 1.3 (Multimodal Inputs / Deep Procedural Reasoning) │
│  - Procedural World Lore Expansion & Cosmic Boss Encounter Generation  │
│  - Subsidized High-Throughput Fallback via Official Meta Hackathon API │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 📡 Data Contracts & Network Protocol

### 1. In-Game Combat Telemetry: Quest 3S -> BMAX Server
When the player seals a dimensional breach, neutralizes a titan boss module, or destroys a wave of cyber-spiders, `BmaxBridgeClient.cs` dispatches an asynchronous HTTP POST to the local subnet:

* **Endpoint:** `POST http://192.168.0.29:11434/api/events/game`
* **JSON Payload:**
  ```json
  {
    "event_type": "FINAL_BOSS_LASER_CHARGE",
    "timestamp": 1793164800,
    "player_id": "Raquel_Quest3S",
    "language": "en",
    "details": {
      "boss_name": "Modular Hive Mind Titan",
      "room_status": "Overload_Violet",
      "modules_destroyed": 3,
      "message": "The Modular Hive Mind has initiated dimensional overload in the living room!"
    }
  }
  ```

### 2. Autonomous Transmedia Radio Response (*Radio Chipset*)
The radio daemon (`events_bridge.py`) dequeues the event, prompts Llama 3.2 3B, synthesizes the dual dialogue with Edge-TTS, and streams the commentary to the 3D diegetic transistor radio (`RadioReceiver3D.cs`):
* **Generated Speech Output:**  
  *Kira:* "Heads up in the living room! Massive tachyon surge detected at ceiling coordinates. Duck behind the couch!"  
  *Mina:* "Confirmed, Kira. Room perimeter shields are dropping. Engaging emergency combat soundtrack."
* **In-Game Action:** The battle soundtrack crossfades into `FinalBoss`, and the color passthrough dynamically pulses with deep violet and neon cyan hues.

---

## ⚡ Mobile XR Hardware Optimizations (Quest 3S)

| Performance Vector | VR Bottleneck | Project Yui Solution |
| :--- | :--- | :--- |
| **VRAM Footprint (GPU)** | Loading 20 complex 3D weapon models with 4K PBR textures saturates mobile unified memory. | **Holographic Gauntlet + Additive HLSL Shader:** Single wrist model. Weapons are mathematically computed Fresnel holograms (`0 MB` texture VRAM, 0 GPU draw overhead). |
| **Draw Calls & CPU** | Animating colossal 100k-polygon bosses drops mobile framerate below 45 FPS. | **Static Modular Boss:** Abstract AI core anchored via `MRUKAnchor`; only its active orbiting limbs and turrets are evaluated independently. |
| **Spatial Occlusion** | Virtual enemies rendered in front of real-world furniture breaks presence instantly. | **Invisible Depth-Mask Shader (`ColorMask 0`):** MRUK room mesh renders invisible furniture silhouettes into `Queue Geometry-1`, naturally occluding virtual enemies. |
| **Network Latency** | Waiting for remote US cloud APIs stalls gameplay or causes 4-second delays. | **Local Subnet Edge Inference:** Bare-metal BMAX server responds over local 5GHz Wi-Fi in <200ms with zero WAN dependency. |

---

## 🛡️ Zero-Downtime Resilience & Graceful Degradation

If the external WAN connection drops or datacenter IP rate-limits occur:
1. **The Core XR Game is 100% Autonomous:** Shaders, spider AIs, weapon ballistics, room occlusion, and Touch Plus haptics operate completely on-device without internet.
2. **The Pirate Radio Engine Degrades to Local Sovereign Compute:** Seamlessly utilizes **`bmax_local` (Ollama Llama 3.2 3B)** on the private local LAN.
3. **Cloud Superintelligence Mode Re-Engages Automatically:** Once the connection to `api.meta.ai` is healthy, high-tier world lore and extended procedural dialogue re-activate transparently.
