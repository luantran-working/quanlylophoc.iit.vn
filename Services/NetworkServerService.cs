using System;
using System.Collections.Concurrent;
using System.IO;
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
    /// TCP Server cho máy Giáo viên
    /// </summary>
    public class NetworkServerService : IDisposable
    {
        private TcpListener? _tcpListener;
        private UdpClient? _udpBroadcaster;
        private CancellationTokenSource? _cts;
        private readonly ConcurrentDictionary<string, TcpClient> _clients = new();
        private readonly ConcurrentDictionary<string, string> _clientNames = new();
        
        public int Port { get; private set; } = 5000;
        public int DiscoveryPort { get; private set; } = 5001;
        public bool IsRunning { get; private set; }
        
        public string ClassName { get; set; } = "Lớp học";
        public string TeacherName { get; set; } = "Giáo viên";

        // Events
        public event EventHandler<ClientConnectedEventArgs>? ClientConnected;
        public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;
        public event EventHandler<MessageReceivedEventArgs>? MessageReceived;
        public event EventHandler<ScreenDataReceivedEventArgs>? ScreenDataReceived;

        public async Task StartAsync(int port = 5000)
        {
            if (IsRunning) return;

            Port = port;
            _cts = new CancellationTokenSource();

            // Start TCP Listener
            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            IsRunning = true;

            // Start accepting clients
            _ = AcceptClientsAsync(_cts.Token);

            // Start UDP Discovery Broadcast
            _ = BroadcastDiscoveryAsync(_cts.Token);

            await Task.CompletedTask;
        }

        public void Stop()
        {
            _cts?.Cancel();
            IsRunning = false;

            foreach (var client in _clients.Values)
            {
                try { client.Close(); } catch { }
            }
            _clients.Clear();
            _clientNames.Clear();

            try { _tcpListener?.Stop(); } catch { }
            try { _udpBroadcaster?.Close(); } catch { }
        }

        private async Task AcceptClientsAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _tcpListener != null)
            {
                try
                {
                    var client = await _tcpListener.AcceptTcpClientAsync(ct);
                    _ = HandleClientAsync(client, ct);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Accept error: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken ct)
        {
            string? clientId = null;
            var stream = client.GetStream();
            var buffer = new byte[1024 * 64]; // 64KB buffer

            try
            {
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, ct);
                    if (bytesRead == 0) break;

                    var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var message = JsonSerializer.Deserialize<NetworkMessage>(json);
                    
                    if (message == null) continue;

                    switch (message.Type)
                    {
                        case MessageType.Connect:
                            clientId = message.SenderId;
                            _clients[clientId] = client;
                            _clientNames[clientId] = message.SenderName;
                            
                            // Send ACK
                            var ack = new NetworkMessage
                            {
                                Type = MessageType.ConnectAck,
                                SenderId = "server",
                                Payload = JsonSerializer.Serialize(new { ClassName, TeacherName })
                            };
                            await SendToClientAsync(clientId, ack);
                            
                            ClientConnected?.Invoke(this, new ClientConnectedEventArgs
                            {
                                ClientId = clientId,
                                ClientName = message.SenderName,
                                IpAddress = ((IPEndPoint?)client.Client.RemoteEndPoint)?.Address.ToString() ?? "",
                                ClientInfo = message.Payload != null 
                                    ? JsonSerializer.Deserialize<ClientInfo>(message.Payload) 
                                    : null
                            });
                            break;

                        case MessageType.Disconnect:
                            throw new Exception("Client disconnected");

                        case MessageType.Heartbeat:
                            // Reply heartbeat
                            await SendToClientAsync(clientId!, new NetworkMessage { Type = MessageType.Heartbeat });
                            break;

                        case MessageType.ScreenData:
                            if (message.Payload != null)
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
                System.Diagnostics.Debug.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                if (clientId != null)
                {
                    _clients.TryRemove(clientId, out _);
                    _clientNames.TryRemove(clientId, out var clientName);
                    
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
            _udpBroadcaster = new UdpClient();
            _udpBroadcaster.EnableBroadcast = true;
            
            var localIp = GetLocalIPAddress();
            var endpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);

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
                    await _udpBroadcaster.SendAsync(bytes, bytes.Length, endpoint);

                    await Task.Delay(3000, ct); // Broadcast every 3 seconds
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Broadcast error: {ex.Message}");
                    await Task.Delay(1000, ct);
                }
            }
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
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Send error: {ex.Message}");
                }
            }
        }

        public async Task BroadcastToAllAsync(NetworkMessage message)
        {
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
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
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
