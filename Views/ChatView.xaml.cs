
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

        public ChatView()
        {
            InitializeComponent();
            MessageList.ItemsSource = Messages;
            ConversationList.ItemsSource = Conversations;

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;

            // Load Conversations (Default: Public Chat)
            var publicChat = new ChatGroupViewModel
            {
                Id = "public",
                Name = "Lớp học chung",
                Type = ChatGroupType.Public
            };
            Conversations.Add(publicChat);
            _selectedConversation = publicChat;
            ConversationList.SelectedItem = publicChat;

            // Check Mode based on ChatService state
            IsTeacherMode = ChatService.Instance.IsMyMessage(new ChatMessage { SenderType = "teacher" });

            if (IsTeacherMode)
            {
                 LoadGroups();
            }
            else
            {
                 if (CreateGroupBtn != null) CreateGroupBtn.Visibility = Visibility.Collapsed;
            }

            ChatService.Instance.MessageReceived += OnMessageReceived;
            ChatService.Instance.GroupCreated += OnGroupCreated;

            // Load initial messages
            LoadMessages();
        }

        public void SetPrivateChat(Student student)
        {
            // Find existing private chat
            var conv = Conversations.FirstOrDefault(c => c.Type == ChatGroupType.Private && c.PartnerId == student.Id);
            
            if (conv == null)
            {
                conv = new ChatGroupViewModel
                {
                    Id = $"private_{student.Id}",
                    Name = student.DisplayName,
                    Type = ChatGroupType.Private,
                    PartnerId = student.Id
                };
                Conversations.Add(conv);
            }

            ConversationList.SelectedItem = conv;
        }

        private void LoadGroups()
        {
            try {
                var groups = DatabaseService.Instance.GetChatGroups();
                foreach (var g in groups)
                {
                    Conversations.Add(new ChatGroupViewModel 
                    { 
                        Id = g.Id.ToString(), 
                        Name = g.Name,
                        Type = ChatGroupType.Group
                    });
                }
            } catch {}
        }

        private void LoadMessages()
        {
             Messages.Clear();
             try {
                var dbMsgs = DatabaseService.Instance.GetChatMessages(SessionManager.Instance.CurrentSession?.Id ?? 0);
                foreach (var m in dbMsgs)
                {
                    AddMessageIfVisible(m);
                }
             } catch {}
        }

        private void OnMessageReceived(object? sender, ChatMessage msg)
        {
            Dispatcher.Invoke(() => 
            {
                // Update Last Message in Sidebar
                UpdateConversationLastMessage(msg);
                
                // Add to list if it belongs to current view
                AddMessageIfVisible(msg);
            });
        }

        private void UpdateConversationLastMessage(ChatMessage msg)
        {
            ChatGroupViewModel? conv = null;
            if (msg.IsGroup)
            {
                if (string.IsNullOrEmpty(msg.GroupId))
                    conv = Conversations.FirstOrDefault(c => c.Type == ChatGroupType.Public);
                else
                    conv = Conversations.FirstOrDefault(c => c.Id == msg.GroupId);
            }
            else
            {
                // Simple logic: find by PartnerId
                int partnerId = IsTeacherMode 
                    ? (msg.SenderType == "teacher" ? (msg.ReceiverId ?? 0) : msg.SenderId) 
                    : (msg.SenderType == "teacher" ? 0 : (msg.ReceiverId ?? 0));

                conv = Conversations.FirstOrDefault(c => c.Type == ChatGroupType.Private && c.PartnerId == partnerId);
                
                if (conv == null && IsTeacherMode)
                {
                    // For teacher, auto-add private chat if message received
                    var studentId = msg.SenderType == "teacher" ? (msg.ReceiverId ?? 0) : msg.SenderId;
                    var student = SessionManager.Instance.OnlineStudents.FirstOrDefault(s => s.Id == studentId);
                    if (student != null)
                    {
                        conv = new ChatGroupViewModel
                        {
                            Id = $"private_{student.Id}",
                            Name = student.DisplayName,
                            Type = ChatGroupType.Private,
                            PartnerId = student.Id
                        };
                        Conversations.Add(conv);
                    }
                }
            }

            if (conv != null)
            {
                conv.LastMessage = msg.Content;
                if (_selectedConversation != conv)
                    conv.UnreadCount++;
            }
        }

        private void AddMessageIfVisible(ChatMessage msg)
        {
             if (_selectedConversation == null) return;

             bool show = false;
             if (_selectedConversation.Type == ChatGroupType.Public)
             {
                 if (msg.IsGroup && string.IsNullOrEmpty(msg.GroupId)) show = true;
             }
             else if (_selectedConversation.Type == ChatGroupType.Group)
             {
                 if (msg.IsGroup && msg.GroupId == _selectedConversation.Id) show = true;
             }
             else if (_selectedConversation.Type == ChatGroupType.Private)
             {
                 bool involvesPartner = (msg.SenderId == _selectedConversation.PartnerId) || (msg.ReceiverId == _selectedConversation.PartnerId);
                 if (!msg.IsGroup && involvesPartner) show = true;
             }

             if (!show) return;

             bool isMine = ChatService.Instance.IsMyMessage(msg);
             Messages.Add(new ChatMessageViewModel(msg, isMine));
             
             // Scroll to bottom
             if (MessageList.Items.Count > 0)
                MessageList.ScrollIntoView(MessageList.Items[MessageList.Items.Count - 1]);
        }

        private void OnGroupCreated(object? sender, ChatGroup group)
        {
             Dispatcher.Invoke(() =>
            {
                Conversations.Add(new ChatGroupViewModel 
                { 
                    Id = group.Id.ToString(), 
                    Name = group.Name,
                    Type = ChatGroupType.Group
                });
            });
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text) || _selectedConversation == null) return;
            var content = InputBox.Text;
            InputBox.Text = "";

            var msg = new ChatMessage
            {
                SessionId = SessionManager.Instance.CurrentSession?.Id ?? 0,
                SenderType = IsTeacherMode ? "teacher" : "student",
                SenderId = IsTeacherMode ? (SessionManager.Instance.CurrentUser?.Id ?? 0) : 0, // Student side resolving sender id is not implemented here
                SenderName = IsTeacherMode ? (SessionManager.Instance.CurrentUser?.DisplayName ?? "Teacher") : ChatService.Instance.GetClientName(),
                Content = content,
                IsGroup = _selectedConversation.Type != ChatGroupType.Private,
                GroupId = _selectedConversation.Type == ChatGroupType.Group ? _selectedConversation.Id : null,
                ReceiverId = _selectedConversation.Type == ChatGroupType.Private ? (int?)_selectedConversation.PartnerId : null,
                CreatedAt = DateTime.Now
            };

            if (IsTeacherMode)
            {
                msg.Id = DatabaseService.Instance.SaveChatMessage(msg);
                await ChatService.Instance.BroadcastMessageAsync(msg);
                AddMessageIfVisible(msg); 
            }
            else
            {
                await ChatService.Instance.SendTextMessageAsync(content);
            }
        }

        private async void AttachBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Images|*.jpg;*.png;*.jpeg|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                if (IsTeacherMode)
                {
                      try {
                          var bytes = System.IO.File.ReadAllBytes(dlg.FileName);
                          var info = System.Text.Json.JsonSerializer.Serialize(new ChatAttachmentInfo
                          {
                             FileName = System.IO.Path.GetFileName(dlg.FileName),
                             Data = bytes
                          });
                          await ChatService.Instance.HandleImageUploadAsync(
                              SessionManager.Instance.CurrentUser?.Id.ToString() ?? "0",
                              info
                          );
                      } catch {}
                }
                else
                {
                    await ChatService.Instance.SendImageAsync(dlg.FileName);
                }
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendBtn_Click(sender, e);
            }
        }

        private void CreateGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CreateChatGroupDialog();
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() == true)
            {
                // Handled via event
            }
        }

        private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedConversation = ConversationList.SelectedItem as ChatGroupViewModel;
            if (_selectedConversation != null)
            {
                _selectedConversation.UnreadCount = 0;
                ChatTitleText.Text = _selectedConversation.Name;
                ChatSubtitleText.Text = _selectedConversation.Type == ChatGroupType.Public ? "• Cả lớp" : 
                                       (_selectedConversation.Type == ChatGroupType.Private ? "• Trực tuyến" : "• Nhóm");
                
                LoadMessages();
            }
        }
    }
}
