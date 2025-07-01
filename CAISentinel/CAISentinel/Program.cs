using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace CAISentinel
{
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

        // Configuración SMTP
        private const string SmtpServer = "smtp.gmail.com";
        private const int SmtpPort = 587;
        private const bool UseSsl = true;
        private const string AlertFrom = "neritpa@gmail.com";
        private const string AlertTo = "fxmspaha@gmail.com";
        private const string AlertCc = "fxmspaha@gmail.com";

 
        // Estado del sistema
        private string _currentFileHash;
        private readonly FileSystemWatcher _watcher;
        private bool _isInitialized = false;

        public AdvancedCanarySystem()
        {
            try
            {
                Console.WriteLine(" === MODO CONSOLA CAI-SENTINEL ===");
                Console.WriteLine("Inicializando sistema...");

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

                Console.WriteLine("FileSystemWatcher configurado correctamente");
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
                Console.WriteLine("Ejecutando como administrador - usando C:\\CAI");
                BasePath = @"C:\CAI";
            }
            else
            {
                Console.WriteLine("Ejecutando como usuario normal - usando directorio de usuario");
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
                Environment.Exit(1);
            }
        }

        private void CreateSecureFolderStructure()
        {
            Console.WriteLine("Creando estructura de directorios...");

            try
            {
                // Crear directorio base si no existe
                if (!Directory.Exists(BasePath))
                {
                    Directory.CreateDirectory(BasePath);
                    Console.WriteLine($"Directorio base creado: {BasePath}");

                    // Solo intentar establecer atributos si es administrador
                    if (isRunningAsAdmin)
                    {
                        try
                        {
                            Console.WriteLine("Atributos de directorio base configurados");
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Console.WriteLine($"Advertencia: No se pudieron establecer atributos en directorio base: {ex.Message}");
                        }
                    }
                }

                // Crear directorio de seguridad si no existe
                string securityPath = Path.GetDirectoryName(LogPath);
                if (!Directory.Exists(securityPath))
                {
                    Directory.CreateDirectory(securityPath);
                    Console.WriteLine($"Directorio de seguridad creado: {securityPath}");
                }

                // Crear directorio de claves con permisos restringidos
                if (!Directory.Exists(CanaryFolder))
                {
                    Directory.CreateDirectory(CanaryFolder);
                    Console.WriteLine($"Directorio de claves creado: {CanaryFolder}");

                    // Solo intentar configurar ACL si es administrador
                    if (isRunningAsAdmin)
                    {
                        try
                        {
                            ConfigureDirectorySecurity();
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            Console.WriteLine($"Advertencia: No se pudieron configurar permisos especiales: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nota: Ejecutando sin permisos de administrador - permisos básicos aplicados");
                    }
                }

                // Solo intentar establecer atributos si es administrador
                if (isRunningAsAdmin)
                {
                    try
                    {
                        File.SetAttributes(CanaryFolder, FileAttributes.Normal);
                        Console.WriteLine("Atributos de directorio configurados");
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Console.WriteLine($"Advertencia: No se pudieron establecer atributos: {ex.Message}");
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
            Console.WriteLine("Creando estructura de carpetas CAI...");

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
                        Console.WriteLine($"[INFO] Carpeta creada: {folder.Item1}");

                        // Configurar permisos si es administrador
                        if (isRunningAsAdmin)
                        {
                            try
                            {
                                ConfigureFolderPermissions(fullPath, folder.Item2, folder.Item3);
                                Console.WriteLine($"[INFO] Permisos configurados para {folder.Item2} en {folder.Item1}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[WARNING] No se pudieron configurar permisos para {folder.Item1}: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[INFO] Carpeta ya existe: {folder.Item1}");
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine($"[ERROR] Error de permisos en la carpeta {folder.Item1}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error al configurar {folder.Item1}: {ex.Message}");
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
                    Console.WriteLine($"[WARNING] No se pudo configurar usuario {userName}: {ex.Message}");
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
            Console.WriteLine("Permisos NTFS configurados: Acceso permitido, eliminación protegida");
        }

        private void CreateAdvancedCanaryFile()
        {
            Console.WriteLine("Creando archivo señuelo avanzado...");

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
                        Console.WriteLine($"Advertencia: No se pudieron establecer atributos en archivo: {ex.Message}");
                    }
                }

                Console.WriteLine($"Archivo señuelo creado: {FullCanaryPath}");
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
                Console.WriteLine($"Evento detectado: {logEntry}");

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
            Console.WriteLine($"Tomando acción de seguridad: {actionMessage}");

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
                using (var client = new SmtpClient(SmtpServer, SmtpPort))
                using (var message = new MailMessage(AlertFrom, AlertTo, subject, body))
                {
                    client.EnableSsl = UseSsl;
                    client.Credentials = new System.Net.NetworkCredential("neritpa@gmail.com", "ymwgrjmbwigsrydm");

                    message.CC.Add(AlertCc);
                    message.Priority = MailPriority.High;

                    client.Send(message);
                    Console.WriteLine($"Alerta enviada: {subject}");
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(LogPath, $"[ERROR] Fallo al enviar alerta: {ex.Message}\n");
                Console.WriteLine($"Error enviando alerta: {ex.Message}");
            }
        }

        private void LogInitialFingerprint()
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ARCHIVO SEÑUELO INICIALIZADO\nHash: {_currentFileHash}";
            File.AppendAllText(IntegrityLog, logEntry + Environment.NewLine);
            Console.WriteLine(logEntry);
        }

        private void LogIntegrityBreach(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] VIOLACIÓN DE INTEGRIDAD\n{message}";
            File.AppendAllText(IntegrityLog, logEntry + Environment.NewLine);
            Console.WriteLine(logEntry);
        }

        private void LogSystemStatus(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ESTADO DEL SISTEMA\n{message}";
            File.AppendAllText(LogPath, logEntry + Environment.NewLine);
            Console.WriteLine(logEntry);
        }

        private void LogCriticalError(string error)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR CRÍTICO\n{error}";
            File.AppendAllText(LogPath, logEntry + Environment.NewLine);
            File.AppendAllText(IntegrityLog, logEntry + Environment.NewLine);
            Console.WriteLine(logEntry);

            // Notificación inmediata para errores críticos
            SendAlert("FALLO EN CAI-SENTINEL", error);
        }

        // Métodos de acción de seguridad
        private void LockWorkstation()
        {
            try
            {
                Process.Start("rundll32.exe", "user32.dll,LockWorkStation");
                Console.WriteLine("Estación de trabajo bloqueada");
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
                Console.WriteLine("Adaptadores de red deshabilitados");
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
                Console.WriteLine("Sesión cerrada forzosamente");
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
                Console.WriteLine("Sistema apagado por seguridad");
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
                Console.WriteLine("Carpeta señuelo aislada tras eliminación.");
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

    public class SentinelService : ServiceBase
    {
        private AdvancedCanarySystem _system;

        public SentinelService()
        {
            ServiceName = "CAI-Sentinel";
            CanStop = true;
            CanPauseAndContinue = false;
            AutoLog = true;
            EventLog.Source = "CAI-Sentinel";
        }

        protected override void OnStart(string[] args)
        {
            EventLog.WriteEntry("Iniciando servicio CAI-Sentinel", EventLogEntryType.Information);

            _system = new AdvancedCanarySystem();
            Thread serviceThread = new Thread(_system.InitializeSystem)
            {
                IsBackground = true
            };
            serviceThread.Start();
        }

        protected override void OnStop()
        {
            EventLog.WriteEntry("Deteniendo servicio CAI-Sentinel", EventLogEntryType.Information);
            _system = null;
        }

        protected override void OnShutdown()
        {
            EventLog.WriteEntry("Sistema apagándose - Deteniendo CAI-Sentinel", EventLogEntryType.Warning);
            _system = null;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--service"))
            {
                ServiceBase.Run(new SentinelService());
            }
            else
            {
                // Modo consola para pruebas/debug
                Console.Title = "CAI-Sentinel Console Mode";
                Console.WriteLine("=== MODO CONSOLA CAI-SENTINEL ===");
                Console.WriteLine("Inicializando sistema...");

                try
                {
                    var system = new AdvancedCanarySystem();
                    system.InitializeSystem();

                    Console.WriteLine("\nSistema en ejecución. Presione Ctrl+C para salir.");
                    Console.WriteLine("Eventos se mostrarán aquí y se registrarán en los logs.");

                    // Mantener la aplicación corriendo
                    Thread.Sleep(Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR FATAL: {ex}");
                    Console.WriteLine("Presione cualquier tecla para salir...");
                    Console.ReadKey();
                }
            }
        }
    }
}

