"""
Broadcast Streamer // Calamar-Net Radio 24/7
Bucle continuo de emisión con Audio Ducking y orquestación multi-hilo
El taller de la gatita tech
"""

import os
import sys
import time
import json
import asyncio
import logging
import subprocess
from typing import List, Optional

from .script_generator import ScriptGenerator
from .audio_synthesizer import AudioSynthesizer
from .events_bridge import EventsBridge
from .broadcast_screen_synthesizer import BroadcastScreenSynthesizer

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger("BroadcastStreamer")

class BroadcastStreamer:
    def __init__(self, config_path: str = "config.json"):
        with open(config_path, "r", encoding="utf-8") as f:
            self.config = json.load(f)
            
        self.music_dir = self.config.get("radio", {}).get("default_music_dir", "music")
        os.makedirs(self.music_dir, exist_ok=True)
        
        base_dir = os.path.dirname(os.path.abspath(config_path))
        self.script_gen = ScriptGenerator(config_path)
        self.synth = AudioSynthesizer(config_path)
        events_file = os.path.join(base_dir, "events_queue.json")
        self.events = EventsBridge(events_file)
        
        bg_img = r"c:\Users\asakura\Documents\suno proyect\estudio_gatita_tech_16_9.jpg"
        cache_dir = os.path.join(base_dir, self.config.get("radio", {}).get("output_cache_dir", "cache"))
        self.screen_synth = BroadcastScreenSynthesizer(bg_img, cache_dir)
        self.current_track_name = "Ronin Code Focus Beats"
        self.running = False

    def list_music_tracks(self) -> List[str]:
        """Obtiene la lista de archivos de música disponibles para la emisión."""
        valid_ext = (".mp3", ".wav", ".ogg", ".flac", ".mpeg", ".m4a")
        tracks = [
            os.path.join(self.music_dir, f)
            for f in os.listdir(self.music_dir)
            if f.lower().endswith(valid_ext)
        ]
        return sorted(tracks)

    async def run_capsule_cycle(self, cycle_index: int, lang: Optional[str] = None):
        """Ejecuta una intervención completa de Kira y Mina basada en el siguiente evento."""
        target_lang = lang or self.config.get("radio", {}).get("language", "es")
        event = self.events.pop_next_event()
        logger.info(f"--- [CICLO #{cycle_index} - {target_lang.upper()}] Evento seleccionado: {event.get('context')}")

        # 1. Generar guion con Llama 3.2 en BMAX o API
        script_data = self.script_gen.generate_radio_dialogue(
            prompt_context=event.get("context", ""),
            category=event.get("category", "music"),
            lang=target_lang
        )
        if not script_data:
            logger.warning("No se pudo generar guion, saltando cápsula...")
            return

        # 2. Sintetizar las voces duales con Edge-TTS
        capsule_id = f"{int(time.time())}_{cycle_index}"
        audio_files = await self.synth.render_dialogue_capsule(script_data, capsule_id, lang=target_lang)

        # 3. Reproducir o encolar las voces y actualizar el feed visual de la emisión
        logger.info(f"Cápsula #{capsule_id} [{target_lang.upper()}] lista para radiodifusión. Temática: {script_data.get('theme')}")
        dialogues = script_data.get("dialogues", [])
        for idx, f in enumerate(audio_files):
            speaker = dialogues[idx].get("speaker", "KIRA") if idx < len(dialogues) else "KIRA"
            text_line = dialogues[idx].get("text", "") if idx < len(dialogues) else ""
            
            # Generar el fotograma activo con subtítulos y telemetría en tiempo real para OBS / Megatorre
            live_frame = self.screen_synth.render_live_frame(speaker, text_line, self.current_track_name)
            logger.info(f"Emitiendo locución [{speaker}]: {os.path.basename(f)} | Visual Frame: {os.path.basename(live_frame)}")
            # En entorno local se puede reproducir o alimentar a FFmpeg

    async def start_station_loop(self, total_cycles: Optional[int] = None):
        """Inicia el bucle continuo de emisión 24/7."""
        self.running = True
        logger.info("====================================================")
        logger.info("🎙️ RADIO CHIPSET // EL TALLER DE LA GATITA TECH — EN EL AIRE 24/7 🎙️")
        logger.info("   Locutoras: Kira & Mina // Cerebro: BMAX Llama 3.2")
        logger.info("====================================================")

        cycle = 1
        try:
            while self.running:
                await self.run_capsule_cycle(cycle)
                cycle += 1
                if total_cycles and cycle > total_cycles:
                    break
                # Pausa entre intervenciones (por defecto 15-30 segundos para pruebas)
                logger.info("Música sonando en antena... Próxima intervención en 20 segundos.")
                await asyncio.sleep(20)
        except asyncio.CancelledError:
            logger.info("Emisión detenida por el operador.")
        finally:
            self.running = False

if __name__ == "__main__":
    streamer = BroadcastStreamer()
    asyncio.run(streamer.start_station_loop(total_cycles=2))
