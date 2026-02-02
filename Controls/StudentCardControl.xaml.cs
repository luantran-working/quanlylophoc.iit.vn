using System.Windows;
using System.Windows.Controls;
using ClassroomManagement.Models;
using ClassroomManagement.Services;
using ClassroomManagement.Views;

namespace ClassroomManagement.Controls
{
    /// <summary>
    /// Interaction logic for StudentCardControl.xaml
    /// </summary>
    public partial class StudentCardControl : UserControl
    {
        public StudentCardControl()
        {
            InitializeComponent();
        }

        private Student? GetStudent()
        {
            return DataContext as Student;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var student = GetStudent();
            if (student == null) return;

            // Notify MainTeacherWindow to switch chat
            var mainWindow = Window.GetWindow(this) as MainTeacherWindow;
            if (mainWindow != null)
            {
                mainWindow.FocusStudentChat(student);
            }
        }

        private void ViewScreen_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudent();
            if (student == null) return;

            var screenWindow = new StudentScreenWindow(student);
            screenWindow.Show();
        }

        private void RemoteControl_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudent();
            if (student == null) return;

            var remoteWindow = new RemoteControlWindow(student);
            remoteWindow.Show();
        }

        private void PrivateChat_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudent();
            if (student == null) return;

            var mainWindow = Window.GetWindow(this) as MainTeacherWindow;
            if (mainWindow != null)
            {
                mainWindow.FocusStudentChat(student);
            }
        }

        private void SendFile_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudent();
            if (student == null) return;

            var openDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = $"Chọn file để gửi cho {student.DisplayName}"
            };

            if (openDialog.ShowDialog() == true)
            {
                ToastService.Instance.ShowInfo("Gửi file", $"Đang gửi file đến {student.DisplayName}...");
                // TODO: Implement file sending
            }
        }

        private async void LockMachine_Click(object sender, RoutedEventArgs e)
        {
            var student = GetStudent();
            if (student == null) return;

            await SessionManager.Instance.LockStudentAsync(student.MachineId, !student.IsLocked);

            var action = student.IsLocked ? "khóa" : "mở khóa";
            ToastService.Instance.ShowSuccess("Thành công", $"Đã {action} máy {student.DisplayName}");
        }
    }
}
