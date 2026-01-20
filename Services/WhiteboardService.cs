using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service quản lý whiteboard sessions và synchronization
    /// </summary>
    public class WhiteboardService
    {
        private static WhiteboardService? _instance;
        public static WhiteboardService Instance => _instance ??= new WhiteboardService();

        private readonly LogService _log = LogService.Instance;
        private WhiteboardSession? _currentSession;

        public ObservableCollection<DrawingStroke> Strokes { get; } = new();
        public DrawingTool CurrentTool { get; } = new();

        public event EventHandler<DrawingStroke>? StrokeAdded;
        public event EventHandler? WhiteboardCleared;
        public event EventHandler<WhiteboardSession>? SessionStarted;
        public event EventHandler? SessionEnded;

        private WhiteboardService() { }

        /// <summary>
        /// Start a new whiteboard session
        /// </summary>
        public async Task<bool> StartSessionAsync()
        {
            try
            {
                _currentSession = new WhiteboardSession
                {
                    IsActive = true,
                    StartTime = DateTime.Now
                };

                Strokes.Clear();

                SessionStarted?.Invoke(this, _currentSession);

                // Broadcast to students
                await BroadcastSessionStartAsync();

                _log.Info("Whiteboard", "Whiteboard session started");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Whiteboard", "Failed to start whiteboard session", ex);
                return false;
            }
        }

        /// <summary>
        /// End current whiteboard session
        /// </summary>
        public async Task EndSessionAsync()
        {
            if (_currentSession == null) return;

            try
            {
                _currentSession.IsActive = false;

                await BroadcastSessionEndAsync();

                SessionEnded?.Invoke(this, EventArgs.Empty);
                _log.Info("Whiteboard", "Whiteboard session ended");
            }
            catch (Exception ex)
            {
                _log.Error("Whiteboard", "Failed to end whiteboard session", ex);
            }
        }

        /// <summary>
        /// Add a stroke to the whiteboard
        /// </summary>
        public async Task AddStrokeAsync(DrawingStroke stroke)
        {
            try
            {
                Strokes.Add(stroke);
                _currentSession?.Strokes.Add(stroke);

                StrokeAdded?.Invoke(this, stroke);

                // Broadcast to students
                await BroadcastStrokeAsync(stroke);
            }
            catch (Exception ex)
            {
                _log.Error("Whiteboard", "Failed to add stroke", ex);
            }
        }

        /// <summary>
        /// Clear all strokes
        /// </summary>
        public async Task ClearAsync()
        {
            try
            {
                Strokes.Clear();
                _currentSession?.Strokes.Clear();

                WhiteboardCleared?.Invoke(this, EventArgs.Empty);

                // Broadcast to students
                await BroadcastClearAsync();

                _log.Info("Whiteboard", "Whiteboard cleared");
            }
            catch (Exception ex)
            {
                _log.Error("Whiteboard", "Failed to clear whiteboard", ex);
            }
        }

        /// <summary>
        /// Undo last stroke
        /// </summary>
        public async Task UndoAsync()
        {
            if (Strokes.Count == 0) return;

            try
            {
                var lastStroke = Strokes.Last();
                Strokes.Remove(lastStroke);
                _currentSession?.Strokes.Remove(lastStroke);
                _redoStack.Push(lastStroke);

                await BroadcastUndoAsync(lastStroke.Id);
            }
            catch (Exception ex)
            {
                _log.Error("Whiteboard", "Failed to undo", ex);
            }
        }

        private System.Collections.Generic.Stack<DrawingStroke> _redoStack = new();

        /// <summary>
        /// Redo last undone stroke
        /// </summary>
        public async Task RedoAsync()
        {
            if (_redoStack.Count == 0) return;

            try
            {
                var stroke = _redoStack.Pop();
                Strokes.Add(stroke);
                _currentSession?.Strokes.Add(stroke);

                await BroadcastStrokeAsync(stroke);
            }
            catch (Exception ex)
            {
                _log.Error("Whiteboard", "Failed to redo", ex);
            }
        }

        /// <summary>
        /// Save whiteboard as image
        /// </summary>
        public async Task<string?> SaveAsImageAsync(string filePath)
        {
            try
            {
                // TODO: Implement image export
                await Task.Delay(100);
                _log.Info("Whiteboard", $"Whiteboard saved to {filePath}");
                return filePath;
            }
            catch (Exception ex)
            {
                _log.Error("Whiteboard", "Failed to save whiteboard", ex);
                return null;
            }
        }

        // Network broadcast methods
        private async Task BroadcastSessionStartAsync()
        {
            // TODO: Send WHITEBOARD_START command to all students
            await Task.Delay(10);
        }

        private async Task BroadcastSessionEndAsync()
        {
            // TODO: Send WHITEBOARD_STOP command to all students
            await Task.Delay(10);
        }

        private async Task BroadcastStrokeAsync(DrawingStroke stroke)
        {
            // TODO: Send WHITEBOARD_STROKE command with stroke data
            await Task.Delay(10);
        }

        private async Task BroadcastClearAsync()
        {
            // TODO: Send WHITEBOARD_CLEAR command
            await Task.Delay(10);
        }

        private async Task BroadcastUndoAsync(string strokeId)
        {
            // TODO: Send WHITEBOARD_UNDO command with stroke ID
            await Task.Delay(10);
        }
    }
}
