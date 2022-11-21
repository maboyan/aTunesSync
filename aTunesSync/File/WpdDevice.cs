using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    internal class WpdDevice : IDisposable
    {
        public MediaDevice Device { get; private set; }

        public WpdDevice(MediaDevice device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            Device = device;
            Device.Connect();
        }

        #region Enum
        /// <summary>
        /// Musicのルートフォルダを取得する
        /// </summary>
        /// <returns></returns>
        public string GetMusicDirectory()
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
            foreach(var dir in dirs)
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

        /// <summary>
        /// 引数で与えられたルートフォルダから音楽ファイルを探し出して集合を作ってくれる
        /// mp3, m4aを探す
        /// </summary>
        /// <param name="root">GetMusicDirectoryの戻り値</param>
        /// <returns></returns>
        public SortedSet<FileBase> GetMusicFiles(string root)
        {
            var result = new SortedSet<FileBase>();

            var mp3List = Device.GetFileSystemEntries(root, "*.mp3", System.IO.SearchOption.AllDirectories);
            foreach (var mp3 in mp3List)
            {
                var info = Device.GetFileInfo(mp3);
                var item = new WpdFile(info, root);
                result.Add(item);
            }

            var m4aList = Device.GetFileSystemEntries(root, "*.m4a", System.IO.SearchOption.AllDirectories);
            foreach (var m4a in m4aList)
            {
                var info = Device.GetFileInfo(m4a);
                var item = new WpdFile(info, root);
                result.Add(item);
            }

            return result;
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
