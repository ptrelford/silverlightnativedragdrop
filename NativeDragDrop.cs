using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System;

public class NativeDragDrop
{
    private Window _dragWindow;
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
        var hwnd = NativeWindow.FindWindow(_dragWindow);
        NativeWindow.SetTransparent(hwnd);
    }

    public void Move(UIElement source, MouseEventArgs e)
    {
        if (!_isDragging) return;

        var pos = e.GetPosition(null);
        
        var window = Window.GetWindow(source);
        Point windowPos = GetWindowContentPosition(window);
        var mousePos = e.GetPosition(null);
        var x = windowPos.X + mousePos.X;
        var y = windowPos.Y + mousePos.Y;
        _dragWindow.Left = x - _dragWindow.Width / 2;
        _dragWindow.Top = y - _dragWindow.Height / 2;

        var hwnd = NativeWindow.HwndFromPoint((int)x, (int)y);
        Window targetWindow;
        if( _targetWindows.TryGetValue(hwnd, out targetWindow) )
        {
            var targetPos = GetWindowContentPosition(targetWindow);

            var x1 = x - targetPos.X;
            var y1 = y - targetPos.Y;

            var xs = VisualTreeHelper.FindElementsInHostCoordinates(new Point(x1, y1), targetWindow);
            System.Diagnostics.Debug.WriteLine("Local " + " " + x1 + " " + y1);
            if (xs.Count() > 0)
            {
                var ks = 
                    xs
                    .Select(k => k.ToString());
                System.Diagnostics.Debug.WriteLine("XS " + string.Join(",", ks));
            }
            System.Diagnostics.Debug.WriteLine("Title " + targetWindow.Title);
        }
    }

    public void Capture(UIElement source, MouseEventArgs e, FrameworkElement content)
    {
        _isDragging = true;

        source.CaptureMouse();

        _targetWindows = 
            Application.Current.Windows
                .Cast<Window>()
                .ToDictionary(NativeWindow.FindWindow);

        _dragWindow.Content = content;
        _dragWindow.Width = content.Width;
        _dragWindow.Height = content.Height;

        var window = Window.GetWindow(source);
        Point windowPos = GetWindowContentPosition(window);

        var mousePos = e.GetPosition(null);
        _dragWindow.Left = windowPos.X + mousePos.X - _dragWindow.Width/2;
        _dragWindow.Top = windowPos.Y + mousePos.Y - _dragWindow.Height/2;

        _dragWindow.Show();
    }

    public void Release(UIElement source)
    {
        source.ReleaseMouseCapture();
        _dragWindow.Hide();
        _dragWindow.Content = null;
        _isDragging = false;
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
}