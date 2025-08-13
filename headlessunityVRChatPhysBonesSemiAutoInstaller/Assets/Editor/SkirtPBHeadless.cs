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
        
        string hierarchyPath = System.IO.Path.Combine(Application.dataPath, "..", hierarchyOut);
        System.IO.File.WriteAllText(hierarchyPath, hierarchyDump.ToString());
        
        Debug.Log($"Hierarchy dump written to: {hierarchyPath}");
        Debug.Log($"Total objects processed: {hierarchyDump.ToString().Split('\n').Length - 4}");
    }

    private static void DumpGameObjectRecursive(Transform transform, StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);
        string fullPath = GetFullPath(transform);
        sb.AppendLine($"{indent}{transform.name} | {fullPath}");
        
        for (int i = 0; i < transform.childCount; i++)
        {
            DumpGameObjectRecursive(transform.GetChild(i), sb, depth + 1);
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