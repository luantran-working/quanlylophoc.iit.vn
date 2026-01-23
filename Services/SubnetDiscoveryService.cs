using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service discovery sử dụng UDP Unicast Scanning + Directed Broadcast
    /// để tìm Server qua các VLAN/Subnet khác nhau
    /// </summary>
    public class SubnetDiscoveryService : IDisposable
    {
        private readonly LogService _log = LogService.Instance;
        private CancellationTokenSource? _cts;
        private UdpClient? _responseListener;
        private readonly ConcurrentBag<ServerDiscoveryInfo> _discoveredServers = new();

        /// <summary>
        /// Port mà Server lắng nghe UDP discovery requests
        /// </summary>
        public int DiscoveryPort { get; set; } = 5001;

        /// <summary>
        /// Timeout cho mỗi lần scan (ms)
        /// </summary>
        public int ScanTimeoutMs { get; set; } = 2000;

        /// <summary>
        /// Số lượng task song song tối đa
        /// </summary>
        public int MaxParallelism { get; set; } = 100;

        /// <summary>
        /// Danh sách các subnet cần quét (ví dụ: "192.168.0", "192.168.1", "10.0.0")
        /// Nếu null hoặc rỗng, tự động detect từ network interfaces
        /// </summary>
        public List<string>? TargetSubnets { get; set; }

        /// <summary>
        /// Có quét cả Directed Broadcast (.255) không
        /// </summary>
        public bool UseDirectedBroadcast { get; set; } = true;

        /// <summary>
        /// Dải IP cần quét (mặc định 1-254)
        /// </summary>
        public int StartHost { get; set; } = 1;
        public int EndHost { get; set; } = 254;

        /// <summary>
        /// Event khi tìm thấy Server
        /// </summary>
        public event EventHandler<ServerDiscoveryInfo>? ServerDiscovered;

        /// <summary>
        /// Event khi scan hoàn tất
        /// </summary>
        public event EventHandler<List<ServerDiscoveryInfo>>? ScanCompleted;

        /// <summary>
        /// Lấy IP local hiện tại (ưu tiên Ethernet/WiFi thực, loại bỏ virtual adapters)
        /// </summary>
        public static string GetLocalIP()
        {
            // Thử lấy tất cả IPs thực, chọn cái đầu tiên
            var realIps = GetAllRealLocalIPs();
            if (realIps.Count > 0)
            {
                return realIps[0];
            }

            // Fallback: thử kết nối internet
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null)
                {
                    return endPoint.Address.ToString();
                }
            }
            catch { }

            return "127.0.0.1";
        }

        /// <summary>
        /// Lấy tất cả IPs thực từ các network interfaces (loại bỏ VMware, Hyper-V, WSL, Loopback)
        /// </summary>
        public static List<string> GetAllRealLocalIPs()
        {
            var ips = new List<string>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                // Loại bỏ virtual adapters
                var name = ni.Name.ToLowerInvariant();
                var description = ni.Description.ToLowerInvariant();

                if (IsVirtualAdapter(name, description)) continue;

                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var ip = addr.Address.ToString();
                        // Loại bỏ link-local (169.254.x.x)
                        if (!ip.StartsWith("169.254."))
                        {
                            ips.Add(ip);
                        }
                    }
                }
            }

            return ips;
        }

        /// <summary>
        /// Kiểm tra xem adapter có phải là virtual không
        /// </summary>
        private static bool IsVirtualAdapter(string name, string description)
        {
            var virtualKeywords = new[]
            {
                "vmware", "vmnet", "virtualbox", "vbox",
                "hyper-v", "vethernet", "wsl",
                "docker", "podman",
                "loopback", "pseudo",
                "bluetooth", "teredo", "isatap",
                "6to4", "tunneling"
            };

            foreach (var keyword in virtualKeywords)
            {
                if (name.Contains(keyword) || description.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Lấy danh sách tất cả subnets từ tất cả network interfaces thực
        /// </summary>
        public static List<string> GetAllRealSubnets()
        {
            var subnets = new HashSet<string>();

            foreach (var ip in GetAllRealLocalIPs())
            {
                var parts = ip.Split('.');
                if (parts.Length == 4)
                {
                    subnets.Add($"{parts[0]}.{parts[1]}.{parts[2]}");
                }
            }

            return subnets.ToList();
        }

        /// <summary>
        /// Lấy danh sách subnet lân cận dựa trên IP hiện tại
        /// Tự động quét các dải lân cận (.0, .1, .2, v.v.)
        /// </summary>
        public static List<string> GetNeighboringSubnets(string localIp, int range = 3)
        {
            var subnets = new HashSet<string>();

            // Thêm subnets từ tất cả real interfaces trước
            foreach (var realSubnet in GetAllRealSubnets())
            {
                subnets.Add(realSubnet);
            }

            // Thêm subnets lân cận từ IP được chỉ định
            try
            {
                var parts = localIp.Split('.');
                if (parts.Length == 4)
                {
                    if (int.TryParse(parts[0], out int o1) &&
                        int.TryParse(parts[1], out int o2) &&
                        int.TryParse(parts[2], out int o3))
                    {
                        string basePrefix = $"{o1}.{o2}";
                        for (int i = Math.Max(0, o3 - range); i <= Math.Min(255, o3 + range); i++)
                        {
                            subnets.Add($"{basePrefix}.{i}");
                        }
                    }
                }
            }
            catch { }

            return subnets.ToList();
        }

        /// <summary>
        /// Lấy tất cả subnet từ các network interface
        /// </summary>
        public static List<string> GetAllLocalSubnets()
        {
            var subnets = new HashSet<string>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;

                var ipProps = ni.GetIPProperties();
                foreach (var addr in ipProps.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var ip = addr.Address.ToString();
                        var parts = ip.Split('.');
                        if (parts.Length == 4)
                        {
                            subnets.Add($"{parts[0]}.{parts[1]}.{parts[2]}");
                        }
                    }
                }
            }

            return subnets.ToList();
        }

        /// <summary>
        /// Tạo danh sách subnet tuần tự từ 0 tăng dần (192.168.0.x, .1.x, ...)
        /// </summary>
        public static List<string> GetSequentialSubnets(int count = 10)
        {
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                list.Add($"192.168.{i}");
            }
            return list;
        }

        /// <summary>
        /// Quét các subnet để tìm Server
        /// Sử dụng UDP Unicast + Directed Broadcast
        /// </summary>
        public async Task<List<ServerDiscoveryInfo>> ScanForServersAsync(
            List<string>? customSubnets = null,
            CancellationToken cancellationToken = default)
        {
            // Chạy trên thread pool để tránh block UI
            await Task.Yield();

            _discoveredServers.Clear();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Xác định các subnet cần quét
            var subnetsToScan = customSubnets ?? TargetSubnets;
            if (subnetsToScan == null || subnetsToScan.Count == 0)
            {
                // Theo yêu cầu: Bắt đầu quét từ lớp mạng 0 sau đó tăng dần
                // Quét rộng hơn (0-50) để đảm bảo tìm thấy trong các VLAN phổ biến
                subnetsToScan = GetSequentialSubnets(50);

                // Bổ sung subnet local nếu chưa có
                var localIp = GetLocalIP();
                var neighbors = GetNeighboringSubnets(localIp);
                foreach (var s in neighbors)
                {
                    if (!subnetsToScan.Contains(s))
                    {
                        subnetsToScan.Add(s);
                    }
                }

                _log.Info("SubnetDiscovery", $"Scanning subnets: {subnetsToScan.Count} subnets (starts with 192.168.0.x...)");
            }

            _log.Info("SubnetDiscovery", $"Starting scan on {subnetsToScan.Count} subnets, port {DiscoveryPort}");
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Khởi tạo UDP listener để nhận response
                await StartResponseListenerAsync();

                // Tạo danh sách tất cả IP cần quét
                var allTargets = new List<string>();

                foreach (var subnet in subnetsToScan)
                {
                    // Thêm tất cả IP từ StartHost đến EndHost
                    for (int i = StartHost; i <= EndHost; i++)
                    {
                        allTargets.Add($"{subnet}.{i}");
                    }

                    // Thêm Directed Broadcast (.255)
                    if (UseDirectedBroadcast)
                    {
                        allTargets.Add($"{subnet}.255");
                    }
                }

                _log.Debug("SubnetDiscovery", $"Total targets: {allTargets.Count} IPs");

                // Gửi UDP packets song song
                await SendDiscoveryPacketsParallelAsync(allTargets, _cts.Token);

                // Đợi responses trong thời gian timeout
                await Task.Delay(ScanTimeoutMs, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                _log.Debug("SubnetDiscovery", "Scan cancelled");
            }
            catch (Exception ex)
            {
                _log.Error("SubnetDiscovery", "Error during scan", ex);
            }
            finally
            {
                StopResponseListener();
                stopwatch.Stop();
            }

            var results = _discoveredServers.ToList();
            _log.Info("SubnetDiscovery", $"Scan completed in {stopwatch.ElapsedMilliseconds}ms. Found {results.Count} server(s)");

            ScanCompleted?.Invoke(this, results);
            return results;
        }

        /// <summary>
        /// Gửi UDP discovery packets song song sử dụng Parallel với Batching để tránh treo UI
        /// </summary>
        private async Task SendDiscoveryPacketsParallelAsync(List<string> targetIps, CancellationToken ct)
        {
            // Yield để đảm bảo task chạy trên background thread ngay lập tức
            await Task.Yield();

            // Tạo discovery request packet
            var discoveryRequest = new
            {
                Type = "DISCOVERY_REQUEST",
                ClientIp = GetLocalIP(),
                Timestamp = DateTime.UtcNow.Ticks
            };
            var requestBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(discoveryRequest));

            // Sử dụng SemaphoreSlim để giới hạn số lượng concurrent operations thực tế
            using var semaphore = new SemaphoreSlim(MaxParallelism);
            var tasks = new List<Task>();

            // Chia nhỏ thành các batch để add tasks dần, tránh lag khi loop quá nhiều
            const int BATCH_SIZE = 500;
            for (int i = 0; i < targetIps.Count; i += BATCH_SIZE)
            {
                if (ct.IsCancellationRequested) break;

                var batch = targetIps.Skip(i).Take(BATCH_SIZE);
                foreach (var targetIp in batch)
                {
                    if (ct.IsCancellationRequested) break;

                    await semaphore.WaitAsync(ct);

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            using var udpClient = new UdpClient();
                            // Timeout cực ngắn để fail fast
                            udpClient.Client.ReceiveTimeout = 50;
                            udpClient.Client.SendTimeout = 50;

                            var endpoint = new IPEndPoint(IPAddress.Parse(targetIp), DiscoveryPort);
                            // Fire and forget send
                            await udpClient.SendAsync(requestBytes, requestBytes.Length, endpoint);
                        }
                        catch { /* Ignore all errors during scan */ }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, ct);

                    tasks.Add(task);
                }

                // Yield sau mỗi batch để UI thread (nếu có) kịp thở
                await Task.Delay(10, ct);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Khởi động listener để nhận UDP responses
        /// </summary>
        private async Task StartResponseListenerAsync()
        {
            try
            {
                _responseListener = new UdpClient();
                _responseListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _responseListener.Client.Bind(new IPEndPoint(IPAddress.Any, 0)); // Random port

                var localPort = ((IPEndPoint)_responseListener.Client.LocalEndPoint!).Port;
                _log.Debug("SubnetDiscovery", $"Response listener started on port {localPort}");

                // Start listening loop
                _ = ListenForResponsesAsync();
            }
            catch (Exception ex)
            {
                _log.Error("SubnetDiscovery", "Failed to start response listener", ex);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Lắng nghe UDP responses từ Server
        /// </summary>
        private async Task ListenForResponsesAsync()
        {
            if (_responseListener == null) return;

            try
            {
                while (_cts != null && !_cts.IsCancellationRequested)
                {
                    try
                    {
                        var receiveTask = _responseListener.ReceiveAsync(_cts.Token).AsTask();
                        var timeoutTask = Task.Delay(100, _cts.Token);

                        var completed = await Task.WhenAny(receiveTask, timeoutTask);

                        if (completed == receiveTask && !receiveTask.IsFaulted)
                        {
                            var result = await receiveTask;
                            var json = Encoding.UTF8.GetString(result.Buffer);

                            _log.Debug("SubnetDiscovery", $"Response from {result.RemoteEndPoint}: {json}");

                            try
                            {
                                var serverInfo = JsonSerializer.Deserialize<ServerDiscoveryInfo>(json);
                                if (serverInfo != null && !string.IsNullOrEmpty(serverInfo.ServerIp))
                                {
                                    // Kiểm tra duplicate
                                    if (!_discoveredServers.Any(s => s.ServerIp == serverInfo.ServerIp && s.ServerPort == serverInfo.ServerPort))
                                    {
                                        _discoveredServers.Add(serverInfo);
                                        _log.Info("SubnetDiscovery", $"Server discovered: {serverInfo.ClassName} at {serverInfo.ServerIp}:{serverInfo.ServerPort}");
                                        ServerDiscovered?.Invoke(this, serverInfo);
                                    }
                                }
                            }
                            catch (JsonException)
                            {
                                // Invalid JSON, ignore
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (SocketException)
                    {
                        // Socket closed
                        break;
                    }
                    catch (Exception ex)
                    {
                        _log.Debug("SubnetDiscovery", $"Listener error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Debug("SubnetDiscovery", $"Listener stopped: {ex.Message}");
            }
        }

        /// <summary>
        /// Dừng response listener
        /// </summary>
        private void StopResponseListener()
        {
            try
            {
                _responseListener?.Close();
                _responseListener?.Dispose();
            }
            catch { }
            _responseListener = null;
        }

        /// <summary>
        /// Quét nhanh với cấu hình mặc định
        /// </summary>
        public async Task<ServerDiscoveryInfo?> QuickScanAsync(int timeoutMs = 2000)
        {
            ScanTimeoutMs = timeoutMs;
            var results = await ScanForServersAsync();
            return results.FirstOrDefault();
        }

        /// <summary>
        /// Quét với danh sách subnet cụ thể
        /// </summary>
        public async Task<List<ServerDiscoveryInfo>> ScanSubnetsAsync(params string[] subnets)
        {
            return await ScanForServersAsync(subnets.ToList());
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            StopResponseListener();
        }
    }

    /// <summary>
    /// Service để Server phản hồi UDP discovery requests
    /// Dùng cho NetworkServerService
    /// </summary>
    public class DiscoveryResponderService : IDisposable
    {
        private readonly LogService _log = LogService.Instance;
        private UdpClient? _udpListener;
        private CancellationTokenSource? _cts;

        public int ListenPort { get; set; } = 5001;
        public string ServerIp { get; set; } = "";
        public int ServerPort { get; set; } = 5000;
        public string ClassName { get; set; } = "Lớp học";
        public string TeacherName { get; set; } = "Giáo viên";
        public Func<int>? GetOnlineCount { get; set; }

        /// <summary>
        /// Khởi động responder service
        /// </summary>
        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();

            try
            {
                _udpListener = new UdpClient();
                _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpListener.Client.Bind(new IPEndPoint(IPAddress.Any, ListenPort));

                _log.Info("DiscoveryResponder", $"Started listening on UDP port {ListenPort}");

                _ = ListenAndRespondAsync(_cts.Token);
            }
            catch (Exception ex)
            {
                _log.Error("DiscoveryResponder", $"Failed to start on port {ListenPort}", ex);
                throw;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Lắng nghe và phản hồi discovery requests
        /// </summary>
        private async Task ListenAndRespondAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _udpListener != null)
            {
                try
                {
                    var result = await _udpListener.ReceiveAsync(ct);
                    var request = Encoding.UTF8.GetString(result.Buffer);

                    _log.Debug("DiscoveryResponder", $"Request from {result.RemoteEndPoint}: {request}");

                    // Kiểm tra xem đây có phải là discovery request không
                    if (request.Contains("DISCOVERY_REQUEST") || request.Contains("Ping"))
                    {
                        var responseInfo = new ServerDiscoveryInfo
                        {
                            ServerIp = ServerIp,
                            ServerPort = ServerPort,
                            ClassName = ClassName,
                            TeacherName = TeacherName,
                            OnlineCount = GetOnlineCount?.Invoke() ?? 0
                        };

                        var responseBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(responseInfo));

                        // Gửi response về client
                        await _udpListener.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);

                        _log.Debug("DiscoveryResponder", $"Sent response to {result.RemoteEndPoint}");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (SocketException)
                {
                    // Socket closed
                    break;
                }
                catch (Exception ex)
                {
                    _log.Warning("DiscoveryResponder", $"Error: {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            try { _udpListener?.Close(); } catch { }
            _log.Info("DiscoveryResponder", "Stopped");
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }

    /// <summary>
    /// Utility class để quản lý Windows Firewall rules
    /// </summary>
    public static class FirewallHelper
    {
        private static readonly LogService _log = LogService.Instance;

        /// <summary>
        /// Thêm Inbound Rule cho Windows Firewall sử dụng netsh
        /// </summary>
        /// <param name="ruleName">Tên rule (unique)</param>
        /// <param name="port">Port cần mở</param>
        /// <param name="protocol">Protocol: TCP hoặc UDP</param>
        /// <returns>True nếu thành công</returns>
        public static bool AddInboundRule(string ruleName, int port, string protocol = "TCP")
        {
            try
            {
                // Kiểm tra xem rule đã tồn tại chưa
                if (RuleExists(ruleName))
                {
                    _log.Info("Firewall", $"Rule '{ruleName}' already exists");
                    return true;
                }

                var arguments = $"advfirewall firewall add rule name=\"{ruleName}\" " +
                               $"dir=in action=allow protocol={protocol} localport={port}";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    Verb = "runas" // Request admin rights
                };

                using var process = Process.Start(processInfo);
                if (process == null)
                {
                    _log.Error("Firewall", "Failed to start netsh process");
                    return false;
                }

                process.WaitForExit(10000);

                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();

                if (process.ExitCode == 0)
                {
                    _log.Info("Firewall", $"Added rule '{ruleName}' for {protocol} port {port}");
                    return true;
                }
                else
                {
                    _log.Warning("Firewall", $"Failed to add rule: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _log.Error("Firewall", "Error adding firewall rule", ex);
                return false;
            }
        }

        /// <summary>
        /// Thêm Inbound Rules cho cả TCP và UDP
        /// </summary>
        public static bool AddInboundRules(string baseRuleName, int port)
        {
            var tcpResult = AddInboundRule($"{baseRuleName}_TCP", port, "TCP");
            var udpResult = AddInboundRule($"{baseRuleName}_UDP", port, "UDP");
            return tcpResult && udpResult;
        }

        /// <summary>
        /// Xóa Inbound Rule
        /// </summary>
        public static bool RemoveInboundRule(string ruleName)
        {
            try
            {
                var arguments = $"advfirewall firewall delete rule name=\"{ruleName}\"";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return false;

                process.WaitForExit(10000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra xem rule đã tồn tại chưa
        /// </summary>
        public static bool RuleExists(string ruleName)
        {
            try
            {
                var arguments = $"advfirewall firewall show rule name=\"{ruleName}\"";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "netsh",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process == null) return false;

                process.WaitForExit(5000);
                var output = process.StandardOutput.ReadToEnd();

                return output.Contains(ruleName) && !output.Contains("No rules match");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tự động thêm firewall rules cho ứng dụng Classroom Management
        /// </summary>
        public static void EnsureClassroomManagementRules(int tcpPort = 5000, int udpPort = 5001)
        {
            _log.Info("Firewall", "Ensuring firewall rules for Classroom Management...");

            // TCP port for main communication
            AddInboundRule("ClassroomManagement_TCP", tcpPort, "TCP");

            // UDP port for discovery
            AddInboundRule("ClassroomManagement_UDP", udpPort, "UDP");

            _log.Info("Firewall", "Firewall rules check completed");
        }

        /// <summary>
        /// Thêm firewall rule sử dụng Windows Firewall COM API (alternative method)
        /// Yêu cầu reference đến NetFwTypeLib hoặc sử dụng dynamic
        /// </summary>
        public static bool AddInboundRuleViaCOM(string ruleName, int port, string protocol = "TCP")
        {
            try
            {
                // Create firewall policy object
                Type? policyType = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (policyType == null)
                {
                    _log.Warning("Firewall", "Cannot create HNetCfg.FwPolicy2");
                    return false;
                }

                dynamic fwPolicy = Activator.CreateInstance(policyType)!;
                dynamic rules = fwPolicy.Rules;

                // Check if rule exists
                foreach (dynamic rule in rules)
                {
                    if (rule.Name == ruleName)
                    {
                        _log.Info("Firewall", $"Rule '{ruleName}' already exists (COM)");
                        return true;
                    }
                }

                // Create new rule
                Type? ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
                if (ruleType == null)
                {
                    _log.Warning("Firewall", "Cannot create HNetCfg.FWRule");
                    return false;
                }

                dynamic newRule = Activator.CreateInstance(ruleType)!;
                newRule.Name = ruleName;
                newRule.Description = $"Allow inbound {protocol} on port {port} for Classroom Management";
                newRule.Protocol = protocol.ToUpper() == "TCP" ? 6 : 17; // 6 = TCP, 17 = UDP
                newRule.LocalPorts = port.ToString();
                newRule.Direction = 1; // Inbound
                newRule.Action = 1; // Allow
                newRule.Enabled = true;
                newRule.Profiles = 7; // All profiles (Domain, Private, Public)

                rules.Add(newRule);

                _log.Info("Firewall", $"Added rule '{ruleName}' via COM for {protocol} port {port}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Firewall", "Error adding firewall rule via COM", ex);
                return false;
            }
        }
    }
}
