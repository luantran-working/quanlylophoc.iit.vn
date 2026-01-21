using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class FileReceiverService
    {
        private static FileReceiverService? _instance;
        public static FileReceiverService Instance => _instance ??= new FileReceiverService();

        private class TransferState
        {
            public FileStream? Stream;
            public string FileName = string.Empty;
            public long FileSize;
            public int ReceivedChunks;
            public int TotalChunks;
            public string TempPath = string.Empty;
        }

        private readonly ConcurrentDictionary<string, TransferState> _transfers = new();

        // Event to notify UI to show popup
        public event EventHandler<BulkFileTransferRequest>? FileRequestReceived;
        public event EventHandler<string>? FileTransferCompleted;

        public void HandleRequest(BulkFileTransferRequest req)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FileRequestReceived?.Invoke(this, req);
            });
        }

        public void AcceptTransfer(BulkFileTransferRequest req)
        {
            try
            {
                string tempPath = Path.GetTempFileName();
                var state = new TransferState
                {
                    FileName = req.FileName,
                    FileSize = req.FileSize,
                    TempPath = tempPath,
                    Stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None),
                    ReceivedChunks = 0
                };

                _transfers.TryAdd(req.FileId, state);
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("FileReceiver", "Error accepting transfer", ex);
            }
        }

        public async Task HandleChunkAsync(BulkFileDataChunk chunk)
        {
            if (_transfers.TryGetValue(chunk.FileId, out var state))
            {
                if (state.Stream != null)
                {
                    // Assuming chunks arrive in order or handle seeking.
                    // For simplicity, assuming Request -> Chunk 0 -> Chunk 1 sent sequentially over TCP.
                    // But to be safe, we should seek. Since chunks are fixed size?
                    // Wait, sender logic was sequential.

                    // Direct write
                    await state.Stream.WriteAsync(chunk.Data, 0, chunk.Data.Length);
                    state.ReceivedChunks++;

                    if (state.ReceivedChunks >= chunk.TotalChunks)
                    {
                        await FinishTransferAsync(chunk.FileId, state);
                    }
                }
            }
        }

        private async Task FinishTransferAsync(string fileId, TransferState state)
        {
            if (state.Stream != null)
            {
                await state.Stream.DisposeAsync();
                state.Stream = null;
            }

            try
            {
                // Move to Downloads
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string destPath = Path.Combine(downloadsPath, state.FileName);

                // Unique name
                int count = 1;
                while (File.Exists(destPath))
                {
                    string nameNoExt = Path.GetFileNameWithoutExtension(state.FileName);
                    string ext = Path.GetExtension(state.FileName);
                    destPath = Path.Combine(downloadsPath, $"{nameNoExt} ({count++}){ext}");
                }

                File.Move(state.TempPath, destPath);
                _transfers.TryRemove(fileId, out _);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    FileTransferCompleted?.Invoke(this, destPath);
                    // Open folder or show notification
                    ToastService.Instance.ShowInfo("Nhận file thành công", $"Đã lưu tại: {destPath}");
                });
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("FileReceiver", "Error finishing transfer", ex);
            }
        }
    }
}
