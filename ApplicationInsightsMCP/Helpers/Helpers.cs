using System;
using System.IO;

namespace ApplicationInsightsMCP.Helpers;

internal static class Helpers
{
    public static string FindRepositoryRoot()
    {
        // Check if REPOSITORY_ROOT environment variable is set
        string? envRepoRoot = Environment.GetEnvironmentVariable("REPOSITORY_ROOT");
        if (!string.IsNullOrEmpty(envRepoRoot) && Directory.Exists(envRepoRoot))
        {
            return envRepoRoot;
        }
        
        // Start with the current directory
        var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
        
        // Look for common repository indicators
        while (currentDir != null && currentDir.Exists)
        {
            // Check for .git directory (Git)
            if (Directory.Exists(Path.Combine(currentDir.FullName, ".git")))
            {
                return currentDir.FullName;
            }

            currentDir = currentDir.Parent;
        }

        throw new InvalidOperationException($"No git repository root found for '{Directory.GetCurrentDirectory()}'. Ensure you are in a git repository.");
    }
}