---
description: Workflow phát triển tính năng Ghi màn hình (Feature 9) - Giáo viên và học sinh ghi lại màn hình học tập lưu vào máy tính.
---

# Phát triển Tính năng Ghi màn hình (Screen Recording)

## Tổng quan

- Giáo viên có thể ghi lại màn hình của mình trong khi giảng bài
- Học sinh có thể ghi lại màn hình học tập của mình
- Video được lưu trực tiếp vào máy tính local
- Hỗ trợ ghi âm thanh (microphone + system audio)
- Xem lại và quản lý các bản ghi

## Các bước thực hiện

### 1. Cập nhật Backend Models

**Files:**

- `Models/RecordingModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật thêm MessageType nếu cần remote control recording)

**Nội dung:**

```csharp
// RecordingModels.cs
public enum RecordingState
{
    Idle,
    Recording,
    Paused,
    Stopped
}

public enum RecordingSource
{
    FullScreen,
    Window,
    Region
}

public class RecordingSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public bool IsTeacher { get; set; }
    public RecordingSource Source { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : DateTime.Now - StartTime;
    public string FilePath { get; set; } = "";
    public long FileSize { get; set; }
    public RecordingState State { get; set; } = RecordingState.Idle;
    public bool IncludeAudio { get; set; } = true;
    public bool IncludeMicrophone { get; set; } = true;
}

public class RecordingSettings
{
    public string OutputFolder { get; set; } = "";
    public int FrameRate { get; set; } = 30;
    public int Quality { get; set; } = 80; // 1-100
    public string VideoCodec { get; set; } = "H264"; // H264, HEVC
    public string AudioCodec { get; set; } = "AAC";
    public bool IncludeSystemAudio { get; set; } = true;
    public bool IncludeMicrophone { get; set; } = true;
    public bool ShowCursor { get; set; } = true;
    public bool HighlightClicks { get; set; } = true;
}
```

### 2. Implement Services

**Files:**

- `Services/ScreenRecordingService.cs` (Tạo mới)
- `Services/AudioCaptureService.cs` (Tạo mới - optional nếu cần tách riêng)
- `Services/DatabaseService.cs` (Thêm bảng Recordings)

**Logic chính - ScreenRecordingService:**

```csharp
// Sử dụng Windows.Graphics.Capture API (Windows 10+) hoặc SharpDX
public class ScreenRecordingService : IDisposable
{
    private readonly RecordingSettings _settings;
    private RecordingState _state = RecordingState.Idle;
    private MediaFoundationVideoWriter? _videoWriter;
    private CancellationTokenSource? _cts;

    public event EventHandler<RecordingState>? StateChanged;
    public event EventHandler<TimeSpan>? DurationUpdated;
    public event EventHandler<Exception>? ErrorOccurred;

    public RecordingState State => _state;
    public TimeSpan CurrentDuration { get; private set; }

    public async Task<bool> StartRecordingAsync(RecordingSource source, string outputPath)
    {
        // 1. Validate settings and permissions
        // 2. Initialize screen capture
        // 3. Initialize audio capture (if enabled)
        // 4. Create video writer with codec settings
        // 5. Start capture loop
        // 6. Update state and fire events
    }

    public void PauseRecording()
    {
        // Pause capture loop
        _state = RecordingState.Paused;
        StateChanged?.Invoke(this, _state);
    }

    public void ResumeRecording()
    {
        // Resume capture loop
        _state = RecordingState.Recording;
        StateChanged?.Invoke(this, _state);
    }

    public async Task<string> StopRecordingAsync()
    {
        // 1. Stop capture loop
        // 2. Finalize video file
        // 3. Save metadata to database
        // 4. Return file path
    }

    public void Dispose()
    {
        // Cleanup resources
    }
}
```

**Dependencies cần thêm (NuGet):**

```xml
<PackageReference Include="SharpDX" Version="4.2.0" />
<PackageReference Include="SharpDX.Direct3D11" Version="4.2.0" />
<PackageReference Include="SharpDX.DXGI" Version="4.2.0" />
<PackageReference Include="NAudio" Version="2.2.1" />
<!-- Hoặc sử dụng -->
<PackageReference Include="ScreenRecorderLib" Version="4.3.0" />
```

### 3. Implement Views

**Files:**

- `Views/RecordingWindow.xaml` & `.cs` (Tạo mới - Cửa sổ ghi hình chính)
- `Views/RecordingListWindow.xaml` & `.cs` (Tạo mới - Danh sách bản ghi)
- `Views/RecordingSettingsDialog.xaml` & `.cs` (Tạo mới - Cài đặt ghi hình)
- `Views/MainTeacherWindow.xaml` (Cập nhật - thêm button ghi hình)
- `Views/StudentWindow.xaml` (Cập nhật - thêm button ghi hình cho học sinh)

**UI Design - RecordingWindow (Floating toolbar-style):**

```
┌─────────────────────────────────────────────────────┐
│  🔴 ĐANG GHI  │  ⏱️ 00:05:32  │  💾 Recording...   │
├─────────────────────────────────────────────────────┤
│                                                     │
│  [⏸️ Pause] [⏹️ Stop] [🔇 Mute] [⚙️ Settings]       │
│                                                     │
│  🎤 Microphone: ON    🔊 System Audio: ON          │
│  📹 Source: Full Screen                             │
│                                                     │
└─────────────────────────────────────────────────────┘
```

**UI Design - Trước khi ghi (Selection Dialog):**

```
┌──────────────────────────────────────────────────────────────┐
│  🎬 Bắt đầu Ghi màn hình                            [✕]     │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Chọn nguồn ghi:                                            │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐            │
│  │ 🖥️          │ │ 🪟          │ │ ▢            │            │
│  │ Toàn màn   │ │ Cửa sổ     │ │ Vùng chọn  │            │
│  │ hình        │ │             │ │             │            │
│  │ ○           │ │ ○           │ │ ○           │            │
│  └─────────────┘ └─────────────┘ └─────────────┘            │
│                                                              │
│  Tùy chọn âm thanh:                                         │
│  ☑ Ghi âm thanh hệ thống                                   │
│  ☑ Ghi microphone                                          │
│                                                              │
│  Tùy chọn khác:                                             │
│  ☑ Hiển thị con trỏ chuột                                  │
│  ☑ Highlight khi click                                      │
│                                                              │
│  Lưu vào: [C:\Users\...\Videos\Recordings      ] [📁]      │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│                            [Hủy] [🔴 Bắt đầu Ghi]           │
└──────────────────────────────────────────────────────────────┘
```

**UI Design - RecordingListWindow:**

```
┌──────────────────────────────────────────────────────────────┐
│  🎬 Danh sách Bản ghi                        [🔄 Refresh]   │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ 📹 Recording_20260122_103045.mp4                       │ │
│  │ ├── Thời gian: 22/01/2026 10:30:45                     │ │
│  │ ├── Thời lượng: 15:23                                  │ │
│  │ ├── Kích thước: 125.4 MB                               │ │
│  │ └── [▶️ Phát] [📁 Mở thư mục] [🗑️ Xóa]               │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ 📹 Recording_20260122_090515.mp4                       │ │
│  │ ├── Thời gian: 22/01/2026 09:05:15                     │ │
│  │ ├── Thời lượng: 45:10                                  │ │
│  │ ├── Kích thước: 320.8 MB                               │ │
│  │ └── [▶️ Phát] [📁 Mở thư mục] [🗑️ Xóa]               │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  Tổng: 5 bản ghi │ Dung lượng: 892.5 MB │ [📁 Mở thư mục]  │
└──────────────────────────────────────────────────────────────┘
```

### 4. Database Schema

```sql
CREATE TABLE Recordings (
    id TEXT PRIMARY KEY,
    user_id TEXT NOT NULL,
    user_name TEXT NOT NULL,
    is_teacher INTEGER NOT NULL DEFAULT 0,
    source TEXT NOT NULL, -- FullScreen, Window, Region
    start_time DATETIME NOT NULL,
    end_time DATETIME,
    duration_seconds INTEGER,
    file_path TEXT NOT NULL,
    file_size INTEGER,
    settings TEXT, -- JSON của RecordingSettings
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_recordings_user ON Recordings(user_id);
CREATE INDEX idx_recordings_date ON Recordings(start_time);
```

### 5. Tích hợp vào UI chính

**MainTeacherWindow - Thêm vào toolbar:**

```xml
<Button x:Name="RecordButton" Click="StartRecording_Click">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="🎬" FontSize="16"/>
        <TextBlock Text="Ghi màn hình" Margin="5,0,0,0"/>
    </StackPanel>
</Button>
<Button x:Name="RecordingListButton" Click="OpenRecordingList_Click">
    <TextBlock Text="📹 Xem bản ghi"/>
</Button>
```

**StudentWindow - Thêm vào sidebar:**

```xml
<Button x:Name="StudentRecordButton" Click="StartStudentRecording_Click">
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="🎬" FontSize="16"/>
        <TextBlock Text="Ghi màn hình học tập"/>
    </StackPanel>
</Button>
```

### 6. Luồng xử lý

```
┌────────────────────────────────────────────────────────────┐
│                    QUY TRÌNH GHI HÌNH                      │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  1. User click "Ghi màn hình"                             │
│                    │                                       │
│                    ▼                                       │
│  2. Hiển thị dialog chọn nguồn + cài đặt                  │
│                    │                                       │
│                    ▼                                       │
│  3. Click "Bắt đầu Ghi"                                   │
│     ├── Khởi tạo ScreenRecordingService                   │
│     ├── Capture screen frames                              │
│     ├── Capture audio streams                              │
│     └── Encode và ghi vào file                            │
│                    │                                       │
│                    ▼                                       │
│  4. Hiển thị floating toolbar với:                        │
│     ├── Thời gian ghi                                      │
│     ├── Nút Pause/Resume                                   │
│     └── Nút Stop                                           │
│                    │                                       │
│                    ▼                                       │
│  5. Click "Stop"                                          │
│     ├── Finalize video file                                │
│     ├── Lưu metadata vào DB                                │
│     └── Thông báo thành công                               │
│                                                            │
└────────────────────────────────────────────────────────────┘
```

### 7. Phím tắt (Keyboard Shortcuts)

| Phím tắt       | Chức năng              |
| -------------- | ---------------------- |
| `Ctrl+Shift+R` | Bắt đầu/Dừng ghi       |
| `Ctrl+Shift+P` | Pause/Resume           |
| `Ctrl+Shift+M` | Mute/Unmute microphone |

### 8. Xử lý đặc biệt

**Floating Window Always-on-Top:**

```csharp
// RecordingWindow.xaml.cs
public RecordingWindow()
{
    InitializeComponent();
    Topmost = true;
    WindowStyle = WindowStyle.ToolWindow;
    ResizeMode = ResizeMode.NoResize;
}
```

**Tự động đặt tên file:**

```csharp
private string GenerateFileName()
{
    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
    var userName = IsTeacher ? "Teacher" : _studentName;
    return $"Recording_{userName}_{timestamp}.mp4";
}
```

## Verification

- [ ] Giáo viên có thể mở dialog ghi hình
- [ ] Chọn nguồn (Full screen, Window, Region) hoạt động
- [ ] Bắt đầu ghi và floating toolbar hiển thị
- [ ] Pause/Resume hoạt động
- [ ] Stop và file được lưu thành công
- [ ] Audio (system + mic) được ghi
- [ ] Xem danh sách bản ghi
- [ ] Phát video đã ghi
- [ ] Học sinh cũng có thể ghi màn hình

## Dependencies

- Windows 10 version 1903+ (cho Windows.Graphics.Capture API)
- SharpDX hoặc ScreenRecorderLib cho screen capture
- NAudio cho audio capture
- H.264/HEVC codec cho video encoding

## Notes

- File video nên được lưu dưới dạng MP4 (H.264 + AAC)
- Mặc định lưu vào `Documents/IIT Recordings/`
- Cảnh báo nếu ổ đĩa còn ít dung lượng (< 1GB)
- Giới hạn thời gian ghi tối đa: 4 giờ
- Tự động dừng ghi khi logout hoặc đóng ứng dụng
