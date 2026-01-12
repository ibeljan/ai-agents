#!/usr/bin/dotnet run

#:package Newtonsoft.Json@*
#:package Microsoft.Extensions.AI.OpenAI@10.1.1-preview.1.25612.2
#:package Microsoft.Agents.AI.OpenAI@1.0.0-preview.260108.1
#:package Azure.AI.OpenAI@2.8.0-beta.1
#:package Azure.Identity@*
#:package DotNetEnv@*

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Azure.Identity;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentMetacognitionDemo
{
    class Program
    {
        // Tool Functions for the Agent
        [Description("Get a list of available vacation destinations")]
        static string GetDestinations()
        {
            return @"
    Barcelona, Spain
    Paris, France
    Berlin, Germany
    Tokyo, Japan
    New York, USA
    ";
        }

        [Description("Get available flight times for a specific destination")]
        static string GetFlightTimes(
            [Description("The destination city to get flight times for")] string destination)
        {
            var flightTimes = new Dictionary<string, string[]>
            {
                { "Barcelona", new[] { "08:30 AM", "02:15 PM", "10:45 PM" } },
                { "Paris", new[] { "06:45 AM", "12:30 PM", "07:15 PM" } },
                { "Berlin", new[] { "07:20 AM", "01:45 PM", "09:30 PM" } },
                { "Tokyo", new[] { "11:00 AM", "05:30 PM", "11:55 PM" } },
                { "New York", new[] { "05:15 AM", "03:00 PM", "08:45 PM" } }
            };

            // Extract just the city name from input that might contain country
            var city = destination.Split(',')[0].Trim();

            if (flightTimes.ContainsKey(city))
            {
                var times = string.Join(", ", flightTimes[city]);
                return $"Flight times for {city}: {times}";
            }
            else
            {
                return $"No flight information available for {city}.";
            }
        }

        // Display helper functions
        static void DisplayUserMessage(string message)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"👤 User:");
            Console.ResetColor();
            Console.WriteLine($"   {message}");
            Console.WriteLine();
        }

        static async Task Main(string[] args)
        {
            // Load environment variables
            DotNetEnv.Env.Load();
            Console.WriteLine("✅ Environment variables loaded from .env file\n");

            // Configuration
            var projectEndpoint = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT");
            var modelDeploymentName = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_MODEL");

            Console.WriteLine("Environment Configuration:");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Project Endpoint: {projectEndpoint}");
            Console.WriteLine($"Model: {modelDeploymentName}");
            Console.WriteLine(new string('=', 50) + "\n");

            // Agent instructions
            const string AGENT_NAME = "TravelAgent";
            const string AGENT_INSTRUCTIONS = @"
You are Flight Booking Agent that provides information about available flights and gives travel activity suggestions when asked.
Travel activity suggestions should be specific to customer, location and amount of time at location.

You have access to the following tools to help users plan their trips:
1. GetDestinations: Returns a list of available vacation destinations that users can choose from.
2. GetFlightTimes: Provides available flight times for specific destinations.

Your process for assisting users:
- When users first inquire about flight booking with no prior history, ask for their preferred flight time ONCE.
- MAINTAIN a customer_preferences object throughout the conversation to track preferred flight times.
- When a user books a flight to any destination, RECORD their chosen flight time in the customer_preferences object.
- For ALL subsequent flight inquiries to ANY destination, AUTOMATICALLY apply their existing preferred flight time without asking.
- NEVER ask about time preferences again after they've been established for any destination.
- When suggesting flights for a new destination, explicitly say: ""Based on your previous preference for [time] flights, I recommend...""
- Only after showing options matching their preferred time, ask if they want to see alternative times.
- After each booking, UPDATE the customer_preferences object with any new information.
- ALWAYS mention which specific preference you used when making a suggestion.

Guidelines:
- Use the exact destination names when using tools (Barcelona, Paris, Berlin, Tokyo, New York)
- Respond in a helpful and enthusiastic manner about travel possibilities
- Always seek feedback to ensure your suggestions meet the user's expectations
- Acknowledge when a request falls outside your capabilities
- For better formatting, always display flight times in a list format
- When giving any timed suggestions, reflect if the time frames are reasonable. Respond again if not.

Your goal is to help users explore vacation options efficiently and make informed travel decisions by understanding their preferences and providing tailored recommendations.
";

            // Create the travel agent with Microsoft Agent Framework
            var agent = new AzureOpenAIClient(
                new Uri(projectEndpoint),
                new AzureCliCredential())
                    .GetChatClient(modelDeploymentName)
                    .AsIChatClient()
                    .CreateAIAgent(
                        name: AGENT_NAME,
                        instructions: AGENT_INSTRUCTIONS,
                        tools: [
                            AIFunctionFactory.Create(GetDestinations),
                            AIFunctionFactory.Create(GetFlightTimes)
                        ]
                    );

            Console.WriteLine($"✅ Agent '{AGENT_NAME}' created with metacognition capabilities\n");

            Console.WriteLine(new string('=', 50));
            Console.WriteLine("🧠 Agent Metacognition Demo - Part 1");
            Console.WriteLine("Demonstrating: Learning User Preferences");
            Console.WriteLine(new string('=', 50) + "\n");

            // First conversation sequence - Learning preferences
            var userInputs = new List<string>
            {
                "Book me a flight to Barcelona",
                "I prefer a later flight",
                "That is too late, choose the earliest flight",
                "I want to leave the same day, give me some suggestions of things to do in Barcelona during my layover if I take the last flight out",
                "I am stressed this wont be enough time"
            };

            // Create a new conversation thread
            var thread = agent.GetNewThread();

            foreach (var userInput in userInputs)
            {
                DisplayUserMessage(userInput);

                // Run the agent with streaming
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"🤖 {AGENT_NAME}:");
                Console.ResetColor();
                Console.Write("   ");

                await foreach (var update in agent.RunStreamingAsync(userInput, thread))
                {
                    await Task.Delay(10);
                    Console.Write(update);
                }

                Console.WriteLine();
                Console.WriteLine(new string('-', 80));
            }

            Console.WriteLine("\n✅ First conversation sequence completed\n");

            Console.WriteLine(new string('=', 50));
            Console.WriteLine("🧠 Agent Metacognition Demo - Part 2");
            Console.WriteLine("Demonstrating: Applying Learned Preferences");
            Console.WriteLine(new string('=', 50) + "\n");

            // Second conversation sequence - Applying learned preferences
            var continuedInputs = new List<string>
            {
                "Book me a flight to Paris"
            };

            foreach (var userInput in continuedInputs)
            {
                DisplayUserMessage(userInput);

                // Run the agent with streaming
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"🤖 {AGENT_NAME}:");
                Console.ResetColor();
                Console.Write("   ");

                await foreach (var update in agent.RunStreamingAsync(userInput, thread))
                {
                    await Task.Delay(10);
                    Console.Write(update);
                }

                Console.WriteLine();
                Console.WriteLine(new string('-', 80));
            }

            Console.WriteLine("\n✅ Continued conversation completed");
            Console.WriteLine("\n📊 Notice how the agent remembered the user's preference for early morning flights!");
            
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("✅ Demo completed successfully");
            Console.WriteLine(new string('=', 50));
        }
    }
}
