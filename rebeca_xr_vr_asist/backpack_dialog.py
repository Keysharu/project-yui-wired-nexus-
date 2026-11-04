"""
=============================================================================
MODAL LAINOS: MOCHILA DE DATOS Y GESTIÓN DE ARCHIVOS DE USUARIO
=============================================================================
Permite a cualquier usuario final:
- Ver los archivos de conocimiento cargados en la memoria de Rebeca
- Añadir nuevos archivos mediante selector nativo de Windows (QFileDialog)
- Abrir la carpeta en el explorador para arrastrar y soltar (drag & drop)
- Eliminar documentos y purgar historial de conversación
=============================================================================
"""

from pathlib import Path
from PySide6.QtCore import Qt
from PySide6.QtWidgets import (
    QDialog, QVBoxLayout, QHBoxLayout, QLabel, QPushButton,
    QListWidget, QListWidgetItem, QFileDialog, QMessageBox, QFrame
)

COLOR_BG_DARK = "#090b0e"
COLOR_BG_PANEL = "#121620"
COLOR_BG_HEADER = "#1a1e2e"
COLOR_NEON_CYAN = "#00ffcc"
COLOR_NEON_MAGENTA = "#ff007f"
COLOR_TERMINAL_GREEN = "#39ff14"
COLOR_TEXT_MAIN = "#d8e2ed"
COLOR_TEXT_MUTED = "#60728c"

STYLE_BACKPACK_DIALOG = f"""
QDialog {{
    background-color: {COLOR_BG_PANEL};
    border: 2px solid {COLOR_NEON_CYAN};
    border-radius: 8px;
}}
QLabel#TitleLabel {{
    color: {COLOR_NEON_CYAN};
    font-family: 'Consolas', monospace;
    font-size: 13px;
    font-weight: bold;
}}
QLabel#SubtitleLabel {{
    color: {COLOR_TEXT_MUTED};
    font-family: 'Consolas', monospace;
    font-size: 10px;
}}
QLabel#StatsLabel {{
    color: {COLOR_TERMINAL_GREEN};
    font-family: 'Consolas', monospace;
    font-size: 10px;
    background-color: {COLOR_BG_DARK};
    border: 1px solid #1e2638;
    border-radius: 4px;
    padding: 6px 10px;
}}
QListWidget {{
    background-color: {COLOR_BG_DARK};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 6px;
    color: {COLOR_TEXT_MAIN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    padding: 6px;
}}
QListWidget::item {{
    padding: 8px 10px;
    border-bottom: 1px solid #192130;
}}
QListWidget::item:selected {{
    background-color: {COLOR_BG_HEADER};
    color: {COLOR_NEON_CYAN};
    border-left: 3px solid {COLOR_NEON_CYAN};
}}
QPushButton {{
    background-color: {COLOR_BG_HEADER};
    border: 1px solid {COLOR_NEON_CYAN};
    border-radius: 4px;
    color: {COLOR_NEON_CYAN};
    font-family: 'Consolas', monospace;
    font-size: 11px;
    font-weight: bold;
    padding: 6px 12px;
}}
QPushButton:hover {{
    background-color: {COLOR_NEON_CYAN};
    color: {COLOR_BG_DARK};
}}
QPushButton#DeleteBtn {{
    border: 1px solid {COLOR_NEON_MAGENTA};
    color: {COLOR_NEON_MAGENTA};
}}
QPushButton#DeleteBtn:hover {{
    background-color: {COLOR_NEON_MAGENTA};
    color: #ffffff;
}}
"""


class BackpackDialog(QDialog):
    def __init__(self, backpack_manager, parent=None):
        super().__init__(parent)
        self.manager = backpack_manager
        self.setWindowTitle("MOCHILA DE DATOS & MEMORIA PERSISTENTE")
        self.setStyleSheet(STYLE_BACKPACK_DIALOG)
        self.setFixedSize(540, 440)

        layout = QVBoxLayout(self)
        layout.setContentsMargins(18, 16, 18, 16)
        layout.setSpacing(10)

        # Cabecera
        self.title_lbl = QLabel("🎒 MOCHILA DE DATOS & MEMORIA NEURAL", self)
        self.title_lbl.setObjectName("TitleLabel")
        layout.addWidget(self.title_lbl)

        self.sub_lbl = QLabel(
            "Archivos de contexto personal para Rebeca (.md, .txt, .json).\n"
            "Puedes subir documentos, notas o arrastrarlos directamente a la carpeta.",
            self
        )
        self.sub_lbl.setObjectName("SubtitleLabel")
        self.sub_lbl.setWordWrap(True)
        layout.addWidget(self.sub_lbl)

        # Lista de archivos
        self.file_list = QListWidget(self)
        layout.addWidget(self.file_list)

        # Barra de telemetría de almacenamiento
        self.stats_lbl = QLabel("", self)
        self.stats_lbl.setObjectName("StatsLabel")
        layout.addWidget(self.stats_lbl)

        # Botonera de acciones
        btn_layout = QHBoxLayout()
        btn_layout.setSpacing(8)

        self.add_btn = QPushButton("➕ Añadir Archivo", self)
        self.add_btn.setToolTip("Seleccionar archivos de tu ordenador para la memoria de Rebeca")
        self.add_btn.clicked.connect(self.on_add_file)

        self.open_btn = QPushButton("📂 Abrir Carpeta", self)
        self.open_btn.setToolTip("Abrir carpeta de la mochila en el Explorador de Windows para arrastrar y soltar")
        self.open_btn.clicked.connect(self.on_open_folder)

        self.del_btn = QPushButton("🗑️ Eliminar", self)
        self.del_btn.setObjectName("DeleteBtn")
        self.del_btn.setToolTip("Eliminar el archivo seleccionado de la mochila")
        self.del_btn.clicked.connect(self.on_delete_file)

        self.clear_history_btn = QPushButton("🧹 Purgar Chat", self)
        self.clear_history_btn.setObjectName("DeleteBtn")
        self.clear_history_btn.setToolTip("Borrar historial de conversación reciente")
        self.clear_history_btn.clicked.connect(self.on_clear_history)

        btn_layout.addWidget(self.add_btn)
        btn_layout.addWidget(self.open_btn)
        btn_layout.addWidget(self.del_btn)
        btn_layout.addWidget(self.clear_history_btn)
        layout.addLayout(btn_layout)

        # Botón cerrar
        close_layout = QHBoxLayout()
        close_layout.addStretch()
        self.close_btn = QPushButton("Sincronizar y Cerrar", self)
        self.close_btn.clicked.connect(self.accept)
        close_layout.addWidget(self.close_btn)
        layout.addLayout(close_layout)

        self.refresh_file_list()

    def refresh_file_list(self):
        self.file_list.clear()
        files = self.manager.get_backpack_files()
        for f in files:
            item_text = f"📄 {f['name']}  ({f['size_kb']} KB)  [🟢 Activo en memoria]"
            item = QListWidgetItem(item_text)
            item.setData(Qt.ItemDataRole.UserRole, f["name"])
            self.file_list.addItem(item)

        stats = self.manager.get_telemetry_stats()
        self.stats_lbl.setText(
            f"⚡ Telemetría: {stats['file_count']} archivos activos | "
            f"{stats['total_kb']} KB de contexto | {stats['history_turns']} turnos de chat en memoria"
        )

    def on_add_file(self):
        file_paths, _ = QFileDialog.getOpenFileNames(
            self,
            "Seleccionar documentos para la mochila de Rebeca",
            "",
            "Archivos de texto (*.md *.txt *.json *.csv);;Todos los archivos (*.*)"
        )
        if file_paths:
            added = self.manager.add_files(file_paths)
            self.refresh_file_list()
            QMessageBox.information(
                self,
                "Mochila Actualizada",
                f"Se han incorporado {added} archivo(s) a la memoria neural de Rebeca."
            )

    def on_open_folder(self):
        self.manager.open_folder()

    def on_delete_file(self):
        current_item = self.file_list.currentItem()
        if not current_item:
            QMessageBox.warning(self, "Selección requerida", "Selecciona un archivo de la lista para eliminar.")
            return

        filename = current_item.data(Qt.ItemDataRole.UserRole)
        confirm = QMessageBox.question(
            self,
            "Confirmar eliminación",
            f"¿Deseas retirar '{filename}' de la mochila de Rebeca?",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
        )
        if confirm == QMessageBox.StandardButton.Yes:
            self.manager.remove_file(filename)
            self.refresh_file_list()

    def on_clear_history(self):
        confirm = QMessageBox.question(
            self,
            "Purgar historial",
            "¿Deseas vaciar la memoria reciente de conversación? Rebeca empezará un hilo limpio.",
            QMessageBox.StandardButton.Yes | QMessageBox.StandardButton.No
        )
        if confirm == QMessageBox.StandardButton.Yes:
            self.manager.clear_chat_history()
            self.refresh_file_list()
