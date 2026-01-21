using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class CreatePollWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<PollOptionViewModel> Options { get; set; } = new();
        public ObservableCollection<PollResultViewModel> Results { get; set; } = new();

        public CreatePollWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Initial options
            Options.Add(new PollOptionViewModel { Text = "Rất hiểu" });
            Options.Add(new PollOptionViewModel { Text = "Hiểu sơ sơ" });
            Options.Add(new PollOptionViewModel { Text = "Chưa hiểu lắm" });

            OptionsControl.ItemsSource = Options;
            ResultList.ItemsSource = Results;

            PollService.Instance.PollUpdated += OnPollUpdated;
        }

        private void AddOption_Click(object sender, RoutedEventArgs e)
        {
            Options.Add(new PollOptionViewModel());
        }

        private void RemoveOption_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is PollOptionViewModel opt)
            {
                Options.Remove(opt);
            }
        }

        private async void StartPoll_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(QuestionBox.Text))
            {
                MessageBox.Show("Vui lòng nhập câu hỏi!");
                return;
            }

            if (Options.Count < 2)
            {
                 MessageBox.Show("Cần ít nhất 2 lựa chọn!");
                 return;
            }

            var poll = new Poll
            {
                Id = new Random().Next(1000, 9999),
                Question = QuestionBox.Text,
                Options = Options.Select((o, i) => new PollOption { Id = i + 1, Text = o.Text }).ToList(),
                IsActive = true
            };

            // Init result view
            Results.Clear();
            foreach (var opt in poll.Options)
            {
                Results.Add(new PollResultViewModel { Id = opt.Id, Text = opt.Text, VoteCount = 0, TotalVotes = 0 });
            }
            ResultQuestionText.Text = poll.Question;
            TotalVotesText.Text = "Tổng số phiếu: 0";

            // UI Switch
            InputGrid.Visibility = Visibility.Collapsed;
            ResultGrid.Visibility = Visibility.Visible;

            await PollService.Instance.StartPollAsync(poll);
        }

        private async void StopPoll_Click(object sender, RoutedEventArgs e)
        {
            await PollService.Instance.StopPollAsync();
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnPollUpdated(object? sender, PollResultUpdate update)
        {
            Dispatcher.Invoke(() =>
            {
                TotalVotesText.Text = $"Tổng số phiếu: {update.TotalVotes}";
                foreach (var res in Results)
                {
                    if (update.VoteCounts.ContainsKey(res.Id))
                    {
                        res.VoteCount = update.VoteCounts[res.Id];
                        res.TotalVotes = update.TotalVotes; // Triggers percent update
                    }
                }
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            PollService.Instance.PollUpdated -= OnPollUpdated;
            base.OnClosed(e);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PollOptionViewModel : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class PollResultViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;

        private int _voteCount;
        public int VoteCount
        {
            get => _voteCount;
            set { _voteCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Percentage)); OnPropertyChanged(nameof(VoteCountString)); }
        }

        private int _totalVotes;
        public int TotalVotes
        {
             get => _totalVotes;
             set { _totalVotes = value; OnPropertyChanged(); OnPropertyChanged(nameof(Percentage)); }
        }

        public double Percentage => _totalVotes == 0 ? 0 : (double)_voteCount / _totalVotes * 100;
        public string VoteCountString => $"{_voteCount} phiếu ({Percentage:F1}%)";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
