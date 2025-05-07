using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

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
    /// <returns>The command output as a string</returns>
    public static async Task<string> ExecuteCommandAsync(string command)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c {command}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = processStartInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                output.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
            {
                error.AppendLine(args.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Command failed with exit code {process.ExitCode}: {error}");
        }

        return output.ToString().Trim();
    }
}