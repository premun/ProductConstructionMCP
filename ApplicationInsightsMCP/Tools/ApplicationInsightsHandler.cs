using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationInsightsMCP.Tools;

public class ApplicationInsightsHandler(
    IOptions<AppConfiguration> options,
    ILogger<ApplicationInsightsHandler> logger)
{
    private readonly IOptions<AppConfiguration> _options = options;
    private readonly ILogger<ApplicationInsightsHandler> _logger = logger;
    private readonly LogsQueryClient _logsQueryClient = new(new DefaultAzureCredential());

    /// <summary>
    /// Executes a Kusto KQL query against an Application Insights instance
    /// </summary>
    /// <param name="query">The Kusto KQL query to execute</param>
    /// <returns>The query results as a JSON string</returns>
    public async Task<string> ExecuteQuery(string query)
    {
        try
        {
            ValidateQueryParameters(query);

            // Execute the query using Azure Monitor Query SDK
            Response<LogsQueryResult> response = await _logsQueryClient.QueryResourceAsync(
                BuildAppInsightsResourceId(),
                query,
                new QueryTimeRange(TimeSpan.FromDays(30)));
            
            // Convert the result to JSON
            var result = ConvertQueryResultToJson(response.Value);
            _logger.LogInformation("Results converted to JSON, received {ResultLength} bytes", result.Length);

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

    private ResourceIdentifier BuildAppInsightsResourceId()
    {
        var appInsights = _options.Value.ApplicationInsights;
        return new ResourceIdentifier($"/subscriptions/{appInsights.SubscriptionId}/resourceGroups/{appInsights.ResourceGroup}/providers/Microsoft.Insights/components/{appInsights.ApplicationName}");
    }

    private string ConvertQueryResultToJson(LogsQueryResult queryResult)
    {
        try
        {
            // Extract the table data and convert to JSON
            var resultData = queryResult.Table.Rows.Select(row =>
            {
                var rowDict = new Dictionary<string, object?>();
                for (var i = 0; i < queryResult.Table.Columns.Count; i++)
                {
                    var column = queryResult.Table.Columns[i];

                    // Get the value based on column type
                    object? value;
                    if (column.Type == LogsColumnType.String)
                    {
                        value = row.GetString(i);
                    }
                    else if (column.Type == LogsColumnType.Int)
                    {
                        value = row.GetInt32(i);
                    }
                    else if (column.Type == LogsColumnType.Long)
                    {
                        value = row.GetInt64(i);
                    }
                    else if (column.Type == LogsColumnType.Real)
                    {
                        value = row.GetDouble(i);
                    }
                    else if (column.Type == LogsColumnType.Datetime)
                    {
                        value = row.GetDateTimeOffset(i);
                    }
                    else if (column.Type == LogsColumnType.Bool)
                    {
                        value = row.GetBoolean(i);
                    }
                    else if (column.Type == LogsColumnType.Dynamic)
                    {
                        try
                        {
                            value = JsonSerializer.Deserialize<object>(row.GetString(i));
                        }
                        catch
                        {
                            value = row.GetString(i); // Fallback to string if JSON parsing fails
                            _logger.LogDebug("Failed to parse JSON for column {ColumnName}: {value}", column.Name, value);
                        }
                    }
                    else
                    {
                        value = row.GetString(i); // Fallback to string for unknown types
                    }
                    rowDict[column.Name] = value;
                }
                return rowDict;
            }).ToList();

            return JsonSerializer.Serialize(resultData, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting query result to JSON");
            throw new InvalidOperationException("Failed to convert query result to JSON", ex);
        }
    }
}
