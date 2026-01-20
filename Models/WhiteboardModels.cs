using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClassroomManagement.Models
{
    /// <summary>
    /// Whiteboard session model
    /// </summary>
    public class WhiteboardSession : INotifyPropertyChanged
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime StartTime { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }
        public List<DrawingStroke> Strokes { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// Drawing stroke on whiteboard
    /// </summary>
    public class DrawingStroke
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DrawingType Type { get; set; }
        public string Color { get; set; } = "#000000";
        public double Thickness { get; set; } = 2.0;
        public List<DrawingPoint> Points { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // For shapes
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        // For text
        public string Text { get; set; } = string.Empty;
        public double FontSize { get; set; } = 14;
    }

    /// <summary>
    /// Point in a drawing stroke
    /// </summary>
    public class DrawingPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Pressure { get; set; } = 1.0;
    }

    /// <summary>
    /// Type of drawing
    /// </summary>
    public enum DrawingType
    {
        Select,
        Pen,
        Highlighter,
        Eraser,
        Line,
        Rectangle,
        Circle,
        Arrow,
        Text
    }

    /// <summary>
    /// Drawing tool selection
    /// </summary>
    public class DrawingTool : INotifyPropertyChanged
    {
        private DrawingType _type = DrawingType.Pen;
        private string _color = "#000000";
        private double _thickness = 2.0;

        public DrawingType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        public double Thickness
        {
            get => _thickness;
            set { _thickness = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
