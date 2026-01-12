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

        [ObservableProperty]
        private string? _gitBranch;

        [ObservableProperty]
        private string? _gitStatus; // e.g., "3 changes", "Clean"

        [ObservableProperty]
        private bool _isGitRepo;
        
        [ObservableProperty]
        private bool _hasGitChanges;
    }
}
