using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using ServiceBusManager.Models;
using ServiceBusManager.Models.Constants;
using ServiceBusManager.Services;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Linq;

namespace ServiceBusManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceBusService _serviceBusService;
    private readonly ILoggingService _loggingService;
    private readonly ObservableCollection<LogItem> _sortedLogs = new();

    [ObservableProperty]
    private ObservableCollection<ServiceBusResourceItem> resources = new();

    [ObservableProperty]
    private ServiceBusResourceItem selectedResource;

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
    public MainViewModel(IServiceBusService serviceBusService, ILoggingService loggingService)
    {
        _serviceBusService = serviceBusService;
        _loggingService = loggingService;

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

        // Load resources asynchronously
        LoadResourcesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadResourcesAsync()
    {
        _loggingService.AddLog("Loading service bus resources...");
        try
        {
            var fetchedResources = await _serviceBusService.GetResourcesAsync();
            Resources.Clear();
            foreach (var resource in fetchedResources)
            {
                Resources.Add(resource);
            }
            _loggingService.AddLog($"Loaded {Resources.Count} resources.");
        }
        catch (Exception ex)
        { 
            // Handle exceptions appropriately (e.g., show error message)
            _loggingService.AddLog($"Error loading resources: {ex.Message}");
        }
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
    private void SelectResource(ServiceBusResourceItem resource)
    {
        if (resource == null) return;
        SelectedResource = resource;
        _loggingService.AddLog($"Selected {resource.Type} '{resource.Name}'");
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
