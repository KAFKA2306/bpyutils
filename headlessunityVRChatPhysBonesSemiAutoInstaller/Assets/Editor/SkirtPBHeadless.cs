using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using VRC.SDK3.Dynamics.PhysBone.Components;

public class SkirtPBHeadless
{
    private static string skirtRoot = "";
    private static float angle = 45f;
    private static float innerAngle = 10f;
    private static string boneRegex = @"\.\d+";
    private static string hierarchyOut = "hierarchy_dump.txt";
    private static bool deepHierarchy = false;
    private static bool findArmature = false;
    private static bool showBoneStructure = false;

    [MenuItem("Tools/SkirtPB Headless Test")]
    public static void TestRun()
    {
        string[] args = {
            "-skirtRoot", "TestRoot",
            "-angle", "45",
            "-innerAngle", "10",
            "-boneRegex", @"\.\d+",
            "-hierarchyOut", "hierarchy_dump.txt"
        };
        
        Main(args);
    }

    public static void Main(string[] args)
    {
        Debug.Log("=== Skirt PhysBone Headless Installer Started ===");
        
        try
        {
            ParseCommandLineArguments(args);
            
            // FR-1: Always dump hierarchy first
            DumpSceneHierarchy();
            
            // FR-1.5: Find and analyze armature if requested
            if (findArmature)
            {
                FindAndAnalyzeArmature();
            }
            
            // FR-2: Check if skirtRoot was specified
            if (string.IsNullOrEmpty(skirtRoot))
            {
                Debug.Log("No skirtRoot specified. Hierarchy dump completed. Exiting with code 1.");
                ExitWithCode(1);
                return;
            }
            
            // Find the skirt root GameObject
            GameObject rootObject = GameObject.Find(skirtRoot);
            if (rootObject == null)
            {
                Debug.LogError($"Skirt root '{skirtRoot}' not found in scene. Exiting with code 2.");
                ExitWithCode(2);
                return;
            }
            
            Debug.Log($"Found skirt root: {rootObject.name} at {GetFullPath(rootObject.transform)}");
            
            // FR-3: Find matching bones
            GameObject[] matchingBones = GetChildrenMatching(rootObject, boneRegex);
            if (matchingBones.Length == 0)
            {
                Debug.LogError($"No bones found matching regex '{boneRegex}' under '{skirtRoot}'. Exiting with code 2.");
                ExitWithCode(2);
                return;
            }
            
            Debug.Log($"Found {matchingBones.Length} matching bones: {GetBoneNames(matchingBones)}");
            
            // FR-4: Check for existing PhysBones
            if (!CheckForPhysBone(matchingBones))
            {
                Debug.LogError("One or more bones are missing VRCPhysBone components. Exiting with code 3.");
                ExitWithCode(3);
                return;
            }
            
            Debug.Log("All bones have VRCPhysBone components. Proceeding with configuration.");
            
            // FR-5: Configure PhysBones
            ConfigurePhysBones(matchingBones);
            
            // FR-6: Save assets
            SaveAssets();
            
            Debug.Log("=== Skirt PhysBone configuration completed successfully ===");
            ExitWithCode(0);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}\n{ex.StackTrace}");
            ExitWithCode(4);
        }
    }

    private static void ParseCommandLineArguments(string[] args)
    {
        Debug.Log($"Parsing {args.Length} command line arguments");
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-skirtRoot":
                    if (i + 1 < args.Length)
                    {
                        skirtRoot = args[++i];
                        Debug.Log($"Skirt root set to: {skirtRoot}");
                    }
                    break;
                case "-angle":
                    if (i + 1 < args.Length && float.TryParse(args[++i], out float angleVal))
                    {
                        angle = angleVal;
                        Debug.Log($"Angle set to: {angle}");
                    }
                    break;
                case "-innerAngle":
                    if (i + 1 < args.Length && float.TryParse(args[++i], out float innerAngleVal))
                    {
                        innerAngle = innerAngleVal;
                        Debug.Log($"Inner angle set to: {innerAngle}");
                    }
                    break;
                case "-boneRegex":
                    if (i + 1 < args.Length)
                    {
                        boneRegex = args[++i];
                        Debug.Log($"Bone regex set to: {boneRegex}");
                    }
                    break;
                case "-hierarchyOut":
                    if (i + 1 < args.Length)
                    {
                        hierarchyOut = args[++i];
                        Debug.Log($"Hierarchy output file set to: {hierarchyOut}");
                    }
                    break;
                case "-deepHierarchy":
                    if (i + 1 < args.Length && bool.TryParse(args[++i], out bool deepVal))
                    {
                        deepHierarchy = deepVal;
                        Debug.Log($"Deep hierarchy set to: {deepHierarchy}");
                    }
                    break;
                case "-findArmature":
                    if (i + 1 < args.Length && bool.TryParse(args[++i], out bool armatureVal))
                    {
                        findArmature = armatureVal;
                        Debug.Log($"Find armature set to: {findArmature}");
                    }
                    break;
                case "-showBoneStructure":
                    if (i + 1 < args.Length && bool.TryParse(args[++i], out bool boneVal))
                    {
                        showBoneStructure = boneVal;
                        Debug.Log($"Show bone structure set to: {showBoneStructure}");
                    }
                    break;
            }
        }
    }

    private static void DumpSceneHierarchy()
    {
        Debug.Log("Starting scene hierarchy dump...");
        
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log($"Active scene: {activeScene.name}");
        
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        Debug.Log($"Found {rootObjects.Length} root objects in scene");
        
        StringBuilder hierarchyDump = new StringBuilder();
        hierarchyDump.AppendLine($"=== Scene Hierarchy Dump: {activeScene.name} ===");
        hierarchyDump.AppendLine($"Generated: {DateTime.Now}");
        hierarchyDump.AppendLine($"Root objects: {rootObjects.Length}");
        hierarchyDump.AppendLine();
        
        foreach (GameObject rootObj in rootObjects)
        {
            DumpGameObjectRecursive(rootObj.transform, hierarchyDump, 0);
        }
        
        string outputDir = System.IO.Path.Combine(Application.dataPath, "..", "data", "output");
        System.IO.Directory.CreateDirectory(outputDir);
        string hierarchyPath = System.IO.Path.Combine(outputDir, hierarchyOut);
        System.IO.File.WriteAllText(hierarchyPath, hierarchyDump.ToString());
        
        Debug.Log($"Hierarchy dump written to: {hierarchyPath}");
        Debug.Log($"Total objects processed: {hierarchyDump.ToString().Split('\n').Length - 4}");
    }

    private static void DumpGameObjectRecursive(Transform transform, StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);
        string fullPath = GetFullPath(transform);
        
        // Enhanced output with component information
        string componentInfo = "";
        if (showBoneStructure)
        {
            var components = transform.GetComponents<Component>();
            var compNames = new List<string>();
            foreach (var comp in components)
            {
                if (comp != null && !(comp is Transform))
                {
                    compNames.Add(comp.GetType().Name);
                }
            }
            if (compNames.Count > 0)
            {
                componentInfo = $" [{string.Join(", ", compNames)}]";
            }
        }
        
        sb.AppendLine($"{indent}{transform.name} | {fullPath}{componentInfo}");
        
        // Continue recursion for deep hierarchy or limit to reasonable depth
        if (deepHierarchy || depth < 10)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                DumpGameObjectRecursive(transform.GetChild(i), sb, depth + 1);
            }
        }
        else if (transform.childCount > 0)
        {
            sb.AppendLine($"{indent}  ... ({transform.childCount} children - depth limit reached)");
        }
    }

    private static string GetFullPath(Transform transform)
    {
        if (transform.parent == null)
            return transform.name;
        return GetFullPath(transform.parent) + "/" + transform.name;
    }

    private static GameObject[] GetChildrenMatching(GameObject parent, string pattern)
    {
        Debug.Log($"Searching for children matching pattern: {pattern}");
        
        var regex = new Regex(pattern);
        var matchingChildren = new List<GameObject>();

        var childCount = parent.transform.childCount;
        Debug.Log($"Parent '{parent.name}' has {childCount} direct children");

        for (int i = 0; i < childCount; i++)
        {
            var child = parent.transform.GetChild(i).gameObject;
            Debug.Log($"Checking child: {child.name}");
            
            if (regex.IsMatch(child.name))
            {
                matchingChildren.Add(child);
                Debug.Log($"  ✓ Match found: {child.name}");
            }
            else
            {
                Debug.Log($"  ✗ No match: {child.name}");
            }
        }

        Debug.Log($"Found {matchingChildren.Count} matching children");
        return matchingChildren.ToArray();
    }

    private static bool CheckForPhysBone(GameObject[] gameObjects)
    {
        Debug.Log("Checking for VRCPhysBone components...");
        
        bool allHavePhysBone = true;
        foreach (GameObject gameObject in gameObjects)
        {
            var physBone = gameObject.GetComponent<VRCPhysBone>();
            if (physBone == null)
            {
                Debug.LogError($"Missing VRCPhysBone on: {gameObject.name}");
                allHavePhysBone = false;
            }
            else
            {
                Debug.Log($"✓ VRCPhysBone found on: {gameObject.name}");
            }
        }
        
        return allHavePhysBone;
    }

    private static string GetBoneNames(GameObject[] bones)
    {
        if (bones.Length == 0)
        {
            return "No bones found.";
        }

        var builder = new StringBuilder();
        builder.Append(bones.Length);
        builder.Append(" bones found: ");

        for (int i = 0; i < bones.Length; i++)
        {
            builder.Append('\'');
            builder.Append(bones[i].name);
            builder.Append('\'');
            if (i < bones.Length - 1)
            {
                builder.Append(", ");
            }
        }

        return builder.ToString();
    }

    private static void ConfigurePhysBones(GameObject[] bones)
    {
        Debug.Log("Configuring PhysBone settings...");
        
        foreach (var bone in bones)
        {
            var physBone = bone.GetComponent<VRCPhysBone>();
            if (physBone == null) continue;

            Debug.Log($"Configuring PhysBone on: {bone.name}");

            // Set limit type to Hinge
            physBone.limitType = VRC.Dynamics.VRCPhysBoneBase.LimitType.Hinge;
            Debug.Log($"  Set limitType to Hinge");

            // Set max angle
            physBone.maxAngleX = angle;
            Debug.Log($"  Set maxAngleX to {angle}");

            // Set limit rotation X
            physBone.limitRotation = new Vector3(angle - innerAngle, physBone.limitRotation.y, physBone.limitRotation.z);
            Debug.Log($"  Set limitRotation.x to {angle - innerAngle}");

            // Calculate and set roll angle (Y rotation)
            var relativePosition = GetRelativePositionToLeaf(bone);
            var roll = Mathf.Atan2(relativePosition.z, relativePosition.x) * Mathf.Rad2Deg + 90;
            
            physBone.limitRotation = new Vector3(physBone.limitRotation.x, roll, physBone.limitRotation.z);
            Debug.Log($"  Calculated roll angle: {roll:F1}° (from position {relativePosition})");
            Debug.Log($"  Set limitRotation.y to {roll}");

            // Mark as dirty for saving
            EditorUtility.SetDirty(physBone);
        }
        
        Debug.Log($"Configured {bones.Length} PhysBone components");
    }

    private static Vector3 GetRelativePositionToLeaf(GameObject root)
    {
        var leaf = FindLeaf(root);
        if (leaf == null)
        {
            Debug.LogWarning($"Could not find leaf for {root.name}, using zero vector");
            return Vector3.zero;
        }
        
        Vector3 relativePos = leaf.transform.position - root.transform.position;
        Debug.Log($"Relative position from {root.name} to leaf {leaf.name}: {relativePos}");
        return relativePos;
    }

    private static GameObject FindLeaf(GameObject root)
    {
        if (root.transform.childCount == 0)
        {
            return root;
        }
        else if (root.transform.childCount == 1)
        {
            GameObject child = root.transform.GetChild(0).gameObject;
            return FindLeaf(child);
        }
        else
        {
            Debug.LogError($"GameObject {root.name} has multiple children - cannot determine single leaf path");
            return null;
        }
    }

    private static void SaveAssets()
    {
        Debug.Log("Saving assets...");
        try
        {
            AssetDatabase.SaveAssets();
            Debug.Log("Assets saved successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save assets: {ex.Message}");
            throw;
        }
    }

    [MenuItem("Tools/PhysBone Validator")]
    public static void ValidatePhysBones()
    {
        ValidatePhysBonesInScene();
    }

    public static void ValidatePhysBonesInScene()
    {
        Debug.Log("=== PhysBone Validation Report ===");
        
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        
        int totalPhysBones = 0;
        int totalGameObjects = 0;
        StringBuilder report = new StringBuilder();
        
        report.AppendLine($"Scene: {activeScene.name}");
        report.AppendLine($"Validation Time: {DateTime.Now}");
        report.AppendLine();
        report.AppendLine("=== PhysBone Components Found ===");
        
        foreach (GameObject rootObj in rootObjects)
        {
            ValidateGameObjectRecursive(rootObj, report, ref totalPhysBones, ref totalGameObjects);
        }
        
        report.AppendLine();
        report.AppendLine($"=== Summary ===");
        report.AppendLine($"Total GameObjects scanned: {totalGameObjects}");
        report.AppendLine($"Total VRCPhysBone components: {totalPhysBones}");
        
        string reportContent = report.ToString();
        Debug.Log(reportContent);
        
        string outputDir = System.IO.Path.Combine(Application.dataPath, "..", "data", "output");
        System.IO.Directory.CreateDirectory(outputDir);
        string reportPath = System.IO.Path.Combine(outputDir, "physbone_validation_report.txt");
        System.IO.File.WriteAllText(reportPath, reportContent);
        Debug.Log($"Validation report written to: {reportPath}");
    }

    private static void ValidateGameObjectRecursive(GameObject obj, StringBuilder report, ref int physBoneCount, ref int gameObjectCount)
    {
        gameObjectCount++;
        
        VRCPhysBone physBone = obj.GetComponent<VRCPhysBone>();
        if (physBone != null)
        {
            physBoneCount++;
            string fullPath = GetFullPath(obj.transform);
            
            report.AppendLine($"PhysBone #{physBoneCount}:");
            report.AppendLine($"  GameObject: {obj.name}");
            report.AppendLine($"  Full Path: {fullPath}");
            report.AppendLine($"  Limit Type: {physBone.limitType}");
            report.AppendLine($"  Max Angle X: {physBone.maxAngleX}°");
            report.AppendLine($"  Limit Rotation: {physBone.limitRotation}");
            
            if (physBone.limitType == VRC.Dynamics.VRCPhysBoneBase.LimitType.Hinge)
            {
                report.AppendLine($"  ✓ Configured as Hinge");
            }
            else
            {
                report.AppendLine($"  ⚠ Not configured as Hinge");
            }
            
            if (physBone.maxAngleX > 0 && physBone.maxAngleX <= 180)
            {
                report.AppendLine($"  ✓ MaxAngleX in reasonable range");
            }
            else
            {
                report.AppendLine($"  ⚠ MaxAngleX may be out of range: {physBone.maxAngleX}°");
            }
            
            report.AppendLine();
        }
        
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            ValidateGameObjectRecursive(obj.transform.GetChild(i).gameObject, report, ref physBoneCount, ref gameObjectCount);
        }
    }

    private static void FindAndAnalyzeArmature()
    {
        Debug.Log("=== Starting Armature Analysis ===");
        
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        
        StringBuilder armatureReport = new StringBuilder();
        armatureReport.AppendLine($"=== Armature Structure Analysis ===");
        armatureReport.AppendLine($"Scene: {activeScene.name}");
        armatureReport.AppendLine($"Generated: {DateTime.Now}");
        armatureReport.AppendLine();
        
        List<Transform> potentialArmatures = new List<Transform>();
        List<Transform> allBones = new List<Transform>();
        
        // Find potential armatures and bones
        foreach (GameObject rootObj in rootObjects)
        {
            FindArmaturesAndBonesRecursive(rootObj.transform, potentialArmatures, allBones, armatureReport);
        }
        
        armatureReport.AppendLine($"=== Summary ===");
        armatureReport.AppendLine($"Potential Armatures Found: {potentialArmatures.Count}");
        armatureReport.AppendLine($"Total Bones Found: {allBones.Count}");
        armatureReport.AppendLine();
        
        // Analyze each potential armature
        for (int i = 0; i < potentialArmatures.Count; i++)
        {
            AnalyzeSingleArmature(potentialArmatures[i], armatureReport, i + 1);
        }
        
        string outputDir = System.IO.Path.Combine(Application.dataPath, "..", "data", "output");
        System.IO.Directory.CreateDirectory(outputDir);
        string armaturePath = System.IO.Path.Combine(outputDir, "armature_bones.txt");
        System.IO.File.WriteAllText(armaturePath, armatureReport.ToString());
        
        Debug.Log($"Armature analysis written to: {armaturePath}");
        Debug.Log($"Found {potentialArmatures.Count} potential armatures with {allBones.Count} total bones");
    }
    
    private static void FindArmaturesAndBonesRecursive(Transform transform, List<Transform> armatures, List<Transform> bones, StringBuilder report)
    {
        string name = transform.name.ToLower();
        
        // Check if this looks like an armature root
        bool isArmatureCandidate = name.Contains("armature") || name.Contains("skeleton") || 
                                  name.Contains("rig") || name.Contains("bones") ||
                                  (name.Contains("root") && transform.childCount > 0);
        
        // Check if this looks like a bone
        bool isBoneCandidate = name.Contains("hip") || name.Contains("spine") || name.Contains("chest") ||
                              name.Contains("neck") || name.Contains("head") || name.Contains("shoulder") ||
                              name.Contains("arm") || name.Contains("leg") || name.Contains("thigh") ||
                              name.Contains("knee") || name.Contains("foot") || name.Contains("hand") ||
                              name.Contains("finger") || name.Contains("thumb") || name.Contains("toe") ||
                              name.Contains("bone") || name.Contains("joint");
        
        if (isArmatureCandidate)
        {
            armatures.Add(transform);
            report.AppendLine($"Potential Armature: {GetFullPath(transform)}");
        }
        
        if (isBoneCandidate)
        {
            bones.Add(transform);
            report.AppendLine($"Bone Found: {GetFullPath(transform)}");
        }
        
        // Recurse through children
        for (int i = 0; i < transform.childCount; i++)
        {
            FindArmaturesAndBonesRecursive(transform.GetChild(i), armatures, bones, report);
        }
    }
    
    private static void AnalyzeSingleArmature(Transform armature, StringBuilder report, int armatureNum)
    {
        report.AppendLine($"=== Armature #{armatureNum}: {armature.name} ===");
        report.AppendLine($"Full Path: {GetFullPath(armature)}");
        report.AppendLine($"Child Count: {armature.childCount}");
        report.AppendLine();
        
        // Find common bone names in hierarchy
        List<Transform> foundBones = new List<Transform>();
        string[] commonBoneNames = { "hip", "spine", "chest", "neck", "head", "shoulder", "arm", "leg", "thigh", "knee", "foot" };
        
        foreach (string boneName in commonBoneNames)
        {
            Transform bone = FindBoneByName(armature, boneName);
            if (bone != null)
            {
                foundBones.Add(bone);
                report.AppendLine($"  {boneName.ToUpper()}: {GetFullPath(bone)}");
            }
        }
        
        if (foundBones.Count > 0)
        {
            report.AppendLine($"  ✓ Found {foundBones.Count} common bones");
        }
        else
        {
            report.AppendLine($"  ⚠ No common bone names found");
        }
        
        report.AppendLine();
        
        // Show first few levels of hierarchy
        report.AppendLine($"Bone Hierarchy (first 3 levels):");
        DumpArmatureHierarchy(armature, report, 0, 3);
        report.AppendLine();
    }
    
    private static Transform FindBoneByName(Transform parent, string boneName)
    {
        if (parent.name.ToLower().Contains(boneName.ToLower()))
        {
            return parent;
        }
        
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindBoneByName(parent.GetChild(i), boneName);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    private static void DumpArmatureHierarchy(Transform transform, StringBuilder report, int depth, int maxDepth)
    {
        if (depth > maxDepth) return;
        
        string indent = new string(' ', depth * 2);
        report.AppendLine($"{indent}{transform.name}");
        
        for (int i = 0; i < transform.childCount; i++)
        {
            DumpArmatureHierarchy(transform.GetChild(i), report, depth + 1, maxDepth);
        }
    }

    private static void ExitWithCode(int exitCode)
    {
        Debug.Log($"Exiting with code: {exitCode}");
        
        string exitMessage = exitCode switch
        {
            0 => "Success - PhysBone configuration completed",
            1 => "Warning - No skirtRoot specified, hierarchy dump only",
            2 => "Error - Specified node not found or no matching bones",
            3 => "Error - Missing PhysBone components",
            4 => "Error - Unexpected error occurred",
            _ => "Unknown exit code"
        };
        
        Debug.Log($"Exit reason: {exitMessage}");
        
        // In headless mode, Application.Quit() will terminate Unity
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(exitCode);
        }
    }
}