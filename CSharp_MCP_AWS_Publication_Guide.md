# C# MCP Publication Guide: AWS → MCP Registry → Claude → ChatGPT

## Overview

This guide shows how to build one C# MCP server, deploy it to AWS, and make it available through multiple MCP ecosystems.

The important concept is:

```text
                    YOUR C# MCP
                         |
                         v
                  ASP.NET Core
                         |
                         v
                    Docker
                         |
                         v
                   AWS Hosting
                         |
                         v
              https://yourdomain.com/mcp
                         |
          +--------------+--------------+
          |              |              |
          v              v              v
     MCP Registry     Claude         ChatGPT
                     Directory      Directory
```

You build **one MCP server**. The registries/directories are separate discovery and distribution channels.

---

# 1. Build the C# MCP

For the simple example, create a calculator MCP with:

```text
add(a, b)
multiply(a, b)
```

A basic ASP.NET Core MCP server can expose these as MCP tools.

Example project:

```text
CalculatorMcp/
├── CalculatorMcp.csproj
├── Program.cs
├── Tools/
│   └── CalculatorTools.cs
├── Dockerfile
├── server.json
└── README.md
```

The remote endpoint will eventually be:

```text
https://calculator-mcp.example.com/mcp
```

For remote distribution, use an MCP HTTP transport supported by the clients you intend to serve.

---

# 2. Deploy the MCP to AWS

A simple deployment architecture is:

```text
C# ASP.NET Core
        |
        v
Docker image
        |
        v
Amazon ECR
        |
        v
AWS App Runner
        |
        v
HTTPS endpoint
```

For example:

```text
https://calculator-mcp.example.com/mcp
```

Before publishing anywhere, verify:

- HTTPS works.
- `/mcp` is publicly reachable.
- MCP initialization works.
- Tool discovery works.
- `add` and `multiply` work.
- Authentication is configured if the tools should not be public.
- Your domain and privacy/security documentation are ready.

AWS is hosting the actual MCP server. The directories do not host the running MCP.

---

# 3. Test with Claude Before Publishing

You do **not** need the Claude Directory just to use your own MCP.

Use Claude's custom remote connector functionality and provide:

```text
Name:
My Calculator

URL:
https://calculator-mcp.example.com/mcp
```

Claude should connect to the server and discover:

```text
add
multiply
```

Conceptually:

```text
AWS MCP
   |
   v
https://calculator-mcp.example.com/mcp
   |
   v
Claude Custom Connector
   |
   v
Tool discovery
   |
   +-- add
   +-- multiply
```

This is the fastest way to verify that the remote MCP actually works.

---

# 4. Publish to the Official MCP Registry

The official MCP Registry is a catalog for MCP servers.

It does not replace your AWS server.

Think of it as:

```text
MCP Registry
      |
      | metadata/discovery
      v
https://calculator-mcp.example.com/mcp
      |
      v
AWS
```

## 4.1 Create `server.json`

Example:

```json
{
  "$schema": "https://static.modelcontextprotocol.io/schemas/2025-12-11/server.schema.json",
  "name": "io.github.YOUR_GITHUB_USERNAME/calculator-mcp",
  "description": "A simple calculator MCP server.",
  "title": "Calculator MCP",
  "version": "1.0.0",
  "websiteUrl": "https://calculator-mcp.example.com",
  "repository": {
    "url": "https://github.com/YOUR_GITHUB_USERNAME/calculator-mcp",
    "source": "github"
  },
  "remotes": [
    {
      "type": "streamable-http",
      "url": "https://calculator-mcp.example.com/mcp"
    }
  ]
}
```

Replace:

```text
YOUR_GITHUB_USERNAME
```

with your actual GitHub identity.

The exact Registry schema should be checked before publishing because schemas and CLI versions can change.

---

# 5. Install the MCP Publisher CLI

The official MCP Registry provides the `mcp-publisher` CLI.

Verify it:

```powershell
mcp-publisher --help
```

Typical commands include:

```text
init
login
validate
publish
status
```

---

# 6. Authenticate

For a GitHub namespace:

```powershell
mcp-publisher login github
```

The GitHub authentication flow establishes ownership of the namespace.

Your server can then use a namespace such as:

```text
io.github.YOUR_GITHUB_USERNAME/calculator-mcp
```

---

# 7. Validate

Run:

```powershell
mcp-publisher validate
```

Fix any schema or metadata errors before publishing.

---

# 8. Publish

Run:

```powershell
mcp-publisher publish
```

The conceptual flow is:

```text
server.json
     |
     v
mcp-publisher
     |
     v
Authentication
     |
     v
Validation
     |
     v
Ownership verification
     |
     v
MCP Registry
```

Your MCP is now registered in the broader MCP ecosystem.

---

# 9. Publish to the Claude Directory

This is a **separate process** from the MCP Registry.

You do not need the Claude Directory in order to use your own MCP through a custom connector.

The directory is useful when you want other Claude users to discover your MCP without manually entering its URL.

For a simple MCP, submit the single remote MCP connector.

Conceptually:

```text
Claude Directory submission
             |
             v
https://calculator-mcp.example.com/mcp
```

The directory/review process can include automated validation and safety checks.

After approval:

```text
Claude
   |
   v
Discover
   |
   v
Calculator MCP
   |
   v
Connect
```

---

# 10. ChatGPT Is a Separate Distribution Channel

Do **not** assume that publishing to the MCP Registry or Claude Directory automatically makes your MCP appear in ChatGPT.

ChatGPT has its own app/plugin distribution and review process.

The architecture is:

```text
Your MCP
   |
   v
OpenAI App / Plugin
   |
   v
OpenAI review
   |
   v
ChatGPT / Codex directory
```

---

# 11. Test Your MCP in ChatGPT First

Before public submission, test the remote MCP using ChatGPT's supported developer/MCP workflow.

Provide your public endpoint:

```text
https://calculator-mcp.example.com/mcp
```

Conceptually:

```text
ChatGPT
   |
   v
Developer MCP connection
   |
   v
https://calculator-mcp.example.com/mcp
   |
   v
Tool discovery
   |
   +-- add
   +-- multiply
```

The purpose of this step is to verify your MCP before going through public review.

---

# 12. Prepare the OpenAI Submission

For public distribution, prepare the information required by the current OpenAI app/plugin submission process.

Typical information includes:

```text
App/plugin name
Logo
Description
Company/developer information
Privacy policy URL
MCP server URL
Tool descriptions
Test prompts
Expected behavior
Security information
```

OpenAI can inspect the MCP endpoint and its tool metadata during the review process.

Your server must be publicly reachable over HTTPS for public distribution.

---

# 13. Organization / Identity Verification

For public publication, OpenAI may require verification of the identity or organization publishing the app.

You may publish under:

```text
Individual developer
```

or:

```text
Business / organization
```

For a business, complete the required organization verification.

---

# 14. Submit for OpenAI Review

The conceptual process is:

```text
OpenAI Platform
      |
      v
Create app/plugin
      |
      v
Add remote MCP
      |
      v
Scan/discover tools
      |
      v
Complete metadata
      |
      v
Add testing information
      |
      v
Submit
      |
      v
OpenAI review
```

If approved, the app can be distributed through OpenAI's supported app directory/discovery system.

---

# 15. Final Architecture

After all three distribution channels are configured:

```text
                       +----------------------+
                       |    Your C# MCP       |
                       |    ASP.NET Core      |
                       +----------+-----------+
                                  |
                                  v
                             Docker
                                  |
                                  v
                           AWS App Runner
                                  |
                                  v
                 https://yourdomain.com/mcp
                                  |
             +--------------------+--------------------+
             |                    |                    |
             v                    v                    v
      MCP Registry         Claude Directory      OpenAI Directory
             |                    |                    |
             v                    v                    v
        MCP clients             Claude            ChatGPT/Codex
```

---

# 16. What Is Actually Required?

| Component | Purpose | Required? |
|---|---|---:|
| C# MCP server | Provides the tools | Yes |
| AWS hosting | Public remote endpoint | Yes for remote public distribution |
| HTTPS/domain | Secure public endpoint | Yes for public distribution |
| `server.json` | MCP Registry metadata | Only for MCP Registry |
| MCP Registry | General MCP discovery | Optional |
| Claude custom connector | Personal Claude testing | Optional |
| Claude Directory | Public Claude discovery | Optional |
| ChatGPT developer/MCP testing | Personal ChatGPT testing | Optional |
| OpenAI public submission | Public ChatGPT/Codex discovery | Optional |

---

# 17. The Key Principle

You **do not build three MCP servers**.

Build:

```text
ONE C# MCP
```

Deploy:

```text
ONE AWS endpoint
```

Then register that same endpoint with the ecosystems you want:

```text
                       ONE MCP
                          |
                          v
              https://yourdomain.com/mcp
                          |
          +---------------+---------------+
          |               |               |
          v               v               v
    MCP Registry       Claude          OpenAI
                      Directory       Directory
```

AWS remains the source of truth for the running MCP.

The directories are discovery/distribution layers.

---

# 18. Recommended Project Layout

For a production-ready version:

```text
CalculatorMcp/
│
├── CalculatorMcp.csproj
├── Program.cs
│
├── Tools/
│   └── CalculatorTools.cs
│
├── Auth/
│   └── Authentication.cs
│
├── Dockerfile
│
├── server.json
│
├── README.md
│
└── docs/
    └── deployment.md
```

GitHub contains:

```text
source code
README
server.json
documentation
```

AWS contains:

```text
running MCP server
```

The registries/directories contain:

```text
metadata
discovery information
links to your MCP
```

---

# 19. Recommended Order

Do not try to publish everything simultaneously.

Use this order:

```text
1. Build C# MCP
        ↓
2. Run locally
        ↓
3. Test MCP tools
        ↓
4. Dockerize
        ↓
5. Push image to ECR
        ↓
6. Deploy to AWS
        ↓
7. Configure HTTPS/domain
        ↓
8. Test remote MCP
        ↓
9. Connect to Claude manually
        ↓
10. Connect to ChatGPT manually
        ↓
11. Create server.json
        ↓
12. Publish to MCP Registry
        ↓
13. Submit to Claude Directory
        ↓
14. Submit to OpenAI directory
```

This order minimizes debugging because you establish that the **actual MCP works before adding directory/distribution complexity**.

---

# 20. Important Security Considerations

Do not expose sensitive tools anonymously.

For production MCP servers consider:

```text
HTTPS
Authentication
Authorization
Rate limiting
Input validation
Audit logging
Tool-level permissions
Secrets stored in AWS Secrets Manager
CloudWatch logging
```

For example:

```text
Claude
   |
   | HTTPS + authentication
   v
AWS
   |
   v
C# MCP
   |
   v
Authorized tool
```

Do not put AWS credentials, database passwords, API keys, or other secrets in `server.json`, GitHub, or the Docker image.

---

# Useful Official Documentation

- MCP Registry: https://registry.modelcontextprotocol.io/
- MCP Registry documentation: https://github.com/modelcontextprotocol/registry
- Anthropic custom remote MCP connectors: https://support.claude.com/en/articles/11175166-get-started-with-custom-connectors-using-remote-mcp
- Anthropic directory/plugin information: https://claude.com/blog/build-plugins-for-claude
- OpenAI MCP/connectors documentation: https://developers.openai.com/api/docs/guides/tools-connectors-mcp
- OpenAI plugin/app quickstart: https://developers.openai.com/plugins/build/app-quickstart
- OpenAI app/plugin review: https://developers.openai.com/plugins/deploy/app-review

---

## Bottom line

The architecture is:

```text
                 BUILD ONCE
                    C# MCP
                       |
                       v
                 DEPLOY ONCE
                     AWS
                       |
                       v
             https://.../mcp
                       |
          +------------+------------+
          |            |            |
          v            v            v
        MCP          Claude       OpenAI
      Registry      Directory    Directory
```

You **do not need to publish to every directory**. You only publish to the directories where you want users to discover your MCP.
