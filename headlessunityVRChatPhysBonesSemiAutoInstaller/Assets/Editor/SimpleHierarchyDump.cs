using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.Text;

public class SimpleHierarchyDump
{
    private static string hierarchyOut = "hierarchy_dump.txt";
    private static bool deepHierarchy = false;
    private static bool findArmature = false;
    private static bool showBoneStructure = false;

    public static void Main()
    {
        Debug.Log("=== Simple Hierarchy Dump Started ===");
        
        try
        {
            ParseCommandLineArguments(System.Environment.GetCommandLineArgs());
            
            // Always dump hierarchy first
            DumpSceneHierarchy();
            
            // Find and analyze armature if requested
            if (findArmature)
            {
                FindAndAnalyzeArmature();
            }
            
            Debug.Log("=== Simple Hierarchy Dump completed successfully ===");
            ExitWithCode(0);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error: {ex.Message}\n{ex.StackTrace}");
            ExitWithCode(1);
        }
    }

    private static void ParseCommandLineArguments(string[] args)
    {
        Debug.Log($"Parsing {args.Length} command line arguments");
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
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
        if (deepHierarchy || depth < 15)
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
        string armaturePath = System.IO.Path.Combine(outputDir, "unity_armature_bones.txt");
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
            0 => "Success - Hierarchy dump completed",
            1 => "Error - Unexpected error occurred",
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