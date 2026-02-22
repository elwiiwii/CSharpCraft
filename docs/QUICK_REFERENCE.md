# CSharpCraft Quick Reference

## Build & Test Commands

```bash
# Build everything
dotnet build CSharpCraft.slnx

# Build one project
dotnet build CSharpCraft.Game/

# Run all tests (419 tests)
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

| File | Purpose |
|------|---------|
| `Pico8.cs` | Static API facade — all static methods |
| `GameOrchestrator.cs` | Top-level coordinator |
| `GraphicsOrchestrator.cs` | Graphics state (camera, palette, display) |
| `AudioOrchestrator.cs` | Audio state (sfx, music, mute) |
| `GraphicsAPI.cs` | FNA rendering (implements `IGraphicsAPI`) |
| `AudioAPI.cs` | FNA audio (implements `IAudioAPI`) |
| `PaletteManager.cs` | Color remapping (implements `IPaletteManager`) |
| `SceneManager.cs` | Scene transitions (implements `ISceneManager`) |
| `InputStateManager.cs` | Button state (implements `IInputStateManager`) |
| `MapManager.cs` | Map tiles (implements `IMapManager`) |
| `GameStateContainer.cs` | Game state (implements `IGameState`) |
| `ServiceFactory.cs` | Service creation (implements `IServiceFactory`) |
| `PauseMenuState.cs` | Pause state machine |
| `MusicManager.cs` | Music state machine |
| `Pico8Classes.cs` | Data classes (PalCol, MenuItem, P8Btns, etc.) |
| `Pico8Utils.cs` | Static utility functions |
| `Pico8MathUtils.cs` | Math utilities (Loop, etc.) |

### Game Scenes (CSharpCraft.Game/)

```
CSharpCraft.Game/
├── Main.cs              # Entry point + orchestrator setup
├── TitleScreen.cs       # Main menu
├── ExitScene.cs         # Exit screen
├── Competitive/         # 15+ multiplayer scenes
├── Pcraft/              # Core gameplay
│   ├── PcraftBase.cs    # Main engine
│   └── Entity.cs        # Game entities
├── OptionsMenu/         # Settings scenes
├── Credits/             # Credits scene
└── Content/             # Assets (graphics, audio)
```

### Tests (CSharpCraft.Tests/)

```
CSharpCraft.Tests/
├── Pico8/
│   ├── Pico8StaticAPITests.cs          # Static API delegation
│   ├── Pico8APIExpansionTests.cs       # Extended API
│   ├── Pico8RenderingAPITests.cs       # Rendering delegation
│   ├── Pico8DisplayConfigTests.cs      # Display config
│   ├── GameOrchestratorTests.cs        # Orchestrator lifecycle
│   ├── GraphicsOrchestratorTests.cs    # Graphics state
│   ├── AudioOrchestratorTests.cs       # Audio state
│   ├── GraphicsAPITests.cs            # Concrete rendering
│   ├── AudioAPITests.cs               # Concrete audio
│   ├── PaletteManagerTests.cs         # Palette (35 tests)
│   ├── SceneManagerTests.cs           # Scenes (22 tests)
│   ├── InputStateManagerTests.cs      # Input (20 tests)
│   ├── PauseMenuStateTests.cs         # Pause (17 tests)
│   ├── GameStateContainerTests.cs     # State (22 tests)
│   ├── ServiceFactoryTests.cs         # Factory (22 tests)
│   ├── MapManagerTests.cs             # Maps (20 tests)
│   ├── TrackManagerTests.cs           # Music (23 tests)
│   └── Pico8UtilsTests.cs            # Utils (31 tests)
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

| File | Purpose |
|------|---------|
| [README.md](../README.md) | Project overview, quick start |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System design, orchestrator hierarchy |
| [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) | How-to guide, patterns, best practices |
| [QUICK_REFERENCE.md](QUICK_REFERENCE.md) | This file — API cheat sheet |

**Start here**: QUICK_REFERENCE.md → DEVELOPER_GUIDE.md → ARCHITECTURE.md

---

**Last Updated**: July 2025
