# CSharpCraft & PSharp8 Context

Multi-root workspace: **CSharpCraft** (game executable on FNA) + **PSharp8** (reusable emulator library on .NET 10.0, C# 13).

## Essential Commands

Use **CLI** (not VS Code Testing view):
```bash
dotnet build CSharpCraft.slnx -c Debug
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```
`.runsettings` pre-configures FNA backends before tests start (required on Linux/Wayland).

## Code Rules

- **Access**: Default `internal`. Only `public` for PSharp8 ↔ CSharpCraft boundary.
- **Nullability**: `#nullable enable` everywhere. Constructor guards: `param ?? throw new ArgumentNullException(nameof(param))`
- **Tests**: xUnit `[Fact]`, FluentAssertions, Moq. Pattern: `Subject_Expected_Context()`
- **Namespaces**: `CSharpCraft.*`, `PSharp8.*`, tests in `.Tests` projects

## Architecture Overview

PSharp8 uses manager-based architecture:
- **GameOrchestrator** creates and wires managers (Graphics, Audio, Input, Memory, Scene)
- **Pico8.cs** static facade provides thread-safe access via `AsyncLocal<GameOrchestrator>`
- CSharpCraft entry: **Main.cs** configures FNA backend, initializes orchestrator

See [GameOrchestrator.cs](../../PSharp8/PSharp8/GameOrchestrator.cs), [Pico8.cs](../../PSharp8/PSharp8/Pico8.cs) for implementation details.

## FNA Backend Configuration (Linux/Wayland Critical)

`.runsettings` file pre-configures:
```xml
<EnvironmentVariables>
  <FNA_PLATFORM_BACKEND>SDL3</FNA_PLATFORM_BACKEND>
  <FNA3D_FORCE_DRIVER>SDLGPU</FNA3D_FORCE_DRIVER>
  <SDL_GPU_DRIVER>vulkan</SDL_GPU_DRIVER>
</EnvironmentVariables>
```

- **Applied by**: `dotnet test` CLI and VS Code shell task
- **NOT by**: VS Code Testing view (C# Dev Kit limitation)

Troubleshooting: See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for FNA3D backend issues.

## References

- **Testing patterns**: [TDD-WORKFLOW.md](TDD-WORKFLOW.md)
- **Doc maintenance**: [DOCUMENTATION-MAINTENANCE.md](DOCUMENTATION-MAINTENANCE.md)
- **Design reviews**: [.github/skills/proactive-design/SKILL.md](skills/proactive-design/SKILL.md)

---

*Last updated: 3 April 2026*
