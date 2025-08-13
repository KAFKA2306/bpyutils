# FBX Material Converter Usage Guide

## Overview

This Unity C# script package automatically converts Standard Shader materials in FBX files to lilToon shaders, restoring transparency settings that are lost during Unity import from Marvelous Designer.

## Features

✅ **Automatic FBX Scanning** - Finds all FBX files in target directory  
✅ **Material Conversion** - Standard Shader → lilToon Shader  
✅ **Transparency Restoration** - Auto-detects and applies alpha settings  
✅ **Backup System** - Creates backups before conversion  
✅ **Batch Processing** - Headless Unity support for automation  
✅ **Detailed Logging** - Comprehensive error handling and reporting  
✅ **Progress Tracking** - Real-time conversion progress  

## Requirements

### Prerequisites
- **Unity 2022.3 LTS or higher**
- **lilToon shader** imported in your project
- **FBX files** with Standard Shader materials from Marvelous Designer

### Installation
1. Copy all scripts to your Unity project:
   - `Scripts/Runtime/*.cs` → `Assets/Scripts/Runtime/`
   - `Scripts/Editor/*.cs` → `Assets/Scripts/Editor/`
2. Place `Config/ConversionConfig.json` in your project root or Assets folder
3. Import lilToon shader into your Unity project

## Usage Methods

### Method 1: Unity Editor GUI (Recommended for beginners)

1. Open Unity Editor
2. Go to menu: **Tools → Convert FBX Materials to lilToon**
3. Configure settings in the window:
   - **Target Folder**: Directory containing FBX files (default: `Assets/Models/`)
   - **Enable Transparency**: Auto-detect and apply transparency
   - **Create Backups**: Create material backups before conversion
4. Click **"Convert All FBX Materials"**
5. Monitor progress in the window

### Method 2: Headless Unity Batch (Recommended for automation)

#### Windows:
```cmd
# Edit convert_fbx_materials.bat to set your Unity path
convert_fbx_materials.bat
```

#### Linux/Mac:
```bash
# Make executable and run
chmod +x convert_fbx_materials.sh
./convert_fbx_materials.sh
```

#### Manual Unity Command:
```bash
Unity.exe -batchmode -quit -projectPath "YourProjectPath" \
-executeMethod FBXMaterialConverter.FBXMaterialConverterBatch.ConvertAllFBXMaterials \
-logFile "conversion.log"
```

### Method 3: Unity Editor Menu (Quick batch)

1. Go to menu: **Tools → Convert FBX Materials to lilToon (Batch)**
2. Confirm the dialog
3. Wait for completion message

## Configuration

### ConversionConfig.json Settings

```json
{
  "targetFolder": "Assets/Models/",           // FBX search directory
  "enableTransparency": true,                 // Auto-apply transparency
  "createBackups": true,                      // Create material backups
  "targetShader": "lilToon",                  // Target shader name
  "alphaThreshold": 0.5,                      // Alpha detection threshold
  "logLevel": "Info",                         // Logging detail level
  "transparencyMode": 2,                      // lilToon transparency mode
  "renderQueue": 3000,                        // Render queue for transparent materials
  "autoDetectTransparency": true,             // Auto-detect alpha channels
  "maxConcurrentProcesses": 4                 // Parallel processing limit
}
```

### Log Levels
- `Error`: Only critical errors
- `Warning`: Warnings and errors  
- `Info`: General information (recommended)
- `Debug`: Detailed debugging information

## Property Mapping

The converter automatically maps Standard Shader properties to lilToon equivalents:

| Standard Shader | lilToon | Notes |
|----------------|---------|--------|
| `_MainTex` | `_MainTex` | Main texture (Albedo) |
| `_BumpMap` | `_BumpMap` | Normal map |
| `_Color` | `_Color` | Main color tint |
| `_Metallic` | `_Metallic` | Metallic value |
| `_Glossiness` | `_Smoothness` | Smoothness/roughness |
| `_EmissionMap` | `_EmissionMap` | Emission texture |
| `_EmissionColor` | `_EmissionColor` | Emission color |

## Transparency Detection

The system automatically detects transparency using multiple methods:

1. **Alpha Channel Detection**: Checks if main texture has alpha channel
2. **Material Color Alpha**: Examines material color alpha value
3. **Rendering Mode**: Checks if material uses transparent rendering mode

When transparency is detected, the converter:
- Sets lilToon transparency mode to `2` (Normal)
- Adjusts render queue to `3000` (Transparent)
- Configures proper blend modes (SrcAlpha, OneMinusSrcAlpha)
- Enables `_ALPHABLEND_ON` keyword

## Backup System

### Automatic Backups
- Created before each material conversion
- Stored in `Backups/Materials/` directory
- Include timestamp in filename
- Tracked in backup manifest JSON

### Restore Operations
```csharp
// Restore single material
backupSystem.RestoreMaterial("MaterialName");

// Restore all materials
backupSystem.RestoreAllMaterials();

// Cleanup old backups (30+ days)
backupSystem.CleanupOldBackups(30);
```

## Output and Reporting

### Console Output
- Real-time progress updates
- Error messages with details
- Final statistics summary

### Log Files
- Detailed logs in `Logs/` directory
- Timestamped log files
- JSON statistics files
- CSV export capability

### HTML Reports
- Visual conversion reports
- Error analysis
- Processing statistics
- System information

## Error Handling

### Common Issues

**"lilToon shader not found"**
- Solution: Import lilToon shader package into Unity project
- Verify shader name in config matches exactly

**"No FBX files found"**
- Solution: Check `targetFolder` path in config
- Ensure FBX files exist in specified directory

**"Material backup failed"**
- Solution: Check write permissions for backup directory
- Ensure sufficient disk space

**"Conversion failed"**
- Solution: Check Unity console for detailed error messages
- Verify FBX file integrity
- Check material references

### Recovery
- Use backup system to restore original materials
- Check detailed logs for error analysis
- Re-run conversion with Debug log level for more information

## Best Practices

### Before Conversion
1. **Backup your project** - Always backup entire project before batch conversion
2. **Test on sample files** - Try conversion on a few FBX files first
3. **Verify lilToon installation** - Ensure lilToon shader works in your project
4. **Check FBX integrity** - Ensure FBX files import correctly in Unity

### During Conversion
1. **Monitor progress** - Watch for errors during processing
2. **Don't interrupt** - Let batch process complete fully
3. **Check memory usage** - Large projects may need more RAM

### After Conversion
1. **Verify results** - Check converted materials in Scene view
2. **Test animations** - Ensure transparency works with animations
3. **Performance check** - Monitor rendering performance
4. **Save project** - Save Unity project after successful conversion

## Troubleshooting

### Performance Issues
- Reduce `maxConcurrentProcesses` in config
- Process smaller batches of files
- Close other applications during conversion

### Memory Issues
- Increase Unity memory allocation
- Process files in smaller batches
- Use Debug log level only when necessary

### Integration Issues
- Ensure Unity Editor version compatibility
- Check script compilation errors
- Verify namespace usage in existing code

## Advanced Usage

### Custom Property Mapping
Modify `MaterialConverter.cs` to add custom property mappings:

```csharp
propertyMapping.Add("_CustomProperty", "_lilToonCustom");
```

### Custom Transparency Logic
Override transparency detection in `MaterialConverter.DetectTransparency()`:

```csharp
private bool DetectTransparency(Material material)
{
    // Custom transparency detection logic
    return customCondition;
}
```

### Integration with CI/CD
Use headless batch mode in automated pipelines:

```bash
# In CI/CD script
Unity -batchmode -quit -projectPath "$PROJECT_PATH" \
-executeMethod FBXMaterialConverter.FBXMaterialConverterBatch.ConvertAllFBXMaterials \
-logFile "conversion.log"

# Check exit code
if [ $? -eq 0 ]; then
    echo "Conversion successful"
else
    echo "Conversion failed"
    exit 1
fi
```

## API Reference

### Main Classes
- `ConversionConfig`: Configuration management
- `FBXScanner`: FBX file scanning and material extraction
- `MaterialConverter`: Core conversion logic
- `ConversionLogger`: Logging and statistics
- `MaterialBackupSystem`: Backup and restore functionality
- `ConversionReporter`: Detailed reporting and output

### Editor Extensions
- `FBXMaterialConverterEditor`: Unity Editor GUI window
- `FBXMaterialConverterBatch`: Headless batch processing

For detailed API documentation, see the inline code comments in each script file.