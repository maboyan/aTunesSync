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
        public async Task<FileSyncContent> CheckAsync(AndroidDevice device, AndroidFileSet android, WindowsFileSet windows)
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
                var calculator = new FileSetCalculator(android, windows);

                var commonSet = calculator.CalcCommonSet();
                var androidOnlySet = calculator.CalcAndroidOnlySet();
                var windowsOnlySet = calculator.CalcWindowsOnlySet();
                result = new FileSyncContent(commonSet, androidOnlySet, windowsOnlySet);
            });
             
            return result;
        }

        /// <summary>
        /// CheckAsyncの内容に基づき同期を行う
        /// </summary>
        /// <param name="device"></param>
        /// <param name="content"></param>
        /// <param name="overwrite"></param>
        /// <param name="token"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SyncAsync(AndroidDevice device, FileSyncContent content, bool overwrite, CancellationToken token)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            await Task.Run(() =>
            {
                var sum = 0;
                if (content.AndroidOnlySet != null)
                    sum += content.AndroidOnlySet.FileSet.Count();
                if (content.WindowsOnlySet != null)
                    sum += content.WindowsOnlySet.FileSet.Count();
                if (overwrite && content.CommonSet != null)
                    sum += content.CommonSet.FileSet.Count();

                var now = 0;

                // android onlyはファイル削除
                // 先に消すことでandroidへコピー時に上書き例外がでないようにする
                if (content.AndroidOnlySet != null)
                {
                    foreach (var item in content.AndroidOnlySet.FileSet)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MessageEvent($"Cancel Delete");
                            return;
                        }

                        MessageEvent($"[DELETE] {item}");
                        device.Delete(item);

                        ++now;
                        SyncProgressEvent(now, sum);
                    }
                }

                // windows onlyはandroidへコピー
                if (content.WindowsOnlySet != null)
                {
                    foreach (var item in content.WindowsOnlySet.FileSet)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MessageEvent($"Cancel Copy");
                            return;
                        }

                        MessageEvent($"[COPY] {item}");
                        try
                        {
                            device.Copy(item);
                        }
                        catch (Exception e)
                        {
                            MessageEvent($"[SKIP] {item} {e.Message}");
                        }

                        ++now;
                        SyncProgressEvent(now, sum);
                    }
                }

                if (overwrite && content.CommonSet != null)
                {
                    foreach (var item in content.CommonSet.FileSet)
                    {
                        if (token.IsCancellationRequested)
                        {
                            MessageEvent($"Cancel Overwrite");
                            return;
                        }

                        var windowsItem = item.Windows;
                        if (windowsItem == null)
                            continue;
                        var androidItem = item.Android;
                        if (androidItem == null)
                            continue;

                        MessageEvent($"[Overwrite] {windowsItem}");
                        try
                        {
                            device.Delete(androidItem);
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
