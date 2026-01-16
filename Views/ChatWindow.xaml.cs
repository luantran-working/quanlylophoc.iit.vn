using System;
using System.Windows;
using System.Windows.Input;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly SessionManager _session;
        private readonly Student? _targetStudent;
        private readonly bool _isPrivateChat;

        public ChatWindow()
        {
            InitializeComponent();
            _session = SessionManager.Instance;
            _isPrivateChat = false;
            
            // Bind to session chat messages
            DataContext = _session;
            LoadChatMessages();
        }

        public ChatWindow(Student student)
        {
            InitializeComponent();
            _session = SessionManager.Instance;
            _targetStudent = student;
            _isPrivateChat = true;
            
            Title = $"Chat với {student.DisplayName}";
            DataContext = _session;
            LoadChatMessages();
        }

        private void LoadChatMessages()
        {
            // Load existing messages
            // For private chat, filter by student
            // TODO: Add proper filtering
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(MessageInput.Text))
            {
                SendMessage();
            }
        }

        private async void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
                return;

            string message = MessageInput.Text;
            MessageInput.Clear();

            try
            {
                if (_isPrivateChat && _targetStudent != null)
                {
                    await _session.SendChatMessageAsync(message, _targetStudent.Id);
                }
                else
                {
                    await _session.SendChatMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                ToastService.Instance.ShowError("Lỗi", $"Không thể gửi tin nhắn: {ex.Message}");
            }
        }
    }
}
