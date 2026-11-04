@echo off
chcp 65001 > nul
title Buzon Automatico Discord - El taller de la gatita tech
echo ==========================================================
echo   PROCESANDO BUZON DE INTELIGENCIA DE DISCORD (META HACKATHON)
echo ==========================================================
echo.
python "%~dp0procesar_buzon.py"
echo.
echo ==========================================================
echo Operacion finalizada. Revisa TELEMETRIA_Y_NOVEDADES_DISCORD.md
echo ==========================================================
pause
