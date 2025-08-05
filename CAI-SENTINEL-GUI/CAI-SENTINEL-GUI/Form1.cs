using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;
using System.Configuration;

namespace CAI_SENTINEL_GUI
{
    // Clase para manejar configuración SMTP
    public class SmtpConfig
    {
        public string Server { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public bool UseSsl { get; set; } = true;
        public string Username { get; set; } = "neritpa@gmail.com";
        public string Password { get; set; } = "ymwgrjmbwigsrydm";
        public string FromEmail { get; set; } = "neritpa@gmail.com";
        public string ToEmail { get; set; } = "fxmspaha@gmail.com";
        public string CcEmail { get; set; } = "fxmspaha@gmail.com";

        public void SaveToFile(string filePath)
        {
            try
            {
                var lines = new[]
                {
                    $"Server={Server}",
                    $"Port={Port}",
                    $"UseSsl={UseSsl}",
                    $"Username={Username}",
                    $"Password={Password}",
                    $"FromEmail={FromEmail}",
                    $"ToEmail={ToEmail}",
                    $"CcEmail={CcEmail}"
                };
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error guardando configuración SMTP: {ex.Message}", ex);
            }
        }

        public static SmtpConfig LoadFromFile(string filePath)
        {
            var config = new SmtpConfig();
            
            if (!File.Exists(filePath))
                return config;

            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        switch (key)
                        {
                            case "Server":
                                config.Server = value;
                                break;
                            case "Port":
                                if (int.TryParse(value, out int port))
                                    config.Port = port;
                                break;
                            case "UseSsl":
                                if (bool.TryParse(value, out bool useSsl))
                                    config.UseSsl = useSsl;
                                break;
                            case "Username":
                                config.Username = value;
                                break;
                            case "Password":
                                config.Password = value;
                                break;
                            case "FromEmail":
                                config.FromEmail = value;
                                break;
                            case "ToEmail":
                                config.ToEmail = value;
                                break;
                            case "CcEmail":
                                config.CcEmail = value;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error cargando configuración SMTP: {ex.Message}", ex);
            }

            return config;
        }
    }

    // Clase para el formulario de configuración SMTP
    public class SmtpConfigForm : Form
    {
        private TextBox txtServer;
        private TextBox txtPort;
        private CheckBox chkUseSsl;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtFromEmail;
        private TextBox txtToEmail;
        private TextBox txtCcEmail;
        private Button btnSave;
        private Button btnCancel;
        private Button btnTest;

        public SmtpConfig CurrentConfig { get; private set; }

        public SmtpConfigForm(SmtpConfig currentConfig)
        {
            CurrentConfig = currentConfig ?? new SmtpConfig();
            InitializeComponents();
            LoadCurrentConfig();
        }

        private void InitializeComponents()
        {
            this.Text = "Configuración SMTP - CAI-Sentinel";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.ForeColor = Color.White;

            // Panel principal
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30, 20, 30, 20), // Más padding horizontal
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Título
            var titleLabel = new Label
            {
                Text = "🔧 Configuración SMTP",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.LightBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            // Crear controles
            int yPos = 60;
            int labelWidth = 150;
            int textBoxWidth = 350;
            int height = 25;

            // Servidor SMTP
            CreateLabelAndTextBox("Servidor SMTP:", "smtp.gmail.com", yPos, labelWidth, textBoxWidth, height, out var lblServer, out txtServer);
            yPos += 35;

            // Puerto
            CreateLabelAndTextBox("Puerto:", "587", yPos, labelWidth, textBoxWidth, height, out var lblPort, out txtPort);
            yPos += 35;

            // Usar SSL
            chkUseSsl = new CheckBox
            {
                Text = "Usar SSL/TLS",
                Location = new Point(labelWidth + 10, yPos),
                Size = new Size(200, height),
                Checked = true,
                ForeColor = Color.White
            };
            yPos += 35;

            // Usuario
            CreateLabelAndTextBox("Usuario:", "tu_email@gmail.com", yPos, labelWidth, textBoxWidth, height, out var lblUsername, out txtUsername);
            yPos += 35;

            // Contraseña
            CreateLabelAndTextBox("Contraseña:", "", yPos, labelWidth, textBoxWidth, height, out var lblPassword, out txtPassword);
            txtPassword.UseSystemPasswordChar = true;
            yPos += 35;

            // Email Remitente
            CreateLabelAndTextBox("Email Remitente:", "tu_email@gmail.com", yPos, labelWidth, textBoxWidth, height, out var lblFrom, out txtFromEmail);
            yPos += 35;

            // Email Destinatario
            CreateLabelAndTextBox("Email Destinatario:", "destinatario@email.com", yPos, labelWidth, textBoxWidth, height, out var lblTo, out txtToEmail);
            yPos += 35;

            // Email CC
            CreateLabelAndTextBox("Email CC:", "cc@email.com", yPos, labelWidth, textBoxWidth, height, out var lblCc, out txtCcEmail);
            yPos += 50;

            // Botones
            btnTest = new Button
            {
                Text = "🧪 Probar Conexión",
                Location = new Point(30, yPos),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.Click += BtnTest_Click;

            btnSave = new Button
            {
                Text = "💾 Guardar",
                Location = new Point(200, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "❌ Cancelar",
                Location = new Point(320, yPos),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            // Agregar controles al panel
            mainPanel.Controls.AddRange(new Control[]
            {
                titleLabel,
                lblServer, txtServer,
                lblPort, txtPort,
                chkUseSsl,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblFrom, txtFromEmail,
                lblTo, txtToEmail,
                lblCc, txtCcEmail,
                btnTest, btnSave, btnCancel
            });

            this.Controls.Add(mainPanel);
        }

        private void CreateLabelAndTextBox(string labelText, string placeholder, int yPos, int labelWidth, int textBoxWidth, int height, out Label label, out TextBox textBox)
        {
            label = new Label
            {
                Text = labelText,
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, height),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };

            textBox = new TextBox
            {
                Location = new Point(labelWidth + 30, yPos), // Más espacio entre label y textbox
                Size = new Size(textBoxWidth - 50, height), // Reducir ancho para dar margen derecho
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9)
            };
        }

        private void LoadCurrentConfig()
        {
            txtServer.Text = CurrentConfig.Server;
            txtPort.Text = CurrentConfig.Port.ToString();
            chkUseSsl.Checked = CurrentConfig.UseSsl;
            txtUsername.Text = CurrentConfig.Username;
            txtPassword.Text = CurrentConfig.Password;
            txtFromEmail.Text = CurrentConfig.FromEmail;
            txtToEmail.Text = CurrentConfig.ToEmail;
            txtCcEmail.Text = CurrentConfig.CcEmail;
        }

        private void BtnTest_Click(object sender, EventArgs e)
        {
            try
            {
                btnTest.Enabled = false;
                btnTest.Text = "🔄 Probando...";

                var testConfig = GetConfigFromForm();
                
                using (var client = new SmtpClient(testConfig.Server, testConfig.Port))
                {
                    client.EnableSsl = testConfig.UseSsl;
                    client.Credentials = new System.Net.NetworkCredential(testConfig.Username, testConfig.Password);

                    using (var message = new MailMessage(testConfig.FromEmail, testConfig.ToEmail, "Prueba CAI-Sentinel", "Esta es una prueba de configuración SMTP desde CAI-Sentinel."))
                    {
                        if (!string.IsNullOrEmpty(testConfig.CcEmail))
                            message.CC.Add(testConfig.CcEmail);

                        client.Send(message);
                    }
                }

                MessageBox.Show("✅ Conexión SMTP exitosa!\n\nLa configuración es correcta y se puede enviar emails.", 
                    "Prueba Exitosa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error en la prueba de conexión:\n\n{ex.Message}\n\nVerifique la configuración e intente nuevamente.", 
                    "Error de Conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTest.Enabled = true;
                btnTest.Text = "🧪 Probar Conexión";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentConfig = GetConfigFromForm();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar configuración:\n{ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private SmtpConfig GetConfigFromForm()
        {
            if (string.IsNullOrWhiteSpace(txtServer.Text))
                throw new Exception("El servidor SMTP es requerido.");

            if (!int.TryParse(txtPort.Text, out int port) || port <= 0)
                throw new Exception("El puerto debe ser un número válido mayor a 0.");

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
                throw new Exception("El usuario es requerido.");

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
                throw new Exception("La contraseña es requerida.");

            if (string.IsNullOrWhiteSpace(txtFromEmail.Text))
                throw new Exception("El email remitente es requerido.");

            if (string.IsNullOrWhiteSpace(txtToEmail.Text))
                throw new Exception("El email destinatario es requerido.");

            return new SmtpConfig
            {
                Server = txtServer.Text.Trim(),
                Port = port,
                UseSsl = chkUseSsl.Checked,
                Username = txtUsername.Text.Trim(),
                Password = txtPassword.Text,
                FromEmail = txtFromEmail.Text.Trim(),
                ToEmail = txtToEmail.Text.Trim(),
                CcEmail = txtCcEmail.Text.Trim()
            };
        }
    }

    public partial class Form1 : Form
    {
        private AdvancedCanarySystem _canarySystem;
        private bool _isSystemRunning = false;
        private System.Windows.Forms.Timer _statusTimer;
        private System.Windows.Forms.Timer _logUpdateTimer;
        private RichTextBox _logDisplay;
        private Button _startButton;
        private Button _stopButton;
        private Label _statusLabel;
        private Label _lastEventLabel;
        private Panel _controlPanel;
        private Panel _logPanel;
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private Label _titleLabel;
        private Panel _statusPanel;
        private Label _uptimeLabel;
        private Label _eventsLabel;
        private ProgressBar _healthBar;
        private Button _minimizeButton;
        private Button _settingsButton;
        private Button _smtpConfigButton;
        private Panel _statsPanel;
        private SmtpConfig _smtpConfig;

        public Form1()
        {
            InitializeComponent();
            LoadSmtpConfig();
            InitializeCustomComponents();
            SetupTimers();
        }

        private void LoadSmtpConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, "smtp.conf");
                _smtpConfig = SmtpConfig.LoadFromFile(configPath);
            }
            catch (Exception ex)
            {
                // Si hay error, usar configuración por defecto
                _smtpConfig = new SmtpConfig();
                LogToDisplay($"Advertencia: No se pudo cargar configuración SMTP: {ex.Message}");
            }
        }

        private void SaveSmtpConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, "smtp.conf");
                _smtpConfig.SaveToFile(configPath);
                LogToDisplay("Configuración SMTP guardada exitosamente.");
            }
            catch (Exception ex)
            {
                LogToDisplay($"Error guardando configuración SMTP: {ex.Message}");
            }
        }

        private void InitializeCustomComponents()
        {
            this.Text = "CAI-Sentinel - Sistema de Detección de Intrusos";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Configurar NotifyIcon
            SetupNotifyIcon();

            // Panel de título
            _titleLabel = new Label
            {
                Text = "🛡️ CAI-SENTINEL",
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Panel de control principal
            _controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.FromArgb(45, 45, 48)
            };

            // Botones de control con diseño mejorado
            _startButton = CreateStyledButton("▶ Iniciar Sistema", Color.FromArgb(40, 167, 69), new Point(20, 20));
            _startButton.Click += StartButton_Click;

            _stopButton = CreateStyledButton("⏹ Detener Sistema", Color.FromArgb(220, 53, 69), new Point(160, 20));
            _stopButton.Click += StopButton_Click;
            _stopButton.Enabled = false;

            _minimizeButton = CreateStyledButton("🗕 Minimizar", Color.FromArgb(108, 117, 125), new Point(300, 20));
            _minimizeButton.Click += (s, e) => this.WindowState = FormWindowState.Minimized;

            _settingsButton = CreateStyledButton("⚙ Configuración", Color.FromArgb(255, 193, 7), new Point(440, 20));
            _settingsButton.Click += SettingsButton_Click;

            _smtpConfigButton = CreateStyledButton("📧 SMTP", Color.FromArgb(138, 43, 226), new Point(580, 20));
            _smtpConfigButton.Click += SmtpConfigButton_Click;

            // Panel de estadísticas
            _statsPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(52, 58, 64)
            };

            _uptimeLabel = new Label
            {
                Text = "⏱️ Tiempo activo: 00:00:00",
                Location = new Point(20, 15),
                Size = new Size(200, 20),
                ForeColor = Color.LightBlue,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _eventsLabel = new Label
            {
                Text = "📊 Eventos detectados: 0",
                Location = new Point(240, 15),
                Size = new Size(200, 20),
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            _healthBar = new ProgressBar
            {
                Location = new Point(20, 45),
                Size = new Size(400, 20),
                Style = ProgressBarStyle.Continuous,
                Value = 100
            };

            // Panel de estado
            _statusPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(40, 40, 40)
            };

            _statusLabel = new Label
            {
                Text = "🔴 Estado: Inactivo",
                Location = new Point(20, 15),
                Size = new Size(300, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            _lastEventLabel = new Label
            {
                Text = "📝 Último evento: Ninguno",
                Location = new Point(350, 15),
                Size = new Size(600, 20),
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9)
            };

            // Panel de logs mejorado
            _logPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25)
            };

            _logDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(10)
            };

            // Agregar controles
            _controlPanel.Controls.AddRange(new Control[] { _startButton, _stopButton, _minimizeButton, _settingsButton, _smtpConfigButton });
            _statsPanel.Controls.AddRange(new Control[] { _uptimeLabel, _eventsLabel, _healthBar });
            _statusPanel.Controls.AddRange(new Control[] { _statusLabel, _lastEventLabel });
            _logPanel.Controls.Add(_logDisplay);

            this.Controls.Add(_logPanel);
            this.Controls.Add(_statusPanel);
            this.Controls.Add(_statsPanel);
            this.Controls.Add(_controlPanel);
            this.Controls.Add(_titleLabel);

            // Configurar evento de minimización
            this.Resize += Form1_Resize;
        }

        private void SmtpConfigButton_Click(object sender, EventArgs e)
        {
            try
            {
                var configForm = new SmtpConfigForm(_smtpConfig);
                if (configForm.ShowDialog() == DialogResult.OK)
                {
                    _smtpConfig = configForm.CurrentConfig;
                    SaveSmtpConfig();
                    LogToDisplay("Configuración SMTP actualizada correctamente.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en configuración SMTP: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogToDisplay($"Error en configuración SMTP: {ex.Message}");
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                _notifyIcon.ShowBalloonTip(2000, "CAI-Sentinel", "La aplicación se ha minimizado a la bandeja del sistema", ToolTipIcon.Info);
            }
        }

        private void SetupTimers()
        {
            _statusTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000 // 1 segundo
            };
            _statusTimer.Tick += StatusTimer_Tick;

            _logUpdateTimer = new System.Windows.Forms.Timer
            {
                Interval = 5000 // 5 segundos
            };
            _logUpdateTimer.Tick += LogUpdateTimer_Tick;
        }

        private void SetupNotifyIcon()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Mostrar ventana", null, (s, e) => ShowMainWindow());
            _contextMenu.Items.Add("Iniciar sistema", null, (s, e) => StartButton_Click(s, e));
            _contextMenu.Items.Add("Detener sistema", null, (s, e) => StopButton_Click(s, e));
            _contextMenu.Items.Add("-");
            _contextMenu.Items.Add("Salir", null, (s, e) => Application.Exit());

            _notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Shield,
                Text = "CAI-Sentinel - Sistema de Detección de Intrusos",
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        private Button CreateStyledButton(string text, Color backColor, Point location)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(120, 35),
                Location = location,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(
                Math.Min(255, backColor.R + 30),
                Math.Min(255, backColor.G + 30),
                Math.Min(255, backColor.B + 30)
            );

            return button;
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Configuración del sistema CAI-Sentinel\n\n" +
                          "• Monitoreo de archivos señuelo activo\n" +
                          "• Detección de intrusiones en tiempo real\n" +
                          "• Alertas automáticas por email\n" +
                          "• Acciones de seguridad automáticas\n\n" +
                          "Para más opciones, contacte al administrador del sistema.",
                          "Configuración", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            try
            {
                _canarySystem = new AdvancedCanarySystem(_smtpConfig);
                _canarySystem.InitializeSystem();
                _isSystemRunning = true;

                _startButton.Enabled = false;
                _stopButton.Enabled = true;
                _statusLabel.Text = "🟢 Estado: Activo";
                _statusLabel.ForeColor = Color.LightGreen;
                _healthBar.Value = 100;
                _healthBar.ForeColor = Color.LightGreen;

                _statusTimer.Start();
                _logUpdateTimer.Start();

                // Actualizar NotifyIcon
                _notifyIcon.Icon = SystemIcons.Application;
                _notifyIcon.Text = "CAI-Sentinel - ACTIVO";

                LogToDisplay("=== SISTEMA CAI-SENTINEL INICIADO ===");
                LogToDisplay($"Hora de inicio: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                LogToDisplay("Monitoreo de archivos señuelo activo...");

                // Mostrar notificación del sistema
                _notifyIcon.ShowBalloonTip(3000, "CAI-Sentinel", "Sistema de detección iniciado correctamente", ToolTipIcon.Info);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar el sistema: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogToDisplay($"ERROR: {ex.Message}");
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            try
            {
                _isSystemRunning = false;
                _canarySystem = null;

                _startButton.Enabled = true;
                _stopButton.Enabled = false;
                _statusLabel.Text = "🔴 Estado: Inactivo";
                _statusLabel.ForeColor = Color.White;
                _healthBar.Value = 0;
                _healthBar.ForeColor = Color.Red;

                _statusTimer.Stop();
                _logUpdateTimer.Stop();

                // Actualizar NotifyIcon
                _notifyIcon.Icon = SystemIcons.Shield;
                _notifyIcon.Text = "CAI-Sentinel - INACTIVO";

                LogToDisplay("=== SISTEMA CAI-SENTINEL DETENIDO ===");
                LogToDisplay($"Hora de detención: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // Mostrar notificación del sistema
                _notifyIcon.ShowBalloonTip(3000, "CAI-Sentinel", "Sistema de detección detenido", ToolTipIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al detener el sistema: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DateTime _startTime = DateTime.MinValue;
        private int _eventCount = 0;

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (_isSystemRunning)
            {
                if (_startTime == DateTime.MinValue)
                {
                    _startTime = DateTime.Now;
                }

                var uptime = DateTime.Now - _startTime;
                _uptimeLabel.Text = $"⏱️ Tiempo activo: {uptime:hh\\:mm\\:ss}";
                _eventsLabel.Text = $"📊 Eventos detectados: {_eventCount}";
                _statusLabel.Text = $"🟢 Estado: Activo - {DateTime.Now:HH:mm:ss}";
            }
        }

        private void LogUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_isSystemRunning)
            {
                UpdateLogDisplay();
            }
        }

        private void LogToDisplay(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(LogToDisplay), message);
                return;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            _logDisplay.AppendText($"[{timestamp}] {message}\n");
            _logDisplay.ScrollToCaret();

            // Incrementar contador de eventos si es un evento de seguridad
            if (message.Contains("EVENTO") || message.Contains("ALERTA") || message.Contains("DETECTADO"))
            {
                _eventCount++;
            }

            // Actualizar último evento
            _lastEventLabel.Text = $"📝 Último evento: {timestamp} - {message.Substring(0, Math.Min(50, message.Length))}...";
        }

        private void UpdateLogDisplay()
        {
            try
            {
                if (_canarySystem != null)
                {
                    // Aquí podrías actualizar información específica del sistema
                    _lastEventLabel.Text = $"Último evento: {DateTime.Now:HH:mm:ss}";
                }
            }
            catch (Exception ex)
            {
                LogToDisplay($"Error actualizando display: {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_isSystemRunning)
            {
                var result = MessageBox.Show("El sistema está activo. ¿Desea realmente cerrar la aplicación?", 
                    "Confirmar cierre", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            // Guardar configuración SMTP antes de cerrar
            SaveSmtpConfig();

            _statusTimer?.Stop();
            _logUpdateTimer?.Stop();
            
            // Limpiar NotifyIcon
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            
            base.OnFormClosing(e);
        }
    }

    public class AdvancedCanarySystem
    {
        // Configuración principal - Ahora dinámica según permisos
        private string BasePath;
        private string CanaryFolder;
        private string CanaryFileName = "Claves.CAI.txt";
        private string FullCanaryPath;
        private string LogPath;
        private string IntegrityLog;
        private bool isRunningAsAdmin;

        // Configuración SMTP - Ahora dinámica
        private SmtpConfig _smtpConfig;

        // Estado del sistema
        private string _currentFileHash;
        private readonly FileSystemWatcher _watcher;
        private bool _isInitialized = false;

        public AdvancedCanarySystem(SmtpConfig smtpConfig)
        {
            try
            {
                _smtpConfig = smtpConfig ?? new SmtpConfig();
                
                // Determinar permisos y configurar rutas
                InitializePaths();

                // 1. Crear estructura de directorios primero
                CreateSecureFolderStructure();

                // 2. Configurar FileSystemWatcher (sin activar todavía)
                _watcher = new FileSystemWatcher
                {
                    Path = CanaryFolder,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName |
                                  NotifyFilters.DirectoryName | NotifyFilters.Security,
                    Filter = CanaryFileName,
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = false // Activaremos después de la inicialización
                };

                // 3. Configurar manejadores de eventos
                _watcher.Changed += OnFileEvent;
                _watcher.Deleted += OnFileEvent;
                _watcher.Renamed += OnFileEvent;
                _watcher.Created += OnFileEvent;
                _watcher.Error += OnWatcherError;
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error en constructor: {ex}");
                throw;
            }
        }

        private void InitializePaths()
        {
            // Verificar si se ejecuta como administrador
            isRunningAsAdmin = IsRunningAsAdministrator();

            if (isRunningAsAdmin)
            {
                BasePath = @"C:\CAI";
            }
            else
            {
                BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CAI");
            }

            CanaryFolder = Path.Combine(BasePath, "Claves de acceso");
            FullCanaryPath = Path.Combine(CanaryFolder, CanaryFileName);
            LogPath = Path.Combine(BasePath, "Security", "SentinelLog.txt");
            IntegrityLog = Path.Combine(BasePath, "Security", "IntegrityAudit.log");
        }

        private bool IsRunningAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public void InitializeSystem()
        {
            try
            {
                // 1. Crear archivo señuelo inicial
                CreateAdvancedCanaryFile();

                // 2. Calcular hash inicial
                _currentFileHash = ComputeFileHash(FullCanaryPath);
                LogInitialFingerprint();

                // 3. Iniciar servicios de apoyo
                StartIntegrityMonitoring();
                StartLogCleanupService();
                StartHealthCheckService();
                StartOpenCloseMonitoring();
                StartAuditEventMonitoring();

                // 4. Activar monitoreo principal
                _watcher.EnableRaisingEvents = true;
                _isInitialized = true;

                LogSystemStatus("Sistema CAI-Sentinel inicializado correctamente");
                SendAlert("CAI-Sentinel Iniciado",
                         $"El sistema de detección de intrusos se ha iniciado correctamente\n" +
                         $"Equipo: {Environment.MachineName}\n" +
                         $"Hora: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                         $"Modo: {(isRunningAsAdmin ? "Administrador" : "Usuario")}");
            }
            catch (Exception ex)
            {
                LogCriticalError($"Fallo en inicialización: {ex}");
                throw;
            }
        }

        private void CreateSecureFolderStructure()
        {
            try
            {
                // Crear directorio base si no existe
                if (!Directory.Exists(BasePath))
                {
                    Directory.CreateDirectory(BasePath);
                }

                // Crear directorio de seguridad si no existe
                string securityPath = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(securityPath))
                {
                    Directory.CreateDirectory(securityPath);
                }

                // Crear directorio de claves con permisos restringidos
                if (!Directory.Exists(CanaryFolder))
                {
                    Directory.CreateDirectory(CanaryFolder);

                    // Solo intentar configurar ACL si es administrador
                    if (isRunningAsAdmin)
                    {
                        try
                        {
                            ConfigureDirectorySecurity();
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Log warning pero continuar
                        }
                    }
                }

                // Crear estructura adicional de carpetas CAI
                CreateCAIFolderStructure();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creando estructura de directorios: {ex.Message}", ex);
            }
        }

        private void CreateCAIFolderStructure()
        {
            // Definir carpetas y usuarios
            var folders = new[]
            {
                ("Administración", "CAI_Adm", "Modify", "Coordinadores del CAI"),
                (@"Proyectos\Públicos", "CAI_Pub", "ReadAndExecute", "Acceso público"),
                (@"Proyectos\Privados", "CAI_Priv", "Modify", "Proyectos internos"),
                ("Base de Datos", "CAI_DB", "Modify", "Responsables técnicos"),
                ("Respaldos", "CAI_Backup", "Modify", "Personal autorizado"),
                (@"Documentación Técnica\Manuales", "CAI_Docs", "ReadAndExecute", "Documentación técnica"),
                (@"Documentación Técnica\Arquitecturas", "CAI_Docs", "ReadAndExecute", "Documentación técnica")
            };

            foreach (var folder in folders)
            {
                try
                {
                    string fullPath = Path.Combine(BasePath, folder.Item1);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);

                        // Configurar permisos si es administrador
                        if (isRunningAsAdmin)
                        {
                            try
                            {
                                ConfigureFolderPermissions(fullPath, folder.Item2, folder.Item3);
                            }
                            catch (Exception ex)
                            {
                                // Log warning pero continuar
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log error pero continuar con otras carpetas
                }
            }
        }

        private void ConfigureFolderPermissions(string folderPath, string userName, string permission)
        {
            try
            {
                var sec = Directory.GetAccessControl(folderPath);

                // Otorgar permisos al usuario especificado
                FileSystemRights rights = FileSystemRights.ReadAndExecute;
                switch (permission.ToLower())
                {
                    case "modify":
                        rights = FileSystemRights.Modify | FileSystemRights.ReadAndExecute | FileSystemRights.Write;
                        break;
                    case "readandexecute":
                        rights = FileSystemRights.ReadAndExecute;
                        break;
                    case "fullcontrol":
                        rights = FileSystemRights.FullControl;
                        break;
                }

                // Intentar agregar permisos para el usuario (si existe)
                try
                {
                    sec.AddAccessRule(new FileSystemAccessRule(
                        userName,
                        rights,
                        InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                        PropagationFlags.None,
                        AccessControlType.Allow));
                }
                catch (Exception ex)
                {
                    // Log warning pero continuar
                }

                // Siempre otorgar permisos a administradores y al usuario actual
                SecurityIdentifier currentUser = WindowsIdentity.GetCurrent().User;
                SecurityIdentifier adminGroup = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);

                sec.AddAccessRule(new FileSystemAccessRule(
                    currentUser,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

                sec.AddAccessRule(new FileSystemAccessRule(
                    adminGroup,
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

                Directory.SetAccessControl(folderPath, sec);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error configurando permisos para {folderPath}: {ex.Message}", ex);
            }
        }

        private void ConfigureDirectorySecurity()
        {
            var sec = Directory.GetAccessControl(CanaryFolder);

            // Otorgar control total al sistema
            sec.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));

            // Otorgar control total al usuario actual
            SecurityIdentifier currentUser = WindowsIdentity.GetCurrent().User;
            sec.AddAccessRule(new FileSystemAccessRule(
                currentUser,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));

            // Otorgar control total a administradores
            sec.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));

            // Otorgar permisos de lectura/escritura a usuarios autenticados
            sec.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                FileSystemRights.ReadAndExecute | FileSystemRights.Write | FileSystemRights.CreateFiles | FileSystemRights.CreateDirectories,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));

            sec.SetAccessRuleProtection(true, false); // Proteger reglas heredadas
            Directory.SetAccessControl(CanaryFolder, sec);
        }

        private void CreateAdvancedCanaryFile()
        {
            try
            {
                // Generar contenido con metadata falsa
                string content = GenerateDeceptiveContent();

                // Escribir archivo
                File.WriteAllText(FullCanaryPath, content, Encoding.UTF8);

                // Dejar el archivo con atributos normales (visible)
                if (isRunningAsAdmin)
                {
                    try
                    {
                        File.SetAttributes(FullCanaryPath, FileAttributes.Normal);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Log warning pero continuar
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creando archivo señuelo: {ex.Message}", ex);
            }
        }

        private string GenerateDeceptiveContent()
        {
            // Generar datos engañosos realistas
            string timestamp = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
            string[] fakeUsers = {
                "admin_cai:AHx72#9!ksL",
                "auditor:Pa$$w0rd2025!",
                "dba:DB@Secure123"
            };

            // Generar múltiples hashes de verificación
            string contentHash = ComputeSHA256Hash(string.Join("|", fakeUsers));
            string fileHash = ComputeMD5Hash(timestamp);

            return $@"=== CREDENCIALES MAESTRAS CAI ===
            Última actualización: {timestamp}

            === USUARIOS PRIVILEGIADOS ===
            {string.Join("\n", fakeUsers)}

            === HASHES DE VERIFICACIÓN ===
            Hash de contenido: {contentHash}
            Hash de archivo: {fileHash}

            === ADVERTENCIA DE SEGURIDAD ===
            Este archivo contiene información sensible protegida por
            el sistema de detección de intrusos CAI-Sentinel.
            Accesos no autorizados serán registrados y reportados
            automáticamente al equipo de seguridad.";
        }

        private void OnFileEvent(object sender, FileSystemEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                string currentUser = WindowsIdentity.GetCurrent().Name;
                string eventType = e.ChangeType.ToString();
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {eventType} en {e.FullPath} por {currentUser}";

                // Registrar evento básico
                File.AppendAllText(LogPath, logEntry + Environment.NewLine);

                // Análisis detallado según tipo de evento
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        HandleModificationEvent(e.FullPath, currentUser);
                        break;

                    case WatcherChangeTypes.Deleted:
                        HandleDeletionEvent(currentUser);
                        break;

                    case WatcherChangeTypes.Renamed:
                        HandleRenameEvent((RenamedEventArgs)e, currentUser);
                        break;

                    case WatcherChangeTypes.Created:
                        HandleCreationEvent(e.FullPath, currentUser);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error procesando evento: {ex}");
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            LogCriticalError($"Error en FileSystemWatcher: {e.GetException()}");

            // Intentar reiniciar el watcher
            try
            {
                _watcher.EnableRaisingEvents = false;
                Thread.Sleep(1000);
                _watcher.EnableRaisingEvents = true;
                LogSystemStatus("FileSystemWatcher reiniciado después de error");
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error al reiniciar FileSystemWatcher: {ex}");
            }
        }

        private void HandleModificationEvent(string filePath, string user)
        {
            try
            {
                string newHash = ComputeFileHash(filePath);

                if (newHash != _currentFileHash)
                {
                    string details = $"Hash original: {_currentFileHash}\nHash actual: {newHash}";
                    LogIntegrityBreach($"MODIFICACIÓN DETECTADA por {user}\n{details}");

                    // Respuesta proporcional
                    if (IsCriticalModification(newHash))
                    {
                        TakeSecurityAction(SecurityAction.FullLockdown, user);
                    }
                    else
                    {
                        TakeSecurityAction(SecurityAction.ImmediateResponse, user);
                    }
                }
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error en manejo de modificación: {ex}");
            }
        }

        private void HandleDeletionEvent(string user)
        {
            // 1. Registrar el evento como crítico
            LogIntegrityBreach($"¡ELIMINACIÓN DETECTADA! Usuario: {user}");

            // 2. Bloquear inmediatamente la estación
            LockWorkstation();

            // 3. Enviar alerta máxima prioridad
            SendAlert("¡ALERTA ROJA! Archivo señuelo eliminado",
                     $"Acción realizada por: {user}\n" +
                     $"Equipo: {Environment.MachineName}\n" +
                     $"Hora del evento: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
                     "SE HA ACTIVADO EL PROTOCOLO DE SEGURIDAD");

            // 4. Cerrar sesión después de 3 segundos
            Thread.Sleep(3000);
            ForceLogoff();

            // 5. Si es admin, apagar el equipo después de 10 segundos
            if (isRunningAsAdmin)
            {
                Thread.Sleep(7000); // +7 segundos (total 10 desde el inicio)
                InitiateShutdown();
            }

            // 6. Recrear el archivo para futuras detecciones
            Thread.Sleep(2000); // Esperar 2 segundos antes de recrear
            CreateAdvancedCanaryFile();
            _currentFileHash = ComputeFileHash(FullCanaryPath);

            IsolateCanaryFolder();
        }

        private void HandleRenameEvent(RenamedEventArgs e, string user)
        {
            LogIntegrityBreach($"INTENTO DE RENOMBRADO: {e.OldName} -> {e.Name} por {user}");
            TakeSecurityAction(SecurityAction.ImmediateResponse, user);
        }

        private void HandleCreationEvent(string filePath, string user)
        {
            // Verificar si es un intento de reemplazo
            if (filePath.Equals(FullCanaryPath, StringComparison.OrdinalIgnoreCase))
            {
                LogIntegrityBreach($"INTENTO DE REEMPLAZO por {user}");
                TakeSecurityAction(SecurityAction.ImmediateResponse, user);
            }
        }

        private void TakeSecurityAction(SecurityAction action, string user)
        {
            string actionMessage = $"Acción: {action} para {user}";
            File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {actionMessage}\n");

            switch (action)
            {
                case SecurityAction.ImmediateResponse:
                    SendAlert("ALERTA: Intento de compromiso detectado",
                             $"{actionMessage}\nEquipo: {Environment.MachineName}");
                    LockWorkstation();
                    if (isRunningAsAdmin)
                    {
                        DisableNetworkAdapters();
                    }
                    break;

                case SecurityAction.FullLockdown:
                    SendAlert("EMERGENCIA: Intento crítico detectado",
                             $"{actionMessage}\nEQUIPO: {Environment.MachineName}\nACCIÓN: Bloqueo total iniciado");
                    LockWorkstation();
                    if (isRunningAsAdmin)
                    {
                        DisableNetworkAdapters();
                        ForceLogoff();
                        InitiateShutdown();
                    }
                    break;
            }
        }

        private void StartIntegrityMonitoring()
        {
            new Thread(() =>
            {
                while (_isInitialized)
                {
                    Thread.Sleep(TimeSpan.FromMinutes(30));

                    try
                    {
                        string currentHash = ComputeFileHash(FullCanaryPath);
                        if (currentHash != _currentFileHash)
                        {
                            LogIntegrityBreach($"ALTERACIÓN DETECTADA EN VERIFICACIÓN PERIÓDICA\nHash esperado: {_currentFileHash}\nHash actual: {currentHash}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCriticalError($"Error en monitoreo periódico: {ex}");
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        private void StartLogCleanupService()
        {
            new Thread(() =>
            {
                while (_isInitialized)
                {
                    Thread.Sleep(TimeSpan.FromHours(6));

                    try
                    {
                        CleanupLogFile(LogPath, 10_000_000); // 10MB max
                        CleanupLogFile(IntegrityLog, 5_000_000); // 5MB max
                    }
                    catch (Exception ex)
                    {
                        LogCriticalError($"Error limpiando logs: {ex}");
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        private void StartHealthCheckService()
        {
            new Thread(() =>
            {
                while (_isInitialized)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(30)); // Verificar cada 30 segundos

                    try
                    {
                        // Verificación de existencia del archivo
                        if (!File.Exists(FullCanaryPath))
                        {
                            HandleDeletionEvent("Sistema (detección periódica)");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCriticalError($"Error en verificación de salud: {ex}");
                    }
                }
            }).Start();
        }

        private void StartOpenCloseMonitoring()
        {
            new Thread(() =>
            {
                bool wasOpen = false;
                while (_isInitialized)
                {
                    Thread.Sleep(1000);
                    try
                    {
                        using (FileStream fs = new FileStream(FullCanaryPath, FileMode.Open, FileAccess.Read, FileShare.None))
                        {
                            if (wasOpen)
                            {
                                // Se acaba de cerrar
                                SendAlert("Archivo señuelo cerrado", $"El archivo fue cerrado en {DateTime.Now}");
                                wasOpen = false;
                            }
                        }
                    }
                    catch (IOException)
                    {
                        if (!wasOpen)
                        {
                            // Se acaba de abrir
                            SendAlert("Archivo señuelo abierto", $"El archivo fue abierto en {DateTime.Now}");

                            // 1. Deshabilitar adaptadores de red
                            DisableNetworkAdapters();

                            // 2. Esperar hasta que no haya ping
                            WaitForNoPing();

                            // 3. Cerrar sesión
                            ForceLogoff();

                            wasOpen = true;
                        }
                    }
                }
            })
            { IsBackground = true }.Start();
        }

        // Método para esperar hasta que no haya ping
        private void WaitForNoPing()
        {
            bool hayPing = true;
            while (hayPing)
            {
                try
                {
                    using (var ping = new System.Net.NetworkInformation.Ping())
                    {
                        var reply = ping.Send("8.8.8.8", 1000); // Google DNS
                        hayPing = (reply.Status == System.Net.NetworkInformation.IPStatus.Success);
                    }
                }
                catch
                {
                    hayPing = false; // Si hay error, asumimos que no hay red
                }
                if (hayPing)
                    Thread.Sleep(1000); // Espera 1 segundo antes de volver a intentar
            }
        }

        private void CleanupLogFile(string path, long maxSize)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                if (fileInfo.Exists && fileInfo.Length > maxSize)
                {
                    // Rotar archivo de log
                    string rotatedPath = path + "." + DateTime.Now.ToString("yyyyMMddHHmmss");
                    File.Move(path, rotatedPath);

                    LogSystemStatus($"Log rotado: {Path.GetFileName(path)} -> {Path.GetFileName(rotatedPath)}");
                }
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error en rotación de logs: {ex}");
            }
        }

        private string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", "");
            }
        }

        private string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        private string ComputeMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        private bool IsCriticalModification(string newHash)
        {
            // Considerar crítica si cambian los primeros 16 caracteres del hash
            return _currentFileHash.Substring(0, 16) != newHash.Substring(0, 16);
        }

        private void SendAlert(string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpConfig.Server, _smtpConfig.Port))
                using (var message = new MailMessage(_smtpConfig.FromEmail, _smtpConfig.ToEmail, subject, body))
                {
                    client.EnableSsl = _smtpConfig.UseSsl;
                    client.Credentials = new System.Net.NetworkCredential(_smtpConfig.Username, _smtpConfig.Password);

                    if (!string.IsNullOrEmpty(_smtpConfig.CcEmail))
                        message.CC.Add(_smtpConfig.CcEmail);
                    message.Priority = MailPriority.High;

                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(LogPath, $"[ERROR] Fallo al enviar alerta: {ex.Message}\n");
            }
        }

        private void LogInitialFingerprint()
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ARCHIVO SEÑUELO INICIALIZADO\nHash: {_currentFileHash}";
            File.AppendAllText(IntegrityLog, logEntry + Environment.NewLine);
        }

        private void LogIntegrityBreach(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] VIOLACIÓN DE INTEGRIDAD\n{message}";
            File.AppendAllText(IntegrityLog, logEntry + Environment.NewLine);
        }

        private void LogSystemStatus(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ESTADO DEL SISTEMA\n{message}";
            File.AppendAllText(LogPath, logEntry + Environment.NewLine);
        }

        private void LogCriticalError(string error)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR CRÍTICO\n{error}";
            File.AppendAllText(LogPath, logEntry + Environment.NewLine);
            File.AppendAllText(IntegrityLog, logEntry + Environment.NewLine);

            // Notificación inmediata para errores críticos
            SendAlert("FALLO EN CAI-SENTINEL", error);
        }

        // Métodos de acción de seguridad
        private void LockWorkstation()
        {
            try
            {
                Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error al bloquear estación: {ex}");
            }
        }

        private void DisableNetworkAdapters()
        {
            try
            {
                Process.Start("powershell", "Disable-NetAdapter -Name '*' -Confirm:$false");
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error al deshabilitar red: {ex}");
            }
        }

        private void ForceLogoff()
        {
            try
            {
                Process.Start("shutdown.exe", "/l /f");
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error al forzar cierre de sesión: {ex}");
            }
        }

        private void InitiateShutdown()
        {
            try
            {
                Process.Start("shutdown.exe", "/s /t 0 /f");
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error al iniciar apagado: {ex}");
            }
        }

        private void IsolateCanaryFolder()
        {
            try
            {
                var sec = new DirectorySecurity();
                // Solo el sistema y administradores pueden acceder
                sec.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                sec.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                Directory.SetAccessControl(CanaryFolder, sec);
            }
            catch (Exception ex)
            {
                LogCriticalError($"Error al aislar carpeta: {ex}");
            }
        }

        // Hilo para monitorear eventos de auditoría de acceso (ID 4663)
        private void StartAuditEventMonitoring()
        {
            new Thread(() =>
            {
                string lastRecordId = null;
                string fileToMonitor = FullCanaryPath.ToLower();
                string currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                string exeName = System.Diagnostics.Process.GetCurrentProcess().ProcessName; // Sin .exe
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location.ToLower();
                while (_isInitialized)
                {
                    try
                    {
                        // Leer los últimos 10 eventos de seguridad con ID 4663
                        var log = new System.Diagnostics.EventLog("Security");
                        var entries = log.Entries.Cast<EventLogEntry>()
                            .Where(e => e.InstanceId == 4663)
                            .OrderByDescending(e => e.TimeGenerated)
                            .Take(10);
                        foreach (var entry in entries)
                        {
                            // Evitar procesar el mismo evento varias veces
                            if (lastRecordId != null && entry.InstanceId.ToString() == lastRecordId)
                                break;
                            // Buscar el nombre del archivo en el mensaje
                            if (entry.Message.ToLower().Contains(fileToMonitor))
                            {
                                // Evitar falso positivo: ignorar si el usuario es el mismo que ejecuta CAI-Sentinel
                                if (!string.IsNullOrEmpty(entry.UserName) && entry.UserName.Equals(currentUser, StringComparison.OrdinalIgnoreCase))
                                    continue;
                                // Evitar falso positivo: ignorar si el proceso es CAI-Sentinel
                                string processInfo = entry.Message.ToLower();
                                if (processInfo.Contains(exeName.ToLower()) || processInfo.Contains(exePath))
                                    continue;
                                SendAlert("Auditoría: Acceso al archivo señuelo detectado", $"Se detectó acceso al archivo por auditoría de Windows. Usuario: {entry.UserName}\nHora: {entry.TimeGenerated}");
                                // 1. Deshabilitar adaptadores de red
                                DisableNetworkAdapters();
                                // 2. Esperar hasta que no haya ping
                                WaitForNoPing();
                                // 3. Cerrar sesión de Windows
                                ForceLogoff();
                                lastRecordId = entry.InstanceId.ToString();
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogCriticalError($"Error monitoreando eventos de auditoría: {ex}");
                    }
                    Thread.Sleep(2000); // Esperar 2 segundos antes de volver a revisar
                }
            })
            { IsBackground = true }.Start();
        }
    }

    public enum SecurityAction
    {
        ImmediateResponse,  // Bloqueo + alerta
        FullLockdown       // Apagado completo
    }
}
