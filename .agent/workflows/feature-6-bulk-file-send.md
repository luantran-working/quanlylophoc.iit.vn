---
description: Workflow phát triển tính năng Gửi File Hàng loạt (Feature 6) - Gửi file cho nhiều học sinh cùng lúc.
---

# Phát triển Tính năng Gửi File Hàng loạt

## Tổng quan
- Giáo viên gửi file đến danh sách học sinh
- Học sinh nhận Popup thông báo tải file

## Các bước thực hiện

### 1. Cập nhật Models
**Files:**
- `Models/BulkFileModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

### 2. Implement Services
**Files:**
- `Services/BulkFileService.cs` (Tạo mới)

**Logic:**
- Server: Broadcast thông báo có file mới
- Client: Hiển thị Popup -> User accept -> Download file

### 3. Implement Views
**Files:**
- `Views/BulkFileSendWindow.xaml` (Giao diện gửi của GV)
- `Views/FileNotificationPopup.xaml` (Popup phía Client)

## Database Tables
- `BulkFileTransfers` (Lịch sử gửi)

## Verification
- [ ] Giáo viên chọn file, bấm Gửi
- [ ] Học sinh thấy Popup
- [ ] Học sinh bấm Lưu -> File tải về thành công
