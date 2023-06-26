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

        public SortedSet<CommonFile> GetPlaylists(string baseDir, string playlistDir)
        {
            var result = new SortedSet<CommonFile>();
            foreach(var item in FileSet)
            {
                var dir = System.IO.Path.Combine(baseDir, playlistDir);
                var path = item.Windows.FullPath;
                if (path.StartsWith(dir))
                    result.Add(item);
            }

            return result;
        }
    }
}
