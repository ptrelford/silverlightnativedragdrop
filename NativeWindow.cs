using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

public class NativeWindow
{
    private HookProc _callback; // Prevent the Garbage Collector from destroying the delegate!
    private readonly IntPtr _hHook;

    public NativeWindow()
    {
        _callback = WindowHook;
        _hHook = SetWindowsHookEx(WH_CALLWNDPROC, _callback, IntPtr.Zero, GetCurrentThreadId());
        Window = new Window();
        if (_hHook != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hHook);
            _hHook = IntPtr.Zero;
        }
    }

    public static NativeWindow FindMainWindow()
    {
        var win = Application.Current.MainWindow;
        var oldTitle = win.Title;
        var id = oldTitle + "(" + Guid.NewGuid().ToString() + ")";
        win.Title = id;
        var hwnd = FindWindowByCaption(IntPtr.Zero, id);
        win.Title = oldTitle;

        var child = hwnd;
        while (child != IntPtr.Zero)
        {
            hwnd = child;
            child = FindWindowEx(hwnd, IntPtr.Zero, null, null);
        }
        return new NativeWindow { Window = win, Hwnd = hwnd };
    }

    public static IntPtr HwndFromPoint(int x, int y)
    {
        return WindowFromPoint(new POINT { X = x, Y = y });
    }

    public IntPtr Hwnd { get; private set; }
    public Window Window { get; private set; }

    public void SetTransparent()
    {
        // Note: the window must be in the hidden state to take effect
        SetWindowLong(Hwnd, GWL_EXSTYLE, (int)(WS_EX_LAYERED + WS_EX_TRANSPARENT + WS_EX_NOACTIVATE));
        SetLayeredWindowAttributes(Hwnd, 0, (255 * 50) / 100, LWA_ALPHA);
    }

    [AllowReversePInvokeCalls]
    private IntPtr WindowHook(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code == HC_ACTION)
        {
            var messageInfo = new CWPSTRUCT();
            Marshal.PtrToStructure(lParam, messageInfo);

            if (messageInfo.message == WM_CREATE)
            {
                Hwnd = messageInfo.hwnd;
            }
        }
        return CallNextHookEx(_hHook, code, wParam, lParam);
    }

    #region Win32 API
    private const int HC_ACTION = 0;
    private const int WM_CREATE = 0x01;
    private const int WH_CALLWNDPROC = 4;
    private const int GWL_EXSTYLE = -20;
    private const UInt32 WS_EX_NOACTIVATE = 0x08000000;
    private const UInt32 WS_EX_LAYERED = 0x80000;
    private const UInt32 WS_EX_TRANSPARENT = 0x00000020;
    private const int LWA_ALPHA = 0x2;

    [StructLayout(LayoutKind.Sequential)]
    public class CWPSTRUCT
    {
        public IntPtr lparam;
        public IntPtr wparam;
        public int message;
        public IntPtr hwnd;
    }

    delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    static extern uint GetCurrentThreadId();

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr SetWindowsHookEx(int hookType, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }

    [DllImport("user32.dll")]
    static extern IntPtr WindowFromPoint(POINT Point);

    [DllImport("user32.dll")]
    static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
    static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
    #endregion
}
