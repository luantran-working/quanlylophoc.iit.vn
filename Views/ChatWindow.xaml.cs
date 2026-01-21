using System.Windows;
using ClassroomManagement.Models;

namespace ClassroomManagement.Views
{
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
        }

        public ChatWindow(Student student) : this()
        {
            if (student != null)
            {
                MainChatView.SetPrivateChat(student);
                Title = $"Chat với {student.DisplayName}";
            }
        }
    }
}
