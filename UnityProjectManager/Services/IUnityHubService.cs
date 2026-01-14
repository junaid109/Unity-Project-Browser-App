using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public interface IUnityHubService
    {
        Task<IEnumerable<string>> GetInstalledEditorsAsync(IEnumerable<string>? additionalPaths = null);
        Task<IEnumerable<UnityProject>> ScanWatchFoldersAsync(IEnumerable<string> watchFolders);
        Task<IEnumerable<UnityProject>> GetHubProjectsAsync();
        Task<string?> GetEditorPathForVersionAsync(string version, IEnumerable<string>? additionalPaths = null);
        Task LaunchProjectAsync(string projectPath, string editorPath);
    }
}
