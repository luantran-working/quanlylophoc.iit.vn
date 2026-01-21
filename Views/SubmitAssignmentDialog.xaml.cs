using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using ClassroomManagement.Models;

namespace ClassroomManagement.Views
{
    public partial class SubmitAssignmentDialog : Window
    {
        public ObservableCollection<SubmittedFileViewModel> SelectedFiles { get; } = new ObservableCollection<SubmittedFileViewModel>();
        public string Note => NoteInput.Text;

        public SubmitAssignmentDialog()
        {
            InitializeComponent();
            FileList.ItemsSource = SelectedFiles;
        }

        private void AddFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = true;
            if (dlg.ShowDialog() == true)
            {
                AddFiles(dlg.FileNames);
            }
        }

        private void RemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is SubmittedFileViewModel file)
            {
                SelectedFiles.Remove(file);
            }
        }

        private void FileList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFiles(files);
            }
        }

        private void AddFiles(string[] paths)
        {
            foreach (var path in paths)
            {
                if (File.Exists(path))
                {
                    var fileInfo = new FileInfo(path);
                    // Warning if file > 5MB
                    if (fileInfo.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show($"File {fileInfo.Name} quá lớn (>5MB). Khuyến nghị nộp file nhỏ hơn.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                    if (!SelectedFiles.Any(f => f.LocalPath == path))
                    {
                        SelectedFiles.Add(new SubmittedFileViewModel
                        {
                            FileName = fileInfo.Name,
                            FileSize = fileInfo.Length,
                            LocalPath = path
                        });
                    }
                }
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFiles.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất một file.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    public class SubmittedFileViewModel
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string LocalPath { get; set; }

        public string FileSizeStr
        {
            get
            {
                if (FileSize < 1024) return $"{FileSize} B";
                if (FileSize < 1024 * 1024) return $"{FileSize / 1024.0:F1} KB";
                return $"{FileSize / (1024.0 * 1024.0):F1} MB";
            }
        }
    }
}
