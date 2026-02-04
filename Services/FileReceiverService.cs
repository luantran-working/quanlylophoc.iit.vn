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
            public bool IsAccepted;
        }

        private readonly ConcurrentDictionary<string, TransferState> _transfers = new();

        // Event to notify UI to show popup
        public event EventHandler<BulkFileTransferRequest>? FileRequestReceived;
        public event EventHandler<string>? FileTransferCompleted;
        public event EventHandler<(string FileId, int Progress)>? FileTransferProgress;

        public void HandleRequest(BulkFileTransferRequest req)
        {
            try
            {
                // Auto-accept: Create transfer state immediately when request is received
                // This ensures we're ready to receive chunks right away
                string tempPath = Path.GetTempFileName();
                var state = new TransferState
                {
                    FileName = req.FileName,
                    FileSize = req.FileSize,
                    TempPath = tempPath,
                    Stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None),
                    ReceivedChunks = 0,
                    IsAccepted = true // Auto-accept
                };

                _transfers.TryAdd(req.FileId, state);
                
                LogService.Instance.Info("FileReceiver", $"Auto-accepted file transfer: {req.FileName} ({req.FileSize} bytes)");

                // Show notification popup on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FileRequestReceived?.Invoke(this, req);
                });
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("FileReceiver", "Error handling file transfer request", ex);
            }
        }

        // Keep this method for backward compatibility, but it's no longer needed
        public void AcceptTransfer(BulkFileTransferRequest req)
        {
            // Transfer is already accepted in HandleRequest
            // This method is now a no-op but kept for compatibility
            if (_transfers.TryGetValue(req.FileId, out var state))
            {
                state.IsAccepted = true;
            }
        }

        public void DeclineTransfer(string fileId)
        {
            if (_transfers.TryRemove(fileId, out var state))
            {
                try
                {
                    state.Stream?.Dispose();
                    if (File.Exists(state.TempPath))
                    {
                        File.Delete(state.TempPath);
                    }
                    LogService.Instance.Info("FileReceiver", $"Declined file transfer: {state.FileName}");
                }
                catch (Exception ex)
                {
                    LogService.Instance.Warning("FileReceiver", $"Error cleaning up declined transfer: {ex.Message}");
                }
            }
        }

        public async Task HandleChunkAsync(BulkFileDataChunk chunk)
        {
            if (_transfers.TryGetValue(chunk.FileId, out var state))
            {
                if (state.Stream != null && state.IsAccepted)
                {
                    try
                    {
                        // Store total chunks info
                        if (state.TotalChunks == 0)
                        {
                            state.TotalChunks = chunk.TotalChunks;
                        }

                        // Write chunk data
                        await state.Stream.WriteAsync(chunk.Data, 0, chunk.Data.Length);
                        state.ReceivedChunks++;

                        // Report progress
                        int progressPercent = (int)((double)state.ReceivedChunks / chunk.TotalChunks * 100);
                        FileTransferProgress?.Invoke(this, (chunk.FileId, progressPercent));

                        LogService.Instance.Debug("FileReceiver", 
                            $"Received chunk {state.ReceivedChunks}/{chunk.TotalChunks} for {state.FileName}");

                        // Check if transfer is complete
                        if (state.ReceivedChunks >= chunk.TotalChunks)
                        {
                            await FinishTransferAsync(chunk.FileId, state);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogService.Instance.Error("FileReceiver", $"Error writing chunk for {state.FileName}", ex);
                    }
                }
                else
                {
                    LogService.Instance.Warning("FileReceiver", 
                        $"Received chunk for file {chunk.FileId} but transfer is not ready (Stream null: {state.Stream == null}, Accepted: {state.IsAccepted})");
                }
            }
            else
            {
                LogService.Instance.Warning("FileReceiver", 
                    $"Received chunk for unknown file transfer: {chunk.FileId}");
            }
        }

        private async Task FinishTransferAsync(string fileId, TransferState state)
        {
            if (state.Stream != null)
            {
                await state.Stream.FlushAsync();
                await state.Stream.DisposeAsync();
                state.Stream = null;
            }

            try
            {
                // Move to Downloads
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string destPath = Path.Combine(downloadsPath, state.FileName);

                // Unique name if file exists
                int count = 1;
                while (File.Exists(destPath))
                {
                    string nameNoExt = Path.GetFileNameWithoutExtension(state.FileName);
                    string ext = Path.GetExtension(state.FileName);
                    destPath = Path.Combine(downloadsPath, $"{nameNoExt} ({count++}){ext}");
                }

                File.Move(state.TempPath, destPath);
                _transfers.TryRemove(fileId, out _);

                LogService.Instance.Info("FileReceiver", $"File transfer completed: {destPath}");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    FileTransferCompleted?.Invoke(this, destPath);
                    // Show toast notification
                    ToastService.Instance.ShowSuccess("Nhan file thanh cong", $"Da luu tai: {destPath}");
                });
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("FileReceiver", "Error finishing transfer", ex);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ToastService.Instance.ShowError("Loi nhan file", $"Khong the luu file: {ex.Message}");
                });
            }
        }
    }
}
