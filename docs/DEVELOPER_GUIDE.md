# CSharpCraft Developer Guide

Guide for navigating and contributing to the CSharpCraft codebase.

## Project Structure

**4 Core Projects:**

1. **CSharpCraft.FixMath** — Fixed-point arithmetic (F32, F64). No dependencies, pure math. Used for deterministic physics.

2. **CSharpCraft.Pico8** — Static PICO-8 API layer (56 source files). Provides `Cls()`, `Spr()`, `Btn()`, etc. via `using static CSharpCraft.Pico8.Pico8`. All platform-specific code (FNA) is behind 18 interfaces.

3. **CSharpCraft.Game** — All game scenes (62 files). Competitive, Pcraft, OptionsMenu, Credits. Main development area.

4. **CSharpCraft.Tests** — xUnit + Moq + FluentAssertions (569 tests, 36 test files). Full coverage of Pico8 API, orchestrators, and services.

### Dependency Flow

```
CSharpCraft.FixMath (no dependencies)
  ↓
CSharpCraft.Pico8 → FNA.Core
  ↓
CSharpCraft.Game → CSharpCraft.Pico8, Protobuf, gRPC
  ↓
CSharpCraft.Tests → CSharpCraft.Pico8, xUnit, Moq, FluentAssertions
```

## The Static API Pattern

All game scenes use bare static method calls via `using static`:

```csharp
using static CSharpCraft.Pico8.Pico8;

public class MyScene : IScene
{
    public string SceneName => "MyScene";
    public double Fps => 60;
    public (int w, int h) Resolution => (128, 128);

    public void Init()
    {
        // No parameters — static methods are available directly
        Cls(0);
        Print("Ready!", 10, 10, 7);
    }

    public void Update()
    {
        if (Btnp(4))       // Confirm button
            ScheduleScene(() => new OtherScene());
    }

    public void Draw()
    {
        Cls(0);
        Spr(1, 64, 64);
    }

    public void Dispose() { }
}
```

Behind the scenes, static methods delegate to a `GameOrchestrator` stored in `AsyncLocal<T>`, providing thread-safe per-context isolation.

## Working on Different Areas

### Adding a New Game Scene

1. Create `CSharpCraft.Game/[Category]/NewScene.cs`
2. Implement `IScene` (parameterless `Init()`)
3. Add `using static CSharpCraft.Pico8.Pico8;` at top
4. Use bare static calls: `Cls()`, `Spr()`, `Btn()`, etc.
5. Register the scene transition in the calling scene

### Working with Graphics

```csharp
using static CSharpCraft.Pico8.Pico8;

// Drawing
Cls(0);                          // Clear screen
Spr(1, 64, 64);                 // Draw sprite
Circ(64, 64, 10, 3);            // Draw circle
Rect(10, 10, 50, 50, 7);        // Draw rectangle
Print("Hello", 10, 10, 7);      // Draw text

// Camera
Camera(offsetX, offsetY);        // Set camera offset

// Palette
Pal(1, 8);                      // Remap color 1 → 8
Palt(0, true);                  // Make color 0 transparent
```

### Working with Input

```csharp
using static CSharpCraft.Pico8.Pico8;

if (Btn(0))  { /* Left held */ }
if (Btn(1))  { /* Right held */ }
if (Btnp(4)) { /* Confirm just pressed */ }
if (Btnp(5)) { /* Cancel just pressed */ }
```

### Working with Audio

```csharp
using static CSharpCraft.Pico8.Pico8;

Music(0);        // Play music track 0
Sfx(3);          // Play sound effect 3
```

### Working with Maps

```csharp
using static CSharpCraft.Pico8.Pico8;

var tile = Mget(x, y);     // Get tile at position
Mset(x, y, 5);             // Set tile
var flags = Fget(tile);     // Get sprite flags
```

## Adding Tests

### Test Structure

Tests mirror source structure:

```
CSharpCraft.Tests/
├── Pico8/                                  # Pico8 API + service tests (30 files)
│   ├── Pico8StaticAPITests.cs              # Static API delegation
│   ├── Pico8APIExpansionTests.cs           # Extended API coverage
│   ├── Pico8RenderingAPITests.cs           # Rendering delegation
│   ├── Pico8DisplayConfigTests.cs          # Display config
│   ├── GameOrchestratorTests.cs            # Orchestrator lifecycle
│   ├── GameHostContextTests.cs             # Host context record
│   ├── GraphicsOrchestratorTests.cs        # Graphics state
│   ├── AudioOrchestratorTests.cs           # Audio state
│   ├── AudioLifecycleTests.cs              # Audio lifecycle
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
│   ├── GraphicsAPITests.cs                 # Concrete rendering
│   ├── AudioAPITests.cs                    # Concrete audio
│   └── Pico8UtilsTests.cs                 # Utility functions
├── Rendering/
│   ├── TextureRendererTests.cs
│   └── FnaTextureRendererTests.cs
└── OptionsMenu/
    ├── OptionsFileInterfaceTests.cs
    └── OptionsFileValidationTests.cs
```

### Test Naming Convention

- Class: `[ComponentName]Tests.cs`
- Method: `[Scenario]_[ExpectedOutcome]()` or `[Method]_[Condition]_[Result]()`

### Writing a Test

```csharp
using FluentAssertions;
using Moq;
using Xunit;

public class MyServiceTests
{
    [Fact]
    public void DoSomething_WhenConditionMet_ReturnsExpected()
    {
        // Arrange
        var mock = new Mock<ISomeDependency>();
        mock.Setup(m => m.GetValue()).Returns(42);
        var sut = new MyService(mock.Object);

        // Act
        var result = sut.DoSomething();

        // Assert
        result.Should().Be(42);
    }
}
```

### Testing with the Static API (Orchestrator Pattern)

Tests create isolated `GameOrchestrator` instances with mocked dependencies:

```csharp
using FluentAssertions;
using Moq;
using Xunit;

public class MySceneTests
{
    [Fact]
    public void Btn_DelegatesToInputManager()
    {
        // Arrange — each test gets its own orchestrator
        var input = new Mock<IInputStateManager>();
        input.Setup(i => i.Btn(4, 0)).Returns(true);
        var orch = CreateOrchestrator(inputStateManager: input.Object);
        orch.Initialize();

        // Act
        var result = Pico8.Btn(4);

        // Assert
        result.Should().BeTrue();
        input.Verify(i => i.Btn(4, 0), Times.Once);
    }
}
```

### Mock Examples

**Mock Graphics API:**
```csharp
var graphics = new Mock<IGraphicsAPI>();
graphics.SetupGet(g => g.Cell).Returns((8, 8));
graphics.SetupGet(g => g.Resolution).Returns((128, 128));
// Verify calls
graphics.Verify(g => g.Cls(0), Times.Once);
```

**Mock Input State Manager:**
```csharp
var input = new Mock<IInputStateManager>();
input.Setup(i => i.Btn(0, 0)).Returns(true);
input.Setup(i => i.Btnp(4, 0)).Returns(true);
```

**Mock Scene Manager:**
```csharp
var sceneManager = new Mock<ISceneManager>();
sceneManager.Verify(s => s.ScheduleScene(It.IsAny<Func<IScene>>()), Times.Once);
```

**Mock Palette Manager:**
```csharp
var palette = new Mock<IPaletteManager>();
palette.Setup(p => p.GetDrawColor(1)).Returns(8);
```

## Build & Run

```bash
# Build everything
dotnet build CSharpCraft.slnx

# Run all tests
dotnet test CSharpCraft.Tests/

# Run specific test class
dotnet test --filter "ClassName"

# Run specific test method
dotnet test --filter "ClassName.MethodName"

# Run with verbose output
dotnet test -v d

# Build game project only
dotnet build CSharpCraft.Game/
```

## Common Scenarios

### "I need to add a new button to the input system"

1. Add button constant in `Pico8Classes.cs` (`P8Btns`)
2. Update `InputStateManager` to handle the new button
3. Add static delegate in `Pico8.cs`
4. Add test in `InputStateManagerTests.cs`

### "Something broke and I don't know what"

```bash
# Clean rebuild
dotnet clean && dotnet build CSharpCraft.slnx

# Check specific project
dotnet build CSharpCraft.Game/CSharpCraft.Game.csproj --verbosity detailed

# Run tests for clues
dotnet test CSharpCraft.Tests/ --logger "console;verbosity=detailed"
```

### "I'm refactoring a large method"

1. Write tests for current behavior first
2. Extract smaller methods incrementally
3. Run tests after each change
4. Verify all 569 tests pass at the end

## Best Practices

**Do:**
- Use `using static CSharpCraft.Pico8.Pico8;` in all scene files
- Write tests before major refactoring (FluentAssertions, never `Assert.*`)
- Use interfaces for platform-specific dependencies
- Keep classes small and focused
- Mock via Moq — never instantiate FNA types in tests
- Commit small, logical changes

**Don't:**
- Pass `GameOrchestrator` or services to scene constructors (use static API)
- Use `Assert.Equal` / `Assert.True` — use `result.Should().Be()` / `.Should().BeTrue()`
- Create classes > 500 lines without good reason
- Mix rendering + physics + input in one class
- Reference FNA types directly in game scenes

## Documentation

- [ARCHITECTURE.md](ARCHITECTURE.md) — System design, orchestrator hierarchy, interface table
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) — API cheat sheet, file locations, templates
- [PROJECT_SUMMARY.md](PROJECT_SUMMARY.md) — Refactoring history and metrics

---

**Last Updated**: February 2026
