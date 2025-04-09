using ServiceBusManager.Models;
using ServiceBusManager.Models.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using System.Diagnostics;

namespace ServiceBusManager.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ILoggingService _loggingService;
    private string _connectionString = "";

    // Replace with actual Azure Service Bus connection string and client logic
    // private ServiceBusAdministrationClient _adminClient;
    // private ServiceBusClient _client;

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
        // If no connection string, return empty list
        if (string.IsNullOrEmpty(_connectionString))
        {
            _loggingService.AddLog("No connection string configured. Please add a connection string.");
            return new List<ServiceBusResourceItem>();
        }
        
        // TODO: Replace with actual API calls to fetch queues and topics/subscriptions
        // Use _adminClient.GetQueuesAsync(), _adminClient.GetTopicsAsync(), _adminClient.GetSubscriptionsAsync()
        
        // Returning sample data for now
        await Task.Delay(500); // Simulate async work

        var resources = new List<ServiceBusResourceItem>
        {
            new ServiceBusResourceItem 
            { 
                Name = "Queue1", 
                Type = ResourceType.Queue,
                Children = new ObservableCollection<ServiceBusResourceItem>()
            },
            new ServiceBusResourceItem 
            { 
                Name = "Topic1", 
                Type = ResourceType.Topic,
                Children = new ObservableCollection<ServiceBusResourceItem>
                {
                    new ServiceBusResourceItem { Name = "Subscription1", Type = ResourceType.Subscription, Parent = "Topic1" },
                    new ServiceBusResourceItem { Name = "Subscription2", Type = ResourceType.Subscription, Parent = "Topic1" }
                }
            }
        };

        return resources;
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
            _connectionString = connectionString;
            
            // In a real app, you'd save this to secure storage
            // await SecureStorage.SetAsync("ServiceBusConnectionString", connectionString);
            
            // Initialize clients with new connection string
            // _adminClient = new ServiceBusAdministrationClient(connectionString);
            // _client = new ServiceBusClient(connectionString);
            
            _loggingService.AddLog("Connection string updated");
            
            // Validate connection by making a test request
            // This would be an actual check in a real app
            await Task.Delay(500); // Simulate some async validation work
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error setting connection string: {ex.Message}");
            throw;
        }
    }

    // Implement other IServiceBusService methods here...
} 