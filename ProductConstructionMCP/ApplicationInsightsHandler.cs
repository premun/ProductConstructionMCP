using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductConstructionMCP;

public class ApplicationInsightsHandler(
    IOptions<AppConfiguration> options,
     ILogger<ApplicationInsightsHandler> logger)
{
    private readonly IOptions<AppConfiguration> _options = options;
    private readonly ILogger<ApplicationInsightsHandler> _logger = logger;

    /// <summary>
    /// Executes a Kusto KQL query against an Application Insights instance
    /// </summary>
    /// <param name="query">The Kusto KQL query to execute</param>
    /// <returns>The query results as a JSON string</returns>
    [McpServerTool, Description("Executes a Kusto KQL query against the configured Application Insights instance")]
    public async Task<string> ExecuteQuery(string query)
    {
        try
        {
            ValidateQueryParameters(query);
            _logger.LogInformation("Query parameters validated successfully");

            // Ensure the user is logged in to Azure CLI
            _logger.LogInformation("Checking Azure CLI login status");
            await EnsureAzureCliLogin();
            _logger.LogInformation("Azure CLI login confirmed");

            // Prepare the Azure CLI command to execute the query
            string escapedQuery = query!.Replace("\"", "\\\"");
            string azCliCommand = BuildAzQueryCommand(escapedQuery);
            _logger.LogInformation("Built Azure CLI command: {Command}", azCliCommand);

            // Execute the command using the Helpers class
            _logger.LogInformation("Executing Azure CLI command");
            var result = await Helpers.ExecuteCommandAsync(azCliCommand, _logger);
            _logger.LogInformation("Command executed, received {ResultLength} bytes", result.Length);

            // Parse and validate the JSON result
            ValidateJsonResult(result);
            _logger.LogInformation("JSON result validated successfully");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {ErrorMessage}", ex.Message);
            return JsonSerializer.Serialize(new
            {
                error = true,
                message = ex.Message,
                details = ex.ToString()
            });
        }
    }

    /// <summary>
    /// Validates that all required parameters for executing a query are provided
    /// </summary>
    /// <param name="query">The query to validate</param>
    private void ValidateQueryParameters(string? query)
    {
        if (string.IsNullOrEmpty(_options.Value.ApplicationInsights.ApplicationName))
        {
            throw new ArgumentException("Application Insights application name not configured. Please check your configuration.");
        }

        if (string.IsNullOrEmpty(_options.Value.ApplicationInsights.ResourceGroup))
        {
            throw new ArgumentException("Application Insights resource group not configured. Please check your configuration.");
        }

        if (string.IsNullOrEmpty(_options.Value.ApplicationInsights.SubscriptionId))
        {
            throw new ArgumentException("Application Insights subscription ID not configured. Please check your configuration.");
        }

        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentException("Query cannot be empty", nameof(query));
        }
    }

    /// <summary>
    /// Builds the Azure CLI command for querying Application Insights based on available configuration
    /// </summary>
    /// <param name="escapedQuery">The escaped query string</param>
    /// <returns>The complete Azure CLI command</returns>
    private string BuildAzQueryCommand(string escapedQuery)
    {
        var appInsights = _options.Value.ApplicationInsights;

        string baseCommand = $"az monitor app-insights query --app {appInsights.ApplicationName}";

        // Add resource group if available
        if (!string.IsNullOrEmpty(appInsights.ResourceGroup))
        {
            baseCommand += $" --resource-group {appInsights.ResourceGroup}";
        }
        
        // Add subscription if available
        if (!string.IsNullOrEmpty(appInsights.SubscriptionId))
        {
            baseCommand += $" --subscription {appInsights.SubscriptionId}";
        }
        
        // Add query and timespan
        baseCommand += $" --analytics-query \"{escapedQuery}\"";
        
        return baseCommand;
    }

    /// <summary>
    /// Ensures the user is logged in to Azure CLI
    /// </summary>
    private async Task EnsureAzureCliLogin()
    {
        try
        {
            // Check if the user is already logged in
            _logger.LogInformation("Checking Azure CLI login status with 'az account show'");
            string accountOutput = await Helpers.ExecuteCommandAsync("az account show", _logger);
            
            // If we reach here, the user is logged in
            // Parse the JSON to get the current account info
            using var doc = JsonDocument.Parse(accountOutput);
            var name = doc.RootElement.GetProperty("name").GetString();
            
            _logger.LogInformation("Using Azure account: {AccountName}", name);
        }
        catch (Exception ex)
        {
            // If az account show fails, the user is not logged in
            _logger.LogError(ex, "Azure CLI login check failed");
            throw new InvalidOperationException("Not logged in to Azure CLI. Please run 'az login' before using this tool.");
        }
    }

    private void ValidateJsonResult(string result)
    {
        try
        {
            _logger.LogInformation("Validating JSON result");
            JsonDocument.Parse(result);
            _logger.LogInformation("JSON result is valid");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON result received");
            throw new InvalidOperationException("The query did not return valid JSON data");
        }
    }
}
