# 🎨 Conexión con Suite de Sprites: Sprite Artist 2D Pro

Este documento vincula el proyecto **Cyber-Companion** con su suite generadora en **Google Flow Tools**.

---

## 📁 Proyecto de la Herramienta

- **Ubicación Independiente:** [`c:\Users\asakura\Documents\sprite_artist_2d`](file:///c:/Users/asakura/Documents/sprite_artist_2d)
- **Copia Interna en Herramientas:** [`tools/flow_sprite_artist/`](file:///c:/Users/asakura/Documents/cyber_companion/tools/flow_sprite_artist/)
- **Imagen Maestra Canónica:** [`assets/rebeca_master_reference.jpg`](file:///c:/Users/asakura/Documents/cyber_companion/assets/rebeca_master_reference.jpg)

---

## 💬 Sesión de Chat de Creación y Soporte (Antigravity)

- **ID de Conversación:** `2ebad5c1-d192-403b-b0e5-77370859559e`
- **Enlace Directo:** [Abrir Chat de Creación: Sprite Artist 2D Pro](conversation://2ebad5c1-d192-403b-b0e5-77370859559e)

---

## ⚡ Flujo de Trabajo para Reemplazo de Sprites

1. Abrir la app en Google Flow y vincular [`assets/rebeca_master_reference.jpg`](file:///c:/Users/asakura/Documents/cyber_companion/assets/rebeca_master_reference.jpg).
2. Seleccionar el preset deseado:
   - **Preset Cyber-Companion (Mascota):** Genera `IDLE`, `TALK`, `WALK` (4-frames), `SLEEP`, `MUSIC` y `THINK`.
   - **Preset Modo Estudio (Presentadora OBS):** Genera `STUDIO_IDLE`, `STUDIO_TALK`, `STUDIO_EXPLAIN`, `STUDIO_ALERT`, `STUDIO_MIC` y `STUDIO_DESK` (Mesa de plató).
3. Descargar el ZIP estructurado y descomprimir en `tools/entrada/`.
4. Ejecutar:
   ```powershell
   python tools/sprite_processor.py
   ```
5. Los nuevos sprites sustituirán automáticamente los estados en [`assets/sprites/`](file:///c:/Users/asakura/Documents/cyber_companion/assets/sprites/).
