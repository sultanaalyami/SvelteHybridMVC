using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Options;

namespace HRCE.Services.BuildOperations;

public sealed class BuildOperationsService
{
    private readonly BuildOperationsOptions _options;
    private readonly ILogger<BuildOperationsService> _logger;
    private readonly ConcurrentDictionary<Guid, BuildOperationJob> _jobs = new();
    private readonly ConcurrentQueue<Guid> _history = new();
    private readonly object _historyGate = new();

    public BuildOperationsService(IOptions<BuildOperationsOptions> options, ILogger<BuildOperationsService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public IReadOnlyList<BuildOperationCommand> GetCommands() => _options.Commands;

    public IReadOnlyList<BuildOperationJob> GetJobs()
    {
        return _history.Select(id => _jobs.TryGetValue(id, out var job) ? job : null)
            .Where(job => job is not null)
            .Cast<BuildOperationJob>()
            .OrderByDescending(job => job.CreatedAtUtc)
            .ToList();
    }

    public BuildOperationJob? GetJob(Guid id) => _jobs.TryGetValue(id, out var job) ? job : null;

    public Task<BuildOperationJob?> StartAsync(string commandKey)
    {
        var command = _options.Commands.FirstOrDefault(cmd => string.Equals(cmd.Key, commandKey, StringComparison.OrdinalIgnoreCase));
        if (command is null)
        {
            return Task.FromResult<BuildOperationJob?>(null);
        }

        var job = new BuildOperationJob
        {
            Id = Guid.NewGuid(),
            CommandKey = command.Key,
            Label = command.Label,
            Description = command.Description,
            CreatedAtUtc = DateTime.UtcNow,
            State = BuildOperationState.Pending
        };

        _jobs[job.Id] = job;
        TrackHistory(job.Id);

        _ = Task.Run(() => ExecuteAsync(job.Id, command), CancellationToken.None);

        return Task.FromResult<BuildOperationJob?>(job);
    }

    public bool Cancel(Guid id)
    {
        if (!_jobs.TryGetValue(id, out var job))
        {
            return false;
        }

        if (job.State is BuildOperationState.Succeeded or BuildOperationState.Failed or BuildOperationState.Canceled)
        {
            return false;
        }

        job.State = BuildOperationState.Canceled;
        job.CompletedAtUtc = DateTime.UtcNow;
        job.Error = job.Error ?? "Canceled by user.";
        return true;
    }

    private async Task ExecuteAsync(Guid jobId, BuildOperationCommand command)
    {
        if (!_jobs.TryGetValue(jobId, out var job))
        {
            return;
        }

        if (job.State == BuildOperationState.Canceled)
        {
            return;
        }

        job.State = BuildOperationState.Running;
        job.StartedAtUtc = DateTime.UtcNow;

        var output = new List<string>();
        var workingDirectory = ResolveWorkingDirectory(command.WorkingDirectory);
        var startInfo = new ProcessStartInfo
        {
            FileName = command.FileName,
            Arguments = command.Arguments ?? string.Empty,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            var outputLock = new object();

            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    lock (outputLock)
                    {
                        AppendOutput(job, output, args.Data);
                    }
                }
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrWhiteSpace(args.Data))
                {
                    lock (outputLock)
                    {
                        AppendOutput(job, output, args.Data);
                    }
                }
            };

            if (!process.Start())
            {
                job.State = BuildOperationState.Failed;
                job.CompletedAtUtc = DateTime.UtcNow;
                job.Error = "Failed to start process.";
                job.Output = output.ToArray();
                return;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            while (!process.HasExited)
            {
            if (job.State == BuildOperationState.Canceled)
            {
                TryStop(process);
                break;
            }

                await Task.Delay(300);
            }

            job.ExitCode = process.HasExited ? process.ExitCode : null;
            if (job.State == BuildOperationState.Canceled)
            {
                job.CompletedAtUtc = DateTime.UtcNow;
                job.Output = output.ToArray();
                return;
            }

            job.State = process.ExitCode == 0 ? BuildOperationState.Succeeded : BuildOperationState.Failed;
            job.CompletedAtUtc = DateTime.UtcNow;
            job.Output = output.ToArray();
            if (job.State == BuildOperationState.Failed && string.IsNullOrWhiteSpace(job.Error))
            {
                job.Error = $"Exit code {process.ExitCode}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Build operation failed: {Key}", command.Key);
            job.State = BuildOperationState.Failed;
            job.CompletedAtUtc = DateTime.UtcNow;
            job.Error = ex.Message;
            job.Output = output.ToArray();
        }
    }

    private string ResolveWorkingDirectory(string? workingDirectory)
    {
        var root = string.IsNullOrWhiteSpace(_options.RootPath) ? AppContext.BaseDirectory : _options.RootPath;
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return root;
        }

        return Path.IsPathRooted(workingDirectory)
            ? workingDirectory
            : Path.Combine(root, workingDirectory);
    }

    private void AppendOutput(BuildOperationJob job, List<string> output, string line)
    {
        output.Add(line);
        var max = Math.Max(10, _options.MaxOutputLines);
        if (output.Count > max)
        {
            output.RemoveRange(0, output.Count - max);
        }
        job.Output = output.ToArray();
    }

    private void TrackHistory(Guid id)
    {
        lock (_historyGate)
        {
            _history.Enqueue(id);
            while (_history.Count > Math.Max(1, _options.MaxHistory))
            {
                _history.TryDequeue(out _);
            }
        }
    }

    private static void TryStop(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(true);
            }
        }
        catch
        {
        }
    }
}
