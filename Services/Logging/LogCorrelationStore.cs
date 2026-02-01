using System.Text.Json;

namespace HRCE.Services.Logging;

public sealed class LogCorrelationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false
    };

    private readonly string _path;

    public LogCorrelationStore(LogLearningOptions options)
    {
        _path = options.CorrelationPath;
        EnsureDirectory();
    }

    public async Task AppendAsync(LogCorrelation correlation, CancellationToken cancellationToken)
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(correlation, SerializerOptions);
        await File.AppendAllTextAsync(_path, json + Environment.NewLine, cancellationToken);
    }

    private void EnsureDirectory()
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}

public sealed record LogCorrelation
{
    public required DateTime Timestamp { get; init; }
    public required LogObservation Error { get; init; }
    public required IReadOnlyList<LogObservation> Context { get; init; }
}
