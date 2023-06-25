using aTunesSync.File.Android;
using aTunesSync.File.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    /// <summary>
    /// AndroidにもWindowsにも存在するファイルを示すためのクラス
    /// 
    /// 比較はWindows基準で行われるので注意
    /// </summary>
    internal class CommonFile
        : IComparable<CommonFile>, IEquatable<CommonFile>
    {
        public AndroidFile Android { get; }
        public WindowsFile Windows { get; }

        public CommonFile(AndroidFile android, WindowsFile windows)
        {
            if (android == null)
                throw new ArgumentNullException(nameof(android));
            if (windows == null)
                throw new ArgumentNullException(nameof(windows));

            Android = android;
            Windows = windows;
        }

        #region Object Override
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var file = obj as CommonFile;
            if (file == null)
                return false;

            return Equals(file);
        }

        public override int GetHashCode()
        {
            var result = HashCode.Combine(Android, Windows);
            return result;
        }

        public override string ToString()
        {
            var result = $"{Android.FullPath} / {Windows.FullPath}";
            return result;
        }
        #endregion

        #region IComparable
        public int CompareTo(CommonFile obj)
        {
            if (obj == null)
                return 1;

            // Windows基準で比較する
            var result = Windows.RelativePath.CompareTo(obj.Windows.RelativePath);
            return result;
        }
        #endregion

        #region IEquatable
        public bool Equals(CommonFile other)
        {
            if (other == null)
                return false;

            if (Android.RelativePath != other.Android.RelativePath)
                return false;
            if (Android.Size != other.Android.Size)
                return false;

            if (Windows.RelativePath != other.Windows.RelativePath)
                return false;
            if (Windows.Size != other.Windows.Size)
                return false;


            return true;
        }
        #endregion
    }
}
