# Nộp Bài tập & Gửi File Hàng loạt

## Tổng quan

Tính năng hỗ trợ việc nộp bài tập từ học sinh và phân phối tài liệu từ giáo viên.

## Phần 1: Nộp Bài tập (Học sinh → Giáo viên)

### Mô tả
- Học sinh chủ động upload bài tập lên máy giáo viên
- File được tổ chức theo thư mục: Phiên học > Tên học sinh
- Giáo viên nhận thông báo khi có bài tập mới

### Giao diện Học sinh - Dialog Nộp bài

```
┌─────────────────────────────────────────────────────┐
│  📤 Nộp Bài tập                                 × │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐ │
│  │   📂 Kéo thả file hoặc [Chọn file...]        │ │
│  └───────────────────────────────────────────────┘ │
│                                                     │
│  FILE ĐÃ CHỌN:                                     │
│  📄 BaiTap.docx      2.5 MB   [❌]                 │
│  📊 Excel.xlsx       500 KB   [❌]                 │
│                                                     │
│  Ghi chú: [                               ]         │
│                                                     │
│             Tổng: 2 file | 3 MB                     │
│                     [📤 Nộp bài]    [Hủy]           │
└─────────────────────────────────────────────────────┘
```

### Giao diện Giáo viên - Quản lý Bài tập

```
┌─────────────────────────────────────────────────────┐
│  📋 Quản lý Bài tập                            ─ □ ×│
├─────────────────────────────────────────────────────┤
│  Phiên: [21/01/2026 ▼]    Đã nộp: 25/30            │
├─────────────────────────────────────────────────────┤
│  Học sinh      │ Thời gian │ Files    │ Thao tác   │
│  ──────────────┼───────────┼──────────┼──────────  │
│  Nguyễn Văn A  │ 10:35     │ 2 (3MB)  │ [📂][📥]   │
│  Trần Thị B    │ 10:38     │ 1 (2MB)  │ [📂][📥]   │
│  Lê C          │ ⏳ Chưa   │ -        │ [🔔 Nhắc]  │
├─────────────────────────────────────────────────────┤
│  [📥 Tải tất cả]  [📂 Mở thư mục]      [Đóng]      │
└─────────────────────────────────────────────────────┘
```

### Cấu trúc Thư mục

```
📁 %LOCALAPPDATA%\IIT\ClassroomManagement\Assignments\
├── 📁 2026-01-21_Lop10A1\
│   ├── 📁 Nguyen_Van_An\
│   │   └── 📄 BaiTap.docx
│   └── 📁 Tran_Thi_Binh\
│       └── 📄 Bai.pdf
```

---

## Phần 2: Gửi File Hàng loạt

### Giao diện Giáo viên

```
┌─────────────────────────────────────────────────────┐
│  📤 Gửi File Hàng loạt                         ─ □ ×│
├─────────────────────────────────────────────────────┤
│  [+ Thêm file...]                                   │
│  📄 TaiLieu.pdf    5.2 MB   [❌]                   │
│  📊 DeCuong.docx   1.1 MB   [❌]                   │
│                                                     │
│  NGƯỜI NHẬN: ◉ Tất cả (30)  ○ Chọn                 │
│                                                     │
│  Tin nhắn: [Tài liệu ôn tập ]                      │
│                                                     │
│  TIẾN TRÌNH: [████████░░░] 55% - 16/30 đã nhận     │
│                     [📤 Gửi]    [Đóng]              │
└─────────────────────────────────────────────────────┘
```

### Popup Học sinh Nhận file

```
┌────────────────────────────────────────┐
│  📥 File mới từ Giáo viên          × │
├────────────────────────────────────────┤
│  📄 TaiLieu.pdf (5.2 MB)              │
│  📊 DeCuong.docx (1.1 MB)             │
│                                        │
│  "Tài liệu ôn tập"                    │
│                                        │
│   [💾 Lưu về máy]    [Bỏ qua]         │
└────────────────────────────────────────┘
```

---

## Protocol Messages

| Code | Type                | Mô tả              |
|------|---------------------|--------------------|
| 0x90 | AssignmentSubmit    | Học sinh nộp bài   |
| 0x91 | AssignmentSubmitAck | Xác nhận nhận bài  |
| 0xA0 | BulkFileSend        | Gửi file hàng loạt |
| 0xA1 | BulkFileNotification| Thông báo file mới |
| 0xA2 | BulkFileDownload    | Yêu cầu tải file   |

---

## Database Tables

```sql
CREATE TABLE Assignments (
    id TEXT PRIMARY KEY,
    session_id INTEGER,
    student_id TEXT,
    student_name TEXT,
    note TEXT,
    submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE AssignmentFiles (
    assignment_id TEXT,
    file_name TEXT,
    file_path TEXT,
    file_size INTEGER
);
```

## Giới hạn

| Thông số           | Giá trị  |
|--------------------|----------|
| File tối đa        | 100 MB   |
| Số file mỗi lần    | 20 files |

_Xem: [Workflow](../../.agent/workflows/new-features-development.md)_
