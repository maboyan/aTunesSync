using aTunesSync.File.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File.Android
{
    internal class AndroidFileSet
    {
        public SortedSet<AndroidFile> FileSet { get; }

        public AndroidFileSet(SortedSet<AndroidFile> androidFileSet)
        {
            if (androidFileSet == null)
                throw new ArgumentNullException(nameof(androidFileSet));

            FileSet = androidFileSet;
        }

        public AndroidFile Search(WindowsFile windows)
        {
            var result = FileSet.SingleOrDefault(x => x.Equals(windows));
            return result;
        }

        public AndroidFile Search(string name)
        {
            var result = FileSet.SingleOrDefault(x => x.Name == name);
            return result;
        }
    }
}
