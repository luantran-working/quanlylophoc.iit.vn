
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
        private bool? _explicitMode = null;

        public ChatView()
        {
            InitializeComponent();
            MessageList.ItemsSource = Messages;
            ConversationList.ItemsSource = Conversations;

            Loaded += OnLoaded;
        }

        public void SetMode(bool isTeacher)
        {
            // Empty Logic
        }

        public void SetStudentMode() { }

        public void InitializeConversations()
        {
            // Empty Logic - No loading conversations
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Empty Logic
        }

        public void SetPrivateChat(Student student, bool select = true)
        {
            // Empty Logic
        }

        private void LoadGroups() { }

        private void LoadOnlineStudents() { }

        private void LoadMessages() { }

        private void OnMessageReceived(object? sender, ChatMessage msg) { }

        private void UpdateConversationLastMessage(ChatMessage msg) { }

        private void AddMessageIfVisible(ChatMessage msg) { }

        private void OnGroupCreated(object? sender, ChatGroup group) { }

        private void SendBtn_Click(object sender, RoutedEventArgs e) { }

        private void AttachBtn_Click(object sender, RoutedEventArgs e) { }

        private void InputBox_KeyDown(object sender, KeyEventArgs e) { }

        private void CreateGroupBtn_Click(object sender, RoutedEventArgs e) { }

        private void ConversationList_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
    }
}
