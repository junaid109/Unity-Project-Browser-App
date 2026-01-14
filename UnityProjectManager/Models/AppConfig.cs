using System.Collections.Generic;

namespace UnityProjectManager.Models
{
    public class AppConfig
    {
        public List<string> WatchFolders { get; set; } = new List<string>();
        public List<string> UnityInstallPaths { get; set; } = new List<string>();
        public string SelectedTab { get; set; } = "Projects";
        public string LastDocsUrl { get; set; } = "https://docs.unity3d.com/Manual/index.html";
        public List<ProjectBoard> Boards { get; set; } = new List<ProjectBoard>();
        // Future: Recent projects, theme settings, etc.
    }
}
