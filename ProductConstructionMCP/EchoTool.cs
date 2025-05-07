using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Linq;

namespace ProductConstructionMCP;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}
