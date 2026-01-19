# Hướng dẫn sử dụng Tính năng Điều khiển Từ xa (Remote Control)

## 1. Giới thiệu

Tính năng Điều khiển Từ xa cho phép giáo viên kết nối trực tiếp đến máy tính của học sinh để:

- Xem màn hình học sinh theo thời gian thực.
- Điều khiển chuột và bàn phím của học sinh (như TeamViewer/UltraView).
- Khóa chuột/bàn phím của học sinh.
- Chụp ảnh màn hình máy học sinh.

## 2. Cách sử dụng

### 2.1. Truy cập

Có 2 cách để mở cửa sổ điều khiển từ xa:

1. **Từ danh sách học sinh (Card View):**
   - Chuột phải vào thẻ học sinh.
   - Chọn **"Điều khiển từ xa"** từ menu.

2. **Từ màn hình giám sát (Thumbnail View):**
   - Chuột phải vào màn hình thu nhỏ của học sinh.
   - Chọn **"Điều khiển từ xa"** từ menu.

### 2.2. Giao diện Điều khiển

Cửa sổ điều khiển bao gồm:

- **Màn hình chính:** Hiển thị màn hình máy học sinh.
- **Thanh công cụ (Toolbar):**
  - **Nút "Điều khiển" (Toggle):** Bật/Tắt chế độ điều khiển chuột/phím. Nếu tắt, chỉ xem màn hình.
  - **Nút "Khóa" (Lock):** Khóa chuột và bàn phím của học sinh (học sinh không thể thao tác).
  - **Nút "Làm mới" (Refresh):** Yêu cầu cập nhật lại màn hình nếu bị lag.
  - **Nút "Bàn phím ảo":** Gửi các phím đặc biệt (Ctrl+Alt+Del, Start Menu, Alt+Tab...).
  - **Nút "Chụp ảnh" (Camera):** Chụp màn hình hiện tại và lưu thành file ảnh.
  - **Chất lượng:** Điều chỉnh chất lượng hình ảnh (Thấp, Trung bình, Cao) để tối ưu băng thông.

### 2.3. Thao tác điều khiển

- **Chuột:** Di chuyển, click trái, phải, giữa, cuộn chuột trên cửa sổ sẽ được gửi đến máy học sinh.
- **Bàn phím:** Các phím nhấn sẽ được gửi đi (trừ các phím tắt hệ thống của máy giáo viên như Alt+Tab của giáo viên).
- **Phím tắt:**
  - `F11`: Chế độ toàn màn hình.
  - `Esc`: Thoát chế độ toàn màn hình hoặc đóng cửa sổ (cần xác nhận).
  - `Ctrl + S`: Chụp màn hình nhanh.

## 3. Lưu ý kỹ thuật

- **Độ trễ (Latency):** Phụ thuộc vào tốc độ mạng LAN. Khuyến nghị sử dụng kết nối dây (Ethernet) để có trải nghiệm tốt nhất.
- **Quyền hạn:** Máy học sinh cần cấp quyền (mặc định phần mềm Client sẽ tự động chấp nhận kết nối từ Giáo viên).
- **Bảo mật:** Chỉ giáo viên mới có thể yêu cầu điều khiển.

---

_Tài liệu này dành cho Giáo viên sử dụng phần mềm Quản lý Lớp học._
