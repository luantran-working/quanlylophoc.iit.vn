using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class ProcessManagerService
    {
        private static ProcessManagerService? _instance;
        public static ProcessManagerService Instance => _instance ??= new ProcessManagerService();

        /// <summary>
        /// Get list of running processes
        /// </summary>
        public List<ProcessInfo> GetRunningProcesses()
        {
            var result = new List<ProcessInfo>();
            try
            {
                var processes = Process.GetProcesses();
                foreach (var p in processes)
                {
                    try
                    {
                        var info = new ProcessInfo
                        {
                            Id = p.Id,
                            Name = p.ProcessName,
                            MainWindowTitle = p.MainWindowTitle,
                            MemoryUsage = p.PrivateMemorySize64
                        };
                        result.Add(info);
                    }
                    catch
                    {
                        // Ignore processes we can't access
                    }
                }
            }
            catch { }

            // Sort: Applications (with window title) first, then by memory usage DESC
            return result
                .OrderByDescending(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .ThenByDescending(p => p.MemoryUsage)
                .ToList();
        }

        /// <summary>
        /// Kill a process by ID
        /// </summary>
        public bool KillProcess(int pid)
        {
            try
            {
                var p = Process.GetProcessById(pid);
                p.Kill();
                return true;
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("ProcessManager", $"Failed to kill process {pid}", ex);
                return false;
            }
        }
    }
}
