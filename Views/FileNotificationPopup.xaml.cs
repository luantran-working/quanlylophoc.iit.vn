using System.Windows;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class FileNotificationPopup : Window
    {
        private readonly BulkFileTransferRequest _request;

        public FileNotificationPopup(BulkFileTransferRequest request)
        {
            InitializeComponent();
            _request = request;
            FileNameText.Text = request.FileName;
            FileSizeText.Text = $"{request.FileSize / 1024.0 / 1024.0:F2} MB";
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            FileReceiverService.Instance.AcceptTransfer(_request);
            Close();
        }

        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
