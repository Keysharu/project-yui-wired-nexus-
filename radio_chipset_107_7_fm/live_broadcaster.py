"""
Radio Chipset 107.7 FM - Bucle de Transmisión Continua a YouTube Live (Modo Oculto).
Emite alternando:
- Bloque Noticias/Debate: Vídeo animado de Kira y Mina en el plató con voces neuronales Edge-TTS.
- Bloque Musical: Vídeo animado de Mits en la mesa de mezclas con los 4 gatos (Lofi Worker).

ARQUITECTURA SEAMLESS (CERO PAUSAS):
- Pre-buffering en segundo plano: Las noticias se consultan en RSS, se guionizan con BMAX
  y se mezclan con atenuación sonora (ducking) MIENTRAS la música está sonando.
- Cuando termina la canción, el audio de las noticias ya está 100% renderizado en disco.
- Transición instantánea: Cero segundos de espera y eliminación de buffer underrun en YouTube Live.

Diseñado para El taller de la gatita tech por Rebeca.
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

# Detección flexible de assets
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

async def prepare_news_segment(segment_num, bg_music, bmax_url, bmax_model):
    """
    Worker Asíncrono de Fondo:
    Pre-renderiza el siguiente bloque de noticias en paralelo mientras la música suena.
    Cuando la canción termina, el archivo de audio ya existe y está listo en disco.
    """
    try:
        print(f"\n[Worker Asíncrono] ⚡ Pre-renderizando Bloque #{segment_num} de Noticias en segundo plano...")
        
        # 1. Rastrear RSS sin bloquear el bucle de eventos
        source, title, summary = await asyncio.to_thread(fetch_latest_tech_news)
        print(f"[Worker Asíncrono] 📡 Titular capturado ({source}): {title}")

        # 2. Inferencia en el servidor BMAX (Ollama)
        print("[Worker Asíncrono] 🧠 BMAX generando guion para Mina y Kira...")
        dialogue = await asyncio.to_thread(
            generate_dialogue_bmax, bmax_url, bmax_model, source, title, summary
        )

        # 3. Síntesis multivoz con Edge-TTS
        speech_audio = os.path.join(TEMP_DIR, f"speech_segment_{segment_num}.mp3")
        await synthesize_dialogue(dialogue, speech_audio)

        # 4. Mezcla con ducking (atenuación musical) en FFmpeg
        news_audio = os.path.join(TEMP_DIR, f"news_mixed_{segment_num}.mp3")
        filter_ducking = (
            "[0:a]volume=1.0[vocal];"
            "[1:a]volume=0.14[bgmusic];"
            "[vocal][bgmusic]amix=inputs=2:duration=first:dropout_transition=2[aout]"
        )
        mix_news_cmd = [
            FFMPEG_PATH, "-y",
            "-i", speech_audio,
            "-stream_loop", "-1", "-i", bg_music,
            "-filter_complex", filter_ducking,
            "-map", "[aout]",
            "-c:a", "libmp3lame", "-b:a", "192k",
            news_audio
        ]
        
        proc = await asyncio.create_subprocess_exec(
            *mix_news_cmd,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL
        )
        await proc.wait()

        print(f"[Worker Asíncrono] ✅ Bloque #{segment_num} preparado y guardado en disco (0s de espera para el pase).")
        return {
            "segment_num": segment_num,
            "source": source,
            "title": title,
            "dialogue": dialogue,
            "audio_path": news_audio
        }
    except Exception as e:
        print(f"[Worker Asíncrono] ⚠️ Error en pre-buffering (usando fallback seguro): {e}")
        # En caso de excepción, preparamos un fallback rápido
        fallback_audio = os.path.join(TEMP_DIR, f"news_mixed_{segment_num}.mp3")
        return {
            "segment_num": segment_num,
            "source": "Radio Chipset",
            "title": "Intervención de cabina en directo",
            "dialogue": [{"speaker": "Mina", "text": "Seguimos en sintonía con la mejor selección musical en Radio Chipset."}],
            "audio_path": fallback_audio if os.path.exists(fallback_audio) else bg_music
        }

async def stream_clip_async(cmd, description=""):
    """Ejecuta FFmpeg de forma asíncrona sin congelar las tareas de fondo de asyncio."""
    if description:
        print(f"[Radio Chipset] 📡 {description}")
    proc = await asyncio.create_subprocess_exec(*cmd)
    return await proc.wait()

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
    print("📻 RADIO CHIPSET 107.7 FM - TRANSMISIÓN CONTINUA A YOUTUBE LIVE (SEAMLESS)")
    print(f"📡 Destino RTMP: rtmp://a.rtmp.youtube.com/live2/**** (Modo Oculto)")
    print(f"🧠 Servidor IA Local: BMAX en {bmax_url} ({bmax_model})")
    print(f"⏱️ Cadencia de Radio: 1 bloque de noticias cada {songs_per_news} canciones")
    print(f"🎬 Vídeo Noticias: Kira & Mina (60s loop) -> {os.path.exists(KIRA_MINA_VIDEO)}")
    print(f"🎧 Vídeo Musical: Mits & 4 Gatos (100s loop) -> {os.path.exists(MITS_VIDEO)}")
    print(f"⚡ Pipeline: Pre-buffering en segundo plano activo (Cero pausas de red)")
    print("=" * 70 + "\n")

    playlist = get_playlist()
    track_idx = 0
    segment_count = 1

    # Pre-cocinamos el PRIMER bloque de noticias de entrada para arrancar sin demora
    bg_music_init = playlist[track_idx % len(playlist)]
    next_news_task = asyncio.create_task(
        prepare_news_segment(segment_count, bg_music_init, bmax_url, bmax_model)
    )

    while True:
        # =========================================================================
        # FASE 1: BLOQUE DE NOTICIAS CON KIRA Y MINA (YA PRE-RENDERIZADO)
        # =========================================================================
        # Esperamos a que termine la tarea en segundo plano (si ya terminó, tarda 0.0s)
        news_data = await next_news_task
        
        print(f"\n[En el Aire: Bloque #{news_data['segment_num']} - Noticias y Debate]")
        print(f"   📡 Titular: {news_data['title']} ({news_data['source']})")
        for line in news_data.get('dialogue', []):
            print(f"   🎙️ {line.get('speaker')}: {line.get('text')}")

        active_video = KIRA_MINA_VIDEO if os.path.exists(KIRA_MINA_VIDEO) else os.path.join(BASE_DIR, "assets", "plato_mina_kira.mp4")
        stream_news_cmd = [
            FFMPEG_PATH, "-re",
            "-stream_loop", "-1", "-i", active_video,
            "-i", news_data["audio_path"],
            "-map", "0:v:0", "-map", "1:a:0",
            "-c:v", "libx264", "-pix_fmt", "yuv420p", "-preset", "ultrafast",
            "-tune", "zerolatency",
            "-g", "48", "-keyint_min", "24", "-sc_threshold", "0",
            "-b:v", "2500k", "-maxrate", "3000k", "-bufsize", "5000k",
            "-c:a", "aac", "-b:a", "160k", "-ar", "44100",
            "-shortest",
            "-flvflags", "no_duration_filesize",
            "-f", "flv", rtmp_url
        ]
        
        # Emitir el bloque de noticias sin bloqueos de red
        await stream_clip_async(stream_news_cmd, "Emitiendo Plató en Directo (Kira y Mina hablando)...")
        segment_count += 1

        # =========================================================================
        # FASE 2: BLOQUE MUSICAL CON MITS Y LOS GATOS (2-3 CANCIONES COMPLETAS)
        # =========================================================================
        # Nada más arrancar la música, lanzamos INMEDIATAMENTE en segundo plano
        # la preparación del SIGUIENTE bloque de noticias para que esté 100% cocinado
        # mucho antes de que termine la última canción.
        bg_music_next = playlist[(track_idx + 1) % len(playlist)]
        next_news_task = asyncio.create_task(
            prepare_news_segment(segment_count, bg_music_next, bmax_url, bmax_model)
        )

        for song_num in range(1, songs_per_news + 1):
            current_music = playlist[track_idx % len(playlist)]
            track_name = os.path.basename(current_music).replace(".mp3", "").replace("_", " ")
            track_idx += 1

            print(f"\n[Música {song_num}/{songs_per_news}] 🎵 Reproduciendo: {track_name}")
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
                "-flvflags", "no_duration_filesize",
                "-f", "flv", rtmp_url
            ]
            await stream_clip_async(stream_music_cmd, f"Cámara en la Cabina: Mits y los 4 gatos...")

if __name__ == "__main__":
    try:
        asyncio.run(broadcast_loop())
    except KeyboardInterrupt:
        print("\n[Radio Chipset] Transmisión pausada por el operador.")
