#!/bin/bash

# FBX Material Converter - Batch execution script for Linux/Mac
# This script runs Unity in headless mode to convert FBX materials to lilToon

set -e  # Exit on any error

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.15f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
LOG_FILE="$PROJECT_PATH/Logs/conversion.log"
CONFIG_FILE="$PROJECT_PATH/Config/ConversionConfig.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Unity exists (try multiple common paths)
UNITY_PATHS=(
    "/Applications/Unity/Hub/Editor/2022.3.15f1/Unity.app/Contents/MacOS/Unity"
    "/Applications/Unity/Hub/Editor/2022.3.0f1/Unity.app/Contents/MacOS/Unity"
    "/opt/unity/Editor/Unity"
    "/usr/bin/unity-editor"
    "$(which unity-editor 2>/dev/null || echo '')"
)

UNITY_FOUND=""
for path in "${UNITY_PATHS[@]}"; do
    if [[ -x "$path" ]]; then
        UNITY_FOUND="$path"
        break
    fi
done

if [[ -z "$UNITY_FOUND" ]]; then
    print_error "Unity not found in any of the expected locations:"
    for path in "${UNITY_PATHS[@]}"; do
        echo "  - $path"
    done
    print_error "Please install Unity or update UNITY_PATH in this script"
    exit 1
fi

UNITY_PATH="$UNITY_FOUND"

# Check if project path exists
if [[ ! -d "$PROJECT_PATH" ]]; then
    print_error "Project path not found: $PROJECT_PATH"
    exit 1
fi

# Create logs directory if it doesn't exist
mkdir -p "$PROJECT_PATH/Logs"

print_status "============================================"
print_status "FBX Material Converter - Headless Unity"
print_status "============================================"
print_status "Unity Path: $UNITY_PATH"
print_status "Project Path: $PROJECT_PATH"
print_status "Config File: $CONFIG_FILE"
print_status "Log File: $LOG_FILE"
print_status "============================================"

# Run Unity in batch mode
print_status "Starting conversion process..."

if "$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -executeMethod FBXMaterialConverter.FBXMaterialConverterBatch.ConvertAllFBXMaterials \
    -logFile "$LOG_FILE"; then
    
    echo ""
    print_success "============================================"
    print_success "CONVERSION COMPLETED SUCCESSFULLY"
    print_success "============================================"
    print_status "Check the log file for details: $LOG_FILE"
else
    echo ""
    print_error "============================================"
    print_error "CONVERSION FAILED WITH ERRORS"
    print_error "============================================"
    print_error "Check the log file for details: $LOG_FILE"
    print_error "Exit code: $?"
fi

# Display log file if it exists
if [[ -f "$LOG_FILE" ]]; then
    echo ""
    print_status "Last 20 lines of log file:"
    echo "----------------------------------------"
    tail -20 "$LOG_FILE"
    echo "----------------------------------------"
    print_status "Full log available at: $LOG_FILE"
else
    print_warning "Log file not found: $LOG_FILE"
fi

# Check for results JSON in log
if [[ -f "$LOG_FILE" ]] && grep -q "CONVERSION_RESULTS_JSON_START" "$LOG_FILE"; then
    echo ""
    print_status "Conversion Results:"
    sed -n '/CONVERSION_RESULTS_JSON_START/,/CONVERSION_RESULTS_JSON_END/p' "$LOG_FILE" | sed '1d;$d'
fi