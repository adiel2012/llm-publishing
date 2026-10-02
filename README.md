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

## Deploying

Intended path: Docker image, then Amazon ECR, then AWS App Runner, giving a public HTTPS endpoint such as `https://calculator-mcp.example.com/mcp`. Set the App Runner health check path to `/health` and the port to `8080`.

## Publishing to Registries

This MCP can be published to three registries to make it available to the public:

```
        MCP          Claude       OpenAI
      Registry      Directory    Directory
        v            v            v
```

### Prerequisites for all registries:

1. **Deploy the server** to a public HTTPS endpoint (AWS App Runner recommended)
2. **Update [server.json](server.json)** with your actual values:
   - Replace `YOUR_GITHUB_USERNAME` with your GitHub username
   - Replace `calculator-mcp.example.com` with your real domain
   - Ensure the `remotes` URL points to your deployed HTTPS endpoint

### 1. MCP Registry

The official [modelcontextprotocol.io registry](https://modelcontextprotocol.io):

```powershell
# Install MCP Publisher (requires Node.js)
npm install -g @modelcontextprotocol/publisher

# Login with GitHub
mcp-publisher login github

# Validate your server.json
mcp-publisher validate

# Publish to the MCP Registry
mcp-publisher publish
```

After publishing, your MCP will be listed at: `https://modelcontextprotocol.io/servers`

### 2. Claude Directory

Make your MCP available in Claude Desktop and Claude.ai:

1. Publish to the MCP Registry first (see above)
2. The Claude team monitors the MCP Registry and may add top-tier MCPs to Claude Directory
3. Alternatively, contact Anthropic at `mcp@anthropic.com` with:
   - Your `server.json` metadata
   - Public HTTPS endpoint URL
   - Brief description of what your MCP does

Your MCP will then appear in:
- Claude Desktop's MCP library
- Claude.ai's custom tools section

### 3. OpenAI Directory

To publish to OpenAI's GPT custom actions marketplace:

1. [Create an OpenAI account](https://openai.com/signup) if you don't have one
2. Access [Custom GPTs](https://chatgpt.com/gpts/mine)
3. Create a new GPT or action
4. Provide your MCP's **OpenAPI spec** (convert from MCP if needed)
5. Configure authentication (if required)
6. Set the endpoint to your deployed HTTPS URL
7. Submit for review in the OpenAI directory

**Note:** OpenAI uses OpenAPI specs, not MCP specs. You may need to:
- Convert your MCP tools to OpenAPI format
- Document parameters and return types clearly
- Ensure endpoints are HTTPS with proper CORS headers

### Publishing Checklist

Before publishing to any registry:

- [ ] Server deployed to public HTTPS endpoint
- [ ] `server.json` updated with correct GitHub username and domain
- [ ] Health check endpoint responds with `{"status":"ok"}`
- [ ] MCP tools are functional and tested
- [ ] Add authentication/rate limiting if handling sensitive operations
- [ ] Document tool parameters clearly
- [ ] Add error handling for edge cases
- [ ] Ensure no secrets in code or Docker image

## Security

The server has no authentication, which is acceptable for a public calculator. Add authentication, rate limiting and logging before exposing anything sensitive. Never put secrets in `server.json`, source control, or the Docker image.
