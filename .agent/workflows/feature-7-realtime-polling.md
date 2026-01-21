---
description: Workflow phát triển tính năng Bình chọn Thời gian thực (Feature 7) - Chat nhóm tùy chỉnh, gửi hình ảnh, gửi file đính kèm.
---

# Phát triển Tính năng Bình chọn Thời gian thực

## Tổng quan
- Tạo poll (câu hỏi + lựa chọn)
- Học sinh vote realtime
- Hiển thị kết quả trực quan

## Các bước thực hiện

### 1. Cập nhật Models
**Files:**
- `Models/PollModels.cs` (Tạo mới)
- `Models/NetworkModels.cs` (Cập nhật)

**Nội dung:**
- `Poll`, `PollOption`, `PollVote`
- MessageType: `PollCreate`, `PollVote`, `PollResult`

### 2. Implement Services
**Files:**
- `Services/PollService.cs` (Tạo mới)
- `Services/DatabaseService.cs` (Bảng Polls)

**Logic:**
- Server quản lý state của Poll
- Khi nhận vote -> Update DB -> Broadcast kết quả mới nhất cho tất cả client (nếu mode show result)

### 3. Implement Views
**Files:**
- `Views/CreatePollWindow.xaml` (Tạo poll)
- `Views/PollResultWindow.xaml` (Xem kết quả)
- `Views/VotePollWindow.xaml` (Giao diện vote của HS)

## Verification
- [ ] Giáo viên tạo Poll -> Start
- [ ] Học sinh thấy cửa sổ Vote hiện lên
- [ ] Học sinh chọn đáp án -> Gửi
- [ ] Giáo viên thấy biểu đồ thay đổi ngay lập tức
