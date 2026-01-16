using System.Windows;
using System.Windows.Input;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
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

        private void SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageInput.Text))
                return;

            // In a real application, this would send the message to the server
            // For now, we just clear the input
            string message = MessageInput.Text;
            MessageInput.Clear();

            // TODO: Add message to chat list and send to server
            System.Diagnostics.Debug.WriteLine($"Sending message: {message}");
        }
    }
}
