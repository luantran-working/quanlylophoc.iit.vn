using System.Windows;

using ClassroomManagement.ViewModels;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for MainTeacherWindow.xaml
    /// </summary>
    public partial class MainTeacherWindow : Window
    {
        public MainTeacherWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void StartPresentation_Click(object sender, RoutedEventArgs e)
        {
            var screenShareWindow = new ScreenShareWindow();
            screenShareWindow.Show();
        }

        private void OpenGroupChat_Click(object sender, RoutedEventArgs e)
        {
            var chatWindow = new ChatWindow();
            chatWindow.Title = "Chat Nhóm - Lớp 10A1";
            chatWindow.Show();
        }

        private void OpenFileTransfer_Click(object sender, RoutedEventArgs e)
        {
            var fileTransferWindow = new FileTransferWindow();
            fileTransferWindow.Show();
        }

        private void OpenTestCreation_Click(object sender, RoutedEventArgs e)
        {
            var testCreationWindow = new TestCreationWindow();
            testCreationWindow.Show();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát khỏi phòng học?",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var roleWindow = new RoleSelectionWindow();
                roleWindow.Show();
                this.Close();
            }
        }
    }
}
