using System;

namespace ClassroomManagement.Models
{
    public class BulkFileTransferRequest
    {
        public string FileId { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string MimeType { get; set; } = "application/octet-stream";
        public string Description { get; set; } = string.Empty;
    }

    public class BulkFileDataChunk
    {
        public string FileId { get; set; } = string.Empty;
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
    }
}
