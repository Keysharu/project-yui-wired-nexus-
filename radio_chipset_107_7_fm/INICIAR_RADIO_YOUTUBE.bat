@echo off
title Radio Chipset 107.7 FM - Emision YouTube Live 24/7
color 0B
echo ======================================================================
echo    RADIO CHIPSET 107.7 FM // EMISORA 24/7 EN DIRECTO (YOUTUBE LIVE)
echo    El taller de la gatita tech - Modo Plato + Noticias + Debate IA
echo ======================================================================
echo.

cd /d "%~dp0"

python live_broadcaster.py

pause
