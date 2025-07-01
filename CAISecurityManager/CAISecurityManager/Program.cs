using System;
using System.DirectoryServices;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Collections.Generic;

namespace CAISecurityManager
{
    class Program
    {
        static string basePath = @"C:\CAI";
        static string usuariosPath = Path.Combine(basePath, "usuarios");
        static string logPath = Path.Combine(basePath, "Seguridad.log");
        static string keyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "CAI_ClavesSeguras.txt");

        // Contraseñas predefinidas (debes cambiarlas antes de usar el script)
        static readonly Dictionary<string, string> predefinedPasswords = new Dictionary<string, string>
        {
            { "CAI_Adm", "Adm1n!CAI#2023Secure" },   // 18 caracteres: Mayúsculas, minúsculas, números y símbolos
            { "CAI_Pub", "PubL1c@CAI$2023Secure" },   // 18 caracteres: Mayúsculas, minúsculas, números y símbolos
            { "CAI_Priv", "Pr1v@t3CAI!2023Secure" },  // 18 caracteres: Mayúsculas, minúsculas, números y símbolos
            { "CAI_DB", "DB@dm1nCAI$2023Secure" },    // 18 caracteres: Mayúsculas, minúsculas, números y símbolos
            { "CAI_Backup", "B@ckUpCAI!2023Secure" }, // 18 caracteres: Mayúsculas, minúsculas, números y símbolos
            { "CAI_Docs", "D0csT3chCAI#2023Secure" },  // 18 caracteres: Mayúsculas, minúsculas, números y símbolos
            { "CAI_Users", "Us3r5CAI@2023Secure" }    // Nuevo usuario para la carpeta usuarios
        };

        static void Main(string[] args)
        {
            try
            {
                InitializeSecurityInfrastructure();
                CreateFolderStructure();
                ConfigureSpecialPermissions();
                Console.WriteLine("Configuración completada con éxito.");
                Console.WriteLine($"Archivo de contraseñas generado en: {keyPath}");
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{keyPath}\"");
            }
            catch (Exception ex)
            {
                LogError($"Error general: {ex.ToString()}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void InitializeSecurityInfrastructure()
        {
            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
                LogAction("✅ Carpeta raíz creada");
            }

            // Crear carpeta usuarios si no existe
            if (!Directory.Exists(usuariosPath))
            {
                Directory.CreateDirectory(usuariosPath);
                LogAction("✅ Carpeta usuarios creada");
            }

            File.WriteAllText(keyPath, $"=== CONTRASEÑAS SEGURAS DEL CAI ===\nGeneradas el {DateTime.Now}\n\n");
        }

        static void CreateFolderStructure()
        {
            var folders = new[]
            {
                new {
                    Path = "Administración",
                    User = "CAI_Adm",
                    Permission = "Modify",
                    Description = "Coordinadores del CAI"
                },
                new {
                    Path = @"Proyectos\Públicos",
                    User = "CAI_Pub",
                    Permission = "ReadAndExecute",
                    Description = "Acceso público"
                },
                new {
                    Path = @"Proyectos\Privados",
                    User = "CAI_Priv",
                    Permission = "Modify",
                    Description = "Proyectos internos"
                },
                new {
                    Path = @"Base de Datos",
                    User = "CAI_DB",
                    Permission = "Modify",
                    Description = "Responsables técnicos"
                },
                new {
                    Path = @"Respaldos",
                    User = "CAI_Backup",
                    Permission = "Modify",
                    Description = "Personal autorizado"
                },
                new {
                    Path = @"Documentación Técnica\Manuales",
                    User = "CAI_Docs",
                    Permission = "ReadAndExecute",
                    Description = "Documentación técnica"
                },
                new {
                    Path = @"Documentación Técnica\Arquitecturas",
                    User = "CAI_Docs",
                    Permission = "ReadAndExecute",
                    Description = "Documentación técnica"
                },
                new {
                    Path = @"usuarios",
                    User = "CAI_Users",
                    Permission = "Modify",
                    Description = "Usuarios del sistema"
                }
            };

            foreach (var folder in folders)
            {
                string fullPath = Path.Combine(basePath, folder.Path);

                // Obtener la contraseña predefinida
                if (!predefinedPasswords.TryGetValue(folder.User, out string password))
                {
                    LogError($"No se encontró contraseña predefinida para {folder.User}");
                    continue;
                }

                // Verificar que la contraseña cumple con los requisitos
                if (!IsPasswordStrong(password))
                {
                    LogError($"La contraseña para {folder.User} no cumple los requisitos de seguridad");
                    continue;
                }

                // Crear carpeta (excepto usuarios que ya se creó)
                if (!Directory.Exists(fullPath) && !fullPath.Equals(usuariosPath))
                {
                    Directory.CreateDirectory(fullPath);
                    LogAction($"📂 Carpeta creada: {folder.Path}");
                }

                // Crear usuario (solo si no existe)
                if (!UserExists(folder.User))
                {
                    if (CreateLocalUser(folder.User, password, folder.Description))
                    {
                        LogAction($"👤 Usuario creado: {folder.User}");
                        File.AppendAllText(keyPath, $"{folder.User}: {password} ({folder.Description})\n");
                    }
                }

                // Configurar permisos
                SetFolderPermissions(fullPath, folder.User, folder.Permission);
                LogAction($"🔒 Permisos configurados para {folder.User} en {folder.Path}");
            }
        }

        static void ConfigureSpecialPermissions()
        {
            try
            {
                // Configurar permisos para la carpeta raíz CAI
                DirectoryInfo caiRoot = new DirectoryInfo(basePath);
                DirectorySecurity caiRootSecurity = caiRoot.GetAccessControl();

                // Permitir acceso a todos los usuarios pero sin eliminación
                caiRootSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.ReadAndExecute | FileSystemRights.Write | FileSystemRights.ListDirectory,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

                // Denegar eliminación para todos excepto administradores
                caiRootSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles,
                    AccessControlType.Deny));

                caiRoot.SetAccessControl(caiRootSecurity);
                LogAction("🔒 Permisos especiales configurados para la carpeta CAI");

                // Configurar permisos para la carpeta usuarios
                DirectoryInfo usuariosDir = new DirectoryInfo(usuariosPath);
                DirectorySecurity usuariosSecurity = usuariosDir.GetAccessControl();

                // Otorgar control total al usuario CAI_Users
                usuariosSecurity.AddAccessRule(new FileSystemAccessRule(
                    new NTAccount(Environment.MachineName + "\\CAI_Users"),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow));

                usuariosDir.SetAccessControl(usuariosSecurity);
                LogAction("🔒 Permisos especiales configurados para la carpeta usuarios");
            }
            catch (Exception ex)
            {
                LogError($"Error al configurar permisos especiales: {ex.Message}");
                throw;
            }
        }

        static bool UserExists(string username)
        {
            try
            {
                DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName);
                DirectoryEntry user = localDirectory.Children.Find(username, "user");
                return true;
            }
            catch
            {
                return false;
            }
        }

        static bool IsPasswordStrong(string password)
        {
            // Verificar longitud mínima de 18 caracteres
            if (password.Length < 18)
                return false;

            // Verificar complejidad
            bool hasUpper = false;
            bool hasLower = false;
            bool hasDigit = false;
            bool hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSpecial = true;
            }

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        static bool CreateLocalUser(string username, string password, string description)
        {
            try
            {
                DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName);
                DirectoryEntry user = localDirectory.Children.Add(username, "user");
                user.Invoke("SetPassword", password);
                user.Properties["Description"].Add(description);
                user.Properties["UserFlags"].Add(0x10000); // Password never expires
                user.CommitChanges();

                // Agregar al grupo Usuarios (Users en español)
                try
                {
                    DirectoryEntry group = localDirectory.Children.Find("Users", "group");
                    group.Invoke("Add", user.Path);
                }
                catch
                {
                    // Intentar con nombre en español si falla
                    DirectoryEntry group = localDirectory.Children.Find("Usuarios", "group");
                    group.Invoke("Add", user.Path);
                }

                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error al crear usuario {username}: {ex.Message}");
                return false;
            }
        }

        static void SetFolderPermissions(string folderPath, string username, string permission)
        {
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(folderPath);
                DirectorySecurity dirSecurity = dirInfo.GetAccessControl();

                // Eliminar herencia
                dirSecurity.SetAccessRuleProtection(true, false);

                // Eliminar permisos existentes no deseados
                var rules = dirSecurity.GetAccessRules(true, false, typeof(NTAccount));
                foreach (FileSystemAccessRule rule in rules)
                {
                    if (!rule.IdentityReference.Value.StartsWith("BUILTIN") &&
                        !rule.IdentityReference.Value.StartsWith("NT AUTHORITY"))
                    {
                        dirSecurity.RemoveAccessRule(rule);
                    }
                }

                // Agregar nuevo permiso
                FileSystemRights rights;
                switch (permission)
                {
                    case "Read":
                        rights = FileSystemRights.Read;
                        break;
                    case "ReadAndExecute":
                        rights = FileSystemRights.ReadAndExecute;
                        break;
                    case "Modify":
                        rights = FileSystemRights.Modify;
                        break;
                    case "FullControl":
                        rights = FileSystemRights.FullControl;
                        break;
                    default:
                        rights = FileSystemRights.ReadAndExecute;
                        break;
                }

                var accessRule = new FileSystemAccessRule(
                    new NTAccount(Environment.MachineName + "\\" + username),
                    rights,
                    InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,
                    AccessControlType.Allow);

                dirSecurity.AddAccessRule(accessRule);

                // Denegar eliminación para todos excepto administradores
                if (!username.Equals("CAI_Adm") && !folderPath.Equals(usuariosPath))
                {
                    var denyRule = new FileSystemAccessRule(
                        new NTAccount(Environment.MachineName + "\\" + username),
                        FileSystemRights.Delete | FileSystemRights.DeleteSubdirectoriesAndFiles,
                        AccessControlType.Deny);

                    dirSecurity.AddAccessRule(denyRule);
                }

                dirInfo.SetAccessControl(dirSecurity);
            }
            catch (Exception ex)
            {
                LogError($"Error al configurar permisos en {folderPath}: {ex.Message}");
                throw;
            }
        }

        static void LogAction(string message)
        {
            string logMessage = $"[{DateTime.Now}] {message}\n";
            File.AppendAllText(logPath, logMessage);
            Console.WriteLine(message);
        }

        static void LogError(string error)
        {
            string logMessage = $"[{DateTime.Now}] ❌ {error}\n";
            File.AppendAllText(logPath, logMessage);
            Console.WriteLine(error);
        }
    }
}