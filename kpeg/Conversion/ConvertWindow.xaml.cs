using kpeg.Conversion.ProcessContainers;
using System.IO;
using System.Windows.Controls;

namespace kpeg.Conversion
{
    public partial class ConvertWindow
    {
        private static ConvertWindow convertWindowInstance;
        private readonly StackPanel gifConversionPanel = (StackPanel)new GIFConversionPanel().Content;
        private readonly StackPanel imageConversionPanel = (StackPanel)new ImageConversionPanel().Content;
        private readonly StackPanel videoConversionPanel = (StackPanel)new VideoConversionPanel().Content;

        private ConvertWindow()
        {
            //MainWindow.GetInstance().RegisterName(gifConversionPanel.Name, gifConversionPanel);
            //MainWindow.GetInstance().RegisterName(videoConversionPanel.Name, videoConversionPanel);
            //MainWindow.GetInstance().RegisterName(imageConversionPanel.Name, imageConversionPanel);
            InitializeComponent();
            //Opacity = 0.0;
            //IsEnabled = false;
            //IsHitTestVisible = false;

            gifConversionPanel.IsHitTestVisible = false;
            videoConversionPanel.IsHitTestVisible = false;
            imageConversionPanel.IsHitTestVisible = false;
            gifConversionPanel.IsEnabled = false;
            videoConversionPanel.IsEnabled = false;
            imageConversionPanel.IsEnabled = false;
            gifConversionPanel.Opacity = 0;
            videoConversionPanel.Opacity = 0;
            imageConversionPanel.Opacity = 0;

            ((GIFConversionPanel)gifConversionPanel.Parent).Content = null;
            ((VideoConversionPanel)videoConversionPanel.Parent).Content = null;
            ((ImageConversionPanel)imageConversionPanel.Parent).Content = null;


            OutputGrid.Children.Add(gifConversionPanel);
            OutputGrid.Children.Add(videoConversionPanel);
            OutputGrid.Children.Add(imageConversionPanel);


            TypeSelectionBox_SelectionChanged(null, null);
        }

        public static ConvertWindow GetInstance()
        {
            if (convertWindowInstance == null)
                convertWindowInstance = new ConvertWindow();
            return convertWindowInstance;
        }

        private void TypeSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((string)((ComboBoxItem)TypeBox.SelectedValue).Content == "Video")
            {
                SetActivePanel(videoConversionPanel);
                return;
            }

            if ((string)((ComboBoxItem)TypeBox.SelectedValue).Content == "GIF")
            {
                SetActivePanel(gifConversionPanel);
                return;
            }

            if ((string)((ComboBoxItem)TypeBox.SelectedValue).Content == "Image") SetActivePanel(imageConversionPanel);
        }

        private void SetActivePanel(StackPanel stackPanel)
        {
            FadeOutCurrentPanel();
            MainWindow.GetInstance().FadeControl(0.0, 1.0, 0.5, stackPanel);
            stackPanel.IsEnabled = true;
            stackPanel.IsHitTestVisible = true;
        }

        private StackPanel GetActivePanel()
        {
            if (OutputGrid == null)
                return null;
            for (int x = OutputGrid.Children.Count - 1; x > OutputGrid.Children.Count - 4; x--)
                if (OutputGrid.Children[x] is StackPanel && OutputGrid.Children[x].IsEnabled)
                    return (StackPanel)OutputGrid.Children[x];
            return null;
        }

        private void FadeOutCurrentPanel()
        {
            if (GetActivePanel() == null)
                return;
            MainWindow.GetInstance().FadeControl(1.0, 0.0, 0.5, GetActivePanel());
            GetActivePanel().IsHitTestVisible = false;
            GetActivePanel().IsEnabled = false;
        }

        private void InputFileTextChanged(object sender, TextChangedEventArgs e)
        {
            if (File.Exists(InputFileTextBox.Text))
                FFProbeContainer.GetInstance().GetFileInfoJson($"-print format json -show_format -show_streams \"{InputFileTextBox.Text}\" > FFProbeResult.json");
        }
    }
}