# 🚀 PIPELINE DE PRODUCCIÓN AUTÓNOMO CON IA (I2V A SPRITES)
> **El Taller de la Gatita Tech — Procesador de Vídeos de Google Flow**  
> **Operadora:** Raquel D. Torres  
> **Motor:** Google Gemini Vision (API Gratuita) + OpenCV + Pillow  
> **Consumo de Tokens Antigravity:** **0% (100% Autónomo en local)**

---

## 🧭 ¿CÓMO FUNCIONA?

1. **Buzón de Entrada:**
   Descarga cualquier clip `.mp4` de Google Flow o Kling y déjalo en:
   📁 [`tools/i2v_sprite_suite/entrada_videos/`](file:///c:/Users/asakura/Documents/juego%20meta/tools/i2v_sprite_suite/entrada_videos)

2. **Doble Clic:**
   Ejecuta el archivo:
   ⚡ [`PROCESAR_VIDEOS_FLOW.bat`](file:///c:/Users/asakura/Documents/juego%20meta/PROCESAR_VIDEOS_FLOW.bat)

3. **Magia de Automatización en Segundo Plano:**
   - La herramienta conecta automáticamente con **Google Gemini** usando tu API Key de `cyber_companion/config.json`.
   - Gemini analiza visualmente los fotogramas del vídeo y detecta automáticamente:
     - Qué personaje o prop es (**Mina**, **Kira**, **Nyx**, **Spider Drone**, **Blaster**, etc.).
     - Qué tipo de animación es (**Turntable 360°**, **Caminata**, **Habla/Lip-sync**).
     - Las marcas de tiempo exactas en segundos para Frente (0°), Perfil (90°) y Espalda (180°).
   - Aplica el recorte de Croma Verde Puro (`#00FF00`) con **despill perimetral** protegiendo los circuitos neón.
   - Nivela la pisada al suelo ($Y=767$) en el lienzo canónico de $688 \times 768$ px.
   - Deposita los sprites y trípticos directamente en sus carpetas de Unity:
     - `quest_mr_nexus/Assets/Sprites/Mina_Kira/Mina/`
     - `quest_mr_nexus/Assets/Sprites/Mina_Kira/Kira/`
     - `quest_mr_nexus/Assets/Sprites/Nyx/`
     - `quest_mr_nexus/Assets/Sprites/Weapons/`
     - `quest_mr_nexus/Assets/Sprites/Enemies/`
   - Mueve el vídeo original a [`procesados/`](file:///c:/Users/asakura/Documents/juego%20meta/tools/i2v_sprite_suite/procesados) para que no se duplique trabajo.

---

## 🛠️ VENTAJAS PARA ESTA NOCHE:
* **Cero gasto de tokens en Antigravity:** Puedes procesar decenas de vídeos sin tocar la cuota de la conversación.
* **Procesamiento por Lotes:** Puedes meter 5 o 10 vídeos a la vez en `entrada_videos/` y procesarlos todos de un solo golpe.
* **Unity siempre al día:** Al depositarse directamente en `Assets/Sprites/`, Unity los detecta en tiempo real y genera sus `.meta`.
