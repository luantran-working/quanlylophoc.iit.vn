using System.Net;
using System.Windows;
using System.Windows.Input;

namespace ClassroomManagement.Views
{
    public partial class ManualConnectDialog : Window
    {
        public string? ServerIp { get; private set; }
        public bool ContinueSearching { get; private set; }

        public ManualConnectDialog()
        {
            InitializeComponent();
            IpTextBox.Focus();
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
            var ip = IpTextBox.Text.Trim();
            
            if (string.IsNullOrEmpty(ip))
            {
                ShowError("Vui lòng nhập địa chỉ IP");
                IpTextBox.Focus();
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
