using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public class UnityHubService : IUnityHubService
    {
        public async Task<IEnumerable<string>> GetInstalledEditorsAsync(IEnumerable<string>? additionalPaths = null)
        {
            var editors = new HashSet<string>();
            
            // 1. Default Unity Hub Path
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Unity", "Hub", "Editor");
            if (Directory.Exists(defaultPath))
            {
                foreach (var dir in Directory.GetDirectories(defaultPath))
                {
                    var editorPath = Path.Combine(dir, "Editor", "Unity.exe");
                    if (File.Exists(editorPath))
                    {
                        editors.Add(editorPath);
                    }
                }
            }

            // 2. Check secondaryInstallPath.conf
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            var pathsToTry = new[] {
                Path.Combine(roaming, "UnityHub"),
                Path.Combine(local, "UnityHub")
            };

            foreach (var hubDataPath in pathsToTry)
            {
                var secondaryPathConf = Path.Combine(hubDataPath, "secondaryInstallPath.json");
                if (!File.Exists(secondaryPathConf))
                    secondaryPathConf = Path.Combine(hubDataPath, "secondaryInstallPath.conf");

                if (File.Exists(secondaryPathConf))
                {
                    try 
                    {
                        var content = await File.ReadAllTextAsync(secondaryPathConf);
                        var path = content.Trim().Replace("\"", ""); // Clean up potential quotes
                        if (Directory.Exists(path))
                        {
                             foreach (var dir in Directory.GetDirectories(path))
                            {
                                var editorPath = Path.Combine(dir, "Editor", "Unity.exe");
                                if (File.Exists(editorPath)) editors.Add(editorPath);
                            }
                        }
                    }
                    catch { }
                }

                // 3. Check editors.json and editors-v2.json
                var editorsJson = Path.Combine(hubDataPath, "editors.json");
                var editorsV2Json = Path.Combine(hubDataPath, "editors-v2.json");

                await ParseEditorsFileAsync(editorsJson, editors);
                await ParseEditorsFileAsync(editorsV2Json, editors);
            }

            // 3. Fallback: Check "C:\Program Files\Unity\Hub\Editor" if not already found
            // (Already checked at step 1)

            // 4. Check Additional user paths
            if (additionalPaths != null)
            {
                foreach (var path in additionalPaths)
                {
                    if (!Directory.Exists(path)) continue;

                     // Check if path IS an installation (e.g. key/2022.3.1)
                    var directEditor = Path.Combine(path, "Editor", "Unity.exe");
                    if (File.Exists(directEditor))
                    {
                        editors.Add(directEditor);
                        continue;
                    }

                    // Check subfolders
                    foreach (var dir in Directory.GetDirectories(path))
                    {
                        var subEditor = Path.Combine(dir, "Editor", "Unity.exe");
                        if (File.Exists(subEditor)) editors.Add(subEditor);
                    }
                }
            }
            
            return editors;
        }

        private async Task ParseEditorsFileAsync(string filePath, HashSet<string> editors)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var node = JsonNode.Parse(json);
                if (node == null) return;

                // Handle editors-v2.json format (has "data" array)
                if (node["data"] is JsonArray dataArray)
                {
                    foreach (var item in dataArray)
                    {
                        var path = item?["location"]?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path)) editors.Add(path);
                    }
                }
                // Handle editors.json format (object or array)
                else if (node is JsonObject obj)
                {
                    foreach (var editorEntry in obj)
                    {
                        var path = editorEntry.Value?["location"]?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path)) editors.Add(path);
                    }
                }
                else if (node is JsonArray arr)
                {
                    foreach (var item in arr)
                    {
                        var path = item?["location"]?.ToString();
                        if (!string.IsNullOrEmpty(path) && File.Exists(path)) editors.Add(path);
                    }
                }
            }
            catch { }
        }

        public async Task<IEnumerable<UnityProject>> GetHubProjectsAsync()
        {
            var hubProjects = new List<UnityProject>();
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var pathsToTry = new[] {
                Path.Combine(roaming, "UnityHub", "projects-v1.json"),
                Path.Combine(roaming, "UnityHub", "projectPrefs.json")
            };

            foreach (var projectPrefsPath in pathsToTry)
            {
                if (!File.Exists(projectPrefsPath)) continue;

                try
                {
                    var json = await File.ReadAllTextAsync(projectPrefsPath);
                    var node = JsonNode.Parse(json);
                    if (node != null)
                    {
                        // projects-v1.json has a "data" property containing the projects
                        var projectsData = node["data"] ?? node;
                        
                        if (projectsData is JsonObject obj)
                        {
                            foreach (var entry in obj)
                            {
                                string? projectPath = null;
                                
                                // Try projects-v1.json style (value is object)
                                if (entry.Value is JsonObject projectObj)
                                {
                                    projectPath = projectObj["path"]?.ToString() ?? projectObj["projectPath"]?.ToString();
                                }
                                else
                                {
                                    // Try projectPrefs.json style (key is path)
                                    projectPath = entry.Key;
                                }

                                if (!string.IsNullOrEmpty(projectPath) && Directory.Exists(projectPath))
                                {
                                    var versionPath = Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt");
                                    if (File.Exists(versionPath))
                                    {
                                        var project = await ParseProjectAsync(projectPath, versionPath);
                                        if (project != null) hubProjects.Add(project);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            return hubProjects;
        }

        public async Task<IEnumerable<UnityProject>> ScanWatchFoldersAsync(IEnumerable<string> watchFolders)
        {
            var projects = new ConcurrentBag<UnityProject>();

            // Limit concurrency for scanning to avoid disk trash
            await Parallel.ForEachAsync(watchFolders, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async (rootPath, ct) => 
            {
                if (Directory.Exists(rootPath))
                {
                    await ScanDirectoryRecursiveAsync(rootPath, projects, 0);
                }
            });

            return projects;
        }

        private async Task ScanDirectoryRecursiveAsync(string directory, ConcurrentBag<UnityProject> projects, int depth)
        {
            if (depth > 4) return; // Hard limit on depth to prevent infinite loops or massive scans

            try
            {
                // Check if this directory is a Unity Project
                var versionPath = Path.Combine(directory, "ProjectSettings", "ProjectVersion.txt");
                var assetsPath = Path.Combine(directory, "Assets");
                var projectSettingsPath = Path.Combine(directory, "ProjectSettings");

                bool isProject = false;
                if (File.Exists(versionPath)) isProject = true;
                else if (Directory.Exists(assetsPath) && Directory.Exists(projectSettingsPath)) isProject = true;

                if (isProject)
                {
                    // It is a project!
                    var project = await ParseProjectAsync(directory, versionPath);
                    if (project != null)
                    {
                        projects.Add(project);
                    }
                    else
                    {
                        // Fallback entry if version file is missing but folders exist
                        if (!File.Exists(versionPath))
                        {
                             projects.Add(new UnityProject 
                             {
                                Name = new DirectoryInfo(directory).Name,
                                Path = directory,
                                UnityVersion = "Unknown",
                                LastModified = Directory.GetLastWriteTime(directory),
                                VersionType = "Unknown"
                             });
                        }
                    }
                    // Don't scan inside a project
                    return;
                }

                // Optimization: Don't go deeper if we are already at depth 3 and haven't found a project yet? 
                // Actually, users might organize like Category/Genre/Project, so depth 4 is reasonable.

                var subDirs = Directory.GetDirectories(directory);
                foreach (var subDir in subDirs)
                {
                    // FILTER: Avoid hidden folders or common non-project folders
                    var name = Path.GetFileName(subDir);
                    if (name.StartsWith(".") || 
                        name.Equals("Library", StringComparison.OrdinalIgnoreCase) || 
                        name.Equals("Temp", StringComparison.OrdinalIgnoreCase) || 
                        name.Equals("Logs", StringComparison.OrdinalIgnoreCase) || 
                        name.Equals("obj", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Build", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("Builds", StringComparison.OrdinalIgnoreCase) ||
                        name.Equals("node_modules", StringComparison.OrdinalIgnoreCase)) 
                    {
                        continue;
                    }

                    // Recurse
                    await ScanDirectoryRecursiveAsync(subDir, projects, depth + 1);
                }
            }
            catch (Exception ex)
            {
                // Access denied or path too long
                Console.WriteLine($"Skipping {directory}: {ex.Message}");
            }
        }

        private async Task<UnityProject?> ParseProjectAsync(string projectPath, string versionFilePath)
        {
            try
            {
                var lines = await File.ReadAllLinesAsync(versionFilePath);
                string unityVersion = "Unknown";
                
                foreach (var line in lines)
                {
                    if (line.StartsWith("m_EditorVersion:"))
                    {
                        unityVersion = line.Split(':')[1].Trim();
                        break;
                    }
                }

                var dirInfo = new DirectoryInfo(projectPath);

                // Attempt to find a thumbnail
                string? thumbnail = null;
                var iconPath = Path.Combine(projectPath, "Assets", "Editor", "ProjectIcon.png");
                if (File.Exists(iconPath))
                {
                    thumbnail = iconPath;
                }

                return new UnityProject
                {
                    Name = dirInfo.Name,
                    Path = projectPath,
                    UnityVersion = unityVersion,
                    LastModified = dirInfo.LastWriteTime,
                    LastAccessTime = dirInfo.LastAccessTime,
                    ThumbnailPath = thumbnail,
                    VersionType = DetermineVersionType(unityVersion)
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<string?> GetEditorPathForVersionAsync(string version, IEnumerable<string>? additionalPaths = null)
        {
            var editors = await GetInstalledEditorsAsync(additionalPaths);
            
            // Try exact match first
            foreach (var editor in editors)
            {
                // Editor path usually is .../2022.3.10f1/Editor/Unity.exe
                if (editor.Contains(version)) return editor;
            }

            // Try partial match (same major.minor)
            var parts = version.Split('.');
            if (parts.Length >= 2)
            {
                var minorVersion = $"{parts[0]}.{parts[1]}.";
                foreach (var editor in editors)
                {
                    if (editor.Contains(minorVersion)) return editor;
                }
            }

            return editors.FirstOrDefault();
        }

        public async Task LaunchProjectAsync(string projectPath, string editorPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = editorPath,
                        Arguments = $"-projectPath \"{projectPath}\"",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Launch failed: {ex.Message}");
                }
            });
        }

        private string DetermineVersionType(string version)
        {
            if (version.Contains("f")) return "f (Final)"; // Standard release often implies stability but not strictly LTS unless 202x.4/3
            if (version.Contains("LTS")) return "LTS"; // Explicit LTS parsing if string has it
            
            // Heuristic for LTS: 2020.3.x, 2021.3.x, 2022.3.x
            if (version.Contains("2020.3.") || version.Contains("2021.3.") || version.Contains("2022.3.")) return "LTS";
            if (version.Contains("a")) return "Alpha";
            if (version.Contains("b")) return "Beta";
            
            return "Release";
        }
    }
}
