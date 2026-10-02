# Calculator MCP

A simple remote [Model Context Protocol](https://modelcontextprotocol.io) server written in C# (ASP.NET Core, .NET 10). It exposes two tools over the Streamable HTTP transport:

| Tool | Arguments | Description |
|---|---|---|
| `add` | `a`, `b` (numbers) | Returns `a + b` |
| `multiply` | `a`, `b` (numbers) | Returns `a * b` |

## Endpoints

| Path | Purpose |
|---|---|
| `/mcp` | MCP endpoint (Streamable HTTP) |
| `/health` | Health check, returns `{"status":"ok"}` |

## Project layout

```text
.
├── CalculatorMcp.csproj
├── Program.cs              # Server setup and endpoint mapping
├── Tools/
│   └── CalculatorTools.cs  # add / multiply tools
├── Dockerfile
├── docker-compose.yml
├── server.json             # MCP Registry metadata
└── README.md
```

## Run locally

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

Run these from the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --urls http://localhost:5099
```

Stop the server with `Ctrl+C`. The MCP endpoint is then `http://localhost:5099/mcp`, and the health check is `http://localhost:5099/health`. Add it as a custom connector in any MCP client, or test it with the MCP Inspector:

```powershell
npx @modelcontextprotocol/inspector
```

### Quick manual test

```powershell
$h = @{ Accept = "application/json, text/event-stream" }
Invoke-WebRequest http://localhost:5099/mcp -Method Post -ContentType "application/json" -Headers $h `
  -Body '{"jsonrpc":"2.0","id":1,"method":"tools/list"}' | Select-Object -ExpandProperty Content
```

## Docker

Requires [Docker Desktop](https://www.docker.com/products/docker-desktop/). Run these from the repository root:

### Docker Compose (recommended)

```powershell
# Build and start in the background
docker compose up -d --build

# Check it, view logs
curl http://localhost:8080/health
docker compose logs -f

# Stop and remove
docker compose down
```

To run in the foreground instead, use `docker compose up --build` (Ctrl+C to stop).

### Plain Docker

```powershell
docker build -t calculator-mcp .
docker run --rm -p 8080:8080 --name calculator-mcp calculator-mcp
```

The container listens on port 8080 and runs as a non-root user. The MCP endpoint is `http://localhost:8080/mcp`.

## MCP Server

This is a **Model Context Protocol (MCP)** server that exposes calculator tools through the MCP interface:

- **Endpoint:** `/mcp` (JSON-RPC over HTTP)
- **Spec:** [server.json](server.json)
- **Used by:** Claude Desktop, Claude.ai, and other MCP-compatible clients
- **Tools exposed:** `add`, `multiply`

## Using with Claude Desktop

To use this MCP server locally with Claude Desktop:

### 1. Start the server

Using Docker Compose (recommended):
```powershell
docker compose up -d --build
```

Or run locally with .NET:
```powershell
dotnet run --urls http://localhost:5099
```

### 2. Edit Claude Desktop config

Locate and edit your Claude Desktop configuration file:

**On Windows:**
```
%APPDATA%\Claude\claude_desktop_config.json
```

**On macOS:**
```
~/.claude/claude_desktop_config.json
```

### 3. Add the MCP server

Add an `mcpServers` section with your calculator server:

```json
{
  "mcpServers": {
    "calculator": {
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

If using local .NET instead of Docker, use `http://localhost:5099/mcp`.

### 4. Restart Claude Desktop

Close and reopen Claude Desktop to load the new MCP configuration.

### 5. Test it

In Claude Desktop, ask Claude to:
- "Add 15 and 27"
- "Multiply 8 by 12"

Claude will use your MCP server's `add` and `multiply` tools automatically.

### Troubleshooting

- **MCP not showing up:** Verify the server is running (`curl http://localhost:8080/health`)
- **Connection refused:** Check Docker container status with `docker ps`
- **View logs:** `docker compose logs -f calculator-mcp`

## Deploying to AWS

To make your MCP accessible to ChatGPT and other remote clients, deploy it to AWS:

### Option 1: AWS App Runner (Easiest)

1. Build Docker image:
   ```powershell
   docker build -t calculator-mcp .
   ```

2. Push to Amazon ECR:
   ```powershell
   aws ecr create-repository --repository-name calculator-mcp
   docker tag calculator-mcp:latest ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/calculator-mcp:latest
   docker push ACCOUNT_ID.dkr.ecr.REGION.amazonaws.com/calculator-mcp:latest
   ```

3. Create AWS App Runner service:
   - Source: Amazon ECR image
   - Port: `8080`
   - Health check path: `/health`
   - CPU: 1 vCPU
   - Memory: 2 GB

4. Result: Public HTTPS endpoint `https://calculator-mcp.RANDOM.us-east-1.awsapprunner.com/mcp`

### Option 2: ECS + Fargate + ALB

For more control, use ECS Fargate with an Application Load Balancer:

```
ALB (Application Load Balancer)
  ↓
Fargate Task
  ↓
C# MCP Server
```

Either approach works; the critical requirement is a publicly reachable HTTPS URL ending in `/mcp`.

## Publishing to MCP Ecosystem

Your MCP server can reach multiple AI platforms through direct connection and registry publication:

```
                    AWS Deployment
                    https://your-domain.com/mcp
                           │
          ┌────────────────┼────────────────┐
          │                │                │
          ▼                ▼                ▼
      ChatGPT           Claude          Other MCP
      (Direct)         Registry          Clients
                          │
                          ▼
                  MCP Ecosystem
                   Discovery/Catalog
```

**Key insight:** AWS hosting, MCP Registry publication, and ChatGPT discovery are **separate paths**. ChatGPT can connect directly to your remote MCP server without registry publication.

## Path 1: ChatGPT Integration (Direct Connection)

Connect your MCP directly to ChatGPT without waiting for registry publication.

### Step 1: Deploy to AWS

Deploy your C# MCP server to a public HTTPS endpoint. Recommended architecture:

```
                Internet
                   │
                   ▼
        ┌─────────────────────┐
        │  AWS API Gateway    │
        │  or ALB             │
        └──────────┬──────────┘
                   │
                   ▼
        ┌─────────────────────┐
        │  ECS / Fargate      │
        │  (C# MCP Server)    │
        └─────────────────────┘
```

Result: `https://your-domain.com/mcp`

### Step 2: Ensure `/mcp` endpoint is exposed

Your server exposes the MCP endpoint:
```
POST https://your-domain.com/mcp
Content-Type: application/json

{"jsonrpc":"2.0","id":1,"method":"tools/list"}
```

### Step 3: Test with MCP Inspector

Before connecting to ChatGPT, test your remote server:

```powershell
npx @modelcontextprotocol/inspector https://your-domain.com/mcp
```

Verify these work:
- `initialize` → server responds
- `tools/list` → shows `add` and `multiply`
- `tools/call` → successfully calls `add(2,4)` → returns `6`

### Step 4: Connect to ChatGPT Developer Mode

1. Open ChatGPT
2. Go to **Settings** → **Developer mode**
3. Click **Create app** or **Add remote MCP**
4. Enter your endpoint: `https://your-domain.com/mcp`
5. Configure authentication if needed (optional for testing)
6. Click **Scan tools**
7. ChatGPT discovers `add` and `multiply` tools
8. Click **Create**
9. Test: Ask ChatGPT "add 15 and 27"

### Step 5: Publish as ChatGPT App (Optional)

For Business/Enterprise/Edu workspaces:
1. Once tested, proceed through **Draft** → **Test** → **Review** → **Publish**
2. App becomes available in your workspace's approved connectors

**Current support:** Developer Mode for Plus/Pro; full MCP support rolling out for Business/Enterprise/Edu

---

## Path 2: MCP Registry Publication (Ecosystem Discovery)

Publish to the [official MCP Registry](https://registry.modelcontextprotocol.io) for broader MCP ecosystem discovery.

**Important:** Registry publication is **separate from** ChatGPT integration. The registry helps other MCP clients discover your server, but doesn't automatically integrate with ChatGPT.

### Step 1: Prepare metadata

Update [server.json](server.json):
- Replace `YOUR_GITHUB_USERNAME` with your GitHub username
- Replace `calculator-mcp.example.com` with your actual domain
- Ensure `remotes[0].url` points to your deployed HTTPS endpoint: `https://your-domain.com/mcp`

### Step 2: Publish to MCP Registry

```powershell
# Install MCP Publisher (requires Node.js)
npm install -g @modelcontextprotocol/publisher

# Login with GitHub
mcp-publisher login github

# Validate your server.json
mcp-publisher validate

# Publish to the official MCP Registry
mcp-publisher publish
```

After publishing:
- Listed at: [registry.modelcontextprotocol.io](https://registry.modelcontextprotocol.io)
- Other MCP clients can discover your server
- Claude ecosystem participants can find your MCP

### Step 3: Maintain your listing

- Keep `server.json` updated with new versions
- Maintain your GitHub repository
- Document changes in `README.md`

---

## Path 3: OpenAI API Integration (Application Level)

For applications using OpenAI's API, use remote MCP tools:

```powershell
# Your application
your-app.js
    │
    ├─→ OpenAI Responses API
    │       │
    │       └─→ Remote MCP tool
    │               │
    │               └─→ https://your-domain.com/mcp
    │                       │
    │                       └─→ AWS (Your C# server)
    │
    └─→ Call add/multiply via your MCP
```

Your MCP server automatically works with OpenAI's remote MCP tooling.


### Pre-Publication Checklist

**Before connecting to ChatGPT:**

- [ ] Server deployed to AWS with public HTTPS URL
- [ ] `/mcp` endpoint is accessible and responds to POST requests
- [ ] Health check (`/health`) responds with `{"status":"ok"}`
- [ ] MCP tools (`add`, `multiply`) work correctly
- [ ] Tested with MCP Inspector: `npx @modelcontextprotocol/inspector https://your-domain.com/mcp`
- [ ] `initialize` → `tools/list` → `tools/call` workflow verified
- [ ] Tool descriptions are clear and helpful
- [ ] Add OAuth/authentication if exposing sensitive operations
- [ ] No secrets in code or Docker image
- [ ] Server handles errors gracefully

**Before MCP Registry publication:**

- [ ] `server.json` has correct GitHub username and domain
- [ ] `server.json` `remotes[0].url` points to your deployed endpoint
- [ ] Server is stable and tested in production
- [ ] Documentation in README.md is complete
- [ ] Semantic versioning in `server.json`


## Summary: The 2026 Path

| Goal | Required | Steps |
|---|---|---|
| **Use locally with Claude Desktop** | Docker + local config | Run locally, add to `claude_desktop_config.json` |
| **Connect to ChatGPT** | AWS deployment only | Deploy to AWS, test, connect in ChatGPT Developer Mode |
| **Reach MCP ecosystem** | AWS + MCP Registry | Deploy to AWS, publish to registry, maintain |
| **Use with OpenAI API apps** | AWS deployment + OpenAI SDK | Deploy to AWS, use remote MCP tool in API |

### What You DON'T Need

```
❌ REST API endpoints
❌ OpenAPI spec
❌ Multiple registries
❌ ChatGPT app publication before testing
```

Just:
```
✅ AWS deployment (HTTPS)
✅ Remote MCP endpoint (/mcp)
✅ Direct connection to ChatGPT
✅ MCP Registry for discovery (optional)
```

---

## Security

The server currently has no authentication. For testing a harmless calculator, this is acceptable. For production services handling sensitive operations:

- Add **OAuth 2.0 / OpenID Connect** authentication
- Ensure **refresh-token support** for long-lived connections
- Implement **rate limiting** and **logging**
- Never store secrets in code, Docker images, or `server.json`
- Only expose necessary tools

The same authentication mechanism works for both ChatGPT and MCP Registry connections.
