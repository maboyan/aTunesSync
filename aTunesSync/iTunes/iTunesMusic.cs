using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace aTunesSync.iTunes
{
    /// <summary>
    /// JsonConverterを書くのがめんどくさくて色々と雑な作り
    /// </summary>
    internal class iTunesMusic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime DateModified { get; set; }

        public iTunesMusic(int id, string name, string path, string dateModified)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException("id");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");
            if (string.IsNullOrWhiteSpace(dateModified))
                throw new ArgumentNullException("dateModified");

            Id = id;
            Name = name;

            // 扱いやすいパス形式に変更
            var unescape = Uri.UnescapeDataString(path);
            var localPath = unescape.Replace("file://localhost/", "");
            var winPath = localPath.Replace('/', '\\');
            if (!System.IO.File.Exists(winPath))
                throw new System.IO.FileNotFoundException();

            Path = winPath;
            DateModified = DateTime.Parse(dateModified);
        }

        /// <summary>
        /// 他のiTunesMusicから流用するときに使う
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="dateModified"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public iTunesMusic(int id, string name, string path, DateTime dateModified)
        {
            if (id < 0)
                throw new ArgumentOutOfRangeException("id");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            Id = id;
            Name = name;
            Path = path;
            DateModified = dateModified;
        }

        /// <summary>
        /// JSON用にからのコンストラクタ
        /// </summary>
        public iTunesMusic()
        {
            Id = -1;
            Name = null;
            Path = null;
            DateModified = DateTime.MinValue;
        }
    }
}
