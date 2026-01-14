# WinEcoSensor Icon Generator
# SPDX-License-Identifier: EUPL-1.2
# 
# This script generates a Windows ICO file from the SVG source using ImageMagick.
# 
# Prerequisites:
#   - ImageMagick with SVG support (choco install imagemagick)
#
# Usage:
#   .\generate-icon.ps1

$ErrorActionPreference = "Stop"

Write-Host "WinEcoSensor Icon Generator" -ForegroundColor Cyan
Write-Host "==========================" -ForegroundColor Cyan
Write-Host ""

# Paths
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ResourcesDir = Join-Path $ScriptDir "Resources"
$SvgPath = Join-Path $ResourcesDir "WinEcoSensor-icon.svg"
$IcoPath = Join-Path $ResourcesDir "WinEcoSensor.ico"

# Check for ImageMagick
$magick = Get-Command "magick" -ErrorAction SilentlyContinue
if (-not $magick) {
    $magick = Get-Command "convert" -ErrorAction SilentlyContinue
    if (-not $magick) {
        Write-Host "ERROR: ImageMagick not found." -ForegroundColor Red
        Write-Host ""
        Write-Host "Please install ImageMagick:" -ForegroundColor Yellow
        Write-Host "  choco install imagemagick" -ForegroundColor Gray
        Write-Host "  # or download from: https://imagemagick.org/script/download.php" -ForegroundColor Gray
        exit 1
    }
}

Write-Host "Found ImageMagick: $($magick.Source)" -ForegroundColor Green

# Check source file
if (-not (Test-Path $SvgPath)) {
    Write-Host "ERROR: SVG source not found: $SvgPath" -ForegroundColor Red
    exit 1
}

Write-Host "Source: $SvgPath" -ForegroundColor Gray
Write-Host "Output: $IcoPath" -ForegroundColor Gray
Write-Host ""

# Generate ICO with multiple sizes
Write-Host "Generating ICO file with sizes: 256, 128, 64, 48, 32, 16..." -ForegroundColor Yellow

try {
    # Use magick command (ImageMagick 7+) or convert (older versions)
    $cmd = if ($magick.Name -eq "magick") { "magick" } else { "convert" }
    
    & $cmd -background none $SvgPath -define icon:auto-resize=256,128,64,48,32,16 $IcoPath
    
    if ($LASTEXITCODE -ne 0) {
        throw "ImageMagick returned exit code $LASTEXITCODE"
    }
    
    Write-Host ""
    Write-Host "SUCCESS: Icon generated at $IcoPath" -ForegroundColor Green
    
    # Show file info
    $fileInfo = Get-Item $IcoPath
    Write-Host "  Size: $([math]::Round($fileInfo.Length / 1KB, 2)) KB" -ForegroundColor Gray
    
} catch {
    Write-Host "ERROR: Failed to generate icon: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Cyan
