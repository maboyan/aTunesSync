using aTunesSync.File.Android;
using aTunesSync.File.Windows;
using Microsoft.VisualBasic;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace aTunesSync.File
{
    internal class FileSetCalculator
    {
        public AndroidFileSet Android { get; }
        public WindowsFileSet Windows { get; }

        public FileSetCalculator(AndroidFileSet android, WindowsFileSet windows)
        {
            if (android == null)
                throw new ArgumentNullException(nameof(android));
            if (windows == null)
                throw new ArgumentNullException(nameof(windows));

            Android = android;
            Windows = windows;
        }

        public WindowsFileSet CalcWindowsOnlySet()
        {
            var androidSet = new SortedSet<FileBase>(Android.FileSet);
            var windowsSet = new SortedSet<FileBase>(Windows.FileSet);

            var onlySet = windowsSet.Except(androidSet);
            var tmp = new SortedSet<WindowsFile>();
            foreach(var item in onlySet)
            {
                var windowsFile = item as WindowsFile;
                if (windowsFile == null)
                    throw new InvalidCastException();

                tmp.Add(windowsFile);
            }

            var result = new WindowsFileSet(tmp);
            return result;
        }

        public AndroidFileSet CalcAndroidOnlySet()
        {
            var androidSet = new SortedSet<FileBase>(Android.FileSet);
            var windowsSet = new SortedSet<FileBase>(Windows.FileSet);

            var onlySet = androidSet.Except(windowsSet);
            var tmp = new SortedSet<AndroidFile>();
            foreach (var item in onlySet)
            {
                var androidFile = item as AndroidFile;
                if (androidFile == null)
                    throw new InvalidCastException();

                tmp.Add(androidFile);
            }

            var result = new AndroidFileSet(tmp);
            return result;
        }

        public CommonFileSet CalcCommonSet()
        {
            var androidSet = new SortedSet<FileBase>(Android.FileSet);
            var windowsSet = new SortedSet<FileBase>(Windows.FileSet);

            var commonSet = androidSet.Intersect(windowsSet);
            var tmp = new SortedSet<CommonFile>();
            foreach (var item in commonSet)
            {
                var androidFile = item as AndroidFile;
                if (androidFile == null)
                    throw new InvalidCastException();

                var windowsFile = Windows.Search(androidFile);
                if (windowsFile == null)
                    throw new InvalidOperationException();

                var common = new CommonFile(androidFile, windowsFile);
                tmp.Add(common);
            }

            var result = new CommonFileSet(tmp);
            return result;
        }
    }
}
