using System.Diagnostics;

var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
var hrcePath = root;

Console.WriteLine($"HRCE Root: {hrcePath}");
Console.WriteLine("Starting HRCE with 'dotnet run'...");

using var runProcess = StartProcess("dotnet", "run", hrcePath);
await Task.Delay(TimeSpan.FromSeconds(6));

Console.WriteLine("Triggering build to reproduce lock scenario...");
using var buildProcess = StartProcess("dotnet", "build", hrcePath);
await Task.Delay(TimeSpan.FromSeconds(6));

Console.WriteLine("Repro finished. Press Enter to stop child processes.");
Console.ReadLine();

TryStop(runProcess);
TryStop(buildProcess);

static Process StartProcess(string fileName, string arguments, string workingDirectory)
{
    var process = new Process
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        }
    };

    process.OutputDataReceived += (_, args) =>
    {
        if (!string.IsNullOrWhiteSpace(args.Data))
        {
            Console.WriteLine(args.Data);
        }
    };

    process.ErrorDataReceived += (_, args) =>
    {
        if (!string.IsNullOrWhiteSpace(args.Data))
        {
            Console.Error.WriteLine(args.Data);
        }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    return process;
}

static void TryStop(Process? process)
{
    if (process is null || process.HasExited)
    {
        return;
    }

    try
    {
        process.Kill(true);
    }
    catch
    {
    }
}
