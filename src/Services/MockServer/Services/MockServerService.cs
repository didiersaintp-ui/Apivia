using Apivia.Services.MockServer.DTOs;
using Apivia.Shared.Data;
using Apivia.Shared.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace Apivia.Services.MockServer.Services;

/// <summary>
/// Service for managing mock servers using Prism
/// </summary>
public class MockServerService : IMockServerService
{
    private readonly ApiviaDbContext _context;
    private readonly ILogger<MockServerService> _logger;
    private readonly string _tempDirectory;

    // Track running mock server processes
    private static readonly ConcurrentDictionary<Guid, MockServerProcess> _runningServers = new();

    public MockServerService(
        ApiviaDbContext context,
        ILogger<MockServerService> logger)
    {
        _context = context;
        _logger = logger;
        _tempDirectory = Path.Combine(Path.GetTempPath(), "apivia-mock-servers");
        Directory.CreateDirectory(_tempDirectory);
    }

    public async Task<PagedResponse<MockServerListItemResponse>> GetPagedAsync(
        MockServerQueryParams queryParams,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MockServers
            .Include(m => m.ApiSpec)
            .Where(m => m.IsActive)
            .AsQueryable();

        // Apply filters
        if (queryParams.ApiSpecId.HasValue)
        {
            query = query.Where(m => m.ApiSpecId == queryParams.ApiSpecId.Value);
        }

        if (queryParams.Status.HasValue)
        {
            var statusString = queryParams.Status.Value.ToString();
            query = query.Where(m => m.Status == statusString);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(m => new MockServerListItemResponse
            {
                Id = m.Id,
                ApiSpecId = m.ApiSpecId,
                ApiSpecName = m.ApiSpec.Name,
                ApiSpecVersion = m.ApiSpec.Version,
                Port = m.Port,
                Status = Enum.Parse<MockServerStatus>(m.Status),
                Url = m.Status == "Running" ? $"http://localhost:{m.Port}" : null,
                CreatedAt = m.CreatedAt,
                StartedAt = m.StartedAt,
                RequestCount = m.RequestCount
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<MockServerListItemResponse>
        {
            Items = items,
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize
        };
    }

    public async Task<MockServerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mockServer = await _context.MockServers
            .Include(m => m.ApiSpec)
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Mock server {id} not found");

        var metrics = await GetMetricsAsync(id, cancellationToken);

        return new MockServerResponse
        {
            Id = mockServer.Id,
            ApiSpecId = mockServer.ApiSpecId,
            ApiSpecName = mockServer.ApiSpec.Name,
            ApiSpecVersion = mockServer.ApiSpec.Version,
            Port = mockServer.Port,
            Status = Enum.Parse<MockServerStatus>(mockServer.Status),
            EnableCors = mockServer.EnableCors,
            EnableDynamicExamples = mockServer.EnableDynamicExamples,
            EnableValidation = mockServer.EnableValidation,
            ResponseDelay = mockServer.ResponseDelay,
            Url = mockServer.Status == "Running" ? $"http://localhost:{mockServer.Port}" : null,
            CreatedAt = mockServer.CreatedAt,
            StartedAt = mockServer.StartedAt,
            StoppedAt = mockServer.StoppedAt,
            ErrorMessage = mockServer.ErrorMessage,
            Metrics = metrics
        };
    }

    public async Task<MockServerResponse> CreateAsync(
        CreateMockServerRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Validate API spec exists
        var apiSpec = await _context.ApiSpecs
            .FirstOrDefaultAsync(a => a.Id == request.ApiSpecId && a.IsActive, cancellationToken)
            ?? throw new InvalidOperationException($"API Spec {request.ApiSpecId} not found");

        // Check if port is already in use
        var portInUse = await _context.MockServers
            .AnyAsync(m => m.Port == request.Port && m.Status == "Running" && m.IsActive, cancellationToken);

        if (portInUse)
        {
            throw new InvalidOperationException($"Port {request.Port} is already in use by another mock server");
        }

        // Create mock server entity
        var mockServer = new Apivia.Shared.Data.Entities.MockServer
        {
            Id = Guid.NewGuid(),
            ApiSpecId = request.ApiSpecId,
            Port = request.Port,
            Status = MockServerStatus.Created.ToString(),
            EnableCors = request.EnableCors,
            EnableDynamicExamples = request.EnableDynamicExamples,
            EnableValidation = request.EnableValidation,
            ResponseDelay = request.ResponseDelay,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsActive = true
        };

        _context.MockServers.Add(mockServer);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Mock server {MockServerId} created for API Spec {ApiSpecId}",
            mockServer.Id, request.ApiSpecId);

        // Auto-start the server
        return await StartAsync(mockServer.Id, cancellationToken);
    }

    public async Task<MockServerResponse> UpdateAsync(
        Guid id,
        UpdateMockServerRequest request,
        CancellationToken cancellationToken = default)
    {
        var mockServer = await _context.MockServers
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Mock server {id} not found");

        var wasRunning = mockServer.Status == "Running";

        // Stop if running
        if (wasRunning)
        {
            await StopAsync(id, cancellationToken);
        }

        // Update configuration
        if (request.EnableCors.HasValue)
            mockServer.EnableCors = request.EnableCors.Value;

        if (request.EnableDynamicExamples.HasValue)
            mockServer.EnableDynamicExamples = request.EnableDynamicExamples.Value;

        if (request.EnableValidation.HasValue)
            mockServer.EnableValidation = request.EnableValidation.Value;

        if (request.ResponseDelay.HasValue)
            mockServer.ResponseDelay = request.ResponseDelay.Value;

        mockServer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Mock server {MockServerId} configuration updated", id);

        // Restart if was running
        if (wasRunning)
        {
            return await StartAsync(id, cancellationToken);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<MockServerResponse> StartAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mockServer = await _context.MockServers
            .Include(m => m.ApiSpec)
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Mock server {id} not found");

        if (mockServer.Status == "Running")
        {
            throw new InvalidOperationException("Mock server is already running");
        }

        // Check port availability
        var portInUse = await _context.MockServers
            .AnyAsync(m => m.Port == mockServer.Port &&
                          m.Status == "Running" &&
                          m.Id != id &&
                          m.IsActive, cancellationToken);

        if (portInUse)
        {
            throw new InvalidOperationException($"Port {mockServer.Port} is already in use");
        }

        try
        {
            // Update status to Starting
            mockServer.Status = MockServerStatus.Starting.ToString();
            mockServer.ErrorMessage = null;
            await _context.SaveChangesAsync(cancellationToken);

            // Write OpenAPI spec to temp file
            var specPath = Path.Combine(_tempDirectory, $"{id}.yaml");
            await File.WriteAllTextAsync(specPath, mockServer.ApiSpec.Content, cancellationToken);

            // Start Prism process (simulated for now)
            var process = await StartPrismProcessAsync(mockServer, specPath);

            // Track the process
            _runningServers.TryAdd(id, process);

            // Update status to Running
            mockServer.Status = MockServerStatus.Running.ToString();
            mockServer.StartedAt = DateTime.UtcNow;
            mockServer.StoppedAt = null;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Mock server {MockServerId} started on port {Port}",
                id, mockServer.Port);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start mock server {MockServerId}", id);

            mockServer.Status = MockServerStatus.Failed.ToString();
            mockServer.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException($"Failed to start mock server: {ex.Message}", ex);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<MockServerResponse> StopAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mockServer = await _context.MockServers
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Mock server {id} not found");

        if (mockServer.Status != "Running")
        {
            throw new InvalidOperationException("Mock server is not running");
        }

        try
        {
            // Update status to Stopping
            mockServer.Status = MockServerStatus.Stopping.ToString();
            await _context.SaveChangesAsync(cancellationToken);

            // Stop the Prism process
            if (_runningServers.TryRemove(id, out var process))
            {
                await StopPrismProcessAsync(process);
            }

            // Update status to Stopped
            mockServer.Status = MockServerStatus.Stopped.ToString();
            mockServer.StoppedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Mock server {MockServerId} stopped", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop mock server {MockServerId}", id);

            mockServer.Status = MockServerStatus.Failed.ToString();
            mockServer.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync(cancellationToken);

            throw new InvalidOperationException($"Failed to stop mock server: {ex.Message}", ex);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mockServer = await _context.MockServers
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Mock server {id} not found");

        // Stop if running
        if (mockServer.Status == "Running")
        {
            await StopAsync(id, cancellationToken);
        }

        // Soft delete
        mockServer.IsActive = false;
        mockServer.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Clean up temp files
        var specPath = Path.Combine(_tempDirectory, $"{id}.yaml");
        if (File.Exists(specPath))
        {
            File.Delete(specPath);
        }

        _logger.LogInformation("Mock server {MockServerId} deleted", id);
    }

    public async Task<MockServerLogsResponse> GetLogsAsync(
        Guid id,
        int maxLines = 100,
        CancellationToken cancellationToken = default)
    {
        var mockServer = await _context.MockServers
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Mock server {id} not found");

        var logs = new List<LogEntry>();

        // Get logs from running process
        if (_runningServers.TryGetValue(id, out var process))
        {
            logs = process.Logs.TakeLast(maxLines).ToList();
        }

        return new MockServerLogsResponse
        {
            MockServerId = id,
            Logs = logs
        };
    }

    public async Task<MockServerMetrics> GetMetricsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mockServer = await _context.MockServers
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException($"Mock server {id} not found");

        var metrics = new MockServerMetrics
        {
            TotalRequests = mockServer.RequestCount,
            SuccessfulRequests = mockServer.SuccessfulRequestCount,
            FailedRequests = mockServer.FailedRequestCount,
            AverageResponseTime = mockServer.AverageResponseTime,
            LastRequestAt = mockServer.LastRequestAt
        };

        // Get additional metrics from running process
        if (_runningServers.TryGetValue(id, out var process))
        {
            metrics.RequestsByEndpoint = process.RequestsByEndpoint.ToDictionary(
                kvp => kvp.Key, kvp => kvp.Value);
            metrics.RequestsByMethod = process.RequestsByMethod.ToDictionary(
                kvp => kvp.Key, kvp => kvp.Value);
        }

        return metrics;
    }

    /// <summary>
    /// Start Prism process (simulated implementation)
    /// In production, this would execute: npx prism mock <spec-file> -p <port>
    /// </summary>
    private async Task<MockServerProcess> StartPrismProcessAsync(
        Apivia.Shared.Data.Entities.MockServer mockServer,
        string specPath)
    {
        // Simulated Prism process
        // In production, you would use Process.Start with Prism CLI:
        // var processInfo = new ProcessStartInfo
        // {
        //     FileName = "npx",
        //     Arguments = $"@stoplight/prism-cli mock {specPath} -p {mockServer.Port}",
        //     UseShellExecute = false,
        //     RedirectStandardOutput = true,
        //     RedirectStandardError = true
        // };
        // var process = Process.Start(processInfo);

        var process = new MockServerProcess
        {
            MockServerId = mockServer.Id,
            Port = mockServer.Port,
            SpecPath = specPath,
            StartedAt = DateTime.UtcNow
        };

        // Add initial log entry
        process.Logs.Add(new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = "INFO",
            Message = $"Mock server started on port {mockServer.Port}",
            Details = $"API Spec: {mockServer.ApiSpec.Name} v{mockServer.ApiSpec.Version}"
        });

        await Task.CompletedTask;
        return process;
    }

    /// <summary>
    /// Stop Prism process
    /// </summary>
    private async Task StopPrismProcessAsync(MockServerProcess process)
    {
        // In production, this would kill the actual process
        // process.Kill();
        // process.WaitForExit(5000);

        process.Logs.Add(new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = "INFO",
            Message = "Mock server stopped"
        });

        await Task.CompletedTask;
    }
}

/// <summary>
/// Represents a running mock server process
/// </summary>
internal class MockServerProcess
{
    public Guid MockServerId { get; set; }
    public int Port { get; set; }
    public string SpecPath { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public List<LogEntry> Logs { get; set; } = new();
    public ConcurrentDictionary<string, int> RequestsByEndpoint { get; set; } = new();
    public ConcurrentDictionary<string, int> RequestsByMethod { get; set; } = new();
}
