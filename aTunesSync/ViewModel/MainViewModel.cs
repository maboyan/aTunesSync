using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Reactive.Bindings;

namespace aTunesSync.ViewModel
{
    internal class MainViewModel
    {
        public ReactiveProperty<string> AndroidDeviceName { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> WindowsRootDirectory { get; set; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> iTunesLibraryPath { get; set; } = new ReactiveProperty<string>();

        public ReactiveCollection<SyncContent> SyncContentList { get; set; } = new ReactiveCollection<SyncContent>();
    }
}
