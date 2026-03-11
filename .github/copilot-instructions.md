# CSharpCraft & PSharp8 Workspace Instructions

## Overview

**CSharpCraft** is a cross-platform recreation and extension of a Pico8 game, built on **FNA** (MonoGame-compatible graphics/audio/input) and **PSharp8** (a Pico-8 emulator library). The workspace hosts both the game executable and the emulator core as a reusable library.

- **CSharpCraft** (executable): Game built on FNA + PSharp8
- **PSharp8** (library): Pico-8 emulation layer with manager-based architecture
- **CSharpCraft.Tests / PSharp8.Tests**: xUnit test suites

### Tech Stack

- **.NET 10.0** (modern C# 13 with `Nullable: enable`, `TreatWarningsAsErrors: true`, `ImplicitUsings: enable`)
- **FNA** + **SDL3/Vulkan** backend (Linux-optimized)
- **xUnit 2.9.1** + **Moq** + **FluentAssertions** (testing)
- **Microsoft.Extensions.DependencyInjection** (IoC)
- **gRPC** (async networking in CSharpCraft)

---

## Build & Test Commands

### Recommended (CLI — Always Works)

```bash
# Build all projects
dotnet build CSharpCraft.slnx -c Debug

# Run tests (REQUIRED way on Linux/Wayland)
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```

**Why CLI for tests?** The `.runsettings` file pre-configures FNA backend selection (`SDL3`, `SDLGPU`, `vulkan`) before the test host starts. This is critical on Linux/Wayland.

### VS Code Tasks

- **test: CSharpCraft.Tests** (shell task) — Use this **instead of** the Testing view if running from VS Code
- **dotnet: build (CSharpCraft)** — Build task already in workspace

### ⚠️ DO NOT USE

- **VS Code Testing view** (C# Dev Kit) — Does not reliably apply `.runsettings` before FNA3D startup; may fail on Wayland with EGL/OpenGL path
- **Ad-hoc `dotnet run`** without backend configuration — Will fail on Wayland

---

## Architecture & Component Boundaries

### CSharpCraft (Entry Point: [Main.cs](../../CSharpCraft/CSharpCraft/Main.cs))

- Subclasses `FNAGame` (FNA game loop)
- Configures FNA backend environment before startup
- Depends on **PSharp8** for emulation and scene management

### PSharp8 (Emulator Library)

**Manager Orchestration Pattern** via [GameOrchestrator.cs](../../PSharp8/PSharp8/GameOrchestrator.cs):

- Centralizes dependency injection and manager lifecycle
- Subsystems (all stateful managers):
  - **Graphics**: `GraphicsManager`, `PaletteManager`, `SpriteTextureManager` (LRU cache), `SpriteMapData`
  - **Audio**: `AudioManager` (stub)
  - **Input**: `InputManager` (stub)
  - **Memory**: `MemoryManager` (Pico-8 emulation)
  - **Scene**: `SceneManager`

**Static API Wrapper** ([Pico8.cs](../../PSharp8/PSharp8/Pico8.cs)):

- `AsyncLocal<GameOrchestrator>` for thread-safe global state access
- Exposes static `Pico8.*` methods calling the orchestrator
- Thread-safety: Each async context gets its own orchestrator instance

### Key Files & Patterns

| File | Pattern | Purpose |
|------|---------|---------|
| [CSharpCraft/Main.cs](../../CSharpCraft/CSharpCraft/Main.cs) | FNA game loop + env var setup | Game entry; pre-configures backend |
| [PSharp8/GameOrchestrator.cs](../../PSharp8/PSharp8/GameOrchestrator.cs) | Dependency-injected manager factory | Composes all subsystems; IoC container |
| [PSharp8/Pico8.cs](../../PSharp8/PSharp8/Pico8.cs) | Static facade + `AsyncLocal<T>` | Thread-safe global orchestrator access |
| [PSharp8/Graphics/LruCache.cs](../../PSharp8/PSharp8/Graphics/LruCache.cs) | Generic LRU eviction | Efficient sprite texture caching |
| [PSharp8.Tests/LruCacheTests.cs](../../PSharp8/PSharp8.Tests/LruCacheTests.cs) | xUnit + FluentAssertions | Standard test structure |

---

## Code Conventions

### Nullability & Constructor Guards

```csharp
public class MyManager
{
    private readonly IDependency _dep;

    public MyManager(IDependency dep)
    {
        _dep = dep ?? throw new ArgumentNullException(nameof(dep));
    }
}
```

- Always guard constructor parameters with `?? throw new ArgumentNullException(...)`
- All public APIs are nullable-aware (`#nullable enable`)

### Testing

- **xUnit `[Fact]`** for unit tests
- **FluentAssertions** for readable assertions (`result.Should().Be(...)`
- **Moq** for mocking dependencies

Standard test structure:
```csharp
public class MyComponentTests
{
    [Fact]
    public void MethodName_GivenContext_ExpectedBehavior()
    {
        // Arrange
        var mock = new Mock<IDependency>();
        var sut = new MyComponent(mock.Object);

        // Act
        var result = sut.Method();

        // Assert
        result.Should().BeTrue();
    }
}
```

### Namespacing & Global Usings

- [GlobalUsings.cs](../../PSharp8/PSharp8/GlobalUsings.cs) eliminates boilerplate `using` statements
- Namespaces: `CSharpCraft`, `CSharpCraft.*`, `PSharp8.*`, `PSharp8.*.Tests`

---

## Critical Pitfalls: FNA3D Backend Selection (Linux/Wayland)

> **⚠️ This is the #1 issue when running tests on this workspace.**

### The Problem

FNA3D probes for available graphics backends at test host startup. On Linux/Wayland:

1. **Default probe order**: EGL/OpenGL → fails on Wayland
2. **Solution**: Pre-set FNA backend environment to `SDL3` + `Vulkan` **before test host launches**

### The Solution: `.runsettings`

Repository `.runsettings` file pre-configures the test environment:

```xml
<RunSettings>
  <RunConfiguration>
    <EnvironmentVariables>
      <FNA_PLATFORM_BACKEND>SDL3</FNA_PLATFORM_BACKEND>
      <FNA3D_FORCE_DRIVER>SDLGPU</FNA3D_FORCE_DRIVER>
      <SDL_GPU_DRIVER>vulkan</SDL_GPU_DRIVER>
    </EnvironmentVariables>
  </RunConfiguration>
</RunSettings>
```

- **Applied by**: `dotnet test` CLI (guaranteed) and VS Code shell task (via project reference)
- **NOT applied by**: VS Code C# Dev Kit Testing view (limitation)

### Fallback: In-Process Configuration

If needed, [GraphicsFixture.cs](../../PSharp8/PSharp8.Tests/Infrastructure/GraphicsFixture.cs) applies additional SDL hints in-process.

### When This Breaks

| Symptom | Typical Cause | Action |
|---------|---------------|--------|
| Tests fail in Testing view but CLI works | `.runsettings` not applied by test runner | Use shell task or CLI |
| Tests fail in CLI (rare) | Native libs missing or environment corrupted | Verify `FNAlibs/*.so.0` present in repo root |
| FNA3D error message about EGL/OpenGL | Backend probe succeeded but native driver missing | Ensure Vulkan drivers installed on system |

### Debugging Backend Issues

```bash
# Check what backend FNA actually selected (only visible in failed test output)
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug --verbosity=diag | grep -i fna

# Manually trigger backend selection
export FNA_PLATFORM_BACKEND=SDL3 FNA3D_FORCE_DRIVER=SDLGPU SDL_GPU_DRIVER=vulkan
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```

---

## Common Tasks for AI Agents

### Adding a New Test

1. Create `MyFeatureTests.cs` in `PSharp8.Tests` or `CSharpCraft.Tests`
2. Use xUnit `[Fact]` + FluentAssertions:
   ```csharp
   [Fact]
   public void NewFeature_GivenContext_Expected()
   {
       var sut = new MyClass();
       sut.Method().Should().Equal(...);
   }
   ```
3. Run: `dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug`

### Adding a New Manager to PSharp8

1. Create `MyManager.cs` in [`PSharp8/PSharp8/MySubsystem/`](../../PSharp8/PSharp8/)
2. Guard constructor parameters with `?? throw new ArgumentNullException(...)`
3. Register in [GameOrchestrator.cs](../../PSharp8/PSharp8/GameOrchestrator.cs) `CreateOrchestrator()`
4. Optionally expose static API via [Pico8.cs](../../PSharp8/PSharp8/Pico8.cs) if public-facing

### Debugging a Test Failure

```bash
# 1. Run with verbose output
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug --verbosity=normal

# 2. If backend-related, check environment
echo $FNA_PLATFORM_BACKEND $FNA3D_FORCE_DRIVER $SDL_GPU_DRIVER

# 3. Check that native libs exist
ls -la CSharpCraft/FNAlibs/
```

---

## File Organization & Where Things Live

```
CSharpCraft/                          # Workspace root
├── .github/
│   └── copilot-instructions.md      # This file
├── .runsettings                      # Test backend configuration (CRITICAL)
├── CSharpCraft/                      # Main game executable
│   ├── Main.cs                       # Game entry point (FNAGame)
│   ├── Content/                      # sprites, sounds, music
│   └── CSharpCraft.csproj
├── PSharp8/                          # Emulator library (reusable)
│   ├── GameOrchestrator.cs           # Manager factory + DI
│   ├── Pico8.cs                      # Static API facade
│   ├── Graphics/                     # *Manager classes for drawing
│   ├── Audio/                        # *Manager classes for sound
│   ├── Input/                        # *Manager classes for input
│   ├── Memory/                       # Pico-8 emulation layer
│   ├── Scene/                        # Scene management
│   ├── GlobalUsings.cs               # Shared using directives
│   └── PSharp8.csproj
├── PSharp8.Tests/                    # Emulator tests
│   ├── Infrastructure/
│   │   └── GraphicsFixture.cs        # Graphics test setup
│   ├── LruCacheTests.cs
│   ├── PaletteManagerTests.cs
│   └── PSharp8.Tests.csproj
├── CSharpCraft.Tests/                # Game tests
│   └── CSharpCraft.Tests.csproj
├── CSharpCraft.Deprecated/           # Legacy code (ignore)
├── FNAlibs/                          # SDL3/FNA3D native libs
│   ├── libFAudio.so.0
│   ├── libFNA3D.so.0
│   └── libSDL3.so.0
└── CSharpCraft.slnx                  # Solution file
```

---

## Quick Reference: Common Errors & Fixes

| Error | Cause | Fix |
|-------|-------|-----|
| `FNA3D error: no suitable driver found` | Vulkan not available or backend not pre-selected | Ensure `FNA_PLATFORM_BACKEND=SDL3` before test starts; check Vulkan install |
| `Test times out in VS Code Testing view` | Backend probe hangs on EGL path | Use shell task or CLI instead |
| `Native library not found (SDL3, FNA3D, FAudio)` | `FNAlibs/*.so.0` missing from repo | Verify `git lfs` is installed and files pulled: `git lfs ls-files` |
| `Argument null exception in manager constructor` | Missing null guard | Add `?? throw new ArgumentNullException(...)` |
| `AsyncLocal orchestrator is null` | Static API called before orchestrator initialized | Ensure `GameOrchestrator.SetCurrent()` called in entry point |

---

## Next Steps for Agents

When working in this workspace:

1. **Always run tests via CLI** (`dotnet test`) or VS Code shell task, **never** via Testing view
2. **Check `.runsettings`** if backend issues arise
3. **Guard constructor parameters** in all new managers
4. **Use FluentAssertions** in all xUnit tests
5. **Reference [GameOrchestrator.cs](../../PSharp8/PSharp8/GameOrchestrator.cs)** when adding new subsystems
6. **Respect the strict C# settings** (`Nullable: enable`, `TreatWarningsAsErrors: true`)

---

*Last updated: 11 March 2026*
