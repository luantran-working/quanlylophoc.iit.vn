# Chat Nâng cao

## Tổng quan

Tính năng Chat Nâng cao mở rộng khả năng giao tiếp trong lớp học với hỗ trợ nhóm chat tùy chỉnh, gửi hình ảnh và file đính kèm.

## Các tính năng

### 1. Chat Cá nhân (1-1)

- Giáo viên chat riêng với từng học sinh
- Học sinh có thể gửi tin nhắn cho giáo viên
- Lịch sử chat được lưu trữ theo phiên

### 2. Nhóm Chat Tùy chỉnh

```
┌─────────────────────────────────────────────────────────────┐
│                   TẠO NHÓM CHAT                             │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ⚠️ Lưu ý: Chỉ Giáo viên mới có thể tạo nhóm chat          │
│                                                             │
│  1. Giáo viên click [+ Tạo nhóm] trong ChatView             │
│                      │                                      │
│                      ▼                                      │
│  2. Nhập tên nhóm: "Nhóm Toán nâng cao"                    │
│                      │                                      │
│                      ▼                                      │
│  3. Chọn thành viên từ danh sách học sinh                  │
│     ☑ Nguyễn Văn A                                         │
│     ☑ Trần Thị B                                           │
│     ☐ Lê Hoàng C                                           │
│                      │                                      │
│                      ▼                                      │
│  4. Click [Tạo nhóm]                                       │
│                      │                                      │
│                      ▼                                      │
│  5. Nhóm xuất hiện trong danh sách chat                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### 3. Gửi Hình ảnh

- Hỗ trợ định dạng: JPG, PNG, GIF, BMP
- Kích thước tối đa: 10MB
- Preview hình trước khi gửi
- Có thể paste trực tiếp từ clipboard (Ctrl+V)

```
Quy trình gửi hình:

1. Click icon [📷] trong ô nhập tin nhắn
        │
        ▼
2. Chọn hình từ máy tính hoặc paste clipboard
        │
        ▼
3. Preview hình ảnh
        │
        ▼
4. Thêm caption (tùy chọn)
        │
        ▼
5. Click [Gửi]
        │
        ▼
6. Hình được nén và gửi đến nhóm/người nhận
```

### 4. Gửi File đính kèm

- Hỗ trợ tất cả định dạng file
- Kích thước tối đa: 50MB
- Hiển thị icon theo loại file
- Người nhận click để tải về

```
Các loại file được hỗ trợ:

| Icon | Loại file                    |
|------|------------------------------|
| 📄   | Document (doc, docx, pdf)    |
| 📊   | Spreadsheet (xls, xlsx)      |
| 📑   | Presentation (ppt, pptx)     |
| 🖼️   | Image (jpg, png, gif)        |
| 🎵   | Audio (mp3, wav)             |
| 🎬   | Video (mp4, avi)             |
| 📦   | Archive (zip, rar)           |
| 📝   | Code (py, js, cs)            |
| 📎   | Other                        |
```

## Giao diện

### ChatView (Teacher & Student)

```
┌──────────────────────────────────────────────────────────────┐
│  💬 Chat                                              ─ □ × │
├─────────────────┬────────────────────────────────────────────┤
│ NHÓM            │  ← Lớp 10A1                    25 online  │
│                 ├────────────────────────────────────────────┤
│ ● Lớp 10A1  (3) │                 Hôm nay                    │
│ ○ Nhóm Toán     │                                            │
│ ○ Nhóm Văn      │  ┌─────────────────────────────────────┐   │
│                 │  │ 👤 Thầy Minh                10:01   │   │
│ ───────────────│  │ Các em xem hình này nhé             │   │
│ CHAT RIÊNG      │  │ ┌────────────────────────┐          │   │
│                 │  │ │    [Hình minh họa]     │          │   │
│ ● Nguyễn Văn A  │  │ └────────────────────────┘          │   │
│ ○ Trần Thị B    │  └─────────────────────────────────────┘   │
│                 │                                            │
│ ───────────────│             ┌──────────────────────────┐   │
│ [+ Tạo nhóm]    │             │ Dạ em hiểu rồi ạ! 🙌    │   │
│ (Chỉ GV)        │             └──────────────────────────┘   │
│                 │                              10:02 ✓✓      │
│                 │                                            │
│                 ├────────────────────────────────────────────┤
│                 │ [📷][📎] [Nhập tin nhắn...        ] [➤]   │
└─────────────────┴────────────────────────────────────────────┘
```

### Dialog Tạo Nhóm

```
┌──────────────────────────────────────────────────────────────┐
│  📁 Tạo Nhóm Chat Mới                                   × │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Tên nhóm: [Nhóm Ôn tập Toán                         ]     │
│                                                              │
│  Chọn thành viên:                                           │
│  ┌────────────────────────────────────────────────────────┐ │
│  │ [🔍 Tìm kiếm...]                                       │ │
│  │                                                        │ │
│  │ ☑ Nguyễn Văn An          ○ Online                     │ │
│  │ ☑ Trần Thị Bình          ● Offline                    │ │
│  │ ☐ Lê Hoàng Cường         ○ Online                     │ │
│  │ ☑ Phạm Thu Dung          ○ Online                     │ │
│  │ ☐ Võ Minh Em             ● Offline                    │ │
│  └────────────────────────────────────────────────────────┘ │
│                                                              │
│  Đã chọn: 3 học sinh                                        │
│                                                              │
│                              [Tạo nhóm]    [Hủy]            │
└──────────────────────────────────────────────────────────────┘
```

## Protocol Messages

### MessageType mới

| Code | Type             | Mô tả                      |
|------|------------------|----------------------------|
| 0x32 | ChatGroupCreate  | Tạo nhóm chat mới          |
| 0x33 | ChatGroupInvite  | Mời thêm thành viên        |
| 0x34 | ChatGroupLeave   | Rời khỏi nhóm              |
| 0x35 | ChatGroupList    | Danh sách nhóm             |
| 0x36 | ChatImageMessage | Tin nhắn có hình           |
| 0x37 | ChatFileMessage  | Tin nhắn có file đính kèm  |

### Payload Format

```json
// ChatGroupCreate
{
  "groupId": "uuid",
  "name": "Nhóm Toán",
  "memberIds": ["student-1", "student-2"],
  "creatorId": "teacher-1"
}

// ChatImageMessage
{
  "groupId": "uuid",
  "senderId": "teacher-1",
  "imageData": "base64-encoded-image",
  "fileName": "screenshot.png",
  "caption": "Hình minh họa"
}

// ChatFileMessage
{
  "groupId": "uuid",
  "senderId": "student-1",
  "fileId": "uuid",
  "fileName": "baitap.docx",
  "fileSize": 1024000,
  "message": "Em nộp bài ạ"
}
```

## Database Tables

```sql
-- Nhóm chat
CREATE TABLE ChatGroups (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    creator_id TEXT NOT NULL,
    session_id INTEGER,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (session_id) REFERENCES Sessions(id)
);

-- Thành viên nhóm
CREATE TABLE ChatGroupMembers (
    group_id TEXT,
    member_id TEXT,
    member_name TEXT,
    joined_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (group_id, member_id)
);

-- File/Hình đính kèm
CREATE TABLE ChatAttachments (
    id TEXT PRIMARY KEY,
    message_id INTEGER,
    file_name TEXT NOT NULL,
    file_path TEXT NOT NULL,
    file_size INTEGER,
    content_type TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (message_id) REFERENCES ChatMessages(id)
);
```

## Phím tắt

| Phím tắt        | Chức năng              |
|-----------------|------------------------|
| `Enter`         | Gửi tin nhắn           |
| `Shift + Enter` | Xuống dòng             |
| `Ctrl + V`      | Paste ảnh/file         |
| `Ctrl + I`      | Chèn hình ảnh          |
| `Ctrl + A`      | Đính kèm file          |
| `Esc`           | Đóng cửa sổ chat       |

## Giới hạn

| Thông số              | Giá trị    |
|-----------------------|------------|
| Số thành viên nhóm    | 50 max     |
| Kích thước hình       | 10 MB      |
| Kích thước file       | 50 MB      |
| Độ dài tin nhắn       | 5000 ký tự |
| Số nhóm mỗi phiên     | 20 nhóm    |

---

_Xem thêm: [Workflow phát triển](../../.agent/workflows/new-features-development.md)_
