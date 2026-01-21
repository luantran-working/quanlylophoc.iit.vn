using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class FileCollectionService
    {
        private readonly NetworkClientService _networkClient;
        private bool _isCollecting = false;

        public FileCollectionService(NetworkClientService networkClient)
        {
            _networkClient = networkClient;
        }

        public async Task StartCollectionAsync(FileCollectionRequest request)
        {
            if (_isCollecting) return;
            _isCollecting = true;

            try
            {
                // Validate path
                string searchPath = request.RemotePath;

                // Special handlers for common paths if needed, but assuming absolute path or environmental vars expansion
                searchPath = Environment.ExpandEnvironmentVariables(searchPath);

                if (!Directory.Exists(searchPath))
                {
                    await SendStatusAsync($"Thư mục không tồn tại: {searchPath}", 0, 0);
                    return;
                }

                // Gather files
                var files = new List<string>();
                if (request.Extensions == null || request.Extensions.Count == 0)
                {
                   files.AddRange(Directory.GetFiles(searchPath, "*.*", request.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                }
                else
                {
                    foreach (var ext in request.Extensions)
                    {
                        var pattern = ext.StartsWith(".") ? $"*{ext}" : $"*.{ext}";
                        files.AddRange(Directory.GetFiles(searchPath, pattern, request.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
                    }
                }

                // Remove duplicates if any
                files = files.Distinct().ToList();

                await SendStatusAsync($"Tìm thấy {files.Count} files. Bắt đầu gửi...", files.Count, 0);

                int processed = 0;
                foreach (var filePath in files)
                {
                    try
                    {
                        // Limit file size (e.g., skip > 20MB) to prevent lockups
                        var info = new FileInfo(filePath);
                        if (info.Length > 20 * 1024 * 1024)
                        {
                            processed++;
                            continue;
                        }

                        byte[] content = await File.ReadAllBytesAsync(filePath);

                        var collectionData = new CollectedFile
                        {
                            StudentName = _networkClient.DisplayName,
                            StudentMachineId = _networkClient.MachineId,
                            FileName = Path.GetFileName(filePath),
                            RelativePath = Path.GetRelativePath(searchPath, filePath),
                            Content = content
                        };

                        var message = new NetworkMessage
                        {
                            Type = MessageType.FileCollectionData,
                            SenderId = _networkClient.MachineId,
                            SenderName = _networkClient.DisplayName,
                            Payload = JsonSerializer.Serialize(collectionData)
                        };

                        await _networkClient.SendMessageAsync(message);

                        // Small delay to prevent flooding
                        await Task.Delay(50);
                    }
                    catch
                    {
                        // Ignore access denied or read errors
                    }

                    processed++;
                    if (processed % 5 == 0) // Update status every 5 files
                    {
                        await SendStatusAsync($"Đang gửi...", files.Count, processed);
                    }
                }

                await SendStatusAsync("Hoàn thành thu thập.", files.Count, files.Count);
            }
            catch (Exception ex)
            {
                await SendStatusAsync($"Lỗi: {ex.Message}", 0, 0);
            }
            finally
            {
                _isCollecting = false;
            }
        }

        private async Task SendStatusAsync(string msg, int total, int current)
        {
            var status = new FileCollectionStatus
            {
                Message = msg,
                TotalFiles = total,
                ProcessedFiles = current
            };

            await _networkClient.SendMessageAsync(new NetworkMessage
            {
                Type = MessageType.FileCollectionStatus,
                SenderId = _networkClient.MachineId,
                SenderName = _networkClient.DisplayName,
                Payload = JsonSerializer.Serialize(status)
            });
        }
    }
}
