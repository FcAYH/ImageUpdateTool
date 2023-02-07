using System.Diagnostics;

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
}
