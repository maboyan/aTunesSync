using aTunesSync.File.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    internal class CommonFileSet
    {
        public SortedSet<CommonFile> FileSet { get; }

        public CommonFileSet(SortedSet<CommonFile> fileSet)
        {
            if (fileSet == null)
                throw new ArgumentNullException(nameof(fileSet));

            FileSet = fileSet;
        }
    }
}
