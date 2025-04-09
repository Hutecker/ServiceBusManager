using ServiceBusManager.Models.Enums;
using System.Collections.ObjectModel;

namespace ServiceBusManager.Models;

public class ServiceBusResourceItem
{
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public string Parent { get; set; }
    public ObservableCollection<ServiceBusResourceItem> Children { get; set; } = new();
} 