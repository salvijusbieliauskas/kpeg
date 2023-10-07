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
using System.Windows.Shell;

namespace kpeg
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Windows.Media.Brush mainBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FFFD0009");
        public static UserControl activeWindow = null;
        private static MainWindow mainWindowInstance = null;
        private static double animationDuration = 0.5;
        public static MainWindow GetInstance()
        {
            if (mainWindowInstance == null)
                mainWindowInstance = new MainWindow();
            return mainWindowInstance;
        }
        private MainWindow()
        {
            InitializeComponent();
            mediaElement.Source = new Uri("Resources/ahri.wmv", UriKind.Relative);
            FadeInWindow(StartingWindow.GetInstance(), false);
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

        private void PlayVideo()
        {
            mediaElement.Play();
            MainWindow.GetInstance().FadeControl(-0.5, 1.0, 5, mediaElement);
        }
        private void ResetPosition()
        {
            mediaElement.Position = TimeSpan.FromSeconds(0);
        }
        private void VideoEnded(object sender, RoutedEventArgs e)
        {
            ResetPosition();
        }
        private void VideoLoaded(object sender, RoutedEventArgs e)
        {
            PlayVideo();
        }
        private void FadeOutCurrent()
        {
            FadeControl(1.0, 0.0, animationDuration, activeWindow);
            UserControl windowToRemove = activeWindow;
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep((int)(animationDuration * 1000));
                Dispatcher.Invoke(new Action(() => 
                {
                    this.mainWindowDockPanel.Children.Remove(windowToRemove);
                }));
            });
        }
        private void CopyLinkFromClipboard()
        {
            if (Utils.IsLinkValid(Clipboard.GetText()))
                DownloadWindow.GetInstance().GetLinkBox().Text = Clipboard.GetText();
        }
        public void FadeInWindow(UserControl window, bool delayed)
        {
            if(activeWindow!=null && !activeWindow.Equals(window))
                FadeOutCurrent();

            this.mainWindowDockPanel.Children.Add(window);
            if (delayed)
                FadeControl(-1.0, 1.0, animationDuration*2, window);
            else
                FadeControl(0.0, 1.0, animationDuration, window);
            activeWindow = window;
        }
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            if (activeWindow.Equals(StartingWindow.GetInstance()))
                return;
            FadeInWindow(StartingWindow.GetInstance(), true);
        }

        public void FadeControl(double from, double to, double durationSeconds, DependencyObject control)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation();
            fadeAnimation.Duration = TimeSpan.FromSeconds(durationSeconds);
            fadeAnimation.AutoReverse = false;
            fadeAnimation.From = from;
            fadeAnimation.To = to;
            Storyboard board = new Storyboard();
            board.Children.Add(fadeAnimation);
            Storyboard.SetTarget(fadeAnimation, control);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(OpacityProperty));
            Storyboard.SetDesiredFrameRate(fadeAnimation, 60);
            board.Begin(this);
        }

        private void TopBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseClick(object sender, RoutedEventArgs e)
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
            if (activeWindow.Equals(DownloadWindow.GetInstance()) && !Utils.IsLinkValid(DownloadWindow.GetInstance().GetLinkBox().Text))
                CopyLinkFromClipboard();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            new ExceptionWindow(e.ErrorException);
        }
    }
}
