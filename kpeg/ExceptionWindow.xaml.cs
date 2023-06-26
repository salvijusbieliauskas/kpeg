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
using System.Windows.Shapes;

namespace kpeg
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow(string message)
        {
            InitializeComponent();
            this.Owner = MainWindow.GetInstance();
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ExceptionMessageLabel.Text = message;
            this.Show();
        }

        public ExceptionWindow(Exception e)
        {
            InitializeComponent();
            this.ExceptionMessageLabel.Text = e.Message;
            System.Diagnostics.Debug.WriteLine("An exception was thrown: " + e);
            this.Show();
        }
        public ExceptionWindow(string message,Exception e)
        {
            InitializeComponent();
            this.ExceptionMessageLabel.Text = message;
            System.Diagnostics.Debug.WriteLine("An exception was thrown: " + e);
            this.Show();
        }

        public void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
