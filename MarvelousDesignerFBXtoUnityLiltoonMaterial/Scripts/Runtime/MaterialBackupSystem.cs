using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FBXMaterialConverter
{
    [Serializable]
    public class BackupEntry
    {
        public string originalMaterialPath;
        public string backupMaterialPath;
        public string fbxPath;
        public string timestamp;
        public string materialName;
        
        public BackupEntry(string originalPath, string backupPath, string fbxPath, string materialName)
        {
            this.originalMaterialPath = originalPath;
            this.backupMaterialPath = backupPath;
            this.fbxPath = fbxPath;
            this.materialName = materialName;
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    [Serializable]
    public class BackupManifest
    {
        public List<BackupEntry> entries = new List<BackupEntry>();
        public string createdAt;
        public string version = "1.0";
        
        public BackupManifest()
        {
            createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    public class MaterialBackupSystem
    {
        private readonly ConversionConfig config;
        private readonly ConversionLogger logger;
        private BackupManifest manifest;
        private string manifestPath;
        
        public MaterialBackupSystem(ConversionConfig config, ConversionLogger logger)
        {
            this.config = config;
            this.logger = logger;
            
            InitializeBackupSystem();
        }
        
        private void InitializeBackupSystem()
        {
            try
            {
                if (!Directory.Exists(config.backupPath))
                {
                    Directory.CreateDirectory(config.backupPath);
                    logger.Log(ConversionConfig.LogLevel.Info, $"Created backup directory: {config.backupPath}");
                }
                
                manifestPath = Path.Combine(config.backupPath, "backup_manifest.json");
                LoadManifest();
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error initializing backup system: {ex.Message}");
            }
        }
        
        private void LoadManifest()
        {
            try
            {
                if (File.Exists(manifestPath))
                {
                    string json = File.ReadAllText(manifestPath);
                    manifest = JsonUtility.FromJson<BackupManifest>(json);
                    logger.Log(ConversionConfig.LogLevel.Debug, $"Loaded backup manifest with {manifest.entries.Count} entries");
                }
                else
                {
                    manifest = new BackupManifest();
                    logger.Log(ConversionConfig.LogLevel.Debug, "Created new backup manifest");
                }
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error loading backup manifest: {ex.Message}");
                manifest = new BackupManifest();
            }
        }
        
        private void SaveManifest()
        {
            try
            {
                string json = JsonUtility.ToJson(manifest, true);
                File.WriteAllText(manifestPath, json);
                logger.Log(ConversionConfig.LogLevel.Debug, "Saved backup manifest");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error saving backup manifest: {ex.Message}");
            }
        }
        
#if UNITY_EDITOR
        public string BackupMaterial(Material material, string fbxPath)
        {
            try
            {
                if (!config.createBackups)
                {
                    logger.Log(ConversionConfig.LogLevel.Debug, "Backup creation is disabled in config");
                    return null;
                }
                
                string originalPath = AssetDatabase.GetAssetPath(material);
                if (string.IsNullOrEmpty(originalPath))
                {
                    logger.Log(ConversionConfig.LogLevel.Warning, $"Cannot backup material {material.name}: no asset path found");
                    return null;
                }
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"{material.name}_backup_{timestamp}.mat";
                string backupPath = Path.Combine(config.backupPath, backupFileName);
                
                var backupMaterial = new Material(material);
                backupMaterial.name = material.name + "_backup";
                
                AssetDatabase.CreateAsset(backupMaterial, backupPath);
                AssetDatabase.SaveAssets();
                
                var backupEntry = new BackupEntry(originalPath, backupPath, fbxPath, material.name);
                manifest.entries.Add(backupEntry);
                SaveManifest();
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Backed up material {material.name} to {backupPath}");
                return backupPath;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error backing up material {material.name}: {ex.Message}");
                return null;
            }
        }
        
        public bool RestoreMaterial(string materialName)
        {
            try
            {
                var entry = manifest.entries.Find(e => e.materialName == materialName);
                if (entry == null)
                {
                    logger.Log(ConversionConfig.LogLevel.Warning, $"No backup found for material: {materialName}");
                    return false;
                }
                
                var backupMaterial = AssetDatabase.LoadAssetAtPath<Material>(entry.backupMaterialPath);
                if (backupMaterial == null)
                {
                    logger.Log(ConversionConfig.LogLevel.Error, $"Backup material not found at path: {entry.backupMaterialPath}");
                    return false;
                }
                
                var originalMaterial = AssetDatabase.LoadAssetAtPath<Material>(entry.originalMaterialPath);
                if (originalMaterial == null)
                {
                    logger.Log(ConversionConfig.LogLevel.Error, $"Original material not found at path: {entry.originalMaterialPath}");
                    return false;
                }
                
                EditorUtility.CopySerialized(backupMaterial, originalMaterial);
                AssetDatabase.SaveAssets();
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Restored material {materialName} from backup");
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error restoring material {materialName}: {ex.Message}");
                return false;
            }
        }
        
        public bool RestoreAllMaterials()
        {
            try
            {
                int successCount = 0;
                int totalCount = manifest.entries.Count;
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Restoring {totalCount} materials from backup");
                
                foreach (var entry in manifest.entries)
                {
                    if (RestoreMaterial(entry.materialName))
                    {
                        successCount++;
                    }
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Restored {successCount}/{totalCount} materials successfully");
                return successCount == totalCount;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error restoring all materials: {ex.Message}");
                return false;
            }
        }
        
        public void CleanupOldBackups(int daysToKeep = 30)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var entriesToRemove = new List<BackupEntry>();
                
                foreach (var entry in manifest.entries)
                {
                    if (DateTime.TryParse(entry.timestamp, out var entryDate))
                    {
                        if (entryDate < cutoffDate)
                        {
                            if (File.Exists(entry.backupMaterialPath))
                            {
                                AssetDatabase.DeleteAsset(entry.backupMaterialPath);
                            }
                            entriesToRemove.Add(entry);
                        }
                    }
                }
                
                foreach (var entry in entriesToRemove)
                {
                    manifest.entries.Remove(entry);
                }
                
                if (entriesToRemove.Count > 0)
                {
                    SaveManifest();
                    AssetDatabase.Refresh();
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Cleaned up {entriesToRemove.Count} old backup entries");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error cleaning up old backups: {ex.Message}");
            }
        }
#endif
        
        public List<BackupEntry> GetBackupEntries()
        {
            return new List<BackupEntry>(manifest.entries);
        }
        
        public BackupEntry FindBackupEntry(string materialName)
        {
            return manifest.entries.Find(e => e.materialName == materialName);
        }
    }
}