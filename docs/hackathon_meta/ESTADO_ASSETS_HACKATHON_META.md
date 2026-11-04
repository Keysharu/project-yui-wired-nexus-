# 🏆 ESTADO OFICIAL DE PRODUCCIÓN DE ASSETS (META HACKATHON 2026)
> **El Taller de la Gatita Tech — Quest MR Nexus**  
> **Directora / Operadora:** Raquel D. Torres  
> **Fecha de Actualización:** 31 de Octubre de 2026 — 09:20 AM  
> **Estado del Pipeline:** **100% PRODUCCIÓN DE ASSETS Y CINEMÁTICAS COMPLETADA**

---

## 📊 INVENTARIO DE ASSETS DESPLEGADOS EN UNITY (`quest_mr_nexus/Assets/`)

### 👥 1. Presentadoras y Protagonistas
* **MINA (`Assets/Sprites/Mina_Kira/Mina/`):**
  - `model_sheet_triptico.png` (Frente 0°, Perfil 90°, Espalda 180°).
  - Visemas completos y ciclo de caminata de 8 frames (`walk_1.png` a `walk_8.png`).
* **KIRA (`Assets/Sprites/Mina_Kira/Kira/`):**
  - `model_sheet_triptico.png` (Frente 0°, Perfil 90°, Espalda con pelo largo 180°).
  - Visemas completos con micrófono de MC y caminata de 8 frames.
* **NYX (`Assets/Sprites/Nyx/` y `Assets/Resources/Sprites/Nyx/`):**
  - **Motor 2.5D HD de 8 Ángulos Direccionales:** `turn360_0.png` a `turn360_7.png` calibrados para conmutar con el ángulo de visión de las Meta Quest 3S.
  - Visemas de habla, parpadeo autónomo y poses de combate.
  - Componente de IA y vinculación afectiva: [`NyxAdaptationEngine.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Companion/NyxAdaptationEngine.cs).

---

### 👾 2. Escuadrón Enemigo (Modelos 3D y Sprites)
* **CYBER-SPIDER DRONE:**
  - **Malla 3D Procedural con Colisionadores:** [`SpiderDrone_Mecha_3D.blend`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/SpiderDrone_Mecha_3D.blend) / [`.fbx`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/SpiderDrone_Mecha_3D.fbx) (8 patas articuladas, ojo sensor cian/rojo, escala 1.2 m, colisionadores `UCX_`).
  - Hojas de sprites 2.5D: Idle, marcha, carga, ataque, impacto y colapso.
  - IA táctica: [`NeoCyberSpiderDroneAI.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Gameplay/NeoCyberSpiderDroneAI.cs).
* **QUANTUM CENTURION BOSS:**
  - **Malla 3D Procedural con Colisionadores:** [`QuantumCenturion_Boss_3D.blend`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/QuantumCenturion_Boss_3D.blend) / [`.fbx`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/QuantumCenturion_Boss_3D.fbx) (Bípode de 2.8 m, minigun pesada, escudo QC-01 y Arc Reactor central emisivo).
  - IA de tres fases y sobrecarga cuántica: [`QuantumCenturionBossAI.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Gameplay/QuantumCenturionBossAI.cs).

---

### 🏙️ 3. Entorno Urbano y Bulevar NeoEclipse (25% Explorable)
* **Bulevar NeoEclipse Maestro:** [`NeoEclipse_Boulevard_Segment_01.blend`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/NeoEclipse_Boulevard_Segment_01.blend) / [`.fbx`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/NeoEclipse_Boulevard_Segment_01.fbx).
  - Calzada con asfalto mojado reflectante (`tex_wet_asphalt_street.png`).
  - Aceras elevadas con guías neón cian (#00FFCC) y bolardos con anillos luminosos.
  - Pasarela superior con cartel gigante de **Radio Chipset 107.7 FM** activo.
  - Fachadas de locales (`Cyber_Cafe`, `Tech_Forge`, `Neon_Shop`, `Neon_Arcade`).
  - Iluminación nocturna con 4 farolas focales y luz ambiental azul oscura.
* **Mobiliario Urbano y Puestos:**
  - `WiredCafe_TerraceSet.blend` / `.fbx` (Mesa y sillas de terraza).
  - `CyberRamen_CounterBar.blend` / `.fbx` (Puesto de ramen con farolillos rojos y barra).
  - `TacticalCover_BallisticBarrier.blend` / `.fbx` (Parapeto balístico AR).
  - `DataWell_Core3D.blend` / `.fbx` (Pozo de extracción de datos con cristal giratorio).
  - `CyberChest_Supply3D.blend` / `.fbx` (Cofre táctico de munición).

---

### 🎬 4. Cinemáticas y Recorridos de Google Flow (`quest_mr_nexus/Assets/Cinematics/`)
* **`cinematic_cyber_ramen_stall.mp4`:** Puesto tradicional de cyber ramen bajo la lluvia con gato cibernético descansando (10s @ 24fps).
* **`cinematic_cyber_cafe_armory_walk.mp4`:** Recorrido en primera persona (POV) por el interior del café y la armería táctica con vitrinas de armas y terminales holográficos (10s @ 24fps).
* **`cinematic_reality_breach_ar_to_vr.mp4`:** Transición cinematográfica "Efecto WOW" desde el salón físico de passthrough a la megaciudad VR a través de la fractura dimensional de alambre cian (10s @ 24fps).

---

### ⚙️ 5. Lógica de Motor y Sincronización en Unity 6
* **[`CyberVisualAndCombatUpgrader.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Core/CyberVisualAndCombatUpgrader.cs):**
  - Asignación dinámica de materiales PBR (asfalto mojado, neones, letreros).
  - Auto-conversión de enemigos a modelos 3D al spawnear.
  - Purga de primitivas blancas del salón físico.
  - Construcción del interior del Cyber Café & Armería.
* **[`CyberProps3DIntegration.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Core/CyberProps3DIntegration.cs):**
  - Carga automática en runtime desde `Assets/Resources/Models/`.
  - Conmutador AR / VR en tiempo real (Tecla `V` o Botón `Y`).
* **[`MRUKEncounterSpawner.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Gameplay/MRUKEncounterSpawner.cs):**
  - Bloqueo de apertura forzada de cofres mientras se explora en modo VR.
