using aTunesSync.File.Windows;
using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace aTunesSync.File.Android
{
    internal class AndroidDevice
        : IDisposable
    {
        /// <summary>
        /// メッセージログイベント
        /// </summary>
        public event MessageEventHandler MessageEvent = delegate { };

        /// <summary>
        /// GetMusicFilesの進捗イベント
        /// </summary>
        public event ProgressEventHandler GetMusicFilesProgressEvent = delegate { };

        /// <summary>
        /// デバイス操作ハンドル
        /// </summary>
        public MediaDevice Device { get; private set; }

        /// <summary>
        /// 音楽格納パス
        /// </summary>
        public string MusicDirectory { get; private set; }

        public AndroidDevice(MediaDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            Device = device;
            Device.Connect();
        }

        #region Initialize
        /// <summary>
        /// 初期化
        /// </summary>
        /// <exception cref="MusicDirectoryNotFoundException"></exception>
        public void Initialize()
        {
            MusicDirectory = GetMusicDirectory();
            if (MusicDirectory == null)
                throw new MusicDirectoryNotFoundException();
        }

        private string GetMusicDirectory()
        {
            var root = Device.GetRootDirectory();
            var result = GetMusicDirectory(root.FullName, 2);
            return result;
        }

        public static readonly string MUSIC_DIRECTORY_NAME = "Music";
        private string GetMusicDirectory(string path, int remain)
        {
            if (remain <= 0)
                return null;

            var dirs = Device.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var sdir = dir.Split('\\');
                var name = sdir.Last();
                if (name == MUSIC_DIRECTORY_NAME)
                    return dir;

                var deepDir = GetMusicDirectory(dir, remain - 1);
                if (deepDir != null)
                    return deepDir;
            }

            return null;
        }
        #endregion

        #region Files
        /// <summary>
        /// GetMusicDirectoryで取得したrootからmp3, m4a, m3u8ファイルを探す
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public SortedSet<AndroidFile> GetMusicFiles()
        {
            if (string.IsNullOrWhiteSpace(MusicDirectory))
                throw new InvalidOperationException("Initialize not call");

            var root = MusicDirectory;
            var result = new SortedSet<AndroidFile>();

            var mp3List = Device.GetFileSystemEntries(root, "*.mp3", System.IO.SearchOption.AllDirectories);
            var m4aList = Device.GetFileSystemEntries(root, "*.m4a", System.IO.SearchOption.AllDirectories);
            var m3u8List = Device.GetFileSystemEntries(root, "*.m3u", System.IO.SearchOption.AllDirectories);
            var sum = mp3List.Count() + m4aList.Count() + m3u8List.Count();
            var now = 0;

            foreach (var mp3 in mp3List)
            {
                var info = Device.GetFileInfo(mp3);
                var item = new AndroidFile(info, root);
                result.Add(item);

                ++now;
                GetMusicFilesProgressEvent(now, sum);
            }

            foreach (var m4a in m4aList)
            {
                var info = Device.GetFileInfo(m4a);
                var item = new AndroidFile(info, root);
                result.Add(item);

                ++now;
                GetMusicFilesProgressEvent(now, sum);
            }

            foreach (var m3u8 in m3u8List)
            {
                var info = Device.GetFileInfo(m3u8);
                var item = new AndroidFile(info, root);
                result.Add(item);

                ++now;
                GetMusicFilesProgressEvent(now, sum);
            }

            return result;
        }

        /// <summary>
        /// TOPディレクトリに置いてあるライブラリファイルを取得する
        /// ファイルが複数ある場合の動作は不定
        /// </summary>
        /// <param name="name"></param>
        /// <returns>ファイルが存在しない場合はnull</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public AndroidFile GetLibraryFile(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(MusicDirectory))
                throw new InvalidOperationException("Initialize not call");

            var root = MusicDirectory;
            var files = Device.GetFileSystemEntries(root, name);
            if (files == null || files.Count() <= 0)
                return null;

            var info = Device.GetFileInfo(files[0]);
            var result = new AndroidFile(info, root);
            return result;

        }
        #endregion

        #region Upload Download Delete
        /// <summary>
        /// ファイルをWindows側からAndroid側にコピーする
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void Upload(WindowsFile file)
        {
            if (file == null)
                return;

            if (string.IsNullOrWhiteSpace(MusicDirectory))
                throw new InvalidOperationException("Initialize not call");

            // ディレクトリがないなら作成
            var path = AndroidFile.CombinePath(MusicDirectory, file.RelativePath);
            var dirPath = AndroidFile.GetDirectoryPath(path);
            if (!Device.DirectoryExists(dirPath))
                Device.CreateDirectory(dirPath);

            // ファイルコピー
            using (var stream = new System.IO.FileStream(file.FullPath, System.IO.FileMode.Open))
            {
                Device.UploadFile(stream, path);
            }
        }

        /// <summary>
        /// ファイルをAndroid側からダウンロードする
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public byte[] Download(AndroidFile file)
        {
            if (string.IsNullOrWhiteSpace(MusicDirectory))
                throw new InvalidOperationException("Initialize not call");

            using (var stream = new System.IO.MemoryStream())
            {
                Device.DownloadFile(file.FullPath, stream);
                var result = stream.ToArray();
                return result;
            }
        }

        /// <summary>
        /// Android側のファイルを削除する
        /// </summary>
        /// <param name="file"></param>
        public void Delete(AndroidFile file)
        {
            if (file == null)
                return;

            var path = file.FullPath;
            Device.DeleteFile(path);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Device.Disconnect();
            Device.Dispose();
        }
        #endregion
    }
}
