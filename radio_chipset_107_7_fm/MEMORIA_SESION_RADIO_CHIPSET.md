# 📻 Memoria de Sesión y Cierre Técnico: Radio Chipset 107.7 FM
*El taller de la gatita tech — Sistema de Transmisión Continua con IA Local y YouTube Live*
**Fecha:** 2 de Noviembre de 2026

---

## 🎯 1. Contexto y Objetivos Cumplidos

La sesión tuvo como propósito consolidar, desacoplar y automatizar la emisora **Radio Chipset 107.7 FM** para que funcione de forma autónoma desde el servidor local **BMAX (LGM-os / 192.168.0.29)**, transmitiendo 24/7 a **YouTube Live** y alimentando la pantalla holográfica gigante de la Megatorre en **Meta Quest 3S (Unity 6)**.

---

## 🏆 2. Hitos y Logros Técnicos Verificados

### A. Producción de Vídeo en Google Flow (720p 24fps H.264)
- **Plató de Noticias (Kira y Mina):** Vídeo de **60 segundos** de duración donde Kira y Mina interactúan en el estudio con micrófonos dedicados, ecualizadores holográficos y gestos fluidos.
- **Cabina de Sonido (Mits y los 4 Gatos):** Vídeo de **100 segundos** de duración en estética *Lofi Worker* con Mits operando la mesa de mezclas rodeada de cuatro gatos con animación orgánica y relajante.
- **Diseño del Concierto Virtual de Kira:** Creado el prompt maestro para situar a Kira cantando en un escenario flotante hexagonal sobre los rascacielos de NeoEclipse, con estadios holográficos, láseres volumétricos y público de avatares con *lightsticks*.

### B. Cerebro de Guion Local en BMAX (`llama3.2:3b`)
- **Migración a 3.2B:** Se sustituyó el modelo de 1B por `llama3.2:3b` en Ollama (`http://192.168.0.29:11434`), eliminando cuelgues por formato JSON.
- **Protocolo de 3 Líneas:** Parser robusto (`Mina:` / `Kira:` / `Mina:`) que genera en menos de 15 segundos diálogos fluidos, con humor sano y referencias pop/gamer orgánicas (ej. *Metal Gear Solid*).
- **Blindaje Estricto de Privacidad:** Prohibición absoluta de mencionar nombres de personas reales, operadoras o datos del mundo real; universo 100% ficticio cyberpunk (*Bulevar NeoEclipse, la Megatorre, el taller clandestino*).

### C. Escáner RSS en Español con Memoria Anti-Duplicados
- **Fuentes activas:** *Xataka*, *Genbeta*, *Hipertextual*, *Microsiervos* y *MuyComputer*.
- **Caché persistente:** Archivo `seen_news_cache.json` que registra los últimos titulares para evitar repetir noticias durante 48 horas.

### D. Afinación de Voces Neurales (Español de España)
- **Mina:** `es-ES-ElviraNeural` (`rate="-2%"`, `pitch="+0Hz"`, locución pausada, culta y profesional).
- **Kira:** `es-ES-XimenaNeural` (`rate="+3%"`, `pitch="+2Hz"`, locución enérgica, alegre y juvenil).

### E. Cadencia de Emisión Profesional
- Configurable en `.env` mediante `SONGS_PER_NEWS=2` (o 3):
  - **Fase Musical:** 2 o 3 canciones completas consecutivas con cámara en Mits y los gatos (volumen 100%).
  - **Fase Noticias:** Intervención de 45 a 60 segundos con cámara en Kira y Mina (*audio ducking* al 14%).

### F. Parámetros de Transmisión RTMP Optimizados para YouTube
- **GOP estricto:** `-g 48 -keyint_min 24 -sc_threshold 0` (fotogramas clave cada 2.0s exactos, eliminando la advertencia de YouTube).
- **Velocidad de codificación:** `-preset ultrafast -tune zerolatency` a 2500 Kbps (velocidad de render superior a 1.03x en tiempo real).

---

## 📦 3. Despliegue Físico en el Servidor BMAX

Se transfirieron íntegramente los **99 archivos** del ecosistema al almacenamiento en red del BMAX:
* **Ruta de red SMB:** `\\GATITA-TALLER-SERVER\keysharu\emisora_247`
* **Punto de montaje en Linux:** `/mnt/datos/homes/keysharu/archivos/emisora_247`

---

## 🗂️ 4. Mapa de Documentación y Código

| Archivo | Función Principal | Ubicaciones Sincronizadas |
| :--- | :--- | :--- |
| **`MEMORIA_Y_ROADMAP_EMISORA_247.md`** | Memoria técnica de ingeniería y fases 2/3 (lipsync, avatares 3D). | Local, BMAX y Proyecto |
| **`docs/GUIA_PROMPTS_GOOGLE_FLOW.md`** | Manual completo de prompts (Kira, Mina, Mits, gatos, concierto). | Local, BMAX y Proyecto |
| **`README.md`** | Manual operativo paso a paso de uso y mantenimiento. | Local, BMAX y Proyecto |
| **`live_broadcaster.py`** | Motor de transmisión continua multiplataforma (Win/Linux). | Local, BMAX y Proyecto |
| **`stream_engine.py`** | Lógica de noticias RSS, conexión BMAX, síntesis TTS y mezcla. | Local, BMAX y Proyecto |
| **`iniciar_emisora_linux.sh`** | Lanzador de fondo con verificación de dependencias para Linux. | Local, BMAX y Proyecto |
| **`INICIAR_RADIO_YOUTUBE.bat`** | Lanzador rápido de un clic para Windows. | Local, BMAX y Proyecto |
| **`.env`** | Stream key de YouTube, URL de BMAX y configuración de cadencia. | Local, BMAX y Proyecto |

---

## 🚀 5. Protocolo de Arranque Autónomo en BMAX (Terminal LGM-os)

Para poner a emitir el BMAX en solitario sin depender del portátil:

```bash
cd /mnt/datos/homes/keysharu/archivos/emisora_247
pip3 install edge-tts pillow --break-system-packages
nohup python3 live_broadcaster.py > emisora.log 2>&1 &
```

* **Monitorizar la emisión en vivo:**
  ```bash
  tail -f emisora.log
  ```
* **Detener la emisión:**
  ```bash
  pkill -f live_broadcaster.py
  ```

---
*Certificado y archivado por Rebeca y Tachikoma Navi para El taller de la gatita tech.*
