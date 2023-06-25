using aTunesSync.File.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File.Windows
{
    internal class WindowsFileSet
    {
        public SortedSet<WindowsFile> FileSet { get; }

        public WindowsFileSet(SortedSet<WindowsFile> windowsFileSet)
        {
            if (windowsFileSet == null)
                throw new ArgumentNullException(nameof(windowsFileSet));

            FileSet = windowsFileSet;
        }

        public WindowsFile Search(AndroidFile android)
        {
            var result = FileSet.SingleOrDefault(x => x.Equals(android));
            return result;
        }
    }
}
