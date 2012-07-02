using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

public class NativeDragDrop
{
    private readonly Window _dragWindow;
    private Dictionary<IntPtr, Window> _targetWindows;
    private bool _isDragging;

    public NativeDragDrop()
    {
        _dragWindow = new Window
        {
            WindowStyle = WindowStyle.None,
            Width = 0,
            Height = 0
        };
        _dragWindow.Show();
        _dragWindow.Hide();
        var hwnd = NativeWindow.FindHwnd(_dragWindow);
        NativeWindow.SetTransparent(hwnd, (255 * 50) / 100);
    }

    public void Capture(UIElement source, MouseEventArgs e, FrameworkElement cue)
    {
        _isDragging = true;

        source.CaptureMouse();

        _targetWindows = 
            Application.Current.Windows
                .Cast<Window>()
                .ToDictionary(NativeWindow.FindHwnd);

        _dragWindow.Content = cue;
        _dragWindow.Width = cue.Width;
        _dragWindow.Height = cue.Height;

        var window = Window.GetWindow(source);
        Point windowPos = GetWindowContentPosition(window);

        var mousePos = e.GetPosition(null);
        _dragWindow.Left = windowPos.X + mousePos.X - _dragWindow.Width/2;
        _dragWindow.Top = windowPos.Y + mousePos.Y - _dragWindow.Height/2;

        _dragWindow.Show();
    }

    public void Move(UIElement source, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var mousePos = e.GetPosition(null);

        double x;
        double y;
        GetXY(source, mousePos, out x, out y);
        _dragWindow.Left = x - _dragWindow.Width / 2;
        _dragWindow.Top = y - _dragWindow.Height / 2;
    }

    public void Release(UIElement source, MouseEventArgs e, object data)
    {
        var mousePos = e.GetPosition(null);
        double x, y;
        GetXY(source, mousePos, out x, out y);

        var elements = FindElements(x, y);
        foreach(var element in elements)
        {
            if (element is IAcceptDrop)
            {
                var target = (IAcceptDrop ) element;
                bool handled = target.OnDrop(data);
                if (handled) break;
            }
        }
        
        source.ReleaseMouseCapture();
        _dragWindow.Hide();
        _dragWindow.Content = null;
        _isDragging = false;
    }

    private static void GetXY(UIElement source, Point mousePos, out double x, out double y)
    {
        var window = Window.GetWindow(source);
        var windowPos = GetWindowContentPosition(window);
        x = windowPos.X + mousePos.X;
        y = windowPos.Y + mousePos.Y;
    }

    private static Point GetWindowContentPosition(Window window)
    {
        var offset = GetWindowContentOffset(GetWindowStyle(window));
        var x = window.Left + offset.X;
        var y = window.Top + offset.Y;
        return new Point(x, y);
    }

    private static WindowStyle GetWindowStyle(Window window)
    {
        return window == Application.Current.MainWindow
            ? Deployment.Current.OutOfBrowserSettings.WindowSettings.WindowStyle
            : window.WindowStyle;
    }

    private static Point GetWindowContentOffset(WindowStyle style)
    {
        return style == WindowStyle.SingleBorderWindow
            ? new Point(10, 32)
            : new Point(0, 0);
    }

    private IEnumerable<UIElement> FindElements(double x, double y)
    {
        var hwnd = NativeWindow.HwndFromPoint((int)x, (int)y);
        Window targetWindow;
        if (_targetWindows.TryGetValue(hwnd, out targetWindow))
        {
            var targetPos = GetWindowContentPosition(targetWindow);

            var x1 = x - targetPos.X;
            var y1 = y - targetPos.Y;

            return VisualTreeHelper.FindElementsInHostCoordinates(new Point(x1, y1), targetWindow);
        }
        else return new UIElement[] { };
    }
}