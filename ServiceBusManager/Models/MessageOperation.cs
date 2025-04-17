using CommunityToolkit.Mvvm.ComponentModel;
using ServiceBusManager.Models.Enums;

namespace ServiceBusManager.Models;

public partial class MessageOperation : ObservableObject
{
    [ObservableProperty]
    private ServiceBusOperation operationType;

    [ObservableProperty]
    private string description;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private string icon;

    public MessageOperation(ServiceBusOperation operationType, string description, string icon)
    {
        OperationType = operationType;
        Description = description;
        Icon = icon;
        IsEnabled = true;
    }
} 