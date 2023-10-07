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
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace kpeg.Downloading
{
    public partial class DownloadWindow : UserControl
    {
        private static DownloadWindow downloadWindowInstance = null;
        public static DownloadWindow GetInstance()
        {
            if (downloadWindowInstance == null)
                downloadWindowInstance = new DownloadWindow();
            return downloadWindowInstance;
        }
        private DownloadWindow()
        {
            InitializeComponent();
            //MainWindow.GetInstance().RegisterName(Name, this);
            //MainWindow.GetInstance().mainGrid.Children.Add(this);

            //IsEnabled = false;
            //IsHitTestVisible = false;
            //Opacity = 0;

            openDirectoryCheckBox.IsChecked = (bool)SettingsManager.Get(Setting.OpenDirectoryAfterDownload);


            openConverterCheckBox.IsChecked = (bool)SettingsManager.Get(Setting.OpenConverterAfterDownload);

            convertToMp4CheckBox.IsChecked = (bool)SettingsManager.Get(Setting.ConvertToMp4);

            audioOnlyCheckBox.IsChecked = (bool)SettingsManager.Get(Setting.DownloadAudioOnly);

            convertAudioCheckBox.IsChecked = (bool)SettingsManager.Get(Setting.DownloadAudioAsWav);


            clipSelector.ClipCheckBox.IsChecked = (bool)SettingsManager.Get(Setting.DownloadClip);
            clipSelector.ClipCheckBox.Checked += ClipCheckBox_Checked;
            clipSelector.ClipCheckBox.Unchecked += ClipCheckBox_Checked;


            textBox2.TextChanged += DownloadDirectoryChanged;
            textBox2.Text = Utils.GetDownloadFolderPath();


            DisableClipSelector();

            TextBox_TextChanged(null, null);
            TextBox2_TextChanged(null, null);
        }

        private void ClipCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set(Setting.DownloadClip, clipSelector.ClipCheckBox.IsChecked);
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
        private void DownloadDirectoryChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set(Setting.DownloadDirectory, textBox2.Text);
        }
        private void OpenDirectoryCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set(Setting.OpenDirectoryAfterDownload, openDirectoryCheckBox.IsChecked.Value);
        }
        private void OpenConverterCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set(Setting.OpenConverterAfterDownload, openConverterCheckBox.IsChecked.Value);
            UpdateCheckBoxAccessibiity();
        }
        private void Mp4ConvertCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set(Setting.ConvertToMp4, convertToMp4CheckBox.IsChecked.Value);
        }
        private void AudioConvertCheckboxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set(Setting.DownloadAudioAsWav, convertAudioCheckBox.IsChecked.Value);
        }
        private void AudioOnlyCheckBoxChanged(object sender, RoutedEventArgs e)
        {
            SettingsManager.Set(Setting.DownloadAudioOnly, audioOnlyCheckBox.IsChecked.Value);
            UpdateCheckBoxAccessibiity();
        }
        private void BrowseButtonClicked(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Choose download folder";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBox2.Text = fbd.SelectedPath;
            }
        }

        private void TextBox2_TextChanged(object sender, TextChangedEventArgs e)
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
        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
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
            if (!Utils.IsLinkValid(url))
            {
                downloadConfirmButton.IsEnabled = false;
                videoTitleBlock.Text = "";
                videoThumbnail.Source = null;
                return;
            }
            downloadConfirmButton.IsEnabled = true;

            Task.Run(() => ThumbnailDownloader.GetInstance().UpdateThumbnail(url));
            Task.Run(() => VideoNameDownloader.GetInstance().UpdateVideoName(url));
            if (!Utils.IsPlayList(url))
                Task.Run(() => VideoMetadataDownloader.GetInstance().UpdateVideoMetadata(url));

        }
        public TextBox GetLinkBox()
        {
            return textBox1;
        }
        public TextBox GetDirectoryBox()
        {
            return textBox2;
        }
        private void DownloadConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!System.IO.Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("Invalid directory.");
                return;
            }

            string url = textBox1.Text;
            int fromSeconds = clipSelector.GetFromSeconds();
            int toSeconds = clipSelector.GetToSeconds();
            Task.Run(() => VideoDownloader.GetInstance().DownloadVideo(url, fromSeconds, toSeconds));
        }
        public void UpdateCheckBoxAccessibiity()
        {
            if (openConverterCheckBox.IsChecked.Value || audioOnlyCheckBox.IsChecked.Value)
            {
                convertToMp4CheckBox.IsEnabled = false;
            }
            else if (!openConverterCheckBox.IsChecked.Value && !audioOnlyCheckBox.IsChecked.Value)
            {
                convertToMp4CheckBox.IsEnabled = true;
            }
            convertAudioCheckBox.IsEnabled = audioOnlyCheckBox.IsChecked.Value;
        }
        public void DisableDownloadChildren()
        {
            MainWindow.GetInstance().Dispatcher.Invoke((Action)(() =>
            {
                foreach (UIElement c in this.DownloadGrid.Children)
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
                foreach (UIElement c in this.DownloadGrid.Children)
                {
                    if (!c.GetType().Equals(typeof(ProgressBar)) && (string)c.GetValue(FrameworkElement.NameProperty) != downloadConfirmButton.Name)
                    {
                        c.IsEnabled = true;
                    }
                }
                downloadConfirmButton.Content = "Download";
                downloadConfirmButton.IsEnabled = true;
                downloadProgressLabel.Content = "";
                UpdateCheckBoxAccessibiity();
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
            clipSelector.ClipCheckBox.IsChecked = (bool)SettingsManager.Get(Setting.DownloadClip);
            clipSelector.ClipCheckBox.IsEnabled = true;
        }
    }
}
