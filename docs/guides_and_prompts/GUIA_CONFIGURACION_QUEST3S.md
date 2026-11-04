# Configuración del Entorno de Realidad Mixta (MR) para Meta Quest 3S

Este documento describe los pasos completados y los siguientes pasos necesarios para desarrollar tu videojuego de Realidad Mixta en Unity para las **Meta Quest 3S**.

---

## 1. Estado de Herramientas Instaladas en el Sistema
- [x] **Unity Hub**: Instalado en el sistema (`C:\Program Files\Unity Hub\Unity Hub.exe`).
- [x] **Android SDK Platform-Tools (ADB)**: Instalado en el sistema para transferir y depurar en las gafas.
- [x] **Visual Studio Code**: Disponible como editor de código.

---

## 2. Instalación del Editor de Unity (Paso a realizar en Unity Hub)
Abre **Unity Hub** en tu ordenador:
1. Ve a la pestaña **Installs** (Instalaciones) -> Haz clic en **Install Editor**.
2. Selecciona la versión recomendada: **Unity 6 LTS** (6000.x) o **Unity 2022.3 LTS**.
3. En la ventana emergente de módulos (**Add modules**), es **imprescindible** marcar:
   - [x] **Android Build Support**
   - [x] **Android SDK & NDK Tools**
   - [x] **OpenJDK**
4. Haz clic en **Install** y espera a que finalice la descarga.

---

## 3. Habilitar el Modo Desarrollador en las Meta Quest 3S
Para poder instalar tus propios juegos y probarlos en tiempo real:
1. **Crear Organización de Desarrollador** (gratuito):
   - Ve a [developer.oculus.com](https://developer.oculus.com) e inicia sesión con la misma cuenta de Meta que usas en las gafas.
   - Crea un nombre de organización (cualquier nombre sirve).
2. **Activar en la App Móvil (Meta Horizon)**:
   - Abre la app Meta Horizon en tu smartphone vinculado a las Quest 3S.
   - Ve a **Menú** -> **Dispositivos** -> Selecciona tus **Quest 3S**.
   - Entra en **Ajustes de auriculares / Configuración** -> **Modo de desarrollador** y actívalo.
3. **Conectar las gafas al PC**:
   - Conecta las Meta Quest 3S con un cable USB-C a tu ordenador.
   - Ponte las gafas y verás un mensaje emergente: *"¿Permitir depuración USB?"*.
   - Marca la casilla **"Permitir siempre desde esta computadora"** y pulsa **Aceptar**.
4. **Verificación rápida**:
   - Abre una terminal y escribe:
     ```bash
     adb devices
     ```
   - Deberás ver el número de serie de tus Quest 3S con el estado `device`.

---

## 4. Crear el Proyecto en Unity para Realidad Mixta (Passthrough)
1. En Unity Hub, pulsa **New project**.
2. Selecciona la plantilla: **3D (URP)** (*Universal Render Pipeline* es la recomendada por Meta por rendimiento y efectos gráficos).
3. Asigna la ubicación en esta carpeta: `c:\Users\asakura\Documents\juego meta`.
4. Una vez abierto Unity:
   - Ve a **File** -> **Build Profiles** (o **Build Settings**).
   - Selecciona **Android** y pulsa el botón **Switch Platform**.
   - En **Texture Compression**, selecciona **ASTC**.

---

## 5. Instalar y Configurar el Meta XR SDK
1. Abre el **Package Manager** en Unity (**Window** -> **Package Manager**).
2. Pulsa en el icono `+` -> **Add package by name...**:
   - Añade: `com.meta.xr.sdk.all`
   - O bien instálalo desde la [Unity Asset Store: Meta XR All-in-One SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269654).
3. Ejecuta la herramienta de configuración automática:
   - En la barra superior: **Oculus** (o **Meta**) -> **Tools** -> **Project Setup Tool**.
   - Haz clic en **Fix All** para que Unity configure automáticamente la configuración de OpenXR, shaders y compatibilidad con Quest 3S.

---

## 6. Claves para la Realidad Mixta (Passthrough & Scene Understanding)
En el **Meta XR All-in-One SDK**, Meta incluye bloques prediseñados (*Building Blocks*):
- **Camera Rig con Passthrough**:
  - En la barra superior: **Meta** -> **Tools** -> **Building Blocks**.
  - Arrastra el bloque **Camera Rig** y el bloque **Passthrough**.
  - En el componente `OVRManager` del *Camera Rig*, asegúrate de que **Passthrough Support** esté configurado en `Supported` o `Required`.
- **Detección de entorno (Scene API / Room Mesh)**:
  - Añade el bloque **Spatial Anchors** (para fijar objetos virtuales en mesas o paredes del mundo real).
  - Añade el bloque **Scene Model** para recibir la malla y colisiones de las paredes y muebles de tu habitación detectados por el sensor de las Quest 3S.

---

## 7. Pruebas Rápidas (Meta Quest Link / Air Link)
Para probar tu juego sin tener que compilar el APK cada vez:
- Si tienes un cable Link de alta velocidad o Wi-Fi 6 (Air Link), puedes instalar la app de **Oculus para PC**.
- Al activar Quest Link en las gafas y pulsar **Play** en Unity, podrás probar tu juego directamente en tiempo real con el visor puesto.
