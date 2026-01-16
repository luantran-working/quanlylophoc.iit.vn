using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service quản lý logging cho ứng dụng
    /// </summary>
    public class LogService
    {
        private static LogService? _instance;
        public static LogService Instance => _instance ??= new LogService();

        private readonly string _logFilePath;
        private readonly object _lock = new();

        public ObservableCollection<LogEntry> Logs { get; } = new();

        public event EventHandler<LogEntry>? LogAdded;

        private LogService()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "IIT", "ClassroomManagement", "Logs");
            Directory.CreateDirectory(logDir);
            
            _logFilePath = Path.Combine(logDir, $"log_{DateTime.Now:yyyy-MM-dd}.txt");
        }

        public void Info(string source, string message)
        {
            Log(LogLevel.Info, source, message);
        }

        public void Warning(string source, string message)
        {
            Log(LogLevel.Warning, source, message);
        }

        public void Error(string source, string message, Exception? ex = null)
        {
            var fullMessage = ex != null ? $"{message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}" : message;
            Log(LogLevel.Error, source, fullMessage);
        }

        public void Debug(string source, string message)
        {
            Log(LogLevel.Debug, source, message);
        }

        public void Network(string source, string message)
        {
            Log(LogLevel.Network, source, message);
        }

        private void Log(LogLevel level, string source, string message)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Source = source,
                Message = message
            };

            // Add to collection (thread-safe)
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Logs.Add(entry);
                
                // Keep only last 1000 entries in memory
                while (Logs.Count > 1000)
                {
                    Logs.RemoveAt(0);
                }
            });

            // Write to file
            lock (_lock)
            {
                try
                {
                    var line = $"[{entry.Timestamp:HH:mm:ss.fff}] [{entry.Level}] [{entry.Source}] {entry.Message}";
                    File.AppendAllText(_logFilePath, line + Environment.NewLine);
                }
                catch { }
            }

            // Also write to Debug output
            System.Diagnostics.Debug.WriteLine($"[{level}] [{source}] {message}");

            // Raise event
            LogAdded?.Invoke(this, entry);
        }

        public void Clear()
        {
            Application.Current?.Dispatcher?.Invoke(() => Logs.Clear());
        }

        public string GetLogFilePath() => _logFilePath;
    }

    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public LogLevel Level { get; set; }
        public string Source { get; set; } = "";
        public string Message { get; set; } = "";

        public string LevelIcon => Level switch
        {
            LogLevel.Info => "ℹ️",
            LogLevel.Warning => "⚠️",
            LogLevel.Error => "❌",
            LogLevel.Debug => "🔧",
            LogLevel.Network => "🌐",
            _ => "📝"
        };

        public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Network
    }
}
