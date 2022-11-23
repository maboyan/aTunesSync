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
        public static readonly char PATH_SEPARATOR = '\\';

        public MediaFileInfo Info { get; private set; }

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
    }
}
