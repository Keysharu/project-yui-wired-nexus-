@echo off
title Cyber-Companion: Rebeca
cd /d "%~dp0"

:: Comprobar pythonw en C:\Python314 o en PATH
if exist "C:\Python314\pythonw.exe" (
    start "" "C:\Python314\pythonw.exe" main.py
    exit /b
)

where pythonw >nul 2>&1
if %errorlevel% equ 0 (
    start "" pythonw main.py
    exit /b
)

:: Fallback con python normal si pythonw no está registrado
start "" python main.py
