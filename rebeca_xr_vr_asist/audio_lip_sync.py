import os
import sys
import numpy as np
from pathlib import Path
from PySide6.QtCore import QObject, Signal, Slot, QUrl, QTimer, QCoreApplication
from PySide6.QtMultimedia import QAudioDecoder, QAudioBuffer, QAudioFormat

class AudioLipSyncAnalyzer(QObject):
    """
    Analizador de sincronización labial fonética y acústica (LainOS Studio Suite).
    Decodifica el archivo MP3 generado por Edge-TTS y calcula la pista de visemas
    frame a frame (cada 40ms = 25 FPS) aplicando las leyes de animación anime:
    - Fast Attack (10ms): Salto inmediato al detectar energía vocal.
    - Vocal Hold (80-120ms): Mantiene la boca viva entre sílabas (sin aleteo 'muppet').
    - Clean Silence Snap: Cierre estricto en comas, puntos y pausas reales.
    - Análisis Espectral FFT: Discriminación de formantes y consonantes mediante Centroide Espectral.
    - Micro-Inercia Física: Amortiguación elástica vertical (0.0 a 1.5 px) reactiva al peso del habla.
    """
    analysis_ready = Signal(str) # Emite la ruta cuando el análisis está listo

    def __init__(self, parent=None):
        super().__init__(parent)
        self.decoder = QAudioDecoder(self)
        self.decoder.bufferReady.connect(self._on_buffer_ready)
        self.decoder.finished.connect(self._on_finished)
        self.decoder.error.connect(self._on_error)

        self.current_audio_path = ""
        self._raw_samples = []
        self._sample_rate = 24000
        self.chunk_duration_ms = 40 # 40ms = 25 FPS

        # Pistas precalculadas
        self.rms_timeline = []
        self.centroids = []
        self.inertia_timeline = []
        self.streamer_visemes = []
        self.normal_visemes = []
        self.is_ready = False

    def analyze_file(self, audio_path: str):
        """Inicia el análisis asíncrono del archivo de audio."""
        try:
            self.current_audio_path = audio_path
            self._raw_samples = []
            self.rms_timeline = []
            self.centroids = []
            self.inertia_timeline = []
            self.streamer_visemes = []
            self.normal_visemes = []
            self.is_ready = False

            if not os.path.exists(audio_path):
                print(f"[!] Archivo no encontrado para análisis de lip-sync: {audio_path}")
                return

            self.decoder.stop()
            self.decoder.setSource(QUrl.fromLocalFile(audio_path))
            self.decoder.start()
        except Exception as e:
            print(f"[!] Error iniciando decodificación de lip-sync: {e}")

    def _on_buffer_ready(self):
        try:
            buf = self.decoder.read()
            if buf.isValid():
                fmt = buf.format()
                sr = fmt.sampleRate()
                if sr > 0:
                    self._sample_rate = sr
                data = buf.constData()
                s_fmt = fmt.sampleFormat()
                if s_fmt == QAudioFormat.SampleFormat.Float:
                    arr = np.frombuffer(data, dtype=np.float32)
                elif s_fmt == QAudioFormat.SampleFormat.Int32:
                    arr = np.frombuffer(data, dtype=np.int32).astype(np.float32) / 2147483648.0
                elif s_fmt == QAudioFormat.SampleFormat.UInt8:
                    arr = (np.frombuffer(data, dtype=np.uint8).astype(np.float32) - 128.0) / 128.0
                else:  # Default Int16
                    arr = np.frombuffer(data, dtype=np.int16).astype(np.float32) / 32768.0
                self._raw_samples.extend(arr)
        except Exception as e:
            print(f"[!] Error leyendo buffer de audio: {e}")

    def _on_finished(self):
        try:
            if not self._raw_samples:
                self.is_ready = False
                return

            samples = np.array(self._raw_samples, dtype=np.float32)
            peak = np.max(np.abs(samples))
            if peak > 0:
                samples /= peak

            window_size = int(self._sample_rate * (self.chunk_duration_ms / 1000.0))
            if window_size <= 0:
                window_size = 960

            # 1. Calcular envolvente RMS, Zero-Crossing Rate y Centroide Espectral por FFT
            raw_rms = []
            zcr_list = []
            centroid_list = []

            for i in range(0, len(samples), window_size):
                chunk = samples[i:i + window_size]
                if len(chunk) > 0:
                    # RMS
                    rms = float(np.sqrt(np.mean(chunk**2)))
                    # ZCR (Zero-Crossing Rate)
                    zcr = float(np.mean(np.abs(np.diff(np.sign(chunk)))) / 2.0)

                    # FFT con ventana de Hann para reducir fugas espectrales
                    hann = np.hanning(len(chunk))
                    fft_vals = np.abs(np.fft.rfft(chunk * hann))
                    freqs = np.fft.rfftfreq(len(chunk), d=1.0 / self._sample_rate)
                    sum_fft = float(np.sum(fft_vals))
                    if sum_fft > 1e-6:
                        centroid = float(np.sum(freqs * fft_vals) / sum_fft)
                    else:
                        centroid = 0.0

                    raw_rms.append(rms)
                    zcr_list.append(zcr)
                    centroid_list.append(centroid)
                else:
                    raw_rms.append(0.0)
                    zcr_list.append(0.0)
                    centroid_list.append(0.0)

            self.rms_timeline = raw_rms
            self.centroids = centroid_list

            # 2. Aplicar el algoritmo Anime Hysteresis & Vocal Hold con Formantes FFT
            SILENCE_THRESH = 0.035
            MID_THRESH = 0.18
            HIGH_THRESH = 0.42

            hold_counter = 0
            streamer_track = []
            normal_track = []

            for idx, (rms, zcr, centroid) in enumerate(zip(raw_rms, zcr_list, centroid_list)):
                if rms < SILENCE_THRESH:
                    hold_counter += 1
                    # Cierre limpio si el silencio supera 120ms (3 chunks)
                    if hold_counter >= 3:
                        s_vis = "stream_idle"
                        n_vis = "idle"
                    else:
                        # Breve pausa sintáctica: mantener entreabierta
                        s_vis = "stream_talk_1"
                        n_vis = "talk_1"
                else:
                    hold_counter = 0
                    if rms >= HIGH_THRESH:
                        # Vocal abierta amplia o énfasis sonoro ('A', 'O')
                        s_vis = "stream_talk_3"
                        n_vis = "talk_3" if (idx % 2 == 0) else "talk_5"
                    elif rms >= MID_THRESH:
                        # Articulación media modulada por Centroide Espectral (Formantes)
                        if centroid < 1400.0 and zcr < 0.10:
                            # Formantes graves: vocal redonda ('O', 'U', o 'A' media)
                            s_vis = "stream_talk_2"
                            n_vis = "talk_2"
                        elif centroid >= 2600.0 or zcr > 0.12:
                            # Formantes agudos / fricativas ('S', 'T', 'CH', 'F'): articulación dental
                            s_vis = "stream_talk_1"
                            n_vis = "talk_4"
                        else:
                            # Formantes medios: vocal clara ('E', 'I')
                            s_vis = "stream_talk_2"
                            n_vis = "talk_2" if (idx % 2 == 0) else "talk_4"
                    else:
                        # Fonación suave inicial o consonante oclusiva
                        s_vis = "stream_talk_1"
                        n_vis = "talk_1"

                streamer_track.append(s_vis)
                normal_track.append(n_vis)

            self.streamer_visemes = streamer_track
            self.normal_visemes = normal_track

            # 3. Calcular Micro-Inercia Física de Locución (Speech Weight)
            # Amortigua el cuerpo sutilmente (0.0 a 1.5 px) según la energía acústica con filtro paso-bajo.
            inertia_track = []
            smoothed_val = 0.0
            for rms in raw_rms:
                if rms < SILENCE_THRESH:
                    target_inertia = 0.0
                else:
                    target_inertia = min(1.5, max(0.0, ((rms - SILENCE_THRESH) / (HIGH_THRESH - SILENCE_THRESH)) * 1.5))
                # Filtro exponencial para rebote orgánico
                smoothed_val = smoothed_val * 0.65 + target_inertia * 0.35
                inertia_track.append(float(smoothed_val))

            self.inertia_timeline = inertia_track
            self.is_ready = True
            self.analysis_ready.emit(self.current_audio_path)
        except Exception as e:
            print(f"[!] Error procesando timeline de lip-sync: {e}")
            self.is_ready = False

    def _on_error(self, err):
        print(f"[!] Incidencia en QAudioDecoder: {err}")
        self.is_ready = False

    def get_viseme_at_time(self, time_ms: int, style: str = "normal") -> str:
        """Devuelve el nombre del sprite correspondiente al milisegundo de reproducción."""
        if not self.is_ready or not self.rms_timeline:
            return ""

        chunk_idx = int(time_ms / self.chunk_duration_ms)
        if chunk_idx < 0:
            return ""
        if chunk_idx >= len(self.rms_timeline):
            return "stream_idle" if style == "streamer" else "idle"

        if style == "streamer":
            return self.streamer_visemes[chunk_idx] if chunk_idx < len(self.streamer_visemes) else "stream_idle"
        else:
            return self.normal_visemes[chunk_idx] if chunk_idx < len(self.normal_visemes) else "idle"

    def get_current_rms_at_time(self, time_ms: int) -> float:
        """Devuelve el nivel RMS en el instante solicitado."""
        if not self.is_ready or not self.rms_timeline:
            return 0.0
        chunk_idx = int(time_ms / self.chunk_duration_ms)
        if 0 <= chunk_idx < len(self.rms_timeline):
            return self.rms_timeline[chunk_idx]
        return 0.0

    def get_speech_inertia_at_time(self, time_ms: int) -> float:
        """Devuelve el desplazamiento vertical en píxeles (0.0 a 1.5 px) por micro-inercia física de locución."""
        if not self.is_ready or not self.inertia_timeline:
            return 0.0
        chunk_idx = int(time_ms / self.chunk_duration_ms)
        if 0 <= chunk_idx < len(self.inertia_timeline):
            return self.inertia_timeline[chunk_idx]
        return 0.0

    def get_spectral_centroid_at_time(self, time_ms: int) -> float:
        """Devuelve el centroide espectral (Hz) en el instante solicitado."""
        if not self.is_ready or not self.centroids:
            return 0.0
        chunk_idx = int(time_ms / self.chunk_duration_ms)
        if 0 <= chunk_idx < len(self.centroids):
            return self.centroids[chunk_idx]
        return 0.0
