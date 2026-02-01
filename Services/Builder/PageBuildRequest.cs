namespace HRCE.Services.Builder;

public sealed record PageBuildRequest
{
    public required string Name { get; init; }
    public string? Title { get; init; }
    public string? Role { get; init; }
    public string? Policy { get; init; }
}
