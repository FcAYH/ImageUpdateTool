using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ImageUpdateTool.Utils;

internal class CommandRunner
{
    public string ExecutablePath { get; }
    public string WorkingDirectory { get; }

    public CommandRunner(string executablePath, string? workingDirectory = null)
    {
        ExecutablePath = executablePath ?? throw new ArgumentNullException(nameof(executablePath));
        WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(executablePath);
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
        var process = new Process
        {
            StartInfo = info,
        };
        process.Start();
        process.WaitForExit();
        if (process.ExitCode == 0)
            return "";
                
        return process.StandardError.ReadToEnd();
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
        var process = new Process { StartInfo = info, };
        process.Start();

        // Read all lines of standard error asynchronously
        string line;
        // Declare a variable to store the error message
        string errorMessage = "";
        // Declare a variable to store the last progress value
        double lastProgress = 0;
        // Create a cancellation token source with 10 seconds timeout
        var cts = new CancellationTokenSource(10000);
        while ((line = await process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            Debug.WriteLine(line);
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
                process.Kill();
                break;
            }
        }

        // Wait for the process to exit asynchronously
        await process.WaitForExitAsync().ConfigureAwait(false);

        if (process.ExitCode == 0)
            return "";
        // Return the error message
        return errorMessage;
    }
}
