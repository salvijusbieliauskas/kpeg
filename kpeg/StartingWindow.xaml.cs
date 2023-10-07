using kpeg.Conversion;
using kpeg.Downloading;
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
    /// Interaction logic for StartingWindow.xaml
    /// </summary>
    public partial class StartingWindow : UserControl
    {
        private static StartingWindow startingWindowInstance = null;
        public static StartingWindow GetInstance()
        {
            if (startingWindowInstance == null)
                startingWindowInstance = new StartingWindow();
            return startingWindowInstance;
        }
        private StartingWindow()
        {
            InitializeComponent();
        }
        private void DownloadClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.GetInstance().FadeInWindow(DownloadWindow.GetInstance(), true);
        }
        private void ConvertClicked(object sender, RoutedEventArgs e)
        {
            MainWindow.GetInstance().FadeInWindow(ConvertWindow.GetInstance(), true);
        }

    }
}
