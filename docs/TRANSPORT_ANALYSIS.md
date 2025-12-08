# FSI MCP Server Transport Analysis

## Current State

### Transport Identification

**Current Implementation:**
- Uses `ModelContextProtocol.AspNetCore` package
- Calls `.WithHttpTransport()` in `Program.fs` line 33
- Uses `app.MapMcp()` to map MCP endpoints
- Documentation states: "MCP transport: HTTP SSE" (line 92 in CLAUDE.md)

**SDK Version:**
- Currently using: `ModelContextProtocol` version `0.3.0-preview.3` (OLD)
- Latest available: `0.5.0-preview.1` (as of Dec 2025)

### Transport Type: **LIKELY DEPRECATED SSE**

Based on the evidence:
1. ✅ Uses HTTP transport (not stdio)
2. ⚠️ Documentation explicitly says "HTTP SSE" (deprecated)
3. ⚠️ Using old SDK version (0.3.0-preview.3)
4. ⚠️ `.WithHttpTransport()` in older versions likely defaults to SSE

## What Needs to Change

### 1. Update SDK Version

**Current:**
```xml
<PackageReference Include="ModelContextProtocol" Version="0.3.0-preview.3" />
<PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.3.0-preview.3" />
```

**Should be:**
```xml
<PackageReference Include="ModelContextProtocol" Version="0.5.0-preview.1" />
<PackageReference Include="ModelContextProtocol.AspNetCore" Version="0.5.0-preview.1" />
```

### 2. Verify Transport Configuration

The newer SDK versions should support Streamable HTTP. Need to verify:
- Does `.WithHttpTransport()` in 0.5.0-preview.1 use Streamable HTTP by default?
- Or does it need explicit configuration?

### 3. Update Documentation

**Current (CLAUDE.md line 92):**
```
- MCP transport: HTTP SSE
```

**Should be:**
```
- MCP transport: Streamable HTTP (NOT deprecated SSE)
```

## Comparison with Bun Server

| Aspect | Bun Server (TypeScript) | F# Server (Current) | F# Server (Target) |
|--------|------------------------|---------------------|-------------------|
| **Transport** | Streamable HTTP ✅ | HTTP SSE ⚠️ | Streamable HTTP ✅ |
| **SDK Version** | Latest (1.24.0) | 0.3.0-preview.3 (old) | 0.5.0-preview.1 (latest) |
| **Status** | Modern ✅ | Deprecated ⚠️ | Modern ✅ |

## Next Steps

1. **Update NuGet packages** to latest version (0.5.0-preview.1)
2. **Verify transport** - check if `.WithHttpTransport()` uses Streamable HTTP in new version
3. **Update documentation** to reflect Streamable HTTP
4. **Test compatibility** with modern MCP clients

## References

- [ModelContextProtocol NuGet Package](https://www.nuget.org/packages/ModelContextProtocol)
- [MCP Specification - Transports](https://modelcontextprotocol.io/specification/2025-03-26/basic/transports)
- [TypeScript SDK Server Docs](https://raw.githubusercontent.com/modelcontextprotocol/typescript-sdk/refs/heads/main/docs/server.md)

