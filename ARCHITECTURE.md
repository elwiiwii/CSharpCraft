# CSharpCraft Architecture — Plan D

> Complete architecture redesign using static PICO-8 API + orchestrator pattern.
> See [MIGRATION_STATUS.md](MIGRATION_STATUS.md) for current progress.

## Overview

The codebase uses a **static facade + orchestrator** pattern: game scenes call `Pico8.MethodName()`
(static methods), which delegate to a `GameOrchestrator` stored in `AsyncLocal<T>` for thread safety.
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
├── CSharpCraft.Pico8/              # Static API + orchestrator layer
│   ├── Pico8.cs                    # Static PICO-8 API facade (THE entry point)
│   ├── GameOrchestrator.cs         # Top-level orchestrator (owns sub-orchestrators)
│   ├── GraphicsOrchestrator.cs     # Graphics state: camera, palette, display config
│   ├── AudioOrchestrator.cs        # Audio state: sfx, music, mute
│   ├── IGraphicsAPI.cs             # Rendering primitives interface
│   ├── GraphicsAPI.cs              # Concrete FNA/XNA rendering implementation
│   ├── IAudioAPI.cs                # Audio primitives interface
│   ├── AudioAPI.cs                 # Concrete FNA/XNA audio implementation
│   ├── IInputStateManager.cs       # Input state interface
│   ├── IPaletteManager.cs          # Palette remapping interface
│   ├── ISceneManager.cs            # Scene lifecycle interface
│   ├── IMapManager.cs              # Map data interface
│   ├── IGameState.cs               # Game state interface
│   ├── IScene.cs                   # Scene contract (parameterless Init())
│   ├── Pico8Functions.cs           # LEGACY god class (to be removed Phase 6)
│   ├── Pico8Classes.cs             # LEGACY data classes (to be removed Phase 6)
│   ├── Services/                   # LEGACY service layer (to be removed Phase 6)
│   └── Font.cs, SinDict.cs, CosDict.cs, Pico8MathUtils.cs
│
├── CSharpCraft.Game/               # Game scenes (being migrated to Plan D)
│   ├── Competitive/                # 15 competitive multiplayer scenes
│   ├── Pcraft/                     # Core gameplay (base classes + variants)
│   ├── OptionsMenu/                # 7 options/settings scenes
│   ├── Credits/                    # Credits scene
│   ├── Main.cs, TitleScreen.cs, ExitScene.cs, MapTest.cs, MapConversion.cs
│   └── Content/                    # Game assets (graphics, audio, UI)
│
├── CSharpCraft/                    # Legacy executable (mirror of CSharpCraft.Game)
│
├── CSharpCraft.Tests/              # xUnit + Moq + FluentAssertions (246 tests)
│   └── Pico8/                      # All Plan D tests
│       ├── Pico8InputTests.cs      # Input API tests
│       ├── Pico8GraphicsTests.cs   # Graphics API tests
│       ├── Pico8DisplayConfigTests.cs  # Cell, Resolution, GetColor tests
│       ├── Pico8CameraTests.cs     # Camera state tests
│       ├── Pico8PaletteTests.cs    # Palette management tests
│       ├── Pico8SceneTests.cs      # Scene lifecycle tests
│       ├── Pico8MapTests.cs        # Map data tests
│       ├── Pico8AudioTests.cs      # Audio API tests
│       ├── Pico8DataUtilityTests.cs    # Data utility tests
│       ├── Pico8MathTests.cs       # Math function tests
│       ├── Pico8StubTests.cs       # Stub/placeholder tests
│       ├── Pico8RenderingTests.cs  # Rendering delegation tests
│       ├── GameOrchestratorTests.cs    # Orchestrator lifecycle tests
│       ├── GraphicsOrchestratorTests.cs # Graphics state tests
│       ├── GraphicsAPITests.cs     # Concrete rendering tests
│       └── AudioOrchestratorTests.cs   # Audio state tests
│
└── RaceServer/                     # Backend gRPC server (independent)
```

## Core Architecture

### Static Facade Pattern

Scenes interact with a single static class — no constructor injection, no service locator:

```csharp
// Scene code is simple and clean
public class MyScene : IScene
{
    public void Init()
    {
        Pico8.Cls(0);
        Pico8.Print("Hello", 10, 10, 7);
    }

    public void Update()
    {
        if (Pico8.Btnp(4))
            Pico8.ScheduleScene(() => new OtherScene());
    }

    public void Draw()
    {
        Pico8.Spr(1, 64, 64);
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
| `IGameState` | Shared game state | Runtime state container |

## Project Dependencies

```
CSharpCraft.FixMath (no dependencies)
  ↓
CSharpCraft.Pico8 → FNA.Core (only FNA dependency)
  ↓
CSharpCraft.Game → CSharpCraft.Pico8, Protobuf, gRPC
  ↓
CSharpCraft (executable) → CSharpCraft.Game
  ↓
CSharpCraft.Tests → CSharpCraft.Pico8, xUnit, Moq, FluentAssertions
```

## Testing Strategy

- **246 tests**, all passing, ~700ms
- **Strict TDD**: RED → GREEN → REFACTOR cycle
- **FluentAssertions** exclusively (zero `Assert.*` calls)
- **Moq** for all interface mocking
- **Parallel execution** enabled via `xunit.runner.json`
- **AsyncLocal isolation** ensures no test interference

## Refactoring Roadmap

| Phase | Status | Description |
|-------|--------|-------------|
| 1 | ✅ | Pico8 static API design + TDD tests |
| 2 | ✅ | GameOrchestrator tests & implementation |
| 3 | ✅ | Sub-orchestrators (Graphics, Audio) |
| 4 | ✅ | API expansion (Camera, Palette, Scene, Map, Audio, Data, Math, Rendering, Display Config) |
| 5 | ⬜  | Scene migration — update 29 IScene implementations to use `Pico8.*` |
| 6 | ⬜  | Cleanup — remove legacy code (Pico8Functions.cs, Pico8Classes.cs, Services/) |

See [MIGRATION_STATUS.md](MIGRATION_STATUS.md) for detailed scene-by-scene tracking.

## Build & Test

```bash
# Build everything
dotnet build CSharpCraft.slnx

# Run tests
dotnet test CSharpCraft.Tests/

# Build game project only (currently has 60 known errors — scene migration pending)
dotnet build CSharpCraft.Game/
```
