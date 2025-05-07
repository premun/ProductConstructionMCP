namespace ProductConstructionMCP;

/// <summary>
/// Root configuration class for the ProductConstructionMCP application
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// Application Insights configuration settings
    /// </summary>
    public ApplicationInsightsConfig ApplicationInsights { get; set; } = new();
    
    // Add other configuration sections here as needed
}

/// <summary>
/// Configuration settings for Application Insights
/// </summary>
public class ApplicationInsightsConfig
{
    /// <summary>
    /// The subscription ID containing the Application Insights resource
    /// </summary>
    public string SubscriptionId { get; set; } = string.Empty;
    
    /// <summary>
    /// The resource group containing the Application Insights resource
    /// </summary>
    public string ResourceGroup { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the Application Insights resource
    /// </summary>
    public string ApplicationName { get; set; } = string.Empty;
}