﻿using aTunesSync.File.Android;
using aTunesSync.File.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    internal class FileSyncContent
    {
        public CommonFileSet CommonSet { get; private set; }
        public AndroidFileSet AndroidOnlySet { get; private set; }
        public WindowsFileSet WindowsOnlySet { get; private set; }
        public CommonFileSet UpdateSet { get; private set; }

        public FileSyncContent(CommonFileSet commonSet, AndroidFileSet androidOnlySet, WindowsFileSet windowsOnlySet)
        {
            if (commonSet == null)
                throw new ArgumentNullException(nameof(commonSet));
            if (androidOnlySet == null)
                throw new ArgumentNullException(nameof(androidOnlySet));
            if (windowsOnlySet == null)
                throw new ArgumentNullException(nameof(windowsOnlySet));

            // UpdateSetはあとから導入した関係でこの時点では確定できないので何もしない

            CommonSet = commonSet;
            AndroidOnlySet = androidOnlySet;
            WindowsOnlySet = windowsOnlySet;
            UpdateSet = null;
        }

        public void AppendUpdateSet(CommonFileSet updateSet)
        {
            if (updateSet == null)
                throw new ArgumentNullException(nameof(updateSet));

            UpdateSet = updateSet;
        }
    }
}
