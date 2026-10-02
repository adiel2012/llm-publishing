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

## Deploying

Intended path: Docker image, then Amazon ECR, then AWS App Runner, giving a public HTTPS endpoint such as `https://calculator-mcp.example.com/mcp`. Set the App Runner health check path to `/health` and the port to `8080`.

## Publishing to the MCP Registry

Before publishing, edit [server.json](server.json) and replace the placeholders:

- `YOUR_GITHUB_USERNAME` with your GitHub username
- `calculator-mcp.example.com` with your real domain

Then:

```powershell
mcp-publisher login github
mcp-publisher validate
mcp-publisher publish
```

## Security

The server has no authentication, which is acceptable for a public calculator. Add authentication, rate limiting and logging before exposing anything sensitive. Never put secrets in `server.json`, source control, or the Docker image.
