using CommunityToolkit.Mvvm.ComponentModel;

namespace ServiceBusManager.Models;

public partial class PropertyItem : ObservableObject
{
    [ObservableProperty]
    private string name;

    [ObservableProperty]
    private string value;
} 