using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityProjectManager.Models;

namespace UnityProjectManager.Services
{
    public class GitService
    {
        public async Task UpdateGitInfoAsync(UnityProject project)
        {
            if (string.IsNullOrEmpty(project.Path)) return;

            try
            {
                // Check if it's a git repo
                var isGit = await RunGitCommandAsync(project.Path, "rev-parse --is-inside-work-tree");
                if (isGit.Trim() != "true")
                {
                    project.IsGitRepo = false;
                    return;
                }

                project.IsGitRepo = true;

                // Get Branch Name
                var branch = await RunGitCommandAsync(project.Path, "rev-parse --abbrev-ref HEAD");
                project.GitBranch = branch.Trim();

                // Get Status (changes count)
                var status = await RunGitCommandAsync(project.Path, "status --porcelain");
                var changes = status.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                project.HasGitChanges = changes.Length > 0;
                project.GitStatus = changes.Length == 0 ? "Clean" : $"{changes.Length} file{(changes.Length == 1 ? "" : "s")} changed";
            }
            catch (Exception)
            {
                project.IsGitRepo = false;
            }
        }

        private async Task<string> RunGitCommandAsync(string workingDir, string arguments)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = arguments,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                // Allow non-zero exit for things like 'status' which might behave differently, 
                // but for our purposes, if it crashes or errors, output is usually empty or error msg.
                // However, rev-parse returns non-zero for non-repo.
                if (process.ExitCode != 0) return ""; 

                return output;
            }
            catch
            {
                return "";
            }
        }
    }
}
