#!/usr/bin/dotnet run

#:package Microsoft.Extensions.AI@10.*
#:package Microsoft.Extensions.AI.OpenAI@10.*-*
#:package Microsoft.Agents.AI.OpenAI@1.*-*
#:package Azure.AI.OpenAI@*-*
#:package Azure.Identity@*-*

using System.ClientModel;
using System.ComponentModel;

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

using OpenAI;

// Tool Function: Random Destination Generator
// This static method will be available to the agent as a callable tool
// The [Description] attribute helps the AI understand when to use this function
// This demonstrates how to create custom tools for AI agents
[Description("Provides a random vacation destination.")]
static string GetRandomDestination()
{
    // List of popular vacation destinations around the world
    // The agent will randomly select from these options
    var destinations = new List<string>
    {
        "Paris, France",
        "Tokyo, Japan",
        "New York City, USA",
        "Sydney, Australia",
        "Rome, Italy",
        "Barcelona, Spain",
        "Cape Town, South Africa",
        "Rio de Janeiro, Brazil",
        "Bangkok, Thailand",
        "Vancouver, Canada"
    };

    // Generate random index and return selected destination
    // Uses System.Random for simple random selection
    var random = new Random();
    int index = random.Next(destinations.Count);
    return destinations[index];
}

// Extract configuration from environment variables
// Retrieve the AI Foundry Models API endpoint
// Retrieve the model ID, defaults to gpt-5-mini if not specified
var ai_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT");
var ai_foundry_model_id = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_MODEL") ?? "gpt-5-mini";

AIAgent agent = new AzureOpenAIClient(
    new Uri(ai_foundry_endpoint),
    new AzureCliCredential())
        .GetChatClient(ai_foundry_model_id)
        .AsIChatClient()
        .CreateAIAgent(
            instructions: "You are a helpful AI Agent that can help plan vacations for customers at random destinations",
            tools: [AIFunctionFactory.Create(GetRandomDestination)]);



// Execute Agent: Plan a Day Trip
// Run the agent with streaming enabled for real-time response display
// Shows the agent's thinking and response as it generates the content
// Provides better user experience with immediate feedback
await foreach (var update in agent.RunStreamingAsync("Plan me a day trip"))
{
    await Task.Delay(10);
    Console.Write(update);
}