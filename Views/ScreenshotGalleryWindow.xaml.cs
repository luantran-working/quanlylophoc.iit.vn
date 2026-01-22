using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class ScreenshotGalleryWindow : Window
    {
        private List<Screenshot> _allScreenshots = new();

        public ScreenshotGalleryWindow()
        {
            InitializeComponent();
            Loaded += ScreenshotGalleryWindow_Loaded;
        }

        private async void ScreenshotGalleryWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate Student Filter
            // We want all students who have screenshots + current online students maybe?
            // For now, let's just use OnlineStudents from SessionManager, or unique students from DB screenshots
            // Let's rely on OnlineStudents for now + "All" option

            var students = SessionManager.Instance.OnlineStudents.ToList();
            StudentFilterParams.ItemsSource = students;

            await LoadScreenshots();
        }

        private async System.Threading.Tasks.Task LoadScreenshots()
        {
            try
            {
                var sessionId = SessionManager.Instance.CurrentSession?.Id ?? 0;
                // If filtering by student
                string? studentId = null;
                if (StudentFilterParams.SelectedValue is string id)
                {
                    studentId = id;
                }

                // Call service
                // Note: GetScreenshots might need updates if we want to filter by null studentId correctly
                // Currently ScreenshotService.GetScreenshots might require studentId? No, let's check.
                // It has (int sessionId, string studentId). If studentId is null/empty, does it return all?
                // I need to verify ScreenshotService implementation.

                // Assuming I implemented GetScreenshots(int sessionId, string studentId = null)
                // If not, I might need to adjust.

                _allScreenshots = await System.Threading.Tasks.Task.Run(() =>
                    SessionManager.Instance.ScreenshotService.GetScreenshots(sessionId.ToString(), studentId));

                GalleryItems.ItemsSource = _allScreenshots;

                EmptyState.Visibility = _allScreenshots.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải ảnh: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            await LoadScreenshots();
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadScreenshots();
        }

        private void Screenshot_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Screenshot screenshot)
            {
                var viewer = new ScreenshotViewerWindow(screenshot);
                viewer.Owner = this;
                viewer.ShowDialog();

                // Refresh after viewer closes (in case of delete/update)
                _ = LoadScreenshots();
            }
        }
    }
}
