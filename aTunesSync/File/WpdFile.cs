using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    internal class WpdFile
        : FileBase
    {
        public static readonly string PATH_SEPARATOR = @"\";

        public MediaFileInfo Info { get; private set; }

        public WpdFile(MediaFileInfo info, string rootPath)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            if (!info.FullName.StartsWith(rootPath + PATH_SEPARATOR))
                throw new ArgumentException("root path is wrong");

            Info = info;

            // FileBase
            RelativePath = info.FullName.Substring(rootPath.Length + 1); // +1 = PATH_SEPARATOR
            Size = info.Length;
        }
    }
}
