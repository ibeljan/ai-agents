
# Azure Setup (Azure AI Foundry + Azure AI Search)

This guide walks you through creating:

- An **Azure AI Foundry resource** + **Foundry project** (and a model deployment you can call from code)
- An **Azure AI Search** service (for the memory/RAG labs)

It’s written for the **Azure Portal** / **Azure AI Foundry UI** and assumes you’re doing everything manually.

---

## Prerequisites

1. An Azure subscription where you can create resources.
2. Permissions: at least **Contributor** on the subscription/resource group.
3. Pick a single Azure region and keep resources in the same region when possible.
	- Azure AI Search and your model endpoint should ideally be co-located to reduce latency.

---

## Step 1 — Create a Resource Group

1. Open the Azure Portal: https://portal.azure.com
2. Search for **Resource groups** → select it.
3. Select **Create**.
4. Fill in:
	- **Subscription**: choose your subscription
	- **Resource group**: e.g., `rg-ai-agents-labs`
	- **Region**: pick one region and remember it
5. Select **Review + create** → **Create**.

---

## Step 2 — Create Azure AI Search (Azure Portal)

1. In https://portal.azure.com, select **Create a resource**.
2. Search for **Azure AI Search** (sometimes shown as “Azure Cognitive Search”).
3. Select **Create**.
4. On the Basics tab:
	- **Subscription**: your subscription
	- **Resource group**: `rg-ai-agents-labs`
	- **Service name**: globally unique, e.g. `aisearch-yourname-labs`
	- **Location**: same region as your other resources
	- **Pricing tier**: choose one appropriate for your demo/lab budget
	  - For learning, start with a low-cost tier; you can scale later.
5. Select **Review + create** → **Create**.

### Get your Search endpoint and key

1. Open the created **Azure AI Search** resource.
2. Copy the **URL** from the Overview page (it looks like `https://<name>.search.windows.net`).
3. In the left nav, go to **Keys**.
4. Copy **Primary admin key** (or Secondary).

You will use these in the labs as environment variables (see “Step 4”).

---

## Step 3 — Create Azure AI Foundry Resource + Project (no hubs)

Microsoft’s current guidance is to use a **Foundry resource** with a **Foundry project** for agent- and model-centric work. Hub-based projects are a legacy path and not recommended for these labs unless you specifically need hub-only features.

### 3A) Create the Foundry resource (Azure Portal)

1. Open the Azure Portal: https://portal.azure.com
2. Create a Foundry resource (this opens the correct create blade):
   - https://portal.azure.com/#create/Microsoft.CognitiveServicesAIFoundry
3. Fill in:
   - **Subscription**: your subscription
   - **Resource group**: `rg-ai-agents-labs`
   - **Region**: same region as your other resources
   - **Name**: e.g. `foundry-yourname-labs`
4. Select **Review + create** → **Create**.

### 3B) Create a Foundry project (Foundry portal)

1. Open Foundry portal: https://ai.azure.com
2. Make sure you’re using the **new Foundry** experience (the UI has a “New Foundry” toggle).
3. Select **Create new** (or **Create project**).
4. When prompted for project type/resource, select **Foundry resource** (not “hub”).
5. Select your subscription/resource group and the Foundry resource you created.
6. Name the project (e.g. `ai-agents-labs`) and create it.

### Deploy a chat model (so the C# labs can call it)

Exact labels can vary by UI updates, but the flow is generally:

1. In your project, go to **Models** / **Model catalog** or **Deployments**.
2. Choose a chat model (for example, a small/fast option suitable for demos).
3. Select **Deploy**.
4. Set:
	- **Deployment name**: e.g. `gpt-4o-mini` (this becomes your “deployment”/model name in code)
	- Confirm region matches your project
5. Create the deployment.

### Get the endpoint you’ll use from code

Foundry supports multiple endpoint styles depending on which SDK/API you use:

- **Foundry resource endpoint** (common in Foundry documentation): `https://<resource-name>.services.ai.azure.com`
- **Azure OpenAI endpoint** (classic Azure OpenAI resource pattern): `https://<resource-name>.openai.azure.com/`

From your project/resource UI, capture:

1. The **endpoint** your sample expects (one of the formats above)
2. The **deployment name** you created (e.g. `gpt-4o-mini`)

---

## Step 4 — Configure environment variables for the labs

Set these in your shell (PowerShell examples below). Adjust names if a specific lab README asks for different variable names.

### Azure AI Search

```powershell
$env:AZURE_SEARCH_ENDPOINT = "https://<your-search-name>.search.windows.net"
$env:AZURE_SEARCH_ADMIN_KEY = "<your-admin-key>"
$env:AZURE_SEARCH_INDEX_NAME = "travel-hotels"
```

### Azure AI Foundry / Azure OpenAI

Depending on the sample, you may see either “Foundry” or “OpenAI” naming.

If the sample expects an **Azure OpenAI endpoint** (`...openai.azure.com`):

```powershell
$env:AZURE_OPENAI_ENDPOINT = "https://<your-openai-resource>.openai.azure.com/"
$env:AZURE_OPENAI_DEPLOYMENT_NAME = "<your-deployment-name>"  # e.g. gpt-4o-mini
```

If the sample expects a **Foundry resource endpoint** (`...services.ai.azure.com`), set it like:

```powershell
$env:AZURE_AI_FOUNDRY_ENDPOINT = "https://<your-foundry-resource>.services.ai.azure.com"
$env:AZURE_AI_FOUNDRY_MODEL = "<your-deployment-name>"  # the deployment name routes the request
```

If a lab uses Foundry-specific names, set those too:

```powershell
$env:AZURE_AI_FOUNDRY_ENDPOINT = $env:AZURE_AI_FOUNDRY_ENDPOINT  # or map from AZURE_OPENAI_ENDPOINT if needed
$env:AZURE_AI_FOUNDRY_MODEL = $env:AZURE_AI_FOUNDRY_MODEL        # or map from AZURE_OPENAI_DEPLOYMENT_NAME
```

---

## Step 5 — Quick validation checklist

- Azure AI Search resource exists and you can see **Overview URL** and **Keys**.
- Azure AI Foundry **resource** exists and at least one **project** exists.
- A model deployment exists in the project.
- Your environment variables are set in the terminal session you run the labs from.

---

## Troubleshooting

- **403 / permission errors**: confirm you have contributor access to the resource group and your account is selected in the portal.
- **Region mismatch**: if the model and search are in different regions, you may see latency and sometimes restricted SKU/model availability.
- **Keys vs Entra auth**: some samples use keys; others use Entra ID (e.g., `AzureCliCredential`). Follow the specific lab’s authentication approach.

