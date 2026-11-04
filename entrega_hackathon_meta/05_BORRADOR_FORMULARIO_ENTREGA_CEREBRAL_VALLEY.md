# 📝 BORRADOR DE RESPUESTAS // FORMULARIO DE ENTREGA CEREBRAL VALLEY
> **Formulario oficial:** [https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit](https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit)  
> **Fecha límite de entrega:** Jueves 5 de noviembre a las 03:00 AM CET (Madrid) // Miércoles 4 de noviembre a las 21:00 ET.  
> **Instrucciones:** Copia y pega estos campos en el formulario web de Cerebral Valley. Puedes guardarlo como borrador en cualquier momento antes del cierre.  
> **Autora:** Raquel D. Torres (*El taller de la gatita tech*)  

---

### 1. Project Information (Información del Proyecto)

* **Project Name:**  
  `Project Yui: The Wired Nexus & Radio Chipset 107.7 FM`

* **Tagline (Elevator Pitch en una frase):**  
  `A room-scale Mixed Reality FPS on Meta Quest 3S featuring 3D anime companion Nyx, upright wall-crawling mecha spiders, and the Quantum Centurion boss with physical occlusion, paired with live Radio Chipset 107.7 FM streaming and on-premise Llama 3.2 3B.`

* **Track Selection (Categoría Principal):**  
  `[X] Indie Developer Track ($150,000)`

* **Bonus Categories (Categorías Bonus):**  
  `[X] Creative Superintelligence ($5,000)`  
  `[X] Open Weights Champion ($10,000)`

---

### 2. Links & Media (Enlaces y Contenido)

* **Public GitHub Repository URL:**  
  `https://github.com/Keysharu/project-yui-wired-nexus-`  
  *(Asegúrate de que el repositorio esté en modo PUBLIC antes de enviar).*

* **3-Minute Video Demo URL (YouTube / Vimeo / Loom):**  
  `https://youtu.be/[TU_ID_DE_VIDEO]`  
  *(Asegúrate de que esté en modo Público u Oculto/Unlisted para que los jueces puedan verlo sin iniciar sesión).*

---

### 3. Technical Description (Descripción Técnica para los Jueces)

#### A. What does your project do? (¿Qué hace tu proyecto?)
> *Project Yui is an all-in-one spatial computing ecosystem designed natively for the Meta Quest 3S. It transforms the player's physical home into a cybernetic battlefield using color passthrough and the Meta Mixed Reality Utility Kit (MRUK).*
>
> *Players are accompanied by Nyx, a full 3D anime companion (0.55m tactical scale) hovering at their flank with procedural animations and tactical voice cues. Players equip a wrist-mounted Holographic Gauntlet projecting solid-light weaponry with haptic feedback and 6DoF hand tracking.*
>
> *Autonomous upright mecha spiders crawl along real-world scanned walls, and the armored Quantum Centurion Boss Mk-IX executes multi-phase combat. Real furniture physically occludes virtual enemies using custom depth-mask shaders at stable 90 FPS.*
>
> *Integrated into the space is a 16:9 holographic display streaming Radio Chipset 107.7 FM with live audio ducking, while an interactive LainOS cyberpunk pause menu allows instant muting of local ambient sound and seamless switching between physical room AR and the full VR Cyberspace Boulevard.*

#### B. How do you use Meta models and technologies? (¿Cómo usas las tecnologías de Meta?)
> *Our project implements a resilient hybrid architecture combining Meta's edge hardware, open-weight models, and cloud superintelligence:*
> 1. **Meta Quest 3S & Meta XR Core SDK (v69+):** Drives room-scale color passthrough, spatial anchors (`OVRSpatialAnchor`), Touch Plus haptics, optical hand tracking (`XRHandAnchorFollower`), and dynamic passthrough color tinting.
> 2. **Meta Mixed Reality Utility Kit (MRUK):** Powers static room scanning, physical wall surface anchoring (`WALL_FACE` for crawling spiders), and invisible spatial depth occluders (`EffectMesh` + depth mask) so real furniture hides virtual enemies.
> 3. **Meta Llama 3.2 3B (On-Premise Sovereign Engine):** Runs locally on a bare-metal mini PC homelab server via Ollama over local Wi-Fi (`192.168.0.29:11434`), delivering instant, zero-cost conversational logic and event interpretation with zero cloud downtime.
> 4. **Meta Muse Spark 1.3 (`https://api.meta.ai/v1`):** Serves as the cloud superintelligence fallback for procedural world lore and tactical radio dialogue.

#### C. Prior Work & Hackathon Timeline Disclosure (Declaración de Trabajo Previo)
> *In accordance with official hackathon rules, we explicitly disclose:*
> - **Pre-existing assets:** An initial desktop PyQt experimental companion (`cyber_companion`) and our base Debian Linux homelab mini PC setup.
> - **New contributions built 100% during the hackathon (Oct 26 – Nov 4):** The entire Unity 6 Quest 3S mixed reality game (48 C# scripts with 100% bilingual ES/EN comments, holographic gauntlet, MRUK occlusion shaders, 3D anime heroine Nyx companion, upright mecha spider drone AI, Quantum Centurion Boss 3D AI, live Radio Chipset 107.7 FM streaming display, interactive LainOS pause menu with audio mute toggle, clean AR/VR environment isolation, Touch Plus & Hand Tracking integration, and strict Zero Secrets architecture).

#### D. Challenges We Ran Into (Obstáculos Superados: Inestabilidad de APIs de Meta y Soluciones Soberanas)
> *Desarrollar un título de Realidad Mixta a escala de habitación en Unity 6 LTS para las Meta Quest 3S, combinando modelos LLM conversacionales y streaming de audio en tiempo real, supuso importantes desafíos técnicos:*
>
> 1. **Caídas Masivas y Latencia en las APIs en la Nube de Meta / Baseten:**
>    En las horas pico de desarrollo, los endpoints en la nube de Meta Llama y servicios asociados como Baseten sufrieron caídas recurrentes, latencias elevadas (>8–12 segundos) y errores `502 / 504 Bad Gateway`. Esta situación fue objeto de quejas constantes por parte de numerosos participantes en los canales oficiales de soporte del Discord de Cerebral Valley (`#llama-api` y `#tech-support`).
>    * **Nuestra Solución:** En lugar de dejar el juego a merced de la nube, diseñamos una **arquitectura soberana con tolerancia a fallos**. Desplegamos **Meta Llama 3.2 3B** en local sobre un servidor homelab Debian 12 (mini PC BMAX en `192.168.0.29:11434` vía Ollama). Implementamos un enrutamiento híbrido de 3 capas: intenta la nube primero, conmuta al cluster local de Llama en menos de 800 ms si la API falla o está saturada, y dispone de un árbol determinista en el motor si no hay red. Gracias a esto, Nyx y las locutoras de la radio jamás se congelan.
>
> 2. **Discrepancias y Cambios de Ruptura en Meta XR Core SDK v69 y Unity 6:**
>    Desarrollar para el visor Meta Quest 3S en el recién lanzado Unity 6 LTS implicó resolver fricciones críticas de bajo nivel:
>    * *Conflicto de Giroscopio y Passthrough:* Los scripts diseñados para simular la vista en el editor de PC forzaban el nivelado artificial del horizonte (`LevelCameraHorizon`). En las gafas físicas, esto anulaba el giroscopio real al arrancar el Passthrough a color, provocando un desbalance visual brusco y desorientación. Lo solucionamos aislando el nivelado exclusivamente con `if (Application.isEditor)`, otorgando control absoluto al tracking 6DoF de hardware en Android.
>    * *Desincronización de Mandos Touch Plus y Hand Tracking:* El paso entre seguimiento óptico de manos y mandos Touch Plus producía desajustes en el anclaje de las armas del guantelete. Desarrollamos un adaptador de doble seguimiento (`XRHandAnchorFollower.cs`) que consulta `UnityEngine.XR.InputDevices` directamente con precisión submilimétrica antes de recurrir a `OVRInput`.
>    * *Directivas Obsoletas en paquetes de Meta:* Unity 6 emitía advertencias por el uso de flags obsoletos (`DEVELOPMENT_BUILD` en `OVRSpatialAnchor.cs`), lo que nos obligó a desacoplar el sistema de anclajes espaciales para garantizar estabilidad a largo plazo.
>
> 3. **Concurrencia de Audio en Android y Permisos de Micrófono:**
>    Activar el micrófono en tiempo real para detección de voz (VAD) junto con audio espacial 3D de combate, pistas musicales y la radio comunitaria provocaba bloqueos en el hilo de audio de Android y excepciones de permisos.
>    * **Nuestra Solución:** Creamos una secuencia asíncrona de solicitud de permisos en tiempo de ejecución (`Permission.Microphone`), encapsulamos la captura en un buffer VAD seguro con cálculo de energía RMS y desacoplamos por completo los callbacks de audio del ciclo de vida de los spawners de enemigos.
>
> 4. **Candados de Archivos en Gradle Daemon y Compilador Burst:**
>    Durante las compilaciones iterativas en Windows, el compilador Burst AOT y el daemon de Gradle (`java.exe`) retenían bloqueadas las librerías nativas `.so` en `mergeReleaseNativeLibs/out/lib/arm64-v8a`, lanzando excepciones fatales `AccessDeniedException`.
>    * **Nuestra Solución:** Configuramos `gradleTemplate.properties` con `org.gradle.daemon=false` y creamos la herramienta `QuestBuildPreparer.cs`, que finaliza automáticamente cualquier proceso huérfano de Java y limpia las cachés bloqueadas antes de compilar, reduciendo el ciclo de compilación a menos de 3 minutos.


---

### 4. Team Information (Información del Equipo)

* **Team Name / Individual:** `El taller de la gatita tech`
* **Lead Developer:** `Raquel D. Torres`
* **Country:** `Spain`
* **Role:** `Solo Creator / XR & AI Engineer`
