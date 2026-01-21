
using System.Linq;
using System.Windows;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class CreateChatGroupDialog : Window
    {
        public CreateChatGroupDialog()
        {
            InitializeComponent();
            StudentList.ItemsSource = SessionManager.Instance.OnlineStudents;
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GroupNameBox.Text))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm");
                return;
            }

            var members = StudentList.SelectedItems.Cast<Student>().Select(s => s.Id).ToList();

            await ChatService.Instance.CreateGroupAsync(
                GroupNameBox.Text,
                SessionManager.Instance.CurrentUser?.Id ?? 0,
                members
            );

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
