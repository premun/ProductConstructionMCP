using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace ApplicationInsightsMCP.Prompts;

[McpServerPromptType]
public static class TestPrompts
{
    [McpServerPrompt, Description("Tests the Application Insights tools")]
    public static Task<string> TestAppInsightsTools()
    {
        return Task.FromResult(
            $"""
            Use the ExecuteQuery tool and give me last 3 traces
            """);
    }
}