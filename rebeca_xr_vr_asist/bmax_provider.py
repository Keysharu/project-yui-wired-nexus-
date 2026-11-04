"""
BMAX AI Provider & Failover Engine
Conexión de Rebeca / Yui XR-VR Assist con el Servidor Homelab BMAX (Llama 3.2 3B)
y failover automático a Google Gemini
El taller de la gatita tech
"""

import os
import json
import logging
import requests
from typing import List, Dict, Any, Optional

logger = logging.getLogger("BmaxProvider")

BMAX_HOST = "192.168.0.29"
BMAX_PORT = 11434
BMAX_MODEL = "llama3.2:3b"
BMAX_TIMEOUT = 8  # segundos antes de conmutar a la nube

def query_bmax_ollama(prompt: str, system_prompt: str = "", history: Optional[List[Dict[str, str]]] = None) -> Optional[str]:
    """
    Envía una petición al motor Ollama en el servidor BMAX (Llama 3.2 3B).
    Devuelve la respuesta en texto plano o None si el servidor no responde.
    """
    messages = []
    if system_prompt:
        messages.append({"role": "system", "content": system_prompt})
        
    if history:
        for msg in history:
            messages.append(msg)
            
    messages.append({"role": "user", "content": prompt})

    endpoint = f"http://{BMAX_HOST}:{BMAX_PORT}/api/chat"
    payload = {
        "model": BMAX_MODEL,
        "messages": messages,
        "stream": False,
        "options": {
            "temperature": 0.7,
            "top_p": 0.9,
            "num_predict": 512
        }
    }

    try:
        res = requests.post(endpoint, json=payload, timeout=BMAX_TIMEOUT)
        res.raise_for_status()
        data = res.json()
        return data.get("message", {}).get("content", "").strip()
    except Exception as e:
        logger.warning(f"BMAX no disponible ({e}). Activando conmutación por error...")
        return None


def query_meta_muse_spark(prompt: str, system_prompt: str = "", history: Optional[List[Dict[str, str]]] = None) -> Optional[str]:
    """
    Envia una peticion al modelo oficial de Meta Muse Spark 1.3 via https://api.meta.ai/v1/responses.
    Devuelve la respuesta generada o None si la API no responde.
    """
    api_key = os.getenv("MODEL_API_KEY") or os.getenv("META_MODEL_API_KEY") or ""
    if not api_key:
        return None

    full_input = []
    if system_prompt:
        full_input.append({"role": "system", "content": system_prompt})
    if history:
        for msg in history:
            full_input.append(msg)
    full_input.append({"role": "user", "content": prompt})

    payload = {
        "model": "muse-spark-1.3",
        "input": full_input,
        "stream": True,
        "temperature": 0.85,
        "max_output_tokens": 4096,
        "top_p": 1,
        "reasoning": {"effort": "low"}
    }

    headers = {
        "Authorization": f"Bearer {api_key}",
        "Content-Type": "application/json",
        "Accept": "text/event-stream"
    }

    try:
        res = requests.post("https://api.meta.ai/v1/responses", headers=headers, json=payload, stream=True, timeout=20)
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
        return collected_text.strip() if collected_text else None
    except Exception as e:
        logger.warning(f"Meta Muse Spark error ({e}).")
        return None

def notify_event_to_radio(category: str, context: str):
    """Notifica un evento a la emisora Radio Chipset 107.7 FM de Kira y Mina."""
    queue_path = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "radio_chipset_107_7_fm", "events_queue.json"))
    try:
        events = []
        if os.path.exists(queue_path):
            with open(queue_path, "r", encoding="utf-8") as f:
                events = json.load(f)
        events.append({
            "id": int(time.time() * 1000),
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "category": category,
            "context": context,
            "author": "rebeca_xr_vr_asist"
        })
        with open(queue_path, "w", encoding="utf-8") as f:
            json.dump(events, f, indent=2, ensure_ascii=False)
    except Exception as e:
        logger.warning(f"No se pudo notificar evento a la radio: {e}")
