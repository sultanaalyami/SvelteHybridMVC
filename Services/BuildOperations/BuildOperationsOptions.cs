namespace HRCE.Services.BuildOperations;

public sealed class BuildOperationsOptions
{
    public string RootPath { get; set; } = string.Empty;
    public int MaxOutputLines { get; set; } = 200;
    public int MaxHistory { get; set; } = 20;
    public List<BuildOperationCommand> Commands { get; set; } = new();
}

public sealed class BuildOperationCommand
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Arguments { get; set; }
    public string? WorkingDirectory { get; set; }
}
