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

### Setting Up the Environment

Configure the Application Insights in `.vscode/mcp.json`.

## Using with VS Code

This repository includes configuration for the MCP extension in VS Code. The server configuration is located in `.vscode/mcp.json`. When using VS Code:

1. Install the MCP extension for VS Code
2. Open the command palette (Ctrl+Shift+P)
3. Select "MCP: Connect to Server"
4. Choose "ProductConstruction MCP" from the list
