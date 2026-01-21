
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class ChatService
    {
        private static ChatService? _instance;
        public static ChatService Instance => _instance ??= new ChatService();

        private NetworkServerService? _server;
        private NetworkClientService? _client;
        private string _imageStorePath;

        public event EventHandler<ChatMessage>? MessageReceived;
        public event EventHandler<ChatGroup>? GroupCreated;

        public ChatService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _imageStorePath = Path.Combine(appData, "IIT", "ClassroomManagement", "ChatImages");
            Directory.CreateDirectory(_imageStorePath);
        }

        public void Initialize(NetworkServerService? server, NetworkClientService? client)
        {
            _server = server;
            _client = client;

            if (_client != null)
            {
                _client.MessageReceived += Client_MessageReceived;
            }
        }

        private void Client_MessageReceived(object? sender, NetworkMessage msg)
        {
            if (msg.Type == MessageType.ChatMessage)
            {
                try
                {
                    var chatMsg = JsonSerializer.Deserialize<ChatMessage>(msg.Payload);
                    if (chatMsg != null) MessageReceived?.Invoke(this, chatMsg);
                }
                catch { }
            }
        }

        #region Server Side Methods

        public async Task<ChatGroup> CreateGroupAsync(string name, int creatorId, List<int> memberIds)
        {
            var group = new ChatGroup
            {
                Name = name,
                CreatorId = creatorId
            };

            DatabaseService.Instance.CreateChatGroup(group);

            // Add creator
            // DatabaseService.Instance.AddChatGroupMember(group.Id, creatorId); // Teacher ID logic varies

            // Add members
            foreach (var studentId in memberIds)
            {
                DatabaseService.Instance.AddChatGroupMember(group.Id, studentId);
            }

            // Sync to clients (Broadcast group info) - To implement
            // await BroadcastGroupInfo(group);

            return group;
        }

        public async Task HandleImageUploadAsync(string senderId, string payload)
        {
            try
            {
                var info = JsonSerializer.Deserialize<ChatAttachmentInfo>(payload);
                if (info == null) return;

                // Save image
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(info.FileName)}";
                var filePath = Path.Combine(_imageStorePath, fileName);
                await File.WriteAllBytesAsync(filePath, info.Data);

                // Create Chat Message
                // We need to resolve SenderId (int) from MachineId (string) if possible, or pass it in payload
                // For now assuming we can look up student
                var student = DatabaseService.Instance.GetOrCreateStudent(senderId, "Unknown", "", "");

                var msg = new ChatMessage
                {
                    SessionId = SessionManager.Instance.CurrentSession?.Id ?? 0,
                    SenderType = "student",
                    SenderId = student.Id,
                    SenderName = student.DisplayName,
                    Content = "[Hình ảnh]",
                    ContentType = "image",
                    AttachmentPath = filePath,
                    IsGroup = true, // Default to group for now
                    CreatedAt = DateTime.Now
                };

                // Save DB
                msg.Id = DatabaseService.Instance.SaveChatMessage(msg);

                // Broadcast back to all
                await BroadcastMessageAsync(msg);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling image upload: {ex.Message}");
            }
        }

        public async Task BroadcastMessageAsync(ChatMessage msg)
        {
            if (_server != null)
            {
                var netMsg = new NetworkMessage
                {
                    Type = MessageType.ChatMessage,
                    SenderId = "server",
                    SenderName = "Server",
                    Payload = JsonSerializer.Serialize(msg)
                };
                await _server.BroadcastToAllAsync(netMsg);

                // Trigger local event for Teacher View
                MessageReceived?.Invoke(this, msg);
            }
        }

        #endregion

        #region Client Side Methods

        public async Task SendTextMessageAsync(string content)
        {
            if (_client != null)
            {
                var msg = new ChatMessage
                {
                    Content = content,
                    SenderType = "student",
                    SenderName = _client.DisplayName,
                    IsGroup = true,
                    CreatedAt = DateTime.Now
                };

                var netMsg = new NetworkMessage
                {
                    Type = MessageType.ChatMessage,
                    SenderId = _client.MachineId,
                    SenderName = _client.DisplayName,
                    Payload = JsonSerializer.Serialize(msg)
                };
                await _client.SendMessageAsync(netMsg);
            }
        }

        public async Task SendImageAsync(string filePath)
        {
            if (_client == null || !File.Exists(filePath)) return;

            var bytes = await File.ReadAllBytesAsync(filePath);
            var info = new ChatAttachmentInfo
            {
                FileName = Path.GetFileName(filePath),
                FileSize = bytes.Length,
                Data = bytes,
                ContentType = "image"
            };

            var msg = new NetworkMessage
            {
                Type = MessageType.ChatImageUpload,
                SenderId = _client.MachineId,
                SenderName = _client.DisplayName,
                Payload = JsonSerializer.Serialize(info)
            };

            await _client.SendMessageAsync(msg);
        }

        public void OnMessageReceived(ChatMessage msg)
        {
            MessageReceived?.Invoke(this, msg);
        }

        public bool IsMyMessage(ChatMessage msg)
        {
            if (_server != null) // I am Server (Teacher)
            {
                return msg.SenderType == "teacher";
            }
            if (_client != null) // I am Client (Student)
            {
                return msg.SenderName == _client.DisplayName;
            }
            return false;
        }

        public string GetClientName()
        {
            if (_server != null) return SessionManager.Instance.CurrentUser?.DisplayName ?? "Giáo viên";
            return _client?.DisplayName ?? "Học sinh";
        }

        #endregion
    }
}
