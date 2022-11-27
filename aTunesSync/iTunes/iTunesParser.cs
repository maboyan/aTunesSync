using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Ribbon.Primitives;
using System.Xml;
using System.Xml.Linq;

namespace aTunesSync.iTunes
{
    internal class iTunesParser
    {
        public static readonly string[] IGNORE_PLAYLIST_NAMES =
        {
            "ライブラリ",
            "ダウンロード済み",
            "ミュージック",
        };
        public string Path { get; private set; }

        public iTunesParser(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (!System.IO.File.Exists(path))
                throw new System.IO.FileNotFoundException(path);

            Path = path;
        }

        /// <summary>
        /// itunes xmlに基づいて解析を行う
        /// </summary>
        /// <returns></returns>
        public async Task<iTunesPlaylist> ParseAsync()
        {
            iTunesPlaylist result = null;
            await Task.Run(() =>
            {
                result = Parse();
            });

            return result;
        }

        public iTunesPlaylist Parse()
        {
            if (!System.IO.File.Exists(Path))
                throw new System.IO.FileNotFoundException(Path);

            var xml = XElement.Load(Path);
            Dictionary<int, iTunesMusic> tracks = null;
            iTunesPlaylist result = null;
            foreach (XElement child in xml.Nodes())
            {
                if (child.Name == "dict")
                {
                    var bNextTracks = false;
                    var bNextPlaylists = false;
                    foreach(XElement gchild in child.Nodes())
                    {
                        if (bNextTracks)
                        {
                            if (gchild.Name != "dict")
                                throw new InvalidOperationException("Unknown format");
                            
                            tracks = ParseTracks(gchild);
                            bNextTracks = false;
                            continue;
                        }
                        
                        if (bNextPlaylists)
                        {
                            if (gchild.Name != "array")
                                throw new InvalidOperationException("Unknown format");

                            result = ParsePlaylist(gchild, tracks);
                            bNextPlaylists = false;
                            break;
                        }

                        // keyの次のdictが対象物なのでフラグを立てる
                        if (gchild.Name == "key" && gchild.Value == "Tracks")
                        {
                            bNextTracks = true;
                            continue;
                        }
                        if (gchild.Name == "key" && gchild.Value == "Playlists")
                        {
                            bNextPlaylists = true;
                            continue;
                        }
                    }
                }
            }

            return result;
        }

        private Dictionary<int, iTunesMusic> ParseTracks(XElement tracks)
        {
            var result = new Dictionary<int, iTunesMusic>();

            var children = tracks.Nodes();
            var count = children.Count();
            for (var i = 0; i < count; i=i+2)
            {
                var key = children.ElementAt(i) as XElement;
                var dict = children.ElementAt(i + 1) as XElement;
                if (key == null || dict == null)
                    throw new InvalidOperationException("node cast failed");
                if (key.Name != "key" || dict.Name != "dict")
                    throw new InvalidOperationException("invalid node type");

                var music = ParseTracksCore(key, dict);
                result.Add(music.Id, music);
            }

            return result;
        }

        private iTunesMusic ParseTracksCore(XElement key, XElement dict)
        {
            var id = int.Parse(key.Value);
            string name = null;
            string path = null;

            var bNextName = false;
            var bNextPath = false;
            foreach(XElement child in dict.Nodes())
            {
                if (bNextName)
                {
                    if (child.Name != "string")
                        throw new InvalidOperationException("Unknown format");

                    name = child.Value;
                    bNextName = false;
                    continue;
                }
                if (bNextPath)
                {
                    if (child.Name != "string")
                        throw new InvalidOperationException("Unknown format");

                    path = child.Value;
                    bNextPath = false;
                    continue;
                }

                if (child.Name == "key" && child.Value == "Name")
                {
                    bNextName = true;
                    continue;
                }
                if (child.Name == "key" && child.Value == "Location")
                {
                    bNextPath = true;
                    continue;
                }

                if (name != null && path != null)
                    break;
            }

            var result = new iTunesMusic(id, name, path);
            return result;
        }

        private iTunesPlaylist ParsePlaylist(XElement playlists, Dictionary<int, iTunesMusic> tracks)
        {
            iTunesPlaylist root = iTunesPlaylist.CreateRoot();

            var children = playlists.Nodes();
            foreach(XElement child in children)
            {
                ParsePlaylistCore(child, tracks, ref root);
            }

            // 最後に余分なプレイリストを抜く
            var removeList = new List<iTunesPlaylist>();
            foreach(var name in IGNORE_PLAYLIST_NAMES)
            {
                var target = root.Children.FindAll((p) => p.Name == name);
                if (target != null && target.Count > 0)
                    removeList.AddRange(target);
            }
            foreach(var removeItem in removeList)
            {
                root.RemoveChildPlaylist(removeItem);
            }

            return root;
        }
        private void ParsePlaylistCore(XElement playlist, Dictionary<int, iTunesMusic> tracks, ref iTunesPlaylist root)
        {
            string id = null;
            string name = null;
            string parentId = null;
            bool isFolder = false;
            bool isVisible = false;
            List<iTunesMusic> playlistItems = null;

            bool bNextId = false;
            bool bNextName = false;
            bool bNextParentId = false;
            bool bNextFolder = false;
            bool bNextVisible = false;
            bool bNextTracks = false;
            foreach(XElement child in playlist.Nodes())
            {
                // valueノード解析
                if (bNextId)
                {
                    if (child.Name != "string")
                        throw new InvalidOperationException("Unknown format");

                    id = child.Value;
                    bNextId = false;
                    continue;
                }
                if (bNextName)
                {
                    if (child.Name != "string")
                        throw new InvalidOperationException("Unknown format");

                    name = child.Value;
                    bNextName = false;
                    continue;
                }
                if (bNextParentId)
                {
                    if (child.Name != "string")
                        throw new InvalidOperationException("Unknown format");

                    parentId = child.Value;
                    bNextParentId = false;
                    continue;
                }
                if (bNextFolder)
                {
                    var nameNode = child.Name;
                    if (nameNode.LocalName != "true" && nameNode.LocalName != "false")
                        throw new InvalidOperationException("Unknown format");

                    if (nameNode.LocalName == "true")
                        isFolder = true;
                    else
                        isFolder = false;

                    bNextFolder = false;
                    continue;
                }
                if (bNextVisible)
                {
                    var nameNode = child.Name;
                    if (nameNode.LocalName != "true" && nameNode.LocalName != "false")
                        throw new InvalidOperationException("Unknown format");

                    if (nameNode.LocalName == "true")
                        isVisible = true;
                    else
                        isVisible = false;

                    bNextVisible = false;
                    continue;
                }
                if (bNextTracks)
                {
                    if (child.Name != "array")
                        throw new InvalidOperationException("Unknown format");

                    playlistItems = ParsePlaylistItems(child, tracks);
                    bNextTracks = false;
                    continue;
                }

                // keyノード解析
                if (child.Name == "key" && child.Value == "Playlist Persistent ID")
                {
                    bNextId = true;
                    continue;
                }
                
                if (child.Name == "key" && child.Value == "Name")
                {
                    bNextName = true;
                    continue;
                }
                if (child.Name == "key" && child.Value == "Parent Persistent ID")
                {
                    bNextParentId = true;
                    continue;
                }
                if (child.Name == "key" && child.Value == "Folder")
                {
                    bNextFolder = true;
                    continue;
                }
                if (child.Name == "key" && child.Value == "Visible")
                {
                    bNextVisible = true;
                    continue;
                }
                if (child.Name == "key" && child.Value == "Playlist Items")
                {
                    bNextTracks = true;
                    continue;
                }
            }

            // playlistitemが存在しないnodeが存在する
            // そういったものはプレイリストとして成り立っていないので何もしない
            if (playlistItems == null)
                return;

            // 親を探す
            iTunesPlaylist parent = null;
            if (parentId != null)
                parent = SearchPlaylist(root, parentId);

            var item = new iTunesPlaylist(id, name, parent, isFolder, isVisible);
            item.AddMusics(playlistItems);
            
            // 親がいない場合はrootの子供として扱う
            if (parent != null)
                parent.AddChild(item);
            else
                root.AddChild(item);
        }

        private List<iTunesMusic> ParsePlaylistItems(XElement items, Dictionary<int, iTunesMusic> tracks)
        {
            var result = new List<iTunesMusic>();

            foreach(XElement child in items.Nodes())
            {
                if (child.Name != "dict")
                    continue;

                var nodes = child.Nodes();
                var count = nodes.Count();
                for (var i = 0; i < count; i = i + 2)
                {
                    var key = nodes.ElementAt(i) as XElement;
                    var value = nodes.ElementAt(i + 1) as XElement;
                    if (key == null || value == null)
                        throw new InvalidOperationException("node cast failed");
                    if (key.Name != "key" || value.Name != "integer")
                        throw new InvalidOperationException("invalid node type");
                    if (key.Value != "Track ID")
                        throw new InvalidOperationException("invalid key name");

                    var id = int.Parse(value.Value);
                    if (tracks.ContainsKey(id))
                        result.Add(tracks[id]);
                }
            }

            return result;

        }

        private iTunesPlaylist SearchPlaylist(iTunesPlaylist playlist, string id)
        {
            if (playlist == null)
                return null;

            if (playlist.Id == id)
                return playlist;

            foreach(var child in playlist.Children)
            {
                var ret = SearchPlaylist(child, id);
                if (ret != null)
                    return ret;
            }

            return null;
        }
    }
}
