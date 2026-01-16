using System.Windows;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for TestCreationWindow.xaml
    /// </summary>
    public partial class TestCreationWindow : Window
    {
        public TestCreationWindow()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn hủy? Các thay đổi sẽ không được lưu.",
                "Xác nhận hủy",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void CreateTest_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Validate form and create test
            MessageBox.Show(
                "Bài kiểm tra đã được tạo và gửi đến học sinh thành công!",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            this.Close();
        }
    }
}
