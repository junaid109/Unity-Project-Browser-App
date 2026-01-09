using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public interface IUnityHubService
    {
        Task<IEnumerable<string>> GetInstalledEditorsAsync();
        Task<IEnumerable<UnityProject>> ScanWatchFoldersAsync(IEnumerable<string> watchFolders);
        Task<IEnumerable<UnityProject>> GetHubProjectsAsync();
        Task<string?> GetEditorPathForVersionAsync(string version);
        Task LaunchProjectAsync(string projectPath, string editorPath);
    }
}
