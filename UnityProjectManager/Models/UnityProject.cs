using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace UnityProjectManager.Models
{
    public partial class UnityProject : ObservableObject
    {
        [ObservableProperty]
        private string? _name;

        [ObservableProperty]
        private string? _path;

        [ObservableProperty]
        private string? _unityVersion;

        [ObservableProperty]
        private DateTime _lastModified;

        [ObservableProperty]
        private DateTime _lastAccessTime;

        [ObservableProperty]
        private string? _thumbnailPath;
        
        [ObservableProperty]
        private string? _versionType; // e.g., LTS, Beta

        public bool IsLts => VersionType?.Contains("LTS", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
