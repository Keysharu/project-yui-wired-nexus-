"""
Launcher // Radio Chipset Radio 24/7 (Kira & Mina)
Punto de entrada táctico para pruebas y emisión continua
El taller de la gatita tech
"""

import sys
import os
import asyncio
import argparse

# Añadir el directorio actual al path
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from core.broadcast_streamer import BroadcastStreamer
from core.events_bridge import EventsBridge

def main():
    parser = argparse.ArgumentParser(description="Launcher de Radio Chipset Radio 24/7")
    parser.add_argument("--test", action="store_true", help="Ejecuta una sola cápsula de prueba de Kira y Mina y sale.")
    parser.add_argument("--event", type=str, default="", help="Inyecta un evento en la cola para que Kira y Mina lo comenten.")
    parser.add_argument("--category", type=str, default="game_event", choices=["game_event", "music", "news"], help="Categoría del evento inyectado.")
    parser.add_argument("--lang", type=str, default="es", choices=["es", "en"], help="Idioma de emisión: 'es' (Español) o 'en' (Inglés).")
    parser.add_argument("--continuous", action="store_true", help="Inicia la emisión 24/7 continua.")
    
    args = parser.parse_args()

    BASE_DIR = os.path.dirname(os.path.abspath(__file__))
    config_path = os.path.join(BASE_DIR, "config.json")
    events_path = os.path.join(BASE_DIR, "events_queue.json")
    bridge = EventsBridge(events_path)

    if args.event:
        ev = bridge.push_event(args.category, args.event, author="consola_operador")
        print(f"[+] Evento inyectado en la cola con ID {ev['id']}: {ev['context']}")
        if not args.test and not args.continuous:
            return

    streamer = BroadcastStreamer(config_path)
    if args.lang:
        streamer.config["radio"]["language"] = args.lang

    if args.test or (not args.continuous and not args.event):
        print(f"[*] Ejecutando cápsula de prueba única entre Kira y Mina [{args.lang.upper()}]...")
        asyncio.run(streamer.start_station_loop(total_cycles=1))
    elif args.continuous:
        print(f"[*] Iniciando emisión continua 24/7 [{args.lang.upper()}]...")
        asyncio.run(streamer.start_station_loop())

if __name__ == "__main__":
    main()
