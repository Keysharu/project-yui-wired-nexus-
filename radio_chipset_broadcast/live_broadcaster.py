"""
[ES] Radio Chipset 107.7 FM - Bucle de Transmisión Continua a YouTube Live (Modo Oculto).
Emite alternando:
- Bloque Noticias/Debate: Vídeo animado de Kira y Mina en el plató con voces neuronales Edge-TTS.
- Bloque Musical: Vídeo animado de Mits en la mesa de mezclas con los 4 gatos (Lofi Worker).
Creado y desarrollado por Raquel D. Torres (El taller de la gatita tech).
---
[EN] Radio Chipset 107.7 FM - Continuous YouTube Live Broadcast Loop (Unlisted Stream).
Alternates between:
- News/Debate Block: Animated video of Kira & Mina in studio with Edge-TTS neural voices.
- Music Block: Animated video of Mits at the mixing console with the 4 cats (Lofi Worker).
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
import glob
import time
import random
import asyncio
import subprocess
from stream_engine import (
    load_env,
    fetch_latest_tech_news,
    generate_dialogue_bmax,
    synthesize_dialogue,
    FFMPEG_PATH,
    BASE_DIR,
    TEMP_DIR
)

# Detección flexible de assets tanto en 'radio_chipset_broadcast' como en 'emisora 247'
VIDEOS_DIR = os.path.join(BASE_DIR, "videos") if os.path.exists(os.path.join(BASE_DIR, "videos")) else os.path.join(BASE_DIR, "assets")
MUSICA_DIR = os.path.join(BASE_DIR, "musica") if os.path.exists(os.path.join(BASE_DIR, "musica")) else os.path.join(BASE_DIR, "radio_assets", "music_tracks")

KIRA_MINA_VIDEO = os.path.join(VIDEOS_DIR, "kira_mina_plato_60s.mp4")
MITS_VIDEO = os.path.join(VIDEOS_DIR, "mits_studio_lofi_100s.mp4")

def get_playlist():
    files = []
    if os.path.exists(MUSICA_DIR):
        files = glob.glob(os.path.join(MUSICA_DIR, "*.mp3"))
    if not files:
        # Fallback a Unity Assets si no se encuentran en la carpeta local
        unity_dir = r"c:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Audio\Battle\Normal"
        if os.path.exists(unity_dir):
            files = glob.glob(os.path.join(unity_dir, "*.mp3"))
    return files if files else [r"c:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets\Audio\Battle\Normal\Ronin_Code_Focus_Beats.mp3"]

def check_stream_key(env):
    key = env.get("YOUTUBE_STREAM_KEY", "").strip()
    if not key or key == "" or "xxxx" in key:
        print("=" * 70)
        print("⚠️ AVISO: FALTA LA CLAVE DE EMISIÓN DE YOUTUBE")
        print(f"Pega tu Stream Key en el archivo: {os.path.join(BASE_DIR, '.env')}")
        print("Línea: YOUTUBE_STREAM_KEY=xxxx-xxxx-xxxx-xxxx")
        print("=" * 70)
        return None
    return key

async def broadcast_loop():
    env = load_env()
    stream_key = check_stream_key(env)
    if not stream_key:
        sys.exit(1)

    rtmp_url = f"{env.get('YOUTUBE_RTMP_URL', 'rtmp://a.rtmp.youtube.com/live2')}/{stream_key}"
    bmax_url = env.get("BMAX_SERVER", "http://192.168.0.29:11434")
    bmax_model = env.get("BMAX_MODEL", "llama3.2:3b")
    songs_per_news = int(env.get("SONGS_PER_NEWS", "2"))

    print("\n" + "=" * 70)
    print("📻 RADIO CHIPSET 107.7 FM - TRANSMISIÓN CONTINUA A YOUTUBE LIVE")
    print(f"📡 Destino RTMP: rtmp://a.rtmp.youtube.com/live2/**** (Modo Oculto)")
    print(f"🧠 Servidor IA Local: BMAX en {bmax_url} ({bmax_model})")
    print(f"⏱️ Cadencia de Radio: 1 bloque de noticias cada {songs_per_news} canciones")
    print(f"🎬 Vídeo Noticias: Kira & Mina (60s loop) -> {os.path.exists(KIRA_MINA_VIDEO)}")
    print(f"🎧 Vídeo Musical: Mits & 4 Gatos (100s loop) -> {os.path.exists(MITS_VIDEO)}")
    print("=" * 70 + "\n")

    playlist = get_playlist()
    track_idx = 0
    segment_count = 1

    while True:
        # =========================================================================
        # FASE 1: BLOQUE DE NOTICIAS CON KIRA Y MINA (1 INTERVENCIÓN CADA X CANCIONES)
        # =========================================================================
        source, title, summary = fetch_latest_tech_news()
        print(f"\n[Bloque #{segment_count} - Noticias] 📡 Rastreando actualidad: {source} -> {title}")
        print("   🧠 Generando debate en BMAX...")
        dialogue = generate_dialogue_bmax(bmax_url, bmax_model, source, title, summary)
        for line in dialogue:
            print(f"   🎙️ {line.get('speaker')}: {line.get('text')}")

        speech_audio = os.path.join(TEMP_DIR, f"speech_segment_{segment_count}.mp3")
        await synthesize_dialogue(dialogue, speech_audio)

        # Música de fondo para la intervención
        bg_music = playlist[track_idx % len(playlist)]
        news_audio = os.path.join(TEMP_DIR, f"news_mixed_{segment_count}.mp3")
        filter_ducking = (
            "[0:a]volume=1.0[vocal];"
            "[1:a]volume=0.14[bgmusic];"
            "[vocal][bgmusic]amix=inputs=2:duration=first:dropout_transition=2[aout]"
        )
        mix_news = [
            FFMPEG_PATH, "-y",
            "-i", speech_audio,
            "-stream_loop", "-1", "-i", bg_music,
            "-filter_complex", filter_ducking,
            "-map", "[aout]",
            "-c:a", "libmp3lame", "-b:a", "192k",
            news_audio
        ]
        subprocess.run(mix_news, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

        # Emitir Bloque Noticias a YouTube con el vídeo de Kira y Mina
        print(f"[Radio Chipset] 📡 Emitiendo Plató en Directo (Kira y Mina hablando)...")
        active_video = KIRA_MINA_VIDEO if os.path.exists(KIRA_MINA_VIDEO) else os.path.join(BASE_DIR, "assets", "plato_mina_kira.mp4")
        stream_news_cmd = [
            FFMPEG_PATH, "-re",
            "-stream_loop", "-1", "-i", active_video,
            "-i", news_audio,
            "-map", "0:v:0", "-map", "1:a:0",
            "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "ultrafast",
            "-tune", "zerolatency",
            "-g", "48", "-keyint_min", "24", "-sc_threshold", "0",
            "-b:v", "2500k", "-maxrate", "3000k", "-bufsize", "5000k",
            "-c:a", "aac", "-b:a", "160k", "-ar", "44100",
            "-shortest",
            "-f", "flv", rtmp_url
        ]
        subprocess.run(stream_news_cmd)

        # =========================================================================
        # FASE 2: BLOQUE MUSICAL CON MITS Y LOS GATOS (2-3 CANCIONES COMPLETAS)
        # =========================================================================
        for song_num in range(1, songs_per_news + 1):
            current_music = playlist[track_idx % len(playlist)]
            track_name = os.path.basename(current_music).replace(".mp3", "").replace("_", " ")
            track_idx += 1

            print(f"\n[Música {song_num}/{songs_per_news}] 🎵 Reproduciendo: {track_name}")
            print(f"[Radio Chipset] 🎧 Cámara en la Cabina: Mits y los 4 gatos...")
            active_mits = MITS_VIDEO if os.path.exists(MITS_VIDEO) else active_video
            stream_music_cmd = [
                FFMPEG_PATH, "-re",
                "-stream_loop", "-1", "-i", active_mits,
                "-i", current_music,
                "-map", "0:v:0", "-map", "1:a:0",
                "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "ultrafast",
                "-tune", "zerolatency",
                "-g", "48", "-keyint_min", "24", "-sc_threshold", "0",
                "-b:v", "2500k", "-maxrate", "3000k", "-bufsize", "5000k",
                "-c:a", "aac", "-b:a", "160k", "-ar", "44100",
                "-shortest",
                "-f", "flv", rtmp_url
            ]
            subprocess.run(stream_music_cmd)

        segment_count += 1
        time.sleep(1)

        segment_count += 1
        time.sleep(1)

if __name__ == "__main__":
    try:
        asyncio.run(broadcast_loop())
    except KeyboardInterrupt:
        print("\n[Radio Chipset] Transmisión pausada por el operador.")
