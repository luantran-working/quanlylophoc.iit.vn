using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClassroomManagement.Models;
using ClassroomManagement.Services;

namespace ClassroomManagement.Views
{
    public partial class WhiteboardWindow : Window
    {
        private readonly WhiteboardService _whiteboardService;
        private DrawingAttributes _currentDrawingAttributes;
        private Point _startPoint;
        private bool _isDrawing;
        private DrawingType _currentTool = DrawingType.Pen;

        public WhiteboardWindow()
        {
            InitializeComponent();
            _whiteboardService = WhiteboardService.Instance;

            InitializeDrawingAttributes();
            _ = StartWhiteboardSession();
        }

        private void InitializeDrawingAttributes()
        {
            _currentDrawingAttributes = new DrawingAttributes
            {
                Color = Colors.Black,
                Width = 2,
                Height = 2,
                FitToCurve = true,
                IgnorePressure = false
            };

            WhiteboardCanvas.DefaultDrawingAttributes = _currentDrawingAttributes;
        }

        private async System.Threading.Tasks.Task StartWhiteboardSession()
        {
            await _whiteboardService.StartSessionAsync();
            StatusText.Text = "Bảng trắng đang hoạt động";
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private async void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đóng bảng trắng? Nội dung chưa lưu sẽ bị mất.",
                "Xác nhận đóng",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _whiteboardService.EndSessionAsync();
                Close();
            }
        }

        private void Tool_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button && button.Tag is string toolName)
            {
                _currentTool = Enum.Parse<DrawingType>(toolName);

                switch (_currentTool)
                {
                    case DrawingType.Select:
                        WhiteboardCanvas.EditingMode = InkCanvasEditingMode.Select;
                        StatusText.Text = "Công cụ: Chọn / Di chuyển";
                        break;

                    case DrawingType.Pen:
                        WhiteboardCanvas.EditingMode = InkCanvasEditingMode.Ink;
                        _currentDrawingAttributes.IsHighlighter = false;
                        StatusText.Text = "Công cụ: Bút vẽ";
                        break;

                    case DrawingType.Highlighter:
                        WhiteboardCanvas.EditingMode = InkCanvasEditingMode.Ink;
                        _currentDrawingAttributes.IsHighlighter = true;
                        _currentDrawingAttributes.Width = 10;
                        _currentDrawingAttributes.Height = 3;
                        StatusText.Text = "Công cụ: Bút dạ quang";
                        break;

                    case DrawingType.Eraser:
                        WhiteboardCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                        StatusText.Text = "Công cụ: Tẩy";
                        break;

                    default:
                        WhiteboardCanvas.EditingMode = InkCanvasEditingMode.None;
                        StatusText.Text = $"Công cụ: {toolName}";
                        break;
                }
            }
        }

        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a simple color menu
            var contextMenu = new ContextMenu();

            var colors = new[]
            {
                ("#000000", "Đen"),
                ("#FF0000", "Đỏ"),
                ("#00FF00", "Xanh lá"),
                ("#0000FF", "Xanh dương"),
                ("#FFFF00", "Vàng"),
                ("#FF00FF", "Tím"),
                ("#00FFFF", "Cyan"),
                ("#FFA500", "Cam"),
                ("#800080", "Tím đậm"),
                ("#FFFFFF", "Trắng")
            };

            foreach (var (hex, name) in colors)
            {
                var menuItem = new MenuItem { Header = name };
                var color = (Color)ColorConverter.ConvertFromString(hex);

                // Add color preview
                var rect = new System.Windows.Shapes.Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = new SolidColorBrush(color),
                    Margin = new Thickness(0, 0, 8, 0)
                };

                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(rect);
                panel.Children.Add(new TextBlock { Text = name, VerticalAlignment = VerticalAlignment.Center });
                menuItem.Header = panel;

                menuItem.Click += (s, args) =>
                {
                    _currentDrawingAttributes.Color = color;
                    ColorButton.Background = new SolidColorBrush(color);
                    StatusText.Text = $"Đã chọn màu: {name}";
                };

                contextMenu.Items.Add(menuItem);
            }

            contextMenu.PlacementTarget = ColorButton;
            contextMenu.IsOpen = true;
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_currentDrawingAttributes != null)
            {
                _currentDrawingAttributes.Width = e.NewValue;
                _currentDrawingAttributes.Height = e.NewValue;
            }
        }

        private async void Undo_Click(object sender, RoutedEventArgs e)
        {
            if (WhiteboardCanvas.Strokes.Count > 0)
            {
                var lastStroke = WhiteboardCanvas.Strokes[WhiteboardCanvas.Strokes.Count - 1];
                _redoStack.Push(lastStroke);
                WhiteboardCanvas.Strokes.RemoveAt(WhiteboardCanvas.Strokes.Count - 1);
                await _whiteboardService.UndoAsync();
                StatusText.Text = "Đã hoàn tác";
            }
        }

        private async void Clear_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa toàn bộ bảng trắng?",
                "Xác nhận xóa",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                WhiteboardCanvas.Strokes.Clear();
                await _whiteboardService.ClearAsync();
                StatusText.Text = "Đã xóa bảng trắng";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg",
                DefaultExt = "png",
                FileName = $"Whiteboard_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    // Get the canvas bounds
                    var bounds = VisualTreeHelper.GetDescendantBounds(WhiteboardCanvas);
                    var renderBitmap = new RenderTargetBitmap(
                        (int)WhiteboardCanvas.ActualWidth,
                        (int)WhiteboardCanvas.ActualHeight,
                        96d, 96d, PixelFormats.Default);

                    renderBitmap.Render(WhiteboardCanvas);

                    // Save to file
                    var encoder = saveDialog.FilterIndex == 2
                        ? (BitmapEncoder)new System.Windows.Media.Imaging.JpegBitmapEncoder()
                        : new System.Windows.Media.Imaging.PngBitmapEncoder();

                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderBitmap));

                    using (var stream = System.IO.File.Create(saveDialog.FileName))
                    {
                        encoder.Save(stream);
                    }

                    StatusText.Text = $"Đã lưu: {saveDialog.FileName}";
                    ToastService.Instance.ShowSuccess("Đã lưu", "Bảng trắng đã được lưu thành công");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void QuickColor_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string hexColor)
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                _currentDrawingAttributes.Color = color;
                ColorButton.Background = new SolidColorBrush(color);
                StatusText.Text = $"Đã chọn màu";
            }
        }

        private System.Collections.Generic.Stack<System.Windows.Ink.Stroke> _redoStack = new();

        private async void Redo_Click(object sender, RoutedEventArgs e)
        {
            if (_redoStack.Count > 0)
            {
                var stroke = _redoStack.Pop();
                WhiteboardCanvas.Strokes.Add(stroke);
                await _whiteboardService.RedoAsync();
                StatusText.Text = "Đã làm lại";
            }
            else
            {
                StatusText.Text = "Không có gì để làm lại";
            }
        }

        private double _currentZoom = 1.0;

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentZoom < 3.0)
            {
                _currentZoom += 0.25;
                ApplyZoom();
            }
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (_currentZoom > 0.25)
            {
                _currentZoom -= 0.25;
                ApplyZoom();
            }
        }

        private void ApplyZoom()
        {
            var transform = new ScaleTransform(_currentZoom, _currentZoom);
            WhiteboardCanvas.LayoutTransform = transform;
            ZoomText.Text = $"{_currentZoom * 100:F0}%";
            StatusText.Text = $"Thu phóng: {_currentZoom * 100:F0}%";
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentTool == DrawingType.Pen || _currentTool == DrawingType.Highlighter || _currentTool == DrawingType.Eraser)
                return; // Let InkCanvas handle these

            _startPoint = e.GetPosition(WhiteboardCanvas);
            _isDrawing = true;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(WhiteboardCanvas);
            CoordinatesText.Text = $"X: {pos.X:F0}, Y: {pos.Y:F0}";

            if (!_isDrawing) return;

            // TODO: Preview shape while drawing
        }

        private async void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing) return;
            _isDrawing = false;

            var endPoint = e.GetPosition(WhiteboardCanvas);

            // Create stroke based on current tool
            var stroke = new DrawingStroke
            {
                Type = _currentTool,
                Color = _currentDrawingAttributes.Color.ToString(),
                Thickness = _currentDrawingAttributes.Width,
                X = _startPoint.X,
                Y = _startPoint.Y,
                Width = endPoint.X - _startPoint.X,
                Height = endPoint.Y - _startPoint.Y
            };

            await _whiteboardService.AddStrokeAsync(stroke);

            // TODO: Draw shape on canvas
        }
    }
}
