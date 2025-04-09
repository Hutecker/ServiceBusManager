using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceBusManager.ViewModels;

public partial class ExplorerViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IServiceBusService _serviceBusService;
    private readonly ConnectionModalViewModel _connectionModalViewModel;

    [ObservableProperty]
    private ObservableCollection<ServiceBusResourceItem> resources = new();

    [ObservableProperty]
    private ServiceBusResourceItem selectedResource;

    public event Action<ServiceBusResourceItem> ResourceSelected;

    public ExplorerViewModel(
        ILoggingService loggingService, 
        IServiceBusService serviceBusService,
        ConnectionModalViewModel connectionModalViewModel)
    {
        _loggingService = loggingService;
        _serviceBusService = serviceBusService;
        _connectionModalViewModel = connectionModalViewModel;
        
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
            
            // Add diagnostic logging
            Debug.WriteLine($"Loaded {fetchedResources.Count()} resources");
            
            foreach (var resource in fetchedResources)
            {
                Debug.WriteLine($"Adding resource: {resource.Name} of type {resource.Type}");
                Resources.Add(resource);
            }
            
            // Verify Resources collection is populated
            Debug.WriteLine($"Resources collection now has {Resources.Count} items");
        }
        catch (Exception ex)
        { 
            // More detailed error logging
            Debug.WriteLine($"ERROR loading resources: {ex.Message}\n{ex.StackTrace}");
            _loggingService.AddLog($"ERROR loading resources: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SelectResource(ServiceBusResourceItem resource)
    {
        if (resource == null) return;
        
        // Add diagnostic logging
        Debug.WriteLine($"Selected resource: {resource.Name} of type {resource.Type}");
        
        SelectedResource = resource;
        ResourceSelected?.Invoke(resource);
        _loggingService.AddLog($"Selected {resource.Type} '{resource.Name}'");
    }
    
    [RelayCommand]
    private void ShowConnectionDialog()
    {
        Debug.WriteLine("Showing connection dialog");
        _loggingService.AddLog("Opening connection string dialog");
        _connectionModalViewModel.Show();
        
        // Subscribe to dialog closed event to refresh resources
        _connectionModalViewModel.DialogClosed += OnConnectionDialogClosed;
    }
    
    private void OnConnectionDialogClosed()
    {
        // Unsubscribe to avoid memory leaks
        _connectionModalViewModel.DialogClosed -= OnConnectionDialogClosed;
        
        // Reload resources when connection string changes
        LoadResourcesCommand.Execute(null);
    }
} 