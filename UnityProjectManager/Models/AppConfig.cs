using System.Collections.Generic;

namespace UnityProjectManager.Models
{
    public class AppConfig
    {
        public List<string> WatchFolders { get; set; } = new List<string>();
        // Future: Recent projects, theme settings, etc.
    }
}
