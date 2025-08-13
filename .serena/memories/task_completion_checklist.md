# Task Completion Checklist

## When Completing Tasks

### 1. Code Quality Checks
- **No Linting Available**: This project doesn't use standard Python linters (no requirements.txt, no config files found)
- **Manual Review**: Check Python syntax and imports manually
- **Type Hints**: Ensure type annotations are included for new functions
- **Docstrings**: Add descriptive docstrings for new functions

### 2. Testing (Manual)
- **Blender Functionality**: Test scripts with Blender 4.0.2
  ```bash
  tools/blender --version  # Verify Blender works
  ```
- **FBX Processing**: Test with sample FBX files if available
- **Unity Import**: Verify output FBX works in Unity (if Unity available)

### 3. Documentation Updates
- Update relevant README.md files if functionality changes
- Update CLAUDE.md if new commands or workflows are added
- Ensure shell script help text is accurate

### 4. File Organization
- Place Python scripts in appropriate directories:
  - `blender-workspace/scripts/` for Blender-specific scripts
  - `unity-scripts/` for Unity-optimized scripts
  - `organized/scripts/` for general scripts
- Place shell scripts in `organized/bin/` or appropriate `tools/` directory

### 5. Configuration Management
- Ensure config files are properly structured
- Test default values work when config files are missing
- Update config examples if new options are added

### 6. System Requirements
- **Linux/WSL2**: Ensure compatibility
- **Blender 4.0.2**: Verify Blender API compatibility
- **Unity 2020.3+**: Check Unity import requirements

### 7. Before Committing
- Test main entry points:
  ```bash
  ./organized/bin/transfer_weights.sh --help
  ./fbx-weight-transfer-unity/tools/fix_fbx_for_unity.sh --help
  ```
- Verify file permissions on shell scripts (executable)
- Check that all paths are correctly resolved