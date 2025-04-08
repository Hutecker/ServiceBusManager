using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ServiceBusManager.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<ServiceBusResourceItem> resources = new();

    [ObservableProperty]
    private ServiceBusResourceItem selectedResource;

    [ObservableProperty]
    private ObservableCollection<LogItem> logs = new();

    [ObservableProperty]
    private bool isLogsVisible = true;
    
    [ObservableProperty]
    private int selectedTabIndex = 0;
    
    [ObservableProperty]
    private string[] tabNames = new[] { "Overview", "Messages", "Properties" };

    public MainViewModel()
    {
        // Add sample data - Replace with actual Service Bus API calls later
        Resources.Add(new ServiceBusResourceItem 
        { 
            Name = "Queue1", 
            Type = ResourceType.Queue,
            Children = new ObservableCollection<ServiceBusResourceItem>()
        });
        
        Resources.Add(new ServiceBusResourceItem 
        { 
            Name = "Topic1", 
            Type = ResourceType.Topic,
            Children = new ObservableCollection<ServiceBusResourceItem>
            {
                new ServiceBusResourceItem { Name = "Subscription1", Type = ResourceType.Subscription, Parent = "Topic1" },
                new ServiceBusResourceItem { Name = "Subscription2", Type = ResourceType.Subscription, Parent = "Topic1" }
            }
        });
        
        // Add initial log
        AddLog("Application started");
    }

    [RelayCommand]
    private void SelectResource(ServiceBusResourceItem resource)
    {
        SelectedResource = resource;
        AddLog($"Selected {resource.Type} '{resource.Name}'");
    }

    [RelayCommand]
    private void ClearLogs()
    {
        Logs.Clear();
        AddLog("Logs cleared");
    }

    [RelayCommand]
    private void ToggleLogs()
    {
        IsLogsVisible = !IsLogsVisible;
        AddLog($"Logs panel {(IsLogsVisible ? "shown" : "hidden")}");
    }
    
    [RelayCommand]
    private void SelectTab(int index)
    {
        if (index >= 0 && index < TabNames.Length)
        {
            SelectedTabIndex = index;
            AddLog($"Selected tab: {TabNames[index]}");
        }
    }

    public void AddLog(string message)
    {
        Logs.Add(new LogItem
        {
            Timestamp = DateTime.Now,
            Message = message
        });
    }
}

public class ServiceBusResourceItem
{
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public string Parent { get; set; }
    public ObservableCollection<ServiceBusResourceItem> Children { get; set; } = new();
}

public enum ResourceType
{
    Queue,
    Topic,
    Subscription
}

public class LogItem
{
    public DateTime Timestamp { get; set; }
    public string Message { get; set; }
    
    public string FormattedLog => $"[{Timestamp:HH:mm:ss}] {Message}";
}
