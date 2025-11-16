namespace Apivia.Shared.Data.Entities;

/// <summary>
/// Mock Server instance for serving API specifications
/// </summary>
public class MockServer : BaseEntity
{
    public Guid ApiSpecId { get; set; }
    public int Port { get; set; }
    public string Status { get; set; } = "Created"; // Created, Starting, Running, Stopping, Stopped, Failed

    // Configuration options
    public bool EnableCors { get; set; } = true;
    public bool EnableDynamicExamples { get; set; } = true;
    public bool EnableValidation { get; set; } = true;
    public int? ResponseDelay { get; set; }

    // Runtime state
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public string? ErrorMessage { get; set; }

    // Metrics
    public int RequestCount { get; set; }
    public int SuccessfulRequestCount { get; set; }
    public int FailedRequestCount { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime? LastRequestAt { get; set; }

    // Navigation properties
    public ApiSpec ApiSpec { get; set; } = null!;
}
