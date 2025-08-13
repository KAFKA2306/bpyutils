# Project Overview: bpyutils2

## Purpose
This is a **Unity FBX Weight Transfer Toolchain** built on Blender 4.0.2 that solves Unity import problems where clothing meshes have no skinning weights and appear frozen during character animation.

## Main Functions
1. **Delete IK Bones**: Remove needless IK bones in Blender FBX files for Marvelous Designer compatibility
2. **Weight Transfer**: Transfer vertex weights from body armature to Marvelous Designer FBX garment files
3. **Unity VRChat PhysBones**: Headless Unity automation for VRChat PhysBones installation
4. **Material Conversion**: Auto-fix transparency issues when importing Marvelous Designer FBX to Unity (converts Standard Shader issues)

## Tech Stack
- **Primary**: Python + Blender 4.0.2 (included)
- **Automation**: Bash shell scripts
- **Target Platform**: Unity 2020.3+ with focus on VRChat development
- **System**: Linux/WSL2
- **Languages**: Python, Bash, C# (Unity scripts)

## Key Directories
- `fbx-weight-transfer-unity/` - Main Unity weight transfer toolchain
- `blender-workspace/` - Blender scripts and workspace
- `headlessunityVRChatPhysBonesSemiAutoInstaller/` - Unity VRChat automation
- `MarvelousDesignerFBXtoUnityLiltoonMaterial/` - Material conversion tools
- `organized/` - Organized scripts and binaries

## Primary Entry Points
- `fbx-weight-transfer-unity/tools/fix_fbx_for_unity.sh` - Main Unity fix script
- `organized/bin/transfer_weights.sh` - Batch weight transfer script
- Individual Python scripts for specific Blender operations