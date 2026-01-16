using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service hiển thị Toast Notifications
    /// </summary>
    public class ToastService
    {
        private static ToastService? _instance;
        public static ToastService Instance => _instance ??= new ToastService();
        private readonly LogService _log = LogService.Instance;

        public enum ToastType
        {
            Success,
            Info,
            Warning,
            Error
        }

        /// <summary>
        /// Hiển thị toast notification
        /// </summary>
        public void Show(string title, string message, ToastType type = ToastType.Info, int durationMs = 3000)
        {
            _log.Debug("ToastService", $"Showing toast: {title} - {message}");
            
            try
            {
                Application.Current?.Dispatcher?.Invoke(async () =>
                {
                    try
                    {
                        var mainWindow = Application.Current.MainWindow;
                        if (mainWindow == null)
                        {
                            _log.Warning("ToastService", "MainWindow is null");
                            return;
                        }

                        _log.Debug("ToastService", $"MainWindow type: {mainWindow.GetType().Name}");

                        // Find or create toast container
                        var container = FindOrCreateToastContainer(mainWindow);
                        if (container == null)
                        {
                            _log.Warning("ToastService", "Could not find or create ToastContainer");
                            return;
                        }

                        _log.Debug("ToastService", $"Toast container found, adding toast");

                        // Create toast
                        var toast = CreateToast(title, message, type);
                        container.Children.Add(toast);

                        // Animate in
                        var slideIn = new DoubleAnimation(50, 0, TimeSpan.FromMilliseconds(200));
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                        toast.RenderTransform = new TranslateTransform();
                        toast.RenderTransform.BeginAnimation(TranslateTransform.XProperty, slideIn);
                        toast.BeginAnimation(UIElement.OpacityProperty, fadeIn);

                        // Wait and animate out
                        await Task.Delay(durationMs);

                        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                        fadeOut.Completed += (s, e) =>
                        {
                            try { container.Children.Remove(toast); } catch { }
                        };
                        toast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("ToastService", "Error showing toast", ex);
                    }
                });
            }
            catch (Exception ex)
            {
                _log.Error("ToastService", "Error invoking on dispatcher", ex);
            }
        }

        public void ShowSuccess(string title, string message, int durationMs = 3000)
        {
            Show(title, message, ToastType.Success, durationMs);
        }

        public void ShowError(string title, string message, int durationMs = 4000)
        {
            Show(title, message, ToastType.Error, durationMs);
        }

        public void ShowInfo(string title, string message, int durationMs = 3000)
        {
            Show(title, message, ToastType.Info, durationMs);
        }

        public void ShowWarning(string title, string message, int durationMs = 3500)
        {
            Show(title, message, ToastType.Warning, durationMs);
        }

        private StackPanel? FindOrCreateToastContainer(Window window)
        {
            // Try to find existing container
            var container = FindChildByName<StackPanel>(window, "ToastContainer");
            if (container != null)
            {
                _log.Debug("ToastService", "Found existing ToastContainer");
                return container;
            }

            _log.Debug("ToastService", "Creating new ToastContainer");

            // Create new container in the window
            if (window.Content is Grid grid)
            {
                container = new StackPanel
                {
                    Name = "ToastContainer",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 80, 20, 0),
                    Width = 320
                };
                Grid.SetRowSpan(container, 100);
                Grid.SetColumnSpan(container, 100);
                Panel.SetZIndex(container, 9999);
                grid.Children.Add(container);
                _log.Debug("ToastService", "ToastContainer created and added to Grid");
                return container;
            }
            else if (window.Content is Panel panel)
            {
                container = new StackPanel
                {
                    Name = "ToastContainer",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 80, 20, 0),
                    Width = 320
                };
                Panel.SetZIndex(container, 9999);
                panel.Children.Add(container);
                _log.Debug("ToastService", "ToastContainer created and added to Panel");
                return container;
            }

            _log.Warning("ToastService", $"Window content is not Grid or Panel: {window.Content?.GetType().Name}");
            return null;
        }

        private T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name)
                    return element;

                var found = FindChildByName<T>(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private Border CreateToast(string title, string message, ToastType type)
        {
            var (bgColor, iconColor, icon) = type switch
            {
                ToastType.Success => ("#E8F5E9", "#4CAF50", "✓"),
                ToastType.Error => ("#FFEBEE", "#F44336", "✕"),
                ToastType.Warning => ("#FFF3E0", "#FF9800", "⚠"),
                _ => ("#E3F2FD", "#2196F3", "ℹ")
            };

            var border = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 0, 0, 8),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 10,
                    Opacity = 0.2,
                    ShadowDepth = 2
                }
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Icon
            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 18,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(iconColor)),
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 2, 0, 0)
            };
            Grid.SetColumn(iconText, 0);
            grid.Children.Add(iconText);

            // Content
            var content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212121")),
                TextWrapping = TextWrapping.Wrap
            });
            content.Children.Add(new TextBlock
            {
                Text = message,
                FontSize = 12,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575")),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            });
            Grid.SetColumn(content, 1);
            grid.Children.Add(content);

            border.Child = grid;
            return border;
        }
    }
}
