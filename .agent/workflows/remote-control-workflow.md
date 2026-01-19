---
description: Quy trình điều khiển máy tính học sinh từ xa
---

# Quy trình Điều khiển từ xa

## Mục tiêu

Điều khiển trực tiếp máy tính học sinh thông qua mạng LAN để hỗ trợ hoặc hướng dẫn.

---

## Workflow: Điều khiển từ xa (Giáo viên)

### Bước 1: Chọn máy học sinh

**Cách 1: Từ danh sách học sinh**

```
1. Trong sidebar trái, tìm học sinh

2. Click phải vào tên học sinh

3. Chọn "Điều khiển từ xa" từ menu:
   ┌─────────────────────────┐
   │ 👁️ Xem màn hình         │
   │ 🖱️ Điều khiển từ xa     │ ← Click
   │ 💬 Chat riêng           │
   │ 🔒 Khóa máy             │
   └─────────────────────────┘
```

**Cách 2: Từ grid màn hình**

```
1. Vào Tab "Màn hình học sinh"

2. Tìm thumbnail của học sinh cần điều khiển

3. Double-click vào thumbnail
```

### Bước 2: Kết nối

```
1. Cửa sổ điều khiển mở ra

2. Hiển thị:
   "Đang kết nối đến Nguyễn Văn An..."

   ⏳

3. Chờ kết nối (2-5 giây)

4. Học sinh nhận thông báo:
   "⚠️ Giáo viên đang điều khiển máy của bạn"
```

### Bước 3: Điều khiển

```
1. Màn hình học sinh hiển thị trong cửa sổ

   ┌──────────────────────────────────────────────┐
   │ Điều khiển: Nguyễn Văn An              ─ □ ×│
   ├──────────────────────────────────────────────┤
   │ [🔙][🔄][⌨️][📷][📁][🔒 Khóa][⛶ Full]       │
   ├──────────────────────────────────────────────┤
   │                                              │
   │                                              │
   │         (Màn hình học sinh)                  │
   │                                              │
   │                                              │
   ├──────────────────────────────────────────────┤
   │ 🟢 Kết nối │ FPS: 30 │ Latency: 15ms        │
   └──────────────────────────────────────────────┘

2. Bắt đầu điều khiển:

   CHUỘT:
   ├── Di chuyển → Con trỏ di chuyển
   ├── Click trái → Click
   ├── Click phải → Right-click
   ├── Double-click → Double-click
   └── Scroll → Cuộn trang

   BÀN PHÍM:
   ├── Gõ phím → Nhập ký tự
   ├── Ctrl+C/V → Copy/Paste
   ├── Alt+Tab → Chuyển cửa sổ
   └── Các phím khác...
```

### Bước 4: Khóa input học sinh (Tùy chọn)

```
Khi muốn điều khiển mà không bị can thiệp:

1. Click nút [🔒 Khóa HS] trên toolbar

2. Input của học sinh bị vô hiệu hóa:
   - Chuột: Không di chuyển được
   - Bàn phím: Không gõ được

3. Chỉ giáo viên có thể điều khiển

4. Học sinh thấy thông báo:
   "Giáo viên đang điều khiển"

5. Click [🔓 Mở khóa] để cho phép lại
```

### Bước 5: Sử dụng công cụ

```
TOOLBAR:

[🔙] Quay lại     ← Kết thúc điều khiển
[🔄] Refresh      ← Làm mới kết nối
[⌨️] Keyboard     ← Bàn phím ảo (gửi Ctrl+Alt+Del...)
[📷] Screenshot   ← Chụp màn hình học sinh
[📁] Send File    ← Gửi file trong khi điều khiển
[🔒] Khóa HS      ← Khóa/mở khóa input
[⛶] Fullscreen   ← Chế độ toàn màn hình
```

### Bước 6: Chụp màn hình (Tùy chọn)

```
1. Click [📷 Screenshot]

2. Ảnh được lưu tại:
   %LOCALAPPDATA%\IIT\...\Screenshots\
   └── NguyenVanAn_2026-01-16_10-30-45.png

3. Thông báo: "Đã lưu ảnh chụp màn hình"
```

### Bước 7: Gửi file (Tùy chọn)

```
1. Click [📁 Send File]

2. Chọn file cần gửi

3. File được truyền đến Desktop học sinh

4. Có thể mở file ngay từ điều khiển
```

### Bước 8: Kết thúc điều khiển

```
1. Click [🔙 Quay lại] trên toolbar

2. Hoặc nhấn Esc (nếu đang fullscreen)

3. Hoặc đóng cửa sổ điều khiển

4. Ngắt kết nối điều khiển

5. Học sinh nhận thông báo:
   "Giáo viên đã ngừng điều khiển"

6. Input học sinh hoạt động bình thường
```

---

## Chế độ xem

### View Mode (Chỉ xem)

```
1. Click phải → "Xem màn hình" (thay vì Điều khiển)

2. Hoặc nhấn phím V trong khi điều khiển

3. Chỉ xem, không điều khiển

4. Học sinh không biết đang bị xem
```

### Control Mode (Điều khiển)

```
1. Mặc định khi mở "Điều khiển từ xa"

2. Xem và điều khiển đầy đủ

3. Học sinh được thông báo
```

---

## Phím tắt

| Phím       | Chức năng                       |
| ---------- | ------------------------------- |
| `Esc`      | Thoát fullscreen / Ngắt kết nối |
| `F11`      | Toggle toàn màn hình            |
| `Ctrl + R` | Refresh kết nối                 |
| `Ctrl + S` | Chụp màn hình                   |
| `Ctrl + L` | Khóa/Mở khóa HS                 |
| `V`        | Toggle View/Control mode        |

---

## Xử lý lỗi

### Không điều khiển được

```
Nguyên nhân:
- Firewall block
- Mất kết nối mạng
- Ứng dụng HS bị crash

Giải pháp:
1. Click [🔄 Refresh]
2. Kiểm tra học sinh còn online
3. Yêu cầu học sinh restart app
```

### Điều khiển lag

```
Nguyên nhân:
- Mạng chậm
- CPU cao

Giải pháp:
1. Giảm chất lượng: Settings → Quality
2. Đóng ứng dụng không cần thiết
3. Sử dụng Ethernet thay WiFi
```

---

## Lưu ý bảo mật

> ⚠️ **Quan trọng**:
>
> - Chỉ điều khiển khi thực sự cần thiết
> - Học sinh luôn được thông báo khi bị điều khiển
> - Tránh truy cập thông tin cá nhân
> - Tuân thủ quy định về quyền riêng tư
> - Ghi nhận log mọi phiên điều khiển
