# 📡 TELEMETRÍA Y NOVEDADES FILTRADAS DE DISCORD
> **Ecosistema:** El taller de la gatita tech // Meta Hackathon 2026
> **Fuente:** Buzón automático de exportaciones de Discord
> **Estado:** Barrido integral completado (26–27 de Octubre de 2026)

---

## 🧠 DOSSIER EJECUTIVO CONSOLIDADO (Inteligencia del Hackathon al 100%)

### 1. 📅 Calendario Oficial y Entrega Definitiva
- **Formulario Oficial de Entrega:** [https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit](https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit)
- **Fecha Límite Inflexible:** **Jueves, 5 de noviembre de 2026, a las 03:00 AM** (hora peninsular española) *(Miércoles 4 nov, 9:00 PM ET)*.
- **Regla de Entrega:** Se puede editar y refinar la entrega tantas veces como queramos hasta el último segundo; no hay penalización por subir un borrador temprano.
- **Próximo Taller en Vivo:** Miércoles 28 de octubre a las 17:00 CET (12:00 PM ET) -> *Agents 101 Pt. 2* con Suraj Subramanian.
- **Grabación del Kickoff:** [Vimeo Kickoff Recording](https://vimeo.com/1238909940/6d91bab1b2?share=copy&fl=sv&fe=ci)

---

### 2. 🗺️ Mapa Canónico de Modelos Meta (Familia Muse 2026)
- **`muse-spark-1.3` (API Cloud Meta):** Multimodal (texto, vídeo, imágenes y documentos en entrada, texto en salida). Es el modelo estándar subvencionado con tarifa económica. Endpoint: `https://api.meta.ai/v1`.
- **`muse-wonder-1.3` (API Cloud Meta):** Modelo de razonamiento frontera de 1M de contexto para agentes de larga duración y uso de ordenador. **PELIGRO:** Es extremadamente caro y los participantes están quemando sus créditos en 24h por usarlo para tareas sencillas.
- **`muse-spark-open` (Baseten):** Modelo gigante de pesos abiertos de 816B (42B activos). Requiere ≥700GB de VRAM, por lo que se corre en Baseten con créditos. Endpoint oficial: `https://inference.baseten.co/v1`.
- **`muse-glimmer` (Fireworks):** Modelo de 30B con pesos abiertos para visión y detección de objetos devolviendo coordenadas JSON nativas.
- **`Llama 3.2 1B / 3B` (Local Bare-Metal):** Los únicos modelos ligeros que corren a la perfección en hardware doméstico (¡nuestro servidor local BMAX Homelab con Ollama a coste 0€!).

---

### 3. 💳 Estado Real de Créditos y Proveedores
- **Meta Model API ($150):** Asignación por lotes en curso. La retención de la tarjeta es una pre-autorización temporal que se libera sola. Soporte directo en caso de fallo: [dev.meta.ai/support](https://dev.meta.ai/support).
- **Baseten ($100 + 2M TPM):** Remesa final de invitaciones enviada por email hoy a 2.000 seleccionados. La clave se envía en el cuerpo del correo (no regenerable).
- **Modal ($100 GPU serverless):** Aprobaciones manuales en cola; cuentas nuevas marcan 1$ temporalmente.
- **Fireworks ($25):** Asignación pequeña para experimentar con Meta Glimmer.
- **BMAX Homelab Local:** Inmunidad total; 0€ de coste, sin depender de esperas de moderadores.

---

### 4. 🛡️ Blindaje de Reglas y Cero Secretos
- **Código Previo (*Prior Work*):** Confirmado al 100% por organizadores. Se permite usar proyectos anteriores (`cyber_companion`) como base, siempre que el `README.md` público y el vídeo de 3 minutos especifiquen de forma cristalina las nuevas aportaciones creadas entre el 26 de octubre y el 4 de noviembre.
- **Proyectos Prohibidos / Descartes de Jueces:** Prohibición estricta de bots básicos ("wrappers"), resúmenes genéricos de PDFs (RAG simple), proyectos médicos, deportivos/apuestas, y política/guerra. *Project Yui* (MR Quest 3S + Radio Pirata + Homelab) está en la zona más segura y creativa del evento.
- **Bug de Red en Cloud:** Meta bloquea con `404 model_not_found` las llamadas a la API desde proveedores cloud como Render/AWS; las llamadas desde conexiones residenciales de fibra doméstica (como la de Raquel) funcionan con `200 OK`.

---

## 🗄️ HISTORIAL DE VOLCADOS REGISTRADOS

## 📥 Volcado Procesado: `aviso_taller_1.txt` (2026-10-27 21:04:42)

### 🐙 Repositorios Oficiales Detectados:
- [https://github.com/meta-models/legal-agent-workshop](https://github.com/meta-models/legal-agent-workshop)

### 🎥 Enlaces de Streams y Talleres:
- [https://cerebralvalley.ai/e/meta-ai-developer-hackathon/live](https://cerebralvalley.ai/e/meta-ai-developer-hackathon/live)

### 📢 Avisos y Requisitos del Hackathon:
- Expect the workshop to use around $7 in tokens
- 9 or newer for this workshop
- com/meta-models/legal-agent-workshop
- cd legal-agent-workshop/agent
- The repository for the workshop is here: https://github

---

## 📥 Volcado Procesado: `build_for_your_world_batch1.txt` (2026-10-27 21:40:06)

### 🧠 Modelos Clave Mencionados:
- `llama 3.2, muse spark, sam 3.1, muse glimmer, muse image, muse spark open, muse wonder`

### 💰 Novedades sobre Créditos, Tokens y Partners:
- im afraid my credits are going to over
- Para casi todos, "ejecutarlo" significa un proveedor alojado como Baseten

---

## 📥 Volcado Procesado: `announcements_batch1.txt` (2026-10-27 21:42:13)

### 🐙 Repositorios Oficiales Detectados:
- [https://github.com/meta-models/legal-agent-workshop](https://github.com/meta-models/legal-agent-workshop)

### 🎥 Enlaces de Streams y Talleres:
- [https://vimeo.com/1238909940/6d91bab1b2?share=copy&fl=sv&fe=ci](https://vimeo.com/1238909940/6d91bab1b2?share=copy&fl=sv&fe=ci)
- [https://cerebralvalley.ai/e/meta-ai-developer-hackathon/live](https://cerebralvalley.ai/e/meta-ai-developer-hackathon/live)

### 💰 Novedades sobre Créditos, Tokens y Partners:
- Official comms from meta on the credits
- Also moving forward ensure all credit related questions go into credit-requests  (read the pinned post there)
- For the partner's credits, you can fill out the forms in the participant resources
- On Credits: We will release information around Meta-provided credits around kickoff
- HERE IS THE FORM TO GET YOUR META CREDITS SET UP : https://docs

### 📢 Avisos y Requisitos del Hackathon:
- cd legal-agent-workshop/agent
- Expect the workshop to use around $7 in tokens
- com/meta-models/legal-agent-workshop
- @Builders  Meta Office Hours are 10:00 AM - 11:00 AM ET today in the office-hours channel
- 9 or newer for this workshop
- The repository for the workshop is here: https://github

---

## 📥 Volcado Procesado: `office_hours_batch1.txt` (2026-10-27 21:46:49)

### 🧠 Modelos Clave Mencionados:
- `muse spark open, sam 3.1, muse-voice, muse voice, muse spark, muse-spark, muse-spark-open`

### 💰 Novedades sobre Créditos, Tokens y Partners:
- Would a pre-launch founder in my situation qualify, or does the startup need to be formally established first? and also havent recived muse credits yet
- Muse Spark Open (Baseten): The Baseten docs use meta-models/muse-spark-open, but the guide says meta-models/muse-spark-open-1026

### 📢 Avisos y Requisitos del Hackathon:
- Are these fine to use, and is there anything specific we should disclose in the submission? Thanks! @Cerebral Valley (editado)

---

## 📥 Volcado Procesado: `credit_requests_batch1.txt` (2026-10-27 21:48:15)

### 💰 Novedades sobre Créditos, Tokens y Partners:
- @Meta Can I use other providers for meta models as I haven't received any update from either of the providers and I don't have a credit card to use meta model API too ?
- [CANAL: #credit-requests]
- hi, i've been set my payment method but i didn't recieve the credits, can anybody help please ?
- y'all genius ppl, who got the meta credits cuz i am still waiting shiiiiiiii
- @Meta @Cerebral Valley I also haven't received my credits after 24 hours
- @Cerebral Valley haven't received credits after 24 hours

---

## 📥 Volcado Procesado: `baseten_batch1.txt` (2026-10-27 21:49:58)

### 🧠 Modelos Clave Mencionados:
- `muse spark open`

### 💰 Novedades sobre Créditos, Tokens y Partners:
- Applied through the form and excited to start working with these credits if we get them
- Apply through the credits form: https://shorturl
- Any other Baseten models and products aren’t included
- If selected, you’ll get an email from Baseten within a few hours with your API key ( save it right away, as you can't regenerate it again) + a getting-started guide
- Looking forward to use credits to test out the platform and model capabilities
- thanks @Baseten for the credits
- Remember, the credits are given to a select 2,000 builders of the entire pool that applies, so not everyone will get the credits
- @everyone Answering the big question: how do Baseten credits work?   $100 in credits + 2M TPM on Muse Spark Open for 2,000 selected builders

---

## 📥 Volcado Procesado: `modal_batch1.txt` (2026-10-27 21:51:33)

### 💰 Novedades sobre Créditos, Tokens y Partners:
- Still waiting, I put the CC but still nothing, Hope they come in soon, we think we need to work 2 solid days once the credits drop in
- Hi Modal team, I've submitted the credits form for the Meta Global AI Developer Hackathon (GitHub: coder-RT, workspace: recall)
- @Modal I requested the Modal credit via form yesterday, haven't heard anything yet and also the account shows only $1 credit and if I add payment then $29 - is that it?
- Has anyone received Modal credits yet?
- @Modal guys till now I have also not received the credits
- [CANAL: #modal-credits / #partner-credits]
- Hi I have requested for Modal credits but haven't received them yet
- @Cerebral Valley How to get modal api

---

## 📥 Volcado Procesado: `fireworks_batch1.txt` (2026-10-27 21:52:45)

### 💰 Novedades sobre Créditos, Tokens y Partners:
- [CANAL: #fireworks-credits / #partner-credits]
- These credits would help me explore model capabilities for multi-agent planning, contract generation, and code generation as I develop my project
- Sorry silly question, how does one get the fireworks credits for hackathon?
- @Fireworks Could you please let us know when the free credits have run out?
- Hi everyone! I haven’t received the Fireworks or Modal credits yet
- Me too bro , till now I have also not received the fireworks and modal credit
- Hi Fireworks Team!
- Anyone got the fireworks credits? I applied

---

## 📥 Volcado Procesado: `credit_requests_batch2.txt` (2026-10-27 21:53:47)

### 💰 Novedades sobre Créditos, Tokens y Partners:
- But credit cards being required to participate? For fucking shame
- As a community leader during the collegiate hackathon boom, organizing my school’s trips to all the other major universities for their hackathons, I am APPALLED at a credit card being required to participate in this event
- I purchased a subscription, but I noticed that my credits are running out pretty quickly while developing
- A pre-authorization of about ₹1,198 went through on my card, but I still don’t see the $150 credit in Billing
- I received the $150 free credit for the hackathon, and I’d like to ask if I can use it toward Muse Code or my subscription
- When will they show up? I checked the team id and my email address i signed up with but still no credits…
- im waisting money on this and still dont have credits
- Second, I’m unable to create an API token

---

## 📥 Volcado Procesado: `showcase_batch1.txt` (2026-10-27 21:54:41)

### 🧠 Modelos Clave Mencionados:
- `muse spark`

---

## 📥 Volcado Procesado: `rules_prior_work_batch1.txt` (2026-10-27 21:55:27)

---

## 📥 Volcado Procesado: `submission_support_batch1.txt` (2026-10-27 21:56:06)

### 🎥 Enlaces de Streams y Talleres:
- [https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit](https://cerebralvalley.ai/e/meta-ai-developer-hackathon/hackathon/submit)
- [https://cerebralvalley.ai/e/meta-ai-developer-hackathon/details.](https://cerebralvalley.ai/e/meta-ai-developer-hackathon/details.)

### 💰 Novedades sobre Créditos, Tokens y Partners:
- For the card or credit problem itself, see the billing setup and payment-failure guidance (https://dev
- You just can't make Meta Model API calls until credits show on your Billing page (that needs a payment method on your account), so in the meantime you can build with Meta's open-weight models through the partner inference credits — Baseten, Modal and Fireworks forms are linked on https://cerebralvalley

### 📢 Avisos y Requisitos del Hackathon:
- ai/e/meta-ai-developer-hackathon/hackathon/submit by Wed Nov 4 at 9:00 PM ET, and you can edit the submission until the deadline, so submitting early has no advantage

---

## 📥 Volcado Procesado: `nuevo buzon discod.md` (2026-10-30 00:42:11)

### 🧠 Modelos Clave Mencionados:
- `muse wonder, muse glimmer, muse-wonder, sam-3.1, muse-spark, muse-voice, muse spark open, sam 3.1, muse voice, muse spark, muse-image, muse-spark-open, muse image`

### 💰 Novedades sobre Créditos, Tokens y Partners:
- Baseten and Modal credits request
- Where can I see how much of my \$100 credit I’ve used? My dashboard shows no keys and \$0 usage
- im waisting money on this and still dont have credits
- How Can I get modal \$100 free credit? where I can submit is application?
- Hello Modal team,
- The same cards that I use are accepted without any problem by Meta, Baseten, Fireworks and many other services
- ⁠modal  Hello Modal team,  I am a participant in the Meta Global AI Developer Hackathon (build window 26 October – 4 November 2026)
- Can you please fill out this form? We will try to figure out what is blocking your credits: https\://docs

### 📢 Avisos y Requisitos del Hackathon:
- What I bring: • Research background in LLM evaluation and multilingual AI — 14 publications, 128 citations, including AgentPersonaBench and IndicMMLU-Pro • The persona framework, data design, and evaluation side are covered — I’ll own those end to end • Full-time on this through the Nov 4 deadline Who I’m looking for: • A builder strong on front-end / demo polish who can make the chat \+ scorecard shine • Native speakers of any of the five languages who can sanity-check persona voice and data • Open to one more ML/backend hand if you’re fast with APIs If that sounds like you, reply here or DM me — happy to jump on a quick call today and lock scope
- 9 or newer for this workshop
- The deadline is very close, so a fast answer would help a lot
- Should the key be tied to my submission somehow (e
- The repository for the workshop is here: https\://github
- Has anyone faced this exact situation and managed to resolve it? Any help would be really appreciated, especially with the November 4 deadline approaching\!
- com/meta-models/legal-agent-workshop
- cd legal-agent-workshop/agent

---
