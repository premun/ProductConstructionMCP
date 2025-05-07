using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;

namespace ProductConstructionMCP;

/// <summary>
/// MCP tool for executing Application Insights queries using local Azure CLI credentials
/// </summary>
[McpServerToolType]
public class ApplicationInsightsQuery
{
    /// <summary>
    /// Executes a Kusto KQL query against an Application Insights instance
    /// </summary>
    /// <param name="query">The Kusto KQL query to execute</param>
    /// <returns>The query results as a JSON string</returns>
    [McpServerTool, Description("Executes a Kusto KQL query against the configured Application Insights instance")]
    public static async Task<string> ExecuteQuery(string query, IOptions<AppConfiguration> options, ILogger<ApplicationInsightsQuery> logger)
    {
        logger.LogInformation("ExecuteQuery called with query: {QueryLength} chars", query?.Length ?? 0);
        
        try
        {
            ValidateQueryParameters(query, options, logger);
            logger.LogInformation("Query parameters validated successfully");

            // Ensure the user is logged in to Azure CLI
            logger.LogInformation("Checking Azure CLI login status");
            await EnsureAzureCliLogin(logger);
            logger.LogInformation("Azure CLI login confirmed");

            // Prepare the Azure CLI command to execute the query
            string escapedQuery = query!.Replace("\"", "\\\"");
            string azCliCommand = BuildAzQueryCommand(escapedQuery, options);
            logger.LogInformation("Built Azure CLI command: {Command}", azCliCommand);

            // Execute the command using the Helpers class
            logger.LogInformation("Executing Azure CLI command");
            var result = await Helpers.ExecuteCommandAsync(azCliCommand, logger);
            logger.LogInformation("Command executed, received {ResultLength} bytes", result.Length);

            // Parse and validate the JSON result
            ValidateJsonResult(result, logger);
            logger.LogInformation("JSON result validated successfully");

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing query: {ErrorMessage}", ex.Message);
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
    private static void ValidateQueryParameters(string? query, IOptions<AppConfiguration> options, ILogger<ApplicationInsightsQuery> logger)
    {
        logger.LogInformation("Validating query parameters");
        
        if (string.IsNullOrEmpty(options.Value.ApplicationInsights.ApplicationName))
        {
            logger.LogError("Application Insights application name not configured");
            throw new ArgumentException("Application Insights application name not configured. Please check your configuration.");
        }

        if (string.IsNullOrEmpty(options.Value.ApplicationInsights.ResourceGroup))
        {
            logger.LogError("Application Insights resource group not configured");
            throw new ArgumentException("Application Insights resource group not configured. Please check your configuration.");
        }

        if (string.IsNullOrEmpty(options.Value.ApplicationInsights.SubscriptionId))
        {
            logger.LogError("Application Insights subscription ID not configured");
            throw new ArgumentException("Application Insights subscription ID not configured. Please check your configuration.");
        }

        if (string.IsNullOrEmpty(query))
        {
            logger.LogError("Query is empty");
            throw new ArgumentException("Query cannot be empty", nameof(query));
        }
    }

    /// <summary>
    /// Builds the Azure CLI command for querying Application Insights based on available configuration
    /// </summary>
    /// <param name="escapedQuery">The escaped query string</param>
    /// <returns>The complete Azure CLI command</returns>
    private static string BuildAzQueryCommand(string escapedQuery, IOptions<AppConfiguration> options)
    {
        var appInsights = options.Value.ApplicationInsights;

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
    private static async Task EnsureAzureCliLogin(ILogger<ApplicationInsightsQuery> logger)
    {
        try
        {
            // Check if the user is already logged in
            logger.LogInformation("Checking Azure CLI login status with 'az account show'");
            string accountOutput = await Helpers.ExecuteCommandAsync("az account show", logger);
            
            // If we reach here, the user is logged in
            // Parse the JSON to get the current account info
            using var doc = JsonDocument.Parse(accountOutput);
            var name = doc.RootElement.GetProperty("name").GetString();
            
            logger.LogInformation("Using Azure account: {AccountName}", name);
        }
        catch (Exception ex)
        {
            // If az account show fails, the user is not logged in
            logger.LogError(ex, "Azure CLI login check failed");
            throw new InvalidOperationException("Not logged in to Azure CLI. Please run 'az login' before using this tool.");
        }
    }

    private static void ValidateJsonResult(string result, ILogger<ApplicationInsightsQuery> logger)
    {
        try
        {
            logger.LogInformation("Validating JSON result");
            JsonDocument.Parse(result);
            logger.LogInformation("JSON result is valid");
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid JSON result received");
            throw new InvalidOperationException("The query did not return valid JSON data");
        }
    }
}