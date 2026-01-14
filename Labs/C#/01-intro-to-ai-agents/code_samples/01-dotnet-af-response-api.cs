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


#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
var responseChatClient = client.GetResponsesClient(ai_foundry_model_id).AsIChatClient();
#pragma warning restore OPENAI001

AIAgent agent = responseChatClient.CreateAIAgent(
    instructions: "You are good at telling jokes.",
    name: "ResponsesJoker");

// Invoke the agent and output the text result.
Console.WriteLine(await agent.RunAsync("Tell me a joke about a ninja."));