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
        /// 引数rootの中からmp3, m4aファイルを探す
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public SortedSet<FileBase> GetMusicFiles(string root)
        {
            var result = new SortedSet<FileBase>();

            var mp3List = Directory.GetFiles(root, "*.mp3", SearchOption.AllDirectories);
            foreach (var mp3 in mp3List)
            {
                var info = new FileInfo(mp3);
                var item = new WindowsFile(info, root);
                result.Add(item);
            }

            var m4aList = Directory.GetFiles(root, "*.m4a", SearchOption.AllDirectories);
            foreach (var m4a in m4aList)
            {
                var info = new FileInfo(m4a);
                var item = new WindowsFile(info, root);
                result.Add(item);
            }

            return result;
        }
    }
}
