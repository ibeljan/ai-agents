#!/usr/bin/dotnet run
#:package Microsoft.Extensions.AI@10.*
#:package Microsoft.Extensions.AI.OpenAI@10.*-*
#:package Microsoft.Agents.AI.OpenAI@1.*-*
#:package Azure.Identity@*-*
#:package Azure.AI.OpenAI@*-*
#:package ModelContextProtocol@*-*

// This sample shows how to create and use a simple AI agent with tools from an MCP Server.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI.Chat;

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://myresearchfoundry.openai.azure.com/";
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5-mini";

Console.WriteLine("=== MCP Agent Framework Demo ===\n");
Console.WriteLine("Connecting to local MCP server (stdio)...\n");

// Use the compiled MCP server project instead of dotnet-script
await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "DummyMCPServer",
    Command = "dotnet",
    Arguments = ["run", "--project", "DummyMcpServer/DummyMcpServer.csproj"],
}));

Console.WriteLine("Connected to MCP server!\n");

// Retrieve the list of tools available from the server
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

Console.WriteLine($"Available tools from MCP server: {mcpTools.Count()}");
foreach (var tool in mcpTools)
{
    Console.WriteLine($"  - {tool.Name}: {tool.Description}");
}
Console.WriteLine();

// Create the agent with MCP tools
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetChatClient(deploymentName)
     .CreateAIAgent(
         instructions: "You are a helpful assistant with access to weather, calculation, time, and echo tools. Use these tools to help answer user questions.",
         tools: [.. mcpTools.Cast<AITool>()]);

Console.WriteLine("Agent created with MCP tools!\n");
Console.WriteLine(new string('=', 50));

// Test 1: Weather query
Console.WriteLine("\n📍 Test 1: Weather Query");
Console.WriteLine("User: What's the weather like in Paris, France?\n");
Console.WriteLine("Agent Response:");
var response1 = await agent.RunAsync("What's the weather like in Paris, France?");
Console.WriteLine(response1);

// Test 2: Calculation
Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("\n🔢 Test 2: Calculation");
Console.WriteLine("User: What is 42 multiplied by 17?\n");
Console.WriteLine("Agent Response:");
var response2 = await agent.RunAsync("What is 42 multiplied by 17?");
Console.WriteLine(response2);

// Test 3: Current time
Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("\n⏰ Test 3: Current Time");
Console.WriteLine("User: What time is it right now?\n");
Console.WriteLine("Agent Response:");
var response3 = await agent.RunAsync("What time is it right now?");
Console.WriteLine(response3);

// Test 4: Multiple tools
Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("\n🔧 Test 4: Multiple Tools");
Console.WriteLine("User: Get the weather in Tokyo, then calculate 25 + 17, and tell me the current time.\n");
Console.WriteLine("Agent Response:");
var response4 = await agent.RunAsync("Get the weather in Tokyo, then calculate 25 + 17, and tell me the current time.");
Console.WriteLine(response4);

Console.WriteLine("\n" + new string('=', 50));
Console.WriteLine("\n✅ All tests completed!");