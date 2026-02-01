using System.Collections.Concurrent;

namespace HRCE.Services.Logging;

public sealed class LogStore
{
    private readonly ConcurrentQueue<LogObservation> _entries = new();
    private readonly object _gate = new();
    private int _maxEntries;

    public LogStore(LogLearningOptions options)
    {
        _maxEntries = options.MaxRecentEntries;
    }

    public void AddRange(IEnumerable<LogObservation> entries)
    {
        lock (_gate)
        {
            foreach (var entry in entries)
            {
                _entries.Enqueue(entry);
            }

            while (_entries.Count > _maxEntries && _entries.TryDequeue(out _))
            {
            }
        }
    }

    public IReadOnlyList<LogObservation> GetRecent(int? take = null)
    {
        var snapshot = _entries.ToArray();
        Array.Reverse(snapshot);
        if (take.HasValue)
        {
            return snapshot.Take(take.Value).ToList();
        }

        return snapshot;
    }
}
