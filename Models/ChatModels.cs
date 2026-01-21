
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClassroomManagement.Models
{
    public static class MessageContentType
    {
        public const string Text = "text";
        public const string Image = "image";
        public const string File = "file";
        public const string SystemString = "system";
    }

    public class ChatGroupViewModel : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ObservableCollection<Student> Members { get; set; } = new();
        public bool IsSelected { get; set; }

        private string _lastMessage = string.Empty;
        public string LastMessage
        {
            get => _lastMessage;
            set { _lastMessage = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ChatAttachmentInfo
    {
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
    }
}
