# 📻 Radio Chipset 107.7 FM — Memoria Técnica y Roadmap del Proyecto Autónomo
*El taller de la gatita tech — Sistema de Radiodifusión Autónoma 24/7 con IA Local y Streaming Neural*

---

## 🏛️ 1. Visión y Fundamentos del Proyecto Independiente

**Radio Chipset 107.7 FM** nace inicialmente como un componente inmersivo para la pantalla de la Megatorre en el juego de realidad mixta (*quest_mr_nexus* en Unity 6), pero su potencial arquitectónico lo define como un **producto y servicio completamente autónomo dentro del ecosistema Home Lab**.

### El Concepto
Una estación de radiodifusión continua (24 horas al día, 7 días a la semana) ambientada en un universo cyberpunk/maker, operada íntegramente por agentes de Inteligencia Artificial que se ejecutan en servidores locales (BMAX en red doméstica), capaz de:
1. **Emitir música continua en alta calidad:** Pistas synthwave, cyberpunk, lofi y electrónica procedentes de la discografía del taller.
2. **Escanear la actualidad tecnológica en tiempo real:** Rastrear noticias de ciencia, hardware, open source, inteligencia artificial y hacking ético en fuentes en español.
3. **Generar debates orgánicos sin intervención humana:** Dos presentadoras virtuales (**Mina** y **Kira**) analizan las noticias con personalidades diferenciadas, humor sano y cultura pop/maker.
4. **Locución neural en castellano:** Síntesis vocal neural con prosodia ajustada en español de España.
5. **Realización visual dinámica:** Alternancia entre el plató de televisión/podcast de las presentadoras y la cabina de sonido en modo *Lofi Worker* con la productora **Mits** y sus cuatro gatos.
6. **Alimentación multicanal:** Emisión pública/oculta a **YouTube Live** y retransmisión local en baja latencia (HLS/RTSP) para proyectarse en la pantalla gigante de la Megatorre en Meta Quest 3S.

---

## 🏗️ 2. Arquitectura Técnica Desacoplada (Home Lab Stack)

```
[Fuentes RSS / Scraping] ───> [Extractor & Deduplicador]
                                       │
                                       ▼
                       [Servidor BMAX (192.168.0.29)]
                          Ollama: llama3.2:3b
                       (Guionista Cyberpunk Local)
                                       │
                                       ▼
                       [Motor de Síntesis Vocal Neural]
                             Edge-TTS (es-ES)
                       Mina: ElviraNeural | Kira: XimenaNeural
                                       │
                                       ▼
        ┌─────────────────────────────────────────────────────────────┐
        │                 MOTOR AUDIOVISUAL & CADENCIA                │
        │                                                             │
        │  [Fase Noticias]: Vídeo Plató Kira & Mina (Flow 720p)       │
        │  [Fase Música]:   Vídeo Cabina Mits & 4 Gatos (Flow 720p)   │
        │  [Audio Ducking]: Música baja al 15% durante la voz         │
        └─────────────────────────────────────────────────────────────┘
                                       │
                                       ▼
                       [Transmisor RTMP FFmpeg Ultra-Fast]
                       - GOP estricto: 2.0s (-g 48 a 24fps)
                       - Bitrate: 2500 - 4500 Kbps
                                       │
                    ┌──────────────────┴──────────────────┐
                    ▼                                     ▼
           [YouTube Live 24/7]             [Megatorre VR / Quest 3S]
```

---

## ⚙️ 3. Especificaciones Quirúrgicas de la Fase Actual (MVP Operativo)

### A. Cadencia de Emisión de Radio Real (Programación)
La radio no interrumpe la música tras cada tema. La cadencia profesional establecida es:
* **Bloque Musical Principal:** **2 a 3 canciones completas** consecutivas (~7 a 10 minutos). La cámara permanece en el plano relajante de **Mits y los 4 gatos** con la música al 100% de volumen.
* **Bloque de Noticias e Intervención:** **1 intervención de 40 a 60 segundos** donde la cámara pasa al plató de **Mina y Kira**. La música baja automáticamente al 15% de volumen (*audio ducking*) y las voces neuronales debaten la noticia.
* **Jingle / Transición:** Retorno a la música con la cabina de sonido.

### B. Fuentes de Noticias en Español (Rotativas y con Memoria Anti-Repetición)
Para garantizar variedad infinita y evitar que se repitan noticias:
* **Fuentes RSS integradas:**
  - *Xataka:* Tecnología, hardware y ciencia.
  - *Genbeta:* Software, IA, sistemas y desarrollo.
  - *Hipertextual:* Innovación, cultura digital y espacio.
  - *Microsiervos:* Cultura hacker, informática clásica y tecnología.
  - *MuyComputer:* Componentes, electrónica y microprocesadores.
* **Filtro Anti-Repetición:** Registro en disco (`temp/seen_news_cache.json`) de los últimos 200 titulares transmitidos para garantizar que nunca se debata la misma noticia dos veces en un intervalo de 48 horas.

### C. Cerebro de Guion Local en BMAX (`llama3.2:3b`)
* **Modelo asignado:** `llama3.2:3b` (reemplaza al 1b por su excelente fluidez y vocabulario en castellano).
* **Protocolo de Salida por Líneas (Zero-Fail Parser):** En lugar de forzar JSON que puede corromperse por saltos de línea, el generador exige el formato natural:
  ```text
  Mina: <análisis técnico sobrio, sin clichés>
  Kira: <reacción alegre, referencia gamer/anime o humor sano>
  Mina: <cierre natural dando paso a la música de Mits>
  ```
* **Blindaje de Privacidad y Lore:** Prohibición absoluta de mencionar nombres u operadores reales. Todo ocurre en el universo del juego (*Bulevar NeoEclipse, la Megatorre, los distritos del ciberespacio*).

### D. Afinación Acústica de Voces Neurales (Español de España)
* **Mina (Ingeniera y Analista Técnica):**
  - Voz: `es-ES-ElviraNeural`
  - Calibración: `rate="-2%"`, `pitch="+0Hz"`
  - Carácter: Locutora de radio nocturna, pausada, culta y reflexiva.
* **Kira (Idol y Netrunner):**
  - Voz: `es-ES-XimenaNeural`
  - Calibración: `rate="+3%"`, `pitch="+2Hz"`
  - Carácter: Streamer joven, brillante, alegre, dinámica y expresiva.

---

## 🗺️ 4. Roadmap de Desarrollo y Evolución Autónoma

### 📍 Fase 1: Estabilización y Despliegue en BMAX (Semana en curso)
- [x] Generación de planos base de alta fidelidad en Google Flow (Plató Kira/Mina 60s + Cabina Mits 100s).
- [x] Configuración de stream RTMP a YouTube con fotogramas clave a 2.0s y codificación `ultrafast`.
- [x] Pruebas de fuego superadas en YouTube Studio (recepción de señal «Buena» confirmada).
- [ ] **Migración a Demonio Autónomo en Servidor BMAX:** Configurar el script de Python + FFmpeg directamente en el servidor BMAX (`192.168.0.29`), ejecutándose como servicio de fondo (*systemd* o tarea programada) para que la emisora emita 24/7 con el portátil apagado.
- [ ] **Ajuste de Cadencia:** 3 temas musicales por cada bloque de noticias.
- [ ] **Ampliación del pool de RSS en castellano con caché anti-duplicados.**

---

### 📍 Fase 2: Sincronización Labial y Animación Neuronal Dinámica (Próxima Iteración)
*El gran salto de inmersión para eliminar la desconexión entre voz y boca:*
- [ ] **Pipeline de Lipsync en Servidor Local (BMAX):**
  - Evaluar e integrar **Wav2Lip** o **SadTalker** ejecutado en la GPU/CPU de BMAX.
  - Al generarse el archivo de audio de Mina y Kira, el modelo procesa los recortes de cara de las presentadoras sobre el fondo del vídeo y deforma los labios en tiempo real para sincronizarlos con las sílabas exactas pronunciadas por Edge-TTS.
- [ ] **Micro-expresiones reactivas:** Parpadeo aleatorio, cejas que se arquean con signos de interrogación o exclamación, y sonrisas automáticas al detectar palabras de humor en el guion.
- [ ] **Transiciones Cyberpunk (Stinger):** Introducción de un clip de 0.4s de interferencia analógica/glitch cian entre la cabina de sonido y el plató de televisión.

---

### 📍 Fase 3: Interacción Viva con la Audiencia y Economía Virtual
- [ ] **Bot Lector del Chat de YouTube:** El script de BMAX consulta periódicamente los comentarios del chat en directo de YouTube Live.
- [ ] **Respuestas en Vivo:** Si un espectador saluda o hace una pregunta sobre hardware en el chat, Mina o Kira leen el mensaje al final del bloque de noticias y le responden en directo con su voz.
- [ ] **Peticiones de Canciones:** Sistema de comandos en el chat (`!pedir <tema>`) para alterar la cola musical de la emisora.
- [ ] **Integración con Unity 6 (Megatorre):** La emisión de YouTube o un stream local directo se proyecta en el shader de la pantalla gigante de la Megatorre, permitiendo a los jugadores en Meta Quest 3S escuchar la misma radio que la gente de YouTube en tiempo real.

---

## 📁 5. Estructura de Archivos en el Home Lab

```text
C:\Users\asakura\Documents\home lab\emisora 247/
├── MEMORIA_Y_ROADMAP_EMISORA_247.md   # Este documento
├── INICIAR_RADIO_YOUTUBE.bat          # Lanzador Windows
├── live_broadcaster.py                 # Bucle orquestador de emisión continua
├── stream_engine.py                    # Motor de RSS, guion BMAX, TTS y mezcla
├── video_stitcher.py                   # Utilidad de disolución suave y bucle
├── .env                                # Claves de YouTube y configuración BMAX
├── musica/                             # Colección de pistas MP3 (Synthwave / Lofi)
├── videos/                             # Vídeos de Google Flow (Kira/Mina y Mits)
│   ├── kira_mina_plato_60s.mp4
│   └── mits_studio_lofi_100s.mp4
├── docs/                               # Guías de prompts de Flow y referencias
└── temp/                               # Archivos temporales de audio y render
```
