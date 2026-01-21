---
description: Workflow phát triển tính năng Chat Nâng cao (Feature 1) - Chat nhóm tùy chỉnh, gửi hình ảnh, gửi file đính kèm.
---

# Phát triển Tính năng Chat Nâng cao

## Tổng quan
- Chat cá nhân (1-1) giữa giáo viên và học sinh
- Tạo nhóm chat tùy chỉnh (chỉ giáo viên mới có thể tạo)
- Gửi hình ảnh vào nhóm chat
- Gửi file đính kèm vào nhóm chat
- Giao diện chat hiện đại

## Các bước thực hiện

### 1. Cập nhật Backend Models
**Files:**
- `Models/ChatModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)
- `Models/Entities.cs` (Cập nhật)

**Nội dung:**
- Định nghĩa `ChatGroup`, `ChatMessage`, `MessageContentType`
- Thêm `MessageType` mới: `ChatGroupCreate`, `ChatImageMessage`, `ChatFileMessage`...

### 2. Implement Services
**Files:**
- `Services/ChatService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Thêm bảng ChatGroups, ChatGroupMembers, ChatAttachments)
- `Services/SessionManager.cs` (Tích hợp ChatService)
- `Services/NetworkServerService.cs` & `NetworkClientService.cs` (Xử lý message mới)

**Logic:**
- `CreateGroupAsync`: Tạo nhóm mới, lưu DB
- `SendImageAsync`, `SendFileAsync`: Xử lý upload và gửi, broadcast message

### 3. Implement Views
**Files:**
- `Views/ChatView.xaml` & `.cs` (Thay thế ChatWindow cũ)
- `Views/CreateChatGroupDialog.xaml` & `.cs` (Dialog tạo nhóm)

**UI:**
- Sidebar hiển thị danh sách Nhóm và Học sinh
- Khu vực chat chính với bong bóng chat (bubble)
- Input bar với nút gửi ảnh, file
- Hiển thị hình ảnh preview trong chat

### 4. Database Schema
```sql
CREATE TABLE ChatGroups (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    creator_id TEXT NOT NULL,
    session_id INTEGER,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);
-- (Thêm các bảng related khác)
```

## Verification
- [ ] Mở ChatView, kiểm tra danh sách học sinh
- [ ] Giáo viên tạo nhóm chat mới, mời học sinh
- [ ] Chat text trong nhóm
- [ ] Gửi hình ảnh, kiểm tra hiển thị
- [ ] Gửi file, kiểm tra download
