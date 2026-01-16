using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
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
            
            try
            {
                LogListBox.ItemsSource = _logService.Logs;
                
                // Update log file path
                LogFilePathText.Text = _logService.GetLogFilePath();
                
                // Auto-scroll when new logs added
                _logService.Logs.CollectionChanged += Logs_CollectionChanged;
                
                // Initial count
                UpdateLogCount();
                
                // Apply filter after loaded
                Loaded += (s, e) => ApplyFilter();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogWindow init error: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _logService.Logs.CollectionChanged -= Logs_CollectionChanged;
        }

        private void Logs_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    UpdateLogCount();
                    
                    if (AutoScrollCheckBox?.IsChecked == true && LogListBox?.Items.Count > 0)
                    {
                        LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
                    }
                });
            }
            catch { }
        }

        private void UpdateLogCount()
        {
            try
            {
                if (LogCountText != null)
                {
                    LogCountText.Text = $"{_logService.Logs.Count} entries";
                }
            }
            catch { }
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            try
            {
                if (LogListBox?.ItemsSource == null) return;
                
                var view = CollectionViewSource.GetDefaultView(LogListBox.ItemsSource);
                if (view == null) return;

                view.Filter = obj =>
                {
                    if (obj is not LogEntry entry) return false;

                    return entry.Level switch
                    {
                        LogLevel.Info => FilterInfo?.IsChecked == true,
                        LogLevel.Warning => FilterWarning?.IsChecked == true,
                        LogLevel.Error => FilterError?.IsChecked == true,
                        LogLevel.Debug => FilterDebug?.IsChecked == true,
                        LogLevel.Network => FilterNetwork?.IsChecked == true,
                        _ => true
                    };
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyFilter error: {ex.Message}");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _logService.Clear();
            }
            catch { }
        }

        private void OpenLogFolder_Click(object sender, RoutedEventArgs e)
        {
            try
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
            catch { }
        }
    }
}
