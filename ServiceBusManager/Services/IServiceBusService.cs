using ServiceBusManager.Models;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceBusManager.Services;

public interface IServiceBusService
{
    // Connection management
    string GetConnectionString();
    Task SetConnectionStringAsync(string connectionString);
    bool HasConnectionString();
    bool IsConnected { get; }
    
    // Resource management
    Task<IEnumerable<ServiceBusResourceItem>> GetResourcesAsync();
    Task<QueueProperties> GetQueuePropertiesAsync(string queueName);
    Task<TopicProperties> GetTopicPropertiesAsync(string topicName);
    Task<SubscriptionProperties> GetSubscriptionPropertiesAsync(string topicName, string subscriptionName);
    Task<(long Active, long DeadLetter, long Scheduled)> GetMessageCountsAsync(string queueOrSubscriptionPath);
    
    // Message operations
    Task<IEnumerable<ServiceBusReceivedMessage>> PeekMessagesAsync(string queueOrSubscriptionPath, int maxMessages);
    Task SendMessageAsync(string queueOrTopicName, ServiceBusMessage message);
    Task SendMessagesAsync(string queueOrTopicName, IEnumerable<ServiceBusMessage> messages);
    Task<ServiceBusReceivedMessage> ReceiveMessageAsync(string queueOrSubscriptionPath, TimeSpan? maxWaitTime = null);
    Task<IEnumerable<ServiceBusReceivedMessage>> ReceiveMessagesAsync(string queueOrSubscriptionPath, int maxMessages, TimeSpan? maxWaitTime = null);
    Task CompleteMessageAsync(ServiceBusReceivedMessage message);
    Task AbandonMessageAsync(ServiceBusReceivedMessage message);
    Task DeadLetterMessageAsync(ServiceBusReceivedMessage message, string? reason = null, string? errorDescription = null);
    
    // Administration operations
    Task CreateQueueAsync(string queueName, CreateQueueOptions options);
    Task CreateTopicAsync(string topicName, CreateTopicOptions options);
    Task CreateSubscriptionAsync(string topicName, string subscriptionName, CreateSubscriptionOptions options);
    Task DeleteQueueAsync(string queueName);
    Task DeleteTopicAsync(string topicName);
    Task DeleteSubscriptionAsync(string topicName, string subscriptionName);
} 