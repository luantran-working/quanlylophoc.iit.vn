using System;
using System.Runtime.InteropServices;
using ClassroomManagement.Models;

namespace ClassroomManagement.Services
{
    public class InputSimulationService
    {
        private static InputSimulationService? _instance;
        public static InputSimulationService Instance => _instance ??= new InputSimulationService();

        private InputSimulationService() { }

        // P/Invoke definitions
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        // Mouse flags
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        // Keyboard flags
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public void SimulateMouse(MouseCommand cmd)
        {
            try
            {
                // Coordinates from server are normalized to 0-65535
                int x = cmd.X;
                int y = cmd.Y;

                // Log mouse simulation
                LogService.Instance.Debug("InputSim", $"Mouse Action: {cmd.Action}, X: {x}, Y: {y}");

                switch (cmd.Action)
                {
                    case MouseAction.Move:
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    // ... other cases (logging is enough at start) ...
                    case MouseAction.LeftDown:
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.LeftUp:
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.LeftClick:
                    case MouseAction.LeftDoubleClick: // Double click handled by sender usually, or verify
                        LogService.Instance.Debug("InputSim", "Executing Left Click");
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_LEFTUP | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.RightDown:
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.RightUp:
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTUP | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.RightClick:
                        LogService.Instance.Debug("InputSim", "Executing Right Click");
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_RIGHTUP | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.MiddleDown:
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_MIDDLEDOWN | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.MiddleUp:
                        mouse_event(MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        mouse_event(MOUSEEVENTF_MIDDLEUP | MOUSEEVENTF_ABSOLUTE, x, y, 0, 0);
                        break;

                    case MouseAction.Scroll:
                        LogService.Instance.Debug("InputSim", $"Executing Scroll: {cmd.Delta}");
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, cmd.Delta, 0);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("InputSim", "SimulateMouse Error", ex);
            }
        }

        public void SimulateKeyboard(KeyboardCommand cmd)
        {
            try
            {
                byte vk = (byte)cmd.KeyCode;
                // Scan code useful for some games/apps, but usually 0 is fine for standard desktop
                byte scan = 0;
                uint flags = 0;

                if (IsExtendedKey(cmd.KeyCode))
                {
                    flags |= KEYEVENTF_EXTENDEDKEY;
                }

                LogService.Instance.Debug("InputSim", $"Key Action: {cmd.Action}, VK: {vk}, Flags: {flags}");

                switch (cmd.Action)
                {
                    case KeyAction.Down:
                        keybd_event(vk, scan, flags, 0);
                        break;

                    case KeyAction.Up:
                        flags |= KEYEVENTF_KEYUP;
                        keybd_event(vk, scan, flags, 0);
                        break;

                    case KeyAction.Press:
                        keybd_event(vk, scan, flags, 0);
                        keybd_event(vk, scan, flags | KEYEVENTF_KEYUP, 0);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogService.Instance.Error("InputSim", "SimulateKeyboard Error", ex);
            }
        }

        private bool IsExtendedKey(int keyCode)
        {
            return keyCode == 0x2D || // VK_INSERT
                   keyCode == 0x2E || // VK_DELETE
                   keyCode == 0x24 || // VK_HOME
                   keyCode == 0x23 || // VK_END
                   keyCode == 0x21 || // VK_PRIOR (Page Up)
                   keyCode == 0x22 || // VK_NEXT (Page Down)
                   keyCode == 0x25 || // VK_LEFT
                   keyCode == 0x26 || // VK_UP
                   keyCode == 0x27 || // VK_RIGHT
                   keyCode == 0x28 || // VK_DOWN
                   keyCode == 0x90 || // VK_NUMLOCK
                   keyCode == 0x2C || // VK_SNAPSHOT (Print Screen)
                   keyCode == 0x6F;   // VK_DIVIDE (Numpad /)
        }
    }
}
