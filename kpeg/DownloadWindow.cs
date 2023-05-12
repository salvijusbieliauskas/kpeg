using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.IO;
using System.Security.Policy;
using kpeg.ProcessContainers;

namespace kpeg
{
    class DownloadWindow
    {
        private static DownloadWindow downloadWindowInstance = null;
        private Border downloadBorder = new Border();
        private TextBox textBox1 = new TextBox();
        private TextBox textBox2 = new TextBox();
        private Label downloadProgressLabel = new Label();
        private ProgressBar downloadProgressBar = new ProgressBar();

        private CheckBox openDirectoryCheckBox = new CheckBox();
        private CheckBox openConverterCheckBox = new CheckBox();
        private CheckBox convertToMp4CheckBox = new CheckBox();
        private CheckBox audioOnlyCheckBox = new CheckBox();
        private CheckBox convertToMp3CheckBox = new CheckBox();
        private CheckBox setDateModifiedToCurrentCheckBox = new CheckBox();
        private CheckBox clipVideoCheckBox = new CheckBox();
        private TextBlock videoTitleBlock = new TextBlock();
        public System.Windows.Controls.Image videoThumbnail = new System.Windows.Controls.Image();
        private Button downloadConfirmButton = new Button();
        private bool downloadInProgress = false;

        private StackPanel clipVideoSpanPanel = new StackPanel();
        private TextBox startMinClipBox = new TextBox();
        private TextBox endMinClipBox = new TextBox();
        private TextBox startSecClipBox = new TextBox();
        private TextBox endSecClipBox = new TextBox();
        private TextBox startHourClipBox = new TextBox();
        private TextBox endHourClipBox = new TextBox();

        private System.Threading.Thread downloadThread;
        private Process downloadProcess = null;
        public static DownloadWindow GetInstance()
        {
            if (downloadWindowInstance == null)
                downloadWindowInstance = new DownloadWindow();
            return downloadWindowInstance;
        }
        private DownloadWindow()
        {
            downloadBorder.Name = "downloadBorder";
            downloadBorder.Opacity = 0.0;
            MainWindow.GetInstance().RegisterName(downloadBorder.Name, downloadBorder);
            MainWindow.GetInstance().mainGrid.Children.Add(downloadBorder);
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
            downloadGrid.Children.Add(clipVideoCheckBox);
            downloadGrid.Children.Add(clipVideoSpanPanel);

            openDirectoryCheckBox.IsChecked = Properties.Settings.Default.openDirectoryAfterDownload;
            openDirectoryCheckBox.FontSize = 16;
            openDirectoryCheckBox.Foreground = System.Windows.Media.Brushes.White;
            openDirectoryCheckBox.VerticalAlignment = VerticalAlignment.Top;
            openDirectoryCheckBox.Margin = new Thickness(10, 10, 0, 0);
            openDirectoryCheckBox.Content = "Open directory after download";
            openDirectoryCheckBox.Background = MainWindow.GetInstance().mainBrush;
            openDirectoryCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            openDirectoryCheckBox.Checked += openDirectoryCheckBoxChanged;
            openDirectoryCheckBox.Unchecked += openDirectoryCheckBoxChanged;

            openConverterCheckBox.IsChecked = Properties.Settings.Default.openConverterAfterDownload;
            openConverterCheckBox.FontSize = 16;
            openConverterCheckBox.Foreground = System.Windows.Media.Brushes.White;
            openConverterCheckBox.VerticalAlignment = VerticalAlignment.Top;
            openConverterCheckBox.Margin = new Thickness(10, 30, 0, 0);
            openConverterCheckBox.Content = "Open converter after download";
            openConverterCheckBox.Background = MainWindow.GetInstance().mainBrush;
            openConverterCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            openConverterCheckBox.Checked += openConverterCheckBoxChanged;
            openConverterCheckBox.Unchecked += openConverterCheckBoxChanged;

            convertToMp4CheckBox.IsChecked = Properties.Settings.Default.convertToMp4;
            convertToMp4CheckBox.FontSize = 16;
            convertToMp4CheckBox.Foreground = System.Windows.Media.Brushes.White;
            convertToMp4CheckBox.VerticalAlignment = VerticalAlignment.Top;
            convertToMp4CheckBox.Margin = new Thickness(10, 50, 0, 0);
            convertToMp4CheckBox.Content = "Convert to mp4";
            convertToMp4CheckBox.Background = MainWindow.GetInstance().mainBrush;
            convertToMp4CheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            convertToMp4CheckBox.Checked += mp4ConvertCheckBoxChanged;
            convertToMp4CheckBox.Unchecked += mp4ConvertCheckBoxChanged;

            audioOnlyCheckBox.IsChecked = Properties.Settings.Default.downloadAudioOnly;
            audioOnlyCheckBox.FontSize = 16;
            audioOnlyCheckBox.Foreground = System.Windows.Media.Brushes.White;
            audioOnlyCheckBox.VerticalAlignment = VerticalAlignment.Top;
            audioOnlyCheckBox.Margin = new Thickness(10, 70, 0, 0);
            audioOnlyCheckBox.Content = "Download audio only";
            audioOnlyCheckBox.Background = MainWindow.GetInstance().mainBrush;
            audioOnlyCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            audioOnlyCheckBox.Checked += audioOnlyCheckBoxChanged;
            audioOnlyCheckBox.Unchecked += audioOnlyCheckBoxChanged;

            convertToMp3CheckBox.IsChecked = Properties.Settings.Default.downloadAudioAsMp3;
            convertToMp3CheckBox.FontSize = 16;
            convertToMp3CheckBox.Foreground = System.Windows.Media.Brushes.White;
            convertToMp3CheckBox.VerticalAlignment = VerticalAlignment.Top;
            convertToMp3CheckBox.Margin = new Thickness(10, 90, 0, 0);
            convertToMp3CheckBox.Content = "Convert audio to wav (slightly reduces quality)";
            convertToMp3CheckBox.Background = MainWindow.GetInstance().mainBrush;
            convertToMp3CheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            convertToMp3CheckBox.Checked += mp3ConvertCheckBoxChanged;
            convertToMp3CheckBox.Unchecked += mp3ConvertCheckBoxChanged;

            setDateModifiedToCurrentCheckBox.IsChecked = Properties.Settings.Default.setModifiedDate;
            setDateModifiedToCurrentCheckBox.FontSize = 16;
            setDateModifiedToCurrentCheckBox.Foreground = System.Windows.Media.Brushes.White;
            setDateModifiedToCurrentCheckBox.VerticalAlignment = VerticalAlignment.Top;
            setDateModifiedToCurrentCheckBox.Margin = new Thickness(700, 10, 0, 0);
            setDateModifiedToCurrentCheckBox.Content = "Set modified date to current";
            setDateModifiedToCurrentCheckBox.Background = MainWindow.GetInstance().mainBrush;
            setDateModifiedToCurrentCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            setDateModifiedToCurrentCheckBox.Checked += setDateCheckBoxChanged;
            setDateModifiedToCurrentCheckBox.Unchecked += setDateCheckBoxChanged;

            clipVideoCheckBox.IsChecked = Properties.Settings.Default.downloadClip;
            clipVideoCheckBox.FontSize = 16;
            clipVideoCheckBox.Foreground = System.Windows.Media.Brushes.White;
            clipVideoCheckBox.VerticalAlignment = VerticalAlignment.Top;
            clipVideoCheckBox.Margin = new Thickness(700, 30, 0, 0);
            clipVideoCheckBox.Content = "Only download part of the video";//TODO:move temporary files to appdata/temp and give them a randomized name
            clipVideoCheckBox.Background = MainWindow.GetInstance().mainBrush;
            clipVideoCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            clipVideoCheckBox.Checked += clipVideoCheckBoxChanged;
            clipVideoCheckBox.Unchecked += clipVideoCheckBoxChanged;

            clipVideoSpanPanel.Orientation = Orientation.Horizontal;
            clipVideoSpanPanel.Margin = new Thickness(700, 50, 0, 0);
            clipVideoSpanPanel.VerticalAlignment = VerticalAlignment.Top;

            Label separator1 = new Label(), separator2 = new Label(), separator3 = new Label(), separator4 = new Label(), fromLabel = new Label(), toLabel = new Label();
            clipVideoSpanPanel.Children.Add(fromLabel);
            clipVideoSpanPanel.Children.Add(startHourClipBox);
            clipVideoSpanPanel.Children.Add(separator1);
            clipVideoSpanPanel.Children.Add(startMinClipBox);
            clipVideoSpanPanel.Children.Add(separator2);
            clipVideoSpanPanel.Children.Add(startSecClipBox);
            clipVideoSpanPanel.Children.Add(toLabel);
            clipVideoSpanPanel.Children.Add(endHourClipBox);
            clipVideoSpanPanel.Children.Add(separator3);
            clipVideoSpanPanel.Children.Add(endMinClipBox);
            clipVideoSpanPanel.Children.Add(separator4);
            clipVideoSpanPanel.Children.Add(endSecClipBox);
            fromLabel.Content = "From ";
            toLabel.Content = " To ";
            separator1.Content = ":";
            separator2.Content = ":";
            separator3.Content = ":";
            separator4.Content = ":";

            startSecClipBox.Width = 17;
            startMinClipBox.Width = 17;
            startHourClipBox.Width = 17;
            endSecClipBox.Width = 17;
            endMinClipBox.Width = 17;
            endHourClipBox.Width = 17;

            startSecClipBox.CaretBrush = MainWindow.GetInstance().mainBrush;
            startMinClipBox.CaretBrush = MainWindow.GetInstance().mainBrush;
            startHourClipBox.CaretBrush = MainWindow.GetInstance().mainBrush;
            endSecClipBox.CaretBrush = MainWindow.GetInstance().mainBrush;
            endMinClipBox.CaretBrush = MainWindow.GetInstance().mainBrush;
            endHourClipBox.CaretBrush = MainWindow.GetInstance().mainBrush;

            startSecClipBox.TextChanged += timeBoxChanged;
            startMinClipBox.TextChanged += timeBoxChanged;
            startHourClipBox.TextChanged += timeBoxChanged;
            endSecClipBox.TextChanged += timeBoxChanged;
            endMinClipBox.TextChanged += timeBoxChanged;
            endHourClipBox.TextChanged += timeBoxChanged;

            //startSecClipBox.GotFocus += timeBoxFocused;
            //startMinClipBox.TextChanged += timeBoxFocused;
            //startHourClipBox.TextChanged += timeBoxFocused;
            //endSecClipBox.TextChanged += timeBoxFocused;
            //endMinClipBox.TextChanged += timeBoxFocused;
            //endHourClipBox.TextChanged += timeBoxFocused;

            //startSecClipBox.MaxLength = 2;
            //startMinClipBox.MaxLength = 2;
            //startHourClipBox.MaxLength = 2;
            //endSecClipBox.MaxLength = 2;
            //endMinClipBox.MaxLength = 2;
            //endHourClipBox.MaxLength = 2;

            startSecClipBox.Text = "00";
            startMinClipBox.Text = "00";
            startHourClipBox.Text = "00";
            endSecClipBox.Text = "00";
            endMinClipBox.Text = "00";
            endHourClipBox.Text ="00";

            downloadGrid.Children.Add(textBox1);
            textBox1.Width = 400;
            textBox1.Height = 30;
            textBox1.TextAlignment = TextAlignment.Center;
            textBox1.TextChanged += textBox_TextChanged;
            textBox1.VerticalAlignment = VerticalAlignment.Top;
            textBox1.Margin = new Thickness(0, 10, 0, 0);
            textBox1.CaretBrush = MainWindow.GetInstance().mainBrush;

            downloadGrid.Children.Add(textBox2);
            textBox2.Width = 340;
            textBox2.Height = 30;
            textBox2.TextAlignment = TextAlignment.Center;
            textBox2.TextChanged += textBox2_TextChanged;
            textBox2.VerticalAlignment = VerticalAlignment.Top;
            textBox2.Margin = new Thickness(0, 50, 60, 0);
            textBox2.CaretBrush = MainWindow.GetInstance().mainBrush;
            textBox2.TextChanged += downloadDirectoryChanged;
            textBox2.Text = Utils.GetDownloadFolderPath();

            Button browseButton = new Button();
            downloadGrid.Children.Add(browseButton);
            browseButton.Width = 60;
            browseButton.Padding = new Thickness(0, 0, 0, 0);
            browseButton.Background = MainWindow.GetInstance().mainBrush;
            browseButton.VerticalAlignment = VerticalAlignment.Top;
            browseButton.Margin = new Thickness(350, 50, 0, 0);
            browseButton.Content = "Browse";
            browseButton.Click += browseButtonClicked;

            downloadGrid.Children.Add(downloadConfirmButton);
            downloadConfirmButton.Background = MainWindow.GetInstance().mainBrush;
            downloadConfirmButton.Margin = new Thickness(100, 0, 100, 300);
            downloadConfirmButton.Content = "Download";
            downloadConfirmButton.IsEnabled = false;
            downloadConfirmButton.Click += downloadConfirmButtonClicked;
            downloadConfirmButton.Name = "downloadConfirmButton";
            MainWindow.GetInstance().RegisterName(downloadConfirmButton.Name, downloadConfirmButton);

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
            downloadProgressBar.VerticalAlignment = VerticalAlignment.Bottom;
            downloadProgressBar.Foreground = MainWindow.GetInstance().mainBrush;

            downloadGrid.Children.Add(downloadProgressLabel);
            downloadProgressLabel.VerticalAlignment = VerticalAlignment.Bottom;
            downloadProgressLabel.HorizontalAlignment = HorizontalAlignment.Center;
            downloadProgressLabel.FontSize = 15;
            downloadProgressLabel.Margin = new Thickness(0, 0, 0, 30);

            textBox_TextChanged(null, null);
            textBox2_TextChanged(null, null);
        }
        public Border getBorder()
        {
            return this.downloadBorder;
        }

        public TextBlock GetVideoTitleBlock()
        {
            return this.videoTitleBlock;
        }
        public Image GetVideoThumbnail()
        {
            return this.videoThumbnail;
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
        private void clipVideoCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.downloadClip = clipVideoCheckBox.IsChecked.Value;
            Properties.Settings.Default.Save();
            updateCheckBoxAccessibiity();
        }
        private void browseButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Choose download folder";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = fbd.SelectedPath;
            }
        }
        private void timeBoxChanged(object sender, RoutedEventArgs e)
        {
            TextBox senderBox = (TextBox)sender;
            if(senderBox.Text.Length>2)
            {
                senderBox.Text = senderBox.Text.Substring(0, 2);
                senderBox.CaretIndex = 2;
            }
            for(int x = 0; x < senderBox.Text.Length; x++)
            {
                if(!Char.IsNumber(senderBox.Text, x))
                {
                    senderBox.Text = senderBox.Text.Remove(x,1);
                }
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
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string url = textBox1.Text.Trim();
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
            if (!Utils.isLinkValid(url))
            {
                downloadConfirmButton.IsEnabled = false;
                videoTitleBlock.Text = "";
                videoThumbnail.Source = null;
                return;
            }

            Task.Run(() => ThumbnailDownloader.GetInstance().UpdateThumbnail(url));
            Task.Run(() => VideoNameDownloader.GetInstance().UpdateVideoName(url));

        }
        public TextBox getLinkBox()
        {
            return textBox1;
        }
        public TextBox getDirectoryBox()
        {
            return textBox2;
        }
        private void downloadConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("Invalid directory.");
                return;
            }
            if (downloadInProgress)
            {
                downloadThread.Abort();
                if (downloadProcess != null)
                {
                    downloadProcess.Kill();
                    downloadProcess.Dispose();
                    downloadProcess.Close();
                    downloadProcess = null;
                }
                foreach (Process proc in (from p in Process.GetProcesses() where p.ProcessName == "yt-dlp" select p))
                {
                    proc.Kill();
                }
                enableDownloadChildren();
                return;
            }
            downloadProgressBar.Value = 0;
            downloadProcess = new Process();
            string url = textBox1.Text;
            downloadThread = new System.Threading.Thread(() => downloadVideo(url, downloadProcess));
            downloadThread.Start();
        }
        public void updateCheckBoxAccessibiity()
        {
            if (openConverterCheckBox.IsChecked.Value || audioOnlyCheckBox.IsChecked.Value)
            {
                convertToMp4CheckBox.IsEnabled = false;
            }
            else if (!openConverterCheckBox.IsChecked.Value && !audioOnlyCheckBox.IsChecked.Value)
            {
                convertToMp4CheckBox.IsEnabled = true;
            }
            convertToMp3CheckBox.IsEnabled = audioOnlyCheckBox.IsChecked.Value;
            clipVideoSpanPanel.IsEnabled = clipVideoCheckBox.IsChecked.Value;
        }
        private void disableDownloadChildren()
        {
            MainWindow.GetInstance().Dispatcher.Invoke((Action)(() =>
            {
                foreach (UIElement c in ((Grid)downloadBorder.Child).Children)
                {
                    if (!c.GetType().Equals(typeof(ProgressBar)) && (string)c.GetValue(FrameworkElement.NameProperty) != downloadConfirmButton.Name)
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
            MainWindow.GetInstance().Dispatcher.Invoke((Action)(() =>
            {
                foreach (UIElement c in ((Grid)downloadBorder.Child).Children)
                {
                    if (!c.GetType().Equals(typeof(ProgressBar)) && (string)c.GetValue(FrameworkElement.NameProperty) != downloadConfirmButton.Name)
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
            info.FileName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Resources\\yt-dlp.exe";
            bool audioOnly = Properties.Settings.Default.downloadAudioOnly;
            bool openDirectory = Properties.Settings.Default.openDirectoryAfterDownload;
            bool convertToMp3 = Properties.Settings.Default.downloadAudioAsMp3;
            bool convertToMp4 = Properties.Settings.Default.convertToMp4;
            bool isPlaylist = Utils.isPlayList(url);
            bool downloadClip = Properties.Settings.Default.downloadClip;
            info.Arguments = "";
            if (audioOnly)
            {
                info.Arguments += "-f \"bestaudio\" -x";
                if (convertToMp3)
                    info.Arguments += " --audio-format wav";
            }
            else if (convertToMp4)
                info.Arguments += " -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"";
            if (Properties.Settings.Default.setModifiedDate)
                info.Arguments += " --no-mtime";
            if (downloadClip)
            {
                MainWindow.GetInstance().Dispatcher.Invoke((Action)(() =>
                {
                    int from = int.Parse(startSecClipBox.Text) + int.Parse(startMinClipBox.Text) * 60 + int.Parse(startHourClipBox.Text) * 3600;
                    int to = int.Parse(endSecClipBox.Text) + int.Parse(endMinClipBox.Text) * 60 + int.Parse(endHourClipBox.Text) * 3600;

                    info.Arguments += string.Format(" --download-sections \"*{0}-{1}\"", from, to);
                }));
            }
            info.Arguments += " " + url;
            System.Diagnostics.Debug.WriteLine(info.Arguments);
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
                (messageString, audioString) = processDownloadProgressString(e.Data, audioOnly, messageString, audioString, (audioOnly && convertToMp3) || (!audioOnly && convertToMp4), isPlaylist);
            });
            p.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                System.Diagnostics.Debug.WriteLine(e.Data);
                (messageString, audioString) = processDownloadProgressString(e.Data, audioOnly, messageString, audioString, (audioOnly && convertToMp3) || (!audioOnly && convertToMp4), isPlaylist);
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
        private (string, string) processDownloadProgressString(string str, bool audioOnly, string startingString, string alternativeString, bool needsConversion, bool isPlaylist)
        {
            if (str == null)
                return (startingString, alternativeString);
            double progress = Utils.downloadProgressStringToDouble(str);
            if (progress == -1)
            {
                if (str.Contains("[download] Downloading item "))
                {
                    int ofIndex = str.IndexOf(" of ");
                    maxItems = int.Parse(str.Substring(ofIndex + 4));
                    int itemIndex = str.IndexOf("item");
                    currentItem = int.Parse(str.Substring(itemIndex + 4, str.Length - itemIndex - 4 - (str.Length - ofIndex)));
                }
                return (startingString, alternativeString);
            }
            MainWindow.GetInstance().Dispatcher.Invoke((Action)(() =>
            {
                bool isfucked = str.Contains("frag");
                if (progress < downloadProgressBar.Value && !isfucked)
                    (startingString, alternativeString) = (alternativeString, startingString);
                downloadProgressBar.Value = progress;
                if (!isPlaylist && currentItem == maxItems)
                {
                    if (progress == 100)
                        downloadProgressBar.Foreground = System.Windows.Media.Brushes.Green;
                    else
                    {
                        downloadProgressBar.Foreground = MainWindow.GetInstance().mainBrush;
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
                            downloadProgressLabel.Content += string.Format(" (item {0} of {1})", currentItem, maxItems);//TODO: green doesnt work, converting text does not work
                    }
                }
                else
                {
                    downloadProgressLabel.Content = "Video is fragmented, no details can be obtained";
                }
            }));
            return (startingString, alternativeString);
        }
    }
}
