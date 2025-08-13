using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FBXMaterialConverter
{
    public class FBXScanner
    {
        private readonly ConversionConfig config;
        private readonly ConversionLogger logger;
        
        public FBXScanner(ConversionConfig config, ConversionLogger logger)
        {
            this.config = config;
            this.logger = logger;
        }
        
        public List<string> ScanForFBXFiles()
        {
            var fbxFiles = new List<string>();
            
            try
            {
                logger.Log(ConversionConfig.LogLevel.Info, $"Scanning for FBX files in: {config.targetFolder}");
                
                if (!Directory.Exists(config.targetFolder))
                {
                    logger.Log(ConversionConfig.LogLevel.Warning, $"Target folder does not exist: {config.targetFolder}");
                    return fbxFiles;
                }
                
                var allFiles = Directory.GetFiles(config.targetFolder, "*.fbx", SearchOption.AllDirectories);
                
                foreach (var file in allFiles)
                {
                    if (ShouldProcessFile(file))
                    {
                        fbxFiles.Add(file);
                        logger.Log(ConversionConfig.LogLevel.Debug, $"Found FBX file: {file}");
                    }
                    else
                    {
                        logger.Log(ConversionConfig.LogLevel.Debug, $"Skipping file (excluded): {file}");
                    }
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Found {fbxFiles.Count} FBX files to process");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error scanning for FBX files: {ex.Message}");
            }
            
            return fbxFiles;
        }
        
        private bool ShouldProcessFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            
            foreach (var pattern in config.excludePatterns)
            {
                if (fileName.Contains(pattern))
                {
                    return false;
                }
            }
            
            return true;
        }
        
#if UNITY_EDITOR
        public List<Material> ExtractMaterialsFromFBX(string fbxPath)
        {
            var materials = new List<Material>();
            
            try
            {
                logger.Log(ConversionConfig.LogLevel.Debug, $"Extracting materials from: {fbxPath}");
                
                var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
                
                foreach (var asset in assets)
                {
                    if (asset is Material material)
                    {
                        materials.Add(material);
                        logger.Log(ConversionConfig.LogLevel.Debug, $"Found material: {material.name}");
                    }
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Extracted {materials.Count} materials from {Path.GetFileName(fbxPath)}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error extracting materials from {fbxPath}: {ex.Message}");
            }
            
            return materials;
        }
        
        public List<Renderer> GetRenderersUsingMaterial(Material material)
        {
            var renderers = new List<Renderer>();
            
            try
            {
                var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
                
                foreach (var renderer in allRenderers)
                {
                    if (renderer.sharedMaterials.Contains(material))
                    {
                        renderers.Add(renderer);
                    }
                }
                
                logger.Log(ConversionConfig.LogLevel.Debug, $"Found {renderers.Count} renderers using material: {material.name}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error finding renderers for material {material.name}: {ex.Message}");
            }
            
            return renderers;
        }
        
        public bool HasAlphaChannel(Texture2D texture)
        {
            if (texture == null) return false;
            
            try
            {
                var textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
                if (textureImporter != null)
                {
                    return textureImporter.DoesSourceTextureHaveAlpha();
                }
                
                return texture.format == TextureFormat.RGBA32 || 
                       texture.format == TextureFormat.ARGB32 ||
                       texture.format == TextureFormat.RGBA4444 ||
                       texture.format == TextureFormat.ARGB4444;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error checking alpha channel for texture {texture.name}: {ex.Message}");
                return false;
            }
        }
#endif
    }
}