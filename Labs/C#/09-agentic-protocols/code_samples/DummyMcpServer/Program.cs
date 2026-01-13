using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WeatherTool>()
    .WithTools<CalculatorTool>()
    .WithTools<TimeTool>()
    .WithTools<EchoTool>();

var app = builder.Build();

Console.Error.WriteLine("[Dummy MCP Server] Starting stdio MCP server...");
Console.Error.WriteLine("[Dummy MCP Server] Registered 4 tools: get_weather, calculate, get_current_time, echo");

await app.RunAsync();

Console.Error.WriteLine("[Dummy MCP Server] Shutting down...");

[McpServerToolType]
public class WeatherTool
{
    [McpServerTool(Name = "get_weather"), Description("Get the current weather for a specified location")]
    public static string GetWeather([Description("The city and country, e.g. 'Paris, France'")] string location)
    {
        Console.Error.WriteLine($"[Dummy MCP Server] get_weather called with location: {location}");

        var weatherData = new
        {
            location = location,
            temperature = 22,
            condition = "Sunny",
            humidity = 65,
            wind_speed = 12
        };

        return JsonSerializer.Serialize(weatherData);
    }
}

[McpServerToolType]
public class CalculatorTool
{
    [McpServerTool(Name = "calculate"), Description("Perform basic arithmetic calculations")]
    public static string Calculate(
        [Description("The operation to perform: add, subtract, multiply, or divide")] string operation,
        [Description("The first number")] double a,
        [Description("The second number")] double b)
    {
        Console.Error.WriteLine($"[Dummy MCP Server] calculate called: {a} {operation} {b}");

        double result = operation switch
        {
            "add" => a + b,
            "subtract" => a - b,
            "multiply" => a * b,
            "divide" => b != 0 ? a / b : throw new InvalidOperationException("Cannot divide by zero"),
            _ => throw new InvalidOperationException($"Unknown operation: {operation}")
        };

        return JsonSerializer.Serialize(new { operation, a, b, result });
    }
}

[McpServerToolType]
public class TimeTool
{
    [McpServerTool(Name = "get_current_time"), Description("Get the current date and time")]
    public static string GetCurrentTime([Description("Optional timezone (e.g., 'UTC', 'PST'). Defaults to local time.")] string? timezone = null)
    {
        Console.Error.WriteLine("[Dummy MCP Server] get_current_time called");

        var currentTime = DateTime.Now;
        var result = new
        {
            timestamp = currentTime.ToString("o"),
            formatted = currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
            timezone = TimeZoneInfo.Local.DisplayName
        };

        return JsonSerializer.Serialize(result);
    }
}

[McpServerToolType]
public class EchoTool
{
    [McpServerTool(Name = "echo"), Description("Echo back the provided message")]
    public static string Echo([Description("The message to echo back")] string message)
    {
        Console.Error.WriteLine($"[Dummy MCP Server] echo called with: {message}");

        return JsonSerializer.Serialize(new
        {
            original = message,
            echoed = message,
            length = message.Length,
            reversed = new string(message.Reverse().ToArray())
        });
    }
}