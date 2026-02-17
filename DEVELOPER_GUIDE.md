# CSharpCraft Developer Guide

Welcome back to CSharpCraft! The project has been restructured into a modular, testable architecture. This guide will help you navigate the new structure and contribute effectively.

## For New Contributors

### Understanding the Project Structure

**5 Core Projects:**

1. **CSharpCraft.FixMath** 📐
   - Fixed-point arithmetic (F32, F64)
   - No dependencies → pure math
   - Used for deterministic physics
   - Start here if working on physics/precision issues

2. **CSharpCraft.Pico8** 🎮
   - Graphics/audio/input abstraction layer
   - Replaces direct FNA API calls
   - Provides interfaces for dependency injection
   - Start here if working on rendering/input systems

3. **CSharpCraft.Game** 🕹️
   - All game logic and scenes
   - Competitive, Pcraft, OptionsMenu, Credits
   - Main development area
   - Start here for game features and scenes

4. **CSharpCraft.Tests** ✅
   - xUnit tests with Moq mocking
   - Test game logic without graphics dependencies
   - Essential for verifying refactoring
   - Add tests here for any new feature

5. **CSharpCraft** (Legacy)
   - Original executable entry point
   - Remains for backward compatibility
   - Will be deprecated once Game project is fully integrated

### Project Dependencies (One Way Flow)

```
                   ┌─────────────────┐
                   │ CSharpCraft     │ (executable)
                   └────────┬────────┘
                            │
                   ┌────────▼────────┐
                   │CSharpCraft.Game │ (game logic)
                   └────────┬────────┘
                            │
                ┌───────────┼───────────┐
                │           │           │
        ┌───────▼──┐┌───────▼──┐┌──────▼───┐
        │  Pico8   ││ FixMath  ││Nuget pkgs│
        └───────┬──┘└──────────┘└──────────┘
                │
        ┌───────▼────────┐
        │  FNA.Core      │ (XNA graphics)
        └────────────────┘

    ┌──────────────────┐
    │CSharpCraft.Tests │ (xUnit, Moq)
    └──────────────────┘
        
    ┌──────────────────┐
    │  RaceServer      │ (backend - independent)
    └──────────────────┘
```

## Working on Different Areas

### 🎨 Adding a New Game Scene

**Steps:**
1. Create file in `CSharpCraft.Game/[Category]/NewScene.cs`
2. Implement `IScene` interface
3. Update namespace usings at top
4. Add to scene registry in Main.cs

**Example:**
```csharp
namespace CSharpCraft.Game.YourCategory;

public class MyNewScene : IScene
{
    private IGraphicsEngine? graphics;
    private IInputManager? input;
    
    public string SceneName => "MyScene";
    public double Fps => 60;
    public (int w, int h) Resolution => (128, 128);
    
    public void Init(Pico8Functions p8)
    {
        // Initialize with Pico8Functions
        // (This will be refactored to inject services)
    }
    
    public void Update()
    {
        // Game logic each frame
    }
    
    public void Draw()
    {
        // Rendering calls
    }
    
    public void Dispose() { }
    
    // ... IScene properties
}
```

### 🔧 Fixing Physics or Math

**Steps:**
1. Look in `CSharpCraft.FixMath/` for Fixed32 or Fixed64 types
2. Add tests first in `CSharpCraft.Tests/Game/Fixed32Tests.cs`
3. Make changes, verify tests pass
4. Update Pcraft physics to use new functionality

**Example Test:**
```csharp
[Fact]
public void Fixed32_Sqrt_Works()
{
    // Arrange
    var value = new Fixed32(4);
    
    // Act
    var result = Fixed32.Sqrt(value);
    
    // Assert - should be close to 2
    Assert.Equal(new Fixed32(2), result);
}
```

### 🎮 Updating Game Logic

**File Locations:**
- Game engine: `CSharpCraft.Game/Pcraft/PcraftBase.cs` (2,259 lines - needs refactoring!)
- Entity types: `CSharpCraft.Game/Pcraft/Entity.cs`
- Competitive: `CSharpCraft.Game/Competitive/CompetitiveScene.cs`

**Before Making Changes:**
1. Check if there are existing tests
2. If not, add tests first (Test-Driven Development)
3. Mock dependencies using Moq
4. Make changes incrementally
5. Verify tests still pass

### 🎵 Working with Audio/Graphics

**For Graphics:**
- Old way: `p8.Batch.Draw()`, `p8.Circ()` etc.
- New way (upcoming): `IGraphicsEngine` interface methods
- Current: Both still work, Pico8Functions delegates

**For Audio:**
- Old way: `p8.Music()`, `p8.Sfx()` 
- New way (upcoming): `IAudioManager` interface
- Current: Both still work

**Migration example:**
```csharp
// Old code - directly on Pico8Functions
p8.Cls(0);
p8.Circ(64, 64, 10, 3);

// New code - will use IGraphicsEngine
graphics.Cls(0);
graphics.Circle(new F32(64), new F32(64), 10, 3);
```

### ✅ Adding Tests

**Folder structure matches game structure:**
- `CSharpCraft.Tests/Game/` - mirrors `CSharpCraft.Game/`
- `CSharpCraft.Tests/Pico8/` - mirrors `CSharpCraft.Pico8/`

**Test naming convention:**
- Class: `[ComponentName]Tests.cs`
- Method: `[ComponentName]_[Scenario]_[ExpectedOutcome]()`

**Examples:**
```csharp
public class PcraftPhysicsTests
{
    [Fact]
    public void PhysicsEngine_CheckCollision_ReturnsTrueWhenColliding()
    {
        // Test collision detection
    }
    
    [Fact]
    public void PhysicsEngine_ApplyForce_UpdatesVelocity()
    {
        // Test force application
    }
}
```

## Using Mocks in Tests

### Mock the Graphics Engine

```csharp
var graphicsMock = new Mock<IGraphicsEngine>();

// Setup return values
graphicsMock.SetupGet(x => x.Cell).Returns((8, 8));
graphicsMock.SetupGet(x => x.Resolution).Returns((128, 128));

// Verify calls
graphicsMock.Verify(x => x.Cls(0), Times.Once);
graphicsMock.Verify(x => x.Print(It.IsAny<string>(), It.IsAny<F32>(), It.IsAny<F32>(), It.IsAny<int>()), Times.AtLeastOnce);
```

### Mock the Input Manager

```csharp
var inputMock = new Mock<IInputManager>();

// Setup button presses
inputMock.Setup(x => x.Btn(0)).Returns(true);   // Up
inputMock.Setup(x => x.Btn(1)).Returns(false);  // Down
inputMock.Setup(x => x.Btnp(4)).Returns(true);  // Confirm pressed

// Use in test
var wasConfirmPressed = inputMock.Object.Btnp(4);
Assert.True(wasConfirmPressed);
```

### Mock the Scene Manager

```csharp
var sceneManagerMock = new Mock<ISceneManager>();

// Setup current scene
sceneManagerMock.SetupGet(x => x.CurrentScene).Returns(currentScene);

// Verify transitions  
sceneManagerMock.Verify(x => x.ScheduleScene(It.IsAny<Func<IScene>>()), Times.Once);
```

## Compilation & Running

### Build Everything
```bash
dotnet build CSharpCraft.slnx -p:Platform=x64
```

### Build Specific Project
```bash
dotnet build CSharpCraft.Game/CSharpCraft.Game.csproj
```

### Run the Game
```bash
cd CSharpCraft
dotnet run -p:Platform=x64
```

### Run Tests (Individual)
```bash
# Run specific test class
dotnet test --filter "ClassName"

# Run specific test method
dotnet test --filter "ClassName.MethodName"

# Run with verbose output
dotnet test -v d
```

### Run All Tests
```bash
dotnet test CSharpCraft.Tests/
```

## Common Scenarios

### "I need to add a new button to the input system"

1. Update `P8Btns` class in `CSharpCraft.Pico8/Pico8Classes.cs`
2. Add button constant (e.g., `public bool Button7 { get; set; }`)
3. Update `Reset()` method in Pico8Functions to initialize new button
4. Add test in `CSharpCraft.Tests/Pico8/InputManagerTests.cs`

### "I want to test the Pcraft physics independently"

1. Create `CSharpCraft.Tests/Game/PcraftPhysicsTests.cs`
2. Mock `IGraphicsEngine` (physics doesn't need rendering for testing)
3. Extract physics methods into mockable methods
4. Unit test each physics function separately
5. Run: `dotnet test --filter "PcraftPhysicsTests"`

### "I'm refactoring a giant method"

1. **Before touching code:** Write tests for current behavior
2. **During refactoring:** Move code into smaller methods
3. **Add tests:** For each new method
4. **Run tests:** After each change
5. **Verify:** All tests still pass at the end

### "Something broke and I don't know what"

```bash
# 1. Try clean rebuild
dotnet clean && dotnet build -p:Platform=x64

# 2. Check which project has errors
dotnet build CSharpCraft.slnx --verbosity detailed

# 3. Look at specific project errors
dotnet build CSharpCraft.Game/CSharpCraft.Game.csproj

# 4. Check test results for clues
dotnet test CSharpCraft.Tests/ --logger "console;verbosity=detailed"
```

## Best Practices

### ✅ Do This:
- Write tests before major refactoring
- Use interfaces for dependencies
- Keep classes small and focused
- Document public methods
- Use meaningful variable names
- Add comments for complex logic
- Commit small, logical changes
- Review your own code before pushing

### ❌ Don't Do This:
- Edit code without tests for critical areas
- Use static methods for business logic
- Create god classes (>500 lines)
- Ignore compiler warnings
- Mix concerns (rendering + physics + input in one class)
- Commit debugging code
- Make massive commits with unrelated changes

## IDE Setup (VS Code)

### Essential Extensions
1. **C# Dev Kit** (Microsoft) - Language support
2. **Test Explorer** - Run tests from UI
3. **.NET Core Tools** - Project templates

### Launch Configuration
Add to `.vscode/launch.json`:
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Launch Game",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/CSharpCraft/bin/Debug/net10.0/CSharpCraft.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "console": "internalConsole"
        }
    ]
}
```

### Build Task
Add to `.vscode/tasks.json`:
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": ["build", "--project", "${workspaceFolder}/CSharpCraft.slnx", "-p:Platform=x64"],
            "problemMatcher": "$msCompile"
        }
    ]
}
```

## Getting Help

### Documentation
- **Architecture**: See [ARCHITECTURE.md](ARCHITECTURE.md)
- **Project README**: In each project folder
- **Code Comments**: Read existing implementations

### Community
- Check git history for similar changes
- Look at existing tests for examples
- Review pull request discussions
- Ask in team channels

## Next Steps (For Maintainers)

Short-term refactoring goals:
1. Extract Pcraft physics into `IPcraftPhysicsEngine`
2. Implement service interfaces with actual classes
3. Update `IScene.Init()` to use dependency injection
4. Add more unit tests (target 70%+ coverage)
5. Set up CI/CD pipeline for automated testing

Medium-term goals:
1. Complete DI setup in Main.cs
2. Decouple static singletons (AccountHandler, RoomHandler)
3. Add integration tests for complete scenes
4. Implement service adapter layer for backend

Long-term vision:
1. Modular scene system (plugins)
2. Hot-reload for rapid iteration
3. Visual development tools
4. Performance profiling infrastructure
5. Multi-platform support (web, mobile)

---

**Last Updated**: 16 February 2026
**Architecture Version**: 1.0 (Modular)

Questions? Check the project README or ARCHITECTURE.md!
