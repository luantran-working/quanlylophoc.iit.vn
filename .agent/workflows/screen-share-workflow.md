---
description: Quy trình chia sẻ màn hình từ máy giáo viên đến học sinh
---

# Quy trình Chia sẻ Màn hình

## Mục tiêu

Trình chiếu nội dung từ màn hình giáo viên đến tất cả máy học sinh.

## Điều kiện tiên quyết

- [ ] Đã đăng nhập với vai trò Giáo viên
- [ ] Có ít nhất 1 học sinh online
- [ ] Nội dung cần share đã sẵn sàng

## Các bước thực hiện

### Bước 1: Vào Tab Trình chiếu

```
1. Trong Main Teacher Window

2. Click tab "Trình chiếu" (Tab thứ 3)
   [Màn hình] [Điều khiển] [Trình chiếu] [Bảng trắng]
                              ▲
                              │
                    ──────────┘

3. Hoặc nhấn phím tắt: Ctrl + 3
```

### Bước 2: Chọn nội dung chia sẻ

```
1. Trong tab Trình chiếu, click "Bắt đầu chia sẻ"

2. Popup hiển thị 3 tùy chọn:

   ┌────────────────────────────────────────┐
   │  Chọn nội dung chia sẻ                 │
   │                                        │
   │  ┌────────────┐  ┌────────────┐       │
   │  │ 🖥️         │  │ 🪟         │       │
   │  │ Toàn màn   │  │ Cửa sổ     │       │
   │  │ hình       │  │            │       │
   │  └────────────┘  └────────────┘       │
   │                                        │
   │  ┌────────────┐                       │
   │  │ ▢          │                       │
   │  │ Vùng chọn  │                       │
   │  │            │                       │
   │  └────────────┘                       │
   │                                        │
   │              [Hủy]                     │
   └────────────────────────────────────────┘
```

### Bước 3a: Chia sẻ toàn màn hình

```
1. Click "Toàn màn hình"

2. Nếu có nhiều màn hình:
   - Hiển thị danh sách màn hình
   - Chọn màn hình muốn share

3. Bắt đầu stream ngay lập tức
```

### Bước 3b: Chia sẻ cửa sổ

```
1. Click "Cửa sổ"

2. Danh sách cửa sổ đang mở hiển thị:
   ┌────────────────────────────────────────┐
   │  Chọn cửa sổ                           │
   │                                        │
   │  ☐ PowerPoint - Bài giảng Chương 1    │
   │  ☐ Chrome - Google Docs               │
   │  ☐ Word - Đề bài tập                  │
   │  ☐ Calculator                         │
   │                                        │
   │         [Chọn] [Hủy]                  │
   └────────────────────────────────────────┘

3. Click chọn cửa sổ muốn share

4. Click "Chọn"

5. Chỉ cửa sổ đó được stream
```

### Bước 3c: Chia sẻ vùng chọn

```
1. Click "Vùng chọn"

2. Toàn màn hình mờ đi với overlay

3. Kéo chuột để chọn vùng:
   ┌─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┐
   │                     │
   │   Vùng được chọn    │
   │                     │
   └─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─┘

4. Thả chuột để xác nhận

5. Chỉ vùng đó được stream
```

### Bước 4: Stream đang hoạt động

```
1. Cửa sổ Screen Share Window mở ra

2. Hiển thị:
   - Nội dung đang share (preview)
   - Toolbar annotation
   - Số học sinh đang xem
   - Nút Pause/Stop

3. Học sinh tự động nhận stream

4. Status: "🔴 LIVE • 25 học sinh đang xem"
```

### Bước 5: Sử dụng công cụ annotation

```
Trong khi đang share:

1. Bút vẽ (P):
   - Click icon ✏️
   - Chọn màu và độ dày
   - Vẽ trực tiếp lên nội dung share

2. Highlight (H):
   - Click icon 🖍️
   - Tô đậm vùng quan trọng

3. Laser pointer (L):
   - Click icon 🔦
   - Di chuyển laser để chỉ điểm

4. Xóa annotation:
   - Click icon 🗑️
   - Hoặc Ctrl + Z để undo
```

### Bước 6: Tạm dừng / Tiếp tục

```
Tạm dừng:
1. Click nút [⏸️ Pause]
2. Màn hình freeze
3. Học sinh thấy ảnh tĩnh
4. Giáo viên có thể làm việc khác

Tiếp tục:
1. Click nút [▶️ Resume]
2. Stream tiếp tục
```

### Bước 7: Kết thúc chia sẻ

```
1. Click nút [⏹️ Stop]

2. Popup xác nhận:
   "Bạn có chắc muốn dừng trình chiếu?"
   [Dừng] [Hủy]

3. Click "Dừng"

4. Stream kết thúc

5. Học sinh thấy:
   "Đang chờ giáo viên trình chiếu..."
```

## Phím tắt

| Phím  | Chức năng          |
| ----- | ------------------ |
| `F5`  | Bắt đầu/Dừng share |
| `F6`  | Pause/Resume       |
| `P`   | Bút vẽ             |
| `H`   | Highlight          |
| `L`   | Laser              |
| `C`   | Xóa annotation     |
| `Esc` | Thoát fullscreen   |

## Xử lý lỗi

### Học sinh không thấy stream

```
1. Kiểm tra học sinh online (sidebar)
2. Yêu cầu học sinh refresh
3. Kiểm tra bandwidth mạng
4. Giảm chất lượng stream: Settings → Quality
```

### Stream lag/giật

```
1. Giảm chất lượng: Settings → Quality → Low
2. Giảm FPS: Settings → Frame Rate → 15
3. Đóng ứng dụng không cần thiết
4. Kiểm tra mạng LAN
```

### Màn hình đen

```
1. Một số app có protected content
2. Thử share cửa sổ thay vì toàn màn hình
3. Restart ứng dụng
```

## Best Practices

1. **Chuẩn bị trước**: Mở sẵn tài liệu cần share
2. **Dùng cửa sổ mode**: Bảo vệ privacy
3. **Sử dụng annotation**: Highlight điểm quan trọng
4. **Pause khi cần**: Cho học sinh kịp ghi chép
5. **Kiểm tra preview**: Trước khi share
