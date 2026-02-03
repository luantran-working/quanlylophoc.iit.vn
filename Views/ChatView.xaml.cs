
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public class ChatMessageViewModel : ChatMessage
    {
        public bool IsMine { get; set; }

        public ChatMessageViewModel(ChatMessage msg, bool isMine)
        {
            this.Id = msg.Id;
            this.SessionId = msg.SessionId;
            this.SenderType = msg.SenderType;
            this.SenderId = msg.SenderId;
            this.SenderName = msg.SenderName;
            this.ReceiverId = msg.ReceiverId;
            this.Content = msg.Content;
            this.IsGroup = msg.IsGroup;
            this.IsRead = msg.IsRead;
            this.CreatedAt = msg.CreatedAt;
            this.ContentType = msg.ContentType;
            this.AttachmentPath = msg.AttachmentPath;
            this.GroupId = msg.GroupId;

            this.IsMine = isMine;
        }
    }

    public partial class ChatView : UserControl
    {
        public ObservableCollection<ChatMessageViewModel> Messages { get; set; } = new();
        public ObservableCollection<ChatGroupViewModel> Conversations { get; set; } = new();

        public bool IsTeacherMode { get; set; } = true;
        
        private ChatGroupViewModel? _selectedConversation;
        private readonly ChatService _chatService;

        public ChatView()
        {
            InitializeComponent();
            MessageList.ItemsSource = Messages;
            ConversationList.ItemsSource = Conversations;

            _chatService = ChatService.Instance;
            _chatService.MessageReceived += OnMessageReceived;
            _chatService.StudentOnline += OnStudentOnline;
            _chatService.StudentOffline += OnStudentOffline;

            Loaded += OnLoaded;
        }

        /// <summary>
        /// Set Teacher or Student mode
        /// </summary>
        public void SetMode(bool isTeacher)
        {
            IsTeacherMode = isTeacher;
            
            // Hide create group button for students
            CreateGroupBtn.Visibility = isTeacher ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Shorthand for student mode
        /// </summary>
        public void SetStudentMode()
        {
            SetMode(false);
        }

        /// <summary>
        /// Initialize conversations list
        /// </summary>
        public void InitializeConversations()
        {
            Conversations.Clear();

            // Always add public chat as first conversation
            var publicChat = new ChatGroupViewModel
            {
                Id = ChatService.PUBLIC_CHAT_ID,
                Name = "Chat chung",
                Type = ChatGroupType.Public,
                LastMessage = "Nhấn để chat với cả lớp"
            };
            Conversations.Add(publicChat);

            // For teacher mode, add existing online students
            if (IsTeacherMode)
            {
                foreach (var student in _chatService.GetOnlineStudents())
                {
                    AddStudentConversation(student, select: false);
                }
            }
            else
            {
                // For student mode, add teacher as private chat option
                var teacherChat = new ChatGroupViewModel
                {
                    Id = _chatService.GetPrivateChatKey("teacher"),
                    Name = "Giáo viên",
                    Type = ChatGroupType.Private,
                    LastMessage = "Nhấn để chat riêng với giáo viên"
                };
                Conversations.Add(teacherChat);
            }

            // Select public chat by default
            ConversationList.SelectedIndex = 0;
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InitializeConversations();
        }

        /// <summary>
        /// Add or focus private chat with a student
        /// </summary>
        public void SetPrivateChat(Student student, bool select = true)
        {
            Dispatcher.Invoke(() =>
            {
                AddStudentConversation(student, select);
            });
        }

        private void AddStudentConversation(Student student, bool select)
        {
            // Check if conversation already exists
            var existing = Conversations.FirstOrDefault(c => 
                c.Type == ChatGroupType.Private && c.Id == _chatService.GetPrivateChatKey(student.MachineId));
            
            if (existing == null)
            {
                var privateChat = new ChatGroupViewModel
                {
                    Id = _chatService.GetPrivateChatKey(student.MachineId),
                    Name = student.DisplayName,
                    Type = ChatGroupType.Private,
                    PartnerId = student.Id,
                    LastMessage = "Nhấn để chat riêng"
                };
                privateChat.Members.Add(student);
                Conversations.Add(privateChat);
                existing = privateChat;
            }

            if (select)
            {
                ConversationList.SelectedItem = existing;
            }
        }

        private void LoadGroups()
        {
            // Custom groups not implemented for in-memory chat
        }

        private void LoadOnlineStudents()
        {
            // Handled via events
        }

        private void LoadMessages()
        {
            if (_selectedConversation == null) return;

            Messages.Clear();

            var conversationId = _selectedConversation.Type == ChatGroupType.Public
                ? ChatService.PUBLIC_CHAT_ID
                : _selectedConversation.Id;

            var messages = _chatService.GetMessages(conversationId);
            foreach (var msg in messages)
            {
                var vm = new ChatMessageViewModel(msg, _chatService.IsMyMessage(msg));
                Messages.Add(vm);
            }

            // Scroll to bottom
            ScrollToBottom();
        }

        private void OnMessageReceived(object? sender, ChatMessage msg)
        {
            Dispatcher.Invoke(() =>
            {
                string msgConversationId;
                if (msg.IsGroup)
                {
                    msgConversationId = ChatService.PUBLIC_CHAT_ID;
                }
                else
                {
                    // For private messages, determine the conversation key
                    if (_chatService.IsMyMessage(msg))
                    {
                        // Message I sent - use receiver's key
                        msgConversationId = _chatService.GetPrivateChatKey(msg.ReceiverId?.ToString() ?? "");
                    }
                    else
                    {
                        // Message received - use sender's key
                        // For teacher, sender is student machine id
                        // For student, sender is "teacher"
                        if (IsTeacherMode)
                        {
                            // Find the student by name or sender info
                            var student = _chatService.GetOnlineStudents()
                                .FirstOrDefault(s => s.DisplayName == msg.SenderName);
                            if (student != null)
                            {
                                msgConversationId = _chatService.GetPrivateChatKey(student.MachineId);
                            }
                            else
                            {
                                msgConversationId = _chatService.GetPrivateChatKey(msg.SenderName);
                            }
                        }
                        else
                        {
                            msgConversationId = _chatService.GetPrivateChatKey("teacher");
                        }
                    }
                }

                // Update conversation last message
                UpdateConversationLastMessage(msgConversationId, msg);

                // If current conversation, add to view
                if (_selectedConversation != null)
                {
                    var currentConvId = _selectedConversation.Type == ChatGroupType.Public
                        ? ChatService.PUBLIC_CHAT_ID
                        : _selectedConversation.Id;

                    if (currentConvId == msgConversationId)
                    {
                        var vm = new ChatMessageViewModel(msg, _chatService.IsMyMessage(msg));
                        Messages.Add(vm);
                        ScrollToBottom();
                    }
                    else
                    {
                        // Increment unread count for other conversations
                        var conv = Conversations.FirstOrDefault(c => c.Id == msgConversationId);
                        if (conv != null)
                        {
                            conv.UnreadCount++;
                        }
                    }
                }
            });
        }

        private void UpdateConversationLastMessage(string conversationId, ChatMessage msg)
        {
            var conv = Conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conv != null)
            {
                string preview = msg.Content;
                if (preview.Length > 30)
                {
                    preview = preview.Substring(0, 30) + "...";
                }
                conv.LastMessage = $"{msg.SenderName}: {preview}";
            }
        }

        private void OnStudentOnline(object? sender, Student student)
        {
            if (!IsTeacherMode) return;

            Dispatcher.Invoke(() =>
            {
                AddStudentConversation(student, select: false);
            });
        }

        private void OnStudentOffline(object? sender, Student student)
        {
            if (!IsTeacherMode) return;

            Dispatcher.Invoke(() =>
            {
                var conv = Conversations.FirstOrDefault(c => 
                    c.Type == ChatGroupType.Private && 
                    c.Members.Any(m => m.MachineId == student.MachineId));
                
                if (conv != null)
                {
                    conv.LastMessage = "(Offline)";
                }
            });
        }

        private void OnGroupCreated(object? sender, ChatGroup group)
        {
            // Custom groups not implemented
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void AttachBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh để gửi",
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // For simplicity, we'll send the file path as message
                    // In production, you'd want to send the actual file data
                    var fileName = System.IO.Path.GetFileName(dialog.FileName);
                    await SendMessageContent($"[Đính kèm: {fileName}]", "file");
                    
                    ToastService.Instance.ShowInfo("Gửi file", $"Đã gửi file: {fileName}");
                }
                catch (Exception ex)
                {
                    ToastService.Instance.ShowError("Lỗi", $"Không thể gửi file: {ex.Message}");
                }
            }
        }

        private async void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async System.Threading.Tasks.Task SendMessage()
        {
            var content = InputBox.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            await SendMessageContent(content, "text");
            InputBox.Text = "";
        }

        private async System.Threading.Tasks.Task SendMessageContent(string content, string contentType)
        {
            if (_selectedConversation == null) return;

            try
            {
                if (_selectedConversation.Type == ChatGroupType.Public)
                {
                    await _chatService.SendPublicMessageAsync(content, contentType);
                }
                else if (_selectedConversation.Type == ChatGroupType.Private)
                {
                    // Get target id
                    string targetId;
                    if (IsTeacherMode)
                    {
                        // Get student machine id from conversation
                        var student = _selectedConversation.Members.FirstOrDefault();
                        targetId = student?.MachineId ?? "";
                    }
                    else
                    {
                        targetId = "teacher";
                    }

                    if (!string.IsNullOrEmpty(targetId))
                    {
                        await _chatService.SendPrivateMessageAsync(targetId, content, contentType);
                    }
                }
            }
            catch (Exception ex)
            {
                ToastService.Instance.ShowError("Lỗi gửi tin nhắn", ex.Message);
            }
        }

        private void CreateGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            // Custom group creation - for future implementation
            ToastService.Instance.ShowInfo("Thông báo", "Tính năng tạo nhóm sẽ được cập nhật trong phiên bản sau.");
        }

        private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationList.SelectedItem is ChatGroupViewModel selected)
            {
                _selectedConversation = selected;
                
                // Reset unread count
                selected.UnreadCount = 0;

                // Update header
                ChatTitleText.Text = selected.Name;
                
                if (selected.Type == ChatGroupType.Public)
                {
                    ChatSubtitleText.Text = "• Chat chung cả lớp";
                }
                else if (selected.Type == ChatGroupType.Private)
                {
                    ChatSubtitleText.Text = "• Chat riêng";
                }
                else
                {
                    ChatSubtitleText.Text = $"• {selected.Members.Count} thành viên";
                }

                // Load messages for this conversation
                LoadMessages();
            }
        }

        private void ScrollToBottom()
        {
            if (MessageList.Items.Count > 0)
            {
                MessageList.ScrollIntoView(MessageList.Items[MessageList.Items.Count - 1]);
            }
        }
    }
}
