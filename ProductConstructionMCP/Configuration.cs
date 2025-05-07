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
    /// The instrumentation key for Application Insights
    /// </summary>
    public string InstrumentationKey { get; set; } = string.Empty;
}