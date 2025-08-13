#!/bin/bash

# Headless Unity VRCPhysBone CLI Test Runner
# Usage: ./run_headless_test.sh [unity_path] [project_path] [scene_path] [additional_args...]

set -e

# Default paths (modify as needed)
UNITY_PATH=${1:-"/opt/unity/Editor/Unity"}
PROJECT_PATH=${2:-"/mnt/wsl/bpyutils-backup - コピー/headlessunityVRChatPhysBonesSemiAutoInstaller"}
SCENE_PATH=${3:-"data/input/MarvelousDesigner.unity"}

# Build additional arguments
ADDITIONAL_ARGS=""
if [ $# -gt 3 ]; then
    shift 3
    ADDITIONAL_ARGS="$@"
fi

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== Headless Unity VRCPhysBone CLI Test Runner ===${NC}"
echo -e "${YELLOW}Unity Path:${NC} $UNITY_PATH"
echo -e "${YELLOW}Project Path:${NC} $PROJECT_PATH"
echo -e "${YELLOW}Scene Path:${NC} $SCENE_PATH"
echo -e "${YELLOW}Additional Args:${NC} $ADDITIONAL_ARGS"
echo ""

# Check if Unity exists
if [ ! -f "$UNITY_PATH" ]; then
    echo -e "${RED}Error: Unity not found at $UNITY_PATH${NC}"
    echo "Please specify correct Unity path as first argument"
    exit 1
fi

# Check if project exists
if [ ! -d "$PROJECT_PATH" ]; then
    echo -e "${RED}Error: Project not found at $PROJECT_PATH${NC}"
    exit 1
fi

# Full scene path
FULL_SCENE_PATH="$PROJECT_PATH/$SCENE_PATH"
if [ ! -f "$FULL_SCENE_PATH" ]; then
    echo -e "${RED}Error: Scene not found at $FULL_SCENE_PATH${NC}"
    exit 1
fi

# Create logs directory
LOG_DIR="$PROJECT_PATH/logs"
mkdir -p "$LOG_DIR"
TIMESTAMP=$(date '+%Y%m%d_%H%M%S')
LOG_FILE="$LOG_DIR/unity_headless_$TIMESTAMP.log"

echo -e "${BLUE}Starting Unity in headless mode...${NC}"
echo -e "${YELLOW}Logs will be written to: $LOG_FILE${NC}"
echo ""

# Test 1: Hierarchy dump only (no skirtRoot)
echo -e "${BLUE}=== Test 1: Hierarchy Dump Only ===${NC}"
"$UNITY_PATH" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -openScene "$FULL_SCENE_PATH" \
    -executeMethod SkirtPBHeadless.Main \
    -hierarchyOut "test1_hierarchy.txt" \
    -logFile "$LOG_FILE.test1" \
    $ADDITIONAL_ARGS

EXIT_CODE=$?
echo -e "${YELLOW}Test 1 Exit Code: $EXIT_CODE${NC}"

# Test 2: With skirtRoot but no matching bones (expected to fail)
echo -e "\n${BLUE}=== Test 2: Invalid Skirt Root ===${NC}"
"$UNITY_PATH" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -openScene "$FULL_SCENE_PATH" \
    -executeMethod SkirtPBHeadless.Main \
    -skirtRoot "NonExistentRoot" \
    -hierarchyOut "test2_hierarchy.txt" \
    -logFile "$LOG_FILE.test2" \
    $ADDITIONAL_ARGS

EXIT_CODE=$?
echo -e "${YELLOW}Test 2 Exit Code: $EXIT_CODE${NC}"

# Test 3: Try to find actual root from hierarchy dump
echo -e "\n${BLUE}=== Test 3: Analyzing Hierarchy Dump ===${NC}"
HIERARCHY_FILE="$PROJECT_PATH/test1_hierarchy.txt"
if [ -f "$HIERARCHY_FILE" ]; then
    echo -e "${GREEN}Hierarchy dump found. Searching for potential skirt roots...${NC}"
    
    # Look for common skirt root patterns
    echo -e "${YELLOW}Potential Hip/Skirt roots:${NC}"
    grep -i "hip\|skirt\|bone" "$HIERARCHY_FILE" | head -10 || true
    
    echo -e "\n${YELLOW}All root objects:${NC}"
    grep "^[^ ]" "$HIERARCHY_FILE" | head -10 || true
    
    # Try with a generic root if available
    POTENTIAL_ROOT=$(grep "^[^ ]" "$HIERARCHY_FILE" | head -1 | cut -d'|' -f1 | xargs)
    if [ ! -z "$POTENTIAL_ROOT" ]; then
        echo -e "\n${BLUE}=== Test 4: Testing with root '$POTENTIAL_ROOT' ===${NC}"
        "$UNITY_PATH" \
            -batchmode \
            -nographics \
            -quit \
            -projectPath "$PROJECT_PATH" \
            -openScene "$FULL_SCENE_PATH" \
            -executeMethod SkirtPBHeadless.Main \
            -skirtRoot "$POTENTIAL_ROOT" \
            -boneRegex ".*" \
            -hierarchyOut "test4_hierarchy.txt" \
            -logFile "$LOG_FILE.test4" \
            $ADDITIONAL_ARGS
        
        EXIT_CODE=$?
        echo -e "${YELLOW}Test 4 Exit Code: $EXIT_CODE${NC}"
    fi
else
    echo -e "${RED}Hierarchy dump not found${NC}"
fi

echo -e "\n${GREEN}=== Test Execution Complete ===${NC}"
echo -e "${YELLOW}Check log files in: $LOG_DIR${NC}"
echo -e "${YELLOW}Check hierarchy dumps in: $PROJECT_PATH${NC}"

# Display recent log content
echo -e "\n${BLUE}=== Recent Log Output ===${NC}"
if [ -f "$LOG_FILE.test1" ]; then
    echo -e "${YELLOW}Test 1 Log (last 20 lines):${NC}"
    tail -20 "$LOG_FILE.test1" || true
fi