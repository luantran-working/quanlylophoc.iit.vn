using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Quản lý phiên làm việc cho Giáo viên
    /// </summary>
    public class SessionManager : INotifyPropertyChanged
    {
        private static SessionManager? _instance;
        public static SessionManager Instance => _instance ??= new SessionManager();

        private readonly DatabaseService _db;
        private readonly NetworkServerService _networkServer;
        private readonly ScreenCaptureService _screenCapture;

        private User? _currentUser;
        private Session? _currentSession;
        private bool _isScreenSharing;
        private bool _isRunning;

        public User? CurrentUser
        {
            get => _currentUser;
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public Session? CurrentSession
        {
            get => _currentSession;
            set { _currentSession = value; OnPropertyChanged(); }
        }

        public bool IsScreenSharing
        {
            get => _isScreenSharing;
            set { _isScreenSharing = value; OnPropertyChanged(); }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Student> OnlineStudents { get; } = new();
        public ObservableCollection<ChatMessage> ChatMessages { get; } = new();

        public NetworkServerService NetworkServer => _networkServer;
        public ScreenCaptureService ScreenCapture => _screenCapture;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Student>? StudentConnected;
        public event EventHandler<Student>? StudentDisconnected;
        public event EventHandler<ChatMessage>? ChatMessageReceived;

        private SessionManager()
        {
            _db = DatabaseService.Instance;
            _networkServer = new NetworkServerService();
            _screenCapture = new ScreenCaptureService();

            // Wire up network events
            _networkServer.ClientConnected += OnClientConnected;
            _networkServer.ClientDisconnected += OnClientDisconnected;
            _networkServer.MessageReceived += OnMessageReceived;
            _networkServer.ScreenDataReceived += OnScreenDataReceived;
        }

        /// <summary>
        /// Đăng nhập giáo viên
        /// </summary>
        public bool Login(string username, string password)
        {
            var user = _db.ValidateUser(username, password);
            if (user != null)
            {
                CurrentUser = user;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Bắt đầu phiên học
        /// </summary>
        public async Task<bool> StartSessionAsync(string className, string subject, int port = 5000)
        {
            if (CurrentUser == null) return false;

            try
            {
                // Create session in DB
                var sessionId = _db.CreateSession(CurrentUser.Id, className, subject);
                CurrentSession = new Session
                {
                    Id = sessionId,
                    UserId = CurrentUser.Id,
                    ClassName = className,
                    Subject = subject,
                    StartTime = DateTime.Now,
                    Status = "active"
                };

                // Start network server
                _networkServer.ClassName = className;
                _networkServer.TeacherName = CurrentUser.DisplayName;
                await _networkServer.StartAsync(port);

                IsRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể khởi động phiên: {ex.Message}", "Lỗi", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Kết thúc phiên học
        /// </summary>
        public void EndSession()
        {
            if (CurrentSession != null)
            {
                _db.EndSession(CurrentSession.Id);
            }

            _networkServer.Stop();
            IsRunning = false;
            IsScreenSharing = false;

            OnlineStudents.Clear();
            ChatMessages.Clear();
            CurrentSession = null;
        }

        /// <summary>
        /// Bắt đầu chia sẻ màn hình
        /// </summary>
        public async Task StartScreenShareAsync()
        {
            IsScreenSharing = true;

            while (IsScreenSharing)
            {
                try
                {
                    var screenData = _screenCapture.CaptureScreen(60); // 60% quality for streaming
                    await _networkServer.SendScreenShareAsync(screenData);
                    await Task.Delay(50); // ~20 FPS
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Screen share error: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Dừng chia sẻ màn hình
        /// </summary>
        public async Task StopScreenShareAsync()
        {
            IsScreenSharing = false;
            await _networkServer.BroadcastToAllAsync(new NetworkMessage
            {
                Type = MessageType.ScreenShareStop,
                SenderId = "server"
            });
        }

        /// <summary>
        /// Khóa màn hình học sinh
        /// </summary>
        public async Task LockStudentAsync(string clientId, bool lockScreen)
        {
            await _networkServer.SendToClientAsync(clientId, new NetworkMessage
            {
                Type = lockScreen ? MessageType.LockScreen : MessageType.UnlockScreen,
                SenderId = "server"
            });

            // Update in database
            _db.SetStudentLocked(clientId, lockScreen);

            // Update UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var student in OnlineStudents)
                {
                    if (student.MachineId == clientId)
                    {
                        student.IsLocked = lockScreen;
                        break;
                    }
                }
            });
        }

        /// <summary>
        /// Khóa tất cả màn hình
        /// </summary>
        public async Task LockAllStudentsAsync(bool lockScreen)
        {
            await _networkServer.BroadcastToAllAsync(new NetworkMessage
            {
                Type = lockScreen ? MessageType.LockScreen : MessageType.UnlockScreen,
                SenderId = "server"
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var student in OnlineStudents)
                {
                    student.IsLocked = lockScreen;
                    _db.SetStudentLocked(student.MachineId, lockScreen);
                }
            });
        }

        /// <summary>
        /// Gửi tin nhắn chat
        /// </summary>
        public async Task SendChatMessageAsync(string content, int? studentId = null)
        {
            if (CurrentSession == null || CurrentUser == null) return;

            var chatMessage = new ChatMessage
            {
                SessionId = CurrentSession.Id,
                SenderType = "teacher",
                SenderId = CurrentUser.Id,
                SenderName = CurrentUser.DisplayName,
                ReceiverId = studentId,
                Content = content,
                IsGroup = !studentId.HasValue,
                CreatedAt = DateTime.Now
            };

            // Save to DB
            _db.SaveChatMessage(chatMessage);

            // Send to clients
            var networkMsg = new NetworkMessage
            {
                Type = studentId.HasValue ? MessageType.ChatPrivate : MessageType.ChatMessage,
                SenderId = "server",
                SenderName = CurrentUser.DisplayName,
                TargetId = studentId?.ToString(),
                Payload = content
            };

            if (studentId.HasValue)
            {
                // Find client ID by student ID
                var student = OnlineStudents.FirstOrDefault(s => s.Id == studentId);
                if (student != null)
                {
                    await _networkServer.SendToClientAsync(student.MachineId, networkMsg);
                }
            }
            else
            {
                await _networkServer.BroadcastToAllAsync(networkMsg);
            }

            // Update UI
            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Add(chatMessage);
            });
        }

        // Event Handlers
        private void OnClientConnected(object? sender, ClientConnectedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var student = _db.GetOrCreateStudent(
                    e.ClientId, 
                    e.ClientName, 
                    e.ClientInfo?.ComputerName ?? "", 
                    e.IpAddress);

                if (student != null)
                {
                    OnlineStudents.Add(student);
                    StudentConnected?.Invoke(this, student);
                    
                    // Show toast notification
                    ToastService.Instance.ShowSuccess(
                        "Học sinh kết nối",
                        $"{student.DisplayName} đã tham gia lớp học\nIP: {e.IpAddress}");
                }
            });
        }

        private void OnClientDisconnected(object? sender, ClientDisconnectedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _db.SetStudentOnline(e.ClientId, false);

                var student = OnlineStudents.FirstOrDefault(s => s.MachineId == e.ClientId);
                if (student != null)
                {
                    OnlineStudents.Remove(student);
                    StudentDisconnected?.Invoke(this, student);
                    
                    // Show toast notification
                    ToastService.Instance.ShowWarning(
                        "Học sinh ngắt kết nối",
                        $"{student.DisplayName} đã rời khỏi lớp học");
                }
            });
        }

        private void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            switch (e.Message.Type)
            {
                case MessageType.ChatMessage:
                case MessageType.ChatPrivate:
                    HandleChatMessage(e);
                    break;

                case MessageType.RaiseHand:
                    HandleRaiseHand(e);
                    break;

                case MessageType.FileStart:
                    HandleFileTransfer(e);
                    break;
            }
        }

        private void HandleChatMessage(MessageReceivedEventArgs e)
        {
            if (CurrentSession == null) return;

            var student = OnlineStudents.FirstOrDefault(s => s.MachineId == e.ClientId);
            if (student == null) return;

            var chatMessage = new ChatMessage
            {
                SessionId = CurrentSession.Id,
                SenderType = "student",
                SenderId = student.Id,
                SenderName = student.DisplayName,
                Content = e.Message.Payload ?? "",
                IsGroup = e.Message.Type == MessageType.ChatMessage,
                CreatedAt = DateTime.Now
            };

            _db.SaveChatMessage(chatMessage);

            Application.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Add(chatMessage);
                ChatMessageReceived?.Invoke(this, chatMessage);
            });
        }

        private void HandleRaiseHand(MessageReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Show notification
                MessageBox.Show($"{e.Message.SenderName} giơ tay!", "Thông báo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void HandleFileTransfer(MessageReceivedEventArgs e)
        {
            // Handle file transfer
        }

        private void OnScreenDataReceived(object? sender, ScreenDataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var student = OnlineStudents.FirstOrDefault(s => s.MachineId == e.ClientId);
                if (student != null)
                {
                    student.ScreenThumbnail = e.ScreenData.ImageData;
                }
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
