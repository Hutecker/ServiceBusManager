using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ServiceBusManager.ViewModels;

public partial class DetailsViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IServiceBusService _serviceBusService;

    [ObservableProperty]
    private ServiceBusResourceItem? selectedResource;

    [ObservableProperty]
    private bool isDetailsVisible;

    [ObservableProperty]
    private string selectedTab = "Overview";

    public ObservableCollection<LogItem> Logs => _loggingService.Logs;

    public DetailsViewModel(ILoggingService loggingService, IServiceBusService serviceBusService)
    {
        _loggingService = loggingService;
        _serviceBusService = serviceBusService;
        Debug.WriteLine("DetailsViewModel created");
    }

    [RelayCommand]
    private void SelectTab(string tabName)
    {
        Debug.WriteLine($"Selecting tab: {tabName}");
        SelectedTab = tabName;
    }

    public void SelectResource(ServiceBusResourceItem? resource)
    {
        Debug.WriteLine($"DetailsViewModel.SelectResource: {resource?.Name}");
        
        if (resource == null)
        {
            IsDetailsVisible = false;
            SelectedResource = null;
            return;
        }

        SelectedResource = resource;
        IsDetailsVisible = true;
    }
} 