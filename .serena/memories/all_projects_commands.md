# All Projects Command Reference

## Project 1: IK Bone Deletion
```bash
# Navigate to workspace
cd blender-workspace/

# Analyze FBX bone structure
./tools/blender --background --python scripts/process_sun_fbx.py

# Delete non-root IK bones
./tools/blender --background --python scripts/delete_non_root_bones.py

# Interactive mode with model
./tools/blender models/SUN_v01.fbx
```

## Project 2: Weight Transfer (PRIMARY)
```bash
# Main Unity fix command (MOST IMPORTANT)
cd fbx-weight-transfer-unity/tools/
./fix_fbx_for_unity.sh input.fbx output_unity_ready.fbx

# Alternative batch processing
./organized/bin/transfer_weights.sh input.fbx output.fbx [config_file]

# Direct Blender script execution
tools/blender --background --python unity-scripts/blender_weight_transfer_for_unity.py -- input.fbx output.fbx

# Batch process multiple files
for fbx in project-files/input-fbx/*.fbx; do
    name=$(basename "$fbx" .fbx)
    ./tools/fix_fbx_for_unity.sh "$fbx" "project-files/output-fbx/${name}_unity_ready.fbx"
done
```

## Project 3: VRChat PhysBones (Unity Headless)
```bash
# Full PhysBones setup
Unity.exe -batchmode -nographics -projectPath "/path/to/unity/project" \
-executeMethod SkirtPBHeadless.ProcessSkirtBones \
-skirtRoot "Hips" -angle 45 -innerAngle 10 \
-boneRegex "Hips\\.\\d+" -hierarchyOut "hierarchy.txt"

# Just hierarchy dump (no processing)
Unity.exe -batchmode -nographics -projectPath "/path/to/unity/project" \
-executeMethod SkirtPBHeadless.DumpHierarchy \
-hierarchyOut "hierarchy_dump.txt"

# Parameters:
# -skirtRoot: Target bone name (required for processing)
# -angle: Max angle (default 45)
# -innerAngle: Inner angle limit (default 10) 
# -boneRegex: Bone matching pattern (default \.\\d+)
# -hierarchyOut: Output file for hierarchy (default hierarchy_dump.txt)
```

## Project 4: Material Transparency Fix (Unity Headless)
```bash
# Convert all FBX materials to lilToon
Unity.exe -batchmode -quit -projectPath "/path/to/unity/project" \
-executeMethod FBXMaterialConverter.ConvertAllFBXMaterials \
-logFile "material_conversion.log"

# Unity Editor menu (when in Unity GUI)
# Menu: Tools → Convert FBX Materials to lilToon
```

## Testing Commands
```bash
# Test Blender installation
tools/blender --version

# View processing logs
ls -la project-files/process-logs/
cat project-files/process-logs/unity_weight_transfer_*.log

# Check Unity project structure
ls -la headlessunityVRChatPhysBonesSemiAutoInstaller/Assets/Editor/

# Verify material conversion results
ls -la MarvelousDesignerFBXtoUnityLiltoonMaterial/Logs/
```

## Complete Workflow (All 4 Projects)
```bash
# 1. Clean IK bones from Marvelous Designer export
cd blender-workspace/
./tools/blender --background --python scripts/delete_non_root_bones.py

# 2. Transfer weights for Unity
cd ../fbx-weight-transfer-unity/tools/
./fix_fbx_for_unity.sh cleaned_character.fbx character_with_weights.fbx

# 3. Fix materials in Unity (run in Unity project)
Unity.exe -batchmode -quit -projectPath "/unity/project" \
-executeMethod FBXMaterialConverter.ConvertAllFBXMaterials

# 4. Setup VRChat PhysBones (run in VRChat Unity project)  
Unity.exe -batchmode -nographics -projectPath "/vrchat/project" \
-executeMethod SkirtPBHeadless.ProcessSkirtBones -skirtRoot "Hips"
```