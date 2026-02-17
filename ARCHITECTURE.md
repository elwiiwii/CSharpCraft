# CSharpCraft - Modular Architecture Restructuring

## Overview

The CSharpCraft project has been successfully restructured into a modular, multi-project solution following SOLID principles and agile design patterns. This restructuring provides better separation of concerns, easier testing, and improved maintainability.

## Project Structure

```
CSharpCraft.slnx                    # Solution file
├── CSharpCraft.FixMath/            # Fixed-point math library (isolated math primitives)
│   ├── F32.cs, F64.cs              # 32/64-bit fixed point types
│   ├── Fixed32.cs, Fixed64.cs      # Alternative fixed arithmetic
│   └── FixedUtil.cs                # Utility functions
│
├── CSharpCraft.Pico8/              # Graphics/Audio/Input abstraction layer
│   ├── IGraphicsEngine.cs          # Graphics rendering interface
│   ├── IAudioManager.cs            # Audio playback interface
│   ├── IInputManager.cs            # Input handling interface
│   ├── ISceneManager.cs            # Scene lifecycle management
│   ├── IGameClock.cs               # Timing and FPS management
│   ├── Pico8Functions.cs           # Main engine coordinator
│   ├── Pico8Classes.cs             # P8Btns, MenuItem, audio structures
│   ├── Pico8Utils.cs               # Data conversion utilities
│   ├── Font.cs, SinDict.cs, CosDict.cs  # Rendering assets
│   └── IScene.cs                   # Scene abstraction (moved to Pico8 namespace)
│
├── CSharpCraft.Game/               # Game logic and scenes (WIP refactoring)
│   ├── Competitive/                # Competitive multiplayer scenes
│   │   ├── CompetitiveScene.cs
│   │   ├── JoinRoomScene.cs
│   │   ├── LobbyScene.cs
│   │   ├── LoginScene.cs
│   │   ├── PickBanScene.cs
│   │   ├── AccountHandler.cs       # To be converted to IAccountService
│   │   └── RoomHandler.cs          # To be converted to IRoomService
│   │
│   ├── Pcraft/                     # Game engine implementation (2259 lines)
│   │   ├── PcraftBase.cs           # Main game coordinator (to be split)
│   │   ├── Entity.cs, Material.cs, Ground.cs, Level.cs
│   │   ├── GenSeedCompetitive.cs, LoadSeed.cs
│   │   ├── PcraftCompetitive.cs
│   │   └── PcraftSingleplayer.cs
│   │
│   ├── OptionsMenu/                # Settings/options scenes
│   │   ├── GeneralOptions.cs
│   │   ├── ControlsOptions.cs
│   │   ├── KeyboardOptions.cs
│   │   └── OptionsFile.cs
│   │
│   ├── Credits/                    # Credits scene
│   │   ├── CreditsScene.cs
│   │   └── CreditsItem.cs
│   │
│   ├── FixMath/                    # Symlink to CSharpCraft.FixMath (during transition)
│   ├── Main.cs                     # Application entry point (to be refactored with DI)
│   ├── TitleScreen.cs              # Title/main menu scene
│   └── Content/                    # Game assets (graphics, audio, UI)
│       ├── Graphics/
│       ├── Music/
│       └── Sfx/
│
├── CSharpCraft/                    # Legacy executable project (to be deprecated)
│   └── [Original game executable code]
│
├── CSharpCraft.Tests/              # xUnit test project
│   ├── Game/                       # Game logic tests
│   │   ├── Fixed32Tests.cs         # Math tests
│   │   └── SceneInitializationTests.cs
│   ├── Pico8/                      # Pico8 service tests
│   │   └── InputManagerTests.cs
│   └── README.md
│
└── RaceServer/                     # Backend server (unchanged)
    ├── Services/
    ├── Protos/
    └── [...Firebase/gRPC services...]
```

## Key Architectural Changes

### 1. **Dependency Separation**

**Before:**
- Monolithic `CSharpCraft.csproj` containing all code
- FNA dependencies mixed with game logic
- Tight coupling between rendering, audio, and input
- No clear abstraction boundaries

**After:**
- `CSharpCraft.FixMath` - Pure math library (no dependencies)
- `CSharpCraft.Pico8` - Rendering/Audio/Input abstraction (only FNA dependency)
- `CSharpCraft.Game` - Game logic (depends on Pico8, uses FixMath)
- `CSharpCraft.Tests` - Tests (xUnit + Moq, mockable services)

### 2. **Service Abstraction Interfaces**

Created five core service interfaces to decouple rendering/input/audio:

```csharp
// Graphics operations abstraction
public interface IGraphicsEngine
{
    void Cls(int color);
    void DrawSprite(int spriteNum, F32 x, F32 y, ...);
    void DrawMap(double celx, double cely, ...);
    // ... 20+ drawing methods
}

// Audio management abstraction
public interface IAudioManager
{
    void PlayMusic(int trackId, double fadeMs);
    void PlaySfx(int sfxId, int channel);
    void Mute();
}

// Input handling abstraction
public interface IInputManager
{
    bool Btn(int i, int p);       // Button held
    bool Btnp(int i, int p);      // Button pressed
    P8Btns GetButtonState(int playerIndex);
}

// Scene lifecycle management
public interface ISceneManager
{
    IScene? CurrentScene { get; }
    void ScheduleScene(Func<IScene> sceneFactory);
    void ProcessPendingTransitions();
}

// Frame timing
public interface IGameClock
{
    double TargetFps { get; }
    double CurrentFps { get; }
    double DeltaTime { get; }
}
```

### 3. **Dependency Injection Foundation**

Added `Microsoft.Extensions.DependencyInjection` to all projects for future DI integration:

```csharp
// Planned usage pattern in Main.cs:
var services = new ServiceCollection();
services.AddSingleton<IGraphicsEngine>(graphicsEngine);
services.AddSingleton<IAudioManager>(audioManager);
services.AddSingleton<IInputManager>(inputManager);
services.AddSingleton<ISceneManager>(sceneManager);

var provider = services.BuildServiceProvider();
var scene = new TitleScreen();
scene.Init(
    provider.GetRequiredService<IGraphicsEngine>(),
    provider.GetRequiredService<IAudioManager>(),
    provider.GetRequiredService<IInputManager>(),
    ...
);
```

### 4. **Testability Improvements**

- Services are now mockable via Moq
- Fixed math tests can run without graphics dependencies
- Scene logic can be tested with mocked input/graphics
- No circular dependencies between projects

Example test pattern:
```csharp
[Fact]
public void Scene_Update_HandlesInput()
{
    // Arrange
    var graphicsMock = new Mock<IGraphicsEngine>();
    var inputMock = new Mock<IInputManager>();
    inputMock.Setup(x => x.Btn(0)).Returns(true);

    // Act - Scene would receive mocked services

    // Assert - Verify expected behavior
    inputMock.Verify(x => x.Btn(0), Times.Once);
}
```

## Project Dependencies

```
CSharpCraft.FixMath
  ↓
CSharpCraft.Pico8 → FNA.Core
  ↓
CSharpCraft.Game → CSharpCraft.Pico8, Nuget packages (Protobuf, gRPC, etc)
  ↓
CSharpCraft (executable) → CSharpCraft.Game
  ↓
CSharpCraft.Tests → CSharpCraft.Game, xUnit, Moq
  ↓
RaceServer (independent) → Nuget packages (Firebase, gRPC, etc)
```

**NO circular dependencies** - Clean dependency hierarchy!

## Refactoring Roadmap (Next Steps)

### Phase 5: Decouple & Consolidate ✅ (This phase)

1. ✅ Extract service interfaces from Pico8Functions god object
2. ✅ Create modular project structure
3. ✅ Set up test infrastructure
4. 🔲 *Next:* Implement adapter patterns for service interfaces
5. 🔲 Decouple static singletons (AccountHandler, RoomHandler)

### Phase 6: Service Implementation Refactoring

1. Split Pico8Functions (1,241 lines) into focused services:
   - `GraphicsEngine` implements `IGraphicsEngine`
   - `AudioManager` implements `IAudioManager`
   - `InputManager` implements `IInputManager`
   - `SceneManager` implements `ISceneManager`
   - `GameClock` implements `IGameClock`

2. Replace Pico8Functions constructor with injected services

3. Update IScene.Init() to accept service interfaces instead of Pico8Functions

### Phase 7: Backend Service Decoupling

1. Create `IAccountService` interface, implement gRPC wrapper
2. Create `IRoomService` interface, implement gRPC wrapper
3. Replace static `AccountHandler`/`RoomHandler` calls with DI
4. Move backend communication to standalone service layer

### Phase 8: Pcraft Monolith Decomposition

1. Extract physics engine → `IPcraftPhysicsEngine`
2. Extract rendering → `IPcraftRenderer` (uses IGraphicsEngine)
3. Extract input handling → `IPcraftInputHandler` (uses IInputManager)
4. Break inheritance hierarchy, use composition instead

### Phase 9: Integration Testing

1. End-to-end scene transition tests
2. Physics simulation tests
3. Multiplayer synchronization tests
4. Replay/validation tests

### Phase 10: Continuous Delivery

1. Set up GitHub Actions for automated testing
2. Code coverage reporting
3. Performance benchmarking
4. Release automation

## Benefits of This Architecture

✅ **Separation of Concerns** - Each project has a single responsibility
✅ **Testability** - All services are mockable, enabling isolated unit tests
✅ **Flexibility** - Easy to swap implementations (e.g., replace audio engine)
✅ **Maintainability** - Clear project boundaries and dependencies
✅ **Scalability** - New features can be added to specific projects
✅ **Reusability** - CSharpCraft.Pico8 could be used in other projects
✅ **Agile Development** - Easier to work on features in parallel
✅ **Code Quality** - Smaller, focused projects are easier to review

## Quick Start

### Build Solution
```bash
cd CSharpCraft
dotnet build CSharpCraft.slnx -p:Platform=x64
```

### Run Game
```bash
dotnet run --project CSharpCraft/CSharpCraft.csproj -p:Platform=x64
```

### Run Tests
```bash
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj
```

### Build for Release
```bash
dotnet publish CSharpCraft/CSharpCraft.csproj -c Release -p:Platform=x64
```

## File Changes Summary

- ✅ Created 4 new projects: FixMath, Pico8, Game, Tests
- ✅ Created 5 service interfaces: IGraphicsEngine, IAudioManager, IInputManager, ISceneManager, IGameClock
- ✅ Copied/reorganized ~100+ code files into modular structure
- ✅ Fixed nullability warnings in FixMath types
- ✅ Updated all project references and dependencies
- ✅ Added sample test files (3 test classes, 6 test methods)
- ✅ Solution compiles successfully with NO errors

## Migration Notes

### For Continuing Development

1. **Focus on Game Project First**
   - Most active development should happen here
   - Competitive scenes, Pcraft, OptionsMenu are ready to refactor

2. **Services Are Pluggable**
   - When ready, implement actual service classes from interfaces
   - Tests will pass immediately with new implementations

3. **Gradual Refactoring**
   - Don't need to refactor everything at once
   - Can leave Pico8Functions as-is while building new features
   - Extract services incrementally as you work on areas

4. **Testing First**
   - Write test before refactoring existing code
   - Mocks let you test game logic independently
   - Increases confidence in changes

5. **Keep RaceServer Unchanged**
   - Backend can continue independently
   - gRPC interface is stable
   - Net new service interfaces can wrap gRPC calls later

## Documentation and Standards

### Code Style Guide
- Use interfaces for all external dependencies
- Prefer composition over inheritance (see Pcraft refactoring plan)
- Keep classes under 500 lines when possible
- One responsibility per class

### Testing Standards  
- Arrange-Act-Assert pattern
- Descriptive test method names
- Mock all external dependencies
- Aim for 70%+ code coverage

### Git Workflow
- Use feature branches for major changes
- Require tests for new code
- Code review before merging
- Tag releases with version numbers

## Resources

- [SOLID Principles in C#](https://en.wikipedia.org/wiki/SOLID)
- [Dependency Injection Patterns](https://martinfowler.com/articles/injection.html)
- [xUnit Testing Framework](https://xunit.net/)
- [Moq Mocking Library](https://github.com/moq/moq4)
- [Agile Software Development](https://agilemanifesto.org/)

---

**Architecture Restructuring Completed**: 16 February 2026
**Next Steps**: Begin Phase 6 implementation of service interfaces
