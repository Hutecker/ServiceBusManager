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
    private PeriodicTimer? _refreshTimer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(60);
    private DateTime _lastRefreshTime;
    private Task? _progressUpdateTask;

    [ObservableProperty]
    private ObservableCollection<ServiceBusResourceItem> resources = new();

    [ObservableProperty]
    private ServiceBusResourceItem? selectedResource;

    [ObservableProperty]
    private bool isRefreshing;

    [ObservableProperty]
    private double refreshProgress;

    [ObservableProperty]
    private bool isConnected;

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
    
    private void StartTimers()
    {
        if (_refreshTimer != null) return; // Already started

        _lastRefreshTime = DateTime.Now;
        
        // Initialize refresh timer
        _refreshTimer = new PeriodicTimer(_refreshInterval);
        StartPeriodicRefresh();
        
        // Start progress update timer
        StartProgressUpdateTimer();
    }
    
    private void UpdateConnectionState()
    {
        IsConnected = _serviceBusService.IsConnected;
    }
    
    private void StartProgressUpdateTimer()
    {
        _progressUpdateTask = Task.Run(async () =>
        {
            try
            {
                while (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(100, _cancellationTokenSource.Token);
                    if (IsConnected)
                    {
                        var elapsed = DateTime.Now - _lastRefreshTime;
                        var newProgress = Math.Min(1.0, elapsed.TotalMilliseconds / _refreshInterval.TotalMilliseconds);
                        
                        // Only update if there's a meaningful change
                        if (Math.Abs(newProgress - RefreshProgress) > 0.001)
                        {
                            RefreshProgress = newProgress;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Timer was cancelled, this is expected
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in progress update timer: {ex.Message}");
            }
        }, _cancellationTokenSource.Token);
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
        
        Debug.WriteLine("Starting refresh...");
        IsRefreshing = true;
        try
        {
            foreach (var resource in Resources)
            {
                Debug.WriteLine($"Processing resource: {resource.Name} of type {resource.Type}");
                
                if (resource.Type == ResourceType.Queue)
                {
                    Debug.WriteLine($"Fetching counts for queue: {resource.Name}");
                    var counts = await _serviceBusService.GetMessageCountsAsync(resource.Name);
                    Debug.WriteLine($"Got counts for {resource.Name} - Active: {counts.Active}, Dead: {counts.DeadLetter}, Scheduled: {counts.Scheduled}");
                    
                    // Set all counts at once to trigger a single update
                    resource.ActiveMessageCount = counts.Active;
                    resource.DeadLetterMessageCount = counts.DeadLetter;
                    resource.ScheduledMessageCount = counts.Scheduled;
                    resource.UpdateMessageCountDisplay(); // Force an update
                    
                    Debug.WriteLine($"Updated counts for {resource.Name} - MessageCountDisplay: {resource.MessageCountDisplay}");
                }
                else if (resource.Type == ResourceType.Topic)
                {
                    Debug.WriteLine($"Processing topic: {resource.Name} with {resource.Children.Count} subscriptions");
                    foreach (var subscription in resource.Children)
                    {
                        Debug.WriteLine($"Fetching counts for subscription: {subscription.Name}");
                        var counts = await _serviceBusService.GetMessageCountsAsync($"{resource.Name}/{subscription.Name}");
                        Debug.WriteLine($"Got counts for {subscription.Name} - Active: {counts.Active}, Dead: {counts.DeadLetter}, Transfer: {counts.Scheduled}");
                        
                        // Set all counts at once to trigger a single update
                        subscription.ActiveMessageCount = counts.Active;
                        subscription.DeadLetterMessageCount = counts.DeadLetter;
                        subscription.ScheduledMessageCount = counts.Scheduled;
                        subscription.UpdateMessageCountDisplay(); // Force an update
                        
                        Debug.WriteLine($"Updated counts for subscription {subscription.Name} - MessageCountDisplay: {subscription.MessageCountDisplay}");
                    }
                }
            }
            
            Debug.WriteLine("Refresh completed, resetting progress...");
            _lastRefreshTime = DateTime.Now;
            RefreshProgress = 0;
            _loggingService.AddLog("Message counts refreshed");
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

            // Update connection state and start timers after successful load
            UpdateConnectionState();
            if (IsConnected)
            {
                StartTimers();
            }
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
        if (_refreshTimer != null)
        {
            _refreshTimer.Dispose();
            _refreshTimer = null;
        }
        
        // Wait for the progress update task to complete
        _progressUpdateTask?.Wait();
        _progressUpdateTask = null;
        
        _cancellationTokenSource.Dispose();
    }
} 