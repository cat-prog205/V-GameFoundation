namespace VGameFoundation.Scripts.Utilities.LogService
{
    using System;
    using System.Diagnostics;
    using UnityEngine;

    public enum LogLevel
    {
        LOG       = 1,
        WARNING   = 2,
        ERROR     = 3,
        EXCEPTION = 4,
    }

    public static class LogService
    {
        public static string Prefix = "[Service]";
        private static string Timestamp => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        
        public static void Log(string logContent, LogLevel logLevel = LogLevel.LOG)
        {
            var msg = $"[{Timestamp}] {Prefix} {logContent}";

            switch (logLevel)
            {
                case LogLevel.LOG:
#if DEV_LOG
                    UnityEngine.Debug.Log(msg);
                    break;
#endif
                case LogLevel.WARNING:
                    UnityEngine.Debug.LogWarning(msg);
                    break;

                case LogLevel.ERROR:
                    UnityEngine.Debug.LogError(msg);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }
        
        public static void LogWithColor(string logContent, Color? c = null)
        {
#if DEV_LOG
            var color = c ?? Color.white;
            
            var finalMsg = logContent.StartsWith("[20") ? logContent : $"[{Timestamp}] {logContent}";

            string hexColor = ColorUtility.ToHtmlStringRGB(color);
            UnityEngine.Debug.Log($"<color=#{hexColor}>{finalMsg}</color>");
#endif
        }

        public static void LogWarning(string logContent)
        {
            Log(logContent, LogLevel.WARNING);
        }

        public static void LogError(string logContent)
        {
            Log(logContent, LogLevel.ERROR);
        }

        public static void Exception(Exception exception)
        {
            UnityEngine.Debug.LogError($"[{Timestamp}] {Prefix} An exception occurred:");
            UnityEngine.Debug.LogException(exception);
        }

        public static void Exception(Exception exception, string message)
        {
            UnityEngine.Debug.LogError($"[{Timestamp}] {Prefix} {message}");
            UnityEngine.Debug.LogException(exception);
        }
    }
}