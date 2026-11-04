# 📻 Radio Chipset 107.7 FM // Emisora Autónoma 24/7 de IA y Synthwave
*El taller de la gatita tech — Sistema de Streaming Neural Continuo para YouTube Live*

---

## ⚡ ¿Qué es Radio Chipset 107.7 FM?

**Radio Chipset 107.7 FM** es una estación de radiodifusión continua y autónoma ambientada en un universo cyberpunk. Está diseñada para emitir **24 horas al día, 7 días a la semana** en **YouTube Live** (en modo Oculto o Público) y proyectarse en la pantalla gigante de la Megatorre en **Meta Quest 3S (Unity 6)**.

El sistema funciona de forma 100% automatizada sin necesidad de locutores humanos:
1. **Música Continua:** Reproduce temas Synthwave, Cyberpunk y Lo-Fi en alta calidad.
2. **Escáner de Noticias en Español:** Rastrea la actualidad tecnológica real (Xataka, Genbeta, Hipertextual, Microsiervos, MuyComputer) con filtro anti-repetición de 48 horas.
3. **Cerebro de Guion Local (BMAX):** La IA local (`llama3.2:3b` en Ollama) genera debates orgánicos y divertidos entre las dos presentadoras con cultura pop/gamer y lore de la ciudad.
4. **Locución Neural de Alta Fidelidad:** Voces diferenciadas en castellano de España (Edge-TTS) para **Mina** (analítica, pausada) y **Kira** (alegre, espontánea).
5. **Realización de Plató Visual:** Alterna automáticamente entre el vídeo animado del plató de noticias (**Kira y Mina**) y la cabina de sonido en modo Lofi Worker con la productora (**Mits y los 4 gatos**).

---

## 🎮 Cadencia de Emisión de Radio

Para que la experiencia se sienta como una radio profesional y no un bucle repetitivo:
* **Fase Musical (Cabina de Mits):** Suenan **2 o 3 canciones completas** seguidas (~7 a 10 minutos) con el plano de Mits produciendo en la mesa de mezclas con los gatos y el volumen al 100%.
* **Fase de Noticias (Plató de Kira y Mina):** Entra una **intervención de 45 a 60 segundos** donde la cámara pasa al plató. La música baja automáticamente de volumen (*audio ducking* al 14%) mientras las presentadoras debaten la noticia.
* **Transición:** La música vuelve a sonar al 100% y la cámara regresa a la cabina de sonido.

---

## 🚀 Cómo Ponerla a Funcionar

El proyecto está preparado para funcionar tanto desde tu **ordenador Windows** como de forma permanente en tu **servidor BMAX (Linux / LGM-os)** para que puedas apagar el portátil.

---

### Opción A: Ejecutar en el Servidor BMAX (Modo 24/7 Autónomo)
*(Recomendado: La emisora se queda emitiendo día y noche sin depender de tu portátil)*

La emisora ya está desplegada en:
📁 **Ruta en el BMAX:** `/DATA/AppData/emisora_247` *(o `~/emisora_247` según la estructura de LGM-os)*  
📁 **Ruta desde Windows:** `\\GATITA-TALLER-SERVER\keysharu\emisora_247`

#### Paso 1: Entrar a la terminal del BMAX
Puedes entrar mediante la interfaz web de CasaOS/LGM-os (abriendo la Terminal) o por SSH desde tu consola:
```bash
ssh keysharu@192.168.0.29
```

#### Paso 2: Ir a la carpeta y dar permisos de ejecución
```bash
cd /DATA/AppData/emisora_247
# O si está en tu carpeta home:
# cd ~/emisora_247

chmod +x iniciar_emisora_linux.sh
```

#### Paso 3: Arrancar la emisión permanente
Para arrancarla y que **siga emitiendo aunque cierres la terminal o apagues tu portátil**:
```bash
nohup ./iniciar_emisora_linux.sh > emisora.log 2>&1 &
```

* **Para ver cómo va emitiendo en tiempo real:**
  ```bash
  tail -f emisora.log
  ```
* **Para detener la emisión cuando quieras:**
  ```bash
  pkill -f live_broadcaster.py
  ```

---

### Opción B: Ejecutar en Windows (Portátil / Pruebas)

1. Abre la carpeta del proyecto en Windows:
   `C:\Users\asakura\Documents\home lab\emisora 247`
2. **Doble clic en el archivo:**
   [`INICIAR_RADIO_YOUTUBE.bat`](file:///C:/Users/asakura/Documents/home%20lab/emisora%20247/INICIAR_RADIO_YOUTUBE.bat)
3. Se abrirá una ventana de terminal cian indicando el estado del directo y comenzará a emitir a YouTube.

---

## 🛠️ Configuración y Variables (`.env`)

En el archivo [`.env`](file:///C:/Users/asakura/Documents/home%20lab/emisora%20247/.env) puedes personalizar el comportamiento de la emisora:

```env
# Clave privada de emisión que te da YouTube Studio
YOUTUBE_STREAM_KEY=kku3-gmu4-cvqg-v6gv-7f48

# Servidor RTMP de YouTube
YOUTUBE_RTMP_URL=rtmp://a.rtmp.youtube.com/live2

# Servidor BMAX de IA Local (Ollama)
# En el portátil usa la IP de red: http://192.168.0.29:11434
# Si corre dentro del BMAX puedes usar: http://127.0.0.1:11434
BMAX_SERVER=http://192.168.0.29:11434
BMAX_MODEL=llama3.2:3b

# Número de canciones completas entre cada bloque de noticias
SONGS_PER_NEWS=2
```

---

## 📁 Estructura del Proyecto

```text
emisora_247/
├── README.md                          # Este manual operativo
├── MEMORIA_Y_ROADMAP_EMISORA_247.md   # Memoria técnica de ingeniería y hoja de ruta futura
├── .env                               # Claves de emisión y configuración del servidor
├── INICIAR_RADIO_YOUTUBE.bat          # Lanzador automático para Windows
├── iniciar_emisora_linux.sh           # Lanzador autónomo para BMAX / Linux
├── live_broadcaster.py                 # Motor principal de streaming RTMP continuo
├── stream_engine.py                    # Rastreador RSS, sintetizador Edge-TTS y mezcla
├── video_stitcher.py                   # Utilidad de fundido suave y bucle transparente
├── musica/                             # Biblioteca de pistas MP3 (Suno / Synthwave)
├── videos/                             # Bucles de vídeo 720p 24fps generados en Flow
│   ├── kira_mina_plato_60s.mp4         # Plató de noticias (Kira y Mina)
│   └── mits_studio_lofi_100s.mp4       # Cabina de sonido (Mits y los 4 gatos)
├── docs/                               # Fichas de personajes y prompts validados de Flow
└── temp/                               # Búfer de trabajo (audios temporales y caché RSS)
```

---

## ➕ Cómo Añadir Más Música o Cambiar Vídeos

* **Añadir canciones:** Descarga cualquier canción de Suno en formato **MP3** y pégala dentro de la carpeta `musica/`. El script la detecta y la incorpora a la rotación automáticamente.
* **Actualizar vídeos:** Si generas nuevas secuencias en Google Flow, solo guárdalas en formato **MP4 (1280x720 a 24fps)** dentro de la carpeta `videos/` reemplazando los archivos existentes.

---

## 🛰️ Visualización en Vivo
* **Panel de Control de Emisión:** [YouTube Studio Live](https://studio.youtube.com/channel/UC/livestreaming)
* **Enlace de visualización:** `https://www.youtube.com/watch?v=KxGQYS8wMfg`
