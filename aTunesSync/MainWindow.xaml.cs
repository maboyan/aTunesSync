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

namespace aTunesSync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
            AddLog("Start Check");
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

                    AddLog("Get Android File List");
                    var androidFiles = await GetAndroidFilesAsync(device);
                    AddLog("Get Windows File List");
                    var windowsFiles = await GetWindowsFilesAsync(m_mainViewModel.WindowsRootDirectory.Value);

                    var sync = new FileSync();
                    AddLog("Create Sync Content");
                    m_fileSync = await sync.CheckAsync(device, androidFiles, windowsFiles);
                    m_mainViewModel.SyncContentList.Clear();
                    foreach (var deleteItem in m_fileSync.AndroidOnlySet)
                    {
                        m_mainViewModel.SyncContentList.Add(new SyncContent()
                        {
                            Category = "Delete",
                            Name = deleteItem.Name,
                            Path = deleteItem.FullPath
                        });
                    }
                    foreach (var addItem in m_fileSync.WindowsOnlySet)
                    {
                        m_mainViewModel.SyncContentList.Add(new SyncContent()
                        {
                            Category = "Copy",
                            Name = addItem.Name,
                            Path = addItem.FullPath
                        });
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
            AddLog("End Check");
        }

        private async Task<SortedSet<FileBase>> GetAndroidFilesAsync(AndroidDevice device)
        {
            SortedSet<FileBase> result = null;

            await Task.Run(() =>
            {
                device.Initialize();
                device.GetMusicFilesProgressEvent += (now, num) =>
                {
                    m_mainViewModel.ProgressBarValue.Value = now * 100 / num;
                    m_mainViewModel.ProgressBarText.Value = $"{now} / {num}";
                };
                result = device.GetMusicFiles();
            });

            return result;
        }

        private async Task<SortedSet<FileBase>> GetWindowsFilesAsync(string root)
        {
            SortedSet<FileBase> result = null;

            await Task.Run(() =>
            {
                var mng = new WindowsFileManager();
                mng.GetMusicFilesProgressEvent += (now, num) =>
                {
                    m_mainViewModel.ProgressBarValue.Value = now * 100 / num;
                    m_mainViewModel.ProgressBarText.Value = $"{now} / {num}";
                };
                result = mng.GetMusicFiles(root);
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
                    await sync.SyncAsync(device, m_fileSync, m_cancelSource.Token);
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
                SaveSettings();
            }
        }
        #endregion

        #region iTunes Library Button
        private void iTunesLibraryButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.DefaultFileName = "iTunes Music Library.xml";
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
            m_mainViewModel.CheckButtonEnable.Value = enable;
            m_mainViewModel.SyncButtonEnable.Value = enable;
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
