using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System.Collections.ObjectModel;

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
    private string selectedTab = "Properties";

    public ObservableCollection<LogItem> Logs => _loggingService.Logs;

    public DetailsViewModel(ILoggingService loggingService, IServiceBusService serviceBusService)
    {
        _loggingService = loggingService;
        _serviceBusService = serviceBusService;
    }

    [RelayCommand]
    private void SelectTab(string tabName)
    {
        SelectedTab = tabName;
        _loggingService.AddLog($"Selected tab: {tabName}");
    }

    public void SelectResource(ServiceBusResourceItem? resource)
    {
        if (resource == null)
        {
            IsDetailsVisible = false;
            return;
        }

        SelectedResource = resource;
        IsDetailsVisible = true;
        _loggingService.AddLog($"Selected resource: {resource.Name}");
    }
} 