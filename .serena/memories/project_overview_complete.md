# Complete Project Overview: bpyutils2

This repository contains **4 distinct projects** for Unity/VRChat character processing:

## Project 1: IK Bone Deletion (`blender-workspace/`)
**Purpose**: Delete needless IK bones in Blender FBX files to run Marvelous Designer FBX output successfully.

**Tech Stack**: Python + Blender 4.0.2
**Key Scripts**:
- `blender-workspace/scripts/ik_bone_manager.py` - Core IK bone management functions
- `blender-workspace/scripts/delete_non_root_bones.py` - Main bone deletion script  
- `blender-workspace/scripts/process_sun_fbx.py` - FBX analysis script

**Usage**:
```bash
cd blender-workspace/
./tools/blender --background --python scripts/delete_non_root_bones.py
```

## Project 2: Weight Transfer (`fbx-weight-transfer-unity/`)
**Purpose**: Transfer vertex weights from Blender Avatar Armature to Marvelous Designer FBX Garment files in a single FBX file.

**Tech Stack**: Python + Blender 4.0.2 + Unity-optimized export
**Key Scripts**:
- `fbx-weight-transfer-unity/tools/fix_fbx_for_unity.sh` - ⭐ **MAIN ENTRY POINT**
- `fbx-weight-transfer-unity/unity-scripts/blender_weight_transfer_for_unity.py` - Core processing
- `organized/bin/transfer_weights.sh` - Batch processing script

**Usage**:
```bash
cd fbx-weight-transfer-unity/tools/
./fix_fbx_for_unity.sh input.fbx output_unity_ready.fbx
```

## Project 3: VRChat PhysBones Installer (`headlessunityVRChatPhysBonesSemiAutoInstaller/`)
**Purpose**: GUI-less (`-batchmode -nographics`) Unity automation CLI tool for VRChat PhysBones installation.

**Tech Stack**: C# + Unity 2022.3 LTS + VRCSDK3-AVATAR
**Key Features**:
- Headless Unity automation for CI/CD
- Automatic skirt PhysBone setup
- Bone hierarchy analysis and configuration
- Command-line parameter support

**Usage**:
```bash
Unity.exe -batchmode -nographics -projectPath "ProjectPath" 
-executeMethod SkirtPBHeadless.ProcessSkirtBones 
-skirtRoot "Hips" -angle 45
```

## Project 4: Material Transparency Fix (`MarvelousDesignerFBXtoUnityLiltoonMaterial/`)
**Purpose**: Auto-fix transparency issues when importing Marvelous Designer FBX files to Unity, automatically converting Standard Shader to lilToon with proper transparency settings.

**Tech Stack**: C# + Unity 2022.3 LTS + lilToon Shader
**Key Features**:
- Automatic Standard → lilToon shader conversion
- Transparency detection and restoration
- Batch processing for multiple FBX files
- Backup system for materials
- JSON configuration support

**Usage**:
```bash
Unity.exe -batchmode -quit -projectPath "ProjectPath" 
-executeMethod FBXMaterialConverter.ConvertAllFBXMaterials 
-logFile "conversion.log"
```

## Unified Workflow
These 4 projects work together for complete Marvelous Designer → Unity character processing:

1. **IK Cleanup** → Remove problematic bones from MD export
2. **Weight Transfer** → Add proper skinning to clothing meshes  
3. **Material Fix** → Restore transparency in Unity materials
4. **PhysBones Setup** → Add VRChat physics to clothing/hair

## System Requirements
- **OS**: Linux/WSL2 (Blender projects) + Windows (Unity projects)
- **Blender**: 4.0.2+ (included in package)
- **Unity**: 2022.3 LTS+
- **Dependencies**: VRCSDK3-AVATAR, lilToon Shader