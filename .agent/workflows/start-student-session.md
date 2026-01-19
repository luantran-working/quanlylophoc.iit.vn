---
description: Quy trình kết nối vào phòng học cho Học sinh (Client)
---

# Quy trình Kết nối Phiên Học sinh

## Mục tiêu

Kết nối máy học sinh vào phòng học do giáo viên tạo.

## Điều kiện tiên quyết

- [ ] Máy tính đã kết nối cùng mạng LAN/WiFi với máy giáo viên
- [ ] Giáo viên đã khởi động Server
- [ ] Ứng dụng ClassroomManagement đã được cài đặt

## Các bước thực hiện

### Bước 1: Khởi động ứng dụng

```
1. Mở ứng dụng ClassroomManagement
   - Double-click icon trên Desktop
   - Hoặc tìm trong Start Menu

2. Chờ màn hình chọn vai trò hiển thị
```

### Bước 2: Chọn vai trò Học sinh

```
1. Tại màn hình "Quản lý phòng học thông minh IIT"

2. Click vào thẻ "Học sinh" (bên phải)
   ┌─────────────────┐
   │    👨‍🎓           │
   │   HỌC SINH      │
   │                 │
   │ [Đăng nhập HS]  │
   └─────────────────┘

3. Hoặc nhấn phím tắt: Alt + S
```

### Bước 3: Nhập thông tin (Nếu yêu cầu)

```
1. Popup nhập tên hiển thị:
   ┌────────────────────────────────────────┐
   │  Thông tin học sinh                    │
   │                                        │
   │  Họ và tên: [Nguyễn Văn An        ]   │
   │                                        │
   │         [Tiếp tục]                     │
   └────────────────────────────────────────┘

2. Nhập họ tên của bạn

3. Click "Tiếp tục"
```

### Bước 4: Tự động tìm Server

```
1. Ứng dụng bắt đầu tìm kiếm:
   ┌────────────────────────────────────────┐
   │                                        │
   │         Đang tìm phòng học...          │
   │                                        │
   │              ⏳                         │
   │                                        │
   │  Đảm bảo bạn đã kết nối cùng mạng      │
   │  WiFi với giáo viên.                   │
   │                                        │
   └────────────────────────────────────────┘

2. Lắng nghe UDP Broadcast từ Server

3. Thời gian tìm: Tối đa 30 giây
```

### Bước 5: Kết nối Server

```
Khi tìm thấy Server:

1. Hiển thị thông tin phòng học:
   ┌────────────────────────────────────────┐
   │  Tìm thấy phòng học!                   │
   │                                        │
   │  📚 Lớp 10A1 - Toán học                │
   │  👨‍🏫 GV: Trần Văn Bình                   │
   │                                        │
   │         [Vào phòng học]                │
   └────────────────────────────────────────┘

2. Click "Vào phòng học"

3. Hoặc tự động vào sau 5 giây
```

### Bước 6: Vào phòng học

```
1. Kết nối TCP được thiết lập

2. Gửi thông tin đăng ký:
   - Machine ID
   - Tên hiển thị
   - Computer Name

3. Nhận xác nhận từ Server

4. Student Window hiển thị

5. Sẵn sàng học tập!
```

## Xác nhận thành công

- [ ] Student Window hiển thị đầy đủ
- [ ] Status bar: "🟢 Đã kết nối"
- [ ] Có thể thấy trình chiếu (nếu GV đang share)
- [ ] Sidebar công cụ hoạt động

## Xử lý lỗi

### Không tìm thấy Server

```
Triệu chứng:
"Không tìm thấy phòng học sau 30 giây"

Giải pháp:
1. Kiểm tra kết nối mạng
   - Đảm bảo cùng WiFi với giáo viên
   - Kiểm tra: Settings → Network

2. Hỏi giáo viên:
   - Server đã khởi động chưa?
   - IP máy giáo viên là gì?

3. Thử kết nối thủ công (nếu có):
   - Click "Kết nối thủ công"
   - Nhập IP máy giáo viên
   - Click "Kết nối"

4. Restart ứng dụng và thử lại
```

### Kết nối bị từ chối

```
Triệu chứng:
"Không thể kết nối đến phòng học"

Giải pháp:
1. Hỏi giáo viên xem phòng học còn hoạt động
2. Firewall có thể block - báo IT/Admin
3. Thử restart ứng dụng
```

### Mất kết nối giữa chừng

```
Triệu chứng:
"Kết nối bị gián đoạn"

Ứng dụng tự động:
1. Thử kết nối lại (5 lần, mỗi lần 3 giây)
2. Nếu thành công → Tiếp tục phiên
3. Nếu thất bại → Quay về màn hình tìm kiếm

Nếu không tự kết nối được:
1. Kiểm tra mạng WiFi
2. Restart ứng dụng
3. Báo giáo viên
```

## Lưu ý

> ⚠️ **Quan trọng**:
>
> - Màn hình của bạn có thể bị giáo viên xem
> - Máy có thể bị điều khiển từ xa
> - Tuân thủ nội quy lớp học
