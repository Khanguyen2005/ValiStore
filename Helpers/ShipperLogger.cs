using System;
using System.IO;
using System.Web;

namespace ValiModern.Helpers
{
    /// <summary>
    /// Simple logger for Shipper operations
    /// Logs to App_Data/Logs/shipper_YYYYMMDD.log
    /// </summary>
    public static class ShipperLogger
    {
        private static readonly object _lockObject = new object();
        private static readonly string LogDirectory = HttpContext.Current != null
            ? Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "Logs")
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs");

        /// <summary>
        /// Log information message
        /// </summary>
        public static void LogInfo(string message, int? shipperId = null)
        {
            Log("INFO", message, shipperId);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void LogWarning(string message, int? shipperId = null)
        {
            Log("WARN", message, shipperId);
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public static void LogError(string message, Exception ex = null, int? shipperId = null)
        {
            var fullMessage = message;
            if (ex != null)
            {
                fullMessage += $"\nException: {ex.Message}\nStackTrace: {ex.StackTrace}";
            }
            Log("ERROR", fullMessage, shipperId);
        }

        /// <summary>
        /// Log delivery action
        /// </summary>
        public static void LogDelivery(int orderId, int shipperId, string action, string details = null)
        {
            var message = $"Order #{orderId} - {action}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" - {details}";
            }
            Log("DELIVERY", message, shipperId);
        }

        /// <summary>
        /// Core logging method
        /// </summary>
        private static void Log(string level, string message, int? shipperId = null)
        {
            try
            {
                // Ensure log directory exists
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                // Create log filename with date
                var logFileName = $"shipper_{DateTime.Now:yyyyMMdd}.log";
                var logFilePath = Path.Combine(LogDirectory, logFileName);

                // Format log entry
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var shipperInfo = shipperId.HasValue ? $" [Shipper #{shipperId}]" : "";
                var logEntry = $"[{timestamp}] [{level}]{shipperInfo} {message}{Environment.NewLine}";

                // Thread-safe file writing
                lock (_lockObject)
                {
                    File.AppendAllText(logFilePath, logEntry);
                }
            }
            catch
            {
                // Silently fail - don't let logging break the application
            }
        }

        /// <summary>
        /// Get recent log entries (last N lines)
        /// </summary>
        public static string[] GetRecentLogs(int lines = 100)
        {
            try
            {
                var logFileName = $"shipper_{DateTime.Now:yyyyMMdd}.log";
                var logFilePath = Path.Combine(LogDirectory, logFileName);

                if (!File.Exists(logFilePath))
                {
                    return new string[0];
                }

                var allLines = File.ReadAllLines(logFilePath);
                var startIndex = Math.Max(0, allLines.Length - lines);
                var recentLines = new string[allLines.Length - startIndex];
                Array.Copy(allLines, startIndex, recentLines, 0, recentLines.Length);
                return recentLines;
            }
            catch
            {
                return new string[0];
            }
        }
    }
}
