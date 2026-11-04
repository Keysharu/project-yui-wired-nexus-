"""
Events Bridge // Radio Chipset Radio & Project Yui
Gestor y cola de eventos del metaverso, misiones, bosses y telemetría de juego
El taller de la gatita tech
"""

import json
import os
import time
import random
from typing import Optional, Dict, Any, List

DEFAULT_EVENTS = [
    {
        "category": "music",
        "context": "Presentación de la nueva pista musical generada en el taller de la gatita tech con estilo cyberpunk synthwave."
    },
    {
        "category": "game_event",
        "context": "¡Alerta en el sector 3! Ha aparecido un World Boss de código corrupto en la plaza central. Se necesitan netrunners de nivel 5 o superior."
    },
    {
        "category": "game_event",
        "context": "La operadora Raquel ha completado con éxito la misión diaria 'Cosecha de Silicio', recolectando 5 microcontroladores ESP32 en el mapa urbano."
    },
    {
        "category": "news",
        "context": "Nueva brecha de seguridad parcheada en el protocolo cuántico de transporte. Mina aconseja reiniciar los firewalls de las gafas Quest."
    },
    {
        "category": "music",
        "context": "Kira quiere poner un tema de rock electrónico pesado, mientras que Mina argumenta que el ambient cyberpunk ayuda a concentrarse mejor al programar."
    },
    {
        "category": "game_event",
        "context": "Yui reporta desde el modo Passthrough de las Quest 3S que ha detectado componentes electrónicos reales en el taller para mejorar los escudos de defensa."
    }
]

class EventsBridge:
    def __init__(self, queue_file: str = "events_queue.json"):
        self.queue_file = queue_file
        self._ensure_queue_file()

    def _ensure_queue_file(self):
        if not os.path.exists(self.queue_file):
            with open(self.queue_file, "w", encoding="utf-8") as f:
                json.dump([], f, indent=2)

    def push_event(self, category: str, context: str, author: str = "sistema"):
        """Inserta un evento en la cola para ser comentado en la radio por Kira y Mina."""
        event = {
            "id": int(time.time() * 1000),
            "timestamp": time.strftime("%Y-%m-%d %H:%M:%S"),
            "category": category,
            "context": context,
            "author": author
        }
        with open(self.queue_file, "r+", encoding="utf-8") as f:
            try:
                events = json.load(f)
            except json.JSONDecodeError:
                events = []
            events.append(event)
            f.seek(0)
            f.truncate()
            json.dump(events, f, indent=2, ensure_ascii=False)
        return event

    def pop_next_event(self) -> Dict[str, Any]:
        """Extrae el siguiente evento prioritario de la cola o devuelve uno temático aleatorio."""
        with open(self.queue_file, "r+", encoding="utf-8") as f:
            try:
                events = json.load(f)
            except json.JSONDecodeError:
                events = []

            if events:
                next_ev = events.pop(0)
                f.seek(0)
                f.truncate()
                json.dump(events, f, indent=2, ensure_ascii=False)
                return next_ev

        # Fallback temático del universo
        return random.choice(DEFAULT_EVENTS)

if __name__ == "__main__":
    bridge = EventsBridge()
    bridge.push_event("game_event", "Prueba de evento: Boss derribado en el sector 7")
    ev = bridge.pop_next_event()
    print("Evento recuperado:", ev)
