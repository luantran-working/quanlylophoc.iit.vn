using System.Windows;
using ClassroomManagement.Models;

namespace ClassroomManagement.Views
{
    public partial class ComputerSpecsWindow : Window
    {
        public ComputerSpecsWindow(SystemInfoPackage info)
        {
            InitializeComponent();
            DataContext = info;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
