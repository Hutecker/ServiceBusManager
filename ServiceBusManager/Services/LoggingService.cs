using ServiceBusManager.Models;
using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel;
using System;

namespace ServiceBusManager.Services;

public class LoggingService : ILoggingService
{
    // The service now owns the log collection
    public ObservableCollection<LogItem> Logs { get; } = new();

    public void AddLog(string message)
    {
        // Ensure UI updates happen on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Logs.Add(new LogItem
            {
                Timestamp = DateTime.Now,
                Message = message
            });
        });
    }

    public void ClearLogs()
    {
        // Ensure UI updates happen on the main thread
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Logs.Clear();
        });
        
        AddLog("Log entries cleared.");
    }
} 