#!/usr/bin/dotnet run

#:package Azure.Identity@*-*
#:package Azure.AI.OpenAI@*-*
#:package Microsoft.Agents.AI.OpenAI@*-*
#:package Microsoft.Agents.AI.AzureAI.Persistent@*-*

using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using Azure.AI.Agents.Persistent;

// Extract configuration from environment variables
// Retrieve the AI Foundry Models API endpoint
// Retrieve the model ID, defaults to gpt-5-mini if not specified
var ai_foundry_project_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_PROJECT_ENDPOINT") ?? "https://myresearchfoundry.services.ai.azure.com/api/projects/embeddingsproject/";
var ai_foundry_model_id = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_MODEL") ?? "gpt-5-mini";

var persistentAgentsClient = new PersistentAgentsClient(
    ai_foundry_project_endpoint,
    new AzureCliCredential());

// Create a persistent agent
var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: ai_foundry_model_id,
    name: "AgentServiceJoker",
    instructions: "You are good at telling jokes.");

// Retrieve the agent that was just created as an AIAgent using its ID
AIAgent agent1 = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);

// Invoke the agent and output the text result.
Console.WriteLine(await agent1.RunAsync("Tell me a joke about a ninja pirate."));