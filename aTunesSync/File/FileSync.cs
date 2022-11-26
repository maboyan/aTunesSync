using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using aTunesSync.File.Android;
using aTunesSync.File.Windows;

namespace aTunesSync.File
{
    internal class FileSync
    {
        public delegate void MessageEventHandler(string message);

        /// <summary>
        /// メソッド呼び出し中のログ
        /// </summary>
        public event MessageEventHandler MessageEvent;

        /// <summary>
        /// 同期するファイル内容を確認する
        /// </summary>
        /// <param name="device"></param>
        /// <param name="android"></param>
        /// <param name="windows"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<FileSyncContent> CheckAsync(AndroidDevice device, SortedSet<FileBase> android, SortedSet<FileBase> windows)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (android == null)
                throw new ArgumentNullException(nameof(android));
            if (windows == null)
                throw new ArgumentNullException(nameof(windows));

            FileSyncContent result = null;
            await Task.Run(() =>
            {
                var commonSet = android.Intersect(windows);
                var androidOnlySet = android.Except(windows);
                var windowsOnlySet = windows.Except(android);
                result = new FileSyncContent(commonSet, androidOnlySet, windowsOnlySet);
            });
             
            return result;
        }

        /// <summary>
        /// CheckAsyncの内容に基づき同期を行う
        /// </summary>
        /// <param name="device"></param>
        /// <param name="content"></param>
        /// <param name="token"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SyncAsync(AndroidDevice device, FileSyncContent content, CancellationToken token)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            await Task.Run(() =>
            {
                // android onlyはファイル削除
                // 先に消すことでandroidへコピー時に上書き例外がでないようにする
                if (content.AndroidOnlySet != null)
                {
                    foreach (var item in content.AndroidOnlySet)
                    {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();

                        var androidItem = item as AndroidFile;
                        if (androidItem == null)
                            continue;

                        MessageEvent($"[DELETE] {androidItem}");
                        device.Delete(androidItem);
                    }
                }

                // windows onlyはandroidへコピー
                if (content.WindowsOnlySet != null)
                {
                    foreach (var item in content.WindowsOnlySet)
                    {
                        if (token.IsCancellationRequested)
                            token.ThrowIfCancellationRequested();

                        var windowsItem = item as WindowsFile;
                        if (windowsItem == null)
                            continue;

                        MessageEvent($"[COPY] {windowsItem}");
                        device.Copy(windowsItem);
                    }
                }

                // android側から空のディレクトリを消したほうがきれいになりそう

            }, token);
        }
    }
}
