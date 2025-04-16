using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace ServiceBusManager.ViewModels;

public partial class ConnectionModalViewModel : ObservableObject
{
    private readonly IServiceBusService _serviceBusService;
    private readonly ILoggingService _loggingService;
    private readonly IConnectionStorageService _connectionStorageService;
    
    [ObservableProperty]
    private bool isVisible;
    
    [ObservableProperty]
    private string connectionString = string.Empty;
    
    [ObservableProperty]
    private string connectionName = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<SavedConnection> savedConnections = new();
    
    [ObservableProperty]
    private SavedConnection? selectedConnection;
    
    [ObservableProperty]
    private bool canConnect;
    
    [ObservableProperty]
    private bool canSaveAndConnect;
    
    public event Action<bool>? DialogClosed;
    
    public ConnectionModalViewModel(
        IServiceBusService serviceBusService, 
        ILoggingService loggingService,
        IConnectionStorageService connectionStorageService)
    {
        _serviceBusService = serviceBusService;
        _loggingService = loggingService;
        _connectionStorageService = connectionStorageService;
    }
    
    public async Task Show()
    {
        ConnectionString = string.Empty;
        await LoadSavedConnectionsAsync();
        IsVisible = true;
    }
    
    private void Hide(bool wasConnected)
    {
        IsVisible = false;
        SelectedConnection = null;
        ConnectionName = string.Empty;
        ConnectionString = string.Empty;
        DialogClosed?.Invoke(wasConnected);
    }
    
    [RelayCommand]
    private async Task LoadSavedConnectionsAsync()
    {
        try
        {
            var connections = await _connectionStorageService.GetConnectionsAsync();
            Debug.WriteLine($"Loaded {connections.Count} connections from storage");
            
            SavedConnections.Clear();
            
            if (!connections.Any())
            {
                Debug.WriteLine("No connections found, adding placeholder");
                SavedConnections.Add(new SavedConnection { Name = "No saved connections" });
            }
            else
            {
                foreach (var connection in connections.OrderByDescending(c => c.LastUsedAt))
                {
                    Debug.WriteLine($"Adding connection: {connection.Name}");
                    SavedConnections.Add(connection);
                }
            }
            
            Debug.WriteLine($"Final SavedConnections count: {SavedConnections.Count}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading saved connections: {ex.Message}");
            _loggingService.AddLog($"Error loading saved connections: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task ConnectAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                if (Application.Current?.Windows?.FirstOrDefault()?.Page is Page page)
                {
                    await page.DisplayAlert("Error", "Connection string cannot be empty", "OK");
                }
                return;
            }
            
            await _serviceBusService.SetConnectionStringAsync(ConnectionString);
            _loggingService.AddLog("Connected to Service Bus");
            Hide(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error connecting: {ex.Message}");
            _loggingService.AddLog($"Error connecting: {ex.Message}");

            if (Application.Current?.Windows?.FirstOrDefault()?.Page is Page page)
            {
                await page.DisplayAlert("Error", $"Failed to connect: {ex.Message}", "OK");
            }
        }
    }
    
    [RelayCommand]
    private async Task SaveAndConnectAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                if (Application.Current?.Windows?.FirstOrDefault()?.Page is Page page)
                {
                    await page.DisplayAlert("Error", "Connection string cannot be empty", "OK");
                }
                return;
            }
            
            if (string.IsNullOrWhiteSpace(ConnectionName))
            {
                if (Application.Current?.Windows?.FirstOrDefault()?.Page is Page page)
                {
                    await page.DisplayAlert("Error", "Please provide a name for this connection", "OK");
                }
                return;
            }
            
            var connection = new SavedConnection
            {
                Name = ConnectionName,
                ConnectionString = ConnectionString,
                LastUsedAt = DateTime.UtcNow
            };
            
            await _serviceBusService.SetConnectionStringAsync(ConnectionString);
            await _connectionStorageService.SaveConnectionAsync(connection);
            _loggingService.AddLog($"Saved and connected to: {ConnectionName}");
            Hide(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving and connecting: {ex.Message}");
            _loggingService.AddLog($"Error saving and connecting: {ex.Message}");

            if (Application.Current?.Windows?.FirstOrDefault()?.Page is Page page)
            {
                await page.DisplayAlert("Error", $"Failed to save and connect: {ex.Message}", "OK");
            }
        }
    }
    
    [RelayCommand]
    private void SelectConnection()
    {
        if (SelectedConnection == null || SelectedConnection.Name == "No saved connections") return;
        
        try
        {
            ConnectionName = SelectedConnection.Name;
            ConnectionString = SelectedConnection.ConnectionString;
            _connectionStorageService.UpdateConnectionLastUsedAsync(SelectedConnection.Name).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error selecting connection: {ex.Message}");
            _loggingService.AddLog($"Error selecting connection: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private async Task DeleteConnectionAsync()
    {
        if (SelectedConnection == null || SelectedConnection.Name == "No saved connections") return;
        
        try
        {
            await _connectionStorageService.DeleteConnectionAsync(SelectedConnection.Name);
            await LoadSavedConnectionsAsync();
            SelectedConnection = null;
            ConnectionName = string.Empty;
            ConnectionString = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deleting connection: {ex.Message}");
            _loggingService.AddLog($"Error deleting connection: {ex.Message}");
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Hide(false);
    }
    
    partial void OnConnectionStringChanged(string value)
    {
        UpdateButtonStates();
    }
    
    partial void OnConnectionNameChanged(string value)
    {
        UpdateButtonStates();
    }
    
    private void UpdateButtonStates()
    {
        CanConnect = !string.IsNullOrWhiteSpace(ConnectionString);
        CanSaveAndConnect = !string.IsNullOrWhiteSpace(ConnectionString) && !string.IsNullOrWhiteSpace(ConnectionName);
    }
} 