"""
Procesador Automático del Buzón de Discord
El taller de la gatita tech
Escanea los volcados de chat (Markdown, TXT, HTML o JSON) depositados en 'entrantes/',
extrae la inteligencia táctica (enlaces a repositorios, avisos de Meta, cupones, modelos)
y genera el informe consolidado 'TELEMETRIA_Y_NOVEDADES_DISCORD.md'.
"""

import os
import re
import sys
import shutil
import time
from datetime import datetime
from typing import List, Dict, Any

# Asegurar salida UTF-8 limpia en consola Windows sin caídas por cp1252
if sys.platform == "win32":
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")

BASE_DIR = os.path.dirname(os.path.abspath(__file__))
INBOX_DIR = os.path.join(BASE_DIR, "entrantes")
PROCESSED_DIR = os.path.join(BASE_DIR, "procesados")
OUTPUT_MD = os.path.join(os.path.dirname(BASE_DIR), "TELEMETRIA_Y_NOVEDADES_DISCORD.md")

os.makedirs(INBOX_DIR, exist_ok=True)
os.makedirs(PROCESSED_DIR, exist_ok=True)

PATTERNS = {
    "github_repos": re.compile(r'https?://github\.com/[a-zA-Z0-9_\-]+/[a-zA-Z0-9_\-]+', re.IGNORECASE),
    "livestreams": re.compile(r'https?://(?:cerebralvalley\.ai/e/[^\s\)]+|vimeo\.com/[^\s\)]+|youtube\.com/[^\s\)]+)', re.IGNORECASE),
    "model_mentions": re.compile(r'\b(muse[-\s]?spark(?:[-\s]?open)?|muse[-\s]?wonder|muse[-\s]?image|muse[-\s]?voice|sam[-\s]?3\.?1|muse[-\s]?glimmer|llama[-\s]?3\.2)\b', re.IGNORECASE),
    "credits_coupons": re.compile(r'([^.\n]*\b(?:credit|credits|coupon|baseten|modal|fireworks|token|rate limit|tpm)\b[^.\n]*)', re.IGNORECASE),
    "announcements": re.compile(r'([^.\n]*\b(?:workshop|announcement|office hours|prerequisites|deadline|submission)\b[^.\n]*)', re.IGNORECASE),
}

def clean_html(raw_html: str) -> str:
    """Elimina etiquetas HTML básicas si el export viene en formato web."""
    clean = re.sub(r'<[^>]+>', ' ', raw_html)
    return re.sub(r'\s+', ' ', clean)

def procesar_archivo(file_path: str) -> Dict[str, Any]:
    with open(file_path, "r", encoding="utf-8", errors="ignore") as f:
        content = f.read()

    if file_path.lower().endswith(".html"):
        text = clean_html(content)
    else:
        text = content

    results = {
        "file_name": os.path.basename(file_path),
        "date": datetime.now().strftime("%Y-%m-%d %H:%M:%S"),
        "repos": list(set(PATTERNS["github_repos"].findall(text))),
        "streams": list(set(PATTERNS["livestreams"].findall(text))),
        "models": list(set([m.lower() for m in PATTERNS["model_mentions"].findall(text)])),
        "credit_lines": list(set([c.strip() for c in PATTERNS["credits_coupons"].findall(text) if len(c.strip()) > 15]))[:8],
        "announcement_lines": list(set([a.strip() for a in PATTERNS["announcements"].findall(text) if len(a.strip()) > 20]))[:8],
        "raw_sample": text[:800].strip()
    }
    return results

def actualizar_informe_maestro(intel_list: List[Dict[str, Any]]):
    if not intel_list:
        return

    report_exists = os.path.exists(OUTPUT_MD)
    
    with open(OUTPUT_MD, "a", encoding="utf-8") as f:
        if not report_exists:
            f.write("# 📡 TELEMETRÍA Y NOVEDADES FILTRADAS DE DISCORD\n")
            f.write("> **Ecosistema:** El taller de la gatita tech // Meta Hackathon 2026\n")
            f.write("> **Fuente:** Buzón automático de exportaciones de Discord\n\n---\n")

        for item in intel_list:
            f.write(f"\n## 📥 Volcado Procesado: `{item['file_name']}` ({item['date']})\n\n")
            
            if item["repos"]:
                f.write("### 🐙 Repositorios Oficiales Detectados:\n")
                for r in item["repos"]:
                    f.write(f"- [{r}]({r})\n")
                f.write("\n")

            if item["models"]:
                f.write("### 🧠 Modelos Clave Mencionados:\n")
                f.write(f"- `{', '.join(item['models'])}`\n\n")

            if item["streams"]:
                f.write("### 🎥 Enlaces de Streams y Talleres:\n")
                for s in item["streams"]:
                    f.write(f"- [{s}]({s})\n")
                f.write("\n")

            if item["credit_lines"]:
                f.write("### 💰 Novedades sobre Créditos, Tokens y Partners:\n")
                for cl in item["credit_lines"]:
                    f.write(f"- {cl}\n")
                f.write("\n")

            if item["announcement_lines"]:
                f.write("### 📢 Avisos y Requisitos del Hackathon:\n")
                for al in item["announcement_lines"]:
                    f.write(f"- {al}\n")
                f.write("\n")

            f.write("---\n")

def main():
    archivos = [
        os.path.join(INBOX_DIR, f)
        for f in os.listdir(INBOX_DIR)
        if os.path.isfile(os.path.join(INBOX_DIR, f))
    ]

    if not archivos:
        print("[*] Buzón vacío: No hay archivos nuevos en 'buzon_discord/entrantes/'.")
        print("    Suelta ahí tus exports (.md, .txt, .html) de Discord y vuelve a ejecutarme.")
        return

    print(f"[*] Procesando {len(archivos)} archivo(s) del buzón de Discord...")
    intel_acumulada = []

    for af in archivos:
        try:
            intel = procesar_archivo(af)
            intel_acumulada.append(intel)
            
            # Mover a procesados con timestamp para no pisar
            destino = os.path.join(PROCESSED_DIR, f"{int(time.time())}_{os.path.basename(af)}")
            shutil.move(af, destino)
            print(f"  [+] Procesado y archivado: {os.path.basename(af)}")
        except Exception as e:
            print(f"  [!] Error procesando {af}: {e}")

    actualizar_informe_maestro(intel_acumulada)
    print(f"\n[✓] ¡Completado! Novedades añadidas a '{os.path.basename(OUTPUT_MD)}'.")

if __name__ == "__main__":
    main()
