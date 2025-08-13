# Suggested Commands

## Primary User Commands

### Main Unity FBX Fix (Most Important)
```bash
# Navigate to tools directory and fix FBX for Unity
cd fbx-weight-transfer-unity/tools/
./fix_fbx_for_unity.sh input.fbx output_unity_ready.fbx
```

### Weight Transfer
```bash
# Using organized scripts
./organized/bin/transfer_weights.sh input.fbx output.fbx [config_file]

# Examples
./organized/bin/transfer_weights.sh character.fbx character_rigged.fbx
./organized/bin/transfer_weights.sh model.fbx output.fbx custom.conf
```

### Direct Blender Script Execution
```bash
# Unity weight transfer script
tools/blender --background --python unity-scripts/blender_weight_transfer_for_unity.py -- input.fbx output.fbx

# IK bone deletion scripts
blender --background --python blender-workspace/scripts/process_sun_fbx.py
blender --background --python blender-workspace/scripts/delete_non_root_bones.py
```

## Testing and Verification
```bash
# Test Blender installation
tools/blender --version

# Check processing logs
ls -la project-files/process-logs/
cat project-files/process-logs/unity_weight_transfer_*.log
```

## Development Commands
```bash
# File operations
ls -la
find . -name "*.py"
find . -name "*.fbx"

# Git operations  
git status
git add .
git commit -m "message"

# Directory navigation
cd fbx-weight-transfer-unity/
cd blender-workspace/
cd organized/
```

## Batch Processing Example
```bash
# Process multiple FBX files
for fbx in project-files/input-fbx/*.fbx; do
    name=$(basename "$fbx" .fbx)
    ./tools/fix_fbx_for_unity.sh "$fbx" "project-files/output-fbx/${name}_unity_ready.fbx"
done
```