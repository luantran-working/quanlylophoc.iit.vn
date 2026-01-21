using System;

namespace ClassroomManagement.Models
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string MainWindowTitle { get; set; } = string.Empty;
        public long MemoryUsage { get; set; } // In Bytes
        public string Icon { get; set; } = string.Empty; // Base64 optional

        // Convert memory to readable string
        public string MemoryUsageMB => $"{MemoryUsage / (1024.0 * 1024.0):F1} MB";

        // Helper to check if it looks like a user app
        public bool IsApplication => !string.IsNullOrEmpty(MainWindowTitle);
    }

    public class ProcessAction
    {
        public int ProcessId { get; set; }
        public string Action { get; set; } = "Kill"; // Kill, data...
    }
}
