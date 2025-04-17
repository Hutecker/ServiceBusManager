using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Models.Enums;
using ServiceBusManager.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ServiceBusManager.ViewModels;

public partial class ExplorerViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IServiceBusService _serviceBusService;
    private readonly ConnectionModalViewModel _connectionModalViewModel;
    private readonly PeriodicTimer _refreshTimer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    [ObservableProperty]
    private ObservableCollection<ServiceBusResourceItem> resources = new();

    [ObservableProperty]
    private ServiceBusResourceItem? selectedResource;

    [ObservableProperty]
    private bool isRefreshing;

    public event Action<ServiceBusResourceItem> ResourceSelected;

    public ExplorerViewModel(
        ILoggingService loggingService, 
        IServiceBusService serviceBusService,
        ConnectionModalViewModel connectionModalViewModel)
    {
        _loggingService = loggingService;
        _serviceBusService = serviceBusService;
        _connectionModalViewModel = connectionModalViewModel;
        
        // Initialize refresh timer (every 60 seconds)
        _refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        StartPeriodicRefresh();
        
        // Load resources when created
        LoadResourcesCommand.Execute(null);
    }
    
    private async void StartPeriodicRefresh()
    {
        try
        {
            while (await _refreshTimer.WaitForNextTickAsync(_cancellationTokenSource.Token))
            {
                if (_serviceBusService.IsConnected)
                {
                    await RefreshMessageCountsAsync();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timer was cancelled, this is expected
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in periodic refresh: {ex.Message}");
            _loggingService.AddLog($"Error in periodic refresh: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task RefreshMessageCountsAsync()
    {
        if (!_serviceBusService.IsConnected || IsRefreshing) return;
        
        IsRefreshing = true;
        try
        {
            foreach (var resource in Resources)
            {
                if (resource.Type == ResourceType.Queue)
                {
                    var counts = await _serviceBusService.GetMessageCountsAsync(resource.Name);
                    resource.ActiveMessageCount = counts.Active;
                    resource.DeadLetterMessageCount = counts.DeadLetter;
                    resource.ScheduledMessageCount = counts.Scheduled;
                }
                else if (resource.Type == ResourceType.Topic)
                {
                    foreach (var subscription in resource.Children)
                    {
                        var counts = await _serviceBusService.GetMessageCountsAsync($"{resource.Name}/{subscription.Name}");
                        subscription.ActiveMessageCount = counts.Active;
                        subscription.DeadLetterMessageCount = counts.DeadLetter;
                        subscription.ScheduledMessageCount = counts.Scheduled;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error refreshing message counts: {ex.Message}");
            _loggingService.AddLog($"Error refreshing message counts: {ex.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }
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
            
            // Initial refresh of message counts
            await RefreshMessageCountsAsync();
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
        if (resource == null)
        {
            Debug.WriteLine("SelectResource called with null resource");
            return;
        }
        
        Debug.WriteLine($"SelectResource called for: {resource.Name} of type {resource.Type}");
        Debug.WriteLine($"Current SelectedResource: {SelectedResource?.Name}");
        
        SelectedResource = resource;
        Debug.WriteLine($"SelectedResource set to: {SelectedResource?.Name}");
        
        if (ResourceSelected == null)
        {
            Debug.WriteLine("ResourceSelected event has no subscribers!");
        }
        else
        {
            Debug.WriteLine("Raising ResourceSelected event");
            ResourceSelected?.Invoke(resource);
        }
        
        _loggingService.AddLog($"Selected {resource.Type} '{resource.Name}'");
    }
    
    [RelayCommand]
    private async Task ShowConnectionDialog()
    {
        Debug.WriteLine("Showing connection dialog");
        await _connectionModalViewModel.Show();
        
        // Subscribe to dialog closed event to refresh resources
        _connectionModalViewModel.DialogClosed += OnConnectionDialogClosed;
    }
    
    private void OnConnectionDialogClosed(bool wasSaved)
    {
        // Unsubscribe to avoid memory leaks
        _connectionModalViewModel.DialogClosed -= OnConnectionDialogClosed;
        
        // Only reload resources if the connection string was saved
        if (wasSaved)
        {
            LoadResourcesCommand.Execute(null);
        }
    }
    
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _refreshTimer.Dispose();
        _cancellationTokenSource.Dispose();
    }
} 