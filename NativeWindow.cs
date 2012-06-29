using System;
using System.Runtime.InteropServices;
using System.Windows;

public static class NativeWindow
{
    public static IntPtr FindWindow(Window win)
    {
        var hwnd = GetHwnd(win);

        if( win == Application.Current.MainWindow)
        {
            var child = hwnd;
            while (child != IntPtr.Zero)
            {
                hwnd = child;
                child = FindWindowEx(hwnd, IntPtr.Zero, null, null);
            }
        }
        return hwnd;
    }

    private static IntPtr GetHwnd(Window win)
    {
        var oldTitle = win.Title;
        var id = oldTitle + "(" + Guid.NewGuid().ToString() + ")";
        win.Title = id;
        var hwnd = FindWindowByCaption(IntPtr.Zero, id);
        win.Title = oldTitle;
        return hwnd;
    }

    public static IntPtr HwndFromPoint(int x, int y)
    {
        return WindowFromPoint(new POINT { X = x, Y = y });
    }

    public static void SetTransparent(IntPtr hwnd, byte alpha)
    {
        // Note: the window must be in the hidden state to take effect
        SetWindowLong(hwnd, GWL_EXSTYLE, (int)(WS_EX_LAYERED + WS_EX_TRANSPARENT + WS_EX_NOACTIVATE));
        SetLayeredWindowAttributes(hwnd, 0, alpha, LWA_ALPHA);
    }

    #region Win32 API
    private const int GWL_EXSTYLE = -20;
    private const UInt32 WS_EX_NOACTIVATE = 0x08000000;
    private const UInt32 WS_EX_LAYERED = 0x80000;
    private const UInt32 WS_EX_TRANSPARENT = 0x00000020;
    private const int LWA_ALPHA = 0x2;

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
    #endregion
}
