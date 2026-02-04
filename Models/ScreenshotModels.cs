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
        
        /// <summary>
        /// Độ phân giải yêu cầu: "thumbnail" (640x360), "hd" (1280x720), "fullhd" (1920x1080), "original" (không resize)
        /// </summary>
        public string Resolution { get; set; } = "fullhd";
        
        /// <summary>
        /// Chất lượng JPEG (1-100). Mặc định 85 cho Full HD.
        /// </summary>
        public int Quality { get; set; } = 85;
        
        /// <summary>
        /// Loại yêu cầu: "screenshot" (chụp và lưu), "preview" (xem chi tiết), "remote" (điều khiển từ xa)
        /// </summary>
        public string RequestType { get; set; } = "screenshot";
    }

    public class ScreenshotResponse
    {
        public bool Success { get; set; }
        public string ScreenshotId { get; set; } = "";
        public string Message { get; set; } = "";
    }
}
