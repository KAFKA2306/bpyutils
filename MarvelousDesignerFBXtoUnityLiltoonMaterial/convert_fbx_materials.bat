@echo off
REM FBX Material Converter - Batch execution script for Windows
REM This script runs Unity in headless mode to convert FBX materials to lilToon

setlocal enabledelayedexpansion

REM Configuration
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.15f1\Editor\Unity.exe"
set "PROJECT_PATH=%~dp0"
set "LOG_FILE=%PROJECT_PATH%Logs\conversion.log"
set "CONFIG_FILE=%PROJECT_PATH%Config\ConversionConfig.json"

REM Check if Unity exists
if not exist "%UNITY_PATH%" (
    echo ERROR: Unity not found at %UNITY_PATH%
    echo Please update UNITY_PATH in this script to point to your Unity installation
    pause
    exit /b 1
)

REM Check if project path exists
if not exist "%PROJECT_PATH%" (
    echo ERROR: Project path not found: %PROJECT_PATH%
    pause
    exit /b 1
)

REM Create logs directory if it doesn't exist
if not exist "%PROJECT_PATH%Logs" (
    mkdir "%PROJECT_PATH%Logs"
)

echo ============================================
echo FBX Material Converter - Headless Unity
echo ============================================
echo Unity Path: %UNITY_PATH%
echo Project Path: %PROJECT_PATH%
echo Config File: %CONFIG_FILE%
echo Log File: %LOG_FILE%
echo ============================================

REM Run Unity in batch mode
echo Starting conversion process...
"%UNITY_PATH%" -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod FBXMaterialConverter.FBXMaterialConverterBatch.ConvertAllFBXMaterials -logFile "%LOG_FILE%"

REM Check exit code
if !errorlevel! equ 0 (
    echo.
    echo ============================================
    echo CONVERSION COMPLETED SUCCESSFULLY
    echo ============================================
    echo Check the log file for details: %LOG_FILE%
) else (
    echo.
    echo ============================================
    echo CONVERSION FAILED WITH ERRORS
    echo ============================================
    echo Check the log file for details: %LOG_FILE%
    echo Exit code: !errorlevel!
)

echo.
echo Press any key to view the log file...
pause >nul

REM Open log file
if exist "%LOG_FILE%" (
    notepad "%LOG_FILE%"
) else (
    echo Log file not found: %LOG_FILE%
)

endlocal