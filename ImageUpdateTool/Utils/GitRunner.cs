using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImageUpdateTool.Utils
{
    internal partial class GitRunner
    {
        public readonly string ExecutablePath = "git";
        public string WorkingDirectory
        {
            get
            {
                return _process != null ? _process.StartInfo.WorkingDirectory : string.Empty;
            }
            set
            {
                if (_process == null)
                    return;

                _process.StartInfo.WorkingDirectory = value;
            }
        }

        private readonly Process _process;

        [GeneratedRegex("(\\d+)%")]
        private static partial Regex ProgressRegex();

        public GitRunner(string workingDirectory)
        {
            if (!Directory.Exists(workingDirectory))
            {
                throw new ArgumentException($"workingDirectory:{workingDirectory} is not exist!");
            }

            _process = new Process();
            _process.StartInfo.FileName = ExecutablePath;
            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.WorkingDirectory = WorkingDirectory;
        }

        private async Task<string> RunAsync(string arguments, IProgress<double> progress = null)
        {
            _process.StartInfo.Arguments = arguments;
            _process.Start();


            string line;
            string errorMessage = "";
            double lastProgress = 0;

            var cts = new CancellationTokenSource(10000);
            while ((line = await _process.StandardError.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                // 在标准错误输出中匹配进度数据
                var match = ProgressRegex().Match(line);
                if (progress != null && match.Success)
                {
                    var number = double.Parse(match.Groups[1].Value) / 100;
                    progress.Report(number);

                    if (Math.Abs(number - lastProgress) >= 1e-3)
                    {
                        lastProgress = number;
                        cts.CancelAfter(10000);
                    }
                }
                else
                {
                    // 将非进度信息的标准错误输出信息保存下来，在运行失败后返回
                    errorMessage += line + "\n";
                }

                // 如果10s内进度没有变化，可能是网络连接断开，杀死进程
                // 这样做是因为在git clone时断开网络居然不会报错，而是卡住，等网络回复连接后自动继续下载
                // 但应该是我代码写丑了，使得这种情况会把UI卡死，所以加了一个10s的限制。
                if (cts.IsCancellationRequested)
                {
                    errorMessage += "The progress hasn't changed for 10s, maybe the Network Connection is lost!\n";
                    _process.Kill(); // kill 时 ExitCode 为 -1
                    break;
                }
            }

            await _process.WaitForExitAsync().ConfigureAwait(false);

            if (_process.ExitCode == 0)
                return "";

            return errorMessage;
        }

        public async Task<string> PullAsync(IProgress<double> progress = null)
        {
            return await RunAsync($"pull", progress).ConfigureAwait(false);
        }

        public async Task<string> CloneAsync(string url, IProgress<double> progress = null)
        {
            return await RunAsync($"clone {url}", progress).ConfigureAwait(false);
        }

        public async Task<string> AddAsync(string path)
        {
            return await RunAsync($"add \"{path}\"").ConfigureAwait(false);
        }

        public async Task<string> CommitAsync(string message)
        {
            return await RunAsync($"commit -m \"{message}\"").ConfigureAwait(false);
        }

        public async Task<string> PushAsync(IProgress<double> progress = null)
        {
            return await RunAsync($"push", progress).ConfigureAwait(false);
        }

        public async Task<string> RemoveAsync(string path)
        {
            return await RunAsync($"rm \"{path}\"").ConfigureAwait(false);
        }
    }
}
