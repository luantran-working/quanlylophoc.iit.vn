# Phần mềm Quản lý Phòng học Thông minh IIT

## Giới thiệu

Phần mềm Quản lý Phòng học Thông minh IIT là giải pháp toàn diện cho việc quản lý và điều khiển lớp học trong môi trường mạng LAN. Phần mềm cho phép giáo viên giám sát, điều khiển và tương tác với máy tính của học sinh trong cùng một mạng nội bộ.

## Tính năng chính

### 🖥️ Giám sát màn hình

- Xem màn hình tất cả học sinh theo thời gian thực
- Hỗ trợ nhiều chế độ hiển thị: 2x2, 4x4, 6x6

### 🎯 Điều khiển từ xa

- Điều khiển trực tiếp máy tính học sinh
- Hỗ trợ chuột, bàn phím và truyền file

### 📺 Chia sẻ màn hình

- Trình chiếu màn hình giáo viên đến tất cả học sinh
- Hỗ trợ chia sẻ toàn màn hình, cửa sổ hoặc vùng chọn

### 💬 Chat & Giao tiếp

- Chat nhóm toàn lớp
- Chat riêng với từng học sinh
- Thông báo và phản hồi nhanh

### 📁 Quản lý tập tin

- Thu bài từ học sinh
- Gửi tài liệu đến học sinh
- Quản lý thư mục chia sẻ

### 📝 Bài kiểm tra

- Tạo bài kiểm tra trắc nghiệm
- Tự động chấm điểm
- Thống kê kết quả

### 🔒 Quản lý máy tính

- Khóa/mở khóa máy học sinh
- Tắt mic/camera từ xa
- Giám sát hoạt động

## Yêu cầu hệ thống

### Máy Giáo viên (Server)

- Windows 10/11
- .NET 10.0 Runtime
- RAM: Tối thiểu 4GB
- Kết nối mạng LAN/WiFi

### Máy Học sinh (Client)

- Windows 10/11
- .NET 10.0 Runtime
- RAM: Tối thiểu 2GB
- Kết nối cùng mạng LAN với máy giáo viên

## Cấu hình mạng

Phần mềm hoạt động trên **mạng LAN nội bộ**:

- Tất cả máy tính phải kết nối cùng một mạng WiFi hoặc Ethernet
- Máy giáo viên đóng vai trò **Server** lưu trữ dữ liệu
- Máy học sinh tự động phát hiện và kết nối đến Server

## Tài khoản mặc định

| Vai trò           | Tên đăng nhập | Mật khẩu |
| ----------------- | ------------- | -------- |
| Giáo viên (Admin) | `admin`       | `123456` |

> ⚠️ **Lưu ý**: Vui lòng đổi mật khẩu sau lần đăng nhập đầu tiên.

## Cấu trúc tài liệu

- [Kiến trúc hệ thống](./ARCHITECTURE.md)
- [Cơ sở dữ liệu](./DATABASE.md)
- **Hướng dẫn tính năng:**
  - [Chế độ Giáo viên](./Features/teacher-mode.md)
  - [Chế độ Học sinh](./Features/student-mode.md)
  - [Chia sẻ màn hình](./Features/screen-sharing.md)
  - [Điều khiển từ xa](./Features/remote-control.md)
  - [Chat & Giao tiếp](./Features/chat.md)
  - [Truyền tập tin](./Features/file-transfer.md)
  - [Bài kiểm tra](./Features/test-creation.md)
  - [Bảng trắng](./Features/whiteboard.md)

## Hỗ trợ

- **Website**: https://quanlylophoc.iit.vn
- **Email**: support@iit.vn

---

_Phiên bản: 1.0.0 | Cập nhật: Tháng 01/2026_
