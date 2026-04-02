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

```mermaid
flowchart TD
  subgraph App [CSharpCraft (Executable)]
    MC["Main.cs\n(FNAGame entry)"]
  end

  subgraph Lib [PSharp8 (Emulator Library)]
    GO["GameOrchestrator.cs\n(creates & wires managers)"]
    Pico["Pico8.cs\n(static facade, AsyncLocal<GameOrchestrator>)"]
    GM["GraphicsManager"]
    AM["AudioManager"]
    IM["InputManager"]
    MM["MemoryManager"]
    SM["SceneManager"]
  end

  subgraph Tests [Test Projects]
    PT[PSharp8.Tests]
    CT[CSharpCraft.Tests]
  end

  subgraph Native [Native libs]
    NL["FNAlibs: libFNA3D, libSDL3, libFAudio"]
  end

  MC -->|initializes| GO
  MC -->|calls static API| Pico
  GO --> GM
  GO --> AM
  GO --> IM
  GO --> MM
  GO --> SM
  Pico -->|accesses| GO
  PT -->|InternalsVisibleTo| PSharp8/GlobalUsings.cs
  MC --> NL
  GO --> NL
```

### CSharpCraft (Entry Point: [Main.cs](../../CSharpCraft/CSharpCraft/Main.cs))

- Subclasses `FNAGame` (FNA game loop)
- Configures FNA backend environment before startup
- Depends on **PSharp8** for emulation and scene management

### PSharp8 (Emulator Library)

**Manager Orchestration Pattern** via [GameOrchestrator.cs](../../PSharp8/PSharp8/GameOrchestrator.cs):

- Centralizes dependency injection and manager lifecycle
- Subsystems (all stateful managers):
  - **Graphics**: `GraphicsManager`, `PaletteManager`, `SpriteTextureManager` (LRU cache), `SpriteMapData`, `Fonts`
  - **Audio**: `AudioManager`, `Soundtrack`, `SfxPack`
  - **Input**: `InputManager`, `InputBindings`, `InputEvent`
  - **Memory**: `MemoryManager`
  - **Scene**: `SceneManager`
  - **PMath**: `MathManager`, `SinDict`, `CosDict` (fixed-point math utilities)

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
| [PSharp8/GlobalUsings.cs](../../PSharp8/PSharp8/GlobalUsings.cs) | Global usings + `InternalsVisibleTo` | Shared imports; exposes internals to test project |
| [PSharp8/Graphics/LruCache.cs](../../PSharp8/PSharp8/Graphics/LruCache.cs) | Generic LRU eviction | Efficient sprite texture caching |
| [PSharp8/Audio/AudioManager.cs](../../PSharp8/PSharp8/Audio/AudioManager.cs) | Stateful music manager | Music playback, crossfade, fade in/out |
| [PSharp8/Audio/Soundtrack.cs](../../PSharp8/PSharp8/Audio/Soundtrack.cs) | Immutable data model | Soundtrack → Track → TrackPart hierarchy |
| [PSharp8.Tests/Infrastructure/FnaFixture.cs](../../PSharp8/PSharp8.Tests/Infrastructure/FnaFixture.cs) | FNA game loop fixture | Graphics & audio initialization for tests |
| [PSharp8.Tests/Infrastructure/GraphicsTestBase.cs](../../PSharp8/PSharp8.Tests/Infrastructure/GraphicsTestBase.cs) | Base class for GPU tests | Texture lifecycle, color constants, helpers |
| [PSharp8.Tests/Infrastructure/FnaCollection.cs](../../PSharp8/PSharp8.Tests/Infrastructure/FnaCollection.cs) | xUnit collection definition | Shared FNA fixture across test classes |
| [CSharpCraft.Tests/Infrastructure/FnaFixture.cs](../../CSharpCraft/CSharpCraft.Tests/Infrastructure/FnaFixture.cs) | FNA game loop fixture | Backend configuration, graphics device setup |
| [CSharpCraft.Tests/Infrastructure/FnaCollection.cs](../../CSharpCraft/CSharpCraft.Tests/Infrastructure/FnaCollection.cs) | xUnit collection definition | Shared FNA fixture for game tests |
| [PSharp8.Tests/Graphics/LruCacheTests.cs](../../PSharp8/PSharp8.Tests/Graphics/LruCacheTests.cs) | xUnit + FluentAssertions | Standard test structure |}

---

## Code Conventions

### Access Modifiers

**Default to `internal`. Only make types or members `public` when they genuinely cross the PSharp8 → CSharpCraft boundary.**

Concretely:
- Manager classes (`InputManager`, `GraphicsManager`, etc.) and their interfaces (`IInputManager`, etc.) → **`internal`**. They are never accessed directly by CSharpCraft.
- Public access to manager functionality is provided **exclusively** through one of two surfaces:
  - [`Pico8.cs`](../../PSharp8/PSharp8/Pico8.cs) — static facade for game-logic code
  - [`GameOrchestrator.cs`](../../PSharp8/PSharp8/GameOrchestrator.cs) — for wiring at startup and per-frame update calls (e.g. `UpdateInput`)
- Data/config types passed in from CSharpCraft (`InputBindings`, `BtnpConfig`, `InputEvent`, `InputSource` subtypes, `PicoButton`, `MouseButton`) → **`public`** because CSharpCraft must construct or reference them.
- xUnit `[InlineData]` forces enum types used in theory parameters to be `public` — this is an acceptable exception.

```csharp
// ✅ internal — stays inside PSharp8
internal interface IInputManager { ... }
internal class InputManager : IInputManager { ... }

// ✅ public — crosses to CSharpCraft
public record InputBindings(...) { ... }
public record BtnpConfig(...) { ... }

// ✅ public access via orchestrator or static facade — not via manager directly
orchestrator.UpdateInput(elapsed, events);  // GameOrchestrator pass-through
Pico8.Btn((int)PicoButton.Left);            // Pico8 static facade
```

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
- **FluentAssertions** for readable assertions (`result.Should().Be(...)`)
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

- [GlobalUsings.cs](../../PSharp8/PSharp8/GlobalUsings.cs) eliminates boilerplate `using` statements and declares `[assembly: InternalsVisibleTo("PSharp8.Tests")]` so tests can access `internal` fields directly (no reflection)
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

If needed, [FnaFixture.cs](../../PSharp8/PSharp8.Tests/Infrastructure/FnaFixture.cs) applies additional SDL hints in-process.

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

This is a **multi-root workspace** using `CSharpCraft.code-workspace` to combine two sibling Git repositories:

```
CSharpCraft/                          # Workspace folder 1: game executable
├── .github/
│   └── copilot-instructions.md      # This file
├── .runsettings                      # Test backend configuration (CRITICAL)
├── CSharpCraft/                      # Main game executable
│   ├── Main.cs                       # Game entry point (FNAGame)
│   ├── Content/                      # sprites, sounds, music
│   └── CSharpCraft.csproj
├── CSharpCraft.Tests/                # Game tests
│   └── CSharpCraft.Tests.csproj
├── CSharpCraft.Deprecated/           # Legacy code (ignore)
├── RaceServer.Deprecated/            # Legacy server code (ignore)
├── FNAlibs/                          # SDL3/FNA3D native libs
│   ├── libFAudio.so.0
│   ├── libFNA3D.so.0
│   └── libSDL3.so.0
└── CSharpCraft.slnx                  # Solution file

PSharp8/                              # Workspace folder 2: emulator library
├── .runsettings                      # Test backend configuration (CRITICAL)
├── PSharp8/                          # Emulator library (reusable)
│   ├── GameOrchestrator.cs           # Manager factory + DI
│   ├── Pico8.cs                      # Static API facade
│   ├── GlobalUsings.cs               # Shared using directives
│   ├── Audio/
│   │   ├── AudioManager.cs           # Audio playback
│   │   └── Soundtrack.cs
│   ├── Graphics/                     # Drawing subsystem
│   │   ├── Fonts.cs
│   │   ├── GraphicsManager.cs
│   │   ├── LruCache.cs               # Generic LRU eviction cache
│   │   ├── PaletteManager.cs
│   │   ├── PaletteSnapshot.cs
│   │   ├── SpriteMapData.cs
│   │   ├── SpriteSnapshot.cs
│   │   └── SpriteTextureManager.cs   # LRU-cached sprite textures
│   ├── Input/
│   │   └── InputManager.cs
│   ├── Memory/
│   │   └── MemoryManager.cs
│   ├── PMath/                        # Fixed-point math utilities
│   │   ├── CosDict.cs
│   │   ├── MathManager.cs
│   │   └── SinDict.cs
│   ├── Scene/
│   │   ├── IScene.cs
│   │   └── SceneManager.cs
│   └── PSharp8.csproj
├── PSharp8.Tests/                    # Emulator tests
│   ├── Infrastructure/
│   │   ├── FnaCollection.cs          # xUnit [CollectionDefinition("Fna")]
│   │   ├── FnaFixture.cs             # FNA test fixture (graphics + audio)
│   │   └── GraphicsTestBase.cs       # Base class for graphics tests
│   ├── Audio/
│   │   └── AudioManagerTests.cs      # Pure logic + FNA audio tests
│   ├── Graphics/
│   │   ├── GraphicsManagerTests.cs
│   │   ├── LruCacheTests.cs
│   │   ├── PaletteManagerTests.cs
│   │   ├── SpriteMapDataTests.cs
│   │   ├── SpriteSnapshotTests.cs
│   │   └── SpriteTextureManagerTests.cs
│   └── PSharp8.Tests.csproj
├── FNAlibs/                          # SDL3/FNA3D native libs
│   ├── libFAudio.so.0
│   ├── libFNA3D.so.0
│   └── libSDL3.so.0
└── PSharp8.slnx                      # Solution file
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

## For AI Agents

**Tests:** Always via `dotnet test` CLI or VS Code shell task — **never** the Testing view. Write tests first (Red-Green-Refactor). See [TDD-WORKFLOW.md](TDD-WORKFLOW.md) for patterns and fixtures.

**Code rules:**
- New managers: constructor null guards (`?? throw new ArgumentNullException(...)`)
- New tests: `[Fact]` + FluentAssertions, named `Method_Context_Expected`
- Zero warnings (`TreatWarningsAsErrors: true`); nullable-aware throughout

**Documentation:** Don't update docs reflexively on every change — that wastes tokens. Instead, **flag** when something is likely stale (e.g. "Key Files table may need updating") and let the user trigger doc updates as a deliberate task. Only edit docs when explicitly asked or when information is actively wrong.

**Reference guides (load on demand):**
- [TDD-WORKFLOW.md](TDD-WORKFLOW.md) — Red-Green-Refactor, test patterns, fixtures, online references
- [DOCUMENTATION-MAINTENANCE.md](DOCUMENTATION-MAINTENANCE.md) — Priority system, when/how to update docs

### Proactive Design Review Skill

- **Purpose:** Be proactive and vigilant in rooting out convoluted, fragile, or poorly designed architecture; propose safe, incremental refactors and migration plans.
- **When to use:** During code reviews, when reading legacy or high-complexity modules, or when proposed changes increase long-term maintenance cost.
- **Agent behavior:** Flag specific design/quality smells, present minimal step-by-step refactor plans with required tests, estimate impact and effort, and prefer non-breaking incremental changes. If a breaking change is necessary, require explicit approval and provide a migration path.
- **Guardrails:** Do not preserve bad design for historical convenience; avoid large one-shot refactors without tests and approval; do not change unrelated code in the same PR.
- **Skill file:** [.github/skills/proactive-design/SKILL.md](skills/proactive-design/SKILL.md)

---

*Last updated: 2 April 2026*
