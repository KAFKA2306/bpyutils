using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FBXMaterialConverter
{
    [Serializable]
    public class LogEntry
    {
        public string timestamp;
        public string level;
        public string message;
        public string stackTrace;
        
        public LogEntry(ConversionConfig.LogLevel level, string message, string stackTrace = null)
        {
            this.timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            this.level = level.ToString();
            this.message = message;
            this.stackTrace = stackTrace;
        }
    }
    
    [Serializable]
    public class ConversionStats
    {
        public int processedFiles;
        public int convertedMaterials;
        public int transparencyApplied;
        public int errors;
        public int warnings;
        public string processingTime;
        public DateTime startTime;
        public DateTime endTime;
        
        public ConversionStats()
        {
            startTime = DateTime.Now;
            processedFiles = 0;
            convertedMaterials = 0;
            transparencyApplied = 0;
            errors = 0;
            warnings = 0;
        }
        
        public void FinishProcessing()
        {
            endTime = DateTime.Now;
            var duration = endTime - startTime;
            processingTime = duration.ToString(@"hh\:mm\:ss");
        }
    }
    
    public class ConversionLogger
    {
        private readonly ConversionConfig config;
        private readonly List<LogEntry> logEntries;
        private readonly ConversionStats stats;
        private string logFilePath;
        
        public ConversionLogger(ConversionConfig config)
        {
            this.config = config;
            this.logEntries = new List<LogEntry>();
            this.stats = new ConversionStats();
            
            InitializeLogger();
        }
        
        private void InitializeLogger()
        {
            try
            {
                if (!Directory.Exists(config.logPath))
                {
                    Directory.CreateDirectory(config.logPath);
                }
                
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                logFilePath = Path.Combine(config.logPath, $"fbx_conversion_{timestamp}.log");
                
                Log(ConversionConfig.LogLevel.Info, "FBX Material Conversion Logger initialized");
                Log(ConversionConfig.LogLevel.Info, $"Log file: {logFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to initialize logger: {ex.Message}");
            }
        }
        
        public void Log(ConversionConfig.LogLevel level, string message, Exception exception = null)
        {
            try
            {
                if (level > config.logLevel)
                {
                    return;
                }
                
                string stackTrace = exception?.StackTrace;
                if (exception != null)
                {
                    message += $" Exception: {exception.Message}";
                }
                
                var logEntry = new LogEntry(level, message, stackTrace);
                logEntries.Add(logEntry);
                
                UpdateStats(level);
                
                LogToUnityConsole(level, message);
                LogToFile(logEntry);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in logger: {ex.Message}");
            }
        }
        
        private void UpdateStats(ConversionConfig.LogLevel level)
        {
            switch (level)
            {
                case ConversionConfig.LogLevel.Error:
                    stats.errors++;
                    break;
                case ConversionConfig.LogLevel.Warning:
                    stats.warnings++;
                    break;
            }
        }
        
        private void LogToUnityConsole(ConversionConfig.LogLevel level, string message)
        {
            switch (level)
            {
                case ConversionConfig.LogLevel.Error:
                    Debug.LogError($"[FBX Converter] {message}");
                    break;
                case ConversionConfig.LogLevel.Warning:
                    Debug.LogWarning($"[FBX Converter] {message}");
                    break;
                case ConversionConfig.LogLevel.Info:
                case ConversionConfig.LogLevel.Debug:
                    Debug.Log($"[FBX Converter] {message}");
                    break;
            }
        }
        
        private void LogToFile(LogEntry entry)
        {
            try
            {
                if (string.IsNullOrEmpty(logFilePath))
                {
                    return;
                }
                
                string logLine = $"[{entry.timestamp}] [{entry.level}] {entry.message}";
                if (!string.IsNullOrEmpty(entry.stackTrace))
                {
                    logLine += $"\nStackTrace: {entry.stackTrace}";
                }
                logLine += "\n";
                
                File.AppendAllText(logFilePath, logLine);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }
        
        public void LogProcessedFile(string fileName)
        {
            stats.processedFiles++;
            Log(ConversionConfig.LogLevel.Debug, $"Processed file: {fileName}");
        }
        
        public void LogConvertedMaterial(string materialName)
        {
            stats.convertedMaterials++;
            Log(ConversionConfig.LogLevel.Debug, $"Converted material: {materialName}");
        }
        
        public void LogTransparencyApplied(string materialName)
        {
            stats.transparencyApplied++;
            Log(ConversionConfig.LogLevel.Debug, $"Applied transparency to material: {materialName}");
        }
        
        public ConversionStats GetStats()
        {
            return stats;
        }
        
        public void FinishLogging()
        {
            stats.FinishProcessing();
            Log(ConversionConfig.LogLevel.Info, "Conversion process completed");
            LogSummary();
            SaveStatsToFile();
        }
        
        private void LogSummary()
        {
            Log(ConversionConfig.LogLevel.Info, "=== CONVERSION SUMMARY ===");
            Log(ConversionConfig.LogLevel.Info, $"Processed Files: {stats.processedFiles}");
            Log(ConversionConfig.LogLevel.Info, $"Converted Materials: {stats.convertedMaterials}");
            Log(ConversionConfig.LogLevel.Info, $"Transparency Applied: {stats.transparencyApplied}");
            Log(ConversionConfig.LogLevel.Info, $"Errors: {stats.errors}");
            Log(ConversionConfig.LogLevel.Info, $"Warnings: {stats.warnings}");
            Log(ConversionConfig.LogLevel.Info, $"Processing Time: {stats.processingTime}");
            Log(ConversionConfig.LogLevel.Info, "==========================");
        }
        
        private void SaveStatsToFile()
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string statsPath = Path.Combine(config.logPath, $"conversion_stats_{timestamp}.json");
                
                string statsJson = JsonUtility.ToJson(stats, true);
                File.WriteAllText(statsPath, statsJson);
                
                Log(ConversionConfig.LogLevel.Debug, $"Saved statistics to: {statsPath}");
            }
            catch (Exception ex)
            {
                Log(ConversionConfig.LogLevel.Warning, $"Failed to save statistics: {ex.Message}");
            }
        }
        
        public List<LogEntry> GetLogEntries()
        {
            return new List<LogEntry>(logEntries);
        }
        
        public List<LogEntry> GetLogEntries(ConversionConfig.LogLevel level)
        {
            return logEntries.FindAll(entry => entry.level == level.ToString());
        }
        
        public void ExportLogsToCSV(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine("Timestamp,Level,Message");
                    
                    foreach (var entry in logEntries)
                    {
                        var escapedMessage = entry.message.Replace("\"", "\"\"");
                        writer.WriteLine($"\"{entry.timestamp}\",\"{entry.level}\",\"{escapedMessage}\"");
                    }
                }
                
                Log(ConversionConfig.LogLevel.Info, $"Exported logs to CSV: {filePath}");
            }
            catch (Exception ex)
            {
                Log(ConversionConfig.LogLevel.Error, $"Failed to export logs to CSV: {ex.Message}");
            }
        }
    }
}