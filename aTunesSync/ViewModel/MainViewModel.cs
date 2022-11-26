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
        public ReactiveProperty<string> AndroidDeviceName { get; set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> WindowsRootDirectory { get; set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> iTunesLibraryPath { get; set; } = new ReactiveProperty<string>("");

        public ReactiveProperty<string> SyncButtonText { get; set; } = new ReactiveProperty<string>("Sync");
        public ReactiveProperty<bool> AndroidDeviceEnable { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> WindowsRootEnable { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> WindowsRootDialogEnable { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> iTunesLibraryEnable { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> iTunesLibraryDialogEnable { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> CheckButtonEnable { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<bool> SyncButtonEnable { get; set; } = new ReactiveProperty<bool>(false); // syncはcheckをしないとtrueにならない

        public ReactiveProperty<string> ProgressBarText { get; set; } = new ReactiveProperty<string>("");
        public ReactiveProperty<int> ProgressBarValue { get; set; } = new ReactiveProperty<int>(0);

        public ReactiveCollection<SyncContent> SyncContentList { get; set; } = new ReactiveCollection<SyncContent>();

        public ReactiveProperty<string> Log { get; set; } = new ReactiveProperty<string>("");
    }
}
