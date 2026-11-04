"""
=============================================================================
MÓDULO: GESTOR DE MOCHILA DE DATOS Y MEMORIA PERSISTENTE (LAINOS / CYBERPUNK)
=============================================================================
Responsable de:
- Almacenar, indexar y suministrar archivos de usuario (.md, .txt, .json)
- Inyección de contexto de largo plazo en las transmisiones con Gemini
- Persistencia de historial de conversación entre reinicios
- Telemetría ambiental en tiempo real (Geo-IP + Open-Meteo)
=============================================================================
"""

import os
import sys
import json
import shutil
import urllib.request
from pathlib import Path
from typing import List, Dict, Optional


WEATHER_CODE_MAP = {
    0: "cielo completamente despejado y soleado",
    1: "cielo mayormente despejado con nubes dispersas",
    2: "cielo parcialmente nublado",
    3: "cielo cubierto y encapotado",
    45: "bancos de niebla densa",
    48: "niebla con escarcha",
    51: "llovizna ligera",
    53: "llovizna moderada",
    55: "llovizna intensa",
    61: "lluvia ligera",
    63: "lluvia moderada",
    65: "lluvia fuerte continua",
    71: "nevada ligera",
    73: "nevada moderada",
    75: "nevada copiosa",
    80: "chubascos intermitentes",
    81: "chubascos moderados",
    82: "chubascos violentos",
    95: "tormenta eléctrica",
    96: "tormenta eléctrica con granizo ligero",
    99: "tormenta eléctrica severa con granizo fuerte"
}


def get_live_weather_telemetry(timeout: float = 3.0) -> Dict[str, any]:
    """
    Obtiene la telemetría climática local mediante Geo-IP y Open-Meteo (0 costes, 0 API keys).
    """
    telemetry = {
        "success": False,
        "city": "Desconocida",
        "country": "Desconocido",
        "temp": 0.0,
        "feels_like": 0.0,
        "humidity": 0,
        "wind_kmh": 0.0,
        "condition": "Sin datos",
        "summary": "Telemetría meteorológica no disponible temporalmente."
    }
    try:
        # 1. Resolver geolocalización de red
        ip_req = urllib.request.Request(
            "http://ip-api.com/json",
            headers={"User-Agent": "CyberCompanion/1.0"}
        )
        with urllib.request.urlopen(ip_req, timeout=timeout) as response:
            geo_data = json.loads(response.read().decode("utf-8"))

        city = geo_data.get("city", "Tu ubicación")
        country = geo_data.get("country", "")
        lat = geo_data.get("lat")
        lon = geo_data.get("lon")

        if lat is None or lon is None:
            return telemetry

        # 2. Consultar satélite meteorológico de Open-Meteo
        weather_url = (
            f"https://api.open-meteo.com/v1/forecast?"
            f"latitude={lat}&longitude={lon}&"
            f"current=temperature_2m,relative_humidity_2m,apparent_temperature,weather_code,wind_speed_10m"
        )
        meteo_req = urllib.request.Request(
            weather_url,
            headers={"User-Agent": "CyberCompanion/1.0"}
        )
        with urllib.request.urlopen(meteo_req, timeout=timeout) as meteo_resp:
            meteo_data = json.loads(meteo_resp.read().decode("utf-8"))

        current = meteo_data.get("current", {})
        temp = current.get("temperature_2m", 0.0)
        feels_like = current.get("apparent_temperature", temp)
        humidity = current.get("relative_humidity_2m", 0)
        wind_kmh = current.get("wind_speed_10m", 0.0)
        w_code = current.get("weather_code", 0)
        condition = WEATHER_CODE_MAP.get(w_code, "condiciones estables")

        summary = (
            f"En {city} ({country}), tenemos actualmente {temp} °C "
            f"(sensación térmica de {feels_like} °C), humedad del {humidity}%, "
            f"viento a {wind_kmh} km/h y {condition}."
        )

        telemetry.update({
            "success": True,
            "city": city,
            "country": country,
            "temp": temp,
            "feels_like": feels_like,
            "humidity": humidity,
            "wind_kmh": wind_kmh,
            "condition": condition,
            "summary": summary
        })
        return telemetry

    except Exception as e:
        print(f"[!] Aviso Telemetría Clima: {e}")
        return telemetry


class BackpackManager:
    """
    Gestor universal de mochilas de contexto, archivos de usuario y memoria episódica.
    """
    def __init__(self, base_dir: Path):
        self.base_dir = Path(base_dir)
        self.backpack_dir = self.base_dir / "backpack"
        self.history_file = self.base_dir / "chat_history.json"

        # Asegurar directorio de mochila
        self.backpack_dir.mkdir(parents=True, exist_ok=True)

        # Si la mochila está vacía, genera una plantilla de bienvenida para el usuario final
        self._ensure_welcome_template()

    def _ensure_welcome_template(self):
        """Si la mochila está vacía, crea una plantilla guía para que el usuario final agregue sus datos."""
        try:
            current_files = list(self.backpack_dir.glob("*.*"))
            if not current_files:
                template_path = self.backpack_dir / "ejemplo_contexto.md"
                template_content = (
                    "# MI CONTEXTO PERSONAL (PLANTILLA PARA REBECA)\n"
                    "- Nombre del usuario: [Escribe aquí tu nombre o alias]\n"
                    "- Profesión / Estudios: [A qué te dedicas o qué estás aprendiendo]\n"
                    "- Mis proyectos actuales: [Describe qué proyectos estás desarrollando]\n"
                    "- Mis temas de interés y hobbies: [Tecnología, videojuegos, libros, anime...]\n"
                    "- Mascotas o notas personales: [Cualquier detalle que quieras que Rebeca recuerde]\n\n"
                    "> Puedes editar este archivo o añadir tus propios documentos (.md, .txt, .json)\n"
                    "> pulsando el botón 🎒 en la burbuja de Rebeca o usando 'Abrir Carpeta'.\n"
                )
                with open(template_path, "w", encoding="utf-8") as f:
                    f.write(template_content)
        except Exception as e:
            print(f"[!] Error generando plantilla inicial de mochila: {e}")

    def get_backpack_files(self) -> List[Dict[str, any]]:
        """Devuelve la lista de archivos indexados en la mochila."""
        results = []
        try:
            allowed_exts = {".md", ".txt", ".json", ".csv", ".yaml", ".yml"}
            for p in sorted(self.backpack_dir.iterdir()):
                if p.is_file() and p.suffix.lower() in allowed_exts:
                    stat = p.stat()
                    results.append({
                        "name": p.name,
                        "path": str(p),
                        "size_bytes": stat.st_size,
                        "size_kb": round(stat.st_size / 1024, 1),
                        "modified": stat.st_mtime
                    })
        except Exception as e:
            print(f"[!] Error listando mochila: {e}")
        return results

    def add_files(self, file_paths: List[str]) -> int:
        """Copia archivos externos a la carpeta de la mochila."""
        count = 0
        for fpath in file_paths:
            src = Path(fpath)
            if src.exists() and src.is_file():
                try:
                    dst = self.backpack_dir / src.name
                    shutil.copy2(src, dst)
                    count += 1
                except Exception as e:
                    print(f"[!] Error copiando archivo {src}: {e}")
        return count

    def remove_file(self, filename: str) -> bool:
        """Elimina un archivo de la mochila."""
        try:
            target = self.backpack_dir / filename
            if target.exists() and target.is_file():
                target.unlink()
                return True
        except Exception as e:
            print(f"[!] Error eliminando {filename}: {e}")
        return False

    def open_folder(self):
        """Abre la carpeta de la mochila en el Explorador de Windows."""
        try:
            os.startfile(str(self.backpack_dir))
        except Exception as e:
            print(f"[!] Error abriendo carpeta mochila: {e}")

    def build_context_backpack(self, max_chars_per_file: int = 2500, total_budget: int = 7000) -> str:
        """
        Construye el bloque inyectable de contexto de la mochila para Gemini.
        Controla el presupuesto de tokens para evitar saturación.
        """
        files = self.get_backpack_files()
        if not files:
            return ""

        blocks = []
        accumulated_chars = 0

        for f_info in files:
            if accumulated_chars >= total_budget:
                break
            try:
                p = Path(f_info["path"])
                with open(p, "r", encoding="utf-8", errors="replace") as f:
                    content = f.read().strip()

                if not content:
                    continue

                if len(content) > max_chars_per_file:
                    content = content[:max_chars_per_file] + "\n...[Contenido truncado por presupuesto]..."

                block = f"--- [DOCUMENTO MOCHILA: {f_info['name']}] ---\n{content}\n"
                blocks.append(block)
                accumulated_chars += len(block)
            except Exception as e:
                print(f"[!] Error leyendo documento mochila {f_info['name']}: {e}")

        if not blocks:
            return ""

        header = (
            "\n[MOCHILA DE CONTEXTO Y PERFIL DEL OPERADOR]\n"
            "A continuación tienes información personal, técnica, profesional y de proyectos "
            "suministrada directamente por tu Operador. Úsala para responder con total personalización, "
            "complicidad y memoria de largo plazo:\n"
        )
        footer = "\n[FIN DE MOCHILA DE CONTEXTO]\n"
        return header + "\n".join(blocks) + footer

    def get_chat_history(self, limit: int = 6) -> List[Dict[str, str]]:
        """Carga el historial reciente de conversación."""
        if not self.history_file.exists():
            return []
        try:
            with open(self.history_file, "r", encoding="utf-8") as f:
                history = json.load(f)
                if isinstance(history, list):
                    return history[-limit:]
        except Exception as e:
            print(f"[!] Error leyendo chat_history.json: {e}")
        return []

    def save_turn(self, user_msg: str, rebeca_msg: str, max_history: int = 10):
        """Guarda un turno de conversación (Operador + Rebeca) en disco."""
        history = []
        if self.history_file.exists():
            try:
                with open(self.history_file, "r", encoding="utf-8") as f:
                    data = json.load(f)
                    if isinstance(data, list):
                        history = data
            except Exception:
                history = []

        history.append({
            "user": user_msg.strip(),
            "rebeca": rebeca_msg.strip()
        })
        history = history[-max_history:]

        try:
            with open(self.history_file, "w", encoding="utf-8") as f:
                json.dump(history, f, indent=2, ensure_ascii=False)
        except Exception as e:
            print(f"[!] Error guardando historial: {e}")

    def clear_chat_history(self):
        """Borra la memoria de conversación reciente."""
        try:
            with open(self.history_file, "w", encoding="utf-8") as f:
                json.dump([], f)
        except Exception as e:
            print(f"[!] Error purgando historial: {e}")

    def get_telemetry_stats(self) -> Dict[str, any]:
        """Telemetría para la interfaz de la mochila."""
        files = self.get_backpack_files()
        total_kb = sum(f["size_kb"] for f in files)
        history_turns = len(self.get_chat_history(limit=50))
        return {
            "file_count": len(files),
            "total_kb": round(total_kb, 1),
            "history_turns": history_turns
        }
