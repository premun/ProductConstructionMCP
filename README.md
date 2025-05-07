# Agent Repository

This repository contains a Model Context Protocol (MCP) implementation for product construction.

## Development Setup

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 10.0 or later)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) installed
- Azure subscription for ApplicationInsights access
- [Visual Studio Code](https://code.visualstudio.com/) (optional, for development)
- [MCP extension for VS Code](https://marketplace.visualstudio.com/items?itemName=Microsoft.mcp) (optional, for development)
- [C# extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) (optional, for development)
- [Azure Account extension for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=ms-vscode.azure-account) (optional, for development)

### Setup Instructions

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd agent
   ```

2. **Authenticate with Azure**

   The application requires Azure authentication for accessing Application Insights resources. Run the following command to authenticate:

   ```bash
   az login
   ```

3. **Build the project**

   ```bash
   dotnet build
   ```

4. **Run the MCP server**

   ```bash
   dotnet run --project ProductConstructionMCP/ProductConstructionMCP.csproj
   ```

   Alternatively, if using VSCode, you can use the MCP extension with the configured server in `.vscode/mcp.json`.

## Using with VS Code

This repository includes configuration for the MCP extension in VS Code. The server configuration is located in `.vscode/mcp.json`. When using VS Code:

1. Install the MCP extension for VS Code
2. Open the command palette (Ctrl+Shift+P)
3. Select "MCP: Connect to Server"
4. Choose "ProductConstruction MCP" from the list

## Security Best Practices

- Never commit your `configuration.json` file with real credentials to source control
- Use managed identities for Azure authentication in production environments
- Implement proper error handling and logging for Azure service interactions
- Follow the principle of least privilege when setting up Azure permissions

## Troubleshooting

- If you encounter authentication issues, ensure you've completed the `az login` step
- If the MCP server fails to start, check that the configuration file has been set up correctly
- For Application Insights query issues, verify that your account has access to the specified resource