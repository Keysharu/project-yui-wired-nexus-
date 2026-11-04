# 🛠️ STACK TÉCNICO, SDKs Y REPOSITORIOS OFICIALES
> **Proyecto:** Project Yui // The Wired Nexus  
> **Ecosistema:** El taller de la gatita tech  
> **Hardware:** Meta Quest 3S + Smartphone Android + Servidor Homelab BMAX B8A Pro (Ryzen 7 8745HS)  

Este documento reúne toda la documentación técnica oficial, los repositorios de código abierto indispensables y la guía de instalación de herramientas para construir el juego.

---

## 📚 1. Motores y SDKs Oficiales

### A. Entorno Meta Quest 3S (Realidad Mixta / Unity)
* **Meta XR All-in-One SDK (Oficial):**
  * **Documentación oficial:** [Meta Horizon OS Developers (Unity)](https://developer.oculus.com/documentation/unity/)
  * **Repositorio Package Manager:** `com.meta.xr.sdk.all`
  * **Asset Store:** [Meta XR All-in-One SDK en Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269654)
  * **Building Blocks para Realidad Mixta:**
    * *Passthrough:* Habilita la visión en color del mundo real.
    * *Scene Model & Mesh:* Detecta paredes, techo, suelo y muebles para colisiones físicas.
    * *Spatial Anchors:* Ancla hologramas de forma fija en coordenadas del mundo real.
    * *Hand Tracking / Synthetic Hands:* Interacción gestual con las manos o mandos Touch Plus.
* **OpenXR Plugin para Unity:**
  * **Documentación:** [Unity OpenXR Plugin Manual](https://docs.unity.cn/Packages/com.unity.xr.openxr@1.10/manual/index.html)

### B. Entorno AR Urbano (Smartphone / Geolocalización)
* **Unity AR Foundation:**
  * **Documentación:** [Unity AR Foundation Manual](https://docs.unity3d.com/Packages/com.unity.xr.arfoundation@5.1/manual/index.html)
  * Multiplataforma para Android (ARCore) e iOS (ARKit).
* **Google ARCore Geospatial API:**
  * **Documentación:** [ARCore Geospatial API for Unity](https://developers.google.com/ar/develop/unity-arf/geospatial/developer-guide)
  * Permite anclar contenido virtual a coordenadas GPS reales con precisión centimétrica usando el Street View de Google.
* **Mapas y Terreno Urbano Open-Source:**
  * **MapLibre GL / OpenStreetMap:** [MapLibre Native](https://github.com/maplibre/maplibre-native) o Mapbox Unity SDK para renderizar el callejero con estética ciberespacio neón.

### C. Cerebro de IA: Meta Llama Stack & Llama 3.2
* **Repositorio Oficial de Meta:** [github.com/meta-llama/llama-stack](https://github.com/meta-llama/llama-stack)
* **Documentación Llama Stack:** [llama-stack.readthedocs.io](https://llama-stack.readthedocs.io/)
* **Modelos Fundacionales:**
  * **Llama 3.2 11B Vision:** Modelo multimodal para análisis visual de imágenes y entorno.
  * **Llama 3.2 3B / 1B (Instruct):** Modelos ultra-rápidos y ligeros para los diálogos instantáneos entre Kira y Mina.
  * **Llama 3.1 8B (Instruct):** El modelo estándar para la orquestación general y el lore del juego.
* **Llama Stack Client para Python:**
  ```bash
  pip install llama-stack-client
  ```

---

## 💻 2. Programas y Herramientas a Descargar

| Programa | Función | Estado en tu PC | Enlace / Comando de Instalación |
| :--- | :--- | :--- | :--- |
| **Unity Hub** | Gestión de versiones del motor Unity | **Instalado** | `C:\Program Files\Unity Hub\Unity Hub.exe` |
| **Android Platform Tools (ADB)** | Depuración y carga de APKs en Quest 3S | **Instalado** | Integrado en el PATH del sistema |
| **VS Code** | Editor de código principal | **Instalado** | Extensiones recomendadas: C#, Unity, Python |
| **Git para Windows** | Control de versiones y clonación de repositorios | Pendiente de elevación UAC | [git-scm.com/download/win](https://git-scm.com/download/win) o `winget install Git.Git` (en terminal de Admin) |
| **Ollama** | Servidor local de modelos Llama para el BMAX / PC | Disponible en winget | `winget install Ollama.Ollama` o [ollama.com](https://ollama.com/) |
| **Meta Quest Developer Hub (MQDH)** | Panel de control oficial de Meta para Quest | Opcional / Recomendado | [developer.oculus.com/downloads/package/oculus-developer-hub-win/](https://developer.oculus.com/downloads/package/oculus-developer-hub-win/) |
| **Ventoy** | Creador de USB booteable para el servidor BMAX | Enlaces documentados | [github.com/ventoy/Ventoy/releases](https://github.com/ventoy/Ventoy/releases) |

---

## 🎙️ 3. Motores de Síntesis de Voz (TTS) y Audio

1. **Edge-TTS (Neural, Gratuito y sin GPU):**
   * Ya probado con éxito en `cyber_companion`.
   * Voces en español para el dúo de locutoras:
     * **Kira:** `es-ES-ElviraNeural` (con rate `+10%` y pitch `+5Hz` para darle energía).
     * **Mina:** `es-ES-AbrilNeural` (tono calmado, analítico y elegante).
     * **Yui:** `es-ES-TrianaNeural` o `es-MX-DaliaNeural` (tono dulce y servicial de hada).
2. **Audio Ducking:**
   * Script Python integrado (`audio_ducking.py`) en `cyber_companion` para bajar automáticamente el volumen de la música de fondo cuando Kira o Mina toman la palabra.
3. **Emisión de Audio 24/7:**
   * **Icecast2 / AzuraCast:** Para servir el stream continuo de radio a través de internet y consumirlo desde la app del juego mediante streaming HTTP.

---

## 📦 4. Módulos y Paquetes de Unity para el Proyecto

Cuando crees el proyecto en Unity Hub (plantilla **3D URP**), deberás añadir estos paquetes en el `manifest.json` o desde el **Package Manager**:

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.0.3",
    "com.unity.xr.openxr": "1.10.0",
    "com.unity.xr.arfoundation": "5.1.0",
    "com.unity.xr.core-utils": "2.3.0",
    "com.meta.xr.sdk.all": "69.0.0"
  }
}
```
