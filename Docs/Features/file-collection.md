# Thu thập File (File Collection)

## Tổng quan

Tính năng cho phép giáo viên thu thập file từ một thư mục cụ thể trên máy tính của học sinh một cách tự động.

## Tính năng

### 1. Chỉ định Thư mục Nguồn
- Giáo viên nhập đường dẫn thư mục trên máy học sinh (ví dụ: `C:\Users\Student\Documents\BaiTap`)
- Hoặc sử dụng các đường dẫn alias: `%Desktop%`, `%Documents%`

### 2. Bộ lọc File
- Lọc theo phần mở rộng: `.docx`, `.pdf`, `.cpp`...
- Tùy chọn thu thập đệ quy (bao gồm thư mục con)
- Lọc theo kích thước hoặc thời gian sửa đổi (Advanced)

### 3. Giao diện Giáo viên

```
┌──────────────────────────────────────────────────────────────┐
│  📂 Thu thập File từ Học sinh                          ─ □ ×│
├──────────────────────────────────────────────────────────────┤
│  CẤU HÌNH THU THẬP                                          │
│  Đường dẫn: [ %Documents%\BaiKiemTra                 ] [📁] │
│                                                              │
│  Tùy chọn:                                                  │
│  ☑ Bao gồm thư mục con                                      │
│  ☐ Ghi đè file cũ nếu trùng tên                             │
│                                                              │
│  Lọc đuôi file: [ *.docx; *.pdf                      ]      │
│                                                              │
│  ĐỐI TƯỢNG:                                                 │
│  ☑ Tất cả lớp (30)    ☐ Nhóm: Tổ 1                          │
│                                                              │
│  ────────────────────────────────────────────────────────── │
│  TIẾN TRÌNH                                                 │
│                                                              │
│  Nguyễn Văn A: [████████░░░░░░░] 5/10 files (2MB)           │
│  Trần Thị B:   [███████████████] Hoàn thành ✓               │
│                                                              │
│                   [▶ Bắt đầu Thu thập]    [Đóng]            │
└──────────────────────────────────────────────────────────────┘
```

## Quy trình Kỹ thuật

1. **Request**: Giáo viên gửi `FileCollectionRequest` (Path, Filter) tới học sinh.
2. **Scan**: Client nhận request -> Scan thư mục -> Tạo danh sách file match.
3. **Upload**: Client lần lượt upload file qua giao thức `FileData`.
4. **Progress**: Client gửi `FileCollectionProgress` cập nhật trạng thái.
5. **Finish**: Gửi `FileCollectionComplete` khi xong.

### Database

Không cần bảng riêng, file thu thập được sẽ lưu như `FileRecords` thông thường nhưng có đánh dấu `source='collection'`.

## Lưu ý Bảo mật
- Chỉ cho phép thu thập trong các thư mục User (Documents, Desktop...).
- Chặn truy cập thư mục hệ thống (Windows, Program Files).
- Hiển thị thông báo (Toast) ở máy học sinh khi quá trình thu thập bắt đầu.

---
_Xem thêm: [Workflow phát triển](../../.agent/workflows/feature-4-file-collection.md)_
