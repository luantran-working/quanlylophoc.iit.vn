using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class LogWindow : Window
    {
        private readonly LogService _logService;

        public LogWindow()
        {
            InitializeComponent();
            
            _logService = LogService.Instance;
            LogListBox.ItemsSource = _logService.Logs;
            
            // Update log file path
            LogFilePathText.Text = _logService.GetLogFilePath();
            
            // Auto-scroll when new logs added
            _logService.Logs.CollectionChanged += Logs_CollectionChanged;
            
            // Initial count
            UpdateLogCount();
            
            // Apply filter
            ApplyFilter();
        }

        private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLogCount();
            
            if (AutoScrollCheckBox.IsChecked == true && LogListBox.Items.Count > 0)
            {
                LogListBox.ScrollIntoView(LogListBox.Items[^1]);
            }
        }

        private void UpdateLogCount()
        {
            LogCountText.Text = $"{_logService.Logs.Count} entries";
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var view = CollectionViewSource.GetDefaultView(LogListBox.ItemsSource);
            if (view == null) return;

            view.Filter = obj =>
            {
                if (obj is not LogEntry entry) return false;

                return entry.Level switch
                {
                    LogLevel.Info => FilterInfo.IsChecked == true,
                    LogLevel.Warning => FilterWarning.IsChecked == true,
                    LogLevel.Error => FilterError.IsChecked == true,
                    LogLevel.Debug => FilterDebug.IsChecked == true,
                    LogLevel.Network => FilterNetwork.IsChecked == true,
                    _ => true
                };
            };
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _logService.Clear();
        }

        private void OpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            var folder = Path.GetDirectoryName(_logService.GetLogFilePath());
            if (folder != null && Directory.Exists(folder))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folder,
                    UseShellExecute = true
                });
            }
        }
    }
}
