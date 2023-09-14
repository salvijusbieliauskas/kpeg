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

namespace kpeg
{
    /// <summary>
    /// Interaction logic for TerminalWindow.xaml
    /// </summary>
    public partial class TerminalWindow : UserControl
    {
        private static TerminalWindow TerminalWindowInstance = null;
        public static TerminalWindow GetInstance()
        {
            if (TerminalWindowInstance == null)
                TerminalWindowInstance = new TerminalWindow();
            return TerminalWindowInstance;
        }
        public TerminalView ThumbnailDownloaderView = new TerminalView("ThumbnailDownloaderView");
        public TerminalView VideoDownloaderView = new TerminalView("VideoDownloaderView");
        public TerminalView VideoNameDownloaderView = new TerminalView("VideoNameDownloaderView");
        public TerminalView YtdlpUpdaterView = new TerminalView("YtdlpUpdaterView");
        public TerminalView VideoMetadataDownloaderView = new TerminalView("VideoMetadataDownloaderView");
        public TerminalView DownloadedVideoConverterView = new TerminalView("DownloadedVideoConverterView");
        public TerminalView FFProbeContainerView = new TerminalView("FFProbeContainerView");
        private TerminalWindow()
        {
            InitializeComponent();
            TerminalViewGrid.Children.Add(ThumbnailDownloaderView);
            TerminalViewGrid.Children.Add(VideoDownloaderView);
            TerminalViewGrid.Children.Add(VideoNameDownloaderView);
            TerminalViewGrid.Children.Add(YtdlpUpdaterView);
            TerminalViewGrid.Children.Add(VideoMetadataDownloaderView);
            TerminalViewGrid.Children.Add(DownloadedVideoConverterView);
            TerminalViewGrid.Children.Add(FFProbeContainerView);
            SetActiveView(VideoDownloaderView);
            this.IsHitTestVisible = false;
            this.IsEnabled = false;
            this.Visibility = Visibility.Hidden;
            MainWindow.GetInstance().mainGrid.Children.Add(this);
        }
        private void SetActiveView(TerminalView activeView)
        {
            foreach (TerminalView view in TerminalViewGrid.Children)
            {
                if (activeView.DName.Equals(view.DName))
                {
                    view.Visibility = Visibility.Visible;
                    view.IsEnabled = true;
                    view.TextBox.Visibility = Visibility.Visible;
                    view.TextBox.IsEnabled = true;
                    continue;
                }
                view.Visibility = Visibility.Hidden;
                view.IsEnabled=false;
                view.TextBox.Visibility = Visibility.Hidden;
                view.TextBox.IsEnabled = false;
            }
        }
        private void ThumbnailDownloaderButtonClicked(object sender, RoutedEventArgs e)
        {
            SetActiveView(ThumbnailDownloaderView);
        }

        private void VideoDownloaderButtonClicked(object sender, RoutedEventArgs e)
        {
            SetActiveView(VideoDownloaderView);
        }

        private void VideoNameDownloaderButtonClicked(object sender, RoutedEventArgs e)
        {
            SetActiveView(VideoNameDownloaderView);
        }

        private void YtdlpUpdaterButtonClicked(object sender, RoutedEventArgs e)
        {
            SetActiveView(YtdlpUpdaterView);
        }

        private void VideoMetadataDownloaderClicked(object sender, RoutedEventArgs e)
        {
            SetActiveView(VideoMetadataDownloaderView);
        }
        private void DownloadedVideoConverterClicked(object sender, RoutedEventArgs e)
        {
            SetActiveView(DownloadedVideoConverterView);
        }

        private void FFProbeContainerClicked(object sender, RoutedEventArgs e)
        {
            SetActiveView(FFProbeContainerView);
        }
    }
}
