using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class BulkFileSender
    {
        private static BulkFileSender? _instance;
        public static BulkFileSender Instance => _instance ??= new BulkFileSender();

        private readonly NetworkServerService _networkServer;
        private const int CHUNK_SIZE = 48 * 1024; // 48KB chunks

        private BulkFileSender()
        {
            _networkServer = SessionManager.Instance.NetworkServer;
        }

        public async Task SendFileToStudentsAsync(string filePath, List<string> targetClientIds, IProgress<double>? progress = null)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("File not found", filePath);
            if (targetClientIds == null || targetClientIds.Count == 0) return;

            var fileInfo = new FileInfo(filePath);
            var req = new BulkFileTransferRequest
            {
                FileId = Guid.NewGuid().ToString(),
                FileName = fileInfo.Name,
                FileSize = fileInfo.Length
            };

            // 1. Send Request
            var reqMsg = new NetworkMessage
            {
                Type = MessageType.BulkFileTransferRequest,
                SenderId = "server",
                Payload = JsonSerializer.Serialize(req)
            };

            foreach (var clientId in targetClientIds)
            {
                await _networkServer.SendToClientAsync(clientId, reqMsg);
            }

            // Small delay for clients to prepare
            await Task.Delay(500);

            // 2. Send Data Chunks
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[CHUNK_SIZE];
                int bytesRead;
                int chunkIndex = 0;
                long totalBytesReads = 0;
                int totalChunks = (int)Math.Ceiling((double)fileInfo.Length / CHUNK_SIZE);

                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] dataToSend = buffer;
                    if (bytesRead < CHUNK_SIZE)
                    {
                        dataToSend = new byte[bytesRead];
                        Array.Copy(buffer, dataToSend, bytesRead);
                    }

                    var chunk = new BulkFileDataChunk
                    {
                        FileId = req.FileId,
                        ChunkIndex = chunkIndex++,
                        TotalChunks = totalChunks,
                        Data = dataToSend
                    };

                    var chunkMsg = new NetworkMessage
                    {
                        Type = MessageType.BulkFileData,
                        SenderId = "server",
                        Payload = JsonSerializer.Serialize(chunk)
                    };

                    // Broadcast chunk to all targets
                    // Efficient approach: Serialize once, send to multiple
                    // Assuming NetworkServer handles concurrent sends well
                    var tasks = targetClientIds.Select(id => _networkServer.SendToClientAsync(id, chunkMsg));
                    await Task.WhenAll(tasks);

                    totalBytesReads += bytesRead;
                    progress?.Report((double)totalBytesReads / fileInfo.Length * 100);

                    // Throttle to prevent network flooding
                    await Task.Delay(10);
                }
            }
        }
    }
}
