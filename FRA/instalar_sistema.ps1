# Script de instalación del Sistema de Alertas Forenses CAI - Edición Cyberpunk
# Autor: Sistema de Seguridad CAI
# Versión: Cyberpunk Edition

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                SISTEMA DE ALERTAS FORENSES CAI               ║" -ForegroundColor Magenta
Write-Host "║                     EDICIÓN CYBERPUNK                        ║" -ForegroundColor Magenta
Write-Host "║                  [MONITOREO DE SEGURIDAD]                    ║" -ForegroundColor Magenta
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Host ""

# Verificar si Python está instalado
Write-Host "🔍 Verificando instalación de Python..." -ForegroundColor Cyan
try {
    $pythonVersion = python --version
    Write-Host "✅ Python encontrado: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Python no está instalado. Por favor instala Python 3.7 o superior." -ForegroundColor Red
    exit 1
}

# Instalar dependencias
Write-Host "📦 Instalando dependencias..." -ForegroundColor Cyan
try {
    pip install -r requirements.txt
    Write-Host "✅ Dependencias instaladas correctamente" -ForegroundColor Green
} catch {
    Write-Host "❌ Error al instalar dependencias" -ForegroundColor Red
    exit 1
}

# Configurar persistencia automática
Write-Host "⚡ Configurando persistencia automática..." -ForegroundColor Cyan

# Obtener la ruta del script actual
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$pythonScript = Join-Path $scriptPath "forensic_alert.py"

# Crear entrada en el registro para ejecutar al iniciar sesión
try {
    $registryPath = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run"
    $name = "CAIForensicMonitor"
    $value = "python `"$pythonScript`" --minimized"
    
    Set-ItemProperty -Path $registryPath -Name $name -Value $value
    Write-Host "✅ Persistencia configurada en el registro" -ForegroundColor Green
} catch {
    Write-Host "❌ Error al configurar persistencia en el registro" -ForegroundColor Red
}

# Crear acceso directo en la carpeta de inicio
try {
    $startupFolder = [Environment]::GetFolderPath("Startup")
    $shortcutPath = Join-Path $startupFolder "CAIForensicMonitor.lnk"
    
    $WshShell = New-Object -comObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($shortcutPath)
    $Shortcut.TargetPath = "python"
    $Shortcut.Arguments = "`"$pythonScript`" --minimized"
    $Shortcut.WorkingDirectory = $scriptPath
    $Shortcut.Description = "Sistema de Alertas Forenses CAI - Cyberpunk Edition"
    $Shortcut.Save()
    
    Write-Host "✅ Acceso directo creado en carpeta de inicio" -ForegroundColor Green
} catch {
    Write-Host "❌ Error al crear acceso directo" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎯 CONFIGURACIÓN COMPLETADA" -ForegroundColor Green
Write-Host "El sistema se ejecutará automáticamente cada vez que inicies sesión." -ForegroundColor Yellow
Write-Host ""

# Preguntar si quiere ejecutar ahora
$response = Read-Host "¿Deseas ejecutar el sistema ahora? (S/N)"
if ($response -eq "S" -or $response -eq "s" -or $response -eq "Y" -or $response -eq "y") {
    Write-Host "🚀 Iniciando sistema de monitoreo..." -ForegroundColor Cyan
    python $pythonScript
} else {
    Write-Host "El sistema está configurado y se ejecutará automáticamente al iniciar sesión." -ForegroundColor Green
    Write-Host "Presiona cualquier tecla para salir..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
} 