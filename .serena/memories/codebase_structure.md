# Codebase Structure

## Top-Level Directory Layout
```
bpyutils2/
├── fbx-weight-transfer-unity/          # Main Unity toolchain
│   ├── tools/fix_fbx_for_unity.sh     # ⭐ PRIMARY ENTRY POINT
│   ├── unity-scripts/                  # Unity-optimized Python scripts
│   ├── tutorial/                       # User guides
│   ├── help/                          # Technical documentation
│   └── project-files/                 # User workspace
├── blender-workspace/                  # Blender scripts and data
│   ├── scripts/                       # Blender Python scripts
│   ├── models/                        # Input models
│   ├── output/                        # Processed output
│   └── data/                          # Processing data
├── organized/                          # Organized tools
│   ├── bin/transfer_weights.sh        # Weight transfer script
│   └── scripts/                       # General scripts
├── headlessunityVRChatPhysBonesSemiAutoInstaller/ # Unity VRChat automation
├── MarvelousDesignerFBXtoUnityLiltoonMaterial/     # Material conversion
└── CLAUDE.md                          # Project instructions
```

## Key Python Scripts
- `fbx-weight-transfer-unity/unity-scripts/blender_weight_transfer_for_unity.py` - Main Unity weight transfer
- `blender-workspace/scripts/ik_bone_manager.py` - IK bone management
- `blender-workspace/scripts/delete_non_root_bones.py` - Bone cleanup
- `blender-workspace/scripts/process_sun_fbx.py` - FBX processing
- `organized/scripts/fbx_weight_transfer.py` - General weight transfer

## Configuration Approach
- Shell scripts support external config files (`.conf` format)
- Python scripts use hardcoded dictionaries (e.g., `UNITY_FBX_SETTINGS`)
- Default values with override capability
- Configuration files are optional with sensible defaults

## Data Flow
1. **Input**: FBX files (usually from Marvelous Designer)
2. **Processing**: Blender Python scripts for weight transfer/bone cleanup
3. **Output**: Unity-ready FBX files with proper skinning
4. **Integration**: Import into Unity with preserved animation

## Dependencies
- **Blender 4.0.2**: Included in the package (symlinked as `tools/blender`)
- **System**: Linux/WSL2 environment
- **Unity**: Target platform (2020.3+)
- **No Python packages**: Uses only Blender's built-in Python and standard library