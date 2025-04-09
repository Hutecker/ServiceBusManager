using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ServiceBusManager.ViewModels;

public partial class ConnectionModalViewModel : ObservableObject
{
    private readonly IServiceBusService _serviceBusService;
    private readonly ILoggingService _loggingService;
    
    [ObservableProperty]
    private bool isVisible;
    
    [ObservableProperty]
    private string connectionString = string.Empty;
    
    public event Action DialogClosed;
    
    public ConnectionModalViewModel(IServiceBusService serviceBusService, ILoggingService loggingService)
    {
        _serviceBusService = serviceBusService;
        _loggingService = loggingService;
    }
    
    public void Show()
    {
        ConnectionString = _serviceBusService.GetConnectionString();
        IsVisible = true;
    }
    
    public void Hide()
    {
        IsVisible = false;
        DialogClosed?.Invoke();
    }
    
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Connection string cannot be empty", "OK");
                return;
            }
            
            await _serviceBusService.SetConnectionStringAsync(ConnectionString);
            _loggingService.AddLog("Connection string saved");
            Hide();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving connection string: {ex.Message}");
            _loggingService.AddLog($"Error saving connection string: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save connection string: {ex.Message}", "OK");
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Hide();
    }
} 