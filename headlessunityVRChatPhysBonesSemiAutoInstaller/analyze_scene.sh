#!/bin/bash

# Direct Unity Scene Analysis Script
# Analyzes Unity scene file without requiring Unity licensing

SCENE_FILE="/mnt/wsl/bpyutils2/headlessunityVRChatPhysBonesSemiAutoInstaller/Assets/scene/MarvelousDesigner.unity"
OUTPUT_DIR="/mnt/wsl/bpyutils2/headlessunityVRChatPhysBonesSemiAutoInstaller/data/output"

echo "=== Unity Scene Direct Analysis ==="
echo "Scene: $SCENE_FILE"
echo "Output: $OUTPUT_DIR"
echo ""

# Create output directory
mkdir -p "$OUTPUT_DIR"

# Check if scene file exists
if [ ! -f "$SCENE_FILE" ]; then
    echo "Error: Scene file not found at $SCENE_FILE"
    exit 1
fi

echo "Analyzing scene file directly..."

# Extract GameObjects and their hierarchy
echo "=== Scene Object Analysis ===" > "$OUTPUT_DIR/hierarchy_dump.txt"
echo "Scene: MarvelousDesigner" >> "$OUTPUT_DIR/hierarchy_dump.txt"
echo "Generated: $(date)" >> "$OUTPUT_DIR/hierarchy_dump.txt"
echo "" >> "$OUTPUT_DIR/hierarchy_dump.txt"

# Find GameObject entries in the Unity scene file
echo "Found GameObjects:" >> "$OUTPUT_DIR/hierarchy_dump.txt"
grep -n "GameObject:" "$SCENE_FILE" | head -20 >> "$OUTPUT_DIR/hierarchy_dump.txt"

echo "" >> "$OUTPUT_DIR/hierarchy_dump.txt"
echo "Transform components:" >> "$OUTPUT_DIR/hierarchy_dump.txt"
grep -n "Transform:" "$SCENE_FILE" | head -20 >> "$OUTPUT_DIR/hierarchy_dump.txt"

# Look for potential armature/bone structures
echo "=== Armature Structure Analysis ===" > "$OUTPUT_DIR/armature_bones.txt"
echo "Scene: MarvelousDesigner" >> "$OUTPUT_DIR/armature_bones.txt"
echo "Generated: $(date)" >> "$OUTPUT_DIR/armature_bones.txt"
echo "" >> "$OUTPUT_DIR/armature_bones.txt"

echo "Searching for bone-related objects..." >> "$OUTPUT_DIR/armature_bones.txt"

# Search for common bone/armature names
bone_keywords="hip spine chest neck head shoulder arm leg thigh knee foot hand finger thumb toe bone joint armature skeleton rig"

for keyword in $bone_keywords; do
    echo "" >> "$OUTPUT_DIR/armature_bones.txt"
    echo "=== Objects containing '$keyword' ===" >> "$OUTPUT_DIR/armature_bones.txt"
    grep -i "$keyword" "$SCENE_FILE" | head -10 >> "$OUTPUT_DIR/armature_bones.txt"
done

# Look for file IDs and names
echo "" >> "$OUTPUT_DIR/armature_bones.txt"
echo "=== Object Names ===" >> "$OUTPUT_DIR/armature_bones.txt"
grep -E "m_Name:" "$SCENE_FILE" | head -30 >> "$OUTPUT_DIR/armature_bones.txt"

# Create a summary
echo "=== Analysis Summary ===" > "$OUTPUT_DIR/analysis_summary.txt"
echo "Scene File: $SCENE_FILE" >> "$OUTPUT_DIR/analysis_summary.txt"
echo "File Size: $(ls -lh "$SCENE_FILE" | awk '{print $5}')" >> "$OUTPUT_DIR/analysis_summary.txt"
echo "Analysis Time: $(date)" >> "$OUTPUT_DIR/analysis_summary.txt"
echo "" >> "$OUTPUT_DIR/analysis_summary.txt"

gameobject_count=$(grep -c "GameObject:" "$SCENE_FILE")
transform_count=$(grep -c "Transform:" "$SCENE_FILE")
name_count=$(grep -c "m_Name:" "$SCENE_FILE")

echo "GameObjects found: $gameobject_count" >> "$OUTPUT_DIR/analysis_summary.txt"
echo "Transforms found: $transform_count" >> "$OUTPUT_DIR/analysis_summary.txt"
echo "Named objects: $name_count" >> "$OUTPUT_DIR/analysis_summary.txt"

echo ""
echo "=== Analysis Complete ==="
echo "Generated files:"
ls -la "$OUTPUT_DIR"

echo ""
echo "=== Quick Preview ==="
echo "Object names (first 10):"
grep -E "m_Name:" "$SCENE_FILE" | head -10 | sed 's/.*m_Name: //'