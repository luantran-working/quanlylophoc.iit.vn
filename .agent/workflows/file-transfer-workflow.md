---
description: Quy trình thu bài và gửi file giữa giáo viên và học sinh
---

# Quy trình Truyền File

## Mục tiêu

Trao đổi tập tin giữa giáo viên và học sinh thông qua mạng LAN.

---

## Workflow 1: Thu bài từ Học sinh (Giáo viên)

### Bước 1: Mở cửa sổ File Transfer

```
1. Trong Main Teacher Window

2. Click "Thu bài" trong sidebar phải
   Công cụ → Quản lý → Thu bài

3. Cửa sổ File Transfer Window mở ra
```

### Bước 2: Chọn học sinh

```
1. Danh sách học sinh online hiển thị

2. Chọn học sinh cần thu bài:
   ☑ Tất cả       ← Chọn tất cả
   ☑ Nguyễn Văn An
   ☑ Trần Thị Bình
   ☐ Lê Hoàng Cường (Offline)
   ☑ Phạm Thu Dung

3. Hoặc click "Chọn tất cả"
```

### Bước 3: Bắt đầu thu bài

```
1. Click "Bắt đầu thu bài"

2. Học sinh nhận thông báo:
   ┌────────────────────────────────────────┐
   │  📤 Giáo viên yêu cầu nộp bài         │
   │                                        │
   │  Vui lòng chọn file để gửi.           │
   │                                        │
   │         [Chọn file]                    │
   └────────────────────────────────────────┘

3. Chờ học sinh gửi file
```

### Bước 4: Theo dõi tiến trình

```
1. Cửa sổ hiển thị tiến độ từng học sinh:

   Nguyễn Văn An
   [████████████████████] 100% ✓

   Trần Thị Bình
   [██████████░░░░░░░░░░]  65%

   Phạm Thu Dung
   [░░░░░░░░░░░░░░░░░░░░]   0%
   Đang chờ...

2. File được lưu tự động vào thư mục theo phiên
```

### Bước 5: Xem file đã thu

```
1. Click "Mở thư mục" để xem file

2. Vị trí: %LOCALAPPDATA%\IIT\ClassroomManagement\Files\Uploads\
   └── 2026-01-16_Session_1\
       ├── Nguyễn Văn An\
       │   └── Bai_tap_1.docx
       └── Trần Thị Bình\
           └── Bai_tap_1.pdf
```

---

## Workflow 2: Gửi file đến Học sinh (Giáo viên)

### Bước 1: Chọn mode Gửi file

```
1. Trong File Transfer Window

2. Click tab "Gửi file"
   [Thu bài] [Gửi file]
                ▲
```

### Bước 2: Chọn file để gửi

```
1. Click "Chọn file" hoặc kéo thả

2. Có thể chọn nhiều file

3. Danh sách file hiển thị:
   📄 Tai_lieu_bai_giang.pptx (2.5 MB)
   📄 De_cuong.pdf (156 KB)
   [Thêm file] [Xóa]
```

### Bước 3: Chọn học sinh nhận

```
1. Tick chọn học sinh:
   ☑ Tất cả học sinh
   ☐ Chọn từng học sinh

2. Nếu chọn từng học sinh:
   ☐ Nguyễn Văn An
   ☑ Trần Thị Bình
   ☐ Lê Hoàng Cường
```

### Bước 4: Chọn thư mục đích (Tùy chọn)

```
1. Mặc định: Desktop\ClassroomFiles\

2. Có thể chọn:
   - Desktop
   - Documents
   - Custom path
```

### Bước 5: Gửi file

```
1. Click "Gửi"

2. File được truyền đến máy học sinh

3. Tiến trình hiển thị:
   Trần Thị Bình
   [████████████████████] 100% ✓

4. Học sinh nhận thông báo file mới
```

---

## Workflow 3: Gửi file (Học sinh)

### Bước 1: Mở công cụ gửi file

```
1. Trong Student Window

2. Click "Gửi file cho GV" trong sidebar
   Công cụ học tập → Gửi file cho GV
```

### Bước 2: Chọn file

```
1. Hộp thoại chọn file mở ra

2. Chọn một hoặc nhiều file
   (Giới hạn: 100MB/file, 20 files/lần)

3. Click "Open"
```

### Bước 3: Xác nhận gửi

```
1. Popup xác nhận:
   ┌────────────────────────────────────────┐
   │  Gửi file cho Giáo viên?               │
   │                                        │
   │  📄 Bai_tap_1.docx (1.2 MB)           │
   │  📄 Anh_minh_hoa.jpg (450 KB)         │
   │                                        │
   │         [Gửi] [Hủy]                   │
   └────────────────────────────────────────┘

2. Click "Gửi"
```

### Bước 4: Upload

```
1. Tiến trình hiển thị:
   [██████████████░░░░░░]  75%

2. Chờ hoàn tất

3. Thông báo:
   "✓ Gửi file thành công!"
```

---

## Vị trí lưu file

### Máy Giáo viên (Server)

```
📁 %LOCALAPPDATA%\IIT\ClassroomManagement\Files\
├── 📁 Uploads\              # File từ học sinh
│   └── 📁 2026-01-16_Session_1\
│       ├── 📁 Nguyễn Văn An\
│       └── 📁 Trần Thị Bình\
│
└── 📁 Shared\               # File để gửi đi
    ├── 📄 Tai_lieu.pptx
    └── 📄 De_cuong.pdf
```

### Máy Học sinh (Client)

```
📁 Desktop\ClassroomFiles\
├── 📄 Tai_lieu_bai_giang.pptx
└── 📄 De_cuong.pdf
```

---

## Giới hạn

| Thông số | Giá trị |
|----------|---------|
| Max file size | 100 MB |
| Max files/lần | 20 |
| Tốc độ | ~10 MB/s (LAN) |

---

## Xử lý lỗi

### File quá lớn

```
Lỗi: "File vượt quá giới hạn 100MB"

Giải pháp:
1. Nén file (ZIP/RAR)
2. Chia nhỏ file
3. Sử dụng USB/Cloud
```

### Truyền thất bại

```
Lỗi: "Không thể gửi file"

Giải pháp:
1. Kiểm tra kết nối mạng
2. Thử lại
3. Restart ứng dụng
```

---

## Best Practices

1. **Đặt tên file rõ ràng**: `NguyenVanAn_BaiTap1.docx`
2. **Nén file lớn**: Giảm thời gian truyền
3. **Kiểm tra trước**: Xem file có mở được không
4. **Backup**: Lưu bản sao cục bộ
