
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
        private int _privatePartnerId = 0;

        public void SetPrivateChat(Student student)
        {
            _privatePartnerId = student.Id;
            IsTeacherMode = true;
            if (ChatTitleText != null) ChatTitleText.Text = $"Chat với {student.DisplayName}";
            if (CreateGroupBtn != null) CreateGroupBtn.Visibility = Visibility.Collapsed;
            Messages.Clear();
            LoadMessages();
        }

        public ChatView()
        {
            InitializeComponent();
            MessageList.ItemsSource = Messages;
            ConversationList.ItemsSource = Conversations;

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) return;

            // Load Conversations (Default: Public Chat)
            Conversations.Add(new ChatGroupViewModel
            {
                Id = "public",
                Name = "Lớp học chung",
                IsSelected = true
            });

            // Check Mode based on ChatService state
            IsTeacherMode = ChatService.Instance.IsMyMessage(new ChatMessage { SenderType = "teacher" });

            // UI Adjustments
            if (!IsTeacherMode)
            {
                 // Hide Create Group button for students
                 if (CreateGroupBtn != null) CreateGroupBtn.Visibility = Visibility.Collapsed;
            }
            else
            {
                 LoadGroups();
            }

            ChatService.Instance.MessageReceived += OnMessageReceived;
            ChatService.Instance.GroupCreated += OnGroupCreated;

            // Load messages
            LoadMessages();
        }

        private void LoadGroups()
        {
            try {
                var groups = DatabaseService.Instance.GetChatGroups();
                foreach (var g in groups)
                {
                    Conversations.Add(new ChatGroupViewModel { Id = g.Id, Name = g.Name });
                }
            } catch {}
        }

        private void LoadMessages()
        {
             try {
                var dbMsgs = DatabaseService.Instance.GetChatMessages(SessionManager.Instance.CurrentSession?.Id ?? 0);
                foreach (var m in dbMsgs)
                {
                    AddMessage(m);
                }
             } catch {}
        }

        private void OnMessageReceived(object? sender, ChatMessage msg)
        {
            Dispatcher.Invoke(() => AddMessage(msg));
        }

        private void AddMessage(ChatMessage msg)
        {
             bool show = false;
             if (_privatePartnerId > 0)
             {
                 bool involvesPartner = (msg.SenderId == _privatePartnerId) || (msg.ReceiverId == _privatePartnerId);
                 if (!msg.IsGroup && involvesPartner) show = true;
             }
             else
             {
                 if (msg.IsGroup) show = true;
             }

             if (!show) return;

             bool isMine = ChatService.Instance.IsMyMessage(msg);
             Messages.Add(new ChatMessageViewModel(msg, isMine));
             try {
                  if (MessageList.Items.Count > 0)
                    MessageList.ScrollIntoView(MessageList.Items[MessageList.Items.Count - 1]);
             } catch {}
        }

        private void OnGroupCreated(object? sender, ChatGroup group)
        {
             Dispatcher.Invoke(() =>
            {
                Conversations.Add(new ChatGroupViewModel { Id = group.Id, Name = group.Name });
            });
        }

        private async void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text)) return;
            var content = InputBox.Text;
            InputBox.Text = "";

            if (IsTeacherMode)
            {
                var msg = new ChatMessage
                {
                    SessionId = SessionManager.Instance.CurrentSession?.Id ?? 0,
                    SenderType = "teacher",
                    SenderId = SessionManager.Instance.CurrentUser?.Id ?? 0,
                    SenderName = SessionManager.Instance.CurrentUser?.DisplayName ?? "Teacher",
                    Content = content,

                    IsGroup = _privatePartnerId == 0,
                    ReceiverId = _privatePartnerId,
                    CreatedAt = DateTime.Now
                };

                msg.Id = DatabaseService.Instance.SaveChatMessage(msg);
                await ChatService.Instance.BroadcastMessageAsync(msg);
                AddMessage(msg); // Add locally
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
                     // Teacher handles local file logic + broadcast.
                     // Not implemented fully in ChatService for local path broadcasting -> remote path conversion
                     // For now reuse HandleImageUpload logic
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
                // Group created handled by Dialog logic (calls ChatService)
            }
        }

        private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationList.SelectedItem is ChatGroupViewModel group)
            {
                if (ChatTitleText != null) ChatTitleText.Text = group.Name;
                // Filter logic can be implemented here
            }
        }
    }
}
