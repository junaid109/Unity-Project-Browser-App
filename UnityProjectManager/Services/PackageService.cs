using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public class PackageService : IPackageService
    {
        public async Task<IEnumerable<UnityPackage>> GetPackagesAsync(string projectPath)
        {
            var manifestPath = Path.Combine(projectPath, "Packages", "manifest.json");
            var packages = new List<UnityPackage>();

            if (!File.Exists(manifestPath)) return packages;

            try
            {
                var jsonString = await File.ReadAllTextAsync(manifestPath);
                var jsonNode = JsonNode.Parse(jsonString);

                if (jsonNode?["dependencies"] is JsonObject dependencies)
                {
                    foreach (var dep in dependencies)
                    {
                        packages.Add(new UnityPackage(dep.Key, dep.Value?.ToString() ?? ""));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing manifest: {ex.Message}");
            }

            return packages;
        }

        public async Task<bool> AddPackageAsync(string projectPath, string packageName, string version)
        {
            var manifestPath = Path.Combine(projectPath, "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return false;

            try
            {
                var jsonString = await File.ReadAllTextAsync(manifestPath);
                var jsonNode = JsonNode.Parse(jsonString);

                if (jsonNode?["dependencies"] is JsonObject dependencies)
                {
                    dependencies[packageName] = version;
                    
                    // Write back
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    await File.WriteAllTextAsync(manifestPath, jsonNode.ToJsonString(options));
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }

        public async Task<bool> RemovePackageAsync(string projectPath, string packageName)
        {
            var manifestPath = Path.Combine(projectPath, "Packages", "manifest.json");
            if (!File.Exists(manifestPath)) return false;

            try
            {
                var jsonString = await File.ReadAllTextAsync(manifestPath);
                var jsonNode = JsonNode.Parse(jsonString);

                if (jsonNode?["dependencies"] is JsonObject dependencies)
                {
                    if (dependencies.ContainsKey(packageName))
                    {
                        dependencies.Remove(packageName);
                        
                        // Write back
                        var options = new JsonSerializerOptions { WriteIndented = true };
                        await File.WriteAllTextAsync(manifestPath, jsonNode.ToJsonString(options));
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}
