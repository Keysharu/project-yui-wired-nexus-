# 📖 MANUAL MAESTRO DE REPLICACIÓN: CYBER-COMPANION 2D LIVE (REBECA)
> **Taller de la Gatita Tech** — Arquitectura de Escritorio Asistente Táctica Cyberpunk (LainOS).  
> *Stack:* Python 3.14+ | PySide6 (Qt6) | Google GenAI SDK | Edge-TTS Neural | SoundDevice + SpeechRecognition | Open-Meteo Telemetry | WASAPI Loopback.

---

## 📑 ÍNDICE DE CONTENIDOS
1. [Filosofía y Arquitectura del Sistema](#1-filosofía-y-arquitectura-del-sistema)
2. [Estructura del Proyecto y Directorios](#2-estructura-del-proyecto-y-directorios)
3. [Entorno y Dependencias de Trinchera](#3-entorno-y-dependencias-de-trinchera)
4. [Fase 1: Preparación de Sprites y Animación 2D Live](#fase-1-preparación-de-sprites-y-animación-2d-live)
5. [Fase 2: Motor Gráfico PySide6 y Estilo LainOS](#fase-2-motor-gráfico-pyside6-y-estilo-lainos)
6. [Fase 3: Cerebro Neural (Google Gemini + Multi-Model Pool)](#fase-3-cerebro-neural-google-gemini--multi-model-pool)
7. [Fase 4: Voz Neural Táctica (Edge-TTS + Lip-Sync)](#fase-4-voz-neural-táctica-edge-tts--lip-sync)
8. [Fase 5: Reconocimiento Vocal y Modo Manos Libres Continuo](#fase-5-reconocimiento-vocal-y-modo-manos-libres-continuo)
9. [Fase 6: Mochila de Datos (Context Backpack) y Gestión de Archivos](#fase-6-mochila-de-datos-context-backpack-y-gestión-de-archivos)
10. [Fase 7: Sensor Satelital de Clima (Geo-IP + Open-Meteo)](#fase-7-sensor-satelital-de-clima-geo-ip--open-meteo)
11. [Fase 8: Blindaje de Dicción Oral (Vacuna Anti-Encabezados)](#fase-8-blindaje-de-dicción-oral-vacuna-anti-encabezados)
12. [Fase 9: Protocolo de Ejecución Silenciosa y Compilación a .EXE](#fase-9-protocolo-de-ejecución-silenciosa-y-compilación-a-exe)
20. [Fase 13: Teleprompter HUD Transparente y Modo Presentadora (OBS Studio / Retransmisiones)](#fase-13-teleprompter-hud-transparente-y-modo-presentadora-obs-studio--retransmisiones)
21. [Fase 14: Modo Plató OBS, Trajes Temáticos y Bloqueo de Vaivén de Respiración](#fase-14-modo-plató-obs-trajes-temáticos-y-bloqueo-de-vaivén-de-respiración)
22. [Fase 15: Pipeline de Audio Fail-Safe & Red de Seguridad Agéntica Determinista](#fase-15-pipeline-de-audio-fail-safe--red-de-seguridad-agéntica-determinista)
23. [Fase 16: Metodología de Replicación y Entrega Llave en Mano para Clientes](#fase-16-metodología-de-replicación-y-entrega-llave-en-mano-para-clientes)
24. [Fase 17: Motor de Lip-Sync Fonético DSP en Tiempo Real y Parpadeo Biológico (Anime Live Studio)](#fase-17-motor-de-lip-sync-fonético-dsp-en-tiempo-real-y-parpadeo-biológico-anime-live-studio)

---

## 1. Filosofía y Arquitectura del Sistema

El objetivo de este proyecto es construir una compañera de escritorio interactiva, autónoma y no intrusiva:
- **Cero Costes de Servidor:** Utiliza la API gratuita de Google AI Studio, síntesis vocal gratuita de Microsoft Edge Neural y satélites meteorológicos abiertos.
- **Sin Bloqueo de Interfaz:** Todo proceso de red, grabación de micrófono o síntesis vocal se ejecuta en hilos independientes (`QThread`) comunicados por señales y slots (`Signal` / `Slot`).
- **Inmersión Estética:** Diseño gráfico estricto inspirado en *Serial Experiments Lain*, *Ghost in the Shell* y *Cyberpunk 2077* con paleta de colores neón sobre negro digital.
- **Portabilidad Universal:** El usuario final no depende de rutas locales fijas; dispone de una interfaz para subir sus propios documentos a la memoria de la asistente.
- **Capacidades Agénticas Nativas:** Control de aplicaciones del sistema operativo, calendario (.ics), notas y navegación web reforzada.

---

## 2. Estructura del Proyecto y Directorios

La estructura completa del proyecto para que sea portable a cualquier equipo es la siguiente:

```text
cyber_companion/
│
├── assets/
│   ├── sprites/
│   │   ├── idle.png                  # Estado de reposo estándar
│   │   ├── walk_1.png .. walk_8.png  # Ciclo de caminata I2V estabilizado al suelo
│   │   ├── talk_1.png .. talk_5.png  # Lip-sync multi-visema progresivo
│   │   ├── think_1.png, think_2.png  # Hacking / holograma de red
│   │   ├── stream_idle.png           # Plató streamer reposo con mesa
│   │   ├── streamer_idle.png         # Traje streamer (auriculares + webcam)
│   │   ├── reportera_idle.png        # Traje reportera (micrófono en mano)
│   │   └── telediario_idle.png       # Traje telediario (mesa de plató formal)
│   ├── backgrounds/                  # Fondos panorámicos 16:9 para OBS
│   └── speech_cache_<timestamp>.mp3  # Buffers temporales rotativos de audio
│
├── backpack/                         # Mochila de contexto local del usuario (.docx, .md, .txt)
│   └── ejemplo_contexto.md           # Plantilla inicial para el usuario final
│
├── calendar_events/                  # Eventos y citas generados en formato universal (.ics)
├── notes/                            # Documentos inteligentes (.docx / .txt)
│
├── config.json                       # Configuración persistente (API Key, voz, escala, estilo)
├── system_prompt.md                  # Personalidad y directivas operativas de la asistente
├── chat_history.json                 # Historial persistente de conversación reciente
│
├── main.py                           # Núcleo del companion (PySide6 GUI, audio y estados)
├── agent_tools.py                    # Módulo desacoplado de herramientas agénticas (PC y móvil)
├── backpack_manager.py               # Gestor de archivos, memoria de chat y sensor climático
├── backpack_dialog.py                # Modal LainOS de subida y gestión de archivos
├── iniciar_rebeca.vbs                 # Lanzador silencioso en Windows (sin ventana negra)
└── iniciar_rebeca.bat                 # Lanzador diagnóstico en consola
```

---

## 3. Entorno y Dependencias de Trinchera

### A. Versión de Python recomendada
Python 3.11 a 3.14 (64 bits).

### B. Instalación de paquetes
Ejecuta en tu terminal de PowerShell:
```powershell
pip install PySide6 pillow rembg onnxruntime edge-tts google-genai SpeechRecognition sounddevice numpy python-docx pycaw comtypes opencv-python
```

> **Nota:** `rembg` y `onnxruntime` solo son necesarios si vas a recortar y procesar sprites automáticamente desde imágenes en bruto. Para la ejecución en caliente de la aplicación solo se necesitan PySide6, edge-tts, google-genai, SpeechRecognition, sounddevice, pycaw y python-docx.

---

## Fase 1: Preparación de Sprites y Animación 2D Live

### A. Eliminación de fondo con IA
Para evitar bordes blancos o artefactos, los sprites se procesan con `rembg`:
```python
from rembg import remove
from PIL import Image

def process_sprite(input_path, output_path):
    with open(input_path, 'rb') as i:
        with open(output_path, 'wb') as o:
            input_img = i.read()
            output_img = remove(input_img)
            o.write(output_img)
```

### B. Los Fotogramas Canónicos
1. **IDLE (1 fotograma base):** `idle.png` — Reposo con respiración sinusoidal procedural.
2. **TALK (5 fotogramas):** `talk_1.png` a `talk_5.png` — Modulación fonética progresiva a 12.5 FPS (80 ms por visema).
3. **WALK (8 fotogramas):** `walk_1.png` a `walk_8.png` — Zancada balanceada a 120 ms por paso con física de rebote en bordes.
4. **THINK (2 fotogramas):** `think_1.png`, `think_2.png` — Modo Netrunner interactuando con interfaz holográfica.

---

## Fase 2: Motor Gráfico PySide6 y Estilo LainOS

### A. Ventana transparente sin bordes
La ventana debe flotar libremente sobre el escritorio de Windows:
```python
self.setWindowFlags(
    Qt.WindowType.FramelessWindowHint |
    Qt.WindowType.WindowStaysOnTopHint |
    Qt.WindowType.SubWindow
)
self.setAttribute(Qt.WidgetAttribute.WA_TranslucentBackground, True)
```

### B. Respiración Orgánica Procedural (25 FPS)
Para que el personaje nunca sea una imagen congelada, se calcula una oscilación senoidal continua en `paintEvent`:
```python
# Actualización cada 40 ms (25 FPS)
self.breath_phase += 0.06
if self.current_state == PetState.IDLE and not self.table_locked:
    y_offset = 8 + math.sin(self.breath_phase) * 3.5
elif self.current_state == PetState.WALK:
    y_offset = 8 + abs(math.sin(self.anim_ticks * 0.25)) * 3.0
else:
    y_offset = 8
```

### C. Inversión de Sprites sin fallos de QPainter
Al patrullar a la izquierda, se utiliza `QTransform`:
```python
if self.current_state == PetState.WALK and self.walk_direction < 0:
    transform = QTransform()
    transform.scale(-1, 1)
    frame = frame.transformed(transform)
```

---

## Fase 3: Cerebro Neural (Google Gemini + Multi-Model Pool)

### A. Obtención de la clave gratuita
El usuario final obtiene su clave gratuita con cuenta de Google en:
`https://aistudio.google.com/app/apikey`

### B. Conexión asíncrona con Timeout y Multi-Model Pool
Para evitar cierres inesperados o bloqueos si una variante de modelo está saturada, se implementa una piscina de modelos con fallback automático:
```python
MODEL_POOL = [
    "gemini-2.5-flash",
    "gemini-2.0-flash",
    "gemini-1.5-flash"
]
```
Si el modelo primario arroja `ResourceExhausted` o `ServiceUnavailable`, el worker pasa al siguiente modelo de la lista de forma transparente para el usuario.

---

## Fase 4: Voz Neural Táctica (Edge-TTS + Lip-Sync)

### A. Generación asíncrona de audio sin dependencias externas
Edge-TTS sintetiza directamente en un archivo de audio temporal con voz neural de Microsoft:
```python
communicate = edge_tts.Communicate(
    text=clean_text,
    voice=self.voice_name,
    rate=self.rate,
    pitch=self.pitch
)
await communicate.save(output_filepath)
```

### B. Lip-Sync Progresivo
El motor gráfico detecta la reproducción activa mediante `QMediaPlayer` y transita suavemente entre los 5 visemas disponibles, cerrando la boca al finalizar el stream.

---

## Fase 5: Reconocimiento Vocal y Modo Manos Libres Continuo

### A. Captura no invasiva con Atajos Globales
- **F2:** Abre de inmediato la caja de texto táctica para órdenes por teclado.
- **F3:** Activa el micrófono para dictado por voz de una sola orden con filtrado de ruido de fondo.
- **Doble clic en el avatar:** Alterna la burbuja de conversación estándar.

---

## Fase 6: Mochila de Datos (Context Backpack) y Gestión de Archivos

### A. Extracción en caliente de documentos
`BackpackManager` recorre la carpeta `backpack/` al inicio y lee archivos `.docx`, `.md` y `.txt`, inyectándolos en el contexto del sistema antes del mensaje del usuario:
```python
class BackpackManager:
    def get_full_context(self) -> str:
        # Lee los documentos de la mochila y devuelve un bloque de memoria
        ...
```

---

## Fase 7: Sensor Satelital de Clima (Geo-IP + Open-Meteo)

Consulta abierta y gratuita sin claves API a la red satelital de Open-Meteo geolocalizando por IP pública, permitiendo a la asistente responder preguntas como "¿cómo está el tiempo hoy?" con temperatura, viento y probabilidad de lluvia reales.

---

## Fase 8: Blindaje de Dicción Oral (Vacuna Anti-Encabezados)

Para evitar que la síntesis vocal lea símbolos extraños como `###`, asteriscos de negrita `**` o enlaces markdown, un filtro de pre-procesamiento fonético limpia el texto antes de enviarlo a Edge-TTS:
```python
def sanitize_for_speech(text: str) -> str:
    # Elimina bloques de código, URLs crudas, marcadores markdown y etiquetas [ESTADO: ...]
    ...
```

---

## Fase 9: Protocolo de Ejecución Silenciosa y Compilación a .EXE

### A. Lanzamiento VBS Silencioso (Windows)
`iniciar_rebeca.vbs` invoca el intérprete de Python en modo oculto (`vbHide` = 0) para no mostrar la clásica consola negra de Windows:
```vbscript
Set WshShell = CreateObject("WScript.Shell")
WshShell.Run "pythonw main.py", 0, False
```

---

## Fase 10: Capacidades Agénticas y Control de Sistema (Function Calling)

### A. Filosofía del Patrón Adaptador de Plataforma
La IA recibe un catálogo agéntico agnóstico (`AGENT_TOOLS_LIST`). En Windows, ejecuta llamadas del sistema; en futuras plataformas (Android), las mismas firmas delegarán en Intents nativos.

### B. Herramientas Nativas en `agent_tools.py`:
- `launch_program(app_name)`: Lanza ejecutables locales.
- `open_url_or_search(target)`: Abre URLs o busca en el navegador predeterminado.
- `add_calendar_event(title, start_datetime)`: Crea eventos universales `.ics` (RFC 5545).
- `create_smart_document(title, content)`: Redacta documentos `.docx` y los abre en Google Docs.
- `get_battery_status()`: Consulta el estado de carga mediante `ctypes`.

---

## Fase 11: Documentos Inteligentes Híbridos (.docx local + Google Docs/Sheets)

La asistente redacta informes formales en formato `.docx` con formato enriquecido (títulos, viñetas), copia el texto limpio al portapapeles del sistema y lanza `https://docs.new` para que el usuario pueda pegar y compartir en la nube con un solo `Ctrl+V`.

---

## Fase 12: Renderizado HiDPI Subpixel, Autonomía y Estados Sensoriales

- **HiDPI Subpixel:** Detección de `devicePixelRatio` para escalado cristalino en pantallas Retina o 4K.
- **Sensor Acústico WASAPI:** Medición del nivel de pico de audio en tiempo real (`GetPeakValue() > 0.005`) para entrar en modo música de forma certera sin falsos positivos de OBS o Discord.
- **Modo Cine (Movie Stealth):** Si hay audio sonando y la ventana activa está a pantalla completa, la asistente reduce su opacidad al 35% para no tapar subtítulos.

---

## Fase 13: Teleprompter HUD Transparente y Modo Presentadora (OBS Studio / Retransmisiones)

### A. Arquitectura del Diálogo No Modal (Modeless HUD)
El teleprompter está diseñado para streamers, presentadores y creadores de contenido que necesitan locutar guiones en directo sobre su juego, escritorio o plató sin perder el control de las demás ventanas.

```python
class TeleprompterDialog(QDialog):
    closed = Signal()

    def __init__(self, parent=None):
        super().__init__(parent)
        # Sin modal para permitir interactuar con OBS y otras ventanas
        self.setWindowFlags(Qt.WindowType.Window | Qt.WindowType.WindowStaysOnTopHint)
        self.setAttribute(Qt.WidgetAttribute.WA_DeleteOnClose, False)
        self.setWindowTitle("TELEPROMPTER HUD // MODO REPORTE")
```

### B. Selector Dinámico de Opacidad
Permite al usuario ajustar la transparencia sobre la marcha para leer texto sin tapar el juego o la señal de vídeo:
```python
self.opacity_combo = QComboBox()
self.opacity_combo.addItems(["100%", "85%", "70%", "55%", "40%"])
self.opacity_combo.currentTextChanged.connect(self._on_opacity_changed)

def _on_opacity_changed(self, text: str):
    mapping = {"100%": 1.0, "85%": 0.85, "70%": 0.70, "55%": 0.55, "40%": 0.40}
    self.setWindowOpacity(mapping.get(text, 0.85))
```

### C. Control Ergonómico de Transmisión (Intro vs Shift+Intro)
- **`Intro`:** Dispara la transmisión del texto al motor de voz de inmediato **sin cerrar la ventana**, permitiendo al streamer seguir dictando noticias o guiones de forma continua y fluida.
- **`Shift + Intro`:** Inserta saltos de línea para maquetar párrafos complejos.

```python
def keyPressEvent(self, event):
    if event.key() in (Qt.Key.Key_Return, Qt.Key.Key_Enter):
        if event.modifiers() & Qt.KeyboardModifier.ShiftModifier:
            super().keyPressEvent(event) # Salto de línea
        else:
            self.transmit_current_text() # Enviar sin cerrar
            event.accept()
    else:
        super().keyPressEvent(event)
```

### D. Auto-Ocultación de la Barra de Escritorio
Durante la sesión activa de teleprompter, la barra de diálogo inferior (`DialogueBubble`) se oculta automáticamente para mantener el plano de cámara despejado, restaurándose al cerrar la ventana mediante la señal `closed`:
```python
self.teleprompter.closed.connect(self.on_teleprompter_closed)

def on_teleprompter_closed(self):
    if hasattr(self, 'bubble') and self.bubble:
        self.bubble.show()
```

---

## Fase 14: Modo Plató OBS, Trajes Temáticos y Bloqueo de Vaivén de Respiración

### A. Trajes Temáticos de Estudio
El sistema soporta conmutación en caliente de trajes mediante el menú contextual:
- **`normal`:** Atuendo cyberpunk casual para escritorio diario.
- **`streamer`:** Auriculares gamer de estudio neón y webcam frontal integrada.
- **`reportera`:** Micrófono de mano profesional de reportaje en vivo.
- **`telediario`:** Mesa de plató formal de noticias de televisión.

### B. Bloqueo de Vaivén de Respiración Procedural (`table_locked`)
En un directo de OBS Studio, si el personaje incluye una mesa en su sprite, la oscilación senoidal de respiración haría que la mesa "baile" o tiemble contra el fondo del plató, arruinando el efecto visual.  
Se introduce una bandera lógica estricta:
```python
# En paintEvent de DesktopPet:
if self.table_locked or self.is_table_mode:
    # La mesa permanece milimétricamente fija; solo el cabello y visemas se animan
    y_offset = 8
else:
    # Respiración senoidal orgánica en modo libre de escritorio
    y_offset = 8 + math.sin(self.breath_phase) * 3.5
```

### C. Modo Eco OBS (Repetidora de Voz Limpia)
Permite al streamer escribir cualquier texto para que la asistente lo vocalice con dicción limpia, registrándolo simultáneamente en el historial persistente de chat (`chat_history.json`) para mantener la coherencia narrativa del directo.

---

### D. Sincronización Labial Acústica en Modo Stream Talk (`stream_talk`)
La modulación fonética por energía acústica (RMS) se aplica de forma simétrica tanto en el modo libre de escritorio como en el modo plató de streaming:
- **Cierre Labial en Pausas de Directo:** Cuando el streamer o la IA hace una pausa en su locución o respira, el motor detecta el umbral de silencio (`RMS < umbral_silencio`) y regresa instantáneamente a `stream_idle.png`, cerrando la boca ante el micrófono de condensador y eliminando la sensación de "boca moviéndose en falso".
- **Mapeo Progresivo de Visemas de Estudio:**
  | Nivel RMS / Fonema | Sprite en Modo Normal | Sprite en Modo Stream Talk | Comportamiento Visual |
  | :--- | :--- | :--- | :--- |
  | **Silencio / Pausa** | `idle.png` | `stream_idle.png` | Boca cerrada, mirada atenta al espectador. |
  | **Bajo (Consonantes)** | `talk_1.png` / `talk_2.png` | `stream_talk_1.png` | Apertura leve frente al micrófono. |
  | **Medio (Vocales suaves)** | `talk_3.png` | `stream_talk_2.png` | Articulación clara y equilibrada. |
  | **Alto (Énfasis / Acentuadas)** | `talk_4.png` / `talk_5.png` | `stream_talk_3.png` | Apertura plena de locución profesional. |
- **Invarianza de Mobiliario:** El filtrado de desplazamiento garantiza que la mesa, el soporte del micrófono y los auriculares no sufran vibración ni distorsión durante los picos de volumen de la voz.

- **Estrategia de Despliegue en Fases (Foco en Producción Actual):**
  - **Fase Activa:** Optimización prioritaria de **Talk Básico** (escritorio) y **Stream Talk** (plató streamer), garantizando sincronización labial orgánica en los dos modos más utilizados.
  - **Ruta Modular Preparada:** Los trajes de *reportera* y *telediario* cuentan con su lógica de fallback desacoplada lista para recibir sus spritesheets cuando se completen sus capturas I2V, integrándose sin fricción en el sistema.

---

## Fase 15: Pipeline de Audio Fail-Safe & Red de Seguridad Agéntica Determinista

### A. Pipeline de Audio Rotativo con Limpieza de Buffers
En Windows, el reproductor multimedia mantiene bloqueos exclusivos de archivos (`WinError 32: El proceso no tiene acceso al archivo porque está siendo utilizado por otro proceso`).  
Para resolver esto de forma definitiva:
1. Cada locución se guarda con un timestamp único: `speech_cache_{int(time.time()*1000)}.mp3`.
2. Una rutina de poda automática purga los archivos antiguos, conservando únicamente los 4 más recientes.
3. Se conecta el monitor de fallos de Qt: `self.media_player.errorOccurred.connect(self.on_audio_error)` para restaurar el estado a `IDLE` de inmediato si el hardware de sonido falla o se desconecta un auricular Bluetooth.

### B. Detección Dinámica del Navegador en Windows
En lugar de forzar Edge o Chrome estáticamente, el sistema interroga el Registro de Windows:
```python
def get_windows_default_browser() -> str:
    key_path = r"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice"
    with winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path) as key:
        prog_id, _ = winreg.QueryValueEx(key, "ProgId")
        if "Chrome" in prog_id: return "chrome"
        if "MSEdge" in prog_id: return "msedge"
        if "Firefox" in prog_id: return "firefox"
        if "Brave" in prog_id: return "brave"
    return "chrome"
```

### C. Lanzamiento Reforzado Tri-Capa
1. **Capa 1:** `webbrowser.open(url, new=2, autoraise=True)`
2. **Capa 2 (Fallback):** `os.startfile(url)`
3. **Capa 3 (Fallback crítico):** `subprocess.Popen(['start', '', url], shell=True)`

### D. Red de Seguridad Agéntica Determinista
Si el usuario le pide a Gemini: *"búscame noticias de IA y ábrelas en el navegador"*, pero el modelo LLM responde únicamente texto afirmando que ya lo hizo sin haber emitido la llamada a la herramienta (`function_call`), la red de seguridad en `on_gemini_success` intercepta la intención y ejecuta la búsqueda en el navegador de manera programática e instantánea.

---

## Fase 16: Metodología de Replicación y Entrega Llave en Mano para Clientes

Cuando un cliente encarga una asistente personalizada ("Tu Avatar IA") o compra la versión empaquetada, sigue este protocolo paso a paso:

### Paso 1: Clonación desde la Plantilla Maestra
Copia el directorio desacoplado `cyber_companion_template/` hacia la nueva carpeta de entrega (ej. `cliente_companion/`). Esta plantilla viene limpia de historiales personales, logs y claves privadas.

### Paso 2: Personalización del Avatar y Sprites
1. Si el cliente tiene un personaje o mascota propia, genera las imágenes base siguiendo las instrucciones de `METODOLOGIA_MAESTRA_SPRITES_I2V.md`.
2. Procesa los fotogramas recortados a **688x768 píxeles** con fondo transparente PNG des-verdecidos (*despilled*):
   - **Suite Normal (Cuerpo Entero):** `idle.png`, `blink.png`, `talk_1.png` .. `talk_5.png`, `happy.png`, `laugh.png`, `pout.png`, `walk_1.png` .. `walk_8.png`.
   - **Suite Streamer (Plató OBS):** `stream_idle.png`, `stream_blink.png`, `stream_talk_1.png` .. `stream_talk_3.png`, `stream_hype.png`.
3. Sobrescribe los archivos en `assets/sprites/`.

### Paso 3: Configuración de la Personalidad y Voz
1. En `system_prompt.md`, ajusta el nombre, rol y tono de la asistente según el manual de marca del cliente.
2. Selecciona la voz neural en `config.json` (ej. `es-ES-XimenaNeural` para tono femenino español, `es-MX-DaliaNeural` para México, `en-US-JennyNeural` para inglés).

### Paso 4: Configuración de Google AI Studio (Modelo BYOK)
1. Guía al cliente para obtener su clave gratuita en [Google AI Studio](https://aistudio.google.com/).
2. El cliente introduce la clave haciendo clic derecho sobre el avatar ➔ **`🔑 Configurar Gemini API Key`**. La clave queda encriptada localmente en su propio `config.json`.

### Paso 5: Integración con OBS Studio (Streaming en 2 Clics)
Para streamers:
1. Abrir OBS Studio ➔ Añadir Fuente ➔ **Captura de Ventana**.
2. Seleccionar la ventana de la asistente (`DesktopPet`).
3. En las propiedades de la fuente de captura, activar **Permitir transparencia**.
4. ¡Listo! El avatar flota en el stream con fondo transparente nativo, sin necesidad de croma verde ni filtros pesados.

---

## Fase 17: Motor de Lip-Sync Fonético DSP en Tiempo Real y Parpadeo Biológico (Anime Live Studio)

La versión v1.0.0 incorpora un salto generacional en fidelidad de animación anime: sincronización labial gobernada por análisis digital de señales de audio (DSP) y parpadeo biológico probabilístico.

### A. Desacoplamiento del Motor DSP (`audio_lip_sync.py`)
Para evitar sobrecargar el hilo de la GUI de Qt y eliminar dependencias pesadas de terceros (como `librosa`, `soundfile` o binarios de `ffmpeg`), el motor aprovecha `QAudioDecoder` nativo de `PySide6.QtMultimedia`:
1. **Decodificación Ultrarrápida:** Convierte el archivo MP3 generado por Edge-TTS a un array lineal de muestras de audio PCM de 16-bits en ~20 milisegundos.
2. **Ventanas de Análisis de 40ms (25 FPS):** Divide la señal en chunks coincidentes con los fotogramas del bucle de animación de Rebeca.
3. **Cálculo de Envolvente RMS:** Determina la potencia acústica de la voz instantánea mediante $RMS = \sqrt{\frac{1}{N} \sum x_i^2}$.
4. **Tasa de Cruces por Cero (ZCR):** Detecta la densidad espectral para diferenciar consonantes fricativas/sibilantes (ZCR alto -> visema entreabierto) de vocales redondas u oscuras (ZCR bajo -> visema redondo 'O'/'U').

### B. Leyes de Animación Anime: Histeresis y Vocal Hold
Los personajes de videojuegos y anime (*Live2D*, *Persona*, *Steins;Gate*) no aletean la boca erráticamente en cada micro-valle de audio:
* **Fast Attack (10ms):** Apertura instantánea al detectar el ataque vocal de una sílaba.
* **Vocal Hold (80-120ms):** Retiene la apertura bucal durante transiciones silábicas para mantener la boca viva y orgánica, eliminando el efecto "marioneta espasmódica".
* **Clean Silence Snap:** Si la energía cae por debajo del umbral de silencio (`RMS < 0.035`) durante más de 120ms (pausa entre frases, comas o puntos), la boca se cierra con absoluta precisión hacia `idle.png` o `stream_idle.png`.

### C. Parpadeo Biológico Orgánico
El parpadeo humano y de animación de alta gama sigue una distribución semi-aleatoria:
* **Frecuencia Natural:** Temporizador con distribución entre 2.8s y 5.5s.
* **Micro-Doble Parpadeo (12% de probabilidad):** Reproduce el tic biológico de dos parpadeos sucesivos rápidos separados por 600-1000ms.
* **Duración Anatómica:** Cierre palpebral estricto durante 140ms (`stream_blink.png` en streamer / `blink.png` en cuerpo entero).
* **Inhibición en Reposo Nocturno:** Cuando Rebeca entra en `PetState.SLEEP`, el parpadeo se desactiva ya que sus ojos permanecen cerrados.

### D. Catálogo de Sprites Masterizados (688x768 píxeles con despill)
* **Streamer Suite (OBS Studio):**
  - `stream_idle.png`: Reposo con headset, boom micro y escritorio mecánico.
  - `stream_talk_1.png`: Apertura sutil / consonante silábica.
  - `stream_talk_2.png`: Articulación labial redonda ('O', 'U').
  - `stream_talk_3.png`: Vocal abierta y énfasis ('A', 'E').
  - `stream_hype.png`: Sonrisa amplia y euforia para donaciones/chistes.
  - `stream_blink.png`: Ojos cerrados sonrientes para parpadeo biológico.
* **Fullbody Suite (Mascota de Escritorio):**
  - `idle.png`: Reposo de cuerpo entero con circuitos cian neón.
  - `talk_1.png` .. `talk_5.png`: Progresión completa de visemes fonéticos.
  - `happy.png`: Expresión alegre y victoriosa.
  - `blink.png`: Parpadeo orgánico de cuerpo entero.
  - `walk_1.png` .. `walk_8.png`: Ciclo biomecánico de caminata completa.

### E. Protocolo de Pruebas de Integración Automatizadas
Antes de desplegar a clientes, se ejecuta la suite automatizada:
```powershell
python scratch/test_pet_integration.py
```
Garantiza:
1. Instanciación limpia de `CyberPetWindow` / `DesktopPet`.
2. Verificación de integridad de los 16 visemes cargados en memoria.
3. Validación del parpadeo biológico (`trigger_blink` / `end_blink`).
4. Verificación de renderizado en modo Streamer y modo normal.
5. Análisis acústico con audio real `rebeca_speech.mp3` y comprobación de timeline de visemes.
6. Cierre sin fugas de memoria con código de salida `0`.

---

## Fase 18: Suite Acústica Avanzada: FFT de Formantes, Micro-Inercia y Smart Audio Ducking WASAPI

La versión Golden Master consolida la acústica de estudio mediante dos avances clave en la interacción del sistema:

### A. Discriminación Espectral FFT y Micro-Inercia Física (`audio_lip_sync.py`)
1. **Transformada Rápida de Fourier (FFT) con Ventana de Hann:**
   En cada ventana de 40ms, se computa la distribución espectral y el **Centroide Espectral**:
   $$\text{Centroide} = \frac{\sum f \cdot |X(f)|}{\sum |X(f)|}$$
   - **Centroide Grave (< 1400 Hz) + RMS Alto:** Formantes abiertos ('O', 'A') $\rightarrow$ `stream_talk_3` o `talk_3`.
   - **Centroide Medio (1400 - 2600 Hz):** Formantes claros ('E', 'I', 'U') $\rightarrow$ `stream_talk_2` o `talk_2`.
   - **Centroide Agudo (> 2600 Hz) o ZCR Elevado:** Fricativas y sibilantes ('S', 'T', 'CH', 'F') $\rightarrow$ `stream_talk_1` o `talk_4`.
2. **Micro-Inercia Física de Locución:**
   Al proyectar la voz con énfasis (picos de decibelios), el cuerpo experimenta una amortiguación elástica hacia abajo de 0.0 a 1.5 píxeles (`get_speech_inertia_at_time`), transmitiendo peso anatómico real al hablar sin desanclar la mesa fija en modo plató streamer.

### B. Módulo de Atenuación Acústica Inteligente (`audio_ducking.py`)
Controla en tiempo real las sesiones de audio de Windows mediante la API COM nativa (`pycaw` / `ISimpleAudioVolume`):
* **Disparo Automático:**
  - Al pulsar F2 (Push-to-Talk) o F3 (Manos Libres) para dictar una orden.
  - Al reproducir la síntesis vocal de respuesta de Rebeca.
* **Curvas Cinematográficas:**
  - **Fade-Out (150ms):** Atenúa suavemente Spotify, YouTube, VLC o reproductores al 20% de su volumen.
  - **Fade-In (350ms):** Restaura de forma gradual el volumen original individual memorizado en memoria.
* **Inmunidad y Seguridad Fail-Safe:**
  - Lista de exclusión estricta: inmune a OBS Studio (`obs64.exe`), Discord (`discord.exe`), Python y servicios de sistema.
  - `emergency_restore_all()`: Se dispara automáticamente ante paradas forzadas (ESC) o cierre de aplicación (`clean_exit`), garantizando que el usuario jamás quede con el sonido atenuado.

---

## 🎯 Resumen de Operatividad y Conclusión

Con esta arquitectura:
- El software es **completamente autónomo, seguro y no intrusivo**.
- **Cero costes de mantenimiento:** ni servidores que pagar ni dependencias frágiles de terceros.
- **Calidad visual cinematográfica:** 25 FPS para respiración orgánica y física de caminata, 25 FPS frame-accurate para visemas con DSP acústico y fondos de plató 16:9 listos para producción.
- **Preparado para monetización llave en mano:** con márgenes superiores al 95%.
