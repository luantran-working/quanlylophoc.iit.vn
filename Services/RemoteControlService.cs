using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service quản lý điều khiển từ xa máy học sinh
    /// </summary>
    public class RemoteControlService
    {
        private static RemoteControlService? _instance;
        public static RemoteControlService Instance => _instance ??= new RemoteControlService();

        private readonly LogService _log = LogService.Instance;
        private readonly ConcurrentDictionary<string, RemoteSession> _activeSessions = new();

        public event EventHandler<RemoteSession>? SessionStarted;
        public event EventHandler<RemoteSession>? SessionEnded;
        public event EventHandler<byte[]>? ScreenReceived;
        public event EventHandler<string>? ConnectionStatusChanged;

        private RemoteControlService() { }

        /// <summary>
        /// Request remote control of a student's machine
        /// </summary>
        public async Task<RemoteSession?> RequestControlAsync(Student student)
        {
            try
            {
                if (student == null)
                {
                    _log.Error("RemoteControl", "Student is null");
                    return null;
                }

                _log.Info("RemoteControl", $"Requesting control of {student.DisplayName}");

                // Check if session manager and network server are available
                var sessionManager = SessionManager.Instance;
                if (sessionManager == null)
                {
                    _log.Error("RemoteControl", "SessionManager is null");
                    return null;
                }

                var networkServer = sessionManager.NetworkServer;
                if (networkServer == null)
                {
                    _log.Error("RemoteControl", "NetworkServer is null - session may not be started");
                    return null;
                }

                var session = new RemoteSession
                {
                    StudentMachineId = student.MachineId,
                    StudentName = student.DisplayName,
                    StudentIpAddress = student.IpAddress,
                    Status = RemoteSessionStatus.Connecting,
                    StartTime = DateTime.Now
                };

                // Send control request to student
                var message = new NetworkMessage
                {
                    Type = MessageType.ControlStart,
                    SenderId = "server",
                    SenderName = "Giáo viên"
                };

                await networkServer.SendToClientAsync(student.MachineId, message);

                // Wait for response (with timeout)
                session.Status = RemoteSessionStatus.WaitingForAccept;

                // For now, auto-accept (in real implementation, wait for student response)
                await Task.Delay(500);

                session.Status = RemoteSessionStatus.Active;
                session.IsControlEnabled = true;

                _activeSessions[student.MachineId] = session;
                SessionStarted?.Invoke(this, session);

                _log.Info("RemoteControl", $"Remote control session started for {student.DisplayName}");
                return session;
            }
            catch (Exception ex)
            {
                _log.Error("RemoteControl", $"Failed to start remote control for {student?.DisplayName ?? "unknown"}", ex);
                return null;
            }
        }

        /// <summary>
        /// End a remote control session
        /// </summary>
        public async Task EndSessionAsync(string machineId)
        {
            if (!_activeSessions.TryRemove(machineId, out var session))
                return;

            try
            {
                session.Status = RemoteSessionStatus.Disconnected;
                session.EndTime = DateTime.Now;

                // Notify student
                var message = new NetworkMessage
                {
                    Type = MessageType.ControlStop,
                    SenderId = "server"
                };

                var networkServer = SessionManager.Instance.NetworkServer;
                await networkServer.SendToClientAsync(machineId, message);

                SessionEnded?.Invoke(this, session);
                _log.Info("RemoteControl", $"Remote control session ended for {session.StudentName}");
            }
            catch (Exception ex)
            {
                _log.Error("RemoteControl", "Failed to end remote control session", ex);
            }
        }

        /// <summary>
        /// Send mouse input to remote machine
        /// </summary>
        public async Task SendMouseInputAsync(string machineId, MouseInputData input)
        {
            if (!_activeSessions.TryGetValue(machineId, out var session) || !session.IsControlEnabled)
                return;

            try
            {
                // Map to NetworkModels.MouseCommand
                var cmd = new Models.MouseCommand
                {
                    X = (int)(input.X * 65535), // Normalized to absolute coords usually
                    Y = (int)(input.Y * 65535),
                    Action = MapMouseAction(input),
                    Delta = input.WheelDelta
                };

                var message = new NetworkMessage
                {
                    Type = MessageType.ControlMouse,
                    SenderId = "server",
                    Payload = JsonSerializer.Serialize(cmd)
                };

                var networkServer = SessionManager.Instance.NetworkServer;
                await networkServer.SendToClientAsync(machineId, message);
                // _log.Debug("RemoteControl", $"Sent mouse input: {input.Action} ({cmd.X},{cmd.Y})"); // Log occasionally or debug
            }
            catch (Exception ex)
            {
                _log.Error("RemoteControl", "Failed to send mouse input", ex);
            }
        }

        private Models.MouseAction MapMouseAction(MouseInputData input)
        {
            if (input.Action == RemoteMouseAction.Move) return Models.MouseAction.Move;
            if (input.Action == RemoteMouseAction.Wheel) return Models.MouseAction.Scroll;

            if (input.Button == RemoteMouseButton.Left)
            {
                if (input.Action == RemoteMouseAction.Down) return Models.MouseAction.LeftDown;
                if (input.Action == RemoteMouseAction.Up) return Models.MouseAction.LeftUp;
                if (input.Action == RemoteMouseAction.DoubleClick) return Models.MouseAction.LeftDoubleClick;
            }
            else if (input.Button == RemoteMouseButton.Right)
            {
                if (input.Action == RemoteMouseAction.Down) return Models.MouseAction.RightDown;
                if (input.Action == RemoteMouseAction.Up) return Models.MouseAction.RightUp;
            }
            else if (input.Button == RemoteMouseButton.Middle)
            {
                if (input.Action == RemoteMouseAction.Down) return Models.MouseAction.MiddleDown;
                if (input.Action == RemoteMouseAction.Up) return Models.MouseAction.MiddleUp;
            }

            return Models.MouseAction.Move; // Fallback
        }

        /// <summary>
        /// Send keyboard input to remote machine
        /// </summary>
        public async Task SendKeyboardInputAsync(string machineId, KeyboardInputData input)
        {
            if (!_activeSessions.TryGetValue(machineId, out var session) || !session.IsControlEnabled)
                return;

            try
            {
                var cmd = new Models.KeyboardCommand
                {
                    KeyCode = input.Key,
                    Action = input.IsKeyDown ? Models.KeyAction.Down : Models.KeyAction.Up,
                    Ctrl = input.Modifiers.Contains("Ctrl"),
                    Alt = input.Modifiers.Contains("Alt"),
                    Shift = input.Modifiers.Contains("Shift")
                };

                var message = new NetworkMessage
                {
                    Type = MessageType.ControlKeyboard,
                    SenderId = "server",
                    Payload = JsonSerializer.Serialize(cmd)
                };

                var networkServer = SessionManager.Instance.NetworkServer;
                await networkServer.SendToClientAsync(machineId, message);
            }
            catch (Exception ex)
            {
                _log.Error("RemoteControl", "Failed to send keyboard input", ex);
            }
        }

        /// <summary>
        /// Lock/unlock student input
        /// </summary>
        public async Task SetInputLockAsync(string machineId, bool locked)
        {
            if (!_activeSessions.TryGetValue(machineId, out var session))
                return;

            try
            {
                session.IsInputLocked = locked;

                var message = new NetworkMessage
                {
                    Type = locked ? MessageType.LockScreen : MessageType.UnlockScreen,
                    SenderId = "server",
                    Payload = JsonSerializer.Serialize(new { Message = locked ? "Máy tính đã bị khóa bởi giáo viên" : "" })
                };

                var networkServer = SessionManager.Instance.NetworkServer;
                await networkServer.SendToClientAsync(machineId, message);

                _log.Info("RemoteControl", $"Input {(locked ? "locked" : "unlocked")} for {session.StudentName}");
            }
            catch (Exception ex)
            {
                _log.Error("RemoteControl", "Failed to set input lock", ex);
            }
        }

        /// <summary>
        /// Toggle view-only mode (no control, just viewing)
        /// </summary>
        public void SetViewOnlyMode(string machineId, bool viewOnly)
        {
            if (_activeSessions.TryGetValue(machineId, out var session))
            {
                session.IsControlEnabled = !viewOnly;
                _log.Info("RemoteControl", $"View-only mode {(viewOnly ? "enabled" : "disabled")} for {session.StudentName}");
            }
        }

        /// <summary>
        /// Take screenshot of remote screen
        /// </summary>
        public async Task<byte[]?> TakeScreenshotAsync(string machineId)
        {
            if (!_activeSessions.TryGetValue(machineId, out var session))
                return null;

            try
            {
                // Request current screen from student
                var message = new NetworkMessage
                {
                    Type = MessageType.ScreenRequest,
                    SenderId = "server",
                    Payload = JsonSerializer.Serialize(new { Quality = session.Quality })
                };

                var networkServer = SessionManager.Instance.NetworkServer;
                await networkServer.SendToClientAsync(machineId, message);

                // In real implementation, wait for response
                await Task.Delay(100);

                return session.LastScreenData;
            }
            catch (Exception ex)
            {
                _log.Error("RemoteControl", "Failed to take screenshot", ex);
                return null;
            }
        }

        /// <summary>
        /// Get active session for a student
        /// </summary>
        public RemoteSession? GetSession(string machineId)
        {
            _activeSessions.TryGetValue(machineId, out var session);
            return session;
        }

        /// <summary>
        /// Check if a session is active
        /// </summary>
        public bool IsSessionActive(string machineId)
        {
            return _activeSessions.ContainsKey(machineId);
        }

        /// <summary>
        /// Handle incoming screen data from student
        /// </summary>
        public void HandleScreenData(string machineId, byte[] screenData)
        {
            if (_activeSessions.TryGetValue(machineId, out var session))
            {
                session.LastScreenData = screenData;
                session.LastScreenTime = DateTime.Now;
                session.FrameCount++;

                ScreenReceived?.Invoke(this, screenData);
            }
        }
    }
}
