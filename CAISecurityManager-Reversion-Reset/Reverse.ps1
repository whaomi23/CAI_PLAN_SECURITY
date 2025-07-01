<#
.SYNOPSIS
    Script de reversión completa para la estructura CAI
.DESCRIPTION
    Este script elimina todos los usuarios, carpetas y permisos creados por el sistema CAI
    Requiere ejecución como administrador y maneja TrustedInstaller
.NOTES
    File Name      : CAI_Reversion.ps1
    Prerequisites  : PowerShell 5.1 o superior, ejecutar como Administrador
    Version        : 2.0
#>

# Configuración inicial
$logPath = Join-Path -Path ([Environment]::GetFolderPath("Desktop")) -ChildPath "CAI_Reversion.log"
$startTime = Get-Date
$basePath = "C:\CAI"

# Lista de usuarios a eliminar
$usersToRemove = @("CAI_Adm", "CAI_Pub", "CAI_Priv", "CAI_DB", "CAI_Backup", "CAI_Docs", "CAI_Users")

# Función para registrar eventos
function Write-Log {
    param (
        [string]$message,
        [string]$level = "INFO"
    )
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logEntry = "[$timestamp] [$level] $message"
    Add-Content -Path $logPath -Value $logEntry
    Write-Host $logEntry
}

# Función para tomar ownership
function Take-Ownership {
    param (
        [string]$path
    )
    try {
        Write-Log "Tomando ownership de $path"
        
        # Usar takeown.exe para archivos y carpetas
        if (Test-Path $path -PathType Container) {
            $null = takeown.exe /F $path /R /A /D Y 2>&1
            $null = icacls.exe $path /grant Administrators:F /T /C /Q 2>&1
        } else {
            $null = takeown.exe /F $path /A 2>&1
            $null = icacls.exe $path /grant Administrators:F /Q 2>&1
        }
        
        # Configurar permisos mediante PowerShell
        $acl = Get-Acl $path
        $adminRule = New-Object System.Security.AccessControl.FileSystemAccessRule(
            "Administrators",
            "FullControl",
            "ContainerInherit,ObjectInherit",
            "None",
            "Allow"
        )
        $acl.SetAccessRule($adminRule)
        Set-Acl -Path $path -AclObject $acl
        
        Write-Log "Ownership tomado y permisos configurados para $path"
        return $true
    } catch {
        Write-Log "Error al tomar ownership de $path - $_" -level "ERROR"
        return $false
    }
}

# Función para eliminar recursivamente con múltiples métodos
function Remove-ItemForce {
    param (
        [string]$path
    )
    
    if (-not (Test-Path $path)) {
        Write-Log "La ruta $path no existe, omitiendo"
        return
    }

    # Intentar eliminación normal primero
    try {
        Remove-Item -Path $path -Recurse -Force -ErrorAction Stop
        Write-Log "Eliminado correctamente: $path"
        return
    } catch {
        Write-Log "Fallo eliminación normal de $path, intentando métodos alternativos..."
    }

    # Tomar ownership si falla
    if (Take-Ownership -path $path) {
        try {
            Remove-Item -Path $path -Recurse -Force -ErrorAction Stop
            Write-Log "Eliminado después de tomar ownership: $path"
            return
        } catch {
            Write-Log "Fallo después de tomar ownership: $path"
        }
    }

    # Método alternativo usando cmd
    try {
        $parent = Split-Path -Path $path -Parent
        $leaf = Split-Path -Path $path -Leaf
        
        # Crear comando CMD para eliminar
        $cmdScript = @"
@echo off
cd /d "$parent"
rmdir /s /q "$leaf" >nul 2>&1
if exist "$leaf" (
    del /f /q /a "$leaf\*" >nul 2>&1
    rmdir /s /q "$leaf" >nul 2>&1
)
"@
        
        $tempBat = [System.IO.Path]::GetTempFileName() + ".bat"
        Set-Content -Path $tempBat -Value $cmdScript
        Start-Process -FilePath $tempBat -WindowStyle Hidden -Wait
        Remove-Item -Path $tempBat -Force
        
        if (-not (Test-Path $path)) {
            Write-Log "Eliminado usando método CMD: $path"
            return
        }
    } catch {
        Write-Log "Fallo método CMD para $path"
    }

    # Último intento con robocopy
    try {
        $emptyDir = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "empty_$(Get-Random)"
        New-Item -ItemType Directory -Path $emptyDir -Force | Out-Null
        
        $null = robocopy.exe $emptyDir $path /mir /njh /njs /ndl /nc /ns /np /nfl /ndl
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue
        
        if (-not (Test-Path $path)) {
            Write-Log "Eliminado usando robocopy: $path"
        } else {
            Write-Log "No se pudo eliminar completamente $path - Puede requerir reinicio o modo seguro" -level "WARNING"
        }
        
        Remove-Item -Path $emptyDir -Recurse -Force -ErrorAction SilentlyContinue
    } catch {
        Write-Log "Error crítico al intentar eliminar $path - $_" -level "ERROR"
    }
}

# Inicio del proceso de reversión
Write-Log "=== INICIANDO PROCESO DE REVERSIÓN CAI ==="
Write-Log "Hora de inicio: $startTime"
Write-Log "Usuario ejecutando: $env:USERNAME"
Write-Log "Equipo: $env:COMPUTERNAME"

# Verificar privilegios de administrador
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Log "Este script debe ejecutarse como Administrador" -level "ERROR"
    pause
    exit 1
}

# 1. Eliminar usuarios locales
Write-Log "=== ELIMINANDO USUARIOS LOCALES ==="
foreach ($user in $usersToRemove) {
    try {
        $userObj = [ADSI]"WinNT://$env:COMPUTERNAME/$user"
        if ($userObj.Path) {
            [ADSI]"WinNT://$env:COMPUTERNAME" | ForEach-Object {
                $_.Delete("user", $user)
            }
            Write-Log "Usuario eliminado: $user"
        }
    } catch {
        Write-Log "No se pudo eliminar el usuario $user - $_" -level "WARNING"
    }
}

# 2. Eliminar estructura de carpetas
Write-Log "=== ELIMINANDO ESTRUCTURA DE CARPETAS ==="
if (Test-Path $basePath) {
    # Eliminar contenido recursivamente
    Get-ChildItem -Path $basePath -Recurse | Sort-Object -Property FullName -Descending | ForEach-Object {
        Remove-ItemForce -path $_.FullName
    }
    
    # Eliminar carpeta principal
    Remove-ItemForce -path $basePath
} else {
    Write-Log "La carpeta CAI no existe en $basePath"
}

# 3. Eliminar archivo de contraseñas si existe
$keyFile = Join-Path -Path ([Environment]::GetFolderPath("Desktop")) -ChildPath "CAI_ClavesSeguras.txt"
if (Test-Path $keyFile) {
    try {
        Remove-Item -Path $keyFile -Force
        Write-Log "Archivo de contraseñas eliminado: $keyFile"
    } catch {
        Write-Log "No se pudo eliminar el archivo de contraseñas - $_" -level "WARNING"
    }
}

# 4. Eliminar registro de eventos si existe
$logFile = Join-Path -Path $basePath -ChildPath "Seguridad.log"
if (Test-Path $logFile) {
    try {
        Remove-Item -Path $logFile -Force
        Write-Log "Archivo de log eliminado: $logFile"
    } catch {
        Write-Log "No se pudo eliminar el archivo de log - $_" -level "WARNING"
    }
}

# 5. Restaurar permisos por defecto en la raíz (si la carpeta persiste)
if (Test-Path $basePath) {
    try {
        $null = icacls.exe $basePath /reset /T /C /Q 2>&1
        Write-Log "Permisos restaurados a valores por defecto en $basePath"
    } catch {
        Write-Log "No se pudieron restaurar los permisos por defecto - $_" -level "WARNING"
    }
}

# Finalización
$endTime = Get-Date
$duration = $endTime - $startTime
Write-Log "=== PROCESO DE REVERSIÓN COMPLETADO ==="
Write-Log "Hora de finalización: $endTime"
Write-Log "Duración total: $($duration.TotalMinutes.ToString('0.00')) minutos"

# Mostrar resumen final
Write-Host "`n=== RESUMEN FINAL ==="
Write-Host "Log completo guardado en: $logPath"
Write-Host "Revise el archivo de log para detalles de cualquier error"

pause