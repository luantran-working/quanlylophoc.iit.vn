---
description: Workflow phát triển tính năng Kiểm tra Thông tin Máy tính (Feature 2) - Xem thông tin chi tiết cấu hình máy tính của học sinh.
---

# Phát triển Tính năng Kiểm tra Thông tin Máy tính (Cập nhật)

## Tổng quan
- Xem thông tin chi tiết cấu hình máy tính (CPU, RAM, GPU, OS, Mainboard, v.v.)
- Xem thông tin ổ đĩa (Dung lượng, phân vùng)
- Bảng cấu hình tổng quát: Xem cấu hình của tất cả các máy học sinh cùng lúc trong một bảng.

## Các bước thực hiện

### 1. Cập nhật Models
**Files:**
- `Models/SystemInfoModels.cs` (Tạo mới hoặc cập nhật)
- `Models/NetworkModels.cs` (Cập nhật MessageType)

**Nội dung:**
- Class `ComputerSpecs`: OS, CPU, RAM, GPU, Motherboard, BIOS, Monitor.
- Class `DiskDriveInfo`: Name, TotalSize, FreeSpace.
- Class `SystemInfoPackage`: ComputerSpecs, List<DiskDriveInfo>.
- MessageType: `SystemSpecsRequest` (0xA0), `SystemSpecsResponse` (0xA1).

### 2. Implement Services (Client-side)
**Files:**
- `Services/SystemInfoService.cs` (Tạo mới)
- `Services/NetworkClientService.cs` (Xử lý request từ giáo viên)

**Logic:**
- `GetSystemSpecs()`: Sử dụng `WMI` (`ManagementObjectSearcher`) để lấy thông tin phần cứng.
- Đóng gói thông tin gửi về Server.

### 3. Implement Services (Server-side)
**Files:**
- `Services/SessionManager.cs`: Lưu trữ thông tin cấu hình các máy nhận được.
- `Services/NetworkServerService.cs`: Gửi tín hiệu yêu cầu cấu hình.

### 4. Implement Views
**Files:**
- `Views/ComputerSpecsWindow.xaml` & `.cs`: Hiển thị chi tiết cấu hình của 1 máy.
- `Views/SystemConfigTableWindow.xaml` & `.cs`: Bảng hiển thị thông số của tất cả các máy học sinh (Bảng cấu hình).
- `Views/MainTeacherWindow.xaml`: Thêm nút "Bảng cấu hình" và Context Menu "Xem cấu hình".

## Verification
- [ ] Chọn 1 học sinh -> Xem cấu hình chi tiết -> Hiển thị đúng thông số.
- [ ] Click nút "Bảng cấu hình" -> Hiển thị bảng tổng quát tất cả học sinh đang online.
- [ ] Kiểm tra các thông số CPU, RAM, GPU có chính xác không.
