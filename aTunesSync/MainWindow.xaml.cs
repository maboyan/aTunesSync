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

using aTunesSync.File;

namespace aTunesSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Test()
        {
            WpdDeviceManager mng = new WpdDeviceManager();
            using (var device = mng.SearchDevice("Pixel 7 Pro"))
            {
                if (device == null)
                    return;

                var musicDir = device.GetMusicDir();
                if (musicDir == null)
                    return;

                var files = device.GetMusicFiles(musicDir);
                foreach (var file in files)
                {
                    Console.WriteLine(file);
                }
            }
                
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Test();
        }
    }
}
