namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Mock Server instance
/// </summary>
public class MockServer : BaseEntity
{
    public Guid ApiSpecId { get; set; }
    public int Port { get; set; }
    public bool IsRunning { get; set; }
    public int? ProcessId { get; set; }
    public DateTime? StartedAt { get; set; }
    public string? Configuration { get; set; } // JSON config

    // Navigation properties
    public ApiSpec ApiSpec { get; set; } = null!;
}
