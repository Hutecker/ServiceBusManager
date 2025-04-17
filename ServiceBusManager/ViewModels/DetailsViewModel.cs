using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceBusManager.Models;
using ServiceBusManager.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Text.Json;
using ServiceBusManager.Models.Enums;

namespace ServiceBusManager.ViewModels;

public partial class DetailsViewModel : ObservableObject
{
    private readonly ILoggingService _loggingService;
    private readonly IServiceBusService _serviceBusService;

    [ObservableProperty]
    private ServiceBusResourceItem? selectedResource;

    [ObservableProperty]
    private bool isDetailsVisible;

    [ObservableProperty]
    private string selectedTab = "Overview";

    [ObservableProperty]
    private ObservableCollection<MessageOperation> messageOperations = new();

    [ObservableProperty]
    private ObservableCollection<PropertyItem> properties = new();

    [ObservableProperty]
    private ObservableCollection<ServiceBusReceivedMessage> messages = new();

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage;

    public ObservableCollection<LogItem> Logs => _loggingService.Logs;

    public DetailsViewModel(ILoggingService loggingService, IServiceBusService serviceBusService)
    {
        _loggingService = loggingService;
        _serviceBusService = serviceBusService;
        Debug.WriteLine("DetailsViewModel created");
        
        // Initialize message operations
        InitializeMessageOperations();
    }

    private void InitializeMessageOperations()
    {
        MessageOperations.Clear();
        MessageOperations.Add(new MessageOperation(ServiceBusOperation.Peek, "View messages without removing them", "\uf06e")); // eye icon
        MessageOperations.Add(new MessageOperation(ServiceBusOperation.Receive, "Receive and lock messages", "\uf019")); // download icon
        MessageOperations.Add(new MessageOperation(ServiceBusOperation.Purge, "Remove all messages", "\uf1f8")); // trash icon
        MessageOperations.Add(new MessageOperation(ServiceBusOperation.Delete, "Delete selected messages", "\uf2ed")); // delete icon
        MessageOperations.Add(new MessageOperation(ServiceBusOperation.Complete, "Complete selected messages", "\uf00c")); // check icon
        MessageOperations.Add(new MessageOperation(ServiceBusOperation.Abandon, "Abandon selected messages", "\uf05e")); // undo icon
        MessageOperations.Add(new MessageOperation(ServiceBusOperation.DeadLetter, "Move selected messages to dead letter queue", "\uf1da")); // dead letter icon
    }

    [RelayCommand]
    private async Task LoadPropertiesAsync()
    {
        if (SelectedResource == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            Properties.Clear();
            var properties = new Dictionary<string, object>();

            switch (SelectedResource.Type)
            {
                case ResourceType.Queue:
                    var queueProps = await _serviceBusService.GetQueuePropertiesAsync(SelectedResource.Name);
                    properties = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(queueProps));
                    break;
                case ResourceType.Topic:
                    var topicProps = await _serviceBusService.GetTopicPropertiesAsync(SelectedResource.Name);
                    properties = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(topicProps));
                    break;
                case ResourceType.Subscription:
                    var parts = SelectedResource.Name.Split('/');
                    if (parts.Length == 2)
                    {
                        var subProps = await _serviceBusService.GetSubscriptionPropertiesAsync(parts[0], parts[1]);
                        properties = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(subProps));
                    }
                    break;
            }

            foreach (var prop in properties)
            {
                Properties.Add(new PropertyItem
                {
                    Name = prop.Key,
                    Value = prop.Value?.ToString() ?? "null"
                });
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error loading properties: {ex.Message}";
            _loggingService.AddLog(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ExecuteMessageOperationAsync(MessageOperation operation)
    {
        if (SelectedResource == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var path = SelectedResource.Type == ResourceType.Subscription 
                ? $"{SelectedResource.Parent}/{SelectedResource.Name}" 
                : SelectedResource.Name;

            switch (operation.OperationType)
            {
                case ServiceBusOperation.Peek:
                    var peekedMessages = await _serviceBusService.PeekMessagesAsync(path, 10);
                    Messages.Clear();
                    foreach (var msg in peekedMessages)
                    {
                        Messages.Add(msg);
                    }
                    break;
                case ServiceBusOperation.Receive:
                    var receivedMessages = await _serviceBusService.ReceiveMessagesAsync(path, 10);
                    Messages.Clear();
                    foreach (var msg in receivedMessages)
                    {
                        Messages.Add(msg);
                    }
                    break;
                case ServiceBusOperation.Purge:
                    // Implement purge logic
                    break;
                case ServiceBusOperation.Delete:
                    // Implement delete logic
                    break;
                case ServiceBusOperation.Complete:
                    // Implement complete logic
                    break;
                case ServiceBusOperation.Abandon:
                    // Implement abandon logic
                    break;
                case ServiceBusOperation.DeadLetter:
                    // Implement dead letter logic
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error executing {operation.OperationType}: {ex.Message}";
            _loggingService.AddLog(ErrorMessage);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectTab(string tabName)
    {
        Debug.WriteLine($"Selecting tab: {tabName}");
        SelectedTab = tabName;

        if (tabName == "Properties")
        {
            LoadPropertiesCommand.Execute(null);
        }
    }

    public void SelectResource(ServiceBusResourceItem? resource)
    {
        Debug.WriteLine($"DetailsViewModel.SelectResource: {resource?.Name}");
        
        if (resource == null)
        {
            IsDetailsVisible = false;
            SelectedResource = null;
            return;
        }

        SelectedResource = resource;
        IsDetailsVisible = true;
        
        // Load properties when a resource is selected
        LoadPropertiesCommand.Execute(null);
    }
} 