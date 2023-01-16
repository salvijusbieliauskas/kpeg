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
        Border downloadBorder = new Border();
        TextBox textBox1 = new TextBox();
        TextBox textBox2 = new TextBox();
        ProgressBar downloadProgressBar = new ProgressBar();

        CheckBox openDirectoryCheckBox = new CheckBox();
        CheckBox openConverterCheckBox = new CheckBox();
        CheckBox convertToMp4CheckBox = new CheckBox();
        CheckBox audioOnlyCheckBox = new CheckBox();

        TextBlock videoTitleBlock = new TextBlock();
        System.Windows.Controls.Image videoThumbnail = new System.Windows.Controls.Image();
        Button downloadConfirmButton = new Button();
        System.Windows.Media.Brush mainBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#FFFD0009");
        int activeScreen = 0;//0 - starting screen; 1 - converter; 2 - downloader
        //int tempImageIndex = 0;
        public MainWindow()
        {
            InitializeComponent();
            //initialize components and their values
            Grid mainGrid = new Grid();
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

            downloadBorder.Name = "downloadBorder";
            downloadBorder.Opacity = 0.0;
            this.RegisterName(downloadBorder.Name,downloadBorder);
            mainGrid.Children.Add(downloadBorder);
            downloadBorder.Margin = new Thickness(0, 30, 0, 0);
            downloadBorder.IsEnabled = false;

            Grid downloadGrid = new Grid();
            downloadBorder.Child = downloadGrid;

            downloadGrid.Children.Add(textBox1);
            textBox1.Width = 400;
            textBox1.Height = 30;
            textBox1.TextAlignment = TextAlignment.Center;
            textBox1.TextChanged += textBox_TextChanged;
            textBox1.VerticalAlignment = VerticalAlignment.Top;
            textBox1.Margin = new Thickness(0, 10, 0, 0);
            textBox1.CaretBrush = mainBrush;

            downloadGrid.Children.Add(textBox2);
            textBox2.Width = 340;
            textBox2.Height = 30;
            textBox2.TextAlignment = TextAlignment.Center;
            textBox2.TextChanged += textBox2_TextChanged;
            textBox2.VerticalAlignment= VerticalAlignment.Top;
            textBox2.Margin = new Thickness(0, 50, 60, 0);
            textBox2.CaretBrush = mainBrush;
            textBox2.Text = GetDownloadFolderPath();

            Button browseButton = new Button();
            downloadGrid.Children.Add(browseButton);
            browseButton.Width = 60;
            browseButton.Padding = new Thickness(0, 0, 0, 0);
            browseButton.Background = mainBrush;
            browseButton.VerticalAlignment = VerticalAlignment.Top;
            browseButton.Margin = new Thickness(350, 50, 0, 0);
            browseButton.Content = "Browse";

            downloadGrid.Children.Add(downloadConfirmButton);
            downloadConfirmButton.Background = mainBrush;
            downloadConfirmButton.Margin = new Thickness(100, 0, 100, 300);
            downloadConfirmButton.Content = "Download";
            downloadConfirmButton.IsEnabled = false;

            downloadGrid.Children.Add(videoTitleBlock);
            videoTitleBlock.VerticalAlignment = VerticalAlignment.Center;
            videoTitleBlock.HorizontalAlignment = HorizontalAlignment.Center;
            videoTitleBlock.Margin = new Thickness(0, 0, 0, 200);
            videoTitleBlock.FontSize = 30;
            videoTitleBlock.Foreground = System.Windows.Media.Brushes.White;
            videoTitleBlock.TextWrapping = TextWrapping.Wrap;
            videoTitleBlock.TextAlignment = TextAlignment.Center;

            downloadGrid.Children.Add(videoThumbnail);
            videoThumbnail.Stretch = Stretch.Uniform;
            videoThumbnail.Width = 800;
            videoThumbnail.Height = 280;
            videoThumbnail.Margin = new Thickness(0, 145, 0, 0);
            videoThumbnail.IsHitTestVisible = false;

            downloadGrid.Children.Add(downloadProgressBar);
            downloadProgressBar.Height = 30;
            downloadProgressBar.Value = 0;
            downloadProgressBar.VerticalAlignment= VerticalAlignment.Bottom;
            downloadProgressBar.Foreground = mainBrush;

            downloadGrid.Children.Add(openDirectoryCheckBox);
            downloadGrid.Children.Add(openConverterCheckBox);
            downloadGrid.Children.Add(convertToMp4CheckBox);
            downloadGrid.Children.Add(audioOnlyCheckBox);

            openDirectoryCheckBox.IsChecked = Properties.Settings.Default.openDirectoryAfterDownload;
            openDirectoryCheckBox.FontSize = 16;
            openDirectoryCheckBox.Foreground = System.Windows.Media.Brushes.White;
            openDirectoryCheckBox.VerticalAlignment = VerticalAlignment.Top;
            openDirectoryCheckBox.Margin = new Thickness(10, 10, 0, 0);
            openDirectoryCheckBox.Content = "Open directory after download";
            openDirectoryCheckBox.Background = mainBrush;
            openDirectoryCheckBox.BorderBrush = mainBrush;
            openDirectoryCheckBox.Checked += openDirectoryCheckBoxChanged;
            openDirectoryCheckBox.Unchecked += openDirectoryCheckBoxChanged;

            openConverterCheckBox.IsChecked = Properties.Settings.Default.openConverterAfterDownload;
            openConverterCheckBox.FontSize = 16;
            openConverterCheckBox.Foreground = System.Windows.Media.Brushes.White;
            openConverterCheckBox.VerticalAlignment = VerticalAlignment.Top;
            openConverterCheckBox.Margin = new Thickness(10, 30, 0, 0);
            openConverterCheckBox.Content = "Open converter after download";
            openConverterCheckBox.Background = mainBrush;
            openConverterCheckBox.BorderBrush = mainBrush;
            openConverterCheckBox.Checked += openConverterCheckBoxChanged;
            openConverterCheckBox.Unchecked+= openConverterCheckBoxChanged;

            convertToMp4CheckBox.IsChecked = Properties.Settings.Default.convertToMp4;
            convertToMp4CheckBox.FontSize = 16;
            convertToMp4CheckBox.Foreground = System.Windows.Media.Brushes.White;
            convertToMp4CheckBox.VerticalAlignment = VerticalAlignment.Top;
            convertToMp4CheckBox.Margin = new Thickness(10, 50, 0, 0);
            convertToMp4CheckBox.Content = "Convert to mp4";
            convertToMp4CheckBox.Background = mainBrush;
            convertToMp4CheckBox.BorderBrush = mainBrush;
            convertToMp4CheckBox.Checked += mp4ConvertCheckBoxChanged;
            convertToMp4CheckBox.Unchecked += mp4ConvertCheckBoxChanged;

            audioOnlyCheckBox.IsChecked = Properties.Settings.Default.downloadAudioOnly;
            audioOnlyCheckBox.FontSize = 16;
            audioOnlyCheckBox.Foreground = System.Windows.Media.Brushes.White;
            audioOnlyCheckBox.VerticalAlignment = VerticalAlignment.Top;
            audioOnlyCheckBox.Margin = new Thickness(10, 70, 0, 0);
            audioOnlyCheckBox.Content = "Download audio only";
            audioOnlyCheckBox.Background = mainBrush;
            audioOnlyCheckBox.BorderBrush = mainBrush;
            audioOnlyCheckBox.Checked += audioOnlyCheckBoxChanged;
            audioOnlyCheckBox.Unchecked+= audioOnlyCheckBoxChanged;

            textBox_TextChanged(null, null);
            textBox2_TextChanged(null, null);

            me.Source = new Uri("Resources/ahri3.mp4", UriKind.Relative);
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
        private void openDirectoryCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.openDirectoryAfterDownload = openDirectoryCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
        }
        private void openConverterCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.openConverterAfterDownload = openConverterCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
            updateCheckBoxAccessibiity();
        }
        private void mp4ConvertCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.convertToMp4 = convertToMp4CheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
        }
        private void audioOnlyCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.downloadAudioOnly = audioOnlyCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
            updateCheckBoxAccessibiity();
        }
        private void updateCheckBoxAccessibiity()
        {
            if (openConverterCheckBox.IsChecked.Value || audioOnlyCheckBox.IsChecked.Value)
            {
                convertToMp4CheckBox.IsEnabled = false;
            }
            else if (!openConverterCheckBox.IsChecked.Value && !audioOnlyCheckBox.IsChecked.Value)
            {
                convertToMp4CheckBox.IsEnabled = true;
            }
        }
        public static string GetHomePath()
        {
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                return System.Environment.GetEnvironmentVariable("HOME");

            return System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }


        public static string GetDownloadFolderPath()
        {
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
            {
                string pathDownload = System.IO.Path.Combine(GetHomePath(), "Downloads");
                return pathDownload;
            }

            return System.Convert.ToString(Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders", "{374DE290-123F-4565-9164-39C4925E467B}", String.Empty));
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
                fadeControl(1.0, 0.0, 0.5, downloadBorder.Name);
                downloadBorder.IsEnabled = false;
            }
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
                downloadBorder.IsEnabled = true;
                if (delayed)
                    fadeControl(-1.0, 1.0, 1.0, downloadBorder.Name);
                else
                    fadeControl(0.0, 1.0, 0.5, downloadBorder.Name);
                if(isLinkValid(Clipboard.GetText()))
                    textBox1.Text= Clipboard.GetText();
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
        private bool isLinkValid(string link)
        {
            string linkType1 = "youtu.be/";
            string linkType2 = "watch?v=";
            if (link.IndexOf(linkType1) == -1 && link.IndexOf(linkType2) == -1)
                return false;
            if(link.IndexOf(linkType1)>-1)
            {
                return link.Length >= link.IndexOf(linkType1) + 11 + linkType1.Length;
            }
            else
            {
                return link.Length >= link.IndexOf(linkType2) + 11 + linkType2.Length;
            }
        }
        private string downloadThumbnail(string url)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/yt-dlp.exe");
                //info.Arguments = string.Format("--skip-download --write-thumbnail --output tmpimage{0} --convert-thumbnails png ", tempImageIndex) + url;
                //string outputPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/") + string.Format("tmpimage{0}.png", tempImageIndex);
                //tempImageIndex++;
                info.Arguments = "--skip-download --write-thumbnail --output tmpimage --convert-thumbnails png " + url;
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.WorkingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/");
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();
                //return outputPath;
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/tmpimage.png");
            }
        }
        private string getVideoName(string url)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/yt-dlp.exe"), "--skip-download --print title " + url);
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.WorkingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/");
                p.StartInfo = info;
                string accumulated = "";
                p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    accumulated += e.Data + '\n';
                });
                p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                {
                    accumulated += e.Data + '\n';
                });
                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                return accumulated.Trim();
            }
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
            Application.Current.Shutdown();
        }

        private void minimizeClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox1.Text == "")
            {

                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/placeholder1.png"));
                textImageBrush.AlignmentX = AlignmentX.Center;
                textImageBrush.Stretch = Stretch.None;

                textBox1.Background = textImageBrush;
            }
            else
            {

                textBox1.Background = null;
            }
            if (isLinkValid(textBox1.Text))
            {
                downloadConfirmButton.IsEnabled = true;
                string url = textBox1.Text;
                System.Threading.Thread thread = new System.Threading.Thread(() =>
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        videoTitleBlock.Text = "";
                    }));
                    string videoName = getVideoName(url);
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        videoTitleBlock.Text = videoName;
                    }));
                });
                System.Threading.Thread thread2 = new System.Threading.Thread(() =>
                {
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        videoThumbnail.Source = null;
                    }));
                    string path = downloadThumbnail(url);
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        videoThumbnail.Source = uriToSource(path);
                    }));
                });
                thread.Start();
                thread2.Start();
            }
            else
            {
                downloadConfirmButton.IsEnabled = false;
                videoTitleBlock.Text = "";
                videoThumbnail.Source = null;
            }
        }
        private BitmapImage uriToSource(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = fs;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox2.Text == "")
            {

                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/placeholder2.png"));
                textImageBrush.AlignmentX = AlignmentX.Center;
                textImageBrush.Stretch = Stretch.None;

                textBox2.Background = textImageBrush;
            }
            else
            {

                textBox2.Background = null;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/"));
            foreach(FileInfo fileInfo in di.GetFiles()) {
                if (fileInfo.Name.ToLower().StartsWith("tmp"))
                    fileInfo.Delete();
            }
        }
    }
}
