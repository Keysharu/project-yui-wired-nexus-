#!/bin/bash
# Radio Chipset 107.7 FM - Lanzador Autónomo para BMAX / LGM-os
cd "$(dirname "$0")"

echo "======================================================================"
echo "   RADIO CHIPSET 107.7 FM // EMISORA 24/7 EN DIRECTO (BMAX SERVER)"
echo "   El taller de la gatita tech - Modo Plató + Noticias + Debate IA"
echo "======================================================================"
echo ""

# Comprobar si existe python3
if ! command -v python3 &> /dev/null; then
    echo "⚠️ Error: python3 no está instalado en este sistema."
    exit 1
fi

# Comprobar si existe edge-tts
if ! python3 -c "import edge_tts" &> /dev/null; then
    echo "⚠️ Instalando dependencias necesarias (edge-tts, pillow)..."
    pip3 install edge-tts pillow --break-system-packages 2>/dev/null || pip3 install edge-tts pillow
fi

echo "🚀 Iniciando bucle de transmisión continua..."
python3 live_broadcaster.py
