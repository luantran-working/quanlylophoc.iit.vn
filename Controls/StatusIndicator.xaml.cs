using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClassroomManagement.Controls
{
    /// <summary>
    /// Interaction logic for StatusIndicator.xaml
    /// </summary>
    public partial class StatusIndicator : UserControl
    {
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(StatusIndicator), new PropertyMetadata("Online"));

        public static readonly DependencyProperty DotColorProperty =
            DependencyProperty.Register("DotColor", typeof(Brush), typeof(StatusIndicator), 
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50))));

        public static readonly DependencyProperty StatusBackgroundProperty =
            DependencyProperty.Register("StatusBackground", typeof(Brush), typeof(StatusIndicator), 
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9))));

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof(Brush), typeof(StatusIndicator), 
                new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32))));

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public Brush DotColor
        {
            get => (Brush)GetValue(DotColorProperty);
            set => SetValue(DotColorProperty, value);
        }

        public Brush StatusBackground
        {
            get => (Brush)GetValue(StatusBackgroundProperty);
            set => SetValue(StatusBackgroundProperty, value);
        }

        public Brush TextColor
        {
            get => (Brush)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public StatusIndicator()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set indicator to Online status
        /// </summary>
        public void SetOnline()
        {
            StatusText = "Online";
            DotColor = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
            StatusBackground = new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9));
            TextColor = new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32));
        }

        /// <summary>
        /// Set indicator to Offline status
        /// </summary>
        public void SetOffline()
        {
            StatusText = "Offline";
            DotColor = new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E));
            StatusBackground = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
            TextColor = new SolidColorBrush(Color.FromRgb(0x61, 0x61, 0x61));
        }

        /// <summary>
        /// Set indicator to Warning status
        /// </summary>
        public void SetWarning(string message = "Cảnh báo")
        {
            StatusText = message;
            DotColor = new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00));
            StatusBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xE0));
            TextColor = new SolidColorBrush(Color.FromRgb(0xE6, 0x5C, 0x00));
        }

        /// <summary>
        /// Set indicator to Error status
        /// </summary>
        public void SetError(string message = "Lỗi")
        {
            StatusText = message;
            DotColor = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
            StatusBackground = new SolidColorBrush(Color.FromRgb(0xFF, 0xEB, 0xEE));
            TextColor = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
        }
    }
}
