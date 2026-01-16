using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ClassroomManagement.ViewModels
{
    /// <summary>
    /// Base ViewModel với INotifyPropertyChanged implementation
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Simple ICommand implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    /// <summary>
    /// Model cho Student
    /// </summary>
    public class StudentModel : ViewModelBase
    {
        private string _id = string.Empty;
        private string _studentName = string.Empty;
        private bool _isOnline;
        private bool _isCameraOn;
        private bool _isMicrophoneOn;
        private bool _isLocked;
        private bool _isHandRaised;
        private string _ipAddress = string.Empty;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string StudentName
        {
            get => _studentName;
            set => SetProperty(ref _studentName, value);
        }

        public bool IsOnline
        {
            get => _isOnline;
            set => SetProperty(ref _isOnline, value);
        }

        public bool IsCameraOn
        {
            get => _isCameraOn;
            set => SetProperty(ref _isCameraOn, value);
        }

        public bool IsMicrophoneOn
        {
            get => _isMicrophoneOn;
            set => SetProperty(ref _isMicrophoneOn, value);
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }

        public bool IsHandRaised
        {
            get => _isHandRaised;
            set => SetProperty(ref _isHandRaised, value);
        }

        public string IpAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        public ICommand? LockCommand { get; set; }
        public ICommand? ChatCommand { get; set; }
    }

    /// <summary>
    /// Model cho Chat Message
    /// </summary>
    public class ChatMessageModel : ViewModelBase
    {
        private string _senderName = string.Empty;
        private string _message = string.Empty;
        private DateTime _timestamp;
        private bool _isSent;
        private bool _isTeacher;

        public string SenderName
        {
            get => _senderName;
            set => SetProperty(ref _senderName, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public bool IsSent
        {
            get => _isSent;
            set => SetProperty(ref _isSent, value);
        }

        public bool IsTeacher
        {
            get => _isTeacher;
            set => SetProperty(ref _isTeacher, value);
        }

        public string FormattedTime => Timestamp.ToString("hh:mm tt");
    }

    /// <summary>
    /// Main ViewModel cho Teacher Window
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private string _className = "Lớp 10A1";
        private string _subject = "Toán học";
        private int _onlineStudentCount;
        private int _totalStudentCount = 30;
        private bool _isPresenting;
        private bool _isAllMicMuted;
        private bool _isAllCameraMuted;
        private string _connectionStatus = "Kết nối ổn định";

        public MainViewModel()
        {
            Students = new ObservableCollection<StudentModel>();
            ChatMessages = new ObservableCollection<ChatMessageModel>();

            // Initialize commands
            StartPresentationCommand = new RelayCommand(_ => StartPresentation());
            StopPresentationCommand = new RelayCommand(_ => StopPresentation());
            MuteAllMicsCommand = new RelayCommand(_ => MuteAllMics());
            MuteAllCamerasCommand = new RelayCommand(_ => MuteAllCameras());
            OpenGroupChatCommand = new RelayCommand(_ => OpenGroupChat());
            LockAllStudentsCommand = new RelayCommand(_ => LockAllStudents());
            RefreshStudentListCommand = new RelayCommand(_ => RefreshStudentList());

            // Load sample data
            LoadSampleData();
        }

        #region Properties

        public string ClassName
        {
            get => _className;
            set => SetProperty(ref _className, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public int OnlineStudentCount
        {
            get => _onlineStudentCount;
            set => SetProperty(ref _onlineStudentCount, value);
        }

        public int TotalStudentCount
        {
            get => _totalStudentCount;
            set => SetProperty(ref _totalStudentCount, value);
        }

        public bool IsPresenting
        {
            get => _isPresenting;
            set => SetProperty(ref _isPresenting, value);
        }

        public bool IsAllMicMuted
        {
            get => _isAllMicMuted;
            set
            {
                if (SetProperty(ref _isAllMicMuted, value))
                {
                    foreach (var student in Students)
                    {
                        student.IsMicrophoneOn = !value;
                    }
                }
            }
        }

        public bool IsAllCameraMuted
        {
            get => _isAllCameraMuted;
            set
            {
                if (SetProperty(ref _isAllCameraMuted, value))
                {
                    foreach (var student in Students)
                    {
                        student.IsCameraOn = !value;
                    }
                }
            }
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        public ObservableCollection<StudentModel> Students { get; }
        public ObservableCollection<ChatMessageModel> ChatMessages { get; }

        #endregion

        #region Commands

        public ICommand StartPresentationCommand { get; }
        public ICommand StopPresentationCommand { get; }
        public ICommand MuteAllMicsCommand { get; }
        public ICommand MuteAllCamerasCommand { get; }
        public ICommand OpenGroupChatCommand { get; }
        public ICommand LockAllStudentsCommand { get; }
        public ICommand RefreshStudentListCommand { get; }

        #endregion

        #region Methods

        private void LoadSampleData()
        {
            var sampleStudents = new[]
            {
                new StudentModel { Id = "1", StudentName = "Nguyễn Văn An", IsOnline = true, IsCameraOn = true, IsMicrophoneOn = false },
                new StudentModel { Id = "2", StudentName = "Trần Thị Bình", IsOnline = true, IsCameraOn = true, IsMicrophoneOn = true },
                new StudentModel { Id = "3", StudentName = "Lê Hoàng Cường", IsOnline = false, IsCameraOn = false, IsMicrophoneOn = false },
                new StudentModel { Id = "4", StudentName = "Phạm Thu Dung", IsOnline = true, IsCameraOn = true, IsMicrophoneOn = false },
                new StudentModel { Id = "5", StudentName = "Hoàng Minh Em", IsOnline = true, IsCameraOn = false, IsMicrophoneOn = true },
                new StudentModel { Id = "6", StudentName = "Ngô Văn Phúc", IsOnline = true, IsCameraOn = true, IsMicrophoneOn = true },
                new StudentModel { Id = "7", StudentName = "Trịnh Thị Giang", IsOnline = true, IsCameraOn = true, IsMicrophoneOn = false },
                new StudentModel { Id = "8", StudentName = "Đỗ Hải Hà", IsOnline = true, IsCameraOn = false, IsMicrophoneOn = false },
                new StudentModel { Id = "9", StudentName = "Vũ Quang Huy", IsOnline = true, IsCameraOn = true, IsMicrophoneOn = true },
                new StudentModel { Id = "10", StudentName = "Bùi Thanh Lan", IsOnline = true, IsCameraOn = true, IsMicrophoneOn = false },
            };

            foreach (var student in sampleStudents)
            {
                student.LockCommand = new RelayCommand(_ => LockStudent(student));
                student.ChatCommand = new RelayCommand(_ => ChatWithStudent(student));
                Students.Add(student);
            }

            UpdateOnlineCount();
        }

        private void UpdateOnlineCount()
        {
            int count = 0;
            foreach (var student in Students)
            {
                if (student.IsOnline) count++;
            }
            OnlineStudentCount = count;
        }

        private void StartPresentation()
        {
            IsPresenting = true;
            // TODO: Start screen sharing
        }

        private void StopPresentation()
        {
            IsPresenting = false;
            // TODO: Stop screen sharing
        }

        private void MuteAllMics()
        {
            IsAllMicMuted = !IsAllMicMuted;
        }

        private void MuteAllCameras()
        {
            IsAllCameraMuted = !IsAllCameraMuted;
        }

        private void OpenGroupChat()
        {
            // TODO: Open chat window
        }

        private void LockAllStudents()
        {
            foreach (var student in Students)
            {
                student.IsLocked = true;
            }
        }

        private void LockStudent(StudentModel student)
        {
            student.IsLocked = !student.IsLocked;
        }

        private void ChatWithStudent(StudentModel student)
        {
            // TODO: Open private chat with student
        }

        private void RefreshStudentList()
        {
            // TODO: Refresh from server
            UpdateOnlineCount();
        }

        #endregion
    }

    /// <summary>
    /// ViewModel cho Student Window
    /// </summary>
    public class StudentViewModel : ViewModelBase
    {
        private string _studentName = "Nguyễn Văn An";
        private string _className = "Lớp 10A1";
        private bool _isCameraOn = true;
        private bool _isMicrophoneOn;
        private bool _isHandRaised;
        private bool _isPresentationActive;
        private string _presentationStatus = "Đang chờ giáo viên trình chiếu...";
        private int _testTimeRemaining = 900; // seconds
        private int _testQuestionsCompleted = 8;
        private int _testTotalQuestions = 10;

        public StudentViewModel()
        {
            RaiseHandCommand = new RelayCommand(_ => RaiseHand());
            SendFileCommand = new RelayCommand(_ => SendFile());
            OpenChatCommand = new RelayCommand(_ => OpenChat());
            RequestHelpCommand = new RelayCommand(_ => RequestHelp());
        }

        #region Properties

        public string StudentName
        {
            get => _studentName;
            set => SetProperty(ref _studentName, value);
        }

        public string ClassName
        {
            get => _className;
            set => SetProperty(ref _className, value);
        }

        public bool IsCameraOn
        {
            get => _isCameraOn;
            set => SetProperty(ref _isCameraOn, value);
        }

        public bool IsMicrophoneOn
        {
            get => _isMicrophoneOn;
            set => SetProperty(ref _isMicrophoneOn, value);
        }

        public bool IsHandRaised
        {
            get => _isHandRaised;
            set => SetProperty(ref _isHandRaised, value);
        }

        public bool IsPresentationActive
        {
            get => _isPresentationActive;
            set => SetProperty(ref _isPresentationActive, value);
        }

        public string PresentationStatus
        {
            get => _presentationStatus;
            set => SetProperty(ref _presentationStatus, value);
        }

        public int TestTimeRemaining
        {
            get => _testTimeRemaining;
            set => SetProperty(ref _testTimeRemaining, value);
        }

        public string FormattedTestTime
        {
            get
            {
                var minutes = _testTimeRemaining / 60;
                var seconds = _testTimeRemaining % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }

        public int TestQuestionsCompleted
        {
            get => _testQuestionsCompleted;
            set => SetProperty(ref _testQuestionsCompleted, value);
        }

        public int TestTotalQuestions
        {
            get => _testTotalQuestions;
            set => SetProperty(ref _testTotalQuestions, value);
        }

        public double TestProgress => (double)_testQuestionsCompleted / _testTotalQuestions * 100;

        #endregion

        #region Commands

        public ICommand RaiseHandCommand { get; }
        public ICommand SendFileCommand { get; }
        public ICommand OpenChatCommand { get; }
        public ICommand RequestHelpCommand { get; }

        #endregion

        #region Methods

        private void RaiseHand()
        {
            IsHandRaised = !IsHandRaised;
        }

        private void SendFile()
        {
            // TODO: Open file dialog and send file
        }

        private void OpenChat()
        {
            // TODO: Open chat window
        }

        private void RequestHelp()
        {
            // TODO: Send help request
        }

        #endregion
    }
}
