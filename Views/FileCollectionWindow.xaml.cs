using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class FileCollectionWindow : Window
    {
        private readonly SessionManager _session;
        private ObservableCollection<Student> _allStudents;

        public FileCollectionWindow()
        {
            InitializeComponent();
            _session = SessionManager.Instance;
            _allStudents = _session.OnlineStudents;
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

        #region Quick Path Buttons
        private void QuickPath_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string folder)
            {
                // Convert to full system path
                string fullPath = folder switch
                {
                    "Documents" => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "Downloads" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
                    "Pictures" => Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    "Videos" => Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                    _ => folder
                };
                
                SourcePathBox.Text = fullPath;
                
                // Visual feedback - highlight selected button
                ResetQuickPathButtons();
                btn.Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0xE3, 0xF2, 0xFD));
            }
        }

        private void ResetQuickPathButtons()
        {
            BtnDocuments.Background = System.Windows.Media.Brushes.Transparent;
            BtnDownloads.Background = System.Windows.Media.Brushes.Transparent;
            BtnPictures.Background = System.Windows.Media.Brushes.Transparent;
            BtnVideos.Background = System.Windows.Media.Brushes.Transparent;
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
                    s.ComputerName.ToLower().Contains(searchText));
                StudentGrid.ItemsSource = filtered;
            }
        }
        #endregion

        #region Build Extensions List
        private List<string> GetSelectedExtensions()
        {
            var extensions = new List<string>();

            // Document types
            if (ChkDoc.IsChecked == true) { extensions.Add("doc"); extensions.Add("docx"); }
            if (ChkXls.IsChecked == true) { extensions.Add("xls"); extensions.Add("xlsx"); }
            if (ChkPpt.IsChecked == true) { extensions.Add("ppt"); extensions.Add("pptx"); }
            if (ChkPdf.IsChecked == true) extensions.Add("pdf");
            if (ChkTxt.IsChecked == true) extensions.Add("txt");

            // Image types
            if (ChkJpg.IsChecked == true) { extensions.Add("jpg"); extensions.Add("jpeg"); }
            if (ChkPng.IsChecked == true) extensions.Add("png");
            if (ChkGif.IsChecked == true) extensions.Add("gif");
            if (ChkBmp.IsChecked == true) extensions.Add("bmp");

            // Code types
            if (ChkCpp.IsChecked == true) { extensions.Add("cpp"); extensions.Add("c"); extensions.Add("h"); }
            if (ChkCs.IsChecked == true) extensions.Add("cs");
            if (ChkPy.IsChecked == true) extensions.Add("py");
            if (ChkJava.IsChecked == true) extensions.Add("java");
            if (ChkJs.IsChecked == true) { extensions.Add("js"); extensions.Add("ts"); }
            if (ChkHtml.IsChecked == true) { extensions.Add("html"); extensions.Add("css"); }

            // Archive types
            if (ChkZip.IsChecked == true) extensions.Add("zip");
            if (ChkRar.IsChecked == true) extensions.Add("rar");
            if (Chk7z.IsChecked == true) extensions.Add("7z");

            // Custom extensions
            if (!string.IsNullOrWhiteSpace(CustomExtensionsBox.Text))
            {
                var custom = CustomExtensionsBox.Text.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().TrimStart('.'))
                    .Where(s => !string.IsNullOrEmpty(s));
                extensions.AddRange(custom);
            }

            return extensions.Distinct().ToList();
        }
        #endregion

        #region Collection
        private async void StartCollection_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SourcePathBox.Text))
            {
                MessageBox.Show("Vui lòng nhập đường dẫn thư mục nguồn!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var extensions = GetSelectedExtensions();
            if (extensions.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một định dạng tập tin!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStudents = _allStudents.Where(s => s.IsSelected && s.IsOnline).ToList();
            if (selectedStudents.Count == 0)
            {
                var result = MessageBox.Show(
                    "Không có học sinh nào được chọn. Bạn có muốn thu thập từ TẤT CẢ học sinh trực tuyến?",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    selectedStudents = _allStudents.Where(s => s.IsOnline).ToList();
                }
                else
                {
                    return;
                }
            }

            var request = new FileCollectionRequest
            {
                RemotePath = SourcePathBox.Text.Trim(),
                Recursive = RecursiveCheck.IsChecked ?? true,
                Extensions = extensions
            };

            // Reset status for selected students
            foreach (var student in selectedStudents)
            {
                student.CollectionStatus = "Đang gửi yêu cầu...";
            }

            // Disable button during collection
            BtnStartCollection.IsEnabled = false;
            BtnStartCollection.Content = "ĐANG THU THẬP...";
            StatusText.Text = $"Đang thu thập file từ {selectedStudents.Count} học sinh...";

            try
            {
                await _session.NetworkServer.BroadcastToAllAsync(new NetworkMessage
                {
                    Type = MessageType.FileCollectionRequest,
                    SenderId = "server",
                    Payload = System.Text.Json.JsonSerializer.Serialize(request)
                });

                StatusText.Text = $"Đã gửi yêu cầu đến {selectedStudents.Count} học sinh. Đang chờ phản hồi...";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi yêu cầu: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Lỗi khi gửi yêu cầu thu thập";
            }
            finally
            {
                BtnStartCollection.IsEnabled = true;
                BtnStartCollection.Content = "BẮT ĐẦU THU THẬP";
            }
        }
        #endregion

        #region Open Folder
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CollectedFiles");
                if (_session.CurrentSession != null)
                {
                    baseDir = Path.Combine(baseDir, _session.CurrentSession.Id.ToString());
                }

                Directory.CreateDirectory(baseDir);
                Process.Start("explorer.exe", baseDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở thư mục: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion
    }
}
