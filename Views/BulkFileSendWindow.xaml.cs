using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class BulkFileSendWindow : Window
    {
        private string _selectedFilePath = string.Empty;

        public BulkFileSendWindow()
        {
            InitializeComponent();
            StudentGrid.ItemsSource = SessionManager.Instance.OnlineStudents;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == true)
            {
                _selectedFilePath = dlg.FileName;
                FilePathBox.Text = _selectedFilePath;
            }
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Vui lòng chọn file!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var targets = SessionManager.Instance.OnlineStudents
                .Where(s => s.IsSelected)
                .Select(s => s.MachineId)
                .ToList();

            if (targets.Count == 0)
            {
                // If none selected, assume all via prompt or logic?
                // Let's ask user or default to all if none "IsSelected" property is checked?
                // But IsSelected defaults to false.
                // Let's just require selection.
                // Or auto select all if none selected?
                if (MessageBox.Show("Bạn chưa chọn học sinh nào. Gửi cho TẤT CẢ?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    targets = SessionManager.Instance.OnlineStudents.Select(s => s.MachineId).ToList();
                }
                else
                {
                    return;
                }
            }

            try
            {
                SendProgress.Visibility = Visibility.Visible;
                SendProgress.Value = 0;
                StatusText.Text = "Đang chuẩn bị...";

                var progress = new Progress<double>(percent =>
                {
                    SendProgress.Value = percent;
                    StatusText.Text = $"Đang gửi {percent:F0}%";
                });

                await BulkFileSender.Instance.SendFileToStudentsAsync(_selectedFilePath, targets, progress);

                StatusText.Text = "Hoàn thành!";
                MessageBox.Show("Đã gửi file thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                 StatusText.Text = "Lỗi!";
                 MessageBox.Show($"Lỗi gửi file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SendProgress.Visibility = Visibility.Hidden;
            }
        }
    }
}
