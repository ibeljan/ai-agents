# MCP Server Demo - Local Implementation

## Overview

This demo includes a **local dummy MCP server** implementation that provides simple tools for testing the Model Context Protocol integration with AI agents.

## Files

- **DummyMcpServer.cs** - A local MCP server with 4 demo tools
- **09-mcp-agent-framework.cs** - Agent client that connects to the local MCP server

## Available Tools

The dummy MCP server provides the following tools:

1. **get_weather** - Get weather information for a location
2. **calculate** - Perform basic arithmetic (add, subtract, multiply, divide)
3. **get_current_time** - Get the current date and time
4. **echo** - Echo back messages with additional info (length, reversed)

## Running the Demo

### Option 1: Server Starts Automatically

The main script automatically starts the MCP server as a child process:

```powershell
dotnet run .\09-mcp-agent-framework.cs
```

This will:
1. Start the dummy MCP server
2. Connect to it
3. List available tools
4. Run 4 test queries demonstrating the tools

### Option 2: Run Server Separately (Manual)

If you want to run the server separately for debugging:

**Terminal 1 - Start the MCP Server:**
```powershell
dotnet run .\DummyMcpServer.cs
```

**Terminal 2 - Run the Agent Client:**
```powershell
# Modify 09-mcp-agent-framework.cs to connect to running server
dotnet run .\09-mcp-agent-framework.cs
```

## Demo Tests

The script runs 4 tests:

1. **Weather Query**: "What's the weather like in Paris, France?"
2. **Calculation**: "What is 42 multiplied by 17?"
3. **Current Time**: "What time is it right now?"
4. **Multiple Tools**: Uses weather, calculation, and time tools together

## Environment Variables Required

```
AZURE_AI_FOUNDRY_ENDPOINT=<your-endpoint>
AZURE_AI_FOUNDRY_MODEL=<your-model-name>
```

## Architecture

```
┌─────────────────────┐
│   AI Agent          │
│   (09-mcp-*.cs)     │
└──────────┬──────────┘
           │ MCP Protocol
           │ (stdio)
           │
┌──────────▼──────────┐
│   MCP Server        │
│  (DummyMcpServer)   │
│  ┌────────────────┐ │
│  │ get_weather    │ │
│  │ calculate      │ │
│  │ get_current_time│ │
│  │ echo           │ │
│  └────────────────┘ │
└─────────────────────┘
```

## Troubleshooting

### Server not starting
- Ensure you have dotnet script capabilities
- Check that ModelContextProtocol package is installed

### Connection issues
- The server outputs debug messages to stderr
- Check console for "[Dummy MCP Server]" log messages

### Tool execution errors
- The server logs each tool invocation
- Review the stderr output for error details

## Next Steps

- Modify DummyMcpServer.cs to add your own custom tools
- Integrate with real data sources instead of mock data
- Deploy the MCP server as a standalone service
