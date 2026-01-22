using System;
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
    /// TCP Client cho máy Học sinh
    /// </summary>
    public class NetworkClientService : IDisposable
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private UdpClient? _udpListener;
        private CancellationTokenSource? _discoveryCts;
        private CancellationTokenSource? _connectionCts;
        private readonly LogService _log = LogService.Instance;
        private bool _isDiscovering = false;

        public string MachineId { get; private set; }
        public string DisplayName { get; set; } = "Học sinh";
        public bool IsConnected { get; private set; }
        public string ServerIp { get; private set; } = "";
        public int ServerPort { get; private set; } = 5000;

        // Events
        public event EventHandler<ServerDiscoveryInfo>? ServerDiscovered;
        public event EventHandler? Connected;
        public event EventHandler<string>? Disconnected;
        public event EventHandler<NetworkMessage>? MessageReceived;
        public event EventHandler<byte[]>? ScreenShareReceived;
        public event EventHandler? ScreenLocked;
        public event EventHandler? ScreenUnlocked;
        public event EventHandler? RemoteControlStarted;
        public event EventHandler? RemoteControlStopped;
        public event EventHandler<ScreenshotRequest>? ScreenshotRequested;

        private readonly FileCollectionService _fileCollectionService; // Add field

        public NetworkClientService()
        {
            MachineId = GetMachineId();
            _log.Info("NetworkClient", $"Initialized with MachineId: {MachineId}");
            _fileCollectionService = new FileCollectionService(this); // Initialize
        }


        /// <summary>
        /// Hủy discovery đang chạy
        /// </summary>
        public void CancelDiscovery()
        {
            try
            {
                _discoveryCts?.Cancel();
                _udpListener?.Close();
            }
            catch { }
        }

        /// <summary>
        /// Tìm Server trong mạng LAN bằng UDP
        /// </summary>
        public async Task<ServerDiscoveryInfo?> DiscoverServerAsync(int timeoutSeconds = 30)
        {
            // Prevent multiple simultaneous discoveries
            if (_isDiscovering)
            {
                _log.Warning("NetworkClient", "Discovery already in progress, cancelling previous...");
                CancelDiscovery();
                await Task.Delay(500);
            }

            _isDiscovering = true;
            _log.Info("NetworkClient", $"Starting server discovery (timeout: {timeoutSeconds}s)...");
            _log.Network("NetworkClient", $"Listening for UDP broadcasts on port 5001");

            // Log network info
            LogNetworkInfo();

            // Create new CTS for this discovery
            _discoveryCts = new CancellationTokenSource();
            var ct = _discoveryCts.Token;

            try
            {
                // Create UDP listener with proper configuration
                _udpListener = new UdpClient();
                _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                _udpListener.Client.ReceiveTimeout = 3000; // 3 second timeout for each receive attempt
                _udpListener.Client.Bind(new IPEndPoint(IPAddress.Any, 5001));

                _log.Debug("NetworkClient", "UDP listener bound to port 5001");
            }
            catch (SocketException ex)
            {
                _log.Error("NetworkClient", $"Failed to bind UDP port 5001: {ex.SocketErrorCode}", ex);
                _isDiscovering = false;
                return null;
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", "Error creating UDP listener", ex);
                _isDiscovering = false;
                return null;
            }

            var startTime = DateTime.Now;
            var endTime = startTime.AddSeconds(timeoutSeconds);

            try
            {
                while (DateTime.Now < endTime && !ct.IsCancellationRequested)
                {
                    try
                    {
                        // Use ReceiveAsync with timeout
                        var receiveTask = _udpListener.ReceiveAsync(ct).AsTask();
                        var timeoutTask = Task.Delay(3000, ct);

                        var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                        if (completedTask == receiveTask && !receiveTask.IsFaulted && !receiveTask.IsCanceled)
                        {
                            var result = await receiveTask;
                            var json = Encoding.UTF8.GetString(result.Buffer);

                            _log.Network("NetworkClient", $"UDP packet received from {result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port}");
                            _log.Debug("NetworkClient", $"Packet content: {json}");

                            try
                            {
                                var serverInfo = JsonSerializer.Deserialize<ServerDiscoveryInfo>(json);
                                if (serverInfo != null && !string.IsNullOrEmpty(serverInfo.ServerIp))
                                {
                                    _log.Info("NetworkClient", $"✓ Server found: {serverInfo.ClassName} at {serverInfo.ServerIp}:{serverInfo.ServerPort}");
                                    ServerDiscovered?.Invoke(this, serverInfo);
                                    return serverInfo;
                                }
                            }
                            catch (JsonException jsonEx)
                            {
                                _log.Warning("NetworkClient", $"Failed to parse server info: {jsonEx.Message}");
                            }
                        }
                        else
                        {
                            var elapsed = (DateTime.Now - startTime).TotalSeconds;
                            var remaining = timeoutSeconds - elapsed;
                            _log.Debug("NetworkClient", $"Waiting for broadcast... ({remaining:F0}s remaining)");
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (SocketException sockEx)
                    {
                        _log.Warning("NetworkClient", $"UDP receive error: {sockEx.SocketErrorCode} - {sockEx.Message}");
                        await Task.Delay(500, ct);
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("NetworkClient", $"Error receiving UDP: {ex.Message}");
                        await Task.Delay(500, ct);
                    }
                }

                _log.Warning("NetworkClient", $"Server discovery timeout after {timeoutSeconds} seconds");
            }
            catch (OperationCanceledException)
            {
                _log.Debug("NetworkClient", "Discovery cancelled");
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", "Error during discovery", ex);
            }
            finally
            {
                try { _udpListener?.Close(); } catch { }
                _udpListener = null;
                _isDiscovering = false;
            }

            return null;
        }

        /// <summary>
        /// Kết nối trực tiếp đến server theo IP
        /// </summary>
        public async Task<bool> ConnectDirectAsync(string serverIp, int port = 5000)
        {
            return await ConnectAsync(serverIp, port);
        }

        private void LogNetworkInfo()
        {
            _log.Debug("NetworkClient", "=== Network Configuration ===");
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    {
                        var ipProps = ni.GetIPProperties();
                        foreach (var addr in ipProps.UnicastAddresses)
                        {
                            if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                            {
                                _log.Debug("NetworkClient",
                                    $"  {ni.Name}: {addr.Address} / {addr.IPv4Mask} (Type: {ni.NetworkInterfaceType})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warning("NetworkClient", $"Could not enumerate network interfaces: {ex.Message}");
            }
        }

        /// <summary>
        /// Kết nối đến Server
        /// </summary>
        public async Task<bool> ConnectAsync(string serverIp, int port = 5000)
        {
            _log.Info("NetworkClient", $"Connecting to server at {serverIp}:{port}...");

            try
            {
                ServerIp = serverIp;
                ServerPort = port;

                // Create new CTS for connection
                _connectionCts?.Cancel();
                _connectionCts = new CancellationTokenSource();

                _tcpClient = new TcpClient();
                _tcpClient.ReceiveTimeout = 30000;
                _tcpClient.SendTimeout = 10000;

                // Connect with timeout
                _log.Debug("NetworkClient", "Starting TCP connect...");
                var connectTask = _tcpClient.ConnectAsync(serverIp, port);
                if (await Task.WhenAny(connectTask, Task.Delay(10000)) != connectTask)
                {
                    _log.Error("NetworkClient", $"Connection timeout after 10 seconds");
                    return false;
                }

                // Check if connect task threw an exception
                if (connectTask.IsFaulted)
                {
                    _log.Error("NetworkClient", $"Connection failed: {connectTask.Exception?.InnerException?.Message}");
                    return false;
                }

                if (!_tcpClient.Connected)
                {
                    _log.Error("NetworkClient", "TCP client not connected after ConnectAsync");
                    return false;
                }

                _stream = _tcpClient.GetStream();
                _log.Network("NetworkClient", $"✓ TCP connection established to {serverIp}:{port}");

                // Mark as connected FIRST so SendMessageAsync works
                IsConnected = true;

                // Send connect message
                var connectMsg = new NetworkMessage
                {
                    Type = MessageType.Connect,
                    SenderId = MachineId,
                    SenderName = DisplayName,
                    Payload = JsonSerializer.Serialize(new ClientInfo
                    {
                        MachineId = MachineId,
                        DisplayName = DisplayName,
                        ComputerName = Environment.MachineName,
                        IpAddress = GetLocalIP()
                    })
                };

                _log.Debug("NetworkClient", $"Sending Connect message: {DisplayName} ({MachineId})");

                // Send directly to stream (bypass IsConnected check)
                var json = JsonSerializer.Serialize(connectMsg);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(bytes);
                _log.Debug("NetworkClient", $"Sent Connect message ({bytes.Length} bytes)");

                // Start listening for messages
                _ = ListenForMessagesAsync(_connectionCts.Token);

                // Start heartbeat
                _ = HeartbeatAsync(_connectionCts.Token);

                Connected?.Invoke(this, EventArgs.Empty);
                _log.Info("NetworkClient", "✓ Successfully connected to server");
                return true;
            }
            catch (SocketException sockEx)
            {
                _log.Error("NetworkClient", $"Socket error connecting to {serverIp}:{port}: {sockEx.SocketErrorCode}", sockEx);
                return false;
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", $"Error connecting to server", ex);
                return false;
            }
        }

        /// <summary>
        /// Ngắt kết nối
        /// </summary>
        public void Disconnect()
        {
            _log.Info("NetworkClient", "Disconnecting...");

            try
            {
                if (IsConnected && _stream != null)
                {
                    var disconnectMsg = new NetworkMessage
                    {
                        Type = MessageType.Disconnect,
                        SenderId = MachineId
                    };
                    SendMessageAsync(disconnectMsg).Wait(1000);
                }
            }
            catch (Exception ex)
            {
                _log.Warning("NetworkClient", $"Error sending disconnect: {ex.Message}");
            }
            finally
            {
                try { _connectionCts?.Cancel(); } catch { }
                IsConnected = false;
                try { _stream?.Close(); } catch { }
                try { _tcpClient?.Close(); } catch { }
                _log.Info("NetworkClient", "Disconnected");
                Disconnected?.Invoke(this, "User disconnected");
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken ct)
        {
            var buffer = new byte[1024 * 1024]; // 1MB buffer for screen data
            _log.Debug("NetworkClient", "Started message listener");

            try
            {
                while (!ct.IsCancellationRequested && _stream != null && _tcpClient?.Connected == true)
                {
                    int bytesRead;
                    try
                    {
                        bytesRead = await _stream.ReadAsync(buffer, ct);
                    }
                    catch (Exception readEx)
                    {
                        if (!ct.IsCancellationRequested)
                        {
                            _log.Warning("NetworkClient", $"Read error: {readEx.Message}");
                        }
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        _log.Network("NetworkClient", "Server closed connection (0 bytes received)");
                        break;
                    }

                    var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _log.Debug("NetworkClient", $"Received {bytesRead} bytes");

                    try
                    {
                        var message = JsonSerializer.Deserialize<NetworkMessage>(json);
                        if (message != null)
                        {
                            HandleMessage(message);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _log.Warning("NetworkClient", $"JSON parse error: {jsonEx.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _log.Debug("NetworkClient", "Message listener cancelled");
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", "Error in message listener", ex);
            }
            finally
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    _log.Warning("NetworkClient", "Connection lost");
                    Disconnected?.Invoke(this, "Connection lost");
                }
            }
        }

        private void HandleMessage(NetworkMessage message)
        {
            _log.Debug("NetworkClient", $"Handling message type: {message.Type}");

            switch (message.Type)
            {
                case MessageType.ConnectAck:
                    _log.Info("NetworkClient", "✓ Received ConnectAck from server");
                    break;

                case MessageType.Heartbeat:
                    // Server is alive
                    break;

                case MessageType.ScreenShare:
                    if (message.Payload != null)
                    {
                        try
                        {
                            var imageData = Convert.FromBase64String(message.Payload);
                            _log.Debug("NetworkClient", $"Screen share frame received: {imageData.Length} bytes");
                            ScreenShareReceived?.Invoke(this, imageData);
                        }
                        catch (Exception ex)
                        {
                            _log.Warning("NetworkClient", $"Error decoding screen share: {ex.Message}");
                        }
                    }
                    break;

                case MessageType.ScreenShareStop:
                    _log.Info("NetworkClient", "Screen share stopped");
                    ScreenShareReceived?.Invoke(this, Array.Empty<byte>());
                    break;

                case MessageType.LockScreen:
                    _log.Info("NetworkClient", "Screen lock command received");
                    ScreenLocked?.Invoke(this, EventArgs.Empty);
                    break;

                case MessageType.UnlockScreen:
                    _log.Info("NetworkClient", "Screen unlock command received");
                    ScreenUnlocked?.Invoke(this, EventArgs.Empty);
                    break;

                case MessageType.ControlStart:
                    _log.Info("NetworkClient", "Remote control started by teacher");
                    RemoteControlStarted?.Invoke(this, EventArgs.Empty);
                    break;

                case MessageType.ControlStop:
                    _log.Info("NetworkClient", "Remote control stopped by teacher");
                    RemoteControlStopped?.Invoke(this, EventArgs.Empty);
                    break;

                case MessageType.ControlMouse:
                    if (message.Payload != null)
                    {
                        try
                        {
                            _log.Debug("NetworkClient", "Received ControlMouse message"); // Add logging
                            var cmd = JsonSerializer.Deserialize<MouseCommand>(message.Payload);
                            if (cmd != null)
                            {
                                InputSimulationService.Instance.SimulateMouse(cmd);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Warning("NetworkClient", $"Error processing mouse control: {ex.Message}");
                        }
                    }
                    break;

                case MessageType.ControlKeyboard:
                    if (message.Payload != null)
                    {
                        try
                        {
                            var cmd = JsonSerializer.Deserialize<KeyboardCommand>(message.Payload);
                            if (cmd != null)
                            {
                                InputSimulationService.Instance.SimulateKeyboard(cmd);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Warning("NetworkClient", $"Error processing keyboard control: {ex.Message}");
                        }
                    }
                    break;

                case MessageType.SystemSpecsRequest:
                    _log.Info("NetworkClient", "System specs request received from teacher");
                    _ = RespondWithSystemSpecs();
                    break;

                case MessageType.ProcessListRequest:
                    _ = RespondWithProcessList();
                    break;

                case MessageType.ProcessKillCommand:
                    HandleProcessKillCommand(message);
                    break;

                case MessageType.FileCollectionRequest:
                    if (message.Payload != null)
                    {
                        try
                        {
                            var request = JsonSerializer.Deserialize<FileCollectionRequest>(message.Payload);
                            if (request != null)
                            {
                                _ = _fileCollectionService.StartCollectionAsync(request);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error("NetworkClient", "Error processing file collection request", ex);
                        }
                    }
                    break;

                case MessageType.BulkFileTransferRequest:
                     if (message.Payload != null)
                     {
                         var req = JsonSerializer.Deserialize<BulkFileTransferRequest>(message.Payload);
                         if (req != null) FileReceiverService.Instance.HandleRequest(req);
                     }
                     break;

                case MessageType.BulkFileData:
                     if (message.Payload != null)
                     {
                         var chunk = JsonSerializer.Deserialize<BulkFileDataChunk>(message.Payload);
                         if (chunk != null) _ = FileReceiverService.Instance.HandleChunkAsync(chunk);
                     }
                     break;

                case MessageType.PollStart:
                    if (message.Payload != null)
                    {
                        var poll = JsonSerializer.Deserialize<Poll>(message.Payload);
                        if (poll != null) PollService.Instance.HandlePollStart(poll);
                    }
                    break;

                case MessageType.PollStop:
                    PollService.Instance.HandlePollStop();
                    break;

                case MessageType.PollUpdate:
                    if (message.Payload != null)
                    {
                        var update = JsonSerializer.Deserialize<PollResultUpdate>(message.Payload);
                        if (update != null) PollService.Instance.HandlePollUpdate(update);
                    }
                    break;

                case MessageType.ScreenshotCaptureRequest:
                    try
                    {
                        var request = message.Payload != null
                            ? JsonSerializer.Deserialize<ScreenshotRequest>(message.Payload)
                            : new ScreenshotRequest();

                        // If TargetStudentId is specified and doesn't match ours, ignore (though usually server filters this)
                        if (request != null && (string.IsNullOrEmpty(request.TargetStudentId) || request.TargetStudentId == MachineId))
                        {
                            _log.Info("NetworkClient", "Screenshot request received");
                            ScreenshotRequested?.Invoke(this, request);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.Warning("NetworkClient", $"Error processing screenshot request: {ex.Message}");
                    }
                    break;

                default:
                    MessageReceived?.Invoke(this, message);
                    break;
            }
        }

        private async Task RespondWithProcessList()
        {
            try
            {
                var processes = ProcessManagerService.Instance.GetRunningProcesses();
                var response = new NetworkMessage
                {
                    Type = MessageType.ProcessListResponse,
                    SenderId = MachineId,
                    SenderName = DisplayName,
                    Payload = JsonSerializer.Serialize(processes)
                };
                await SendMessageAsync(response);
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", "Error sending process list", ex);
            }
        }

        private void HandleProcessKillCommand(NetworkMessage message)
        {
            if (message.Payload == null) return;
            try
            {
                var action = JsonSerializer.Deserialize<ProcessAction>(message.Payload);
                if (action != null)
                {
                    bool success = ProcessManagerService.Instance.KillProcess(action.ProcessId);
                    if (success)
                    {
                        // Optional: Send ack or notification
                         _log.Info("NetworkClient", $"Killed process {action.ProcessId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", "Error executing kill command", ex);
            }
        }

        private async Task RespondWithSystemSpecs()
        {
            try
            {
                var package = SystemInfoService.Instance.GetFullSystemInfo(MachineId, DisplayName);
                var response = new NetworkMessage
                {
                    Type = MessageType.SystemSpecsResponse,
                    SenderId = MachineId,
                    SenderName = DisplayName,
                    Payload = JsonSerializer.Serialize(package)
                };
                await SendMessageAsync(response);
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", "Error collecting system specs", ex);
            }
        }

        private async Task HeartbeatAsync(CancellationToken ct)
        {
            _log.Debug("NetworkClient", "Started heartbeat");

            while (!ct.IsCancellationRequested && IsConnected)
            {
                try
                {
                    await Task.Delay(10000, ct); // Every 10 seconds

                    if (IsConnected)
                    {
                        await SendMessageAsync(new NetworkMessage
                        {
                            Type = MessageType.Heartbeat,
                            SenderId = MachineId
                        });
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _log.Warning("NetworkClient", $"Heartbeat error: {ex.Message}");
                }
            }
        }

        public async Task SendMessageAsync(NetworkMessage message)
        {
            if (_stream == null || !IsConnected)
            {
                _log.Warning("NetworkClient", "Cannot send message: not connected");
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(bytes);
                if (message.Type != MessageType.Heartbeat)
                {
                    _log.Debug("NetworkClient", $"Sent {message.Type} ({bytes.Length} bytes)");
                }
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", $"Send error", ex);
            }
        }

        public async Task SendScreenDataAsync(byte[] imageData, int width, int height)
        {
            var screenData = new ScreenData
            {
                ClientId = MachineId,
                ImageData = imageData,
                Width = width,
                Height = height,
                CaptureTime = DateTime.Now
            };

            var message = new NetworkMessage
            {
                Type = MessageType.ScreenData,
                SenderId = MachineId,
                Payload = JsonSerializer.Serialize(screenData)
            };

            await SendMessageAsync(message);
        }

        public async Task SendChatMessageAsync(string content, int? receiverId = null)
        {
            var message = new NetworkMessage
            {
                Type = receiverId.HasValue ? MessageType.ChatPrivate : MessageType.ChatMessage,
                SenderId = MachineId,
                SenderName = DisplayName,
                TargetId = receiverId?.ToString(),
                Payload = content
            };
            await SendMessageAsync(message);
        }

        public async Task SubmitAssignmentAsync(AssignmentSubmission submission)
        {
            var message = new NetworkMessage
            {
                Type = MessageType.AssignmentSubmit,
                SenderId = MachineId,
                SenderName = DisplayName,
                Payload = JsonSerializer.Serialize(submission)
            };
            await SendMessageAsync(message);
        }

        public async Task RaiseHandAsync(bool raise)
        {
            var message = new NetworkMessage
            {
                Type = raise ? MessageType.RaiseHand : MessageType.LowerHand,
                SenderId = MachineId,
                SenderName = DisplayName
            };
            await SendMessageAsync(message);
            _log.Info("NetworkClient", $"Raise hand: {raise}");
        }

        private static string GetMachineId()
        {
            // Use a combination of machine name and a stable identifier
            var machineName = Environment.MachineName;
            var userName = Environment.UserName;
            return $"{machineName}_{userName}_{Guid.NewGuid().ToString("N")[..6]}";
        }

        private static string GetLocalIP()
        {
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

            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }

        public void Dispose()
        {
            CancelDiscovery();
            Disconnect();
            try { _discoveryCts?.Dispose(); } catch { }
            try { _connectionCts?.Dispose(); } catch { }
            try { _udpListener?.Dispose(); } catch { }
        }
    }
}
