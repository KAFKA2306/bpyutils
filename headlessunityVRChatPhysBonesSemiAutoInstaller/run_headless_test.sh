#!/bin/bash

# Headless Unity VRCPhysBone CLI Runner
# Usage: ./run_headless_test.sh [unity_path] [skirt_root] [bone_regex] [angle] [inner_angle]

UNITY_PATH=${1:-"/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"}
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCENE_PATH="Assets/scene/MarvelousDesigner.unity"
SKIRT_ROOT=${2:-""}
BONE_REGEX=${3:-".*\\.\\d+"}
ANGLE=${4:-"45"}
INNER_ANGLE=${5:-"10"}

echo "=== Headless Unity VRCPhysBone CLI Runner ==="
echo "Unity Path: $UNITY_PATH"
echo "Project: $PROJECT_PATH"
echo "Scene: $SCENE_PATH"

if [ ! -f "$UNITY_PATH" ]; then
    echo "Error: Unity not found at $UNITY_PATH"
    echo "Please specify correct Unity path as first argument"
    exit 1
fi

FULL_SCENE_PATH="$PROJECT_PATH/$SCENE_PATH"
if [ ! -f "$FULL_SCENE_PATH" ]; then
    echo "Error: Scene not found at $FULL_SCENE_PATH"
    exit 1
fi

echo ""
echo "Executing Unity in headless mode..."

if [ -z "$SKIRT_ROOT" ]; then
    # Hierarchy dump only - with detailed armature analysis
    echo "Command: Detailed hierarchy dump with armature analysis"
    "$UNITY_PATH" \
        -batchmode \
        -nographics \
        -quit \
        -disable-assembly-updater \
        -silent-crashes \
        -logFile "$PROJECT_PATH/data/output/unity.log" \
        -projectPath "$PROJECT_PATH" \
        -openScene "$FULL_SCENE_PATH" \
        -executeMethod SimpleHierarchyDump.Main \
        -hierarchyOut "hierarchy_dump.txt" \
        -deepHierarchy true \
        -findArmature true \
        -showBoneStructure true
else
    # Full configuration
    echo "Command: Configure PhysBones for '$SKIRT_ROOT'"
    "$UNITY_PATH" \
        -batchmode \
        -nographics \
        -quit \
        -disable-assembly-updater \
        -silent-crashes \
        -logFile "$PROJECT_PATH/data/output/unity.log" \
        -projectPath "$PROJECT_PATH" \
        -openScene "$FULL_SCENE_PATH" \
        -executeMethod SimpleHierarchyDump.Main \
        -skirtRoot "$SKIRT_ROOT" \
        -boneRegex "$BONE_REGEX" \
        -angle "$ANGLE" \
        -innerAngle "$INNER_ANGLE" \
        -hierarchyOut "hierarchy_dump.txt" \
        -deepHierarchy true \
        -findArmature true \
        -showBoneStructure true
fi

echo "Exit Code: $?"
echo ""
OUTPUT_DIR="$PROJECT_PATH/data/output"

echo "Check generated files in: $OUTPUT_DIR"
echo "  - hierarchy_dump.txt (complete scene hierarchy with armature structure)"
echo "  - armature_bones.txt (detected armature bones: root, hips, spine, etc.)"
echo "  - physbone_validation_report.txt (if validation run)"

echo ""
echo "=== Quick Armature Preview ==="
if [ -f "$OUTPUT_DIR/unity_armature_bones.txt" ]; then
    echo "Detected armature structure from Unity:"
    head -20 "$OUTPUT_DIR/unity_armature_bones.txt"
elif [ -f "$OUTPUT_DIR/armature_bones.txt" ]; then
    echo "Detected armature structure from shell analysis:"
    head -20 "$OUTPUT_DIR/armature_bones.txt"
else
    echo "No armature structure file generated - check Unity execution logs"
fi

echo ""
echo "=== Generated Files ==="
ls -la "$OUTPUT_DIR/"

echo ""
echo "=== Running Enhanced Analysis ==="
./extract_armature.sh

echo ""
echo "=== ARMATURE DETECTION RESULTS ==="
if [ -f "$OUTPUT_DIR/vrchat_bone_paths.txt" ]; then
    echo "✅ Armature structure successfully detected!"
    echo ""
    cat "$OUTPUT_DIR/vrchat_bone_paths.txt"
else
    echo "❌ Enhanced armature analysis failed"
fi