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
using kpeg.Downloading.ProcessContainers;
using kpeg.Conversion.UserControls;

namespace kpeg.Downloading
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
        private TextBlock videoTitleBlock = new TextBlock();
        public System.Windows.Controls.Image videoThumbnail = new System.Windows.Controls.Image();
        private Button downloadConfirmButton = new Button();

        public ClipSelector clipSelector = new ClipSelector();


        private System.Threading.Thread downloadThread;
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
            downloadGrid.Children.Add(clipSelector);

            openDirectoryCheckBox.IsChecked = (bool)SettingsManager.Get("openDirectoryAfterDownload");
            openDirectoryCheckBox.FontSize = 16;
            openDirectoryCheckBox.Foreground = System.Windows.Media.Brushes.White;
            openDirectoryCheckBox.VerticalAlignment = VerticalAlignment.Top;
            openDirectoryCheckBox.Margin = new Thickness(10, 10, 0, 0);
            openDirectoryCheckBox.Content = "Open directory after download";
            openDirectoryCheckBox.Background = MainWindow.GetInstance().mainBrush;
            openDirectoryCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            openDirectoryCheckBox.Checked += openDirectoryCheckBoxChanged;
            openDirectoryCheckBox.Unchecked += openDirectoryCheckBoxChanged;

            openConverterCheckBox.IsChecked = (bool)SettingsManager.Get("openConverterAfterDownload");
            openConverterCheckBox.FontSize = 16;
            openConverterCheckBox.Foreground = System.Windows.Media.Brushes.White;
            openConverterCheckBox.VerticalAlignment = VerticalAlignment.Top;
            openConverterCheckBox.Margin = new Thickness(10, 30, 0, 0);
            openConverterCheckBox.Content = "Open converter after download";
            openConverterCheckBox.Background = MainWindow.GetInstance().mainBrush;
            openConverterCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            openConverterCheckBox.Checked += openConverterCheckBoxChanged;
            openConverterCheckBox.Unchecked += openConverterCheckBoxChanged;

            convertToMp4CheckBox.IsChecked = (bool)SettingsManager.Get("convertToMp4");
            convertToMp4CheckBox.FontSize = 16;
            convertToMp4CheckBox.Foreground = System.Windows.Media.Brushes.White;
            convertToMp4CheckBox.VerticalAlignment = VerticalAlignment.Top;
            convertToMp4CheckBox.Margin = new Thickness(10, 50, 0, 0);
            convertToMp4CheckBox.Content = "Convert to mp4";
            convertToMp4CheckBox.Background = MainWindow.GetInstance().mainBrush;
            convertToMp4CheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            convertToMp4CheckBox.Checked += mp4ConvertCheckBoxChanged;
            convertToMp4CheckBox.Unchecked += mp4ConvertCheckBoxChanged;

            audioOnlyCheckBox.IsChecked = (bool)SettingsManager.Get("downloadAudioOnly");
            audioOnlyCheckBox.FontSize = 16;
            audioOnlyCheckBox.Foreground = System.Windows.Media.Brushes.White;
            audioOnlyCheckBox.VerticalAlignment = VerticalAlignment.Top;
            audioOnlyCheckBox.Margin = new Thickness(10, 70, 0, 0);
            audioOnlyCheckBox.Content = "Download audio only";
            audioOnlyCheckBox.Background = MainWindow.GetInstance().mainBrush;
            audioOnlyCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            audioOnlyCheckBox.Checked += audioOnlyCheckBoxChanged;
            audioOnlyCheckBox.Unchecked += audioOnlyCheckBoxChanged;

            convertToMp3CheckBox.IsChecked = (bool)SettingsManager.Get("downloadAudioAsMp3");
            convertToMp3CheckBox.FontSize = 16;
            convertToMp3CheckBox.Foreground = System.Windows.Media.Brushes.White;
            convertToMp3CheckBox.VerticalAlignment = VerticalAlignment.Top;
            convertToMp3CheckBox.Margin = new Thickness(10, 90, 0, 0);
            convertToMp3CheckBox.Content = "Convert audio to wav (slightly reduces quality)";
            convertToMp3CheckBox.Background = MainWindow.GetInstance().mainBrush;
            convertToMp3CheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            convertToMp3CheckBox.Checked += mp3ConvertCheckBoxChanged;
            convertToMp3CheckBox.Unchecked += mp3ConvertCheckBoxChanged;

            setDateModifiedToCurrentCheckBox.IsChecked = (bool)SettingsManager.Get("setModifiedDate");
            setDateModifiedToCurrentCheckBox.FontSize = 16;
            setDateModifiedToCurrentCheckBox.Foreground = System.Windows.Media.Brushes.White;
            setDateModifiedToCurrentCheckBox.VerticalAlignment = VerticalAlignment.Top;
            setDateModifiedToCurrentCheckBox.Margin = new Thickness(700, 10, 0, 0);
            setDateModifiedToCurrentCheckBox.Content = "Set modified date to current";
            setDateModifiedToCurrentCheckBox.Background = MainWindow.GetInstance().mainBrush;
            setDateModifiedToCurrentCheckBox.BorderBrush = MainWindow.GetInstance().mainBrush;
            setDateModifiedToCurrentCheckBox.Checked += setDateCheckBoxChanged;
            setDateModifiedToCurrentCheckBox.Unchecked += setDateCheckBoxChanged;

            //yea
            setDateModifiedToCurrentCheckBox.IsChecked = true;
            setDateModifiedToCurrentCheckBox.IsEnabled = false;
            setDateModifiedToCurrentCheckBox.Visibility = Visibility.Hidden;
            //

            //clipVideoCheckBox.IsChecked = (bool)SettingsManager.Get("downloadClip");
            clipSelector.ClipCheckBox.IsChecked = (bool)SettingsManager.Get("downloadClip");
            clipSelector.ClipCheckBox.Checked += ClipCheckBox_Checked;
            clipSelector.ClipCheckBox.Unchecked += ClipCheckBox_Checked;

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
            videoThumbnail.Height = 235;
            videoThumbnail.Margin = new Thickness(0, 105, 0, 0);
            videoThumbnail.IsHitTestVisible = false;

            downloadGrid.Children.Add(downloadProgressBar);
            downloadProgressBar.Height = 30;
            downloadProgressBar.Value = 0;
            downloadProgressBar.VerticalAlignment = VerticalAlignment.Bottom;
            downloadProgressBar.Foreground = MainWindow.GetInstance().mainBrush;
            downloadProgressBar.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#181818");
            downloadProgressBar.BorderBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFromString("#181818");

            downloadGrid.Children.Add(downloadProgressLabel);
            downloadProgressLabel.VerticalAlignment = VerticalAlignment.Bottom;
            downloadProgressLabel.HorizontalAlignment = HorizontalAlignment.Center;
            downloadProgressLabel.FontSize = 15;
            downloadProgressLabel.Margin = new Thickness(0, 0, 0, 30);

            clipSelector.VerticalAlignment = VerticalAlignment.Bottom;
            clipSelector.Margin = new Thickness(0, 0, 0, 45);
            DisableClipSelector();

            textBox_TextChanged(null, null);
            textBox2_TextChanged(null, null);
        }

        private void ClipCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("downloadClip",clipSelector.ClipCheckBox.IsChecked);
        }

        public Border GetBorder()
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

        public ProgressBar GetProgressBar()
        {
            return downloadProgressBar;
        }
        public Label GetProgressBarLabel()
        {
            return downloadProgressLabel;
        }
        private void downloadDirectoryChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("downloadDirectory", textBox2.Text);
        }
        private void openDirectoryCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("openDirectoryAfterDownload", openDirectoryCheckBox.IsChecked.Value);
        }
        private void openConverterCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("openConverterAfterDownload", openConverterCheckBox.IsChecked.Value);
            updateCheckBoxAccessibiity();
        }
        private void mp4ConvertCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("convertToMp4", convertToMp4CheckBox.IsChecked.Value);
        }
        private void mp3ConvertCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("downloadAudioAsMp3", convertToMp3CheckBox.IsChecked.Value);
        }
        private void audioOnlyCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("downloadAudioOnly", audioOnlyCheckBox.IsChecked.Value);
            updateCheckBoxAccessibiity();
        }
        private void setDateCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set("setModifiedDate", setDateModifiedToCurrentCheckBox.IsChecked.Value);
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
        public void textBox_TextChanged(object sender, TextChangedEventArgs e)
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
            downloadConfirmButton.IsEnabled = true;

            Task.Run(() => ThumbnailDownloader.GetInstance().UpdateThumbnail(url));
            Task.Run(() => VideoNameDownloader.GetInstance().UpdateVideoName(url));
            if (!Utils.isPlayList(url))
                Task.Run(() => VideoMetadataDownloader.GetInstance().UpdateVideoMetadata(url));

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

            string url = textBox1.Text;
            Task.Run(() => VideoDownloader.GetInstance().DownloadVideo(url));
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
        }
        public void DisableDownloadChildren()
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
                downloadConfirmButton.Content = "Downloading...";
                downloadConfirmButton.IsEnabled = false;
            }));
        }
        public void EnableDownloadChildren()
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
                downloadConfirmButton.Content = "Download";
                downloadConfirmButton.IsEnabled = true;
                downloadProgressLabel.Content = "";
                updateCheckBoxAccessibiity();
            }));
        }

        public TextBox GetStartMinBox()
        {
            return clipSelector.FromMinBox;
        }
        public TextBox GetEndMinBox()
        {
            return clipSelector.ToMinBox;
        }
        public TextBox GetStartSecBox()
        {
            return clipSelector.FromSecBox;
        }
        public TextBox GetEndSecBox()
        {
            return clipSelector.ToSecBox;
        }
        public TextBox GetStartHourBox()
        {
            return clipSelector.FromHourBox;
        }
        public TextBox GetEndHourBox()
        {
            return clipSelector.ToHourBox;
        }
        public void DisableClipSelector()
        {
            clipSelector.ClipCheckBox.IsChecked = false;
            clipSelector.ClipCheckBox.IsEnabled = false;
        }
        public void EnableClipSelector()
        {
            clipSelector.ClipCheckBox.IsChecked = (bool)SettingsManager.Get("downloadClip");
            clipSelector.ClipCheckBox.IsEnabled = true;
        }
    }
}
