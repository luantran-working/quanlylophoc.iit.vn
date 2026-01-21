using System;
using System.Collections.Generic;

namespace ClassroomManagement.Models
{
    public class ComputerSpecs
    {
        public string OS { get; set; } = "";
        public string CPU { get; set; } = "";
        public double RAMTotalGB { get; set; }
        public string GPU { get; set; } = "";
        public string Motherboard { get; set; } = "";
        public string BIOS { get; set; } = "";
        public string Monitor { get; set; } = "";
        public string ComputerName { get; set; } = "";
        public string IpAddress { get; set; } = "";
    }

    public class DiskDriveInfo
    {
        public string Name { get; set; } = "";
        public long TotalSize { get; set; }
        public long FreeSpace { get; set; }

        // Calculated property for Display
        public double FreeSpaceGB => Math.Round(FreeSpace / (1024.0 * 1024.0 * 1024.0), 2);
        public double TotalSizeGB => Math.Round(TotalSize / (1024.0 * 1024.0 * 1024.0), 2);
        public double UsedPercent => TotalSize > 0 ? (1.0 - (double)FreeSpace / TotalSize) * 100 : 0;
    }

    public class SystemInfoPackage
    {
        public string MachineId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public ComputerSpecs Specs { get; set; } = new ComputerSpecs();
        public List<DiskDriveInfo> Drives { get; set; } = new List<DiskDriveInfo>();
        public DateTime CollectedAt { get; set; } = DateTime.Now;
    }
}
