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
        /// <summary>
        /// メソッド呼び出し中のログ
        /// </summary>
        public event MessageEventHandler MessageEvent;

        /// <summary>
        /// Syncの進捗
        /// </summary>
        public event ProgressEventHandler SyncProgressEvent;

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
                var sum = 0;
                if (content.AndroidOnlySet != null)
                    sum += content.AndroidOnlySet.Count();
                if (content.WindowsOnlySet != null)
                    sum += content.WindowsOnlySet.Count();

                var now = 0;

                // android onlyはファイル削除
                // 先に消すことでandroidへコピー時に上書き例外がでないようにする
                if (content.AndroidOnlySet != null)
                {
                    foreach (var item in content.AndroidOnlySet)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MessageEvent($"Cancel Delete");
                            return;
                        }

                        var androidItem = item as AndroidFile;
                        if (androidItem == null)
                            continue;

                        MessageEvent($"[DELETE] {androidItem}");
                        device.Delete(androidItem);

                        ++now;
                        SyncProgressEvent(now, sum);
                    }
                }

                // windows onlyはandroidへコピー
                if (content.WindowsOnlySet != null)
                {
                    foreach (var item in content.WindowsOnlySet)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MessageEvent($"Cancel Copy");
                            return;
                        }

                        var windowsItem = item as WindowsFile;
                        if (windowsItem == null)
                            continue;

                        MessageEvent($"[COPY] {windowsItem}");
                        try
                        {
                            device.Copy(windowsItem);
                        }
                        catch (Exception e)
                        {
                            MessageEvent($"[SKIP] {windowsItem} {e.Message}");
                        }

                        ++now;
                        SyncProgressEvent(now, sum);
                    }
                }

                // android側から空のディレクトリを消したほうがきれいになりそう
            });
        }
    }
}
