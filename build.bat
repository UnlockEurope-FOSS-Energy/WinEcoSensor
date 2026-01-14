@echo off
REM WinEcoSensor Build Script
REM SPDX-License-Identifier: EUPL-1.2
REM
REM This script builds the WinEcoSensor solution and creates the installer.
REM
REM Prerequisites:
REM   - Visual Studio 2019 or later with .NET Framework 4.8 SDK
REM   - Inno Setup 6.0 or later (for installer creation)
REM   - MSBuild in PATH
REM
REM Usage:
REM   build.bat [debug|release] [installer]

setlocal enabledelayedexpansion

echo ============================================
echo  WinEcoSensor Build Script
echo ============================================
echo.

REM Default configuration
set CONFIGURATION=Release
set BUILD_INSTALLER=0

REM Parse arguments
:parse_args
if "%1"=="" goto :start_build
if /i "%1"=="debug" set CONFIGURATION=Debug
if /i "%1"=="release" set CONFIGURATION=Release
if /i "%1"=="installer" set BUILD_INSTALLER=1
shift
goto :parse_args

:start_build
echo Configuration: %CONFIGURATION%
echo.

REM Find MSBuild
set MSBUILD=
for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul`) do (
    set MSBUILD=%%i
)

if "%MSBUILD%"=="" (
    echo ERROR: MSBuild not found. Please install Visual Studio with .NET Framework build tools.
    exit /b 1
)

echo Found MSBuild: %MSBUILD%
echo.

REM Restore NuGet packages (if any)
echo [1/4] Checking NuGet packages...
if exist ".nuget\NuGet.exe" (
    .nuget\NuGet.exe restore WinEcoSensor.sln
) else (
    echo No NuGet packages to restore.
)
echo.

REM Clean solution
echo [2/4] Cleaning solution...
"%MSBUILD%" WinEcoSensor.sln /t:Clean /p:Configuration=%CONFIGURATION% /p:Platform="Any CPU" /verbosity:minimal
if errorlevel 1 (
    echo ERROR: Clean failed.
    exit /b 1
)
echo Clean completed.
echo.

REM Build solution
echo [3/4] Building solution...
"%MSBUILD%" WinEcoSensor.sln /t:Build /p:Configuration=%CONFIGURATION% /p:Platform="Any CPU" /verbosity:minimal
if errorlevel 1 (
    echo ERROR: Build failed.
    exit /b 1
)
echo Build completed successfully.
echo.

REM Build installer if requested
if %BUILD_INSTALLER%==1 (
    echo [4/4] Building installer...
    
    REM Find Inno Setup
    set ISCC=
    if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" (
        set ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe
    )
    if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" (
        set ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe
    )
    
    if "!ISCC!"=="" (
        echo WARNING: Inno Setup not found. Skipping installer creation.
        echo Please install Inno Setup 6 from https://jrsoftware.org/isinfo.php
    ) else (
        echo Found Inno Setup: !ISCC!
        "!ISCC!" Installer\WinEcoSensor.iss
        if errorlevel 1 (
            echo ERROR: Installer creation failed.
            exit /b 1
        )
        echo Installer created: Installer\Output\WinEcoSensor-Setup-1.0.0.exe
    )
) else (
    echo [4/4] Skipping installer (use 'build.bat installer' to create installer)
)

echo.
echo ============================================
echo  Build completed successfully!
echo ============================================
echo.
echo Output locations:
echo   Service:  WinEcoSensor.Service\bin\%CONFIGURATION%\
echo   TrayApp:  WinEcoSensor.TrayApp\bin\%CONFIGURATION%\
echo   Common:   WinEcoSensor.Common\bin\%CONFIGURATION%\
if %BUILD_INSTALLER%==1 (
    echo   Installer: Installer\Output\
)
echo.

exit /b 0
