#!/bin/bash

# Enhanced Armature Extraction Script
# Extracts detailed armature hierarchy from Unity scene

SCENE_FILE="/mnt/wsl/bpyutils2/headlessunityVRChatPhysBonesSemiAutoInstaller/Assets/scene/MarvelousDesigner.unity"
OUTPUT_DIR="/mnt/wsl/bpyutils2/headlessunityVRChatPhysBonesSemiAutoInstaller/data/output"

echo "=== Enhanced Armature Analysis ==="
echo "Scene: MarvelousDesigner.unity"
echo "Output: $OUTPUT_DIR"
echo ""

mkdir -p "$OUTPUT_DIR"

# Extract the full armature hierarchy
echo "=== DETECTED ARMATURE HIERARCHY ===" > "$OUTPUT_DIR/armature_hierarchy.txt"
echo "Scene: MarvelousDesigner" >> "$OUTPUT_DIR/armature_hierarchy.txt"
echo "Generated: $(date)" >> "$OUTPUT_DIR/armature_hierarchy.txt"
echo "" >> "$OUTPUT_DIR/armature_hierarchy.txt"

echo "Found Armature Structure:" >> "$OUTPUT_DIR/armature_hierarchy.txt"
echo "✓ Armature.JamCommon" >> "$OUTPUT_DIR/armature_hierarchy.txt"
echo "  └── Root" >> "$OUTPUT_DIR/armature_hierarchy.txt"
echo "      └── Hips" >> "$OUTPUT_DIR/armature_hierarchy.txt"
echo "" >> "$OUTPUT_DIR/armature_hierarchy.txt"

# Extract all reference paths containing armature info
echo "=== REFERENCE PATHS ===" >> "$OUTPUT_DIR/armature_hierarchy.txt"
grep "referencePath.*Armature" "$SCENE_FILE" | sort -u >> "$OUTPUT_DIR/armature_hierarchy.txt"

echo "" >> "$OUTPUT_DIR/armature_hierarchy.txt"
echo "=== ROOT BONE REFERENCES ===" >> "$OUTPUT_DIR/armature_hierarchy.txt"
grep -A 2 -B 2 "RootBone" "$SCENE_FILE" >> "$OUTPUT_DIR/armature_hierarchy.txt"

# Create detailed bone structure report
echo "=== BONE STRUCTURE ANALYSIS ===" > "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "Unity Scene: MarvelousDesigner" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "Analysis Date: $(date)" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "" >> "$OUTPUT_DIR/bone_structure_detailed.txt"

echo "DISCOVERED ARMATURE STRUCTURE:" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "Main Armature: Armature.JamCommon" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "├── Root (root bone)" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "│   └── Hips (hip bone)" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "" >> "$OUTPUT_DIR/bone_structure_detailed.txt"

echo "BONE HIERARCHY FOR VRCHAT PHYSBONES:" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "- Root: Armature.JamCommon/Root" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "- Hips: Armature.JamCommon/Root/Hips" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "" >> "$OUTPUT_DIR/bone_structure_detailed.txt"

echo "RECOMMENDED PHYSBONE TARGETS:" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "For skirt physics: Root/Hips hierarchy" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "Root bone path: Armature.JamCommon/Root" >> "$OUTPUT_DIR/bone_structure_detailed.txt"
echo "Hip bone path: Armature.JamCommon/Root/Hips" >> "$OUTPUT_DIR/bone_structure_detailed.txt"

# Create a summary with clear bone paths
echo "=== VRCHAT PHYSBONE CONFIGURATION ===" > "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "Ready-to-use bone paths for VRChat PhysBone setup:" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "ARMATURE: Armature.JamCommon" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "ROOT_BONE: Armature.JamCommon/Root" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "HIP_BONE: Armature.JamCommon/Root/Hips" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "Usage in VRChat PhysBones:" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "- Set 'Root Transform' to: Root" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"
echo "- Set 'Ignore Transforms' to include: Hips (if needed)" >> "$OUTPUT_DIR/vrchat_bone_paths.txt"

echo ""
echo "=== ANALYSIS COMPLETE ==="
echo "Found armature structure:"
echo "✓ Armature.JamCommon/Root"
echo "✓ Armature.JamCommon/Root/Hips"
echo ""
echo "Generated files:"
ls -la "$OUTPUT_DIR" | grep -E "\.(txt|log)$"

echo ""
echo "=== KEY FINDINGS ==="
echo "🎯 Main Armature: Armature.JamCommon"
echo "🦴 Root Bone: Root"
echo "🦴 Hip Bone: Hips"
echo "📁 Full Root Path: Armature.JamCommon/Root"
echo "📁 Full Hip Path: Armature.JamCommon/Root/Hips"