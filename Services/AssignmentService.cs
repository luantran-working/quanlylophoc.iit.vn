using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class AssignmentService
    {
        private static AssignmentService? _instance;
        public static AssignmentService Instance => _instance ??= new AssignmentService();

        private readonly string _basePath;
        private readonly DatabaseService _db;

        private AssignmentService()
        {
            _db = DatabaseService.Instance;
            _basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "IIT", "ClassroomManagement", "Assignments");

            Directory.CreateDirectory(_basePath);
        }

        public async Task ProcessSubmissionAsync(AssignmentSubmission submission)
        {
            try
            {
                // Create directory structure: SessionId / StudentName_StudentId
                var sessionFolder = Path.Combine(_basePath, $"Session_{submission.SessionId}");
                var studentFolderName = $"{Sanitize(submission.StudentName)}_{Sanitize(submission.StudentId)}";
                var uniqueStudentFolder = Path.Combine(sessionFolder, studentFolderName);

                Directory.CreateDirectory(uniqueStudentFolder);

                foreach (var file in submission.Files)
                {
                    var fileName = Sanitize(file.FileName);
                    var fullPath = Path.Combine(uniqueStudentFolder, fileName);

                    // Save file content if available
                    if (file.Data != null && file.Data.Length > 0)
                    {
                        await File.WriteAllBytesAsync(fullPath, file.Data);
                    }

                    // Update LocalPath in model for DB
                    file.LocalPath = fullPath;
                }

                // Save to Database
                _db.SaveAssignment(submission);
            }
            catch (Exception ex)
            {
                // Log error ideally
                Console.WriteLine($"Error processing submission: {ex.Message}");
                throw;
            }
        }

        public List<AssignmentSubmission> GetSubmissions(int sessionId)
        {
            return _db.GetAssignments(sessionId);
        }

        public async Task<byte[]> GetFileContentAsync(string localPath)
        {
            if (File.Exists(localPath))
            {
                return await File.ReadAllBytesAsync(localPath);
            }
            return Array.Empty<byte>();
        }

        public void OpenFileFolder(string localPath)
        {
             if (File.Exists(localPath))
             {
                 string argument = "/select, \"" + localPath + "\"";
                 System.Diagnostics.Process.Start("explorer.exe", argument);
             }
             else if (Directory.Exists(localPath))
             {
                 System.Diagnostics.Process.Start("explorer.exe", localPath);
             }
        }

        private string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "unknown";
            var invalidChars = Path.GetInvalidFileNameChars();
            return string.Join("_", input.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }
    }
}
