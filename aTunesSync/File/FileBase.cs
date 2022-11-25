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
    /// ファイルの比較をRelativePathとSizeだけで行う
    /// RootPathは関係ないので注意
    /// </summary>
    internal abstract class FileBase
        : IComparable<FileBase>, IEquatable<FileBase>
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

        /// <summary>
        /// ファイルパスの区切りを表す文字
        /// </summary>
        public abstract char DirectorySeparatorChar
        {
            get;
        }

        /// <summary>
        /// フルパス
        /// </summary>
        public string FullPath
        {
            get
            {
                var result = $"{RootPath}{DirectorySeparatorChar}{RelativePath}";
                return result;
            }
        }

        /// <summary>
        /// ファイル名を取得
        /// </summary>
        public string Name
        {
            get
            {
                var lastIndex = RelativePath.LastIndexOf(DirectorySeparatorChar);
                if (lastIndex < 0)
                    return "";

                // 最後がDirectorySeparatorCharで終わっていて多分ディレクトリ
                if (lastIndex + 1 >= RelativePath.Length)
                    return "";

                return RelativePath.Substring(lastIndex+1);
            }
        }

        #region Object Override
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var file = obj as FileBase;
            if (file == null)
                return false;

            return Equals(file);
        }

        public override int GetHashCode()
        {
            var result = HashCode.Combine(RelativePath, Size);
            return result;
        }

        public override string ToString()
        {
            return FullPath;
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

        #region IEquatable
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
        #endregion
    }
}
