using System.Text.Json.Serialization;

namespace ApplicationInsightsMCP;

/// <summary>
/// Root configuration class for the ProductConstructionMCP application
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Application Insights configuration settings
    /// </summary>
    [JsonPropertyName("applicationInsights")]
    public ApplicationInsightsConfig ApplicationInsights { get; set; } = new();

    public string RepositoryRoot { get; set; } = null!;
}

/// <summary>
/// Configuration settings for Application Insights
/// </summary>
public class ApplicationInsightsConfig
{
    /// <summary>
    /// The subscription ID containing the Application Insights resource
    /// </summary>
    [JsonPropertyName("subscriptionId")]
    public string SubscriptionId { get; set; } = string.Empty;

    /// <summary>
    /// The resource group containing the Application Insights resource
    /// </summary>
    [JsonPropertyName("resourceGroup")]
    public string ResourceGroup { get; set; } = string.Empty;

    /// <summary>
    /// The name of the Application Insights resource
    /// </summary>
    [JsonPropertyName("applicationName")]
    public string ApplicationName { get; set; } = string.Empty;
}