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

    [ObservableProperty]
    private ObservableCollection<UnityProject> _projects;

    [ObservableProperty]
    private UnityProject? _selectedProject;

    [ObservableProperty]
    private string _selectedTab = "Projects";

    [ObservableProperty]
    private bool _isDetailsOpen;

    [ObservableProperty]
    private ObservableCollection<UnityPackage> _packages;

    [ObservableProperty]
    private ObservableCollection<string> _watchFolders;

    [ObservableProperty]
    private ObservableCollection<string> _installedEditors;

    [ObservableProperty]
    private ObservableCollection<LearnContent> _learnContents;

    [ObservableProperty]
    private string _learnSearchQuery = "";

    [ObservableProperty]
    private bool _isLoadingLearn;

    [ObservableProperty]
    private string _selectedDocsUrl = "https://docs.unity3d.com/Manual/index.html";

    public MainWindowViewModel()
    {
        _unityService = new UnityHubService();
        _packageService = new PackageService();
        _configService = new ConfigService();
        _editorService = new ProjectEditorService();

        _watchFolders = new ObservableCollection<string>();
        _packages = new ObservableCollection<UnityPackage>();
        _projects = new ObservableCollection<UnityProject>();
        _installedEditors = new ObservableCollection<string>();
        _learnContents = new ObservableCollection<LearnContent>();
        _learnService = new LearnService();
        
        // Initial Load
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        // 1. Load Config
        var config = await _configService.LoadConfigAsync();
        SelectedTab = config.SelectedTab;
        SelectedDocsUrl = config.LastDocsUrl;
        
        foreach (var folder in config.WatchFolders)
        {
            if (System.IO.Directory.Exists(folder))
                WatchFolders.Add(folder);
        }

        // Add default if empty
        if (WatchFolders.Count == 0)
        {
            var defaultDocs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Unity Projects");
            if (System.IO.Directory.Exists(defaultDocs)) AddWatchFolder(defaultDocs);
        }

        // 2. Load Mock Data for visuals if no real projects found initially
        if (Projects.Count == 0)
        {
            Projects.Add(new UnityProject { Name = "RPG Adventure (Mock)", UnityVersion = "2022.3.10f1", VersionType="LTS", LastModified = DateTime.Now.AddDays(-2) });
            Projects.Add(new UnityProject { Name = "Sci-Fi Shooter (Mock)", UnityVersion = "2023.1.0b5", VersionType="Beta", LastModified = DateTime.Now.AddDays(-10) });
        }

        // 3. Trigger Scan
        await LoadProjectsAsync();
        
        // 4. Load initial Learn content
        await SearchLearnContent();
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
        // 1. Scan Watch Folders
        var realProjects = await Task.Run(() => _unityService.ScanWatchFoldersAsync(WatchFolders));
        MergeProjects(realProjects);

        // 2. Sync with Hub
        var hubProjects = await _unityService.GetHubProjectsAsync();
        MergeProjects(hubProjects);

        // 3. Load Installs
        var editors = await _unityService.GetInstalledEditorsAsync();
        InstalledEditors.Clear();
        foreach (var editor in editors) InstalledEditors.Add(editor);
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

    private async void SaveConfig()
    {
        var config = new AppConfig 
        { 
            WatchFolders = WatchFolders.ToList(),
            SelectedTab = SelectedTab,
            LastDocsUrl = SelectedDocsUrl
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

        var editorPath = await _unityService.GetEditorPathForVersionAsync(SelectedProject.UnityVersion);
        
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
            LearnContents.Clear();
            foreach (var item in results)
            {
                LearnContents.Add(item);
            }
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
}
