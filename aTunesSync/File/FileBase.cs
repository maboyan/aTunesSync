using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    /// <summary>
    /// Android、Windowsでのそれぞれのファイルを表すためのベースクラス
    /// </summary>
    internal abstract class FileBase
        : IComparable<FileBase>
    {
        /// <summary>
        /// 基準となるパス
        /// </summary>
        public string RootPath { get; protected set; }

        /// <summary>
        /// Rootパスからの相対パス
        /// </summary>
        public string RelativePath { get; protected set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public ulong Size { get; protected set; }

        #region Object Override
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(FileBase other)
        {
            if (other == null)
                return false;

            if (RelativePath != other.RelativePath)
                return false;
            if (Size != other.Size)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            var result = HashCode.Combine(RelativePath, Size);
            return result;
        }

        public override string ToString()
        {
            var result = RelativePath;
            return result;
        }
        #endregion

        #region IComparable
        public int CompareTo(FileBase obj)
        {
            if (obj == null)
                return 1;

            var result = RelativePath.CompareTo(obj.RelativePath);
            return result;
        }
        #endregion
    }
}
