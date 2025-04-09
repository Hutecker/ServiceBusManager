using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ServiceBusManager.Models;
using ServiceBusManager.Models.Constants;
using ServiceBusManager.Services;
using System.Diagnostics;

namespace ServiceBusManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly ObservableCollection<LogItem> _sortedLogs = new();

    [ObservableProperty]
    private ServiceBusResourceItem selectedResource;

    [ObservableProperty]
    private DetailsViewModel detailsViewModel;

    [ObservableProperty]
    private ExplorerViewModel explorerViewModel;

    // Expose sorted Logs from the Logging Service
    public ObservableCollection<LogItem> Logs => _sortedLogs;

    [ObservableProperty]
    private bool isLogsVisible = true;
    
    [ObservableProperty]
    private int selectedTabIndex = 0;
    
    [ObservableProperty]
    private string[] tabNames = new[] { "Overview", "Messages", "Properties" };

    [ObservableProperty]
    private string themeIcon = FontAwesomeIcons.Sun;

    // Inject services
    public MainViewModel(ILoggingService loggingService, DetailsViewModel detailsViewModel, ExplorerViewModel explorerViewModel)
    {
        _loggingService = loggingService;
        DetailsViewModel = detailsViewModel;
        ExplorerViewModel = explorerViewModel;

        Debug.WriteLine("MainViewModel constructor");
        Debug.WriteLine($"DetailsViewModel: {detailsViewModel != null}");
        Debug.WriteLine($"ExplorerViewModel: {explorerViewModel != null}");

        // Subscribe to resource selection in ExplorerViewModel
        ExplorerViewModel.ResourceSelected += OnResourceSelected;
        Debug.WriteLine("Subscribed to ExplorerViewModel.ResourceSelected");

        // Subscribe to log changes
        _loggingService.Logs.CollectionChanged += (s, e) =>
        {
            _sortedLogs.Clear();
            foreach (var log in _loggingService.Logs.OrderByDescending(l => l.Timestamp))
            {
                _sortedLogs.Add(log);
            }
        };

        // Initialize theme icon based on current theme
        UpdateThemeIcon();
                
        // Add initial log via service
        _loggingService.AddLog("Application started");
        Debug.WriteLine("Application started");
    }

    private void OnResourceSelected(ServiceBusResourceItem resource)
    {
        Debug.WriteLine($"MainViewModel.OnResourceSelected: {resource?.Name}");
        SelectedResource = resource;
        DetailsViewModel.SelectResource(resource);
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        var currentTheme = Application.Current.RequestedTheme;
        var newTheme = currentTheme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;
        Application.Current.UserAppTheme = newTheme;
        UpdateThemeIcon();
        _loggingService.AddLog($"Theme switched to {newTheme}");
    }

    private void UpdateThemeIcon()
    {
        ThemeIcon = Application.Current.RequestedTheme == AppTheme.Dark ? FontAwesomeIcons.Sun : FontAwesomeIcons.Moon;
    }

    [RelayCommand]
    private void ClearLogs()
    {
        // Use the service
        _loggingService.ClearLogs();
    }

    [RelayCommand]
    private void ToggleLogs()
    {
        IsLogsVisible = !IsLogsVisible;
        // Log using the service after toggling state
        _loggingService.AddLog($"Logs panel {(IsLogsVisible ? "shown" : "hidden")}");
    }
    
    [RelayCommand]
    private void SelectTab(int index)
    {
        if (index >= 0 && index < TabNames.Length)
        {
            SelectedTabIndex = index;
            _loggingService.AddLog($"Selected tab: {TabNames[index]}");
        }
    }
}
