"""
Audio Synthesizer // Calamar-Net Radio (Edge-TTS Neural)
Síntesis dual para Kira y Mina con modulación de pitch/rate
El taller de la gatita tech
"""

import os
import asyncio
import json
import logging
from typing import List, Dict, Any
import edge_tts

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger("AudioSynthesizer")

class AudioSynthesizer:
    def __init__(self, config_path: str = "config.json"):
        with open(config_path, "r", encoding="utf-8") as f:
            self.config = json.load(f)
        
        base_dir = os.path.dirname(os.path.abspath(config_path))
        self.voices_cfg = self.config.get("voices", {})
        cache_rel = self.config.get("radio", {}).get("output_cache_dir", "cache")
        self.cache_dir = cache_rel if os.path.isabs(cache_rel) else os.path.join(base_dir, cache_rel)
        os.makedirs(self.cache_dir, exist_ok=True)

    async def sintetizar_locucion(self, locutor: str, texto: str, archivo_destino: str, lang: str = None) -> str:
        """Sintetiza una sola frase aplicando los parámetros de voz y entonación de Kira o Mina según el idioma."""
        target_lang = lang or self.config.get("radio", {}).get("language", "es")
        key = locutor.lower()
        
        # Buscar en bloque de idioma específico (es/en) o fallback directo
        lang_voices = self.voices_cfg.get(target_lang, {})
        if isinstance(lang_voices, dict) and key in lang_voices:
            v_info = lang_voices[key]
        else:
            v_info = self.voices_cfg.get(key, {})
        
        default_voice = "es-ES-ElviraNeural" if target_lang == "es" else "en-US-AriaNeural"
        voice_id = v_info.get("voice_id", default_voice)
        rate = v_info.get("rate", "+0%")
        pitch = v_info.get("pitch", "+0Hz")

        communicate = edge_tts.Communicate(
            text=texto,
            voice=voice_id,
            rate=rate,
            pitch=pitch
        )
        await communicate.save(archivo_destino)
        logger.info(f"[{locutor.upper()} ({target_lang.upper()})] Audio generado: {archivo_destino} | Voz: {voice_id} ({rate}, {pitch})")
        return archivo_destino

    async def render_dialogue_capsule(self, script_data: Dict[str, Any], capsule_id: str, lang: str = None) -> List[str]:
        """
        Renderiza la secuencia completa de intervenciones entre Kira y Mina
        guardando cada archivo con timestamp correlativo en el directorio de caché.
        """
        dialogues = script_data.get("dialogues", [])
        rendered_files = []
        target_lang = lang or script_data.get("language") or self.config.get("radio", {}).get("language", "es")

        for idx, turn in enumerate(dialogues):
            speaker = turn.get("speaker", "KIRA")
            text = turn.get("text", "")
            out_path = os.path.join(self.cache_dir, f"capsule_{capsule_id}_part{idx}_{speaker.lower()}_{target_lang}.mp3")
            
            await self.sintetizar_locucion(speaker, text, out_path, lang=target_lang)
            rendered_files.append(out_path)

        logger.info(f"Cápsula de locución #{capsule_id} [{target_lang.upper()}] completada ({len(rendered_files)} archivos de audio).")
        return rendered_files

if __name__ == "__main__":
    synthesizer = AudioSynthesizer()
    sample_script = {
        "theme": "Prueba de audio",
        "dialogues": [
            {"speaker": "KIRA", "text": "¡Probando micro en Calamar-Net! ¡Mina, sube el máster que entramos al aire!"},
            {"speaker": "MINA", "text": "Canal uno ecualizado y sin saturación. Bienvenidas a la sintonía del Wired."}
        ]
    }
    archivos = asyncio.run(synthesizer.render_dialogue_capsule(sample_script, "test01"))
    print("Archivos renderizados:", archivos)
