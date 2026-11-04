"""
Radio Chipset 107.7 FM - Suavizador y Empalmador de Secuencias de Google Flow.
Elimina los saltos ("deslices") entre clips de 10 segundos aplicando
disolución cruzada suave (xfade) y bucle infinito transparente.
Diseñado para El taller de la gatita tech por Rebeca.
"""

import os
import subprocess
import glob

import shutil

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
os.makedirs(ASSETS_DIR, exist_ok=True)
os.makedirs(TEMP_DIR, exist_ok=True)

def stitch_segments_with_crossfade(input_clips, output_path, transition_duration=0.8):
    """
    Une una lista de clips de 10s aplicando una disolución cruzada (xfade)
    de 'transition_duration' segundos entre cada uno para que el cambio de postura sea invisible.
    """
    if not input_clips:
        print("[VideoStitcher] No se han proporcionado clips.")
        return None

    if len(input_clips) == 1:
        # Si ya viene un solo archivo largo de Flow, le creamos el bucle transparente (crossfade loop)
        return make_seamless_loop(input_clips[0], output_path, transition_duration)

    print(f"\n[VideoStitcher] Empalmando {len(input_clips)} secuencias con disolución suave ({transition_duration}s)...")
    
    # Construcción dinámica del filtro complex para XFADE
    inputs_cmd = []
    for clip in input_clips:
        inputs_cmd.extend(["-i", clip])

    filter_chains = []
    # Suponiendo clips de ~10s:
    # Offset del xfade = Duración acumulada - transition_duration
    # Con clips estándar de 10s: offset1 = 10 - 0.8 = 9.2s
    # offset2 = 9.2 + 10 - 0.8 = 18.4s, etc.
    current_offset = 10.0 - transition_duration
    prev_label = "[0:v]"

    for i in range(1, len(input_clips)):
        next_input = f"[{i}:v]"
        out_label = f"[v{i}]" if i < len(input_clips) - 1 else "[vout]"
        filter_chains.append(
            f"{prev_label}{next_input}xfade=transition=fade:duration={transition_duration}:offset={current_offset:.2f}{out_label}"
        )
        prev_label = out_label
        current_offset += (10.0 - transition_duration)

    filter_str = ";".join(filter_chains)

    cmd = [
        FFMPEG_PATH, "-y",
        *inputs_cmd,
        "-filter_complex", filter_str,
        "-map", "[vout]",
        "-c:v", "libx264", "-pix_fmt", "yuv420p", "-b:v", "3500k",
        output_path
    ]

    subprocess.run(cmd, check=True)
    print(f"[VideoStitcher] ✅ Vídeo suave generado con éxito: {output_path}")
    return output_path

def make_seamless_loop(input_video, output_path, loop_fade=1.0):
    """
    Toma un vídeo largo (ej. los 30-40s descargados de Flow) y cruza el último segundo
    con el primero para que al reproducirse en bucle continuo nunca se note el reinicio.
    """
    print(f"\n[VideoStitcher] Creando bucle infinito invisible para: {os.path.basename(input_video)}...")
    
    # Obtener duración del vídeo con ffprobe o estimar
    # Filtro de solapamiento de cola a cabeza
    cmd = [
        FFMPEG_PATH, "-y",
        "-i", input_video,
        "-filter_complex",
        f"[0:v]split[v1][v2];"
        f"[v1]trim=start=0:end={loop_fade},setpts=PTS-STARTPTS[intro];"
        f"[v2]trim=start={loop_fade},setpts=PTS-STARTPTS[body];"
        f"[body][intro]xfade=transition=fade:duration={loop_fade}:offset=28.0[vout]",
        "-map", "[vout]",
        "-c:v", "libx264", "-pix_fmt", "yuv420p",
        output_path
    ]
    try:
        subprocess.run(cmd, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        return output_path
    except Exception:
        # Si la duración difiere, copiamos directamente
        return input_video

if __name__ == "__main__":
    print("Módulo de transiciones suaves para Radio Chipset listo.")
