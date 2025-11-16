using System.ComponentModel.DataAnnotations;

namespace Apivia.Services.MockServer.DTOs;

/// <summary>
/// Request to create a new mock server
/// </summary>
public class CreateMockServerRequest
{
    [Required]
    public Guid ApiSpecId { get; set; }

    [Required]
    [Range(3000, 65535)]
    public int Port { get; set; }

    public bool EnableCors { get; set; } = true;

    public bool EnableDynamicExamples { get; set; } = true;

    public bool EnableValidation { get; set; } = true;

    public int? ResponseDelay { get; set; }
}

/// <summary>
/// Response containing mock server details
/// </summary>
public class MockServerResponse
{
    public Guid Id { get; set; }
    public Guid ApiSpecId { get; set; }
    public string ApiSpecName { get; set; } = string.Empty;
    public string ApiSpecVersion { get; set; } = string.Empty;
    public int Port { get; set; }
    public MockServerStatus Status { get; set; }
    public bool EnableCors { get; set; }
    public bool EnableDynamicExamples { get; set; }
    public bool EnableValidation { get; set; }
    public int? ResponseDelay { get; set; }
    public string? Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? StoppedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public MockServerMetrics? Metrics { get; set; }
}

/// <summary>
/// List item for mock servers
/// </summary>
public class MockServerListItemResponse
{
    public Guid Id { get; set; }
    public Guid ApiSpecId { get; set; }
    public string ApiSpecName { get; set; } = string.Empty;
    public string ApiSpecVersion { get; set; } = string.Empty;
    public int Port { get; set; }
    public MockServerStatus Status { get; set; }
    public string? Url { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public int RequestCount { get; set; }
}

/// <summary>
/// Mock server status enum
/// </summary>
public enum MockServerStatus
{
    Created,
    Starting,
    Running,
    Stopping,
    Stopped,
    Failed
}

/// <summary>
/// Mock server metrics
/// </summary>
public class MockServerMetrics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime? LastRequestAt { get; set; }
    public Dictionary<string, int> RequestsByEndpoint { get; set; } = new();
    public Dictionary<string, int> RequestsByMethod { get; set; } = new();
}

/// <summary>
/// Query parameters for listing mock servers
/// </summary>
public class MockServerQueryParams
{
    public Guid? ApiSpecId { get; set; }
    public MockServerStatus? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Request to update mock server configuration
/// </summary>
public class UpdateMockServerRequest
{
    public bool? EnableCors { get; set; }
    public bool? EnableDynamicExamples { get; set; }
    public bool? EnableValidation { get; set; }
    public int? ResponseDelay { get; set; }
}

/// <summary>
/// Response for mock server logs
/// </summary>
public class MockServerLogsResponse
{
    public Guid MockServerId { get; set; }
    public List<LogEntry> Logs { get; set; } = new();
}

/// <summary>
/// Individual log entry
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
