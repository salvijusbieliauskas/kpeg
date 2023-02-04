using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace kpeg
{
    class ConvertWindow
    {
        private static ConvertWindow convertWindowInstance = null;
        public static ConvertWindow getInstance(MainWindow window)
        {
            if (convertWindowInstance == null)
                convertWindowInstance = new ConvertWindow(window);
            return convertWindowInstance;
        }
        private Border convertBorder;
        private Grid convertGrid;
        private ConvertWindow(MainWindow window)
        {

        }
    }
}
