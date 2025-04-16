using System.ComponentModel.DataAnnotations;

namespace ServiceBusManager.Models;

public class SavedConnection
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string ConnectionString { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedAt { get; set; }
} 