using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityProjectManager.Models;
using UnityProjectManager.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace UnityProjectManager.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IUnityHubService _unityService;
    private readonly IPackageService _packageService;
    private readonly IConfigService _configService;
    private readonly IProjectEditorService _editorService;
    private readonly ILearnService _learnService;
    private readonly IInspirationService _inspirationService;
    private readonly GitService _gitService; // Using concrete class for now

    [ObservableProperty]
    private ObservableCollection<UnityProject> _projects;

    [ObservableProperty]
    private UnityProject? _selectedProject;
// ... (skip unchanged lines) ...


    [ObservableProperty]
    private string _selectedTab = "Projects";

    [ObservableProperty]
    private bool _isDetailsOpen;

    [ObservableProperty]
    private ObservableCollection<UnityPackage> _packages;

    [ObservableProperty]
    private ObservableCollection<string> _watchFolders;

    [ObservableProperty]
    private ObservableCollection<string> _unityInstallPaths;

    [ObservableProperty]
    private ObservableCollection<string> _installedEditors;

    [ObservableProperty]
    private ObservableCollection<InspirationItem> _inspirationItems;

    [ObservableProperty]
    private bool _isLoadingInspiration;

    [ObservableProperty]
    private ObservableCollection<LearnContent> _learnContents;

    [ObservableProperty]
    private string _learnSearchQuery = "";

    [ObservableProperty]
    private bool _isLoadingLearn;

    [ObservableProperty]
    private string _selectedDocsUrl = "https://docs.unity3d.com/Manual/index.html";

    [ObservableProperty]
    private ObservableCollection<ProjectBoard> _boards;

    [ObservableProperty]
    private ProjectBoard? _selectedBoard;

    [ObservableProperty]
    private ObservableCollection<BoardTask> _todoTasks = new();

    [ObservableProperty]
    private ObservableCollection<BoardTask> _doingTasks = new();

    [ObservableProperty]
    private ObservableCollection<BoardTask> _doneTasks = new();

    [ObservableProperty]
    private ObservableCollection<string> _debugLogs = new ObservableCollection<string>();

    [ObservableProperty]
    private string _fullLogText = "";

    private void Log(string message)
    {
        var msg = $"[{DateTime.Now:HH:mm:ss}] {message}";
        Console.WriteLine(msg);
        Avalonia.Threading.Dispatcher.UIThread.Post(() => 
        {
            DebugLogs.Add(msg);
            FullLogText += msg + "\n";
        });
    }

    public MainWindowViewModel()
    {
        _unityService = new UnityHubService();
        _packageService = new PackageService();
        _configService = new ConfigService();
        _editorService = new ProjectEditorService();
        _inspirationService = new InspirationService();
        _gitService = new GitService();

        _watchFolders = new ObservableCollection<string>();
        _unityInstallPaths = new ObservableCollection<string>();
        _inspirationItems = new ObservableCollection<InspirationItem>();
        _packages = new ObservableCollection<UnityPackage>();
        _projects = new ObservableCollection<UnityProject>();
        _installedEditors = new ObservableCollection<string>();
        _learnContents = new ObservableCollection<LearnContent>();
        _boards = new ObservableCollection<ProjectBoard>();
        _learnService = new LearnService();
        
        // Initial Load
        // InitializeAsync(); // Removed to avoid running before UI is ready. Called from View now or dispatched.
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(InitializeAsync);
    }

    private async void InitializeAsync()
    {
        Log("Initializing Application...");
        try 
        {
            // 1. Load Config
            Log("Loading Config...");
            var config = await _configService.LoadConfigAsync();
            SelectedTab = config.SelectedTab;
            SelectedDocsUrl = config.LastDocsUrl;
            
            Log($"Loaded Config. WatchFolders: {config.WatchFolders.Count}, InstallPaths: {config.UnityInstallPaths.Count}");

            bool isFirstRun = true;

            if (config.WatchFolders.Count > 0)
            {
                isFirstRun = false;
                foreach (var folder in config.WatchFolders)
                {
                    if (System.IO.Directory.Exists(folder))
                    {
                        WatchFolders.Add(folder);
                        Log($"Added Watch Folder: {folder}");
                    }
                    else
                    {
                        Log($"Watch Folder missing: {folder}");
                    }
                }
            }

            if (config.UnityInstallPaths.Count > 0)
            {
                foreach (var folder in config.UnityInstallPaths)
                {
                    if (System.IO.Directory.Exists(folder))
                        UnityInstallPaths.Add(folder);
                }
            }

            if (config.Boards.Count > 0)
            {
                isFirstRun = false;
                foreach (var board in config.Boards)
                {
                    Boards.Add(board);
                }
            }

            // Only add defaults if it's strictly a first run (no folders, no boards)
            if (isFirstRun)
            {
                Log("First Run Detected. Adding defaults.");
                // Default Board
                var defaultBoard = new ProjectBoard { Name = "Main Project Board" };
                defaultBoard.Tasks.Add(new BoardTask { Title = "Setup Unity Project", Description = "Initialize git and project folders", Status = BoardTaskStatus.Todo });
                defaultBoard.Tasks.Add(new BoardTask { Title = "Design System", Description = "Create UI components", Status = BoardTaskStatus.Doing });
                Boards.Add(defaultBoard);

                // Default Docs Folder
                var defaultDocs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Unity Projects");
                if (System.IO.Directory.Exists(defaultDocs)) 
                {
                     WatchFolders.Add(defaultDocs);
                     Log($"Added Default Watch Folder: {defaultDocs}");
                     // We added a folder, so save this state immediately
                     SaveConfig(); 
                }
            }
            else
            {
                 // If we have boards but somehow no selected board, select first
                 if (Boards.Count > 0) SelectedBoard = Boards.FirstOrDefault();
            }

            if (SelectedBoard == null && Boards.Count > 0) SelectedBoard = Boards.FirstOrDefault();
            UpdateBoardColumns();

            // 2. Load Mock Data ONLY if it's first run AND we found no real projects later?
            // Actually, let's just NOT add mocks if we have ever saved state. 
            // If the user has cleared all their projects, they shouldn't see mocks again.
            if (isFirstRun && Projects.Count == 0)
            {
                Log("Adding Mock Projects.");
                Projects.Add(new UnityProject { Name = "RPG Adventure (Mock)", UnityVersion = "2022.3.10f1", VersionType="LTS", LastModified = DateTime.Now.AddDays(-2) });
                Projects.Add(new UnityProject { Name = "Sci-Fi Shooter (Mock)", UnityVersion = "2023.1.0b5", VersionType="Beta", LastModified = DateTime.Now.AddDays(-10) });
            }

            // 3. Trigger Scan
            Log("Triggering Project Scan...");
            await LoadProjectsAsync();
            
            // 4. Load initial Learn content
            Log("Searching Learn Content...");
            await SearchLearnContent();
            
            // 5. Load Inspiration
            Log("Loading Inspiration...");
            await LoadInspirationAsync();
        }
        catch (Exception ex)
        {
            Log($"CRITICAL ERROR IN INIT: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    [RelayCommand]
    private async Task OpenProjectDetails(UnityProject project)
    {
        SelectedProject = project;
        IsDetailsOpen = true;
        await LoadPackagesAsync();
    }

    [RelayCommand]
    private void CloseProjectDetails()
    {
        IsDetailsOpen = false;
        SelectedProject = null;
        Packages.Clear();
    }

    private async Task LoadPackagesAsync()
    {
        if (SelectedProject == null) return;
        
        Packages.Clear();
        if (!System.IO.Directory.Exists(SelectedProject.Path)) 
        {
             Packages.Add(new UnityPackage("com.unity.render-pipelines.universal", "14.0.8"));
             Packages.Add(new UnityPackage("com.unity.textmeshpro", "3.0.6"));
             Packages.Add(new UnityPackage("com.unity.ide.visualstudio", "2.0.20"));
             return;
        }

        var packages = await _packageService.GetPackagesAsync(SelectedProject.Path);
        foreach(var p in packages) Packages.Add(p);
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        Log("Starting LoadProjectsAsync...");
        try
        {
             // 1. Scan Watch Folders
            Log($"Scanning {WatchFolders.Count} watch folders...");
            var realProjects = await Task.Run(() => _unityService.ScanWatchFoldersAsync(WatchFolders));
            
            // 2. Sync with Hub
            Log("Syncing with Unity Hub...");
            var hubProjects = await _unityService.GetHubProjectsAsync();

            // Update Projects on UI Thread
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
            {
                Log($"Merging Projects (Scanned: {realProjects?.Count() ?? 0}, Hub: {hubProjects?.Count() ?? 0})");
                MergeProjects(realProjects ?? Enumerable.Empty<UnityProject>());
                MergeProjects(hubProjects ?? Enumerable.Empty<UnityProject>());
                Log($"Total Projects: {Projects.Count}");
            });

            // 3. Load Installs
            Log($"Scanning for Editors in {UnityInstallPaths.Count} custom paths...");
            var editors = await _unityService.GetInstalledEditorsAsync(UnityInstallPaths);
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
            {
                InstalledEditors.Clear();
                foreach (var editor in editors) InstalledEditors.Add(editor);
                Log($"Found {InstalledEditors.Count} Editors.");
            });

            // 4. Update Git Info (Background)
            // Note: Git info updates modify properties on existing objects, 
            // which notify property changed. This is *usually* ok if bindings listen on UI thread,
            // but let's be safe if it modifies collections.
            // GitService updates properties so it should be fine, but let's see.
            foreach (var project in Projects)
            {
                await _gitService.UpdateGitInfoAsync(project);
            }
        }
        catch (Exception ex)
        {
             Log($"ERROR IN LOAD PROJECTS: {ex.Message}");
        }
    }

    private void MergeProjects(IEnumerable<UnityProject> newProjects)
    {
        foreach (var project in newProjects)
        {
            var existing = Projects.FirstOrDefault(p => p.Path == project.Path);
            if (existing == null)
            {
                Projects.Add(project);
            }
            else
            {
                // Update existing if it was a mock
                if (existing.Name?.Contains("(Mock)") == true)
                {
                     existing.Name = project.Name;
                }
                existing.UnityVersion = project.UnityVersion;
                existing.LastModified = project.LastModified;
                existing.LastAccessTime = project.LastAccessTime;
                existing.VersionType = project.VersionType;
                existing.ThumbnailPath = project.ThumbnailPath;
            }
        }
    }

    [RelayCommand]
    private void SelectTab(string tabName)
    {
        SelectedTab = tabName;
    }

    partial void OnSelectedTabChanged(string value)
    {
        SaveConfig();
    }

    partial void OnSelectedBoardChanged(ProjectBoard? value)
    {
        UpdateBoardColumns();
    }

    private void UpdateBoardColumns()
    {
        TodoTasks.Clear();
        DoingTasks.Clear();
        DoneTasks.Clear();

        if (SelectedBoard == null) return;

        foreach (var task in SelectedBoard.Tasks)
        {
            switch (task.Status)
            {
                case BoardTaskStatus.Todo: TodoTasks.Add(task); break;
                case BoardTaskStatus.Doing: DoingTasks.Add(task); break;
                case BoardTaskStatus.Done: DoneTasks.Add(task); break;
            }
        }
    }

    [RelayCommand]
    private async Task SyncWithHub()
    {
        await LoadProjectsAsync();
    }

    [RelayCommand]
    private void AddWatchFolder(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && System.IO.Directory.Exists(path) && !WatchFolders.Contains(path))
        {
            WatchFolders.Add(path);
            SaveConfig();
            LoadProjectsCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void AddUnityInstallPath(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && System.IO.Directory.Exists(path) && !UnityInstallPaths.Contains(path))
        {
            UnityInstallPaths.Add(path);
            SaveConfig();
            LoadProjectsCommand.Execute(null);
        }
    }

    private async void SaveConfig()
    {
        var config = new AppConfig 
        { 
            WatchFolders = WatchFolders.ToList(),
            UnityInstallPaths = UnityInstallPaths.ToList(),
            SelectedTab = SelectedTab,
            LastDocsUrl = SelectedDocsUrl,
            Boards = Boards.ToList()
        };
        await _configService.SaveConfigAsync(config);
    }

    [RelayCommand]
    private async Task RenameProject(string newName)
    {
        if (SelectedProject == null) return;
        var success = await _editorService.RenameProjectAsync(SelectedProject, newName);
        if (success)
        {
            // Refresh list if needed (already updated in model)
        }
    }

    [RelayCommand]
    private async Task ChangeUnityVersion(string newVersion)
    {
        if (SelectedProject == null) return;
        await _editorService.ChangeUnityVersionAsync(SelectedProject, newVersion);
    }

    [RelayCommand]
    private async Task CleanLibrary()
    {
        if (SelectedProject == null) return;
        await _editorService.CleanLibraryAsync(SelectedProject);
        // Show notification or status?
    }

    [RelayCommand]
    private async Task OpenProject()
    {
        if (SelectedProject == null || string.IsNullOrEmpty(SelectedProject.Path) || string.IsNullOrEmpty(SelectedProject.UnityVersion)) return;

        var editorPath = await _unityService.GetEditorPathForVersionAsync(SelectedProject.UnityVersion, UnityInstallPaths);
        
        if (!string.IsNullOrEmpty(editorPath))
        {
            await _unityService.LaunchProjectAsync(SelectedProject.Path, editorPath);
            CloseProjectDetails(); // Close details after launching
        }
        else
        {
            // Handle case where no suitable editor is found
            // TODO: Show a prompt to manually locate Unity.exe
            Console.WriteLine($"No Unity editor found for version {SelectedProject.UnityVersion}");
        }
    }

    [RelayCommand]
    private async Task SearchLearnContent()
    {
        IsLoadingLearn = true;
        try
        {
            var results = await _learnService.SearchContentAsync(LearnSearchQuery);
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
            {
                LearnContents.Clear();
                foreach (var item in results)
                {
                    LearnContents.Add(item);
                }
            });
        }
        finally
        {
            IsLoadingLearn = false;
        }
    }

    [RelayCommand]
    private void OpenLearnUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to open URL: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task LoadInspirationAsync()
    {
        IsLoadingInspiration = true;
        try
        {
            var items = await _inspirationService.GetInspirationItemsAsync();
            
            Avalonia.Threading.Dispatcher.UIThread.Post(() => 
            {
                InspirationItems.Clear();
                foreach (var item in items) InspirationItems.Add(item);
            });
        }
        finally
        {
            IsLoadingInspiration = false;
        }
    }

    [RelayCommand]
    private void AddBoard(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "New Board";
        var board = new ProjectBoard { Name = name };
        Boards.Add(board);
        SelectedBoard = board;
        SaveConfig();
    }

    [RelayCommand]
    private void AddTask(string title)
    {
        if (SelectedBoard == null || string.IsNullOrWhiteSpace(title)) return;
        var task = new BoardTask { Title = title };
        SelectedBoard.Tasks.Add(task);
        UpdateBoardColumns();
        SaveConfig();
    }

    [RelayCommand]
    private void MoveTask(BoardTask task)
    {
        if (task.Status == BoardTaskStatus.Todo) task.Status = BoardTaskStatus.Doing;
        else if (task.Status == BoardTaskStatus.Doing) task.Status = BoardTaskStatus.Done;
        else task.Status = BoardTaskStatus.Todo;
        
        UpdateBoardColumns();
        SaveConfig();
    }

    [RelayCommand]
    private void DeleteBoard(ProjectBoard board)
    {
        Boards.Remove(board);
        if (SelectedBoard == board) SelectedBoard = Boards.FirstOrDefault();
        SaveConfig();
    }

    [RelayCommand]
    private void DeleteTask(BoardTask task)
    {
        if (SelectedBoard == null) return;
        SelectedBoard.Tasks.Remove(task);
        UpdateBoardColumns();
        SaveConfig();
    }
}
