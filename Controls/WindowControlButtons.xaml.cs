using System.Windows;
using System.Windows.Controls;

namespace ClassroomManagement.Controls
{
    /// <summary>
    /// Shared Window Control Buttons (Minimize, Maximize, Close)
    /// </summary>
    public partial class WindowControlButtons : UserControl
    {
        public WindowControlButtons()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        /// <summary>
        /// Show/Hide Minimize button
        /// </summary>
        public static readonly DependencyProperty ShowMinimizeProperty =
            DependencyProperty.Register(
                nameof(ShowMinimize),
                typeof(bool),
                typeof(WindowControlButtons),
                new PropertyMetadata(true));

        public bool ShowMinimize
        {
            get => (bool)GetValue(ShowMinimizeProperty);
            set => SetValue(ShowMinimizeProperty, value);
        }

        /// <summary>
        /// Show/Hide Maximize button
        /// </summary>
        public static readonly DependencyProperty ShowMaximizeProperty =
            DependencyProperty.Register(
                nameof(ShowMaximize),
                typeof(bool),
                typeof(WindowControlButtons),
                new PropertyMetadata(true));

        public bool ShowMaximize
        {
            get => (bool)GetValue(ShowMaximizeProperty);
            set => SetValue(ShowMaximizeProperty, value);
        }

        #endregion

        #region Event Handlers

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                    MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.SquareOutline;
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                    MaximizeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window?.Close();
        }

        #endregion
    }
}
