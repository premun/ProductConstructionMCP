using ApplicationInsightsMCP;
using ApplicationInsightsMCP.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(b => b
    .AddConsole(o => o.FormatterName = CompactConsoleLoggerFormatter.FormatterName)
    .AddConsoleFormatter<CompactConsoleLoggerFormatter, SimpleConsoleFormatterOptions>()
    .SetMinimumLevel(LogLevel.Information));

builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args);

builder.Services.Configure<AppConfiguration>(builder.Configuration);
builder.Services.AddTransient<ApplicationInsightsApiHandler>();
builder.Services.AddTransient<ProcessRunner>();
builder.Services.AddTransient<KqlQueryLibrary>();

builder.Services.PostConfigure<AppConfiguration>(config =>
{
    config.RepositoryRoot ??= Helpers.FindRepositoryRoot();
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly()
    .WithPromptsFromAssembly();

await builder.Build().RunAsync();
