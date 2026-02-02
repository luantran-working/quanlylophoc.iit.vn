using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace ClassroomManagement.Controls
{
    /// <summary>
    /// Shared Window Header Bar with Logo, Title, and Window Controls
    /// </summary>
    public partial class WindowHeaderBar : UserControl
    {
        public WindowHeaderBar()
        {
            InitializeComponent();
        }

        #region Dependency Properties

        /// <summary>
        /// Window Title
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(WindowHeaderBar),
                new PropertyMetadata("Window Title"));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Window Subtitle (optional)
        /// </summary>
        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                nameof(Subtitle),
                typeof(string),
                typeof(WindowHeaderBar),
                new PropertyMetadata(string.Empty));

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        /// <summary>
        /// Icon Kind for the logo
        /// </summary>
        public static readonly DependencyProperty IconKindProperty =
            DependencyProperty.Register(
                nameof(IconKind),
                typeof(PackIconKind),
                typeof(WindowHeaderBar),
                new PropertyMetadata(PackIconKind.School));

        public PackIconKind IconKind
        {
            get => (PackIconKind)GetValue(IconKindProperty);
            set => SetValue(IconKindProperty, value);
        }

        /// <summary>
        /// Show/Hide Minimize button
        /// </summary>
        public static readonly DependencyProperty ShowMinimizeProperty =
            DependencyProperty.Register(
                nameof(ShowMinimize),
                typeof(bool),
                typeof(WindowHeaderBar),
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
                typeof(WindowHeaderBar),
                new PropertyMetadata(true));

        public bool ShowMaximize
        {
            get => (bool)GetValue(ShowMaximizeProperty);
            set => SetValue(ShowMaximizeProperty, value);
        }

        /// <summary>
        /// Center Content
        /// </summary>
        public static readonly DependencyProperty CenterContentProperty =
            DependencyProperty.Register(
                nameof(CenterContent),
                typeof(object),
                typeof(WindowHeaderBar),
                new PropertyMetadata(null));

        public object CenterContent
        {
            get => GetValue(CenterContentProperty);
            set => SetValue(CenterContentProperty, value);
        }

        /// <summary>
        /// Right Content (before window controls)
        /// </summary>
        public static readonly DependencyProperty RightContentProperty =
            DependencyProperty.Register(
                nameof(RightContent),
                typeof(object),
                typeof(WindowHeaderBar),
                new PropertyMetadata(null));

        public object RightContent
        {
            get => GetValue(RightContentProperty);
            set => SetValue(RightContentProperty, value);
        }

        #endregion

        #region Event Handlers

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (e.ClickCount == 2)
            {
                // Double click to maximize/restore
                if (window != null)
                {
                    window.WindowState = window.WindowState == WindowState.Maximized 
                        ? WindowState.Normal 
                        : WindowState.Maximized;
                }
            }
            else
            {
                // Single click to drag
                window?.DragMove();
            }
        }

        #endregion
    }
}
