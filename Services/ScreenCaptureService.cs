using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service chụp và xử lý ảnh màn hình
    /// </summary>
    public class ScreenCaptureService
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest,
            IntPtr hdcSrc, int xSrc, int ySrc, int rop);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        private const int SRCCOPY = 0x00CC0020;

        /// <summary>
        /// Chụp toàn bộ màn hình
        /// </summary>
        public byte[] CaptureScreen(int quality = 75)
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            using var bitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            
            graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));

            return CompressToJpeg(bitmap, quality);
        }

        /// <summary>
        /// Chụp màn hình Full HD (1920x1080) hoặc giữ nguyên nếu màn hình nhỏ hơn
        /// Dùng cho xem chi tiết và điều khiển từ xa
        /// </summary>
        public byte[] CaptureScreenFullHD(int quality = 85)
        {
            return CaptureScreenWithResolution(1920, 1080, quality);
        }

        /// <summary>
        /// Chụp màn hình HD (1280x720)
        /// </summary>
        public byte[] CaptureScreenHD(int quality = 80)
        {
            return CaptureScreenWithResolution(1280, 720, quality);
        }

        /// <summary>
        /// Chụp màn hình với độ phân giải tùy chỉnh
        /// </summary>
        public byte[] CaptureScreenWithResolution(int maxWidth, int maxHeight, int quality = 85)
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            // Capture full screen first
            using var fullBitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(fullBitmap);
            graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));

            // Nếu màn hình nhỏ hơn hoặc bằng yêu cầu, không resize
            if (screenWidth <= maxWidth && screenHeight <= maxHeight)
            {
                return CompressToJpeg(fullBitmap, quality);
            }

            // Calculate target size maintaining aspect ratio
            var ratio = Math.Min((double)maxWidth / screenWidth, (double)maxHeight / screenHeight);
            var targetWidth = (int)(screenWidth * ratio);
            var targetHeight = (int)(screenHeight * ratio);

            using var resizedBitmap = new Bitmap(targetWidth, targetHeight);
            using var resizedGraphics = Graphics.FromImage(resizedBitmap);
            resizedGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            resizedGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            resizedGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            resizedGraphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            resizedGraphics.DrawImage(fullBitmap, 0, 0, targetWidth, targetHeight);

            return CompressToJpeg(resizedBitmap, quality);
        }

        /// <summary>
        /// Chụp màn hình theo yêu cầu từ ScreenshotRequest
        /// </summary>
        public byte[] CaptureScreenByRequest(string resolution, int quality)
        {
            return resolution.ToLower() switch
            {
                "thumbnail" => CaptureScreenThumbnail(640, 360, Math.Min(quality, 70)),
                "hd" => CaptureScreenWithResolution(1280, 720, quality),
                "fullhd" => CaptureScreenWithResolution(1920, 1080, quality),
                "original" => CaptureScreen(quality),
                _ => CaptureScreenWithResolution(1920, 1080, quality)
            };
        }

        /// <summary>
        /// Chụp màn hình với kích thước thu nhỏ (thumbnail)
        /// maxWidth 480 cho list view (360p equivalent), 960 cho detail view (720p equivalent)
        /// </summary>
        public byte[] CaptureScreenThumbnail(int maxWidth = 480, int maxHeight = 270, int quality = 70)
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

            // Calculate thumbnail size maintaining aspect ratio
            var ratio = Math.Min((double)maxWidth / screenWidth, (double)maxHeight / screenHeight);
            var thumbWidth = (int)(screenWidth * ratio);
            var thumbHeight = (int)(screenHeight * ratio);

            using var fullBitmap = new Bitmap(screenWidth, screenHeight, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(fullBitmap);
            graphics.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(screenWidth, screenHeight));

            using var thumbnail = new Bitmap(thumbWidth, thumbHeight);
            using var thumbGraphics = Graphics.FromImage(thumbnail);
            thumbGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            thumbGraphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            thumbGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            thumbGraphics.DrawImage(fullBitmap, 0, 0, thumbWidth, thumbHeight);

            return CompressToJpeg(thumbnail, quality);
        }

        /// <summary>
        /// Chụp một vùng cụ thể của màn hình
        /// </summary>
        public byte[] CaptureRegion(int x, int y, int width, int height, int quality = 75)
        {
            using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));

            return CompressToJpeg(bitmap, quality);
        }

        /// <summary>
        /// Nén ảnh thành JPEG
        /// </summary>
        private byte[] CompressToJpeg(Bitmap bitmap, int quality)
        {
            var encoder = GetEncoder(ImageFormat.Jpeg);
            var encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);

            using var stream = new MemoryStream();
            bitmap.Save(stream, encoder, encoderParams);
            return stream.ToArray();
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                    return codec;
            }
            return codecs[0];
        }

        /// <summary>
        /// Chuyển byte array thành BitmapImage để hiển thị trong WPF
        /// </summary>
        public static BitmapImage BytesToBitmapImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return new BitmapImage();

            var bitmap = new BitmapImage();
            using var stream = new MemoryStream(imageData);
            
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        /// <summary>
        /// Lấy kích thước màn hình
        /// </summary>
        public static (int Width, int Height) GetScreenSize()
        {
            return ((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
        }
    }
}
