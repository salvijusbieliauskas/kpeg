using kpeg.Downloading;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace kpeg
{
    public partial class MainWindow : Window
    {
        public System.Windows.Media.Brush MainBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FFFD0009");
        public static UserControl ActiveWindow = null;
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
            this.Height = (double)SettingsManager.Get(Setting.WindowHeight);
            this.Width = (double)SettingsManager.Get(Setting.WindowWidth);
            SetStartingWindowPosition();
            if ((bool)SettingsManager.Get(Setting.IsMaximized))
                SetMaximizedState();
        }
        private void SetStartingWindowPosition()
        {
            object lastPositionTop = SettingsManager.Get(Setting.LastPositionTop);
            object lastPositionLeft = SettingsManager.Get(Setting.LastPositionLeft);
            if (lastPositionTop == null || (lastPositionTop.GetType().Equals(typeof(string)) && (string)lastPositionTop=="NaN")) 
            {
                lastPositionTop = this.Top;
            }
            if (lastPositionLeft == null || (lastPositionLeft.GetType().Equals(typeof(string)) && (string)lastPositionLeft == "NaN"))
            {
                lastPositionLeft = this.Left;
            }
            this.Left = (double)lastPositionLeft;
            this.Top = (double)lastPositionTop;
        }

        private void TerminalWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindowContentGrid.Children.Contains(TerminalWindow.GetInstance()))
            {
                mainWindowContentGrid.Children.Remove(TerminalWindow.GetInstance());
            }
            else
            {
                mainWindowContentGrid.Children.Add(TerminalWindow.GetInstance());
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
            FadeControl(1.0, 0.0, animationDuration, ActiveWindow);
            UserControl windowToRemove = ActiveWindow;
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep((int)(animationDuration * 1000));
                Dispatcher.Invoke(new Action(() =>
                {
                    this.mainWindowContentGrid.Children.Remove(windowToRemove);
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
            if (ActiveWindow != null && !ActiveWindow.Equals(window))
                FadeOutCurrent();
            if (!window.Equals(StartingWindow.GetInstance()))
            {
                FadeControl(1.0, 0.0, animationDuration, mediaElement);
            }
            else
            {
                FadeControl(0.0, 1.0, animationDuration, mediaElement);
            }
            if (window.Equals(ActiveWindow))
                return;

            this.mainWindowContentGrid.Children.Insert(0, window);
            if (delayed)
                FadeControl(-1.0, 1.0, animationDuration * 2, window);
            else
                FadeControl(0.0, 1.0, animationDuration, window);
            ActiveWindow = window;
        }
        private void StartClicked(object sender, RoutedEventArgs e)
        {
            if (ActiveWindow.Equals(StartingWindow.GetInstance()))
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
            SettingsManager.Set(Setting.LastPositionTop, this.Top);
            SettingsManager.Set(Setting.LastPositionLeft, this.Left);
            SettingsManager.Set(Setting.IsMaximized, this.WindowState==WindowState.Maximized);
            if (this.WindowState != WindowState.Maximized)
            {
                SettingsManager.Set(Setting.WindowWidth, this.Width);
                SettingsManager.Set(Setting.WindowHeight, this.Height);
            }
        }

        private void WindowActivated(object sender, EventArgs e)
        {
            if (!Utils.IsLinkValid(DownloadWindow.GetInstance().GetLinkBox().Text))
                CopyLinkFromClipboard();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }
        private void SetNormalState()
        {
            this.WindowState = WindowState.Normal;
            maximizeBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/kpeg;component/Resources/maximizeIcon.png"));
            this.ResizeMode = ResizeMode.CanResize;
        }
        private void SetMaximizedState()
        {
            System.Drawing.Rectangle r = System.Windows.Forms.Screen.GetWorkingArea(new System.Drawing.Point((int)this.Left + (int)(this.Width / 2), (int)this.Top + (int)(this.Height / 2)));
            SettingsManager.Set(Setting.LastPositionTop, this.Top);
            SettingsManager.Set(Setting.LastPositionLeft, this.Left);
            SettingsManager.Set(Setting.WindowWidth, this.Width);
            SettingsManager.Set(Setting.WindowHeight, this.Height);
            this.MaxWidth = r.Width;
            this.MaxHeight = r.Height;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowState = WindowState.Maximized;
            maximizeBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/kpeg;component/Resources/unMaximizeIcon.png"));
        }
        private void AdjustWindowSize()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                SetNormalState();
            }
            else
            {
                SetMaximizedState();
            }
        }
        private void mediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            new ExceptionWindow(e.ErrorException);
        }
        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            AdjustWindowSize();
        }
    }
}
