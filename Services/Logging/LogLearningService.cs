using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.ML;

namespace HRCE.Services.Logging;

public sealed class LogLearningService : BackgroundService
{
    private readonly ILogger<LogLearningService> _logger;
    private readonly LogLearningOptions _options;
    private readonly LogFileTailer _tailer;
    private readonly LogStore _store;
    private readonly LogCorrelationStore _correlationStore;
    private readonly List<LogObservation> _buffer = new();
    private readonly Queue<LogObservation> _recentWindow;
    private readonly MLContext _mlContext = new(seed: 42);

    public LogLearningService(
        ILogger<LogLearningService> logger,
        IOptions<LogLearningOptions> options,
        LogFileTailer tailer,
        LogStore store,
        LogCorrelationStore correlationStore)
    {
        _logger = logger;
        _options = options.Value;
        _tailer = tailer;
        _store = store;
        _correlationStore = correlationStore;
        _recentWindow = new Queue<LogObservation>(_options.CorrelationWindowSize);
        EnsureLogDirectories();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.PollIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var lines = await _tailer.ReadNewLinesAsync(_options.LogFilePath, stoppingToken);
                if (lines.Count > 0)
                {
                    var observations = Parse(lines);
                    if (observations.Count > 0)
                    {
                        _store.AddRange(observations);
                        AppendToBuffer(observations);
                        await StoreCorrelationsAsync(observations, stoppingToken);
                        TrainIfNeeded();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Log learning cycle failed");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private List<LogObservation> Parse(IReadOnlyList<string> lines)
    {
        var list = new List<LogObservation>();
        foreach (var line in lines)
        {
            try
            {
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                var observation = new LogObservation
                {
                    Timestamp = root.TryGetProperty("timestamp", out var timestamp) && timestamp.ValueKind == JsonValueKind.String
                        ? DateTime.Parse(timestamp.GetString() ?? DateTime.UtcNow.ToString())
                        : DateTime.UtcNow,
                    Level = root.TryGetProperty("level", out var level) ? level.GetString() ?? "INFO" : "INFO",
                    Message = root.TryGetProperty("message", out var message) ? message.GetString() ?? string.Empty : string.Empty,
                    Exception = root.TryGetProperty("exception", out var exception) ? exception.GetString() : null,
                    Logger = root.TryGetProperty("logger", out var logger) ? logger.GetString() : null,
                    EventId = root.TryGetProperty("eventId", out var eventId) ? eventId.GetString() : null,
                    EventName = root.TryGetProperty("eventName", out var eventName) ? eventName.GetString() : null,
                    Path = root.TryGetProperty("path", out var path) ? path.GetString() : null,
                    TraceId = root.TryGetProperty("traceId", out var traceId) ? traceId.GetString() : null
                };

                if (!string.IsNullOrWhiteSpace(observation.Message))
                {
                    list.Add(observation);
                }
            }
            catch (JsonException)
            {
                // ignore malformed log lines
            }
        }

        return list;
    }

    private void AppendToBuffer(IReadOnlyList<LogObservation> observations)
    {
        _buffer.AddRange(observations);
        if (_buffer.Count > _options.MaxBufferSize)
        {
            _buffer.RemoveRange(0, _buffer.Count - _options.MaxBufferSize);
        }
    }

    private async Task StoreCorrelationsAsync(IReadOnlyList<LogObservation> observations, CancellationToken cancellationToken)
    {
        foreach (var observation in observations)
        {
            if (_recentWindow.Count >= _options.CorrelationWindowSize)
            {
                _recentWindow.Dequeue();
            }

            _recentWindow.Enqueue(observation);

            if (observation.IsError)
            {
                var context = _recentWindow.ToList();
                var correlation = new LogCorrelation
                {
                    Timestamp = observation.Timestamp,
                    Error = observation,
                    Context = context
                };
                await _correlationStore.AppendAsync(correlation, cancellationToken);
            }
        }
    }

    private void TrainIfNeeded()
    {
        if (_buffer.Count < _options.MinTrainingBatch)
        {
            return;
        }

        try
        {
            var data = _mlContext.Data.LoadFromEnumerable(_buffer.Select(entry => new LogLearningInput
            {
                Message = entry.Message,
                Exception = entry.Exception ?? string.Empty,
                Level = entry.Level,
                Logger = entry.Logger ?? string.Empty,
                IsError = entry.IsError
            }));

            var pipeline = _mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "Features",
                    inputColumnName: nameof(LogLearningInput.Message))
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "ExceptionFeatures",
                    inputColumnName: nameof(LogLearningInput.Exception)))
                .Append(_mlContext.Transforms.Text.FeaturizeText(
                    outputColumnName: "LoggerFeatures",
                    inputColumnName: nameof(LogLearningInput.Logger)))
                .Append(_mlContext.Transforms.Concatenate("Features", "Features", "ExceptionFeatures", "LoggerFeatures"))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

            var model = pipeline.Fit(data);

            EnsureModelDirectory();
            _mlContext.Model.Save(model, data.Schema, _options.ModelPath);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Model training skipped");
        }
    }

    private void EnsureLogDirectories()
    {
        var logDirectory = Path.GetDirectoryName(_options.LogFilePath);
        if (!string.IsNullOrWhiteSpace(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    private void EnsureModelDirectory()
    {
        var directory = Path.GetDirectoryName(_options.ModelPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private sealed class LogLearningInput
    {
        public required string Message { get; init; }
        public required string Exception { get; init; }
        public required string Level { get; init; }
        public required string Logger { get; init; }
        public bool IsError { get; init; }
    }
}
