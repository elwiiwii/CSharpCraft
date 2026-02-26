# CSharpCraft Quick Reference

## Build & Test Commands

```bash
# Build everything
dotnet build CSharpCraft.slnx

# Build one project
dotnet build CSharpCraft.Game/

# Run all tests (544 tests)
dotnet test CSharpCraft.Tests/

# Run specific test class
dotnet test --filter "PaletteManagerTests"

# Run specific test method
dotnet test --filter "PaletteManagerTests.Pal_RemapsColor"

# Verbose test output
dotnet test CSharpCraft.Tests/ --logger "console;verbosity=normal"

# Clean rebuild
dotnet clean && dotnet build CSharpCraft.slnx
```

---

## Where to Find Things

### Pico8 API Layer (CSharpCraft.Pico8/)

**Root files:**

| File | Purpose |
|------|---------|
| `Pico8.cs` | Static API facade — all static methods (645 lines) |
| `GameOrchestrator.cs` | Top-level coordinator (373 lines) |
| `GameRendering.cs` | Rendering pipeline + static accessor |
| `Notifications.cs` | Static popup accessor (AsyncLocal pattern) |
| `GlobalUsings.cs` | Cross-namespace imports for sub-folders |

**Audio/** (namespace `CSharpCraft.Pico8.Audio`)

| File | Purpose |
|------|---------|
| `AudioAPI.cs` | FNA audio (implements `IAudioAPI`) |
| `AudioChannels.cs` | Multi-channel SFX playback |
| `AudioOrchestrator.cs` | Audio state: sfx, music, mute |
| `MusicManager.cs` | Music playback state machine (200 lines) |
| `TrackManager.cs` | Music/SFX track selection (implements `ITrackManager`) |
| `IAudioAPI.cs`, `IAudioSettings.cs`, `ITrackManager.cs` | Interfaces |

**Graphics/** (namespace `CSharpCraft.Pico8.Graphics`)

| File | Purpose |
|------|---------|
| `GraphicsAPI.cs` | FNA rendering (implements `IGraphicsAPI`) |
| `GraphicsOrchestrator.cs` | Graphics state (camera, palette, display) |
| `DisplayManager.cs` | Resolution/cell management (implements `IDisplayManager`) |
| `FnaTextureRenderer.cs` | Texture rendering (implements `ITextureRenderer`) |
| `PaletteManager.cs` | Color remapping (implements `IPaletteManager`) |
| `SpriteCache.cs` | Sprite caching |
| `IGraphicsAPI.cs`, `IDisplayManager.cs`, `IDisplaySettings.cs`, `IPaletteManager.cs`, `ITextureRenderer.cs` | Interfaces |

**Input/** (namespace `CSharpCraft.Pico8.Input`)

| File | Purpose |
|------|---------|
| `InputStateManager.cs` | Button state (implements `IInputStateManager`) |
| `InputBindings.cs` | Input binding configuration |
| `DefaultInputBindings.cs` | Null Object for `IInputBindingProvider` |
| `P8Btns.cs` | Button state tracker class |
| `IInputBindingProvider.cs`, `IInputStateManager.cs` | Interfaces |

**Scene/** (namespace `CSharpCraft.Pico8.Scene`)

| File | Purpose |
|------|---------|
| `SceneManager.cs` | Scene transitions (implements `ISceneManager`) |
| `MapManager.cs` | Map tiles (implements `IMapManager`) |
| `CartDataLoader.cs` | Cart data parsing (implements `ICartDataLoader`) |
| `NullScene.cs` | Null Object for `IScene` |
| `IScene.cs`, `ISceneManager.cs`, `IMapManager.cs`, `ICartDataLoader.cs` | Interfaces |

**Menu/** (namespace `CSharpCraft.Pico8.Menu`)

| File | Purpose |
|------|---------|
| `PauseMenuBuilder.cs` | Pause menu construction (no circular deps) |
| `PauseMenuRenderer.cs` | Pause menu overlay (implements `IPauseMenuRenderer`) |
| `PauseMenuState.cs` | Pause state machine |
| `PopupService.cs` | Notification popups (implements `IPopupService`) |
| `PopupSeverity.cs` | Info/Error severity enum |
| `MenuItem.cs`, `MenuInput.cs` | Menu data types |
| `IPauseMenuContext.cs`, `IPauseMenuRenderer.cs`, `IPopupService.cs` | Interfaces |

**Models/** (namespace `CSharpCraft.Pico8.Models`)

| File | Purpose |
|------|---------|
| `GameHostContext.cs` | FNA platform dependency record (12 properties) |
| `CartData.cs` | Cart data record |
| `InMemorySettings.cs` | Null Object for settings |
| `MusicInst.cs` | Active music instance record |
| `SongInst.cs` | Song definition record |
| `PalCol.cs` | Palette color remapping record |

**Utilities/** (namespace `CSharpCraft.Pico8.Utilities`)

| File | Purpose |
|------|---------|
| `Pico8Utils.cs` | Static utility functions |
| `Pico8MathUtils.cs` | Math utilities (Loop, etc.) |
| `CosDict.cs`, `SinDict.cs` | Trig lookup tables |
| `IntArrayEqualityComparer.cs` | Array equality comparer |

### Game Scenes (CSharpCraft.Game/)

```
CSharpCraft.Game/
├── Main.cs              # Entry point + audio/video wiring (264 lines)
├── TitleScreen.cs       # Main menu
├── ExitScene.cs         # Exit screen
├── Competitive/         # 15+ multiplayer scenes
├── Pcraft/              # Core gameplay
│   ├── PcraftBase.cs    # Main engine
│   └── Entity.cs        # Game entities
├── OptionsMenu/         # Settings scenes + OptionsFile
├── Credits/             # Credits scene
└── Content/             # Assets (graphics, audio)
```

### Tests (CSharpCraft.Tests/ — 569 tests, 36 files)

```
CSharpCraft.Tests/
├── Pico8/                                  # 30 test files
│   ├── Pico8StaticAPITests.cs              # Static API delegation
│   ├── Pico8APIExpansionTests.cs           # Extended API
│   ├── Pico8RenderingAPITests.cs           # Rendering delegation
│   ├── Pico8DisplayConfigTests.cs          # Display config
│   ├── GameOrchestratorTests.cs            # Orchestrator lifecycle
│   ├── GameHostContextTests.cs             # Host context record
│   ├── GraphicsOrchestratorTests.cs        # Graphics state
│   ├── AudioOrchestratorTests.cs           # Audio state
│   ├── AudioLifecycleTests.cs              # Audio lifecycle
│   ├── GraphicsAPITests.cs                 # Concrete rendering
│   ├── AudioAPITests.cs                    # Concrete audio
│   ├── PaletteManagerTests.cs              # Palette remapping
│   ├── SceneManagerTests.cs                # Scene transitions
│   ├── InputStateManagerTests.cs           # Input state
│   ├── PauseMenuStateTests.cs              # Pause state machine
│   ├── PauseMenuRendererTests.cs           # Pause menu rendering
│   ├── PauseMenuContextTests.cs            # IPauseMenuContext
│   ├── PauseMenuBuilderDependencyTests.cs  # Builder isolation
│   ├── PopupServiceTests.cs                # Notification popups
│   ├── NotificationsTests.cs               # Static popup accessor
│   ├── OverlayIntegrationTests.cs          # Overlay integration
│   ├── ConstructorConsolidationTests.cs    # Unified constructor
│   ├── StateUnificationTests.cs            # State unification
│   ├── FactoryInjectionTests.cs            # Factory injection
│   ├── DisplayManagerTests.cs              # Display management
│   ├── CartDataLoaderTests.cs              # Cart data parsing
│   ├── ServiceFactoryTests.cs              # Factory verification
│   ├── MapManagerTests.cs                  # Map tile data
│   ├── TrackManagerTests.cs                # Music track management
│   └── Pico8UtilsTests.cs                 # Utility functions
├── Rendering/
│   ├── TextureRendererTests.cs
│   └── FnaTextureRendererTests.cs
└── OptionsMenu/
    ├── OptionsFileInterfaceTests.cs
    └── OptionsFileValidationTests.cs
```

### Fixed-Point Math (CSharpCraft.FixMath/)

```
F32.cs, F64.cs             # 32/64-bit fixed types
Fixed32.cs, Fixed64.cs     # Alternative implementations
FixedUtil.cs               # Utility functions
```

---

## API Cheat Sheet

All methods use `using static CSharpCraft.Pico8.Pico8;`

### Graphics

```csharp
Cls(0);                              // Clear screen (color 0)
Spr(spriteNum, x, y);               // Draw sprite
Spr(spriteNum, x, y, w, h);         // Draw sprite region
Print(text, x, y, color);           // Draw text
Circ(x, y, radius, color);          // Circle outline
Circfill(x, y, radius, color);      // Filled circle
Rect(x1, y1, x2, y2, color);       // Rectangle outline
Rectfill(x1, y1, x2, y2, color);   // Filled rectangle
Line(x1, y1, x2, y2, color);       // Draw line
Pset(x, y, color);                  // Set pixel
Camera(offsetX, offsetY);            // Set camera offset
```

### Palette

```csharp
Pal(fromColor, toColor);            // Remap draw color
Palt(color, isTransparent);          // Set transparency
Pal();                               // Reset all palette
```

### Input

```csharp
Btn(button);                         // Is button held? (0-5)
Btn(button, player);                 // Button for specific player
Btnp(button);                        // Was button just pressed?
Btnp(button, player);               // Pressed for specific player
// Buttons: 0=left, 1=right, 2=up, 3=down, 4=confirm, 5=cancel
```

### Audio

```csharp
Music(trackId);                      // Play music track
Sfx(sfxId);                          // Play sound effect
```

### Maps

```csharp
Mget(x, y);                         // Get tile at position
Mset(x, y, tile);                   // Set tile
Fget(sprite);                       // Get sprite flags
Fget(sprite, flag);                 // Get specific flag
Map(celX, celY, sx, sy, celW, celH); // Draw map region
```

### Scene Management

```csharp
ScheduleScene(() => new MyScene());  // Queue scene transition
```

### Math (Pico8MathUtils)

```csharp
Pico8MathUtils.Loop(value, max);     // Wrap value in range
```

---

## Templates

### New Scene

```csharp
using static CSharpCraft.Pico8.Pico8;

namespace CSharpCraft.Game.YourCategory;

public class YourScene : IScene
{
    public string SceneName => "Your Scene";
    public double Fps => 60;
    public (int w, int h) Resolution => (128, 128);
    public string SpriteImage => "";
    public string SpriteData => "";
    public string FlagData => "";
    public (int x, int y) MapDimensions => (0, 0);
    public string MapData => "";
    public Dictionary<string, List<SongInst>> Music => [];
    public Dictionary<string, Dictionary<int, string>> Sfx => [];

    public void Init()
    {
        Cls(0);
    }

    public void Update()
    {
        if (Btnp(4))
            ScheduleScene(() => new OtherScene());
    }

    public void Draw()
    {
        Cls(0);
        Print("Your Scene", 10, 10, 7);
    }

    public void Dispose() { }
}
```

### New Test

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace CSharpCraft.Tests.Pico8;

public class MyComponentTests
{
    [Fact]
    public void Method_WhenCondition_ReturnsExpected()
    {
        // Arrange
        var mock = new Mock<ISomeDependency>();
        mock.Setup(m => m.GetValue()).Returns(42);
        var sut = new MyComponent(mock.Object);

        // Act
        var result = sut.Method();

        // Assert
        result.Should().Be(42);
        mock.Verify(m => m.GetValue(), Times.Once);
    }
}
```

---

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `CS0246: namespace not found` | Check `using` statements and project references |
| FNA not found | Verify FNA.Core NuGet package is restored |
| Circular dependency | Dependencies flow: Game → Pico8 → FixMath (one way) |
| Test hangs | Don't create real FNA types in tests — use mocks |
| Build slow | Use `dotnet build ProjectName/` for targeted builds |

---

## Documentation

- [README.md](../README.md) — Project overview, quick start
- [ARCHITECTURE.md](ARCHITECTURE.md) — System design, orchestrator hierarchy, namespace organization, interface table
- [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) — How-to guide, patterns, best practices
- [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) — Refactoring history and metrics
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) — This file — API cheat sheet
- [PICO8_CLASS_DIAGRAM.md](PICO8_CLASS_DIAGRAM.md) — Mermaid class & interface diagram

**Start here**: QUICK_REFERENCE.md → DEVELOPER_GUIDE.md → ARCHITECTURE.md

---

**Last Updated**: February 2026
