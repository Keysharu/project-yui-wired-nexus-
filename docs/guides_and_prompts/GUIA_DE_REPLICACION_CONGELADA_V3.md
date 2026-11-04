# Guía Maestra de Replicación y Arquitectura Congelada (Versión V3)
**Proyecto:** *Wired Nexus MR: Invasión en El Taller de la Gatita Tech* (Meta Quest 3 / 3S & PC)  
**Operadora y Directora:** Raquel D. Torres (*El taller de la gatita tech*)  
**Orquestadora Principal:** Rebeca (Antigravity)  
**Subrutina Local de Código:** `jeny-coder:latest` (Servidor Homelab BMAX `192.168.0.29`)  
**Ubicación del Backup Congelado:** `c:\Users\asakura\Documents\juego meta\BACKUPS_CONGELADOS\Snapshot_20261030_0308_V3_Congelado`

---

## 1. Visión General de la Arquitectura (4 Pilares)

El ecosistema completo se divide en 4 bloques desacoplados que trabajan en red local con **coste 0,00 € de nube** durante el desarrollo y pruebas, manteniendo listo el conmutador a la API oficial de **Baseten** para el vídeo final del Hackathon:

1. **Nodo IA Homelab (Servidor BMAX `192.168.0.29:11434`):**
   - Ejecuta Ollama con los pesos compartidos en RAM de `llama3.2:3b` (`Q4_K_M`, `1925.8 MB`).
   - Alimenta simultáneamente a **Mina y Kira** (locutoras de Radio Chipset 107.7 FM), a **Nyx** (compañera holográfica) y a **`jeny-coder:latest`** (agente programadora local con el prompt íntegro de `AGENTE JENY.md`), con **0 MB de sobrecarga adicional en RAM** y **0 ms de latencia de intercambio**.
2. **Emisora 24/7 «Radio Chipset 107.7 FM 107.7 FM» (`radio_chipset_107_7_fm/`):**
   - Genera guiones dinámicos y música reactiva por capas (`AmbientRadio`, `NormalBattle`, `BossLevel`, `FinalBoss`).
3. **Pipeline de Sprites 2.5D y Audio DSP (`tools/i2v_sprite_suite/`):**
   - Extrae fotogramas limpios con despill de croma verde, ángulos 360° (`turn360_0..7.png`), arañas panorámicas de 8 patas completas (`1280x768`) y efectos de sonido `.wav` multicapa a `44100 Hz`.
4. **Cliente XR en Unity 6 (`quest_mr_nexus/`):**
   - Escena principal `Assets/MainMRScene.unity` compatible con **Meta Quest 3 / 3S (MRUK + Passthrough)** y con **Simulador Táctico en PC**.

---

## 2. Paso a Paso para Replicar el Entorno desde Cero

### Paso 2.1: Levantar y Verificar el Servidor BMAX (`192.168.0.29`)
Si en algún momento reinstalas el servidor BMAX o cambias de máquina local, ejecuta desde PowerShell en tu PC:

```powershell
# 1. Registrar el perfil canónico de Jeny sobre los pesos compartidos de llama3.2:3b
python "c:\Users\asakura\Documents\home lab\scripts\jeny_bmax_bridge.py" --setup

# 2. Verificar que jeny-coder:latest y llama3.2:3b comparten memoria (Zero-Swap)
python "c:\Users\asakura\Documents\home lab\scripts\jeny_bmax_bridge.py" --status
```

Para delegar cualquier tarea de programación a `jeny-coder:latest` sin gastar tokens de nube:
```powershell
python "c:\Users\asakura\Documents\home lab\scripts\jeny_bmax_bridge.py" --task "Especificación en C# o Python" --output "ruta_archivo.cs"
```

---

### Paso 2.2: Re-generar los Sprites de 8 Patas y el Audio Pesado Sci-Fi (`.wav`)
Si necesitas volver a procesar los vídeos fuente (`procesados/Cyber_spider_drone_combat_actions.mp4` y `cyber spider 360 degrees.mp4`) o regenerar los archivos `.wav` de disparo de plasma, brecha y cofres:

```powershell
python "c:\Users\asakura\Documents\juego meta\tools\i2v_sprite_suite\fix_spider_and_sfx.py"
```

Esto genera:
- `quest_mr_nexus/Assets/Sprites/Enemies/Spider_drone/combat_*.png` y `turn360_0..7.png` en formato panorámico `1280x768` (8 patas completas).
- `quest_mr_nexus/Assets/Audio/SFX/sfx_plasma_heavy_shot.wav`, `sfx_rift_open_vortex.wav`, `sfx_chest_unlock.wav` y `sfx_enemy_armor_hit.wav`.

---

### Paso 2.3: Restaurar el Proyecto desde la Copia de Seguridad Congelada
Si deseas deshacer cualquier cambio futuro y volver exactamente a este punto congelado (`V3`):

```powershell
$snap = "c:\Users\asakura\Documents\juego meta\BACKUPS_CONGELADOS\Snapshot_20261030_0308_V3_Congelado"
robocopy "$snap\quest_mr_nexus\Assets" "c:\Users\asakura\Documents\juego meta\quest_mr_nexus\Assets" /MIR
robocopy "$snap\quest_mr_nexus\ProjectSettings" "c:\Users\asakura\Documents\juego meta\quest_mr_nexus\ProjectSettings" /MIR
robocopy "$snap\quest_mr_nexus\Packages" "c:\Users\asakura\Documents\juego meta\quest_mr_nexus\Packages" /MIR
```

---

### Paso 2.4: Reconstruir la Escena y Capturas desde Unity 6
Dentro del Editor de Unity 6 (`6000.6.5f1`), tienes disponible el menú superior personalizado:
- **`Wired Nexus MR -> 4. Capturar Vista Táctica POV (1080p Blog & Ajuste)`**

O bien por línea de comandos (con el Editor cerrado):
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe" -batchmode -quit -projectPath "c:\Users\asakura\Documents\juego meta\quest_mr_nexus" -executeMethod ProjectNyx.EditorTools.TacticalPOVSnapshotter.CaptureTacticalPOV
```

---

## 3. Mapa de Scripts Clave en Unity (`Assets/Scripts/`)

| Script | Ruta | Función |
| :--- | :--- | :--- |
| `MRUKEncounterSpawner.cs` | `Assets/Scripts/Gameplay/MRUKEncounterSpawner.cs` | Máquina de estados del bucle de juego: Cofres (`+`) $\rightarrow$ Advertencia y apertura de Brecha $\rightarrow$ Oleada 1 (1 araña) $\rightarrow$ 3s espera $\rightarrow$ Oleada 2 (2 arañas) $\rightarrow$ 3s espera $\rightarrow$ Expansión de Brecha x2 + Pose de Nyx + Boss. |
| `QuantumCenturionBossAI.cs` | `Assets/Scripts/Gameplay/QuantumCenturionBossAI.cs` | IA del Boss Quantum Centurión y activación del Clímax Musical Rítmico al bajar del **35% de vida (`<= 0.35f`)**. |
| `CyberSupplyChest.cs` | `Assets/Scripts/Gameplay/CyberSupplyChest.cs` | Cofre 3D interactivo con cruz holográfica flotante `+` (se abre al dispararle, acercarse o pulsar `E`). |
| `DimensionalRift3D.cs` | `Assets/Scripts/Gameplay/DimensionalRift3D.cs` | Portal circular 3D en la pared con anillos rotatorios y expansión al doble en fase Boss. |
| `CyberRoomDecorator3D.cs` | `Assets/Scripts/Core/CyberRoomDecorator3D.cs` | Decoración AR (circuitos en pared, pozo de datos, escudo de cobertura) y conmutador con tecla `V` / botón `Y` al modo VR Inmersivo Completo. |
| `NyxFloatingCompanion.cs` | `Assets/Scripts/Companion/NyxFloatingCompanion.cs` | Posicionamiento de Nyx en el flanco derecho a ras de suelo para no bloquear nunca la cámara. |
| `NyxBillboardAnimator.cs` | `Assets/Scripts/Companion/NyxBillboardAnimator.cs` | IA de apoyo de Nyx: modo remolona en enemigos básicos (no roba XP) y Pose de Batalla con fuego real contra el Boss. |
| `SynthSfxGenerator.cs` | `Assets/Scripts/AudioRadio/SynthSfxGenerator.cs` | Motor DSP multicapa de sonido sci-fi pesado (sub-grave + plasma FM + cerrojo mecánico). |
