# CSharpCraft Architecture

> Static PICO-8 API facade + orchestrator pattern.  
> All projects compile with 0 errors. 419 tests passing.

## Overview

The codebase uses a **static facade + orchestrator** pattern: game scenes call bare static methods
via `using static CSharpCraft.Pico8.Pico8` (e.g. `Cls(0)`, `Btnp(4)`, `Spr(1, 64, 64)`),
which delegate to a `GameOrchestrator` stored in `AsyncLocal<T>` for thread safety.
This eliminates constructor injection, simplifies scene code, and enables full test isolation
via per-test orchestrator instances.

## Project Structure

```
CSharpCraft.slnx
├── CSharpCraft.FixMath/            # Fixed-point math library (no dependencies)
│   ├── F32.cs, F64.cs              # 32/64-bit fixed point types
│   ├── Fixed32.cs, Fixed64.cs      # Alternative fixed arithmetic
│   └── FixedUtil.cs                # Utility functions
│
├── CSharpCraft.Pico8/              # Static API + orchestrator layer (43 files)
│   ├── Pico8.cs                    # Static PICO-8 API facade (THE entry point)
│   ├── GameOrchestrator.cs         # Top-level orchestrator (owns sub-orchestrators)
│   ├── GraphicsOrchestrator.cs     # Graphics state: camera, palette, display config
│   ├── AudioOrchestrator.cs        # Audio state: sfx, music, mute
│   ├── GraphicsAPI.cs              # Concrete FNA/XNA rendering (IGraphicsAPI)
│   ├── AudioAPI.cs                 # Concrete FNA/XNA audio (IAudioAPI)
│   ├── PaletteManager.cs           # Color remapping + transparency (IPaletteManager)
│   ├── SceneManager.cs             # Scene transitions + scheduling (ISceneManager)
│   ├── InputStateManager.cs        # Button state + lockout (IInputStateManager)
│   ├── MapManager.cs               # Map tile data (IMapManager)
│   ├── GameStateContainer.cs       # Consolidated game state (IGameState)
│   ├── ServiceFactory.cs           # Service creation (IServiceFactory)
│   ├── PauseMenuState.cs           # Pause state machine
│   ├── PauseMenuBuilder.cs         # Menu construction
│   ├── MusicManager.cs             # Audio playback state machine
│   ├── GameRendering.cs            # Rendering pipeline
│   ├── FnaTextureRenderer.cs       # Texture rendering (ITextureRenderer)
│   ├── OutputFacade.cs             # Graphics+Audio facade (IOutputFacade)
│   ├── Pico8Classes.cs             # Data classes (PalCol, MenuItem, P8Btns, etc.)
│   ├── Pico8Utils.cs               # Static utility functions
│   ├── Pico8MathUtils.cs           # Math utilities (Loop, etc.)
│   ├── Font.cs                     # Font rendering
│   ├── SinDict.cs, CosDict.cs     # Lookup tables
│   └── [Interfaces]                # IScene, IGraphicsAPI, IAudioAPI, etc.
│
├── CSharpCraft.Game/               # Game scenes (62 files, all migrated)
│   ├── Main.cs                     # Entry point + GameOrchestrator setup
│   ├── Competitive/                # 15+ competitive multiplayer scenes
│   ├── Pcraft/                     # Core gameplay (base classes + variants)
│   ├── OptionsMenu/                # 7 options/settings scenes
│   ├── Credits/                    # Credits scene
│   ├── TitleScreen.cs, ExitScene.cs, MapTest.cs, MapConversion.cs
│   └── Content/                    # Game assets (graphics, audio, UI)
│
├── CSharpCraft.Tests/              # xUnit + Moq + FluentAssertions (419 tests)
│   ├── Pico8/                      # Pico8 API + service tests (20 files)
│   │   ├── Pico8StaticAPITests.cs          # Static API delegation
│   │   ├── Pico8APIExpansionTests.cs       # Extended API coverage
│   │   ├── Pico8RenderingAPITests.cs       # Rendering delegation
│   │   ├── Pico8DisplayConfigTests.cs      # Cell, Resolution, GetColor
│   │   ├── GameOrchestratorTests.cs        # Orchestrator lifecycle
│   │   ├── GraphicsOrchestratorTests.cs    # Graphics state
│   │   ├── AudioOrchestratorTests.cs       # Audio state
│   │   ├── GraphicsAPITests.cs             # Concrete rendering
│   │   ├── AudioAPITests.cs                # Concrete audio
│   │   ├── PaletteManagerTests.cs          # Palette remapping (35 tests)
│   │   ├── SceneManagerTests.cs            # Scene transitions (22 tests)
│   │   ├── InputStateManagerTests.cs       # Input state (20 tests)
│   │   ├── PauseMenuStateTests.cs          # Pause state machine (17 tests)
│   │   ├── GameStateContainerTests.cs      # State container (22 tests)
│   │   ├── ServiceFactoryTests.cs          # Factory verification (22 tests)
│   │   ├── MapManagerTests.cs              # Map tile data (20 tests)
│   │   ├── TrackManagerTests.cs            # Music track management (23 tests)
│   │   └── Pico8UtilsTests.cs             # Utility functions (31 tests)
│   ├── Rendering/                  # Rendering abstraction tests
│   │   ├── TextureRendererTests.cs
│   │   └── FnaTextureRendererTests.cs
│   └── OptionsMenu/               # Game-level tests
│       ├── OptionsFileInterfaceTests.cs
│       └── OptionsFileValidationTests.cs
│
├── RaceServer/                     # Backend gRPC server (independent)
│
└── docs/                           # Documentation + analysis notebooks
```

## Core Architecture

### Static Facade Pattern

Scenes interact with a single static class via `using static` — no constructor injection, no service locator:

```csharp
using static CSharpCraft.Pico8.Pico8;

public class MyScene : IScene
{
    public void Init()
    {
        Cls(0);
        Print("Hello", 10, 10, 7);
    }

    public void Update()
    {
        if (Btnp(4))
            ScheduleScene(() => new OtherScene());
    }

    public void Draw()
    {
        Spr(1, 64, 64);
    }
}
```

### Thread Safety via AsyncLocal

The orchestrator is stored in `AsyncLocal<T>`, providing automatic isolation per execution context:

```csharp
// Pico8.cs (simplified)
public static class Pico8
{
    private static readonly AsyncLocal<GameOrchestrator?> _orchestrator = new();

    private static GameOrchestrator Orch
        => _orchestrator.Value ?? throw new InvalidOperationException("Not initialized");

    public static void Initialize(GameOrchestrator orchestrator)
        => _orchestrator.Value = orchestrator;

    // All API methods delegate to the orchestrator
    public static void Cls(int color = 0) => Orch.Graphics.Cls(color);
    public static bool Btn(int button, int player = 0) => Orch.InputManager.Btn(button, player);
}
```

This means tests can run in parallel with isolated state:

```csharp
[Fact]
public void Btn_DelegatesToInputManager()
{
    // Each test gets its own orchestrator — no shared state
    var input = new Mock<IInputStateManager>();
    input.Setup(i => i.Btn(4, 0)).Returns(true);
    var orch = CreateOrchestrator(inputStateManager: input.Object);
    orch.Initialize();

    Pico8.Btn(4).Should().BeTrue();
}
```

### Orchestrator Hierarchy

```
GameOrchestrator (top-level coordinator)
├── InputManager        : IInputStateManager
├── Graphics            : GraphicsOrchestrator
│   ├── API             : IGraphicsAPI (concrete rendering)
│   ├── PaletteManager  : IPaletteManager
│   ├── CameraOffset    : (F32 x, F32 y)
│   ├── Cell            : (int Width, int Height) — pixel scaling
│   └── Resolution      : (int w, int h) — virtual canvas size
├── Audio               : AudioOrchestrator
│   └── API             : IAudioAPI (concrete audio)
├── SceneManager        : ISceneManager
├── MapManager?         : IMapManager
├── GameState?          : IGameState
└── CurrentScene, IsPaused, etc.
```

### Interface Design

Interfaces wrap **platform-specific** operations (FNA/XNA). Everything above the interface layer
is pure C# and fully testable with mocks.

| Interface | Responsibility | Concrete Implementation |
|-----------|---------------|------------------------|
| `IGraphicsAPI` | Draw primitives (Pset, Rect, Circ, Spr, Map, etc.) | `GraphicsAPI` (FNA SpriteBatch) |
| `IAudioAPI` | Play sfx/music, mute | `AudioAPI` (FNA SoundEffect) |
| `IInputStateManager` | Button state queries | FNA keyboard/gamepad wrapper |
| `IPaletteManager` | Palette remapping (Pal/Palt) | Color index remapper |
| `ISceneManager` | Scene transition scheduling | Scene lifecycle manager |
| `IMapManager` | Map tile data (Mget/Mset/Fget) | Tile data storage |
| `IServiceFactory` | Service creation | Runtime factory |
| `ITextureRenderer` | Texture rendering | `FnaTextureRenderer` |
| `IOutputFacade` | Graphics+Audio facade | `OutputFacade` |

## Project Dependencies

```
CSharpCraft.FixMath (no dependencies)
  ↓
CSharpCraft.Pico8 → FNA.Core (only FNA dependency)
  ↓
CSharpCraft.Game → CSharpCraft.Pico8, Protobuf, gRPC
  ↓
CSharpCraft.Tests → CSharpCraft.Pico8, xUnit, Moq, FluentAssertions
```

## Testing Strategy

- **419 tests**, all passing
- **FluentAssertions** exclusively (zero `Assert.*` calls)
- **Moq** for all interface mocking
- **Parallel execution** enabled via `xunit.runner.json`
- **AsyncLocal isolation** ensures no test interference

### Test Coverage by Area

| Area | Tests | Key Scenarios |
|------|-------|---------------|
| PaletteManager | 35 | Remapping, transparency, reset, chaining |
| Pico8Utils | 31 | Mathematical utilities, edge cases |
| TrackManager | 23 | Music state machine, transitions |
| SceneManager | 22 | Scene transitions, scheduling, lifecycle |
| ServiceFactory | 22 | Service creation, validation |
| GameStateContainer | 22 | State management, accessors |
| MapManager | 20 | Tile data, get/set, flags |
| InputStateManager | 20 | Button state, lockout, multi-player |
| PauseMenuState | 17 | State machine, menu navigation |
| Static API + Orchestrators | ~100+ | Delegation, lifecycle, config |
| Rendering + OptionsMenu | ~30+ | Texture rendering, file I/O |

## Refactoring Roadmap

All phases complete.

| Phase | Status | Description |
|-------|--------|-------------|
| 1 | ✅ | Pico8 static API design + TDD tests |
| 2 | ✅ | GameOrchestrator tests & implementation |
| 3 | ✅ | Sub-orchestrators (Graphics, Audio) |
| 4 | ✅ | API expansion (Camera, Palette, Scene, Map, Audio, Data, Math, Rendering, Display Config) |
| 5 | ✅ | Scene migration — all 29 IScene implementations migrated to `using static` |
| 6 | ✅ | Cleanup — Pico8Functions.cs (875 lines) and Services/ directory removed |
| 7-10 | ✅ | Extended service extraction, test coverage expansion, GameStateContainer, ServiceFactory |

## Build & Test

```bash
# Build everything
dotnet build CSharpCraft.slnx

# Run tests
dotnet test CSharpCraft.Tests/

# Build game project only
dotnet build CSharpCraft.Game/
```
