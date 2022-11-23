using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace aTunesSync.File.Android
{
    internal class AndroidFileManager
    {
        public AndroidDevice SearchDevice(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            var devices = MediaDevice.GetDevices();
            var device = devices.FirstOrDefault(d => d.FriendlyName == name);
            if (device == null)
                return null;

            return new AndroidDevice(device);
        }

        /// <summary>
        /// 引数deviceNameのデバイスから音楽フォルダを探し出してmp3, m4aファイルを探す
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        /// <exception cref="DeviceNotFoundException">引数デバイスが見つからない</exception>
        /// <exception cref="MusicDirectoryNotFoundException">デバイスは見つかったけど音楽フォルダが見つからない</exception>
        public SortedSet<FileBase> GetMusicFiles(string deviceName)
        {
            using (var device = SearchDevice(deviceName))
            {
                if (device == null)
                    throw new DeviceNotFoundException(deviceName);

                var musicDir = device.GetMusicDirectory();
                if (musicDir == null)
                    throw new MusicDirectoryNotFoundException();

                var files = device.GetMusicFiles(musicDir);
                return files;
            }
        }
    }
}
