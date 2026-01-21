using System.Collections.Generic;

namespace ClassroomManagement.Models
{
    public class FileCollectionRequest
    {
        public string RemotePath { get; set; } = string.Empty;
        public List<string> Extensions { get; set; } = new(); // e.g. .doc, .docx
        public bool Recursive { get; set; } = true;
    }

    public class CollectedFile
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentMachineId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public byte[] Content { get; set; } = System.Array.Empty<byte>();
    }

    public class FileCollectionStatus
    {
        public string Message { get; set; } = string.Empty;
        public int TotalFiles { get; set; }
        public int ProcessedFiles { get; set; }
    }
}
