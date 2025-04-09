using ServiceBusManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceBusManager.Services;

public interface IServiceBusService
{
    // Example method - adjust based on actual needs
    Task<IEnumerable<ServiceBusResourceItem>> GetResourcesAsync();

    // Connection string management
    string GetConnectionString();
    Task SetConnectionStringAsync(string connectionString);
    bool HasConnectionString();

    // Add other methods as needed, e.g.:
    // Task<QueueProperties> GetQueuePropertiesAsync(string queueName);
    // Task<TopicProperties> GetTopicPropertiesAsync(string topicName);
    // Task<SubscriptionProperties> GetSubscriptionPropertiesAsync(string topicName, string subscriptionName);
    // Task<IEnumerable<ServiceBusReceivedMessage>> PeekMessagesAsync(string queueOrSubscriptionPath, int maxMessages);
} 