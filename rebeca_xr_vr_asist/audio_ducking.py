"""
=============================================================================
MÓDULO: SMART AUDIO DUCKING (WASAPI / WINDOWS CORE AUDIO API)
=============================================================================
Atenúa suavemente el volumen de aplicaciones multimedia (Spotify, Chrome, YouTube,
VLC, reproductores) en Windows cuando el usuario habla (F2/F3) o Rebeca responde,
restaurando gradualmente el volumen previo al terminar la interacción.

Características Tácticas:
- Rampa Fade-Out rápida (~150ms): Evita cortes bruscos o clics de audio.
- Rampa Fade-In suave (~350ms): Transición cinematográfica al volumen original.
- Lista de Exclusión Táctica: Inmune a OBS Studio, Discord, Python y servicios del sistema.
- Fail-Safe COM: Restauración inmediata en parada forzada o cierre de aplicación.
=============================================================================
"""

import os
import sys
from typing import Dict, List, Optional
from PySide6.QtCore import QObject, QTimer, Signal, Slot

class SmartAudioDucker(QObject):
    """
    Controlador de atenuación acústica automática inteligente para Windows.
    """
    ducking_started = Signal()
    ducking_restored = Signal()

    IGNORED_PROCESSES = {
        "python.exe", "pythonw.exe", "antigravity.exe",
        "obs64.exe", "obs32.exe", "obs.exe", "streamlabs obs.exe",
        "audiodg.exe", "system", "registry",
        "voicemeeter.exe", "voicemeeter8.exe", "voicemeeter8x64.exe",
        "discord.exe", "telegram.exe", "whatsapp.exe", "slack.exe", "teams.exe",
        "steamwebhelper.exe", "nvcontainer.exe", "nvidia share.exe"
    }

    def __init__(self, parent=None):
        super().__init__(parent)
        self.is_windows = (sys.platform == "win32")
        self.my_pid = os.getpid()

        # Registro de volúmenes: {session_id_or_pid: {'session': session_obj, 'orig_vol': float, 'target_vol': float, 'current_vol': float}}
        self._tracked_sessions: Dict[int, dict] = {}
        self._is_ducked = False
        self._transition_mode = "idle" # "ducking", "restoring", "idle"

        # Temporizador de interpolación de volumen (paso a paso cada 25ms)
        self._ramp_timer = QTimer(self)
        self._ramp_timer.setInterval(25)
        self._ramp_timer.timeout.connect(self._on_ramp_step)

        self._ramp_total_steps = 1
        self._ramp_current_step = 0

    @property
    def is_ducked(self) -> bool:
        return self._is_ducked

    def _get_active_multimedia_sessions(self) -> List[tuple]:
        """Obtiene las sesiones de audio activas elegibles para atenuación."""
        if not self.is_windows:
            return []

        eligible = []
        try:
            from pycaw.pycaw import AudioUtilities
            sessions = AudioUtilities.GetAllSessions()
            for session in sessions:
                try:
                    proc = session.Process
                    if not proc:
                        continue
                    pid = proc.pid
                    pname = proc.name().lower()

                    # Filtrar nuestro proceso y la lista negra de streaming/comunicaciones
                    if pid == self.my_pid or pname in self.IGNORED_PROCESSES or "python" in pname:
                        continue

                    vol_ctl = session.SimpleAudioVolume
                    if not vol_ctl:
                        continue

                    cur_vol = float(vol_ctl.GetMasterVolume())
                    if cur_vol > 0.02: # Si ya está muteada o casi inaudible no la tocamos
                        eligible.append((pid, session, vol_ctl, cur_vol))
                except Exception:
                    continue
        except Exception as e:
            print(f"[!] Error enumerando sesiones para ducking: {e}")

        return eligible

    def duck_audio(self, target_factor: float = 0.20, duration_ms: int = 150):
        """
        Atenúa suavemente las sesiones de música activas al porcentaje indicado (por defecto 20%).
        """
        if not self.is_windows:
            return

        try:
            # Si ya estamos en estado atenuado, no sobreescribir los volúmenes originales
            if not self._is_ducked and not self._tracked_sessions:
                eligible = self._get_active_multimedia_sessions()
                if not eligible:
                    self._is_ducked = True
                    return

                for pid, session, vol_ctl, cur_vol in eligible:
                    target_vol = cur_vol * max(0.0, min(1.0, target_factor))
                    self._tracked_sessions[pid] = {
                        "session": session,
                        "vol_ctl": vol_ctl,
                        "orig_vol": cur_vol,
                        "start_vol": cur_vol,
                        "target_vol": target_vol,
                        "current_vol": cur_vol
                    }
            elif self._tracked_sessions:
                # Si estábamos restaurando y entra una nueva orden, reajustamos inicio y objetivo
                for pid, info in self._tracked_sessions.items():
                    try:
                        info["start_vol"] = float(info["vol_ctl"].GetMasterVolume())
                        info["target_vol"] = info["orig_vol"] * max(0.0, min(1.0, target_factor))
                    except Exception:
                        pass

            if not self._tracked_sessions:
                self._is_ducked = True
                return

            self._is_ducked = True
            self._transition_mode = "ducking"
            self._ramp_total_steps = max(1, int(duration_ms / 25))
            self._ramp_current_step = 0
            self._ramp_timer.start()
            self.ducking_started.emit()
        except Exception as e:
            print(f"[!] Error iniciando ducking: {e}")

    def restore_audio(self, duration_ms: int = 350):
        """
        Restaura gradualmente el volumen original de todas las aplicaciones atenuadas.
        """
        if not self.is_windows or not self._tracked_sessions:
            self._is_ducked = False
            self._transition_mode = "idle"
            return

        try:
            for pid, info in self._tracked_sessions.items():
                try:
                    info["start_vol"] = float(info["vol_ctl"].GetMasterVolume())
                    info["target_vol"] = info["orig_vol"]
                except Exception:
                    pass

            self._is_ducked = False
            self._transition_mode = "restoring"
            self._ramp_total_steps = max(1, int(duration_ms / 25))
            self._ramp_current_step = 0
            self._ramp_timer.start()
        except Exception as e:
            print(f"[!] Error iniciando restauración de audio: {e}")
            self.emergency_restore_all()

    def _on_ramp_step(self):
        """Ejecuta un paso de interpolación lineal hacia el volumen objetivo."""
        self._ramp_current_step += 1
        t = min(1.0, float(self._ramp_current_step) / float(self._ramp_total_steps))

        dead_pids = []
        for pid, info in self._tracked_sessions.items():
            try:
                start_v = info["start_vol"]
                target_v = info["target_vol"]
                new_v = start_v + (target_v - start_v) * t
                new_v = max(0.0, min(1.0, new_v))
                info["vol_ctl"].SetMasterVolume(new_v, None)
                info["current_vol"] = new_v
            except Exception:
                dead_pids.append(pid)

        for d_pid in dead_pids:
            self._tracked_sessions.pop(d_pid, None)

        if t >= 1.0:
            self._ramp_timer.stop()
            if self._transition_mode == "restoring":
                self._tracked_sessions.clear()
                self._transition_mode = "idle"
                self.ducking_restored.emit()
            else:
                self._transition_mode = "idle"

    def emergency_restore_all(self):
        """
        Fail-safe de máxima prioridad: restaura de golpe el 100% de los volúmenes originales
        y limpia la memoria inmediatamente (útil en cierre o pulsación de escape).
        """
        self._ramp_timer.stop()
        self._is_ducked = False
        self._transition_mode = "idle"

        if not self.is_windows or not self._tracked_sessions:
            return

        for pid, info in list(self._tracked_sessions.items()):
            try:
                orig_v = info.get("orig_vol", 1.0)
                info["vol_ctl"].SetMasterVolume(orig_v, None)
            except Exception:
                pass

        self._tracked_sessions.clear()
        self.ducking_restored.emit()

    def cleanup(self):
        """Limpia el ducker al salir de la aplicación."""
        self.emergency_restore_all()
