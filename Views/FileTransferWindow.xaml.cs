using System.Windows;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for FileTransferWindow.xaml
    /// </summary>
    public partial class FileTransferWindow : Window
    {
        public FileTransferWindow()
        {
            InitializeComponent();
        }

        private void RequestFiles_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Yêu cầu nộp file đã được gửi đến tất cả học sinh.\nHọc sinh sẽ nhận được thông báo và có thể nộp file.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
