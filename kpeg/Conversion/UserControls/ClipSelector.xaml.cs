using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace kpeg.Conversion.UserControls
{
    /// <summary>
    /// Interaction logic for ClipSelector.xaml
    /// </summary>
    public partial class ClipSelector : UserControl
    {
        public ClipSelector()
        {
            InitializeComponent();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartMouseCapture(e);
            UserControl_MouseMove(sender, e);
        }
        private Rectangle capturedRectangle;
        private bool first = true;
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!IsMouseCaptured) return;
            Rectangle otherRectangle = capturedRectangle.Name == VerticalBar1.Name ? VerticalBar2 : VerticalBar1;

            
            double newPosition = e.GetPosition(this).X - (capturedRectangle.ActualWidth / 2);
            if (newPosition < HorizontalBar.Margin.Left)
                newPosition = HorizontalBar.Margin.Left;
            else if (newPosition > HorizontalBar.ActualWidth - HorizontalBar.Margin.Right)
                newPosition = HorizontalBar.ActualWidth - HorizontalBar.Margin.Right;

            if (!first)
            {
                if (capturedRectangle.Margin.Left < newPosition)
                {
                    if (capturedRectangle.Margin.Left < otherRectangle.Margin.Left)
                        if (newPosition > otherRectangle.Margin.Left - otherRectangle.ActualWidth)
                            newPosition = otherRectangle.Margin.Left - otherRectangle.ActualWidth;
                }
                if (capturedRectangle.Margin.Left > newPosition)
                {
                    if (capturedRectangle.Margin.Left > otherRectangle.Margin.Left)
                        if (newPosition < otherRectangle.Margin.Left + otherRectangle.ActualWidth)
                            newPosition = otherRectangle.Margin.Left + otherRectangle.ActualWidth;
                }
            }

            capturedRectangle.Margin = new Thickness(newPosition, 0, 0, 0);

            double smallerMargin = VerticalBar1.Margin.Left<VerticalBar2.Margin.Left?VerticalBar1.Margin.Left:VerticalBar2.Margin.Left;
            double biggerMargin = VerticalBar1.Margin.Left > VerticalBar2.Margin.Left ? VerticalBar1.Margin.Left : VerticalBar2.Margin.Left;
            HorizontalFillBar.Margin = new Thickness(HorizontalBar.Margin.Left+smallerMargin, 0, HorizontalBar.Margin.Right+(HorizontalBar.ActualWidth-biggerMargin), 0);

            first = false;
        }

        private void StartMouseCapture(MouseEventArgs e)
        {
            Cursor = Cursors.SizeWE;
            ForceCursor = true;
            double firstDistanceToCursor = Math.Abs(e.GetPosition(VerticalBar1).X);
            double secondDistanceToCursor = Math.Abs(e.GetPosition(VerticalBar2).X);
            if (capturedRectangle == null)
            {
                if (firstDistanceToCursor < secondDistanceToCursor)
                    capturedRectangle = VerticalBar1;
                else
                    capturedRectangle = VerticalBar2;
            }
            CaptureMouse();
        }

        private void EndMouseCapture()
        {
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            ForceCursor = false;
            capturedRectangle = null;
            first = true;
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EndMouseCapture();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            VerticalBar2.Margin = new Thickness(HorizontalBar.ActualWidth - HorizontalBar.Margin.Right, 0, 0, 0);
        }
    }
}
