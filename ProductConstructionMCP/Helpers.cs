using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ProductConstructionMCP;

/// <summary>
/// Helper methods for common functionality used across the application
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Executes a command line process and returns the output
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="logger">Optional logger to log details of command execution</param>
    /// <returns>The command output as a string</returns>
    public static async Task<string> ExecuteCommandAsync(string command, ILogger? logger = null)
    {
        logger?.LogDebug("Executing command: {Command}", command);
        
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        logger?.LogTrace("Process start info configured with UseShellExecute={UseShellExecute}, CreateNoWindow={CreateNoWindow}", 
            processStartInfo.UseShellExecute, processStartInfo.CreateNoWindow);

        var process = new Process { StartInfo = processStartInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                logger?.LogTrace("Process output: {Output}", args.Data);
                output.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                logger?.LogTrace("Process error: {Error}", args.Data);
                error.AppendLine(args.Data);
            }
        };

        try
        {
            logger?.LogDebug("Starting process");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            logger?.LogDebug("Waiting for process to exit");
            await process.WaitForExitAsync();

            logger?.LogDebug("Process exited with code {ExitCode}", process.ExitCode);

            if (process.ExitCode != 0)
            {
                var errorMsg = $"Command failed with exit code {process.ExitCode}: {error}";
                logger?.LogError(errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

            var result = output.ToString().Trim();
            logger?.LogDebug("Command completed successfully, output length: {OutputLength} characters", result.Length);
            return result;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            var errorMsg = $"Exception executing command: {ex.Message}";
            logger?.LogError(ex, errorMsg);
            throw new InvalidOperationException(errorMsg, ex);
        }
    }
}