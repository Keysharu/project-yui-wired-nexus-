# 🌐 Project NYX: The Wired Nexus & Radio Chipset 24/7
> **Meta Global AI Developer Hackathon 2026**  
> **Author / Autora:** Raquel D. Torres (*El taller de la gatita tech*)  
> **Command Center / Centro de Mando:** [`00_CENTRO_DE_MANDO_Y_MAPA_CANONICO_GLOBAL.md`](00_CENTRO_DE_MANDO_Y_MAPA_CANONICO_GLOBAL.md)  
> **Submission Dossier (Spanish):** [`entrega_hackathon_meta/`](entrega_hackathon_meta/)  
> **Official Submission Dossier (English):** [`entrega_hackathon_meta/en/`](entrega_hackathon_meta/en/)  
> **Documentation / Documentación:** [`docs/`](docs/)  
> **Status:** 100% Bilingual Codebase | Zero Secrets Enforced | BMAX Sovereign Local AI Cluster | Quest 3 & 3S Optimized

---

## 🌐 Language Navigation / Navegación por Idioma
- 🇬🇧 **[English Public README](entrega_hackathon_meta/en/03_PUBLIC_GITHUB_README.md)** | **[Cerebral Valley Form Draft](entrega_hackathon_meta/en/05_CEREBRAL_VALLEY_SUBMISSION_FORM_DRAFT.md)** | **[Technical Architecture](entrega_hackathon_meta/en/06_YUI_ECOSYSTEM_TECHNICAL_ARCHITECTURE.md)**
- 🇪🇸 **[README Público en Español](entrega_hackathon_meta/03_README_GITHUB_PUBLICO.md)** | **[Borrador del Formulario en Español](entrega_hackathon_meta/05_BORRADOR_FORMULARIO_ENTREGA_CEREBRAL_VALLEY.md)** | **[Arquitectura Técnica](entrega_hackathon_meta/06_ARQUITECTURA_TECNICA_ECOSISTEMA_YUI.md)**

---

## 🗺️ Quick Ecosystem Index / Índice Rápido del Ecosistema

| Module / Módulo | Path / Ruta | Purpose / Propósito |
| :--- | :--- | :--- |
| **🏆 Official Submission (EN)** | [`entrega_hackathon_meta/en/`](entrega_hackathon_meta/en/) | Complete English submission package for Cerebral Valley and GitHub judges. |
| **🏆 Dossier de Entrega (ES)** | [`entrega_hackathon_meta/`](entrega_hackathon_meta/) | Checklist, Roadmap de 7 días, README público, guión del vídeo de 3 min y formulario. |
| **🥽 Mixed Reality (Quest 3 / 3S)** | [`quest_mr_nexus/`](quest_mr_nexus/) | Unity 6 LTS. Passthrough, MRUK occlusion, solid-light gauntlet, spiders, titan boss & VAD mic. |
| **📚 Documentation / Docs** | [`docs/`](docs/) | Clean categorized engineering documentation, architecture guides, and prompts. |
| **📻 Radio Chipset 24/7** | [`radio_chipset_broadcast/`](radio_chipset_broadcast/) | Autonomous DJ engine (*Kira & Mina*) with YouTube live broadcast engine and TTS audio ducking. |
| **🖥️ Sovereign Homelab** | [`192.168.0.29:11434`](http://192.168.0.29:11434) | BMAX B8A Pro Mini PC (LGM-OS 3.15.1, Debian 12, Ollama Llama 3.2 3B) at zero operating cost. |
| **🤖 Companion Assistant** | [`rebeca_xr_vr_asist/`](rebeca_xr_vr_asist/) | PySide6 2D Live companion with lip-sync, hybrid failover, and hardware telemetry. |
| **📡 Discord Intelligence** | [`buzon_discord/`](buzon_discord/) | Automated parser and de-noising filter for official hackathon Discord channels. |

---

## ⚡ Quick Start & Building / Compilación Rápida de la APK
1. **Requisitos:** Unity 6 LTS (6000.6.5f1) con módulos de Android Player, OpenXR y Meta XR Core SDK v69.
2. **Compilación de la APK:**
   - Abrir el proyecto en `quest_mr_nexus/`.
   - En el menú superior de la barra de herramientas de Unity, seleccionar:  
     `Wired Nexus MR > 3. Compilar APK para Meta Quest 3S`
   - El script ejecutará automáticamente la purga de procesos bloqueados de Gradle/Burst y compilará la APK en `Builds/`.
3. **Instalación en las gafas Meta Quest 3 / 3S:**
   ```bash
   adb install -r quest_mr_nexus/Builds/ProjectNyx_WiredNexus_v1.0.apk
   ```

---
*Created and developed by Raquel D. Torres (El taller de la gatita tech) - 2026*
