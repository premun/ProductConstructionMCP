using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApplicationInsightsMCP;

/// <summary>
/// Provides functionality to manage and execute KQL queries from the knowledge base
/// </summary>
public class KqlQueryLibrary
{
    private readonly ApplicationInsightsApiHandler _appInsightsHandler;
    private readonly ILogger<KqlQueryLibrary> _logger;
    private readonly string _kqlBasePath;

    public KqlQueryLibrary(
        ApplicationInsightsApiHandler appInsightsHandler,
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
    /// <param name="parameters">Dictionary of parameter names and values to replace in the query</param>
    /// <returns>The result of the query execution as JSON</returns>
    public async Task<string> ExecuteKnowledgeBaseQuery(string category, string queryName, Dictionary<string, string>? parameters = null)
    {
        try
        {
            var queryText = GetQueryText(category, queryName);
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return $"{{\"error\": \"Query not found or empty: {category}/{queryName}\"}}";
            }

            _logger.LogInformation("Executing knowledge base query: {Category}/{QueryName} with {ParameterCount} parameters",
                category, queryName, parameters?.Count ?? 0);

            // Apply parameter substitutions
            foreach (var param in parameters ?? [])
            {
                string placeholder = $"{{{{{param.Key}}}}}";
                string value = param.Value;

                // Special handling for TimeStart parameter which needs ago() function
                if (param.Key == "TimeStart" && !value.StartsWith("ago(", StringComparison.OrdinalIgnoreCase))
                {
                    value = $"ago({value})";
                }

                queryText = queryText.Replace(placeholder, value);
            }

            // Check for missing parameters and provide defaults where possible
            var missingParameters = new List<string>();

            // Apply default for TimeStart if not provided
            if (queryText.Contains("{{TimeStart}}"))
            {
                queryText = queryText.Replace("{{TimeStart}}", "ago(1d)");
            }

            // Check for any remaining parameters that are missing
            var remainingParams = Regex.Matches(queryText, @"\{\{([^}]+)\}\}");
            foreach (Match match in remainingParams)
            {
                missingParameters.Add(match.Groups[1].Value);
            }

            // If there are missing parameters, throw an exception
            if (missingParameters.Count > 0)
            {
                throw new ArgumentException($"Required parameters missing: {string.Join(", ", missingParameters)}");
            }

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

    /// <summary>
    /// Extracts parameter names and descriptions from a KQL query file
    /// </summary>
    /// <param name="category">The category of the query</param>
    /// <param name="queryName">The filename of the query</param>
    /// <returns>Dictionary of parameter names and their descriptions</returns>
    public Dictionary<string, string> ExtractQueryParameters(string category, string queryName)
    {
        try
        {
            if (queryName.EndsWith(".kql", StringComparison.OrdinalIgnoreCase))
            {
                queryName = queryName.Substring(0, queryName.Length - 4);
            }

            var queryPath = Path.Combine(_kqlBasePath, category, queryName + ".kql");
            if (!File.Exists(queryPath))
            {
                _logger.LogWarning("Query file not found: {QueryPath}", queryPath);
                return [];
            }

            var content = File.ReadAllText(queryPath);
            var parameters = new Dictionary<string, string>();            // Find the Parameters section - account for possible malformed comments without line breaks
            var parametersSection = Regex.Match(content, @"\/\/\s*Parameters:([\s\S]*?)(?:\/\/\s*Query:)");

            if (!parametersSection.Success)
            {
                throw new ArgumentException($"Parameters section not found in {queryPath}. Please check the file format.");
            }

            // Extract individual parameter definitions
            var paramText = parametersSection.Groups[1].Value;

            // Try an even more liberal pattern to match parameters, accounting for the malformed format
            var paramMatches = Regex.Matches(paramText, @"-\s*([^:]+):\s*([^\/]*)", RegexOptions.Multiline);
            foreach (Match match in paramMatches)
            {
                if (match.Groups.Count >= 3)
                {
                    var paramName = match.Groups[1].Value.Trim();
                    var paramDesc = match.Groups[2].Value.Trim();
                    parameters[paramName] = paramDesc;
                }
            }

            return parameters;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting parameters for query: {Category}/{QueryName}", category, queryName);
            return [];
        }
    }
}