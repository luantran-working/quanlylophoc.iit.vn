---
description: Workflow phát triển tính năng Tắt/Khởi động lại máy tính (Feature 10) - Giáo viên tắt hoặc restart máy tính học sinh từ xa.
---

# Phát triển Tính năng Tắt/Khởi động lại Máy tính

## Tổng quan

- Giáo viên có thể tắt máy tính học sinh từ xa
- Giáo viên có thể khởi động lại máy tính học sinh
- Hỗ trợ tắt/restart một hoặc nhiều máy cùng lúc
- Có countdown và cảnh báo trước khi thực hiện

## Các bước thực hiện

### 1. Cập nhật Models

**Files:**

- `Models/PowerModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Thêm MessageType)

**PowerModels.cs:**

```csharp
public enum PowerAction { Shutdown, Restart, LogOff, Sleep, CancelPending }

public class PowerCommand
{
    public PowerAction Action { get; set; }
    public int DelaySeconds { get; set; } = 60;
    public string Message { get; set; } = "";
    public bool Force { get; set; } = false;
    public List<string> TargetStudentIds { get; set; } = new();
}
```

**NetworkModels.cs - Thêm:**

```csharp
PowerShutdown = 0x70, PowerRestart = 0x71, PowerCancel = 0x74
```

### 2. Implement Services

**Files:**

- `Services/PowerControlService.cs` (Server)
- `Services/PowerExecutionService.cs` (Client)
- `Services/DatabaseService.cs` (Thêm bảng PowerLogs)

### 3. Implement Views

**Files:**

- `Views/PowerControlDialog.xaml` & `.cs` (Dialog cho giáo viên)
- `Views/PowerWarningWindow.xaml` & `.cs` (Cảnh báo cho học sinh)

### 4. Database Schema

```sql
CREATE TABLE PowerLogs (
    id TEXT PRIMARY KEY,
    student_id TEXT NOT NULL,
    action TEXT NOT NULL,
    command_time DATETIME NOT NULL,
    executed INTEGER DEFAULT 0
);
```

## Verification

- [ ] Giáo viên gửi lệnh shutdown/restart
- [ ] Học sinh nhận cảnh báo với countdown
- [ ] Sau countdown, máy thực hiện action
