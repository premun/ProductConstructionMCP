using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationInsightsMCP;

/// <summary>
/// Helper methods for common functionality used across the application
/// </summary>
public class ProcessRunner(ILogger<ProcessRunner> logger)
{
    private readonly ILogger<ProcessRunner> _logger = logger;

    /// <summary>
    /// Executes a command line process and returns the output
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="_logger">Optional logger to log details of command execution</param>
    /// <returns>The command output as a string</returns>
    public async Task<string> ExecuteCommandAsync(string command)
    {
        _logger.LogDebug("Executing command: {Command}", command);
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _logger.LogTrace("Process start info configured with UseShellExecute={UseShellExecute}, CreateNoWindow={CreateNoWindow}",
            processStartInfo.UseShellExecute, processStartInfo.CreateNoWindow);

        var process = new Process { StartInfo = processStartInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogTrace("Process output: {Output}", args.Data);
                output.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                _logger.LogTrace("Process error: {Error}", args.Data);
                error.AppendLine(args.Data);
            }
        };

        try
        {
            _logger.LogDebug("Starting process");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            _logger.LogDebug("Waiting for process to exit");
            await process.WaitForExitAsync();

            _logger.LogDebug("Process exited with code {ExitCode}", process.ExitCode);

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}: {error}");
            }

            var result = output.ToString().Trim();
            return result;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            var errorMsg = $"Exception executing command: {ex.Message}";
            _logger.LogError(ex, errorMsg);
            throw new InvalidOperationException(errorMsg, ex);
        }
    }
}