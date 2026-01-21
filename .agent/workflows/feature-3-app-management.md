---
description: Workflow phát triển tính năng Quản lý Ứng dụng (Feature 3) - Xem danh sách ứng dụng, đóng ứng dụng từ xa.
---

# Phát triển Tính năng Quản lý Ứng dụng

## Tổng quan
- Xem danh sách process đang chạy
- Đóng ứng dụng từ xa
- Cảnh báo với process hệ thống

## Các bước thực hiện

### 1. Cập nhật Models
**Files:**
- `Models/ProcessModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**Nội dung:**
- `ProcessInfo` (PID, Name, Memory)
- MessageType: `ProcessListRequest`, `ProcessKillCommand`...

### 2. Implement Services
**Files:**
- `Services/ProcessManagerService.cs` (Tạo mới - Client)
- `Services/NetworkClientService.cs` (Xử lý lệnh)

**Logic:**
- `GetRunningProcesses()`: Lấy danh sách process, lọc process hệ thống
- `KillProcess(pid)`: Đóng ứng dụng

### 3. Implement Views
**Files:**
- `Views/ProcessManagerWindow.xaml` & `.cs` (Tạo mới)

**UI:**
- DataGrid hiển thị danh sách process
- Nút "Đóng" (Kill) cho từng dòng
- Nút Refresh

## Verification
- [ ] Mở cửa sổ Quản lý ứng dụng
- [ ] Kiểm tra danh sách process
- [ ] Chọn một ứng dụng (ví dụ Notepad) -> Bấm Đóng
- [ ] Kiểm tra ứng dụng trên máy học sinh có tắt không
