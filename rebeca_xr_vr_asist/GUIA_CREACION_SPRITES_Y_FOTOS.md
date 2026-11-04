# 📸 GUÍA MAESTRA: CREACIÓN DE FOTOS Y SPRITES DE ALTA FIDELIDAD
> **El Taller de la Gatita Tech** — Protocolo visual para Cyber-Companion 2D Live.
> *Apto tanto para fotos reales de personas como para generación artística con Inteligencia Artificial.*

---

## 📑 ÍNDICE RÁPIDO
1. [El Mensaje Sencillo para Pedir Fotos a Otros Usuarios](#1-el-mensaje-sencillo-para-pedir-fotos-a-otros-usuarios)
2. [Generación de Sprites con Inteligencia Artificial (Prompts Exactos)](#2-generación-de-sprites-con-inteligencia-artificial-prompts-exactos)
3. [Catálogo de Estados y Poses de la Asistente](#3-catálogo-de-estados-y-poses-de-la-asistente)
4. [Procesamiento Automático de Recorte (Fondo Transparente sin Halos)](#4-procesamiento-automático-de-recorte-fondo-transparente-sin-halos)
5. [Resolución, Nitidez y Escalado HiDPI sin Deformaciones](#5-resolución-nitidez-y-escalado-hidpi-sin-deformaciones)

---

## 1. El Mensaje Sencillo para Pedir Fotos a Otros Usuarios

Cuando le pidas fotos a un amigo, cliente o familiar para convertirlo en su propio companion, **no le hables de píxeles ni de canales alfa**. Pídeselo con esta plantilla clara:

`	ext
¡Hola! Para preparar tu muñeca virtual interactiva necesito que te hagas unas cuantas fotos siguiendo estas 4 pautas muy fáciles:

1. 📷 DE CUERPO ENTERO O HASTA LAS RODILLAS: Ponte de pie a unos 2 o 3 metros de la cámara, mirando al frente.
2. 💡 BUENA LUZ: Que haya buena luz natural de frente (cerca de una ventana o habitación bien iluminada). Evita sombras oscuras o luces a tu espalda.
3. 🧱 FONDO LISO: Colócate delante de una pared lisa de un solo color (blanca, gris o crema), donde no haya cuadros, muebles ni otras personas detrás.
4. 🎭 4 O 5 POSES DISTINTAS:
   - Pose 1: De pie, relajada/o, con la boca cerrada (Reposo).
   - Pose 2: Sonriendo o con la boca entreabierta como hablando.
   - Pose 3: Pensando (con la mano en la barbilla o mirando de reojo).
   - Pose 4: Dando un paso o caminando (un pie delante y otro detrás).
   - Pose 5 (Opcional): Con auriculares puestos o una pose divertida.

⚠️ MUY IMPORTANTE AL ENVIARLAS:
Envíamelas por WhatsApp o Telegram como DOCUMENTO (o por Google Drive / WeTransfer), NO como foto normal, para que el móvil no les baje la calidad.
`

---

## 2. Generación de Sprites con Inteligencia Artificial (Prompts Exactos)

Si vas a crear una asistente original o temática (Cyberpunk, Anime, Táctica, Maker) usando generadores de IA (**Midjourney, DALL-E 3, Leonardo AI o Stable Diffusion**), utiliza estas fórmulas probadas:

### A. La Fórmula del Fondo Limpio
Para que la herramienta de recorte automático funcione al 100% sin dejar pelos cortados ni bordes blancos, el truco es forzar un **fondo de estudio gris neutro**:

> ull body standing pose, [DESCRIPCIÓN DEL PERSONAJE], facing front, solid flat neutral grey background, clean studio lighting, isolated on plain background, sharp clean edges, 8k definition --no complex background, shadows on floor, gradients

### B. Prompts por Estado Emocional

* **Estado IDLE (Reposo):**
  > ull body character sprite, cyberpunk tactical girl with dark jumpsuit and glowing cyan lines, resting posture, neutral relaxed face, closed mouth, arms at side, solid flat neutral grey background, crisp vector-like edges

* **Estado TALK (Hablando / Lip-Sync):**
  > ull body character sprite, same cyberpunk girl, talking expression, slightly open mouth smiling, expressive friendly posture, solid flat neutral grey background

* **Estado THINK (Pensando / Hacking):**
  > ull body character sprite, same cyberpunk girl, tapping on glowing holographic virtual keyboards in front of her, tactical visor active, analytical posture, solid flat neutral grey background

* **Estado WALK (Caminando - Ciclo de 2 pasos):**
  > ull body character sprite, same cyberpunk girl, side-stepping walking pose, left foot forward, arms in natural walking motion, solid flat neutral grey background

* **Estado SLEEP / SNOOZE (Durmiendo / Inactividad del PC):**
  > cute chibi or resting posture, sitting down curled up sleeping peacefully, night cap or glowing circuits dimmed, solid flat neutral grey background

* **Estado MUSIC (Escuchando Música):**
  > ull body character sprite, same cyberpunk girl, wearing sleek glowing neon over-ear headphones, head bobbing happily, musical posture, solid flat neutral grey background

* **Estado MOVIE (Viendo Películas en Pantalla Completa):**
  > character sprite sitting or leaning, wearing retro-futuristic 3D glasses or holding a miniature popcorn bucket, relaxed movie spectator posture, solid flat neutral grey background

---

## 3. Catálogo de Estados y Nomenclatura de Archivos

Guarda los archivos en la carpeta ssets/sprites/ siguiendo esta estructura estándar:

| Estado | Nombre de Archivo | Función en la Aplicación |
| :--- | :--- | :--- |
| **IDLE** | idle.png | Posición por defecto con respiración sinusoidal fluida a 25 FPS. |
| **TALK** | 	alk.png | Boca abierta. Se alterna con idle.png cada 120 ms cuando habla. |
| **WALK 1** | walk_1.png | Fotograma 1 del paso al patrullar por la pantalla. |
| **WALK 2** | walk_2.png | Fotograma 2 del paso (rebote elástico procedural). |
| **THINK 1** | 	hink_1.png | Fotograma 1 del teclado holográfico (mientras consulta la IA). |
| **THINK 2** | 	hink_2.png | Fotograma 2 del teclado holográfico. |
| **SLEEP** | sleep.png | Se activa cuando el ordenador lleva más de 5 minutos inactivo. |
| **MUSIC** | music.png | Se activa cuando Windows detecta Spotify, YouTube o reproductor. |
| **MOVIE** | movie.png | Se activa cuando hay una ventana en pantalla completa. |

---

## 4. Procesamiento Automático de Recorte (Fondo Transparente sin Halos)

Nuestro taller incluye el script 	ools/sprite_processor.py. 
Para recortar automáticamente cualquier foto en bruto y convertirla en un PNG transparente con bordes perfectos:

1. Coloca tus fotos en bruto en una carpeta (ej. 	ools/entrada/).
2. Ejecuta en tu terminal:
`powershell
python tools/sprite_processor.py
`
3. El script aplica el modelo de red neuronal **ISNet-General** con **Alpha Matting**, eliminando el fondo sin morder el pelo y guardando los sprites limpios directamente en ssets/sprites/.

---

## 5. Resolución, Nitidez y Escalado HiDPI sin Deformaciones

### ¿Por qué mi muñeca no se hace gigante al usar fotos de alta calidad?
En nuestro motor PySide6, la altura del personaje en pantalla la define la propiedad current_height:
- **Pequeño:** 240 píxeles de alto en pantalla.
- **Normal:** 330 píxeles de alto en pantalla.
- **Grande:** 450 píxeles de alto en pantalla.

Si tu foto original mide **2048×2048 píxeles**, el motor **no** la muestra a 2048 píxeles. La escala al tamaño exacto de tu ventana, pero conservando toda la densidad de información gracias a:

`python
dpr = self.devicePixelRatio()
target_h = int(self.current_height * dpr)
scaled = frame.scaledToHeight(target_h, Qt.TransformationMode.SmoothTransformation)
scaled.setDevicePixelRatio(dpr)
`

Esto significa que una foto de alta calidad se verá **ultra-nítida y definida** en cualquier monitor (1080p, 2K o 4K), sin verse jamás pixelada ni salirse de la pantalla.
