using System;
using System.Windows;
using System.Windows.Media.Imaging;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class ScreenshotViewerWindow : Window
    {
        private Screenshot _screenshot;

        public ScreenshotViewerWindow(Screenshot screenshot)
        {
            InitializeComponent();
            _screenshot = screenshot;
            Loaded += ScreenshotViewerWindow_Loaded;
        }

        private void ScreenshotViewerWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_screenshot == null)
            {
                Close();
                return;
            }

            TitleText.Text = $"{_screenshot.StudentName} - {_screenshot.CapturedAt:HH:mm dd/MM/yyyy}";
            PathText.Text = _screenshot.FilePath;
            NoteTextBox.Text = _screenshot.Note;

            try
            {
                // Use BitmapImage with CacheOption to avoid locking the file
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(_screenshot.FilePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                MainImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = NoteTextBox.Text;
                SessionManager.Instance.ScreenshotService.AddNote(_screenshot.Id, note);
                _screenshot.Note = note; // Update local object
                ToastService.Instance.ShowSuccess("Đã lưu", "Đã cập nhật ghi chú.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu ghi chú: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa ảnh này? File ảnh cũng sẽ bị xóa.",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    SessionManager.Instance.ScreenshotService.DeleteScreenshot(_screenshot.Id);
                    // Also try to delete physical file if Service doesn't do it (Currently service only deletes DB record)
                    // The workflow says "DeleteScreenshot" in service should handle it, but I recall analyzing it only deletes DB.
                    // Let's implement full delete here or verify service.
                    // Service code: "_database.DeleteScreenshot(id)".
                    // So we must manually delete file here or update service.
                    // Ideally update service, but for now let's do it here to be safe and quick.

                    if (System.IO.File.Exists(_screenshot.FilePath))
                    {
                        try { System.IO.File.Delete(_screenshot.FilePath); } catch { }
                    }
                    if (!string.IsNullOrEmpty(_screenshot.ThumbnailPath) && System.IO.File.Exists(_screenshot.ThumbnailPath))
                    {
                        try { System.IO.File.Delete(_screenshot.ThumbnailPath); } catch { }
                    }

                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi xóa ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
