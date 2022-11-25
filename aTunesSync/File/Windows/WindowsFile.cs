using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace aTunesSync.File.Windows
{
    internal class WindowsFile
        : FileBase
    {

        public FileInfo Info { get; private set; }

        public static readonly char PATH_SEPARATOR = Path.DirectorySeparatorChar;
        public override char DirectorySeparatorChar
        {
            get
            {
                return PATH_SEPARATOR;
            }
        }

        public WindowsFile(FileInfo info, string rootPath)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (!info.FullName.StartsWith(rootPath + PATH_SEPARATOR))
                throw new ArgumentException("root path is wrong");

            Info = info;

            // FileBase
            RootPath = rootPath;
            RelativePath = info.FullName.Substring(rootPath.Length + 1); // +1 = PATH_SEPARATOR
            Size = (ulong)info.Length;
        }
    }
}
