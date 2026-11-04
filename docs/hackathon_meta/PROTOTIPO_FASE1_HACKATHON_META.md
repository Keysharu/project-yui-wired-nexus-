# 🏆 GUÍA TÁCTICA: PROTOTIPO FASE 1 PARA EL META HACKATHON
> **Evento:** Meta Global AI Developer Hackathon (Cerebral Valley & Meta)  
> **Fechas:** 26 de Octubre – 04 de Noviembre de 2026  
> **Categoría:** Indie Developer Track  
> **Objetivo:** Construir un *Vertical Slice* funcional que integre a **Yui con visión** y a las **locutoras Kira & Mina (Llama Stack)** en tiempo real.  

---

## 🎯 1. El Concepto del Pitch para el Jurado de Meta

> *«Project Yui: The Wired Nexus es el primer ecosistema transmedia de Realidad Mixta impulsado por Meta Llama Stack, donde tu compañera holográfica personal y la emisora de radio del metaverso comparten el mismo cerebro cognitivo en tiempo real.»*

### ¿Por qué este proyecto destaca ante el jurado?
1. **Alineación 100% con las tecnologías de Meta:**
   * Utiliza **Meta Llama Stack** y **Llama 3.2 Vision**.
   * Diseñado para el hardware insignia de consumo de Meta: **Meta Quest 3S** (Realidad Mixta con Passthrough).
2. **Creatividad genuina:**
   * No es otro chatbot de texto aburrido en una ventana web.
   * Une videojuegos físicos (AR/MR), cultura maker (detección de electrónica) y radiodifusión autónoma viva inspirada en la cultura pop japonesa (*Splatoon*, *SAO*).
3. **Factible en 10 días:**
   * Nos apoyamos en la base que ya tienes construida en `cyber_companion` para el audio y las animaciones.
   * El mini PC BMAX B8A Pro (Ryzen 7 8745HS) procesará los modelos Llama en local sin costes de APIs ni latencias externas.

---

## 📅 2. Cronograma de Desarrollo de 10 Días (26 Oct – 04 Nov)

```
[ 24-25 OCT ] Puesta a punto del BMAX B8A Pro (LGM-OS + Ollama / Llama Stack + SSH)
      │
[ DÍA 1-2: 26-27 OCT ] NÚCLEO LLAMA STACK (El Dúo de Radio)
      │  ├─ Script multi-agente en Python: diálogo entre Kira y Mina
      │  └─ Inyección de eventos simulados (p. ej. "anomalía detectada")
      ▼
[ DÍA 3-4: 28-29 OCT ] MOTOR DE AUDIO & STREAMING
      │  ├─ Síntesis de voz con Edge-TTS para las dos personalidades
      │  └─ Ducking de música: baja la música, hablan Kira y Mina, sube la música
      ▼
[ DÍA 5-7: 30 OCT-01 NOV ] LA ESCENA DE REALIDAD MIXTA (Quest 3S)
      │  ├─ Escena Unity URP con Passthrough (se ve tu salón real)
      │  ├─ Modelo 3D de Yui flotando junto a la jugadora
      │  └─ Transmisión de audio de la radio dentro de las gafas
      ▼
[ DÍA 8-9: 02-03 NOV ] EL FACTOR VISIÓN (Llama 3.2 Vision)
      │  ├─ Yui analiza un objeto del mundo real captado por la cámara
      │  └─ Kira y Mina comentan en la radio el objeto analizado por Yui
      ▼
[ DÍA 10: 04 NOV ] GRABACIÓN DEL VÍDEO DE DEMO Y SUBMISSION
      ├─ Vídeo gameplay de 2 minutos grabado desde dentro de las Quest 3S
      └─ Repositorio de GitHub pulido con README y licencia MIT
```

---

## 💻 3. Código Base: Prototipo del Dúo de Radio en Python (`radio_splatoon_core.py`)

A continuación tienes el script orquestador preliminar que conectaremos al servidor de audio:

```python
"""
Radio Splatoon Core // Kira & Mina (Meta Llama Stack)
El taller de la gatita tech
"""
import os
import asyncio
import edge_tts

# Personalidades y perfiles vocales
VOICE_KIRA = "es-ES-ElviraNeural"  # Rápida, enérgica, netrunner
VOICE_MINA = "es-ES-AbrilNeural"   # Analítica, elegante, arcana

PROMPT_SISTEMA_RADIO = """
Sois Kira y Mina, las dos locutoras de Calamar-Net, la radio pirata del Wired.
- Kira es impulsiva, apasionada del synthwave, el combate y los glitches digitales.
- Mina es calmada, sarcástica, experta en arquitectura de datos y lore del mundo.
Vuestro trabajo es comentar con chispa y humor un evento reciente que ha ocurrido en el juego, alternando una frase cada una en un diálogo corto y muy dinámico (máximo 4 intervenciones en total).
"""

async def sintetizar_dialogo(locutor: str, texto: str, archivo_salida: str):
    voz = VOICE_KIRA if locutor.upper() == "KIRA" else VOICE_MINA
    comunicador = edge_tts.Communicate(texto, voz)
    await comunicador.save(archivo_salida)
    print(f"[{locutor}] Audio generado: {archivo_salida}")

# Ejemplo de estructura de prueba
if __name__ == "__main__":
    print("Iniciando motor de locución autónoma...")
```

---

## 🎬 4. Criterios de Evaluación del Jurado y Cómo Garantizar el Éxito

| Criterio del Jurado | Cómo lo cumplimos en el proyecto |
| :--- | :--- |
| **Uso de Llama / Llama Stack** | Llama 3.2 coordina tanto las conversaciones entre las locutoras como la visión contextual de Yui. |
| **Innovación y Originalidad** | Es una experiencia que trasciende la pantalla plana, llevando la IA al espacio físico del usuario en Realidad Mixta. |
| **Calidad de Ejecución** | El Lip-Sync y la cadencia de audio probada en `cyber_companion` aseguran que la presentación no parezca un prototipo tosco. |
| **Completitud y Documentación** | Repositorio documentado con arquitectura C4, diagramas de flujo y manual de instalación limpio. |
