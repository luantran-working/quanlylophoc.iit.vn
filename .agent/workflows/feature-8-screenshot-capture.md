---
description: Workflow phát triển tính năng Chụp màn hình (Feature 8) - Chụp màn hình học sinh, lưu và xem lại danh sách ảnh đã chụp.
---

# Phát triển Tính năng Chụp màn hình

## Tổng quan

- Giáo viên có thể chụp màn hình học sinh bất kỳ lúc nào
- Ảnh chụp được lưu vào thư mục theo phiên học và tên học sinh
- Xem lại danh sách ảnh đã chụp với gallery view
- Hỗ trợ chụp đồng loạt nhiều học sinh
- Đánh dấu và annotation trên ảnh đã chụp

## Các bước thực hiện

### 1. Cập nhật Backend Models

**Files:**

- `Models/ScreenshotModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật thêm MessageType)

**Nội dung:**

```csharp
// ScreenshotModels.cs
public class Screenshot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string SessionId { get; set; } = "";
    public DateTime CapturedAt { get; set; } = DateTime.Now;
    public string FilePath { get; set; } = "";
    public string ThumbnailPath { get; set; } = "";
    public string? Note { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class ScreenshotRequest
{
    public string TargetStudentId { get; set; } = "";
    public bool SaveToLocal { get; set; } = true;
}

public class ScreenshotResponse
{
    public bool Success { get; set; }
    public string ScreenshotId { get; set; } = "";
    public string Message { get; set; } = "";
}
```

**NetworkModels.cs - Thêm MessageType:**

```csharp
ScreenshotRequest = 0x60,   // Yêu cầu chụp màn hình
ScreenshotData = 0x61,      // Dữ liệu ảnh chụp
ScreenshotConfirm = 0x62,   // Xác nhận đã nhận
```

### 2. Implement Services

**Files:**

- `Services/ScreenshotService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Thêm bảng Screenshots)
- `Services/SessionManager.cs` (Tích hợp ScreenshotService)
- `Services/NetworkServerService.cs` & `NetworkClientService.cs` (Xử lý message mới)

**Logic chính:**

```csharp
// ScreenshotService.cs
public class ScreenshotService
{
    private readonly DatabaseService _database;
    private readonly string _screenshotFolder;

    public async Task<Screenshot> CaptureAndSaveAsync(string studentId, string studentName, byte[] imageData)
    {
        // 1. Tạo thư mục theo ngày/session
        // 2. Lưu ảnh gốc và thumbnail
        // 3. Lưu metadata vào DB
        // 4. Return Screenshot object
    }

    public async Task<List<Screenshot>> GetScreenshotsAsync(string? sessionId = null, string? studentId = null)
    {
        // Lấy danh sách ảnh theo filter
    }

    public async Task<bool> DeleteScreenshotAsync(string screenshotId)
    {
        // Xóa ảnh và metadata
    }

    public async Task<bool> AddNoteAsync(string screenshotId, string note)
    {
        // Thêm ghi chú cho ảnh
    }
}
```

### 3. Implement Views

**Files:**

- `Views/ScreenshotGalleryWindow.xaml` & `.cs` (Tạo mới - Gallery xem ảnh)
- `Views/ScreenshotViewerWindow.xaml` & `.cs` (Tạo mới - Xem chi tiết ảnh)
- `Views/MainTeacherWindow.xaml.cs` (Cập nhật - thêm button chụp và mở gallery)
- `Controls/ScreenThumbnailControl.xaml` (Cập nhật - thêm context menu chụp màn hình)

**UI Design - ScreenshotGalleryWindow:**

```
┌──────────────────────────────────────────────────────────────┐
│  📸 Thư viện Ảnh chụp màn hình               [🔍 Tìm kiếm]  │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Filter: [Tất cả ▼] [Hôm nay ▼] [Học sinh: Tất cả ▼]        │
│                                                              │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐            │
│  │ 🖼️      │ │ 🖼️      │ │ 🖼️      │ │ 🖼️      │            │
│  │ Thumb   │ │ Thumb   │ │ Thumb   │ │ Thumb   │            │
│  ├─────────┤ ├─────────┤ ├─────────┤ ├─────────┤            │
│  │Nguyễn A │ │Trần B   │ │Lê C     │ │Phạm D   │            │
│  │10:30 AM │ │10:35 AM │ │10:40 AM │ │10:45 AM │            │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘            │
│                                                              │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐            │
│  │ 🖼️      │ │ 🖼️      │ │ 🖼️      │ │ 🖼️      │            │
│  │ Thumb   │ │ Thumb   │ │ Thumb   │ │ Thumb   │            │
│  └─────────┘ └─────────┘ └─────────┘ └─────────┘            │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  Tổng: 24 ảnh │ Đã chọn: 2 │  [🗑️ Xóa] [💾 Xuất] [📧 Gửi]  │
└──────────────────────────────────────────────────────────────┘
```

**UI Design - ScreenshotViewerWindow:**

```
┌──────────────────────────────────────────────────────────────┐
│  Ảnh chụp - Nguyễn Văn An - 10:30:45 AM     [◀] [▶] [✕]     │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │                                                        │ │
│  │                                                        │ │
│  │                    (Full size image)                   │ │
│  │                                                        │ │
│  │                                                        │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  Ghi chú: [                                              ]  │
│  Tags: [Quan trọng] [+]                                     │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  [🔍 Zoom] [🔄 Xoay] [✏️ Annotation] [💾 Lưu] [🗑️ Xóa]     │
└──────────────────────────────────────────────────────────────┘
```

### 4. Database Schema

```sql
CREATE TABLE Screenshots (
    id TEXT PRIMARY KEY,
    student_id TEXT NOT NULL,
    student_name TEXT NOT NULL,
    session_id INTEGER,
    captured_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    file_path TEXT NOT NULL,
    thumbnail_path TEXT,
    note TEXT,
    tags TEXT, -- JSON array
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);

CREATE INDEX idx_screenshots_session ON Screenshots(session_id);
CREATE INDEX idx_screenshots_student ON Screenshots(student_id);
CREATE INDEX idx_screenshots_date ON Screenshots(captured_at);
```

### 5. Cập nhật ScreenThumbnailControl

**Thêm Context Menu:**

```xml
<ContextMenu>
    <MenuItem Header="📸 Chụp màn hình" Click="CaptureScreenshot_Click"/>
    <Separator/>
    <MenuItem Header="🖥️ Xem chi tiết" Click="ViewFullScreen_Click"/>
    <MenuItem Header="🎮 Điều khiển từ xa" Click="RemoteControl_Click"/>
</ContextMenu>
```

### 6. Luồng xử lý

```
GIÁO VIÊN                          HỌC SINH
    │                                  │
    │ 1. Click "Chụp màn hình"         │
    │ ─────────────────────────────►   │
    │     (ScreenshotRequest)          │
    │                                  │
    │                                  │ 2. Capture screen
    │                                  │    Encode to JPEG
    │                                  │
    │     3. Gửi dữ liệu ảnh           │
    │ ◄─────────────────────────────   │
    │     (ScreenshotData)             │
    │                                  │
    │ 4. Lưu ảnh + metadata            │
    │ 5. Hiển thị thông báo            │
    │                                  │
```

### 7. Tích hợp vào MainTeacherWindow

**Thêm vào sidebar hoặc toolbar:**

- Button "📸 Chụp tất cả" - Chụp màn hình tất cả học sinh
- Button "🖼️ Thư viện ảnh" - Mở ScreenshotGalleryWindow

**Thêm vào context menu của thumbnail:**

- "Chụp màn hình" - Chụp màn hình học sinh được chọn

## Verification

- [ ] Mở MainTeacherWindow, kiểm tra button chụp màn hình
- [ ] Chụp màn hình 1 học sinh, kiểm tra ảnh được lưu
- [ ] Chụp đồng loạt nhiều học sinh
- [ ] Mở gallery xem danh sách ảnh
- [ ] Xem chi tiết, thêm ghi chú, xóa ảnh
- [ ] Filter theo ngày/học sinh hoạt động đúng
- [ ] Xuất/download ảnh thành công

## Dependencies

- Sử dụng `ScreenCaptureService` hiện có để capture trên client
- Sử dụng `SessionManager` để gửi/nhận message
- Sử dụng `DatabaseService` để lưu metadata

## Notes

- Ảnh nên được nén để tiết kiệm dung lượng (JPEG quality 80%)
- Thumbnail size: 200x150 pixels
- Tạo thư mục theo pattern: `Screenshots/{SessionId}/{StudentName}/`
- Tên file: `{StudentName}_{Timestamp}.jpg`
