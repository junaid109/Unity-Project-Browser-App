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
        public async Task<IEnumerable<string>> GetInstalledEditorsAsync()
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

                // 3. Check editors.json
                var editorsJson = Path.Combine(hubDataPath, "editors.json");
                if (File.Exists(editorsJson))
                {
                    try
                    {
                        var json = await File.ReadAllTextAsync(editorsJson);
                        var node = JsonNode.Parse(json);
                        if (node != null)
                        {
                            // If it's an object (old format) or array (new format)
                            if (node is JsonObject obj)
                            {
                                foreach (var editorEntry in obj)
                                {
                                    var path = editorEntry.Value?["location"]?.ToString();
                                    if (!string.IsNullOrEmpty(path) && File.Exists(path)) editors.Add(path);
                                }
                            }
                        }
                    }
                    catch { }
                }
            }

            return editors;
        }

        public async Task<IEnumerable<UnityProject>> GetHubProjectsAsync()
        {
            var hubProjects = new List<UnityProject>();
            var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            var pathsToTry = new[] {
                Path.Combine(roaming, "UnityHub", "projectPrefs.json"),
                Path.Combine(local, "UnityHub", "projectPrefs.json")
            };

            foreach (var projectPrefsPath in pathsToTry)
            {
                if (!File.Exists(projectPrefsPath)) continue;

                try
                {
                    var json = await File.ReadAllTextAsync(projectPrefsPath);
                    var node = JsonNode.Parse(json);
                    if (node != null && node is JsonObject obj)
                    {
                        foreach (var entry in obj)
                        {
                            var projectPath = entry.Key;
                            if (Directory.Exists(projectPath))
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
                catch { }
            }

            return hubProjects;
        }

        public async Task<IEnumerable<UnityProject>> ScanWatchFoldersAsync(IEnumerable<string> watchFolders)
        {
            var projects = new ConcurrentBag<UnityProject>();

            foreach (var rootPath in watchFolders)
            {
                if (Directory.Exists(rootPath))
                {
                    await ScanDirectoryRecursiveAsync(rootPath, projects);
                }
            }

            return projects;
        }

        private async Task ScanDirectoryRecursiveAsync(string directory, ConcurrentBag<UnityProject> projects)
        {
            try
            {
                // Check if this directory is a Unity Project
                var versionPath = Path.Combine(directory, "ProjectSettings", "ProjectVersion.txt");
                if (File.Exists(versionPath))
                {
                    // It is a project!
                    var project = await ParseProjectAsync(directory, versionPath);
                    if (project != null)
                    {
                        projects.Add(project);
                    }
                    // Don't scan inside a project
                    return;
                }

                // Otherwise scan subdirectories (limit depth if needed, but for now simple recursion)
                // We shouldn't go too deep or scan system folders
                var subDirs = Directory.GetDirectories(directory);
                foreach (var subDir in subDirs)
                {
                    // Avoid hidden folders or common non-project folders
                    var name = Path.GetFileName(subDir);
                    if (name.StartsWith(".") || name == "Library" || name == "Temp" || name == "Logs") continue;

                    await ScanDirectoryRecursiveAsync(subDir, projects);
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

        public async Task<string?> GetEditorPathForVersionAsync(string version)
        {
            var editors = await GetInstalledEditorsAsync();
            
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
