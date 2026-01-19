# Chế độ Giáo viên

## Tổng quan

Chế độ Giáo viên là vai trò chính của phần mềm, đóng vai trò **Server** trong hệ thống. Người dùng với vai trò này có toàn quyền quản lý lớp học và điều khiển máy tính học sinh.

## Quyền hạn

| Chức năng            | Mô tả                             |
| -------------------- | --------------------------------- |
| ✅ Giám sát màn hình | Xem màn hình tất cả học sinh      |
| ✅ Điều khiển từ xa  | Điều khiển trực tiếp máy học sinh |
| ✅ Chia sẻ màn hình  | Trình chiếu đến tất cả học sinh   |
| ✅ Khóa/Mở khóa máy  | Khóa màn hình học sinh            |
| ✅ Tắt mic/camera    | Quản lý thiết bị học sinh         |
| ✅ Chat nhóm/riêng   | Giao tiếp với học sinh            |
| ✅ Thu/Gửi file      | Quản lý tập tin                   |
| ✅ Tạo bài kiểm tra  | Tạo và chấm điểm                  |
| ✅ Bảng trắng        | Vẽ và chia sẻ                     |

## Giao diện chính

```
┌──────────────────────────────────────────────────────────────────┐
│  🎓 Quản lý Lớp học │ Lớp 10A1 - Toán học │ 🔔 ⚙️ 👤            │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ ┌─────────────┐ ┌─────────────────────────────┐ ┌──────────────┐│
│ │ DANH SÁCH   │ │                             │ │  CÔNG CỤ     ││
│ │ HỌC SINH    │ │   [Màn hình] [Điều khiển]   │ │              ││
│ │             │ │   [Trình chiếu] [Bảng trắng]│ │  Chat nhóm   ││
│ │ 🟢 Nguyễn A │ │                             │ │  Chat riêng  ││
│ │ 🟢 Trần B   │ │   ┌─────┐ ┌─────┐ ┌─────┐   │ │  Thu bài     ││
│ │ 🔴 Lê C     │ │   │ HS1 │ │ HS2 │ │ HS3 │   │ │  Khóa máy    ││
│ │ 🟢 Phạm D   │ │   └─────┘ └─────┘ └─────┘   │ │              ││
│ │             │ │   ┌─────┐ ┌─────┐ ┌─────┐   │ │  ────────    ││
│ │ [Tìm kiếm]  │ │   │ HS4 │ │ HS5 │ │ HS6 │   │ │  Tạo bài KT  ││
│ │             │ │   └─────┘ └─────┘ └─────┘   │ │  Trò chơi    ││
│ │ [Chọn hết]  │ │                             │ │  Thống kê    ││
│ │ [Bỏ chọn]   │ │                             │ │              ││
│ └─────────────┘ └─────────────────────────────┘ └──────────────┘│
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│  🟢 25/30 học sinh online │ 📶 Kết nối ổn định │ ⚙️ Cài đặt    │
└──────────────────────────────────────────────────────────────────┘
```

## Các khu vực chính

### 1. Header (Thanh tiêu đề)

| Thành phần    | Chức năng                          |
| ------------- | ---------------------------------- |
| Logo & Tên    | Hiển thị tên phần mềm và vai trò   |
| Thông tin lớp | Tên lớp và môn học đang dạy        |
| Thông báo     | Hiển thị thông báo mới từ học sinh |
| Cài đặt       | Mở cửa sổ cài đặt                  |
| Tài khoản     | Thông tin và đăng xuất             |

### 2. Sidebar trái - Danh sách học sinh

Hiển thị danh sách tất cả học sinh đang kết nối:

- **Trạng thái online**: 🟢 Online, 🔴 Offline
- **Tên học sinh**: Tên hiển thị
- **Nút chức năng**: Camera, Mic, Khóa, Chat
- **Tìm kiếm**: Lọc theo tên
- **Chọn nhanh**: Chọn tất cả / Bỏ chọn

### 3. Khu vực chính - Tab Control

#### Tab Màn hình học sinh

- Hiển thị thumbnail màn hình các học sinh
- Hỗ trợ layout: 2x2, 4x4, 6x6
- Click để xem chi tiết
- Double-click để điều khiển

#### Tab Điều khiển từ xa

- Màn hình điều khiển full-size
- Toolbar: Chuột, Bàn phím, Gửi file
- Nút ngắt kết nối

#### Tab Trình chiếu

- Chia sẻ màn hình đến học sinh
- Chọn: Toàn màn hình / Cửa sổ / Vùng chọn
- Hiển thị số học sinh đang xem

#### Tab Bảng trắng

- Vẽ và ghi chú
- Công cụ: Bút, Highlight, Shapes
- Chia sẻ bảng trắng đến học sinh

### 4. Sidebar phải - Công cụ

| Nhóm          | Công cụ                              |
| ------------- | ------------------------------------ |
| **Giao tiếp** | Chat nhóm, Chat riêng                |
| **Quản lý**   | Thu bài, Khóa máy, Tắt mic/camera    |
| **Học tập**   | Tạo bài kiểm tra, Trò chơi, Thống kê |

### 5. Status Bar (Thanh trạng thái)

- Số học sinh online
- Trạng thái kết nối mạng
- Nút cài đặt và thoát

## Quy trình sử dụng

### Bắt đầu phiên học

```
1. Mở ứng dụng
        │
        ▼
2. Chọn "Giáo viên" tại màn hình chọn vai trò
        │
        ▼
3. Đăng nhập (admin / 123456)
        │
        ▼
4. Server khởi động, phát broadcast
        │
        ▼
5. Chờ học sinh kết nối
        │
        ▼
6. Bắt đầu giảng dạy
```

### Giám sát lớp học

```
1. Vào Tab "Màn hình học sinh"
        │
        ▼
2. Chọn layout phù hợp (4x4 mặc định)
        │
        ▼
3. Xem thumbnail các máy học sinh
        │
        ▼
4. Click vào thumbnail để zoom
        │
        ▼
5. Double-click để điều khiển từ xa
```

### Trình chiếu bài giảng

```
1. Vào Tab "Trình chiếu"
        │
        ▼
2. Click "Bắt đầu chia sẻ màn hình"
        │
        ▼
3. Chọn nội dung chia sẻ:
   - Toàn màn hình
   - Cửa sổ cụ thể
   - Vùng chọn
        │
        ▼
4. Học sinh tự động nhận stream
        │
        ▼
5. Click "Dừng trình chiếu" khi xong
```

### Kết thúc phiên học

```
1. Đảm bảo đã thu bài (nếu có)
        │
        ▼
2. Gửi thông báo kết thúc (tùy chọn)
        │
        ▼
3. Click "Thoát" ở Status Bar
        │
        ▼
4. Xác nhận kết thúc phiên
        │
        ▼
5. Tất cả học sinh bị ngắt kết nối
```

## Phím tắt

| Phím tắt   | Chức năng                  |
| ---------- | -------------------------- |
| `Ctrl + 1` | Tab Màn hình               |
| `Ctrl + 2` | Tab Điều khiển             |
| `Ctrl + 3` | Tab Trình chiếu            |
| `Ctrl + 4` | Tab Bảng trắng             |
| `Ctrl + L` | Khóa tất cả máy            |
| `Ctrl + U` | Mở khóa tất cả máy         |
| `Ctrl + M` | Tắt mic tất cả             |
| `F5`       | Làm mới danh sách          |
| `Esc`      | Thoát chế độ toàn màn hình |

## Xử lý sự cố

| Vấn đề                  | Giải pháp                    |
| ----------------------- | ---------------------------- |
| Học sinh không hiển thị | Kiểm tra kết nối mạng LAN    |
| Màn hình học sinh đen   | Yêu cầu học sinh restart app |
| Không điều khiển được   | Kiểm tra firewall            |
| Trình chiếu lag         | Giảm chất lượng stream       |

---

_Xem thêm: [Workflows - Khởi động phiên Giáo viên](../../.agent/workflows/start-teacher-session.md)_
