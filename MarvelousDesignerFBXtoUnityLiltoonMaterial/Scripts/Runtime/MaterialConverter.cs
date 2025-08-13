using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FBXMaterialConverter
{
    public class MaterialConverter
    {
        private readonly ConversionConfig config;
        private readonly ConversionLogger logger;
        private readonly FBXScanner scanner;
        
        private Shader lilToonShader;
        private readonly Dictionary<string, string> propertyMapping;
        
        public MaterialConverter(ConversionConfig config, ConversionLogger logger, FBXScanner scanner)
        {
            this.config = config;
            this.logger = logger;
            this.scanner = scanner;
            
            InitializePropertyMapping();
        }
        
        private void InitializePropertyMapping()
        {
            propertyMapping = new Dictionary<string, string>
            {
                { "_MainTex", "_MainTex" },
                { "_BumpMap", "_BumpMap" },
                { "_Color", "_Color" },
                { "_Metallic", "_Metallic" },
                { "_Glossiness", "_Smoothness" },
                { "_BumpScale", "_BumpScale" },
                { "_OcclusionMap", "_OcclusionMap" },
                { "_EmissionMap", "_EmissionMap" },
                { "_EmissionColor", "_EmissionColor" }
            };
        }
        
#if UNITY_EDITOR
        public bool InitializeLilToonShader()
        {
            try
            {
                lilToonShader = Shader.Find(config.targetShader);
                if (lilToonShader == null)
                {
                    logger.Log(ConversionConfig.LogLevel.Error, $"lilToon shader not found: {config.targetShader}");
                    return false;
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Found lilToon shader: {lilToonShader.name}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error initializing lilToon shader: {ex.Message}");
                return false;
            }
        }
        
        public Material ConvertMaterial(Material originalMaterial)
        {
            try
            {
                logger.Log(ConversionConfig.LogLevel.Info, $"Converting material: {originalMaterial.name}");
                
                if (lilToonShader == null && !InitializeLilToonShader())
                {
                    logger.Log(ConversionConfig.LogLevel.Error, "Cannot convert material: lilToon shader not available");
                    return null;
                }
                
                if (originalMaterial.shader.name.Contains("lilToon"))
                {
                    logger.Log(ConversionConfig.LogLevel.Info, $"Material {originalMaterial.name} is already using lilToon shader");
                    return originalMaterial;
                }
                
                var convertedMaterial = new Material(lilToonShader)
                {
                    name = originalMaterial.name + "_lilToon"
                };
                
                TransferProperties(originalMaterial, convertedMaterial);
                
                if (config.enableTransparency)
                {
                    ApplyTransparencySettings(originalMaterial, convertedMaterial);
                }
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Successfully converted material: {originalMaterial.name}");
                return convertedMaterial;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error converting material {originalMaterial.name}: {ex.Message}");
                return null;
            }
        }
        
        private void TransferProperties(Material source, Material target)
        {
            try
            {
                foreach (var mapping in propertyMapping)
                {
                    var sourceProperty = mapping.Key;
                    var targetProperty = mapping.Value;
                    
                    if (source.HasProperty(sourceProperty) && target.HasProperty(targetProperty))
                    {
                        var sourceValue = source.GetFloat(sourceProperty);
                        target.SetFloat(targetProperty, sourceValue);
                        logger.Log(ConversionConfig.LogLevel.Debug, $"Transferred float property {sourceProperty} -> {targetProperty}: {sourceValue}");
                    }
                    
                    if (source.HasProperty(sourceProperty) && target.HasProperty(targetProperty))
                    {
                        var sourceTexture = source.GetTexture(sourceProperty);
                        if (sourceTexture != null)
                        {
                            target.SetTexture(targetProperty, sourceTexture);
                            logger.Log(ConversionConfig.LogLevel.Debug, $"Transferred texture property {sourceProperty} -> {targetProperty}: {sourceTexture.name}");
                        }
                    }
                    
                    if (source.HasProperty(sourceProperty) && target.HasProperty(targetProperty))
                    {
                        var sourceColor = source.GetColor(sourceProperty);
                        target.SetColor(targetProperty, sourceColor);
                        logger.Log(ConversionConfig.LogLevel.Debug, $"Transferred color property {sourceProperty} -> {targetProperty}: {sourceColor}");
                    }
                }
                
                target.CopyPropertiesFromMaterial(source);
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error transferring properties: {ex.Message}");
            }
        }
        
        private void ApplyTransparencySettings(Material source, Material target)
        {
            try
            {
                bool hasTransparency = false;
                
                if (config.autoDetectTransparency)
                {
                    hasTransparency = DetectTransparency(source);
                }
                
                if (hasTransparency)
                {
                    logger.Log(ConversionConfig.LogLevel.Info, $"Applying transparency to material: {target.name}");
                    
                    if (target.HasProperty("_TransparentMode"))
                    {
                        target.SetFloat("_TransparentMode", config.transparencyMode);
                    }
                    
                    target.renderQueue = config.renderQueue;
                    
                    if (target.HasProperty("_SrcBlend"))
                    {
                        target.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    }
                    
                    if (target.HasProperty("_DstBlend"))
                    {
                        target.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    }
                    
                    if (target.HasProperty("_ZWrite"))
                    {
                        target.SetFloat("_ZWrite", 0);
                    }
                    
                    target.EnableKeyword("_ALPHABLEND_ON");
                    target.DisableKeyword("_ALPHATEST_ON");
                    target.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    
                    logger.Log(ConversionConfig.LogLevel.Debug, "Transparency settings applied successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error applying transparency settings: {ex.Message}");
            }
        }
        
        private bool DetectTransparency(Material material)
        {
            try
            {
                var mainTexture = material.GetTexture("_MainTex") as Texture2D;
                if (mainTexture != null && scanner.HasAlphaChannel(mainTexture))
                {
                    logger.Log(ConversionConfig.LogLevel.Debug, $"Alpha channel detected in main texture: {mainTexture.name}");
                    return true;
                }
                
                var color = material.GetColor("_Color");
                if (color.a < 1.0f - config.alphaThreshold)
                {
                    logger.Log(ConversionConfig.LogLevel.Debug, $"Transparency detected in material color: {color.a}");
                    return true;
                }
                
                if (material.HasProperty("_Mode"))
                {
                    var mode = material.GetFloat("_Mode");
                    if (mode == 2 || mode == 3)
                    {
                        logger.Log(ConversionConfig.LogLevel.Debug, $"Transparent rendering mode detected: {mode}");
                        return true;
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error detecting transparency: {ex.Message}");
                return false;
            }
        }
        
        public void SaveMaterial(Material material, string path)
        {
            try
            {
                AssetDatabase.CreateAsset(material, path);
                AssetDatabase.SaveAssets();
                logger.Log(ConversionConfig.LogLevel.Debug, $"Saved material to: {path}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error saving material to {path}: {ex.Message}");
            }
        }
#endif
    }
}