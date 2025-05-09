using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ApplicationInsightsMCP.Tools;

/// <summary>
/// MCP tool for executing Application Insights queries using local Azure CLI credentials
/// </summary>
[McpServerToolType]
public static class ApplicationInsightsTools
{
    /// <summary>
    /// Executes a Kusto KQL query against an Application Insights instance
    /// </summary>
    /// <param name="query">The Kusto KQL query to execute</param>
    /// <returns>The query results as a JSON string</returns>
    [McpServerTool, Description("Executes a Kusto KQL query against the configured Application Insights instance")]
    public static async Task<string> ExecuteApplicationInsightsQuery(ApplicationInsightsHandler appInsightsHandler, string query)
    {
        return await appInsightsHandler.ExecuteQuery(query);
    }
}