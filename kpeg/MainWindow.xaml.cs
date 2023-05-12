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
            //initialize components and their values
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
            startButton.Click += startClicked;

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
            convertButton.Content = "Convert";

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
            downloadButton.Click += downloadClicked;
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
            me.Loaded += videoLoaded;
            me.MediaEnded += videoEnded;
            me.Opacity = 0;
            me.Volume = 0;
            me.Name = "mediaElement";
            this.RegisterName(me.Name, me);
            vb.Visual = me;
            startingBorder.Background = vb;
        }
        private void fadeOutCurrent()
        {
            if (activeScreen == 0)
            {
                fadeControl(1.0, 0.0, 0.5, startingBorder.Name);
                startingBorder.IsEnabled = false;
            }
            else if(activeScreen == 2)
            {
                fadeControl(1.0, 0.0, 0.5, DownloadWindow.GetInstance().getBorder().Name);
                DownloadWindow.GetInstance().getBorder().IsEnabled = false;
            }
        }
        private void copyLinkFromClipboard()
        {
            if (Utils.isLinkValid(Clipboard.GetText()))
                DownloadWindow.GetInstance().getLinkBox().Text = Clipboard.GetText();
        }
        private void fadeInScreen(int screen, bool delayed)
        {
            if(screen == 0)
            {
                fadeOutCurrent();
                startingBorder.IsEnabled = true;
                if (delayed)
                    fadeControl(-1.0, 1.0, 1.0, startingBorder.Name);
                else
                    fadeControl(0.0, 1.0, 0.5, startingBorder.Name);
            }
            else if(screen == 2)
            {
                fadeOutCurrent();
                DownloadWindow.GetInstance().getBorder().IsEnabled = true;
                if (delayed)
                    fadeControl(-1.0, 1.0, 1.0, DownloadWindow.GetInstance().getBorder().Name);
                else
                    fadeControl(0.0, 1.0, 0.5, DownloadWindow.GetInstance().getBorder().Name);
                copyLinkFromClipboard();
            }
            activeScreen = screen;
        }
        private void videoLoaded(object sender, RoutedEventArgs e)
        {
            playVideo();
        }
        private void startClicked(object sender, RoutedEventArgs e)
        {
            if (activeScreen == 0)
                return;
            fadeInScreen(0, true);
        }
        private void fadeControl(double from, double to, double durationSeconds, string controlName)
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
        private void playVideo()
        {
            if (activeScreen == 0)
            {
                me.Play();
                fadeControl(-0.5, 1.0, 5, me.Name);
            }
        }
        private void stopVideo()
        {
            if (activeScreen == 0)
            {
                me.Stop();
                resetPosition();
            }
        }
        private void resetPosition()
        {
            me.Position = TimeSpan.FromSeconds(0);
        }
        private void videoEnded(object sender, RoutedEventArgs e)
        {
            resetPosition();
        }
        private void downloadClicked(object sender, RoutedEventArgs e)
        {
            fadeInScreen(2, true);
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
            Utils.cleanupFiles();
        }

        private void windowActivated(object sender, EventArgs e)
        {
            if (activeScreen == 2 && !Utils.isLinkValid(DownloadWindow.GetInstance().getLinkBox().Text))
                copyLinkFromClipboard();
        }
    }
}
