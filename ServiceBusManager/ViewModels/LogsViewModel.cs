using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ServiceBusManager.ViewModels;

public partial class LogsViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly ObservableCollection<LogItem> _sortedLogs = new();

    // Expose sorted Logs from the Logging Service
    public ObservableCollection<LogItem> Logs => _sortedLogs;

    [ObservableProperty]
    private bool isLogsVisible = true;

    public LogsViewModel(ILoggingService loggingService)
    {
        _loggingService = loggingService;
        
        // Subscribe to log changes
        _loggingService.Logs.CollectionChanged += (s, e) =>
        {
            _sortedLogs.Clear();
            foreach (var log in _loggingService.Logs.OrderByDescending(l => l.Timestamp))
            {
                _sortedLogs.Add(log);
            }
        };
        
        Debug.WriteLine("LogsViewModel created");
    }

    [RelayCommand]
    private void ClearLogs()
    {
        // Use the service
        _loggingService.ClearLogs();
        Debug.WriteLine("Logs cleared");
    }

    [RelayCommand]
    private void ToggleLogs()
    {
        IsLogsVisible = !IsLogsVisible;
        Debug.WriteLine($"Logs panel {(IsLogsVisible ? "shown" : "hidden")}");
    }
} 