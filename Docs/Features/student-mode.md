# Chế độ Học sinh

## Tổng quan

Chế độ Học sinh là vai trò **Client** trong hệ thống. Máy tính học sinh tự động phát hiện và kết nối đến máy giáo viên (Server) khi cùng nằm trong một mạng LAN.

## Quyền hạn

| Chức năng | Mô tả |
|-----------|-------|
| ✅ Xem trình chiếu | Xem màn hình giáo viên chia sẻ |
| ✅ Chat với giáo viên | Gửi tin nhắn, đặt câu hỏi |
| ✅ Giơ tay | Yêu cầu phát biểu |
| ✅ Gửi file | Nộp bài tập cho giáo viên |
| ✅ Làm bài kiểm tra | Tham gia bài kiểm tra online |
| ✅ Ghi chú | Ghi chú cá nhân |
| ⚠️ Bị giám sát | Màn hình có thể bị xem |
| ⚠️ Bị điều khiển | Có thể bị điều khiển từ xa |
| ⚠️ Bị khóa máy | Có thể bị khóa màn hình |

## Giao diện chính

```
┌──────────────────────────────────────────────────────────────────┐
│  🎓 Phòng học trực tuyến │ Nguyễn Văn An - Lớp 10A1 │ [Phản hồi]│
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│ ┌────────────────────────────────────────┐ ┌───────────────────┐│
│ │  MÀN HÌNH TRÌNH CHIẾU                  │ │  CÔNG CỤ HỌC TẬP  ││
│ │  ┌─────────────────────────────┐       │ │                   ││
│ │  │                             │       │ │  [Gửi file cho GV]││
│ │  │                             │       │ │                   ││
│ │  │    Đang chờ giáo viên       │       │ │  [GIƠ TAY]  🔘    ││
│ │  │       trình chiếu...        │       │ │                   ││
│ │  │                             │       │ │  [Chat với GV]    ││
│ │  │           ⏳                 │       │ │                   ││
│ │  │                             │       │ │  ─────────────    ││
│ │  └─────────────────────────────┘       │ │  BÀI KIỂM TRA     ││
│ │                                        │ │  Kiểm tra 15p     ││
│ │  ℹ️ GV: Trần Văn Bình đang điều khiển  │ │  ⏱️ Còn lại: 14:32 ││
│ │                         [⛶] [⛶]       │ │  Tiến độ: 8/10    ││
│ └────────────────────────────────────────┘ │                   ││
│                                            │  ─────────────    ││
│                                            │  GHI CHÚ NHANH    ││
│                                            │  [___________]    ││
│                                            └───────────────────┘│
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│  📷 Cam: Bật │ 🎤 Mic: Tắt │ 🟢 Kết nối ổn định │ [Yêu cầu TG] │
└──────────────────────────────────────────────────────────────────┘
```

## Các khu vực chính

### 1. Header (Thanh tiêu đề)

| Thành phần | Chức năng |
|------------|-----------|
| Logo & Tên | Hiển thị tên phần mềm |
| Thông tin HS | Tên học sinh, lớp, môn học |
| Nút Phản hồi | Gửi phản hồi nhanh cho giáo viên |

### 2. Khu vực chính - Màn hình trình chiếu

- **Trạng thái chờ**: Hiển thị khi GV chưa share màn hình
- **Đang trình chiếu**: Hiển thị màn hình của giáo viên
- **Thanh điều khiển**: 
  - Thông tin giáo viên
  - Nút phóng to / thu nhỏ

### 3. Sidebar phải - Công cụ học tập

#### Gửi file cho GV
- Nộp bài tập, file cho giáo viên
- Hỗ trợ kéo thả file

#### Giơ tay
- Toggle để yêu cầu phát biểu
- Giáo viên sẽ nhận thông báo

#### Chat với GV
- Chat nhóm với cả lớp
- Chat riêng với giáo viên

#### Bài kiểm tra
- Hiển thị bài kiểm tra đang làm
- Thời gian còn lại
- Tiến độ hoàn thành

#### Ghi chú nhanh
- Ghi chú cá nhân
- Lưu tự động

### 4. Status Bar (Thanh trạng thái)

| Thành phần | Mô tả |
|------------|-------|
| Camera | Trạng thái camera (Bật/Tắt) |
| Microphone | Trạng thái micro (Bật/Tắt) |
| Kết nối | Trạng thái kết nối với Server |
| Yêu cầu TG | Nút yêu cầu trợ giúp khẩn cấp |

## Quy trình sử dụng

### Kết nối vào lớp học

```
1. Mở ứng dụng
        │
        ▼
2. Chọn "Học sinh" tại màn hình chọn vai trò
        │
        ▼
3. Nhập tên hiển thị (nếu yêu cầu)
        │
        ▼
4. Ứng dụng tự động tìm Server trong mạng LAN
        │
        ▼
5. Kết nối thành công ──► Vào phòng học
        │
   Không tìm thấy ──► Hiển thị thông báo, thử lại
```

### Xem bài giảng

```
1. Chờ giáo viên bắt đầu trình chiếu
        │
        ▼
2. Màn hình tự động hiển thị nội dung share
        │
        ▼
3. Sử dụng nút phóng to để xem rõ hơn
        │
        ▼
4. Ghi chú nếu cần
```

### Giơ tay phát biểu

```
1. Click vào toggle "GIƠ TAY"
        │
        ▼
2. Toggle chuyển sang ON
        │
        ▼
3. Giáo viên nhận thông báo
        │
        ▼
4. Chờ giáo viên cho phép
        │
        ▼
5. Phát biểu xong ──► Tắt toggle
```

### Làm bài kiểm tra

```
1. Giáo viên gửi bài kiểm tra
        │
        ▼
2. Popup thông báo xuất hiện
        │
        ▼
3. Click "Bắt đầu làm bài"
        │
        ▼
4. Đọc câu hỏi và chọn đáp án
        │
        ▼
5. Di chuyển giữa các câu
        │
        ▼
6. Click "Nộp bài" khi hoàn thành
   (hoặc tự động nộp khi hết giờ)
```

### Gửi file cho giáo viên

```
1. Click "Gửi file cho GV"
        │
        ▼
2. Chọn file từ máy tính
        │
        ▼
3. Xác nhận gửi
        │
        ▼
4. Chờ upload hoàn tất
        │
        ▼
5. Nhận thông báo "Gửi thành công"
```

## Trạng thái máy tính

### Trạng thái bình thường
- Màn hình hoạt động bình thường
- Có thể sử dụng các chức năng

### Bị khóa màn hình
```
┌──────────────────────────────────────┐
│                                      │
│           🔒 MÁY ĐANG BỊ KHÓA        │
│                                      │
│     Vui lòng chờ giáo viên mở khóa   │
│                                      │
│              ● ● ● ●                 │
│                                      │
└──────────────────────────────────────┘
```
- Không thể thao tác
- Chỉ giáo viên có thể mở khóa

### Đang bị điều khiển
- Hiển thị thông báo "GV đang điều khiển"
- Chuột/bàn phím bị vô hiệu hóa (tùy cấu hình)

## Thông báo

| Loại | Mô tả |
|------|-------|
| 💬 | Tin nhắn mới từ giáo viên |
| 📄 | File mới từ giáo viên |
| 📝 | Bài kiểm tra mới |
| 🔒 | Máy bị khóa/mở khóa |
| 📢 | Thông báo từ giáo viên |

## Xử lý sự cố

| Vấn đề | Giải pháp |
|--------|-----------|
| Không tìm thấy Server | Kiểm tra kết nối cùng mạng WiFi |
| Mất kết nối | Đợi tự động kết nối lại |
| Màn hình share lag | Báo giáo viên |
| Không gửi được file | Kiểm tra dung lượng file |

## Lưu ý

> ⚠️ **Quan trọng**: 
> - Màn hình của bạn có thể được giáo viên xem bất cứ lúc nào
> - Không sử dụng máy tính vào mục đích cá nhân trong giờ học
> - Tuân thủ nội quy lớp học

---
*Xem thêm: [Workflows - Kết nối phiên Học sinh](../../.agent/workflows/start-student-session.md)*
