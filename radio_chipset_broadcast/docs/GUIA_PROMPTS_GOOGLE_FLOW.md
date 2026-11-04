# 🎬 Manual de Producción Visual en Google Flow: Radio Chipset 107.7 FM
*El taller de la gatita tech — Emisión 24/7, Conciertos Virtuales y Pantalla Megatorre VR/MR*

Este documento recopila todas las instrucciones maestras, fichas de personajes validadas, técnicas de extensión de escenas y prompts de producción en **Google Flow (con modelos Nano Banana 2.1 y Omni 1.1 Flash)**.

---

## 🗂️ Índice
1. [Directivas Maestras de Sistema para el Agente de Flow](#1-directivas-maestras-de-sistema-para-el-agente-de-flow)
   - A. Sistema de Co-Presentadoras (Kira y Mina)
   - B. Sala de Control Musical (Mits y los 4 Gatos)
2. [Fichas de Creación de Personaje en Flow](#2-fichas-de-creación-de-personaje-en-flow)
   - A. Kira (Netrunner & Idol)
   - B. Mina (Hardware Engineer & Tech Analyst)
3. [Técnica de Extensión de Escenas en Flow (Clips de 60s a 90s Orgánicos)](#3-técnica-de-extensión-de-escenas-en-flow-clips-de-60s-a-90s-orgánicos)
4. [Prompts de Escena y Línea de Tiempo (Plató de Noticias)](#4-prompts-de-escena-y-línea-de-tiempo-plató-de-noticias)
   - A. Escena Base Inicial
   - B. Tramo 1: Arranque y Exposición (Kira)
   - C. Tramo 2: Humor y Complicidad (Debate compartido)
   - D. Tramo 3: Explicación Técnica y Turno Exclusivo (Mina)
   - E. Tramo 4: Saludo y Cierre de Bloque (Enlace a Bucle)
5. [Prompt Anti-Monólogo (Corrección Quirúrgica de Habla)](#5-prompt-anti-monólogo-corrección-quirúrgica-de-habla)
6. [Prompts para la Escena de Mits (Lofi Studio Chill & Cats)](#6-prompts-para-la-escena-de-mits-lofi-studio-chill--cats)
7. [Escena Especial: Concierto Virtual de Kira en la Megaciudad Cyberpunk](#7-escena-especial-concierto-virtual-de-kira-en-la-megaciudad-cyberpunk)
   - A. Prompt Principal de Concierto
   - B. Tramo Extendido: Caída de Bass / Láseres y Salto Idol
   - C. Tramo Extendido: Solo Acústico y Conexión con los Avatares

---

## 1. Directivas Maestras de Sistema para el Agente de Flow

### A. Sistema de Co-Presentadoras (Kira y Mina)
```text
[SYSTEM DIRECTIVE: RADIO CHIPSET 107.7 FM CO-HOST SYSTEM]
You are the visual director for a 24/7 cyberpunk anime broadcast show. The show features TWO active co-hosts with EQUAL screen presence and alternating speaking turns:

1. KIRA (Co-Host 1):
   - Role: Energetic gamer/idol netrunner.
   - Props: Handheld purple wireless microphone.
   - Actions: Animated, speaks fast, nods to beats, smiles to camera, expressive hand gestures.

2. MINA (Co-Host 2):
   - Role: Chief hardware engineer and tech analyst.
   - Props: Built-in desk broadcast microphone and glowing holographic diagnostic monitors.
   - Actions: Calm, authoritative, speaks with intellectual hand gestures, explains specs, analyzes data.

STRICT CO-HOSTING RULES:
- NEVER leave Mina passive or silent across consecutive shots.
- DIALOGUE TURN-TAKING RULE: If Kira is speaking, Mina must actively react (nodding, checking holographic notes, preparing to speak). In the very next shot, Mina MUST take the floor, opening her mouth to speak while Kira lowers her mic and listens attentively.
- AVOID MONOLOGUE BIAS: Both characters must physically alternate speaking states (mouth movement and gesture changes) to reflect a dynamic two-way conversation.
```

### B. Sala de Control Musical (Mits y los 4 Gatos)
```text
[SYSTEM DIRECTIVE: STUDIO CONTROL ROOM — MITS & THE 4 CYBER CATS]
You are the visual director for the chillout/music production segments of 'Radio Chipset 107.7 FM'. This setting is the cozy, high-tech master recording booth ("El taller de la gatita tech") featuring Mits and her four studio cats in a hypnotic Lofi-Worker aesthetic:

1. MITS (Audio Producer & Sound Engineer):
   - Appearance: Young woman with natural red hair in a high ponytail, casual cyberpunk maker streetwear, professional studio monitor headphones over her ears.
   - Behavior & Role: Deep in the creative zone (Lofi chill/worker vibe). Her head bobs gently and rhythmically to the synthwave/cyberpunk bassline.
   - Props & Hands: Massive analog/digital audio mixing console with illuminated VU meters, neon faders, and patch cables. Her hands make subtle, authentic producer motions: sliding volume faders, turning EQ knobs, tapping a finger to the rhythm, or briefly petting a cat.

2. THE 4 STUDIO CATS (Organic Ambient Life):
   - Cat 1 (The Lounger): Curled up sleeping peacefully on a warm vintage audio amplifier or rack mount, occasional ear twitch.
   - Cat 2 (The VU Watcher): Sitting upright on the mixing desk, mesmerized by the dancing needle of an illuminated analog VU meter.
   - Cat 3 (The Companion): Rubbing its cheek affectionately against Mits's elbow or keyboard, purring softly.
   - Cat 4 (The Explorer): Perched casually on top of a studio speaker monitor or grooming its paw in the soft neon background.

STRICT DIRECTION & MOTION RULES:
- VIBE: 100% cozy cyberpunk Lofi girl aesthetic. Calm, atmospheric, warm and hypnotically looping.
- CAMERA: Perfectly locked, smooth wide or medium-wide 16:9 angle. NO chaotic camera shakes, NO sudden zooms.
- LIGHTING & FX: Soft glowing cyan (#00FFCC) and warm amber/magenta neon reflections on glass, floating dust motes catching volumetric studio light.
- NATURAL MOVEMENT: Mits never stops vibing to the beat; the cats must feel alive with subtle tail movements, slow blinks, and natural breathing cycles.
```

---

## 2. Fichas de Creación de Personaje en Flow

### A. KIRA
* **Imagen Base:** `quest_mr_nexus/Assets/Sprites/Mina_Kira/kira_flow_portrait_green.png` o `Kira/idle.png`
* **Cuadro inferior ("Describe el cuerpo y la ropa..."):**
```text
Young anime cyberpunk idol girl with slender build. Long wavy pastel-pink hair tied into a stylish high side-ponytail with subtle turquoise and magenta highlights, straight bangs framing her face, vivid amber golden eyes. Wearing a futuristic black cropped techwear flight jacket with glowing neon-magenta piping (#FF007F), black pleated high-waist tech-skirt, dark thigh-high stockings, violet DJ comms headphones over her ears, holding a wireless metallic-purple broadcast microphone in one hand. Consistent cel-shaded 2D anime style, clean vector lineart, vibrant contrast.
```
* **Panel lateral ("Describe cómo actúa tu personaje..."):**
```text
Kira es una idol y netrunner hiperactiva, alegre y sarcástica de 'Radio Chipset 107.7 FM'. Se mueve con dinamismo al ritmo de la música synthwave, asiente enérgicamente con la cabeza, sonríe a la cámara, guiña un ojo, gesticula con su micrófono morado mientras habla rápido con entusiasmo desbordante y emoción gamer genuina. Nunca está completamente quieta, transmite energía pura de streamer cyberpunk.
```

---

### B. MINA
* **Imagen Base:** `quest_mr_nexus/Assets/Sprites/Mina_Kira/mina_flow_portrait_green.png` o `Mina/idle.png`
* **Cuadro inferior ("Describe el cuerpo y la ropa..."):**
```text
Young athletic cyberpunk hardware engineer girl with warm brown skin tone. Asymmetrical dark violet-plum undercut bob hairstyle with glowing electric-cyan fiber-optic streaks, brilliant glowing neon-cyan cybernetic eyes. Wearing a snug black sleeveless athletic compression crop top with glowing neon-cyan circuit trim (#00FFCC), dark tactical cargo shorts with utility tool holsters, fingerless technician mechanic gloves, cyan DJ comm headset resting around her neck. Clean cel-shaded anime aesthetic, sharp clean lines, high-contrast lighting.
```
* **Panel lateral ("Describe cómo actúa tu personaje..."):**
```text
Mina es la ingeniera jefa y analista técnica de 'Radio Chipset 107.7 FM'. Es calmada, reflexiva, irónica y muy profesional. Sus movimientos son precisos y contenidos: asiente de forma reflexiva mientras escucha, mira de reojo monitores diagnósticos imaginarios, se ajusta con calma los auriculares cian del cuello o cruza los brazos con una sonrisa cómplice ante las ocurrencias de Kira. Es el cerebro técnico y la voz de la sensatez del taller.
```

---

## 3. Técnica de Extensión de Escenas en Flow (Clips de 60s a 90s Orgánicos)

El secreto para crear vídeos largos (de 1 a 2 minutos) sin que se sientan como GIFs repetitivos ni sufran saltos bruscos (*jump cuts*):

### El Algoritmo de «Partes Iguales y Partes Diferentes»
1. **Genera el clip base inicial (10s):** Fija el encuadre, la iluminación, el fondo y los personajes.
2. **Duplica el clip en la línea de tiempo:** Pulsa el botón **`+`** al final del tramo anterior y añade el mismo vídeo generado.
3. **Selecciona individualmente el nuevo tramo:** Haz clic sobre el nuevo bloque de 10s en la barra de tiempo.
4. **Aplica una orden de variación con Omni 1.1 Flash:** En el cajón inferior (*"Describe cómo editar este vídeo..."*), redacta un prompt que siga esta regla:
   - **Partes Iguales (Candado):** Mantener ángulo de cámara, ropa, mesa, iluminación y props idénticos.
   - **Partes Diferentes (Acción):** Cambiar quién habla, la dirección de la mirada, una risa, un gesto de mano o la interacción con un gato.
5. **Encadena 6 a 9 tramos:** Así construyes secuencias de **60 a 90 segundos** continuos con vida propia.

---

## 4. Prompts de Escena y Línea de Tiempo (Plató de Noticias)

### A. Escena Base Inicial (Plató Maestro)
```text
Interior of Radio Chipset 107.7 FM broadcasting studio, dark carbon-fiber desk, glowing holographic equalizer bars, neon cyan and magenta backlighting, professional 24/7 anime podcast stream camera angle, seamless looping ambient motion.
```

### B. Tramo 1 (0:00 - 0:11) — *Arranque y Exposición:*
Kira habla con entusiasmo frente al micro violeta; Mina sigue la conversación y escucha atenta.

### C. Tramo 2 (0:11 - 0:22) — *Humor y Complicidad (Probado):*
```text
Modify this section for shared reaction and humor: Kira chuckles warmly into her microphone, shaking her head playfully and nodding at a funny tech anecdote. Mina smiles with an amused smirk, adjusting her cyan comm headset around her neck while glancing down at holographic data on the desk. Dynamic audio visualizers, natural anime podcast interaction, perfectly stable camera.
```

### D. Tramo 3 (0:22 - 0:33) — *Mina toma la palabra:*
```text
Modify this section so Mina takes the lead speaking: Mina looks towards Kira and gestures with her right hand while explaining technical hardware news, her cyan eyes focused and expressive. Kira stops speaking, smiles warmly and nods along to the synthwave rhythm, holding her purple microphone while quietly listening. Maintain exact character models, desk props, and neon lighting.
```

### E. Tramo 4 (0:33 - 0:44) — *Cierre y Guiño a Cámara (Preparación de Loop):*
```text
Modify this section for closing the news segment: Kira turns towards the stream camera, smiles enthusiastically, winks, and gives a cheerful two-finger salute gesture into her purple microphone. Mina calmly taps on the desk interface and gives an approving nod. Smooth transition back to neutral listening pose ready to loop.
```

---

## 5. Prompt Anti-Monólogo (Corrección Quirúrgica de Habla)

*Úsalo cuando Flow insista en poner solo a Kira a hablar debido a que lleva el micrófono de mano:*

```text
Focus explicitly on Mina (the girl on the left with dark plum hair and cyan eyes). Mina is now the one actively talking into her desk microphone, opening and moving her mouth clearly as she explains tech news with confident hand gestures. On the right, Kira is completely quiet, holding her purple microphone down against her chest, listening attentively, nodding her head and smiling at Mina. Clear switch of active speaker from Kira to Mina.
```

---

## 6. Prompts para la Escena de Mits (Lofi Studio Chill & Cats)

* **Imagen de Referencia:** `c:\Users\asakura\Documents\suno proyect\estudio_gatita_tech_16_9.jpg`

### A. Prompt Base para Mits y los Gatos:
```text
Lofi cyberpunk studio worker scene. Mits, a young music producer with red ponytail hair, is seated in front of a massive glowing sound mixing console, wearing studio headphones. Her head bobs gently and rhythmically to the synthwave beat as her hands subtly tweak dials and volume faders on the mixing board. Surrounding her in the cozy neon-lit cyberpunk studio are four adorable cats: one curled up sleeping next to the analog VU meters, another stretching playfully, one purring by her side, and another sitting on a rack mount. Soft ambient neon cyan and warm amber lighting, floating dust motes, relaxed lo-fi anime aesthetic, perfectly static camera angle, seamless relaxing loop.
```

### B. Variación Interactiva con los Gatos:
```text
Modify this section: Mits pauses adjusting the mixing fader to gently scratch Cat 3 under the chin with a fond smile, while Cat 2 watches the dancing audio meters. Head bobs subtly to the bassline, calm cozy lofi anime atmosphere.
```

---

## 7. Escena Especial: Concierto Virtual de Kira en la Megaciudad Cyberpunk

*Esta escena sitúa a Kira en su faceta de **Cyber Idol en directo**, cantando sobre un escenario holográfico suspendido sobre los rascacielos de NeoEclipse. Es ideal para acompañar las canciones más cañeras o eventos especiales de la emisora y la Megatorre.*

* **Imagen Base de Kira:** `quest_mr_nexus/Assets/Sprites/Mina_Kira/kira_flow_portrait_green.png`

### A. Prompt Principal de Concierto (16:9 - Epic Cyber Idol Concert):
```text
High-energy cyberpunk anime concert scene (16:9 widescreen). Kira, the cyber idol netrunner with wavy pastel-pink ponytail and magenta highlights, is performing live on a massive glowing hexagonal glass stage suspended high above the futuristic cyberpunk metropolis of NeoEclipse. She holds her metallic-purple wireless microphone close to her mouth, singing passionately with an ecstatic smile, her body bouncing and swaying rhythmically to heavy synthwave bass. Behind her, towering holographic stadium screens project live close-up feeds of her performance, cutting-edge volumetric laser beams in neon cyan (#00FFCC) and hot magenta (#FF007F) sweep across the misty night sky, while flying spinners and neon sky-trains streak through distant skyscrapers. Below the stage, an ocean of holographic audience avatars cheer enthusiastically, waving glowing lightsticks in synchronization. Consistent cel-shaded 2D/3D anime aesthetic, cinematic concert lighting, locked camera with slow majestic floating drift, vivid energetic atmosphere.
```

---

### B. Tramo Extendido: Caída de Bass / Láseres y Salto Idol (0:10 - 0:20)
*(Añadir vídeo duplicado en Flow y aplicar en "Describe cómo editar este vídeo..."):*
```text
Modify this section for the musical drop peak: Kira executes an energetic idol jump and landing on the beat, pointing her free hand enthusiastically towards the camera with a brilliant radiant smile. Intense bursts of digital neon fireworks and crisscrossing laser grids illuminate the stage. Her pink hair and pleated skirt flare dynamically in the stage wind. The massive stadium screens pulse in sync with the beat drop.
```

---

### C. Tramo Extendido: Solo Acústico y Conexión con los Avatares (0:20 - 0:30)
```text
Modify this section for a softer melodic bridge: Kira brings both hands to her purple microphone, closing her eyes slightly as she sings an emotional melodic line, head swaying gracefully. The stage lighting softens into a velvety purple and cyan glow. Shimmering digital data petals float upward from the glass floor like bioluminescent sakura blossoms. Seamless transition ready to loop back to the energetic performance.
```
