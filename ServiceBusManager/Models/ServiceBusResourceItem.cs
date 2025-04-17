using ServiceBusManager.Models.Enums;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ServiceBusManager.Models;

public partial class ServiceBusResourceItem : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private ResourceType type;

    [ObservableProperty]
    private string parent;

    [ObservableProperty]
    private ObservableCollection<ServiceBusResourceItem> children = new();

    [ObservableProperty]
    private string icon = "\uf1b2"; // Font Awesome bus icon

    [ObservableProperty]
    private bool isSelected;
    
    // Message count properties
    [ObservableProperty]
    private long activeMessageCount;

    [ObservableProperty]
    private long deadLetterMessageCount;

    [ObservableProperty]
    private long scheduledMessageCount;
    
    // Formatted message count string for display
    public string MessageCountDisplay => 
        Type == ResourceType.Queue
            ? $"Active: {ActiveMessageCount} | Dead: {DeadLetterMessageCount} | Scheduled: {ScheduledMessageCount}"
            : Type == ResourceType.Subscription
                ? $"Active: {ActiveMessageCount} | Dead: {DeadLetterMessageCount} | Transfer: {ScheduledMessageCount}"
                : string.Empty;
} 