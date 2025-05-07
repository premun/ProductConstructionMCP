using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductConstructionMCP;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Debug; // Changed from Trace to Debug
});

// Add Debug provider for more detailed logs
builder.Logging.AddDebug();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args);

// Bind the root configuration section to AppConfiguration
builder.Services.Configure<AppConfiguration>(c =>
{
    c.ApplicationInsights.SubscriptionId = "e6b5f9f5-0ca4-4351-879b-014d78400ec2";
    c.ApplicationInsights.ResourceGroup = "product-construction-service";
    c.ApplicationInsights.ApplicationName = "product-construction-service-ai-int";
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
