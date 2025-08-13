# System Commands Reference

## Linux/WSL2 System Commands

### File Operations
```bash
ls -la                    # List files with details
find . -name "*.py"       # Find Python files
find . -name "*.fbx"      # Find FBX files
find . -type f -executable # Find executable files
du -h file.fbx           # Check file size
realpath file.fbx        # Get absolute path
mkdir -p output/         # Create directories
```

### File Permissions
```bash
chmod +x script.sh       # Make script executable
chmod 755 script.sh      # Set standard executable permissions
```

### Text Processing
```bash
grep -r "pattern" .      # Search for pattern in files
awk -F= "/pattern/" file # Parse config files
cat file.txt             # Display file contents
head -n 20 file.txt      # Show first 20 lines
tail -f logfile.log      # Follow log file
```

### Process Management
```bash
ps aux | grep blender    # Find Blender processes
kill -9 PID             # Force kill process
nohup command &         # Run command in background
```

### Git Operations
```bash
git status              # Check repository status
git add .               # Stage all changes
git commit -m "msg"     # Commit with message
git log --oneline       # View commit history
git diff                # Show changes
```

### Archive Operations
```bash
tar -xzf file.tar.gz    # Extract tar.gz
zip -r archive.zip dir/ # Create zip archive
unzip archive.zip       # Extract zip
```

### System Information
```bash
uname -a                # System information
df -h                   # Disk space
free -h                 # Memory usage
which blender           # Find command location
echo $PATH              # Show PATH variable
```

### WSL2 Specific
```bash
/mnt/c/                 # Access Windows C: drive
explorer.exe .          # Open Windows Explorer
cmd.exe /c "command"    # Run Windows command
```

### Blender Specific
```bash
blender --version       # Check Blender version
blender --help          # Show Blender help
blender --background    # Run without GUI
blender --python script.py # Run Python script
```