@echo off
echo ========================================
echo    SISTEMA DE ALERTAS FORENSES CAI
echo         EDICIÓN CYBERPUNK
echo ========================================
echo.
echo Instalando dependencias...
pip install -r requirements.txt
echo.
echo Configurando persistencia automática...
echo.
echo El sistema se ejecutará automáticamente cada vez que inicies sesión.
echo.
echo Presiona cualquier tecla para continuar...
pause >nul
echo.
echo Iniciando sistema de monitoreo...
python forensic_alert.py 