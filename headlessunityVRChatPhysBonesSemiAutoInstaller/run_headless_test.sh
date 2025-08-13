#!/bin/bash

# Headless Unity VRCPhysBone CLI Runner
# Usage: ./run_headless_test.sh [unity_path] [skirt_root] [bone_regex] [angle] [inner_angle]

UNITY_PATH=${1:-"/opt/unity/Editor/Unity"}
PROJECT_PATH="/mnt/wsl/bpyutils-backup - コピー/headlessunityVRChatPhysBonesSemiAutoInstaller"
SCENE_PATH="data/input/MarvelousDesigner.unity"
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
    # Hierarchy dump only
    echo "Command: Hierarchy dump only"
    "$UNITY_PATH" \
        -batchmode \
        -nographics \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -openScene "$FULL_SCENE_PATH" \
        -executeMethod SkirtPBHeadless.Main \
        -hierarchyOut "hierarchy_dump.txt"
else
    # Full configuration
    echo "Command: Configure PhysBones for '$SKIRT_ROOT'"
    "$UNITY_PATH" \
        -batchmode \
        -nographics \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -openScene "$FULL_SCENE_PATH" \
        -executeMethod SkirtPBHeadless.Main \
        -skirtRoot "$SKIRT_ROOT" \
        -boneRegex "$BONE_REGEX" \
        -angle "$ANGLE" \
        -innerAngle "$INNER_ANGLE" \
        -hierarchyOut "hierarchy_dump.txt"
fi

echo "Exit Code: $?"
echo ""
echo "Check generated files:"
echo "  - hierarchy_dump.txt (scene structure)"
echo "  - physbone_validation_report.txt (if validation run)"