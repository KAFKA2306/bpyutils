#!/bin/bash
#
# FBX Weight Transfer - Batch Script
# ==================================
#
# Easy-to-use script for transferring weights from body mesh to clothing meshes.
# Optimized for Unity import with configurable settings.
#
# Usage:
#     ./transfer_weights.sh input.fbx output.fbx [config_file]
#
# Examples:
#     ./transfer_weights.sh ../workspace/input/character.fbx ../workspace/output/character_rigged.fbx
#     ./transfer_weights.sh model.fbx model_with_weights.fbx custom.conf
#
# Requirements:
# - Input FBX with at least one rigged mesh (body with vertex groups)
# - Blender 4.0.2 (included in this package)
# - Configuration file (weight_transfer.conf by default)
#
# Author: Generated with Claude Code
#

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="${3:-$SCRIPT_DIR/../scripts/weight_transfer.conf}"

# Function to read config values
read_config() {
    local section="$1"
    local key="$2"
    local default="$3"
    
    if [ -f "$CONFIG_FILE" ]; then
        # Read value from config file
        local value=$(awk -F= "/^\[$section\]/,/^\[/{if(/^$key=/) print \$2}" "$CONFIG_FILE" | tr -d ' ')
        echo "${value:-$default}"
    else
        echo "$default"
    fi
}

# Load configuration
BLENDER_BIN="$SCRIPT_DIR/$(read_config "PATHS" "BLENDER_BIN" "blender")"
PYTHON_SCRIPT="$SCRIPT_DIR/../scripts/$(read_config "PATHS" "PYTHON_SCRIPT" "fbx_weight_transfer.py")"
VERBOSE=$(read_config "OUTPUT" "VERBOSE" "true")
SHOW_PROGRESS=$(read_config "OUTPUT" "SHOW_PROGRESS" "true")

# Check arguments
if [ $# -lt 2 ] || [ $# -gt 3 ]; then
    echo "Usage: $0 <input.fbx> <output.fbx> [config_file]"
    echo ""
    echo "Examples:"
    echo "  $0 character.fbx character_with_weights.fbx"
    echo "  $0 ../input/model.fbx ../output/model_rigged.fbx"
    echo "  $0 model.fbx output.fbx custom_config.conf"
    echo ""
    echo "Config file: ${CONFIG_FILE}"
    exit 1
fi

INPUT_FBX="$1"
OUTPUT_FBX="$2"

# Check if input file exists
if [ ! -f "$INPUT_FBX" ]; then
    echo "Error: Input file '$INPUT_FBX' not found!"
    exit 1
fi

# Check if Blender exists
if [ ! -f "$BLENDER_BIN" ]; then
    echo "Error: Blender not found at '$BLENDER_BIN'"
    echo "Please ensure Blender 4.0.2 is properly installed."
    exit 1
fi

# Check if Python script exists
if [ ! -f "$PYTHON_SCRIPT" ]; then
    echo "Error: Python script not found at '$PYTHON_SCRIPT'"
    exit 1
fi

# Convert to absolute paths
INPUT_FBX="$(realpath "$INPUT_FBX")"
OUTPUT_FBX="$(realpath "$OUTPUT_FBX")"

# Create output directory if needed
OUTPUT_DIR="$(dirname "$OUTPUT_FBX")"
mkdir -p "$OUTPUT_DIR"

echo "=== FBX Weight Transfer ==="
echo "Input:   $INPUT_FBX"
echo "Output:  $OUTPUT_FBX"
echo "Config:  $CONFIG_FILE"
if [ "$VERBOSE" = "true" ]; then
    echo "Blender: $BLENDER_BIN"
    echo "Script:  $PYTHON_SCRIPT"
fi
echo "Starting weight transfer process..."
echo ""

# Run Blender with the weight transfer script, passing config file
"$BLENDER_BIN" --background --python "$PYTHON_SCRIPT" -- "$INPUT_FBX" "$OUTPUT_FBX" "$CONFIG_FILE"

# Check if output file was created
if [ -f "$OUTPUT_FBX" ]; then
    echo ""
    echo "✅ Weight transfer completed successfully!"
    echo "Output file: $OUTPUT_FBX"
    echo "File size: $(du -h "$OUTPUT_FBX" | cut -f1)"
    echo ""
    echo "The file is now ready for Unity import with full weight painting."
else
    echo ""
    echo "❌ Weight transfer failed!"
    echo "Check the log files in ../workspace/logs/ for details."
    exit 1
fi