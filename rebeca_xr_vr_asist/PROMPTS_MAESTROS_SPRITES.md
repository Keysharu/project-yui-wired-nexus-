# 📜 CATÁLOGO MAESTRO DE PROMPTS: SPRITE SUITE (REBECA & MECENAS)
> **El Taller de la Gatita Tech** — Especificación de Prompts para Producción y Pedidos.  
> **Metodología Principal:** Image-to-Video (I2V) + Extracción Python / OpenCV (Ver guía exhaustiva: [`METODOLOGIA_MAESTRA_SPRITES_I2V.md`](file:///c:/Users/asakura/Documents/cyber_companion/METODOLOGIA_MAESTRA_SPRITES_I2V.md)).  
> **Fondo:** Chroma Verde Puro (#00FF00) para recorte automático y anclaje Y=767.

---

## 🏆 PROMPTS TRIUNFADORES EN VÍDEO (MÉTODO RECOMENDADO I2V)

El método de generación de vídeo Image-to-Video ha demostrado ser el más consistente y económico (45 puntos de 1010 para 3 vídeos completos), generando físicas de cabello y ropa naturales sin deriva anatómica:

### 1. Habla Frontal / Visemes (`talk_1` a `talk_5`, `happy`, `laugh`):
```text
Positive:
full body frontal shot, eye level framing. The anime character from the reference image stands firmly in place, talking directly and attentively to the viewer as a friendly AI assistant. Clear and expressive mouth movements with smooth lip-sync modulation, natural phonetic speech articulation. Subtle breathing motion, gentle head movements, occasional friendly eye blinks. Both feet remain planted firmly on the ground, stationary posture, no walking, no stepping. The camera is 100% static and locked, no zoom, no pan, no rotation. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and still with zero changes. High quality 60fps anime animation.

Negative:
walking, stepping, pacing, turning around, back view, profile view, camera zoom, camera shake, camera movement, distorted mouth, extra teeth, distorted fingers, blurry face, background change, shadows on floor, textures in background, morphing
```

### 2. Caminata Continua en Cinta 90° Derecha (`walk_1` a `walk_8`):
```text
Positive:
Full body side profile view. The anime character from the reference image walks forward steadily in place on an invisible treadmill, strictly moving towards the right side. Natural human walking stride, fluid arm swinging, subtle hair bounce synchronized with steps. Completely static 90 degree side profile camera, locked framing, no camera pans, no camera zooms, no camera rotations. Solid pure chroma green screen background #00FF00 remains completely flat, still, and uniform with zero background changes. Seamless 60fps anime animation walk cycle loop.

Negative:
turning, facing camera, front view, 3/4 view, turning head, camera rotation, camera panning, zooming, 3d perspective shift, distorted feet, multiple legs, extra limbs, background changes, floor patterns, shadows on ground, morphing
```

---

## 🧭 Reglas Generales de Generación

1. **Imagen de Referencia Obligatoria:** Vincular siempre la imagen maestra canónica (ssets/rebeca_master_reference.jpg o walk_1.png para caminatas).
2. **Dirección de Perfil:** En animaciones de desplazamiento, todos los fotogramas deben mirar estrictamente hacia la DERECHA (strictly facing right).
3. **Fondo Plano:** Forzar siempre #00FF00 sin degradados, sombras en el suelo ni elementos de fondo.

---

## 🚶 1. Ciclo de Caminata Táctica (4 Fotogramas Fluidos)

*Usar como Imagen de Referencia walk_1.png.*

### WALK_1 — Contacto Derecho
`	ext
full body side profile view strictly walking right, facing right. Contact pose: near right leg (with tactical holster) stepping forward with heel touching the ground, far left leg extended back with toe pushing off. Left arm forward, right arm back in natural walking stride. Same character, long red hair, straight hair, blunt bangs, purple eyes, black turtleneck, black techwear jacket, glowing cyan circuit patterns, high collar, dark tactical pants, cargo pockets, knee pads, black combat boots, tactical belt, leg holster, anime style, full body, on pure chroma green background #00FF00, flat color --no facing left, turn, front view
`

### WALK_2 — Pase / Cruce de Pierna 1
`	ext
full body side profile view strictly walking right, facing right, identical scale and ground line as reference image. Passing pose: supporting leg straight upright bearing weight with foot flat on ground, other leg bent at knee swinging forward in mid-air, arms relaxed at sides passing torso. Same character, long red hair, straight hair, blunt bangs, purple eyes, black turtleneck, black techwear jacket, glowing cyan circuit patterns, high collar, dark tactical pants, cargo pockets, knee pads, black combat boots, tactical belt, leg holster, anime style, full body, on pure chroma green background #00FF00, flat color --no facing left, turn, front view
`

### WALK_3 — Contacto Izquierdo (Zancada Invertida)
`	ext
full body side profile view strictly walking right, facing right, identical scale and ground line as reference image. Opposite stride pose: right leg forward with heel striking ground, left leg trailing extended back with toe pushing off, arms naturally alternated in stride. Same character, long red hair, straight hair, blunt bangs, purple eyes, black turtleneck, black techwear jacket, glowing cyan circuit patterns, high collar, dark tactical pants, cargo pockets, knee pads, black combat boots, tactical belt, leg holster, anime style, full body, on pure chroma green background #00FF00, flat color --no facing left, turn, front view
`

### WALK_4 — Pase / Cruce de Pierna 2 (Cierre del Loop)
`	ext
full body side profile view strictly walking right, facing right, identical scale and ground line as reference image. Opposite passing pose: opposite supporting leg straight upright bearing weight, other leg bent at knee swinging forward in mid-air, arms relaxed at sides passing torso. Same character, long red hair, straight hair, blunt bangs, purple eyes, black turtleneck, black techwear jacket, glowing cyan circuit patterns, high collar, dark tactical pants, cargo pockets, knee pads, black combat boots, tactical belt, leg holster, anime style, full body, on pure chroma green background #00FF00, flat color --no facing left, turn, front view
`

---

## 🎮 2. Modo Estudio: Streamer / Webcam (OBS Studio)

*Encuadre: Plano medio corto / Busto (Chest-up).*

### STREAM_IDLE — Reposo frente a Webcam
`	ext
professional live streamer seated at streaming setup, chest-up bust shot framing, facing webcam directly, relaxed friendly posture, professional condenser studio microphone on floating boom arm close to mouth, mouth closed. Same character, long red hair, blunt bangs, purple eyes, black techwear jacket, glowing cyan lines, high collar, anime style, on pure chroma green background #00FF00, flat color
`

### STREAM_TALK — Hablando por Micrófono (Lip-Sync Pair)
`	ext
professional live streamer seated at streaming setup, exact identical chest-up framing, shoulders, neck, clothes and floating boom microphone as previous shot, professional live streamer facing webcam, mouth open speaking clearly into studio condenser microphone, lip-sync pair. Same character, long red hair, blunt bangs, purple eyes, black techwear jacket, glowing cyan lines, anime style, on pure chroma green background #00FF00, flat color
`

### STREAM_HYPE — Celebración / Reacción
`	ext
professional live streamer in identical streaming setup with boom microphone, chest-up bust shot, energetic cheerful celebration pose, smiling warmly and waving one hand to stream chat. Same character, long red hair, blunt bangs, purple eyes, black techwear jacket, glowing cyan lines, anime style, on pure chroma green background #00FF00, flat color
`

---

## 🎙️ 3. Modo Estudio: Reportera de Campo (Con Micro)

### MIC_IDLE — De pie con micrófono en mano
`	ext
live news reporter holding a broadcast microphone to chest level, medium shot, looking at camera, mouth closed, neutral attentive expression, professional tactical outfit, same character, red hair, cyan neon jacket, on pure chroma green background #00FF00, flat color
`

### MIC_TALK — Hablando por el micrófono
`	ext
exact identical body posture and camera framing as previous shot, live news reporter holding a broadcast microphone to chest level, mouth open speaking into mic, broadcasting news, lip-sync pair, same character, red hair, cyan neon jacket, on pure chroma green background #00FF00, flat color
`

---

## 📺 4. Modo Estudio: Telediario (Mesa de Noticias)

### DESK_IDLE — Sentada en la mesa
`	ext
television news anchor seated behind a newsroom desk, formal posture, hands resting on desk, facing camera directly, mouth closed, professional studio broadcast, same character, red hair, cyan neon jacket, on pure chroma green background #00FF00, flat color
`

### DESK_TALK — Presentando noticias (Boca abierta)
`	ext
exact identical body posture and desk position as previous shot, television news anchor seated behind newsroom desk, facing camera directly, mouth open speaking clearly into teleprompter, lip-sync pair, same character, on pure chroma green background #00FF00, flat color
`

### DESK_EXPLAIN — Señalando gráficos
`	ext
television news anchor seated behind desk, gesturing with right hand pointing sideways to floating news graphic, presenting news story, same character, on pure chroma green background #00FF00, flat color
`

---

## 🐱 5. Cyber-Companion Mascota (Estados Emocionales)

- **IDLE:** ull body character sprite, frontal view, neutral expression, closed mouth, relaxed standing posture, same character, dark tactical jumpsuit, glowing cyan lines, on pure chroma green background #00FF00
- **TALK:** ull body character sprite, exact identical body posture as IDLE, mouth open talking, lip-sync pair, same character, on pure chroma green background #00FF00
- **THINK:** ull body character sprite, typing on floating glowing holographic virtual keyboard, analytical posture, same character, on pure chroma green background #00FF00
- **SLEEP:** full body character sprite, cute chibi or resting posture, sitting curled up sleeping peacefully, dimmed circuits, same character, on pure chroma green background #00FF00
- **MUSIC:** full body character sprite, wearing sleek glowing neon over-ear headphones, head bobbing happily, same character, on pure chroma green background #00FF00

---

## 🎬 6. Protocolo Táctico I2V (Image-to-Video) — Caminata Cinemática Continua

> **Ventaja de la Técnica:** Elimina al 100% las mutaciones y cambios de escala entre fotogramas. Se sube la imagen de perfil canónica (`assets/rebeca_walk_reference.jpg`) a un generador de vídeo IA (Kling AI, Runway Gen-3, Luma Dream Machine o Minimax) y se extraen los fotogramas limpios mediante `tools/extract_walk_frames.py`.

### A. Prompt para Kling AI (Recomendado — Modo Image-to-Video)
- **Imagen de Entrada:** `assets/rebeca_walk_reference.jpg` (Chroma verde #00FF00).
- **Ajustes:** Motion / Creatividad: 4 / 10 | Cámara: Estática (Static / Lock).
- **Prompt:**
```text
Full body side profile view. The anime character from the reference image walks forward steadily in place on an invisible treadmill, strictly moving towards the right side. Natural human walking stride, fluid arm swinging, subtle hair bounce synchronized with steps. Completely static 90 degree side profile camera, locked framing, no camera pans, no camera zooms, no camera rotations. Solid pure chroma green screen background #00FF00 remains completely flat, still, and uniform with zero background changes. Seamless 60fps anime animation walk cycle loop.
```
- **Negative Prompt:**
```text
turning, facing camera, front view, 3/4 view, turning head, camera rotation, camera panning, zooming, 3d perspective shift, distorted feet, multiple legs, extra limbs, background changes, floor patterns, shadows on ground, morphing
```

### B. Prompt para Runway Gen-3 Alpha / Luma Dream Machine
- **Prompt:**
```text
[Camera]: Static, fixed side view 90 degree angle.
[Subject]: The anime girl from the reference image walks steadily in place towards the right side on a treadmill. Natural human stride cycle, alternating leg movements, smooth arm swings, slight bounce with each step.
[Lighting & Background]: Perfectly flat solid green chroma screen #00FF00, consistent even lighting, no shadows on floor, no background shifts, seamless walk cycle loop.
```

### C. Prompt para Hailuo AI / Minimax
- **Prompt:**
```text
The girl in the image walks forward at a steady pace facing right in side profile view, walking in place on a treadmill. Natural walk cycle loop, side view, fixed camera position, solid green screen background #00FF00, anime style.
```

---

## 🛠️ 7. Extracción y Despliegue Automático en Cyber-Companion

Una vez descargado el clip `.mp4` de 5 a 10 segundos:

```powershell
python tools/extract_walk_frames.py --video "C:\Ruta\Tu_Video.mp4" --frames 8 --start 1.0 --cycle 1.2
```

El script se encarga de:
1. Analizar el vídeo y recortar el ciclo de zancada.
2. Generar `walk_cycle_sheet.png` (hoja de contacto de 8 poses para verificación).
3. Eliminar el fondo verde mediante segmentación neuronal (`rembg`).
4. Generar `walk_cycle_preview.gif` para validar el bucle animado a 8 FPS.
5. Guardar directamente los sprites en `assets/sprites/walk_1.png` a `walk_8.png`.

---

## 🎙️ 8. Protocolo Táctico I2V (Image-to-Video) — Presentadora OBS / Streamer Webcam

> **Ventaja de la Técnica:** Proporciona una sincronización labial (lip-sync) de nivel profesional. En vez de alternar mecánicamente entre 2 imágenes estáticas, extraemos fotogramas reales de modulación bucal (visemas) y expresiones naturales de webcam (parpadeos, inclinaciones sutiles de cabeza y gestos con el chat) sin que el cuerpo ni el fondo muten.

### Imagen de Entrada Recomendada:
- Imagen frontal o plano medio busto de Rebeca (`assets/rebeca_master_reference.jpg` o un render estático con micrófono `stream_idle`).

---

### A. Prompt para Kling AI (Modo Image-to-Video — Plató Streamer)
- **Ajustes:** Modo Image-to-Video | Creatividad / Motion: 3 a 5 | Cámara: Lock / Static.
- **Prompt:**
```text
Frontal chest-up bust shot, webcam point-of-view framing. The anime character from the reference image is seated at a live streaming battle station, speaking expressively and warmly directly into the webcam lens. A professional studio condenser microphone on a floating boom arm is positioned near her mouth. Natural mouth movement with clear phonetic articulation, smooth lip-sync modulation, subtle friendly head tilts, natural eye blinks, lively VTuber streamer expressions. The camera remains 100% static, locked webcam angle, no camera pan, no zoom, no rotation. Solid pure chroma green screen background #00FF00 remains completely flat, uniform and still with zero changes. High quality 60fps anime streaming broadcast.
```
- **Negative Prompt:**
```text
turning away from camera, looking down, full body zoom out, standing up, camera panning, camera shaking, 3d camera orbit, distorted mouth, extra teeth, distorted eyes, blurry lips, changing microphone position, background patterns, shadows, room background, morphing
```

---

### B. Prompt para Runway Gen-3 Alpha / Luma Dream Machine
- **Prompt:**
```text
[Camera]: Static, fixed frontal webcam angle, chest-up bust shot framing, locked position.
[Subject]: The anime girl from the reference image is a live streamer speaking warmly to the webcam. Natural human speech lip-sync, mouth opening and closing clearly with phonetic articulation, subtle rhythmic head nods, occasional friendly blinks, professional condenser boom mic in front.
[Lighting & Background]: Perfectly flat solid chroma green screen #00FF00, consistent studio lighting, zero background movement or textures.
```

---

### C. Prompt para Hailuo AI / Minimax
- **Prompt:**
```text
Anime girl streamer seated facing webcam in chest-up framing, talking directly to stream chat, speaking clearly into studio boom microphone. Natural lip-sync mouth movements, blinking, friendly head tilts. Fixed camera position, solid green screen background #00FF00.
```

---

## 🛠️ 9. Extracción Automática de Visemas para OBS

Una vez generado y descargado el vídeo `.mp4` de la streamer hablando (5 a 10 segundos):

```powershell
python tools/extract_presenter_frames.py --video "C:\Ruta\rebeca_stream.mp4" --style streamer
```

El script se encarga automáticamente de:
1. Analizar el vídeo e identificar los estados clave del habla.
2. Extraer `stream_idle.png` (boca cerrada, mirada atenta).
3. Extraer `stream_talk_1.png`, `stream_talk_2.png`, `stream_talk_3.png` (diferentes aperturas fonéticas).
4. Extraer `stream_hype.png` (sonrisa y reacción viva para el chat).
5. Generar `stream_contact_sheet.png` (hoja de contacto para inspección).
6. Aplicar segmentación neuronal (`rembg` / `bria-rmbg`) eliminando el croma verde.
7. Generar `stream_speech_preview.gif` simulando la animación en bucle.
8. Desplegar los sprites en `assets/sprites/` para que `cyber_companion/main.py` los active automáticamente en el menú *Modo Presentadora / OBS Studio -> Estilo Streamer*.

---

## 🗣️ 10. Protocolo Táctico I2V — Conversación de Mascota (Talk & Idle Básico)

> **Objetivo:** Conseguir un habla orgánica de cuerpo entero o 3/4 cuando el operador le hace preguntas a Rebeca y ella responde en el escritorio (modo mascota estándar).

### Imagen de Entrada Preparada:
- **Archivo:** [`assets/rebeca_talk_reference_green.png`](file:///c:/Users/asakura/Documents/cyber_companion/assets/rebeca_talk_reference_green.png)
- **Formato:** 720x1280 (9:16 vertical), encuadre 3/4 cuerpo erguido sobre croma verde puro `#00FF00`.

---

### A. Prompt para Kling AI (Modo Image-to-Video — Habla de Mascota)
- **Ajustes:** Modo Image-to-Video | Creatividad / Motion: 3 a 4 | Cámara: Lock / Static.
- **Prompt Positivo:**
```text
Full body frontal shot, eye level framing. The anime character from the reference image stands firmly in place, talking directly and attentively to the viewer as a friendly AI assistant. Clear and expressive mouth movements with smooth lip-sync modulation, natural phonetic speech articulation. Subtle breathing motion, gentle head movements, occasional friendly eye blinks. Both feet remain planted firmly on the ground, stationary posture, no walking, no stepping. The camera is 100% static and locked, no zoom, no pan, no rotation. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and still with zero changes. High quality 60fps anime animation.
```
- **Negative Prompt:**
```text
walking, stepping, pacing, turning around, back view, profile view, camera zoom, camera shake, camera movement, distorted mouth, extra teeth, distorted fingers, blurry face, background change, shadows on floor, textures in background, morphing
```

---

### B. Prompt para Runway Gen-3 Alpha / Luma Dream Machine
- **Prompt:**
```text
[Camera]: Static, fixed eye-level camera, locked framing.
[Subject]: The anime girl from the reference image stands stationary, speaking directly to the operator in a friendly conversational manner. Natural lip-sync articulation, mouth opening and modulating words, subtle head nods, natural blinking, relaxed posture with feet planted on the floor.
[Lighting & Background]: Perfectly flat solid green chroma screen #00FF00, uniform lighting, zero background movement or textures.
```

---

### C. Prompt para Hailuo AI / Minimax
- **Prompt:**
```text
Full body anime girl standing stationary facing forward, speaking clearly to the camera with natural lip-sync mouth movement and friendly expressions. Fixed camera, solid pure green background #00FF00, smooth animation.
```

---

## 🛠️ 11. Extracción y Despliegue del Habla de Mascota

Una vez descargado el vídeo (ej. `rebeca_talk.mp4`):

```powershell
python tools/extract_talk_frames.py --video "C:\Ruta\rebeca_talk.mp4"
```

El script extrae y despliega automáticamente:
- `idle.png` (boca cerrada en reposo)
- `talk_1.png`, `talk_2.png`, `talk_3.png` (visemas de habla)
- `talk.png` (respaldo estándar)
- `happy.png` (expresión risueña al interactuar)
- `companion_talk_preview.gif` (comprobación de animación)




---

## 🎮 12. Suite Completa de Emociones: Plató Streamer / Webcam (OBS Studio)

> **Regla de Oro de Plató:** En todas las generaciones Image-to-Video de esta suite, el plano inferior que contiene el **escritorio de gaming negro, el teclado mecánico y el micrófono de condensador con brazo articulado flotante** debe permanecer 100% inmóvil y anclado al fondo para garantizar cero saltos visuales al alternar estados en OBS Studio.  
> **Imagen de Entrada I2V Canónica:** [`assets/rebeca_streamer_reference_green.png`](file:///c:/Users/asakura/Documents/cyber_companion/assets/rebeca_streamer_reference_green.png) (720x1280 vertical, croma verde puro `#00FF00`, anclaje $Y=767$).

---

### 12.1 STREAM_HAPPY / STREAM_HYPE — Celebración y Saludo al Chat

#### A. Kling AI (Modo Image-to-Video)
- **Imagen de Entrada:** `assets/rebeca_streamer_reference_green.png`
- **Ajustes:** Modo I2V | Motion / Creatividad: 4 / 10 | Cámara: Lock / Static | Calidad: High (1080p).
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image celebrates cheerfully at her streaming desk. She raises one hand waving warmly and enthusiastically to the stream chat, smiling with genuine joy and radiant eyes. The gaming desk, mechanical keyboard, and scissor-arm condenser microphone remain completely static and locked in the foreground. The camera is 100% static, locked tripod webcam angle, no camera pan, no zoom, no tilt. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and still with zero changes. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
standing up, moving desk, shifting microphone, camera movement, camera zoom, camera pan, 3d orbit, morphing hands, extra fingers, distorted eyes, changing clothes, shadows on green screen, textures in background, glitch
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static, fixed webcam angle, chest-up bust framing, zero camera movement.
[Subject]: The anime streamer girl from the reference image cheerfully celebrates at her streaming desk, waving one hand happily to her audience with a bright beaming smile. The desk, mechanical keyboard, and condenser boom microphone stay completely still and stationary.
[Lighting & Background]: Perfectly flat solid green chroma screen #00FF00, studio lighting, zero background movement or textures.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer seated at desk waving happily to chat with warm smile, chest-up webcam framing. Static camera, locked gaming desk and boom mic in foreground, solid green screen background #00FF00.
```

---

### 12.2 STREAM_LAUGH — Risa a Carcajadas ante el Chat

#### A. Kling AI (Modo Image-to-Video)
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image laughs out loud heartily at a funny joke from stream chat. Her head tilts slightly back with joyous laughter, eyes curved into happy closed arcs, wide open laughing smile, subtle shoulder shaking from chuckling. The foreground gaming desk, mechanical keyboard, and floating boom microphone remain completely motionless and locked in place. Camera is 100% static, locked webcam framing, zero zoom, zero pan. Solid pure chroma green screen background #00FF00 remains totally flat, uniform, and still. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
standing up, looking away, covering face completely, moving microphone, camera shake, camera zoom, extra teeth, distorted mouth, changing clothes, background noise, floor shadows, morphing
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static locked webcam framing, chest-up bust shot, no camera motion.
[Subject]: The anime streamer girl from the reference image bursts into joyful laughter at her streaming desk, head tilted slightly back, eyes closed happily, genuine beaming laugh, subtle chuckling body bounce. Desk and studio condenser mic remain 100% fixed and motionless.
[Lighting & Background]: Solid pure green screen background #00FF00, flat studio lighting, zero texture changes.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer laughing joyfully at desk, eyes closed in cheerful expression, genuine happy laugh. Static camera, stationary boom mic and desk, solid green screen #00FF00.
```

---

### 12.3 STREAM_MOVIE — Sesión de Cine / Reacción a Vídeo

#### A. Kling AI (Modo Image-to-Video)
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image is watching a cinematic movie stream. She is wearing stylish retro-cyberpunk 3D glasses with one cyan lens and one red-magenta lens. A popcorn bucket rests neatly on the gaming desk next to the keyboard. She gazes attentively forward at the screen in fascinated wonder, occasionally taking a piece of popcorn, subtle captivated breathing, eyes shining through the tinted 3D lenses. Foreground desk, keyboard, popcorn bucket, and scissor-arm microphone stay firmly grounded and stationary. Camera is 100% static, locked webcam angle, zero pan, zero zoom. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and motionless. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
standing up, taking off glasses, spilling popcorn, moving desk, camera shake, camera zoom, shifting microphone, distorted hands, background changes, cinema hall background, morphing
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static locked webcam framing, chest-up framing, zero camera movement.
[Subject]: The anime streamer girl from the reference image wears cyan and magenta 3D glasses watching a movie screen with captivated wonder, a striped popcorn bucket on her desk. Calm attentive reaction, subtle breathing, desk and boom microphone completely stationary.
[Lighting & Background]: Perfectly flat chroma green screen #00FF00, even studio lighting, zero background movement.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer seated at desk wearing cyan-red 3D glasses watching movie in wonder, popcorn bucket on desk. Static webcam shot, motionless desk and microphone, solid green screen background #00FF00.
```

---

### 12.4 STREAM_MUSIC — Disfrutando de Música / Ritmo en Plató

#### A. Kling AI (Modo Image-to-Video)
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image wears sleek black over-ear techwear headphones with softly pulsing neon cyan LED light rings. She is vibing and grooving happily to music, head bobbing gently and rhythmically to the beat, eyes relaxed with a content smile, shoulders swaying softly. The gaming desk, mechanical keyboard, and studio boom microphone stay completely stationary in the foreground. Camera is 100% static, locked webcam angle, zero pan, zero zoom. Solid pure chroma green screen background #00FF00 remains completely flat, still, and uniform. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
standing up, dancing frantically, taking off headphones, camera shake, camera movement, moving microphone, distorted ears, changing hair color, background disco lights, morphing
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static locked webcam shot, chest-up framing, no camera motion.
[Subject]: The anime streamer girl from the reference image wears glowing cyan techwear headphones, bobbing her head rhythmically to the music with a peaceful happy smile. Desk, keyboard, and condenser mic stay perfectly still.
[Lighting & Background]: Uniform chroma green screen #00FF00, consistent studio lighting, zero background changes.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer wearing glowing cyan headphones vibing and bobbing head to music at desk, relaxed smiling expression. Fixed camera, locked desk and boom mic, solid green screen #00FF00.
```

---

### 12.5 STREAM_SLEEP — Pausa / Descanso sobre el Escritorio

#### A. Kling AI (Modo Image-to-Video)
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image is resting peacefully at her streaming station. Her eyes are gently closed in deep serene sleep, head tilted softly, calm rhythmic chest breathing, relaxed sleeping facial expression, circuits on her dark jacket glowing very dimly in standby mode. A subtle small glowing cyan holographic 'zZZ' icon floats gently above her shoulder. The foreground desk, mechanical keyboard, and studio microphone remain completely static and fixed in place. Camera is 100% static, locked tripod framing, zero zoom, zero pan. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and still. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
waking up, opening eyes, sudden movement, standing up, camera shake, camera zoom, moving microphone, distorted face, background changes, morphing
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static, fixed webcam angle, chest-up bust framing, zero camera motion.
[Subject]: The anime streamer girl from the reference image sleeps peacefully at her streaming desk, eyes closed, gentle calm breathing, peaceful relaxed face, dimmed techwear jacket circuits, floating cyan zZZ hologram. Desk and condenser mic remain completely stationary.
[Lighting & Background]: Perfectly flat chroma green screen #00FF00, soft even lighting, zero background movement.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer sleeping peacefully at streaming desk, closed eyes, calm breathing, subtle floating cyan zZZ hologram. Fixed camera, stationary desk and boom mic, solid green screen #00FF00.
```

---

### 12.6 STREAM_THINK — Análisis Táctico / Tecleando en el Escritorio

#### A. Kling AI (Modo Image-to-Video)
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image is in deep tactical focus at her streaming battle station. She types rapidly and skillfully on the mechanical keyboard with subtle hand movement, one hand briefly touching her chin in thoughtful analysis. A subtle translucent glowing cyan HUD holographic data overlay floats near her side showing streaming analytics and code graphs. Focused intelligent gaze, slight thoughtful head tilt. The gaming desk, mechanical keyboard, and boom microphone stay firmly locked in the foreground. Camera is 100% static, locked webcam angle, zero pan, zero zoom. Solid pure chroma green screen background #00FF00 remains completely flat and uniform. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
standing up, moving desk, camera shake, camera zoom, distorted fingers, extra hands, messy typing, moving microphone, background room, morphing
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static locked webcam shot, chest-up framing, no camera motion.
[Subject]: The anime streamer girl from the reference image is focused in analytical thought, hand on chin while typing on mechanical keyboard, subtle floating cyan holographic HUD data visor. Desk and condenser mic stay perfectly still.
[Lighting & Background]: Solid green screen background #00FF00, clean studio lighting, zero background shifts.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer thinking deeply and typing on keyboard at desk, hand on chin, focused expression, cyan HUD hologram. Fixed camera, locked desk and mic, solid green screen #00FF00.
```

---

### 12.7 STREAM_SURPRISE — Alerta de Stream / Jumpscare

#### A. Kling AI (Modo Image-to-Video)
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image reacts in sudden shock and amazement to a stream jumpscare or massive raid alert. Her eyes open wide with dilated pupils, eyebrows raised high, mouth dropping open in a small 'O' shape of astonishment, hands instinctively pulling back towards chest in surprise. A glowing cyan holographic exclamation mark '!' flashes briefly above her head. The desk, mechanical keyboard, and boom microphone remain completely fixed and grounded in the foreground. Camera is 100% static, locked webcam angle, zero pan, zero zoom. Solid pure chroma green screen background #00FF00 remains flat, uniform, and still. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
standing up, falling backward, running away, moving desk, shifting microphone, camera shake, camera zoom, distorted eyes, blurry face, background changes, morphing
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static, fixed webcam angle, chest-up bust framing, zero camera movement.
[Subject]: The anime streamer girl from the reference image is startled in comic surprise by a stream alert, wide eyes, raised eyebrows, mouth open in 'O' shape, hands pulled to chest, small cyan holographic exclamation mark. Desk and condenser mic remain completely stationary.
[Lighting & Background]: Perfectly flat chroma green screen #00FF00, studio lighting, zero background motion.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer reacting in shock and surprise to stream alert, wide eyes, open mouth, startled comic expression, floating cyan exclamation mark. Fixed camera, locked desk and mic, solid green screen #00FF00.
```

---

### 12.8 STREAM_POUT — Puchero Tsundere / Ofendida Cómica

#### A. Kling AI (Modo Image-to-Video)
- **Prompt Positivo:**
```text
Frontal chest-up webcam shot. The anime streamer girl from the reference image pouts comically at the stream chat. Her cheeks are adorably puffed with vivid anime blush lines, cute downturned pouty mouth, eyebrows slightly furrowed in playful annoyance, looking slightly sideways away from camera with arms crossed over the desk. The gaming desk, mechanical keyboard, and boom microphone remain completely static and anchored in the foreground. Camera is 100% static, locked webcam angle, zero pan, zero zoom. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and still. High quality 60fps anime animation.
```
- **Prompt Negativo:**
```text
standing up, smiling, laughing, moving desk, shifting microphone, camera zoom, camera shake, distorted mouth, changing clothes, background noise, morphing
```

#### B. Runway Gen-3 Alpha / Luma Dream Machine
```text
[Camera]: Static locked webcam shot, chest-up bust framing, no camera motion.
[Subject]: The anime streamer girl from the reference image does a cute anime pout at stream chat, puffed blushing cheeks, downturned mouth, arms crossed over desk, looking sideways in playful annoyance. Desk and boom mic stay perfectly still.
[Lighting & Background]: Solid green chroma screen #00FF00, flat studio lighting, zero background shifts.
```

#### C. Minimax / Hailuo AI
```text
Anime girl streamer cute tsundere pout at chat, puffed blushing cheeks, crossed arms on desk, annoyed expression. Fixed camera, locked desk and boom mic, solid green screen #00FF00.
```

---

## 📺 13. Suite Completa de Emociones: Plató Telediario (Mesa de Noticias)

> **Encuadre:** Plano medio formal tras la mesa de noticias del informativo (*news anchor desk*).  
> **Consistencia:** La mesa de telediario y las manos apoyadas en ella marcan la estabilidad visual en $Y = 767$.

### 13.1 DESK_IDLE / DESK_TALK — Informativo en Directo
```text
Positive:
Television news anchor seated behind a modern newsroom desk, medium shot framing, facing the camera directly. The anime girl from the reference image delivers news professionally, wearing sleek black techwear blazer with glowing cyan lines. Attentive neutral posture, hands resting formally on the desk, natural mouth movements with clear articulate speech lip-sync, occasional calm eye blinks. The news desk stays completely motionless in the foreground. Camera is 100% static, locked studio tripod framing, zero zoom, zero pan. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and still. High quality 60fps anime animation.

Negative:
standing up, leaving desk, casual slumping, camera pan, camera zoom, camera shake, distorted mouth, distorted fingers, background studio screens, morphing
```

### 13.2 DESK_HAPPY — Buenas Noticias / Cierre Festivo
```text
Positive:
Television news anchor seated behind the newsroom desk, medium shot. The anime girl from the reference image smiles warmly and cheerfully as she delivers uplifting good news or closes the broadcast. A graceful nod of the head, bright friendly eyes, professional welcoming smile. Hands resting neatly on the desk. News desk remains completely stationary. Camera 100% static locked shot. Solid pure chroma green screen background #00FF00. High quality 60fps anime animation.

Negative:
standing up, waving wildly, moving desk, camera zoom, camera shake, distorted eyes, background shifts, morphing
```

### 13.3 DESK_SURPRISE — Última Hora / Breaking News
```text
Positive:
Television news anchor seated behind newsroom desk, medium shot. The anime girl from the reference image receives urgent breaking news. Her posture straightens sharply, eyes wide with alert urgency, attentive professional shock, mouth slightly parted, looking intently into the camera as a red and cyan 'BREAKING NEWS' alert graphic flashes subtly near the desk corner. Desk remains firmly locked in place. Camera 100% static locked tripod. Solid pure chroma green screen background #00FF00. High quality 60fps anime animation.

Negative:
panicking, ducking, leaving desk, camera shake, camera zoom, blurred face, background patterns, morphing
```

### 13.4 DESK_THINK — Análisis Táctico / Revisión de Datos
```text
Positive:
Television news anchor seated behind newsroom desk, medium shot. The anime girl from the reference image reviews tactical data on a sleek transparent holographic tablet resting on the desk. She gazes down thoughtfully at the data, one finger tapping gently on the desk, intelligent analytical expression, looking back up to camera. News desk stays completely stationary. Camera 100% static locked framing. Solid pure chroma green screen background #00FF00. High quality 60fps anime animation.

Negative:
standing up, moving desk, camera movement, distorted hands, background noise, morphing
```

---

## 🎙️ 14. Suite Completa de Emociones: Plató Reportera (Micrófono en Mano)

> **Encuadre:** Plano americano o plano medio de pie sosteniendo un micrófono de reportaje con logotipo cibernético.  
> **Consistencia:** Micrófono sostenido a la altura del pecho con una mano, mirando a cámara.

### 14.1 MIC_IDLE / MIC_TALK — Conexión en Directo
```text
Positive:
Live field news reporter standing outdoors, medium shot framing, facing the camera directly. The anime girl from the reference image holds a sleek black broadcast microphone with glowing cyan logo to chest level with her right hand. She speaks clearly and articulately into the microphone with natural lip-sync modulation, engaging friendly expression, subtle natural posture shift while standing firmly in place. Microphone stays firmly in her hand. Camera is 100% static, locked tripod framing, zero zoom, zero pan. Solid pure chroma green screen background #00FF00 remains completely flat, uniform, and still. High quality 60fps anime animation.

Negative:
walking, running, dropping microphone, camera shake, camera pan, camera zoom, distorted fingers, distorted mouth, street background, morphing
```

### 14.2 MIC_HAPPY — Crónica de Victoria / Cobertura Especial
```text
Positive:
Live field news reporter standing, medium shot framing. The anime girl from the reference image reports on an exciting victory or celebration, holding the broadcast microphone to chest level. Vibrant cheerful smile, energetic friendly enthusiasm, slight joyous head tilt, expressive joyful eyes. Both feet stay planted firmly on the ground. Static locked camera, zero zoom, zero pan. Solid pure chroma green screen background #00FF00. High quality 60fps anime animation.

Negative:
jumping around, running, camera movement, dropping mic, distorted hands, background noise, morphing
```

### 14.3 MIC_SURPRISE — Suceso Inesperado en Directo
```text
Positive:
Live field news reporter standing, medium shot framing. The anime girl from the reference image turns her head slightly in shock at an unexpected live event, then looks back to the camera with wide astonished eyes and parted mouth, holding the broadcast microphone tightly to chest level. Alert dynamic posture while staying firmly in place. Camera 100% static locked tripod. Solid pure chroma green screen background #00FF00. High quality 60fps anime animation.

Negative:
running away, dropping mic, camera shake, camera zoom, blurred facial features, background crowds, morphing
```

---

## 🐱 15. Suite Completa de Emociones: Cyber-Companion Mascota (Cuerpo Entero)

> **Imagen de Entrada:** [`assets/rebeca_talk_reference_green.png`](file:///c:/Users/asakura/Documents/cyber_companion/assets/rebeca_talk_reference_green.png) o [`assets/rebeca_master_reference.jpg`](file:///c:/Users/asakura/Documents/cyber_companion/assets/rebeca_master_reference.jpg).  
> **Anclaje:** Pies firmemente plantados sobre el suelo horizontal en $Y = 767$.

### 15.1 SLEEP — Modo Reposo / Standby Mascota
```text
Positive:
Full body shot, eye-level framing. The anime girl from the reference image is curled up sitting peacefully in a cute resting posture on the floor, sleeping soundly. Her eyes are closed serenely, slow gentle breathing, circuits on her black tactical jumpsuit dimmed to a soft gentle glow. Both feet resting on the ground, stationary posture. Camera is 100% static, locked tripod angle, zero zoom, zero pan. Solid pure chroma green screen background #00FF00 remains completely flat and still. High quality 60fps anime animation.

Negative:
standing up, walking, camera movement, camera zoom, distorted limbs, shadows on ground, morphing
```

### 15.2 MUSIC — Modo Música Mascota
```text
Positive:
Full body shot, eye-level framing. The anime girl from the reference image stands firmly in place, wearing glowing cyan over-ear techwear headphones. She grooves happily to music, body and head bobbing rhythmically to the beat, weight shifting subtly between feet while remaining planted in place, joyful relaxed smile. Camera is 100% static, locked tripod framing, zero zoom, zero pan. Solid pure chroma green screen background #00FF00. High quality 60fps anime animation.

Negative:
walking, dancing away, turning around, camera pan, camera zoom, distorted headphones, morphing
```

### 15.3 MOVIE — Modo Cine Mascota
```text
Positive:
Full body shot, eye-level framing. The anime girl from the reference image is seated comfortably on a low tactical crate, holding a striped popcorn bucket in both hands, wearing retro cyan and red 3D glasses. She gazes forward in fascinated awe at an invisible movie screen, occasionally popping a popcorn into her mouth, captivated expression. Camera is 100% static, locked tripod framing, zero zoom. Solid pure chroma green screen background #00FF00. High quality 60fps anime animation.

Negative:
standing up, walking, spilling popcorn, camera movement, cinema background, morphing
```

---

## 🛠️ 16. Protocolo Táctico de Extracción con `extract_emotion_frame.py`

Una vez descargado el clip `.mp4` (de 5 a 10 segundos) de cualquiera de los estados generados con estos prompts:

### Paso 1: Inspeccionar los fotogramas del vídeo (Opcional)
Para generar una tira de contacto con 10 fotogramas numerados y elegir visualmente el mejor segundo:
```powershell
python tools/extract_emotion_frame.py --video "C:\Ruta\Tu_Video.mp4" --strip
```
Se guardará un archivo `strip_Tu_Video.jpg` en la carpeta raíz para inspección inmediata.

### Paso 2: Extraer y recortar el sprite al vuelo
Para extraer automáticamente el momento de mayor expresividad y desplegarlo en el motor:
```powershell
# Ejemplo para Modo Streamer:
python tools/extract_emotion_frame.py --video "C:\Ruta\rebeca_movie.mp4" --state movie --style streamer

# Ejemplo especificando fotograma exacto (ej. fotograma 48):
python tools/extract_emotion_frame.py --video "C:\Ruta\rebeca_laugh.mp4" --state laugh --style streamer --frame 48

# Ejemplo para Plató Telediario:
python tools/extract_emotion_frame.py --video "C:\Ruta\rebeca_news.mp4" --state happy --style telediario

# Ejemplo para Plató Reportera:
python tools/extract_emotion_frame.py --video "C:\Ruta\rebeca_field.mp4" --state surprise --style reportera
```

El script se encarga de:
1. Eliminar el croma verde `#00FF00` con antialiasing y despill subpíxel.
2. Centrar y normalizar la imagen al lienzo maestro de 688x768 px.
3. Fijar el anclaje del escritorio o suelo exactamente a $Y = 767$.
4. Guardar directamente el archivo resultante en `assets/sprites/<prefijo><estado>.png`.
5. Generar una imagen comparativa lado a lado con `stream_idle.png` para certificar la estabilidad milimétrica de la escenografía.


---

## 🎯 17. HABILIDAD DE ACCIÓN TÁCTICA: NEUTRALIZACIÓN Y DESTRUCCIÓN DE ARCHIVOS (`PetState.SHOOT`)

### A. Especificación y Metodología
- **Propósito:** Permitir a Rebeca localizar cualquier archivo en el escritorio, desplazarse caminando en 2D hasta él, desenfundar su arma cibernética, disparar un rayo láser al suelo/icono, enviar el archivo de forma segura a la Papelera de Reciclaje de Windows (`SHFileOperationW`) y retornar a su puesto.
- **Vídeo Maestro I2V:** `Anime_girl_drawing_cyberpunk_gun_20261005035551.mp4` (1280x720 @ 24 fps, 10 segundos).
- **Lienzo de Bóveda:** $768 \times 768$ píxeles, con anclaje al suelo a $Y = 767$ y despill espectral.

### B. Prompt de Generación I2V
* **Positive:**
```text
Full body 3/4 angle shot, eye level. The anime girl from the reference image stands firmly in place on ground level. She looks down attentively at an object on the ground, swiftly reaches down to her tactical leg holster, and unholsters a sleek futuristic cyan glowing cyberpunk energy handgun. She extends both arms aiming diagonally downward towards the floor and ground surface, strictly aiming at a target on the floor and NOT at the camera. She fires two rapid energy laser blasts downward with visible weapon recoil and bright cyan muzzle flash #00ffcc hitting the ground. After the shots, she lowers the weapon, smoothly holsters it back, looks up at the viewer and smiles with a confident friendly nod. Feet remain strictly planted firmly on the ground, stationary posture, no stepping. Completely static locked camera, zero zoom, zero panning, zero rotation. Solid pure chroma green screen background #00FF00 remains completely flat, still, and uniform with zero background changes. High quality 60fps anime animation.
```
* **Negative:**
```text
aiming at camera, shooting at viewer, pointing gun at viewer, looking straight into lens while shooting, walking, stepping forward, leaving frame, jumping, running, turning around, back view, distorted hands, extra arms, extra fingers, three legs, distorted gun, deformed face, camera shake, camera zoom, camera pan, 3d camera orbit, background change, textures on green screen, floor shadows, gradient background, morphing, jitter, blurry
```

### C. Muestreo de la Suite de 14 Fotogramas Canónicos (`shoot_1.png` a `shoot_14.png`)
| Sprite | Fotograma Vídeo | Fase Coreográfica | Comportamiento en Motor |
| :--- | :--- | :--- | :--- |
| `shoot_1.png` | Frame 53 | Mano en la cartuchera lateral | Inicio de desenfunde |
| `shoot_2.png` | Frame 65 | Elevación del arma | Transición ascendente |
| `shoot_3.png` | Frame 75 | Empuñadura táctica a dos manos | Cierre de agarre |
| `shoot_4.png` | Frame 88 | Apuntado diagonal hacia el suelo | Encarando objetivo |
| `shoot_5.png` | Frame 98 | Bloqueo de blanco (`LOCK-ON`) | Foco en el archivo |
| `shoot_6.png` | Frame 108 | Tensión previa al disparo | Anticipación |
| `shoot_7.png` | Frame 114 | **Disparo y fogonazo cian (`#00ffcc`)** | Impacto de plasma |
| `shoot_8.png` | Frame 118 | Rayo de energía impactando en el suelo | Envío de archivo a la papelera (Tick 14) |
| `shoot_9.png` | Frame 123 | Explosión de chispas en la superficie | Destrucción del blanco |
| `shoot_10.png` | Frame 132 | Disipación de humo digital | Recuperación de retroceso |
| `shoot_11.png` | Frame 142 | Descenso del arma | Postura post-disparo |
| `shoot_12.png` | Frame 152 | Inserción en cartuchera | Re-enfunde |
| `shoot_13.png` | Frame 162 | Soltando cartuchera | Retorno a reposo |
| `shoot_14.png` | Frame 175 | Guiño y sonrisa de misión cumplida | Transición a `HAPPY` |

