using ServiceBusManager.Models;
using ServiceBusManager.Models.Enums;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace ServiceBusManager.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ILoggingService _loggingService;
    private string _connectionString = "";
    private ServiceBusAdministrationClient? _adminClient;

    public ServiceBusService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
        
        // Get saved connection string from secure storage if available
        LoadConnectionStringAsync().ConfigureAwait(false);
    }
    
    private async Task LoadConnectionStringAsync()
    {
        try
        {
            // In a real app, you'd retrieve this from secure storage
            // _connectionString = await SecureStorage.GetAsync("ServiceBusConnectionString") ?? "";
            
            // For now, just log that we're trying to load it
            _loggingService.AddLog("Attempting to load saved connection string");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading connection string: {ex.Message}");
            _loggingService.AddLog($"Error loading connection string: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ServiceBusResourceItem>> GetResourcesAsync()
    {
        if (string.IsNullOrEmpty(_connectionString))
        {
            _loggingService.AddLog("No connection string configured. Please add a connection string.");
            return new List<ServiceBusResourceItem>();
        }

        try
        {
            var resources = new List<ServiceBusResourceItem>();
            
            // Create admin client if not exists
            _adminClient ??= new ServiceBusAdministrationClient(_connectionString);

            // Get all queues
            _loggingService.AddLog("Fetching queues...");
            await foreach (var queueProperties in _adminClient.GetQueuesAsync())
            {
                resources.Add(new ServiceBusResourceItem
                {
                    Name = queueProperties.Name,
                    Type = ResourceType.Queue,
                    Children = new ObservableCollection<ServiceBusResourceItem>()
                });
            }

            // Get all topics and their subscriptions
            _loggingService.AddLog("Fetching topics and subscriptions...");
            await foreach (var topicProperties in _adminClient.GetTopicsAsync())
            {
                var topicItem = new ServiceBusResourceItem
                {
                    Name = topicProperties.Name,
                    Type = ResourceType.Topic,
                    Children = new ObservableCollection<ServiceBusResourceItem>()
                };

                // Get subscriptions for this topic
                await foreach (var subscriptionProperties in _adminClient.GetSubscriptionsAsync(topicProperties.Name))
                {
                    topicItem.Children.Add(new ServiceBusResourceItem
                    {
                        Name = subscriptionProperties.SubscriptionName,
                        Type = ResourceType.Subscription,
                        Parent = topicProperties.Name
                    });
                }

                resources.Add(topicItem);
            }

            _loggingService.AddLog($"Found {resources.Count} resources");
            return resources;
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error fetching resources: {ex.Message}");
            Debug.WriteLine($"Error fetching resources: {ex}");
            return new List<ServiceBusResourceItem>();
        }
    }
    
    public string GetConnectionString()
    {
        return _connectionString;
    }
    
    public bool HasConnectionString()
    {
        return !string.IsNullOrWhiteSpace(_connectionString);
    }
    
    public async Task SetConnectionStringAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));
        }
        
        try
        {
            // Validate connection string by attempting to create a client
            _adminClient = new ServiceBusAdministrationClient(connectionString);
            
            // If we get here, connection string is valid
            _connectionString = connectionString;
            _loggingService.AddLog("Connection string updated successfully");
            
            // In a real app, you'd save this to secure storage
            // await SecureStorage.SetAsync("ServiceBusConnectionString", connectionString);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Invalid connection string: {ex.Message}");
            throw new ArgumentException("Invalid connection string");
        }
    }
}