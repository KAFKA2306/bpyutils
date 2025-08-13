using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FBXMaterialConverter
{
    public class FBXMaterialConverterEditor : EditorWindow
    {
        private ConversionConfig config;
        private ConversionLogger logger;
        private FBXScanner scanner;
        private MaterialConverter converter;
        private MaterialBackupSystem backupSystem;
        
        private Vector2 scrollPosition;
        private bool isProcessing = false;
        private float processingProgress = 0f;
        private string processingStatus = "";
        
        private bool showAdvancedOptions = false;
        private bool showLogs = false;
        private List<string> recentLogs = new List<string>();
        
        [MenuItem("Tools/Convert FBX Materials to lilToon")]
        public static void ShowWindow()
        {
            var window = GetWindow<FBXMaterialConverterEditor>("FBX Material Converter");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }
        
        private void OnEnable()
        {
            InitializeComponents();
        }
        
        private void InitializeComponents()
        {
            try
            {
                string configPath = Path.Combine(Application.dataPath, "../Config/ConversionConfig.json");
                config = ConversionConfig.LoadFromFile(configPath);
                
                logger = new ConversionLogger(config);
                scanner = new FBXScanner(config, logger);
                converter = new MaterialConverter(config, logger, scanner);
                backupSystem = new MaterialBackupSystem(config, logger);
                
                logger.Log(ConversionConfig.LogLevel.Info, "FBX Material Converter Editor initialized");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error initializing FBX Material Converter: {ex.Message}");
            }
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            DrawHeader();
            DrawBasicSettings();
            DrawAdvancedOptions();
            DrawConversionControls();
            DrawProgressBar();
            DrawLogs();
            DrawStatistics();
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("FBX Material Converter", EditorStyles.largeLabel);
            EditorGUILayout.LabelField("Convert Standard Shader materials to lilToon", EditorStyles.helpBox);
            EditorGUILayout.Space(10);
        }
        
        private void DrawBasicSettings()
        {
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            
            config.targetFolder = EditorGUILayout.TextField("Target Folder", config.targetFolder);
            config.targetShader = EditorGUILayout.TextField("Target Shader", config.targetShader);
            config.enableTransparency = EditorGUILayout.Toggle("Enable Transparency", config.enableTransparency);
            config.createBackups = EditorGUILayout.Toggle("Create Backups", config.createBackups);
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawAdvancedOptions()
        {
            showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "Advanced Options");
            
            if (showAdvancedOptions)
            {
                EditorGUI.indentLevel++;
                
                config.alphaThreshold = EditorGUILayout.Slider("Alpha Threshold", config.alphaThreshold, 0f, 1f);
                config.transparencyMode = EditorGUILayout.IntSlider("Transparency Mode", config.transparencyMode, 0, 3);
                config.renderQueue = EditorGUILayout.IntField("Render Queue", config.renderQueue);
                config.maxConcurrentProcesses = EditorGUILayout.IntSlider("Max Concurrent Processes", config.maxConcurrentProcesses, 1, 8);
                config.autoDetectTransparency = EditorGUILayout.Toggle("Auto Detect Transparency", config.autoDetectTransparency);
                config.preserveOriginalTextures = EditorGUILayout.Toggle("Preserve Original Textures", config.preserveOriginalTextures);
                
                config.logLevel = (ConversionConfig.LogLevel)EditorGUILayout.EnumPopup("Log Level", config.logLevel);
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }
        
        private void DrawConversionControls()
        {
            EditorGUILayout.LabelField("Conversion Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = !isProcessing;
            if (GUILayout.Button("Convert All FBX Materials", GUILayout.Height(30)))
            {
                StartConversion();
            }
            
            if (GUILayout.Button("Scan FBX Files", GUILayout.Width(120)))
            {
                ScanFBXFiles();
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Restore All Backups"))
            {
                RestoreAllBackups();
            }
            
            if (GUILayout.Button("Cleanup Old Backups"))
            {
                CleanupOldBackups();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawProgressBar()
        {
            if (isProcessing)
            {
                EditorGUILayout.LabelField("Processing Progress", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(processingStatus);
                
                Rect progressRect = EditorGUILayout.GetControlRect();
                EditorGUI.ProgressBar(progressRect, processingProgress, $"{(processingProgress * 100):F1}%");
                
                EditorGUILayout.Space(5);
            }
        }
        
        private void DrawLogs()
        {
            showLogs = EditorGUILayout.Foldout(showLogs, $"Recent Logs ({recentLogs.Count})");
            
            if (showLogs)
            {
                EditorGUI.indentLevel++;
                
                if (recentLogs.Count == 0)
                {
                    EditorGUILayout.LabelField("No recent logs", EditorStyles.helpBox);
                }
                else
                {
                    for (int i = Math.Max(0, recentLogs.Count - 10); i < recentLogs.Count; i++)
                    {
                        EditorGUILayout.LabelField(recentLogs[i], EditorStyles.wordWrappedLabel);
                    }
                }
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear Logs"))
                {
                    recentLogs.Clear();
                }
                if (GUILayout.Button("Export Logs to CSV"))
                {
                    ExportLogsToCSV();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }
        
        private void DrawStatistics()
        {
            if (logger != null)
            {
                var stats = logger.GetStats();
                
                EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Processed Files: {stats.processedFiles}");
                EditorGUILayout.LabelField($"Converted Materials: {stats.convertedMaterials}");
                EditorGUILayout.LabelField($"Transparency Applied: {stats.transparencyApplied}");
                EditorGUILayout.LabelField($"Errors: {stats.errors}");
                EditorGUILayout.LabelField($"Warnings: {stats.warnings}");
                
                if (!string.IsNullOrEmpty(stats.processingTime))
                {
                    EditorGUILayout.LabelField($"Processing Time: {stats.processingTime}");
                }
            }
        }
        
        private void StartConversion()
        {
            try
            {
                isProcessing = true;
                processingProgress = 0f;
                processingStatus = "Initializing conversion...";
                
                EditorApplication.update += UpdateProgress;
                
                var fbxFiles = scanner.ScanForFBXFiles();
                
                if (fbxFiles.Count == 0)
                {
                    AddRecentLog("No FBX files found to process");
                    isProcessing = false;
                    return;
                }
                
                ConvertMaterials(fbxFiles);
            }
            catch (Exception ex)
            {
                AddRecentLog($"Error starting conversion: {ex.Message}");
                isProcessing = false;
            }
        }
        
        private void ConvertMaterials(List<string> fbxFiles)
        {
            try
            {
                if (!converter.InitializeLilToonShader())
                {
                    AddRecentLog("Failed to initialize lilToon shader");
                    isProcessing = false;
                    return;
                }
                
                int totalFiles = fbxFiles.Count;
                int processedFiles = 0;
                
                foreach (var fbxPath in fbxFiles)
                {
                    processingStatus = $"Processing: {Path.GetFileName(fbxPath)}";
                    processingProgress = (float)processedFiles / totalFiles;
                    
                    var materials = scanner.ExtractMaterialsFromFBX(fbxPath);
                    
                    foreach (var material in materials)
                    {
                        if (material.shader.name.Contains("Standard"))
                        {
                            backupSystem.BackupMaterial(material, fbxPath);
                            
                            var convertedMaterial = converter.ConvertMaterial(material);
                            if (convertedMaterial != null)
                            {
                                EditorUtility.CopySerialized(convertedMaterial, material);
                                logger.LogConvertedMaterial(material.name);
                            }
                        }
                    }
                    
                    logger.LogProcessedFile(Path.GetFileName(fbxPath));
                    processedFiles++;
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                logger.FinishLogging();
                AddRecentLog($"Conversion completed! Processed {processedFiles} files");
                
                isProcessing = false;
                EditorApplication.update -= UpdateProgress;
            }
            catch (Exception ex)
            {
                AddRecentLog($"Error during conversion: {ex.Message}");
                isProcessing = false;
                EditorApplication.update -= UpdateProgress;
            }
        }
        
        private void ScanFBXFiles()
        {
            try
            {
                var fbxFiles = scanner.ScanForFBXFiles();
                AddRecentLog($"Scan completed: Found {fbxFiles.Count} FBX files");
                
                foreach (var file in fbxFiles)
                {
                    AddRecentLog($"  - {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                AddRecentLog($"Error scanning FBX files: {ex.Message}");
            }
        }
        
        private void RestoreAllBackups()
        {
            try
            {
                bool success = backupSystem.RestoreAllMaterials();
                AddRecentLog(success ? "All backups restored successfully" : "Some backups failed to restore");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                AddRecentLog($"Error restoring backups: {ex.Message}");
            }
        }
        
        private void CleanupOldBackups()
        {
            try
            {
                backupSystem.CleanupOldBackups(30);
                AddRecentLog("Old backups cleaned up");
            }
            catch (Exception ex)
            {
                AddRecentLog($"Error cleaning up backups: {ex.Message}");
            }
        }
        
        private void ExportLogsToCSV()
        {
            try
            {
                string path = EditorUtility.SaveFilePanel("Export Logs", "", "conversion_logs.csv", "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    logger.ExportLogsToCSV(path);
                    AddRecentLog($"Logs exported to: {path}");
                }
            }
            catch (Exception ex)
            {
                AddRecentLog($"Error exporting logs: {ex.Message}");
            }
        }
        
        private void UpdateProgress()
        {
            Repaint();
        }
        
        private void AddRecentLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            recentLogs.Add($"[{timestamp}] {message}");
            
            if (recentLogs.Count > 50)
            {
                recentLogs.RemoveAt(0);
            }
        }
        
        private void OnDestroy()
        {
            if (isProcessing)
            {
                EditorApplication.update -= UpdateProgress;
            }
            
            if (config != null)
            {
                string configPath = Path.Combine(Application.dataPath, "../Config/ConversionConfig.json");
                config.SaveToFile(configPath);
            }
        }
    }
}