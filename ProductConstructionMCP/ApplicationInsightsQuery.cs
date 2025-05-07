using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ProductConstructionMCP;

/// <summary>
/// MCP tool for executing Application Insights queries using local Azure CLI credentials
/// </summary>
[McpServerToolType]
public class ApplicationInsightsQuery(IOptions<AppConfiguration> options)
{
    private static readonly Regex TimespanRegex = new(@"^P(?:(\d+)D)?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly AppConfiguration _config = options.Value;

    /// <summary>
    /// Executes a Kusto KQL query against an Application Insights instance
    /// </summary>
    /// <param name="query">The Kusto KQL query to execute</param>
    /// <param name="timespan">Optional timespan for the query (e.g. "P1D" for last 24 hours). Default is P1D.</param>
    /// <returns>The query results as a JSON string</returns>
    [McpServerTool, Description("Executes a Kusto KQL query against the configured Application Insights instance")]
    public async Task<string> ExecuteQuery(string query, string timespan = "P1D")
    {
        // Check for required configuration values
        if (string.IsNullOrEmpty(_config.ApplicationInsights.ApplicationName))
        {
            throw new InvalidOperationException("Application Insights application name not configured. Please check your configuration.");
        }

        if (string.IsNullOrEmpty(query))
        {
            throw new ArgumentException("Query cannot be empty", nameof(query));
        }

        // Validate the timespan format
        if (!IsValidTimespan(timespan))
        {
            throw new ArgumentException("Invalid timespan format. Use ISO 8601 duration format (e.g., 'P1D', 'PT12H')", nameof(timespan));
        }

        try
        {
            // Ensure the user is logged in to Azure CLI
            await EnsureAzureCliLogin();

            // Prepare the Azure CLI command to execute the query
            string escapedQuery = query.Replace("\"", "\\\"");
            string azCliCommand = BuildAzQueryCommand(escapedQuery, timespan);

            // Execute the command using the Helpers class
            var result = await Helpers.ExecuteCommandAsync(azCliCommand);

            // Parse and validate the JSON result
            ValidateJsonResult(result);

            return result;
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                error = true,
                message = ex.Message,
                details = ex.ToString()
            });
        }
    }

    /// <summary>
    /// Builds the Azure CLI command for querying Application Insights based on available configuration
    /// </summary>
    /// <param name="escapedQuery">The escaped query string</param>
    /// <param name="timespan">The timespan for the query</param>
    /// <returns>The complete Azure CLI command</returns>
    private string BuildAzQueryCommand(string escapedQuery, string timespan)
    {
        var appInsights = _config.ApplicationInsights;
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
        baseCommand += $" --analytics-query \"{escapedQuery}\" --timespan {timespan}";
        
        return baseCommand;
    }

    /// <summary>
    /// Ensures the user is logged in to Azure CLI
    /// </summary>
    private static async Task EnsureAzureCliLogin()
    {
        try
        {
            // Check if the user is already logged in
            string accountOutput = await Helpers.ExecuteCommandAsync("az account show");
            
            // If we reach here, the user is logged in
            // Parse the JSON to get the current account info
            using var doc = JsonDocument.Parse(accountOutput);
            var name = doc.RootElement.GetProperty("name").GetString();
            
            Console.Error.WriteLine($"Using Azure account: {name}");
        }
        catch
        {
            // If az account show fails, the user is not logged in
            throw new InvalidOperationException("Not logged in to Azure CLI. Please run 'az login' before using this tool.");
        }
    }
    private static bool IsValidTimespan(string timespan) => TimespanRegex.IsMatch(timespan);

    private static void ValidateJsonResult(string result)
    {
        try
        {
            JsonDocument.Parse(result);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("The query did not return valid JSON data");
        }
    }
}