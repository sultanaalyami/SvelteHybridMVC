namespace HRCE.Services.Logging;

public sealed class LogFileTailer
{
    private long _position;

    public async Task<IReadOnlyList<string>> ReadNewLinesAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            return Array.Empty<string>();
        }

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        stream.Seek(_position, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);

        var lines = new List<string>();
        while (!reader.EndOfStream)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        _position = stream.Position;
        return lines;
    }
}
