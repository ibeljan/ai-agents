#!/usr/bin/dotnet run

#:package Newtonsoft.Json@*
#:package Microsoft.Extensions.AI.OpenAI@10.1.1-preview.1.25612.2
#:package Microsoft.Agents.AI.OpenAI@1.0.0-preview.260108.1
#:package Azure.AI.OpenAI@2.8.0-beta.1
#:package Azure.Identity@*
#:package Azure.Search.Documents@*
#:package DotNetEnv@*

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Azure.Core.Serialization;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace AgentMemoryDemo
{
    // Hotel data model
    public class Hotel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public string amenities { get; set; }
        public double price_per_night { get; set; }
        public double rating { get; set; }
        public string[] tags { get; set; }
    }

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(Hotel))]
    [JsonSerializable(typeof(List<Hotel>))]
    internal partial class HotelJsonContext : JsonSerializerContext { }

    // Simple in-memory store for user preferences
    public class SimpleMemoryStore
    {
        private readonly Dictionary<string, List<string>> _userMemories = new();

        public void AddMemory(string userId, string memory)
        {
            if (!_userMemories.ContainsKey(userId))
            {
                _userMemories[userId] = new List<string>();
            }
            _userMemories[userId].Add(memory);
        }

        public List<string> GetMemories(string userId)
        {
            return _userMemories.ContainsKey(userId)
                ? _userMemories[userId]
                : new List<string>();
        }

        public List<string> SearchMemories(string userId, string query)
        {
            var memories = GetMemories(userId);
            // Simple keyword search (in production, use semantic search)
            return memories.Where(m =>
                m.Contains(query, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }
    }

    // Travel booking tools with memory integration
    public class TravelBookingTools
    {
        private readonly SearchClient _searchClient;
        private readonly SimpleMemoryStore _memoryStore;

        public TravelBookingTools(SearchClient searchClient, SimpleMemoryStore memoryStore)
        {
            _searchClient = searchClient;
            _memoryStore = memoryStore;
        }

        [Description("Search for hotels based on criteria like location, amenities, or tags")]
        public async Task<string> SearchHotels(
            [Description("Search query for hotels (location, amenities, etc.)")] string query,
            [Description("Maximum number of results to return")] int maxResults = 3)
        {
            var searchOptions = new SearchOptions
            {
                Size = maxResults,
                IncludeTotalCount = true
            };

            var results = await _searchClient.SearchAsync<Hotel>(query, searchOptions);
            var hotels = new List<object>();

            await foreach (var result in results.Value.GetResultsAsync())
            {
                hotels.Add(new
                {
                    name = result.Document.name,
                    location = result.Document.location,
                    description = result.Document.description,
                    price_per_night = result.Document.price_per_night,
                    rating = result.Document.rating,
                    amenities = result.Document.amenities,
                    tags = result.Document.tags
                });
            }

            return JsonConvert.SerializeObject(hotels, Formatting.Indented);
        }

        [Description("Store user travel preferences and important information in memory")]
        public string StoreUserPreference(
            [Description("User identifier")] string userId,
            [Description("User preference or information to remember")] string preference)
        {
            Console.WriteLine($"DEBUG: Storing preference for {userId}: {preference}");
            _memoryStore.AddMemory(userId, preference);
            return $"✅ Stored: {preference}";
        }

        [Description("Get all stored preferences for a user")]
        public string GetUserPreferences(
            [Description("User identifier")] string userId)
        {
            Console.WriteLine($"DEBUG: Getting all preferences for {userId}");
            var memories = _memoryStore.GetMemories(userId);

            if (memories.Count == 0)
            {
                return $"No preferences found for user {userId}";
            }

            return $"User preferences for {userId}:\n- " + string.Join("\n- ", memories);
        }

        [Description("Search user's memories for relevant information")]
        public string SearchMemories(
            [Description("User identifier")] string userId,
            [Description("What to search for (e.g., 'family vacation', 'dietary restrictions')")] string query)
        {
            Console.WriteLine($"DEBUG: Searching memories for {userId} with query: '{query}'");
            var memories = _memoryStore.SearchMemories(userId, query);

            if (memories.Count == 0)
            {
                return $"No memories found for query: {query}";
            }

            return "Relevant memories:\n- " + string.Join("\n- ", memories);
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            // Load environment variables from specific path
            DotNetEnv.Env.Load();
            Console.WriteLine($"✅ Environment variables loaded from .env \n");

            // Configuration
            var azureOpenAIDeployment = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_MODEL");
            var azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_AI_FOUNDRY_ENDPOINT");
            var searchServiceEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_SERVICE_ENDPOINT");
            var searchApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY");
            var travelIndexName = "travel-hotels";

            Console.WriteLine("Environment Configuration:");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Azure OpenAI Model: {azureOpenAIDeployment}");
            Console.WriteLine($"Azure OpenAI Endpoint: {azureOpenAIEndpoint}");
            Console.WriteLine($"Search Service Endpoint: {searchServiceEndpoint}");
            Console.WriteLine(new string('=', 50) + "\n");

            // Initialize Azure AI Search
            var credential = new AzureKeyCredential(searchApiKey);
            var searchClientOptions = new SearchClientOptions
            {
                Serializer = new JsonObjectSerializer(new JsonSerializerOptions
                {
                    TypeInfoResolver = HotelJsonContext.Default
                })
            };

            var indexClient = new SearchIndexClient(new Uri(searchServiceEndpoint), credential, searchClientOptions);

            // Create travel data index if it doesn't exist
            await CreateTravelIndexAsync(indexClient, travelIndexName);

            // Initialize search client for travel data
            var travelSearchClient = new SearchClient(new Uri(searchServiceEndpoint), travelIndexName, credential, searchClientOptions);

            // Add sample hotel data (only if index is empty)
            await PopulateHotelDataAsync(travelSearchClient);

            // Initialize memory store
            var memoryStore = new SimpleMemoryStore();
            Console.WriteLine("✅ Memory store initialized\n");

            // Create travel booking tools
            var travelTools = new TravelBookingTools(travelSearchClient, memoryStore);

            // Agent instructions
            var AGENT_INSTRUCTIONS = @"
You are a personalized travel booking assistant with memory.

WORKFLOW:
1. When a user asks for help, search their memories using SearchMemories with a relevant query
2. Use the memories to personalize your response
3. Store any new preferences they mention using StoreUserPreference
4. When the user is booking a new trip, first retrieve the user's general travel preferences
5. Then use SearchHotels to find suitable options
6. Do not recommend hotels that are over budget

IMPORTANT: For ALL memory operations, use userId='sarah_johnson_123' exactly as written.

Always acknowledge what you found in their memories when responding.
";

            // Create the travel agent with Agent Framework
            var travelAgent = new AzureOpenAIClient(
                new Uri(azureOpenAIEndpoint),
                new AzureCliCredential())
                    .GetChatClient(azureOpenAIDeployment)
                    .AsIChatClient()
                    .CreateAIAgent(
                        name: "TravelBookingAssistant",
                        instructions: AGENT_INSTRUCTIONS,
                        tools: [
                            AIFunctionFactory.Create(travelTools.SearchHotels),
                            AIFunctionFactory.Create(travelTools.StoreUserPreference),
                            AIFunctionFactory.Create(travelTools.GetUserPreferences),
                            AIFunctionFactory.Create(travelTools.SearchMemories)
                        ]
                    );

            Console.WriteLine("✅ Travel booking agent initialized\n");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine("Starting Travel Booking Agent Demo");
            Console.WriteLine(new string('=', 50) + "\n");

            // Test 1: Store preferences
            var userMessage1 = "Hi! I'm planning trips for this year. I prefer luxury hotels with spa services and I love romantic destinations. My budget is around $500-800 per night.";
            Console.WriteLine($"👤 User: {userMessage1}\n");
            Console.WriteLine("🤖 Agent:");

            var thread = travelAgent.GetNewThread();
            await foreach (var update in travelAgent.RunStreamingAsync(userMessage1, thread))
            {
                await Task.Delay(10);
                Console.Write(update);
            }
            Console.WriteLine("\n");

            // Test 2: Use stored preferences
            var userMessage2 = "Can you help me find a hotel for my next trip? I'm thinking somewhere in Europe.";
            Console.WriteLine($"👤 User: {userMessage2}\n");
            Console.WriteLine("🤖 Agent:");

            await foreach (var update in travelAgent.RunStreamingAsync(userMessage2, thread))
            {
                await Task.Delay(10);
                Console.Write(update);
            }

            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("✅ Demo completed");
            Console.WriteLine(new string('=', 50));
        }

        static async Task CreateTravelIndexAsync(SearchIndexClient indexClient, string indexName)
        {
            var travelFields = new List<SearchField>
            {
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true },
                new SearchableField("name"),
                new SearchableField("description"),
                new SearchableField("location"),
                new SearchableField("amenities"),
                new SimpleField("price_per_night", SearchFieldDataType.Double),
                new SimpleField("rating", SearchFieldDataType.Double),
                new SearchableField("tags") { IsFilterable = true }
            };

            var travelIndex = new SearchIndex(indexName, travelFields);

            try
            {
                await indexClient.GetIndexAsync(indexName);
                Console.WriteLine($"✅ Index '{indexName}' already exists");
            }
            catch
            {
                await indexClient.CreateIndexAsync(travelIndex);
                Console.WriteLine($"✅ Created index '{indexName}'");
            }
        }

        static async Task PopulateHotelDataAsync(SearchClient searchClient)
        {
            // Try to upload documents - if they already exist, this will update them
            var sampleHotels = new List<Hotel>
            {
                new Hotel
                {
                    id = "1",
                    name = "Le Meurice Paris",
                    description = "Luxury palace hotel with Michelin-starred dining and views of the Tuileries Garden",
                    location = "Paris, France",
                    amenities = "Spa, Michelin Restaurant, Concierge, Room Service, Fitness Center",
                    price_per_night = 850,
                    rating = 4.8,
                    tags = new[] { "luxury", "romantic", "historic", "fine-dining", "spa" }
                },
                new Hotel
                {
                    id = "2",
                    name = "Four Seasons Maui",
                    description = "Beachfront resort with world-class spa and family-friendly activities",
                    location = "Maui, Hawaii",
                    amenities = "Beach Access, Kids Club, Multiple Pools, Spa, Golf Course",
                    price_per_night = 695,
                    rating = 4.7,
                    tags = new[] { "beach", "family-friendly", "resort", "spa", "golf" }
                },
                new Hotel
                {
                    id = "3",
                    name = "Aman Tokyo",
                    description = "Minimalist luxury hotel with panoramic city views and traditional onsen",
                    location = "Tokyo, Japan",
                    amenities = "Onsen, City Views, Fine Dining, Spa, Business Center",
                    price_per_night = 780,
                    rating = 4.9,
                    tags = new[] { "luxury", "business", "spa", "city", "minimalist" }
                },
                new Hotel
                {
                    id = "4",
                    name = "Hotel Sacher Vienna",
                    description = "Historic hotel home of the original Sachertorte with elegant rooms",
                    location = "Vienna, Austria",
                    amenities = "Historic Cafe, Concierge, Accessible Rooms, Pet-Friendly",
                    price_per_night = 420,
                    rating = 4.6,
                    tags = new[] { "historic", "accessible", "pet-friendly", "cultural", "cafe" }
                },
                new Hotel
                {
                    id = "5",
                    name = "Fairmont Whistler",
                    description = "Ski-in/ski-out resort with family suites and mountain views",
                    location = "Whistler, Canada",
                    amenities = "Ski Access, Family Suites, Heated Pool, Kids Programs",
                    price_per_night = 380,
                    rating = 4.5,
                    tags = new[] { "ski", "family-friendly", "mountain", "resort", "accessible" }
                }
            };

            try
            {
                await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(sampleHotels));
                Console.WriteLine($"✅ Uploaded/Updated {sampleHotels.Count} hotels to search index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Could not upload documents: {ex.Message}");
                Console.WriteLine("Continuing with existing index data...");
            }
        }
    }
}
