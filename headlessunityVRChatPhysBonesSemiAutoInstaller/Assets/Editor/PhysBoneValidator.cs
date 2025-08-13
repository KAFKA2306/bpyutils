using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using VRC.SDK3.Dynamics.PhysBone.Components;

public class PhysBoneValidator
{
    [MenuItem("Tools/PhysBone Validator")]
    public static void ValidatePhysBones()
    {
        Main(new string[0]);
    }

    public static void Main(string[] args)
    {
        Debug.Log("=== PhysBone Validation Report ===");
        
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();
        
        int totalPhysBones = 0;
        int totalGameObjects = 0;
        StringBuilder report = new StringBuilder();
        
        report.AppendLine($"Scene: {activeScene.name}");
        report.AppendLine($"Validation Time: {System.DateTime.Now}");
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
        
        // Write report to file
        string reportPath = System.IO.Path.Combine(Application.dataPath, "..", "physbone_validation_report.txt");
        System.IO.File.WriteAllText(reportPath, reportContent);
        Debug.Log($"Validation report written to: {reportPath}");
        
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
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
            
            // Check for reasonable angle values
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
        
        // Recurse through children
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            ValidateGameObjectRecursive(obj.transform.GetChild(i).gameObject, report, ref physBoneCount, ref gameObjectCount);
        }
    }
    
    private static string GetFullPath(Transform transform)
    {
        if (transform.parent == null)
            return transform.name;
        return GetFullPath(transform.parent) + "/" + transform.name;
    }
}