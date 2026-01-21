using System;
using System.Text.Json.Serialization;

namespace ClassroomManagement.Models
{
    /// <summary>
    /// Message gửi qua mạng
    /// </summary>
    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string? TargetId { get; set; }
        public string? Payload { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Loại message
    /// </summary>
    public enum MessageType
    {
        // Connection
        Connect = 0x01,
        Disconnect = 0x02,
        Heartbeat = 0x03,
        ConnectAck = 0x04,

        // Screen
        ScreenData = 0x10,
        ScreenRequest = 0x11,
        ScreenShare = 0x12,
        ScreenShareStop = 0x13,

        // Control
        ControlMouse = 0x20,
        ControlKeyboard = 0x21,
        ControlStart = 0x22,
        ControlStop = 0x23,

        // Chat
        ChatMessage = 0x30,
        ChatPrivate = 0x31,

        // File
        FileStart = 0x40,
        FileData = 0x41,
        FileEnd = 0x42,
        FileRequest = 0x43,

        // Lock
        LockScreen = 0x50,
        UnlockScreen = 0x51,

        // Test
        TestStart = 0x60,
        TestSubmit = 0x61,
        TestResult = 0x62,

        // Misc
        RaiseHand = 0x70,
        LowerHand = 0x71,
        Notification = 0x72,

        // Assignment
        AssignmentSubmit = 0x90,
        AssignmentSubmitAck = 0x91,
        AssignmentList = 0x92,

        // System Info (Feature 2)
        SystemSpecsRequest = 0xA0,
        SystemSpecsResponse = 0xA1,

        // App Management (Feature 3)
        ProcessListRequest = 0xB0,
        ProcessListResponse = 0xB1,
        ProcessKillCommand = 0xB2,

        // File Collection (Feature 4)
        FileCollectionRequest = 0xC0,
        FileCollectionData = 0xC1,
        FileCollectionStatus = 0xC2,

        // Bulk File Send (Feature 6)
        BulkFileTransferRequest = 0xD0,
        BulkFileData = 0xD1
    }

    /// <summary>
    /// Thông tin Server Discovery (UDP Broadcast)
    /// </summary>
    public class ServerDiscoveryInfo
    {
        public string ServerIp { get; set; } = string.Empty;
        public int ServerPort { get; set; } = 5000;
        public string ClassName { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int OnlineCount { get; set; }
    }

    /// <summary>
    /// Thông tin kết nối Client
    /// </summary>
    public class ClientInfo
    {
        public string MachineId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ComputerName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dữ liệu màn hình
    /// </summary>
    public class ScreenData
    {
        public string ClientId { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime CaptureTime { get; set; }
    }

    /// <summary>
    /// Lệnh điều khiển chuột
    /// </summary>
    public class MouseCommand
    {
        public int X { get; set; }
        public int Y { get; set; }
        public MouseAction Action { get; set; }
        public int Delta { get; set; } // For scroll
    }

    public enum MouseAction
    {
        Move,
        LeftDown,
        LeftUp,
        LeftClick,
        LeftDoubleClick,
        RightDown,
        RightUp,
        RightClick,
        MiddleDown,
        MiddleUp,
        Scroll
    }

    /// <summary>
    /// Lệnh điều khiển bàn phím
    /// </summary>
    public class KeyboardCommand
    {
        public int KeyCode { get; set; }
        public KeyAction Action { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
    }

    public enum KeyAction
    {
        Down,
        Up,
        Press
    }

    /// <summary>
    /// Thông tin truyền file
    /// </summary>
    public class FileTransferInfo
    {
        public string FileId { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public int TotalChunks { get; set; }
        public int ChunkIndex { get; set; }
        public byte[] ChunkData { get; set; } = Array.Empty<byte>();
    }
}
