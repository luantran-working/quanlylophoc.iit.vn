using System.Windows;
using System.Windows.Controls;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class VotePollWindow : Window
    {
        private readonly Poll _poll;

        public VotePollWindow(Poll poll)
        {
            InitializeComponent();
            _poll = poll;
            QuestionText.Text = poll.Question;
            OptionsList.ItemsSource = poll.Options;

            PollService.Instance.PollUpdated += OnPollUpdated;
            PollService.Instance.PollStopped += OnPollStopped;
        }

        private async void Option_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int optionId)
            {
                await PollService.Instance.SubmitVoteAsync(optionId);

                // Disable all buttons to prevent multiple votes (or just visual feedback)
                OptionsList.IsEnabled = false;
                MessageBox.Show("Đã gửi bình chọn của bạn!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // Option: Close window? Or stay to see results if we implement result sharing to students.
                // Assuming just submit.
                // Close();
            }
        }

        private void OnPollUpdated(object? sender, PollResultUpdate update)
        {
            // Optional: Show live results to students
        }

        private void OnPollStopped(object? sender, System.EventArgs e)
        {
            Dispatcher.Invoke(Close);
        }

        protected override void OnClosed(System.EventArgs e)
        {
             PollService.Instance.PollUpdated -= OnPollUpdated;
             PollService.Instance.PollStopped -= OnPollStopped;
             base.OnClosed(e);
        }
    }
}
