using ServiceBusManager.Models;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace ServiceBusManager.Services;

public class ConnectionStorageService : IConnectionStorageService
{
    private const string ConnectionsKey = "saved_connections";
    private readonly ILoggingService _loggingService;

    public ConnectionStorageService(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task SaveConnectionAsync(SavedConnection connection)
    {
        try
        {
            var connections = await GetConnectionsAsync();
            var existingConnection = connections.FirstOrDefault(c => c.Name == connection.Name);
            
            if (existingConnection != null)
            {
                connections.Remove(existingConnection);
            }
            
            connections.Add(connection);
            await SaveConnectionsAsync(connections);
            _loggingService.AddLog($"Saved connection: {connection.Name}");
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error saving connection: {ex.Message}");
            throw;
        }
    }

    public async Task<List<SavedConnection>> GetConnectionsAsync()
    {
        try
        {
            var json = await SecureStorage.GetAsync(ConnectionsKey);
            Debug.WriteLine($"Retrieved JSON from storage: {json}");
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.WriteLine("No connections found in storage");
                return new List<SavedConnection>();
            }

            var connections = JsonSerializer.Deserialize<List<SavedConnection>>(json) ?? new List<SavedConnection>();
            Debug.WriteLine($"Deserialized {connections.Count} connections");
            return connections;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting connections: {ex.Message}");
            _loggingService.AddLog($"Error getting connections: {ex.Message}");
            return new List<SavedConnection>();
        }
    }

    public async Task DeleteConnectionAsync(string name)
    {
        try
        {
            var connections = await GetConnectionsAsync();
            var connection = connections.FirstOrDefault(c => c.Name == name);
            
            if (connection != null)
            {
                connections.Remove(connection);
                await SaveConnectionsAsync(connections);
                _loggingService.AddLog($"Deleted connection: {name}");
            }
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error deleting connection: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateConnectionLastUsedAsync(string name)
    {
        try
        {
            var connections = await GetConnectionsAsync();
            var connection = connections.FirstOrDefault(c => c.Name == name);
            
            if (connection != null)
            {
                connection.LastUsedAt = DateTime.UtcNow;
                await SaveConnectionsAsync(connections);
            }
        }
        catch (Exception ex)
        {
            _loggingService.AddLog($"Error updating connection last used: {ex.Message}");
        }
    }

    private async Task SaveConnectionsAsync(List<SavedConnection> connections)
    {
        var json = JsonSerializer.Serialize(connections);
        await SecureStorage.SetAsync(ConnectionsKey, json);
    }
} 