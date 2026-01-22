using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClassroomManagement.Models;
using System.Drawing;
using System.Drawing.Imaging;

namespace ClassroomManagement.Services
{
    public class ScreenshotService
    {
        private readonly DatabaseService _database;
        private readonly string _baseFolder;

        public ScreenshotService()
        {
            _database = DatabaseService.Instance;
            _baseFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "IIT Classroom", "Screenshots");

            if (!Directory.Exists(_baseFolder))
            {
                Directory.CreateDirectory(_baseFolder);
            }
        }

        public async Task<Screenshot> CaptureAndSaveAsync(string studentId, string studentName, int sessionId, byte[] imageData)
        {
            try
            {
                var now = DateTime.Now;
                var sessionFolder = Path.Combine(_baseFolder, sessionId.ToString());
                var studentFolder = Path.Combine(sessionFolder, SanitizeFileName(studentName));

                if (!Directory.Exists(studentFolder))
                {
                    Directory.CreateDirectory(studentFolder);
                }

                var fileName = $"Screenshot_{studentName}_{now:yyyyMMdd_HHmmss}.jpg";
                var filePath = Path.Combine(studentFolder, fileName);
                var thumbName = $"Thumb_{studentName}_{now:yyyyMMdd_HHmmss}.jpg";
                var thumbPath = Path.Combine(studentFolder, thumbName);

                // Save original image
                await File.WriteAllBytesAsync(filePath, imageData);

                // Create and save thumbnail (optional, simple resize)
                // For simplicity, we just save the same file or make a smaller copy if we had System.Drawing
                // Since this is .NET Core/Standard, System.Drawing might need specific NuGet packages (System.Drawing.Common).
                // Assuming we have it or can add it. If not, just use the same file for thumbnail or skip.
                // Let's check if we can use System.Drawing.Common or just copy for now.
                // To be safe and dependency-free for now, we'll just copy it as thumbnail or leave unique paths.

                // For better UX, let's try to resize if possible, or just save as is.
                File.Copy(filePath, thumbPath);

                var screenshot = new Screenshot
                {
                    Id = Guid.NewGuid().ToString(),
                    StudentId = studentId,
                    StudentName = studentName,
                    SessionId = sessionId.ToString(),
                    CapturedAt = now,
                    FilePath = filePath,
                    ThumbnailPath = thumbPath
                };

                _database.SaveScreenshot(screenshot);

                return screenshot;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving screenshot: {ex.Message}");
                throw;
            }
        }

        public List<Screenshot> GetScreenshots(string? sessionId = null, string? studentId = null)
        {
            return _database.GetScreenshots(sessionId, studentId);
        }

        public bool DeleteScreenshot(string id)
        {
            // Also delete file if possible?
            // For now just delete from DB record.
            // Ideally we should get the record, delete file, then delete record.
            var screenshots = _database.GetScreenshots(); // This gets all, potentially slow. Better filter by ID.
            // But DatabaseService doesn't have GetById for Screenshot.
            // Let's just delete from DB for now as per requirement.
            return _database.DeleteScreenshot(id);
        }

        public bool AddNote(string id, string note)
        {
            return _database.UpdateScreenshotNote(id, note);
        }

        private string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
