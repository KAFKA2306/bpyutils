#!/bin/bash

# SkirtPB PhysBone Installation Script
# Executes SkirtPBInstallerHeadless for confirmed HipRoot target

UNITY_PATH=${1:-"/home/kafka/Unity/Hub/Editor/2022.3.22f1/Editor/Unity"}
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SCENE_PATH="Assets/scene/MarvelousDesigner.unity"
SKIRT_ROOT=${2:-"HipRoot"}
BONE_REGEX=${3:-".*\\.001"}
ANGLE=${4:-"45"}
INNER_ANGLE=${5:-"10"}

echo "=== SkirtPB PhysBone Installation ==="
echo "Unity Path: $UNITY_PATH"
echo "Project: $PROJECT_PATH"
echo "Scene: $SCENE_PATH"
echo "Skirt Root: $SKIRT_ROOT"
echo "Bone Regex: $BONE_REGEX"
echo "Angle: $ANGLE°"
echo "Inner Angle: $INNER_ANGLE°"
echo ""

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

OUTPUT_DIR="$PROJECT_PATH/data/output"
mkdir -p "$OUTPUT_DIR"

echo "Executing Unity SkirtPB Installation..."
echo "Command: Installing PhysBones for '$SKIRT_ROOT'"

"$UNITY_PATH" \
    -batchmode \
    -nographics \
    -quit \
    -disable-assembly-updater \
    -silent-crashes \
    -logFile "$OUTPUT_DIR/physbone_installation.log" \
    -projectPath "$PROJECT_PATH" \
    -openScene "$FULL_SCENE_PATH" \
    -executeMethod SkirtPBInstallerHeadless.Main \
    -skirtRoot "$SKIRT_ROOT" \
    -boneRegex "$BONE_REGEX" \
    -angle "$ANGLE" \
    -innerAngle "$INNER_ANGLE"

EXIT_CODE=$?
echo "Exit Code: $EXIT_CODE"
echo ""

echo "Check generated files in: $OUTPUT_DIR"
echo "  - physbone_installation.log (Unity execution log)"
echo "  - physbone_installation_report.txt (Installation details)"
echo ""

if [ $EXIT_CODE -eq 0 ]; then
    echo "🎉 SUCCESS: SkirtPB PhysBone installation completed!"
    
    if [ -f "$OUTPUT_DIR/physbone_installation_report.txt" ]; then
        echo ""
        echo "=== Installation Report Preview ==="
        head -20 "$OUTPUT_DIR/physbone_installation_report.txt"
    fi
else
    echo "❌ FAILED: SkirtPB PhysBone installation failed"
    echo "Check the Unity log for details:"
    
    if [ -f "$OUTPUT_DIR/physbone_installation.log" ]; then
        echo ""
        echo "=== Unity Log (last 20 lines) ==="
        tail -20 "$OUTPUT_DIR/physbone_installation.log"
    fi
fi

echo ""
echo "=== Generated Files ==="
ls -la "$OUTPUT_DIR/" | grep -E "\.(log|txt)$"