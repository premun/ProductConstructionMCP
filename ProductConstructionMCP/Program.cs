using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductConstructionMCP;
using System.IO;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add configuration from multiple sources in order of precedence
builder.Configuration
    // Load from your knowledge-base/configuration.json file (lowest precedence)
    .AddJsonFile(Path.Combine("knowledge-base", "configuration.json"), optional: true, reloadOnChange: true)
    // Load from appsettings.json and environment-specific appsettings files (e.g., appsettings.Development.json)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    // Load from environment variables (higher precedence)
    .AddEnvironmentVariables()
    // Load from command-line arguments (highest precedence)
    .AddCommandLine(args);

// Register the strongly-typed configuration classes
builder.Services.Configure<AppConfiguration>(builder.Configuration);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
