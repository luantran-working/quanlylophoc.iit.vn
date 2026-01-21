using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class SystemInfoService
    {
        private static SystemInfoService? _instance;
        public static SystemInfoService Instance => _instance ??= new SystemInfoService();

        public SystemInfoPackage GetFullSystemInfo(string machineId, string studentName)
        {
            var package = new SystemInfoPackage
            {
                MachineId = machineId,
                StudentName = studentName,
                Specs = GetComputerSpecs(),
                Drives = GetDiskDrives()
            };
            return package;
        }

        private ComputerSpecs GetComputerSpecs()
        {
            var specs = new ComputerSpecs
            {
                ComputerName = Environment.MachineName,
                OS = GetOSVersion(),
                CPU = GetCpuName(),
                RAMTotalGB = GetTotalRamGB(),
                GPU = GetGpuNames(),
                Motherboard = GetMotherboardInfo(),
                BIOS = GetBiosInfo(),
                Monitor = GetMonitorInfo(),
                IpAddress = GetLocalIp()
            };
            return specs;
        }

        private string GetOSVersion()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem");
                foreach (var obj in searcher.Get())
                {
                    return obj["Caption"]?.ToString() ?? Environment.OSVersion.ToString();
                }
            }
            catch { }
            return Environment.OSVersion.ToString();
        }

        private string GetCpuName()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    return obj["Name"]?.ToString()?.Trim() ?? "Unknown CPU";
                }
            }
            catch { }
            return "Unknown CPU";
        }

        private double GetTotalRamGB()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (var obj in searcher.Get())
                {
                    var totalMemory = Convert.ToDouble(obj["TotalPhysicalMemory"]);
                    return Math.Round(totalMemory / (1024.0 * 1024.0 * 1024.0), 1);
                }
            }
            catch { }
            return 0;
        }

        private string GetGpuNames()
        {
            try
            {
                var gpus = new List<string>();
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (var obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (!string.IsNullOrEmpty(name)) gpus.Add(name);
                }
                return gpus.Count > 0 ? string.Join(", ", gpus) : "Integrated Graphics";
            }
            catch { }
            return "Unknown GPU";
        }

        private string GetMotherboardInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                foreach (var obj in searcher.Get())
                {
                    return $"{obj["Manufacturer"]} {obj["Product"]}";
                }
            }
            catch { }
            return "Unknown Motherboard";
        }

        private string GetBiosInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Version FROM Win32_BIOS");
                foreach (var obj in searcher.Get())
                {
                    return obj["Version"]?.ToString() ?? "Unknown BIOS";
                }
            }
            catch { }
            return "Unknown BIOS";
        }

        private string GetMonitorInfo()
        {
            try
            {
                var monitors = new List<string>();
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_DesktopMonitor");
                foreach (var obj in searcher.Get())
                {
                    var name = obj["Name"]?.ToString();
                    if (!string.IsNullOrEmpty(name)) monitors.Add(name);
                }
                return monitors.Count > 0 ? string.Join(", ", monitors) : "Generic PnP Monitor";
            }
            catch { }
            return "Unknown Monitor";
        }

        private string GetLocalIp()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        private List<DiskDriveInfo> GetDiskDrives()
        {
            var drives = new List<DiskDriveInfo>();
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && (drive.DriveType == DriveType.Fixed))
                    {
                        drives.Add(new DiskDriveInfo
                        {
                            Name = drive.Name,
                            TotalSize = drive.TotalSize,
                            FreeSpace = drive.TotalFreeSpace
                        });
                    }
                }
            }
            catch { }
            return drives;
        }
    }
}
