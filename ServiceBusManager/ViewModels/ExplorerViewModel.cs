using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ServiceBusManager.ViewModels;

public partial class ExplorerViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IServiceBusService _serviceBusService;

    [ObservableProperty]
    private ObservableCollection<ServiceBusResourceItem> resources = new();

    [ObservableProperty]
    private ServiceBusResourceItem selectedResource;

    public event Action<ServiceBusResourceItem> ResourceSelected;

    public ExplorerViewModel(ILoggingService loggingService, IServiceBusService serviceBusService)
    {
        _loggingService = loggingService;
        _serviceBusService = serviceBusService;
        
        // Load resources when created
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
            
            Debug.WriteLine($"Loaded {fetchedResources.Count()} resources");
            
            foreach (var resource in fetchedResources)
            {
                Debug.WriteLine($"Adding resource: {resource.Name} of type {resource.Type}");
                Resources.Add(resource);
            }
            
            Debug.WriteLine($"Resources collection now has {Resources.Count} items");
        }
        catch (Exception ex)
        { 
            Debug.WriteLine($"ERROR loading resources: {ex.Message}\n{ex.StackTrace}");
            _loggingService.AddLog($"ERROR loading resources: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SelectResource(ServiceBusResourceItem resource)
    {
        if (resource == null) return;
        
        Debug.WriteLine($"Selected resource: {resource.Name} of type {resource.Type}");
        
        SelectedResource = resource;
        ResourceSelected?.Invoke(resource);
    }
} 