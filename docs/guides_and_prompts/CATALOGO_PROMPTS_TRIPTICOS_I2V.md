# 📐 CATÁLOGO MAESTRO DE PROMPTS PARA TRÍPTICOS Y MODEL SHEETS I2V
> **El Taller de la Gatita Tech — Meta Quest 3S MR / Horizon OS**  
> **Operadora / Autora:** Raquel D. Torres  
> **Pipeline:** Image-to-Video (I2V) en Google Flow / Kling AI + Extracción Neuronal Python (`tools/i2v_sprite_suite/extract_turnaround_triptico.py`)  
> **Fondo de Estudio:** Croma Verde Puro (`#00FF00`) para recorte automático y anclaje al milímetro.

---

## 🧭 METODOLOGÍA DEL TRÍPTICO EN GENERACIÓN I2V (CERO DERIVA ANATÓMICA)

En lugar de generar 3 imágenes estáticas sueltas (donde cada una altera el diseño, proporciones o ropa), aplicamos la metodología maestra descubierta en el taller:
1. **Vídeo Turntable 360° (Rotación Pura):** Solicitamos a Kling AI / Google Flow que la cámara orbite 360° de forma suave o que el modelo rote sobre una plataforma giratoria invisible en el centro exacto.
2. **Cámara Bloqueada en Eje / Fondo Inmóvil:** La cámara permanece a nivel de ojos, con encuadre completo, sin zoom y sobre fondo `#00FF00` uniforme.
3. **Muestreo Automatizado:** El script `extract_turnaround_triptico.py` extrae con precisión matemática:
   - **0° (Frente):** Vista frontal ortogonal.
   - **90° (Perfil):** Vista lateral estricta.
   - **180° (Espalda):** Vista trasera completa con detalles de mochila/reactores.
4. **Ensamblado Automático en Tríptico:** Genera el archivo `model_sheet_triptico.png` listo para cargar como imagen de referencia en Blender, Tripo3D, Meshy o Unity para modelar o texturizar la malla 3D.

---

## 👤 1. COMPAÑERA NYX (MODEL SHEET DE CUERPO 3D COMPLETO)

* **Objetivo:** Model Sheet canónico de cuerpo completo para modelar la malla 3D / avatar interactivo de Nyx en Realidad Mixta.
* **Imagen de Referencia de Entrada:** `Assets/Sprites/Nyx/idle.png` o `Assets/Sprites/Nyx/walk_1.png`.

### A. Kling AI / Google Flow (Modo I2V Turntable 360°)
* **Ajustes:** Modo Image-to-Video | Motion / Movimiento: `3` | Modo Cámara: `Lock / Static` (el personaje rota en el centro) | Fondo: `#00FF00`.
* **Positive Prompt:**
```text
Full body 360 degree turntable rotation reference model sheet. The anime character from the reference image stands firmly in A-pose or relaxed neutral stance on an invisible rotating platform, smoothly rotating a full 360 degrees around her vertical axis. Clear continuous views of Front (0 degrees), Side Profile (90 degrees), Back (180 degrees), and Opposite Profile (270 degrees). Long sleek straight red hair with blunt bangs, deep purple eyes, high-collar black tactical techwear jacket, glowing cyan neon circuit lines #00FFCC along seams, dark cargo pants with knee pads, combat boots, and tactical leg holster. Neutral calm expression, feet grounded at center. Stationary eye-level camera, locked framing, no camera zoom, no camera pitch, no camera shake. Solid pure chroma green screen background #00FF00 remains completely flat, still, and uniform with zero shadows or textures. High resolution 60fps clean anime 3D model sheet turn-around.
```
* **Negative Prompt:**
```text
walking, stepping off platform, waving hands, dancing, opening mouth, camera movement, camera zoom, camera panning, 3d camera perspective distortion, blurry limbs, extra fingers, changing clothing colors, fading neon lines, shadows on floor, floor textures, background noise, morphing
```

---

## 🕷️ 2. CYBER-SPIDER DRONE (ENEMIGO BÁSICO TREPAMUROS)

* **Objetivo:** Tríptico ortogonal del dron cuadrúpedo/octópodo de combate que patrulla las paredes físicas en Realidad Mixta (Passthrough).
* **Referencia Visual:** Chasis hexagonal compacto estilo tachikoma/araña cibernética, 8 patas mecánicas articuladas con ventosas electromagnéticas, sensor óptico central cian/rojo y blindaje cerámico militar en grafito oscuro.

### A. Kling AI / Google Flow (Modo Turntable 360°)
* **Positive Prompt:**
```text
Full body 360 degree turntable rotation model sheet of a military cyber spider drone. A mechanical robotic spider stands firmly in neutral pose on an invisible rotating disc, rotating smoothly a full 360 degrees. Eight sharp segmented robotic legs with hydraulic pistons and electromagnetic wall-climbing pads, angular hexagonal carbon-fiber armor chassis in dark matte graphite, glowing cyan circuit conduits, and a glowing central robotic optical camera eye in cyan neon. Shows clear 0 degree frontal assault view, 90 degree side profile mechanics, and 180 degree rear exhaust battery vents. Eye level camera, completely static framing, no camera shake, no zoom. Solid pure chroma green screen background #00FF00 remains totally flat, uniform, and still with zero floor shadows. High quality 60fps hard-surface sci-fi 3D model sheet turntable.
```
* **Negative Prompt:**
```text
organic spider, insect flesh, biological eyes, walking away, jumping, firing lasers, camera zoom, camera shake, camera panning, blurry joints, extra broken legs, floor textures, ground patterns, background elements, morphing
```

---

## 🛰️ 3. GLITCH RECON DRONE (ENEMIGO VOLADOR / FLOTANTE)

* **Objetivo:** Tríptico del dron de reconocimiento aéreo que flota en el aire de la habitación física emitiendo pulsos de glitch digital.
* **Referencia Visual:** Orbe central metálico rodeado por dos anillos giroscópicos flotantes con circuitos impresos expuestos, micro-propulsores iónicos y una lente ocular poligonal con aberración cromática sutil.

### A. Kling AI / Google Flow (Modo Turntable 360°)
* **Positive Prompt:**
```text
Full body 360 degree turntable rotation model sheet of a futuristic flying recon glitch drone. A sleek spherical cybernetic drone hovers smoothly in place at center framing on a revolving vertical axis, rotating a full 360 degrees. Spherical central titanium core with a glowing cyan sensor lens, surrounded by two concentric floating magnetic gyroscope rings with intricate glowing circuit traces, small rear ion micro-thrusters emitting soft blue glow, and subtle digital glitch hologram particles. Shows clear frontal sensor view, side ring mechanics, and rear ion engine exhaust. Camera is 100% static, locked eye-level framing, zero zoom, zero tilt. Solid pure chroma green screen background #00FF00 remains flat, uniform, and still with zero shadows. High quality 60fps sci-fi hard surface prop turn-around.
```
* **Negative Prompt:**
```text
flying away, leaving center, propeller blades, airplane wings, feathers, camera zoom, camera movement, blurry details, distorted rings, background textures, floor shadows, morphing
```

---

## 🛡️ 4. SECTOR BOSS: QUANTUM CENTURION (JEFE DE SECTOR BÍPEDO)

* **Objetivo:** Model Sheet de combate de alta escala para el jefe de sector: un mecha bípode pesado blindado con cañón de plasma derecho y reactor cuántico en el torso.
* **Referencia Visual:** Blindaje militar angular en gris carbón y titanio pulido, hombreras con lanzaderas de micro-misiles cerradas, cañón de plasma pesado integrado en el antebrazo derecho, reactor cuántico resplandeciente en el centro del pecho y grandes rejillas de ventilación de disipación de calor en la espalda.

### A. Kling AI / Google Flow (Modo Turntable 360°)
* **Positive Prompt:**
```text
Full body 360 degree turntable rotation model sheet of a heavy armored sci-fi combat mecha, the Quantum Centurion boss. A towering bipedal robotic mech stands in balanced neutral stance on an invisible rotating platform, rotating smoothly 360 degrees around its central axis. Angular reinforced dark grey and tungsten plating, exposed hydraulic leg pistons, heavy plasma beam cannon integrated into right forearm, shielded buckler on left arm, glowing cyan quantum reactor core exposed in chest center, and twin heavy heat dissipation exhaust radiators on upper back with pulsing magenta heat fins. Full body shot showing complete feet to head framing, Front view (0 deg), Side view (90 deg), and Back radiator view (180 deg). Completely static camera, locked tripod eye-level angle, zero zoom, zero pan. Solid pure chroma green screen background #00FF00 remains flat and still. High fidelity 60fps mecha concept art model sheet turn-around.
```
* **Negative Prompt:**
```text
human pilot visible, organic monster parts, running, shooting, explosions, smoke, fire, camera rotation, camera panning, zooming, blurred armor, extra limbs, floor patterns, floor shadows, morphing
```

---

## 🧠 5. FINAL BOSS: MODULAR HIVE MIND (NÚCLEO CENTRAL DE LA RED)

* **Objetivo:** Tríptico conceptual del Jefe Final: un gigantesco núcleo flotante bio-mecánico de datos corrupto con placas modulares flotantes y zarcillos de cableado de fibra óptica.
* **Referencia Visual:** Poliedro central de cristal oscuro con código binario fluyendo en su interior, placas de blindaje flotantes suspendidas por campos magnéticos que giran a su alrededor, zarcillos de datos en forma de tentáculos mecánicos colgando por debajo y un ojo neural pulsante en el centro.

### A. Kling AI / Google Flow (Modo Turntable 360°)
* **Positive Prompt:**
```text
Full body 360 degree turntable rotation model sheet of the Final Boss, the Modular Hive Mind entity. A massive floating biomechanical core hovers in the center of the frame, rotating a full 360 degrees smoothly. Central dark geometric crystalline matrix pulsing with internal neon red and cyan digital code streams, surrounded by 6 floating segmented magnetic shield plates, several hanging fiber-optic data tendrils and cybernetic cables below, and a large multi-faceted digital neural eye in the center. Shows complete Front neural core view, Side orbital plate configuration, and Rear server heatsink array. Camera is 100% static, locked eye-level framing, zero zoom, zero camera movement. Solid pure chroma green screen background #00FF00 remains flat, uniform, and still with zero shadows. High resolution 60fps sci-fi boss design model sheet.
```
* **Negative Prompt:**
```text
organic alien blood, slimy tentacles, human face, drifting away, camera zoom, camera shake, camera panning, distorted geometry, background noise, floor textures, morphing
```

---

## 🔫 6. ENERGY BLASTER (ARMA PRINCIPAL DEL JUGADOR)

* **Objetivo:** Model Sheet de 3 vistas del arma de mano (pistola de plasma táctica) para modelado de assets y visualización en primera persona con las Quest Touch Controllers.
* **Referencia Visual:** Chasis de polímero mate negro y fibra de carbono, recámara de plasma traslúcida en la empuñadura con fulgor cian neón, mirilla réflex holográfica flotante sobre el cañón con retícula proyectada y riel Picatinny táctico inferior.

### A. Kling AI / Google Flow (Modo Model Sheet 3 Vistas en Croma Verde)
* **Positive Prompt:**
```text
Three-view orthogonal weapon model sheet of a futuristic tactical energy blaster handgun on a single horizontal layout. Side profile view showing left side mechanics, top down view showing rail and holographic sight mount, and first-person iron sight rear view looking down the barrel. Matte black tactical polymer frame with carbon fiber slide accents, translucent plasma power cell magazine in grip glowing neon cyan #00FFCC, floating cyan holographic reflex red-dot sight above barrel, vented muzzle compensator, and digital ammo counter display on rear slide. Completely static studio lighting, flat 2D weapon design sheet. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and clean with zero shadows. High resolution crisp 4K hard-surface weapon concept art.
```
* **Negative Prompt:**
```text
hands holding weapon, person, holster, firing muzzle flash, smoke, bullets ejecting, camera angle perspective tilt, 3d isometric distortion, shadows on green screen, gradients, textures in background, morphing
```

---

## 🧤 7. ENERGY GAUNTLET & HOLO-SHIELD (GUANTELETE TÁCTICO Y ESCUDO)

* **Objetivo:** Tríptico de diseño del guantelete del brazo izquierdo del jugador con su emisor de escudo de energía defensivo para bloquear proyectiles en Realidad Mixta.
* **Referencia Visual:** Guantelete articulado de exoesqueleto sobre el antebrazo y nudillos en fibra de carbono y titanio, bobinas de inducción de energía en cian neón a lo largo del antebrazo y disco emisor en la muñeca que despliega un escudo hexagonal translúcido cian.

### A. Kling AI / Google Flow (Modo Tríptico 3 Vistas)
* **Positive Prompt:**
```text
Three-view prop model sheet of a futuristic sci-fi tactical combat gauntlet with deployable holographic energy shield. Shows three distinct angles: Outer dorsal forearm view showing carbon-fiber armor and neon cyan power coils, Inner palm view showing magnetic grip and power conduit, and Active shield view showing the wrist projector casting a translucent glowing cyan hexagonal energy barrier shield. High-tech exo-gauntlet design, titanium knuckle plates, glowing cyan telemetry LEDs. Flat orthogonal studio sheet, completely locked framing. Solid pure chroma green screen background #00FF00 remains totally flat, uniform, and clean with zero floor shadows. High resolution crisp hard-surface concept art.
```
* **Negative Prompt:**
```text
human arm flesh, full body, weapon firing, blurry armor, camera shake, perspective tilt, background shadows, textures on green screen, morphing
```

---

## ⚡ 8. EJECUCIÓN DE EXTRACCIÓN CON EL EXTRACTOR DEL TALLER

Una vez descargado el vídeo generado de Kling AI o Google Flow (ejemplo: `spider_drone_turntable.mp4` o `nyx_turntable.mp4`):

```powershell
# Extraer las 3 vistas ortogonales y ensamblar el tríptico automáticamente
python "c:\Users\asakura\Documents\juego meta\tools\i2v_sprite_suite\extract_turnaround_triptico.py" `
    --video "C:\Ruta\Al\Video_Descargado.mp4" `
    --output "c:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Sprites\ModelSheets\SpiderDrone" `
    --angles 3
```

El script genera al instante en la carpeta destino:
1. `view_front.png` (0° - Frente ortogonal limpio).
2. `view_side.png` (90° - Perfil lateral con despill verde).
3. `view_back.png` (180° - Espalda y detalles de reactor/mochila).
4. `model_sheet_triptico.png` (Lienzo horizontal de 2064 x 828 px con las 3 vistas alineadas, marcas de altura de cabeza, suelo y cuadrícula neón, listo para cargar en software de modelado 3D o para usar directamente en Unity).

---
*Catálogo de Prompts y Pipeline I2V sellado para El Taller de la Gatita Tech. Listo para producción, Raquel.*
