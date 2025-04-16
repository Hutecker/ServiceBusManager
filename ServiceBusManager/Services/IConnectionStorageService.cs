using ServiceBusManager.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceBusManager.Services;

public interface IConnectionStorageService
{
    Task SaveConnectionAsync(SavedConnection connection);
    Task<List<SavedConnection>> GetConnectionsAsync();
    Task DeleteConnectionAsync(string name);
    Task UpdateConnectionLastUsedAsync(string name);
} 