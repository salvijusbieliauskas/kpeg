using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.Windows.Shapes;
using System.Drawing.Imaging;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Diagnostics;
using System.Security.Policy;
using kpeg.Conversion;
using kpeg.Downloading;
using Microsoft.Win32;

namespace kpeg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaElement me = new MediaElement();
        VisualBrush vb = new VisualBrush();
        Border startingBorder = new Border();
        Border convertBorder = new Border();

        public Grid mainGrid = new Grid();
        public System.Windows.Media.Brush mainBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FFFD0009");
        int activeScreen = 0;//0 - starting screen; 1 - converter; 2 - downloader
        //int tempImageIndex = 0;
        private static MainWindow mainWindowInstance = null;

        public static MainWindow GetInstance()
        {
            if (mainWindowInstance == null)
                mainWindowInstance = new MainWindow();
            return mainWindowInstance;
        }
        private MainWindow()
        {
            InitializeComponent();
            this.AddChild(mainGrid);

            Border barBorder = new Border();
            mainGrid.Children.Add(barBorder);
            barBorder.Height = 30;
            barBorder.VerticalAlignment = VerticalAlignment.Top;

            Grid barGrid = new Grid();
            barBorder.Child = barGrid;

            System.Windows.Controls.Image barImage = new System.Windows.Controls.Image();
            barGrid.Children.Add(barImage);
            barImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/top.png"));
            barImage.MouseDown += Image_MouseDown;

            Button exitButton = new Button();
            barGrid.Children.Add(exitButton);
            exitButton.VerticalAlignment = VerticalAlignment.Top;
            exitButton.HorizontalAlignment = HorizontalAlignment.Right;
            exitButton.Height = 30;
            exitButton.Width = 30;

            exitButton.BorderBrush = null;
            exitButton.Foreground = null;
            exitButton.Click += closeClick;
            ImageBrush exitImage = new ImageBrush();
            exitImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/exitbutton.png"));
            exitButton.Background = exitImage;

            Button minimizeButton = new Button();
            barGrid.Children.Add(minimizeButton);
            minimizeButton.VerticalAlignment = VerticalAlignment.Top;
            minimizeButton.HorizontalAlignment = HorizontalAlignment.Right;
            minimizeButton.Height = 30;
            minimizeButton.Width = 30;
            minimizeButton.BorderBrush = null;
            minimizeButton.Foreground = null;
            minimizeButton.Click += minimizeClick;
            minimizeButton.Margin = new Thickness(0, 0, 30, 0);
            ImageBrush minimizeImage = new ImageBrush();
            minimizeImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/minimizebutton.png"));
            minimizeButton.Background = minimizeImage;

            Button startButton = new Button();
            barGrid.Children.Add(startButton);
            startButton.VerticalAlignment = VerticalAlignment.Top;
            startButton.HorizontalAlignment = HorizontalAlignment.Left;
            startButton.Height = 30;
            startButton.Width = 95;
            startButton.Background = null;
            startButton.BorderBrush = null;
            startButton.Foreground = null;
            startButton.Click += StartClicked;

            Button terminalWindowButton = new Button();
            barGrid.Children.Add(terminalWindowButton);
            terminalWindowButton.VerticalAlignment = VerticalAlignment.Top;
            terminalWindowButton.HorizontalAlignment = HorizontalAlignment.Left;
            terminalWindowButton.Height = 30;
            terminalWindowButton.Width = 30;
            terminalWindowButton.Margin = new Thickness(95, 0, 0, 0);
            terminalWindowButton.BorderBrush = null;
            terminalWindowButton.Click += TerminalWindowButton_Click;
            terminalWindowButton.Foreground = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/terminalIcon.png")));
            terminalWindowButton.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/terminalIcon.png")));


            mainGrid.Children.Add(startingBorder);
            startingBorder.Name = "startingBorder";
            this.RegisterName(startingBorder.Name, startingBorder);
            startingBorder.Margin = new Thickness(0, 30, 0, 0);

            Grid startingGrid = new Grid();
            startingBorder.Child = startingGrid;

            Button convertButton = new Button();
            startingGrid.Children.Add(convertButton);
            convertButton.VerticalAlignment = VerticalAlignment.Center;
            convertButton.HorizontalAlignment = HorizontalAlignment.Left;
            convertButton.Margin = new Thickness(50, 0, 0, 0);
            convertButton.FontSize = 30;
            convertButton.Width = 200;
            convertButton.Height = 50;
            convertButton.Background = mainBrush;
            convertButton.Click += convertClicked;
            convertButton.Content = "Convert";
            convertButton.IsEnabled = false;

            MaterialDesignThemes.Wpf.ElevationAssist.SetElevation(convertButton, MaterialDesignThemes.Wpf.Elevation.Dp24);

            Button downloadButton = new Button();
            startingGrid.Children.Add(downloadButton);
            downloadButton.VerticalAlignment = VerticalAlignment.Center;
            downloadButton.HorizontalAlignment = HorizontalAlignment.Right;
            downloadButton.Margin = new Thickness(0, 0, 50, 0);
            downloadButton.FontSize = 30;
            downloadButton.Width = 200;
            downloadButton.Height = 50;
            downloadButton.Background = mainBrush;
            downloadButton.Content = "Download";
            downloadButton.Click += DownloadClicked;
            MaterialDesignThemes.Wpf.ElevationAssist.SetElevation(downloadButton, MaterialDesignThemes.Wpf.Elevation.Dp24);



            convertBorder.Name = "convertBorder";
            convertBorder.Opacity = 0.0;
            this.RegisterName(convertBorder.Name, convertBorder);
            mainGrid.Children.Add(convertBorder);
            convertBorder.Margin = new Thickness(0, 30, 0, 0);
            convertBorder.IsEnabled = false;

            Grid convertGrid = new Grid();
            convertBorder.Child = convertGrid;

            me.Source = new Uri("Resources/ahri.wmv", UriKind.Relative);
            me.Stretch = Stretch.Fill;
            me.LoadedBehavior = MediaState.Manual;
            me.Loaded += VideoLoaded;
            me.MediaEnded += VideoEnded;
            me.Opacity = 0;
            me.Volume = 0;
            me.Name = "mediaElement";
            this.RegisterName(me.Name, me);
            vb.Visual = me;
            startingBorder.Background = vb;
        }

        private void TerminalWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (TerminalWindow.GetInstance().IsEnabled)
            {
                TerminalWindow.GetInstance().Visibility = Visibility.Hidden;
                TerminalWindow.GetInstance().IsHitTestVisible = false;
                TerminalWindow.GetInstance().IsEnabled = false;
            }
            else
            {
                TerminalWindow.GetInstance().Visibility = Visibility.Visible;
                TerminalWindow.GetInstance().IsHitTestVisible = true;
                TerminalWindow.GetInstance().IsEnabled = true;
            }

        }

        private void FadeOutCurrent()
        {
            if (activeScreen == 0)
            {
                FadeControl(1.0, 0.0, 0.5, startingBorder.Name);
                startingBorder.IsEnabled = false;
            }
            else if (activeScreen == 1)
            {
                FadeControl(1.0, 0.0, 0.5, ConvertWindow.GetInstance().Name);
                ConvertWindow.GetInstance().IsEnabled = false;
                ConvertWindow.GetInstance().IsHitTestVisible = false;
            }
            else if(activeScreen == 2)
            {
                FadeControl(1.0, 0.0, 0.5, DownloadWindow.GetInstance().GetBorder().Name);
                DownloadWindow.GetInstance().GetBorder().IsEnabled = false;
            }
        }
        private void CopyLinkFromClipboard()
        {
            if (Utils.IsLinkValid(Clipboard.GetText()))
                DownloadWindow.GetInstance().GetLinkBox().Text = Clipboard.GetText();
        }
        private void FadeInScreen(int screen, bool delayed)
        {
            if(screen == 0)
            {
                FadeOutCurrent();
                startingBorder.IsEnabled = true;
                if (delayed)
                    FadeControl(-1.0, 1.0, 1.0, startingBorder.Name);
                else
                    FadeControl(0.0, 1.0, 0.5, startingBorder.Name);
            }
            else if (screen == 1)
            {
                FadeOutCurrent();
                ConvertWindow.GetInstance().IsEnabled = true;
                ConvertWindow.GetInstance().IsHitTestVisible = true;
                if (delayed)
                    FadeControl(-1.0, 1.0, 1.0, ConvertWindow.GetInstance().Name);
                else
                    FadeControl(0.0, 1.0, 0.5, ConvertWindow.GetInstance().Name);
            }
            else if(screen == 2)
            {
                FadeOutCurrent();
                DownloadWindow.GetInstance().GetBorder().IsEnabled = true;
                if (delayed)
                    FadeControl(-1.0, 1.0, 1.0, DownloadWindow.GetInstance().GetBorder().Name);
                else
                    FadeControl(0.0, 1.0, 0.5, DownloadWindow.GetInstance().GetBorder().Name);
                CopyLinkFromClipboard();
            }
            activeScreen = screen;
        }
        private void VideoLoaded(object sender, RoutedEventArgs e)
        {
            PlayVideo();
        }
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            if (activeScreen == 0)
                return;
            FadeInScreen(0, true);
        }

        public void FadeControl(double from, double to, double durationSeconds, string controlName)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromSeconds(durationSeconds);
            fadeAnimation.AutoReverse = false;
            fadeAnimation.From = from;
            fadeAnimation.To = to;
            Storyboard board = new Storyboard();
            board.Children.Add(fadeAnimation);
            Storyboard.SetTargetName(fadeAnimation, controlName);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(OpacityProperty));
            Storyboard.SetDesiredFrameRate(fadeAnimation, 60);
            board.Begin(this);
        }
        private void PlayVideo()
        {
            if (activeScreen == 0)
            {
                me.Play();
                FadeControl(-0.5, 1.0, 5, me.Name);
            }
        }
        private void StopVideo()
        {
            if (activeScreen == 0)
            {
                me.Stop();
                ResetPosition();
            }
        }
        private void ResetPosition()
        {
            me.Position = TimeSpan.FromSeconds(0);
        }
        private void VideoEnded(object sender, RoutedEventArgs e)
        {
            ResetPosition();
        }
        private void DownloadClicked(object sender, RoutedEventArgs e)
        {
            FadeInScreen(2, true);
        }
        private void convertClicked(object sender, RoutedEventArgs e)
        {
            FadeInScreen(1, true);
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void closeClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void minimizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Utils.CleanupFiles();
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            if (activeScreen == 2 && !Utils.IsLinkValid(DownloadWindow.GetInstance().GetLinkBox().Text))
                CopyLinkFromClipboard();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
    }
}
