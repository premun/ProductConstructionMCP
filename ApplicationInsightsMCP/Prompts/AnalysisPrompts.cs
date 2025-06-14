using System.ComponentModel;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace ApplicationInsightsMCP.Prompts;

[McpServerPromptType]
public static class TestPrompts
{
    [McpServerPrompt, Description("Analyzes the exceptions in the service in the last day and identifies affected repositories.")]
    public static Task<string> DailyAnalysis()
    {
        return Task.FromResult(
            $"""
            Can you check the exceptions in the service in the last day? Can you check which repos are affected by these exceptions?
            """);
    }
}