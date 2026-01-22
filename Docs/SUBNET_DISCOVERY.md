# UDP Unicast Scanning + Directed Broadcast Discovery

## Tổng quan

Giải pháp này giúp máy Student tìm được máy Teacher qua các VLAN/Subnet khác nhau, khi mà UDP Broadcast bị Router chặn.

## Vấn đề

- Hệ thống mạng trường học chia nhiều VLAN/Subnet (ví dụ: 192.168.0.x, 192.168.1.x, 192.168.2.x)
- Router chặn UDP Broadcast đi qua các Subnet khác
- Máy Student ở subnet khác không thể tìm thấy máy Teacher

## Giải pháp

### 1. UDP Unicast Scanning (Brute-force Scanning)

Thay vì chờ broadcast từ Server, Client sẽ chủ động gửi UDP packets đến từng IP trong các subnet được cấu hình:

```csharp
// Sử dụng SubnetDiscoveryService
var discoveryService = new SubnetDiscoveryService
{
    DiscoveryPort = 5001,
    ScanTimeoutMs = 2000,     // Timeout 2 giây
    MaxParallelism = 100,     // 100 concurrent connections
    UseDirectedBroadcast = true
};

// Quét với auto-detect subnets
var servers = await discoveryService.ScanForServersAsync();

// Hoặc chỉ định subnets cụ thể
var servers = await discoveryService.ScanSubnetsAsync(
    "192.168.0",
    "192.168.1",
    "192.168.2",
    "10.0.0"
);
```

### 2. Tích hợp vào NetworkClientService

```csharp
var networkClient = new NetworkClientService();

// Cách 1: Chỉ dùng subnet scanning
var server = await networkClient.DiscoverServerViaScanAsync(
    subnetsToScan: new List<string> { "192.168.0", "192.168.1" },
    timeoutMs: 2000
);

// Cách 2: Auto discovery (thử broadcast trước, nếu thất bại thì quét subnet)
var server = await networkClient.DiscoverServerAutoAsync(
    subnetsToScan: null,           // Auto-detect
    broadcastTimeoutSeconds: 5,
    scanTimeoutMs: 2000
);

if (server != null)
{
    await networkClient.ConnectAsync(server.ServerIp, server.ServerPort);
}
```

### 3. Server-side (Teacher)

Server tự động:

- Chạy `DiscoveryResponderService` để phản hồi UDP unicast requests
- Cấu hình Windows Firewall rules

```csharp
var serverService = new NetworkServerService();
serverService.ClassName = "Lớp 10A1";
serverService.TeacherName = "Thầy Nguyễn Văn A";

await serverService.StartAsync(5000);
// Discovery Responder tự động khởi động trên port 5001
// Firewall rules tự động được thêm
```

## Cấu hình Subnet

### Auto-detect

Mặc định, service tự động detect các subnet lân cận dựa trên IP local:

- Nếu IP local là `192.168.1.x`, sẽ quét: `192.168.0.x`, `192.168.1.x`, `192.168.2.x`, `192.168.3.x`, `192.168.4.x`

### Cấu hình thủ công

```csharp
var discoveryService = new SubnetDiscoveryService
{
    TargetSubnets = new List<string>
    {
        "192.168.0",
        "192.168.1",
        "192.168.10",
        "10.0.0",
        "10.0.1"
    },
    StartHost = 1,    // Bắt đầu từ .1
    EndHost = 254     // Kết thúc ở .254
};
```

## Windows Firewall

### Tự động

Server tự động thêm Firewall rules khi khởi động:

```csharp
// Tự động gọi khi StartAsync()
FirewallHelper.EnsureClassroomManagementRules(tcpPort: 5000, udpPort: 5001);
```

### Thủ công (via netsh)

```csharp
// Thêm TCP rule
FirewallHelper.AddInboundRule("ClassroomManagement_TCP", 5000, "TCP");

// Thêm UDP rule
FirewallHelper.AddInboundRule("ClassroomManagement_UDP", 5001, "UDP");

// Thêm cả TCP và UDP
FirewallHelper.AddInboundRules("ClassroomManagement", 5000);
```

### Thủ công (via COM)

```csharp
// Alternative method sử dụng Windows Firewall COM API
FirewallHelper.AddInboundRuleViaCOM("MyApp_TCP", 5000, "TCP");
```

## Performance

### Tốc độ quét

- **1 subnet (254 IPs)**: ~0.5 giây
- **4 subnets (1016 IPs)**: ~1.5 giây
- **10 subnets (2540 IPs)**: ~3 giây

### Tối ưu

```csharp
var discoveryService = new SubnetDiscoveryService
{
    MaxParallelism = 200,     // Tăng concurrent connections (mặc định: 100)
    ScanTimeoutMs = 1500      // Giảm timeout
};
```

## Sơ đồ hoạt động

```
┌──────────────────────────────────────────────────────────────────┐
│                     STUDENT (192.168.1.100)                       │
│                                                                   │
│  1. Lấy IP local: 192.168.1.100                                  │
│  2. Xác định subnets: 192.168.0.x, 192.168.1.x, 192.168.2.x      │
│  3. Song song gửi UDP packets đến tất cả IPs                      │
│     ├─ 192.168.0.1:5001                                          │
│     ├─ 192.168.0.2:5001                                          │
│     ├─ ...                                                        │
│     ├─ 192.168.0.255:5001 (Directed Broadcast)                   │
│     ├─ 192.168.1.1:5001                                          │
│     └─ ...                                                        │
│  4. Chờ response trong 2 giây                                     │
│  5. Nhận response từ Teacher → Kết nối TCP                        │
└──────────────────────────────────────────────────────────────────┘
                              │
                              │ UDP Unicast
                              ▼
┌──────────────────────────────────────────────────────────────────┐
│                     TEACHER (192.168.0.50)                        │
│                                                                   │
│  DiscoveryResponderService lắng nghe UDP port 5001               │
│  ├─ Nhận DISCOVERY_REQUEST                                        │
│  └─ Gửi về ServerDiscoveryInfo (IP, Port, ClassName...)          │
└──────────────────────────────────────────────────────────────────┘
```

## Classes

### SubnetDiscoveryService

Service chính cho Client để quét tìm Server.

| Property             | Type         | Default | Mô tả                |
| -------------------- | ------------ | ------- | -------------------- |
| DiscoveryPort        | int          | 5001    | Port UDP             |
| ScanTimeoutMs        | int          | 2000    | Timeout (ms)         |
| MaxParallelism       | int          | 100     | Max concurrent scans |
| TargetSubnets        | List<string> | null    | Subnets cần quét     |
| UseDirectedBroadcast | bool         | true    | Gửi thêm vào .255    |
| StartHost            | int          | 1       | IP bắt đầu           |
| EndHost              | int          | 254     | IP kết thúc          |

### DiscoveryResponderService

Service cho Server để phản hồi discovery requests.

| Property       | Type      | Default     | Mô tả                           |
| -------------- | --------- | ----------- | ------------------------------- |
| ListenPort     | int       | 5001        | Port UDP lắng nghe              |
| ServerIp       | string    | ""          | IP của server                   |
| ServerPort     | int       | 5000        | TCP port                        |
| ClassName      | string    | "Lớp học"   | Tên lớp                         |
| TeacherName    | string    | "Giáo viên" | Tên giáo viên                   |
| GetOnlineCount | Func<int> | null        | Callback lấy số học sinh online |

### FirewallHelper

Utility class để quản lý Windows Firewall.

| Method                                     | Mô tả                 |
| ------------------------------------------ | --------------------- |
| AddInboundRule(name, port, protocol)       | Thêm rule inbound     |
| AddInboundRules(name, port)                | Thêm cả TCP và UDP    |
| RemoveInboundRule(name)                    | Xóa rule              |
| RuleExists(name)                           | Kiểm tra rule tồn tại |
| EnsureClassroomManagementRules(tcp, udp)   | Đảm bảo rules cho app |
| AddInboundRuleViaCOM(name, port, protocol) | Thêm via COM API      |

## Events

### SubnetDiscoveryService

```csharp
discoveryService.ServerDiscovered += (sender, serverInfo) =>
{
    Console.WriteLine($"Found: {serverInfo.ClassName} at {serverInfo.ServerIp}");
};

discoveryService.ScanCompleted += (sender, results) =>
{
    Console.WriteLine($"Scan completed, found {results.Count} servers");
};
```

## Lưu ý

1. **Firewall**: Đảm bảo port UDP 5001 được mở trên máy Teacher
2. **Router ACL**: Một số router có thể chặn unicast UDP giữa các VLAN, cần cấu hình ACL cho phép
3. **Performance**: Quét nhiều subnet có thể tạo traffic lớn, nên giới hạn số subnets cần quét
4. **Security**: Không có mã hóa cho gói tin discovery (theo yêu cầu)
