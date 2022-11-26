using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File.Android
{
    /// <summary>
    /// Android用のファイルクラス
    /// </summary>
    internal class AndroidFile
        : FileBase
    {
        public MediaFileInfo Info { get; private set; }

        public static readonly char PATH_SEPARATOR = '\\';
        public override char DirectorySeparatorChar
        {
            get
            {
                return PATH_SEPARATOR;
            }
        }

        public AndroidFile(MediaFileInfo info, string rootPath)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (!info.FullName.StartsWith(rootPath + PATH_SEPARATOR))
                throw new ArgumentException("root path is wrong");

            Info = info;

            // FileBase
            RootPath = rootPath;
            RelativePath = info.FullName.Substring(rootPath.Length + 1); // +1 = PATH_SEPARATOR
            Size = info.Length;
        }

        /// <summary>
        /// インスタンスの親ディレクトリパスを取得
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">PATH_SEPARATORで区切られていない</exception>
        public static string GetDirectoryPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            var lastIndex = path.LastIndexOf(PATH_SEPARATOR);
            if (lastIndex < 0)
                throw new InvalidOperationException();

            var result = path.Substring(0, lastIndex);
            return result;
        }

        /// <summary>
        /// パスを結合する
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string CombinePath(params string[] list)
        {
            var result = string.Join(PATH_SEPARATOR, list);
            return result;
        }
    }
}
