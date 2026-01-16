using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace ClassroomManagement.Views
{
    /// <summary>
    /// Cửa sổ khóa màn hình - chặn mọi thao tác chuột/bàn phím
    /// </summary>
    public partial class LockScreenWindow : Window
    {
        private readonly DateTime _lockTime;
        private readonly DispatcherTimer _updateTimer;
        
        // Win32 API để chặn Alt+Tab, Alt+F4, Ctrl+Esc, etc.
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;
        
        private static IntPtr _hookId = IntPtr.Zero;
        private static LowLevelKeyboardProc? _proc;
        private static LockScreenWindow? _instance;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public LockScreenWindow()
        {
            InitializeComponent();
            
            _lockTime = DateTime.Now;
            _instance = this;
            
            // Update time display
            LockTimeText.Text = $"Đã khóa lúc: {_lockTime:HH:mm:ss}";
            
            // Timer to keep window focused
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Install keyboard hook to block Alt+Tab, Alt+F4, etc.
            InstallKeyboardHook();
            
            // Start update timer
            _updateTimer.Start();
            
            // Force focus
            Activate();
            Focus();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _updateTimer.Stop();
            UninstallKeyboardHook();
            _instance = null;
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Keep window on top and focused
            Activate();
            Topmost = true;
            
            // Update lock duration
            var duration = DateTime.Now - _lockTime;
            LockTimeText.Text = $"Đã khóa lúc: {_lockTime:HH:mm:ss} ({(int)duration.TotalMinutes} phút)";
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Block all keyboard input
            e.Handled = true;
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Block all mouse clicks
            e.Handled = true;
        }

        #region Keyboard Hook

        private void InstallKeyboardHook()
        {
            try
            {
                _proc = HookCallback;
                using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
                using var curModule = curProcess.MainModule;
                if (curModule != null)
                {
                    _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, 
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }
            catch (Exception ex)
            {
                Services.LogService.Instance.Warning("LockScreen", $"Failed to install keyboard hook: {ex.Message}");
            }
        }

        private void UninstallKeyboardHook()
        {
            if (_hookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookId);
                _hookId = IntPtr.Zero;
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && _instance != null)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                
                // Block these key combinations:
                // - Alt+Tab (VK_TAB = 0x09)
                // - Alt+F4 (VK_F4 = 0x73)
                // - Ctrl+Esc (VK_ESCAPE = 0x1B)
                // - Windows key (VK_LWIN = 0x5B, VK_RWIN = 0x5C)
                // - Ctrl+Alt+Delete is handled by Windows and cannot be blocked
                
                bool isSystemKey = (int)wParam == WM_SYSKEYDOWN;
                bool isAlt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
                bool isCtrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                
                // Block Alt+Tab, Alt+F4
                if (isAlt && (vkCode == 0x09 || vkCode == 0x73))
                {
                    return (IntPtr)1;
                }
                
                // Block Ctrl+Esc
                if (isCtrl && vkCode == 0x1B)
                {
                    return (IntPtr)1;
                }
                
                // Block Windows keys
                if (vkCode == 0x5B || vkCode == 0x5C)
                {
                    return (IntPtr)1;
                }
                
                // Block all other keys when lock screen is active
                return (IntPtr)1;
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        #endregion

        /// <summary>
        /// Mở khóa màn hình (được gọi từ bên ngoài)
        /// </summary>
        public void Unlock()
        {
            Dispatcher.Invoke(() =>
            {
                Close();
            });
        }
    }
}
