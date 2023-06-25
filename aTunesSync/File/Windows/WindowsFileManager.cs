using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace aTunesSync.File.Windows
{
    internal class WindowsFileManager
    {
        /// <summary>
        /// メソッド呼び出し中のログ
        /// </summary>
        public event MessageEventHandler MessageEvent;

        /// <summary>
        /// GetMusicFilesの進捗
        /// </summary>
        public event ProgressEventHandler GetMusicFilesProgressEvent;

        /// <summary>
        /// 引数rootの中からmp3, m4a, m3u8ファイルを探す
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public SortedSet<WindowsFile> GetMusicFiles(string root)
        {
            var result = new SortedSet<WindowsFile>();

            var mp3List = Directory.GetFiles(root, "*.mp3", SearchOption.AllDirectories);
            var m4aList = Directory.GetFiles(root, "*.m4a", SearchOption.AllDirectories);
            var m3u8List = Directory.GetFiles(root, "*.m3u", SearchOption.AllDirectories);
            var sum = mp3List.Count() + m4aList.Count() + m3u8List.Count();
            var now = 0;

            foreach (var mp3 in mp3List)
            {
                var info = new FileInfo(mp3);
                var item = new WindowsFile(info, root);
                result.Add(item);

                ++now;
                GetMusicFilesProgressEvent(now, sum);
            }

            foreach (var m4a in m4aList)
            {
                var info = new FileInfo(m4a);
                var item = new WindowsFile(info, root);
                result.Add(item);

                ++now;
                GetMusicFilesProgressEvent(now, sum);
            }

            foreach (var m3u8 in m3u8List)
            {
                var info = new FileInfo(m3u8);
                var item = new WindowsFile(info, root);
                result.Add(item);

                ++now;
                GetMusicFilesProgressEvent(now, sum);
            }

            return result;
        }
    }
}
