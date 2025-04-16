using ServiceBusManager.Models;
using ServiceBusManager.Models.Enums;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Collections.Concurrent;
using System.Threading;

namespace ServiceBusManager.Services;

public class ServiceBusService : IServiceBusService
{
    private readonly ILoggingService _loggingService;
    private string _connectionString = "";
    private ServiceBusAdministrationClient? _adminClient;
    private readonly ConcurrentDictionary<string, ServiceBusClient> _clients = new();
    private readonly ConcurrentDictionary<string, ServiceBusSender> _senders = new();
    private readonly ConcurrentDictionary<string, ServiceBusReceiver> _receivers = new();
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _isConnected;

    public bool IsConnected => _isConnected;

    public ServiceBusService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
        LoadConnectionStringAsync().ConfigureAwait(false);
    }
    
    private async Task LoadConnectionStringAsync()
    {
        try
        {
            if (!string.IsNullOrEmpty(_connectionString))
            {
                await ValidateAndSetConnectionAsync(_connectionString);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading connection string: {ex.Message}");
            _loggingService.AddLog($"Error loading connection string: {ex.Message}");
        }
    }

    private async Task ValidateAndSetConnectionAsync(string connectionString)
    {
        await _connectionLock.WaitAsync();
        try
        {
            // Create a new admin client to validate the connection
            var adminClient = new ServiceBusAdministrationClient(connectionString);
            
            // Test the connection by getting the namespace properties
            await adminClient.GetNamespacePropertiesAsync();
            
            // If we get here, the connection is valid
            _connectionString = connectionString;
            _adminClient = adminClient;
            _isConnected = true;
            
            await ClearExistingClientsAsync();
            
            _loggingService.AddLog("Successfully connected to Service Bus namespace");
        }
        catch (Exception ex)
        {
            _isConnected = false;
            _loggingService.AddLog($"Failed to connect to Service Bus: {ex.Message}");
            throw new ArgumentException("Invalid connection string or unable to connect to Service Bus", ex);
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    private async Task ClearExistingClientsAsync()
    {
        foreach (var client in _clients.Values)
        {
            await client.DisposeAsync();
        }
        _clients.Clear();
        _senders.Clear();
        _receivers.Clear();
    }

    private async Task<ServiceBusClient> GetOrCreateClientAsync()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Not connected to Service Bus");
        }

        var client = _clients.GetOrAdd(_connectionString, cs => new ServiceBusClient(cs));
        return client;
    }

    private async Task<ServiceBusSender> GetOrCreateSenderAsync(string queueOrTopicName)
    {
        var client = await GetOrCreateClientAsync();
        return _senders.GetOrAdd(queueOrTopicName, name => client.CreateSender(name));
    }

    private async Task<ServiceBusReceiver> GetOrCreateReceiverAsync(string queueOrSubscriptionPath, ServiceBusReceiveMode receiveMode = ServiceBusReceiveMode.PeekLock)
    {
        var client = await GetOrCreateClientAsync();
        return _receivers.GetOrAdd(queueOrSubscriptionPath, path => client.CreateReceiver(path, new ServiceBusReceiverOptions
        {
            ReceiveMode = receiveMode
        }));
    }

    public async Task<IEnumerable<ServiceBusResourceItem>> GetResourcesAsync()
    {
        if (!_isConnected)
        {
            _loggingService.AddLog("Not connected to Service Bus");
            return new List<ServiceBusResourceItem>();
        }

        try
        {
            var resources = new List<ServiceBusResourceItem>();
            
            // Get all queues
            _loggingService.AddLog("Fetching queues...");
            await foreach (var queueProperties in _adminClient!.GetQueuesAsync())
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
    
    public string GetConnectionString() => _connectionString;
    
    public bool HasConnectionString() => !string.IsNullOrWhiteSpace(_connectionString);
    
    public async Task SetConnectionStringAsync(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));
        }
        
        await ValidateAndSetConnectionAsync(connectionString);
    }

    public async Task<QueueProperties> GetQueuePropertiesAsync(string queueName)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        return await _adminClient!.GetQueueAsync(queueName);
    }

    public async Task<TopicProperties> GetTopicPropertiesAsync(string topicName)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        return await _adminClient!.GetTopicAsync(topicName);
    }

    public async Task<SubscriptionProperties> GetSubscriptionPropertiesAsync(string topicName, string subscriptionName)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        return await _adminClient!.GetSubscriptionAsync(topicName, subscriptionName);
    }

    public async Task<IEnumerable<ServiceBusReceivedMessage>> PeekMessagesAsync(string queueOrSubscriptionPath, int maxMessages)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        var receiver = await GetOrCreateReceiverAsync(queueOrSubscriptionPath, ServiceBusReceiveMode.PeekLock);
        var messages = new List<ServiceBusReceivedMessage>();
        
        try
        {
            var receivedMessages = await receiver.PeekMessagesAsync(maxMessages);
            messages.AddRange(receivedMessages);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error peeking messages: {ex.Message}");
            throw;
        }
        
        return messages;
    }

    public async Task SendMessageAsync(string queueOrTopicName, ServiceBusMessage message)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        var sender = await GetOrCreateSenderAsync(queueOrTopicName);
        try
        {
            await sender.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error sending message: {ex.Message}");
            throw;
        }
    }

    public async Task SendMessagesAsync(string queueOrTopicName, IEnumerable<ServiceBusMessage> messages)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        var sender = await GetOrCreateSenderAsync(queueOrTopicName);
        try
        {
            await sender.SendMessagesAsync(messages);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error sending messages: {ex.Message}");
            throw;
        }
    }

    public async Task<ServiceBusReceivedMessage> ReceiveMessageAsync(string queueOrSubscriptionPath, TimeSpan? maxWaitTime = null)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        var receiver = await GetOrCreateReceiverAsync(queueOrSubscriptionPath);
        try
        {
            return await receiver.ReceiveMessageAsync(maxWaitTime ?? TimeSpan.FromSeconds(30));
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error receiving message: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ServiceBusReceivedMessage>> ReceiveMessagesAsync(string queueOrSubscriptionPath, int maxMessages, TimeSpan? maxWaitTime = null)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        var receiver = await GetOrCreateReceiverAsync(queueOrSubscriptionPath);
        var messages = new List<ServiceBusReceivedMessage>();
        
        try
        {
            var receivedMessages = await receiver.ReceiveMessagesAsync(maxMessages, maxWaitTime ?? TimeSpan.FromSeconds(30));
            messages.AddRange(receivedMessages);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error receiving messages: {ex.Message}");
            throw;
        }
        
        return messages;
    }

    public async Task CompleteMessageAsync(ServiceBusReceivedMessage message)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            // Get the receiver for the message's source
            var receiver = await GetOrCreateReceiverAsync(message.Subject);
            await receiver.CompleteMessageAsync(message);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error completing message: {ex.Message}");
            throw;
        }
    }

    public async Task AbandonMessageAsync(ServiceBusReceivedMessage message)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            // Get the receiver for the message's source
            var receiver = await GetOrCreateReceiverAsync(message.Subject);
            await receiver.AbandonMessageAsync(message);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error abandoning message: {ex.Message}");
            throw;
        }
    }

    public async Task DeadLetterMessageAsync(ServiceBusReceivedMessage message, string? reason = null, string? errorDescription = null)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            // Get the receiver for the message's source
            var receiver = await GetOrCreateReceiverAsync(message.Subject);
            await receiver.DeadLetterMessageAsync(message, reason, errorDescription);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error dead-lettering message: {ex.Message}");
            throw;
        }
    }

    public async Task CreateQueueAsync(string queueName, CreateQueueOptions options)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            await _adminClient!.CreateQueueAsync(options);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error creating queue: {ex.Message}");
            throw;
        }
    }

    public async Task CreateTopicAsync(string topicName, CreateTopicOptions options)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            await _adminClient!.CreateTopicAsync(options);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error creating topic: {ex.Message}");
            throw;
        }
    }

    public async Task CreateSubscriptionAsync(string topicName, string subscriptionName, CreateSubscriptionOptions options)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            await _adminClient!.CreateSubscriptionAsync(options);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error creating subscription: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteQueueAsync(string queueName)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            await _adminClient!.DeleteQueueAsync(queueName);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error deleting queue: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteTopicAsync(string topicName)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            await _adminClient!.DeleteTopicAsync(topicName);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error deleting topic: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteSubscriptionAsync(string topicName, string subscriptionName)
    {
        if (!_isConnected) throw new InvalidOperationException("Not connected to Service Bus");
        
        try
        {
            await _adminClient!.DeleteSubscriptionAsync(topicName, subscriptionName);
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error deleting subscription: {ex.Message}");
            throw;
        }
    }
}