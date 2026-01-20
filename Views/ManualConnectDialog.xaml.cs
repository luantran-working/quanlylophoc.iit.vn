using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class ManualConnectDialog : Window
    {
        public string? ServerIp { get; private set; }
        public bool ContinueSearching { get; private set; }

        public ManualConnectDialog()
        {
            InitializeComponent();
            ConnectionCodeTextBox.Focus();

            // Try to load saved connection code
            TryLoadSavedConnectionCode();
        }

        private void TryLoadSavedConnectionCode()
        {
            var savedCode = LocalSettings.Instance.SavedConnectionCode;
            if (!string.IsNullOrEmpty(savedCode))
            {
                ConnectionCodeTextBox.Text = savedCode;
            }
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

        private void IpTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TryConnect();
            }
        }

        private void ConnectionCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Auto-format: uppercase and remove spaces
            var textBox = sender as TextBox;
            if (textBox != null)
            {
                var cursorPosition = textBox.SelectionStart;
                var text = textBox.Text.ToUpper().Replace(" ", "");

                if (text != textBox.Text)
                {
                    textBox.Text = text;
                    textBox.SelectionStart = cursorPosition;
                }
            }
        }

        private void ContinueSearch_Click(object sender, RoutedEventArgs e)
        {
            ContinueSearching = true;
            DialogResult = true;
            Close();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            TryConnect();
        }

        private void TryConnect()
        {
            ErrorText.Visibility = Visibility.Collapsed;

            var connectionCode = ConnectionCodeTextBox.Text.Trim().ToUpper();
            var ip = IpTextBox.Text.Trim();

            // Prioritize connection code
            if (!string.IsNullOrEmpty(connectionCode))
            {
                // Validate connection code format (5-7 alphanumeric characters)
                if (connectionCode.Length < 5 || connectionCode.Length > 7 ||
                    !connectionCode.All(c => char.IsLetterOrDigit(c)))
                {
                    ShowError("Mã kết nối không hợp lệ. Mã phải có 5-7 ký tự (chữ và số).");
                    ConnectionCodeTextBox.Focus();
                    return;
                }

                // Save the connection code for next time
                LocalSettings.Instance.SavedConnectionCode = connectionCode;
                LocalSettings.Instance.LastConnectionAttempt = System.DateTime.Now;
                LocalSettings.Instance.Save();

                // Signal that we have a connection code (not IP)
                ServerIp = "CONNECTION_CODE:" + connectionCode;
                ContinueSearching = false;
                DialogResult = true;
                Close();
                return;
            }

            // Fallback to IP input
            if (string.IsNullOrEmpty(ip))
            {
                ShowError("Vui lòng nhập mã kết nối HOẶC địa chỉ IP");
                ConnectionCodeTextBox.Focus();
                return;
            }

            // Validate IP format
            if (!IPAddress.TryParse(ip, out _))
            {
                ShowError("Địa chỉ IP không hợp lệ. Ví dụ: 192.168.0.100");
                IpTextBox.Focus();
                return;
            }

            ServerIp = ip;
            ContinueSearching = false;
            DialogResult = true;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
