#!/usr/bin/dotnet run

#:package Azure.Identity@*-*
#:package Azure.AI.OpenAI@*-*
#:package Microsoft.Agents.AI.OpenAI@*-*

using System;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

// Extract configuration from environment variables
// Retrieve the AI Foundry Models API endpoint
// Retrieve the model ID, defaults to gpt-5-mini if not specified
var ai_foundry_endpoint = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT") ?? "https://myresearchfoundry.openai.azure.com/";
var ai_foundry_model_id = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_MODEL") ?? "gpt-5-mini";

AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri(ai_foundry_endpoint),
    new AzureCliCredential());

var chatCompletionClient = client.GetChatClient(ai_foundry_model_id).AsIChatClient();

AIAgent agent = chatCompletionClient.CreateAIAgent(
    instructions: "You are good at telling jokes.",
    name: "CompletionsJoker");

// Invoke the agent and output the text result.
Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate."));