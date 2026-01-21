# Giám sát Hệ thống Học sinh

## Tổng quan

Tính năng cho phép giáo viên xem thông tin hệ thống của máy tính học sinh, bao gồm thông tin ổ đĩa, USB đang kết nối, và danh sách ứng dụng đang chạy.

## Các tính năng

### 1. Xem Thông tin Ổ đĩa

- Hiển thị tất cả ổ đĩa: C:, D:, E:...
- Thông tin chi tiết:
  - Loại ổ (SSD/HDD/Removable)
  - Dung lượng tổng
  - Dung lượng đã dùng
  - Dung lượng trống
  - Hệ thống file (NTFS/FAT32)
- Biểu đồ visual dạng progress bar

### 2. Xem USB Đang kết nối

- Danh sách USB đang cắm vào máy
- Thông tin:
  - Tên USB (VD: KINGSTON, SANDISK)
  - Ký tự ổ đĩa (E:, F:...)
  - Dung lượng
  - Thời điểm phát hiện kết nối

### 3. Quản lý Ứng dụng Đang chạy

- Xem danh sách tất cả process đang chạy
- Thông tin:
  - Tên ứng dụng
  - PID (Process ID)
  - Bộ nhớ đang sử dụng
  - Thời gian chạy
- Đóng ứng dụng từ xa
- Cảnh báo khi đóng process hệ thống

## Giao diện

### Cửa sổ Thông tin Hệ thống

```
┌──────────────────────────────────────────────────────────────┐
│  📊 Thông tin Hệ thống - Nguyễn Văn A                   ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│  [💿 Ổ đĩa]  [🔌 USB]  [📱 Ứng dụng]                        │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ỔN ĐĨA CỨNG                                                │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 💿 C: (Windows)                                      │   │
│  │    Loại: SSD | NTFS                                  │   │
│  │    [████████████████████░░░░░░░░░░] 120GB/250GB 48%  │   │
│  ├──────────────────────────────────────────────────────┤   │
│  │ 💿 D: (Data)                                         │   │
│  │    Loại: HDD | NTFS                                  │   │
│  │    [██░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 45GB/500GB 9%    │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  Cập nhật lần cuối: 10:30:45 AM                 [🔄 Refresh] │
└──────────────────────────────────────────────────────────────┘
```

### Tab USB

```
┌──────────────────────────────────────────────────────────────┐
│  📊 Thông tin Hệ thống - Nguyễn Văn A                   ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│  [💿 Ổ đĩa]  [🔌 USB]  [📱 Ứng dụng]                        │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  USB ĐANG KẾT NỐI (2 thiết bị)                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ 🔌 KINGSTON DataTraveler                             │   │
│  │    Ổ đĩa: E:                                         │   │
│  │    [████████░░░░░░░░░░░░░░░░░░░░░░] 8GB/16GB 50%     │   │
│  │    Kết nối lúc: 09:15:30 AM                          │   │
│  ├──────────────────────────────────────────────────────┤   │
│  │ 🔌 SanDisk Ultra                                     │   │
│  │    Ổ đĩa: F:                                         │   │
│  │    [█░░░░░░░░░░░░░░░░░░░░░░░░░░░░░] 2GB/32GB 6%      │   │
│  │    Kết nối lúc: 10:20:15 AM                          │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ⚠️ Lưu ý: USB có thể được sử dụng để copy dữ liệu          │
└──────────────────────────────────────────────────────────────┘
```

### Tab Ứng dụng

```
┌──────────────────────────────────────────────────────────────┐
│  📊 Thông tin Hệ thống - Nguyễn Văn A                   ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│  [💿 Ổ đĩa]  [🔌 USB]  [📱 Ứng dụng]                        │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  [🔍 Tìm kiếm...]                          [🔄 Refresh]     │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ Tên ứng dụng         │ PID  │ Memory  │ Thao tác       │ │
│  ├──────────────────────┼──────┼─────────┼────────────────┤ │
│  │ 🎮 Minecraft.exe     │ 1234 │ 2.5 GB  │ [🔴 Đóng]      │ │
│  │ 🌐 chrome.exe        │ 5678 │ 512 MB  │ [🔴 Đóng]      │ │
│  │ 📝 notepad.exe       │ 9012 │ 12 MB   │ [🔴 Đóng]      │ │
│  │ 📁 explorer.exe      │ 3456 │ 85 MB   │ [🔒 Hệ thống]  │ │
│  │ ⚙️ svchost.exe       │ 7890 │ 45 MB   │ [🔒 Hệ thống]  │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  Tổng cộng: 42 process | Memory: 8.2/16 GB                  │
│                                                              │
│  [⚠️ Đóng tất cả game]              [Đóng]                  │
└──────────────────────────────────────────────────────────────┘
```

## Quy trình

### Xem thông tin máy học sinh

```
1. Trong MainTeacherWindow, click chuột phải vào học sinh
        │
        ▼
2. Chọn "Xem thông tin hệ thống"
        │
        ▼
3. Cửa sổ SystemInfoWindow mở ra
        │
        ▼
4. Gửi request đến máy học sinh
        │
        ▼ (nền)
5. Máy học sinh thu thập thông tin và gửi về
        │
        ▼
6. Hiển thị thông tin trong các tab
```

### Đóng ứng dụng từ xa

```
1. Mở tab Ứng dụng trong SystemInfoWindow
        │
        ▼
2. Tìm ứng dụng cần đóng
        │
        ▼
3. Click nút [🔴 Đóng] bên cạnh ứng dụng
        │
        ▼
4. Xác nhận (nếu là ứng dụng quan trọng)
        │
        ▼
5. Gửi lệnh KillProcess đến máy học sinh
        │
        ▼
6. Nhận kết quả và cập nhật danh sách
```

## Protocol Messages

### MessageType

| Code | Type               | Mô tả                       |
|------|--------------------|-----------------------------|
| 0x80 | SystemInfoRequest  | Yêu cầu thông tin hệ thống  |
| 0x81 | SystemInfoResponse | Phản hồi thông tin          |
| 0x82 | ProcessListRequest | Yêu cầu danh sách process   |
| 0x83 | ProcessListResponse| Danh sách process           |
| 0x84 | ProcessKillCommand | Lệnh đóng process           |
| 0x85 | ProcessKillResult  | Kết quả đóng process        |

### Payload Format

```json
// SystemInfoRequest
{
  "targetId": "student-1",
  "requestType": "all" // "drives", "usb", "all"
}

// SystemInfoResponse
{
  "clientId": "student-1",
  "drives": [
    {
      "name": "C:",
      "label": "Windows",
      "driveType": "Fixed",
      "fileSystem": "NTFS",
      "totalSize": 268435456000,
      "freeSpace": 134217728000
    }
  ],
  "usbDevices": [
    {
      "deviceId": "usb-001",
      "name": "KINGSTON",
      "driveLetter": "E:",
      "totalSize": 16000000000,
      "freeSpace": 8000000000,
      "connectedAt": "2026-01-21T09:15:30"
    }
  ],
  "timestamp": "2026-01-21T10:30:45"
}

// ProcessKillCommand
{
  "targetId": "student-1",
  "processId": 1234,
  "processName": "Minecraft.exe"
}

// ProcessKillResult
{
  "processId": 1234,
  "success": true,
  "message": "Process terminated successfully"
}
```

## Models

```csharp
public class DriveInfoModel
{
    public string Name { get; set; }           // "C:"
    public string Label { get; set; }          // "Windows"
    public string DriveType { get; set; }      // "Fixed", "Removable"
    public string FileSystem { get; set; }     // "NTFS", "FAT32"
    public long TotalSize { get; set; }
    public long FreeSpace { get; set; }
    public double UsedPercent => (TotalSize - FreeSpace) * 100.0 / TotalSize;
}

public class UsbDeviceModel
{
    public string DeviceId { get; set; }
    public string Name { get; set; }
    public string DriveLetter { get; set; }
    public long TotalSize { get; set; }
    public long FreeSpace { get; set; }
    public DateTime ConnectedAt { get; set; }
}

public class ProcessInfoModel
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; }
    public string WindowTitle { get; set; }
    public long MemoryUsage { get; set; }
    public string ExecutablePath { get; set; }
    public DateTime StartTime { get; set; }
    public bool IsSystemProcess { get; set; }
}
```

## Implementation Notes

### Phía Student (NetworkClientService)

```csharp
// Xử lý SystemInfoRequest
private async Task HandleSystemInfoRequest(NetworkMessage message)
{
    var systemInfo = SystemInfoService.CollectSystemInfo();
    var response = new NetworkMessage
    {
        Type = MessageType.SystemInfoResponse,
        Payload = JsonSerializer.Serialize(systemInfo)
    };
    await SendMessageAsync(response);
}

// Xử lý ProcessKillCommand
private async Task HandleProcessKillCommand(NetworkMessage message)
{
    var command = JsonSerializer.Deserialize<KillProcessCommand>(message.Payload);
    var result = ProcessManagerService.KillProcess(command.ProcessId);

    var response = new NetworkMessage
    {
        Type = MessageType.ProcessKillResult,
        Payload = JsonSerializer.Serialize(result)
    };
    await SendMessageAsync(response);
}
```

### Process Blacklist (Không cho đóng)

```csharp
private static readonly string[] SystemProcesses = {
    "System",
    "smss.exe",
    "csrss.exe",
    "wininit.exe",
    "services.exe",
    "lsass.exe",
    "svchost.exe",
    "explorer.exe",
    "ClassroomManagement.exe" // Bản thân ứng dụng
};
```

## Lưu ý Bảo mật

1. **Validate ProcessId**: Kiểm tra process tồn tại trước khi kill
2. **Blacklist**: Không cho phép đóng process hệ thống
3. **Logging**: Ghi log mọi lệnh đóng process
4. **Confirmation**: Yêu cầu xác nhận từ giáo viên

## Auto-Refresh

- Thông tin cập nhật mỗi 30 giây (có thể tùy chỉnh)
- Hoặc click nút Refresh để cập nhật ngay
- Timer tạm dừng khi cửa sổ không focus

---

_Xem thêm: [Workflow phát triển](../../.agent/workflows/new-features-development.md)_
