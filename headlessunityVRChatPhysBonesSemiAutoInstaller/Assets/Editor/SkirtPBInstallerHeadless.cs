using UnityEditor;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using VRC.SDK3.Dynamics.PhysBone.Components;

public class SkirtPBInstallerHeadless
{
    public static void Main()
    {
        Debug.Log("=== SkirtPB Installer Headless Started ===");
        
        try
        {
            // Get command line arguments  
            string[] args = System.Environment.GetCommandLineArgs();
            
            string targetRootName = "HipRoot";
            string boneRegex = ".*\\.001"; 
            float angle = 45f;
            float innerAngle = 10f;
            
            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-skirtRoot":
                        if (i + 1 < args.Length)
                        {
                            targetRootName = args[++i];
                            Debug.Log($"Target root set to: {targetRootName}");
                        }
                        break;
                    case "-boneRegex":
                        if (i + 1 < args.Length)
                        {
                            boneRegex = args[++i];
                            Debug.Log($"Bone regex set to: {boneRegex}");
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
                }
            }
            
            // Execute SkirtPB installation with confirmed path
            InstallSkirtPBForConfirmedTarget(targetRootName, boneRegex, angle, innerAngle);
            
            Debug.Log("=== SkirtPB Installer Headless Completed Successfully ===");
            ExitWithCode(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SkirtPB Installation failed: {ex.Message}");
            ExitWithCode(1);
        }
    }
    
    private static void InstallSkirtPBForConfirmedTarget(string targetRootName, string boneRegex, float angle, float innerAngle)
    {
        Debug.Log($"Installing SkirtPB for target: {targetRootName}");
        Debug.Log($"Using regex: {boneRegex}");
        Debug.Log($"Angles: {angle}° (max), {innerAngle}° (inner)");
        
        // Find all GameObjects matching the target root name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        List<GameObject> targetRoots = new List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == targetRootName)
            {
                targetRoots.Add(obj);
                Debug.Log($"Found potential target: {GetFullPath(obj.transform)}");
            }
        }
        
        if (targetRoots.Count == 0)
        {
            Debug.LogError($"No GameObject named '{targetRootName}' found in scene.");
            return;
        }
        
        // Process each target root
        int successCount = 0;
        foreach (GameObject rootBone in targetRoots)
        {
            string fullPath = GetFullPath(rootBone.transform);
            Debug.Log($"Processing SkirtPB installation for: {fullPath}");
            
            // Check if this is the confirmed target path
            if (fullPath.Contains("2025-08-13_rigged"))
            {
                Debug.Log($"🎯 CONFIRMED TARGET: {fullPath}");
                
                if (InstallSkirtPBForRoot(rootBone, boneRegex, angle, innerAngle))
                {
                    successCount++;
                    Debug.Log($"✅ SUCCESS: SkirtPB installed for {fullPath}");
                    
                    // Generate detailed report
                    GenerateInstallationReport(rootBone, boneRegex, angle, innerAngle);
                }
                else
                {
                    Debug.LogError($"❌ FAILED: SkirtPB installation failed for {fullPath}");
                }
            }
            else
            {
                Debug.Log($"⏭ SKIPPED: {fullPath} (not confirmed target)");
            }
        }
        
        Debug.Log($"SkirtPB installation completed. Successfully processed: {successCount} targets");
    }
    
    private static bool InstallSkirtPBForRoot(GameObject rootBone, string boneRegex, float angle, float innerAngle)
    {
        try
        {
            // Get matching child bones
            GameObject[] bones = GetChildrenMatching(rootBone, boneRegex);
            
            if (bones.Length == 0)
            {
                Debug.LogWarning($"No bones found matching pattern '{boneRegex}' under {rootBone.name}");
                return false;
            }
            
            Debug.Log($"Found {bones.Length} matching bones: {GetBoneNames(bones)}");
            
            // Check for existing PhysBone components
            bool allHavePhysBone = CheckForPhysBone(bones);
            if (!allHavePhysBone)
            {
                Debug.Log("Some bones are missing VRCPhysBone components. Adding them...");
                
                // Add VRCPhysBone components to bones that don't have them
                foreach (GameObject bone in bones)
                {
                    if (bone.GetComponent<VRCPhysBone>() == null)
                    {
                        VRCPhysBone physBone = bone.AddComponent<VRCPhysBone>();
                        Debug.Log($"Added VRCPhysBone component to: {bone.name}");
                        EditorUtility.SetDirty(bone);
                    }
                }
            }
            
            // Configure PhysBone settings
            foreach (GameObject bone in bones)
            {
                ConfigurePhysBone(bone, angle, innerAngle);
            }
            
            // Save changes
            AssetDatabase.SaveAssets();
            Debug.Log("Assets saved successfully");
            
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error installing SkirtPB: {ex.Message}");
            return false;
        }
    }
    
    private static void ConfigurePhysBone(GameObject bone, float angle, float innerAngle)
    {
        VRCPhysBone physBone = bone.GetComponent<VRCPhysBone>();
        if (physBone == null)
        {
            Debug.LogError($"VRCPhysBone component not found on {bone.name}");
            return;
        }
        
        Debug.Log($"Configuring PhysBone for: {bone.name}");
        
        // Set limit type to Hinge
        physBone.limitType = VRC.Dynamics.VRCPhysBoneBase.LimitType.Hinge;
        
        // Set max angle
        physBone.maxAngleX = angle;
        
        // Set limit rotation X
        Vector3 limitRotation = physBone.limitRotation;
        limitRotation.x = angle - innerAngle;
        
        // Calculate roll angle (Y rotation) based on leaf position
        Vector3 relativePosition = GetRelativePositionToLeaf(bone);
        float roll = Mathf.Atan2(relativePosition.z, relativePosition.x) * Mathf.Rad2Deg + 90;
        limitRotation.y = roll;
        
        physBone.limitRotation = limitRotation;
        
        // Set other recommended parameters
        physBone.integrationType = VRC.Dynamics.VRCPhysBoneBase.IntegrationType.Simplified;
        physBone.immobile = 0f;
        physBone.gravity = 0f;
        physBone.inert = 0.2f;
        physBone.elasticity = 0.1f;
        physBone.stiffness = 0.2f;
        physBone.damping = 0.1f;
        physBone.radius = 0.05f;
        physBone.allowCollision = true;
        
        Debug.Log($"  Limit Type: {physBone.limitType}");
        Debug.Log($"  Max Angle X: {physBone.maxAngleX}°");
        Debug.Log($"  Limit Rotation: {physBone.limitRotation}");
        Debug.Log($"  Roll angle calculated: {roll:F1}°");
        
        EditorUtility.SetDirty(physBone);
    }
    
    private static void GenerateInstallationReport(GameObject rootBone, string boneRegex, float angle, float innerAngle)
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("=== SKIRT PHYSBONE INSTALLATION REPORT ===");
        report.AppendLine($"Generated: {System.DateTime.Now}");
        report.AppendLine($"Root Bone: {GetFullPath(rootBone.transform)}");
        report.AppendLine($"Bone Regex: {boneRegex}");
        report.AppendLine($"Max Angle: {angle}°");
        report.AppendLine($"Inner Angle: {innerAngle}°");
        report.AppendLine();
        
        GameObject[] bones = GetChildrenMatching(rootBone, boneRegex);
        report.AppendLine($"Configured {bones.Length} PhysBone components:");
        
        foreach (GameObject bone in bones)
        {
            VRCPhysBone physBone = bone.GetComponent<VRCPhysBone>();
            if (physBone != null)
            {
                report.AppendLine($"  - {bone.name}:");
                report.AppendLine($"    Full Path: {GetFullPath(bone.transform)}");
                report.AppendLine($"    Limit Type: {physBone.limitType}");
                report.AppendLine($"    Max Angle X: {physBone.maxAngleX}°");
                report.AppendLine($"    Limit Rotation: {physBone.limitRotation}");
                report.AppendLine($"    Integration Type: {physBone.integrationType}");
                report.AppendLine();
            }
        }
        
        string outputDir = System.IO.Path.Combine(Application.dataPath, "..", "data", "output");
        System.IO.Directory.CreateDirectory(outputDir);
        string reportPath = System.IO.Path.Combine(outputDir, "physbone_installation_report.txt");
        System.IO.File.WriteAllText(reportPath, report.ToString());
        
        Debug.Log($"Installation report written to: {reportPath}");
    }
    
    // Utility methods from original SkirtPBInstaller
    private static bool CheckForPhysBone(GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject.GetComponent<VRCPhysBone>() == null)
            {
                return false;
            }
        }
        return true;
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
    
    private static GameObject[] GetChildrenMatching(GameObject parent, string pattern)
    {
        var regex = new Regex(pattern);
        var matchingChildren = new List<GameObject>();
        
        var childCount = parent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = parent.transform.GetChild(i).gameObject;
            if (regex.IsMatch(child.name))
            {
                matchingChildren.Add(child);
            }
        }
        
        return matchingChildren.ToArray();
    }
    
    private static Vector3 GetRelativePositionToLeaf(GameObject root)
    {
        GameObject leaf = FindLeaf(root);
        if (leaf == null)
        {
            Debug.LogWarning($"Could not find leaf for {root.name}, using zero vector");
            return Vector3.zero;
        }
        
        return leaf.transform.position - root.transform.position;
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
    
    private static string GetFullPath(Transform transform)
    {
        if (transform.parent == null)
            return transform.name;
        return GetFullPath(transform.parent) + "/" + transform.name;
    }
    
    private static void ExitWithCode(int exitCode)
    {
        Debug.Log($"Exiting SkirtPB Installer with code: {exitCode}");
        
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(exitCode);
        }
    }
}