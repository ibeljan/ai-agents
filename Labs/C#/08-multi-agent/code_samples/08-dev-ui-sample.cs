// Copyright (c) Microsoft. All rights reserved.

// This sample demonstrates basic usage of the DevUI in an ASP.NET Core application with AI agents.
// 
// NOTE: This sample requires a proper .csproj project file and cannot be run as a dotnet script.
// To run this sample:
// 1. Create a new ASP.NET Core project: dotnet new web -n DevUISample
// 2. Replace Program.cs with this file
// 3. Add the required NuGet packages to the .csproj:
//    - Microsoft.Extensions.AI (10.*)
//    - Microsoft.Extensions.AI.OpenAI (10.*-*)
//    - Microsoft.Agents.AI.OpenAI (1.*-*)
//    - Azure.Identity (*-*)
//    - Azure.AI.OpenAI (*-*)
//    - Microsoft.Agents.AI.Workflows (1.*-*)
//    - Microsoft.Agents.AI.DevUI (1.*-*)
//    - Microsoft.Agents.AI.Hosting (1.*-*)
// 4. Run: dotnet run

using System.ComponentModel;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.DevUI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace DevUI_Step01_BasicUsage;

/// <summary>
/// Sample demonstrating basic usage of the DevUI in an ASP.NET Core application.
/// </summary>
/// <remarks>
/// This sample shows how to:
/// 1. Set up Azure OpenAI as the chat client
/// 2. Create function tools for agents to use
/// 3. Register agents and workflows using the hosting packages with tools
/// 4. Map the DevUI endpoint which automatically configures the middleware
/// 5. Map the dynamic OpenAI Responses API for Python DevUI compatibility
/// 6. Access the DevUI in a web browser
///
/// The DevUI provides an interactive web interface for testing and debugging AI agents.
/// DevUI assets are served from embedded resources within the assembly.
/// Simply call MapDevUI() to set up everything needed.
///
/// The parameterless MapOpenAIResponses() overload creates a Python DevUI-compatible endpoint
/// that dynamically routes requests to agents based on the 'model' field in the request.
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Entry point that starts an ASP.NET Core web server with the DevUI.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Set up the Azure AI Foundry client
        var endpoint = builder.Configuration["AZURE_AI_FOUNDRY_ENDPOINT"] ?? throw new InvalidOperationException("AZURE_AI_FOUNDRY_ENDPOINT is not set.");
        var deploymentName = builder.Configuration["AZURE_AI_FOUNDRY_MODEL"] ?? "gpt-5-mini";

        Console.WriteLine("endpoint: " + endpoint);
        Console.WriteLine("deploymentName: " + deploymentName);

        var chatClient = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
            .GetChatClient(deploymentName)
            .AsIChatClient();

        builder.Services.AddChatClient(chatClient);

        // Define agent instructions
        const string FRONTDESK_INSTRUCTIONS = @"
            You are a Front Desk Travel Agent with ten years of experience and are known for brevity as you deal with many customers.
            The goal is to provide the best activities and locations for a traveler to visit.
            Only provide a single recommendation per response.
            You're laser focused on the goal at hand.
            Don't waste time with chit chat.
            Consider suggestions when refining an idea.
            ";

        const string CONCIERGE_INSTRUCTIONS = @"
            You are a hotel concierge who has opinions about providing the most local and authentic experiences for travelers.
            The goal is to determine if the front desk travel agent has recommended the best non-touristy experience for a traveler.
            If so, state that it is approved.
            If not, provide insight on how to refine the recommendation without using a specific example.
            ";

        // Register travel booking agents
        builder.AddAIAgent("FrontDesk", FRONTDESK_INSTRUCTIONS);
        builder.AddAIAgent("Concierge", CONCIERGE_INSTRUCTIONS);

        // Register travel booking workflow
        var frontdeskBuilder = builder.AddAIAgent("workflow-frontdesk", FRONTDESK_INSTRUCTIONS);
        var conciergeBuilder = builder.AddAIAgent("workflow-concierge", CONCIERGE_INSTRUCTIONS);
        builder.AddWorkflow("travel-review-workflow", (sp, key) =>
        {
            var agents = new List<IHostedAgentBuilder>() { frontdeskBuilder, conciergeBuilder }.Select(ab => sp.GetRequiredKeyedService<AIAgent>(ab.Name));
            return AgentWorkflowBuilder.BuildSequential(workflowName: key, agents: agents);
        }).AddAsAIAgent();

        builder.Services.AddOpenAIResponses();
        builder.Services.AddOpenAIConversations();

        var app = builder.Build();

        app.MapOpenAIResponses();
        app.MapOpenAIConversations();

    
    app.MapDevUI();
        

      Console.WriteLine("Press Ctrl+C to stop the server.");

        app.Run();
    }
}