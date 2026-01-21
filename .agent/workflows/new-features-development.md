---
description: Workflow phát triển các tính năng mới cho Phần mềm Quản lý Lớp học IIT. Bao gồm: Chat nâng cao, Kiểm tra thông tin máy tính, Quản lý ứng dụng, Thu thập file, Nộp bài tập, Gửi file hàng loạt, và Bình chọn.
---

# Workflow Phát triển Tính năng Mới

## Tổng quan Tính năng Cần Phát triển

| STT | Tính năng | Mô tả ngắn | Độ ưu tiên |
|-----|-----------|------------|------------|
| 1 | Chat Nâng cao | Chat nhóm tùy chỉnh, gửi hình/file | Cao |
| 2 | Kiểm tra Thông tin Máy tính | Thông tin ổ đĩa, USB | Trung bình |
| 3 | Quản lý Ứng dụng | Xem và đóng ứng dụng đang chạy | Cao |
| 4 | Thu thập File | Thu file từ thư mục chỉ định | Trung bình |
| 5 | Nộp Bài tập | Học sinh upload bài tập | Cao |
| 6 | Gửi File Hàng loạt | Phát file với thông báo | Trung bình |
| 7 | Bình chọn Thời gian thực | Tạo poll và vote realtime | Cao |

---

## Tính năng 1: Chat Nâng cao

### Mô tả Chi tiết
- Chat cá nhân (1-1) giữa giáo viên và học sinh
- Tạo nhóm chat tùy chỉnh (chỉ giáo viên mới có thể tạo)
- Gửi hình ảnh vào nhóm chat
- Gửi file đính kèm vào nhóm chat
- Giao diện chat hiện đại, đầy đủ tính năng

### Thay đổi Cần thực hiện

#### A. Backend/Models
**Files:**
- `Models/ChatModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật - thêm MessageType mới)
- `Models/Entities.cs` (Cập nhật - thêm ChatGroup entity)

**Nội dung:**
```csharp
// ChatModels.cs
public class ChatGroup
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CreatorId { get; set; }
    public List<string> MemberIds { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ChatMessage
{
    public string Id { get; set; }
    public string GroupId { get; set; } // null = private chat
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string ReceiverId { get; set; } // for private chat
    public string Content { get; set; }
    public MessageContentType ContentType { get; set; }
    public string? AttachmentPath { get; set; }
    public string? AttachmentName { get; set; }
    public long? AttachmentSize { get; set; }
    public DateTime Timestamp { get; set; }
}

public enum MessageContentType
{
    Text,
    Image,
    File
}
```

**NetworkModels.cs - Thêm MessageType:**
```csharp
// Chat group
ChatGroupCreate = 0x32,
ChatGroupInvite = 0x33,
ChatGroupLeave = 0x34,
ChatGroupList = 0x35,
ChatImageMessage = 0x36,
ChatFileMessage = 0x37,
```

#### B. Services
**Files:**
- `Services/ChatService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Cập nhật - thêm bảng chat)
- `Services/SessionManager.cs` (Cập nhật)
- `Services/NetworkServerService.cs` (Cập nhật)
- `Services/NetworkClientService.cs` (Cập nhật)

**ChatService.cs - Các method chính:**
```csharp
public class ChatService
{
    // Singleton pattern
    public static ChatService Instance { get; }

    // Group management (Teacher only)
    public async Task<ChatGroup> CreateGroupAsync(string name, List<string> memberIds);
    public async Task<bool> AddMemberToGroupAsync(string groupId, string memberId);
    public async Task<bool> RemoveMemberFromGroupAsync(string groupId, string memberId);
    public async Task<List<ChatGroup>> GetMyGroupsAsync();

    // Messaging
    public async Task SendTextMessageAsync(string groupId, string content);
    public async Task SendPrivateMessageAsync(string receiverId, string content);
    public async Task SendImageAsync(string groupId, byte[] imageData, string fileName);
    public async Task SendFileAsync(string groupId, string filePath);

    // Events
    public event EventHandler<ChatMessageReceivedEventArgs> OnMessageReceived;
    public event EventHandler<ChatGroupEventArgs> OnGroupCreated;
}
```

#### C. Views
**Files:**
- `Views/ChatView.xaml` (Tạo mới - thay thế ChatWindow)
- `Views/ChatView.xaml.cs` (Tạo mới)
- `Views/CreateChatGroupDialog.xaml` (Tạo mới)
- `Views/CreateChatGroupDialog.xaml.cs` (Tạo mới)

**Giao diện ChatView:**
```
┌──────────────────────────────────────────────────────────────┐
│  💬 Chat                                              ─ □ × │
├─────────────────┬────────────────────────────────────────────┤
│ NHÓM CHAT       │  ← Lớp 10A1                    25 online  │
│                 ├────────────────────────────────────────────┤
│ ● Lớp 10A1  (3) │                                            │
│ ○ Nhóm Toán     │  [Tin nhắn chat ở đây]                     │
│ ○ Nhóm Văn      │                                            │
│                 │                                            │
│ ───────────────│                                            │
│ CHAT RIÊNG      │                                            │
│                 │                                            │
│ ● Nguyễn Văn A  │                                            │
│ ○ Trần Thị B    │                                            │
│                 ├────────────────────────────────────────────┤
│ [+ Tạo nhóm]    │ [📷][📎] [Nhập tin nhắn...        ] [➤]   │
└─────────────────┴────────────────────────────────────────────┘
```

### Verification Commands
```bash
# Build kiểm tra
dotnet build

# Chạy unit tests
dotnet test

# Chạy ứng dụng
dotnet run
```

---

## Tính năng 2: Kiểm tra Thông tin Máy tính Học sinh

### Mô tả Chi tiết
- Xem thông tin ổ đĩa (C:, D:, E:...) của máy học sinh
- Xem danh sách USB đang kết nối
- Hiển thị dung lượng trống/đã dùng
- Thông tin cập nhật theo thời gian thực

### Thay đổi Cần thực hiện

#### A. Models
**Files:**
- `Models/SystemInfoModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**SystemInfoModels.cs:**
```csharp
public class DriveInfo
{
    public string Name { get; set; }
    public string DriveType { get; set; }
    public long TotalSize { get; set; }
    public long FreeSpace { get; set; }
    public string FileSystem { get; set; }
}

public class UsbDeviceInfo
{
    public string DeviceId { get; set; }
    public string Name { get; set; }
    public string DriveLabel { get; set; }
    public long TotalSize { get; set; }
    public long FreeSpace { get; set; }
    public DateTime ConnectedAt { get; set; }
}

public class SystemInfoPackage
{
    public string ClientId { get; set; }
    public List<DriveInfo> Drives { get; set; }
    public List<UsbDeviceInfo> UsbDevices { get; set; }
    public DateTime Timestamp { get; set; }
}
```

**NetworkModels.cs - Thêm MessageType:**
```csharp
// System Info
SystemInfoRequest = 0x80,
SystemInfoResponse = 0x81,
```

#### B. Services
**Files:**
- `Services/SystemInfoService.cs` (Tạo mới - chạy ở Client)
- `Services/NetworkClientService.cs` (Cập nhật)
- `Services/NetworkServerService.cs` (Cập nhật)
- `Services/SessionManager.cs` (Cập nhật)

**SystemInfoService.cs (Student side):**
```csharp
public class SystemInfoService
{
    public static SystemInfoPackage CollectSystemInfo();
    public static List<DriveInfo> GetDriveInfo();
    public static List<UsbDeviceInfo> GetUsbDevices();
}
```

#### C. Views
**Files:**
- `Views/StudentInfoWindow.xaml` (Tạo mới)
- `Views/StudentInfoWindow.xaml.cs` (Tạo mới)
- `Views/MainTeacherWindow.xaml` (Cập nhật - thêm context menu)

**Giao diện:**
```
┌──────────────────────────────────────────────────────────────┐
│  📊 Thông tin Máy tính - Nguyễn Văn A                   × │
├──────────────────────────────────────────────────────────────┤
│  ỔN ĐĨA                                                      │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ 💿 C: (Windows)     120GB/250GB    [████████░░] 48%    │ │
│  │ 💿 D: (Data)        45GB/500GB     [██░░░░░░░░] 9%     │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  USB ĐANG KẾT NỐI                                           │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ 🔌 KINGSTON (E:)    8GB/16GB       [████░░░░░] 50%     │ │
│  │ 🔌 SANDISK (F:)     2GB/32GB       [█░░░░░░░░] 6%      │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  Cập nhật lần cuối: 10:30:45                   [🔄 Refresh] │
└──────────────────────────────────────────────────────────────┘
```

---

## Tính năng 3: Quản lý Ứng dụng Đang Chạy

### Mô tả Chi tiết
- Xem danh sách tất cả ứng dụng/process đang chạy trên máy học sinh
- Hiển thị thông tin: Tên, PID, Memory usage
- Đóng ứng dụng bất kỳ từ xa
- Cảnh báo khi đóng ứng dụng quan trọng

### Thay đổi Cần thực hiện

#### A. Models
**Files:**
- `Models/ProcessModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**ProcessModels.cs:**
```csharp
public class ProcessInfo
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; }
    public string WindowTitle { get; set; }
    public long MemoryUsage { get; set; }
    public string ExecutablePath { get; set; }
    public DateTime StartTime { get; set; }
}

public class ProcessListPackage
{
    public string ClientId { get; set; }
    public List<ProcessInfo> Processes { get; set; }
    public DateTime Timestamp { get; set; }
}

public class KillProcessCommand
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; }
}
```

**NetworkModels.cs - Thêm MessageType:**
```csharp
// Process management
ProcessListRequest = 0x82,
ProcessListResponse = 0x83,
ProcessKillCommand = 0x84,
ProcessKillResult = 0x85,
```

#### B. Services
**Files:**
- `Services/ProcessManagerService.cs` (Tạo mới - Student side)
- `Services/NetworkClientService.cs` (Cập nhật)
- `Services/NetworkServerService.cs` (Cập nhật)

**ProcessManagerService.cs:**
```csharp
public class ProcessManagerService
{
    public static List<ProcessInfo> GetRunningProcesses();
    public static bool KillProcess(int processId);
}
```

#### C. Views
**Files:**
- `Views/ProcessManagerWindow.xaml` (Tạo mới)
- `Views/ProcessManagerWindow.xaml.cs` (Tạo mới)

**Giao diện:**
```
┌──────────────────────────────────────────────────────────────┐
│  📱 Ứng dụng đang chạy - Nguyễn Văn A                   × │
├──────────────────────────────────────────────────────────────┤
│  [🔍 Tìm kiếm...]                          [🔄 Refresh]    │
├──────────────────────────────────────────────────────────────┤
│  Tên ứng dụng          │ PID   │ Memory  │ Thao tác         │
│  ──────────────────────┼───────┼─────────┼─────────────────  │
│  🎮 Minecraft.exe      │ 1234  │ 2.5GB   │ [Đóng]           │
│  🌐 chrome.exe         │ 5678  │ 500MB   │ [Đóng]           │
│  📝 notepad.exe        │ 9012  │ 10MB    │ [Đóng]           │
│  ⚙️ explorer.exe       │ 3456  │ 80MB    │ [🔒 Hệ thống]    │
└──────────────────────────────────────────────────────────────┘
```

---

## Tính năng 4: Thu thập File từ Thư mục Chỉ định

### Mô tả Chi tiết
- Giáo viên chỉ định đường dẫn thư mục trên máy học sinh
- Thu thập tất cả file trong thư mục đó
- Hỗ trợ thu thập đệ quy (subfolder)
- Hiển thị tiến trình thu thập

### Thay đổi Cần thực hiện

#### A. Models
**Files:**
- `Models/FileCollectionModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**FileCollectionModels.cs:**
```csharp
public class FileCollectionRequest
{
    public string RequestId { get; set; }
    public string FolderPath { get; set; }
    public bool IncludeSubfolders { get; set; }
    public string[] FileExtensions { get; set; } // null = all files
}

public class FileCollectionProgress
{
    public string RequestId { get; set; }
    public string ClientId { get; set; }
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public string CurrentFile { get; set; }
}
```

**NetworkModels.cs - Thêm MessageType:**
```csharp
// File collection
FileCollectionRequest = 0x44,
FileCollectionProgress = 0x45,
FileCollectionComplete = 0x46,
```

#### B. Services
**Files:**
- `Services/FileCollectionService.cs` (Tạo mới)
- `Services/NetworkClientService.cs` (Cập nhật)
- `Services/NetworkServerService.cs` (Cập nhật)

#### C. Views
**Files:**
- `Views/FileCollectionWindow.xaml` (Tạo mới)
- `Views/FileCollectionWindow.xaml.cs` (Tạo mới)

**Giao diện:**
```
┌──────────────────────────────────────────────────────────────┐
│  📂 Thu thập File từ Học sinh                          ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│  Thư mục cần thu: [C:\Users\Student\Documents        ] [📁] │
│  ☑ Bao gồm thư mục con                                      │
│  Loại file: [*.docx, *.pdf, *.pptx                   ] [?]  │
├──────────────────────────────────────────────────────────────┤
│  CHỌN HỌC SINH                                              │
│  ☑ Tất cả   ☑ Nguyễn Văn A   ☑ Trần Thị B   ☐ Lê C         │
├──────────────────────────────────────────────────────────────┤
│  TIẾN TRÌNH                                                  │
│  Nguyễn Văn A: [██████████░░░░] 15/20 files  BaiTap3.docx   │
│  Trần Thị B:   [████████████░░] 12/12 files  ✓ Hoàn thành   │
├──────────────────────────────────────────────────────────────┤
│                         [Bắt đầu Thu thập]    [Đóng]        │
└──────────────────────────────────────────────────────────────┘
```

---

## Tính năng 5: Nộp Bài tập (Upload từ Học sinh)

### Mô tả Chi tiết
- Học sinh bấm nút "Nộp bài tập" để upload file
- File được lưu vào thư mục cố định ở máy giáo viên
- Tổ chức file theo: Phiên học > Tên học sinh > Thời gian nộp
- Thông báo cho giáo viên khi có bài tập mới

### Thay đổi Cần thực hiện

#### A. Models
**Files:**
- `Models/AssignmentModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**AssignmentModels.cs:**
```csharp
public class AssignmentSubmission
{
    public string Id { get; set; }
    public string StudentId { get; set; }
    public string StudentName { get; set; }
    public string SessionId { get; set; }
    public List<SubmittedFile> Files { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string Note { get; set; }
}

public class SubmittedFile
{
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string LocalPath { get; set; } // Path on teacher's machine
}
```

**NetworkModels.cs - Thêm MessageType:**
```csharp
// Assignment submission
AssignmentSubmit = 0x90,
AssignmentSubmitAck = 0x91,
AssignmentList = 0x92,
```

#### B. Services
**Files:**
- `Services/AssignmentService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Cập nhật - thêm bảng Assignments)

#### C. Views
**Files:**
- `Views/StudentWindow.xaml` (Cập nhật - thêm nút Nộp bài)
- `Views/SubmitAssignmentDialog.xaml` (Tạo mới)
- `Views/SubmitAssignmentDialog.xaml.cs` (Tạo mới)
- `Views/AssignmentListWindow.xaml` (Tạo mới - Teacher side)
- `Views/AssignmentListWindow.xaml.cs` (Tạo mới)

**Giao diện Học sinh:**
```
┌──────────────────────────────────────────────────────────────┐
│  📤 Nộp Bài tập                                         × │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Kéo thả file vào đây hoặc [Chọn file...]                   │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                                                        │ │
│  │           📄 BaiTap_Toan.docx (2.5MB)  [❌]            │ │
│  │           📄 Hinh_minh_hoa.png (500KB) [❌]            │ │
│  │                                                        │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  Ghi chú: [Em nộp muộn vì...                           ]   │
│                                                              │
│                                    [Nộp bài]    [Hủy]       │
└──────────────────────────────────────────────────────────────┘
```

**Giao diện Giáo viên xem bài nộp:**
```
┌──────────────────────────────────────────────────────────────┐
│  📋 Danh sách Bài tập Đã nộp                           ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│  Phiên: [Buổi sáng 21/01/2026 ▼]     Đã nộp: 25/30 học sinh │
├──────────────────────────────────────────────────────────────┤
│  Học sinh        │ Thời gian    │ File           │ Thao tác │
│  ────────────────┼──────────────┼────────────────┼───────── │
│  Nguyễn Văn A   │ 10:30:15     │ 2 files (3MB)  │ [📂][📥] │
│  Trần Thị B     │ 10:31:22     │ 1 file (1MB)   │ [📂][📥] │
│  Lê Hoàng C     │ ❌ Chưa nộp  │ -              │ [🔔]     │
├──────────────────────────────────────────────────────────────┤
│              [📥 Tải tất cả]    [📂 Mở thư mục]   [Đóng]   │
└──────────────────────────────────────────────────────────────┘
```

---

## Tính năng 6: Gửi File Hàng loạt

### Mô tả Chi tiết
- Giáo viên chọn file để gửi đến tất cả hoặc một số học sinh
- Học sinh nhận thông báo popup
- Học sinh bấm vào để lưu file vào máy
- Hiển thị tiến trình gửi/nhận

### Thay đổi Cần thực hiện

#### A. Models
**Files:**
- `Models/BulkFileModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**BulkFileModels.cs:**
```csharp
public class BulkFileSend
{
    public string TransferId { get; set; }
    public List<string> FileIds { get; set; }
    public List<string> TargetStudentIds { get; set; }
    public string Message { get; set; }
}

public class FileNotification
{
    public string TransferId { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public string SenderName { get; set; }
    public string Message { get; set; }
}

public class FileDownloadRequest
{
    public string TransferId { get; set; }
    public string FileId { get; set; }
    public string SavePath { get; set; }
}
```

**NetworkModels.cs - Thêm MessageType:**
```csharp
// Bulk file transfer
BulkFileSend = 0xA0,
BulkFileNotification = 0xA1,
BulkFileDownload = 0xA2,
BulkFileProgress = 0xA3,
```

#### B. Services
**Files:**
- `Services/BulkFileService.cs` (Tạo mới)
- `Services/NetworkClientService.cs` (Cập nhật)
- `Services/NetworkServerService.cs` (Cập nhật)

#### C. Views
**Files:**
- `Views/BulkFileSendWindow.xaml` (Tạo mới - Teacher)
- `Views/BulkFileSendWindow.xaml.cs` (Tạo mới)
- `Views/FileNotificationPopup.xaml` (Tạo mới - Student)
- `Views/FileNotificationPopup.xaml.cs` (Tạo mới)

**Giao diện Giáo viên:**
```
┌──────────────────────────────────────────────────────────────┐
│  📤 Gửi File Hàng loạt                                  ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│  FILE CẦN GỬI                                               │
│  [+ Thêm file...]    [Kéo thả file vào đây]                 │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ 📄 TaiLieu_Chuong1.pdf      (5.2MB)   [❌]             │ │
│  │ 📄 BaiTap_Mau.docx          (1.1MB)   [❌]             │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  CHỌN HỌC SINH NHẬN                                         │
│  ☑ Tất cả (30 học sinh)                                     │
│  ☐ Chọn từng học sinh...                                    │
│                                                              │
│  Tin nhắn kèm theo: [Tài liệu ôn tập cho bài kiểm tra  ]   │
│                                                              │
│  ────────────────────────────────────────────────────────── │
│  TIẾN TRÌNH GỬI                                             │
│  [████████████░░░░░░░░] 60%  -  18/30 học sinh đã nhận      │
│                                                              │
│                                    [Gửi file]    [Đóng]     │
└──────────────────────────────────────────────────────────────┘
```

**Popup Học sinh nhận file:**
```
┌──────────────────────────────────────────────┐
│  📥 Có file mới từ Giáo viên              × │
├──────────────────────────────────────────────┤
│                                              │
│  📄 TaiLieu_Chuong1.pdf (5.2MB)             │
│  📄 BaiTap_Mau.docx (1.1MB)                 │
│                                              │
│  "Tài liệu ôn tập cho bài kiểm tra"         │
│                                              │
│       [💾 Lưu về máy]    [Bỏ qua]           │
└──────────────────────────────────────────────┘
```

---

## Tính năng 7: Bình chọn Thời gian Thực (Polling)

### Mô tả Chi tiết
- Giáo viên tạo cuộc bình chọn với câu hỏi và các lựa chọn
- Học sinh vote đáp án theo thời gian thực
- Kết quả hiển thị realtime với biểu đồ
- Có thể ẩn/hiện kết quả cho học sinh
- Hỗ trợ nhiều loại poll: Single choice, Multiple choice

### Thay đổi Cần thực hiện

#### A. Models
**Files:**
- `Models/PollModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**PollModels.cs:**
```csharp
public class Poll
{
    public string Id { get; set; }
    public string Question { get; set; }
    public List<PollOption> Options { get; set; }
    public PollType Type { get; set; }
    public bool ShowResultsToStudents { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class PollOption
{
    public string Id { get; set; }
    public string Text { get; set; }
    public int VoteCount { get; set; }
}

public enum PollType
{
    SingleChoice,
    MultipleChoice
}

public class PollVote
{
    public string PollId { get; set; }
    public string StudentId { get; set; }
    public List<string> SelectedOptionIds { get; set; }
    public DateTime VotedAt { get; set; }
}

public class PollResult
{
    public string PollId { get; set; }
    public int TotalVotes { get; set; }
    public Dictionary<string, int> OptionVotes { get; set; }
}
```

**NetworkModels.cs - Thêm MessageType:**
```csharp
// Polling
PollCreate = 0xB0,
PollStart = 0xB1,
PollVote = 0xB2,
PollResult = 0xB3,
PollClose = 0xB4,
PollUpdate = 0xB5,
```

#### B. Services
**Files:**
- `Services/PollService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Cập nhật - thêm bảng Polls)
- `Services/NetworkClientService.cs` (Cập nhật)
- `Services/NetworkServerService.cs` (Cập nhật)

**PollService.cs:**
```csharp
public class PollService
{
    // Singleton
    public static PollService Instance { get; }

    // Teacher actions
    public async Task<Poll> CreatePollAsync(string question, List<string> options, PollType type);
    public async Task StartPollAsync(string pollId);
    public async Task ClosePollAsync(string pollId);
    public async Task ToggleResultVisibilityAsync(string pollId, bool show);

    // Student actions
    public async Task VoteAsync(string pollId, List<string> optionIds);

    // Events (Real-time updates)
    public event EventHandler<PollResultEventArgs> OnResultUpdated;
    public event EventHandler<PollEventArgs> OnPollStarted;
    public event EventHandler<PollEventArgs> OnPollClosed;
}
```

#### C. Views
**Files:**
- `Views/CreatePollWindow.xaml` (Tạo mới - Teacher)
- `Views/CreatePollWindow.xaml.cs` (Tạo mới)
- `Views/PollResultWindow.xaml` (Tạo mới - Teacher, realtime)
- `Views/PollResultWindow.xaml.cs` (Tạo mới)
- `Views/VotePollWindow.xaml` (Tạo mới - Student)
- `Views/VotePollWindow.xaml.cs` (Tạo mới)

**Giao diện Tạo Poll (Giáo viên):**
```
┌──────────────────────────────────────────────────────────────┐
│  📊 Tạo Bình chọn Mới                                   ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Câu hỏi:                                                   │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Các em đã hiểu bài hôm nay chưa?                      │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  Loại: ◉ Chọn một   ○ Chọn nhiều                           │
│                                                              │
│  CÁC LỰA CHỌN:                                              │
│  [A] [Hiểu rõ                                        ] [❌] │
│  [B] [Hiểu một phần                                  ] [❌] │
│  [C] [Chưa hiểu                                      ] [❌] │
│  [D] [Cần giải thích thêm                            ] [❌] │
│                                       [+ Thêm lựa chọn]     │
│                                                              │
│  ☑ Hiển thị kết quả cho học sinh                           │
│                                                              │
│                     [Tạo và Bắt đầu]    [Hủy]               │
└──────────────────────────────────────────────────────────────┘
```

**Giao diện Kết quả Realtime (Giáo viên):**
```
┌──────────────────────────────────────────────────────────────┐
│  📊 Kết quả Bình chọn                              🔴 LIVE  │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Các em đã hiểu bài hôm nay chưa?                           │
│                                                              │
│  A. Hiểu rõ                                                  │
│     [████████████████████░░░░░░░░░░] 55% (11 votes)         │
│                                                              │
│  B. Hiểu một phần                                            │
│     [████████░░░░░░░░░░░░░░░░░░░░░░] 25% (5 votes)          │
│                                                              │
│  C. Chưa hiểu                                                │
│     [████░░░░░░░░░░░░░░░░░░░░░░░░░░] 10% (2 votes)          │
│                                                              │
│  D. Cần giải thích thêm                                      │
│     [███░░░░░░░░░░░░░░░░░░░░░░░░░░░] 10% (2 votes)          │
│                                                              │
│  ─────────────────────────────────────────────────────────  │
│  Đã vote: 20/30 học sinh                                    │
│                                                              │
│  [👁 Ẩn kết quả với HS]    [⏹ Kết thúc]    [📊 Xuất kết quả]│
└──────────────────────────────────────────────────────────────┘
```

**Giao diện Vote (Học sinh):**
```
┌──────────────────────────────────────────────────────────────┐
│  📊 Bình chọn từ Giáo viên                              × │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Các em đã hiểu bài hôm nay chưa?                           │
│                                                              │
│  ○ A. Hiểu rõ                                               │
│  ● B. Hiểu một phần                                         │
│  ○ C. Chưa hiểu                                             │
│  ○ D. Cần giải thích thêm                                   │
│                                                              │
│                              [✓ Gửi Phiếu bầu]              │
└──────────────────────────────────────────────────────────────┘
```

---

## Database Schema Updates

### Các bảng mới cần thêm vào `DatabaseService.cs`:

```sql
-- Chat Groups
CREATE TABLE ChatGroups (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    creator_id TEXT NOT NULL,
    session_id INTEGER,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);

CREATE TABLE ChatGroupMembers (
    group_id TEXT,
    member_id TEXT,
    joined_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (group_id, member_id)
);

CREATE TABLE ChatAttachments (
    id TEXT PRIMARY KEY,
    message_id INTEGER,
    file_name TEXT,
    file_path TEXT,
    file_size INTEGER,
    content_type TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (message_id) REFERENCES ChatMessages(id)
);

-- Assignments
CREATE TABLE Assignments (
    id TEXT PRIMARY KEY,
    session_id INTEGER,
    student_id TEXT NOT NULL,
    student_name TEXT,
    note TEXT,
    submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);

CREATE TABLE AssignmentFiles (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    assignment_id TEXT,
    file_name TEXT,
    file_path TEXT,
    file_size INTEGER,
    FOREIGN KEY (assignment_id) REFERENCES Assignments(id)
);

-- Polls
CREATE TABLE Polls (
    id TEXT PRIMARY KEY,
    session_id INTEGER,
    question TEXT NOT NULL,
    poll_type TEXT DEFAULT 'single',
    show_results INTEGER DEFAULT 1,
    is_active INTEGER DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    closed_at DATETIME,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);

CREATE TABLE PollOptions (
    id TEXT PRIMARY KEY,
    poll_id TEXT,
    option_text TEXT NOT NULL,
    vote_count INTEGER DEFAULT 0,
    FOREIGN KEY (poll_id) REFERENCES Polls(id)
);

CREATE TABLE PollVotes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    poll_id TEXT,
    student_id TEXT,
    option_id TEXT,
    voted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (poll_id) REFERENCES Polls(id)
);
```

---

## Thứ tự Phát triển Đề xuất

### Phase 1: Core Features (1-2 tuần)
1. **Tính năng 5: Nộp Bài tập** - Tính năng cơ bản nhất, ít phụ thuộc
2. **Tính năng 7: Bình chọn** - Độc lập, có thể phát triển song song

### Phase 2: Communication Enhancement (1 tuần)
3. **Tính năng 1: Chat Nâng cao** - Mở rộng từ ChatWindow hiện có
4. **Tính năng 6: Gửi File Hàng loạt** - Liên quan đến file transfer

### Phase 3: Monitoring & Control (1 tuần)
5. **Tính năng 2: Kiểm tra Thông tin Máy tính**
6. **Tính năng 3: Quản lý Ứng dụng**
7. **Tính năng 4: Thu thập File**

---

## Lưu ý Kỹ thuật

### Bảo mật
- Validate tất cả input từ network
- Không cho phép truy cập file system ngoài thư mục được phép
- Xác thực người dùng trước mọi thao tác quan trọng

### Performance
- Sử dụng async/await cho tất cả network operations
- Chunk file khi transfer file lớn
- Cache kết quả system info để giảm tải

### UX
- Hiển thị loading indicator cho mọi operation
- Thông báo lỗi rõ ràng và hữu ích
- Confirm dialog trước các action quan trọng (đóng app, xóa file)

---

## Verification Checklist cho Mỗi Tính năng

- [ ] Unit tests cho Services
- [ ] Integration tests cho Network communication
- [ ] Manual testing với nhiều clients
- [ ] Build thành công
- [ ] Documentation cập nhật
- [ ] UI responsive và hoạt động tốt

---

_Tài liệu workflow - Phiên bản 1.0.0 | Ngày tạo: 21/01/2026_
