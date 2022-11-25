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
        /// <summary>
        /// 引数の名前のデバイスを探す
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
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
    }
}
