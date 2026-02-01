namespace HRCE.Services.Builder;

public sealed record PageBuildResult
{
    public required bool Created { get; init; }
    public required string ControllerPath { get; init; }
    public required string ViewPath { get; init; }
}
