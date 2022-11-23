using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File.Android
{
    internal class MusicDirectoryNotFoundException
        : Exception
    {
        public MusicDirectoryNotFoundException()
        {
        }

        public MusicDirectoryNotFoundException(string message)
            : base(message)
        {
        }

        public MusicDirectoryNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
