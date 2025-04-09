using ServiceBusManager.Models.Enums;
using System.Collections.ObjectModel;

namespace ServiceBusManager.Models;

public class ServiceBusResourceItem : ServiceBusResource
{
    public string Name { get; set; }
    public ResourceType Type { get; set; }
    public string Parent { get; set; }
    public ObservableCollection<ServiceBusResourceItem> Children { get; set; } = new();
    public string Icon { get; set; } = "\uf1b2"; // Font Awesome bus icon
    public bool IsSelected { get; set; }
} 