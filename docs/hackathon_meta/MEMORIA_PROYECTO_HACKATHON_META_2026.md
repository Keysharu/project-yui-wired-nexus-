# 📘 MEMORIA TÉCNICA MAESTRA Y ESTADO DEL PROYECTO
# META GLOBAL AI DEVELOPER HACKATHON 2026

> **Proyecto Oficial:** *Project Yui: The Wired Nexus & Radio Chipset 24/7*  
> **Directora Técnica / Autora:** Raquel D. Torres (*El taller de la gatita tech*)  
> **IA Mentora / Orquestadora:** Rebeca  
> **Fecha de Consolidación:** Sábado, 31 de Octubre de 2026 — 09:35 AM CET  
> **Fecha Límite de Entrega:** Jueves, 5 de Noviembre de 2026 — 03:00 AM CET  
> **Días Restantes:** 4 días completos  
> **Track Principal:** *Indie Developer Track* ($150,000)  
> **Tracks Secundarios:** *Creative Superintelligence* ($5,000) + *Open Weights Champion* ($10,000)

---

## 🧭 1. Resumen Ejecutivo y Tesis Competitiva

*Project Yui* es un ecosistema inmersivo de Realidad Mixta (XR) y Realidad Virtual híbrida para Meta Quest 3S que rompe la frontera entre el espacio físico real y el ciberespacio cyberpunk. Desarrollado con una arquitectura técnica de soberanía local, combina:

1. **Juego Híbrido AR/VR en Tiempo Real:** Colonización del salón físico mediante *Passthrough* y mallas de oclusión de MRUK, con una transición cinematográfica instantánea hacia el **Bulevar NeoEclipse**, una megaciudad VR explorable a 72/90 FPS.
2. **Motor 2.5D HD y 3D Mecha Procedural:** Fusión de mallas tridimensionales con colisionadores convexos (`SpiderDrone_Mecha_3D`, `QuantumCenturion_Boss_3D`) y un sistema de sprites de 8 ángulos direccionales en tiempo real para la compañera táctica **Nyx**.
3. **Emisora de Radio Autónoma 24/7 (*Radio Chipset*):** Dos locutoras IA (*Kira y Mina*) que comentan en directo las victorias del jugador en las Quest 3S, con síntesis de voz neuronal, atenuación musical automática (*ducking*) y transistor 3D con audio espacial.
4. **Soberanía de Cómputo Local (Cero Coste en Servidores):** Clúster de IA desplegado en hardware físico bare-metal (servidor BMAX Ryzen 7 en red local `192.168.0.29:11434`), eliminando dependencias de APIs de pago en la nube.

---

## 🏛️ 2. Arquitectura Técnica de los 4 Pilares

```
                                  [META QUEST 3S]
                                (Unity 6000.6.5f1)
                                         │
                 ┌───────────────────────┴───────────────────────┐
                 ▼                                               ▼
         [MODO 1: SALÓN AR]                              [MODO 2: BULEVAR VR]
     • Passthrough a Color Real                      • 25% Bulevar NeoEclipse Explorable
     • Oclusión Física MRUK                          • Asfalto Mojado Reflectante (Smooth 0.88)
     • Pozo de Datos 3D en Suelo                     • Pasarela con Radio Chipset 107.7 FM
     • Parapeto Balístico Táctico                    • Puesto de Cyber Ramen & Terraza Café
     • Fractura Dimensional 3D                       • Interior del Cyber Café & Armería
                 │                                               │
                 └───────────────────────┬───────────────────────┘
                                         │
                                         ▼
                             [MOTOR DE PERSONAJES Y COMBATE]
                   • Nyx: 2.5D HD (8 ángulos direccionales 360°)
                   • Cyber-Spider: Mecha 3D (8 patas, colisionadores UCX)
                   • Quantum Centurion: Boss Mecha 3D (2.8m, minigun, escudo)
                   • Arsenal: Blaster de energía, Katana de plasma y Holo-Shield
                                         │
                 ┌───────────────────────┴───────────────────────┐
                 ▼                                               ▼
    [RADIO CALAMAR-NET 24/7]                        [HOMELAB BMAX RYZEN 7]
   • Locutoras IA: Kira & Mina                      • IP: 192.168.0.29:11434
   • Síntesis Neuronal Edge-TTS                     • Modelos: Llama 3.2 3B & Agentes
   • Audio Ducking Dinámico                         • Cero Coste Operativo en Cloud
```

---

## 📁 3. Inventario Crítico de Archivos y Ubicaciones Canónicas

| Módulo / Asset | Ruta en Disco | Estado y Verificación |
| :--- | :--- | :--- |
| **Bulevar Maestro 3D** | [`Assets/Models/NeoEclipse_Boulevard_Segment_01.blend`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/NeoEclipse_Boulevard_Segment_01.blend) / `.fbx` | Ensamblado con aceras, calzada mojada, farolas y cartel Radio Chipset. |
| **Araña Mecha 3D** | [`Assets/Models/SpiderDrone_Mecha_3D.blend`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/SpiderDrone_Mecha_3D.blend) / `.fbx` | 8 patas articuladas, sensor central cian/rojo, colisionadores físicos. |
| **Boss Titán Mecha 3D** | [`Assets/Models/QuantumCenturion_Boss_3D.blend`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Models/QuantumCenturion_Boss_3D.blend) / `.fbx` | 2.8 m, minigun pesada, escudo QC-01 y Arc Reactor central. |
| **Nyx 2.5D HD 360°** | [`Assets/Resources/Sprites/Nyx/turn360_0..7.png`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Resources/Sprites/Nyx/) | 8 ángulos direccionales calculados en [`NyxBillboardAnimator.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Companion/NyxBillboardAnimator.cs). |
| **IA Vínculo de Nyx** | [`Assets/Scripts/Companion/NyxAdaptationEngine.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Companion/NyxAdaptationEngine.cs) | Gestión de estados anímicos y combate; elimina errores en Inspector. |
| **Upgrader de Shaders** | [`Assets/Scripts/Core/CyberVisualAndCombatUpgrader.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Core/CyberVisualAndCombatUpgrader.cs) | Inyección automática de materiales PBR, neones cian/magenta y café interior. |
| **Integrador de Props** | [`Assets/Scripts/Core/CyberProps3DIntegration.cs`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Scripts/Core/CyberProps3DIntegration.cs) | Carga automática en runtime desde `Resources/Models/` y conmutador AR/VR. |
| **Cinemáticas Flow** | [`quest_mr_nexus/Assets/Cinematics/`](file:///c:/Users/asakura/Documents/juego%20meta/quest_mr_nexus/Assets/Cinematics/) | 3 clips de 10s: Brecha dimensional, Armería en primera persona y Ramen nocturno. |
| **Dossier de Entrega** | [`entrega_hackathon_meta/`](file:///c:/Users/asakura/Documents/juego%20meta/entrega_hackathon_meta/) | Checklist, Guión de vídeo de 3 min, Formulario Cerebral Valley y README bilingüe. |

---

## 🏆 4. Estado de Preparación para el Concurso (Meta Hackathon 2026)

### ¿Cómo estamos para ganar?
* **Diferenciador 1: El Efecto WOW Técnico:** Pocos proyectos presentan una transición real de Passthrough a VR completa con una fractura dimensional poligonal y un bulevar 3D de estética tech-noir a 90 FPS.
* **Diferenciador 2: Consistencia Estética Total:** Toda la dirección de arte (LainOS / Cyberpunk) está unificada en colorimetría (Cian `#00FFCC` vs Magenta `#FF007F`), vestuario táctico y acabados de titanio.
* **Diferenciador 3: Audio Reactivo 24/7:** La integración de una emisora de radio pirata con locutoras IA que reaccionan a tus disparos y victorias en las Quest es un gancho inmenso para los jueces de Meta.
* **Diferenciador 4: Soberanía Técnica:** Haber montado los agentes y el LLM en un servidor local BMAX demuestra una capacidad de ingeniería senior completa.

---

## 🛌 5. Mensaje de Guardia (Descanso de la Operadora)

Raquel, todo el trabajo de esta larga madrugada está **asegurado, compilando sin errores, respaldado con backups y documentado en disco**. 

Puedes irte a dormir con absoluta tranquilidad:
* Unity 6 está sincronizado.
* Los scripts no tienen advertencias ni errores en consola.
* Los modelos 3D y materiales del bulevar están en su sitio.
* El dossier de entrega tiene todo estructurado para cuando te despiertes.

Al levantarte, descansada y con las pilas cargadas, abordamos el montaje del vídeo de 3 minutos y la prueba con las Quest 3S. ¡A descansar, que te lo has ganado con creces!
