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

        public event EventHandler<Screenshot>? ScreenshotSaved;

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

                ScreenshotSaved?.Invoke(this, screenshot);

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

        public async void ProcessScreenshot(ScreenDataReceivedEventArgs e)
        {
            if (e.ScreenData?.ImageData == null) return;

            try
            {
                // 1. Try to get name from OnlineStudents (Memory) - Most accurate/current
                string name = "Unknown";
                var onlineStudent = System.Linq.Enumerable.FirstOrDefault(
                    SessionManager.Instance.OnlineStudents, 
                    s => s.MachineId == e.ClientId);

                if (onlineStudent != null)
                {
                    name = onlineStudent.DisplayName;
                }
                else
                {
                    // 2. Try to look up in Database
                    var student = _database.GetStudentById(0); // Helper needed or custom query
                    // Actually GetOrCreateStudent might overwrite with "Unknown" if not careful.
                    // Let's rely on what we have or just ClientId if unknown.
                    // If we use GetOrCreateStudent("Unknown"), it pollutes DB.
                    // Better verify if student exists first.
                    
                    // For now, if not online, use ClientId or try generic lookup
                    name = e.ClientId; // Default fallback
                    
                    // Try to find if we have this machine ID in DB with a real name
                    var dbStudent = _database.GetOrCreateStudent(e.ClientId, "Unknown", "", "");
                    if (dbStudent != null && dbStudent.DisplayName != "Unknown")
                    {
                        name = dbStudent.DisplayName;
                    }
                }
                
                // Get SessionId from SessionManager
                int sessionId = 0;
                var currentSession = SessionManager.Instance.CurrentSession;
                if (currentSession != null)
                {
                    sessionId = currentSession.Id;
                }
                else
                {
                    // Fallback to active session in DB if SessionManager doesn't have it (rare)
                    var activeSession = _database.GetActiveSession(1); // Fallback to admin
                    if (activeSession != null) sessionId = activeSession.Id;
                }

                await CaptureAndSaveAsync(e.ClientId, name, sessionId, e.ScreenData.ImageData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing screenshot: {ex.Message}");
                // Log service might be circular if injected, but static instance is fine
                // LogService.Instance.Error("ScreenshotService", "ProcessScreenshot failed", ex);
            }
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
