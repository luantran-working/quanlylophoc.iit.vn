using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
    /// TCP Server cho máy Giáo viên
    /// </summary>
    public class NetworkServerService : IDisposable
    {
        private TcpListener? _tcpListener;
        private UdpClient? _udpBroadcaster;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private readonly ConcurrentDictionary<string, string> _clientNames = new();
        private readonly LogService _log = LogService.Instance;

        public int Port { get; private set; } = 5000;
        public int DiscoveryPort { get; private set; } = 5001;
        public bool IsRunning { get; private set; }

        public string ClassName { get; set; } = "Lớp học";
        public string TeacherName { get; set; } = "Giáo viên";
        public string ServerIp { get; private set; } = "";

        // Events
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public event EventHandler<ScreenDataReceivedEventArgs>? ScreenDataReceived;
        public event EventHandler<ScreenDataReceivedEventArgs>? ScreenshotReceived;

        public async Task StartAsync(int port = 5000)
        {
            if (IsRunning)
            {
                _log.Warning("NetworkServer", "Server already running, skipping start");
                return;
            }

            Port = port;
            _cts = new CancellationTokenSource();

            try
            {
                // Get local IP
                ServerIp = GetLocalIPAddress();
                _log.Network("NetworkServer", $"Local IP detected: {ServerIp}");

                // Log network interfaces
                LogNetworkInterfaces();

                // Start TCP Listener
                _log.Network("NetworkServer", $"Starting TCP listener on port {Port}...");
                _tcpListener = new TcpListener(IPAddress.Any, Port);
                _tcpListener.Start();
                _log.Info("NetworkServer", $"TCP Server started on {ServerIp}:{Port}");

                IsRunning = true;

                // Start accepting clients
                _ = AcceptClientsAsync(_cts.Token);

                // Start UDP Discovery Broadcast
                _ = BroadcastDiscoveryAsync(_cts.Token);

                await Task.CompletedTask;
            }
            catch (SocketException ex)
            {
                _log.Error("NetworkServer", $"Failed to start TCP listener on port {Port}", ex);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("NetworkServer", "Unexpected error starting server", ex);
                throw;
            }
        }

        private void LogNetworkInterfaces()
        {
            _log.Debug("NetworkServer", "=== Network Interfaces ===");
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up &&
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
                     ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
                {
                    var ipProps = ni.GetIPProperties();
                    foreach (var addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            _log.Debug("NetworkServer",
                                $"  {ni.Name}: {addr.Address} (Type: {ni.NetworkInterfaceType})");
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            _log.Info("NetworkServer", "Stopping server...");
            _cts?.Cancel();
            IsRunning = false;

            foreach (var clientId in _clients.Keys)
            {
                try
                {
                    if (_clients.TryRemove(clientId, out var client))
                    {
                        client.Close();
                        _log.Debug("NetworkServer", $"Closed connection to client: {clientId}");
                    }
                }
                catch (Exception ex)
                {
                    _log.Warning("NetworkServer", $"Error closing client {clientId}: {ex.Message}");
                }
            }
            _clients.Clear();
            _clientNames.Clear();

            try { _tcpListener?.Stop(); } catch { }
            try { _udpBroadcaster?.Close(); } catch { }

            _log.Info("NetworkServer", "Server stopped");
        }

        private async Task AcceptClientsAsync(CancellationToken ct)
        {
            _log.Info("NetworkServer", "Started accepting clients...");

            while (!ct.IsCancellationRequested && _tcpListener != null)
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(ct);
                    var remoteEndpoint = (IPEndPoint?)client.Client.RemoteEndPoint;
                    _log.Network("NetworkServer", $"New connection from {remoteEndpoint?.Address}:{remoteEndpoint?.Port}");

                    _ = HandleClientAsync(client, ct);
                }
                catch (OperationCanceledException)
                {
                    _log.Debug("NetworkServer", "Accept loop cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    _log.Error("NetworkServer", "Error accepting client", ex);
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            string? clientId = null;
            var remoteEndpoint = (IPEndPoint?)client.Client.RemoteEndPoint;
            var clientAddress = remoteEndpoint?.Address.ToString() ?? "unknown";

            _log.Debug("NetworkServer", $"Handling client from {clientAddress}...");

            var stream = client.GetStream();
            var buffer = new byte[50 * 1024 * 1024]; // 50MB buffer to handle large file submissions

            try
            {
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    // Set read timeout
                    stream.ReadTimeout = 60000; // 60 seconds

                    int bytesRead;
                    try
                    {
                        bytesRead = await stream.ReadAsync(buffer, ct);
                    }
                    catch (IOException ioEx)
                    {
                        _log.Warning("NetworkServer", $"Read timeout or IO error from {clientAddress}: {ioEx.Message}");
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        _log.Network("NetworkServer", $"Client {clientAddress} closed connection (0 bytes)");
                        break;
                    }

                    var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _log.Debug("NetworkServer", $"Received {bytesRead} bytes from {clientAddress}: {json.Substring(0, Math.Min(200, json.Length))}...");

                    NetworkMessage? message;
                    try
                    {
                        message = JsonSerializer.Deserialize<NetworkMessage>(json);
                    }
                    catch (JsonException jsonEx)
                    {
                        _log.Warning("NetworkServer", $"JSON parse error from {clientAddress}: {jsonEx.Message}");
                        continue;
                    }

                    if (message == null)
                    {
                        _log.Warning("NetworkServer", $"Null message from {clientAddress}");
                        continue;
                    }

                    _log.Network("NetworkServer", $"Message type {message.Type} from {message.SenderName ?? clientAddress}");

                    switch (message.Type)
                    {
                        case MessageType.Connect:
                            clientId = message.SenderId;
                            _clients[clientId] = client;
                            _clientNames[clientId] = message.SenderName;

                            _log.Info("NetworkServer", $"Client registered: {message.SenderName} (ID: {clientId})");

                            // Send ACK
                            var ack = new NetworkMessage
                            {
                                Type = MessageType.ConnectAck,
                                SenderId = "server",
                                Payload = JsonSerializer.Serialize(new { ClassName, TeacherName })
                            };
                            await SendToClientAsync(clientId, ack);
                            _log.Debug("NetworkServer", $"Sent ConnectAck to {clientId}");

                            ClientConnected?.Invoke(this, new ClientConnectedEventArgs
                            {
                                ClientId = clientId,
                                ClientName = message.SenderName,
                                IpAddress = clientAddress,
                                ClientInfo = message.Payload != null
                                    ? JsonSerializer.Deserialize<ClientInfo>(message.Payload)
                                    : null
                            });
                            break;

                        case MessageType.Disconnect:
                            _log.Info("NetworkServer", $"Client {clientId} sent disconnect");
                            throw new Exception("Client disconnected");

                        case MessageType.Heartbeat:
                            // Reply heartbeat
                            if (clientId != null)
                            {
                                await SendToClientAsync(clientId, new NetworkMessage { Type = MessageType.Heartbeat });
                            }
                            break;

                        case MessageType.ScreenData:
                            if (message.Payload != null)
                            {
                                try
                                {
                                    var screenData = JsonSerializer.Deserialize<ScreenData>(message.Payload);
                                    if (screenData != null)
                                    {
                                        ScreenDataReceived?.Invoke(this, new ScreenDataReceivedEventArgs
                                        {
                                            ClientId = message.SenderId,
                                            ScreenData = screenData
                                        });
                                    }
                                }
                                catch (JsonException)
                                {
                                    _log.Warning("NetworkServer", $"Failed to parse ScreenData from {clientId}");
                                }
                            }
                            break;

                        case MessageType.ScreenshotCaptureData:
                            if (message.Payload != null)
                            {
                                try
                                {
                                    // Reuse ScreenData for screenshot payload as it has the same structure
                                    var screenData = JsonSerializer.Deserialize<ScreenData>(message.Payload);
                                    if (screenData != null)
                                    {
                                        ScreenshotReceived?.Invoke(this, new ScreenDataReceivedEventArgs
                                        {
                                            ClientId = message.SenderId,
                                            ScreenData = screenData
                                        });
                                    }
                                }
                                catch (JsonException)
                                {
                                    _log.Warning("NetworkServer", $"Failed to parse ScreenshotCaptureData from {clientId}");
                                }
                            }
                            break;

                        default:
                            MessageReceived?.Invoke(this, new MessageReceivedEventArgs
                            {
                                ClientId = message.SenderId,
                                Message = message
                            });
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warning("NetworkServer", $"Client handler error for {clientAddress}: {ex.Message}");
            }
            finally
            {
                if (clientId != null)
                {
                    _clients.TryRemove(clientId, out _);
                    _clientNames.TryRemove(clientId, out var clientName);

                    _log.Info("NetworkServer", $"Client disconnected: {clientName ?? clientId}");

                    ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs
                    {
                        ClientId = clientId,
                        ClientName = clientName ?? ""
                    });
                }
                try { client.Close(); } catch { }
            }
        }

        private async Task BroadcastDiscoveryAsync(CancellationToken ct)
        {
            _log.Info("NetworkServer", $"Starting UDP broadcast on port {DiscoveryPort}...");

            try
            {
                _udpBroadcaster = new UdpClient();
                _udpBroadcaster.EnableBroadcast = true;

                // Bind to any available port for sending
                _udpBroadcaster.Client.Bind(new IPEndPoint(IPAddress.Any, 0));
                _log.Debug("NetworkServer", $"UDP client bound to local port {((IPEndPoint)_udpBroadcaster.Client.LocalEndPoint!).Port}");
            }
            catch (Exception ex)
            {
                _log.Error("NetworkServer", "Failed to create UDP broadcaster", ex);
                return;
            }

            var localIp = GetLocalIPAddress();

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var info = new ServerDiscoveryInfo
                    {
                        ServerIp = localIp,
                        ServerPort = Port,
                        ClassName = ClassName,
                        TeacherName = TeacherName,
                        OnlineCount = _clients.Count
                    };

                    var json = JsonSerializer.Serialize(info);
                    var bytes = Encoding.UTF8.GetBytes(json);

                    // Broadcast to 255.255.255.255
                    var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
                    await _udpBroadcaster.SendAsync(bytes, bytes.Length, broadcastEndpoint);

                    // Also try sending to subnet broadcast (e.g., 192.168.1.255)
                    var subnetBroadcast = GetSubnetBroadcast(localIp);
                    if (subnetBroadcast != null)
                    {
                        var subnetEndpoint = new IPEndPoint(IPAddress.Parse(subnetBroadcast), DiscoveryPort);
                        await _udpBroadcaster.SendAsync(bytes, bytes.Length, subnetEndpoint);
                    }

                    _log.Debug("NetworkServer", $"UDP broadcast sent: {ClassName} at {localIp}:{Port} ({_clients.Count} clients)");

                    await Task.Delay(3000, ct); // Broadcast every 3 seconds
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _log.Warning("NetworkServer", $"Broadcast error: {ex.Message}");
                    await Task.Delay(1000, ct);
                }
            }

            _log.Debug("NetworkServer", "UDP broadcast stopped");
        }

        private string? GetSubnetBroadcast(string ip)
        {
            try
            {
                var parts = ip.Split('.');
                if (parts.Length == 4)
                {
                    return $"{parts[0]}.{parts[1]}.{parts[2]}.255";
                }
            }
            catch { }
            return null;
        }

        public async Task SendToClientAsync(string clientId, NetworkMessage message)
        {
            if (_clients.TryGetValue(clientId, out var client) && client.Connected)
            {
                try
                {
                    var json = JsonSerializer.Serialize(message);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await client.GetStream().WriteAsync(bytes);
                    _log.Debug("NetworkServer", $"Sent {message.Type} to {clientId} ({bytes.Length} bytes)");
                }
                catch (Exception ex)
                {
                    _log.Warning("NetworkServer", $"Failed to send to {clientId}: {ex.Message}");
                }
            }
            else
            {
                _log.Warning("NetworkServer", $"Cannot send to {clientId}: client not found or disconnected");
            }
        }

        public async Task BroadcastToAllAsync(NetworkMessage message)
        {
            _log.Debug("NetworkServer", $"Broadcasting {message.Type} to {_clients.Count} clients");
            var tasks = new List<Task>();
            foreach (var clientId in _clients.Keys)
            {
                tasks.Add(SendToClientAsync(clientId, message));
            }
            await Task.WhenAll(tasks);
        }

        public async Task SendScreenShareAsync(byte[] imageData)
        {
            var message = new NetworkMessage
            {
                Type = MessageType.ScreenShare,
                SenderId = "server",
                Payload = Convert.ToBase64String(imageData)
            };
            await BroadcastToAllAsync(message);
        }

        public int GetOnlineCount() => _clients.Count;

        public IEnumerable<string> GetConnectedClientIds() => _clients.Keys;

        private static string GetLocalIPAddress()
        {
            try
            {
                // Try to get the IP that can reach the internet
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null)
                {
                    return endPoint.Address.ToString();
                }
            }
            catch { }

            // Fallback: enumerate network interfaces
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(ip))
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
            Stop();
            _cts?.Dispose();
        }
    }

    // Event Args
    public class ClientConnectedEventArgs : EventArgs
    {
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public ClientInfo? ClientInfo { get; set; }
    }

    public class ClientDisconnectedEventArgs : EventArgs
    {
        public string ClientId { get; set; } = "";
        public string ClientName { get; set; } = "";
    }

    public class MessageReceivedEventArgs : EventArgs
    {
        public string ClientId { get; set; } = "";
        public NetworkMessage Message { get; set; } = new();
    }

    public class ScreenDataReceivedEventArgs : EventArgs
    {
        public string ClientId { get; set; } = "";
        public ScreenData ScreenData { get; set; } = new();
    }
}
