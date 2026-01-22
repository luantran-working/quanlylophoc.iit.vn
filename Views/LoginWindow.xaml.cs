using System.Windows;
using System.Windows.Input;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class LoginWindow : Window
    {
        public bool IsLoggedIn { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            UsernameTextBox.Text = "admin";
            PasswordBox.Password = "123456";
            UsernameTextBox.Focus();
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

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoLogin();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            DoLogin();
        }

        private void DoLogin()
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username))
            {
                ShowError("Vui lòng nhập tên đăng nhập");
                UsernameTextBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Vui lòng nhập mật khẩu");
                PasswordBox.Focus();
                return;
            }

            LoginButton.IsEnabled = false;
            LoginButton.Content = "Đang đăng nhập...";

            try
            {
                var sessionManager = SessionManager.Instance;
                if (sessionManager.Login(username, password))
                {
                    IsLoggedIn = true;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError("Sai tên đăng nhập hoặc mật khẩu");
                }
            }
            finally
            {
                LoginButton.IsEnabled = true;
                LoginButton.Content = "ĐĂNG NHẬP";
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
