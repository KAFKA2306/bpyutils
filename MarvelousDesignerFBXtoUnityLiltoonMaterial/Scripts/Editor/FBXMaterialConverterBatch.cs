using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FBXMaterialConverter
{
    public class FBXMaterialConverterBatch
    {
        private static ConversionConfig config;
        private static ConversionLogger logger;
        private static FBXScanner scanner;
        private static MaterialConverter converter;
        private static MaterialBackupSystem backupSystem;
        
        [MenuItem("Tools/Convert FBX Materials to lilToon (Batch)")]
        public static void ConvertAllFBXMaterialsBatch()
        {
            try
            {
                InitializeComponents();
                ExecuteBatchConversion();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Batch conversion failed: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }
        
        public static void ConvertAllFBXMaterials()
        {
            try
            {
                Debug.Log("Starting headless FBX material conversion...");
                
                InitializeComponents();
                
                if (!ValidatePrerequisites())
                {
                    EditorApplication.Exit(1);
                    return;
                }
                
                var fbxFiles = scanner.ScanForFBXFiles();
                
                if (fbxFiles.Count == 0)
                {
                    logger.Log(ConversionConfig.LogLevel.Warning, "No FBX files found to process");
                    FinalizeBatchProcess(true);
                    return;
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Found {fbxFiles.Count} FBX files to process");
                
                bool success = ProcessFBXFiles(fbxFiles);
                FinalizeBatchProcess(success);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Critical error in batch conversion: {ex.Message}");
                if (logger != null)
                {
                    logger.Log(ConversionConfig.LogLevel.Error, "Critical error in batch conversion", ex);
                    logger.FinishLogging();
                }
                EditorApplication.Exit(1);
            }
        }
        
        private static void InitializeComponents()
        {
            try
            {
                string configPath = GetConfigPath();
                config = ConversionConfig.LoadFromFile(configPath);
                
                logger = new ConversionLogger(config);
                scanner = new FBXScanner(config, logger);
                converter = new MaterialConverter(config, logger, scanner);
                backupSystem = new MaterialBackupSystem(config, logger);
                
                logger.Log(ConversionConfig.LogLevel.Info, "Batch conversion components initialized");
                logger.Log(ConversionConfig.LogLevel.Info, $"Config loaded from: {configPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize components: {ex.Message}");
                throw;
            }
        }
        
        private static string GetConfigPath()
        {
            string[] possiblePaths = {
                Path.Combine(Application.dataPath, "../Config/ConversionConfig.json"),
                Path.Combine(Application.dataPath, "Config/ConversionConfig.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "Config/ConversionConfig.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "ConversionConfig.json")
            };
            
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }
            
            Debug.LogWarning("Config file not found, using default configuration");
            return string.Empty;
        }
        
        private static bool ValidatePrerequisites()
        {
            try
            {
                if (!converter.InitializeLilToonShader())
                {
                    logger.Log(ConversionConfig.LogLevel.Error, "lilToon shader not found - cannot proceed with conversion");
                    return false;
                }
                
                if (!Directory.Exists(config.targetFolder))
                {
                    logger.Log(ConversionConfig.LogLevel.Error, $"Target folder does not exist: {config.targetFolder}");
                    return false;
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, "Prerequisites validation passed");
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, "Prerequisites validation failed", ex);
                return false;
            }
        }
        
        private static bool ProcessFBXFiles(List<string> fbxFiles)
        {
            try
            {
                int totalFiles = fbxFiles.Count;
                int processedFiles = 0;
                int failedFiles = 0;
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Starting to process {totalFiles} FBX files");
                
                foreach (var fbxPath in fbxFiles)
                {
                    try
                    {
                        logger.Log(ConversionConfig.LogLevel.Info, $"Processing file {processedFiles + 1}/{totalFiles}: {Path.GetFileName(fbxPath)}");
                        
                        bool fileSuccess = ProcessSingleFBX(fbxPath);
                        
                        if (fileSuccess)
                        {
                            logger.LogProcessedFile(Path.GetFileName(fbxPath));
                            processedFiles++;
                        }
                        else
                        {
                            failedFiles++;
                            logger.Log(ConversionConfig.LogLevel.Warning, $"Failed to process file: {Path.GetFileName(fbxPath)}");
                        }
                        
                        DisplayProgress(processedFiles + failedFiles, totalFiles);
                    }
                    catch (Exception ex)
                    {
                        failedFiles++;
                        logger.Log(ConversionConfig.LogLevel.Error, $"Error processing file {Path.GetFileName(fbxPath)}", ex);
                    }
                }
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Batch processing completed: {processedFiles} successful, {failedFiles} failed");
                
                float successRate = (float)processedFiles / totalFiles;
                return successRate >= 0.95f;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, "Error in batch processing", ex);
                return false;
            }
        }
        
        private static bool ProcessSingleFBX(string fbxPath)
        {
            try
            {
                var materials = scanner.ExtractMaterialsFromFBX(fbxPath);
                
                if (materials.Count == 0)
                {
                    logger.Log(ConversionConfig.LogLevel.Debug, $"No materials found in {Path.GetFileName(fbxPath)}");
                    return true;
                }
                
                int convertedCount = 0;
                
                foreach (var material in materials)
                {
                    if (ShouldConvertMaterial(material))
                    {
                        try
                        {
                            backupSystem.BackupMaterial(material, fbxPath);
                            
                            var convertedMaterial = converter.ConvertMaterial(material);
                            if (convertedMaterial != null)
                            {
                                EditorUtility.CopySerialized(convertedMaterial, material);
                                logger.LogConvertedMaterial(material.name);
                                convertedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Log(ConversionConfig.LogLevel.Warning, $"Failed to convert material {material.name}", ex);
                        }
                    }
                }
                
                logger.Log(ConversionConfig.LogLevel.Debug, $"Converted {convertedCount}/{materials.Count} materials in {Path.GetFileName(fbxPath)}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error processing FBX file {Path.GetFileName(fbxPath)}", ex);
                return false;
            }
        }
        
        private static bool ShouldConvertMaterial(Material material)
        {
            if (material == null) return false;
            
            if (material.shader.name.Contains("lilToon"))
            {
                logger.Log(ConversionConfig.LogLevel.Debug, $"Material {material.name} already uses lilToon shader");
                return false;
            }
            
            if (!material.shader.name.Contains("Standard"))
            {
                logger.Log(ConversionConfig.LogLevel.Debug, $"Material {material.name} does not use Standard shader: {material.shader.name}");
                return false;
            }
            
            return true;
        }
        
        private static void DisplayProgress(int current, int total)
        {
            float progress = (float)current / total;
            if (EditorUtility.DisplayCancelableProgressBar("Converting FBX Materials", 
                $"Processing file {current}/{total}", progress))
            {
                EditorUtility.ClearProgressBar();
                throw new OperationCanceledException("Conversion cancelled by user");
            }
        }
        
        private static void FinalizeBatchProcess(bool success)
        {
            try
            {
                EditorUtility.ClearProgressBar();
                
                if (logger != null)
                {
                    logger.FinishLogging();
                    
                    var stats = logger.GetStats();
                    LogFinalStatistics(stats);
                    
                    OutputResultsToConsole(stats, success);
                }
                
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(success ? 0 : 1);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error finalizing batch process: {ex.Message}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
        
        private static void LogFinalStatistics(ConversionStats stats)
        {
            logger.Log(ConversionConfig.LogLevel.Info, "=== FINAL CONVERSION STATISTICS ===");
            logger.Log(ConversionConfig.LogLevel.Info, $"Total Processed Files: {stats.processedFiles}");
            logger.Log(ConversionConfig.LogLevel.Info, $"Total Converted Materials: {stats.convertedMaterials}");
            logger.Log(ConversionConfig.LogLevel.Info, $"Total Transparency Applied: {stats.transparencyApplied}");
            logger.Log(ConversionConfig.LogLevel.Info, $"Total Errors: {stats.errors}");
            logger.Log(ConversionConfig.LogLevel.Info, $"Total Warnings: {stats.warnings}");
            logger.Log(ConversionConfig.LogLevel.Info, $"Total Processing Time: {stats.processingTime}");
            
            float successRate = stats.processedFiles > 0 ? 
                (float)(stats.processedFiles - stats.errors) / stats.processedFiles * 100 : 100;
            logger.Log(ConversionConfig.LogLevel.Info, $"Success Rate: {successRate:F1}%");
            logger.Log(ConversionConfig.LogLevel.Info, "===================================");
        }
        
        private static void OutputResultsToConsole(ConversionStats stats, bool overallSuccess)
        {
            string resultJson = JsonUtility.ToJson(new 
            {
                processedFiles = stats.processedFiles,
                convertedMaterials = stats.convertedMaterials,
                transparencyApplied = stats.transparencyApplied,
                errors = stats.errors,
                warnings = stats.warnings,
                processingTime = stats.processingTime,
                success = overallSuccess
            }, true);
            
            Debug.Log("CONVERSION_RESULTS_JSON_START");
            Debug.Log(resultJson);
            Debug.Log("CONVERSION_RESULTS_JSON_END");
        }
        
        private static void ExecuteBatchConversion()
        {
            if (Application.isBatchMode)
            {
                ConvertAllFBXMaterials();
            }
            else
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Batch Conversion", 
                    "This will convert all FBX materials in the target folder to lilToon. Continue?", 
                    "Yes", "Cancel"
                );
                
                if (proceed)
                {
                    ConvertAllFBXMaterials();
                }
            }
        }
    }
}