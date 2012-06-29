using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SilverlightNativeDragDrop
{
    public partial class MainPage : UserControl
    {
        public MainPage()
        {
            InitializeComponent();

            var dragElement = 
                new Rectangle { 
                    Fill = new SolidColorBrush(Colors.Red), 
                    Width = 32, 
                    Height = 32 
                };
            
            AllowDrag(this, dragElement);
        }

        private static void AllowDrag(UIElement source, FrameworkElement dragElement)
        {
            source.MouseLeftButtonDown += (s, e) => App.Drag.Capture(source, e, dragElement);
            source.MouseMove += (s, e) => App.Drag.Move(source, e);
            source.MouseLeftButtonUp += (s, e) => App.Drag.Release(source);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            var window = new Window();
            window.Width = 200;
            window.Height = 150;

            var grid = new Grid();
            var block = new TextBlock { Text = "Block", IsHitTestVisible = true };
            grid.Children.Add(block);
            window.Content = new UserControl { Content = grid };
            
            var dragElement = new Canvas { Width = 32, Height = 32 };
            dragElement.Background = new SolidColorBrush(Colors.Transparent);
            dragElement.Children.Add(new Ellipse { Fill = new SolidColorBrush(Colors.Blue), Width = 32, Height = 32 });

            AllowDrag(window.Content, dragElement);
            window.Show();
        }
    }
}
