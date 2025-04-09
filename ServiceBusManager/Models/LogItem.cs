using System;

namespace ServiceBusManager.Models;

public class LogItem
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }
    public string FormattedLog => $"[{Timestamp:HH:mm:ss}] {Message}";
} 