using System;
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
            FileSizeText.Text = FormatFileSize(request.FileSize);
            
            // Subscribe to progress updates
            FileReceiverService.Instance.FileTransferProgress += OnTransferProgress;
            FileReceiverService.Instance.FileTransferCompleted += OnTransferCompleted;
            
            // Start fade-in animation
            Loaded += (s, e) =>
            {
                var storyboard = (System.Windows.Media.Animation.Storyboard)FindResource("FadeIn");
                storyboard.Begin(this);
            };
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void OnTransferProgress(object? sender, (string FileId, int Progress) e)
        {
            if (e.FileId == _request.FileId)
            {
                Dispatcher.Invoke(() =>
                {
                    DownloadProgress.Value = e.Progress;
                    ProgressText.Text = $"{e.Progress}%";
                    StatusText.Text = e.Progress < 100 ? "Đang nhận dữ liệu..." : "Đang lưu file...";
                });
            }
        }

        private void OnTransferCompleted(object? sender, string filePath)
        {
            Dispatcher.Invoke(() =>
            {
                // Unsubscribe
                FileReceiverService.Instance.FileTransferProgress -= OnTransferProgress;
                FileReceiverService.Instance.FileTransferCompleted -= OnTransferCompleted;
                
                // Update UI to show completion
                DownloadProgress.Value = 100;
                ProgressText.Text = "100%";
                StatusText.Text = "Hoàn thành! File đã được lưu vào thư mục Downloads";
                
                BtnCancel.Visibility = Visibility.Collapsed;
                BtnClose.Visibility = Visibility.Visible;
                
                // Auto close after 3 seconds
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    Close();
                };
                timer.Start();
            });
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Decline/cancel the transfer
            FileReceiverService.Instance.DeclineTransfer(_request.FileId);
            
            // Unsubscribe
            FileReceiverService.Instance.FileTransferProgress -= OnTransferProgress;
            FileReceiverService.Instance.FileTransferCompleted -= OnTransferCompleted;
            
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Ensure we unsubscribe when window closes
            FileReceiverService.Instance.FileTransferProgress -= OnTransferProgress;
            FileReceiverService.Instance.FileTransferCompleted -= OnTransferCompleted;
            base.OnClosed(e);
        }
    }
}
