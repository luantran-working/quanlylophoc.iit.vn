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
        private CancellationTokenSource? _cts;
        private readonly LogService _log = LogService.Instance;
        
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

        public NetworkClientService()
        {
            MachineId = GetMachineId();
            _log.Info("NetworkClient", $"Initialized with MachineId: {MachineId}");
        }

        /// <summary>
        /// Tìm Server trong mạng LAN bằng UDP
        /// </summary>
        public async Task<ServerDiscoveryInfo?> DiscoverServerAsync(int timeoutSeconds = 30)
        {
            _log.Info("NetworkClient", $"Starting server discovery (timeout: {timeoutSeconds}s)...");
            _log.Network("NetworkClient", $"Listening for UDP broadcasts on port 5001");
            
            // Log network info
            LogNetworkInfo();
            
            _cts = new CancellationTokenSource();

            try
            {
                _udpListener = new UdpClient();
                _udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _udpListener.Client.Bind(new IPEndPoint(IPAddress.Any, 5001));
                _udpListener.EnableBroadcast = true;
                
                _log.Debug("NetworkClient", "UDP listener bound to port 5001");
            }
            catch (SocketException ex)
            {
                _log.Error("NetworkClient", $"Failed to bind UDP port 5001", ex);
                
                // Try alternative port
                try
                {
                    _udpListener = new UdpClient(0);
                    _udpListener.EnableBroadcast = true;
                    _log.Warning("NetworkClient", "Bound to alternative UDP port");
                }
                catch (Exception ex2)
                {
                    _log.Error("NetworkClient", "Failed to create UDP listener", ex2);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _log.Error("NetworkClient", "Error creating UDP listener", ex);
                return null;
            }

            var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
            int attemptCount = 0;

            try
            {
                while (DateTime.Now < endTime && !_cts.Token.IsCancellationRequested)
                {
                    attemptCount++;
                    
                    // Check for available data with polling
                    if (_udpListener.Available > 0)
                    {
                        try
                        {
                            var endpoint = new IPEndPoint(IPAddress.Any, 0);
                            var data = _udpListener.Receive(ref endpoint);
                            var json = Encoding.UTF8.GetString(data);
                            
                            _log.Network("NetworkClient", $"UDP packet received from {endpoint.Address}:{endpoint.Port}");
                            _log.Debug("NetworkClient", $"Packet content: {json}");
                            
                            var serverInfo = JsonSerializer.Deserialize<ServerDiscoveryInfo>(json);
                            if (serverInfo != null && !string.IsNullOrEmpty(serverInfo.ServerIp))
                            {
                                _log.Info("NetworkClient", $"Server found: {serverInfo.ClassName} at {serverInfo.ServerIp}:{serverInfo.ServerPort}");
                                ServerDiscovered?.Invoke(this, serverInfo);
                                return serverInfo;
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            _log.Warning("NetworkClient", $"Failed to parse server info: {jsonEx.Message}");
                        }
                        catch (Exception recvEx)
                        {
                            _log.Warning("NetworkClient", $"Error receiving UDP: {recvEx.Message}");
                        }
                    }
                    
                    // Log progress every 5 seconds
                    if (attemptCount % 50 == 0)
                    {
                        var remaining = (endTime - DateTime.Now).TotalSeconds;
                        _log.Debug("NetworkClient", $"Still searching... ({remaining:F0}s remaining)");
                    }
                    
                    await Task.Delay(100, _cts.Token);
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
            }

            return null;
        }

        private void LogNetworkInfo()
        {
            _log.Debug("NetworkClient", "=== Network Configuration ===");
            try
            {
                foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
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
                _cts = new CancellationTokenSource();

                _tcpClient = new TcpClient();
                _tcpClient.ReceiveTimeout = 30000;
                _tcpClient.SendTimeout = 10000;
                
                // Connect with timeout
                var connectTask = _tcpClient.ConnectAsync(serverIp, port);
                if (await Task.WhenAny(connectTask, Task.Delay(10000)) != connectTask)
                {
                    _log.Error("NetworkClient", $"Connection timeout after 10 seconds");
                    return false;
                }
                
                if (!_tcpClient.Connected)
                {
                    _log.Error("NetworkClient", "TCP client not connected after ConnectAsync");
                    return false;
                }
                
                _stream = _tcpClient.GetStream();
                _log.Network("NetworkClient", $"TCP connection established to {serverIp}:{port}");

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
                await SendMessageAsync(connectMsg);

                IsConnected = true;
                
                // Start listening for messages
                _ = ListenForMessagesAsync(_cts.Token);

                // Start heartbeat
                _ = HeartbeatAsync(_cts.Token);

                Connected?.Invoke(this, EventArgs.Empty);
                _log.Info("NetworkClient", "Successfully connected to server");
                return true;
            }
            catch (SocketException sockEx)
            {
                _log.Error("NetworkClient", $"Socket error connecting to {serverIp}:{port}", sockEx);
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
                _cts?.Cancel();
                IsConnected = false;
                try { _stream?.Close(); } catch { }
                try { _tcpClient?.Close(); } catch { }
                _log.Info("NetworkClient", "Disconnected");
                Disconnected?.Invoke(this, "User disconnected");
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken ct)
        {
            var buffer = new byte[1024 * 256]; // 256KB buffer for screen data
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
                        _log.Warning("NetworkClient", $"Read error: {readEx.Message}");
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
                    _log.Info("NetworkClient", "Received ConnectAck from server");
                    break;
                    
                case MessageType.Heartbeat:
                    // Server is alive
                    _log.Debug("NetworkClient", "Heartbeat received");
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

                default:
                    MessageReceived?.Invoke(this, message);
                    break;
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
                        _log.Debug("NetworkClient", "Heartbeat sent");
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
                _log.Debug("NetworkClient", $"Sent {message.Type} ({bytes.Length} bytes)");
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
            Disconnect();
            _cts?.Dispose();
            _udpListener?.Dispose();
        }
    }
}
