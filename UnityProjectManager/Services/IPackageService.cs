using System.Collections.Generic;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public interface IPackageService
    {
        Task<IEnumerable<UnityPackage>> GetPackagesAsync(string projectPath);
        Task<bool> AddPackageAsync(string projectPath, string packageName, string version);
        Task<bool> RemovePackageAsync(string projectPath, string packageName);
    }
}
