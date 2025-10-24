# AI Agents A Course

![Generative AI](./images/repo-thumbnailv2.png)

## A course teaching everything you need to know to start Agent Workshop for Adobe :)
## Setup Instructions

### 1. Install Python 3.13.9
- Download from https://www.python.org/downloads/release/python-3139/
- Windows: during install enable "Add Python to PATH".
- macOS (Homebrew): `brew install python@3.13`
- Verify version:
```bash
python --version
```
```powershell
python --version
```
Expected output: `Python 3.13.9`

### 2. Create Virtual Environment (Python 3.13.9)
```bash
# Windows (CMD or Git Bash)
python -m venv .venv
./.venv/Scripts/activate

# macOS / Linux
python3 -m venv .venv
source .venv/bin/activate
```
```powershell
# Windows PowerShell
python -m venv .venv
./.venv/Scripts/Activate.ps1
```
Upgrade packaging tools:
```bash
python -m pip install --upgrade pip setuptools wheel
```
```powershell
python -m pip install --upgrade pip setuptools wheel
```

### 3. Install VS Code
- Download from https://code.visualstudio.com
- Launch and install extensions:
    - Python (ms-python.python)
    - Pylance (ms-python.vscode-pylance)
    - Jupyter (ms-toolsai.jupyter) if notebooks needed
    - Docker (ms-azuretools.vscode-docker)
    - Dev Containers (ms-vscode-remote.remote-containers)
    - GitHub Copilot (GitHub.copilot) optional

### 4. Configure VS Code Python Interpreter
- Open Command Palette: "Python: Select Interpreter"
- Choose `.venv` environment (ensure path shows Python 3.13.9)
- Install dependencies from `requirements.txt`:
```bash
pip install -r requirements.txt
```
```powershell
pip install -r requirements.txt
```

### 5. Install Node.js (for frontend or tooling)
- Download LTS from https://nodejs.org
- Verify:
```bash
node -v
npm -v
```
```powershell
node -v
npm -v
```
- Optional: enable Corepack for pnpm/yarn
```bash
corepack enable
```
```powershell
corepack enable
```

### 6. Install Docker Desktop
- Download from https://www.docker.com/products/docker-desktop
- Enable virtualization (BIOS) if required (Windows)
- Verify Docker works:
```bash
docker info
docker run hello-world
```
```powershell
docker info
docker run hello-world
```

### 7. Install Azure CLI (az) & Azure Developer CLI (azd)
Azure tooling is required for deploying resources like Azure Container Apps.

#### Windows (recommended via winget)
```powershell
winget install -e --id Microsoft.AzureCLI
winget install -e --id Microsoft.Azd
```
If winget is unavailable:
1. Download Azure CLI MSI: https://aka.ms/installazurecliwindows
2. Download azd MSI: https://azure.github.io/azure-dev/azure-dev/install.html

#### macOS (Homebrew)
```bash
brew update
brew install azure-cli
brew tap azure/azure-dev
brew install azd
```

#### Linux (apt example Ubuntu/Debian)
```bash
curl -fsSL https://aka.ms/InstallAzureCLIDeb | sudo bash
curl -fsSL https://azure-dev.azureedge.net/azd/install.sh | bash
```

#### Verify installations
```bash
az version
azd version
```
```powershell
az version
azd version
```

#### Login
```bash
az login
azd auth login
```
```powershell
az login
azd auth login
```

#### Set subscription (replace <SUB_ID>)
```bash
az account set --subscription <SUB_ID>
azd config set defaults.subscription <SUB_ID>
```
```powershell
az account set --subscription <SUB_ID>
azd config set defaults.subscription <SUB_ID>
```

#### Optional: Install Bicep CLI (for infra-as-code)
```bash
az bicep upgrade
```
```powershell
az bicep upgrade
```

### 8. (Optional) Dev Containers Workflow

### 7. (Optional) Dev Containers Workflow
- Add `.devcontainer/devcontainer.json` for consistent environment
- Reopen in container via Command Palette

### 9. Common Commands
```bash
pip install package_name
```
```powershell
pip install package_name
```

### 10. Cleanup / Deactivate
```bash
deactivate
```
```powershell
deactivate
```

Ready to build and run Python agents locally with Python 3.13.9 and Azure tooling.