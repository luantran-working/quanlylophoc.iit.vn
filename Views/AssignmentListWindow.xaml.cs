using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class AssignmentListWindow : Window
    {
        public ObservableCollection<AssignmentSubmission> Submissions { get; } = new ObservableCollection<AssignmentSubmission>();

        public AssignmentListWindow()
        {
            InitializeComponent();
            SubmissionGrid.ItemsSource = Submissions;
            LoadSubmissions();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSubmissions();
        }

        private void LoadSubmissions()
        {
            Submissions.Clear();
            var currentSession = SessionManager.Instance.CurrentSession;
            if (currentSession != null)
            {
                var list = AssignmentService.Instance.GetSubmissions(currentSession.Id);
                foreach (var item in list)
                {
                    Submissions.Add(item);
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
             if (sender is Button btn && btn.Tag is AssignmentSubmission submission)
             {
                 if (submission.Files.Count > 0)
                 {
                     var path = submission.Files[0].LocalPath;
                     var dir = Path.GetDirectoryName(path);
                     if (Directory.Exists(dir))
                     {
                         Process.Start("explorer.exe", dir);
                     }
                 }
             }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var path = e.Uri.OriginalString;
            AssignmentService.Instance.OpenFileFolder(path);
            e.Handled = true;
        }
    }
}
