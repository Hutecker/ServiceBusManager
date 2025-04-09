using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Models.Constants;
using ServiceBusManager.Services;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;

namespace ServiceBusManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;

    [ObservableProperty]
    private ServiceBusResourceItem selectedResource;

    [ObservableProperty]
    private DetailsViewModel detailsViewModel;

    [ObservableProperty]
    private ExplorerViewModel explorerViewModel;
    
    [ObservableProperty]
    private LogsViewModel logsViewModel;
    
    [ObservableProperty]
    private ConnectionModalViewModel connectionModalViewModel;

    [ObservableProperty]
    private int selectedTabIndex = 0;
    
    [ObservableProperty]
    private string[] tabNames = new[] { "Overview", "Messages", "Properties" };

    [ObservableProperty]
    private string themeIcon = FontAwesomeIcons.Sun;

    // Inject services
    public MainViewModel(
        ILoggingService loggingService, 
        DetailsViewModel detailsViewModel, 
        ExplorerViewModel explorerViewModel,
        LogsViewModel logsViewModel,
        ConnectionModalViewModel connectionModalViewModel)
    {
        _loggingService = loggingService;
        DetailsViewModel = detailsViewModel;
        ExplorerViewModel = explorerViewModel;
        LogsViewModel = logsViewModel;
        ConnectionModalViewModel = connectionModalViewModel;

        Debug.WriteLine("MainViewModel constructor");
        Debug.WriteLine($"DetailsViewModel: {detailsViewModel != null}");
        Debug.WriteLine($"ExplorerViewModel: {explorerViewModel != null}");
        Debug.WriteLine($"LogsViewModel: {logsViewModel != null}");
        Debug.WriteLine($"ConnectionModalViewModel: {connectionModalViewModel != null}");

        // Subscribe to resource selection in ExplorerViewModel
        ExplorerViewModel.ResourceSelected += OnResourceSelected;
        Debug.WriteLine("Subscribed to ExplorerViewModel.ResourceSelected");

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
    private void SelectTab(int index)
    {
        if (index >= 0 && index < TabNames.Length)
        {
            SelectedTabIndex = index;
            Debug.WriteLine($"Selected tab: {TabNames[index]}");
        }
    }
}
