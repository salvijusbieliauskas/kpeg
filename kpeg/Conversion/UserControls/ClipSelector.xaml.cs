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
            VideoDuration = 3600*5;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StartMouseCapture(e);
            UserControl_MouseMove(sender, e);
        }
        private Path capturedRectangle;
        public int VideoDuration { get; set; } = 1;
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!IsMouseCaptured) return;
            
            double newPosition = e.GetPosition(this).X - (capturedRectangle.ActualWidth / 2);
            MoveBarTo(newPosition, capturedRectangle,true);
        }

        private void MoveBarTo(double newPosition, Path rectangleToMove, bool updateTextBoxes)
        {
            Path otherRectangle = rectangleToMove.Name == VerticalBar1.Name ? VerticalBar2 : VerticalBar1;
            double maxPosition = HorizontalBar.ActualWidth - HorizontalBar.Margin.Right+rectangleToMove.ActualWidth/2;
            double minPosition = HorizontalBar.Margin.Left-rectangleToMove.ActualWidth/2;

            if (newPosition < minPosition)
                newPosition = minPosition;
            else if (newPosition > maxPosition)
                newPosition = maxPosition;

            rectangleToMove.Margin = new Thickness(newPosition, 0, 0, 0);

            double smallerMargin = VerticalBar1.Margin.Left < VerticalBar2.Margin.Left ? VerticalBar1.Margin.Left : VerticalBar2.Margin.Left;
            double biggerMargin = VerticalBar1.Margin.Left > VerticalBar2.Margin.Left ? VerticalBar1.Margin.Left : VerticalBar2.Margin.Left;
            HorizontalFillBar.Margin = new Thickness(HorizontalBar.Margin.Left + smallerMargin, 0, HorizontalBar.Margin.Right + (HorizontalBar.ActualWidth - biggerMargin), 0);

            if (updateTextBoxes)
            {
                UpdateFrom((int)((smallerMargin / maxPosition) * VideoDuration));
                UpdateTo((int)((biggerMargin / maxPosition) * VideoDuration));
            }
        }

        private void UpdateFrom(int seconds)
        {
            TimeSpan span = TimeSpan.FromSeconds(seconds);

            FromHourBox.Text = span.Hours.ToString().PadLeft(2, '0');
            FromMinBox.Text = span.Minutes.ToString().PadLeft(2, '0');
            FromSecBox.Text = span.Seconds.ToString().PadLeft(2, '0');
        }
        private void UpdateTo(int seconds)
        {
            TimeSpan span = TimeSpan.FromSeconds(seconds);

            ToHourBox.Text = span.Hours.ToString().PadLeft(2, '0');
            ToMinBox.Text = span.Minutes.ToString().PadLeft(2, '0');
            ToSecBox.Text = span.Seconds.ToString().PadLeft(2, '0');
        }

        private void UpdateBarFrom(int seconds)
        {
            double maxPosition = HorizontalBar.ActualWidth - HorizontalBar.Margin.Right;
            double newPosition = ((double)seconds / VideoDuration) * maxPosition;
            MoveBarTo(newPosition, VerticalBar1,false);
        }

        private void UpdateBarTo(int seconds)
        {
            double maxPosition = HorizontalBar.ActualWidth - HorizontalBar.Margin.Right;
            double newPosition = (seconds / VideoDuration) * maxPosition;
            MoveBarTo(newPosition, VerticalBar2,false);
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
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            EndMouseCapture();
        }
        private int GetFromSeconds()
        {
            return int.Parse(FromHourBox.Text) * 3600 + int.Parse(FromMinBox.Text) * 60 +
                              int.Parse(FromSecBox.Text);
        }
        private int GetToSeconds()
        {
            return int.Parse(ToHourBox.Text) * 3600 + int.Parse(ToMinBox.Text) * 60 +
                            int.Parse(ToSecBox.Text);
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            VerticalBar2.Margin = new Thickness(HorizontalBar.ActualWidth - HorizontalBar.Margin.Right, 0, 0, 0);
        }
        private void TimeBoxChanged(object sender, RoutedEventArgs e)
        {
            if (FromHourBox == null || FromMinBox == null || FromSecBox == null || ToHourBox == null || ToMinBox == null || ToSecBox == null)
                return;
            TextBox senderBox = (TextBox)sender;
            if (senderBox.Text.Length > 2)
            {
                senderBox.Text = senderBox.Text.Substring(0, 2);
                senderBox.CaretIndex = 2;
            }
            for (int x = 0; x < senderBox.Text.Length; x++)
            {
                if (!Char.IsNumber(senderBox.Text, x))
                {
                    senderBox.Text = senderBox.Text.Remove(x, 1);
                }
            }
            if(DurationLabel!=null)
                DurationLabel.Content = $"Duration: {TimeSpan.FromSeconds(GetToSeconds()-GetFromSeconds()).ToString(@"hh\:mm\:ss")}";
            if (IsMouseCaptured)
                return;

            try
            {
                UpdateBarFrom(GetFromSeconds());
                UpdateBarTo(GetToSeconds());
            }
            catch (System.FormatException)
            {
            }
        }
    }
}
