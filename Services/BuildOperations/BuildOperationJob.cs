namespace HRCE.Services.BuildOperations;

public sealed class BuildOperationJob
{
    public Guid Id { get; init; }
    public string CommandKey { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int? ExitCode { get; set; }
    public BuildOperationState State { get; set; } = BuildOperationState.Pending;
    public IReadOnlyList<string> Output { get; set; } = Array.Empty<string>();
    public string? Error { get; set; }
}

public enum BuildOperationState
{
    Pending = 0,
    Running = 1,
    Succeeded = 2,
    Failed = 3,
    Canceled = 4
}
