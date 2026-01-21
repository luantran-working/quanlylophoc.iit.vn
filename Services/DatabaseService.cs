using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service quản lý Database SQLite
    /// </summary>
    public class DatabaseService
    {
        private static DatabaseService? _instance;
        private readonly string _connectionString;
        private readonly string _databasePath;

        public static DatabaseService Instance => _instance ??= new DatabaseService();

        private DatabaseService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "IIT", "ClassroomManagement");

            Directory.CreateDirectory(appDataPath);
            _databasePath = Path.Combine(appDataPath, "classroom.db");
            _connectionString = $"Data Source={_databasePath}";

            InitializeDatabase();
        }

        private SqliteConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private void InitializeDatabase()
        {
            using var connection = GetConnection();

            var createTables = @"
                -- Users table
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    DisplayName TEXT NOT NULL,
                    Role TEXT DEFAULT 'teacher',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                -- Sessions table
                CREATE TABLE IF NOT EXISTS Sessions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId INTEGER NOT NULL,
                    ClassName TEXT NOT NULL,
                    Subject TEXT,
                    StartTime DATETIME DEFAULT CURRENT_TIMESTAMP,
                    EndTime DATETIME,
                    Status TEXT DEFAULT 'active',
                    FOREIGN KEY (UserId) REFERENCES Users(Id)
                );

                -- Students table
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    MachineId TEXT NOT NULL UNIQUE,
                    DisplayName TEXT NOT NULL,
                    ComputerName TEXT,
                    IpAddress TEXT,
                    IsOnline INTEGER DEFAULT 0,
                    IsLocked INTEGER DEFAULT 0,
                    MicEnabled INTEGER DEFAULT 1,
                    CameraEnabled INTEGER DEFAULT 1,
                    LastSeen DATETIME,
                    SessionId INTEGER,
                    FOREIGN KEY (SessionId) REFERENCES Sessions(Id)
                );

                -- Tests table
                CREATE TABLE IF NOT EXISTS Tests (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER,
                    Title TEXT NOT NULL,
                    Subject TEXT,
                    Duration INTEGER DEFAULT 900,
                    TotalQuestions INTEGER DEFAULT 0,
                    ShuffleQuestions INTEGER DEFAULT 0,
                    ShuffleAnswers INTEGER DEFAULT 0,
                    ShowResult INTEGER DEFAULT 1,
                    Status TEXT DEFAULT 'draft',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (SessionId) REFERENCES Sessions(Id)
                );

                -- Questions table
                CREATE TABLE IF NOT EXISTS Questions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TestId INTEGER NOT NULL,
                    OrderIndex INTEGER DEFAULT 0,
                    Content TEXT NOT NULL,
                    Type TEXT DEFAULT 'multiple_choice',
                    Options TEXT,
                    CorrectAnswer TEXT,
                    Points INTEGER DEFAULT 1,
                    FOREIGN KEY (TestId) REFERENCES Tests(Id) ON DELETE CASCADE
                );

                -- TestResults table
                CREATE TABLE IF NOT EXISTS TestResults (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StudentId INTEGER NOT NULL,
                    TestId INTEGER NOT NULL,
                    Answers TEXT,
                    CorrectCount INTEGER DEFAULT 0,
                    TotalCount INTEGER DEFAULT 0,
                    Score REAL DEFAULT 0,
                    StartedAt DATETIME,
                    SubmittedAt DATETIME,
                    Status TEXT DEFAULT 'in_progress',
                    FOREIGN KEY (StudentId) REFERENCES Students(Id),
                    FOREIGN KEY (TestId) REFERENCES Tests(Id)
                );

                -- ChatMessages table
                CREATE TABLE IF NOT EXISTS ChatMessages (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER NOT NULL,
                    SenderType TEXT NOT NULL,
                    SenderId INTEGER NOT NULL,
                    ReceiverId INTEGER,
                    Content TEXT NOT NULL,
                    IsGroup INTEGER DEFAULT 1,
                    IsRead INTEGER DEFAULT 0,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (SessionId) REFERENCES Sessions(Id)
                );

                -- FileRecords table
                CREATE TABLE IF NOT EXISTS FileRecords (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SessionId INTEGER NOT NULL,
                    StudentId INTEGER,
                    FileName TEXT NOT NULL,
                    OriginalName TEXT,
                    FilePath TEXT NOT NULL,
                    Size INTEGER DEFAULT 0,
                    Direction TEXT,
                    Status TEXT DEFAULT 'completed',
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (SessionId) REFERENCES Sessions(Id),
                    FOREIGN KEY (StudentId) REFERENCES Students(Id)
                );

                -- Assignments table
                CREATE TABLE IF NOT EXISTS Assignments (
                    Id TEXT PRIMARY KEY,
                    SessionId INTEGER,
                    StudentId TEXT,
                    StudentName TEXT,
                    Note TEXT,
                    SubmittedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );

                -- AssignmentFiles table
                CREATE TABLE IF NOT EXISTS AssignmentFiles (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AssignmentId TEXT,
                    FileName TEXT,
                    FileSize INTEGER,
                    LocalPath TEXT,
                    FOREIGN KEY (AssignmentId) REFERENCES Assignments(Id) ON DELETE CASCADE
                );
            ";

            using var command = new SqliteCommand(createTables, connection);
            command.ExecuteNonQuery();

            // Migration for Existing Database
            try { new SqliteCommand("ALTER TABLE ChatMessages ADD COLUMN ContentType TEXT DEFAULT 'text'", connection).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE ChatMessages ADD COLUMN AttachmentPath TEXT", connection).ExecuteNonQuery(); } catch { }
            try { new SqliteCommand("ALTER TABLE ChatMessages ADD COLUMN GroupId TEXT", connection).ExecuteNonQuery(); } catch { }

            // New Tables if not exists (in case they were missed above oradded later)
            var newTables = @"
                CREATE TABLE IF NOT EXISTS ChatGroups (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    CreatorId INTEGER NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );
                CREATE TABLE IF NOT EXISTS ChatGroupMembers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    GroupId TEXT NOT NULL,
                    StudentId INTEGER NOT NULL,
                    JoinedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (GroupId) REFERENCES ChatGroups(Id)
                );
            ";
            new SqliteCommand(newTables, connection).ExecuteNonQuery();

            // Insert default admin user if not exists
            InsertDefaultUser(connection);
        }

        private void InsertDefaultUser(SqliteConnection connection)
        {
            var checkUser = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
            using var checkCmd = new SqliteCommand(checkUser, connection);
            var count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count == 0)
            {
                var passwordHash = HashPassword("123456");
                var insertUser = @"
                    INSERT INTO Users (Username, PasswordHash, DisplayName, Role)
                    VALUES ('admin', @hash, 'Quản trị viên', 'admin')";

                using var insertCmd = new SqliteCommand(insertUser, connection);
                insertCmd.Parameters.AddWithValue("@hash", passwordHash);
                insertCmd.ExecuteNonQuery();
            }
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        #region User Methods

        public User? ValidateUser(string username, string password)
        {
            using var connection = GetConnection();
            var passwordHash = HashPassword(password);

            var sql = @"SELECT Id, Username, PasswordHash, DisplayName, Role, CreatedAt, UpdatedAt
                        FROM Users WHERE Username = @username AND PasswordHash = @hash";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@hash", passwordHash);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    DisplayName = reader.GetString(3),
                    Role = reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    UpdatedAt = reader.GetDateTime(6)
                };
            }
            return null;
        }

        public bool UpdatePassword(int userId, string newPassword)
        {
            using var connection = GetConnection();
            var passwordHash = HashPassword(newPassword);

            var sql = "UPDATE Users SET PasswordHash = @hash, UpdatedAt = CURRENT_TIMESTAMP WHERE Id = @id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@hash", passwordHash);
            command.Parameters.AddWithValue("@id", userId);

            return command.ExecuteNonQuery() > 0;
        }

        #endregion

        #region Session Methods

        public int CreateSession(int userId, string className, string subject)
        {
            using var connection = GetConnection();

            var sql = @"INSERT INTO Sessions (UserId, ClassName, Subject)
                        VALUES (@userId, @className, @subject);
                        SELECT last_insert_rowid();";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);
            command.Parameters.AddWithValue("@className", className);
            command.Parameters.AddWithValue("@subject", subject);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public void EndSession(int sessionId)
        {
            using var connection = GetConnection();

            var sql = "UPDATE Sessions SET EndTime = CURRENT_TIMESTAMP, Status = 'ended' WHERE Id = @id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", sessionId);
            command.ExecuteNonQuery();
        }

        public Session? GetActiveSession(int userId)
        {
            using var connection = GetConnection();

            var sql = @"SELECT Id, UserId, ClassName, Subject, StartTime, EndTime, Status
                        FROM Sessions WHERE UserId = @userId AND Status = 'active'
                        ORDER BY StartTime DESC LIMIT 1";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", userId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Session
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ClassName = reader.GetString(2),
                    Subject = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    StartTime = reader.GetDateTime(4),
                    EndTime = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    Status = reader.GetString(6)
                };
            }
            return null;
        }

        #endregion

        #region Student Methods

        public Student? GetOrCreateStudent(string machineId, string displayName, string computerName, string ipAddress)
        {
            using var connection = GetConnection();

            // Try to get existing
            var selectSql = "SELECT Id FROM Students WHERE MachineId = @machineId";
            using var selectCmd = new SqliteCommand(selectSql, connection);
            selectCmd.Parameters.AddWithValue("@machineId", machineId);
            var existingId = selectCmd.ExecuteScalar();

            if (existingId != null)
            {
                // Update existing
                var updateSql = @"UPDATE Students SET
                    DisplayName = @displayName, ComputerName = @computerName,
                    IpAddress = @ipAddress, IsOnline = 1, LastSeen = CURRENT_TIMESTAMP
                    WHERE MachineId = @machineId";
                using var updateCmd = new SqliteCommand(updateSql, connection);
                updateCmd.Parameters.AddWithValue("@displayName", displayName);
                updateCmd.Parameters.AddWithValue("@computerName", computerName);
                updateCmd.Parameters.AddWithValue("@ipAddress", ipAddress);
                updateCmd.Parameters.AddWithValue("@machineId", machineId);
                updateCmd.ExecuteNonQuery();

                return GetStudentById(Convert.ToInt32(existingId));
            }
            else
            {
                // Create new
                var insertSql = @"INSERT INTO Students (MachineId, DisplayName, ComputerName, IpAddress, IsOnline, LastSeen)
                    VALUES (@machineId, @displayName, @computerName, @ipAddress, 1, CURRENT_TIMESTAMP);
                    SELECT last_insert_rowid();";
                using var insertCmd = new SqliteCommand(insertSql, connection);
                insertCmd.Parameters.AddWithValue("@machineId", machineId);
                insertCmd.Parameters.AddWithValue("@displayName", displayName);
                insertCmd.Parameters.AddWithValue("@computerName", computerName);
                insertCmd.Parameters.AddWithValue("@ipAddress", ipAddress);

                var newId = Convert.ToInt32(insertCmd.ExecuteScalar());
                return GetStudentById(newId);
            }
        }

        public Student? GetStudentById(int id)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Students WHERE Id = @id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapStudent(reader);
            }
            return null;
        }

        public List<Student> GetOnlineStudents(int? sessionId = null)
        {
            using var connection = GetConnection();
            var sql = sessionId.HasValue
                ? "SELECT * FROM Students WHERE IsOnline = 1 AND SessionId = @sessionId"
                : "SELECT * FROM Students WHERE IsOnline = 1";

            using var command = new SqliteCommand(sql, connection);
            if (sessionId.HasValue)
                command.Parameters.AddWithValue("@sessionId", sessionId.Value);

            var students = new List<Student>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                students.Add(MapStudent(reader));
            }
            return students;
        }

        public void SetStudentOnline(string machineId, bool online)
        {
            using var connection = GetConnection();
            var sql = "UPDATE Students SET IsOnline = @online, LastSeen = CURRENT_TIMESTAMP WHERE MachineId = @machineId";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@online", online ? 1 : 0);
            command.Parameters.AddWithValue("@machineId", machineId);
            command.ExecuteNonQuery();
        }

        public void SetStudentLocked(string machineId, bool locked)
        {
            using var connection = GetConnection();
            var sql = "UPDATE Students SET IsLocked = @locked WHERE MachineId = @machineId";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@locked", locked ? 1 : 0);
            command.Parameters.AddWithValue("@machineId", machineId);
            command.ExecuteNonQuery();
        }

        private Student MapStudent(SqliteDataReader reader)
        {
            return new Student
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                MachineId = reader.GetString(reader.GetOrdinal("MachineId")),
                DisplayName = reader.GetString(reader.GetOrdinal("DisplayName")),
                ComputerName = reader.IsDBNull(reader.GetOrdinal("ComputerName")) ? "" : reader.GetString(reader.GetOrdinal("ComputerName")),
                IpAddress = reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? "" : reader.GetString(reader.GetOrdinal("IpAddress")),
                IsOnline = reader.GetInt32(reader.GetOrdinal("IsOnline")) == 1,
                IsLocked = reader.GetInt32(reader.GetOrdinal("IsLocked")) == 1,
                MicEnabled = reader.GetInt32(reader.GetOrdinal("MicEnabled")) == 1,
                CameraEnabled = reader.GetInt32(reader.GetOrdinal("CameraEnabled")) == 1,
                LastSeen = reader.IsDBNull(reader.GetOrdinal("LastSeen")) ? null : reader.GetDateTime(reader.GetOrdinal("LastSeen")),
                SessionId = reader.IsDBNull(reader.GetOrdinal("SessionId")) ? null : reader.GetInt32(reader.GetOrdinal("SessionId"))
            };
        }

        #endregion

        #region Chat Methods

        public int SaveChatMessage(ChatMessage message)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO ChatMessages (SessionId, SenderType, SenderId, ReceiverId, Content, IsGroup, ContentType, AttachmentPath, GroupId)
                        VALUES (@sessionId, @senderType, @senderId, @receiverId, @content, @isGroup, @contentType, @attachmentPath, @groupId);
                        SELECT last_insert_rowid();";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@sessionId", message.SessionId);
            command.Parameters.AddWithValue("@senderType", message.SenderType);
            command.Parameters.AddWithValue("@senderId", message.SenderId);
            command.Parameters.AddWithValue("@receiverId", message.ReceiverId.HasValue ? message.ReceiverId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@content", message.Content);
            command.Parameters.AddWithValue("@isGroup", message.IsGroup ? 1 : 0);
            command.Parameters.AddWithValue("@contentType", message.ContentType);
            command.Parameters.AddWithValue("@attachmentPath", message.AttachmentPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@groupId", message.GroupId ?? (object)DBNull.Value);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        public List<ChatMessage> GetChatMessages(int sessionId, int? studentId = null, int limit = 100)
        {
            using var connection = GetConnection();
            var sql = studentId.HasValue
                ? @"SELECT * FROM ChatMessages WHERE SessionId = @sessionId
                    AND (IsGroup = 1 OR (ReceiverId = @studentId OR SenderId = @studentId))
                    ORDER BY CreatedAt DESC LIMIT @limit"
                : @"SELECT * FROM ChatMessages WHERE SessionId = @sessionId AND IsGroup = 1
                    ORDER BY CreatedAt DESC LIMIT @limit";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@sessionId", sessionId);
            command.Parameters.AddWithValue("@limit", limit);
            if (studentId.HasValue)
                command.Parameters.AddWithValue("@studentId", studentId.Value);

            var messages = new List<ChatMessage>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                messages.Add(new ChatMessage
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    SessionId = reader.GetInt32(reader.GetOrdinal("SessionId")),
                    SenderType = reader.GetString(reader.GetOrdinal("SenderType")),
                    SenderId = reader.GetInt32(reader.GetOrdinal("SenderId")),
                    ReceiverId = reader.IsDBNull(reader.GetOrdinal("ReceiverId")) ? null : reader.GetInt32(reader.GetOrdinal("ReceiverId")),
                    Content = reader.GetString(reader.GetOrdinal("Content")),
                    IsGroup = reader.GetInt32(reader.GetOrdinal("IsGroup")) == 1,
                    IsRead = reader.GetInt32(reader.GetOrdinal("IsRead")) == 1,
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    ContentType = reader.IsDBNull(reader.GetOrdinal("ContentType")) ? "text" : reader.GetString(reader.GetOrdinal("ContentType")),
                    AttachmentPath = reader.IsDBNull(reader.GetOrdinal("AttachmentPath")) ? null : reader.GetString(reader.GetOrdinal("AttachmentPath")),
                    GroupId = reader.IsDBNull(reader.GetOrdinal("GroupId")) ? null : reader.GetString(reader.GetOrdinal("GroupId")),
                });
            }
            messages.Reverse(); // Oldest first
            return messages;
        }

        public List<ChatGroup> GetChatGroups()
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM ChatGroups ORDER BY CreatedAt DESC";
            using var command = new SqliteCommand(sql, connection);

            var groups = new List<ChatGroup>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                groups.Add(new ChatGroup
                {
                    Id = reader.GetString(0),
                    Name = reader.GetString(1),
                    CreatorId = reader.GetInt32(2),
                    CreatedAt = reader.GetDateTime(3)
                });
            }
            return groups;
        }

        public void CreateChatGroup(ChatGroup group)
        {
            using var connection = GetConnection();
            var sql = "INSERT INTO ChatGroups (Id, Name, CreatorId, CreatedAt) VALUES (@id, @name, @creatorId, @createdAt)";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", group.Id);
            command.Parameters.AddWithValue("@name", group.Name);
            command.Parameters.AddWithValue("@creatorId", group.CreatorId);
            command.Parameters.AddWithValue("@createdAt", group.CreatedAt);
            command.ExecuteNonQuery();
        }

        public void AddChatGroupMember(string groupId, int studentId)
        {
            using var connection = GetConnection();
            var sql = "INSERT INTO ChatGroupMembers (GroupId, StudentId) VALUES (@groupId, @studentId)";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@groupId", groupId);
            command.Parameters.AddWithValue("@studentId", studentId);
            command.ExecuteNonQuery();
        }

        public List<int> GetChatGroupMemberIds(string groupId)
        {
            using var connection = GetConnection();
            var sql = "SELECT StudentId FROM ChatGroupMembers WHERE GroupId = @groupId";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@groupId", groupId);

            var ids = new List<int>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                ids.Add(reader.GetInt32(0));
            }
            return ids;
        }

        #endregion

        #region File Methods

        public int SaveFileRecord(FileRecord record)
        {
            using var connection = GetConnection();
            var sql = @"INSERT INTO FileRecords (SessionId, StudentId, FileName, OriginalName, FilePath, Size, Direction)
                        VALUES (@sessionId, @studentId, @fileName, @originalName, @filePath, @size, @direction);
                        SELECT last_insert_rowid();";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@sessionId", record.SessionId);
            command.Parameters.AddWithValue("@studentId", record.StudentId.HasValue ? record.StudentId.Value : DBNull.Value);
            command.Parameters.AddWithValue("@fileName", record.FileName);
            command.Parameters.AddWithValue("@originalName", record.OriginalName);
            command.Parameters.AddWithValue("@filePath", record.FilePath);
            command.Parameters.AddWithValue("@size", record.Size);
            command.Parameters.AddWithValue("@direction", record.Direction);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        #endregion
        #region Assignment Methods

        public void SaveAssignment(AssignmentSubmission submission)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"INSERT INTO Assignments (Id, SessionId, StudentId, StudentName, Note, SubmittedAt)
                            VALUES (@id, @sessionId, @studentId, @studentName, @note, @submittedAt)";

                using (var command = new SqliteCommand(sql, connection, transaction))
                {
                    command.Parameters.AddWithValue("@id", submission.Id);
                    command.Parameters.AddWithValue("@sessionId", submission.SessionId);
                    command.Parameters.AddWithValue("@studentId", submission.StudentId);
                    command.Parameters.AddWithValue("@studentName", submission.StudentName);
                    command.Parameters.AddWithValue("@note", submission.Note ?? "");
                    command.Parameters.AddWithValue("@submittedAt", submission.SubmittedAt);
                    command.ExecuteNonQuery();
                }

                foreach (var file in submission.Files)
                {
                    var fileSql = @"INSERT INTO AssignmentFiles (AssignmentId, FileName, FileSize, LocalPath)
                                    VALUES (@assignmentId, @fileName, @fileSize, @localPath)";
                    using (var fileCmd = new SqliteCommand(fileSql, connection, transaction))
                    {
                        fileCmd.Parameters.AddWithValue("@assignmentId", submission.Id);
                        fileCmd.Parameters.AddWithValue("@fileName", file.FileName);
                        fileCmd.Parameters.AddWithValue("@fileSize", file.FileSize);
                        fileCmd.Parameters.AddWithValue("@localPath", file.LocalPath);
                        fileCmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public List<AssignmentSubmission> GetAssignments(int sessionId)
        {
            using var connection = GetConnection();
            var sql = "SELECT * FROM Assignments WHERE SessionId = @sessionId ORDER BY SubmittedAt DESC";
            var assignments = new List<AssignmentSubmission>();

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@sessionId", sessionId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetString(reader.GetOrdinal("Id"));
                var assignment = new AssignmentSubmission
                {
                    Id = id,
                    SessionId = reader.IsDBNull(reader.GetOrdinal("SessionId")) ? "" : reader.GetInt32(reader.GetOrdinal("SessionId")).ToString(),
                    StudentId = reader.GetString(reader.GetOrdinal("StudentId")),
                    StudentName = reader.GetString(reader.GetOrdinal("StudentName")),
                    Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? "" : reader.GetString(reader.GetOrdinal("Note")),
                    SubmittedAt = reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                    Files = GetAssignmentFiles(id, connection)
                };
                assignments.Add(assignment);
            }
            return assignments;
        }

        private List<SubmittedFile> GetAssignmentFiles(string assignmentId, SqliteConnection connection)
        {
            var files = new List<SubmittedFile>();
            var sql = "SELECT FileName, FileSize, LocalPath FROM AssignmentFiles WHERE AssignmentId = @assignmentId";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@assignmentId", assignmentId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                files.Add(new SubmittedFile
                {
                    FileName = reader.GetString(0),
                    FileSize = reader.GetInt64(1),
                    LocalPath = reader.GetString(2)
                });
            }
            return files;
        }

        #endregion
    }
}
