using System;
using System.Windows;
using System.Windows.Input;

namespace ClassroomManagement.Views
{
    public partial class WhiteboardWindow : Window
    {
        public WhiteboardWindow()
        {
            InitializeComponent();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
    }
}

