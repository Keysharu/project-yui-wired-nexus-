# 📝 SUBMISSION FORM DRAFT // CEREBRAL VALLEY
> **Official Form URL:** [https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit](https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit)  
> **Submission Deadline:** Wednesday, November 4 at 9:00 PM ET // Thursday, November 5 at 03:00 AM CET (Madrid time).  
> **Instructions:** Copy and paste these fields into the Cerebral Valley web form. You can save as draft at any time before the closing deadline.  
> **Author:** Raquel D. Torres (*El taller de la gatita tech*)  

---

### 1. Project Information

* **Project Name:**  
  `Project Yui: The Wired Nexus & Radio Chipset 107.7 FM`

* **Tagline (One-sentence Elevator Pitch):**  
  `A room-scale Mixed Reality FPS on Meta Quest 3S featuring 3D anime companion Nyx, upright wall-crawling mecha spiders, and the Quantum Centurion boss with physical occlusion, paired with live Radio Chipset 107.7 FM streaming and on-premise Llama 3.2 3B.`

* **Track Selection (Primary Category):**  
  `[X] Indie Developer Track ($150,000)`

* **Bonus Categories:**  
  `[X] Creative Superintelligence ($5,000)`  
  `[X] Open Weights Champion ($10,000)`

---

### 2. Links & Media

* **Public GitHub Repository URL:**  
  `https://github.com/[YOUR_GITHUB_USERNAME]/project-yui-wired-nexus`  
  *(Make sure the repository visibility is set to PUBLIC prior to submission).*

* **3-Minute Video Demo URL (YouTube / Vimeo / Loom):**  
  `https://youtu.be/[YOUR_VIDEO_ID]`  
  *(Ensure the video is set to Public or Unlisted so evaluators can view it without sign-in).*

---

### 3. Technical Description (For the Judges)

#### A. What does your project do?
> *Project Yui is an all-in-one spatial computing ecosystem designed natively for the Meta Quest 3S. It transforms the player's physical home into a cybernetic battlefield using color passthrough and the Meta Mixed Reality Utility Kit (MRUK).*
>
> *Players are accompanied by Nyx, a full 3D anime companion (0.55m tactical scale) hovering at their flank with procedural animations and tactical voice cues. Players equip a wrist-mounted Holographic Gauntlet projecting solid-light weaponry with haptic feedback and 6DoF hand tracking.*
>
> *Autonomous upright mecha spiders crawl along real-world scanned walls, and the armored Quantum Centurion Boss Mk-IX executes multi-phase combat. Real furniture physically occludes virtual enemies using custom depth-mask shaders at stable 90 FPS.*
>
> *Integrated into the space is a 16:9 holographic display streaming Radio Chipset 107.7 FM with live audio ducking, while an interactive LainOS cyberpunk pause menu allows instant muting of local ambient sound and seamless switching between physical room AR and the full VR Cyberspace Boulevard.*

#### B. How do you use Meta models and technologies?
> *Our project implements a resilient hybrid architecture combining Meta's edge hardware, open-weight models, and cloud superintelligence:*
> 1. **Meta Quest 3S & Meta XR Core SDK (v69+):** Drives room-scale color passthrough, spatial anchors (`OVRSpatialAnchor`), Touch Plus haptics, optical hand tracking (`XRHandAnchorFollower`), and dynamic passthrough color tinting.
> 2. **Meta Mixed Reality Utility Kit (MRUK):** Powers static room scanning, physical wall surface anchoring (`WALL_FACE` for crawling spiders), and invisible spatial depth occluders (`EffectMesh` + depth mask) so real furniture hides virtual enemies.
> 3. **Meta Llama 3.2 3B (On-Premise Sovereign Engine):** Runs locally on a bare-metal mini PC homelab server via Ollama over local Wi-Fi (`192.168.0.29:11434`), delivering instant, zero-cost conversational logic and event interpretation with zero cloud downtime.
> 4. **Meta Muse Spark 1.3 (`https://api.meta.ai/v1`):** Serves as the cloud superintelligence fallback for procedural world lore and tactical radio dialogue.

#### C. Prior Work & Hackathon Timeline Disclosure
> *In accordance with official hackathon rules, we explicitly disclose:*
> - **Pre-existing assets:** An initial desktop PyQt experimental companion (`cyber_companion`) and our base Debian Linux homelab mini PC setup.
> - **New contributions built 100% during the hackathon (Oct 26 – Nov 4):** The entire Unity 6 Quest 3S mixed reality game (48 C# scripts with 100% bilingual ES/EN comments, holographic gauntlet, MRUK occlusion shaders, 3D anime heroine Nyx companion, upright mecha spider drone AI, Quantum Centurion Boss 3D AI, live Radio Chipset 107.7 FM streaming display, interactive LainOS pause menu with audio mute toggle, clean AR/VR environment isolation, Touch Plus & Hand Tracking integration, and strict Zero Secrets architecture).

#### D. Challenges We Ran Into (Overcoming Meta Cloud Instability & Engineering Sovereign Fallbacks)
> *Building a room-scale Mixed Reality title on Unity 6 LTS for the Meta Quest 3S while integrating conversational LLMs and real-time audio presented several critical engineering challenges:*
>
> 1. **Meta Cloud LLM & Baseten Outages (The Discord Downtime Crisis):**
>    During peak hackathon development windows, hosted Meta Llama API endpoints and third-party hosting partners (such as Baseten) suffered recurring service disruptions, high-latency spikes (>8–12 seconds), and cascading `502 / 504 Gateway Timeouts`. This instability was widely discussed by developers across the official Cerebral Valley Discord channels (`#llama-api` and `#tech-support`).
>    * **Our Solution:** Rather than allowing cloud downtime to freeze gameplay, we engineered a **Zero-Cloud-Dependency Architecture**. We deployed open-weight **Meta Llama 3.2 3B** locally on a dedicated bare-metal Debian Linux mini PC (BMAX cluster at `192.168.0.29:11434` via Ollama). We implemented a three-tier hybrid failover: the game attempts cloud generation first, falls back immediately to the local LAN Llama cluster in sub-800ms, and drops to an in-engine deterministic fallback if network access drops entirely. This guaranteed that companion Nyx and the Radio Chipset dialogue remained 100% operational regardless of external cloud outages.
>
> 2. **Meta XR Core SDK v69 & Unity 6.6+ LTS Integration Hurdles:**
>    Targeting the brand-new Meta Quest 3S with Unity 6 LTS revealed multiple cutting-edge SDK discrepancies:
>    * *Hardware Gyroscope & Passthrough Horizon Conflicts:* Early simulation scripts written for desktop debugging attempted to level the camera horizon programmatically (`LevelCameraHorizon`). On physical Quest 3S hardware, this zeroed the real-world tracking origin upon booting into Color Passthrough, producing extreme horizon tilt and severe motion sickness. We solved this by strictly encapsulating all simulation clamps within `if (Application.isEditor)` and granting raw, unconstrained 6DoF hardware authority to OpenXR on the headset.
>    * *Touch Plus Controllers vs. Hand Tracking Desynchronization:* Hand tracking and physical Touch Plus controllers frequently conflicted during weapon alignment. We replaced brittle legacy bindings with a dedicated dual-input adapter (`XRHandAnchorFollower.cs`) that queries `UnityEngine.XR.InputDevices` directly with sub-millimeter precision before falling back to `OVRInput`.
>    * *Deprecated Directives in Meta Core Packages:* Unity 6 flagged deprecation warnings on Meta’s internal preprocessor flags (`DEVELOPMENT_BUILD` in `OVRSpatialAnchor.cs`). We sanitized our build pipeline to decouple custom spatial anchoring logic from deprecated SDK branches.
>
> 3. **Android Mixed Reality Audio Thread Contention & VAD:**
>    Enabling real-time voice input on Android for dynamic Voice Activity Detection (VAD) alongside spatialized 3D combat sound, multi-channel mecha audio, and Radio Chipset music triggered audio thread contention and microphone permission exceptions on Quest 3S.
>    * **Our Solution:** We engineered an asynchronous runtime permission handshake (`Permission.Microphone`), isolated the mic capture loop into a thread-safe VAD buffer with RMS energy thresholds, and decoupled audio callbacks entirely from scene spawner lifecycles.
>
> 4. **Windows Android Build Pipeline & Gradle Daemon File Locks:**
>    During continuous iterative APK builds, Unity’s Burst AOT compiler and the Android Gradle Daemon (`java.exe`) frequently held open file locks on merged native `.so` binaries in `mergeReleaseNativeLibs/out/lib/arm64-v8a`, throwing fatal `AccessDeniedException` errors.
>    * **Our Solution:** We modified `gradleTemplate.properties` to enforce `org.gradle.daemon=false` and `org.gradle.parallel=false`. Furthermore, we developed an automated pre-build tool (`QuestBuildPreparer.cs`) that systematically terminates lingering Java daemon processes and clears Burst/Gradle intermediate locks prior to building, reducing build times from 15 minutes to under 3 minutes.


---

### 4. Team Information

* **Team Name / Individual:** `El taller de la gatita tech`
* **Lead Developer:** `Raquel D. Torres`
* **Country:** `Spain`
* **Role:** `Solo Creator / XR & AI Engineer`
