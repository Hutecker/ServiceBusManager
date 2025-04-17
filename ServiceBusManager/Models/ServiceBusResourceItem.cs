using ServiceBusManager.Models.Enums;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Diagnostics;

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
    private string messageCountDisplay = string.Empty;
    
    partial void OnActiveMessageCountChanged(long value)
    {
        Debug.WriteLine($"ActiveMessageCount changed to {value} for {Name}");
        UpdateMessageCountDisplay();
    }
    
    partial void OnDeadLetterMessageCountChanged(long value)
    {
        Debug.WriteLine($"DeadLetterMessageCount changed to {value} for {Name}");
        UpdateMessageCountDisplay();
    }
    
    partial void OnScheduledMessageCountChanged(long value)
    {
        Debug.WriteLine($"ScheduledMessageCount changed to {value} for {Name}");
        UpdateMessageCountDisplay();
    }
    
    partial void OnTypeChanged(ResourceType value)
    {
        Debug.WriteLine($"Type changed to {value} for {Name}");
        UpdateMessageCountDisplay();
    }

    public void UpdateMessageCountDisplay()
    {
        Debug.WriteLine($"UpdateMessageCountDisplay called for {Name} of type {Type}");
        Debug.WriteLine($"Current counts - Active: {ActiveMessageCount}, Dead: {DeadLetterMessageCount}, Scheduled: {ScheduledMessageCount}");
        
        switch (Type)
        {
            case ResourceType.Queue:
                MessageCountDisplay = $"Active: {ActiveMessageCount} | Dead: {DeadLetterMessageCount} | Scheduled: {ScheduledMessageCount}";
                Debug.WriteLine($"Set queue message display to: {MessageCountDisplay}");
                break;
            case ResourceType.Topic:
                // For topics, show the sum of all subscription message counts
                var totalActive = Children.Sum(c => c.ActiveMessageCount);
                var totalDead = Children.Sum(c => c.DeadLetterMessageCount);
                var totalTransfer = Children.Sum(c => c.ScheduledMessageCount);
                MessageCountDisplay = $"Active: {totalActive} | Dead: {totalDead} | Transfer: {totalTransfer}";
                Debug.WriteLine($"Set topic message display to: {MessageCountDisplay}");
                break;
            case ResourceType.Subscription:
                MessageCountDisplay = $"Active: {ActiveMessageCount} | Dead: {DeadLetterMessageCount} | Transfer: {ScheduledMessageCount}";
                Debug.WriteLine($"Set subscription message display to: {MessageCountDisplay}");
                break;
            default:
                MessageCountDisplay = string.Empty;
                Debug.WriteLine("Set empty message display for unknown type");
                break;
        }
    }
} 