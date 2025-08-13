# Unity Import Guide - Step by Step

This guide shows you exactly how to import your fixed FBX into Unity and get perfect character animation.

## 🎯 What You Should Have

Before starting this guide, you should have:
- ✅ Processed FBX file (ending with `_unity_ready.fbx`)
- ✅ Unity 2020.3 or newer
- ✅ A Unity project open

## 📥 Step 1: Import FBX into Unity

### 1.1 Drag and Drop
```
1. Open your Unity project
2. In Project window, navigate to Assets/Models/ (create folder if needed)
3. Drag your `character_unity_ready.fbx` file into this folder
4. Wait for Unity to process the import
```

### 1.2 Verify Import Success
After import, you should see in the Project window:
```
character_unity_ready.fbx
├── 📦 Meshes (all your character parts)
├── 🎭 Materials (character materials)
└── 🦴 Armature (bone structure)
```

## ⚙️ Step 2: Configure Import Settings

### 2.1 Select the FBX in Project Window
Click on your imported FBX file to see the Inspector.

### 2.2 Model Tab Settings
```
Scale Factor: 1 (usually correct)
Mesh Compression: Off (for best quality)
Read/Write Enabled: ✓ (if you need mesh access)
Optimize Mesh: ✓ (recommended)
Generate Colliders: ✗ (unless needed)
```

### 2.3 Rig Tab Settings (IMPORTANT!)
```
Animation Type: Generic (for most characters)
                OR
Animation Type: Humanoid (for human-like characters)

Avatar Definition: Create From This Model
Optimize Game Objects: ✗ (keep off for rigged characters)
```

**Humanoid vs Generic Guide:**
- **Humanoid**: Choose if your character is human-like with standard limb structure
- **Generic**: Choose for creatures, animals, or non-standard rigs

### 2.4 Animation Tab Settings
```
Import Animation: ✓ (if your FBX has animations)
Bake Animations: ✓ (recommended)
Resample Curves: ✓ (for smoother animation)
```

### 2.5 Materials Tab Settings
```
Material Creation Mode: Standard (Legacy)
                        OR  
Material Creation Mode: Standard (Specular setup) 

Location: Use Embedded Materials
```

## 🎮 Step 3: Test in Unity Scene

### 3.1 Add Character to Scene
```
1. Drag the FBX from Project window into Scene view
2. Position the character at (0, 0, 0)
3. The character should appear with all meshes visible
```

### 3.2 Verify Rigging (CRITICAL TEST)
```
1. In Hierarchy, expand your character object
2. You should see all mesh parts (body, clothing, accessories)
3. Select the main character object
4. In Inspector, look for "Animator" component
```

### 3.3 Test Animation
```
1. Create an Animator Controller:
   - Right-click in Project → Create → Animator Controller
   - Name it "CharacterController"

2. Assign to character:
   - Select character in Hierarchy
   - In Animator component, assign your Controller

3. Add test animation:
   - Double-click the Controller to open Animator window
   - Right-click in Animator → Create State → Empty
   - In Inspector, assign any animation clip

4. Test play:
   - Press Play button in Unity
   - All meshes should animate together!
```

## ✅ Step 4: Verification Checklist

After import, verify these points:

### 4.1 Visual Check
- ✅ All character parts are visible
- ✅ Materials look correct
- ✅ No missing meshes or textures
- ✅ Character is properly positioned

### 4.2 Rigging Check  
- ✅ Animator component is present
- ✅ Avatar is configured (if using Humanoid)
- ✅ All meshes are children of main character object
- ✅ Bones are visible in Scene view (enable bone display)

### 4.3 Animation Check
- ✅ Test animation plays without errors
- ✅ ALL meshes move together (body + clothing + accessories)
- ✅ No frozen or static parts
- ✅ Smooth animation without glitches

## 🚨 Common Unity Import Issues

### Issue 1: "Some meshes are still frozen"
**Cause:** Import settings incorrect
**Solution:**
```
1. Select FBX in Project window
2. Go to Rig tab
3. Make sure Animation Type is NOT "None"  
4. Click "Apply" button
5. Re-test animation
```

### Issue 2: "Character looks broken/distorted"
**Cause:** Wrong avatar configuration  
**Solution:**
```
1. Select FBX in Project window
2. Go to Rig tab  
3. Try changing from "Humanoid" to "Generic" (or vice versa)
4. Click "Apply"
5. Test again
```

### Issue 3: "Materials are missing/pink"
**Cause:** Material import settings
**Solution:**
```
1. Select FBX in Project window
2. Go to Materials tab
3. Try "Extract Materials..." button
4. Choose a folder to extract materials
5. Reassign materials if needed
```

### Issue 4: "Animation is choppy/rough"  
**Cause:** Import animation settings
**Solution:**
```
1. Select FBX in Project window
2. Go to Animation tab
3. Enable "Resample Curves"
4. Set higher quality settings
5. Click "Apply"
```

## 🎯 Advanced Unity Setup

### For Game Development:
```
1. Add Character Controller component for movement
2. Create Animation Controller with states (Idle, Walk, Run, Jump)
3. Set up animation transitions
4. Add scripts for character control
5. Configure physics if needed
```

### For VR/AR Applications:
```
1. Ensure proper scale (usually 1:1)
2. Test with hand tracking if applicable
3. Optimize for target platform performance
4. Consider LOD (Level of Detail) for distance rendering
```

### Performance Optimization:
```
1. Use "Optimize Game Objects" for background characters
2. Set up LOD groups for crowd scenes  
3. Use texture compression
4. Consider mesh compression for distant characters
```

## 📊 Unity Performance Metrics

After successful import, check these metrics:

### Polygon Count:
```
Window → Analysis → Frame Debugger
- Check total triangle count
- Ensure reasonable for target platform
- Consider LOD if too high
```

### Texture Memory:
```
Window → Analysis → Memory Profiler  
- Check texture memory usage
- Optimize texture sizes if needed
- Use appropriate compression formats
```

### Animation Performance:
```
Profiler → CPU Usage → Scripts & Animation
- Monitor animation overhead
- Optimize bone count if necessary
- Use animation compression
```

## 🎉 Success! What's Next?

Once your character is successfully imported and animating:

1. **Add Game Logic** - Character controllers, AI, etc.
2. **Create Animations** - Import or create animation clips  
3. **Set Up Materials** - Fine-tune shaders and textures
4. **Optimize Performance** - LOD, compression, culling
5. **Test on Target Platform** - Mobile, PC, console, etc.

Your character is now ready for Unity game development! 🚀

---

## 🆘 Still Having Problems?

If you're still experiencing issues:

1. **Check Process Logs**: Look in `project-files/process-logs/` for detailed information
2. **Verify Source FBX**: Make sure the weight transfer tool completed successfully  
3. **Test in Blender**: Open the processed FBX in Blender to verify it's correct
4. **Unity Console**: Check for error messages in Unity's Console window
5. **Unity Version**: Ensure you're using Unity 2020.3 or newer

Remember: The weight transfer tool creates Unity-optimized FBX files, so if the tool completed successfully, the issue is likely in the Unity import settings rather than the FBX itself.