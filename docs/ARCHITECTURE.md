# CSharpCraft Architecture

> Static PICO-8 API facade + orchestrator pattern.  
> All projects compile with 0 errors. 569 tests passing.

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
├── CSharpCraft.Pico8/              # Static API + orchestrator layer (56 files)
│   ├── Pico8.cs                    # Static PICO-8 API facade (THE entry point)
│   ├── GameOrchestrator.cs         # Top-level orchestrator (373 lines, owns sub-orchestrators)
│   ├── GameHostContext.cs          # FNA platform dependency record (11 properties)
│   ├── GraphicsOrchestrator.cs     # Graphics state: camera, palette, display config
│   ├── AudioOrchestrator.cs        # Audio state: sfx, music, mute
│   ├── GraphicsAPI.cs              # Concrete FNA/XNA rendering (IGraphicsAPI)
│   ├── AudioAPI.cs                 # Concrete FNA/XNA audio (IAudioAPI)
│   ├── AudioChannels.cs            # Multi-channel SFX playback
│   ├── MusicManager.cs             # Music playback state machine
│   ├── PaletteManager.cs           # Color remapping + transparency (IPaletteManager)
│   ├── SceneManager.cs             # Scene transitions + scheduling (ISceneManager)
│   ├── InputStateManager.cs        # Button state + lockout (IInputStateManager)
│   ├── MapManager.cs               # Map tile data (IMapManager)
│   ├── DisplayManager.cs           # Resolution/cell management (IDisplayManager)
│   ├── CartDataLoader.cs           # Cart data parsing (ICartDataLoader)
│   ├── ServiceFactory.cs           # Service creation (IServiceFactory)
│   ├── PauseMenuState.cs           # Pause state machine
│   ├── PauseMenuBuilder.cs         # Menu construction (no circular deps)
│   ├── PauseMenuRenderer.cs        # Pause menu overlay (IPauseMenuRenderer)
│   ├── PopupService.cs             # Notification popups (IPopupService)
│   ├── PopupSeverity.cs            # Info/Error severity enum
│   ├── Notifications.cs            # Static popup accessor (AsyncLocal)
│   ├── GameRendering.cs            # Rendering pipeline + static accessor
│   ├── FnaTextureRenderer.cs       # Texture rendering (ITextureRenderer)
│   ├── OutputFacade.cs             # Graphics+Audio facade (IOutputFacade)
│   ├── NullScene.cs                # Null Object for IScene (test default)
│   ├── InMemorySettings.cs         # Null Object for IAudioGraphicsSettings
│   ├── DefaultInputBindings.cs     # Null Object for IInputBindingProvider
│   ├── Pico8Classes.cs             # Data classes (PalCol, MenuItem, P8Btns, etc.)
│   ├── Pico8Utils.cs               # Static utility functions
│   ├── Pico8MathUtils.cs           # Math utilities (Loop, etc.)
│   ├── Font.cs                     # Font rendering
│   ├── SinDict.cs, CosDict.cs     # Lookup tables
│   └── [18 Interfaces]             # IScene, IGraphicsAPI, IAudioAPI, etc.
│
├── CSharpCraft.Game/               # Game scenes (62 files, all migrated)
│   ├── Main.cs                     # Entry point + audio/video wiring (264 lines)
│   ├── Competitive/                # 15+ competitive multiplayer scenes
│   ├── Pcraft/                     # Core gameplay (base classes + variants)
│   ├── OptionsMenu/                # 7 options/settings scenes + OptionsFile
│   ├── Credits/                    # Credits scene
│   ├── TitleScreen.cs, ExitScene.cs, MapTest.cs, MapConversion.cs
│   └── Content/                    # Game assets (graphics, audio, UI)
│
├── CSharpCraft.Tests/              # xUnit + Moq + FluentAssertions (569 tests, 36 files)
│   ├── Pico8/                      # Pico8 API + service tests (30 files)
│   │   ├── Pico8StaticAPITests.cs          # Static API delegation
│   │   ├── Pico8APIExpansionTests.cs       # Extended API coverage
│   │   ├── Pico8RenderingAPITests.cs       # Rendering delegation
│   │   ├── Pico8DisplayConfigTests.cs      # Cell, Resolution, GetColor
│   │   ├── GameOrchestratorTests.cs        # Orchestrator lifecycle
│   │   ├── GameHostContextTests.cs         # Host context record + constructor
│   │   ├── GraphicsOrchestratorTests.cs    # Graphics state
│   │   ├── AudioOrchestratorTests.cs       # Audio state
│   │   ├── AudioLifecycleTests.cs          # Audio lifecycle
│   │   ├── GraphicsAPITests.cs             # Concrete rendering
│   │   ├── AudioAPITests.cs                # Concrete audio
│   │   ├── PaletteManagerTests.cs          # Palette remapping
│   │   ├── SceneManagerTests.cs            # Scene transitions
│   │   ├── InputStateManagerTests.cs       # Input state
│   │   ├── PauseMenuStateTests.cs          # Pause state machine
│   │   ├── PauseMenuRendererTests.cs       # Pause menu rendering
│   │   ├── PauseMenuContextTests.cs        # IPauseMenuContext
│   │   ├── PauseMenuBuilderDependencyTests.cs # Builder circular-dep elimination
│   │   ├── PopupServiceTests.cs            # Notification popups
│   │   ├── NotificationsTests.cs           # Static popup accessor
│   │   ├── OverlayIntegrationTests.cs      # Overlay integration
│   │   ├── ConstructorConsolidationTests.cs # Unified constructor
│   │   ├── StateUnificationTests.cs        # State unification
│   │   ├── FactoryInjectionTests.cs        # Factory injection
│   │   ├── DisplayManagerTests.cs          # Display management
│   │   ├── CartDataLoaderTests.cs          # Cart data parsing
│   │   ├── ServiceFactoryTests.cs          # Factory verification
│   │   ├── MapManagerTests.cs              # Map tile data
│   │   ├── TrackManagerTests.cs            # Music track management
│   │   └── Pico8UtilsTests.cs             # Utility functions
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
GameOrchestrator (373 lines, top-level coordinator, implements IPauseMenuContext)
├── InputManager         : IInputStateManager
├── Graphics             : GraphicsOrchestrator
│   ├── API              : IGraphicsAPI (concrete rendering)
│   ├── PaletteManager   : IPaletteManager
│   ├── CameraOffset     : (F32 x, F32 y)
│   └── DisplayManager   : IDisplayManager (Cell, Resolution)
├── Audio                : AudioOrchestrator
│   └── API              : IAudioAPI (concrete audio)
│       ├── AudioChannels    (4-channel SFX playback)
│       └── MusicManager     (music state machine)
├── SceneManager         : ISceneManager
├── CartDataLoader       : ICartDataLoader
├── DisplayManager       : IDisplayManager
├── PauseMenuState?      : PauseMenuState
├── PauseMenuRenderer?   : IPauseMenuRenderer
├── PopupService?        : IPopupService
├── MapManager?          : IMapManager
├── TrackManager?        : ITrackManager
├── ServiceFactory       : IServiceFactory
├── TitleSceneFactory    : Func<IScene>
└── CurrentCart, IsPaused, etc.
```

### GameHostContext Record

Groups FNA platform dependencies for the production constructor (test constructor bypasses it):

```csharp
public record GameHostContext(
    SpriteBatch Batch, Texture2D Pixel,
    GraphicsDeviceManager Graphics, GraphicsDevice GraphicsDevice, GameWindow Window,
    Dictionary<string, Texture2D> TextureDictionary,
    Dictionary<string, SoundEffect> MusicDictionary,
    Dictionary<string, SoundEffect> SoundEffectDictionary,
    IAudioGraphicsSettings Settings, IInputBindingProvider InputBindings,
    List<IScene> Scenes);
```

### Interface Design

Interfaces wrap **platform-specific** operations (FNA/XNA). Everything above the interface layer
is pure C# and fully testable with mocks.

| Interface | Responsibility | Concrete Implementation |
|-----------|---------------|------------------------|
| `IGraphicsAPI` | Draw primitives (Pset, Rect, Circ, Spr, Map, etc.) | `GraphicsAPI` (FNA SpriteBatch) |
| `IAudioAPI` | Play sfx/music, mute | `AudioAPI` (FNA SoundEffect) |
| `IInputStateManager` | Button state queries | `InputStateManager` (FNA keyboard/gamepad) |
| `IPaletteManager` | Palette remapping (Pal/Palt) | `PaletteManager` (color index remapper) |
| `ISceneManager` | Scene transition scheduling | `SceneManager` |
| `IMapManager` | Map tile data (Mget/Mset/Fget) | `MapManager` |
| `ITrackManager` | Music/SFX track selection | `TrackManager` |
| `IServiceFactory` | Service creation | `ServiceFactory` |
| `ITextureRenderer` | Texture rendering | `FnaTextureRenderer` |
| `IOutputFacade` | Graphics+Audio facade | `OutputFacade` |
| `IDisplayManager` | Resolution/cell scaling | `DisplayManager` |
| `ICartDataLoader` | Cart data parsing | `CartDataLoader` |
| `IPauseMenuRenderer` | Pause menu overlay drawing | `PauseMenuRenderer` |
| `IPopupService` | Notification popup lifecycle | `PopupService` |
| `IPauseMenuContext` | System operations for pause menu | `GameOrchestrator` |
| `IInputBindingProvider` | Input binding configuration | `DefaultInputBindings` (Null Object) |
| `IAudioGraphicsSettings` | Audio/graphics settings | `InMemorySettings` (Null Object) |
| `IScene` | Scene contract (Init/Update/Draw/Dispose) | `NullScene` (Null Object) |

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

- **569 tests**, all passing
- **FluentAssertions** exclusively (zero `Assert.*` calls)
- **Moq** for all interface mocking
- **Parallel execution** enabled via `xunit.runner.json`
- **AsyncLocal isolation** ensures no test interference
- **36 test files** across 3 categories

### Test Coverage by Area

| Area | Key Scenarios |
|------|---------------|
| Static API + Orchestrators | Delegation, lifecycle, config |
| PaletteManager | Remapping, transparency, reset, chaining |
| Pico8Utils | Mathematical utilities, edge cases |
| TrackManager | Music state machine, transitions |
| SceneManager | Scene transitions, scheduling, lifecycle |
| ServiceFactory | Service creation, validation |
| MapManager | Tile data, get/set, flags |
| InputStateManager | Button state, lockout, multi-player |
| PauseMenuState | State machine, menu navigation |
| PauseMenuRenderer | Overlay drawing, arrow rendering |
| PopupService + Notifications | Popup lifecycle, severity colors |
| PauseMenuBuilder | Builder dependency isolation |
| CartDataLoader | Cart data parsing |
| DisplayManager | Resolution/cell management |
| Constructor Consolidation | Unified constructor, Null Objects |
| GameHostContext | Record properties, production wiring |
| Audio Lifecycle | AudioChannels → MusicManager → AudioAPI chain |
| Rendering + OptionsMenu | Texture rendering, file I/O |

## Refactoring Roadmap

| Phase | Status | Description |
|-------|--------|-------------|
| 1–4 | ✅ | Pico8 static API + GameOrchestrator + sub-orchestrators + API expansion |
| 5 | ✅ | Scene migration — all 29 IScene implementations migrated to `using static` |
| 6 | ✅ | Cleanup — Pico8Functions.cs (875 lines) and Services/ directory removed |
| 7–8 | ✅ | Extended service extraction (GameOrchestrator 1,072 → 328 lines) |
| 9 | ✅ | Notifications + PopupService, IPauseMenuRenderer, hotkey extraction |
| 10 | ✅ | Constructor consolidation — NullScene, InMemorySettings, DefaultInputBindings |
| 11 | ✅ | PauseMenuBuilder circular dependency removal (MenuInput record) |
| 12 | ✅ | YAGNI cleanup — dead code removal from GameOrchestrator and Main.cs |
| 13 | ✅ | Content loading — GameHostContext expanded, music pipeline wired |

## Build & Test

```bash
# Build everything
dotnet build CSharpCraft.slnx

# Run tests
dotnet test CSharpCraft.Tests/

# Build game project only
dotnet build CSharpCraft.Game/
```
