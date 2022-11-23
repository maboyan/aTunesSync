using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace aTunesSync
{
    public class UserSettings
    {
        public string AndroidDeviceName { get; set; }
        public string WindowsRootDirectory { get; set; }
        public string iTunesLibraryFile { get; set; }

        private void Copy(UserSettings obj)
        {
            AndroidDeviceName = obj.AndroidDeviceName;
            WindowsRootDirectory = obj.WindowsRootDirectory;
            iTunesLibraryFile = obj.iTunesLibraryFile;
        }

        public void Save(string path)
        {
            var option = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                WriteIndented = true
            };

            var str = JsonSerializer.Serialize(this, option);
            System.IO.File.WriteAllText(path, str, Encoding.UTF8);
        }

        public void Load(string path)
        {
            var str = System.IO.File.ReadAllText(path);
            var obj = JsonSerializer.Deserialize<UserSettings>(str);
            Copy(obj);
        }

        
    }
}
