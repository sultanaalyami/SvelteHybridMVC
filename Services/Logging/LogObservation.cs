namespace HRCE.Services.Logging;

public sealed record LogObservation
{
    public required DateTime Timestamp { get; init; }
    public required string Level { get; init; }
    public required string Message { get; init; }
    public string? Exception { get; init; }
    public string? Logger { get; init; }
    public string? EventId { get; init; }
    public string? EventName { get; init; }
    public string? Path { get; init; }
    public string? TraceId { get; init; }
    public bool IsError => Level is "ERROR" or "CRITICAL";
}
