
# Feature 6: Bulk File Send - Implementation Summary

## Overview
Tính năng "Gửi File Hàng loạt" cho phép giáo viên gửi tài liệu học tập tới toàn bộ hoặc một nhóm học sinh được chọn, với tốc độ cao và theo dõi tiến trình.

## Components Implemented
1.  **Models (`Models/BulkFileModels.cs`)**
    -   `BulkFileTransferRequest`: Metadata của file (Tên, Size, Type).
    -   `BulkFileDataChunk`: Các gói dữ liệu nhỏ dùng để truyền tải.

2.  **Services**
    -   `Services/BulkFileSender.cs` (Server): Chia nhỏ file thành các chunk 48KB, gửi multicast tới danh sách học sinh. Quản lý tốc độ gửi để tránh nghẽn mạng.
    -   `Services/FileReceiverService.cs` (Client): Nhận request, tạo file tạm, nhận chunks, ghép file và lưu vào thư mục Downloads.

3.  **UI - Teacher (`Views/BulkFileSendWindow.xaml`)**
    -   Chọn file từ máy tính.
    -   Danh sách học sinh online với Checkbox để chọn người nhận.
    -   Thanh tiến trình (ProgressBar) hiển thị trạng thái gửi.

4.  **UI - Student (`Views/FileNotificationPopup.xaml`)**
    -   Popup góc màn hình thông báo có file được gửi đến.
    -   Nút "Chấp nhận" hoặc "Từ chối".

5.  **Integration**
    -   **MainTeacherWindow**: Thêm nút "Gửi File" vào sidebar.
    -   **Notification**: Sử dụng Toast hoặc Popup để báo trạng thái hoàn thành.

## How to Verify
1.  **Start Teacher App**: Mở `MainTeacherWindow`, click nút "Gửi File".
2.  **Select File & Students**: Chọn một file (ví dụ PDF, Image), chọn các học sinh trong danh sách -> Bấm "Gửi Ngay".
3.  **Student Side**: Học sinh nhận Popup -> Bấm "Chấp nhận".
4.  **Transfer**: Quan sát thanh tiến trình trên máy Teacher chạy đến 100%.
5.  **Completion**: File xuất hiện trong thư mục `Downloads` của học sinh.

## Notes
-   Cơ chế Chunking đảm bảo gửi được các file lớn mà không gây treo ứng dụng.
-   File tên trùng sẽ tự động được đổi tên (ví dụ: `Tailieu(1).pdf`).
