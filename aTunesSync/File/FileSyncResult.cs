using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    internal class FileSyncContent
    {
        public IEnumerable<FileBase> CommonSet { get; private set; }
        public IEnumerable<FileBase> AndroidOnlySet { get; private set; }
        public IEnumerable<FileBase> WindowsOnlySet { get; private set; }

        public FileSyncContent(IEnumerable<FileBase> commonSet, IEnumerable<FileBase> androidOnlySet, IEnumerable<FileBase> windowsOnlySet)
        {
            if (commonSet == null)
                throw new ArgumentNullException(nameof(commonSet));
            if (androidOnlySet == null)
                throw new ArgumentNullException(nameof(androidOnlySet));
            if (windowsOnlySet == null)
                throw new ArgumentNullException(nameof(windowsOnlySet));

            CommonSet = commonSet;
            AndroidOnlySet = androidOnlySet;
            WindowsOnlySet = windowsOnlySet;
        }
    }
}
