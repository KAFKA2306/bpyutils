# Fix FBX for Unity - Weight Transfer Tool

**Solve the frozen clothing problem in Unity!** This tool automatically transfers vertex weights from body meshes to clothing meshes, making your character models animate properly in Unity.

## 🎯 The Unity Problem This Solves

**Before (Broken):**
- Import character FBX into Unity ❌
- Body animates correctly, but clothing is frozen ❌  
- Accessories don't move with character ❌
- Only some meshes have skinning weights ❌

**After (Fixed):**
- Import the processed FBX into Unity ✅
- All meshes animate together perfectly ✅
- Clothing moves naturally with body ✅
- Complete character animation works ✅

## 🚀 Quick Start (2 Simple Steps!)

### Step 1: Run the Fix Tool
```bash
# Navigate to tools directory
cd fbx-weight-transfer-unity/tools/

# Fix your character FBX for Unity
./fix_fbx_for_unity.sh your_character.fbx your_character_unity_ready.fbx
```

### Step 2: Import to Unity
- Import the `_unity_ready.fbx` file into Unity
- Set Animation Type to "Humanoid" or "Generic"  
- Apply animations - everything should work!

## 📁 Clear Directory Structure

```
fbx-weight-transfer-unity/
├── 🛠️ tools/                                    # Main tools
│   ├── fix_fbx_for_unity.sh                     # ⭐ MAIN TOOL - Run this!
│   └── blender                                   # Blender 4.0.2 (auto-linked)
├── 🔧 unity-scripts/                             # Technical scripts  
│   ├── blender_weight_transfer_for_unity.py     # Core weight transfer
│   └── Kiseru/                                   # Advanced weight tools
├── 📖 tutorial/                                  # Step-by-step guides
│   ├── unity_import_guide.md                    # Unity import instructions
│   └── troubleshooting.md                       # Fix common problems
├── ❓ help/                                      # Documentation
│   └── technical_details.md                     # How it works
└── 📂 project-files/                            # Your files
    ├── input-fbx/                               # Put your FBX files here
    ├── output-fbx/                              # Fixed files appear here
    └── process-logs/                            # Detailed logs
```

## ✨ What Makes This Tool Unity-Focused

### Unity-Specific Features
- **Unity FBX Settings**: Optimized export settings for Unity import
- **Unity Bone Axis**: Correct bone orientations (Y-up, X-forward)
- **Unity Scaling**: Proper scale handling for Unity scenes
- **Unity Skinning**: Compatible with Unity's skinning system
- **Unity Animation**: Works with Unity's animation system

### Technical Advantages  
- **Surface Sampling**: Uses `POLYINTERP_NEAREST` for accurate weight transfer
- **Batch Processing**: Handles multiple clothing meshes at once
- **Automatic Detection**: Finds body mesh and clothing automatically
- **Unity Validation**: Verifies Unity readiness before export
- **Detailed Logging**: Complete logs for troubleshooting Unity issues

## 🎮 Unity Developer Workflow

### Typical Unity Problem:
```
1. Export character from Maya/Blender → FBX
2. Import FBX into Unity
3. Problem: Clothing meshes are frozen during animation
4. Cause: Clothing has no vertex weights/skinning
```

### Solution with This Tool:
```
1. Export character from Maya/Blender → FBX  
2. Run: ./fix_fbx_for_unity.sh character.fbx character_fixed.fbx
3. Import character_fixed.fbx into Unity
4. Result: Perfect animation - all meshes move correctly! ✅
```

## 📋 Requirements

### Input FBX Must Have:
- ✅ **Rigged Body Mesh** - At least one mesh with vertex weights (usually "Body", "Bodybody", etc.)
- ✅ **Character Armature** - Bone structure for the character  
- ✅ **Clothing Meshes** - Meshes that need weights (accessories, clothing, etc.)

### System Requirements:
- ✅ **Linux/WSL2** - Windows Subsystem for Linux recommended
- ✅ **Blender 4.0.2+** - Included in this package
- ✅ **Unity 2020.3+** - For importing the fixed FBX

## 🔥 Real Example: Before/After

### Input FBX Analysis:
```
hero_character.fbx (2.3 MB)
├── Armature (99 bones) ✅
├── Body (58 vertex groups) ✅ - Source for weights  
└── 41 clothing meshes (0 vertex groups) ❌ - Need fixing
    ├── Jacket, Pants, Shoes...
    ├── Accessories, Belts...
    └── Hair, Hat...
```

### Processing Command:
```bash
./fix_fbx_for_unity.sh hero_character.fbx hero_character_unity_ready.fbx
```

### Output FBX Result:
```
hero_character_unity_ready.fbx (6.7 MB)
├── Armature (99 bones) ✅
├── Body (58 vertex groups) ✅
└── 41 clothing meshes (58 vertex groups each) ✅ - All fixed!
    ├── All meshes have proper weights
    ├── All meshes have armature modifiers
    └── All meshes ready for Unity animation
```

### Unity Import Result:
```
Perfect character animation! 
✅ Body moves correctly
✅ All clothing follows body movement  
✅ Accessories animate properly
✅ No frozen parts
✅ Ready for game development
```

## 🐛 Troubleshooting Unity Issues

### Problem: "No suitable source mesh found"
**Unity Cause:** Body mesh has no vertex weights  
**Solution:** Ensure your body mesh is rigged before export

### Problem: "All weight transfers failed"  
**Unity Cause:** Clothing meshes too different from body mesh  
**Solution:** Check mesh topology, try with simpler clothing

### Problem: Unity animation still broken
**Unity Cause:** Wrong import settings in Unity  
**Solution:** Set Animation Type to "Humanoid" or "Generic"

### Get Detailed Help:
```bash
# View detailed logs
ls -la project-files/process-logs/
cat project-files/process-logs/unity_weight_transfer_*.log

# Test Blender directly  
tools/blender --version

# Manual Blender test
tools/blender your_character.fbx
```

## 💡 Advanced Usage

### Batch Process Multiple Characters:
```bash
# Process all FBX files in input directory
for fbx in project-files/input-fbx/*.fbx; do
    name=$(basename "$fbx" .fbx)
    ./fix_fbx_for_unity.sh "$fbx" "project-files/output-fbx/${name}_unity_ready.fbx"
done
```

### Direct Blender Script Usage:
```bash
tools/blender --background --python unity-scripts/blender_weight_transfer_for_unity.py -- input.fbx output.fbx
```

### Unity Import Checklist:
1. ✅ Import the `_unity_ready.fbx` file
2. ✅ In Model tab: Keep default settings
3. ✅ In Rig tab: Set Animation Type (Humanoid/Generic)  
4. ✅ In Animation tab: Import animations as needed
5. ✅ Test animation - all parts should move!

## 🤝 Support & Help

### Check These First:
1. **Log Files** - `project-files/process-logs/` has detailed info
2. **File Sizes** - Output should be larger (weights add data)
3. **Unity Console** - Check for import warnings/errors
4. **Blender Test** - Open output FBX in Blender to verify

### Common Unity Workflow Issues:
- **Animation Type**: Set to Humanoid for character rigs
- **Scale Factor**: Unity uses meters, adjust if needed
- **Material Assignment**: Materials might need reassignment
- **Avatar Configuration**: Configure avatar for humanoid rigs

## 📄 Technical Information

- **Tool Version**: 1.0.0
- **Blender Version**: 4.0.2+ 
- **Unity Compatibility**: 2020.3+ (all versions)
- **Weight Transfer Method**: Surface interpolation with POLYINTERP_NEAREST
- **Export Format**: FBX 7.7 with Unity optimizations
- **License**: MIT License

## 👨‍💻 For Unity Developers

This tool was specifically designed for Unity game developers who face the common character rigging import problems. It automates the tedious process of weight transfer that would otherwise require manual work in Blender or other 3D software.

**Perfect for:**
- Character artists importing to Unity
- Game developers with rigging issues  
- Teams working with outsourced character assets
- Anyone tired of frozen clothing in Unity!

---

**Generated with Claude Code for Unity developers** 🎮