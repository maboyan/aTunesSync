using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.iTunes
{
    internal class iTunesMusic
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Path { get; private set; }

        public iTunesMusic(int id, string name, string path)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException("id");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            Id = id;
            Name = name;

            // 扱いやすいパス形式に変更
            var unescape = Uri.UnescapeDataString(path);
            var localPath = unescape.Replace("file://localhost/", "");
            var winPath = localPath.Replace('/', '\\');
            if (!System.IO.File.Exists(winPath))
                throw new System.IO.FileNotFoundException();

            Path = winPath;
        }
    }
}
