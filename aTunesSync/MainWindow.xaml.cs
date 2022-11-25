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

using Microsoft.WindowsAPICodePack.Dialogs;
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

        #region Check Sync Button
        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckAsync();
            SaveSettings();
        }

        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
        }

        public  async Task CheckAsync()
        {
            var mng = new AndroidFileManager();
            using (var device = mng.SearchDevice(m_mainViewModel.AndroidDeviceName.Value))
            {
                if (device == null)
                    return;

                var androidFiles = GetAndroidFiles(device);
                var windowsFiles = GetWindowsFiles(m_mainViewModel.WindowsRootDirectory.Value);

                var sync = new FileSync();
                var content = await sync.CheckAsync(device, androidFiles, windowsFiles);
                m_mainViewModel.SyncContentList.Clear();
                foreach(var deleteItem in content.AndroidOnlySet)
                {
                    m_mainViewModel.SyncContentList.Add(new SyncContent()
                    {
                        Category = "Delete",
                        Name = deleteItem.Name,
                        Path = deleteItem.FullPath
                    });
                }
                foreach (var addItem in content.WindowsOnlySet)
                {
                    m_mainViewModel.SyncContentList.Add(new SyncContent()
                    {
                        Category = "Copy",
                        Name = addItem.Name,
                        Path = addItem.FullPath
                    });
                }
            }
        }

        private SortedSet<FileBase> GetAndroidFiles(AndroidDevice device)
        {
            device.Initialize();
            var result = device.GetMusicFiles();
            return result;
        }

        private SortedSet<FileBase> GetWindowsFiles(string root)
        {
            var mng = new WindowsFileManager();
            var result = mng.GetMusicFiles(root);
            return result;
        }
        #endregion

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

        #region Windows Music Root Button
        private void WindowsRootButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.IsFolderPicker = true;
            if (!string.IsNullOrWhiteSpace(m_mainViewModel.WindowsRootDirectory.Value) && System.IO.Directory.Exists(m_mainViewModel.WindowsRootDirectory.Value))
                dlg.InitialDirectory = m_mainViewModel.WindowsRootDirectory.Value;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                m_mainViewModel.WindowsRootDirectory.Value = dlg.FileName;
            }
        }
        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
