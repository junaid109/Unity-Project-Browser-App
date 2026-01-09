using System;
using System.IO;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public interface IProjectEditorService
    {
        Task<bool> RenameProjectAsync(UnityProject project, string newName);
        Task<bool> ChangeUnityVersionAsync(UnityProject project, string newVersion);
        Task<bool> CleanLibraryAsync(UnityProject project);
    }

    public class ProjectEditorService : IProjectEditorService
    {
        public async Task<bool> RenameProjectAsync(UnityProject project, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) return false;

            try
            {
                var parentDir = Path.GetDirectoryName(project.Path);
                if (parentDir == null) return false;

                var newPath = Path.Combine(parentDir, newName);

                if (Directory.Exists(newPath)) return false; // Already exists

                // Rename directory
                Directory.Move(project.Path, newPath);
                
                // Update project object
                project.Path = newPath;
                project.Name = newName;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rename failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ChangeUnityVersionAsync(UnityProject project, string newVersion)
        {
            var versionPath = Path.Combine(project.Path, "ProjectSettings", "ProjectVersion.txt");
            if (!File.Exists(versionPath)) return false;

            try
            {
                var lines = await File.ReadAllLinesAsync(versionPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("m_EditorVersion:"))
                    {
                        lines[i] = $"m_EditorVersion: {newVersion}";
                        break;
                    }
                }

                await File.WriteAllLinesAsync(versionPath, lines);
                project.UnityVersion = newVersion;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Version change failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CleanLibraryAsync(UnityProject project)
        {
            var libraryPath = Path.Combine(project.Path, "Library");
            if (!Directory.Exists(libraryPath)) return true; // Already clean or doesn't exist

            try
            {
                // Simple deletion of Library folder
                // Note: On Windows this might fail if Unity is open.
                await Task.Run(() => Directory.Delete(libraryPath, true));
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Clean Library failed: {ex.Message}");
                return false;
            }
        }
    }
}
