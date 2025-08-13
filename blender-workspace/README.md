# Blender Bone Processing Workspace

Clean workspace for Blender bone manipulation tasks.

## Directory Structure

```
blender-workspace/
├── models/           # Input FBX/Blend files
│   └── SUN_v01.fbx  # Original model
├── scripts/          # Python scripts for bone processing
│   ├── ik_bone_manager.py      # Core IK bone functions
│   ├── process_sun_fbx.py      # Analysis script
│   └── delete_non_root_bones.py # Bone deletion script
├── output/           # Processed output files
│   └── SUN_v01_cleaned.blend   # Cleaned model (IK bones removed)
└── tools/            # Blender executable
    └── blender -> symlink to Blender
```

## Quick Commands

```bash
# Run bone analysis
./tools/blender --background --python scripts/process_sun_fbx.py

# Process and clean bones
./tools/blender --background --python scripts/delete_non_root_bones.py

# Launch Blender GUI with model
./tools/blender models/SUN_v01.fbx
```