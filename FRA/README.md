# 🚨 Sistema de Alertas Forenses CAI - Edición Cyberpunk 🚨

## 📋 Descripción
Sistema de alertas forenses con interfaz cyberpunk futurista para monitoreo de seguridad y captura de evidencias. **Ahora completamente en español** y con **activación automática al iniciar sesión**.

## 🎨 Características Cyberpunk
- **Interfaz futurista** con colores neón
- **Animaciones dinámicas** y efectos visuales
- **Línea de escaneo animada** estilo ciencia ficción
- **Texto animado** que aparece carácter por carácter
- **Estados dinámicos** en tiempo real
- **Diseño inspirado en Blade Runner** y Ghost in the Shell
- **Completamente en español** para mejor usabilidad

## 🚀 Activación Automática
- **Se ejecuta automáticamente** al iniciar sesión en Windows
- **Persistencia en registro** de Windows
- **Acceso directo en carpeta de inicio**
- **Modo minimizado** para funcionamiento en segundo plano

## 🛠️ Instalación

### Opción 1: Instalación Automática (Recomendada)
```bash
# Ejecutar como administrador
.\instalar_sistema.ps1
```

### Opción 2: Instalación Manual
1. **Navegar al directorio:**
```bash
cd "c:\Users\GMKtec\Documents\FRA"
```

2. **Instalar dependencias:**
```bash
pip install -r requirements.txt
```

3. **Ejecutar el programa:**
```bash
python forensic_alert.py
```

## 📦 Dependencias

### Principales:
- `opencv-python` - Captura de webcam
- `pyautogui` - Captura de pantalla
- `Pillow` - Procesamiento de imágenes
- `numpy` - Requerido por OpenCV

### Incluidas con Python:
- `tkinter` - Interfaz gráfica
- `smtplib` - Envío de correos
- `email` - Manejo de correos
- `os`, `sys`, `socket`, `getpass`, `datetime`, `platform`, `winreg`, `threading`, `time`

## 🎮 Uso del Sistema

### Funciones Principales:

1. **🚨 ENVIAR ALERTA** - Envía alerta forense con evidencias
2. **⚡ INSTALAR PERSISTENCIA** - Instala persistencia en registro
3. **❌ SALIR DEL SISTEMA** - Cierra el sistema

### Configuración de Evidencias:
- **CAPTURAR DATOS BIOMÉTRICOS** - Captura foto de webcam
- **CAPTURAR DATOS DE PANTALLA** - Captura pantalla completa

### Tipos de Eventos:
- ACCESO NO AUTORIZADO DETECTADO
- ACTIVIDAD SOSPECHOSA MONITOREADA
- INTENTO DE EXFILTRACIÓN DE DATOS
- ALERTA DE COMPROMISO DEL SISTEMA
- VIOLACIÓN CRÍTICA DE SEGURIDAD
- INTRUSIÓN DE RED DETECTADA

## 🔧 Configuración SMTP

Editar las credenciales en `forensic_alert.py`:

```python
SMTP_CONFIG = {
    "server": "smtp.gmail.com",
    "port": 587,
    "user": "tu_email@gmail.com",
    "password": "tu_password_de_aplicacion",
    "recipient": "destinatario@gmail.com"
}
```

## 🎯 Características Técnicas

### Interfaz Cyberpunk:
- **Colores neón**: Rosa (#ff00ff), Azul (#00ffff), Verde (#00ff00), Amarillo (#ffff00)
- **Fondo oscuro**: Negro (#0a0a0a) para contraste máximo
- **Fuente Consolas** para aspecto terminal
- **Animaciones constantes** y efectos visuales

### Funcionalidades de Seguridad:
- **Captura automática** de webcam y pantalla
- **Envío de correos** con evidencias adjuntas
- **Persistencia en registro** de Windows
- **Monitoreo en tiempo real** del sistema
- **Activación automática** al iniciar sesión

## 🚀 Ejecución

```bash
# Ejecutar normalmente
python forensic_alert.py

# Ejecutar en modo minimizado
python forensic_alert.py --minimized
```

## 📧 Formato de Alertas

Las alertas incluyen:
- Información del sistema (hostname, IP, usuario, OS)
- Timestamp del evento
- Evidencias adjuntas (fotos y screenshots)
- HTML formateado con estilo cyberpunk

## 🔒 Seguridad

- **Evidencias temporales**: Los archivos se eliminan después del envío
- **Persistencia automática**: Se ejecuta al iniciar sesión
- **Configuración SMTP**: Usar contraseñas de aplicación para Gmail
- **Modo minimizado**: Funciona en segundo plano

## 🎨 Personalización

El sistema permite personalizar:
- Colores neón en `CyberpunkStyle`
- Animaciones en `AnimatedLabel`
- Efectos visuales en `animate_scan_line()`
- Textos y etiquetas en la interfaz

## 📞 Soporte

Para problemas o mejoras:
- Verificar que todas las dependencias estén instaladas
- Comprobar configuración SMTP
- Revisar permisos de webcam y pantalla
- Ejecutar como administrador para persistencia

## 🎯 Scripts de Instalación

### `instalar_sistema.ps1`
Script PowerShell completo que:
- Verifica Python
- Instala dependencias
- Configura persistencia automática
- Crea acceso directo en inicio
- Ejecuta el sistema

### `instalar_persistencia.bat`
Script batch simple para:
- Instalar dependencias
- Configurar persistencia
- Ejecutar sistema

---

**⚡ Sistema de Seguridad Cyberpunk - Protegiendo el futuro digital ⚡** 