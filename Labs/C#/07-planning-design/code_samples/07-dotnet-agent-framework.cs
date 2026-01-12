#!/usr/bin/dotnet run
#:package Microsoft.Extensions.AI@10.*
#:package Microsoft.Extensions.AI.OpenAI@10.*-*
#:package Microsoft.Agents.AI.OpenAI@1.*-*
#:package Azure.Identity@*-*
#:package Azure.AI.OpenAI@*-*

using System;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using Azure.AI.OpenAI;
using Azure.Identity;
using OpenAI;

// Extract configuration from environment variables
// Retrieve the AI Foundry Models API endpoint
// Retrieve the model ID, defaults to gpt-5-mini if not specified
var ai_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_FOUNDRY_ENDPOINT is not set.");
var ai_foundry_model_id = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_MODEL") ?? "gpt-5-mini";

// Define agent configuration
const string AGENT_NAME = "TravelPlanAgent";

const string AGENT_INSTRUCTIONS = @"You are an planner agent.
    Your job is to decide which agents to run based on the user's request.
    Below are the available agents specialised in different tasks:
    - FlightBooking: For booking flights and providing flight information
    - HotelBooking: For booking hotels and providing hotel information
    - CarRental: For booking cars and providing car rental information
    - ActivitiesBooking: For booking activities and providing activity information
    - DestinationInfo: For providing information about destinations
    - DefaultAgent: For handling general request";

// Create JSON serializer options with the context
var jsonOptions = new JsonSerializerOptions
{
    TypeInfoResolver = TravelPlanJsonContext.Default
};

// Configure agent with structured output
ChatClientAgentOptions agentOptions = new ChatClientAgentOptions()
{
    Name = AGENT_NAME,
    ChatOptions = new()
    {
        Instructions = AGENT_INSTRUCTIONS,
        ResponseFormat = ChatResponseFormatJson.ForJsonSchema(
            schema: AIJsonUtilities.CreateJsonSchema(typeof(TravelPlan), serializerOptions: jsonOptions),
            schemaName: "TravelPlan",
            schemaDescription: "Travel Plan with main_task and subtasks")
    }
};



// Create AI agent
AIAgent agent = new AzureOpenAIClient(
    new Uri(ai_foundry_endpoint),
    new AzureCliCredential())
        .GetChatClient(ai_foundry_model_id)
        .AsIChatClient()
        .CreateAIAgent(agentOptions);

// Execute planning request
Console.WriteLine(await agent.RunAsync("Create a travel plan for a family of 4, with 2 kids, from Singapore to Melbourne"));

// Define data models for structured output
public class Plan
{
    [JsonPropertyName("assigned_agent")]
    public string? Assigned_agent { get; set; }

    [JsonPropertyName("task_details")]
    public string? Task_details { get; set; }
}

public class TravelPlan
{
    [JsonPropertyName("main_task")]
    public string? Main_task { get; set; }

    [JsonPropertyName("subtasks")]
    public IList<Plan>? Subtasks { get; set; }
}

// JSON serialization context for source generation
[JsonSerializable(typeof(TravelPlan))]
[JsonSerializable(typeof(Plan))]
[JsonSerializable(typeof(IList<Plan>))]
internal partial class TravelPlanJsonContext : JsonSerializerContext
{
}
