namespace HRCE.Services.Logging;

public sealed class LogLearningOptions
{
    public string LogFilePath { get; set; } = "logs/hrce.log";
    public string ModelPath { get; set; } = "App_Data/ml/log-model.zip";
    public string CorrelationPath { get; set; } = "App_Data/ml/correlations.jsonl";
    public int MaxBufferSize { get; set; } = 2000;
    public int MaxRecentEntries { get; set; } = 400;
    public int MinTrainingBatch { get; set; } = 80;
    public int PollIntervalSeconds { get; set; } = 4;
    public int CorrelationWindowSize { get; set; } = 8;
}
