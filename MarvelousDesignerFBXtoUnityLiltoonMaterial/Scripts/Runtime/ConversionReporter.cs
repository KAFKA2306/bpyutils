using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace FBXMaterialConverter
{
    [Serializable]
    public class ConversionReport
    {
        public ConversionStats statistics;
        public List<ProcessedFileInfo> processedFiles;
        public List<ConversionError> errors;
        public List<BackupInfo> backups;
        public SystemInfo systemInfo;
        public string configUsed;
        
        public ConversionReport()
        {
            processedFiles = new List<ProcessedFileInfo>();
            errors = new List<ConversionError>();
            backups = new List<BackupInfo>();
            systemInfo = new SystemInfo();
        }
    }
    
    [Serializable]
    public class ProcessedFileInfo
    {
        public string fileName;
        public string filePath;
        public int materialsFound;
        public int materialsConverted;
        public int transparencyApplied;
        public bool success;
        public string processingTime;
        public List<string> materialNames;
        
        public ProcessedFileInfo()
        {
            materialNames = new List<string>();
        }
    }
    
    [Serializable]
    public class ConversionError
    {
        public string fileName;
        public string errorType;
        public string errorMessage;
        public string timestamp;
        public string stackTrace;
        
        public ConversionError(string fileName, string errorType, string errorMessage, string stackTrace = null)
        {
            this.fileName = fileName;
            this.errorType = errorType;
            this.errorMessage = errorMessage;
            this.stackTrace = stackTrace;
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    [Serializable]
    public class BackupInfo
    {
        public string materialName;
        public string originalPath;
        public string backupPath;
        public string timestamp;
        
        public BackupInfo(string materialName, string originalPath, string backupPath)
        {
            this.materialName = materialName;
            this.originalPath = originalPath;
            this.backupPath = backupPath;
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    
    [Serializable]
    public class SystemInfo
    {
        public string unityVersion;
        public string platform;
        public string operatingSystem;
        public string processorType;
        public int processorCount;
        public int systemMemorySize;
        
        public SystemInfo()
        {
            unityVersion = Application.unityVersion;
            platform = Application.platform.ToString();
            operatingSystem = SystemInfo.operatingSystem;
            processorType = SystemInfo.processorType;
            processorCount = SystemInfo.processorCount;
            systemMemorySize = SystemInfo.systemMemorySize;
        }
    }
    
    public class ConversionReporter
    {
        private readonly ConversionConfig config;
        private readonly ConversionLogger logger;
        private ConversionReport report;
        
        public ConversionReporter(ConversionConfig config, ConversionLogger logger)
        {
            this.config = config;
            this.logger = logger;
            this.report = new ConversionReport();
            
            InitializeReport();
        }
        
        private void InitializeReport()
        {
            try
            {
                report.configUsed = JsonUtility.ToJson(config, true);
                logger.Log(ConversionConfig.LogLevel.Debug, "Conversion reporter initialized");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error initializing reporter: {ex.Message}");
            }
        }
        
        public void StartFileProcessing(string filePath)
        {
            try
            {
                var fileInfo = new ProcessedFileInfo
                {
                    fileName = Path.GetFileName(filePath),
                    filePath = filePath,
                    success = false
                };
                
                report.processedFiles.Add(fileInfo);
                logger.Log(ConversionConfig.LogLevel.Debug, $"Started processing file: {fileInfo.fileName}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error starting file processing report: {ex.Message}");
            }
        }
        
        public void FinishFileProcessing(string filePath, bool success, int materialsFound, int materialsConverted, int transparencyApplied, List<string> materialNames = null)
        {
            try
            {
                var fileInfo = report.processedFiles.Find(f => f.filePath == filePath);
                if (fileInfo != null)
                {
                    fileInfo.success = success;
                    fileInfo.materialsFound = materialsFound;
                    fileInfo.materialsConverted = materialsConverted;
                    fileInfo.transparencyApplied = transparencyApplied;
                    
                    if (materialNames != null)
                    {
                        fileInfo.materialNames.AddRange(materialNames);
                    }
                    
                    logger.Log(ConversionConfig.LogLevel.Debug, $"Finished processing file: {fileInfo.fileName} (Success: {success})");
                }
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error finishing file processing report: {ex.Message}");
            }
        }
        
        public void AddError(string fileName, string errorType, string errorMessage, string stackTrace = null)
        {
            try
            {
                var error = new ConversionError(fileName, errorType, errorMessage, stackTrace);
                report.errors.Add(error);
                logger.Log(ConversionConfig.LogLevel.Debug, $"Added error to report: {errorType} in {fileName}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error adding error to report: {ex.Message}");
            }
        }
        
        public void AddBackupInfo(string materialName, string originalPath, string backupPath)
        {
            try
            {
                var backupInfo = new BackupInfo(materialName, originalPath, backupPath);
                report.backups.Add(backupInfo);
                logger.Log(ConversionConfig.LogLevel.Debug, $"Added backup info to report: {materialName}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error adding backup info to report: {ex.Message}");
            }
        }
        
        public void FinalizeReport()
        {
            try
            {
                report.statistics = logger.GetStats();
                logger.Log(ConversionConfig.LogLevel.Info, "Conversion report finalized");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error finalizing report: {ex.Message}");
            }
        }
        
        public void SaveReportToFile(string filePath)
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string reportJson = JsonUtility.ToJson(report, true);
                File.WriteAllText(filePath, reportJson);
                
                logger.Log(ConversionConfig.LogLevel.Info, $"Conversion report saved to: {filePath}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error saving report to file: {ex.Message}");
            }
        }
        
        public void SaveReportToCSV(string filePath)
        {
            try
            {
                var csv = new StringBuilder();
                
                csv.AppendLine("=== CONVERSION SUMMARY ===");
                csv.AppendLine($"Total Files Processed,{report.statistics.processedFiles}");
                csv.AppendLine($"Total Materials Converted,{report.statistics.convertedMaterials}");
                csv.AppendLine($"Total Transparency Applied,{report.statistics.transparencyApplied}");
                csv.AppendLine($"Total Errors,{report.statistics.errors}");
                csv.AppendLine($"Total Warnings,{report.statistics.warnings}");
                csv.AppendLine($"Processing Time,{report.statistics.processingTime}");
                csv.AppendLine();
                
                csv.AppendLine("=== PROCESSED FILES ===");
                csv.AppendLine("File Name,Materials Found,Materials Converted,Transparency Applied,Success,Material Names");
                
                foreach (var file in report.processedFiles)
                {
                    var materialNamesStr = string.Join(";", file.materialNames);
                    csv.AppendLine($"\"{file.fileName}\",{file.materialsFound},{file.materialsConverted},{file.transparencyApplied},{file.success},\"{materialNamesStr}\"");
                }
                
                csv.AppendLine();
                csv.AppendLine("=== ERRORS ===");
                csv.AppendLine("File Name,Error Type,Error Message,Timestamp");
                
                foreach (var error in report.errors)
                {
                    csv.AppendLine($"\"{error.fileName}\",\"{error.errorType}\",\"{error.errorMessage.Replace("\"", "\"\"\")}\",\"{error.timestamp}\"");
                }
                
                csv.AppendLine();
                csv.AppendLine("=== BACKUPS ===");
                csv.AppendLine("Material Name,Original Path,Backup Path,Timestamp");
                
                foreach (var backup in report.backups)
                {
                    csv.AppendLine($"\"{backup.materialName}\",\"{backup.originalPath}\",\"{backup.backupPath}\",\"{backup.timestamp}\"");
                }
                
                File.WriteAllText(filePath, csv.ToString());
                logger.Log(ConversionConfig.LogLevel.Info, $"CSV report saved to: {filePath}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error saving CSV report: {ex.Message}");
            }
        }
        
        public void GenerateHTMLReport(string filePath)
        {
            try
            {
                var html = new StringBuilder();
                
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html><head>");
                html.AppendLine("<title>FBX Material Conversion Report</title>");
                html.AppendLine("<style>");
                html.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
                html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
                html.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                html.AppendLine("th { background-color: #f2f2f2; }");
                html.AppendLine(".success { color: green; }");
                html.AppendLine(".error { color: red; }");
                html.AppendLine(".warning { color: orange; }");
                html.AppendLine("</style>");
                html.AppendLine("</head><body>");
                
                html.AppendLine("<h1>FBX Material Conversion Report</h1>");
                html.AppendLine($"<p>Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
                
                html.AppendLine("<h2>Summary</h2>");
                html.AppendLine("<table>");
                html.AppendLine($"<tr><td>Total Files Processed</td><td>{report.statistics.processedFiles}</td></tr>");
                html.AppendLine($"<tr><td>Total Materials Converted</td><td>{report.statistics.convertedMaterials}</td></tr>");
                html.AppendLine($"<tr><td>Total Transparency Applied</td><td>{report.statistics.transparencyApplied}</td></tr>");
                html.AppendLine($"<tr><td>Total Errors</td><td class=\"error\">{report.statistics.errors}</td></tr>");
                html.AppendLine($"<tr><td>Total Warnings</td><td class=\"warning\">{report.statistics.warnings}</td></tr>");
                html.AppendLine($"<tr><td>Processing Time</td><td>{report.statistics.processingTime}</td></tr>");
                html.AppendLine("</table>");
                
                html.AppendLine("<h2>Processed Files</h2>");
                html.AppendLine("<table>");
                html.AppendLine("<tr><th>File Name</th><th>Materials Found</th><th>Materials Converted</th><th>Transparency Applied</th><th>Success</th></tr>");
                
                foreach (var file in report.processedFiles)
                {
                    var statusClass = file.success ? "success" : "error";
                    html.AppendLine($"<tr><td>{file.fileName}</td><td>{file.materialsFound}</td><td>{file.materialsConverted}</td><td>{file.transparencyApplied}</td><td class=\"{statusClass}\">{file.success}</td></tr>");
                }
                html.AppendLine("</table>");
                
                if (report.errors.Count > 0)
                {
                    html.AppendLine("<h2>Errors</h2>");
                    html.AppendLine("<table>");
                    html.AppendLine("<tr><th>File Name</th><th>Error Type</th><th>Error Message</th><th>Timestamp</th></tr>");
                    
                    foreach (var error in report.errors)
                    {
                        html.AppendLine($"<tr><td>{error.fileName}</td><td>{error.errorType}</td><td>{error.errorMessage}</td><td>{error.timestamp}</td></tr>");
                    }
                    html.AppendLine("</table>");
                }
                
                html.AppendLine("<h2>System Information</h2>");
                html.AppendLine("<table>");
                html.AppendLine($"<tr><td>Unity Version</td><td>{report.systemInfo.unityVersion}</td></tr>");
                html.AppendLine($"<tr><td>Platform</td><td>{report.systemInfo.platform}</td></tr>");
                html.AppendLine($"<tr><td>Operating System</td><td>{report.systemInfo.operatingSystem}</td></tr>");
                html.AppendLine($"<tr><td>Processor</td><td>{report.systemInfo.processorType}</td></tr>");
                html.AppendLine($"<tr><td>Processor Count</td><td>{report.systemInfo.processorCount}</td></tr>");
                html.AppendLine($"<tr><td>System Memory</td><td>{report.systemInfo.systemMemorySize} MB</td></tr>");
                html.AppendLine("</table>");
                
                html.AppendLine("</body></html>");
                
                File.WriteAllText(filePath, html.ToString());
                logger.Log(ConversionConfig.LogLevel.Info, $"HTML report saved to: {filePath}");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Error, $"Error generating HTML report: {ex.Message}");
            }
        }
        
        public ConversionReport GetReport()
        {
            return report;
        }
        
        public void PrintSummaryToConsole()
        {
            try
            {
                Debug.Log("=== FBX MATERIAL CONVERSION SUMMARY ===");
                Debug.Log($"Files Processed: {report.statistics.processedFiles}");
                Debug.Log($"Materials Converted: {report.statistics.convertedMaterials}");
                Debug.Log($"Transparency Applied: {report.statistics.transparencyApplied}");
                Debug.Log($"Errors: {report.statistics.errors}");
                Debug.Log($"Warnings: {report.statistics.warnings}");
                Debug.Log($"Processing Time: {report.statistics.processingTime}");
                
                if (report.errors.Count > 0)
                {
                    Debug.Log("=== ERRORS ===");
                    foreach (var error in report.errors)
                    {
                        Debug.LogError($"{error.fileName}: {error.errorType} - {error.errorMessage}");
                    }
                }
                
                Debug.Log("========================================");
            }
            catch (Exception ex)
            {
                logger.Log(ConversionConfig.LogLevel.Warning, $"Error printing summary to console: {ex.Message}");
            }
        }
    }
}