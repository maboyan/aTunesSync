using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    public delegate void MessageEventHandler(string message);
    public delegate void ProgressEventHandler(int now, int num);
}
