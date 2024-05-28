using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ImageUpdateTool.Utils;

internal class CommandRunner
{
    private static List<CommandRunner> _runnerList = new List<CommandRunner>();

    public string ExecutablePath { get; }
    public string WorkingDirectory { get; }

    private Process _process;

#nullable enable
    public CommandRunner(string executablePath, string? workingDirectory = null)
    {
        ExecutablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
        WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(executablePath);

        _runnerList.Add(this);
    }
#nullable disable

    public static void CloseAll()
    {
        foreach (var runner in _runnerList)
        {
            if (runner._process != null)
            {
                runner._process.Kill();
            }
        }
    }

    public string Run(string arguments)
    {
        var info = new ProcessStartInfo(ExecutablePath, arguments)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = WorkingDirectory,
        };
        _process = new Process
        {
            StartInfo = info,
        };
        _process.Start();
        _process.WaitForExit();
        if (_process.ExitCode == 0)
            return "";

        return _process.StandardError.ReadToEnd();
    }

    public async Task<string> RunAsync(string arguments, IProgress<double> progress = null)
    {
        var info = new ProcessStartInfo(ExecutablePath, arguments)
        {
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = WorkingDirectory,
        };
        _process = new Process { StartInfo = info, };
        _process.Start();

        // Read all lines of standard error asynchronously
        string line;
        // Declare a variable to store the error message
        string errorMessage = "";
        // Declare a variable to store the last progress value
        double lastProgress = 0;
        // Create a cancellation token source with 10 seconds timeout
        var cts = new CancellationTokenSource(10000);
        while ((line = await _process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            // Extract the progress data from the line
            var match = Regex.Match(line, @"(\d+)%");
            // Check if the match is successful
            if (match.Success)
            {
                // Get the progress value
                var number = double.Parse(match.Groups[1].Value) / 100;
                // Do something with the progress value
                progress.Report(number);
                // Update the last progress value
                lastProgress = number;
                // Reset the cancellation token source
                cts.CancelAfter(10000);
            }
            else
            {
                // Append the line to the error message
                errorMessage += line + "\n";
            }
            // Check if the cancellation token is triggered
            if (cts.IsCancellationRequested)
            {
                // Add internet error to the error message
                errorMessage += "Internet Error!\n";
                // Kill the process
                _process.Kill();
                break;
            }
        }

        // Wait for the process to exit asynchronously
        await _process.WaitForExitAsync().ConfigureAwait(false);

        if (_process.ExitCode == 0)
            return "";
        // Return the error message
        return errorMessage;
    }
}
