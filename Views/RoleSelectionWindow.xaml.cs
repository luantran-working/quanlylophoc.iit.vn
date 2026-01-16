using System.Windows;
using System.Windows.Input;
using ClassroomManagement.Services;

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
            // Mở cửa sổ đăng nhập
            var loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            
            if (loginWindow.ShowDialog() == true && loginWindow.IsLoggedIn)
            {
                // Đăng nhập thành công, mở màn hình Giáo viên
                var teacherWindow = new MainTeacherWindow();
                teacherWindow.Show();
                this.Close();
            }
        }

        private void StudentJoin_Click(object sender, RoutedEventArgs e)
        {
            // Mở cửa sổ nhập tên học sinh
            var nameDialog = new StudentNameDialog();
            nameDialog.Owner = this;
            
            if (nameDialog.ShowDialog() == true)
            {
                var studentWindow = new StudentWindow(nameDialog.StudentName);
                studentWindow.Show();
                this.Close();
            }
        }
    }
}
