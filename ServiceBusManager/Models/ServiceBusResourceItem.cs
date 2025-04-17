using ServiceBusManager.Models.Enums;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;

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

    [ObservableProperty]
    private string messageCountDisplay;
    
    partial void OnActiveMessageCountChanged(long value) => UpdateMessageCountDisplay();
    partial void OnDeadLetterMessageCountChanged(long value) => UpdateMessageCountDisplay();
    partial void OnScheduledMessageCountChanged(long value) => UpdateMessageCountDisplay();
    partial void OnTypeChanged(ResourceType value) => UpdateMessageCountDisplay();

    private void UpdateMessageCountDisplay()
    {
        switch (Type)
        {
            case ResourceType.Queue:
                MessageCountDisplay = $"Active: {ActiveMessageCount} | Dead: {DeadLetterMessageCount} | Scheduled: {ScheduledMessageCount}";
                break;
            case ResourceType.Topic:
                // For topics, show the sum of all subscription message counts
                var totalActive = Children.Sum(c => c.ActiveMessageCount);
                var totalDead = Children.Sum(c => c.DeadLetterMessageCount);
                var totalTransfer = Children.Sum(c => c.ScheduledMessageCount);
                MessageCountDisplay = $"Active: {totalActive} | Dead: {totalDead} | Transfer: {totalTransfer}";
                break;
            case ResourceType.Subscription:
                MessageCountDisplay = $"Active: {ActiveMessageCount} | Dead: {DeadLetterMessageCount} | Transfer: {ScheduledMessageCount}";
                break;
            default:
                MessageCountDisplay = string.Empty;
                break;
        }
    }
} 