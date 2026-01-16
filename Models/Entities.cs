using System;

namespace ClassroomManagement.Models
{
    /// <summary>
    /// Thông tin người dùng (Giáo viên)
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = "teacher";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Phiên học
    /// </summary>
    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "active";
    }

    /// <summary>
    /// Thông tin học sinh (Client)
    /// </summary>
    public class Student
    {
        public int Id { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ComputerName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public bool IsLocked { get; set; }
        public bool MicEnabled { get; set; } = true;
        public bool CameraEnabled { get; set; } = true;
        public DateTime? LastSeen { get; set; }
        public int? SessionId { get; set; }
        
        // Screen thumbnail (không lưu DB)
        public byte[]? ScreenThumbnail { get; set; }
    }

    /// <summary>
    /// Bài kiểm tra
    /// </summary>
    public class Test
    {
        public int Id { get; set; }
        public int? SessionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public int Duration { get; set; } = 900; // Seconds
        public int TotalQuestions { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleAnswers { get; set; }
        public bool ShowResult { get; set; } = true;
        public string Status { get; set; } = "draft";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Câu hỏi
    /// </summary>
    public class Question
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public int OrderIndex { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Type { get; set; } = "multiple_choice";
        public string Options { get; set; } = "[]"; // JSON array
        public string CorrectAnswer { get; set; } = string.Empty;
        public int Points { get; set; } = 1;
    }

    /// <summary>
    /// Kết quả kiểm tra
    /// </summary>
    public class TestResult
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int TestId { get; set; }
        public string Answers { get; set; } = "{}"; // JSON object
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public double Score { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string Status { get; set; } = "in_progress";
    }

    /// <summary>
    /// Tin nhắn chat
    /// </summary>
    public class ChatMessage
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public string SenderType { get; set; } = string.Empty; // "teacher" or "student"
        public int SenderId { get; set; }
        public int? ReceiverId { get; set; } // null = group chat
        public string Content { get; set; } = string.Empty;
        public bool IsGroup { get; set; } = true;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties (không lưu DB)
        public string SenderName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lịch sử file
    /// </summary>
    public class FileRecord
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int? StudentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long Size { get; set; }
        public string Direction { get; set; } = string.Empty; // "upload" or "download"
        public string Status { get; set; } = "completed";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
