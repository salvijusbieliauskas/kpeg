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
        Border downloadBorder = new Border();
        TextBox textBox1 = new TextBox();
        TextBox textBox2 = new TextBox();
        Label downloadProgressLabel = new Label();
        ProgressBar downloadProgressBar = new ProgressBar();

        CheckBox openDirectoryCheckBox = new CheckBox();
        CheckBox openConverterCheckBox = new CheckBox();
        CheckBox convertToMp4CheckBox = new CheckBox();
        CheckBox audioOnlyCheckBox = new CheckBox();
        CheckBox convertToMp3CheckBox = new CheckBox();
        CheckBox setDateModifiedToCurrentCheckBox = new CheckBox();

        TextBlock videoTitleBlock = new TextBlock();
        System.Windows.Controls.Image videoThumbnail = new System.Windows.Controls.Image();
        Button downloadConfirmButton = new Button();
        private bool downloadInProgress = false;
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

            downloadGrid.Children.Add(openDirectoryCheckBox);
            downloadGrid.Children.Add(openConverterCheckBox);
            downloadGrid.Children.Add(convertToMp4CheckBox);
            downloadGrid.Children.Add(audioOnlyCheckBox);
            downloadGrid.Children.Add(convertToMp3CheckBox);
            downloadGrid.Children.Add(setDateModifiedToCurrentCheckBox);

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
            openConverterCheckBox.Unchecked += openConverterCheckBoxChanged;

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
            audioOnlyCheckBox.Unchecked += audioOnlyCheckBoxChanged;

            convertToMp3CheckBox.IsChecked = Properties.Settings.Default.downloadAudioAsMp3;
            convertToMp3CheckBox.FontSize = 16;
            convertToMp3CheckBox.Foreground = System.Windows.Media.Brushes.White;
            convertToMp3CheckBox.VerticalAlignment = VerticalAlignment.Top;
            convertToMp3CheckBox.Margin = new Thickness(10, 90, 0, 0);
            convertToMp3CheckBox.Content = "Convert audio to mp3 (nukes quality)";
            convertToMp3CheckBox.Background = mainBrush;
            convertToMp3CheckBox.BorderBrush = mainBrush;
            convertToMp3CheckBox.Checked += mp3ConvertCheckBoxChanged;
            convertToMp3CheckBox.Unchecked += mp3ConvertCheckBoxChanged;

            setDateModifiedToCurrentCheckBox.IsChecked = Properties.Settings.Default.setModifiedDate;
            setDateModifiedToCurrentCheckBox.FontSize = 16;
            setDateModifiedToCurrentCheckBox.Foreground = System.Windows.Media.Brushes.White;
            setDateModifiedToCurrentCheckBox.VerticalAlignment = VerticalAlignment.Top;
            setDateModifiedToCurrentCheckBox.Margin = new Thickness(700, 10, 0, 0);
            setDateModifiedToCurrentCheckBox.Content = "Set modified date to current";
            setDateModifiedToCurrentCheckBox.Background = mainBrush;
            setDateModifiedToCurrentCheckBox.BorderBrush = mainBrush;
            setDateModifiedToCurrentCheckBox.Checked += setDateCheckBoxChanged;
            setDateModifiedToCurrentCheckBox.Unchecked += setDateCheckBoxChanged;

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
            textBox2.TextChanged += downloadDirectoryChanged;
            textBox2.Text = GetDownloadFolderPath();

            Button browseButton = new Button();
            downloadGrid.Children.Add(browseButton);
            browseButton.Width = 60;
            browseButton.Padding = new Thickness(0, 0, 0, 0);
            browseButton.Background = mainBrush;
            browseButton.VerticalAlignment = VerticalAlignment.Top;
            browseButton.Margin = new Thickness(350, 50, 0, 0);
            browseButton.Content = "Browse";
            browseButton.Click += browseButtonClicked;

            downloadGrid.Children.Add(downloadConfirmButton);
            downloadConfirmButton.Background = mainBrush;
            downloadConfirmButton.Margin = new Thickness(100, 0, 100, 300);
            downloadConfirmButton.Content = "Download";
            downloadConfirmButton.IsEnabled = false;
            downloadConfirmButton.Click += downloadConfirmButtonClicked;
            downloadConfirmButton.Name = "downloadConfirmButton";
            this.RegisterName(downloadConfirmButton.Name, downloadConfirmButton);

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

            downloadGrid.Children.Add(downloadProgressLabel);
            downloadProgressLabel.VerticalAlignment= VerticalAlignment.Bottom;
            downloadProgressLabel.HorizontalAlignment= HorizontalAlignment.Center;
            downloadProgressLabel.FontSize = 15;
            downloadProgressLabel.Margin = new Thickness(0, 0, 0, 30);


            updateCheckBoxAccessibiity();

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
        private void downloadDirectoryChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.downloadDirectory = textBox2.Text;
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
        private void mp3ConvertCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.downloadAudioAsMp3 = convertToMp3CheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
        }
        private void audioOnlyCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.downloadAudioOnly = audioOnlyCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
            updateCheckBoxAccessibiity();
        }
        private void setDateCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.setModifiedDate = setDateModifiedToCurrentCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
        }
        private void browseButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Choose download folder";
            if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = fbd.SelectedPath;
            }
        }
        private bool isPlayList(string url)
        {
            return url.Contains("&list");
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
            if(audioOnlyCheckBox.IsChecked.Value)
            {
                convertToMp3CheckBox.IsEnabled = true;
            }
            else
            {
                convertToMp3CheckBox.IsEnabled = false;
            }
        }
        public static string GetHomePath()
        {
            if (System.Environment.OSVersion.Platform == System.PlatformID.Unix)
                return System.Environment.GetEnvironmentVariable("HOME");

            return System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        }
        private void disableDownloadChildren()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                foreach (UIElement c in ((Grid)downloadBorder.Child).Children)
                {
                    if (!c.GetType().Equals(typeof(ProgressBar)) && (string)c.GetValue(NameProperty) != downloadConfirmButton.Name)
                    {
                        c.IsEnabled = false;
                    }
                }
                downloadInProgress = true;
                downloadConfirmButton.Content = "Cancel";
            }));
        }
        private void enableDownloadChildren()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                foreach (UIElement c in ((Grid)downloadBorder.Child).Children)
                {
                    if (!c.GetType().Equals(typeof(ProgressBar)) && (string)c.GetValue(NameProperty) != downloadConfirmButton.Name)
                    {
                        c.IsEnabled = true;
                    }
                }
                downloadInProgress = false;
                downloadConfirmButton.Content = "Download";
                downloadProgressLabel.Content = "";
                updateCheckBoxAccessibiity();
            }));
        }
        int currentItem = 0, maxItems = 0;
        private void downloadVideo(string url, Process p)
        {
            disableDownloadChildren();
            Properties.Settings.Default.Save();
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)+"\\Resources\\yt-dlp.exe";
            bool audioOnly = Properties.Settings.Default.downloadAudioOnly;
            bool openDirectory = Properties.Settings.Default.openDirectoryAfterDownload;
            bool convertToMp3 = Properties.Settings.Default.downloadAudioAsMp3;
            bool convertToMp4 = Properties.Settings.Default.convertToMp4;
            bool isPlaylist = isPlayList(url);
            info.Arguments = "";
            if (audioOnly)
            {
                info.Arguments += "-f \"bestaudio\" -x";
                if (convertToMp3)
                    info.Arguments += " --audio-format mp3";
            }
            else if (convertToMp4)
                info.Arguments += " -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"";
            if (Properties.Settings.Default.setModifiedDate)
                info.Arguments += " --no-mtime";
            info.Arguments += " "+url;
            info.WorkingDirectory = Properties.Settings.Default.downloadDirectory;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            p.StartInfo = info;
            string messageString, alternativeString;
            string audioString = "Downloading audio";
            string videoString = "Downloading video";
            if (audioOnly)
            {
                messageString = audioString;
                alternativeString = audioString;
            }
            else
            {
                messageString = videoString;
                alternativeString = audioString;
            }
            p.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(e.Data);
                (messageString,audioString) = processDownloadProgressString(e.Data, audioOnly, messageString, audioString, (audioOnly && convertToMp3) || (!audioOnly && convertToMp4), isPlaylist);
            });
            p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(e.Data);
                (messageString,audioString) = processDownloadProgressString(e.Data, audioOnly, messageString, audioString,(audioOnly&&convertToMp3)||(!audioOnly&&convertToMp4), isPlaylist);
            });
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            while (!p.HasExited) { }
            enableDownloadChildren();
            p.Dispose();
            if (openDirectory)
                Process.Start(info.WorkingDirectory);
        }
        private (string,string) processDownloadProgressString(string str, bool audioOnly, string startingString, string alternativeString, bool needsConversion, bool isPlaylist)
        {
            if(str==null)
                return (startingString,alternativeString);
            double progress = downloadProgressStringToDouble(str);
            if (progress == -1)
            {
                if(str.Contains("[download] Downloading item "))
                {
                    int ofIndex = str.IndexOf(" of ");
                    maxItems = int.Parse(str.Substring(ofIndex + 4));
                    int itemIndex = str.IndexOf("item");
                    currentItem = int.Parse(str.Substring(itemIndex+4,str.Length - itemIndex-4-(str.Length-ofIndex)));
                }
                return (startingString,alternativeString);
            }
            this.Dispatcher.Invoke((Action)(() =>
            {
                bool isfucked = str.Contains("frag");
                if (progress < downloadProgressBar.Value && !isfucked)
                    (startingString, alternativeString) = (alternativeString, startingString);
                downloadProgressBar.Value = progress;
                if (!isPlaylist && currentItem==maxItems)
                {
                    if (progress == 100)
                        downloadProgressBar.Foreground = System.Windows.Media.Brushes.Green;
                    else
                    {
                        downloadProgressBar.Foreground = mainBrush;
                    }
                }
                if (progress == 100 && startingString == "Downloading audio" && needsConversion && currentItem == maxItems)
                    startingString = "Converting";
                if (!isfucked)
                {
                    downloadProgressLabel.Content = startingString;
                    if (isPlaylist)
                    {
                        if (currentItem > 0 && maxItems > 0)
                            downloadProgressLabel.Content += string.Format(" (item {0} of {1})", currentItem, maxItems);
                    }
                }
                else
                {
                    downloadProgressLabel.Content = "Video is fragmented, no details can be obtained";
                }
            }));
            return (startingString,alternativeString);
        }
        private double downloadProgressStringToDouble(string str)
        {
            if (str == null)
                return -1;
            if (str == "")
                return -1;
            if (str.IndexOf("download") < 0|| str.IndexOf("% of") < 0)
                return -1;
            string progress = str.Substring(11, 5).Trim();
            if (double.TryParse(progress,out _))
                return double.Parse(progress);
            return -1;
        }


        public static string GetDownloadFolderPath()
        {
            if (Properties.Settings.Default.downloadDirectory != null && Properties.Settings.Default.downloadDirectory != "")
                return Properties.Settings.Default.downloadDirectory;
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
        private void copyLinkFromClipboard()
        {
            if (isLinkValid(Clipboard.GetText()))
                textBox1.Text = Clipboard.GetText();
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
        System.Threading.Thread downloadThread;
        Process downloadProcess = null;
        private void downloadConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("Invalid directory.");
                return;
            }
            if(downloadInProgress)
            {
                downloadThread.Abort();
                if (downloadProcess != null)
                {
                    downloadProcess.Kill();
                    downloadProcess.Dispose();
                    downloadProcess.Close();
                    downloadProcess = null;
                }
                foreach(Process proc in (from p in Process.GetProcesses()  where p.ProcessName == "yt-dlp" select p))
                {
                    proc.Kill();
                }
                enableDownloadChildren();
                return;
            }
            downloadProgressBar.Value = 0;
            downloadProcess = new Process();
            string url = textBox1.Text;
            downloadThread = new System.Threading.Thread(()=>downloadVideo(url,downloadProcess));
            downloadThread.Start();
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
        private string trimListPart(string list)
        {
            if(isPlayList(list))
            {
                return list.Substring(0, list.IndexOf("&list"));
            }
            return list;
        }
        private string downloadThumbnail(string url)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/yt-dlp.exe");
                info.Arguments = "--skip-download --write-thumbnail --output tmpimage --convert-thumbnails png " + trimListPart(url);
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.WorkingDirectory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/");
                p.StartInfo = info;
                p.Start();
                p.WaitForExit();
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/tmpimage.png");
            }
        }
        private string getVideoName(string url)
        {
            using (Process p = new Process())
            {
                ProcessStartInfo info = new ProcessStartInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/yt-dlp.exe"), "--skip-download --print title " + trimListPart(url));
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
                return isPlayList(url)?accumulated.Trim()+" (PLAYLIST)":accumulated.Trim();
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
            System.Windows.Application.Current.Shutdown();
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
                    cleanupFiles();
                    string path = downloadThumbnail(url);
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        if (System.IO.File.Exists(path))
                        {
                            videoThumbnail.Source = uriToSource(path);
                        }
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
        private void cleanupFiles()
        {
            DirectoryInfo di = new DirectoryInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Resources/"));
            foreach (FileInfo fileInfo in di.GetFiles())
            {
                if (fileInfo.Name.ToLower().StartsWith("tmp"))
                    fileInfo.Delete();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cleanupFiles();
        }

        private void windowActivated(object sender, EventArgs e)
        {
            if (activeScreen == 2 && !isLinkValid(textBox1.Text))
                copyLinkFromClipboard();
        }
    }
}
