---
description: Workflow phát triển tính năng Nộp Bài tập (Feature 5) - Học sinh upload bài tập, giáo viên quản lý bài nộp.
---

# Phát triển Tính năng Nộp Bài tập

## Tổng quan
- Học sinh chủ động nộp bài (upload file)
- Giáo viên nhận thông báo và quản lý bài nộp

## Các bước thực hiện

### 1. Cập nhật Models
**Files:**
- `Models/AssignmentModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**Nội dung:**
- `AssignmentSubmission`, `SubmittedFile`
- MessageType: `AssignmentSubmit`

### 2. Implement Services
**Files:**
- `Services/AssignmentService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Thêm bảng Assignments)

**Logic:**
- Client: Chọn file, gửi lên Server
- Server: Lưu file, ghi nhận vào Database, thông báo UI

### 3. Implement Views
**Files:**
- `Views/StudentWindow.xaml` (Thêm nút Nộp bài)
- `Views/SubmitAssignmentDialog.xaml` (Dialog chọn file)
- `Views/AssignmentListWindow.xaml` (Teacher view quản lý bài)

## Verification
- [ ] Học sinh bấm Nộp bài
- [ ] Chọn file upload
- [ ] Giáo viên thấy thông báo có bài mới
- [ ] Giáo viên mở danh sách, thấy file vừa nộp
