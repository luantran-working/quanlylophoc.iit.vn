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
        private readonly ScreenshotService _screenshotService;

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
        public ObservableCollection<SystemInfoPackage> OnlineStudentsSystemInfo { get; } = new();

        public NetworkServerService NetworkServer => _networkServer;
        public ScreenCaptureService ScreenCapture => _screenCapture;
        public ScreenshotService ScreenshotService => _screenshotService;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Student>? StudentConnected;
        public event EventHandler<Student>? StudentDisconnected;
        public event EventHandler<ChatMessage>? ChatMessageReceived;
        public event EventHandler<ProcessListReceivedEventArgs>? ProcessListReceived;

        private SessionManager()
        {
            _db = DatabaseService.Instance;
            _networkServer = new NetworkServerService();
            _screenCapture = new ScreenCaptureService();
            _screenshotService = new ScreenshotService();

            // Wire up network events
            _networkServer.ClientConnected += OnClientConnected;
            _networkServer.ClientDisconnected += OnClientDisconnected;
            _networkServer.MessageReceived += OnMessageReceived;
            _networkServer.ScreenDataReceived += OnScreenDataReceived;
            _networkServer.ScreenshotReceived += OnScreenshotReceived;

            ChatService.Instance.Initialize(_networkServer, null);
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
        /// Yêu cầu tất cả học sinh gửi thông tin cấu hình máy tính
        /// </summary>
        public async Task RequestAllSystemSpecsAsync()
        {
            await _networkServer.BroadcastToAllAsync(new NetworkMessage
            {
                Type = MessageType.SystemSpecsRequest,
                SenderId = "server"
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
            var log = LogService.Instance;
            log.Info("SessionManager", $"OnClientConnected triggered: {e.ClientName} ({e.ClientId}) from {e.IpAddress}");

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    log.Debug("SessionManager", "Inside Dispatcher.Invoke");

                    var student = _db.GetOrCreateStudent(
                        e.ClientId,
                        e.ClientName ?? "Unknown",
                        e.ClientInfo?.ComputerName ?? "",
                        e.IpAddress);

                    log.Debug("SessionManager", $"GetOrCreateStudent result: {student?.DisplayName ?? "null"}");

                    if (student != null)
                    {
                        OnlineStudents.Add(student);
                        log.Info("SessionManager", $"Student added to OnlineStudents. Count: {OnlineStudents.Count}");

                        StudentConnected?.Invoke(this, student);

                        // Show toast notification
                        log.Debug("SessionManager", "Calling ToastService.ShowSuccess...");
                        ToastService.Instance.ShowSuccess(
                            "Học sinh kết nối",
                            $"{student.DisplayName} đã tham gia lớp học\nIP: {e.IpAddress}");
                    }
                    else
                    {
                        log.Warning("SessionManager", "GetOrCreateStudent returned null!");
                    }
                });
            }
            catch (Exception ex)
            {
                log.Error("SessionManager", "Error in OnClientConnected", ex);
            }
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


                case MessageType.RaiseHand:
                    HandleRaiseHand(e);
                    break;

                case MessageType.FileStart:
                    HandleFileTransfer(e);
                    break;

                case MessageType.AssignmentSubmit:
                    HandleAssignmentSubmit(e);
                    break;

                case MessageType.SystemSpecsResponse:
                    HandleSystemSpecsResponse(e);
                    break;

                case MessageType.ProcessListResponse:
                    HandleProcessListResponse(e);
                    break;

                case MessageType.FileCollectionData:
                    HandleFileCollectionData(e);
                    break;

                case MessageType.FileCollectionStatus:
                    HandleFileCollectionStatus(e);
                    break;

                case MessageType.PollVote:
                    HandlePollVote(e);
                    break;

                case MessageType.ChatMessage:
                case MessageType.ChatPrivate:
                    HandleChatMessage(e);
                    break;

                case MessageType.ChatImageUpload:
                    HandleChatImageUpload(e);
                    break;
            }
        }

        private async void HandleChatMessage(MessageReceivedEventArgs e)
        {
            if (e.Message.Payload == null) return;
            try
            {
                var msg = System.Text.Json.JsonSerializer.Deserialize<ChatMessage>(e.Message.Payload);
                if (msg != null)
                {
                    // Update Sender Info from Network Info to be safe
                    msg.SenderType = "student";
                    msg.SenderId = DatabaseService.Instance.GetOrCreateStudent(e.Message.SenderId, e.Message.SenderName, "", "")?.Id ?? 0;

                    msg.Id = DatabaseService.Instance.SaveChatMessage(msg);
                    await ChatService.Instance.BroadcastMessageAsync(msg);
                }
            } catch {}
        }

        private async void HandleChatImageUpload(MessageReceivedEventArgs e)
        {
            if (e.Message.Payload == null) return;
            await ChatService.Instance.HandleImageUploadAsync(e.Message.SenderId, e.Message.Payload);
        }

        private async void HandlePollVote(MessageReceivedEventArgs e)
        {
             if (e.Message.Payload == null) return;
             try
             {
                 var vote = System.Text.Json.JsonSerializer.Deserialize<Models.PollVote>(e.Message.Payload);
                 if (vote != null)
                 {
                     await PollService.Instance.ProcessVoteAsync(vote);
                 }
             }
             catch (Exception ex)
             {
                 LogService.Instance.Error("SessionManager", "Error processing vote", ex);
             }
        }

        private void HandleFileCollectionData(MessageReceivedEventArgs e)
        {
            if (e.Message.Payload == null) return;
            try
            {
                var fileData = System.Text.Json.JsonSerializer.Deserialize<CollectedFile>(e.Message.Payload);
                if (fileData != null)
                {
                    // Save file
                    string baseDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CollectedFiles");
                    string sessionDir = CurrentSession?.Id.ToString() ?? "UnknownSession";
                    string studentDir = $"{fileData.StudentName}_{fileData.StudentMachineId}";

                    // Combine and create directory
                    string targetDir = System.IO.Path.Combine(baseDir, sessionDir, studentDir);
                    if (!string.IsNullOrEmpty(fileData.RelativePath) && fileData.RelativePath != fileData.FileName)
                    {
                         // Maintain relative structure if possible, but sanitize
                         string relDir = System.IO.Path.GetDirectoryName(fileData.RelativePath) ?? "";
                         targetDir = System.IO.Path.Combine(targetDir, relDir);
                    }

                    System.IO.Directory.CreateDirectory(targetDir);

                    string savePath = System.IO.Path.Combine(targetDir, fileData.FileName);
                    System.IO.File.WriteAllBytes(savePath, fileData.Content);

                    // Update UI status (optional log)
                    // LogService.Instance.Info("SessionManager", $"Received file {fileData.FileName} from {fileData.StudentName}");
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("SessionManager", "Error saving collected file", ex);
            }
        }

        private void HandleFileCollectionStatus(MessageReceivedEventArgs e)
        {
            if (e.Message.Payload == null) return;
            try
            {
                var status = System.Text.Json.JsonSerializer.Deserialize<FileCollectionStatus>(e.Message.Payload);
                if (status != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var student = OnlineStudents.FirstOrDefault(s => s.MachineId == e.ClientId);
                        if (student != null)
                        {
                            student.CollectionStatus = $"{status.ProcessedFiles}/{status.TotalFiles} - {status.Message}";
                        }
                    });
                }
            }
            catch {}
        }

        private async void HandleAssignmentSubmit(MessageReceivedEventArgs e)
        {
            if (e.Message.Payload == null) return;

            try
            {
                var submission = System.Text.Json.JsonSerializer.Deserialize<AssignmentSubmission>(e.Message.Payload);
                if (submission != null)
                {
                     // Ensure SessionId is from current active session
                     if (CurrentSession != null)
                     {
                         submission.SessionId = CurrentSession.Id.ToString();
                     }

                     // Use AssignmentService to process
                     await AssignmentService.Instance.ProcessSubmissionAsync(submission);

                     // Notify UI
                     Application.Current.Dispatcher.Invoke(() =>
                     {
                         ToastService.Instance.ShowSuccess(
                             "Nộp bài tập",
                             $"Học sinh {submission.StudentName} đã nộp bài.");
                     });

                     // Send ACK
                     await _networkServer.SendToClientAsync(e.ClientId, new NetworkMessage
                     {
                         Type = MessageType.AssignmentSubmitAck,
                         SenderId = "server",
                         TargetId = e.ClientId,
                         Payload = "Received successfully"
                     });
                }
            }
            catch (Exception ex)
            {
                 LogService.Instance.Error("SessionManager", "Error handling assignment submission", ex);
            }
        }

        private void HandleSystemSpecsResponse(MessageReceivedEventArgs e)
        {
            if (e.Message.Payload == null) return;
            try
            {
                var info = System.Text.Json.JsonSerializer.Deserialize<SystemInfoPackage>(e.Message.Payload);
                if (info != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        var existing = OnlineStudentsSystemInfo.FirstOrDefault(s => s.MachineId == info.MachineId);
                        if (existing != null)
                        {
                            OnlineStudentsSystemInfo.Remove(existing);
                        }
                        OnlineStudentsSystemInfo.Add(info);
                    });
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("SessionManager", "Error parsing system specs response", ex);
            }
        }



        private void HandleProcessListResponse(MessageReceivedEventArgs e)
        {
            if (e.Message.Payload == null) return;
            try
            {
                var processes = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<ProcessInfo>>(e.Message.Payload);
                if (processes != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProcessListReceived?.Invoke(this, new ProcessListReceivedEventArgs
                        {
                            ClientId = e.ClientId,
                            Processes = processes
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("SessionManager", "Error parsing process list response", ex);
            }
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

            // Forward to RemoteControlService for remote control window
            if (RemoteControlService.Instance.IsSessionActive(e.ClientId))
            {
                RemoteControlService.Instance.HandleScreenData(e.ClientId, e.ScreenData.ImageData);
            }
        }

        private async void OnScreenshotReceived(object? sender, ScreenDataReceivedEventArgs e)
        {
            try
            {
                var student = OnlineStudents.FirstOrDefault(s => s.MachineId == e.ClientId);
                string studentName = student?.DisplayName ?? "Unknown";
                int sessionId = CurrentSession?.Id ?? 0;

                var screenshot = await _screenshotService.CaptureAndSaveAsync(e.ClientId, studentName, sessionId, e.ScreenData.ImageData);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ToastService.Instance.ShowSuccess("Chụp ảnh thành công", $"Đã lưu ảnh màn hình của {studentName}");
                });
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("SessionManager", "Error saving screenshot", ex);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ProcessListReceivedEventArgs : EventArgs
    {
        public string ClientId { get; set; } = string.Empty;
        public System.Collections.Generic.List<ProcessInfo> Processes { get; set; } = new();
    }
}
