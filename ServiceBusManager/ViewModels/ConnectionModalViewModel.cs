using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace ServiceBusManager.ViewModels;

public partial class ConnectionModalViewModel : ObservableObject
{
    private readonly IServiceBusService _serviceBusService;
    private readonly ILoggingService _loggingService;
    
    [ObservableProperty]
    private bool isVisible;
    
    [ObservableProperty]
    private string connectionString = string.Empty;
    
    public event Action<bool>? DialogClosed;
    
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
    
    private void Hide(bool wasSaved)
    {
        IsVisible = false;
        DialogClosed?.Invoke(wasSaved);
    }
    
    [RelayCommand]
    private async Task SaveAsync()
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
            _loggingService.AddLog("Connecting...");
            Hide(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving connection string: {ex.Message}");
            _loggingService.AddLog($"Error saving connection string: {ex.Message}");

            if (Application.Current?.Windows?.FirstOrDefault()?.Page is Page page)
            {
                await page.DisplayAlert("Error", $"Failed to save connection string: {ex.Message}", "OK");
            }
        }
    }
    
    [RelayCommand]
    private void Cancel()
    {
        Hide(false);
    }
} 