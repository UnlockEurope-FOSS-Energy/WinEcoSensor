# WinEcoSensor Resources

This folder contains resource files for the WinEcoSensor project.

## Icon Generation

The `WinEcoSensor-icon.svg` file contains the vector source for the application icon.

### Converting SVG to ICO

To create the Windows icon file (`WinEcoSensor.ico`), use one of these methods:

#### Method 1: ImageMagick (Command Line)

```bash
# Install ImageMagick if not present
# Windows: choco install imagemagick
# Linux: apt-get install imagemagick

# Convert SVG to ICO with multiple sizes
convert -background none WinEcoSensor-icon.svg -define icon:auto-resize=256,128,64,48,32,16 WinEcoSensor.ico
```

#### Method 2: Online Converter

1. Go to [convertio.co/svg-ico/](https://convertio.co/svg-ico/) or similar service
2. Upload `WinEcoSensor-icon.svg`
3. Download the resulting ICO file
4. Rename to `WinEcoSensor.ico`

#### Method 3: GIMP

1. Open GIMP
2. File > Open > Select `WinEcoSensor-icon.svg`
3. Choose import size (256x256 recommended)
4. File > Export As > `WinEcoSensor.ico`
5. Select sizes to include (256, 128, 64, 48, 32, 16)

### Icon Sizes

Windows icons should include these sizes for best appearance:
- 256x256 (Windows Vista+ high-res)
- 128x128 (Large icons)
- 64x64 (Extra large icons)
- 48x48 (Large list view)
- 32x32 (Normal list view)
- 16x16 (Small icons, status bar)

## Icon Design

The icon represents:
- **Green leaf**: Eco-friendly energy monitoring
- **Lightning bolt**: Energy/power measurement
- **Forest green colors**: Environmental awareness
- **Gold accent**: Energy highlighting

## Files

| File | Description |
|------|-------------|
| `WinEcoSensor-icon.svg` | Vector source file |
| `WinEcoSensor.ico` | Windows icon (generate from SVG) |

## Usage

The icon is used in:
- Application executable (TrayApp)
- System tray
- Installer
- Start Menu shortcuts
- Desktop shortcut

## License

All resources in this folder are licensed under EUPL-1.2.
