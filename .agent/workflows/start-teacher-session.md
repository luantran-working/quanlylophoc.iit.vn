---
description: Quy trình khởi động phiên làm việc cho Giáo viên (Server)
---

# Quy trình Khởi động Phiên Giáo viên

## Mục tiêu

Khởi động ứng dụng với vai trò Giáo viên, đăng nhập và sẵn sàng nhận kết nối từ học sinh.

## Điều kiện tiên quyết

- [ ] Máy tính đã kết nối mạng LAN/WiFi
- [ ] Ứng dụng ClassroomManagement đã được cài đặt
- [ ] Có tài khoản đăng nhập (mặc định: admin/123456)

## Các bước thực hiện

### Bước 1: Khởi động ứng dụng

```
1. Mở ứng dụng ClassroomManagement
   - Double-click icon trên Desktop
   - Hoặc tìm trong Start Menu

2. Chờ màn hình chọn vai trò hiển thị
```

### Bước 2: Chọn vai trò Giáo viên

```
1. Tại màn hình "Quản lý phòng học thông minh IIT"

2. Click vào thẻ "Giáo viên" (bên trái)
   ┌─────────────────┐
   │    👨‍🏫           │
   │   GIÁO VIÊN     │
   │                 │
   │ [Đăng nhập GV]  │
   └─────────────────┘

3. Hoặc nhấn phím tắt: Alt + T
```

### Bước 3: Đăng nhập

```
1. Cửa sổ đăng nhập hiển thị

2. Nhập thông tin:
   - Tên đăng nhập: admin
   - Mật khẩu: 123456

3. Click "Đăng nhập"
   (Hoặc nhấn Enter)

4. Nếu đăng nhập lần đầu:
   - Hệ thống yêu cầu đổi mật khẩu
   - Nhập mật khẩu mới 2 lần
   - Click "Xác nhận"
```

### Bước 4: Cấu hình phiên học (Tùy chọn)

```
1. Popup cấu hình hiển thị (lần đầu hoặc khi được bật):

   ┌────────────────────────────────────────┐
   │  Cấu hình phiên học                    │
   │                                        │
   │  Tên lớp: [Lớp 10A1              ]    │
   │  Môn học: [Toán học          ▼]       │
   │                                        │
   │         [Bắt đầu] [Bỏ qua]            │
   └────────────────────────────────────────┘

2. Nhập tên lớp và chọn môn học

3. Click "Bắt đầu"
```

### Bước 5: Server khởi động

```
1. Hệ thống tự động:
   ├── Khởi tạo database (nếu chưa có)
   ├── Mở port TCP 5000 để lắng nghe
   ├── Bắt đầu phát UDP Broadcast
   └── Hiển thị Main Teacher Window

2. Status bar hiển thị:
   "🟢 Server đang chạy | Chờ kết nối..."

3. Sẵn sàng nhận học sinh
```

### Bước 6: Chờ học sinh kết nối

```
1. Danh sách học sinh ban đầu trống

2. Khi học sinh kết nối:
   ├── Tên hiển thị trong sidebar trái
   ├── Trạng thái: 🟢 Online
   ├── Thumbnail màn hình trong grid
   └── Thông báo popup (nếu bật)

3. Status bar cập nhật:
   "🟢 5/30 học sinh online"
```

## Xác nhận thành công

- [ ] Main Teacher Window hiển thị
- [ ] Status bar cho thấy "Server đang chạy"
- [ ] Có thể thấy học sinh khi họ kết nối
- [ ] Các công cụ bên sidebar hoạt động

## Xử lý lỗi

### Lỗi đăng nhập

```
Triệu chứng: "Sai tên đăng nhập hoặc mật khẩu"

Giải pháp:
1. Kiểm tra lại username/password
2. Mặc định: admin / 123456
3. Nếu đã đổi mật khẩu, liên hệ admin reset
```

### Lỗi "Port đang được sử dụng"

```
Triệu chứng: "Không thể khởi động server"

Giải pháp:
1. Kiểm tra có instance khác đang chạy
2. Mở Task Manager → End "ClassroomManagement"
3. Restart ứng dụng
```

### Lỗi mạng

```
Triệu chứng: Học sinh không thể kết nối

Giải pháp:
1. Kiểm tra kết nối mạng
2. Xem workflow: network-connection.md
3. Kiểm tra Firewall
```

## Kết thúc phiên

Xem workflow: `/end-teacher-session`
