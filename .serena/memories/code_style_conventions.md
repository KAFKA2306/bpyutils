# Code Style and Conventions

## Python Style
- **Type Hints**: Full type annotations used (e.g., `log_dir: str) -> logging.Logger`)
- **Docstrings**: Brief, descriptive docstrings for functions
- **Naming**: Snake_case for functions and variables
- **Imports**: Standard library imports organized at top
- **Logging**: Comprehensive logging with different levels (DEBUG for files, INFO for console)
- **Error Handling**: Proper exception handling and validation

## Example Function Pattern
```python
def setup_detailed_logging(log_dir: str) -> logging.Logger:
    """Setup comprehensive logging system for debugging Unity import issues"""
    # Implementation with proper error handling
```

## Shell Script Style
- **Headers**: Comprehensive header comments with usage examples
- **Configuration**: External config file support with defaults
- **Validation**: Input validation and file existence checks
- **Output**: User-friendly colored/formatted output with emojis (✅❌)
- **Error Handling**: Proper exit codes and error messages
- **Paths**: Absolute path resolution for reliability

## File Organization
- Scripts in language-specific directories (`scripts/`, `unity-scripts/`)
- Binaries in `bin/` directories
- Configuration files alongside scripts
- Clear separation between workspace and implementation code

## Constants and Settings
- Configuration dictionaries for tool settings (e.g., `UNITY_FBX_SETTINGS`)
- Centralized configuration management
- Default values with override capability