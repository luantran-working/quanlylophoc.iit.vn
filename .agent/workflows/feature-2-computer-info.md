---
description: Workflow phát triển tính năng Kiểm tra Thông tin Máy tính (Feature 2) - Xem thông tin ổ đĩa, USB đang kết nối.
---

# Phát triển Tính năng Kiểm tra Thông tin Máy tính

## Tổng quan
- Xem thông tin ổ đĩa (C:, D:) của học sinh
- Xem danh sách USB đang kết nối
- Hiển thị dung lượng trống/đã dùng realtime

## Các bước thực hiện

### 1. Cập nhật Models
**Files:**
- `Models/SystemInfoModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**Nội dung:**
- Class `DriveInfo`, `UsbDeviceInfo`, `SystemInfoPackage`
- MessageType: `SystemInfoRequest`, `SystemInfoResponse`

### 2. Implement Services
**Files:**
- `Services/SystemInfoService.cs` (Tạo mới - chạy ở Client)
- `Services/NetworkClientService.cs` (Xử lý Request, gửi Response)
- `Services/NetworkServerService.cs` (Gửi Request, nhận Response)

**Logic:**
- `SystemInfoService.CollectSystemInfo()`: Sử dụng `DriveInfo.GetDrives()` và WMI để lấy thông tin USB

### 3. Implement Views
**Files:**
- `Views/StudentInfoWindow.xaml` & `.cs` (Tạo mới)
- `Views/MainTeacherWindow.xaml` (Thêm Context Menu "Xem thông tin")

**UI:**
- Tab "Ổ đĩa": Progress bar hiển thị dung lượng
- Tab "USB": Danh sách thiết bị ngoại vi

## Verification
- [ ] Chuột phải vào học sinh -> Chọn xem thông tin
- [ ] Kiểm tra hiển thị đúng danh sách ổ đĩa
- [ ] Cắm USB vào máy học sinh -> Refresh -> Kiểm tra hiển thị
