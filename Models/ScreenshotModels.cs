using System;
using System.Collections.Generic;

namespace ClassroomManagement.Models
{
    public class Screenshot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string SessionId { get; set; } = "";
        public DateTime CapturedAt { get; set; } = DateTime.Now;
        public string FilePath { get; set; } = "";
        public string ThumbnailPath { get; set; } = "";
        public string? Note { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class ScreenshotRequest
    {
        public string TargetStudentId { get; set; } = "";
        // Nếu true, server sẽ lưu ảnh. Nếu false, có thể chỉ request để view (preview) nhưng hiện tại ta focus vào lưu.
        public bool SaveToLocal { get; set; } = true;
    }

    public class ScreenshotResponse
    {
        public bool Success { get; set; }
        public string ScreenshotId { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
