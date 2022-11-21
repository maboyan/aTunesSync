using MediaDevices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aTunesSync.File
{
    internal class WpdDeviceManager
    {
        public WpdDeviceManager()
        {
        }

        public WpdDevice SearchDevice(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            var devices = MediaDevice.GetDevices();
            var device = devices.FirstOrDefault(d => d.FriendlyName == name);
            if (device == null)
                return null;

            return new WpdDevice(device);
        }
    }
}
