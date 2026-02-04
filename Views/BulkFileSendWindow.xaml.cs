using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class BulkFileSendWindow : Window
    {
        private string _selectedFilePath = string.Empty;
        private ObservableCollection<Student> _allStudents;

        public BulkFileSendWindow()
        {
            InitializeComponent();
            _allStudents = SessionManager.Instance.OnlineStudents;
            StudentGrid.ItemsSource = _allStudents;
            
            UpdateOnlineCount();
            
            // Auto-select all online students
            foreach (var student in _allStudents.Where(s => s.IsOnline))
            {
                student.IsSelected = true;
            }
        }

        private void UpdateOnlineCount()
        {
            int onlineCount = _allStudents.Count(s => s.IsOnline);
            OnlineCountText.Text = $"{onlineCount} học sinh trực tuyến";
        }

        #region File Selection
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Chọn file cần gửi",
                Filter = "Tất cả các file (*.*)|*.*|Văn bản (*.doc;*.docx;*.pdf)|*.doc;*.docx;*.pdf|Hình ảnh (*.jpg;*.png;*.gif)|*.jpg;*.png;*.gif|Video (*.mp4;*.avi)|*.mp4;*.avi"
            };
            
            if (dlg.ShowDialog() == true)
            {
                _selectedFilePath = dlg.FileName;
                FilePathBox.Text = _selectedFilePath;
                
                // Show file info
                var fileInfo = new FileInfo(_selectedFilePath);
                SelectedFileName.Text = fileInfo.Name;
                SelectedFileSize.Text = FormatFileSize(fileInfo.Length);
                FileInfoPanel.Visibility = Visibility.Visible;
            }
        }

        private void ClearFile_Click(object sender, RoutedEventArgs e)
        {
            _selectedFilePath = string.Empty;
            FilePathBox.Text = string.Empty;
            FileInfoPanel.Visibility = Visibility.Collapsed;
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
        #endregion

        #region Selection
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var student in _allStudents.Where(s => s.IsOnline))
            {
                student.IsSelected = true;
            }
            SelectAllCheckBox.IsChecked = true;
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var student in _allStudents)
            {
                student.IsSelected = false;
            }
            SelectAllCheckBox.IsChecked = false;
        }

        private void SelectAllCheckBox_Click(object sender, RoutedEventArgs e)
        {
            bool isChecked = SelectAllCheckBox.IsChecked ?? false;
            foreach (var student in _allStudents.Where(s => s.IsOnline))
            {
                student.IsSelected = isChecked;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower().Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                StudentGrid.ItemsSource = _allStudents;
            }
            else
            {
                var filtered = _allStudents.Where(s => 
                    s.DisplayName.ToLower().Contains(searchText) ||
                    s.ComputerName.ToLower().Contains(searchText) ||
                    s.IpAddress.Contains(searchText));
                StudentGrid.ItemsSource = filtered;
            }
        }
        #endregion

        #region Send
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedFilePath))
            {
                MessageBox.Show("Vui lòng chọn file cần gửi!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(_selectedFilePath))
            {
                MessageBox.Show("File không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var targets = _allStudents
                .Where(s => s.IsSelected && s.IsOnline)
                .Select(s => s.MachineId)
                .ToList();

            if (targets.Count == 0)
            {
                var result = MessageBox.Show(
                    "Bạn chưa chọn học sinh nào. Gửi cho TẤT CẢ học sinh trực tuyến?", 
                    "Xác nhận", 
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    targets = _allStudents
                        .Where(s => s.IsOnline)
                        .Select(s => s.MachineId)
                        .ToList();
                }
                else
                {
                    return;
                }
            }

            if (targets.Count == 0)
            {
                MessageBox.Show("Không có học sinh trực tuyến nào!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                BtnSend.IsEnabled = false;
                BtnSend.Content = "ĐANG GỬI...";
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
                MessageBox.Show(
                    $"Đã gửi file thành công đến {targets.Count} học sinh!", 
                    "Thông báo", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Information);
                Close();
            }
            catch (Exception ex)
            {
                StatusText.Text = "Lỗi!";
                MessageBox.Show($"Lỗi gửi file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                BtnSend.IsEnabled = true;
                BtnSend.Content = "GỬI FILE NGAY";
                SendProgress.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
