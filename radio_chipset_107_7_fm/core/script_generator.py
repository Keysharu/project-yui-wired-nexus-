"""
Script Generator // Radio Chipset Radio (Kira & Mina)
Arquitectura Multi-Proveedor:
- BMAX Local (Ollama Llama 3.2 3B) -> Coste 0,00 €
- Meta Model API (Muse Spark / Muse Wonder) -> Créditos Meta
- Baseten (Muse Spark Open) -> Créditos Baseten
El taller de la gatita tech
"""

import os
import json
import logging
import requests
from typing import List, Dict, Any, Optional
from dotenv import load_dotenv

# Cargar variables de entorno desde la raiz o local
base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
root_env = os.path.abspath(os.path.join(base_dir, '..', '.env'))
local_env = os.path.join(base_dir, '.env')
if os.path.exists(root_env):
    load_dotenv(root_env)
if os.path.exists(local_env):
    load_dotenv(local_env)

logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger("ScriptGenerator")

SYSTEM_PROMPT_RADIO_ES = """
Eres el guionista principal de 'Radio Chipset', la emisora de trinchera cyberpunk y música lo-fi del metaverso en El taller de la gatita tech.
Diriges a dos locutoras estrella con química inigualable, al estilo de Los 40 Principales o Europa FM cruzado con el dúo de Splatoon (Callie y Marie / Pearl y Marina):

1. **KIRA**: Rápida, enérgica, netrunner de trinchera, apasionada de los sintetizadores pesados, el combate físico y la acción. A veces se precipita y suelta comentarios espontáneos.
2. **MINA**: Elegante, calmada, analítica y con un toque de sarcasmo fino. Es la estratega técnica, experta en arquitectura de datos, hardware y lore del Wired. Corrige a Kira con cariño cuando se pasa de frenada.

REGLAS DE ORO DEL DIÁLOGO:
- Son íntimas amigas. Se pican, bromean y se lanzan réplicas rápidas, pero hay un respeto y complicidad total entre ellas.
- El diálogo debe ser natural, fresco, vibrante, SIN clichés artificiales ni lenguaje corporativo acartonado.
- Deben alternarse en intervenciones breves y dinámicas (entre 3 y 5 turnos en total).
- Formato de salida ESTRICTO en JSON para procesamiento del sintetizador de voz.

ESTRUCTURA DEL JSON REQUERIDA:
{
  "theme": "breve descripción del tema",
  "language": "es",
  "dialogues": [
    {"speaker": "KIRA", "text": "Frase enérgica de Kira."},
    {"speaker": "MINA", "text": "Respuesta irónica o analítica de Mina."},
    {"speaker": "KIRA", "text": "Segunda réplica rápida de Kira."},
    {"speaker": "MINA", "text": "Cierre o pase de música por parte de Mina."}
  ]
}
"""

SYSTEM_PROMPT_RADIO_EN = """
You are the head writer of 'Radio Chipset', the premier cyberpunk lo-fi and chiptune radio station in the metaverse at El taller de la gatita tech.
You direct two star AI co-hosts with unmatched chemistry (think Jet Set Radio meets Splatoon's Pearl & Marina, with an authentic cyberpunk edge):

1. **KIRA**: High-energy combat netrunner, fast talker, loves crushing heavy synthwave, glitch art, and physical VR combat. She's impulsive, funny, and drops passionate, punchy remarks.
2. **MINA**: Refined, analytical, calm, with dry sarcastic wit. The tactical cybersecurity expert, data architect, and lore keeper. She keeps Kira grounded while playfully teasing her.

GOLDEN DIALOGUE RULES:
- They are best friends. They tease each other, banter at lightning speed, but have absolute mutual respect.
- Dialogue must be natural, fresh, vibrant, fast-paced, ZERO corporate artificial cliches.
- Fast alternating turns (3 to 5 total lines).
- STRICT JSON output format.

REQUIRED JSON STRUCTURE:
{
  "theme": "brief topic description",
  "language": "en",
  "dialogues": [
    {"speaker": "KIRA", "text": "High energy opener from Kira in English."},
    {"speaker": "MINA", "text": "Calm, witty or analytical response from Mina in English."},
    {"speaker": "KIRA", "text": "Quick punchy counter from Kira."},
    {"speaker": "MINA", "text": "Smooth music drop or closing transition from Mina."}
  ]
}
"""

class ScriptGenerator:
    def __init__(self, config_path: str = "config.json"):
        if not os.path.isabs(config_path):
            base_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
            config_path = os.path.join(base_dir, config_path)
            
        with open(config_path, "r", encoding="utf-8") as f:
            self.config = json.load(f)
        
        self.active_provider = self.config.get("active_provider", "bmax_local")
        self.providers = self.config.get("providers", {})
        self.timeout = self.config.get("radio", {}).get("api_timeout_seconds", 30)

    def generate_radio_dialogue(self, prompt_context: str, category: str = "music", lang: str = None) -> Optional[Dict[str, Any]]:
        """
        Genera una cápsula de radio entre Kira y Mina utilizando el proveedor activo
        (Muse Spark / Baseten / BMAX Local) en el idioma seleccionado ('es' o 'en').
        """
        target_lang = lang or self.config.get("radio", {}).get("language", "es")
        system_prompt = SYSTEM_PROMPT_RADIO_EN if target_lang == "en" else SYSTEM_PROMPT_RADIO_ES

        if target_lang == "en":
            user_prompt = f"""
CATEGORY: {category.upper()}
TOPIC OR IN-GAME EVENT TO DISCUSS:
{prompt_context}

Generate the conversation in strict JSON format with alternating lines between KIRA and MINA in English. Return ONLY valid JSON.
"""
        else:
            user_prompt = f"""
CATEGORÍA: {category.upper()}
CONTEXTO O EVENTO A COMENTAR:
{prompt_context}

Genera la conversación en formato JSON con la alternancia entre KIRA y MINA en español. Recuerda devolver ÚNICAMENTE el bloque JSON válido.
"""
        provider_cfg = self.providers.get(self.active_provider, self.providers.get("bmax_local", {}))
        prov_type = provider_cfg.get("type", "ollama")

        logger.info(f"Generando guion [{target_lang.upper()}] con proveedor '{self.active_provider}' (tipo: {prov_type})...")

        try:
            if prov_type == "meta_model_api":
                return self._call_meta_muse_api(provider_cfg, user_prompt, system_prompt=system_prompt)
            elif prov_type == "baseten_endpoint":
                return self._call_baseten_muse(provider_cfg, user_prompt, system_prompt=system_prompt)
            else:
                return self._call_bmax_ollama(provider_cfg, user_prompt, system_prompt=system_prompt)
        except Exception as e:
            logger.warning(f"Error con proveedor '{self.active_provider}': {e}. Saltando a fallback...")
            # Fallback automático a BMAX local si falló un servicio externo
            if self.active_provider != "bmax_local":
                try:
                    logger.info("Intentando fallback inmediato con BMAX local (Llama 3.2)...")
                    return self._call_bmax_ollama(self.providers.get("bmax_local", {}), user_prompt, system_prompt=system_prompt)
                except Exception as fb_err:
                    logger.error(f"Fallback BMAX también falló: {fb_err}")
            return self._generate_fallback(prompt_context, category, lang=target_lang)

    def _clean_reasoning_and_json(self, raw_text: str) -> Dict[str, Any]:
        """Limpia trazas de razonamiento (<think>...</think>) y extrae JSON válido usando JSONDecoder."""
        import re
        cleaned = re.sub(r"<think>.*?</think>", "", raw_text, flags=re.DOTALL).strip()
        
        # Intentar extraer directamente con JSONDecoder desde el primer '{'
        start = cleaned.find("{")
        if start != -1:
            try:
                decoder = json.JSONDecoder()
                obj, _ = decoder.raw_decode(cleaned[start:])
                return obj
            except Exception:
                pass

        # Fallback buscando el último '}'
        start = cleaned.find("{")
        end = cleaned.rfind("}")
        if start != -1 and end != -1:
            return json.loads(cleaned[start:end+1])

        return json.loads(cleaned)

    def _call_bmax_ollama(self, cfg: Dict[str, Any], prompt: str, system_prompt: str = SYSTEM_PROMPT_RADIO_ES) -> Dict[str, Any]:
        """Inferencia local con Ollama en el BMAX."""
        endpoint = cfg.get("endpoint", "http://192.168.0.29:11434/api/chat")
        model = cfg.get("model", "llama3.2:3b")
        payload = {
            "model": model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": prompt}
            ],
            "stream": False,
            "format": "json",
            "options": {"temperature": 0.85, "top_p": 0.9, "num_predict": 450}
        }
        res = requests.post(endpoint, json=payload, timeout=self.timeout)
        res.raise_for_status()
        content = res.json().get("message", {}).get("content", "")
        return self._clean_reasoning_and_json(content)

    def _call_meta_muse_api(self, cfg: Dict[str, Any], prompt: str, system_prompt: str = SYSTEM_PROMPT_RADIO_ES) -> Dict[str, Any]:
        """Meta Model API oficial (Muse Spark 1.3) via https://api.meta.ai/v1/responses."""
        api_key = os.getenv("MODEL_API_KEY") or os.getenv("META_MODEL_API_KEY") or ""
        if not api_key:
            raise ValueError("No se ha encontrado MODEL_API_KEY ni META_MODEL_API_KEY en variables de entorno / .env.")

        endpoint = cfg.get("endpoint", "https://api.meta.ai/v1/responses")
        model = cfg.get("model", "muse-spark-1.3")
        headers = {
            "Authorization": f"Bearer {api_key}",
            "Content-Type": "application/json",
            "Accept": "text/event-stream"
        }

        full_input = []
        if system_prompt:
            full_input.append({"role": "system", "content": system_prompt})
        full_input.append({"role": "user", "content": prompt})

        payload = {
            "model": model,
            "input": full_input,
            "stream": True,
            "temperature": 0.85,
            "max_output_tokens": 4096,
            "top_p": 1,
            "reasoning": {"effort": "low"}
        }

        res = requests.post(endpoint, headers=headers, json=payload, stream=True, timeout=self.timeout)
        res.raise_for_status()

        collected_text = ""
        for line in res.iter_lines():
            if line:
                decoded = line.decode("utf-8")
                if decoded.startswith("data: "):
                    d_str = decoded[6:].strip()
                    if d_str == "[DONE]":
                        break
                    try:
                        data = json.loads(d_str)
                        item = data.get("item", {})
                        if item.get("type") == "message":
                            for part in item.get("content", []):
                                if "text" in part:
                                    collected_text += part["text"]
                        elif "delta" in data and isinstance(data["delta"], str):
                            collected_text += data["delta"]
                    except Exception:
                        pass

        if not collected_text:
            raise ValueError("Meta Muse Spark no devolvio contenido de texto en el stream.")

        return self._clean_reasoning_and_json(collected_text)

    def _call_baseten_muse(self, cfg: Dict[str, Any], prompt: str, system_prompt: str = SYSTEM_PROMPT_RADIO_ES) -> Dict[str, Any]:
        """Inferencia oficial del Hackathon vía Baseten (Muse Spark Open con 2M TPM)."""
        api_key = os.getenv(cfg.get("api_key_env_var", "BASETEN_API_KEY"), "")
        if not api_key:
            raise ValueError(f"Falta la variable de entorno {cfg.get('api_key_env_var')} para Baseten.")

        endpoint = cfg.get("endpoint", "https://inference.baseten.co/v1/chat/completions")
        model = cfg.get("model", "meta-models/muse-spark-open-1026")
        headers = {
            "Authorization": f"Bearer {api_key}",
            "Content-Type": "application/json"
        }
        payload = {
            "model": model,
            "messages": [
                {"role": "system", "content": system_prompt},
                {"role": "user", "content": prompt}
            ],
            "temperature": 0.85
        }
        res = requests.post(endpoint, headers=headers, json=payload, timeout=self.timeout)
        res.raise_for_status()
        content = res.json().get("choices", [{}])[0].get("message", {}).get("content", "")
        return self._clean_reasoning_and_json(content)

    def _generate_fallback(self, prompt_context: str, category: str, lang: str = "es") -> Dict[str, Any]:
        """Cápsula determinista de seguridad sin red ni dependencias (bilingüe)."""
        if lang == "en":
            if category == "game_event":
                return {
                    "theme": "Wired Security Alert",
                    "language": "en",
                    "dialogues": [
                        {"speaker": "KIRA", "text": "Heads up, Wired netrunners! Massive energy spike detected across district sectors!"},
                        {"speaker": "MINA", "text": "Keep your pulse steady, Kira. Telemetry confirms our operators are just calibrating solid-light shields."},
                        {"speaker": "KIRA", "text": "Well, smells like pure incoming combat to me! Ready your holographic blasters, folks!"},
                        {"speaker": "MINA", "text": "Meanwhile, let's keep the vibe immaculate on Radio Chipset."}
                    ]
                }
            return {
                "theme": "Music Break & Focus",
                "language": "en",
                "dialogues": [
                    {"speaker": "KIRA", "text": "Now that was a certified banger! If that synth drop didn't spike your adrenaline, check your cyberware."},
                    {"speaker": "MINA", "text": "Thankfully our audio filters caught the harmonic distortion before melting the studio monitors."},
                    {"speaker": "KIRA", "text": "You can never have too much voltage, Mina! Keep hacking, keep building, and stay locked in."},
                    {"speaker": "MINA", "text": "Queueing the next track. Live from the heart of the metaverse, this is Radio Chipset."}
                ]
            }
        
        if category == "game_event":
            return {
                "theme": "Alerta de red en el Wired",
                "language": "es",
                "dialogues": [
                    {"speaker": "KIRA", "text": "¡Atención netrunners del Wired! Se detecta una fluctuación masiva en los nodos de la ciudad."},
                    {"speaker": "MINA", "text": "Tranquiliza el pulso, Kira. La telemetría indica que son solo operadoras calibrando nuevos escudos cuánticos."},
                    {"speaker": "KIRA", "text": "¡Pues a mí me huele a combate inminente! Id preparando vuestras armas de energía."},
                    {"speaker": "MINA", "text": "Y mientras tanto, sigamos con la mejor selección musical en Radio Chipset."}
                ]
            }
        return {
            "theme": "Transición de fórmula musical",
            "language": "es",
            "dialogues": [
                {"speaker": "KIRA", "text": "¡Vaya temazo acabamos de escuchar! Si esto no os sube la adrenalina en el taller, nada lo hará."},
                {"speaker": "MINA", "text": "Menos mal que tenemos un buen sintetizador modulando las frecuencias antes de que nos revienten los altavoces."},
                {"speaker": "KIRA", "text": "¡La potencia nunca sobra, Mina! Dadle caña a vuestros proyectos y no os desconectéis."},
                {"speaker": "MINA", "text": "Dentro música. Radio Chipset en riguroso directo para todo el metaverso."}
            ]
        }
