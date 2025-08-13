# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is a **Unity FBX Weight Transfer Toolchain** built on Blender 4.0.2 that solves the common Unity import problem where clothing meshes have no skinning weights and appear frozen during character animation.

**Primary Purpose:** Fix Unity character import issues by transferring vertex weights from body meshes to clothing meshes automatically.

## Quick Start

The main tool Unity developers need:

```bash
# Navigate to tools directory
cd fbx-weight-transfer-unity/tools/

# Fix your character FBX for Unity (main command!)
./fix_fbx_for_unity.sh your_character.fbx your_character_unity_ready.fbx
```

## Key Directory Structure

```
fbx-weight-transfer-unity/
├── tools/                                    # Main user tools
│   ├── fix_fbx_for_unity.sh                 # ⭐ MAIN TOOL - Primary Unity fix script
│   └── blender                               # Blender 4.0.2 executable (symlinked)
├── unity-scripts/                            # Core implementation
│   ├── blender_weight_transfer_for_unity.py # Unity-optimized weight transfer
│   └── Kiseru/                               # Advanced weight transfer algorithms
├── project-files/                           # User workspace
│   ├── input-fbx/                           # Place your FBX files here
│   ├── output-fbx/                          # Fixed files appear here
│   └── process-logs/                        # Detailed processing logs
├── tutorial/                                # Step-by-step guides
├── help/                                    # Technical documentation
└── README.md                                # Complete Unity-focused guide
```

## Unity Problem This Solves

**Before (Broken):**
- Import character FBX into Unity ❌
- Body animates correctly, but clothing is frozen ❌  
- Only some meshes have skinning weights ❌

**After (Fixed):**
- Import the processed FBX into Unity ✅
- All meshes animate together perfectly ✅
- Complete character animation works ✅

## Core Scripts

### Primary User Script
- **`tools/fix_fbx_for_unity.sh`** - Main entry point for Unity developers
  - Colorized output and comprehensive help system
  - Handles file validation, path resolution, and error reporting
  - Wraps the Python processing with user-friendly interface

### Core Processing Script  
- **`unity-scripts/blender_weight_transfer_for_unity.py`** - Unity-optimized weight transfer
  - Implements surface-to-surface weight transfer using `POLYINTERP_NEAREST`
  - Unity-specific FBX export settings (Y-up, correct bone axes)
  - Detailed logging for troubleshooting Unity import issues
  - Batch processing of multiple clothing meshes

### Weight Transfer Algorithms
- **`unity-scripts/Kiseru/`** - Advanced weight transfer system
  - `WeightTransfer.py` - Core weight transfer algorithms
  - `VertexCleaner.py` - Vertex group management
  - `Kiseru.py` - Main addon interface

## Unity-Specific Features

### FBX Export Settings
```python
UNITY_FBX_SETTINGS = {
    'global_scale': 1.0,
    'primary_bone_axis': 'Y',     # Unity coordinate system
    'secondary_bone_axis': 'X',
    'add_leaf_bones': True,       # Unity requirement
    'armature_nodetype': 'NULL'   # Unity compatibility
}
```

### Processing Pipeline
1. **Import FBX** with Unity-compatible settings
2. **Find Body Mesh** - Automatically detect rigged mesh (source)
3. **Find Clothing** - Identify meshes needing weights (targets)  
4. **Transfer Weights** - Use surface sampling for accurate transfer
5. **Setup Unity Skinning** - Add armature modifiers and parent relationships
6. **Export for Unity** - Optimized FBX settings for Unity import

## Command Reference

### Main Usage (Recommended)
```bash
# Fix character FBX for Unity
./tools/fix_fbx_for_unity.sh input.fbx output.fbx

# Batch process multiple files
for fbx in project-files/input-fbx/*.fbx; do
    name=$(basename "$fbx" .fbx)
    ./tools/fix_fbx_for_unity.sh "$fbx" "project-files/output-fbx/${name}_unity_ready.fbx"
done
```

### Direct Blender Script Usage
```bash
# Advanced users - direct Python script
tools/blender --background --python unity-scripts/blender_weight_transfer_for_unity.py -- input.fbx output.fbx
```

### Testing and Verification
```bash
# Test Blender installation
tools/blender --version

# View processing logs
ls -la project-files/process-logs/
cat project-files/process-logs/unity_weight_transfer_*.log
```

## Technical Implementation

### Weight Transfer Method
- **Algorithm:** Surface interpolation with `POLYINTERP_NEAREST` 
- **Source Detection:** Automatically finds mesh with most vertex groups
- **Target Processing:** Transfers to all meshes with 0 vertex groups
- **Unity Validation:** Verifies armature modifiers and parent relationships

### Unity Compatibility
- **Coordinate System:** Y-up axis with X-forward (Unity standard)
- **Bone Structure:** Preserves bone hierarchy with leaf bones
- **Skinning Setup:** Proper armature modifiers and parenting
- **Export Format:** FBX 7.7 with Unity-optimized settings

## Typical Unity Workflow

```bash
# 1. Export character from Maya/Blender/etc → character.fbx
# 2. Fix for Unity using this tool
./tools/fix_fbx_for_unity.sh character.fbx character_unity_ready.fbx
# 3. Import character_unity_ready.fbx into Unity  
# 4. Set Animation Type to "Humanoid" or "Generic"
# 5. Apply animations - everything works! ✅
```

## Requirements

### Input FBX Requirements
- ✅ **Rigged Body Mesh** - At least one mesh with vertex weights 
- ✅ **Character Armature** - Bone structure for animation
- ✅ **Clothing Meshes** - Meshes that need weights transferred

### System Requirements  
- ✅ **Linux/WSL2** - Windows Subsystem for Linux recommended
- ✅ **Blender 4.0.2+** - Included in this package
- ✅ **Unity 2020.3+** - For importing the processed FBX

## Troubleshooting Common Unity Issues

### "No suitable source mesh found"
- **Cause:** Body mesh has no vertex weights
- **Fix:** Ensure body mesh is properly rigged before export

### "All weight transfers failed"  
- **Cause:** Clothing meshes too different from body topology
- **Fix:** Check mesh similarity, try with different clothing

### "Unity animation still broken"
- **Cause:** Wrong Unity import settings
- **Fix:** Set Animation Type to "Humanoid" or "Generic"

## Development Notes

This toolchain evolved from a simple Blender installation to solve a specific Unity development problem. The architecture prioritizes:

1. **User Experience** - Simple one-command solution for Unity developers
2. **Unity Optimization** - All processing tuned for Unity's requirements  
3. **Reliability** - Comprehensive error handling and logging
4. **Batch Processing** - Handle multiple characters efficiently

For Unity developers, focus on the `tools/fix_fbx_for_unity.sh` script - it handles all the complexity automatically.