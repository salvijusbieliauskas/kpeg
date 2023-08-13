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
        private Path capturedRectangle;
        public int VideoDuration { get; set; } = 1;
        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!IsMouseCaptured) return;
            
            double newPosition = e.GetPosition(this).X - (capturedRectangle.ActualWidth / 2);
            MoveBarTo(newPosition, capturedRectangle,true);
        }
        private double GetMaxPosition()
        {
            return HorizontalBar.ActualWidth - HorizontalBar.Margin.Right + VerticalBar1.ActualWidth / 2;
        }
        private void MoveBarTo(double newPosition, Path rectangleToMove, bool updateTextBoxes)
        {
            Path otherRectangle = rectangleToMove.Name == VerticalBar1.Name ? VerticalBar2 : VerticalBar1;
            double maxPosition = GetMaxPosition();
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
                if (newPosition == minPosition)
                    UpdateFrom(0);
                else
                    UpdateFrom((int)((smallerMargin / maxPosition) * VideoDuration));
                if (newPosition == maxPosition)
                    UpdateTo(VideoDuration);
                else
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
            double maxPosition = GetMaxPosition();
            double newPosition = ((double)seconds / VideoDuration) * maxPosition;
            MoveBarTo(newPosition, VerticalBar1,false);
        }

        private void UpdateBarTo(int seconds)
        {
            double maxPosition = GetMaxPosition();
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
            try
            {
                return int.Parse(FromHourBox.Text) * 3600 + int.Parse(FromMinBox.Text) * 60 +
                                  int.Parse(FromSecBox.Text);
            }catch(FormatException e)
            {
                return 0;
            }
        }
        private int GetToSeconds()
        {
            try {
            return int.Parse(ToHourBox.Text) * 3600 + int.Parse(ToMinBox.Text) * 60 +
                            int.Parse(ToSecBox.Text);
            }
            catch(FormatException e)
            {
                return 0;
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //VerticalBar2.Margin = new Thickness(HorizontalBar.ActualWidth - HorizontalBar.Margin.Right/2, 0, 0, 0);
            UpdateTo(VideoDuration);
            UpdateFrom(0);
            UpdateBarTo(VideoDuration);
            UpdateBarFrom(0);
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

        private void ClipCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            foreach(UIElement element in SliderGrid.Children)
            {
                element.IsEnabled = (bool)ClipCheckBox.IsChecked;
                if(element.GetType().Equals(typeof(StackPanel)))
                {
                    foreach (UIElement element2 in ((StackPanel)element).Children)
                    {
                        element2.IsEnabled = (bool)ClipCheckBox.IsChecked;
                    }
                }
            }
            foreach (UIElement element in TextBoxGrid.Children)
            {
                element.IsEnabled = (bool)ClipCheckBox.IsChecked;
                if (element.GetType().Equals(typeof(StackPanel)))
                {
                    foreach (UIElement element2 in ((StackPanel)element).Children)
                    {
                        element2.IsEnabled = (bool)ClipCheckBox.IsChecked;
                    }
                }
            }

            Brush primaryColor = ((bool)ClipCheckBox.IsChecked) ? (Brush)new BrushConverter().ConvertFromString("#9f9f9f") : (Brush)new BrushConverter().ConvertFromString("#5f5f5f");
            Brush secondaryColor = ((bool)ClipCheckBox.IsChecked) ? (Brush)new BrushConverter().ConvertFromString("#FFFD0009") : (Brush)new BrushConverter().ConvertFromString("#a80006");

            VerticalBar1.Fill = primaryColor;
            VerticalBar2.Fill = primaryColor;
            HorizontalBar.Fill = primaryColor;
            HorizontalFillBar.Fill = secondaryColor;
        }
        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (((TextBox)sender).SelectionLength == 0)
                ((TextBox)sender).SelectAll();
        }

        private void TextBox_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (((TextBox)sender).SelectionLength == 0)
                ((TextBox)sender).SelectAll();

            ((TextBox)sender).LostMouseCapture -= TextBox_LostMouseCapture;
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).LostMouseCapture += TextBox_LostMouseCapture;
            if (GetToSeconds() > VideoDuration)
                UpdateTo(VideoDuration);
            if (GetFromSeconds() < 0)
                UpdateFrom(0);
        }
    }
}
