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
using System.Threading;

using Microsoft.WindowsAPICodePack.Dialogs;
using aTunesSync.File;
using aTunesSync.File.Android;
using aTunesSync.File.Windows;
using aTunesSync.ViewModel;
using aTunesSync.iTunes;

namespace aTunesSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly string LIBRARY_FILE_NAME = "library.json";

        private MainViewModel m_mainViewModel = new MainViewModel();
        private FileSyncContent m_fileSync = null;
        private CancellationTokenSource m_cancelSource = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = m_mainViewModel;

            LoadSettings();
        }

        #region Check Button
        private async void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckAsync();
        }

        public  async Task CheckAsync()
        {
            AddLog("Start Prepare");
            var mng = new AndroidFileManager();
            using (var device = mng.SearchDevice(m_mainViewModel.AndroidDeviceName.Value))
            {
                if (device == null)
                {
                    AddLog($"{m_mainViewModel.AndroidDeviceName.Value} Not Found");
                    return;
                }

                try
                {
                    SetAllButtonEnable(false);

                    AddLog("Create iTunes Playlist");
                    var itunes = new iTunesParser(m_mainViewModel.iTunesLibraryPath.Value);
                    var rootPlayList = await itunes.ParseAsync();
                    rootPlayList.SaveM3u(m_mainViewModel.WindowsRootDirectory.Value, m_mainViewModel.PlaylistDirectoryName.Value);

                    AddLog("Create iTunes Playlist All Musics");
                    var libraryPath = System.IO.Path.Combine(m_mainViewModel.WindowsRootDirectory.Value, LIBRARY_FILE_NAME);
                    rootPlayList.SaveAllMusics(m_mainViewModel.WindowsRootDirectory.Value, libraryPath);

                    AddLog("Get Android File List");
                    var androidFiles = await GetAndroidFilesAsync(device, LIBRARY_FILE_NAME);
                    AddLog("Get Windows File List");
                    var windowsFiles = await GetWindowsFilesAsync(m_mainViewModel.WindowsRootDirectory.Value, LIBRARY_FILE_NAME);

                    AddLog("Create Sync Content");
                    var sync = new FileSync();
                    m_fileSync = await sync.CheckAsync(device, androidFiles, windowsFiles);
                    m_mainViewModel.SyncContentList.Clear();
                    foreach (var deleteItem in m_fileSync.AndroidOnlySet.FileSet)
                    {
                        m_mainViewModel.SyncContentList.Add(new SyncContent()
                        {
                            Category = "Delete",
                            Name = deleteItem.Name,
                            Path = deleteItem.FullPath
                        });
                    }
                    foreach (var addItem in m_fileSync.WindowsOnlySet.FileSet)
                    {
                        m_mainViewModel.SyncContentList.Add(new SyncContent()
                        {
                            Category = "Copy",
                            Name = addItem.Name,
                            Path = addItem.FullPath
                        });
                    }

                    if (m_mainViewModel.IsOverwrite.Value)
                    {// 強制上書き命令がでているので何も考えずに上書き
                        foreach (var overwriteItem in m_fileSync.CommonSet.FileSet)
                        {
                            m_mainViewModel.SyncContentList.Add(new SyncContent()
                            {
                                Category = "Overwrite",
                                Name = overwriteItem.Android.Name,
                                Path = $"{overwriteItem.Android.FullPath} = {overwriteItem.Windows.FullPath}"
                            });
                        }
                    }
                    else
                    {// 強制上書き命令がでていないのでちゃんとUpdateする
                        var androidLibrary = androidFiles.Search(LIBRARY_FILE_NAME);
                        var windowsLibrary = windowsFiles.Search(LIBRARY_FILE_NAME);
                        if (androidFiles != null && windowsLibrary != null)
                        {
                            // TODO
                            // windowsとandroidの中身を比較してアップデート
                        }
                    }
                
                }
                catch (Exception e)
                {
                    AddLog(e.ToString());
                }
                finally
                {
                    SetAllButtonEnable(true);
                    ClearProgressBar();
                }

            }
            AddLog("End Prepare");
        }

        private async Task<AndroidFileSet> GetAndroidFilesAsync(AndroidDevice device, string libraryName)
        {
            AndroidFileSet result = null;

            await Task.Run(() =>
            {
                device.Initialize();
                device.GetMusicFilesProgressEvent += (now, num) =>
                {
                    m_mainViewModel.ProgressBarValue.Value = now * 100 / num;
                    m_mainViewModel.ProgressBarText.Value = $"{now} / {num}";
                };
                var set = device.GetMusicFiles();
                var library = device.GetLibraryFile(libraryName);
                if (library != null)
                    set.Add(library);
                result = new AndroidFileSet(set);
            });

            return result;
        }

        private async Task<WindowsFileSet> GetWindowsFilesAsync(string root, string libraryName)
        {
            WindowsFileSet result = null;

            await Task.Run(() =>
            {
                var mng = new WindowsFileManager();
                mng.GetMusicFilesProgressEvent += (now, num) =>
                {
                    m_mainViewModel.ProgressBarValue.Value = now * 100 / num;
                    m_mainViewModel.ProgressBarText.Value = $"{now} / {num}";
                };
                var set = mng.GetMusicFiles(root);
                var library = mng.GetLibraryFile(root, libraryName);
                if (library != null)
                    set.Add(library);
                result = new WindowsFileSet(set);
            });
            
            return result;
        }
        #endregion

        #region Sync Bottun
        private async void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            // キャンセルボタンになっている模様
            // ちゃんと判定したほうがいいけど面倒なのでこの程度の判定
            if (m_cancelSource != null)
            {
                AddLog("Request Cancel Sync");
                m_cancelSource.Cancel();
                return;
            }

            await SyncAsync();
        }

        private async Task SyncAsync()
        {
            AddLog("Start Sync");
            if (m_fileSync == null)
            {
                AddLog("No Update");
                return;
            }


            var mng = new AndroidFileManager();
            using (var device = mng.SearchDevice(m_mainViewModel.AndroidDeviceName.Value))
            {
                if (device == null)
                {
                    AddLog($"{m_mainViewModel.AndroidDeviceName.Value} Not Found");
                    return;
                }

                m_cancelSource = new CancellationTokenSource();

                var sync = new FileSync();
                // イベント登録
                sync.MessageEvent += (str) =>
                {
                    AddLog(str);
                };
                sync.SyncProgressEvent += (now, num) =>
                {
                    m_mainViewModel.ProgressBarValue.Value = now * 100 / num;
                    m_mainViewModel.ProgressBarText.Value = $"{now} / {num}";
                };

                // 同期開始
                try
                {
                    SetButtonSyncToCancel();
                    device.Initialize();
                    await sync.SyncAsync(device, m_fileSync, m_mainViewModel.IsOverwrite.Value, m_cancelSource.Token);
                }
                catch(MusicDirectoryNotFoundException)
                {
                    AddLog("Music Directory Not Found");
                }
                catch(Exception e)
                {
                    AddLog(e.ToString());
                }
                finally
                {
                    SetButtonCancelToSync();
                    ClearProgressBar();
                }
                m_cancelSource.Dispose();
                m_cancelSource = null;
            }

            AddLog("End Sync");
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
                PlaylistDirectoryName = m_mainViewModel.PlaylistDirectoryName.Value,
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
            
            if (!string.IsNullOrWhiteSpace(settings.AndroidDeviceName))
                m_mainViewModel.AndroidDeviceName.Value = settings.AndroidDeviceName;
            if (!string.IsNullOrWhiteSpace(settings.WindowsRootDirectory))
                m_mainViewModel.WindowsRootDirectory.Value = settings.WindowsRootDirectory;
            if (!string.IsNullOrWhiteSpace(settings.iTunesLibraryFile))
                m_mainViewModel.iTunesLibraryPath.Value = settings.iTunesLibraryFile;
            if (!string.IsNullOrWhiteSpace(settings.PlaylistDirectoryName))
                m_mainViewModel.PlaylistDirectoryName.Value = settings.PlaylistDirectoryName;
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
                SaveSettings();
            }
        }
        #endregion

        #region iTunes Library Button
        private void iTunesLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.DefaultFileName = "iTunes Library.xml";
            dlg.Filters.Add(new CommonFileDialogFilter("iTunesライブラリファイル(*.xml)", ".xml"));
            if (!string.IsNullOrWhiteSpace(m_mainViewModel.iTunesLibraryPath.Value) && System.IO.Directory.Exists(m_mainViewModel.iTunesLibraryPath.Value))
                dlg.InitialDirectory = m_mainViewModel.iTunesLibraryPath.Value;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                m_mainViewModel.iTunesLibraryPath.Value = dlg.FileName;
                SaveSettings();
            }
        }
        #endregion

        private void AddLog(string log, bool bAddDate=true)
        {
            if (log == null)
                return;

            var addLog = log;
            if (bAddDate)
            {
                var now = DateTime.Now.ToString("hh:mm:ss");
                addLog = $"{now} {log}";
            }
            m_mainViewModel.Log.Value += addLog + "\n";
        }

        #region WPF
        private void SetAllButtonEnable(bool enable)
        {
            m_mainViewModel.AndroidDeviceEnable.Value = enable;
            m_mainViewModel.WindowsRootEnable.Value = enable;
            m_mainViewModel.WindowsRootDialogEnable.Value = enable;
            m_mainViewModel.iTunesLibraryEnable.Value = enable;
            m_mainViewModel.iTunesLibraryDialogEnable.Value = enable;
            m_mainViewModel.PlaylistDirectoryEnable.Value = enable;
            m_mainViewModel.CheckButtonEnable.Value = enable;
            m_mainViewModel.SyncButtonEnable.Value = enable;
            m_mainViewModel.OverwriteCheckBoxEnable.Value = enable;
        }

        private void SetButtonSyncToCancel()
        {
            SetAllButtonEnable(false);
            m_mainViewModel.SyncButtonText.Value = "Cancel";
            m_mainViewModel.SyncButtonEnable.Value = true;
        }

        private void SetButtonCancelToSync()
        {
            SetAllButtonEnable(true);
            m_mainViewModel.SyncButtonText.Value = "Sync";
            m_mainViewModel.SyncButtonEnable.Value = false;
        }

        private void ClearProgressBar()
        {
            m_mainViewModel.ProgressBarText.Value = "";
            m_mainViewModel.ProgressBarValue.Value = 0;
        }
        #endregion

        private void LogTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (textbox == null)
                return;

            textbox.ScrollToEnd();
        }

        
    }
}
