"""
[ES] Radio Chipset 107.7 FM - Engine de Transmisión Continua para YouTube Live (Modo Oculto).
Combina:
1. Música Cyberpunk / Lo-Fi continua en alta calidad.
2. Noticias dinámicas obtenidas vía RSS (Hackaday, Tech, Maker, Anime).
3. Debate con humor sano entre Mina y Kira generado por el servidor local BMAX (192.168.0.29).
4. Text-To-Speech multivoz (Edge-TTS) con sincronización de audio.
5. Modo Plató: Presentadoras en pantalla con cambio de planos (Mina, Kira, Plano General).
6. Emisión RTMP continua directa hacia YouTube Live usando FFmpeg.
Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
---
[EN] Radio Chipset 107.7 FM - Continuous YouTube Live Streaming Engine (Unlisted Mode).
Combines:
1. High-fidelity continuous Cyberpunk / Lo-Fi music stream.
2. Real-time tech news aggregated via RSS (Hackaday, Tech, Maker, Anime).
3. Witty, entertaining banter between Mina & Kira powered by local BMAX server (192.168.0.29).
4. Multi-voice Neural Text-To-Speech (Edge-TTS) with ducked audio timing.
5. TV Studio Mode: Dynamic camera angle switching (Mina close-up, Kira close-up, wide studio).
6. Direct RTMP live broadcast pipeline to YouTube Live powered by FFmpeg.
Created and developed by Raquel D. Torres (El taller de la gatita tech).
"""

import os
import sys
import io

if sys.platform == "win32":
    try:
        sys.stdout.reconfigure(encoding='utf-8', errors='replace')
        sys.stderr.reconfigure(encoding='utf-8', errors='replace')
    except Exception:
        pass
import time
import json
import random
import asyncio
import subprocess
import urllib.request
import xml.etree.ElementTree as ET
try:
    from PIL import Image, ImageDraw, ImageFont
except ImportError:
    Image = None
    ImageDraw = None
    ImageFont = None

import shutil

# Detección automática y transparente de FFmpeg (Windows y Linux/BMAX)
if sys.platform == "win32":
    if os.path.exists(r"C:\Program Files\kdenlive\bin\ffmpeg.exe"):
        FFMPEG_PATH = r"C:\Program Files\kdenlive\bin\ffmpeg.exe"
    else:
        FFMPEG_PATH = shutil.which("ffmpeg") or "ffmpeg"
else:
    FFMPEG_PATH = shutil.which("ffmpeg") or "ffmpeg"
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
ASSETS_DIR = os.path.join(BASE_DIR, "assets")
TEMP_DIR = os.path.join(BASE_DIR, "temp")
os.makedirs(TEMP_DIR, exist_ok=True)

# 1. Leer Configuración desde .env
def load_env():
    env_file = os.path.join(BASE_DIR, ".env")
    config = {}
    if os.path.exists(env_file):
        with open(env_file, "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#") and "=" in line:
                    k, v = line.split("=", 1)
                    config[k.strip()] = v.strip()
    return config

# 2. Rastreador de Noticias RSS en Español con Memoria Anti-Repetición
SEEN_NEWS_FILE = os.path.join(TEMP_DIR, "seen_news_cache.json")

def load_seen_news():
    if os.path.exists(SEEN_NEWS_FILE):
        try:
            with open(SEEN_NEWS_FILE, "r", encoding="utf-8") as f:
                return set(json.load(f))
        except Exception:
            return set()
    return set()

def save_seen_news(seen_set):
    try:
        # Mantener solo las últimas 150 noticias para que no crezca infinito
        recent = list(seen_set)[-150:]
        with open(SEEN_NEWS_FILE, "w", encoding="utf-8") as f:
            json.dump(recent, f, ensure_ascii=False, indent=2)
    except Exception:
        pass

def fetch_latest_tech_news():
    feeds = [
        ("Xataka", "https://feeds.weblogssl.com/xataka2"),
        ("Genbeta", "https://feeds.weblogssl.com/genbeta"),
        ("Hipertextual", "https://hipertextual.com/feed"),
        ("Microsiervos", "https://www.microsiervos.com/index.xml"),
        ("MuyComputer", "https://www.muycomputer.com/feed/")
    ]
    random.shuffle(feeds)
    seen = load_seen_news()

    for source, url in feeds:
        try:
            req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"})
            with urllib.request.urlopen(req, timeout=6) as response:
                xml_data = response.read()
                root = ET.fromstring(xml_data)
                items = root.findall(".//item")
                for item in items[:8]:
                    title_elem = item.find("title")
                    if title_elem is None or not title_elem.text:
                        continue
                    title = title_elem.text.strip()
                    if title in seen:
                        continue
                    
                    desc_elem = item.find("description")
                    desc = desc_elem.text if desc_elem is not None else title
                    import re
                    clean_desc = re.sub(r'<[^>]+>', '', desc).strip()[:250]
                    
                    # Registrar como vista
                    seen.add(title)
                    save_seen_news(seen)
                    return source, title, clean_desc
        except Exception:
            continue

    # Fallback si no hay conexión externa o todas fueron leídas
    fallbacks = [
        ("CyberNews", "Despliegue del nuevo enlace cuántico en el distrito norte", "Los ingenieros logran enlazar la red troncal de la Megatorre con cero latencia."),
        ("MakerFeed", "Sintetizador analógico de frecuencias bajas restaurado", "Un grupo de hackers del bulevar recupera un banco de osciladores de los años 80."),
        ("NeuroNet", "Actualización del protocolo de telemetría de la ciudad", "El tráfico de datos del Bulevar NeoEclipse alcanza picos históricos de transmisión.")
    ]
    return random.choice(fallbacks)

# 3. Generación de Diálogo 100% Local BMAX (Coste Cero 0,00 € - Cero Consumo de Créditos Cloud)
def generate_dialogue_bmax(bmax_url, model, source, title, summary):
    prompt = f"""Eres el guionista de la emisora cyberpunk 'Radio Chipset 107.7 FM'.
Presentadoras:
- Mina: Analítica, observadora, experta en hardware y ciencia, tono sereno y culto.
- Kira: Alegre, espontánea, divertida, humor sano, fan del anime, la música synthwave y los videojuegos.

REGLA ESTRICTA DE PRIVACIDAD:
- PROHIBIDO mencionar nombres reales ni datos privados. Todo ocurre en el universo ficticio de NeoEclipse y la Megatorre.

Noticia ({source}):
Titular: {title}
Resumen: {summary}

Escribe exactamente 3 líneas de diálogo, una por personaje, sin introducciones:
Mina: <comentario técnico breve y directo>
Kira: <reacción alegre, humor sano o referencia gamer>
Mina: <cierre breve invitando a seguir con la música en Radio Chipset>"""

    try:
        payload = json.dumps({
            "model": model,
            "prompt": prompt,
            "stream": False,
            "options": {"temperature": 0.75, "num_predict": 250}
        }).encode("utf-8")
        req = urllib.request.Request(f"{bmax_url}/api/generate", data=payload, headers={"Content-Type": "application/json"})
        with urllib.request.urlopen(req, timeout=20) as resp:
            data = json.loads(resp.read().decode("utf-8"))
            response_text = data.get("response", "").strip()

            lines = [l.strip() for l in response_text.split("\n") if l.strip()]
            dialogue = []
            for l in lines:
                if l.lower().startswith("mina:"):
                    clean_txt = l.split(":", 1)[1].strip().strip('"')
                    dialogue.append({"speaker": "Mina", "text": clean_txt})
                elif l.lower().startswith("kira:"):
                    clean_txt = l.split(":", 1)[1].strip().strip('"')
                    dialogue.append({"speaker": "Kira", "text": clean_txt})

            if len(dialogue) >= 3:
                return dialogue[:3]
    except Exception as e:
        print(f"[Radio Chipset] BMAX fallback local activado: {e}")

    # Guiones de reserva rotativos cyberpunk (Cero datos personales)
    backup_variations = [
        [
            {"speaker": "Mina", "text": f"Atención a todos los cybernautas en el dial de Radio Chipset. Nos llega una novedad técnica desde {source}: {title}."},
            {"speaker": "Kira", "text": "¡Vaya pieza de ingeniería, Mina! Como caiga en manos de los netrunners del distrito sur, nos reprograman los carteles de la Megatorre en dos minutos."},
            {"speaker": "Mina", "text": "Totalmente de acuerdo, Kira. Dejemos que la música siga fluyendo por el bulevar mientras asimilamos los paquetes de datos..."}
        ],
        [
            {"speaker": "Mina", "text": f"Nuevo boletín técnico en el 107.7 FM. Atención al siguiente reporte de {source}: {title}."},
            {"speaker": "Kira", "text": "¡Me encanta esa tecnología! Eso en mis consolas retro me daría por lo menos sesenta fotogramas más por segundo."},
            {"speaker": "Mina", "text": "Siempre pensando en exprimir los chips, Kira. Subamos el volumen de nuevo a la mesa de mezclas de Mits."}
        ],
        [
            {"speaker": "Mina", "text": f"Interrumpimos brevemente las frecuencias de Radio Chipset con este informe de {source}: {title}."},
            {"speaker": "Kira", "text": "¡Madre mía! Los avances en silicio van más rápido que un bólido por la autopista orbital."},
            {"speaker": "Mina", "text": "Efectivamente. Y para que no baje el ritmo, volvemos a la música en directo."}
        ]
    ]
    return random.choice(backup_variations)

# 4. Síntesis de Voz Multivoz con Edge-TTS Afinada (Español de España)
async def synthesize_dialogue(dialogue, out_audio_path):
    import edge_tts
    temp_clips = []
    
    # Afinación fina:
    # Mina: es-ES-ElviraNeural (pausada, aterciopelada, profesional)
    # Kira: es-ES-XimenaNeural (brillante, alegre, enérgica)
    voice_configs = {
        "Mina": {"voice": "es-ES-ElviraNeural", "rate": "-2%", "pitch": "+0Hz"},
        "Kira": {"voice": "es-ES-XimenaNeural", "rate": "+3%", "pitch": "+2Hz"}
    }

    for i, line in enumerate(dialogue):
        spk = line.get("speaker", "Mina")
        txt = line.get("text", "")
        cfg = voice_configs.get(spk, voice_configs["Mina"])
        clip_path = os.path.join(TEMP_DIR, f"speech_clip_{i}.mp3")
        communicate = edge_tts.Communicate(txt, cfg["voice"], rate=cfg["rate"], pitch=cfg["pitch"])
        await communicate.save(clip_path)
        temp_clips.append(clip_path)

    # Concatenar los clips con FFmpeg
    concat_list = os.path.join(TEMP_DIR, "concat_speech.txt")
    with open(concat_list, "w", encoding="utf-8") as f:
        for c in temp_clips:
            f.write(f"file '{c.replace(chr(92), '/')}'\n")

    cmd = [
        FFMPEG_PATH, "-y", "-f", "concat", "-safe", "0",
        "-i", concat_list, "-c", "copy", out_audio_path
    ]
    subprocess.run(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    return out_audio_path

# 5. Generador Visual del Modo Plató / Ticker en vivo
def generate_plato_frame(active_speaker, news_title, track_name, out_frame_path):
    # Fondo base del plató (1280x720)
    bg_path = r"c:\Users\asakura\Documents\cyber_companion\assets\fondo_obs_streamer_gatita_tech.png"
    if os.path.exists(bg_path):
        frame = Image.open(bg_path).convert("RGBA").resize((1280, 720))
    else:
        frame = Image.new("RGBA", (1280, 720), (12, 14, 20, 255))

    draw = ImageDraw.Draw(frame)

    # 1. Overlay Oscuro Cyberpunk semitransparente para legibilidad
    draw.rectangle([0, 0, 1280, 72], fill=(16, 20, 30, 230))
    draw.line([(0, 72), (1280, 72)], fill=(0, 255, 204, 255), width=2)

    # 2. Cabecera Radio Chipset 107.7 FM
    draw.text((28, 16), "📻 RADIO CHIPSET 107.7 FM // EN DIRECTO", fill=(0, 255, 204, 255))
    draw.text((28, 42), f"Bulevar NeoEclipse • El taller de la gatita tech • Sintonía 24/7", fill=(200, 214, 229, 255))

    # 3. Presentadoras en Pantalla (Plano Plató)
    mina_sprite = r"c:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Sprites\Mina_Kira\Mina\talk_1.png"
    kira_sprite = r"c:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Sprites\Mina_Kira\Kira\talk_1.png"

    # Si habla una de ellas, aumentamos su brillo o tamaño ligeramente
    if os.path.exists(mina_sprite):
        m_img = Image.open(mina_sprite).convert("RGBA").resize((300, 480))
        frame.paste(m_img, (120, 170), m_img)
        # Etiqueta Mina
        draw.rectangle([170, 600, 370, 634], fill=(18, 22, 34, 240), outline=(0, 255, 204, 255) if active_speaker == "Mina" else (60, 70, 90, 255), width=2)
        draw.text((195, 608), "MINA (Técnica / IA)", fill=(0, 255, 204, 255) if active_speaker == "Mina" else (180, 190, 210, 255))

    if os.path.exists(kira_sprite):
        k_img = Image.open(kira_sprite).convert("RGBA").resize((300, 480))
        frame.paste(k_img, (860, 170), k_img)
        # Etiqueta Kira
        draw.rectangle([910, 600, 1110, 634], fill=(18, 22, 34, 240), outline=(255, 0, 128, 255) if active_speaker == "Kira" else (60, 70, 90, 255), width=2)
        draw.text((930, 608), "KIRA (Cultura / Humor)", fill=(255, 0, 128, 255) if active_speaker == "Kira" else (180, 190, 210, 255))

    # 4. Ticker Inferior (Noticia + Pista musical)
    draw.rectangle([0, 660, 1280, 720], fill=(9, 11, 14, 245))
    draw.line([(0, 660), (1280, 660)], fill=(255, 0, 128, 255), width=2)
    ticker_text = f"⚡ NOTICIA: {news_title}  |  🎵 PISTA: {track_name}  |  TRANSMIENDO EN MODO OCULTO PARA PRUEBAS"
    draw.text((24, 678), ticker_text, fill=(255, 255, 255, 255))

    frame.save(out_frame_path, format="PNG")
    return out_frame_path

print("Módulo de transmisión de Radio Chipset listo.")
