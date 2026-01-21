
# Feature 7: Realtime Polling - Implementation Summary

## Overview
Tính năng "Bình chọn Thời gian thực" cho phép giáo viên tạo các cuộc thăm dò ý kiến nhanh và nhận kết quả tức thì từ học sinh.

## Components Implemented
1.  **Models (`Models/PollModels.cs`)**
    -   `Poll`: Cấu trúc dữ liệu cho cuộc bình chọn (Câu hỏi, Các lựa chọn, Trạng thái).
    -   `PollVote`: Dữ liệu phiếu bầu từ học sinh.
    -   `PollResultUpdate`: Dữ liệu tổng hợp kết quả để cập nhật realtime.

2.  **Services (`Services/PollService.cs`)**
    -   Singleton Service quản lý trạng thái bình chọn.
    -   **Server-side**: Broadcast câu hỏi, nhận phiếu bầu, tính toán kết quả, broadcast kết quả cập nhật.
    -   **Client-side**: Nhận câu hỏi, hiển thị UI, gửi phiếu bầu.

3.  **UI - Teacher (`Views/CreatePollWindow.xaml`)**
    -   Giao diện tạo câu hỏi và thêm/bớt các lựa chọn tùy ý.
    -   Màn hình theo dõi kết quả trực quan với Progress Bar và tỉ lệ %.
    -   Nút "Kết thúc bình chọn" để đóng phiên.

4.  **UI - Student (`Views/VotePollWindow.xaml`)**
    -   Tự động hiển thị khi giáo viên bắt đầu bình chọn.
    -   Giao diện đơn giản, dễ thao tác để chọn và gửi đáp án.
    -   Chặn gửi nhiều lần.

5.  **Integration**
    -   **MainTeacherWindow**: Thêm nút "Bình chọn" vào sidebar.
    -   **StudentWindow**: Lắng nghe sự kiện `PollStarted` để mở cửa sổ bình chọn.
    -   **Network**: Thêm các MessageType mới (`PollStart`, `PollVote`, `PollStop`, `PollUpdate`).

## How to Verify
1.  **Start Teacher App**: Mở `MainTeacherWindow`, click nút "Bình chọn" ở sidebar phải.
2.  **Start Student App**: Chạy Client học sinh, kết nối tới Teacher.
3.  **Create Poll**: Tại Teacher App, nhập câu hỏi và options -> Bấm "Bắt đầu".
4.  **Vote**: Tại Student App, cửa sổ bình chọn hiện ra -> Chọn đáp án.
5.  **Observe**: Xem thanh kết quả trên Teacher App tăng lên theo thời gian thực.
6.  **Stop**: Bấm "Kết thúc bình chọn" -> Cửa sổ bên học sinh tự đóng (nếu chưa đóng).

## Notes
-   Hệ thống sử dụng cơ chế sự kiện (Events) để đảm bảo UI phản hồi tức thì mà không cần polling liên tục.
-   Dữ liệu bình chọn hiện tại là **in-memory** (mất khi tắt app). Nếu cần lưu lịch sử, cần tích hợp thêm Database.
