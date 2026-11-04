# Juego híbrido VR/AR en Quest 3/3S: guía técnica

Oct 22, 2026

## Resumen y arquitectura

Dos clientes comparten una sola cuenta: Quest en modo AR por la calle (farmeo, combate, misiones) y Quest en modo VR en casa (base, crafteo, mundos propios). El mismo backend valida la posición, guarda el inventario y reparte las apariciones de monstruos.

Las Quest 3 y 3S no llevan GPS ni módem de datos: solo acelerómetro, giroscopio y cámaras a color. Fuera de casa dependen por completo del móvil para ubicarse y para tener internet.

El diagrama siguiente resume cómo encaja cada pieza; la conectividad del móvil se detalla más abajo.

&#91;embedded content: arquitectura · app móvil, backend, Quest AR, Quest VR, gafas futuras\]

El móvil manda la posición al backend; el backend reparte spawns a las Quest en modo calle y progreso a las Quest en modo casa.

## La realidad del uso en la calle

El [centro de seguridad de Meta](https://www.meta.com/es/en/quest/safety-center/quest-3s/) indica que las Quest 3S no se recomiendan para uso en exteriores. Algunas guías impresas en EE. UU. llegan a decir que el uso al aire libre, incluso dentro de un vehículo, no está permitido.

Un proyecto real de AR caminando con Quest 3S, documentado en [este artículo técnico](https://zenn.dev/bison/articles/6f3d4a9ea7197c), confirma dos riesgos concretos: el sol directo puede dañar las cámaras exteriores, y el diseño no contempla el uso cerca de coches o peatones, por lo que recomienda ir siempre acompañado.

Para caminar libremente hay que desactivar el límite de seguridad ("boundary"). El permiso `BOUNDARYLESS_APP` del manifiesto de Android solo lo hace a medias: tras un rato aparece el aviso "te has movido demasiado lejos" y la app se detiene. Desactivarlo del todo exige activar el modo desarrollador y apagar el límite desde los ajustes del sistema, lo que además quita el passthrough en la pantalla de inicio.

Ese mismo proyecto registró un error conocido: tras caminar por terreno irregular, la interfaz del sistema puede aparecer a una altura incorrecta; reiniciar el dispositivo lo soluciona.

La batería ronda las 2-2,5 horas de uso, así que las sesiones en la calle serán cortas por diseño.

## Stack técnico recomendado

| Pieza | Herramienta | Para qué sirve |
| --- | --- | --- |
| Motor | Unity 6 LTS + OpenXR | Mismo proyecto para las Quest y, más adelante, otras gafas con soporte OpenXR |
| Passthrough y MR | Meta XR Core SDK (OVRManager, OVRPassthroughLayer) | Activar la cámara a color y mezclar el mundo real con el juego |
| Comprensión del espacio | [Mixed Reality Utility Kit (MRUK)](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-gs/) | Anclar monstruos y objetos al suelo y a las paredes detectadas |
| Ejemplo de referencia | [Passthrough sample de Meta](https://developers.meta.com/horizon/documentation/unity/unity-sample-starter-passthrough) | Punto de partida probado para activar y mezclar passthrough |
| Mapa base | Mapbox u OpenStreetMap | Mostrar el mapa del barrio en el modo calle |
| Celdas geográficas | H3 (Uber) o S2 (Google) | Repartir monstruos y recursos por zonas, calculado en el servidor |
| Backend | Colyseus, Nakama o un servidor propio | Cuentas, inventario, posición validada, estado compartido |
| App del móvil | Android nativo con FusedLocationProviderClient | Capturar el GPS y, si se decide, compartirlo en tiempo real |
| Escaneo de QR y objetos | Google ML Kit (Barcode Scanning) o Vuforia Object Recognition | Misiones de escaneo y eventos puntuales en plazas o centros comerciales |

## Cómo llevar el GPS del móvil a las gafas

Las Quest necesitan el punto de acceso Wi-Fi (hotspot) del móvil para tener internet en la calle; esto es obligatorio en cualquiera de las dos opciones siguientes.

| Opción | Cómo funciona | Ventaja | Inconveniente |
| --- | --- | --- | --- |
| Enlace directo móvil-gafas | La app del móvil expone un pequeño servidor local; las Quest, conectadas a su hotspot, lo consultan directamente | Latencia mínima | Hay que mantener un protocolo propio además del backend; si falla el hotspot, se pierde todo a la vez |
| Todo a través del backend (recomendado) | El móvil manda su posición al servidor cada 1-3 segundos; las Quest solo hablan con ese mismo servidor, que devuelve posición validada y mundo cercano | Una sola fuente de verdad; el mismo código sirve para anti-trampas y multijugador | Depende de tener cobertura de datos móviles, no solo Wi-Fi local |

En Android, capturar GPS en segundo plano exige un servicio en primer plano con permiso de ubicación desde Android 10; sin eso, el sistema corta la app al minuto.

El hotspot más el GPS activo gasta batería del móvil rápido: conviene prever una batería externa para sesiones largas en la calle.

## Backend y mundo compartido

El servidor nunca debe fiarse de la posición que manda el cliente sin comprobarla. Dos validaciones básicas: descartar saltos de posición que impliquen más velocidad de la que permite caminar, y detectar aplicaciones de ubicación falsa (mock location) en Android.

Cada celda H3 o S2 define qué puede aparecer cerca y con qué frecuencia; el cliente solo pide las celdas próximas a su posición, nunca el mapa completo. El mismo backend sirve cuentas, inventario y progreso a los dos modos de juego.

Las plazas, calles peatonales cortas o centros comerciales en interiores también sirven para eventos puntuales: misiones de escaneo de objetos o de códigos QR colocados en el mundo real, validadas por el backend igual que el resto de misiones.

## Modo VR en casa y cómo conecta con la calle

En casa, la base es donde se usa lo conseguido fuera: crafteo de armas con materiales farmeados, defensa de la base o jefes que solo aparecen en VR. El inventario es el mismo en los dos modos, así que progresar en uno avanza también el otro.

Nadie queda obligado a salir a la calle para avanzar, ni penalizado por no tener sitio para jugar en VR en casa: cada modo aporta algo distinto a la misma cuenta.

El modo VR en casa no se limita a la base: también incluye mundos propios, sin GPS, para explorar y luchar contra monstruos en niveles diseñados a mano.

Dos variantes: offline, una partida local contra enemigos controlados por IA sin depender de la red, y online, con salas en tiempo real donde varios jugadores luchan juntos contra los mismos monstruos.

El modo online pide un servidor de autoridad para la partida (posición, daño, loot), aparte de las validaciones de GPS del modo calle. Nakama y Colyseus traen multijugador en tiempo real de serie; si el backend es otro, Photon Fusion o Netcode for GameObjects cubren solo esa parte.

## Distribución: tienda oficial o fuera de ella

No hay una norma de contenido de la Horizon Store que prohíba expresamente el movimiento al aire libre; el aviso de "no recomendado en exteriores" pertenece a la guía de seguridad del dispositivo, no al reglamento de la tienda. El riesgo real está en la revisión discrecional de seguridad del jugador, no en una prohibición explícita.

Dos alternativas habituales en proyectos de MR experimental como el citado antes: App Lab, con una revisión más ligera e instalación por enlace directo, o el sideloading puro activando el modo desarrollador.

Conviene no anunciar el modo calle como "caminar por la ciudad con las gafas puestas" en el texto de la tienda, y mostrar los avisos de seguridad dentro de la propia app antes de cada sesión en exteriores.

Por tipo de espacio: en centros comerciales, al ser interiores, el aviso de exteriores no aplica directamente; el obstáculo real suele ser el propio centro, que normalmente exige permiso para grabar o llevar cámaras puestas en sus instalaciones.

En plazas y parques, el aviso de exteriores sigue aplicando, pero un evento fijo en una zona acotada es distinto de caminar varios kilómetros: se puede dejar activo el límite de seguridad ("boundary") normal, sin desactivarlo como hace falta en el modo calle con movimiento libre.

## Seguridad y UX del jugador en la calle

| Riesgo | Mitigación |
| --- | --- |
| Choque con coches, bordillos o gente | Calles peatonales cortas, plazas o centros comerciales en interiores, el mismo criterio que usan Pokémon GO y Monster Hunter Now; bloquear el combate si se detecta velocidad de vehículo |
| Daño a las cámaras por sol directo | Avisar antes de sesiones a mediodía en verano; evitar apuntar directamente al sol |
| Batería (2-2,5 h) y calor | Sesiones cortas con pausas, recordatorio en la app, batería externa opcional |
| Vergüenza o incomodidad social | Modo "solo en casa" claro en el menú para quien no quiera salir con las gafas puestas |
| El límite de seguridad corta el juego | Desactivarlo por el modo desarrollador, no solo con el permiso del manifiesto |
| Jugar sin compañía | Recomendar explícitamente ir acompañado, como hacen los proyectos de MR en exteriores ya documentados |

## Plan de prototipado por fases

Las fases 1 y 2 no dependen entre sí, y las fases 4 y 5 pueden ir en paralelo una vez lista la 3: se pueden repartir entre agentes distintos. El resto sigue el orden de dependencias de la tabla.

| Fase | Objetivo | Entregable | Depende de | Criterio de aceptación |
| --- | --- | --- | --- | --- |
| 1. Prototipo AR base | Anclar un monstruo al suelo con MRUK y poder golpearlo o dispararlo | Build de Quest con un monstruo interactivo, sin red ni GPS | Ninguna | El monstruo se ancla al suelo real y reacciona a un golpe o disparo |
| 2. App complementaria mínima | Capturar el GPS del móvil y enviarlo a un servidor de prueba | App Android que publica la posición cada 1-3 s | Ninguna | El servidor recibe coordenadas válidas cada pocos segundos |
| 3. Backend mínimo | Guardar la última posición de cada cuenta y devolver apariciones de prueba cercanas | Endpoint de posición y 2-3 spawns de ejemplo por celda | Fase 2 | Una llamada con unas coordenadas devuelve spawns coherentes con esa zona |
| 4. Unión AR + backend | Las Quest piden al servidor qué hay cerca y lo dibujan con passthrough | Build de Quest que muestra spawns reales del backend en el mundo | Fases 1 y 3 | Un spawn del backend aparece anclado en el lugar correcto al moverse |
| 5. Prototipo VR en casa | Escena VR que lee el inventario de la misma cuenta | Build de Quest en VR con base simple e inventario compartido | Fase 3 | Un objeto conseguido en modo calle aparece en el inventario del modo casa |
| 6. Prueba de campo controlada | Validar el conjunto en un espacio real, acompañado | Sesión grabada en una plaza o parque, con incidencias anotadas | Fase 4 | Sesión completa sin desconexiones ni errores de posición graves |
| 7. Misiones y pulido | Añadir misiones, progresión y contenido | Build jugable con al menos una misión completa de principio a fin | Fases 4, 5 y 6 | Un jugador nuevo completa una misión sin ayuda externa |

## Enlaces de documentación oficial y recursos citados

| Recurso | Qué cubre |
| --- | --- |
| [Centro de seguridad de Quest 3S](https://www.meta.com/es/en/quest/safety-center/quest-3s/) | Aviso oficial de Meta sobre el uso en exteriores |
| [Mixed Reality Utility Kit, primeros pasos](https://developers.meta.com/horizon/documentation/unity/unity-mr-utility-kit-gs/) | Instalación y configuración de MRUK en Unity |
| [Passthrough sample de Meta](https://developers.meta.com/horizon/documentation/unity/unity-sample-starter-passthrough) | Ejemplo oficial de Unity para activar passthrough |
| [AR caminando con Meta Quest: notas técnicas](https://zenn.dev/bison/articles/6f3d4a9ea7197c) | Experiencia real desactivando el boundary y moviéndose largas distancias |
| [Meta Ray-Ban Display](https://engadget.com/wearables/meta-ray-ban-display-glasses-offer-an-ar-display-for-799-002921925.html) | Contexto: las "gafas nuevas" de Meta son hoy un dispositivo de notificaciones con IA, no una plataforma de juego AR abierta |
| [Panorama de gafas AR de calle en 2026](https://the-gadgeteer.com/2026/05/04/6-smart-glasses-worth-watching-as-apple-joins-the-race/) | Meta no es la única: Snap (Specs), Google Android XR (con Samsung y Xreal) y Apple preparan sus propias gafas AR para la calle; ninguna es aún una plataforma de juego abierta |
