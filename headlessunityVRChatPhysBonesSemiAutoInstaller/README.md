# VRChat PhysBone Headless Unity Installer

Automated Unity headless CLI tool for VRChat PhysBone configuration and armature analysis.

## Overview

This tool provides automated VRChat PhysBone setup and comprehensive armature structure detection through Unity's headless mode (`-batchmode -nographics`). It analyzes Unity scenes to detect bone hierarchies (root, hips, spine, etc.) and outputs detailed armature information for VRChat configuration.

## Environment

* Unity 2022.3.22f1 LTS (Linux/WSL2 tested)
* Unity Hub with active license  
* Headless execution: `-batchmode -nographics`
* VRChat SDK3 (optional - fallback analysis available)
* .NET Standard 2.1 (Unity runtime)

## Quick Start

```bash
# Main script - analyzes Unity scene and detects armature
./run_headless_test.sh

# Enhanced armature extraction (fallback method)
./analyze_scene.sh
./extract_armature.sh
```

## Input/Output

### Input
- Unity scene: `Assets/scene/MarvelousDesigner.unity`
- Command line arguments:
  - `-skirtRoot <path>` (optional - for PhysBone config)
  - `-angle <degrees>` (default: 45)
  - `-innerAngle <degrees>` (default: 10)
  - `-boneRegex <pattern>` (default: `\.\d+`)
  - `-hierarchyOut <filename>` (default: `hierarchy_dump.txt`)
  - `-deepHierarchy true/false` (deep scene analysis)
  - `-findArmature true/false` (armature detection)
  - `-showBoneStructure true/false` (show component info)

### Output Directory: `data/output/`
- **`hierarchy_dump.txt`** - Complete scene hierarchy 
- **`unity_armature_bones.txt`** - Unity-detected bone structure
- **`vrchat_bone_paths.txt`** - Ready-to-use VRChat PhysBone paths
- **`bone_structure_detailed.txt`** - Human-readable bone analysis
- **`armature_hierarchy.txt`** - Armature structure summary
- **`unity.log`** - Unity execution log

### Exit Codes
- 0 = Success 
- 1 = No skirt root specified / licensing issues
- 2 = Specified node not found
- 3 = Missing PhysBone components  
- 4 = Other errors

## Project Structure & Code Paths

```
/mnt/wsl/bpyutils2/headlessunityVRChatPhysBonesSemiAutoInstaller/
├── run_headless_test.sh                    # Main Unity headless runner
├── analyze_scene.sh                        # Fallback scene analyzer 
├── extract_armature.sh                     # Enhanced armature extractor
├── installation.md                         # Unity Hub setup guide
├── README.md                               # This documentation
│
├── Assets/
│   ├── scene/
│   │   └── MarvelousDesigner.unity         # Target Unity scene
│   └── Editor/
│       ├── SimpleHierarchyDump.cs          # VRChat SDK-free Unity script
│       ├── SkirtPBHeadless.cs.bak          # Original VRChat SDK script (disabled)
│       └── (other editor scripts...)       
│
├── data/
│   └── output/                             # Analysis output directory
│       ├── hierarchy_dump.txt              # Complete scene structure
│       ├── unity_armature_bones.txt        # Unity-detected bones
│       ├── vrchat_bone_paths.txt           # VRChat configuration paths
│       ├── bone_structure_detailed.txt     # Human-readable analysis
│       ├── armature_hierarchy.txt          # Armature summary
│       └── unity.log                       # Unity execution log
│
└── Assets/Pielotopica/Editor/              # Additional editor tools
    ├── SkirtPBInstaller.cs.bak            # Original PhysBone installer (disabled)
    └── AnimationBaker.cs                   # Animation utilities
```

## Core Components

### Main Scripts
- **`run_headless_test.sh`** - Primary Unity headless execution script
  - Launches Unity 2022.3.22f1 in headless mode
  - Executes `SimpleHierarchyDump.Main()` method
  - Outputs results to `data/output/` directory
  - Runs enhanced analysis via `extract_armature.sh`

- **`Assets/Editor/SimpleHierarchyDump.cs`** - Unity C# script for scene analysis
  - VRChat SDK-independent implementation
  - Recursively analyzes scene hierarchy
  - Detects armature structures and bone hierarchies
  - Exports detailed bone structure to `unity_armature_bones.txt`

### Fallback Analysis Tools
- **`analyze_scene.sh`** - Direct Unity scene file parser
  - Parses `.unity` files without launching Unity
  - Extracts GameObject and Transform references
  - Creates basic armature structure analysis

- **`extract_armature.sh`** - Enhanced armature structure extractor
  - Generates VRChat-ready bone path configurations
  - Creates human-readable bone hierarchy documentation
  - Outputs ready-to-use PhysBone configuration paths

## Detected Armature Structure

**Character Model:** SUN_Yukata Variant Variant  
**Main Armature:** Armature.JamCommon

### Bone Hierarchy
```
Root
├── Hips
│   ├── HipRoot (skirt physics bones)
│   │   ├── HipRoot_L.001 
│   │   └── HipRoot_R.001
│   ├── Spine → Chest → UpperChest
│   │   ├── BustRoot (breast physics)
│   │   ├── Neck → Head
│   │   └── Shoulder_L/R → Arms → Hands → Fingers
│   └── UpperLeg_L/R → LowerLeg → Foot → Toes
```

### VRChat PhysBone Configuration Paths
- **Skirt Physics Root**: `HipRoot` 
- **Full Path**: `SUN_Yukata Variant Variant/Armature.JamCommon/Root/Hips/HipRoot`
- **Breast Physics**: `BustRoot`
- **Root Transform**: `Armature.JamCommon/Root`

## Functional Requirements

- **FR-1**: Scene hierarchy dump via `Scene.GetRootGameObjects()` DFS traversal
- **FR-2**: Optional skirt root specification for PhysBone configuration  
- **FR-3**: Bone enumeration with regex filtering of direct children
- **FR-4**: VRCPhysBone component validation (when VRChat SDK available)
- **FR-5**: PhysBone property configuration (limitType, angles, rotations)
- **FR-6**: Asset persistence via `AssetDatabase.SaveAssets()`

## Non-Functional Requirements

- **NFR-1**: Headless compatibility (GraphicsDeviceType.Null support)
- **NFR-2**: Minimal dependencies (VRChat SDK optional)  
- **NFR-3**: Self-contained implementation
- **NFR-4**: Comprehensive logging and error handling

## Usage Examples

```bash
# Basic armature analysis
./run_headless_test.sh

# PhysBone configuration for specific root
./run_headless_test.sh /home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity "HipRoot" ".*\.001" 45 10

# Fallback analysis without Unity
./analyze_scene.sh && ./extract_armature.sh
```
