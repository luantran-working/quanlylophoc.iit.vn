using System;
using System.Net;
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
        }

        /// <summary>
        /// Tìm Server trong mạng LAN bằng UDP
        /// </summary>
        public async Task<ServerDiscoveryInfo?> DiscoverServerAsync(int timeoutSeconds = 30)
        {
            _cts = new CancellationTokenSource();
            _udpListener = new UdpClient(5001);
            _udpListener.EnableBroadcast = true;

            var endTime = DateTime.Now.AddSeconds(timeoutSeconds);

            try
            {
                while (DateTime.Now < endTime && !_cts.Token.IsCancellationRequested)
                {
                    if (_udpListener.Available > 0)
                    {
                        var endpoint = new IPEndPoint(IPAddress.Any, 5001);
                        var data = _udpListener.Receive(ref endpoint);
                        var json = Encoding.UTF8.GetString(data);
                        
                        var serverInfo = JsonSerializer.Deserialize<ServerDiscoveryInfo>(json);
                        if (serverInfo != null)
                        {
                            ServerDiscovered?.Invoke(this, serverInfo);
                            return serverInfo;
                        }
                    }
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Discovery error: {ex.Message}");
            }
            finally
            {
                _udpListener?.Close();
            }

            return null;
        }

        /// <summary>
        /// Kết nối đến Server
        /// </summary>
        public async Task<bool> ConnectAsync(string serverIp, int port = 5000)
        {
            try
            {
                ServerIp = serverIp;
                ServerPort = port;
                _cts = new CancellationTokenSource();

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(serverIp, port);
                _stream = _tcpClient.GetStream();
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
                await SendMessageAsync(connectMsg);

                // Start listening for messages
                _ = ListenForMessagesAsync(_cts.Token);

                // Start heartbeat
                _ = HeartbeatAsync(_cts.Token);

                Connected?.Invoke(this, EventArgs.Empty);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connect error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ngắt kết nối
        /// </summary>
        public void Disconnect()
        {
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
            catch { }
            finally
            {
                _cts?.Cancel();
                IsConnected = false;
                _stream?.Close();
                _tcpClient?.Close();
                Disconnected?.Invoke(this, "User disconnected");
            }
        }

        private async Task ListenForMessagesAsync(CancellationToken ct)
        {
            var buffer = new byte[1024 * 256]; // 256KB buffer for screen data

            try
            {
                while (!ct.IsCancellationRequested && _stream != null && _tcpClient?.Connected == true)
                {
                    var bytesRead = await _stream.ReadAsync(buffer, ct);
                    if (bytesRead == 0) break;

                    var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    // Handle multiple messages in buffer
                    try
                    {
                        var message = JsonSerializer.Deserialize<NetworkMessage>(json);
                        if (message != null)
                        {
                            HandleMessage(message);
                        }
                    }
                    catch (JsonException)
                    {
                        // May have partial message, handle appropriately
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Listen error: {ex.Message}");
            }
            finally
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    Disconnected?.Invoke(this, "Connection lost");
                }
            }
        }

        private void HandleMessage(NetworkMessage message)
        {
            switch (message.Type)
            {
                case MessageType.Heartbeat:
                    // Server is alive
                    break;

                case MessageType.ScreenShare:
                    if (message.Payload != null)
                    {
                        var imageData = Convert.FromBase64String(message.Payload);
                        ScreenShareReceived?.Invoke(this, imageData);
                    }
                    break;

                case MessageType.ScreenShareStop:
                    ScreenShareReceived?.Invoke(this, Array.Empty<byte>());
                    break;

                case MessageType.LockScreen:
                    ScreenLocked?.Invoke(this, EventArgs.Empty);
                    break;

                case MessageType.UnlockScreen:
                    ScreenUnlocked?.Invoke(this, EventArgs.Empty);
                    break;

                default:
                    MessageReceived?.Invoke(this, message);
                    break;
            }
        }

        private async Task HeartbeatAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && IsConnected)
            {
                try
                {
                    await Task.Delay(10000, ct); // Every 10 seconds
                    await SendMessageAsync(new NetworkMessage
                    {
                        Type = MessageType.Heartbeat,
                        SenderId = MachineId
                    });
                }
                catch (OperationCanceledException) { break; }
                catch { }
            }
        }

        public async Task SendMessageAsync(NetworkMessage message)
        {
            if (_stream == null || !IsConnected) return;

            try
            {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(bytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Send error: {ex.Message}");
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
        }

        private static string GetMachineId()
        {
            // Use a combination of machine name and MAC address for unique ID
            var machineName = Environment.MachineName;
            var guid = Guid.NewGuid().ToString("N")[..8]; // For uniqueness across sessions
            return $"{machineName}_{guid}";
        }

        private static string GetLocalIP()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
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
