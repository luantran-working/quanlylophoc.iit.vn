using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Interaction logic for RoleSelectionWindow.xaml
    /// </summary>
    public partial class RoleSelectionWindow : Window
    {
        public RoleSelectionWindow()
        {
            InitializeComponent();
            
            // Enable window dragging
            this.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                    this.DragMove();
            };
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }



        private void TeacherLogin_Click(object sender, RoutedEventArgs e)
        {
            var teacherWindow = new MainTeacherWindow();
            teacherWindow.Show();
            this.Close();
        }

        private void StudentJoin_Click(object sender, RoutedEventArgs e)
        {
            var studentWindow = new StudentWindow();
            studentWindow.Show();
            this.Close();
        }
    }
}
