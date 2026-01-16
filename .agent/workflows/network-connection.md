---
description: Quy trình kết nối mạng LAN giữa máy giáo viên (Server) và máy học sinh (Client)
---

# Quy trình Kết nối Mạng LAN

## Tổng quan

Phần mềm sử dụng mạng **LAN cục bộ** để kết nối các máy tính với nhau. Khi tất cả máy tính cùng kết nối một mạng WiFi hoặc Ethernet, chúng sẽ tự động phát hiện và kết nối.

## Yêu cầu mạng

### Cấu hình mạng

| Yêu cầu | Chi tiết |
|---------|----------|
| Loại mạng | LAN (Ethernet hoặc WiFi) |
| Subnet | Tất cả máy cùng subnet (VD: 192.168.1.x) |
| Firewall | Cho phép các port: 5000-5005 |
| Broadcast | Cho phép UDP Broadcast |

### Ports sử dụng

| Port | Protocol | Chức năng |
|------|----------|-----------|
| 5000 | TCP | Kết nối chính (Main Connection) |
| 5001 | UDP | Discovery (Tìm kiếm Server) |
| 5002 | TCP | Screen Streaming |
| 5003 | TCP | File Transfer |
| 5004 | TCP | Chat Messages |
| 5005 | TCP | Remote Control |

## Các bước kết nối

### Bước 1: Chuẩn bị mạng

```
1. Đảm bảo tất cả máy tính kết nối cùng mạng
   ├── Cùng WiFi Access Point
   └── Hoặc cùng Switch/Router Ethernet

// turbo
2. Kiểm tra kết nối mạng:
   - Mở Command Prompt
   - Chạy: ipconfig
   - Xác nhận IP cùng dải (VD: 192.168.1.x)

3. Tắt/Cấu hình Firewall:
   - Cho phép ứng dụng ClassroomManagement
   - Mở ports 5000-5005
```

### Bước 2: Khởi động Server (Máy Giáo viên)

```
1. Mở ứng dụng ClassroomManagement

2. Chọn "Giáo viên" tại màn hình chọn vai trò

3. Đăng nhập:
   - Username: admin
   - Password: 123456

4. Server khởi động:
   - Lắng nghe TCP Port 5000
   - Phát UDP Broadcast mỗi 3 giây trên Port 5001
   - Message: "IIT_CLASSROOM_SERVER|<IP>|<ClassName>"

5. Chờ học sinh kết nối
```

### Bước 3: Kết nối Client (Máy Học sinh)

```
1. Mở ứng dụng ClassroomManagement

2. Chọn "Học sinh" tại màn hình chọn vai trò

3. Ứng dụng tự động:
   - Lắng nghe UDP Broadcast trên Port 5001
   - Nhận thông tin Server (IP, Port, Class Name)
   - Kết nối TCP đến Server

4. Gửi thông tin đăng ký:
   - Machine ID (Định danh máy)
   - Display Name (Tên hiển thị)
   - Computer Name

5. Nhận xác nhận từ Server

6. Vào phòng học thành công
```

## Xử lý lỗi kết nối

### Lỗi: "Không tìm thấy Server"

```
Nguyên nhân:
├── Máy không cùng mạng LAN
├── Server chưa khởi động
├── Firewall block UDP Broadcast
└── IP conflict

Giải pháp:
1. Kiểm tra kết nối mạng (ipconfig)
2. Đảm bảo Server đã chạy
3. Tắt Firewall tạm thời để test
4. Restart ứng dụng
5. Kết nối thủ công nếu có (Nhập IP Server)
```

### Lỗi: "Kết nối bị ngắt"

```
Nguyên nhân:
├── Mạng không ổn định
├── Server crash/restart
├── Timeout do không có heartbeat
└── Firewall mới cấu hình

Giải pháp:
1. Ứng dụng tự động reconnect (5 lần)
2. Nếu thất bại, hiển thị thông báo
3. Quay về màn hình chờ Discovery
4. Khi Server up, tự động kết nối lại
```

### Lỗi: Firewall Block

```
Windows Firewall:
1. Mở Windows Defender Firewall
2. Click "Allow an app through firewall"
3. Click "Change settings"
4. Tìm "ClassroomManagement"
5. Check cả Private và Public
6. Click OK

Hoặc thêm Rule:
1. Mở Windows Firewall with Advanced Security
2. Inbound Rules → New Rule
3. Port → TCP/UDP → 5000-5005
4. Allow the connection
5. Đặt tên: "Classroom Management"
```

## Kiểm tra kết nối

### Trên máy Server (Giáo viên)

```powershell
# Kiểm tra port đang listen
netstat -an | findstr "5000"

# Kết quả mong đợi:
# TCP    0.0.0.0:5000    0.0.0.0:0    LISTENING
```

### Trên máy Client (Học sinh)

```powershell
# Ping đến Server
ping 192.168.1.100

# Test kết nối TCP
Test-NetConnection -ComputerName 192.168.1.100 -Port 5000
```

## Sơ đồ kết nối

```
┌─────────────────────────────────────────────────────────────┐
│                      MẠNG LAN                               │
│                    (192.168.1.0/24)                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│     ┌─────────────────────────────────────────┐             │
│     │           ROUTER / SWITCH               │             │
│     └───────────────────┬─────────────────────┘             │
│                         │                                   │
│         ┌───────────────┼───────────────┐                   │
│         │               │               │                   │
│         ▼               ▼               ▼                   │
│   ┌───────────┐   ┌───────────┐   ┌───────────┐             │
│   │   SERVER  │   │  CLIENT   │   │  CLIENT   │             │
│   │ (Giáo viên)│   │(Học sinh) │   │(Học sinh) │             │
│   │           │   │           │   │           │             │
│   │IP: .100   │   │IP: .101   │   │IP: .102   │             │
│   └───────────┘   └───────────┘   └───────────┘             │
│         │               ▲               ▲                   │
│         │               │               │                   │
│         └───────────────┴───────────────┘                   │
│                    TCP Connections                          │
│                                                             │
│         ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─                    │
│                 UDP Broadcast                               │
│                 (Discovery)                                 │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

## Best Practices

1. **Sử dụng mạng chuyên dụng**: Nếu có thể, dùng mạng riêng cho lớp học
2. **Ethernet > WiFi**: Ethernet ổn định hơn cho chia sẻ màn hình
3. **IP tĩnh cho Server**: Cấu hình IP tĩnh cho máy giáo viên
4. **Kiểm tra trước buổi học**: Đảm bảo kết nối hoạt động
5. **Backup plan**: Chuẩn bị phương án nếu mạng gặp sự cố
