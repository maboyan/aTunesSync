using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace aTunesSync.iTunes
{
    [System.Diagnostics.DebuggerDisplay("{Name} {Id}")]
    internal class iTunesPlaylist
    {
        /// <summary>
        /// 作成するプレイリスト拡張子
        /// </summary>
        public static readonly string PLAYLIST_EXTENSION = "m3u";

        /// <summary>
        /// ログイベント
        /// </summary>
        public event MessageEventHandler MessageEvent = delegate { };

        /// <summary>
        /// rootかどうか？
        /// 「ライブラリ」や「ダウンロード済み」は親がいないので
        /// 扱いやすいように仮想的にrootという空のプレイリストを作っている
        /// </summary>
        public bool IsRoot { get; private set; }

        public string Id { get; private set; }

        /// <summary>
        /// プレイリスト名
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// プレイリストの親
        /// プレイリストはライブラリの下につくので親がいるはず
        /// </summary>
        public iTunesPlaylist Parent { get; private set; }

        /// <summary>
        /// フォルダかどうか
        /// </summary>
        public bool IsFolder { get; private set; }

        /// <summary>
        /// ユーザー設定で見えないようになっているか
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// 含まれている楽曲たち
        /// </summary>
        public List<iTunesMusic> Musics { get; private set; }

        /// <summary>
        /// 子要素
        /// </summary>
        public List<iTunesPlaylist> Children { get; private set; }

        /// <summary>
        /// 普通のコンストラクタ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="isFolder"></param>
        /// <param name="visible"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public iTunesPlaylist(string id, string name, iTunesPlaylist parent, bool isFolder, bool visible)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException("id");
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("name");

            IsRoot = false;

            Id = id;
            Name = name;
            Parent = parent;
            IsFolder = isFolder;
            IsVisible = visible;
            Musics = new List<iTunesMusic>();
            Children = new List<iTunesPlaylist>();
        }

        /// <summary>
        /// root用特別なコンストラクタ
        /// </summary>
        private iTunesPlaylist()
        {
            IsRoot = true;

            Id = null;
            Name = "root";
            Parent = null;
            IsFolder = true;
            IsVisible = false;
            Musics = new List<iTunesMusic>();
            Children = new List<iTunesPlaylist>();
        }

        /// <summary>
        /// 実際にはプレイリストとして存在しない架空のrootを作成
        /// </summary>
        /// <returns></returns>
        public static iTunesPlaylist CreateRoot()
        {
            return new iTunesPlaylist();
        }

        /// <summary>
        /// 楽曲を追加する
        /// </summary>
        /// <param name="musics"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddMusics(List<iTunesMusic> musics)
        {
            if (musics == null)
                throw new ArgumentNullException("musics");

            Musics.AddRange(musics);
        }

        /// <summary>
        /// 子プレイリストを追加する
        /// </summary>
        /// <param name="child"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddChild(iTunesPlaylist child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            Children.Add(child);
        }

        /// <summary>
        /// 指定されたプレイリストを消す
        /// </summary>
        /// <param name="item"></param>
        public void RemoveChildPlaylist(iTunesPlaylist item)
        {
            Children.Remove(item);
        }

        #region M3U
        /// <summary>
        /// プレイリストを引数の場所に保存する
        /// </summary>
        /// <param name="baseDir"></param>
        /// <param name="playlistDir"></param>
        public void SaveM3u(string baseDir, string playlistDir)
        {
            // 自分と子供のプレイリストを保存
            var savedList = new List<string>();
            SaveM3uCore(baseDir, playlistDir, ref savedList);

            // 現在のBaseDirに存在するplaylistファイルを取得
            var currentList = System.IO.Directory.GetFiles(baseDir, $"*.{PLAYLIST_EXTENSION}", SearchOption.AllDirectories);

            // current - saved = 最初から存在しないファイル = プレイリストから消されたファイル
            var diffList = currentList.Except(savedList);
            foreach (var item in diffList)
            {
                MessageEvent($"[DELETE] {item}");
                System.IO.File.Delete(item);
            }
        }

        private void SaveM3uCore(string baseDir, string playlistDir, ref List<string> savedPaths)
        {
            if (!System.IO.Directory.Exists(baseDir))
                throw new System.IO.DirectoryNotFoundException();
            if (string.IsNullOrWhiteSpace(playlistDir))
                throw new ArgumentNullException("playlistDir");

            if (!IsRoot && !IsFolder)
            {
                var path = CreateM3uPath(baseDir, playlistDir);
                var lines = CreatePlaylistContent(baseDir);
                if (lines != null)
                {
                    MessageEvent($"[CREATE] {path}");
                    var utf8nobom = new System.Text.UTF8Encoding(false);
                    System.IO.File.WriteAllLines(path, lines, utf8nobom);
                    savedPaths.Add(path);
                }
            }

            // 子供も作る
            foreach (var child in Children)
            {
                child.SaveM3uCore(baseDir, playlistDir,ref savedPaths);
            }
        }

        private static readonly char PLAYLIST_TREE_SEPARATOR = '_';
        private string CreateM3uPath(string baseDir, string playlistDir)
        {
            // ルートまで親の名前を調べる
            var tree = new List<string>();
            tree.Add($"{Name}.{PLAYLIST_EXTENSION}");
            var parent = Parent;
            while(parent != null)
            {
                if (parent.IsRoot)
                    break;

                tree.Add(parent.Name);
                parent = parent.Parent;
            }

            // ほしいのはrootから現在のファイルまでなので逆順にする
            tree.Reverse();

            // 階層構造をファイル名にまとめる
            // フォルダを考慮した階層構造に対応したAndroid音楽プレイヤーが見つからなかったのでしょうがなく
            var name = string.Join(PLAYLIST_TREE_SEPARATOR, tree.ToArray());
            
            // ファイル名に使えない文字を置換
            var denyList = System.IO.Path.GetInvalidFileNameChars();
            foreach (var deny in denyList)
            {
                name = name.Replace(deny+"", "");
            }

            // 最終的なパスを決める
            var dir = Path.Combine(baseDir, playlistDir);
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);
            var result = System.IO.Path.Combine(dir, name);

            return result;
        }

        private static readonly string ANDROID_ROOT_PATH = "/storage/emulated/0/Music/";
        public List<string> CreatePlaylistContent(string baseDir)
        {
            if (Musics.Count <= 0)
                return null;

            var result = new List<string>();
            result.Add("#EXTM3U");

            foreach(var music in Musics)
            {
                if (!music.Path.StartsWith(baseDir))
                    throw new InvalidOperationException($"file path belongs to root {music.Path}");
                var relativePath = music.Path.Substring(baseDir.Length);
                if (relativePath.StartsWith(System.IO.Path.DirectorySeparatorChar))
                    relativePath = relativePath.Substring(1);
                var androidPath = ANDROID_ROOT_PATH + relativePath;
                var addPath = androidPath.Replace('\\', '/');

                result.Add("#EXT-X-RATING:0");
                result.Add(addPath);
            }

            return result;
        }
        #endregion

        #region Music List
        /// <summary>
        /// 自分自信や子供のプレイリストに含まれている楽曲をすべて取得
        /// </summary>
        /// <returns></returns>
        public List<iTunesMusic> GetAllMusics()
        {
            var list = new List<iTunesMusic>();
            GetAllMusicsCore(ref list);

            // プレイリストを走査しただけなので同じやつが含まれている
            var result = list.GroupBy((a) => a.Id).Select(a => a.First()).ToList();

            return result;
        }

        private void GetAllMusicsCore(ref List<iTunesMusic> result)
        {
            result.AddRange(Musics);

            foreach(var child in Children)
            {
                child.GetAllMusicsCore(ref result);
            }
        }

        /// <summary>
        /// GetAllMusicsの結果をファイルに書き出す
        /// </summary>
        /// <param name="baseDir"></param>
        /// <param name="savePath"></param>
        public void SaveAllMusics(string baseDir, string savePath)
        {
            var list = GetAllMusics();
            list.Sort((a, b) => a.Id.CompareTo(b.Id));

            // Pathからbasepathを消した新しいlistを作成
            var relativeList = new List<iTunesMusic>(list.Count);
            foreach(var music in list)
            {
                var path = music.Path;
                if (!music.Path.StartsWith(baseDir))
                    throw new InvalidOperationException($"file path belongs to root {music.Path}");
                var relativePath = path.Substring(baseDir.Length);

                var newMusic = new iTunesMusic(music.Id, music.Name, relativePath, music.DateModified);
                relativeList.Add(newMusic);
            }

            var options = new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true,
            };
            var json = JsonSerializer.Serialize(relativeList, options);

            System.IO.File.WriteAllText(savePath, json);
        }
        #endregion
    }
}
