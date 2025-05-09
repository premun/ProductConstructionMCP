using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace ApplicationInsightsMCP.Tools;

/// <summary>
/// MCP tools for accessing and executing queries from the KQL knowledge base
/// </summary>
[McpServerToolType]
public static class KqlQueryLibraryTools
{
    /// <summary>
    /// Lists all available categories and queries in the knowledge base
    /// </summary>
    /// <returns>JSON object containing categories and their queries</returns>
    [McpServerTool, Description("Lists all available KQL query categories and queries in the knowledge base")]
    public static object ListKqlQueries(KqlQueryLibrary queryLibrary)
    {
        var result = new Dictionary<string, IEnumerable<string>>();

        foreach (var category in queryLibrary.GetCategories())
        {
            result[category] = queryLibrary.GetQueriesInCategory(category);
        }

        return result;
    }

    /// <summary>
    /// Gets the raw KQL query text from the knowledge base
    /// </summary>
    /// <param name="category">The category of the query</param>
    /// <param name="queryName">The name of the query file (including .kql extension)</param>
    /// <returns>The KQL query text</returns>
    [McpServerTool, Description("Gets the raw KQL query text from a knowledge base entry")]
    public static string GetQueryText(KqlQueryLibrary queryLibrary, string category, string queryName)
    {
        return queryLibrary.GetQueryText(category, queryName);
    }

    /// <summary>
    /// Executes a query from the knowledge base
    /// </summary>
    /// <param name="category">The category of the query</param>
    /// <param name="queryName">The name of the query file (including .kql extension)</param>
    /// <param name="parameters">Optional. Dictionary of parameter names and values to replace in the query</param>
    /// <returns>The query results as a JSON string</returns>
    [McpServerTool, Description("Executes a KQL query from the knowledge base against Application Insights")]
    public static async Task<string> ExecuteKnowledgeBaseQuery(
        KqlQueryLibrary queryLibrary,
        string category,
        string queryName,
        Dictionary<string, string>? parameters = null)
    {
        return await queryLibrary.ExecuteKnowledgeBaseQuery(category, queryName, parameters);
    }

    /// <summary>
    /// Searches for queries containing a specific term
    /// </summary>
    /// <param name="searchTerm">The term to search for</param>
    /// <returns>List of matching query paths</returns>
    [McpServerTool, Description("Searches across all KQL queries in the knowledge base for specific terms")]
    public static IEnumerable<string> SearchQueries(KqlQueryLibrary queryLibrary, string searchTerm)
    {
        return queryLibrary.SearchQueries(searchTerm);
    }

    /// <summary>
    /// Gets the available parameters for a specific query
    /// </summary>
    /// <param name="category">The category of the query</param>
    /// <param name="queryName">The name of the query file (including .kql extension)</param>
    /// <returns>Dictionary of parameter names and their descriptions</returns>
    [McpServerTool, Description("Gets the available parameters for a specific KQL query")]
    public static Dictionary<string, string> GetQueryParameters(KqlQueryLibrary queryLibrary, string category, string queryName)
    {
        return queryLibrary.ExtractQueryParameters(category, queryName);
    }
}