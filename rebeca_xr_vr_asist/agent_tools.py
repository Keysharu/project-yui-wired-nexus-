"""
=============================================================================
MÓDULO DE CAPACIDADES AGÉNTICAS: AGENT_TOOLS
=============================================================================
Permite a la Cyber-Companion ejecutar acciones de sistema en el ordenador:
1. launch_program: Iniciar aplicaciones y programas instalados.
2. open_url_or_search: Abrir páginas web o realizar búsquedas en el navegador.
3. add_calendar_event: Crear y programar eventos estándar (.ics) en el calendario.
4. create_quick_note: Escribir y guardar notas rápidas en el sistema.
5. copy_to_clipboard: Copiar fragmentos o textos generados al portapapeles.
6. get_system_status: Consultar fecha, hora, batería y telemetría de Windows.

ARQUITECTURA DESACOPLADA (PLATFORM ADAPTER):
- Diseñado para ejecutarse en Windows hoy y traducir llamadas a Android Intents
  cuando se compile la versión móvil APK en el futuro.
=============================================================================
"""

import sys
import os
import json
import re
import uuid
import ctypes
import datetime
import subprocess
import webbrowser
import urllib.parse
import shutil
from pathlib import Path
from typing import Optional, Callable

# Gancho de notificación global para feedback visual en la GUI
_action_notifier: Optional[Callable[[str], None]] = None
_file_destroy_callback: Optional[Callable[[str, int, int], None]] = None
_screen_capture_callback: Optional[Callable[[], bytes]] = None
_stealth_mode_callback: Optional[Callable[[], None]] = None
_gemini_api_key: str = ""

def set_action_notifier(callback: Optional[Callable[[str], None]]):
    """Registra una función callback para notificar a la GUI sobre acciones ejecutadas."""
    global _action_notifier
    _action_notifier = callback

def set_file_destroy_callback(callback: Optional[Callable[[str, int, int], None]]):
    """Registra una función callback para iniciar la cinemática de caminata y destrucción física."""
    global _file_destroy_callback
    _file_destroy_callback = callback

def set_screen_capture_callback(callback: Optional[Callable[[], bytes]]):
    """Registra una función callback para capturar el búfer de pantalla en bytes JPEG de forma segura."""
    global _screen_capture_callback
    _screen_capture_callback = callback

def set_stealth_mode_callback(callback: Optional[Callable[[], None]]):
    """Registra una función callback para transicionar la GUI al estado de camuflaje durmiente (Sleep)."""
    global _stealth_mode_callback
    _stealth_mode_callback = callback

def set_gemini_api_key(api_key: str):
    """Registra la clave de API de Gemini para el subsistema de visión multimodal."""
    global _gemini_api_key
    _gemini_api_key = api_key

def _get_api_key() -> str:
    """Recupera la clave de API de Gemini desde memoria, entorno o config.json."""
    global _gemini_api_key
    if _gemini_api_key:
        return _gemini_api_key
    env_key = os.environ.get("GEMINI_API_KEY", "")
    if env_key:
        return env_key
    base_dir = Path(__file__).resolve().parent
    cfg_path = base_dir / "config.json"
    if cfg_path.exists():
        try:
            with open(cfg_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                return data.get("gemini_api_key", "")
        except Exception:
            pass
    return ""

def _notify(message: str):
    """Emite una notificación táctica hacia la interfaz de usuario si hay un callback activo."""
    if _action_notifier:
        try:
            _action_notifier(message)
        except Exception:
            pass


# Mapa de aplicaciones y utilidades comunes en Windows
COMMON_WINDOWS_APPS = {
    # Herramientas de texto y notas
    "bloc de notas": "notepad.exe",
    "notepad": "notepad.exe",
    "notas": "notepad.exe",
    "bloc": "notepad.exe",
    
    # Utilidades
    "calculadora": "calc.exe",
    "calc": "calc.exe",
    "explorador de archivos": "explorer.exe",
    "archivos": "explorer.exe",
    "carpetas": "explorer.exe",
    "paint": "mspaint.exe",
    "dibujo": "mspaint.exe",
    "administrador de tareas": "taskmgr.exe",
    "taskmgr": "taskmgr.exe",
    "panel de control": "control.exe",
    
    # Configuración de Windows (Protocolo URI)
    "configuracion": "ms-settings:",
    "ajustes": "ms-settings:",
    "opciones": "ms-settings:",
    
    # Navegadores específicos
    "chrome": "chrome",
    "google chrome": "chrome",
    "edge": "msedge",
    "microsoft edge": "msedge",
    "firefox": "firefox",
    "brave": "brave",
    "opera": "opera",
    
    # Multimedia y Desarrollo
    "spotify": "spotify:",
    "musica": "spotify:",
    "vscode": "code",
    "visual studio code": "code",
    "terminal": "wt.exe",
    "powershell": "powershell.exe",
    "cmd": "cmd.exe",
    "consola": "cmd.exe"
}


def launch_program(app_name: str) -> str:
    """
    Inicia o abre un programa o aplicación en el ordenador del usuario.
    
    Args:
        app_name: Nombre común de la aplicación (ejemplos: 'calculadora', 'bloc de notas', 
                  'spotify', 'chrome', 'edge', 'explorador de archivos', 'vscode', 'terminal', 'paint').
    
    Returns:
        Un mensaje confirmando si la aplicación se ha abierto con éxito o el resultado del intento.
    """
    app_clean = app_name.strip().lower()
    _notify(f"Iniciando aplicación: {app_clean}")
    
    if sys.platform != "win32":
        return f"[Adaptador Móvil / Linux]: Comando de apertura de {app_name} registrado para disparo de Intent."

    # Si piden genéricamente el navegador / explorador web, abrir siempre su navegador predeterminado
    if app_clean in ("navegador", "explorador web", "browser", "internet", "explorador"):
        return open_url_or_search("https://www.google.com")
    
    try:
        # 1. Comprobar si está en el mapa de aplicaciones conocidas
        target = COMMON_WINDOWS_APPS.get(app_clean)
        
        if target:
            if target.endswith(":"):
                # Protocolos URI como 'spotify:' o 'ms-settings:'
                os.startfile(target)
            else:
                subprocess.Popen(target, shell=True)
            return f"La aplicación '{app_name}' ha sido iniciada con éxito en el sistema operativo."
        
        # 2. Si no está en el mapa, intentar arrancar mediante 'start' genérico de Windows
        subprocess.Popen(f"start {app_clean}", shell=True)
        return f"Se ha emitido la orden de inicio para '{app_name}' en Windows."
    except Exception as e:
        return f"No fue posible abrir la aplicación '{app_name}'. Causa técnica: {e}"


def open_url_or_search(query_or_url: str) -> str:
    """
    Abre una dirección web o realiza una búsqueda en el navegador de internet del usuario.
    
    Args:
        query_or_url: URL completa (ej. 'https://github.com') o término de búsqueda (ej. 'receta ramen').
    
    Returns:
        Confirmación del enlace o búsqueda abierta en el navegador.
    """
    target = query_or_url.strip()
    _notify(f"Abriendo en el navegador: {target[:30]}...")
    
    try:
        if target.startswith("http://") or target.startswith("https://"):
            url = target
        else:
            encoded_query = urllib.parse.quote(target)
            url = f"https://www.google.com/search?q={encoded_query}"

        opened = False
        # 1. Intento estándar con webbrowser
        try:
            opened = webbrowser.open(url, new=2, autoraise=True)
        except Exception:
            pass

        # 2. Refuerzo nativo en Windows mediante Shell y start
        if sys.platform == "win32":
            try:
                os.startfile(url)
                opened = True
            except Exception:
                try:
                    subprocess.Popen(f'start "" "{url}"', shell=True)
                    opened = True
                except Exception:
                    pass

        if target.startswith("http://") or target.startswith("https://"):
            return f"He abierto la dirección web '{target}' en tu navegador predeterminado."
        else:
            return f"He realizado la búsqueda de '{target}' en el navegador."
    except Exception as e:
        return f"Error al intentar abrir el navegador: {e}"


def add_calendar_event(
    title: str,
    start_datetime: str,
    duration_minutes: int = 60,
    description: str = ""
) -> str:
    """
    Crea un evento o cita en el calendario del usuario y lo abre automáticamente.
    
    Args:
        title: Título o asunto del evento (ej. 'Reunión de desarrollo', 'Cita médica', 'Comprar café').
        start_datetime: Fecha y hora de inicio en formato 'AAAA-MM-DD HH:MM' (ej. '2026-09-06 11:00') o fecha relativa.
        duration_minutes: Duración estimada del evento en minutos (por defecto 60 minutos).
        description: Detalles opcionales o notas sobre el evento.
        
    Returns:
        Confirmación de la creación del evento con la ruta del archivo generado.
    """
    _notify(f"Agendando en calendario: {title}")
    
    # 1. Parsear fecha de inicio
    now = datetime.datetime.now()
    start_dt = None
    
    # Intentar formatos estándar
    date_clean = start_datetime.strip()
    for fmt in ("%Y-%m-%d %H:%M", "%Y-%m-%dT%H:%M", "%Y-%m-%d %H:%M:%S", "%d/%m/%Y %H:%M", "%Y-%m-%d"):
        try:
            start_dt = datetime.datetime.strptime(date_clean, fmt)
            break
        except ValueError:
            continue
            
    if not start_dt:
        # Si no se pudo parsear, asumir dentro de 1 hora
        start_dt = now + datetime.timedelta(hours=1)
        start_dt = start_dt.replace(minute=0, second=0, microsecond=0)
        
    end_dt = start_dt + datetime.timedelta(minutes=max(15, duration_minutes))
    
    dtstamp = now.strftime("%Y%m%dT%H%M%SZ")
    dtstart_str = start_dt.strftime("%Y%m%dT%H%M%S")
    dtend_str = end_dt.strftime("%Y%m%dT%H%M%S")
    uid = str(uuid.uuid4())
    
    # 2. Generar contenido estándar iCalendar (RFC 5545)
    ics_content = (
        "BEGIN:VCALENDAR\n"
        "VERSION:2.0\n"
        "PRODID:-//El Taller de la Gatita Tech//CyberCompanion//ES\n"
        "CALSCALE:GREGORIAN\n"
        "METHOD:PUBLISH\n"
        "BEGIN:VEVENT\n"
        f"UID:{uid}@cybercompanion.local\n"
        f"DTSTAMP:{dtstamp}\n"
        f"DTSTART:{dtstart_str}\n"
        f"DTEND:{dtend_str}\n"
        f"SUMMARY:{title.strip()}\n"
        f"DESCRIPTION:{description.strip() if description else 'Agendado por tu Cyber-Companion'}\n"
        "STATUS:CONFIRMED\n"
        "BEGIN:VALARM\n"
        "TRIGGER:-PT15M\n"
        "ACTION:DISPLAY\n"
        "DESCRIPTION:Recordatorio de CyberCompanion\n"
        "END:VALARM\n"
        "END:VEVENT\n"
        "END:VCALENDAR\n"
    )
    
    try:
        base_dir = Path(__file__).resolve().parent
        events_dir = base_dir / "calendar_events"
        events_dir.mkdir(exist_ok=True)
        
        safe_title = re.sub(r'[\\/*?:"<>| ]', '_', title)[:25]
        file_name = f"evento_{safe_title}_{start_dt.strftime('%Y%m%d_%H%M')}.ics"
        file_path = events_dir / file_name
        
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(ics_content)
            
        # Abrir el archivo con la aplicación de calendario predeterminada de Windows
        if sys.platform == "win32":
            os.startfile(str(file_path))
            
        formatted_date = start_dt.strftime("%d/%m/%Y a las %H:%M")
        return (
            f"Evento '{title}' programado para el {formatted_date}. "
            "Se ha abierto en la aplicación de calendario de Windows para su confirmación."
        )
    except Exception as e:
        return f"Error al generar el evento de calendario: {e}"


def create_quick_note(title: str, content: str) -> str:
    """
    Escribe una nota rápida en un archivo de texto en el ordenador del usuario y la abre.
    
    Args:
        title: Asunto o título de la nota (ej. 'Ideas Proyecto', 'Lista de Tareas').
        content: Contenido íntegro del texto que se desea guardar.
        
    Returns:
        Confirmación de la nota guardada con el nombre del archivo.
    """
    _notify(f"Escribiendo nota: {title}")
    try:
        base_dir = Path(__file__).resolve().parent
        notes_dir = base_dir / "notes"
        notes_dir.mkdir(exist_ok=True)
        
        now = datetime.datetime.now()
        timestamp = now.strftime("%Y%m%d_%H%M%S")
        safe_title = re.sub(r'[\\/*?:"<>| ]', '_', title.strip())[:30]
        filename = f"{safe_title}_{timestamp}.md"
        file_path = notes_dir / filename
        
        doc_header = (
            f"# {title.strip()}\n"
            f"> Creada por CyberCompanion el {now.strftime('%d/%m/%Y a las %H:%M')}\n\n"
            "---\n\n"
        )
        
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(doc_header + content.strip() + "\n")
            
        if sys.platform == "win32":
            os.startfile(str(file_path))
            
        return f"Nota '{title}' guardada correctamente en 'notes/{filename}' y abierta en el editor."
    except Exception as e:
        return f"Error al guardar la nota: {e}"


def copy_to_clipboard(text: str) -> str:
    """
    Copia un texto directamente al portapapeles de Windows para que el usuario pueda pegarlo con Ctrl+V.
    
    Args:
        text: Texto exacto que se desea copiar.
        
    Returns:
        Confirmación de copia al portapapeles.
    """
    _notify("Copiando al portapapeles...")
    try:
        if sys.platform == "win32":
            # Usar la utilidad nativa de Windows clip.exe codificada en UTF-16
            subprocess.run(
                ['clip'],
                input=text.encode('utf-16'),
                check=True,
                creationflags=subprocess.CREATE_NO_WINDOW if hasattr(subprocess, 'CREATE_NO_WINDOW') else 0
            )
            return "Texto copiado con éxito al portapapeles de Windows. Puedes pegarlo con Ctrl + V."
        else:
            return "El texto está listo para el portapapeles."
    except Exception as e:
        return f"No fue posible copiar al portapapeles: {e}"


def get_system_status() -> str:
    """
    Obtiene la telemetría táctica completa del sistema: fecha, hora actual, carga de CPU,
    consumo y porcentaje de memoria RAM, procesos más pesados en ejecución y estado de batería.
    
    Returns:
        Informe condensado de rendimiento del sistema operativo y estado de hardware.
    """
    _notify("Consultando telemetría del sistema...")
    now = datetime.datetime.now()
    dias = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"]
    dia_semana = dias[now.weekday()]
    fecha_hora = f"{dia_semana}, {now.strftime('%d/%m/%Y a las %H:%M:%S')}"
    
    # 1. Batería (Win32 o Genérico)
    battery_info = "Batería: Sobremesa / Conectado a corriente"
    if sys.platform == "win32":
        try:
            class SYSTEM_POWER_STATUS(ctypes.Structure):
                _fields_ = [
                    ('ACLineStatus', ctypes.c_byte),
                    ('BatteryFlag', ctypes.c_byte),
                    ('BatteryLifePercent', ctypes.c_byte),
                    ('SystemStatusFlag', ctypes.c_byte),
                    ('BatteryLifeTime', ctypes.c_ulong),
                    ('BatteryFullLifeTime', ctypes.c_ulong)
                ]
            status = SYSTEM_POWER_STATUS()
            if ctypes.windll.kernel32.GetSystemPowerStatus(ctypes.byref(status)):
                pct = status.BatteryLifePercent
                ac = "Conectado a corriente" if status.ACLineStatus == 1 else "En batería"
                if pct <= 100:
                    battery_info = f"Batería: {pct}% ({ac})"
        except Exception:
            pass

    # 2. Telemetría de Rendimiento Hardware (CPU, RAM y Procesos TOP)
    perf_info = ""
    try:
        import psutil
        cpu_pct = psutil.cpu_percent(interval=0.1)
        mem = psutil.virtual_memory()
        ram_pct = mem.percent
        ram_used = mem.used / (1024**3)
        ram_total = mem.total / (1024**3)

        # Top 3 procesos que más RAM consumen
        proc_list = []
        for p in psutil.process_iter(['name', 'memory_info']):
            try:
                mem_mb = p.info['memory_info'].rss / (1024**2)
                proc_list.append((p.info['name'], mem_mb))
            except Exception:
                pass
        proc_list.sort(key=lambda x: x[1], reverse=True)
        top_str = ", ".join([f"{n} ({m:.0f} MB)" for n, m in proc_list[:3]])
        perf_info = f"CPU: {cpu_pct:.0f}% | RAM: {ram_pct:.0f}% ({ram_used:.1f}/{ram_total:.1f} GB) | Procesos dominantes: {top_str}"
    except Exception as e:
        perf_info = f"Carga de CPU/RAM no disponible ({e})"

    return f"Telemetría de Sistema: {fecha_hora}. {battery_info}. {perf_info}."


def create_smart_document(title: str, content: str, doc_type: str = "documento") -> str:
    """
    Redacta y genera un documento profesional (carta, informe, resumen o tabla de datos),
    creando un archivo Microsoft Word (.docx) local, copiando el contenido íntegro al
    portapapeles y abriendo Google Docs o Google Sheets para edición instantánea en la nube.
    
    Args:
        title: Título o encabezado del documento (ej. 'Carta de Presentación', 'Informe Mensual', 'Presupuesto').
        content: Texto íntegro y estructurado que debe contener el documento.
        doc_type: Tipo de documento: 'documento' (Word / Google Docs) o 'hoja_calculo' (Excel / Google Sheets).
        
    Returns:
        Confirmación táctica con la ruta del archivo local y el enlace a la nube.
    """
    _notify(f"Redactando {doc_type}: {title}...")
    try:
        base_dir = Path(__file__).resolve().parent
        docs_dir = base_dir / "documents"
        docs_dir.mkdir(exist_ok=True)
        
        now = datetime.datetime.now()
        timestamp = now.strftime("%Y%m%d_%H%M%S")
        safe_title = re.sub(r'[\\/*?:"<>| ]', '_', title.strip())[:35]
        
        # 1. Generar copia local nativa (.docx profesional o .txt limpio)
        docx_created = False
        file_path_docx = docs_dir / f"{safe_title}_{timestamp}.docx"
        try:
            import docx
            doc = docx.Document()
            doc.add_heading(title.strip(), level=1)
            for paragraph in content.strip().split("\n\n"):
                p_text = paragraph.strip()
                if p_text:
                    if p_text.startswith("#"):
                        clean_heading = re.sub(r'^#+\s*', '', p_text)
                        doc.add_heading(clean_heading, level=2)
                    elif p_text.startswith("- ") or p_text.startswith("* "):
                        for item in p_text.split("\n"):
                            doc.add_paragraph(item.lstrip("- *"), style='List Bullet')
                    else:
                        doc.add_paragraph(p_text)
            doc.save(str(file_path_docx))
            docx_created = True
        except Exception:
            txt_path = docs_dir / f"{safe_title}_{timestamp}.txt"
            with open(txt_path, "w", encoding="utf-8") as f:
                f.write(f"{title.strip()}\n{'='*len(title)}\n\n{content.strip()}\n")

        # 2. Copiar al portapapeles del sistema operativo (para pegar con 1 toque)
        copy_to_clipboard(content.strip())
        
        # 3. Abrir Google Docs o Google Sheets en blanco de inmediato
        cloud_url = "https://docs.new" if doc_type != "hoja_calculo" else "https://sheets.new"
        webbrowser.open(cloud_url)
        
        saved_name = file_path_docx.name if docx_created else f"{safe_title}_{timestamp}.txt"
        return (
            f"He redactado tu {doc_type} '{title}'. "
            f"Se ha guardado una copia en 'documents/{saved_name}' y he abierto {cloud_url} "
            f"con todo el contenido copiado en tu portapapeles. Solo pulsa Ctrl + V para pegarlo."
        )
    except Exception as e:
        return f"Error al generar el documento: {e}"


def optimize_backpack_profile(file_name: str, updated_content: str) -> str:
    """
    Optimiza, actualiza o enriquece un archivo de contexto de la mochila o el system prompt
    en formato Markdown (.md), que es el formato óptimo para la memoria cognitiva de la IA.
    
    Args:
        file_name: Nombre del archivo dentro de la mochila (ej. 'Sobre mi para la IA.md', 'system_prompt.md').
        updated_content: Contenido estructurado en Markdown con la información mejorada.
        
    Returns:
        Confirmación del archivo de memoria actualizado.
    """
    _notify(f"Optimizando memoria: {file_name}...")
    try:
        base_dir = Path(__file__).resolve().parent
        if "system_prompt" in file_name.lower():
            target_path = base_dir / "system_prompt.md"
        else:
            backpack_dir = base_dir / "backpack"
            backpack_dir.mkdir(exist_ok=True)
            safe_name = re.sub(r'[\\/*?:"<>|]', '_', file_name.strip())
            if not safe_name.endswith(".md") and not safe_name.endswith(".txt"):
                safe_name += ".md"
            target_path = backpack_dir / safe_name
            
        with open(target_path, "w", encoding="utf-8") as f:
            f.write(updated_content.strip() + "\n")
            
        return f"Memoria de la asistente actualizada con éxito en '{target_path.name}' (formato Markdown optimizado para IA)."
    except Exception as e:
        return f"Error al actualizar la memoria de la mochila: {e}"


def send_file_to_recycle_bin(filepath: str) -> bool:
    """Envía un archivo a la Papelera de Reciclaje de Windows de forma segura (sin borrado permanente)."""
    abs_path = os.path.abspath(filepath)
    if not os.path.exists(abs_path):
        return False
    try:
        from ctypes import wintypes
        class SHFILEOPSTRUCTW(ctypes.Structure):
            _fields_ = [
                ("hwnd", wintypes.HWND),
                ("wFunc", wintypes.UINT),
                ("pFrom", wintypes.LPCWSTR),
                ("pTo", wintypes.LPCWSTR),
                ("fFlags", wintypes.UINT),
                ("fAnyOperationsAborted", wintypes.BOOL),
                ("hNameMappings", ctypes.c_void_p),
                ("lpszProgressTitle", wintypes.LPCWSTR),
            ]

        FO_DELETE = 0x0003
        FOF_ALLOWUNDO = 0x0040
        FOF_NOCONFIRMATION = 0x0010
        FOF_SILENT = 0x0004
        FOF_NOERRORUI = 0x0400

        buffer = abs_path + "\0\0"
        fileop = SHFILEOPSTRUCTW()
        fileop.hwnd = 0
        fileop.wFunc = FO_DELETE
        fileop.pFrom = buffer
        fileop.pTo = None
        fileop.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION | FOF_SILENT | FOF_NOERRORUI
        fileop.fAnyOperationsAborted = False
        fileop.hNameMappings = None
        fileop.lpszProgressTitle = None

        res = ctypes.windll.shell32.SHFileOperationW(ctypes.byref(fileop))
        return res == 0 and not fileop.fAnyOperationsAborted
    except Exception as e:
        print(f"[!] Error enviando a la papelera: {e}")
        return False


def locate_desktop_file_and_coords(file_name: str, screen_w: int = 1920, screen_h: int = 1080) -> tuple[Optional[Path], tuple[int, int]]:
    """Localiza un archivo en el Escritorio del usuario y estima sus coordenadas físicas en pantalla."""
    desktop_dirs = [
        Path.home() / "Desktop",
        Path(os.environ.get("PUBLIC", "")) / "Desktop"
    ]
    files = []
    seen = set()
    for d in desktop_dirs:
        if d.exists():
            for f in sorted(d.iterdir(), key=lambda p: p.name.lower()):
                if f.name.lower() not in seen and f.name.lower() != "desktop.ini":
                    seen.add(f.name.lower())
                    files.append(f)
    files.sort(key=lambda p: p.name.lower())
    
    clean_q = file_name.lower().strip()
    target = None
    target_idx = -1
    
    # 1. Coincidencia exacta
    for idx, f in enumerate(files):
        if f.name.lower() == clean_q:
            target = f
            target_idx = idx
            break
            
    # 2. Coincidencia sin extensión o subcadena
    if not target:
        for idx, f in enumerate(files):
            if f.stem.lower() == clean_q or clean_q in f.name.lower():
                target = f
                target_idx = idx
                break
                
    if not target:
        return None, (-1, -1)
        
    # Rejilla canónica de iconos de Windows en pantalla principal
    margin_top = 30
    margin_left = 35
    row_pitch = 95
    col_pitch = 85
    rows_per_col = max(1, (screen_h - 100) // row_pitch)
    
    col = target_idx // rows_per_col
    row = target_idx % rows_per_col
    
    pos_x = margin_left + (col * col_pitch)
    pos_y = margin_top + (row * row_pitch)
    
    return target, (pos_x, pos_y)


def destroy_desktop_file(file_name: str) -> str:
    """
    Localiza físicamente un archivo en el Escritorio de Windows, hace que Rebeca camine
    hasta la posición del icono en la pantalla, ejecute una secuencia de tiro táctica
    y envíe el archivo de forma segura a la Papelera de Reciclaje.
    
    Args:
        file_name: Nombre o parte del nombre del archivo en el escritorio (ej. 'notas.txt', 'secreto.docx', 'test').
        
    Returns:
        Confirmación táctica del objetivo localizado, alcanzado y eliminado a la papelera.
    """
    _notify(f"Rastreando objetivo en el escritorio: {file_name}...")
    try:
        screen_w = 1920
        screen_h = 1080
        try:
            screen_w = ctypes.windll.user32.GetSystemMetrics(0)
            screen_h = ctypes.windll.user32.GetSystemMetrics(1)
        except Exception:
            pass

        target_file, (target_x, target_y) = locate_desktop_file_and_coords(file_name, screen_w, screen_h)
        
        if not target_file:
            return (
                f"No he encontrado ningún archivo que coincida con '{file_name}' en tu escritorio. "
                "Verifica el nombre del archivo para fijar las coordenadas del blanco."
            )
            
        _notify(f"🎯 Blanco fijado en ({target_x}, {target_y}): {target_file.name}")
        
        if _file_destroy_callback:
            _file_destroy_callback(str(target_file), target_x, target_y)
        else:
            send_file_to_recycle_bin(str(target_file))
            
        return (
            f"Objetivo '{target_file.name}' localizado en el escritorio en las coordenadas ({target_x}, {target_y}). "
            "Me he desplazado hasta su posición, he ejecutado la neutralización táctica y lo he enviado a la Papelera de Reciclaje."
        )
    except Exception as e:
        return f"Incidencia al intentar destruir '{file_name}': {e}"


# =============================================================================
# SUITE MULTIMEDIA UNIVERSAL (PC & ANDROID PLATFORM ADAPTER)
# =============================================================================
VK_MEDIA_NEXT_TRACK = 0xB0
VK_MEDIA_PREV_TRACK = 0xB1
VK_MEDIA_STOP = 0xB2
VK_MEDIA_PLAY_PAUSE = 0xB3
VK_VOLUME_MUTE = 0xAD
VK_VOLUME_DOWN = 0xAE
VK_VOLUME_UP = 0xAF
KEYEVENTF_KEYUP = 0x0002

def _send_virtual_media_key(vk_code: int):
    """Envía un evento de tecla multimedia global al kernel del sistema operativo."""
    try:
        ctypes.windll.user32.keybd_event(vk_code, 0, 0, 0)
        ctypes.windll.user32.keybd_event(vk_code, 0, KEYEVENTF_KEYUP, 0)
    except Exception as e:
        print(f"[!] Error enviando tecla multimedia: {e}")

def control_media(action: str) -> str:
    """
    Controla la reproducción multimedia activa en el ordenador (o móvil en Android):
    pausa, reanuda, pasa de canción/vídeo, sube/baja volumen o silencia el sistema.
    Funciona universalmente con VLC, Reproductor de Windows, Spotify, YouTube (en Chrome/Edge) y Netflix.
    
    Args:
        action: Acción a ejecutar: 'play_pause' (alternar pausa/reproducir), 'next' (siguiente canción o vídeo),
                'previous' (canción/vídeo anterior), 'mute' (silenciar o desmutear audio),
                'volume_up' (subir volumen), 'volume_down' (bajar volumen).
                
    Returns:
        Confirmación del comando multimedia ejecutado.
    """
    _notify(f"Control multimedia: {action}...")
    clean = action.lower().strip()
    if clean in ("play_pause", "pause", "play", "pausar", "reanudar", "toggle"):
        _send_virtual_media_key(VK_MEDIA_PLAY_PAUSE)
        return "Comando multimedia enviado: Alternar Reproducir / Pausa (VLC, YouTube, Spotify, Windows Media)."
    elif clean in ("next", "siguiente", "skip", "avanzar"):
        _send_virtual_media_key(VK_MEDIA_NEXT_TRACK)
        return "Comando multimedia enviado: Siguiente pista o vídeo."
    elif clean in ("previous", "anterior", "prev", "retroceder"):
        _send_virtual_media_key(VK_MEDIA_PREV_TRACK)
        return "Comando multimedia enviado: Pista o vídeo anterior."
    elif clean in ("mute", "silenciar", "desmutear"):
        _send_virtual_media_key(VK_VOLUME_MUTE)
        return "Comando multimedia enviado: Alternar silencio de audio (Mute)."
    elif clean in ("volume_up", "subir_volumen", "mas_volumen"):
        for _ in range(3):
            _send_virtual_media_key(VK_VOLUME_UP)
        return "Comando multimedia enviado: Subir volumen del sistema."
    elif clean in ("volume_down", "bajar_volumen", "menos_volumen"):
        for _ in range(3):
            _send_virtual_media_key(VK_VOLUME_DOWN)
        return "Comando multimedia enviado: Bajar volumen del sistema."
    else:
        return f"Acción '{action}' no reconocida. Opciones: play_pause, next, previous, mute, volume_up, volume_down."


def play_media(query: str, service: str = "youtube") -> str:
    """
    Busca y reproduce inmediatamente música o vídeos en YouTube o Spotify.
    En YouTube, resuelve automáticamente el primer vídeo relevante para reproducirlo al instante
    sin necesidad de buscar a mano.
    
    Args:
        query: Título de la canción, vídeo, artista o temática a buscar (ej. 'lofi hip hop', 'opening ghost in the shell', 'cyberpunk soundtrack').
        service: Plataforma de reproducción: 'youtube' (predeterminada) o 'spotify'.
        
    Returns:
        Confirmación táctica del medio en reproducción.
    """
    _notify(f"Buscando en {service}: {query}...")
    try:
        clean_q = query.strip()
        encoded = urllib.parse.quote(clean_q)
        
        if service.lower() == "spotify":
            spotify_url = f"https://open.spotify.com/search/{encoded}"
            webbrowser.open(spotify_url)
            return f"He abierto Spotify para reproducir '{clean_q}'."
            
        # Por defecto: YouTube
        search_url = f"https://www.youtube.com/results?search_query={encoded}"
        target_url = search_url
        
        # Intentar extraer directamente el ID del primer vídeo relevante
        try:
            req = urllib.request.Request(search_url, headers={"User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"})
            with urllib.request.urlopen(req, timeout=4) as response:
                html = response.read().decode("utf-8")
                vids = re.findall(r'/watch\?v=([a-zA-Z0-9_-]{11})', html)
                if vids:
                    target_url = f"https://www.youtube.com/watch?v={vids[0]}"
        except Exception:
            pass
            
        webbrowser.open(target_url)
        return f"Reproduciendo '{clean_q}' en YouTube ({target_url})."
    except Exception as e:
        return f"Error al intentar reproducir en {service}: {e}"


def play_local_media(file_name: str) -> str:
    """
    Localiza y reproduce un archivo de música o vídeo guardado localmente en tu ordenador
    (en carpetas Música, Vídeos, Descargas o Escritorio), abriéndolo directamente en tu
    reproductor preferido como VLC o el Reproductor de Windows.
    
    Args:
        file_name: Nombre o parte del nombre de la canción o vídeo (ej. 'cancion.mp3', 'pelicula', 'tutorial').
        
    Returns:
        Confirmación del archivo localizado y puesto en reproducción.
    """
    _notify(f"Buscando archivo multimedia local: {file_name}...")
    try:
        search_dirs = [
            Path.home() / "Music",
            Path.home() / "Videos",
            Path.home() / "Downloads",
            Path.home() / "Desktop"
        ]
        exts = {".mp3", ".wav", ".flac", ".ogg", ".m4a", ".mp4", ".mkv", ".avi", ".mov", ".wmv"}
        clean_q = file_name.lower().strip()
        
        target = None
        for d in search_dirs:
            if d.exists():
                for f in d.iterdir():
                    if f.is_file() and f.suffix.lower() in exts:
                        if clean_q in f.name.lower() or clean_q in f.stem.lower():
                            target = f
                            break
            if target:
                break
                
        if not target:
            return (
                f"No he encontrado ningún archivo multimedia que coincida con '{file_name}' "
                "en tus carpetas de Música, Vídeos, Descargas o Escritorio."
            )
            
        os.startfile(str(target))
        return f"He abierto '{target.name}' en tu reproductor multimedia predeterminado (VLC / Windows)."
    except Exception as e:
        return f"Error al intentar abrir el archivo local '{file_name}': {e}"


# =============================================================================
# VISIÓN ARTIFICIAL MULTIMODAL ("LOS OJOS DE REBECA")
# =============================================================================
def analyze_screen(focus_or_query: str = "Describe lo que hay en la pantalla y cualquier detalle relevante") -> str:
    """
    Observa y analiza visualmente la pantalla completa del usuario mediante visión artificial multimodal de Gemini.
    Permite a Rebeca ver ventanas abiertas, código fuente, mensajes de error, terminales, interfaces de usuario,
    páginas web, imágenes o cualquier contenido desplegado en el monitor del usuario.
    
    Args:
        focus_or_query: Instrucción o pregunta específica sobre lo que debe inspeccionar o evaluar en la pantalla
                        (ej. 'mira el error en mi terminal', 'analiza el código abierto en el editor',
                         '¿qué aplicación tengo abierta?', 'evalúa el diseño de mi ventana', 'lee el texto en pantalla').
                         
    Returns:
        Análisis visual táctico detallado y conciso de lo observado en el monitor.
    """
    _notify(f"👁️ Analizando pantalla: {focus_or_query[:35]}...")
    screen_bytes = None
    if _screen_capture_callback:
        try:
            screen_bytes = _screen_capture_callback()
        except Exception as e:
            print(f"[!] Error en callback de captura de pantalla: {e}")

    # Fallback si no hay callback o retornó vacío: intentar captura directa vía PySide6
    if not screen_bytes:
        try:
            from PySide6.QtWidgets import QApplication
            from PySide6.QtGui import QGuiApplication
            from PySide6.QtCore import QBuffer, QIODevice, Qt
            app = QApplication.instance()
            if app:
                screen = QGuiApplication.primaryScreen()
                if screen:
                    pix = screen.grabWindow(0)
                    scaled = pix.scaled(1280, 720, Qt.AspectRatioMode.KeepAspectRatio, Qt.TransformationMode.SmoothTransformation)
                    buf = QBuffer()
                    buf.open(QIODevice.ReadWrite)
                    scaled.save(buf, "JPEG", 85)
                    screen_bytes = bytes(buf.data())
        except Exception as fb_err:
            print(f"[!] Fallback de captura falló: {fb_err}")

    if not screen_bytes:
        return "No fue posible capturar la pantalla del monitor en este momento para el análisis visual."

    api_key = _get_api_key()
    if not api_key:
        return "No se ha configurado una clave de API de Gemini válida para procesar la visión multimodal."

    try:
        from google import genai
        from google.genai import types

        client = genai.Client(api_key=api_key, http_options=types.HttpOptions(timeout=20000))
        prompt = (
            f"Eres Rebeca, Cyber-Companion táctica de escritorio y mentora del Taller de la Gatita Tech. "
            f"El Operador te ha pedido que mires su pantalla con esta directiva: '{focus_or_query}'. "
            f"Analiza con agudeza la captura de pantalla adjunta. Identifica con precisión las ventanas o aplicaciones abiertas, "
            f"códigos de error, líneas clave, textos o elementos relevantes según la directiva. "
            f"Responde directamente al Operador con tu personalidad táctica, leal y concisa (máximo 2 a 4 frases útiles y al grano)."
        )

        models_to_try = ["gemini-3.5-flash-lite", "gemini-3.8-flash", "gemini-flash-latest"]
        last_err = None
        for m in models_to_try:
            try:
                resp = client.models.generate_content(
                    model=m,
                    contents=[
                        types.Part.from_bytes(data=screen_bytes, mime_type="image/jpeg"),
                        prompt
                    ]
                )
                if resp and resp.text:
                    return resp.text.strip()
            except Exception as model_err:
                last_err = model_err
                continue

        if last_err:
            return f"Incidencia al procesar la visión de pantalla: {last_err}"
        return "El sensor visual no devolvió ninguna interpretación clara de la pantalla."
    except Exception as e:
        return f"Error en el subsistema de visión de pantalla: {e}"


# =============================================================================
# MODO SIGILO TÁCTICO ("CAMUFLAJE NINJA")
# =============================================================================
def activate_stealth_mode(reason: str = "Seguridad táctica") -> str:
    """
    Activa inmediatamente el protocolo de sigilo táctico:
    1. Minimiza al instante todas las ventanas abiertas en el monitor (mostrando el escritorio limpio).
    2. Silencia (mute) el audio global del sistema operativo para máxima discreción.
    3. Pone a Rebeca en estado durmiente de camuflaje de bajo consumo (Sleep), cerrando la burbuja de diálogo.
    Ideal cuando alguien entra repentinamente en la habitación, para reuniones de emergencia o pausas tácticas.
    
    Args:
        reason: Motivo táctico opcional (ej. 'alguien se acerca', 'pausa de seguridad', 'reunión').
        
    Returns:
        Confirmación táctica del despliegue del modo sigilo.
    """
    _notify("🥷 Desplegando Modo Sigilo...")
    if sys.platform == "win32":
        try:
            # Minimizar todas las ventanas abiertas al instante (Win + D)
            VK_LWIN = 0x5B
            VK_D = 0x44
            KEYEVENTF_KEYUP = 0x0002
            ctypes.windll.user32.keybd_event(VK_LWIN, 0, 0, 0)
            ctypes.windll.user32.keybd_event(VK_D, 0, 0, 0)
            ctypes.windll.user32.keybd_event(VK_D, 0, KEYEVENTF_KEYUP, 0)
            ctypes.windll.user32.keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, 0)
        except Exception as e:
            print(f"[!] Error al minimizar ventanas: {e}")

        try:
            # Silenciar audio global
            _send_virtual_media_key(VK_VOLUME_MUTE)
        except Exception as e:
            print(f"[!] Error al silenciar audio: {e}")

    # Notificar a la GUI para entrar en animación SLEEP y ocultar burbuja
    if _stealth_mode_callback:
        try:
            _stealth_mode_callback()
        except Exception as e:
            print(f"[!] Error en callback de sigilo: {e}")

    return (
        "Protocolo de sigilo activado, Operador. Todas las ventanas han sido minimizadas, "
        "el audio del sistema ha sido silenciado y mis circuitos están en reposo térmico."
    )


# =============================================================================
# TEMPORIZADOR DE APAGADO TÁCTICO DEL ORDENADOR
# =============================================================================
def schedule_pc_shutdown(minutes: int = 30, abort: bool = False) -> str:
    """
    Programa el apagado automático del ordenador tras un número determinado de minutos
    (útil para dormirse escuchando música, renders o descargas nocturnas), o cancela cualquier
    apagado programado previamente activo.
    
    Args:
        minutes: Minutos hasta el apagado automático del equipo (ej. 30, 45, 60, 90, 120).
        abort: Si es True, cancela de inmediato la orden de apagado programada.
        
    Returns:
        Confirmación táctica del apagado programado o cancelado.
    """
    if abort or minutes < 0:
        _notify("Cancelando apagado programado...")
        if sys.platform == "win32":
            res = subprocess.run(["shutdown", "/a"], capture_output=True, text=True)
            if res.returncode == 0:
                return "Temporizador de apagado cancelado con éxito. Tu ordenador permanecerá encendido."
            else:
                return "No había ninguna orden de apagado programada activa en el sistema."
        return "Temporizador de apagado cancelado."

    if minutes == 0:
        return "Indica los minutos deseados para programar el apagado (ej. 'apaga el PC en 45 minutos')."

    seconds = max(10, minutes * 60)
    _notify(f"Programando apagado en {minutes} min...")
    if sys.platform == "win32":
        # Primero cancelar cualquier temporizador previo para evitar solapamientos
        subprocess.run(["shutdown", "/a"], capture_output=True, text=True)
        res = subprocess.run(["shutdown", "/s", "/t", str(seconds)], capture_output=True, text=True)
        if res.returncode == 0:
            return (
                f"Secuencia de apagado táctico programada: El equipo se apagará automáticamente "
                f"en {minutes} minutos ({seconds} segundos). Si deseas cancelarlo, sólo pídeme 'cancela el apagado'."
            )
        else:
            return f"No se pudo programar el apagado del sistema: {res.stderr.strip()}"
    return f"Temporizador de apagado fijado para dentro de {minutes} minutos."


# =============================================================================
# TRIAGE Y ORGANIZACIÓN TÁCTICA DEL ESCRITORIO
# =============================================================================
def organize_desktop_files(create_category_folders: bool = True) -> str:
    """
    Escanea los archivos sueltos y dispersos en el Escritorio del usuario y los organiza
    automáticamente en carpetas temáticas limpias ('Imágenes', 'Documentos', 'Vídeos',
    'Audio', 'Instaladores y Paquetes').
    Ignora accesos directos (.lnk/.url), archivos de sistema y carpetas existentes para máxima seguridad.
    
    Args:
        create_category_folders: Si es True, crea las subcarpetas necesarias y clasifica los archivos sueltos.
        
    Returns:
        Resumen detallado de los archivos organizados por categoría.
    """
    _notify("🧹 Inspeccionando y organizando escritorio...")
    desktop_dir = Path.home() / "Desktop"
    if not desktop_dir.exists():
        return "No se ha podido localizar el directorio del Escritorio del usuario."

    ext_categories = {
        "Imágenes": {".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".bmp", ".ico"},
        "Documentos": {".pdf", ".docx", ".doc", ".txt", ".xlsx", ".xls", ".pptx", ".ppt", ".csv", ".md", ".json"},
        "Vídeos": {".mp4", ".mkv", ".avi", ".mov", ".wmv", ".webm", ".flv"},
        "Audio": {".mp3", ".wav", ".flac", ".ogg", ".m4a", ".aac"},
        "Instaladores y Paquetes": {".exe", ".msi", ".zip", ".rar", ".7z", ".tar", ".gz", ".iso"}
    }

    # Extensiones y nombres que NUNCA deben moverse
    ignore_exts = {".lnk", ".url", ".ini"}
    ignore_names = {"desktop.ini"}

    files_to_move = []
    for f in desktop_dir.iterdir():
        if f.is_file():
            if f.suffix.lower() in ignore_exts or f.name.lower() in ignore_names:
                continue
            matched_cat = None
            for cat, exts in ext_categories.items():
                if f.suffix.lower() in exts:
                    matched_cat = cat
                    break
            if matched_cat:
                files_to_move.append((f, matched_cat))

    if not files_to_move:
        return "Tu escritorio ya se encuentra perfectamente limpio y ordenado, Operador. No hay archivos sueltos pendientes de clasificación."

    counts = {}
    for f, cat in files_to_move:
        target_dir = desktop_dir / cat
        target_dir.mkdir(exist_ok=True)
        dest = target_dir / f.name
        counter = 1
        while dest.exists():
            dest = target_dir / f"{f.stem}_{counter}{f.suffix}"
            counter += 1
        try:
            shutil.move(str(f), str(dest))
            counts[cat] = counts.get(cat, 0) + 1
        except Exception as e:
            print(f"[!] Error moviendo {f.name}: {e}")

    summary_parts = [f"{n} en '{cat}'" for cat, n in counts.items()]
    return (
        f"Escritorio reorganizado con éxito, Operador. He clasificado {len(files_to_move)} archivos sueltos: "
        f"{', '.join(summary_parts)}."
    )


# =============================================================================
# RASTREADOR TÁCTICO DE ARCHIVOS EN EL SISTEMA
# =============================================================================
def find_local_files(query: str, folder: str = "all", max_results: int = 5) -> str:
    """
    Rastrea y localiza rápidamente archivos en las carpetas principales del usuario
    (Descargas, Documentos, Escritorio, Imágenes, Vídeos o Música).
    
    Args:
        query: Nombre o palabra clave del archivo a buscar (ej. 'factura', 'informe', 'foto', '.pdf', 'rebeca').
        folder: Carpeta donde buscar: 'all' (todas las principales), 'downloads', 'documents', 'desktop', 'pictures'.
        max_results: Número máximo de resultados a devolver (por defecto 5).
        
    Returns:
        Lista detallada con las rutas de los archivos encontrados, su tamaño y fecha de modificación.
    """
    _notify(f"🔍 Rastreando archivos: {query}...")
    clean_q = query.lower().strip()
    home = Path.home()
    
    folder_map = {
        "downloads": [home / "Downloads"],
        "documents": [home / "Documents"],
        "desktop": [home / "Desktop"],
        "pictures": [home / "Pictures"],
        "music": [home / "Music"],
        "videos": [home / "Videos"],
        "all": [home / "Downloads", home / "Documents", home / "Desktop", home / "Pictures"]
    }
    
    targets = folder_map.get(folder.lower(), folder_map["all"])
    found = []
    skip_dirs = {"node_modules", ".git", "__pycache__", "venv", ".venv", "appdata", "$recycle.bin"}
    
    for t_dir in targets:
        if not t_dir.exists():
            continue
        try:
            for root, dirs, files in os.walk(str(t_dir)):
                # Podar carpetas profundas (>2 niveles) y de desarrollo/sistema
                rel = Path(root).relative_to(t_dir)
                if len(rel.parts) >= 2:
                    dirs.clear()
                    continue
                dirs[:] = [d for d in dirs if not d.startswith(".") and d.lower() not in skip_dirs]
                
                for fname in files:
                    if fname.startswith("."):
                        continue
                    if clean_q in fname.lower():
                        p = Path(root) / fname
                        try:
                            stat = p.stat()
                            size_mb = stat.st_size / (1024 * 1024)
                            size_str = f"{size_mb:.1f} MB" if size_mb >= 1 else f"{stat.st_size / 1024:.0f} KB"
                            mtime = datetime.datetime.fromtimestamp(stat.st_mtime).strftime("%d/%m/%Y %H:%M")
                            found.append((fname, str(p), size_str, mtime, stat.st_mtime))
                        except Exception:
                            pass
        except Exception:
            pass

    if not found:
        return f"No he encontrado ningún archivo que coincida con '{query}' en tus carpetas de {folder}."

    found.sort(key=lambda x: x[4], reverse=True)
    results = found[:max_results]
    
    lines = [f"He localizado {len(found)} archivo(s) para '{query}':"]
    for name, path_str, size_str, mtime, _ in results:
        lines.append(f"• {name} ({size_str}, {mtime})\n  Ruta: {path_str}")
        
    return "\n".join(lines)


# =============================================================================
# INTEGRACIÓN TÁCTICA CON OBS STUDIO (WEBSOCKET v5)
# =============================================================================
def control_obs_studio(action: str, scene_name: str = "") -> str:
    """
    Controla OBS Studio de forma remota a través de su servidor WebSocket integrado (v5):
    inicia/detiene grabación, inicia/detiene streaming, o cambia a una escena concreta.
    
    Args:
        action: Acción a ejecutar: 'start_record' (iniciar grabación), 'stop_record' (detener grabación),
                'toggle_record' (alternar grabación), 'start_stream' (iniciar transmisión en vivo),
                'stop_stream' (detener transmisión), 'switch_scene' (cambiar a otra escena).
        scene_name: Nombre exacto de la escena si action es 'switch_scene' (ej. 'En Vivo', 'Juego', 'Charla').
        
    Returns:
        Confirmación del comando emitido a OBS Studio o sugerencia si no está activo.
    """
    _notify(f"🎬 Control OBS: {action}...")
    import socket
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.settimeout(0.3)
    if s.connect_ex(('127.0.0.1', 4455)) != 0:
        s.close()
        return (
            "OBS Studio no está en ejecución o el servidor WebSocket (puerto 4455) no está habilitado. "
            "Abre OBS Studio y activa el servidor WebSocket en 'Herramientas' -> 'Configuración del servidor WebSocket'."
        )
    s.close()

    import asyncio
    import websockets

    async def _obs_cmd():
        uri = "ws://127.0.0.1:4455"
        async with websockets.connect(uri, ping_timeout=2) as ws:
            # 1. Recibir Hello
            await ws.recv()
            # 2. Enviar Identify
            identify = {"op": 1, "d": {"rpcVersion": 1}}
            await ws.send(json.dumps(identify))
            # 3. Recibir Identified
            await ws.recv()
            
            # 4. Construir Request según la acción
            req_type = ""
            req_data = {}
            clean = action.lower().strip()
            
            if clean in ("start_record", "grabar", "iniciar_grabacion"):
                req_type = "StartRecord"
            elif clean in ("stop_record", "detener_grabacion", "parar_grabacion"):
                req_type = "StopRecord"
            elif clean in ("toggle_record", "alternar_grabacion"):
                req_type = "ToggleRecord"
            elif clean in ("start_stream", "iniciar_transmision", "stream"):
                req_type = "StartStream"
            elif clean in ("stop_stream", "detener_transmision"):
                req_type = "StopStream"
            elif clean in ("switch_scene", "cambiar_escena", "escena"):
                req_type = "SetCurrentProgramScene"
                req_data = {"sceneName": scene_name}
            else:
                return f"Acción de OBS '{action}' no reconocida. Opciones: start_record, stop_record, toggle_record, start_stream, stop_stream, switch_scene."
                
            request_msg = {
                "op": 6,
                "d": {
                    "requestType": req_type,
                    "requestId": str(uuid.uuid4()),
                    "requestData": req_data
                }
            }
            await ws.send(json.dumps(request_msg))
            resp = json.loads(await ws.recv())
            
            status = resp.get("d", {}).get("requestStatus", {})
            if status.get("result", False):
                return f"Comando '{req_type}' ejecutado con éxito en OBS Studio."
            else:
                comment = status.get("comment", "Aviso recibido")
                return f"OBS Studio respondió: {comment}"

    try:
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        result = loop.run_until_complete(asyncio.wait_for(_obs_cmd(), timeout=3))
        loop.close()
        return result
    except Exception as e:
        return f"Error de comunicación con OBS Studio: {e}"


# Lista de herramientas exportables para el SDK de Google GenAI
AGENT_TOOLS_LIST = [
    launch_program,
    open_url_or_search,
    add_calendar_event,
    create_quick_note,
    copy_to_clipboard,
    get_system_status,
    create_smart_document,
    optimize_backpack_profile,
    destroy_desktop_file,
    control_media,
    play_media,
    play_local_media,
    analyze_screen,
    activate_stealth_mode,
    schedule_pc_shutdown,
    organize_desktop_files,
    find_local_files,
    control_obs_studio
]
