using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using aTunesSync.File.Android;
using aTunesSync.File.Windows;
using aTunesSync.ViewModel;

namespace aTunesSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel m_mainViewModel = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = m_mainViewModel;

            LoadSettings();
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            Test();
            SaveSettings();
        }

        public void Test()
        {
            var androidFiles = GetAndroidFiles(m_mainViewModel.AndroidDeviceName.Value);
            var windowsFiles = GetWindowsFiles(m_mainViewModel.WindowsRootDirectory.Value);
        }

        private SortedSet<FileBase> GetAndroidFiles(string deviceName)
        {
            var mng = new AndroidFileManager();
            var result = mng.GetMusicFiles(deviceName);
            return result;
        }

        private SortedSet<FileBase> GetWindowsFiles(string root)
        {
            var mng = new WindowsFileManager();
            var result = mng.GetMusicFiles(root);
            return result;
        }

        #region User Settings
        public void SaveSettings()
        {
            var path = GetSettingsFilePath();

            var settings = new UserSettings()
            {
                AndroidDeviceName = m_mainViewModel.AndroidDeviceName.Value,
                WindowsRootDirectory = m_mainViewModel.WindowsRootDirectory.Value,
                iTunesLibraryFile = m_mainViewModel.iTunesLibraryPath.Value,
            };

            settings.Save(path);
        }

        public void LoadSettings()
        {
            var path = GetSettingsFilePath();
            if (!System.IO.File.Exists(path))
                return;

            var settings = new UserSettings();
            settings.Load(path);

            m_mainViewModel.AndroidDeviceName.Value = settings.AndroidDeviceName;
            m_mainViewModel.WindowsRootDirectory.Value = settings.WindowsRootDirectory;
            m_mainViewModel.iTunesLibraryPath.Value = settings.iTunesLibraryFile;
        }

        private static readonly string SETTING_JSON_NAME = "settings.json";
        private string GetSettingsFilePath()
        {
            var asm = Assembly.GetEntryAssembly();
            var path = asm.Location;
            var dir = System.IO.Path.GetDirectoryName(path);
            var result = System.IO.Path.Combine(dir, SETTING_JSON_NAME);

            return result;
        }
        #endregion
    }
}
