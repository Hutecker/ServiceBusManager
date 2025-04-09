using ServiceBusManager.Models;
using System.Collections.ObjectModel;

namespace ServiceBusManager.Services;

public interface ILoggingService
{
    ObservableCollection<LogItem> Logs { get; }
    void AddLog(string message);
    void ClearLogs();
} 