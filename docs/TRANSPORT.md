# Streamable HTTP Transport Configuration

## Overview

The FSI MCP Server uses **Streamable HTTP transport** for remote HTTP access. This enables multiple concurrent sessions, server-to-client notifications, and is ideal for web applications, remote clients, or distributed systems.

**Important**: This uses **Streamable HTTP** (the modern transport), **NOT** the deprecated HTTP+SSE transport.

## Transport Type

✅ **Streamable HTTP** - Modern, recommended transport for HTTP-based MCP servers

## When to Use

- ✅ Remote HTTP access
- ✅ Multiple concurrent clients
- ✅ Web applications
- ✅ Server-to-client notifications
- ✅ Distributed systems
- ✅ AI assistant integrations (Claude Code, GitHub Copilot, etc.)

## Configuration

### Server Endpoint

The server listens on:
- **Default**: `http://0.0.0.0:5020`
- **MCP Endpoint**: `http://HOST:5020/mcp`
- **Health Check**: `http://HOST:5020/health`

### Code Configuration

The transport is configured in `server/Program.fs`:

```fsharp
builder
    .Services
    .AddMcpServer()
    .WithHttpTransport(fun httpOptions ->
        // Configure Streamable HTTP transport (not deprecated SSE)
        httpOptions.Stateless <- false  // Enable stateful sessions
        httpOptions.IdleTimeout <- TimeSpan.FromMinutes(30)
    )
    .WithTools<FsiMcpTools.FsiTools>()
```

### Key Configuration Options

- **Stateless**: Set to `false` to enable stateful sessions (required for Streamable HTTP)
- **IdleTimeout**: Session timeout duration (default: 30 minutes)
- **OnSessionStart**: Optional callback when a session starts
- **OnSessionEnd**: Optional callback when a session ends

## Client Connection

### Python Client Example

```python
from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client

async def connect():
    async with streamable_http_client("http://localhost:5020/mcp") as (read, write, _):
        async with ClientSession(read, write) as session:
            await session.initialize()
            
            # List available tools
            tools = await session.list_tools()
            print(f"Tools: {[t.name for t in tools.tools]}")
            
            # Call a tool
            result = await session.call_tool("SendFSharpCode", {
                "code": "printfn \"Hello from MCP!\""
            })
            print(result)
```

### C# Client Example

```csharp
using ModelContextProtocol.Client;

var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri("http://localhost:5020/mcp"),
    TransportMode = HttpTransportMode.AutoDetect,
    ConnectionTimeout = TimeSpan.FromSeconds(30),
    RequestTimeout = TimeSpan.FromMinutes(5)
});

var client = await McpClient.CreateAsync(transport);

// List available tools
foreach (var tool in await client.ListToolsAsync())
{
    Console.WriteLine($"{tool.Name} ({tool.Description})");
}

// Call a tool
var result = await client.CallToolAsync(
    "SendFSharpCode",
    new Dictionary<string, object?> { ["code"] = "printfn \"Hello from MCP!\"" },
    cancellationToken: CancellationToken.None);

await client.DisposeAsync();
```

### TypeScript/JavaScript Client Example

```typescript
import { ClientSession } from "@modelcontextprotocol/sdk/client/index.js";
import { StreamableHTTPClientTransport } from "@modelcontextprotocol/sdk/client/streamableHttp.js";

async function connect() {
  const transport = new StreamableHTTPClientTransport(
    new URL("http://localhost:5020/mcp")
  );
  
  const session = new ClientSession({
    name: "FSI Client",
    version: "1.0.0"
  }, {
    capabilities: {}
  });
  
  await session.connect(transport);
  
  // List available tools
  const tools = await session.listTools();
  console.log("Tools:", tools.tools.map(t => t.name));
  
  // Call a tool
  const result = await session.callTool("SendFSharpCode", {
    code: "printfn \"Hello from MCP!\""
  });
  console.log(result);
  
  await session.close();
}
```

## Available Tools

The FSI MCP Server exposes the following tools:

1. **SendFSharpCode**: Execute F# code in the FSI session
2. **LoadFSharpScript**: Load and execute .fsx files
3. **GetRecentFsiEvents**: Access FSI event history
4. **GetFsiStatus**: Get session information

## Session Management

### Session Lifecycle

1. **Initialization**: Client sends `initialize` request to `/mcp` endpoint
2. **Session ID**: Server generates a unique session ID
3. **Stateful Communication**: All subsequent requests use the session ID
4. **Cleanup**: Session is cleaned up on timeout or explicit close

### Session Headers

When making requests, include the session ID header:

```
mcp-session-id: <session-id>
```

## Claude Code Integration

### Configure Claude Code

Claude Code supports HTTP transport. To connect to the FSI MCP Server:

```bash
# Add the FSI MCP server to Claude Code
# Note: Use --transport http (Claude Code uses "http" for Streamable HTTP)
claude mcp add --transport http fsi-mcp-server http://localhost:5020/mcp
```

**Important**: 
- Make sure the FSI MCP Server is running before adding it to Claude Code
- Claude Code's `http` transport type is compatible with Streamable HTTP servers
- The server must be accessible at `http://localhost:5020/mcp`

### Verify Configuration

```bash
# List configured MCP servers
claude mcp list

# You should see:
# fsi-mcp-server: http://localhost:5020/mcp (HTTP) - ✓ Connected
```

### Using in Claude Code

Once configured, you can use the MCP tools in Claude Code:

1. Start the FSI MCP Server:
   ```bash
   cd /path/to/fsi-mcp-server
   dotnet run --project server
   ```

2. In Claude Code, use `/mcp` to see available tools:
   - `SendFSharpCode` - Execute F# code
   - `LoadFSharpScript` - Load .fsx files
   - `GetRecentFsiEvents` - Get event history
   - `GetFsiStatus` - Get session status

## Testing

### Using MCP Inspector

```bash
# Start the FSI MCP Server
dotnet run --project server

# In another terminal, use MCP Inspector
npx @modelcontextprotocol/inspector
```

### Manual Testing with cURL

```bash
# Initialize session
curl -X POST http://localhost:5020/mcp \
  -H "Content-Type: application/json" \
  -H "ProtocolVersion: 2025-06-18" \
  -H "Accept: application/json, text/event-stream" \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "initialize",
    "params": {
      "protocolVersion": "2025-06-18",
      "capabilities": {},
      "clientInfo": {
        "name": "test-client",
        "version": "1.0.0"
      }
    }
  }'

# List tools (use session ID from initialize response)
curl -X POST http://localhost:5020/mcp \
  -H "Content-Type: application/json" \
  -H "mcp-session-id: <session-id>" \
  -d '{
    "jsonrpc": "2.0",
    "id": 2,
    "method": "tools/list"
  }'
```

## Troubleshooting

### Connection Issues

- **Check server is running**: Verify `http://localhost:5020/health` returns "Ready to work!"
- **Check endpoint**: Ensure you're using `/mcp` endpoint, not root
- **Check session ID**: Ensure session ID header is included in subsequent requests

### Transport Errors

- **"Invalid session"**: Session may have timed out or been closed
- **"Transport not supported"**: Ensure client is using Streamable HTTP transport
- **Connection timeout**: Check firewall settings and network connectivity

## SDK Version

- **Current**: ModelContextProtocol v0.5.0-preview.1
- **Package**: `ModelContextProtocol.AspNetCore`

## References

- [MCP Specification - Streamable HTTP](https://modelcontextprotocol.io/specification/2025-03-26/basic/transports#streamable-http)
- [ModelContextProtocol NuGet Package](https://www.nuget.org/packages/ModelContextProtocol)
- [C# SDK Documentation](https://github.com/modelcontextprotocol/csharp-sdk)

