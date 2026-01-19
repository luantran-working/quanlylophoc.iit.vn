using System;

namespace ClassroomManagement.Models
{
    public class RemoteSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string StudentMachineId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentIpAddress { get; set; } = string.Empty;
        public RemoteSessionStatus Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool IsControlEnabled { get; set; } = true;
        public bool IsInputLocked { get; set; }
        public byte[]? LastScreenData { get; set; }
        public DateTime? LastScreenTime { get; set; }
        public int FrameCount { get; set; }
        public int TargetFps { get; set; } = 15;
        public int Quality { get; set; } = 70; // JPEG quality
    }

    public enum RemoteSessionStatus
    {
        Connecting,
        WaitingForAccept,
        Active,
        Paused,
        Disconnected,
        Error
    }

    public class MouseInputData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public RemoteMouseButton Button { get; set; }
        public RemoteMouseAction Action { get; set; }
        public int WheelDelta { get; set; }
    }

    // Renamed to avoid conflicts
    public enum RemoteMouseButton
    {
        None,
        Left,
        Right,
        Middle
    }

    // Renamed to avoid conflicts
    public enum RemoteMouseAction
    {
        Move,
        Down,
        Up,
        Click,
        DoubleClick,
        Wheel
    }

    public class KeyboardInputData
    {
        public int Key { get; set; }
        public bool IsKeyDown { get; set; }
        public string Modifiers { get; set; } = string.Empty; // Ctrl, Alt, Shift
    }
}
