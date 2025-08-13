using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBXMaterialConverter
{
    [Serializable]
    public class ConversionConfig
    {
        [Header("Conversion Settings")]
        public string targetFolder = "Assets/Models/";
        public bool enableTransparency = true;
        public bool createBackups = true;
        public string targetShader = "lilToon";
        public float alphaThreshold = 0.5f;
        
        [Header("Logging")]
        public LogLevel logLevel = LogLevel.Info;
        
        [Header("File Processing")]
        public string[] fileFilters = { "*.fbx" };
        public string[] excludePatterns = { "_backup", "_temp" };
        public int maxConcurrentProcesses = 4;
        
        [Header("Material Properties")]
        public int transparencyMode = 2;
        public int renderQueue = 3000;
        
        [Header("Paths")]
        public string backupPath = "Backups/Materials/";
        public string logPath = "Logs/";
        
        [Header("Advanced Options")]
        public bool preserveOriginalTextures = true;
        public bool autoDetectTransparency = true;
        
        public enum LogLevel
        {
            Error = 0,
            Warning = 1,
            Info = 2,
            Debug = 3
        }
        
        public static ConversionConfig LoadFromFile(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    string json = System.IO.File.ReadAllText(path);
                    return JsonUtility.FromJson<ConversionConfig>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load config from {path}: {ex.Message}");
            }
            
            return new ConversionConfig();
        }
        
        public void SaveToFile(string path)
        {
            try
            {
                string directory = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }
                
                string json = JsonUtility.ToJson(this, true);
                System.IO.File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save config to {path}: {ex.Message}");
            }
        }
    }
}