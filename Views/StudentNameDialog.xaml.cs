using System.Windows;
using System.Windows.Input;

namespace ClassroomManagement.Views
{
    public partial class StudentNameDialog : Window
    {
        public string StudentName { get; private set; } = "";

        public StudentNameDialog()
        {
            InitializeComponent();
            NameTextBox.Text = "Nguyễn Văn A"; // Default name as requested
            NameTextBox.SelectAll();
            NameTextBox.Focus();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void NameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Submit();
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            Submit();
        }

        private void Submit()
        {
            var name = NameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                ErrorText.Text = "Vui lòng nhập họ tên";
                ErrorText.Visibility = Visibility.Visible;
                NameTextBox.Focus();
                return;
            }

            if (name.Length < 2)
            {
                ErrorText.Text = "Họ tên phải có ít nhất 2 ký tự";
                ErrorText.Visibility = Visibility.Visible;
                NameTextBox.Focus();
                return;
            }

            StudentName = name;
            DialogResult = true;
            Close();
        }
    }
}
