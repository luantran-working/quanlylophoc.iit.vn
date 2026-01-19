# Kiến trúc Hệ thống

## Tổng quan

Phần mềm Quản lý Phòng học Thông minh IIT sử dụng kiến trúc **Client-Server** truyền thống, hoạt động hoàn toàn trên **mạng LAN nội bộ**.

```
┌─────────────────────────────────────────────────────────────┐
│                      MẠNG LAN / WIFI                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   ┌─────────────────┐                                       │
│   │  MÁY GIÁO VIÊN  │◄──────────────────────────────────┐   │
│   │    (SERVER)     │                                    │   │
│   │                 │                                    │   │
│   │  • Database     │     ┌──────────┐  ┌──────────┐    │   │
│   │  • Auth Service │◄────│ Học sinh │  │ Học sinh │    │   │
│   │  • File Storage │     │    01    │  │    02    │    │   │
│   │  • Screen Share │     └──────────┘  └──────────┘    │   │
│   │  • Chat Server  │                                    │   │
│   └─────────────────┘     ┌──────────┐  ┌──────────┐    │   │
│           ▲               │ Học sinh │  │ Học sinh │────┘   │
│           │               │    03    │  │    ...   │        │
│           │               └──────────┘  └──────────┘        │
│           │                                                 │
│           └─────────── Auto Discovery (UDP Broadcast) ──────┤
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Mô hình kết nối

### 1. Khởi động Server (Máy Giáo viên)

```
┌───────────────────────────────────────────────────────┐
│                 KHỞI ĐỘNG SERVER                      │
├───────────────────────────────────────────────────────┤
│                                                       │
│  1. Khởi động ứng dụng với quyền Giáo viên           │
│                    ▼                                  │
│  2. Khởi tạo Database cục bộ (SQLite)                │
│                    ▼                                  │
│  3. Bắt đầu lắng nghe kết nối TCP (Port: 5000)       │
│                    ▼                                  │
│  4. Phát UDP Broadcast để thông báo sự hiện diện     │
│     (Port: 5001, Interval: 3 giây)                   │
│                    ▼                                  │
│  5. Sẵn sàng nhận kết nối từ Client                  │
│                                                       │
└───────────────────────────────────────────────────────┘
```

### 2. Kết nối Client (Máy Học sinh)

```
┌───────────────────────────────────────────────────────┐
│                 KẾT NỐI CLIENT                        │
├───────────────────────────────────────────────────────┤
│                                                       │
│  1. Khởi động ứng dụng với vai trò Học sinh          │
│                    ▼                                  │
│  2. Lắng nghe UDP Broadcast từ Server                │
│     (Port: 5001)                                      │
│                    ▼                                  │
│  3. Nhận thông tin Server (IP, Port, Class Name)     │
│                    ▼                                  │
│  4. Kết nối TCP đến Server                           │
│     (IP: Server IP, Port: 5000)                      │
│                    ▼                                  │
│  5. Gửi thông tin đăng ký (Tên, Mã máy)              │
│                    ▼                                  │
│  6. Nhận xác nhận và bắt đầu phiên học               │
│                                                       │
└───────────────────────────────────────────────────────┘
```

## Các thành phần chính

### Server Components (Máy Giáo viên)

| Thành phần             | Mô tả                             | Port/Protocol |
| ---------------------- | --------------------------------- | ------------- |
| **Discovery Service**  | Phát broadcast để client tìm kiếm | UDP 5001      |
| **Connection Manager** | Quản lý kết nối TCP từ clients    | TCP 5000      |
| **Auth Service**       | Xác thực giáo viên                | Local         |
| **Database Service**   | SQLite lưu trữ dữ liệu            | Local File    |
| **Screen Capture**     | Chụp và stream màn hình           | TCP 5002      |
| **File Transfer**      | Truyền nhận file                  | TCP 5003      |
| **Chat Service**       | Xử lý tin nhắn                    | TCP 5004      |
| **Remote Control**     | Điều khiển từ xa                  | TCP 5005      |

### Client Components (Máy Học sinh)

| Thành phần             | Mô tả                         |
| ---------------------- | ----------------------------- |
| **Discovery Listener** | Lắng nghe broadcast từ Server |
| **Connection Client**  | Kết nối và duy trì session    |
| **Screen Agent**       | Chụp và gửi ảnh màn hình      |
| **Input Agent**        | Nhận lệnh điều khiển          |
| **File Agent**         | Xử lý truyền file             |
| **Chat Client**        | Gửi/nhận tin nhắn             |

## Luồng dữ liệu chính

### 1. Giám sát màn hình

```
┌────────────┐                              ┌────────────┐
│ Học sinh   │  ──── Ảnh màn hình ────►    │ Giáo viên  │
│ (Client)   │      (JPEG, 30fps)          │ (Server)   │
│            │  ◄──── Yêu cầu refresh ──── │            │
└────────────┘                              └────────────┘
```

### 2. Điều khiển từ xa

```
┌────────────┐                              ┌────────────┐
│ Giáo viên  │  ──── Lệnh điều khiển ────► │ Học sinh   │
│ (Server)   │      (Mouse, Keyboard)      │ (Client)   │
│            │  ◄──── Màn hình stream ──── │            │
└────────────┘                              └────────────┘
```

### 3. Chia sẻ màn hình (Trình chiếu)

```
                    ┌────────────┐
                    │ Giáo viên  │
                    │  (Server)  │
                    └─────┬──────┘
                          │
            Stream màn hình (Multicast)
                          │
         ┌────────────────┼────────────────┐
         ▼                ▼                ▼
   ┌──────────┐     ┌──────────┐     ┌──────────┐
   │ Học sinh │     │ Học sinh │     │ Học sinh │
   │    01    │     │    02    │     │    03    │
   └──────────┘     └──────────┘     └──────────┘
```

## Giao thức truyền thông

### Protocol Stack

```
┌─────────────────────────────────────┐
│         Application Layer           │
│   (Screen, Chat, File, Control)     │
├─────────────────────────────────────┤
│       Custom Message Protocol       │
│   (Header + Payload + Checksum)     │
├─────────────────────────────────────┤
│         Transport Layer             │
│        TCP (Reliable Data)          │
│        UDP (Discovery Only)         │
├─────────────────────────────────────┤
│         Network Layer               │
│            IPv4/IPv6                │
└─────────────────────────────────────┘
```

### Message Format

```
┌──────────────────────────────────────────────────────┐
│                   MESSAGE FRAME                      │
├────────┬────────┬──────────┬────────────┬───────────┤
│ MAGIC  │  TYPE  │  LENGTH  │  PAYLOAD   │ CHECKSUM  │
│ 2 byte │ 1 byte │  4 byte  │  N bytes   │  4 byte   │
└────────┴────────┴──────────┴────────────┴───────────┘

MAGIC: 0x49 0x49 ("II" - IIT)
TYPE: Message type code
LENGTH: Payload length (Big Endian)
PAYLOAD: Data content
CHECKSUM: CRC32
```

### Message Types

| Code | Type             | Mô tả               |
| ---- | ---------------- | ------------------- |
| 0x01 | CONNECT          | Yêu cầu kết nối     |
| 0x02 | DISCONNECT       | Ngắt kết nối        |
| 0x03 | HEARTBEAT        | Kiểm tra kết nối    |
| 0x10 | SCREEN_DATA      | Dữ liệu màn hình    |
| 0x11 | SCREEN_REQUEST   | Yêu cầu màn hình    |
| 0x20 | CONTROL_MOUSE    | Lệnh chuột          |
| 0x21 | CONTROL_KEYBOARD | Lệnh bàn phím       |
| 0x30 | CHAT_MESSAGE     | Tin nhắn chat       |
| 0x40 | FILE_START       | Bắt đầu truyền file |
| 0x41 | FILE_DATA        | Dữ liệu file        |
| 0x42 | FILE_END         | Kết thúc file       |
| 0x50 | LOCK_SCREEN      | Khóa màn hình       |
| 0x51 | UNLOCK_SCREEN    | Mở khóa màn hình    |

## Bảo mật

### Xác thực

1. **Giáo viên**: Đăng nhập bằng tài khoản (mặc định: admin/123456)
2. **Học sinh**: Tự động xác thực qua mã máy (Machine ID)

### Mã hóa

- Kết nối trong mạng LAN nội bộ
- Dữ liệu nhạy cảm (mật khẩu) được hash bằng SHA-256
- Có thể bật TLS cho môi trường yêu cầu bảo mật cao

## Xử lý lỗi & Recovery

### Auto-Reconnect

```
Client bị mất kết nối
        │
        ▼
Chờ 3 giây
        │
        ▼
Thử kết nối lại (Tối đa 5 lần)
        │
        ├── Thành công ──► Tiếp tục phiên
        │
        └── Thất bại ──► Quay về màn hình chờ
                         Lắng nghe Discovery
```

### Server Failover

- Nếu Server ngắt kết nối, tất cả Client tự động chuyển về trạng thái Standby
- Khi Server khởi động lại, Client tự động kết nối lại

---

_Tài liệu kỹ thuật - Phiên bản 1.0.0_
