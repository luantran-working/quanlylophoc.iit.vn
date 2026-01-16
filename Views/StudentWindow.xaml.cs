using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;

using ClassroomManagement.ViewModels;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        private bool _isHandRaised = false;

        public StudentWindow()
        {
            InitializeComponent();
            DataContext = new StudentViewModel();
        }

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Chọn file để gửi cho giáo viên",
                Filter = "Tất cả tệp (*.*)|*.*|Tài liệu Word (*.docx)|*.docx|PDF (*.pdf)|*.pdf|Hình ảnh (*.jpg;*.png)|*.jpg;*.png",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string files = string.Join("\n", openFileDialog.FileNames);
                MessageBox.Show(
                    $"Đã chọn {openFileDialog.FileNames.Length} file để gửi:\n{files}",
                    "Gửi file",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void RaiseHand_Click(object sender, MouseButtonEventArgs e)
        {
            _isHandRaised = !_isHandRaised;
            RaiseHandToggle.IsChecked = _isHandRaised;
            
            if (_isHandRaised)
            {
                RaiseHandStatus.Text = "Đang giơ tay...";
                RaiseHandBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x4D, 0x4D, 0x5D));
            }
            else
            {
                RaiseHandStatus.Text = "Nhấn để giơ tay";
                RaiseHandBorder.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0x3D, 0x3D, 0x4D));
            }
        }

        private void OpenChat_Click(object sender, RoutedEventArgs e)
        {
            var chatWindow = new ChatWindow();
            chatWindow.Title = "Chat với Giáo viên";
            chatWindow.Show();
        }

        private void RequestHelp_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có muốn gửi yêu cầu trợ giúp đến giáo viên không?\n\nGiáo viên sẽ nhận được thông báo và có thể hỗ trợ bạn.",
                "Yêu cầu trợ giúp",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show(
                    "Yêu cầu trợ giúp đã được gửi!\nGiáo viên sẽ phản hồi sớm nhất có thể.",
                    "Thông báo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
