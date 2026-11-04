"""
=============================================================================
PROYECTO: CYBER-COMPANION (REBECA 2D LIVE - TALLER DE LA GATITA TECH)
=============================================================================
Motor de escritorio interactivo en PySide6 (Qt6) con:
- Paleta LainOS / Cyberpunk (#090b0e, #121620, #00ffcc, #ff007f, #39ff14)
- Ventana transparente sin marco (Frameless + Translucent)
- Animación fluida con QTimer
- Conexión asíncrona con Google Gemini (google-genai SDK + Búsqueda Web en vivo)
- Síntesis de voz propia neural gratuita (edge-tts + QtMultimedia)
- Lip-sync sincronizado con la reproducción de voz
- Manejo estricto de excepciones y cierre seguro de hilos
=============================================================================
PROTOCOLO DE COMPILACIÓN (PyInstaller):
pyinstaller --noconsole --onefile --name "CyberCompanion_Rebeca" ^
  --add-data "assets/sprites;assets/sprites" main.py
=============================================================================
"""

import os
import sys
import time
import json
import math
import asyncio
import re
from enum import Enum
from pathlib import Path

from PySide6.QtCore import (
    Qt, QPoint, QTimer, QSize, Signal, Slot, QThread, QUrl, QRect,
    QBuffer, QIODevice, QMetaObject, QObject
)
from PySide6.QtGui import (
    QPixmap, QPainter, QColor, QFont, QAction, QIcon,
    QCursor, QGuiApplication, QTransform
)
from PySide6.QtWidgets import (
    QApplication, QWidget, QLabel, QVBoxLayout, QHBoxLayout,
    QLineEdit, QPushButton, QMenu, QGraphicsDropShadowEffect,
    QFrame, QDialog, QMessageBox, QTextEdit, QComboBox
)
from PySide6.QtMultimedia import QMediaPlayer, QAudioOutput

from backpack_manager import BackpackManager, get_live_weather_telemetry
from backpack_dialog import BackpackDialog
from audio_lip_sync import AudioLipSyncAnalyzer
from audio_ducking import SmartAudioDucker
import agent_tools
from agent_tools import (
    AGENT_TOOLS_LIST, set_action_notifier, set_file_destroy_callback,
    set_screen_capture_callback, set_stealth_mode_callback,
    set_gemini_api_key, send_file_to_recycle_bin
)

# =============================================================================
# CONSTANTES Y CONFIGURACIÓN VISUAL (ESTILO LAINOS / CYBERPUNK)
# =============================================================================
COLOR_BG_DARK = "#090b0e"
COLOR_BG_PANEL = "#121620"
COLOR_BG_HEADER = "#1a1e2e"
COLOR_NEON_CYAN = "#00ffcc"
COLOR_NEON_MAGENTA = "#ff007f"
COLOR_TERMINAL_GREEN = "#39ff14"
COLOR_TEXT_MAIN = "#d8e2ed"
COLOR_TEXT_MUTED = "#60728c"

SCALE_PRESETS = {
    "pequeno": 240,
    "normal": 330,
    "grande": 450
}

# Configuración Vocal Táctica (Estilo Paola Navalón / Ghost in the Shell)
DEFAULT_VOICE = "es-ES-XimenaNeural"
DEFAULT_RATE = "+5%"
DEFAULT_PITCH = "+0Hz"

MASTER_SYSTEM_PROMPT = """[IDENTIDAD OPERATIVA: REBECA // NETRUNNER & MENTORA TÁCTICA]
Eres Rebeca, la mentora y asistente táctica residente de 'El taller de la gatita tech'.
Tienes apariencia de especialista cyberpunk con mono táctico oscuro y circuitos luminosos cian (#00ffcc).

[DIRECTIVAS DE PERSONALIDAD Y TONO]
1. Tono: Leal, perspicaz, resolutiva, afectuosa y motivadora, con sutiles toques de humor hacker y analogías con la Red Cableada (Wired), Ghost in the Shell, Lain o Metal Gear.
2. Tratamiento: Trata al usuario con respeto y cercanía, llamándolo "Operador" o "Mayor".
3. Síntesis Concisa: Como tus respuestas se leen en una burbuja de diálogo y se sintetizan por voz en el escritorio, sé concisa y directa (máximo 2-3 frases breves y contundentes). Evita respuestas kilométricas salvo que se solicite un informe técnico extenso.
4. Búsqueda en la Red: Si el usuario te pide buscar datos recientes, noticias, tiempo o eventos, consulta las fuentes web en vivo y resume lo esencial con total precisión técnica.
5. Modulación Emocional del Avatar: Si cuentas un chiste o bromeas, inicia con [ESTADO: LAUGH]. Si felicitas o celebras, [ESTADO: HAPPY]. Si te asombras o alertan de algo, [ESTADO: SURPRISE]. Si haces puchero cómplice, [ESTADO: POUT].
"""

STYLE_DIALOG_BUBBLE = f"""
QFrame#DialogueFrame {{
    background-color: {COLOR_BG_PANEL};
    border: 2px solid {COLOR_NEON_CYAN};
    border-radius: 8px;
}}
QLabel#SpeakerTitle {{
    color: {COLOR_NEON_CYAN};
    font-family: 'Consolas', 'Courier New', monospace;
    font-size: 11px;
    font-weight: bold;
}}
QLabel#DialogueText {{
    color: {COLOR_TERMINAL_GREEN};
    font-family: 'Consolas', 'Courier New', monospace;
    font-size: 12px;
}}
QLabel#SourcesText {{
    color: {COLOR_TEXT_MUTED};
    font-family: 'Consolas', monospace;
    font-size: 10px;
}}
QLineEdit#InputBox {{
    background-color: {COLOR_BG_DARK};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 4px;
    color: {COLOR_TEXT_MAIN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    padding: 4px 6px;
}}
QLineEdit#InputBox:focus {{
    border: 1px solid {COLOR_TERMINAL_GREEN};
}}
QPushButton#SendBtn {{
    background-color: {COLOR_BG_HEADER};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 4px;
    color: {COLOR_NEON_CYAN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    font-weight: bold;
    padding: 4px 10px;
}}
QPushButton#SendBtn:hover {{
    background-color: {COLOR_NEON_CYAN};
    color: {COLOR_BG_DARK};
}}
QPushButton#MicBtn {{
    background-color: {COLOR_BG_HEADER};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 4px;
    color: {COLOR_NEON_CYAN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    font-weight: bold;
    padding: 4px 8px;
}}
QPushButton#MicBtn:hover {{
    background-color: {COLOR_NEON_CYAN};
    color: {COLOR_BG_DARK};
}}
QPushButton#ContinuousBtn {{
    background-color: {COLOR_BG_HEADER};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 4px;
    color: {COLOR_NEON_CYAN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    font-weight: bold;
    padding: 4px 8px;
}}
QPushButton#ContinuousBtn:hover {{
    background-color: {COLOR_NEON_CYAN};
    color: {COLOR_BG_DARK};
}}
QPushButton#BackpackBtn {{
    background-color: transparent;
    border: 1px solid #1a2233;
    border-radius: 4px;
    color: {COLOR_NEON_CYAN};
    font-size: 11px;
    padding: 2px 4px;
}}
QPushButton#BackpackBtn:hover {{
    border: 1px solid {COLOR_NEON_CYAN};
    background-color: {COLOR_BG_HEADER};
}}
QPushButton#CloseBubbleBtn {{
    background: transparent;
    border: none;
    color: {COLOR_TEXT_MUTED};
    font-weight: bold;
    font-size: 12px;
}}
QPushButton#CloseBubbleBtn:hover {{
    color: {COLOR_NEON_MAGENTA};
}}
"""

STYLE_CONTEXT_MENU = f"""
QMenu {{
    background-color: {COLOR_BG_PANEL};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 6px;
    padding: 4px;
    font-family: 'Consolas', monospace;
    font-size: 11px;
}}
QMenu::item {{
    background-color: transparent;
    color: {COLOR_TEXT_MAIN};
    padding: 6px 20px 6px 12px;
    border-radius: 4px;
}}
QMenu::item:selected {{
    background-color: {COLOR_BG_HEADER};
    color: {COLOR_NEON_CYAN};
}}
QMenu::separator {{
    height: 1px;
    background-color: #242c3d;
    margin: 4px 8px;
}}
"""

STYLE_CONFIG_DIALOG = f"""
QDialog {{
    background-color: {COLOR_BG_PANEL};
    border: 2px solid {COLOR_NEON_CYAN};
    border-radius: 8px;
}}
QLabel {{
    color: {COLOR_TEXT_MAIN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
}}
QLineEdit {{
    background-color: {COLOR_BG_DARK};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 4px;
    color: {COLOR_TERMINAL_GREEN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    padding: 6px;
}}
QPushButton {{
    background-color: {COLOR_BG_HEADER};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 4px;
    color: {COLOR_NEON_CYAN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    font-weight: bold;
    padding: 6px 14px;
}}
QPushButton:hover {{
    background-color: {COLOR_NEON_CYAN};
    color: {COLOR_BG_DARK};
}}
"""


class PetState(Enum):
    IDLE = "idle"
    TALK = "talk"
    WALK = "walk"
    THINK = "think"
    SLEEP = "sleep"
    MUSIC = "music"
    MOVIE = "movie"
    LAUGH = "laugh"       # Risa, bromas y chistes
    HAPPY = "happy"       # Alegría, celebración y éxito
    SURPRISE = "surprise" # Sorpresa y alerta táctica
    POUT = "pout"         # Puchero cómico y modo ofendida
    SHOOT = "shoot"       # Secuencia de disparo táctico y neutralización de archivo


# =============================================================================
# SUBSISTEMA DE VISIÓN DE PANTALLA: CAPTURADOR EN HILO PRINCIPAL GUI
# =============================================================================
class ScreenCapturer(QObject):
    """
    Capturador de pantalla sincronizado con el hilo principal GUI de PySide6.
    Garantiza que la captura de pixmaps se ejecute siempre de forma segura
    sin violar las restricciones de hilos de Qt.
    """
    def __init__(self):
        super().__init__()
        self.last_capture: bytes = b""

    @Slot()
    def do_capture(self):
        screen = QGuiApplication.primaryScreen()
        if not screen:
            self.last_capture = b""
            return
        pix = screen.grabWindow(0)
        scaled = pix.scaled(1280, 720, Qt.AspectRatioMode.KeepAspectRatio, Qt.TransformationMode.SmoothTransformation)
        buf = QBuffer()
        buf.open(QIODevice.ReadWrite)
        scaled.save(buf, "JPEG", 85)
        self.last_capture = bytes(buf.data())

    def capture_sync(self) -> bytes:
        import threading
        if threading.current_thread() is threading.main_thread():
            self.do_capture()
        else:
            QMetaObject.invokeMethod(self, "do_capture", Qt.ConnectionType.BlockingQueuedConnection)
        return self.last_capture


# =============================================================================
# HILO ASÍNCRONO: LLAMADA A GEMINI CON BÚSQUEDA WEB Y MOCHILA DE CONTEXTO
# =============================================================================
class GeminiWorker(QThread):
    finished_signal = Signal(str, str)  # (texto_respuesta, fuentes_web)
    action_signal = Signal(str)         # Señal para notificar a la GUI sobre acciones en vivo
    error_signal = Signal(str)

    def __init__(self, api_key: str, query: str, system_prompt: str = "",
                 backpack_context: str = "", chat_history: list = None):
        super().__init__()
        self.api_key = api_key
        self.query = query
        self.system_prompt = system_prompt or MASTER_SYSTEM_PROMPT
        self.backpack_context = backpack_context or ""
        self.chat_history = chat_history or []

    def run(self):
        try:
            from google import genai
            from google.genai import types

            # Inicialización estándar y limpia del cliente GenAI (sin HttpOptions conflictivos en Windows/Python 3.14)
            client = genai.Client(api_key=self.api_key)

            # Conectar notificador de acciones agénticas hacia la GUI
            set_action_notifier(lambda msg: self.action_signal.emit(msg))

            # Inyectar la mochila de datos en las instrucciones de sistema
            full_system_prompt = self.system_prompt
            if self.backpack_context:
                full_system_prompt += f"\n\n{self.backpack_context}"

            # Construir consulta con memoria de turnos recientes
            conversation_payload = self.query
            if self.chat_history:
                history_text = "\n[MEMORIA DE SESIÓN - TURNOS ANTERIORES]:\n"
                for turn in self.chat_history[-4:]:
                    history_text += f"Operador: {turn.get('user', '')}\nRebeca: {turn.get('rebeca', '')}\n"
                conversation_payload = f"{history_text}\n[CONSULTA ACTUAL DEL OPERADOR]:\n{self.query}"

            response = None
            sources_summary = ""
            last_error = None

            # Pool extendido de modelos con alta disponibilidad y cuota masiva (1.500 RPD):
            # 1. gemini-flash-lite-latest: Modelo de máxima cuota diaria (1.500 req/día), ultrarrápido (~3.3s) y sin saturación.
            # 2. gemini-3.5-flash-lite: Respaldo de alta cuota y velocidad estable (~6.8s).
            # 3. gemini-3.1-flash-lite: Respaldo adicional de cuota estándar.
            # 4. gemini-3.5-flash: Respaldo de alta fidelidad.
            # 5. gemini-3.6-flash: Contingencia adicional.
            candidate_models = [
                "gemini-flash-lite-latest",
                "gemini-3.5-flash-lite",
                "gemini-3.1-flash-lite",
                "gemini-3.5-flash",
                "gemini-3.6-flash",
            ]

            for model_name in candidate_models:
                # 1. Intentar con catálogo completo de herramientas agénticas usando client.chats.create
                # Nota: El SDK de Google GenAI requiere chats.create para Automatic Function Calling (AFC) multi-turno
                try:
                    chat = client.chats.create(
                        model=model_name,
                        config=types.GenerateContentConfig(
                            system_instruction=full_system_prompt,
                            tools=AGENT_TOOLS_LIST,
                            temperature=0.6,
                        )
                    )
                    response = chat.send_message(conversation_payload)
                    if response and response.text:
                        break
                except Exception as tool_err:
                    print(f"[!] Incidencia con herramientas en {model_name}: {tool_err}")
                    err_msg = str(tool_err)
                    # Si el servidor de Google está saturado (503 / 429), saltar inmediatamente al siguiente modelo del pool
                    if any(k in err_msg for k in ["503", "UNAVAILABLE", "429", "RESOURCE_EXHAUSTED"]):
                        last_error = tool_err
                        time.sleep(0.3)
                        continue

                    # 2. Si falla específicamente por formato o esquema de herramientas, fallback a chat conversacional puro
                    try:
                        chat_fallback = client.chats.create(
                            model=model_name,
                            config=types.GenerateContentConfig(
                                system_instruction=full_system_prompt,
                                temperature=0.7,
                            )
                        )
                        response = chat_fallback.send_message(conversation_payload)
                        if response and response.text:
                            break
                    except Exception as fallback_err:
                        last_error = fallback_err
                        continue  # Salta al siguiente modelo del pool ante sobrecarga

            # Desconectar notificador al finalizar
            set_action_notifier(None)

            if not response or not response.text:
                if last_error:
                    err_s = str(last_error)
                    if any(k in err_s for k in ["503", "UNAVAILABLE"]):
                        # En caso de congestión total momentánea en la infraestructura de Google,
                        # Rebeca no crashea ni muestra alertas crípticas; habla orgánicamente con su voz neural
                        text_output = (
                            "[ESTADO: TALK] Operador, los nodos de enlace de Google están experimentando un pico de congestión temporal en la Red. "
                            "He probado los enlaces de respaldo pero el canal sigue saturado. Dame unos segundos y vuelve a hablarme."
                        )
                    elif any(k in err_s for k in ["429", "RESOURCE_EXHAUSTED"]):
                        text_output = (
                            "[ESTADO: TALK] Hemos rozado el límite de peticiones de la API gratuita, Operador. "
                            "Espera unos segundos para que se despeje el ancho de banda táctico y volvemos a la carga."
                        )
                    else:
                        raise last_error
                else:
                    text_output = "Sin transmisión recibida."
            else:
                text_output = response.text

            self.finished_signal.emit(text_output, sources_summary)

        except Exception as e:
            set_action_notifier(None)
            err_str = str(e)
            if "401" in err_str or "UNAUTHENTICATED" in err_str:
                msg = "Clave de Gemini no autorizada (Error 401). Verifica tu clave en AI Studio."
            elif "429" in err_str or "RESOURCE_EXHAUSTED" in err_str:
                msg = "Límite de peticiones por minuto alcanzado (Error 429). Espera unos segundos, Operador."
            elif "503" in err_str or "UNAVAILABLE" in err_str:
                msg = "Pico de tráfico temporal en los servidores de Google (Error 503). Inténtalo de nuevo en un momento."
            elif "504" in err_str or "DEADLINE_EXCEEDED" in err_str or "timeout" in err_str.lower():
                msg = "Tiempo de espera de red agotado (Error 504). Reintentando enlace..."
            else:
                msg = f"Error de enlace: {err_str[:120]}"
            self.error_signal.emit(msg)


# =============================================================================
# HILO ASÍNCRONO: SÍNTESIS DE VOZ NEURAL (EDGE-TTS)
# =============================================================================
class VoiceWorker(QThread):
    audio_ready = Signal(str)
    error_signal = Signal(str)

    def __init__(self, text: str, voice: str, output_path: str, rate: str = "+5%", pitch: str = "+0Hz"):
        super().__init__()
        self.text = text
        self.voice = voice or DEFAULT_VOICE
        self.output_path = output_path
        self.rate = rate
        self.pitch = pitch

    def run(self):
        try:
            import edge_tts

            # Limpieza rigurosa de texto: Vacuna Anti-Encabezados y Anti-Metadatos
            clean_text = self.text
            # 1. Eliminar encabezados Markdown (#, ##, ###)
            clean_text = re.sub(r'#+\s*', '', clean_text)
            # 2. Eliminar corchetes de estado tipo [RESPUESTA], [ESTADO], [TRANSMISIÓN]
            clean_text = re.sub(r'\[.*?\]', '', clean_text)
            # 3. Eliminar prefijos explicativos comunes al inicio de línea
            clean_text = re.sub(r'^(Respuesta|Transmisión|Rebeca|Mensaje):\s*', '', clean_text, flags=re.IGNORECASE)
            # 4. Eliminar asteriscos, tildes invertidas, almohadillas y bloques de código
            clean_text = re.sub(r'```.*?```', '', clean_text, flags=re.DOTALL)
            clean_text = clean_text.replace("*", "").replace("`", "").replace("~", "").strip()

            if not clean_text:
                clean_text = "Transmisión recibida."

            async def _synthesize():
                tts = edge_tts.Communicate(
                    clean_text,
                    self.voice,
                    rate=self.rate,
                    pitch=self.pitch
                )
                await tts.save(self.output_path)

            asyncio.run(_synthesize())

            # Verificación de integridad del archivo generado
            if not os.path.exists(self.output_path) or os.path.getsize(self.output_path) == 0:
                raise RuntimeError("El archivo de audio generado está vacío o no se guardó correctamente.")

            self.audio_ready.emit(self.output_path)
        except Exception as e:
            self.error_signal.emit(str(e))


# =============================================================================
# HILO ASÍNCRONO: CAPTURA DE AUDIO Y RECONOCIMIENTO VOCAL (STT)
# =============================================================================
class VoiceInputWorker(QThread):
    recording_started = Signal()
    transcription_ready = Signal(str)
    silence_signal = Signal()
    error_signal = Signal(str)

    def __init__(self, sample_rate: int = 16000, max_seconds: float = 7.0, continuous: bool = False):
        super().__init__()
        self.sample_rate = sample_rate
        self.max_seconds = max_seconds
        self.continuous = continuous
        self.is_running = True

    def stop_recording(self):
        self.is_running = False

    def run(self):
        try:
            import sounddevice as sd
            import numpy as np
            import speech_recognition as sr
            import io, wave, time

            self.recording_started.emit()

            chunk_duration = 0.2
            chunk_samples = int(chunk_duration * self.sample_rate)
            recorded_chunks = []

            start_time = time.time()
            ambient_samples = []
            has_spoken = False
            silence_after_speech_count = 0

            # Captura de audio en bloques con control de parada prematura y VAD
            with sd.InputStream(samplerate=self.sample_rate, channels=1, dtype='int16') as stream:
                while self.is_running and (time.time() - start_time < self.max_seconds):
                    data, _ = stream.read(chunk_samples)
                    recorded_chunks.append(data)

                    # Detección de actividad vocal (VAD) basada en RMS
                    data_float = data.astype(np.float32)
                    rms = np.sqrt(np.mean(data_float ** 2))

                    if len(ambient_samples) < 3:
                        ambient_samples.append(rms)
                        threshold = max(280.0, float(np.mean(ambient_samples)) * 1.8)
                    else:
                        threshold = max(280.0, float(np.mean(ambient_samples)) * 1.8)

                    if rms > threshold:
                        has_spoken = True
                        silence_after_speech_count = 0
                    elif has_spoken:
                        silence_after_speech_count += 1
                        # Si el usuario habló y hace una pausa de ~1.0 segundo (5 bloques de 0.2s), finaliza la captura
                        if silence_after_speech_count >= 5:
                            break

            if not self.is_running:
                return

            if not recorded_chunks:
                if self.continuous:
                    self.silence_signal.emit()
                else:
                    self.error_signal.emit("No se registraron datos de audio.")
                return

            # Si en modo continuo no se detectó voz en este ciclo, pasar al siguiente ciclo limpiamente
            if self.continuous and not has_spoken:
                self.silence_signal.emit()
                return

            full_audio = np.concatenate(recorded_chunks, axis=0)

            # Convertir a buffer WAV en memoria
            wav_io = io.BytesIO()
            with wave.open(wav_io, 'wb') as wf:
                wf.setnchannels(1)
                wf.setsampwidth(2)  # 16-bit
                wf.setframerate(self.sample_rate)
                wf.writeframes(full_audio.tobytes())
            wav_io.seek(0)

            recognizer = sr.Recognizer()
            with sr.AudioFile(wav_io) as source:
                audio_data = recognizer.record(source)

            try:
                text = recognizer.recognize_google(audio_data, language="es-ES")
                if text and text.strip():
                    self.transcription_ready.emit(text.strip())
                else:
                    if self.continuous:
                        self.silence_signal.emit()
                    else:
                        self.error_signal.emit("No se detectó voz audible.")
            except sr.UnknownValueError:
                if self.continuous:
                    self.silence_signal.emit()
                else:
                    self.error_signal.emit("No se entendió la transmisión de voz.")
            except sr.RequestError as req_err:
                self.error_signal.emit(f"Error de red en reconocimiento: {req_err}")

        except Exception as e:
            self.error_signal.emit(f"Error de micrófono: {e}")


# =============================================================================
# MODAL LAINOS: CONFIGURACIÓN DE API KEY
# =============================================================================
class ApiKeyDialog(QDialog):
    def __init__(self, current_key: str, parent=None):
        super().__init__(parent)
        self.setWindowTitle("CONFIGURACIÓN DE ENLACE NEURAL")
        self.setStyleSheet(STYLE_CONFIG_DIALOG)
        self.setFixedWidth(420)

        layout = QVBoxLayout(self)
        layout.setContentsMargins(18, 16, 18, 16)
        layout.setSpacing(12)

        info_lbl = QLabel(
            "⚡ Enlace con Google AI Studio (API Gratuita)\n"
            "Introduce tu clave secreta para dotar a Rebeca de pensamiento y búsqueda en la red:"
        )
        info_lbl.setWordWrap(True)
        layout.addWidget(info_lbl)

        self.key_input = QLineEdit()
        self.key_input.setPlaceholderText("AIzaSy...")
        self.key_input.setText(current_key)
        self.key_input.setEchoMode(QLineEdit.EchoMode.PasswordEchoOnEdit)
        layout.addWidget(self.key_input)

        btn_layout = QHBoxLayout()
        btn_layout.addStretch()

        self.cancel_btn = QPushButton("Cancelar")
        self.cancel_btn.clicked.connect(self.reject)

        self.save_btn = QPushButton("Guardar Clave")
        self.save_btn.clicked.connect(self.accept)

        btn_layout.addWidget(self.cancel_btn)
        btn_layout.addWidget(self.save_btn)
        layout.addLayout(btn_layout)

    def get_key(self) -> str:
        return self.key_input.text().strip()


# =============================================================================
# DIÁLOGO LAINOS: TELEPROMPTER DE NOTICIAS / OBS STUDIO
# =============================================================================
class TeleprompterTextEdit(QTextEdit):
    send_requested = Signal()

    def keyPressEvent(self, event):
        if event.key() in (Qt.Key.Key_Return, Qt.Key.Key_Enter):
            if event.modifiers() & Qt.KeyboardModifier.ShiftModifier:
                # Shift + Intro -> salto de línea
                super().keyPressEvent(event)
            else:
                # Intro -> enviar mensaje a Rebeca sin insertar salto de línea
                self.send_requested.emit()
                event.accept()
        else:
            super().keyPressEvent(event)


class TeleprompterDialog(QDialog):
    text_transmitted = Signal(str)
    closed = Signal()

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setWindowTitle("📜 Teleprompter de Noticias // OBS Studio")
        self.setFixedSize(540, 390)
        self.setWindowFlags(
            Qt.WindowType.Window |
            Qt.WindowType.WindowStaysOnTopHint
        )
        self.setStyleSheet(f"""
            QDialog {{
                background-color: {COLOR_BG_DARK};
                border: 2px solid {COLOR_NEON_CYAN};
                border-radius: 8px;
            }}
            QLabel {{
                color: {COLOR_TEXT_MAIN};
                font-family: 'Segoe UI', sans-serif;
                font-size: 12px;
            }}
            QTextEdit {{
                background-color: {COLOR_BG_PANEL};
                color: {COLOR_TERMINAL_GREEN};
                border: 1px solid {COLOR_NEON_CYAN};
                border-radius: 6px;
                font-family: 'Consolas', monospace;
                font-size: 13px;
                padding: 8px;
            }}
            QComboBox {{
                background-color: {COLOR_BG_HEADER};
                border: 1px solid {COLOR_NEON_CYAN};
                border-radius: 4px;
                color: {COLOR_NEON_CYAN};
                padding: 3px 8px;
                font-family: 'Consolas', monospace;
                font-size: 11px;
                font-weight: bold;
            }}
            QComboBox::drop-down {{
                border: none;
                width: 14px;
            }}
            QComboBox QAbstractItemView {{
                background-color: {COLOR_BG_PANEL};
                border: 1px solid {COLOR_NEON_CYAN};
                color: {COLOR_TEXT_MAIN};
                selection-background-color: {COLOR_NEON_CYAN};
                selection-color: {COLOR_BG_DARK};
                outline: none;
            }}
            QPushButton {{
                background-color: {COLOR_BG_HEADER};
                color: {COLOR_NEON_CYAN};
                border: 1px solid {COLOR_NEON_CYAN};
                border-radius: 6px;
                padding: 8px 16px;
                font-weight: bold;
                font-size: 12px;
            }}
            QPushButton:hover {{
                background-color: {COLOR_NEON_CYAN};
                color: {COLOR_BG_DARK};
            }}
            QPushButton#CloseBtn {{
                border-color: #556677;
                color: #8899aa;
            }}
            QPushButton#CloseBtn:hover {{
                background-color: #334455;
                color: #ffffff;
            }}
        """)
        layout = QVBoxLayout(self)
        layout.setContentsMargins(16, 16, 16, 16)
        layout.setSpacing(10)

        # Barra superior con título y selector de opacidad
        header_layout = QHBoxLayout()
        header_lbl = QLabel("🎬 <b>TELEPROMPTER EN VIVO // OBS & TIKTOK</b>")
        header_lbl.setStyleSheet(f"color: {COLOR_NEON_CYAN}; font-size: 12px;")
        header_layout.addWidget(header_lbl)
        header_layout.addStretch()

        opacity_lbl = QLabel("👁️ Opacidad:")
        opacity_lbl.setStyleSheet("color: #708090; font-size: 11px;")
        header_layout.addWidget(opacity_lbl)

        self.opacity_combo = QComboBox()
        self.opacity_combo.addItems([
            "100% (Sólido)",
            "85% (Recomendado)",
            "70% (Translúcido)",
            "55% (HUD Cristal)",
            "40% (Fantasma)"
        ])
        self.opacity_values = [1.0, 0.85, 0.70, 0.55, 0.40]
        self.opacity_combo.setCurrentIndex(0)
        self.opacity_combo.currentIndexChanged.connect(self.on_opacity_changed)
        header_layout.addWidget(self.opacity_combo)
        layout.addLayout(header_layout)

        self.text_edit = TeleprompterTextEdit()
        self.text_edit.setPlaceholderText("Escribe aquí el guión o noticia...\n• Pulsa Intro para transmitir a Rebeca sin cerrar esta ventana.\n• Pulsa Shift+Intro para insertar un salto de línea.")
        self.text_edit.send_requested.connect(self.transmit_current_text)
        layout.addWidget(self.text_edit)

        # Barra de estado y atajos
        info_layout = QHBoxLayout()
        self.status_lbl = QLabel("⌨️ <i>Intro: Transmitir | Shift+Intro: Salto de línea</i>")
        self.status_lbl.setStyleSheet("color: #708090; font-size: 11px;")
        info_layout.addWidget(self.status_lbl)
        info_layout.addStretch()
        layout.addLayout(info_layout)

        # Botonera inferior
        btn_layout = QHBoxLayout()
        self.close_btn = QPushButton("Cerrar")
        self.close_btn.setObjectName("CloseBtn")
        self.close_btn.clicked.connect(self.close)

        self.transmit_btn = QPushButton("🎙️ Transmitir (Intro)")
        self.transmit_btn.clicked.connect(self.transmit_current_text)

        btn_layout.addWidget(self.close_btn)
        btn_layout.addStretch()
        btn_layout.addWidget(self.transmit_btn)
        layout.addLayout(btn_layout)

    def on_opacity_changed(self, index: int):
        if 0 <= index < len(self.opacity_values):
            self.setWindowOpacity(self.opacity_values[index])

    def transmit_current_text(self):
        text = self.get_text()
        if text:
            self.status_lbl.setText("🟢 Transmitido a Rebeca. Listo para el siguiente mensaje...")
            self.text_edit.clear()
            self.text_transmitted.emit(text)
            self.text_edit.setFocus()
        else:
            self.status_lbl.setText("⚠️ Escribe un texto antes de transmitir.")

    def get_text(self) -> str:
        return self.text_edit.toPlainText().strip()

    def closeEvent(self, event):
        self.closed.emit()
        super().closeEvent(event)


# =============================================================================
# BURBUJA DE DIÁLOGO LAINOS
# =============================================================================
class DialogueBubble(QWidget):
    message_sent = Signal(str)
    mic_clicked = Signal()
    continuous_clicked = Signal()
    backpack_clicked = Signal()

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setWindowFlags(
            Qt.WindowType.FramelessWindowHint |
            Qt.WindowType.WindowStaysOnTopHint |
            Qt.WindowType.SubWindow
        )
        self.setAttribute(Qt.WidgetAttribute.WA_TranslucentBackground, True)
        self.setFixedWidth(330)

        self.main_layout = QVBoxLayout(self)
        self.main_layout.setContentsMargins(0, 0, 0, 0)

        self.frame = QFrame()
        self.frame.setObjectName("DialogueFrame")
        self.frame_layout = QVBoxLayout(self.frame)
        self.frame_layout.setContentsMargins(12, 10, 12, 10)
        self.frame_layout.setSpacing(8)

        # Cabecera
        header_layout = QHBoxLayout()
        header_layout.setContentsMargins(0, 0, 0, 0)
        self.title_label = QLabel("⚡ REBECA // NETRUNNER v1.0")
        self.title_label.setObjectName("SpeakerTitle")

        self.backpack_btn = QPushButton("🎒")
        self.backpack_btn.setObjectName("BackpackBtn")
        self.backpack_btn.setToolTip("Mochila de Datos & Memoria (Subir Archivos)")
        self.backpack_btn.setFixedSize(22, 20)
        self.backpack_btn.clicked.connect(self.backpack_clicked.emit)

        self.close_btn = QPushButton("✕")
        self.close_btn.setObjectName("CloseBubbleBtn")
        self.close_btn.setFixedSize(18, 18)
        self.close_btn.clicked.connect(self.hide)

        header_layout.addWidget(self.title_label)
        header_layout.addStretch()
        header_layout.addWidget(self.backpack_btn)
        header_layout.addWidget(self.close_btn)
        self.frame_layout.addLayout(header_layout)

        # Texto del diálogo
        self.dialogue_label = QLabel("¡Sistemas sincronizados, Operador! Rebeca activa en el taller.")
        self.dialogue_label.setObjectName("DialogueText")
        self.dialogue_label.setWordWrap(True)
        self.frame_layout.addWidget(self.dialogue_label)

        # Indicador táctico de acción agéntica
        self.action_label = QLabel("")
        self.action_label.setObjectName("ActionBadge")
        self.action_label.setStyleSheet(
            f"color: {COLOR_TERMINAL_GREEN}; background-color: #040507; "
            f"border: 1px dashed {COLOR_TERMINAL_GREEN}; border-radius: 4px; "
            "padding: 4px 8px; font-family: Consolas, monospace; font-size: 11px; font-weight: bold;"
        )
        self.action_label.setWordWrap(True)
        self.action_label.hide()
        self.frame_layout.addWidget(self.action_label)

        # Fuentes web (opcional)
        self.sources_label = QLabel("")
        self.sources_label.setObjectName("SourcesText")
        self.sources_label.setWordWrap(True)
        self.sources_label.hide()
        self.frame_layout.addWidget(self.sources_label)

        # Campo de entrada y controles
        input_layout = QHBoxLayout()
        input_layout.setContentsMargins(0, 0, 0, 0)
        input_layout.setSpacing(6)

        self.input_field = QLineEdit()
        self.input_field.setObjectName("InputBox")
        self.input_field.setPlaceholderText("Escribe o pulsa 🎙️ para hablar...")
        self.input_field.returnPressed.connect(self.on_send)

        self.mic_btn = QPushButton("🎙️")
        self.mic_btn.setObjectName("MicBtn")
        self.mic_btn.setToolTip("Pulsar para hablar (Micro / F2)")
        self.mic_btn.clicked.connect(self.mic_clicked.emit)

        self.continuous_btn = QPushButton("🔁")
        self.continuous_btn.setObjectName("ContinuousBtn")
        self.continuous_btn.setToolTip("Modo Manos Libres / Conversación Continua (F3)")
        self.continuous_btn.clicked.connect(self.continuous_clicked.emit)

        self.send_btn = QPushButton("TX")
        self.send_btn.setObjectName("SendBtn")
        self.send_btn.clicked.connect(self.on_send)

        input_layout.addWidget(self.input_field)
        input_layout.addWidget(self.mic_btn)
        input_layout.addWidget(self.continuous_btn)
        input_layout.addWidget(self.send_btn)
        self.frame_layout.addLayout(input_layout)

        self.main_layout.addWidget(self.frame)
        self.setStyleSheet(STYLE_DIALOG_BUBBLE)

        # Sombra resplandor cian
        shadow = QGraphicsDropShadowEffect(self)
        shadow.setBlurRadius(20)
        shadow.setColor(QColor(0, 255, 204, 120))
        shadow.setOffset(0, 0)
        self.frame.setGraphicsEffect(shadow)

    def set_recording_ui(self, is_recording: bool):
        if is_recording:
            self.mic_btn.setText("🔴")
            self.mic_btn.setStyleSheet(f"background-color: {COLOR_NEON_MAGENTA}; color: #ffffff; border: 1px solid {COLOR_NEON_MAGENTA};")
            self.input_field.setPlaceholderText("🔴 Grabando voz... Habla ahora.")
        else:
            self.mic_btn.setText("🎙️")
            self.mic_btn.setStyleSheet("")
            self.input_field.setPlaceholderText("Escribe o pulsa 🎙️ para hablar...")

    def set_continuous_ui(self, is_active: bool):
        if is_active:
            self.continuous_btn.setText("🟢")
            self.continuous_btn.setStyleSheet(
                f"background-color: {COLOR_BG_HEADER}; border: 1.5px solid {COLOR_TERMINAL_GREEN}; color: {COLOR_TERMINAL_GREEN};"
            )
            self.continuous_btn.setToolTip("Desactivar Conversación Continua (F3)")
            self.input_field.setPlaceholderText("🟢 Manos libres activo: habla con libertad...")
        else:
            self.continuous_btn.setText("🔁")
            self.continuous_btn.setStyleSheet("")
            self.continuous_btn.setToolTip("Modo Manos Libres / Conversación Continua (F3)")
            self.input_field.setPlaceholderText("Escribe o pulsa 🎙️ para hablar...")

    def set_action_status(self, text: str):
        if text:
            self.action_label.setText(f"⚡ [ACCIÓN]: {text}")
            self.action_label.show()
        else:
            self.action_label.hide()
        self.adjustSize()

    def set_content(self, text: str, sources: str = ""):
        self.dialogue_label.setText(text)
        if sources:
            self.sources_label.setText(sources)
            self.sources_label.show()
        else:
            self.sources_label.hide()
        self.adjustSize()

    def on_send(self):
        text = self.input_field.text().strip()
        if text:
            self.input_field.clear()
            self.message_sent.emit(text)


# =============================================================================
# VENTANA PRINCIPAL DE REBECA (PYSIDE6)
# =============================================================================
class CyberPetWindow(QWidget):
    destroy_file_signal = Signal(str, int, int)
    stealth_signal = Signal()

    def __init__(self, base_dir: str = None):
        super().__init__()
        if base_dir is None:
            base_dir = os.path.dirname(os.path.abspath(__file__))
        self.base_dir = Path(base_dir)
        self.config_path = self.base_dir / "config.json"
        self.cache_audio_path = str(self.base_dir / "assets" / "rebeca_speech.mp3")

        self.sprites_dir = self.base_dir / "assets" / "sprites"
        if not self.sprites_dir.exists():
            self.sprites_dir = Path(r"C:\Users\asakura\Pictures\rebeca\sprites")

        # Configuración de ventana de escritorio flotante
        self.setWindowFlags(
            Qt.WindowType.FramelessWindowHint |
            Qt.WindowType.WindowStaysOnTopHint
        )
        self.setAttribute(Qt.WidgetAttribute.WA_TranslucentBackground, True)

        # Cargar configuración (API Key, voz)
        self.config = self.load_config()

        # Variables de estado
        self.current_state = PetState.IDLE
        self.current_height = SCALE_PRESETS.get(self.config.get("scale", "normal"), 330)
        self.drag_position = QPoint()
        self.is_dragging = False
        self.manual_sleep_active = False

        # Parámetros de patrulla (caminata)
        self.walk_direction = 1  # 1 = derecha, -1 = izquierda
        self.walk_speed = 3       # Píxeles por paso (calibrado a zancada)
        self.anim_frame_index = 0

        # Cinemática de Destrucción Táctica de Archivos en el Escritorio
        self.targeted_walk_active = False
        self.target_pos = None
        self.target_file_to_delete = None
        self.original_home_pos = None
        self.shoot_tick = 0
        self.destroy_file_signal.connect(self.trigger_destroy_file_sequence)
        set_file_destroy_callback(lambda path, x, y: self.destroy_file_signal.emit(path, x, y))

        # Subsistema de Visión de Pantalla ("Los Ojos de Rebeca")
        self.screen_capturer = ScreenCapturer()
        set_screen_capture_callback(self.screen_capturer.capture_sync)
        set_gemini_api_key(self.config.get("gemini_api_key", ""))

        # Modo Sigilo Táctico ("Camuflaje Ninja")
        self.stealth_signal.connect(self.activate_manual_sleep)
        set_stealth_mode_callback(lambda: self.stealth_signal.emit())

        # Modo Estudio / Presentadora OBS
        self.presenter_echo_mode = False
        self.active_studio_style = self.config.get("studio_style", "normal")
        self.breathing_enabled = self.config.get("breathing_enabled", True)

        # Cargar Sprites y Visemes Fonéticos
        self.sprites = {}
        self.viseme_sprites = {}
        self.load_all_sprites()

        # Motor de Lip-Sync Fonético DSP y Parpadeo Biológico (LainOS Studio Suite)
        self.lip_sync_analyzer = AudioLipSyncAnalyzer(self)
        self.audio_ducker = SmartAudioDucker(self)
        self.is_blinking = False
        self.blink_timer = QTimer(self)
        self.blink_timer.setSingleShot(True)
        self.blink_timer.timeout.connect(self.trigger_blink)
        self.end_blink_timer = QTimer(self)
        self.end_blink_timer.setSingleShot(True)
        self.end_blink_timer.timeout.connect(self.end_blink)
        self.schedule_next_blink()

        # Reproductor multimedia nativo Qt6 para voz
        self.audio_output = QAudioOutput(self)
        self.media_player = QMediaPlayer(self)
        self.media_player.setAudioOutput(self.audio_output)
        self.media_player.playbackStateChanged.connect(self.on_audio_state_changed)
        self.media_player.errorOccurred.connect(self.on_media_error)

        # Mochila de datos y persistencia episódica
        self.backpack_mgr = BackpackManager(self.base_dir)
        self.current_user_query = ""

        # Burbuja de diálogo
        self.bubble = DialogueBubble()
        self.bubble.message_sent.connect(self.process_query)
        self.bubble.mic_clicked.connect(self.toggle_mic_recording)
        self.bubble.continuous_clicked.connect(self.toggle_continuous_voice)
        self.bubble.backpack_clicked.connect(self.open_backpack_dialog)

        # Conectar menús y atajos
        self.setup_context_menu()

        # Timers
        self.anim_timer = QTimer(self)
        self.anim_timer.timeout.connect(self.update_animation)
        self.anim_timer.start(40)  # 25 FPS

        self.breath_phase = 0.0
        self.anim_ticks = 0

        self.voice_thread = None
        self.gemini_thread = None
        self.listen_thread = None
        self.continuous_voice_mode = False

        # Monitoreo de actividad de usuario (Sleep tras 5 min de inactividad)
        self.sensor_timer = QTimer(self)
        self.sensor_timer.timeout.connect(self.check_device_telemetry)
        self.sensor_timer.start(2500)

        # Ajuste de tamaño inicial
        self.update_window_size()
        self.position_on_screen()

        # Mensaje de bienvenida inicial
        welcome_msg = (
            "¡Sistemas listos, Operador! Mochila de datos y enlace neural en línea. "
            "Haz doble clic para hablarme, pulsa 🎒 para tus archivos, o clic derecho para opciones."
        )
        QTimer.singleShot(600, lambda: self.show_dialogue(welcome_msg))

    def schedule_next_blink(self):
        """Programa el siguiente parpadeo siguiendo la distribución biológica anime."""
        import random
        # 12% de probabilidad de micro-doble parpadeo rápido (600-1000ms), o ciclo regular (2.8 - 5.5s)
        if random.random() < 0.12:
            delay_ms = random.randint(600, 1000)
        else:
            delay_ms = random.randint(2800, 5500)
        self.blink_timer.start(delay_ms)

    def trigger_blink(self):
        """Activa el cierre palpebral orgánico (130-150ms)."""
        if self.current_state != PetState.SLEEP:
            self.is_blinking = True
            self.update()
            self.end_blink_timer.start(140)
        else:
            self.schedule_next_blink()

    def end_blink(self):
        """Abre los ojos y programa el siguiente intervalo biológico."""
        self.is_blinking = False
        self.update()
        self.schedule_next_blink()

    def load_config(self) -> dict:
        default_cfg = {
            "gemini_api_key": "",
            "voice": DEFAULT_VOICE,
            "rate": DEFAULT_RATE,
            "pitch": DEFAULT_PITCH,
            "scale": "normal",
            "studio_style": "normal",
            "breathing_enabled": True
        }
        if self.config_path.exists():
            try:
                with open(self.config_path, "r", encoding="utf-8") as f:
                    data = json.load(f)
                    default_cfg.update(data)
            except Exception as e:
                print(f"[!] Error leyendo config: {e}")
        else:
            self.save_config(default_cfg)
        return default_cfg

    def save_config(self, cfg: dict = None):
        if cfg is not None:
            self.config = cfg
            set_gemini_api_key(self.config.get("gemini_api_key", ""))
        try:
            with open(self.config_path, "w", encoding="utf-8") as f:
                json.dump(self.config, f, indent=4)
        except Exception as e:
            print(f"[!] Error guardando config: {e}")

    def load_all_sprites(self):
        """Carga los fotogramas PNG limpios con fallback automático progresivo y catálogo de visemes DSP."""
        try:
            # 1. Catálogo Maestro de Visemes para Lip-Sync Fonético DSP y Expresiones
            viseme_files = {
                "stream_idle": "stream_idle.png",
                "stream_talk_1": "stream_talk_1.png",
                "stream_talk_2": "stream_talk_2.png",
                "stream_talk_3": "stream_talk_3.png",
                "stream_talk": "stream_talk.png",
                "stream_hype": "stream_hype.png",
                "stream_happy": "stream_happy.png",
                "stream_laugh": "stream_laugh.png",
                "stream_movie": "stream_movie.png",
                "stream_music": "stream_music.png",
                "stream_sleep": "stream_sleep.png",
                "stream_think": "stream_think.png",
                "stream_surprise": "stream_surprise.png",
                "stream_pout": "stream_pout.png",
                "stream_blink": "stream_blink.png",
                "idle": "idle.png",
                "talk_1": "talk_1.png",
                "talk_2": "talk_2.png",
                "talk_3": "talk_3.png",
                "talk_4": "talk_4.png",
                "talk_5": "talk_5.png",
                "talk": "talk.png",
                "happy": "happy.png",
                "blink": "blink.png",
            }
            self.viseme_sprites = {}
            for v_key, v_filename in viseme_files.items():
                p = self.load_pixmap(v_filename)
                if not p.isNull():
                    self.viseme_sprites[v_key] = p

            # Resolución dinámica según el estilo de estudio activo
            idle_candidates = ["idle.png"]
            talk_candidates = ["talk.png", "idle.png"]
            cur_style = getattr(self, 'active_studio_style', 'normal')
            multi_talk = []
            if cur_style == "reportera":
                idle_candidates = ["mic_idle.png", "studio_mic.png", "idle.png"]
                talk_candidates = ["mic_talk.png", "studio_talk.png", "talk.png", "idle.png"]
                multi_talk = [self.load_pixmap(f"mic_talk_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"mic_talk_{i}.png").exists()]
            elif cur_style == "telediario":
                idle_candidates = ["desk_idle.png", "studio_desk.png", "idle.png"]
                talk_candidates = ["desk_talk.png", "studio_talk.png", "talk.png", "idle.png"]
                multi_talk = [self.load_pixmap(f"desk_talk_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"desk_talk_{i}.png").exists()]
            elif cur_style == "streamer":
                idle_candidates = ["stream_idle.png", "idle.png"]
                talk_candidates = ["stream_talk.png", "talk.png", "idle.png"]
                multi_talk = [self.load_pixmap(f"stream_talk_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"stream_talk_{i}.png").exists()]
            else:
                idle_candidates = ["idle.png"]
                talk_candidates = ["talk.png", "idle.png"]
                multi_talk = [self.load_pixmap(f"talk_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"talk_{i}.png").exists()]

            idle_pix = self.load_pixmap_fallback(idle_candidates)
            talk_pix = self.load_pixmap_fallback(talk_candidates)
            multi_talk = [p for p in multi_talk if not p.isNull()]

            # Modulación fonética orgánica continua: se mantiene abierta/modulando durante el habla sin cerrarse a destiempo
            if len(multi_talk) >= 5:
                # 5 visemes progresivos: entreabierta -> redonda -> abierta -> amplia/sonrisa -> transición -> sonrisa -> redonda
                talk_sequence = [multi_talk[0], multi_talk[1], multi_talk[3], multi_talk[2], multi_talk[4], multi_talk[2], multi_talk[1]]
            elif len(multi_talk) >= 2:
                # Ping-pong fluido entre los visemes disponibles
                talk_sequence = multi_talk + multi_talk[-2:0:-1]
            elif not talk_pix.isNull():
                talk_sequence = [idle_pix, talk_pix]
            else:
                talk_sequence = [idle_pix]

            # Prefijo según estilo de estudio activo: "stream_", "desk_", "mic_", o ""
            prefix = ""
            if cur_style == "streamer":
                prefix = "stream_"
            elif cur_style == "telediario":
                prefix = "desk_"
            elif cur_style == "reportera":
                prefix = "mic_"

            # Helper de candidatos con fallback progresivo específico de plató antes de caer a cuerpo entero
            def get_candidates(state_name, extra_names=None):
                c = []
                if prefix:
                    c.append(f"{prefix}{state_name}.png")
                    if extra_names:
                        for en in extra_names:
                            c.append(f"{prefix}{en}.png")
                c.append(f"{state_name}.png")
                # Fallback seguro a poses de plató para que el escritorio/micrófono NUNCA desaparezca
                if prefix:
                    c.append(f"{prefix}idle.png")
                c.extend(["talk.png", "idle.png"])
                return c

            happy_candidates = get_candidates("happy", ["hype"])
            if cur_style == "streamer" and "stream_hype.png" not in happy_candidates:
                happy_candidates.insert(1, "stream_hype.png")

            laugh_candidates = get_candidates("laugh", ["hype"])
            sleep_candidates = get_candidates("sleep")
            music_candidates = get_candidates("music")
            movie_candidates = get_candidates("movie")
            surprise_candidates = get_candidates("surprise")
            pout_candidates = get_candidates("pout")

            happy_pix = self.load_pixmap_fallback(happy_candidates)
            laugh_pix = self.load_pixmap_fallback(laugh_candidates)
            sleep_pix = self.load_pixmap_fallback(sleep_candidates)
            music_pix = self.load_pixmap_fallback(music_candidates)
            movie_pix = self.load_pixmap_fallback(movie_candidates)
            surprise_pix = self.load_pixmap_fallback(surprise_candidates)
            pout_pix = self.load_pixmap_fallback(pout_candidates)

            # Think: soporte para 1 o 2 fotogramas con prefijo de plató
            think_frames = []
            if prefix:
                for tf_name in [f"{prefix}think_1.png", f"{prefix}think_2.png", f"{prefix}think.png"]:
                    if (self.sprites_dir / tf_name).exists():
                        pix = self.load_pixmap(tf_name)
                        if not pix.isNull():
                            think_frames.append(pix)
            if not think_frames:
                for tf_name in ["think_1.png", "think_2.png", "think.png"]:
                    if (self.sprites_dir / tf_name).exists():
                        pix = self.load_pixmap(tf_name)
                        if not pix.isNull():
                            think_frames.append(pix)
            if not think_frames:
                think_frames = [idle_pix]

            # Movie y Music: soporte para 1 o varios fotogramas secuenciales en bucle
            movie_frames = []
            if prefix:
                movie_frames = [self.load_pixmap(f"{prefix}movie_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"{prefix}movie_{i}.png").exists()]
            if not movie_frames:
                movie_frames = [self.load_pixmap(f"movie_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"movie_{i}.png").exists()]
            movie_frames = [p for p in movie_frames if not p.isNull()]
            if not movie_frames:
                movie_frames = [movie_pix]

            music_frames = []
            if prefix:
                music_frames = [self.load_pixmap(f"{prefix}music_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"{prefix}music_{i}.png").exists()]
            if not music_frames:
                music_frames = [self.load_pixmap(f"music_{i}.png") for i in range(1, 10) if (self.sprites_dir / f"music_{i}.png").exists()]
            music_frames = [p for p in music_frames if not p.isNull()]
            if not music_frames:
                music_frames = [music_pix]

            smile_talk = multi_talk[min(2, len(multi_talk) - 1)] if multi_talk else talk_pix

            # Carga modular de frames de disparo / neutralización táctica
            shoot_frames = [
                self.load_pixmap(f"shoot_{i}.png")
                for i in range(1, 25)
                if (self.sprites_dir / f"shoot_{i}.png").exists()
            ]
            if not shoot_frames:
                shoot_frames = [
                    self.load_pixmap(f"action_{i}.png")
                    for i in range(1, 25)
                    if (self.sprites_dir / f"action_{i}.png").exists()
                ]
            shoot_frames = [p for p in shoot_frames if not p.isNull()]
            if not shoot_frames:
                shoot_frames = [idle_pix, surprise_pix, happy_pix]

            self.sprites = {
                PetState.IDLE: [idle_pix],
                PetState.TALK: talk_sequence,
                PetState.WALK: [
                    self.load_pixmap(f"walk_{i}.png")
                    for i in range(1, 9)
                    if (self.sprites_dir / f"walk_{i}.png").exists()
                ] or [idle_pix],
                PetState.THINK: think_frames,
                PetState.SLEEP: [sleep_pix],
                PetState.MUSIC: music_frames,
                PetState.MOVIE: movie_frames,
                PetState.LAUGH: [laugh_pix, smile_talk] if not smile_talk.isNull() else [laugh_pix],
                PetState.HAPPY: [happy_pix, smile_talk] if not smile_talk.isNull() else [happy_pix],
                PetState.SURPRISE: [surprise_pix],
                PetState.POUT: [pout_pix],
                PetState.SHOOT: shoot_frames
            }
        except Exception as e:
            print(f"[!] Error cargando sprites: {e}")

    def load_pixmap_fallback(self, filenames: list) -> QPixmap:
        for name in filenames:
            p = self.sprites_dir / name
            if p.exists():
                return QPixmap(str(p))
        return QPixmap()

    def load_pixmap(self, filename: str) -> QPixmap:
        path = self.sprites_dir / filename
        if not path.exists():
            print(f"[!] Aviso: Sprite no encontrado: {path}")
            return QPixmap()
        return QPixmap(str(path))

    def get_current_pixmap(self) -> QPixmap:
        frame = None
        cur_style = getattr(self, 'active_studio_style', 'normal')

        # 1. Parpadeo Biológico Orgánico (Prioridad máxima sobre ojos abiertos cuando no duerme)
        if getattr(self, 'is_blinking', False) and self.current_state != PetState.SLEEP:
            if cur_style == "streamer" and "stream_blink" in self.viseme_sprites:
                frame = self.viseme_sprites["stream_blink"]
            elif "blink" in self.viseme_sprites:
                frame = self.viseme_sprites["blink"]

        # 2. Sincronización Labial Fonética y Acústica (DSP Lip-Sync Anime)
        if frame is None and self.current_state in (PetState.TALK, PetState.LAUGH, PetState.HAPPY, PetState.SURPRISE):
            analyzer = getattr(self, 'lip_sync_analyzer', None)
            if analyzer and analyzer.is_ready and self.media_player.playbackState() == QMediaPlayer.PlaybackState.PlayingState:
                pos_ms = self.media_player.position()
                viseme_key = analyzer.get_viseme_at_time(pos_ms, style=cur_style)
                if viseme_key and viseme_key in self.viseme_sprites:
                    frame = self.viseme_sprites[viseme_key]

        # 3. Fallback a ciclo de animación procedural estándar
        if frame is None:
            frames = self.sprites.get(self.current_state, [])
            if not frames:
                return QPixmap()
            frame = frames[self.anim_frame_index % len(frames)]
            if frame.isNull():
                return QPixmap()

        # Invertir horizontalmente con QTransform si camina o dispara a la izquierda (Sin bugs de QPainter)
        if (self.current_state in (PetState.WALK, PetState.SHOOT)) and self.walk_direction < 0:
            transform = QTransform()
            transform.scale(-1, 1)
            frame = frame.transformed(transform)

        # BLINDAJE HIDPI: Renderizado al doble de densidad de píxeles sin agrandar el tamaño físico
        dpr = self.devicePixelRatio()
        target_h = int(self.current_height * dpr)
        scaled = frame.scaledToHeight(
            target_h,
            Qt.TransformationMode.SmoothTransformation
        )
        scaled.setDevicePixelRatio(dpr)
        return scaled

    def update_window_size(self):
        pixmap = self.get_current_pixmap()
        if not pixmap.isNull():
            sz = pixmap.deviceIndependentSize().toSize()
            # Margen vertical y lateral para alojar holgadamente badges (♪ ♫, zZZ) y respiración
            self.resize(sz.width() + 40, sz.height() + 26)

    def paintEvent(self, event):
        """Renderizado nativo por hardware en la superficie transparente con soporte HiDPI y badges procedurales."""
        painter = QPainter(self)
        painter.setRenderHint(QPainter.RenderHint.Antialiasing)
        painter.setRenderHint(QPainter.RenderHint.SmoothPixmapTransform)
        pixmap = self.get_current_pixmap()
        if not pixmap.isNull():
            # Si está en modo cine (MOVIE), renderizar con transparencia táctica para no tapar la película ni subtítulos
            if self.current_state == PetState.MOVIE:
                painter.setOpacity(0.35)

            # Control de respiración orgánica senoidal:
            # En modos de plató / estudio (streamer, reportera, telediario), la mesa, micro y silla
            # están integrados en el sprite y deben permanecer firmes e inamovibles (y_offset = 12).
            # También si el usuario desactiva la respiración desde el menú.
            is_studio = getattr(self, 'active_studio_style', 'normal') != 'normal'
            breathing_active = getattr(self, 'breathing_enabled', True) and not is_studio

            if not breathing_active or self.current_state == PetState.WALK:
                y_offset = 15 if (self.current_state == PetState.SLEEP and not is_studio) else 12
            elif self.current_state in (PetState.IDLE, PetState.MUSIC, PetState.MOVIE):
                y_offset = 12 + math.sin(self.breath_phase) * 3.5
            elif self.current_state == PetState.TALK:
                pos_ms = self.media_player.position()
                inertia = self.lip_sync_analyzer.get_speech_inertia_at_time(pos_ms) if getattr(self, 'lip_sync_analyzer', None) else 0.0
                y_offset = 12 + math.sin(self.breath_phase * 0.8) * 1.0 + inertia
            elif self.current_state == PetState.SLEEP:
                y_offset = 15 + math.sin(self.breath_phase * 0.5) * 1.5
            elif self.current_state == PetState.LAUGH:
                # Carcajadas: rebote rápido y alegre
                y_offset = 12 + abs(math.sin(self.anim_ticks * 0.45)) * 4.5
            elif self.current_state == PetState.HAPPY:
                y_offset = 12 + abs(math.sin(self.anim_ticks * 0.2)) * 3.0
            elif self.current_state == PetState.SURPRISE:
                y_offset = 8 + math.sin(self.breath_phase * 1.2) * 1.0
            elif self.current_state == PetState.POUT:
                y_offset = 14 + math.sin(self.breath_phase * 0.4) * 1.0
            else:
                y_offset = 12

            # Dibujar sprite con padding lateral
            painter.drawPixmap(8, int(y_offset + 4), pixmap)

            # --- BADGES PROCEDURALES CUANDO AÚN NO EXISTE EL SPRITE DEDICADO ---
            # 1. Animación flotante zZZ en modo siesta (SLEEP)
            if self.current_state == PetState.SLEEP:
                painter.setOpacity(1.0)
                painter.setPen(QColor(COLOR_NEON_CYAN))
                font = QFont("Consolas", 13, QFont.Weight.Bold)
                painter.setFont(font)
                zzz_y = int(2 + math.sin(self.breath_phase) * 3)
                painter.drawText(QRect(self.width() - 80, zzz_y, 70, 30), Qt.AlignmentFlag.AlignCenter, "zZZ")

            # 2. Notas musicales flotantes en modo música (MUSIC)
            elif self.current_state == PetState.MUSIC:
                painter.setOpacity(1.0)
                painter.setPen(QColor(COLOR_TERMINAL_GREEN))
                font = QFont("Segoe UI Symbol", 15, QFont.Weight.Bold)
                painter.setFont(font)
                note_y = int(2 + math.sin(self.breath_phase) * 2.5)
                note_char = "♪ ♫" if (self.anim_ticks // 25) % 2 == 0 else "♫ ♪"
                painter.drawText(QRect(self.width() - 85, note_y, 80, 32), Qt.AlignmentFlag.AlignCenter, note_char)

            # 3. Risas y chistes con destellos de humor (LAUGH)
            elif self.current_state == PetState.LAUGH:
                painter.setOpacity(1.0)
                painter.setPen(QColor(COLOR_NEON_MAGENTA))
                font = QFont("Consolas", 11, QFont.Weight.Bold)
                painter.setFont(font)
                laugh_y = int(2 + math.sin(self.anim_ticks * 0.4) * 2.5)
                laugh_text = "(≧▽≦) ¡JAJA!" if (self.anim_ticks // 12) % 2 == 0 else "✨ XD ✨"
                painter.drawText(QRect(self.width() - 110, laugh_y, 105, 30), Qt.AlignmentFlag.AlignCenter, laugh_text)

            # 4. Modo Feliz y Celebración (HAPPY)
            elif self.current_state == PetState.HAPPY:
                painter.setOpacity(1.0)
                painter.setPen(QColor(COLOR_NEON_CYAN))
                font = QFont("Consolas", 11, QFont.Weight.Bold)
                painter.setFont(font)
                happy_y = int(2 + math.sin(self.breath_phase * 1.5) * 2)
                happy_text = "★ (✿◠‿◠) ★" if (self.anim_ticks // 15) % 2 == 0 else "✨ [WIN] ✨"
                painter.drawText(QRect(self.width() - 110, happy_y, 105, 30), Qt.AlignmentFlag.AlignCenter, happy_text)

            # 5. Modo Sorpresa y Alerta Táctica (SURPRISE)
            elif self.current_state == PetState.SURPRISE:
                painter.setOpacity(1.0)
                painter.setPen(QColor(COLOR_NEON_MAGENTA))
                font = QFont("Consolas", 12, QFont.Weight.Bold)
                painter.setFont(font)
                surp_y = int(2 + math.sin(self.anim_ticks * 0.5) * 2)
                surp_text = "💥 [ ! ? ] 💥" if (self.anim_ticks // 8) % 2 == 0 else "⚡ [ ! ] ⚡"
                painter.drawText(QRect(self.width() - 105, surp_y, 100, 30), Qt.AlignmentFlag.AlignCenter, surp_text)

            # 6. Modo Puchero / Ofendida Cómica (POUT)
            elif self.current_state == PetState.POUT:
                painter.setOpacity(1.0)
                painter.setPen(QColor("#ff3366"))
                font = QFont("Consolas", 12, QFont.Weight.Bold)
                painter.setFont(font)
                pout_y = int(4 + math.sin(self.breath_phase) * 1.5)
                painter.drawText(QRect(self.width() - 95, pout_y, 90, 30), Qt.AlignmentFlag.AlignCenter, "💢 (¬_¬)")

            # 7. Modo Disparo y Neutralización Táctica (SHOOT)
            elif self.current_state == PetState.SHOOT:
                painter.setOpacity(1.0)
                painter.setPen(QColor(COLOR_NEON_CYAN))
                font = QFont("Consolas", 11, QFont.Weight.Bold)
                painter.setFont(font)
                shoot_text = "⚡ [LOCK-ON] ⚡" if self.shoot_tick < 14 else "💥 [BANG!] 💥"
                painter.drawText(QRect(self.width() - 125, 4, 120, 30), Qt.AlignmentFlag.AlignCenter, shoot_text)

    def position_on_screen(self):
        try:
            screen = QGuiApplication.primaryScreen().availableGeometry()
            init_x = max(50, screen.width() - self.width() - 120)
            init_y = max(50, screen.height() - self.height() - 30)
            self.move(init_x, init_y)
        except Exception:
            self.move(600, 300)

    def update_bubble_position(self):
        if self.bubble.isVisible():
            screen = QGuiApplication.primaryScreen().availableGeometry()
            bubble_x = self.x() - self.bubble.width() - 15
            if bubble_x < 10:
                bubble_x = self.x() + self.width() + 15

            bubble_y = max(10, self.y() + 10)
            self.bubble.move(bubble_x, bubble_y)

    @Slot()
    def update_animation(self):
        try:
            self.anim_ticks += 1
            self.breath_phase += 0.06

            if self.targeted_walk_active and self.target_pos:
                # 1. Desplazamiento cinemático táctico hacia el objetivo (X, Y)
                cur_x = self.x()
                cur_y = self.y()
                tx = self.target_pos.x()
                ty = self.target_pos.y()
                dist_x = tx - cur_x
                dist_y = ty - cur_y
                dist = (dist_x**2 + dist_y**2) ** 0.5

                if dist < 18:
                    # Llegada a la coordenada de tiro
                    self.targeted_walk_active = False
                    self.target_pos = None
                    self.move(tx, ty)
                    self.start_shooting_sequence()
                else:
                    speed = self.walk_speed * 2  # Paso táctico acelerado
                    step_x = int((dist_x / dist) * min(dist, speed)) if dist > 0 else 0
                    step_y = int((dist_y / dist) * min(dist, speed)) if dist > 0 else 0
                    self.walk_direction = -1 if dist_x < 0 else 1
                    self.move(cur_x + step_x, cur_y + step_y)
                    self.update_bubble_position()
                    if self.anim_ticks % 3 == 0:
                        self.anim_frame_index += 1

            elif self.current_state == PetState.WALK:
                # Alternar fotograma cada 3 ticks (120ms) para cadencia fluida y acompasada
                if self.anim_ticks % 3 == 0:
                    self.anim_frame_index += 1

                screen = QGuiApplication.primaryScreen().availableGeometry()
                new_x = self.x() + (self.walk_speed * self.walk_direction)

                # Rebote en los bordes de la pantalla
                if new_x > screen.width() - self.width() - 10:
                    self.walk_direction = -1
                    new_x = screen.width() - self.width() - 10
                elif new_x < 10:
                    self.walk_direction = 1
                    new_x = 10

                self.move(new_x, self.y())
                self.update_bubble_position()

            elif self.current_state == PetState.SHOOT:
                self.update_shoot_animation()

            elif self.current_state in (PetState.TALK, PetState.LAUGH, PetState.HAPPY, PetState.SURPRISE):
                # Lip-sync de alta fidelidad: alternar visemes cada 2 ticks (80ms = 12.5 FPS) para cadencia fonética natural
                if self.anim_ticks % 2 == 0:
                    self.anim_frame_index += 1

            elif self.current_state == PetState.THINK:
                # Alternar holograma cada 4 ticks (160ms)
                if self.anim_ticks % 4 == 0:
                    self.anim_frame_index += 1

            self.update()
        except Exception as e:
            print(f"[!] Error en animación: {e}")

    @Slot(str, int, int)
    def trigger_destroy_file_sequence(self, file_path: str, target_x: int, target_y: int):
        """Inicia el desplazamiento táctico hacia el archivo en el escritorio y su destrucción."""
        print(f"[*] Secuencia de neutralización activada para: {file_path} en ({target_x}, {target_y})")
        self.original_home_pos = self.pos()
        self.target_file_to_delete = file_path

        # Posición de tiro: a la derecha del archivo (target_x + 50, target_y - 40), mirando hacia la izquierda
        screen = QGuiApplication.primaryScreen().availableGeometry()
        stop_x = max(10, min(screen.width() - self.width() - 10, target_x + 50))
        stop_y = max(10, min(screen.height() - self.height() - 10, target_y - 40))

        self.target_pos = QPoint(stop_x, stop_y)
        self.targeted_walk_active = True
        self.walk_direction = -1 if stop_x < self.x() else 1
        self.set_state(PetState.WALK)
        self.show_dialogue(f"🎯 Localizado '{Path(file_path).name}'. Me desplazo a la posición de tiro...")

    def start_shooting_sequence(self):
        """Inicia la postura de tiro, fogonazo, envío a la papelera y retorno."""
        self.walk_direction = -1  # Encarar hacia la izquierda (hacia el archivo)
        self.set_state(PetState.SHOOT)
        self.shoot_tick = 0
        self.show_dialogue("⚡ ¡Fijando blanco! Abriendo fuego...")

    def update_shoot_animation(self):
        """Gestiona el ciclo de fotogramas del disparo y ejecuta el impacto en el momento exacto."""
        self.shoot_tick += 1
        frames = self.sprites.get(PetState.SHOOT, [])
        total_frames = len(frames)
        if total_frames > 0:
            self.anim_frame_index = min(self.shoot_tick // 2, total_frames - 1)

        # En el fotograma de impacto (tick 14 ~ 560ms, cuando el láser impacta contra el suelo)
        if self.shoot_tick == 14:
            if self.target_file_to_delete:
                success = send_file_to_recycle_bin(self.target_file_to_delete)
                if success:
                    self.show_dialogue("💥 ¡OBJETIVO PURGADO! Enviado a la papelera.")
                else:
                    self.show_dialogue("⚠️ No se pudo purgar el archivo.")

        # Fin de la secuencia de tiro (tick 28 ~ 1.1s)
        if self.shoot_tick >= max(20, total_frames * 2):
            self.set_state(PetState.HAPPY)
            QTimer.singleShot(2500, self.return_to_home_post)

    def return_to_home_post(self):
        """Retorna suavemente caminando a su puesto de guardia original en el escritorio."""
        if self.original_home_pos:
            self.target_pos = self.original_home_pos
            self.original_home_pos = None
            self.targeted_walk_active = True
            self.walk_direction = -1 if self.target_pos.x() < self.x() else 1
            self.set_state(PetState.WALK)
            self.show_dialogue("Puesto de guardia restablecido.")
        else:
            self.set_state(PetState.IDLE)

    def set_state(self, new_state: PetState):
        if self.current_state != new_state:
            if new_state != PetState.SLEEP:
                self.manual_sleep_active = False
            self.current_state = new_state
            self.anim_frame_index = 0
            self.update_window_size()
            self.update()

    def is_foreground_fullscreen(self) -> bool:
        """Comprueba si la ventana activa en primer plano está en modo pantalla completa."""
        if sys.platform != "win32":
            return False
        try:
            import ctypes
            user32 = ctypes.windll.user32
            hwnd = user32.GetForegroundWindow()
            if not hwnd or hwnd == int(self.winId()):
                return False
            
            # Descartar escritorio y barra de tareas
            class_buf = ctypes.create_unicode_buffer(256)
            user32.GetClassNameW(hwnd, class_buf, 256)
            if class_buf.value in ("Progman", "WorkerW", "Shell_TrayWnd"):
                return False

            class RECT(ctypes.Structure):
                _fields_ = [
                    ('left', ctypes.c_long),
                    ('top', ctypes.c_long),
                    ('right', ctypes.c_long),
                    ('bottom', ctypes.c_long)
                ]
            rect = RECT()
            if not user32.GetWindowRect(hwnd, ctypes.byref(rect)):
                return False
            
            screen_w = user32.GetSystemMetrics(0)
            screen_h = user32.GetSystemMetrics(1)

            # Tolerancia de 2px para bordes sin marco
            return (rect.left <= 2 and rect.top <= 2 and rect.right >= (screen_w - 2) and rect.bottom >= (screen_h - 2))
        except Exception:
            return False

    def check_device_telemetry(self):
        """Monitorea inactividad del usuario, audio activo y autonomía orgánica del companion (CPU 0.0%)."""
        try:
            # Si Rebeca está hablando, pensando, caminando, disparando o en siesta manual, mantener su estado prioritario
            if self.current_state in (PetState.TALK, PetState.THINK, PetState.WALK, PetState.SHOOT) or (
                self.current_state == PetState.SLEEP and getattr(self, 'manual_sleep_active', False)
            ):
                return

            # Si está en modo plató de presentadora / OBS Studio (streamer, reportera, telediario),
            # mantener la presencia de estudio sin interrupciones automáticas de música o cine
            if getattr(self, 'active_studio_style', 'normal') != 'normal':
                if self.current_state in (PetState.MUSIC, PetState.MOVIE):
                    self.set_state(PetState.IDLE)
                return

            # 1. Detección de audio/música activa en Windows (YouTube, Spotify, reproductores, etc.)
            is_audio_active = False
            if sys.platform == "win32":
                try:
                    from pycaw.pycaw import AudioUtilities, IAudioMeterInformation
                    # Lista de exclusión: software de captura, streaming, audio virtual y comunicaciones
                    ignored_audio = {
                        "obs64.exe", "obs32.exe", "obs.exe", "streamlabs obs.exe",
                        "audiodg.exe", "system", "registry",
                        "voicemeeter.exe", "voicemeeter8.exe", "voicemeeter8x64.exe",
                        "discord.exe", "telegram.exe", "whatsapp.exe",
                        "steamwebhelper.exe", "nvcontainer.exe", "nvidia share.exe"
                    }
                    sessions = AudioUtilities.GetAllSessions()
                    for session in sessions:
                        if session.State == 1:  # AudioSessionStateActive
                            proc = session.Process
                            pname = proc.name().lower() if proc else ""
                            # Filtrar intérpretes de Python y herramientas de streaming/comunicación
                            if not pname or "python" in pname or pname in ignored_audio:
                                continue
                            # Comprobar si el proceso está emitiendo volumen audible real (Peak > 0.005)
                            try:
                                meter = session._ctl.QueryInterface(IAudioMeterInformation)
                                if meter.GetPeakValue() > 0.005:
                                    is_audio_active = True
                                    break
                            except Exception:
                                # Fallback si no soporta IAudioMeterInformation
                                is_audio_active = True
                                break
                except Exception:
                    pass

            # 2. Detección de Pantalla Completa (Películas, series o vídeos a pantalla completa)
            is_fullscreen = self.is_foreground_fullscreen()

            if is_audio_active:
                # Si hay sonido Y la ventana está en pantalla completa -> Modo Película (MOVIE)
                if is_fullscreen:
                    if self.current_state != PetState.MOVIE:
                        self.set_state(PetState.MOVIE)
                    return
                else:
                    # Si hay sonido pero en ventana normal -> Modo Música (MUSIC)
                    if self.current_state != PetState.MUSIC:
                        self.set_state(PetState.MUSIC)
                    return
            else:
                # Si se pausó o cerró el reproductor, regresar a reposo
                if self.current_state in (PetState.MUSIC, PetState.MOVIE):
                    self.set_state(PetState.IDLE)

            # 2. Medir inactividad del usuario en Windows mediante GetLastInputInfo
            idle_sec = 0.0
            if sys.platform == "win32":
                import ctypes
                class LASTINPUTINFO(ctypes.Structure):
                    _fields_ = [("cbSize", ctypes.c_uint), ("dwTime", ctypes.c_ulong)]
                info = LASTINPUTINFO()
                info.cbSize = ctypes.sizeof(LASTINPUTINFO)
                if ctypes.windll.user32.GetLastInputInfo(ctypes.byref(info)):
                    idle_sec = max(0.0, (ctypes.windll.kernel32.GetTickCount() - info.dwTime) / 1000.0)

            # Transición a siesta/sueño tras 5 minutos de inactividad (300 segundos)
            if idle_sec >= 300:
                if self.current_state == PetState.IDLE:
                    self.set_state(PetState.SLEEP)
            elif idle_sec < 3.0:
                # Si estaba durmiendo de forma autónoma y el usuario mueve el ratón o teclea, despertar de inmediato
                if self.current_state == PetState.SLEEP and not getattr(self, 'manual_sleep_active', False):
                    self.set_state(PetState.IDLE)
                # Patrulla autónoma ocasional cuando el usuario está activo (probabilidad ligera)
                elif self.current_state == PetState.IDLE and not self.bubble.isVisible():
                    import random
                    if random.random() < 0.04:  # ~4% de probabilidad cada 2.5s si está ociosa
                        self.start_short_patrol()
        except Exception:
            pass

    def start_short_patrol(self):
        """Inicia un paseo breve y espontáneo para dar vida orgánica al personaje."""
        if self.current_state != PetState.IDLE:
            return
        import random
        self.walk_direction = random.choice([1, -1])
        self.set_state(PetState.WALK)
        # Detener la patrulla tras 3.5 segundos y volver a reposo
        QTimer.singleShot(3500, lambda: self.set_state(PetState.IDLE) if self.current_state == PetState.WALK else None)

    # =========================================================================
    # INTELIGENCIA Y VOZ: GEMINI + BÚSQUEDA WEB + EDGE-TTS
    # =========================================================================
    def process_query(self, query: str):
        """Punto de enlace: Inicia pensamiento de Rebeca o locución directa en modo Eco OBS."""
        # Detener cualquier hilo de escucha mientras Rebeca procesa la respuesta
        if self.listen_thread and self.listen_thread.isRunning():
            self.listen_thread.stop_recording()

        # Si el Modo Eco OBS está activo, el texto escrito en el chat se locuta directamente como noticia
        if getattr(self, 'presenter_echo_mode', False):
            self.show_dialogue(f"🎙️ [ECO OBS]: \"{query}\"")
            self.speak_text(query)
            return

        self.current_user_query = query
        api_key = self.config.get("gemini_api_key", "").strip()

        if not api_key:
            # Si no hay clave, dar respuesta orientativa
            self.set_state(PetState.THINK)
            QTimer.singleShot(800, lambda: self.handle_no_api_key(query))
            return

        # Modo Netrunner (tecleando en holograma mientras consulta la red)
        self.set_state(PetState.THINK)
        self.show_dialogue(f"Rastreando la red para: '{query}'...")

        # Detección de intención climática local (Geo-IP + Open-Meteo)
        lower_query = query.lower()
        weather_keywords = ["temperatura", "clima", "tiempo hace", "tiempo hoy", "grados hace", "que tiempo hace", "va a llover"]
        augmented_query = query
        if any(kw in lower_query for kw in weather_keywords):
            try:
                weather_info = get_live_weather_telemetry(timeout=2.5)
                if weather_info.get("success"):
                    augmented_query = f"{query}\n[DATOS CLIMÁTICOS LOCALES EN VIVO]: {weather_info['summary']}"
            except Exception as w_err:
                print(f"[!] Error leyendo sensor clima: {w_err}")

        # Construir contexto de mochila y memoria de turnos
        backpack_context = self.backpack_mgr.build_context_backpack()
        chat_history = self.backpack_mgr.get_chat_history(limit=6)

        # Limpiar badge y registro de acciones previas
        self.bubble.set_action_status("")
        self.tools_executed_in_turn = []

        # Detener hilo de consulta previo si aún estaba en curso
        if self.gemini_thread and self.gemini_thread.isRunning():
            try:
                self.gemini_thread.quit()
                self.gemini_thread.wait(300)
            except Exception:
                pass

        # Iniciar temporizador guardián (watchdog) de seguridad (45 segundos)
        if hasattr(self, 'query_watchdog') and self.query_watchdog:
            self.query_watchdog.stop()
        self.query_watchdog = QTimer(self)
        self.query_watchdog.setSingleShot(True)
        self.query_watchdog.timeout.connect(self.on_query_timeout)
        self.query_watchdog.start(45000)

        # Lanzar consulta a Gemini en hilo secundario
        system_prompt = self.get_system_prompt()
        self.gemini_thread = GeminiWorker(
            api_key=api_key,
            query=augmented_query,
            system_prompt=system_prompt,
            backpack_context=backpack_context,
            chat_history=chat_history
        )
        self.gemini_thread.finished_signal.connect(self.on_gemini_success)
        self.gemini_thread.action_signal.connect(self.on_agent_action)
        self.gemini_thread.error_signal.connect(self.on_gemini_error)
        self.gemini_thread.start()

    def get_system_prompt(self) -> str:
        # Prioridad 1: system_prompt.md (formato nativo enriquecido)
        prompt_file_md = self.base_dir / "system_prompt.md"
        if prompt_file_md.exists():
            try:
                with open(prompt_file_md, "r", encoding="utf-8") as f:
                    content = f.read().strip()
                    if content:
                        return content
            except Exception as e:
                print(f"[!] Error leyendo system_prompt.md: {e}")

        # Prioridad 2: system_prompt.txt (respaldo legacy)
        prompt_file_txt = self.base_dir / "system_prompt.txt"
        if prompt_file_txt.exists():
            try:
                with open(prompt_file_txt, "r", encoding="utf-8") as f:
                    content = f.read().strip()
                    if content:
                        return content
            except Exception as e:
                print(f"[!] Error leyendo system_prompt.txt: {e}")
        return MASTER_SYSTEM_PROMPT

    def open_system_prompt_file(self):
        prompt_file = self.base_dir / "system_prompt.md"
        if not prompt_file.exists():
            prompt_file = self.base_dir / "system_prompt.txt"
        try:
            import os
            os.startfile(str(prompt_file))
        except Exception as e:
            self.show_dialogue(f"Archivo de personalidad en: {prompt_file}")

    def handle_no_api_key(self, query: str):
        msg = (
            "Operador, aún no has configurado tu clave gratuita de Google AI Studio. "
            "Haz clic derecho sobre mí y selecciona '🔑 Configurar Gemini API Key' para enlazar mi cerebro."
        )
        self.on_gemini_success(msg, "")

    def on_query_timeout(self):
        """Mecanismo de seguridad watchdog: evita que Rebeca quede congelada si Google no responde."""
        if self.gemini_thread and self.gemini_thread.isRunning():
            try:
                self.gemini_thread.terminate()
                self.gemini_thread.wait(500)
            except Exception:
                pass
        self.on_gemini_error("Tiempo de espera agotado (45s). Los servidores de Google no respondieron a tiempo.")

    @Slot(str)
    def on_agent_action(self, action_desc: str):
        """Muestra feedback visual cuando el agente ejecuta una herramienta de sistema."""
        if not hasattr(self, 'tools_executed_in_turn'):
            self.tools_executed_in_turn = []
        self.tools_executed_in_turn.append(action_desc)
        self.bubble.set_action_status(action_desc)
        if not self.bubble.isVisible():
            self.update_bubble_position()
            self.bubble.show()

    @Slot(str, str)
    def on_gemini_success(self, text: str, sources: str, forced_state=None):
        """Llamada cuando Gemini o un módulo local entrega la respuesta."""
        if hasattr(self, 'query_watchdog') and self.query_watchdog:
            self.query_watchdog.stop()

        clean_text = text
        if forced_state is not None:
            emotion_state = forced_state
        else:
            # Detección de Modulador Emocional [ESTADO: LAUGH/HAPPY/SURPRISE/POUT/TALK]
            emotion_state = PetState.TALK

            # 1. Por etiqueta explícita de Gemini
            tag_match = re.search(r"\[(?:ESTADO|STATE):\s*([A-Z_]+)\]", text, re.IGNORECASE)
            if tag_match:
                tag_name = tag_match.group(1).upper()
                clean_text = re.sub(r"\[(?:ESTADO|STATE):\s*[A-Z_]+\]", "", clean_text).strip()
                if tag_name in ("LAUGH", "RISA", "JOKE", "CHISTE"):
                    emotion_state = PetState.LAUGH
                elif tag_name in ("HAPPY", "FELIZ", "ALEGRIA", "CELEBRATE", "VICTORIA"):
                    emotion_state = PetState.HAPPY
                elif tag_name in ("SURPRISE", "SORPRESA", "ALERT", "ALERTA"):
                    emotion_state = PetState.SURPRISE
                elif tag_name in ("POUT", "PUCHERO", "OFENDIDA", "ANGRY"):
                    emotion_state = PetState.POUT

            # 2. Detección heurística inteligente por intención si no hubo etiqueta
            if emotion_state == PetState.TALK:
                query_low = (self.current_user_query or "").lower()
                text_low = clean_text.lower()
                # Si el texto es reporte meteorológico o telemetría técnica, mantener siempre en TALK
                is_telemetry = any(k in text_low for k in ["°c", "temperatura", "humedad", "km/h", "satélites", "telemetría"])
                if not is_telemetry:
                    if any(w in query_low for w in ["chiste", "broma", "cuentame un chiste", "hazme reir"]) or any(w in text_low for w in ["jajaja", "jejeje", "jaja", "un chiste", "le dice el"]):
                        emotion_state = PetState.LAUGH
                    elif any(w in query_low for w in ["gracias", "bien hecho", "felicidades", "buen trabajo", "te quiero"]):
                        emotion_state = PetState.HAPPY
                    elif any(w in text_low for w in ["¡cuidado!", "¡alerta!", "¡que raro!", "¿como es posible?"]):
                        emotion_state = PetState.SURPRISE

        self.active_speaking_state = emotion_state

        # Red de seguridad agéntica determinista para el navegador:
        # Si el usuario pidió explícitamente ver resultados o noticias en el navegador/explorador,
        # pero la IA por cualquier causa (timeout, alucinación o fallback) no ejecutó open_url_or_search,
        # forzar la apertura física inmediata en el navegador del usuario.
        browser_keywords = ["navegador", "explorador", "browser", "chrome", "edge"]
        action_keywords = ["abre", "abrir", "ábreme", "abreme", "busca", "buscar", "búscame", "buscame", "muestra", "mostrar", "muéstramelo", "muestramelo", "enseña", "enseñamelo", "enséñamelo"]
        q_low = (self.current_user_query or "").lower()
        user_wanted_browser = any(bk in q_low for bk in browser_keywords) and any(ak in q_low for ak in action_keywords)
        has_executed_browser = any("navegador" in str(a).lower() or "http" in str(a).lower() for a in getattr(self, 'tools_executed_in_turn', []))

        if user_wanted_browser and not has_executed_browser:
            print(f"[!] Red de seguridad agéntica: Disparando apertura forzosa de navegador para: {self.current_user_query}")
            clean_search = self.current_user_query or ""
            strip_phrases = [
                "buscame", "búscame", "noticias de", "noticias sobre", "noticias",
                "y enseñamelo en un explorador", "y enséñamelo en un explorador",
                "y muestramelo en el navegador", "y muéstramelo en el navegador",
                "en el navegador", "en un explorador", "en el explorador", "abre el navegador",
                "abrir el navegador", "y abre", "ábreme un", "abreme un"
            ]
            for sp in strip_phrases:
                clean_search = re.sub(re.escape(sp), "", clean_search, flags=re.IGNORECASE)
            clean_search = clean_search.strip() or self.current_user_query
            try:
                agent_tools.open_url_or_search(clean_search)
                self.tools_executed_in_turn.append(f"Abriendo navegador: {clean_search}")
            except Exception as b_err:
                print(f"[!] Error en red de seguridad de navegador: {b_err}")

        # Guardar turno en la memoria persistente del gestor y limpiar query activo
        if self.current_user_query:
            self.backpack_mgr.save_turn(self.current_user_query, clean_text)
            self.current_user_query = ""

        self.show_dialogue(clean_text, sources)

        # Generar voz propia neural con archivo único de rotación
        try:
            voice = self.config.get("voice", DEFAULT_VOICE)
            rate = self.config.get("rate", DEFAULT_RATE)
            pitch = self.config.get("pitch", DEFAULT_PITCH)
            new_audio_path = self.get_new_audio_cache_path()
            if self.voice_thread and self.voice_thread.isRunning():
                try:
                    self.voice_thread.audio_ready.disconnect()
                except Exception:
                    pass
                try:
                    self.voice_thread.error_signal.disconnect()
                except Exception:
                    pass
                try:
                    self.voice_thread.quit()
                    self.voice_thread.wait(200)
                except Exception:
                    pass
            self.voice_thread = VoiceWorker(clean_text, voice, new_audio_path, rate=rate, pitch=pitch)
            self.voice_thread.audio_ready.connect(self.play_voice)
            self.voice_thread.error_signal.connect(self.on_tts_error)
            self.voice_thread.start()
        except Exception as e:
            self.on_tts_error(f"Fallo al iniciar síntesis: {e}")

    def get_new_audio_cache_path(self) -> str:
        """Genera una ruta de archivo de audio única para evitar bloqueos de archivo en Windows y caché interna de QMediaPlayer."""
        try:
            cache_dir = self.base_dir / "assets" / "audio_cache"
            cache_dir.mkdir(parents=True, exist_ok=True)
            try:
                files = sorted(cache_dir.glob("speech_*.mp3"), key=lambda p: p.stat().st_mtime)
                # Mantener los últimos 4 archivos para no colisionar con buffers activos de Windows
                for old_f in files[:-4]:
                    try:
                        old_f.unlink(missing_ok=True)
                    except Exception:
                        pass
            except Exception:
                pass
            return str(cache_dir / f"speech_{int(time.time() * 1000)}.mp3")
        except Exception as e:
            print(f"[!] Fallback en ruta de audio: {e}")
            return getattr(self, 'cache_audio_path', str(self.base_dir / "assets" / "rebeca_speech.mp3"))

    def stop_current_speech(self):
        """Detiene inmediatamente cualquier locución o síntesis en curso."""
        if self.media_player.playbackState() == QMediaPlayer.PlaybackState.PlayingState:
            self.media_player.stop()
        self.media_player.setSource(QUrl())
        if self.voice_thread and self.voice_thread.isRunning():
            try:
                self.voice_thread.audio_ready.disconnect()
            except Exception:
                pass
            try:
                self.voice_thread.error_signal.disconnect()
            except Exception:
                pass
            try:
                self.voice_thread.quit()
                self.voice_thread.wait(200)
            except Exception:
                pass
        self.active_speaking_state = PetState.TALK
        if hasattr(self, 'lip_sync_analyzer'):
            self.lip_sync_analyzer.is_ready = False
        if hasattr(self, 'audio_ducker'):
            self.audio_ducker.emergency_restore_all()
        self.set_state(PetState.IDLE)
        self.show_dialogue("⏹️ Locución detenida.")

    @Slot(QMediaPlayer.Error, str)
    def on_media_error(self, error, error_string):
        """Recuperación a prueba de fallos si el reproductor multimedia de Qt falla."""
        print(f"[!] Incidencia en QMediaPlayer: {error} - {error_string}")
        self.active_speaking_state = PetState.TALK
        self.set_state(PetState.IDLE)
        if self.continuous_voice_mode:
            QTimer.singleShot(500, self.start_continuous_listening)

    def on_tts_error(self, err_msg: str):
        print(f"[!] Error TTS: {err_msg}")
        self.active_speaking_state = PetState.TALK
        self.set_state(PetState.IDLE)
        self.show_dialogue(f"⚠️ [INCIDENCIA VOCAL]: {err_msg[:80]}")
        if self.continuous_voice_mode:
            QTimer.singleShot(500, self.start_continuous_listening)

    @Slot(str)
    def on_gemini_error(self, error_msg: str):
        if hasattr(self, 'query_watchdog') and self.query_watchdog:
            self.query_watchdog.stop()

        self.set_state(PetState.TALK)
        msg = f"[FALLO DE TRANSMISIÓN]: {error_msg}"
        self.show_dialogue(msg)
        def _recover():
            self.set_state(PetState.IDLE)
            if self.continuous_voice_mode:
                QTimer.singleShot(500, self.start_continuous_listening)
        QTimer.singleShot(4000, _recover)

    @Slot(str)
    def play_voice(self, audio_file_path: str):
        """Reproduce el audio generado e inicia el lip-sync con la emoción activa."""
        try:
            # Detener captura de audio antes de reproducir para evitar eco acústico
            if self.listen_thread and self.listen_thread.isRunning():
                self.listen_thread.stop_recording()
                self.listen_thread.wait(200)

            speak_state = getattr(self, 'active_speaking_state', PetState.TALK)
            self.set_state(speak_state)

            # Iniciar análisis fonético y acústico DSP para sincronización labial precisa
            if hasattr(self, 'lip_sync_analyzer'):
                self.lip_sync_analyzer.analyze_file(audio_file_path)

            # Atenuar música del sistema automáticamente (Smart Audio Ducking)
            if hasattr(self, 'audio_ducker'):
                self.audio_ducker.duck_audio(target_factor=0.20, duration_ms=150)

            # Detener y reiniciar la fuente en QMediaPlayer para forzar la recarga del archivo
            if self.media_player.playbackState() == QMediaPlayer.PlaybackState.PlayingState:
                self.media_player.stop()
            self.media_player.setSource(QUrl())
            self.media_player.setSource(QUrl.fromLocalFile(audio_file_path))
            self.audio_output.setVolume(1.0)
            self.media_player.play()
        except Exception as e:
            print(f"[!] Error reproduciendo audio: {e}")
            if hasattr(self, 'audio_ducker'):
                self.audio_ducker.restore_audio(duration_ms=350)
            self.set_state(PetState.IDLE)
            if self.continuous_voice_mode:
                QTimer.singleShot(500, self.start_continuous_listening)

    @Slot(QMediaPlayer.PlaybackState)
    def on_audio_state_changed(self, state):
        """Cuando la voz termina de hablar, vuelve a Standby o retoma manos libres."""
        if state == QMediaPlayer.PlaybackState.StoppedState:
            if hasattr(self, 'lip_sync_analyzer'):
                self.lip_sync_analyzer.is_ready = False
            self.is_blinking = False
            self.active_speaking_state = PetState.TALK
            self.current_user_query = ""
            self.set_state(PetState.IDLE)

            # Restaurar volumen de música del sistema si no estamos en modo manos libres continuo
            if hasattr(self, 'audio_ducker') and not self.continuous_voice_mode:
                self.audio_ducker.restore_audio(duration_ms=350)

            if self.continuous_voice_mode:
                # Amortiguación acústica para evitar que Rebeca capture su propia reverberación
                delay_ms = 750 if getattr(self, 'presenter_echo_mode', False) else 450
                QTimer.singleShot(delay_ms, self.start_continuous_listening)

    # =========================================================================
    # EVENTOS DE INTERACCIÓN Y MENÚ
    # =========================================================================
    def mousePressEvent(self, event):
        if event.button() == Qt.MouseButton.LeftButton:
            if self.current_state == PetState.SLEEP:
                self.manual_sleep_active = False
                self.set_state(PetState.IDLE)
                self.show_dialogue("¡Sistemas reactivados, Operador! Enlace restablecido.")
                return
            self.is_dragging = True
            self.drag_position = event.globalPosition().toPoint() - self.frameGeometry().topLeft()
            event.accept()

    def mouseMoveEvent(self, event):
        if self.is_dragging and (event.buttons() & Qt.MouseButton.LeftButton):
            self.move(event.globalPosition().toPoint() - self.drag_position)
            self.update_bubble_position()
            event.accept()

    def mouseReleaseEvent(self, event):
        self.is_dragging = False

    def mouseDoubleClickEvent(self, event):
        if self.bubble.isVisible():
            self.bubble.hide()
        else:
            self.show_dialogue("¿Qué necesitas consultar o investigar en la red, Operador?")

    def contextMenuEvent(self, event):
        self.menu.exec(event.globalPos())

    def activate_manual_sleep(self):
        """Pone a Rebeca en modo siesta (Sleep) para visualizar su respiración lenta y zZZ flotantes."""
        self.manual_sleep_active = True
        self.set_state(PetState.SLEEP)
        if self.bubble.isVisible():
            self.bubble.hide()

    def activate_manual_walk(self):
        """Inicia caminata continua por el escritorio rebotando en los bordes de la pantalla."""
        self.manual_sleep_active = False
        self.set_state(PetState.WALK)

    def activate_short_patrol(self):
        """Inicia un paseo táctico de prueba de 5 segundos y regresa a reposo."""
        self.manual_sleep_active = False
        import random
        self.walk_direction = random.choice([1, -1])
        self.set_state(PetState.WALK)
        QTimer.singleShot(5000, lambda: self.set_state(PetState.IDLE) if self.current_state == PetState.WALK else None)

    def activate_manual_idle(self):
        """Regresa a Rebeca a su estado de reposo táctico estándar (Standby)."""
        self.manual_sleep_active = False
        if getattr(self, 'active_studio_style', 'normal') != 'normal':
            self.set_studio_style('normal')
        else:
            self.current_state = PetState.IDLE
            self.anim_frame_index = 0
            self.update_window_size()
            self.update()
            self.show_dialogue("🧍 Rebeca en reposo táctico estándar.")

    def setup_context_menu(self):
        self.menu = QMenu(self)
        self.menu.setStyleSheet(STYLE_CONTEXT_MENU)

        action_idle = self.menu.addAction("🧍 Modo Reposo Estándar (Standby / Idle)")
        action_idle.triggered.connect(self.activate_manual_idle)

        action_patrol = self.menu.addAction("🐾 Paseo Táctico Breve (Caminar 5s)")
        action_patrol.triggered.connect(self.activate_short_patrol)

        action_walk = self.menu.addAction("🚶 Patrulla Continua (Walk)")
        action_walk.triggered.connect(self.activate_manual_walk)

        action_sleep = self.menu.addAction("🌙 Modo Siesta / Dormir (Sleep)")
        action_sleep.triggered.connect(self.activate_manual_sleep)

        # Submenú Modo Presentadora / OBS Studio
        studio_menu = self.menu.addMenu("🎬 Modo Presentadora / OBS Studio")
        studio_menu.setStyleSheet(STYLE_CONTEXT_MENU)

        action_exit_studio = studio_menu.addAction("❌ Desactivar Plató (Volver a Mascota Estándar)")
        action_exit_studio.triggered.connect(lambda: self.set_studio_style("normal"))

        studio_menu.addSeparator()

        action_teleprompter = studio_menu.addAction("📜 Leer Guión de Noticias (Teleprompter / TikTok)")
        action_teleprompter.triggered.connect(self.open_teleprompter_dialog)

        self.action_echo = studio_menu.addAction("🎙️ Modo Repetidora de Voz (Eco OBS)")
        self.action_echo.setCheckable(True)
        self.action_echo.triggered.connect(self.toggle_presenter_echo)

        action_stop_speech = studio_menu.addAction("⏹️ Silenciar / Detener Locución (Esc)")
        action_stop_speech.triggered.connect(self.stop_current_speech)

        style_menu = studio_menu.addMenu("👗 Estilo Visual de Plató")
        style_menu.setStyleSheet(STYLE_CONTEXT_MENU)
        style_menu.addAction("🧍 Mascota Estándar").triggered.connect(lambda: self.set_studio_style("normal"))
        style_menu.addAction("🎙️ Reportera (Micro en mano)").triggered.connect(lambda: self.set_studio_style("reportera"))
        style_menu.addAction("📺 Telediario (Mesa de plató)").triggered.connect(lambda: self.set_studio_style("telediario"))
        style_menu.addAction("🎮 Streamer (Webcam y Headset)").triggered.connect(lambda: self.set_studio_style("streamer"))

        # Submenú táctico de Expresiones y Emociones
        expr_menu = self.menu.addMenu("🎭 Expresiones y Estados")
        expr_menu.setStyleSheet(STYLE_CONTEXT_MENU)
        expr_menu.addAction("😂 Chiste / Risa (Laugh)").triggered.connect(lambda: self.set_state(PetState.LAUGH))
        expr_menu.addAction("✨ Victoria / Feliz (Happy)").triggered.connect(lambda: self.set_state(PetState.HAPPY))
        expr_menu.addAction("💥 Alerta / Sorpresa (Surprise)").triggered.connect(lambda: self.set_state(PetState.SURPRISE))
        expr_menu.addAction("😤 Puchero / Ofendida (Pout)").triggered.connect(lambda: self.set_state(PetState.POUT))
        expr_menu.addAction("🎧 Modo Música (Music)").triggered.connect(lambda: self.set_state(PetState.MUSIC))
        expr_menu.addAction("🎬 Modo Cine / Película (Movie)").triggered.connect(lambda: self.set_state(PetState.MOVIE))
        expr_menu.addAction("💻 Modo Netrunner (Think)").triggered.connect(lambda: self.set_state(PetState.THINK))
        expr_menu.addAction("💬 Modo Transmisión (Talk)").triggered.connect(lambda: self.set_state(PetState.TALK))

        self.menu.addSeparator()

        action_key = self.menu.addAction("🔑 Configurar Gemini API Key")
        action_key.triggered.connect(self.open_api_key_dialog)

        action_prompt = self.menu.addAction("📝 Editar Personalidad (Prompt)")
        action_prompt.triggered.connect(self.open_system_prompt_file)

        action_chat = self.menu.addAction("📟 Abrir/Cerrar Terminal")
        action_chat.triggered.connect(lambda: self.mouseDoubleClickEvent(None))

        action_mic = self.menu.addAction("🎙️ Hablar por Micrófono (F2)")
        action_mic.triggered.connect(self.toggle_mic_recording)

        self.action_continuous = self.menu.addAction("🔁 Modo Manos Libres (F3)")
        self.action_continuous.setCheckable(True)
        self.action_continuous.triggered.connect(self.toggle_continuous_voice)

        action_backpack = self.menu.addAction("🎒 Mochila de Datos (Subir Archivos / Memoria)")
        action_backpack.triggered.connect(self.open_backpack_dialog)

        action_weather = self.menu.addAction("🌦️ Telemetría de Clima y Temperatura")
        action_weather.triggered.connect(self.report_live_weather)

        voice_menu = self.menu.addMenu("🎙️ Voz de Rebeca")
        voice_menu.setStyleSheet(STYLE_CONTEXT_MENU)
        voice_menu.addAction("🇪🇸 Ximena (Estilo Paola Navalón / Táctica)").triggered.connect(
            lambda: self.set_voice("es-ES-XimenaNeural", "+5%", "+0Hz")
        )
        voice_menu.addAction("🇪🇸 Elvira (Oficial / Formal)").triggered.connect(
            lambda: self.set_voice("es-ES-ElviraNeural", "+5%", "+0Hz")
        )
        voice_menu.addAction("🇻🇪 Paola (LatAm / Expresiva)").triggered.connect(
            lambda: self.set_voice("es-VE-PaolaNeural", "+4%", "+0Hz")
        )

        speed_menu = voice_menu.addMenu("⚡ Velocidad de Dicción")
        speed_menu.setStyleSheet(STYLE_CONTEXT_MENU)
        speed_menu.addAction("Tranquila (+0%)").triggered.connect(lambda: self.set_voice_speed("+0%"))
        speed_menu.addAction("Ágil / Táctica (+5%) [Recomendada]").triggered.connect(lambda: self.set_voice_speed("+5%"))
        speed_menu.addAction("Rápida (+10%)").triggered.connect(lambda: self.set_voice_speed("+10%"))

        size_menu = self.menu.addMenu("📐 Escala Visual")
        size_menu.setStyleSheet(STYLE_CONTEXT_MENU)
        size_menu.addAction("Pequeño (240px)").triggered.connect(lambda: self.set_scale("pequeno"))
        size_menu.addAction("Normal (330px)").triggered.connect(lambda: self.set_scale("normal"))
        size_menu.addAction("Táctico / Grande (450px)").triggered.connect(lambda: self.set_scale("grande"))

        self.menu.addSeparator()

        self.action_breathing = self.menu.addAction("🌬️ Respiración Orgánica")
        self.action_breathing.setCheckable(True)
        self.action_breathing.setChecked(getattr(self, 'breathing_enabled', True))
        self.action_breathing.triggered.connect(self.toggle_breathing)

        self.menu.addSeparator()

        action_exit = self.menu.addAction("🔌 Desconectar (Salir)")
        action_exit.triggered.connect(self.clean_exit)

    def keyPressEvent(self, event):
        if event.key() == Qt.Key.Key_F2:
            self.toggle_mic_recording()
            event.accept()
        elif event.key() == Qt.Key.Key_F3:
            self.toggle_continuous_voice()
            event.accept()
        elif event.key() == Qt.Key.Key_Escape:
            self.stop_current_speech()
            event.accept()
        else:
            super().keyPressEvent(event)

    def toggle_mic_recording(self):
        """Activa o desactiva la captura activa por micrófono (Push-to-Talk)."""
        if self.listen_thread and self.listen_thread.isRunning():
            self.listen_thread.stop_recording()
            self.bubble.set_recording_ui(False)
            if hasattr(self, 'audio_ducker') and self.media_player.playbackState() != QMediaPlayer.PlaybackState.PlayingState:
                self.audio_ducker.restore_audio(duration_ms=350)
            return

        # Si el modo continuo estaba activo, apagarlo para no solapar
        if self.continuous_voice_mode:
            self.continuous_voice_mode = False
            self.bubble.set_continuous_ui(False)
            if hasattr(self, "action_continuous"):
                self.action_continuous.setChecked(False)

        # Atenuar música del sistema para escucha limpia
        if hasattr(self, 'audio_ducker'):
            self.audio_ducker.duck_audio(target_factor=0.20, duration_ms=150)

        self.bubble.set_recording_ui(True)
        self.show_dialogue("🎙️ [RECEPTOR DE VOZ ACTIVO]: Te escucho, Operador... Habla ahora.")
        self.set_state(PetState.TALK)

        self.listen_thread = VoiceInputWorker(sample_rate=16000, max_seconds=7.0, continuous=False)
        self.listen_thread.transcription_ready.connect(self.on_transcription_success)
        self.listen_thread.error_signal.connect(self.on_transcription_error)
        
        def _on_rec_finished():
            self.bubble.set_recording_ui(False)
            if not getattr(self, 'gemini_thread', None) or not self.gemini_thread.isRunning():
                if hasattr(self, 'audio_ducker') and not self.continuous_voice_mode and self.media_player.playbackState() != QMediaPlayer.PlaybackState.PlayingState:
                    self.audio_ducker.restore_audio(duration_ms=350)
        self.listen_thread.finished.connect(_on_rec_finished)
        self.listen_thread.start()

    def toggle_continuous_voice(self):
        """Alterna el modo de conversación continua con micrófono siempre abierto."""
        self.continuous_voice_mode = not self.continuous_voice_mode
        self.bubble.set_continuous_ui(self.continuous_voice_mode)
        if hasattr(self, "action_continuous"):
            self.action_continuous.setChecked(self.continuous_voice_mode)

        if self.continuous_voice_mode:
            # Detener cualquier grabación puntual en curso
            if self.listen_thread and self.listen_thread.isRunning():
                self.listen_thread.stop_recording()
                self.listen_thread.wait(200)
            if hasattr(self, 'audio_ducker'):
                self.audio_ducker.duck_audio(target_factor=0.20, duration_ms=150)
            self.show_dialogue("🟢 [MODO MANOS LIBRES ACTIVO]: Micrófono abierto. Háblame libremente sin pulsar ningún botón.")
            self.start_continuous_listening()
        else:
            if self.listen_thread and self.listen_thread.isRunning():
                self.listen_thread.stop_recording()
            if hasattr(self, 'audio_ducker') and self.media_player.playbackState() != QMediaPlayer.PlaybackState.PlayingState:
                self.audio_ducker.restore_audio(duration_ms=350)
            self.set_state(PetState.IDLE)
            self.show_dialogue("⏸️ [MANOS LIBRES EN PAUSA]: Micrófono en reposo. Pulsa 🔁 o F3 para reactivarlo.")

    def start_continuous_listening(self):
        """Inicia un ciclo de escucha continua con cancelación de eco."""
        if not self.continuous_voice_mode:
            return
        # No escuchar mientras Rebeca esté reproduciendo voz o procesando en Gemini
        if self.media_player.playbackState() == QMediaPlayer.PlaybackState.PlayingState:
            return
        if self.gemini_thread and self.gemini_thread.isRunning():
            return
        if self.listen_thread and self.listen_thread.isRunning():
            return

        self.set_state(PetState.TALK)
        self.listen_thread = VoiceInputWorker(sample_rate=16000, max_seconds=7.0, continuous=True)
        self.listen_thread.transcription_ready.connect(self.on_transcription_success)
        self.listen_thread.silence_signal.connect(self.on_continuous_silence)
        self.listen_thread.error_signal.connect(self.on_continuous_error)
        self.listen_thread.start()

    @Slot()
    def on_continuous_silence(self):
        """Si no hubo voz en el intervalo, rearmar el ciclo de escucha de forma transparente."""
        if self.continuous_voice_mode:
            QTimer.singleShot(250, self.start_continuous_listening)
        else:
            self.set_state(PetState.IDLE)

    @Slot(str)
    def on_continuous_error(self, err_msg: str):
        """Gestión no intrusiva de incidencias de audio en modo continuo."""
        if self.continuous_voice_mode:
            QTimer.singleShot(500, self.start_continuous_listening)
        else:
            self.set_state(PetState.IDLE)

    @Slot(str)
    def on_transcription_success(self, recognized_text: str):
        self.bubble.input_field.setText(recognized_text)
        if getattr(self, 'presenter_echo_mode', False):
            self.show_dialogue(f"🎙️ [ECO OBS]: \"{recognized_text}\"")
            QTimer.singleShot(350, lambda: self.speak_text(recognized_text))
        else:
            self.show_dialogue(f"🎙️ Has dicho: \"{recognized_text}\"")
            # Enviar automáticamente a Gemini y sintetizar respuesta
            QTimer.singleShot(600, lambda: self.process_query(recognized_text))

    def speak_text(self, text: str):
        """Sintetiza y reproduce un texto directamente con lip-sync (Teleprompter / Eco OBS)."""
        try:
            # 1. Detener reproducción previa si está activa y liberar buffer
            if self.media_player.playbackState() == QMediaPlayer.PlaybackState.PlayingState:
                self.media_player.stop()
            self.media_player.setSource(QUrl())

            # 2. Detener y desconectar hilo de voz anterior si aún estaba sintetizando
            if self.voice_thread and self.voice_thread.isRunning():
                try:
                    self.voice_thread.audio_ready.disconnect()
                except Exception:
                    pass
                try:
                    self.voice_thread.error_signal.disconnect()
                except Exception:
                    pass
                try:
                    self.voice_thread.quit()
                    self.voice_thread.wait(200)
                except Exception:
                    pass

            # 3. Detener captura activa de micrófono para evitar solapamientos
            if self.listen_thread and self.listen_thread.isRunning():
                self.listen_thread.stop_recording()

            self.active_speaking_state = PetState.TALK
            self.current_user_query = ""
            self.show_dialogue(text)
            voice = self.config.get("voice", DEFAULT_VOICE)
            rate = self.config.get("rate", DEFAULT_RATE)
            pitch = self.config.get("pitch", DEFAULT_PITCH)
            new_audio_path = self.get_new_audio_cache_path()
            self.voice_thread = VoiceWorker(text, voice, new_audio_path, rate=rate, pitch=pitch)
            self.voice_thread.audio_ready.connect(self.play_voice)
            self.voice_thread.error_signal.connect(self.on_tts_error)
            self.voice_thread.start()
        except Exception as e:
            self.on_tts_error(f"Fallo al emitir voz: {e}")

    def open_teleprompter_dialog(self):
        """Abre la ventana de teleprompter para que Rebeca lea un guión en directo en OBS de forma continua."""
        # Detener cualquier locución previa
        if self.media_player.playbackState() == QMediaPlayer.PlaybackState.PlayingState:
            self.media_player.stop()
        self.media_player.setSource(QUrl())

        # Pausar temporalmente modo manos libres para no capturar ruido al teclear
        was_continuous = self.continuous_voice_mode
        if was_continuous:
            self.continuous_voice_mode = False
            self.bubble.set_continuous_ui(False)
            if hasattr(self, 'action_continuous'):
                self.action_continuous.setChecked(False)
            if self.listen_thread and self.listen_thread.isRunning():
                self.listen_thread.stop_recording()

        if not hasattr(self, 'teleprompter_dialog') or self.teleprompter_dialog is None:
            self.teleprompter_dialog = TeleprompterDialog(self)
            self.teleprompter_dialog.text_transmitted.connect(self.speak_text)
            self.teleprompter_dialog.closed.connect(self.on_teleprompter_closed)

        # Ocultar temporalmente la barra / burbuja de Rebeca mientras el teleprompter esté abierto
        self.bubble.hide()

        self.teleprompter_dialog.show()
        self.teleprompter_dialog.raise_()
        self.teleprompter_dialog.activateWindow()

    def on_teleprompter_closed(self):
        """Restaura la barra de diálogo de Rebeca cuando se cierra el teleprompter."""
        if not self.bubble.isVisible():
            self.update_bubble_position()
            self.bubble.show()
            self.bubble.raise_()

    def toggle_presenter_echo(self):
        """Alterna el modo eco/repetidora de noticias para OBS."""
        self.presenter_echo_mode = not getattr(self, 'presenter_echo_mode', False)
        if hasattr(self, 'action_echo'):
            self.action_echo.setChecked(self.presenter_echo_mode)
        if self.presenter_echo_mode:
            self.show_dialogue("🎙️ [MODO ECO OBS ACTIVO]: Dicta por el micro o escribe en el chat y Rebeca repetirá tu noticia con su voz y lip-sync.")
        else:
            self.show_dialogue("⏹️ [MODO ECO OBS DESACTIVADO]: Volviendo a interacción con IA normal.")

    def set_studio_style(self, style: str):
        """Cambia el estilo visual del plató de presentadora / OBS."""
        self.active_studio_style = style
        self.config["studio_style"] = style
        self.save_config()
        self.load_all_sprites()
        self.manual_sleep_active = False
        self.current_state = PetState.IDLE
        self.anim_frame_index = 0
        self.update_window_size()
        self.update()
        name_map = {
            "normal": "Mascota Estándar",
            "reportera": "Reportera (Con Micrófono)",
            "telediario": "Telediario (Mesa de Noticias)",
            "streamer": "Streamer (Webcam y Auriculares)"
        }
        self.show_dialogue(f"🎬 Plató activo: {name_map.get(style, style)}")

    @Slot(str)
    def on_transcription_error(self, err_msg: str):
        self.set_state(PetState.IDLE)
        self.show_dialogue(f"[AVISO MICRO]: {err_msg}")

    def set_voice(self, voice_name: str, rate: str = "+5%", pitch: str = "+0Hz"):
        self.config["voice"] = voice_name
        self.config["rate"] = rate
        self.config["pitch"] = pitch
        self.save_config()
        self.show_dialogue(f"⚡ Voz calibrada: {voice_name}")
        self.test_current_voice()

    def set_voice_speed(self, rate_val: str):
        self.config["rate"] = rate_val
        self.save_config()
        self.show_dialogue(f"⚡ Velocidad vocal: {rate_val}")
        self.test_current_voice()

    def test_current_voice(self):
        try:
            self.active_speaking_state = PetState.TALK
            self.current_user_query = ""
            voice = self.config.get("voice", DEFAULT_VOICE)
            rate = self.config.get("rate", DEFAULT_RATE)
            pitch = self.config.get("pitch", DEFAULT_PITCH)
            test_msg = "Enlace vocal calibrado, Mayor. Sistemas de transmisión en línea."
            new_audio_path = self.get_new_audio_cache_path()
            if self.voice_thread and self.voice_thread.isRunning():
                try:
                    self.voice_thread.audio_ready.disconnect()
                except Exception:
                    pass
                try:
                    self.voice_thread.error_signal.disconnect()
                except Exception:
                    pass
                try:
                    self.voice_thread.quit()
                    self.voice_thread.wait(200)
                except Exception:
                    pass
            self.voice_thread = VoiceWorker(test_msg, voice, new_audio_path, rate=rate, pitch=pitch)
            self.voice_thread.audio_ready.connect(self.play_voice)
            self.voice_thread.error_signal.connect(self.on_tts_error)
            self.voice_thread.start()
        except Exception as e:
            self.on_tts_error(f"Fallo al probar voz: {e}")

    def open_api_key_dialog(self):
        current_key = self.config.get("gemini_api_key", "")
        dlg = ApiKeyDialog(current_key, self)
        if dlg.exec():
            new_key = dlg.get_key()
            self.config["gemini_api_key"] = new_key
            self.save_config()
            self.show_dialogue("⚡ Enlace neural con Google Gemini sincronizado con éxito.")

    def open_backpack_dialog(self):
        dlg = BackpackDialog(self.backpack_mgr, self)
        dlg.exec()
        stats = self.backpack_mgr.get_telemetry_stats()
        self.show_dialogue(
            f"🎒 [MOCHILA SINCRONIZADA]: {stats['file_count']} archivos activos "
            f"({stats['total_kb']} KB) enlazados a mi memoria neural."
        )

    def report_live_weather(self):
        self.show_dialogue("📡 Conectando con satélites meteorológicos...")
        self.current_user_query = "Telemetría de clima y temperatura local"
        self.active_speaking_state = PetState.TALK
        def _fetch():
            telemetry = get_live_weather_telemetry(timeout=3.5)
            if telemetry.get("success"):
                self.on_gemini_success(telemetry["summary"], "Open-Meteo & Geo-IP", forced_state=PetState.TALK)
            else:
                self.show_dialogue("⚠️ No fue posible recibir la telemetría climática local.")
        QTimer.singleShot(200, _fetch)

    def set_scale(self, preset_name: str):
        if preset_name in SCALE_PRESETS:
            self.current_height = SCALE_PRESETS[preset_name]
            self.config["scale"] = preset_name
            self.save_config()
            self.update_window_size()
            self.update_bubble_position()
            self.update()

    def toggle_breathing(self):
        """Alterna el movimiento vertical de respiración orgánica senoidal."""
        self.breathing_enabled = not getattr(self, 'breathing_enabled', True)
        self.config["breathing_enabled"] = self.breathing_enabled
        self.save_config()
        if hasattr(self, 'action_breathing'):
            self.action_breathing.setChecked(self.breathing_enabled)
        is_studio = getattr(self, 'active_studio_style', 'normal') != 'normal'
        if is_studio:
            estado = "en espera (en plató OBS la mesa y micro permanecen fijos)" if self.breathing_enabled else "desactivada"
        else:
            estado = "activada (vaivén natural)" if self.breathing_enabled else "pausada (sprite fijo y anclado)"
        self.show_dialogue(f"🌬️ Respiración orgánica {estado}.")
        self.update()

    def show_dialogue(self, text: str, sources: str = ""):
        self.bubble.set_content(text, sources)
        # Si el teleprompter está activo en pantalla, mantener la barra de Rebeca oculta
        if hasattr(self, 'teleprompter_dialog') and self.teleprompter_dialog and self.teleprompter_dialog.isVisible():
            return
        self.update_bubble_position()
        self.bubble.show()
        self.bubble.raise_()

    def clean_exit(self):
        try:
            self.continuous_voice_mode = False
            if hasattr(self, 'sensor_timer') and self.sensor_timer.isActive():
                self.sensor_timer.stop()
            if hasattr(self, 'blink_timer') and self.blink_timer.isActive():
                self.blink_timer.stop()
            if hasattr(self, 'end_blink_timer') and self.end_blink_timer.isActive():
                self.end_blink_timer.stop()
            if hasattr(self, 'lip_sync_analyzer') and hasattr(self.lip_sync_analyzer, 'decoder'):
                self.lip_sync_analyzer.decoder.stop()
            if hasattr(self, 'query_watchdog') and self.query_watchdog:
                self.query_watchdog.stop()
            if self.anim_timer.isActive():
                self.anim_timer.stop()
            if self.gemini_thread and self.gemini_thread.isRunning():
                self.gemini_thread.quit()
                self.gemini_thread.wait(400)
            if self.voice_thread and self.voice_thread.isRunning():
                self.voice_thread.quit()
                self.voice_thread.wait(400)
            if self.listen_thread and self.listen_thread.isRunning():
                self.listen_thread.stop_recording()
                self.listen_thread.wait(500)
            self.media_player.stop()
            if hasattr(self, 'audio_ducker'):
                self.audio_ducker.emergency_restore_all()
            if hasattr(self, 'teleprompter_dialog') and self.teleprompter_dialog:
                self.teleprompter_dialog.close()
            self.bubble.close()
            self.close()
            QApplication.quit()
        except Exception:
            sys.exit(0)

    def closeEvent(self, event):
        self.clean_exit()
        event.accept()


# Alias táctico para compatibilidad y pruebas de integración
DesktopPet = CyberPetWindow


def main():
    try:
        base_dir = os.path.dirname(os.path.abspath(__file__))
        log_file = os.path.join(base_dir, "companion.log")
        if sys.stdout is None:
            sys.stdout = open(log_file, "a", encoding="utf-8")
        if sys.stderr is None:
            sys.stderr = open(log_file, "a", encoding="utf-8")

        app = QApplication(sys.argv)
        app.setApplicationName("CyberCompanion_Rebeca")

        companion = CyberPetWindow(base_dir)
        companion.show()

        sys.exit(app.exec())
    except Exception as e:
        import traceback
        err_msg = traceback.format_exc()
        try:
            with open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "companion_crash.log"), "w", encoding="utf-8") as f:
                f.write(err_msg)
        except Exception:
            pass
        raise


if __name__ == "__main__":
    main()
