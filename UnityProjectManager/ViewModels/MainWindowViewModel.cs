using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    [ObservableProperty]
    private ObservableCollection<UnityProject> _projects;

    [ObservableProperty]
    private UnityProject? _selectedProject;

    [ObservableProperty]
    private bool _isDetailsOpen;

    [ObservableProperty]
    private ObservableCollection<UnityPackage> _packages;

    [ObservableProperty]
    private ObservableCollection<string> _watchFolders;

    public MainWindowViewModel()
    {
        _unityService = new UnityHubService();
        _packageService = new PackageService();
        _watchFolders = new ObservableCollection<string>();
        _packages = new ObservableCollection<UnityPackage>();
        
        // Add a default watch folder for demonstration if it exists
        var defaultDocs = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Unity Projects");
        if (System.IO.Directory.Exists(defaultDocs)) _watchFolders.Add(defaultDocs);

        // Keep mock data for UI testing, but clear if you want only real data. 
        // For now, I'll keep mock data to ensure the "Netflix" look is visible immediately.
        Projects = new ObservableCollection<UnityProject>
        {
            new UnityProject { Name = "RPG Adventure", UnityVersion = "2022.3.10f1", VersionType="LTS", LastModified = DateTime.Now.AddDays(-2) },
            new UnityProject { Name = "Sci-Fi Shooter", UnityVersion = "2023.1.0b5", VersionType="Beta", LastModified = DateTime.Now.AddDays(-10) },
        };

        // Trigger scan
        LoadProjectsCommand.Execute(null);
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
        // Fallback for mock projects that don't satisfy File.Exists
        if (!System.IO.Directory.Exists(SelectedProject.Path)) 
        {
             // Mock packages for mock projects
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
        // Offload to background thread to prevent UI freeze during heavy IO
        var realProjects = await Task.Run(() => _unityService.ScanWatchFoldersAsync(WatchFolders));
        
        foreach (var project in realProjects)
        {
            // Avoid duplicates based on path
            if (!Projects.Any(p => p.Path == project.Path))
            {
                Projects.Add(project);
            }
        }
        
        // 2. Scan Editors (Just logging or storing for now)
        var editors = await Task.Run(() => _unityService.GetInstalledEditorsAsync());
        // TODO: Store these editors in a list for the "Version Management" feature
    }

    [RelayCommand]
    private void AddWatchFolder(string path)
    {
        if (!string.IsNullOrWhiteSpace(path) && System.IO.Directory.Exists(path) && !WatchFolders.Contains(path))
        {
            WatchFolders.Add(path);
            LoadProjectsCommand.Execute(null);
        }
    }
}
