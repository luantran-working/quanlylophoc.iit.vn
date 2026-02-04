
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service quản lý chat in-memory (không lưu database)
    /// Hỗ trợ chat toàn bộ (public) và chat riêng (private)
    /// </summary>
    public class ChatService
    {
        private static ChatService? _instance;
        public static ChatService Instance => _instance ??= new ChatService();

        private NetworkServerService? _server;
        private NetworkClientService? _client;
        private readonly LogService _log = LogService.Instance;

        // In-memory message storage
        private readonly ConcurrentDictionary<string, List<ChatMessage>> _messagesByConversation = new();
        
        // Track online students for private chats (teacher side)
        private readonly ConcurrentDictionary<string, Student> _onlineStudents = new();

        // Events
        public event EventHandler<ChatMessage>? MessageReceived;
        public event EventHandler<ChatGroup>? GroupCreated;
        public event EventHandler<Student>? StudentOnline;
        public event EventHandler<Student>? StudentOffline;

        // Properties
        public bool IsTeacherMode => _server != null;
        public string MyId => IsTeacherMode ? "teacher" : (_client?.MachineId ?? "");
        public string MyName => IsTeacherMode ? "Giáo viên" : (_client?.DisplayName ?? "Học sinh");

        public const string PUBLIC_CHAT_ID = "public";

        public ChatService()
        {
            // Initialize public chat storage
            _messagesByConversation[PUBLIC_CHAT_ID] = new List<ChatMessage>();
        }

        /// <summary>
        /// Initialize service for Teacher or Student mode
        /// </summary>
        public void Initialize(NetworkServerService? server, NetworkClientService? client)
        {
            _server = server;
            _client = client;

            if (_server != null)
            {
                _log.Info("ChatService", "Initialized in Teacher mode");
            }
            else if (_client != null)
            {
                _log.Info("ChatService", "Initialized in Student mode");
            }
        }

        /// <summary>
        /// Add/update online student (for teacher mode)
        /// </summary>
        public void AddOnlineStudent(Student student)
        {
            _onlineStudents[student.MachineId] = student;
            
            // Ensure private chat storage exists
            var privateKey = GetPrivateChatKey(student.MachineId);
            if (!_messagesByConversation.ContainsKey(privateKey))
            {
                _messagesByConversation[privateKey] = new List<ChatMessage>();
            }

            StudentOnline?.Invoke(this, student);
        }

        /// <summary>
        /// Remove offline student (for teacher mode)
        /// </summary>
        public void RemoveOnlineStudent(string machineId)
        {
            if (_onlineStudents.TryRemove(machineId, out var student))
            {
                StudentOffline?.Invoke(this, student);
            }
        }

        /// <summary>
        /// Get all online students
        /// </summary>
        public IEnumerable<Student> GetOnlineStudents() => _onlineStudents.Values;

        /// <summary>
        /// Get private chat key for a student
        /// </summary>
        public string GetPrivateChatKey(string partnerId)
        {
            return $"private_{partnerId}";
        }

        /// <summary>
        /// Send message to public chat (broadcast to all)
        /// </summary>
        public async Task SendPublicMessageAsync(string content, string contentType = "text")
        {
            var msg = new ChatMessage
            {
                Id = GenerateMessageId(),
                SenderType = IsTeacherMode ? "teacher" : "student",
                SenderId = IsTeacherMode ? 0 : GetStudentId(),
                SenderName = MyName,
                Content = content,
                ContentType = contentType,
                IsGroup = true,
                GroupId = PUBLIC_CHAT_ID,
                CreatedAt = DateTime.Now
            };

            // Store locally
            StoreMessage(PUBLIC_CHAT_ID, msg);

            // Send over network
            var networkMsg = new NetworkMessage
            {
                Type = MessageType.ChatMessage,
                SenderId = MyId,
                SenderName = MyName,
                Payload = JsonSerializer.Serialize(msg)
            };

            if (IsTeacherMode && _server != null)
            {
                await _server.BroadcastToAllAsync(networkMsg);
                _log.Debug("ChatService", $"Broadcast public message: {content}");
            }
            else if (_client != null)
            {
                await _client.SendMessageAsync(networkMsg);
                _log.Debug("ChatService", $"Sent public message to server: {content}");
            }

            // Notify UI
            MessageReceived?.Invoke(this, msg);
        }

        /// <summary>
        /// Send private message to specific student/teacher
        /// </summary>
        public async Task SendPrivateMessageAsync(string targetId, string content, string contentType = "text")
        {
            var chatKey = GetPrivateChatKey(targetId);

            var msg = new ChatMessage
            {
                Id = GenerateMessageId(),
                SenderType = IsTeacherMode ? "teacher" : "student",
                SenderId = IsTeacherMode ? 0 : GetStudentId(),
                SenderName = MyName,
                ReceiverId = IsTeacherMode ? GetStudentIdFromMachineId(targetId) : 0,
                Content = content,
                ContentType = contentType,
                IsGroup = false,
                GroupId = chatKey, // IMPORTANT: Set conversation key here for UI routing
                CreatedAt = DateTime.Now
            };

            // Store locally
            StoreMessage(chatKey, msg);

            // Send over network
            // Note: We don't send the GroupId (chatKey) over network because it differs per side
            // Teacher sees "private_StudentA", StudentA sees "private_teacher"
            var networkMsg = new NetworkMessage
            {
                Type = MessageType.ChatPrivate,
                SenderId = MyId,
                SenderName = MyName,
                TargetId = targetId,
                Payload = JsonSerializer.Serialize(msg)
            };

            if (IsTeacherMode && _server != null)
            {
                await _server.SendToClientAsync(targetId, networkMsg);
                _log.Debug("ChatService", $"Sent private message to {targetId}: {content}");
            }
            else if (_client != null)
            {
                await _client.SendMessageAsync(networkMsg);
                _log.Debug("ChatService", $"Sent private message to teacher: {content}");
            }

            // Notify UI
            MessageReceived?.Invoke(this, msg);
        }

        /// <summary>
        /// Handle incoming chat message from network
        /// </summary>
        public void HandleIncomingMessage(NetworkMessage networkMsg)
        {
            if (networkMsg.Payload == null) return;

            try
            {
                var chatMsg = JsonSerializer.Deserialize<ChatMessage>(networkMsg.Payload);
                if (chatMsg == null) return;

                // Ensure sender info from network message
                chatMsg.SenderName = networkMsg.SenderName ?? chatMsg.SenderName;

                string chatKey;
                if (networkMsg.Type == MessageType.ChatMessage)
                {
                    // Public message
                    chatKey = PUBLIC_CHAT_ID;
                    chatMsg.IsGroup = true;
                    chatMsg.GroupId = PUBLIC_CHAT_ID;
                }
                else
                {
                    // Private message received
                    // If I am teacher, key is sender (student)
                    // If I am student, key is sender (teacher) -> "private_teacher"
                    if (IsTeacherMode)
                    {
                        chatKey = GetPrivateChatKey(networkMsg.SenderId);
                    }
                    else
                    {
                        chatKey = GetPrivateChatKey("teacher");
                    }
                    
                    chatMsg.IsGroup = false;
                    chatMsg.GroupId = chatKey; // Set for UI routing
                }

                // Store locally
                StoreMessage(chatKey, chatMsg);

                _log.Debug("ChatService", $"Received message from {chatMsg.SenderName}: {chatMsg.Content}");

                // If teacher, broadcast public messages to all students
                if (IsTeacherMode && _server != null && networkMsg.Type == MessageType.ChatMessage)
                {
                    // Broadcast to all except sender
                    _ = BroadcastExceptAsync(networkMsg.SenderId, networkMsg);
                }

                // Notify UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageReceived?.Invoke(this, chatMsg);
                });
            }
            catch (Exception ex)
            {
                _log.Error("ChatService", "Error handling incoming message", ex);
            }
        }

        /// <summary>
        /// Remove conversation and messages for a specific machine ID
        /// </summary>
        public void RemoveConversation(string machineId)
        {
            var key = GetPrivateChatKey(machineId);
            if (_messagesByConversation.TryRemove(key, out _))
            {
                _log.Info("ChatService", $"Removed conversation for {machineId}");
            }
        }

        /// <summary>
        /// Broadcast message to all except sender
        /// </summary>
        private async Task BroadcastExceptAsync(string exceptId, NetworkMessage msg)
        {
            if (_server == null) return;

            foreach (var clientId in _server.GetConnectedClientIds())
            {
                if (clientId != exceptId)
                {
                    await _server.SendToClientAsync(clientId, msg);
                }
            }
        }

        /// <summary>
        /// Get messages for a conversation
        /// </summary>
        public List<ChatMessage> GetMessages(string conversationId)
        {
            if (_messagesByConversation.TryGetValue(conversationId, out var messages))
            {
                return messages.ToList();
            }
            return new List<ChatMessage>();
        }

        /// <summary>
        /// Check if a message is from current user
        /// </summary>
        public bool IsMyMessage(ChatMessage msg)
        {
            if (IsTeacherMode)
            {
                return msg.SenderType == "teacher";
            }
            else
            {
                return msg.SenderName == MyName;
            }
        }

        /// <summary>
        /// Get client/student name
        /// </summary>
        public string GetClientName()
        {
            return MyName;
        }

        private void StoreMessage(string conversationId, ChatMessage msg)
        {
            if (!_messagesByConversation.ContainsKey(conversationId))
            {
                _messagesByConversation[conversationId] = new List<ChatMessage>();
            }
            _messagesByConversation[conversationId].Add(msg);
        }

        private int GenerateMessageId()
        {
            return Math.Abs(Guid.NewGuid().GetHashCode());
        }

        private int GetStudentId()
        {
            // For student mode, use hash of machine id as student id
            return Math.Abs((_client?.MachineId ?? "").GetHashCode());
        }

        private int GetStudentIdFromMachineId(string machineId)
        {
            if (_onlineStudents.TryGetValue(machineId, out var student))
            {
                return student.Id;
            }
            return Math.Abs(machineId.GetHashCode());
        }

        /// <summary>
        /// Clear all messages (for session end)
        /// </summary>
        public void ClearAll()
        {
            _messagesByConversation.Clear();
            _messagesByConversation[PUBLIC_CHAT_ID] = new List<ChatMessage>();
            _onlineStudents.Clear();
        }
    }
}
