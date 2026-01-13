# DevUI Sample - Travel Booking Agents

This sample demonstrates the DevUI with Azure AI Foundry and multi-agent workflows for travel booking.

## Prerequisites

- .NET 9.0 SDK or later
- Azure AI Foundry endpoint and model configured
- Environment variables set:
  - `AZURE_AI_FOUNDRY_ENDPOINT`
  - `AZURE_AI_FOUNDRY_MODEL` (optional, defaults to gpt-4o-mini)

## Running the Sample

1. Navigate to the DevUISample directory:
   ```powershell
   cd DevUISample
   ```

2. Restore packages:
   ```powershell
   dotnet restore
   ```

3. Run the application:
   ```powershell
   dotnet run
   ```

4. Open the DevUI in your browser:
   - **DevUI**: https://localhost:50516/devui
   - **OpenAI Responses API**: https://localhost:50516/v1/responses

## Agents Available

- **FrontDesk**: Travel agent providing destination recommendations
- **Concierge**: Reviews recommendations for authenticity and local experiences
- **travel-review-workflow**: Sequential workflow combining both agents

## Features

- Interactive web interface for testing agents
- Real-time streaming responses
- Multi-agent workflow coordination
- Python DevUI compatibility endpoint
