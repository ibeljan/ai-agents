# Module 00 - Prerequisites - Deployment and Setup

**[Home](Home.md)** - [Creating Your First Agent >](./Module-01.md)

## Introduction

In this Module, you'll deploy the Azure Services needed to run this workshop and get your local environment configured and ready. You will also learn about the structure of this workshop and get an overview of Multi-Agent Systems.

## Learning Objectives and Activities

- Begin the deployment of the Azure Services
- Learn the structure and get an overview of this workshop
- Explore the core principals of multi-agent systems
- Complete the configuration of your local environment
- Compile and run the starter solution locally

## Module Exercises

1. [Activity 1: Configure Workshop Environment](#activity-1-configure-workshop-environment)
1. [Activity 2: Deploy Azure Services](#activity-2-deploy-azure-services)
1. [Activity 3: Workshop Structure and Overview Session](#activity-3-workshop-structure-and-overview-session)
1. [Activity 4: Configure Environment Variables](#activity-4-configure-environment-variables)
1. [Activity 5: Compile and Run](#activity-5-compile-and-run)

## Activity 1 Configure Workshop Environment

Complete the following tasks in order to prepare your environment for this workshop.

**Note:** These pre-requisites are a hard requirement for successful completion of this workshop. If you do not have all three you will not be able to successfully complete this workshop.

### Prerequisites

- Laptop or workstation with **administrator rights** (Alternatively you can run this workshop virtually in [GitHub Codespaces](https://github.com/features/codespaces))

  #### Checking Azure OpenAI quota limits

  For this sample to deploy successfully, there needs to be enough Azure OpenAI quota for the models used by this sample within your subscription. This sample deploys a new Azure OpenAI account with two models, **gpt-4o with 30K tokens** per minute and **text-3-embedding-small with 5k tokens** per minute. For more information on how to check your model quota and change it, see [Manage Azure OpenAI Service Quota](https://learn.microsoft.com/azure/ai-services/openai/how-to/quota)

### Get Started

You can choose from the following options to get started with the workshop.

#### GitHub Codespaces

You can run this sample app and workshop virtually by using GitHub Codespaces (requires a GitHub account). The button will open a web-based VS Code instance in your browser:

1. Open the template (this may take several minutes):

   [![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/AzureCosmosDB/banking-multi-agent-workshop?branch=start&devcontainer_path=.devcontainer%2Fpython%2Fdevcontainer.json)

2. Move on to the [Deployment](Module-00.md#deployment) section.

#### Local Environment without VS Code Dev Containers

1. To run the workshop locally on your machine, install the following:

   - [Docker Desktop](https://docs.docker.com/desktop/)
   - [Git](https://git-scm.com/downloads)
   - [Azure Developer CLI (azd)](https://aka.ms/install-azd)
   - [Python 3.12+](https://www.python.org/downloads/)
   - Your Python IDE or [VS Code](https://code.visualstudio.com/Download) with [Python Extension](https://marketplace.visualstudio.com/items?itemName=ms-python.python)
   - To build and run the frontend component, install [Node.js](https://nodejs.org/en/download/) and [Angular CLI](https://angular.dev/installation#install-angular-cli)

2. Clone the repository and navigate to the folder:

   ```bash
   git clone https://github.com/ibeljan/ai-agents/
   cd Labs\Python\13-MultiAgent-Langraph
   git checkout start
   ```

3. Move on to the [Deployment](Module-00.md#deployment) section.


## Activity : Configure Environment Variables

You will be provided with endpoints and configuration values for the required resources into a `.env` file at root (python) folder.

But you will still need to install dependencies to run the solution locally.

1. Navigate to the python folder of the project.
2. Create and activate a virtual environment (Linux/Mac):

   ```shell
   python -m venv .venv
   source .venv/bin/activate
   ```

   For Windows:

   ```shell
   python -m venv .venv
   .venv\Scripts\Activate.ps1
   ```

3. Install the required dependencies for the project.

   ```shell
   pip install -r ../src/app/requirements.txt
   ```
   Note: If getting `requirements.txt` file not found when using GitHub codespaces, please navigate to the `src/app` folder and run the command there ` pip install -r requirements.txt`

## Activity 5: Compile and Run

### Running the solution

1. Navigate to the python folder of the project.
2. Start the fastapi server.

   ```shell
   uvicorn src.app.banking_agents_api:app --reload --host 0.0.0.0 --port 8000
   ```

The API will be available at `http://localhost:8000/docs`. This has been pre-built with boilerplate code that will create chat sessions and store the chat history in Cosmos DB.

#### Run the Frontend App locally

1. Update the `apiUrl` values in `frontend/src/environments/environment.ts` file with the API endpoint (http://localhost:8000/)
1. Open a new terminal, navigate to the `frontend` folder and run the following to start the application:

   ```sh
   npm install
   npm start
   ```
1. Open your browser and navigate to <http://localhost:8000/>.

#### Run the Frontend App on Codespaces


1. Open a new terminal, navigate to the `frontend` folder and run the following to start the application:

   ```sh
   npm install
   npm start
   ```
1. From the **Ports** tab:
   1. Right-click and select the **Port Visibility** option to set port **ChatAPI (8000)** as **Public**.
   1. For the port with the label **Frontend app**. Hover over the address and choose **Open in Browser** (second icon) to access the frontend application.

Lets try a couple of things:

- Try out the API by creating a chat session in the front end. This should return a response saying "Hello, I am not yet implemented".
- Navigate to the Cosmos DB account in the Azure portal to view the containers. You should see an entry in the `Chat` container. If you selected "yes" to the option during `azd up`, there will also be some transactional data in the `OffersData`, `AccountsData`, and `Users` containers as well.
- Take a look at the files in the `src/app/services` folder - these are the boilerplate code for interacting with the Cosmos DB and Azure OpenAI services.
- You will also see an empty file `src/app/banking_agents.py` as well as empty files in the `src/app/tools` and `src/app/prompts` folder. This is where you will build your multi-agent system!

Next, we will start building the agents that will be served by the API layer and interact with Cosmos DB and Azure OpenAI using LangGraph!

### Deployment Validation

Use the steps below to validate that the solution was deployed successfully.

- [ ] You can compile the solution in CodeSpaces or locally
- [ ] You can start the project and it runs without errors

### Common Issues and Troubleshooting

1. Azure OpenAI deployment issues:

- Ensure your subscription has access to Azure OpenAI
- Check regional availability

1. Python environment issues:

- Ensure correct Python version
- Verify all dependencies are installed

1.

## Success Criteria

To complete this Module successfully, you should be able to:

- Verify that all services have been deployed successfully.
- Have an open IDE or CodeSpaces session with the source code and environment variables loaded.
- Be able to compile and run the application with no warnings or errors.

## Next Steps

Proceed to [Creating Your First Agent](./Module-01.md)

## Resources

- [azd Command Reference](https://learn.microsoft.com/azure/developer/azure-developer-cli/reference)
- [Semantic Kernel Agent Framework](https://learn.microsoft.com/semantic-kernel/frameworks/agent)
- [LangGraph](https://langchain-ai.github.io/langgraph/concepts/)
- [Azure OpenAI Service documentation](https://learn.microsoft.com/azure/cognitive-services/openai/)
- [Azure Cosmos DB Vector Database](https://learn.microsoft.com/azure/cosmos-db/vector-database)

