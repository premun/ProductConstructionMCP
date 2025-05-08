using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationInsightsMCP.Tools;

/// <summary>
/// Provides functionality to manage and execute KQL queries from the knowledge base
/// </summary>
public class KqlQueryLibrary
{
    private readonly ApplicationInsightsHandler _appInsightsHandler;
    private readonly ILogger<KqlQueryLibrary> _logger;
    private readonly string _kqlBasePath;

    public KqlQueryLibrary(
        ApplicationInsightsHandler appInsightsHandler,
        IOptions<AppConfiguration> config,
        ILogger<KqlQueryLibrary> logger)
    {
        _appInsightsHandler = appInsightsHandler;
        _logger = logger;
        _kqlBasePath = Path.Combine(config.Value.RepositoryRoot, "knowledge-base", "kql-queries");
    }

    /// <summary>
    /// Gets a collection of available query categories
    /// </summary>
    public IEnumerable<string> GetCategories()
    {
        try
        {
            return Directory.GetDirectories(_kqlBasePath)
                .Select(path => new DirectoryInfo(path).Name)
                .Where(dir => !dir.StartsWith("."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving query categories");
            return [];
        }
    }

    /// <summary>
    /// Gets available queries within a category
    /// </summary>
    /// <param name="category">The category to list queries from</param>
    /// <returns>Collection of query names</returns>
    public IEnumerable<string> GetQueriesInCategory(string category)
    {
        try
        {
            var categoryPath = Path.Combine(_kqlBasePath, category);
            if (!Directory.Exists(categoryPath))
            {
                _logger.LogWarning("Category directory not found: {Category}", category);
                return [];
            }

            return Directory.GetFiles(categoryPath, "*.kql")
                .Select(path => new FileInfo(path).Name.Replace(".kql", null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving queries for category: {Category}", category);
            return [];
        }
    }

    /// <summary>
    /// Get metadata about a specific query
    /// </summary>
    /// <param name="category">The category of the query</param>
    /// <param name="queryName">The filename of the query</param>
    /// <returns>Dictionary containing query metadata</returns>
    public Dictionary<string, string> GetQueryMetadata(string category, string queryName)
    {
        try
        {
            var queryPath = Path.Combine(_kqlBasePath, category, queryName);
            if (!File.Exists(queryPath))
            {
                _logger.LogWarning("Query file not found: {QueryPath}", queryPath);
                return new Dictionary<string, string>();
            }

            var content = File.ReadAllText(queryPath);
            var metadata = new Dictionary<string, string>();

            // Extract title
            var titleMatch = Regex.Match(content, @"\/\/\s*Title:\s*(.+)");
            if (titleMatch.Success) metadata["Title"] = titleMatch.Groups[1].Value.Trim();

            // Extract description
            var descMatch = Regex.Match(content, @"\/\/\s*Description:\s*(.+)");
            if (descMatch.Success) metadata["Description"] = descMatch.Groups[1].Value.Trim();

            // Extract use case
            var useMatch = Regex.Match(content, @"\/\/\s*Use Case:\s*(.+)");
            if (useMatch.Success) metadata["UseCase"] = useMatch.Groups[1].Value.Trim();

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metadata for query: {Category}/{QueryName}",
                category, queryName);
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// Gets the raw KQL query text from a file
    /// </summary>
    /// <param name="category">The category of the query</param>
    /// <param name="queryName">The filename of the query</param>
    /// <returns>The KQL query text</returns>
    public string GetQueryText(string category, string queryName)
    {
        if (queryName.EndsWith(".kql", StringComparison.OrdinalIgnoreCase))
        {
            queryName = queryName.Substring(0, queryName.Length - 4);
        }

        try
        {
            var queryPath = Path.Combine(_kqlBasePath, category, queryName + ".kql");
            if (!File.Exists(queryPath))
            {
                _logger.LogWarning("Query file not found: {QueryPath}", queryPath);
                return string.Empty;
            }

            var content = File.ReadAllText(queryPath);

            // Extract the actual query (between "// Query:" and the next comment block)
            var match = Regex.Match(content, @"\/\/\s*Query:\s*\n([\s\S]*?)(?:\/\/|$)");

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
            else
            {
                // Fallback: return the whole file content if query section not found
                _logger.LogWarning("Query section not found in {QueryPath}, returning full content", queryPath);
                return content;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving query text for {Category}/{QueryName}",
                category, queryName);
            return string.Empty;
        }
    }

    /// <summary>
    /// Executes a query from the knowledge base
    /// </summary>
    /// <param name="category">The category of the query</param>
    /// <param name="queryName">The filename of the query</param>
    /// <param name="period">The time period to analyze, e.g. "1d" for 1 day (default)</param>
    /// <returns>The result of the query execution as JSON</returns>
    public async Task<string> ExecuteKnowledgeBaseQuery(string category, string queryName, string period = "1d")
    {
        try
        {
            var queryText = GetQueryText(category, queryName);
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return $"{{\"error\": \"Query not found or empty: {category}/{queryName}\"}}";
            }

            _logger.LogInformation("Executing knowledge base query: {Category}/{QueryName} with period {Period}",
                category, queryName, period);

            // Replace the TimeStart parameter with the provided period or default
            queryText = queryText.Replace("{{TimeStart}}", $"ago({period})");

            return await _appInsightsHandler.ExecuteQuery(queryText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing knowledge base query: {Category}/{QueryName}",
                category, queryName);
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
    }

    /// <summary>
    /// Searches across all queries in the knowledge base for a specific term
    /// </summary>
    /// <param name="searchTerm">The term to search for</param>
    /// <returns>List of matching query paths</returns>
    public IEnumerable<string> SearchQueries(string searchTerm)
    {
        try
        {
            var results = new List<string>();

            foreach (var category in GetCategories())
            {
                var categoryPath = Path.Combine(_kqlBasePath, category);
                var queryFiles = Directory.GetFiles(categoryPath, "*.kql");

                foreach (var file in queryFiles)
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add($"{category}/{new FileInfo(file).Name.Replace(".kql", null)}");
                    }
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching queries for term: {SearchTerm}", searchTerm);
            return [];
        }
    }
}