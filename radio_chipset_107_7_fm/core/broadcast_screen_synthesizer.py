"""
OBS Virtual Camera & Scene Linker // Radio Chipset 107.7 FM Radio 24/7
Genera los fotogramas en vivo del estudio con telemetria en tiempo real,
los inyecta en OBS Studio (o Virtual Cam) para emitir a YouTube 24/7,
y sincroniza el stream hacia la Megatorre de Unity 6.
El taller de la gatita tech
"""

import os
import sys
import time
import json
import cv2
import numpy as np
from datetime import datetime

class BroadcastScreenSynthesizer:
    def __init__(self, bg_image_path: str, output_feed_dir: str):
        self.bg_image_path = bg_image_path
        self.output_feed_dir = output_feed_dir
        os.makedirs(output_feed_dir, exist_ok=True)
        self.base_frame = cv2.imread(bg_image_path)
        if self.base_frame is None:
            raise FileNotFoundError(f"No se pudo cargar la imagen de fondo: {bg_image_path}")
        self.height, self.width, _ = self.base_frame.shape

    def render_live_frame(self, current_speaker: str, dialogue_text: str, current_track: str) -> str:
        frame = self.base_frame.copy()
        h, w = self.height, self.width

        # 1. Overlay inferior LainOS estilo Cyberpunk
        cv2.rectangle(frame, (0, h - 110), (w, h), (9, 11, 14), -1)
        # Línea cian neón superior
        cv2.line(frame, (0, h - 110), (w, h - 110), (204, 255, 0), 2)

        # 2. Telemetría de la Emisora
        now_str = datetime.now().strftime("%H:%M:%S")
        cv2.putText(frame, f"[ LIVE ] CALAMAR-NET 107.7 FM | {now_str}", (30, h - 75), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.75, (0, 255, 204), 2)
        cv2.putText(frame, f"NOW PLAYING: {current_track[:35]}", (w - 550, h - 75), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.55, (200, 214, 229), 1)

        # 3. Subtítulo dinámico de la locutora activa (Kira en magenta, Mina en cian)
        speaker_color = (127, 0, 255) if current_speaker.upper() == "KIRA" else (204, 255, 0)
        cv2.putText(frame, f"{current_speaker.upper()}:", (30, h - 35), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.7, speaker_color, 2)
        cv2.putText(frame, f'"{dialogue_text[:75]}"', (150, h - 35), 
                    cv2.FONT_HERSHEY_SIMPLEX, 0.65, (255, 255, 255), 1)

        # Guardar fotograma activo para OBS Image Source o Unity Video Bridge
        live_frame_path = os.path.join(self.output_feed_dir, "live_broadcast_feed.jpg")
        cv2.imwrite(live_frame_path, frame)
        return live_frame_path

if __name__ == "__main__":
    bg = r"c:\Users\asakura\Documents\suno proyect\estudio_gatita_tech_16_9.jpg"
    out = r"c:\Users\asakura\Documents\juego meta\radio_chipset_107_7_fm\cache"
    synth = BroadcastScreenSynthesizer(bg, out)
    path = synth.render_live_frame("KIRA", "¡Subid los faders en el taller que entramos en combate!", "Ronin Code Focus Beats")
    print(f"[+] Fotograma de emisión generado con éxito: {path}")
