---
description: Workflow phát triển tính năng Thu thập File (Feature 4) - Thu thập tất cả file từ thư mục chỉ định.
---

# Phát triển Tính năng Thu thập File

## Tổng quan
- Giáo viên chỉ định thư mục trên máy học sinh
- Thu thập tất cả file (hoặc lọc theo extension) về máy giáo viên
- Hiển thị tiến trình

## Các bước thực hiện

### 1. Cập nhật Models
**Files:**
- `Models/FileCollectionModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**Nội dung:**
- `FileCollectionRequest` (FolderPath, Extensions)
- MessageType: `FileCollectionRequest`, `FileCollectionProgress`

### 2. Implement Services
**Files:**
- `Services/FileCollectionService.cs` (Tạo mới)
- `Services/NetworkClientService.cs`

**Logic:**
- Client: Quét thư mục, upload từng file về Server
- Server: Lưu file vào cấu trúc thư mục định sẵn

### 3. Implement Views
**Files:**
- `Views/FileCollectionWindow.xaml` & `.cs` (Tạo mới)

**UI:**
- Input nhập đường dẫn thư mục nguồn
- Checkbox chọn loại file
- Progress bar hiển thị trạng thái từng học sinh

## Verification
- [ ] Nhập đường dẫn tồn tại trên máy học sinh (VD: Documents)
- [ ] Chọn thu thập file .docx
- [ ] Kiểm tra file được tải về máy giáo viên
