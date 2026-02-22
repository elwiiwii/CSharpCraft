# PLAN D: Complete Architecture Redesign
## Implementation Roadmap with Strict TDD & Fluent Assertions

**Decision Date:** February 22, 2026  
**Approach:** Option D (Complete Redesign - API-First)  
**Duration:** 12-16 hours (6 phases, intensive push)  
**TDD Discipline:** Strict RED-GREEN-REFACTOR for every component  
**Test Framework:** xUnit + Moq + FluentAssertions EXCLUSIVELY  

---

## Executive Summary

### The Problem
Pico8Functions is a "god object" with two conflicting responsibilities:
1. **PICO-8 API Façade** - Should be simple, flat, stateless
2. **Game Orchestrator** - Must be complex, managing state and coordination

Result: SRP plateau at 4/5, cannot improve further with extraction alone.

### The Solution
Separate these responsibilities into completely independent classes:
- **Pico8** (200 LOC): Pure static API, delegates to GameOrchestrator
- **GameOrchestrator** (400 LOC): Game loop and state management

### The Outcome
- **SOLID Improvement:** 4.1 → 4.6/5 (+12%)
- **SRP Achievement:** 4/5 → 5/5 for both classes
- **Tests:** 110+ tests passing, all using FluentAssertions
- **Effort:** 12-16 hours (single intensive push)
- **Risk:** HIGH (big bang approach, but worth it for ambition)

---

## Six Implementation Phases

### Phase 1: Architecture Design + TDD Tests (2.5 hours)

**Goal:** Define Pico8 API contract through tests (RED-GREEN-REFACTOR)

#### Step 1.1: Create Pico8StaticAPITests.cs (RED)

Write comprehensive failing tests first - these define the API contract:

```csharp
[TestClass]
public class Pico8StaticAPITests
{
    private Mock<IInputStateManager> _mockInput;
    private Mock<IGraphicsAPI> _mockGraphics;
    private Mock<IAudioAPI> _mockAudio;
    private GameOrchestrator _gameOrchestrator;
    
    [TestInitialize]
    public void Setup()
    {
        _mockInput = new Mock<IInputStateManager>();
        _mockGraphics = new Mock<IGraphicsAPI>();
        _mockAudio = new Mock<IAudioAPI>();
        _gameOrchestrator = new GameOrchestrator(_mockInput.Object, _mockGraphics.Object, _mockAudio.Object);
        Pico8.Initialize(_gameOrchestrator);
    }
    
    // INPUT TESTS
    [TestMethod]
    public void Btn_ReturnsTrue_WhenButtonPressed()
    {
        _mockInput.Setup(i => i.Btn(4, 0)).Returns(true);
        
        var result = Pico8.Btn(4);
        
        result.Should().BeTrue("button 4 was pressed");
        _mockInput.Verify(i => i.Btn(4, 0), Times.Once(), 
            "Btn should query InputStateManager exactly once");
    }
    
    [TestMethod]
    public void Btn_WithPlayer_DelegatesCorrectly()
    {
        _mockInput.Setup(i => i.Btn(2, 1)).Returns(false);
        
        var result = Pico8.Btn(2, 1);
        
        result.Should().BeFalse();
        _mockInput.Verify(i => i.Btn(2, 1), Times.Once());
    }
    
    [TestMethod]
    public void Btnp_ReturnsTrue_WhenButtonPressedThisFrame()
    {
        _mockInput.Setup(i => i.Btnp(3, 0)).Returns(true);
        
        var result = Pico8.Btnp(3);
        
        result.Should().BeTrue("button was pressed this frame");
    }
    
    // GRAPHICS TESTS
    [TestMethod]
    public void Cls_Clears_WithDefaultBlackColor()
    {
        Pico8.Cls();
        
        _mockGraphics.Verify(g => g.Cls(It.IsAny<int>()), Times.Once(), 
            "Cls should clear screen");
    }
    
    [TestMethod]
    public void Circ_DrawsCircle_WithCorrectParameters()
    {
        Pico8.Circ(64, 64, 8, 3);
        
        _mockGraphics.Verify(
            g => g.Circle(64, 64, 8, It.IsAny<Color>()),
            Times.Once(),
            "Circ should draw circle at (64,64) with radius 8");
    }
    
    [TestMethod]
    public void Circfill_DrawsFilledCircle()
    {
        Pico8.Circfill(50, 50, 5, 7);
        
        _mockGraphics.Verify(
            g => g.CircleFilled(50, 50, 5, It.IsAny<Color>()),
            Times.Once());
    }
    
    [TestMethod]
    public void Print_OutputsText_WithCorrectCoordinates()
    {
        Pico8.Print("Hello", 10, 20, 1);
        
        _mockGraphics.Verify(
            g => g.Print("Hello", 10, 20, It.IsAny<Color>()),
            Times.Once(),
            "Print should output text at (10,20)");
    }
    
    [TestMethod]
    public void Pal_ChangesColorMapping()
    {
        Pico8.Pal(1, 2);
        
        _mockGraphics.Verify(
            g => g.Pal(1, 2),
            Times.Once(),
            "Pal should map color 1 to color 2");
    }
    
    [TestMethod]
    public void Palt_SetsColorTransparency()
    {
        Pico8.Palt(0, true);
        
        _mockGraphics.Verify(
            g => g.Palt(0, true),
            Times.Once());
    }
    
    [TestMethod]
    public void Spr_DrawsSprite()
    {
        Pico8.Spr(3, 32, 64);
        
        _mockGraphics.Verify(
            g => g.Sprite(3, 32, 64, false, false),
            Times.Once());
    }
    
    [TestMethod]
    public void Map_DrawsMap_WithCorrectRegion()
    {
        Pico8.Map(0, 0, 0, 0, 16, 16);
        
        _mockGraphics.Verify(
            g => g.Map(0, 0, 0, 0, 16, 16),
            Times.Once());
    }
    
    [TestMethod]
    public void Camera_SetsCameraOffset()
    {
        Pico8.Camera(8, 8);
        
        _mockGraphics.Verify(
            g => g.Camera(8, 8),
            Times.Once(),
            "Camera should set view offset");
    }
    
    // AUDIO TESTS
    [TestMethod]
    public void Sfx_PlaysSound_WithCorrectChannel()
    {
        Pico8.Sfx(2, 0);
        
        _mockAudio.Verify(
            a => a.PlaySfx(2, 0),
            Times.Once(),
            "Sfx should play sound 2 on channel 0");
    }
    
    [TestMethod]
    public void Music_PlaysMusic_WithFadeOption()
    {
        Pico8.Music(1, 1000);
        
        _mockAudio.Verify(
            a => a.PlayMusic(1, 1000),
            Times.Once());
    }
    
    [TestMethod]
    public void Mute_MutesAudio()
    {
        Pico8.Mute(true);
        
        _mockAudio.Verify(
            a => a.SetMuted(true),
            Times.Once(),
            "Mute should set audio muted state");
    }
    
    // MATH TESTS
    [TestMethod]
    public void Cos_ReturnsCorrectValue()
    {
        var result = Pico8.Cos(0);
        
        result.Should().Be(1.0f, "Cos(0) should equal 1.0");
    }
    
    [TestMethod]
    public void Sin_ReturnsCorrectValue()
    {
        var result = Pico8.Sin(0.25f);
        
        result.Should().BeApproximately(MathF.Sin(MathF.PI / 2), 0.0001f);
    }
    
    [TestMethod]
    public void Rnd_ReturnsValueBetween0And1()
    {
        var result = Pico8.Rnd(1);
        
        result.Should().BeGreaterThanOrEqualTo(0)
            .And.BeLessThan(1)
            .And.NotBeNull("Rnd should return value in [0, 1)");
    }
    
    [TestMethod]
    public void Mid_ReturnsMiddleValue()
    {
        var result = Pico8.Mid(5, 15, 10);
        
        result.Should().Be(10, "Mid(5, 15, 10) should clamp 10 between 5 and 15");
    }
    
    [TestMethod]
    public void Flr_FloorsFractionalValue()
    {
        var result = Pico8.Flr(3.7f);
        
        result.Should().Be(3f, "Flr(3.7) should return 3");
    }
}
```

**Test Count:** 20+ comprehensive tests covering:
- Input (Btn, Btnp)
- Graphics (Cls, Circ, Circfill, Print, Pal, Palt, Spr, Map, Camera)
- Audio (Sfx, Music, Mute)
- Math (Cos, Sin, Rnd, Mid, Flr)

**All tests use FluentAssertions:**
- `.Should().BeTrue("reason")`
- `.Should().Be(expected)`
- `.Should().Verify(Times.Once())`

**Status:** RED - All tests fail (Pico8 class doesn't exist yet)

#### Step 1.2: Implement Pico8.cs (GREEN)

Create the static Pico8 class with minimal implementations:

```csharp
public static class Pico8
{
    private static GameOrchestrator _gameOrchestrator;
    
    public static void Initialize(GameOrchestrator orchestrator)
    {
        _gameOrchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }
    
    // INPUT API
    public static bool Btn(int button, int player = 0) => 
        _gameOrchestrator.InputManager.Btn(button, player);
    
    public static bool Btnp(int button, int player = 0) => 
        _gameOrchestrator.InputManager.Btnp(button, player);
    
    // GRAPHICS API
    public static void Cls(int color = 0) => 
        _gameOrchestrator.Graphics.Cls(color);
    
    public static void Circ(int x, int y, int radius, int color) => 
        _gameOrchestrator.Graphics.Circle(x, y, radius, GetColor(color));
    
    public static void Circfill(int x, int y, int radius, int color) => 
        _gameOrchestrator.Graphics.CircleFilled(x, y, radius, GetColor(color));
    
    public static void Print(string text, int x, int y, int color) => 
        _gameOrchestrator.Graphics.Print(text, x, y, GetColor(color));
    
    public static void Pal(int c0, int c1) => 
        _gameOrchestrator.Graphics.Pal(c0, c1);
    
    public static void Palt(int c, bool transparent) => 
        _gameOrchestrator.Graphics.Palt(c, transparent);
    
    public static void Spr(int n, int x, int y, bool flipX = false, bool flipY = false) => 
        _gameOrchestrator.Graphics.Sprite(n, x, y, flipX, flipY);
    
    public static void Map(int cellX, int cellY, int screenX, int screenY, int width, int height) => 
        _gameOrchestrator.Graphics.Map(cellX, cellY, screenX, screenY, width, height);
    
    public static void Camera(int x, int y) => 
        _gameOrchestrator.Graphics.Camera(x, y);
    
    // AUDIO API
    public static void Sfx(int n, int channel = 0) => 
        _gameOrchestrator.Audio.PlaySfx(n, channel);
    
    public static void Music(int n, int fadeout = 0) => 
        _gameOrchestrator.Audio.PlayMusic(n, fadeout);
    
    public static void Mute(bool muted) => 
        _gameOrchestrator.Audio.SetMuted(muted);
    
    // MATH API
    public static float Cos(float x) => MathF.Cos(x * MathF.PI * 2);
    
    public static float Sin(float x) => MathF.Sin(x * MathF.PI * 2);
    
    public static float Rnd(float x) => (float)Random.Shared.NextDouble() * x;
    
    public static int Mid(int x, int y, int z) => Math.Max(Math.Min(z, Math.Max(x, y)), Math.Min(x, y));
    
    public static float Flr(float x) => MathF.Floor(x);
    
    private static Color GetColor(int paletteIndex) => 
        _gameOrchestrator.PaletteManager.GetColor(paletteIndex);
}
```

**Key Characteristics:**
- 35+ public static methods
- Each method is 1-2 lines (pure delegation)
- Zero orchestration logic
- Zero state management
- All tests pass (GREEN)

#### Step 1.3: Run Tests (VERIFY GREEN)

```bash
dotnet test CSharpCraft.Tests/Pico8/Pico8StaticAPITests.cs -v detailed
# Expected: 20+ tests passing ✓
```

---

### Phase 2: GameOrchestrator Tests & Implementation (2.5 hours)

**Goal:** Define orchestration behavior through tests, implement GameOrchestrator

#### Step 2.1: Create GameOrchestratorTests.cs (RED)

```csharp
[TestClass]
public class GameOrchestratorTests
{
    private GameOrchestrator _orchestrator;
    private Mock<IGraphicsAPI> _mockGraphics;
    private Mock<IAudioAPI> _mockAudio;
    private Mock<IInputStateManager> _mockInput;
    private Mock<ISceneManager> _mockSceneManager;
    
    [TestInitialize]
    public void Setup()
    {
        _mockGraphics = new Mock<IGraphicsAPI>();
        _mockAudio = new Mock<IAudioAPI>();
        _mockInput = new Mock<IInputStateManager>();
        _mockSceneManager = new Mock<ISceneManager>();
        
        _orchestrator = new GameOrchestrator(
            _mockInput.Object,
            _mockGraphics.Object,
            _mockAudio.Object,
            _mockSceneManager.Object
        );
    }
    
    // INITIALIZATION TESTS
    [TestMethod]
    public void Initialize_RegistersPico8Static()
    {
        _orchestrator.Initialize();
        
        // Verify that Pico8.Initialize was called (static reference set)
        Pico8.InputManager.Should().NotBeNull();
    }
    
    [TestMethod]
    public void Initialize_CreatesGameState()
    {
        _orchestrator.Initialize();
        
        _orchestrator.GameState.Should().NotBeNull();
        _orchestrator.GameState.IsPaused.Should().BeFalse();
    }
    
    // SCENE MANAGEMENT TESTS
    [TestMethod]
    public void LoadScene_TransitionsToNewScene()
    {
        _orchestrator.Initialize();
        var mockScene = new Mock<IScene>();
        
        _orchestrator.LoadScene(mockScene.Object);
        
        mockScene.Verify(s => s.Init(), Times.Once(), 
            "LoadScene should initialize the new scene");
    }
    
    [TestMethod]
    public void CurrentScene_ReturnsLoadedScene()
    {
        _orchestrator.Initialize();
        var mockScene = new Mock<IScene>();
        _orchestrator.LoadScene(mockScene.Object);
        
        _orchestrator.CurrentScene.Should().Be(mockScene.Object);
    }
    
    // PAUSE STATE TESTS
    [TestMethod]
    public void Pause_SetsPausedState()
    {
        _orchestrator.Initialize();
        
        _orchestrator.Pause();
        
        _orchestrator.GameState.IsPaused.Should().BeTrue(
            "GameState should indicate paused status");
    }
    
    [TestMethod]
    public void Resume_ClearsPausedState()
    {
        _orchestrator.Initialize();
        _orchestrator.Pause();
        
        _orchestrator.Resume();
        
        _orchestrator.GameState.IsPaused.Should().BeFalse();
    }
    
    [TestMethod]
    public void Update_DoesNotCallScene_WhenPaused()
    {
        _orchestrator.Initialize();
        var mockScene = new Mock<IScene>();
        _orchestrator.LoadScene(mockScene.Object);
        _orchestrator.Pause();
        
        _orchestrator.Update();
        
        mockScene.Verify(s => s.Update(), Times.Never(),
            "Update should not call scene.Update() when paused");
    }
    
    [TestMethod]
    public void Update_CallsScene_WhenNotPaused()
    {
        _orchestrator.Initialize();
        var mockScene = new Mock<IScene>();
        _orchestrator.LoadScene(mockScene.Object);
        
        _orchestrator.Update();
        
        mockScene.Verify(s => s.Update(), Times.Once(),
            "Update should call scene.Update() when not paused");
    }
    
    // DRAW TESTS
    [TestMethod]
    public void Draw_ClearsScreen()
    {
        _orchestrator.Initialize();
        var mockScene = new Mock<IScene>();
        _orchestrator.LoadScene(mockScene.Object);
        
        _orchestrator.Draw();
        
        _mockGraphics.Verify(g => g.Cls(It.IsAny<int>()), Times.Once(),
            "Draw should clear screen before drawing");
    }
    
    [TestMethod]
    public void Draw_DrawsCurrentScene()
    {
        _orchestrator.Initialize();
        var mockScene = new Mock<IScene>();
        _orchestrator.LoadScene(mockScene.Object);
        
        _orchestrator.Draw();
        
        mockScene.Verify(s => s.Draw(), Times.Once(),
            "Draw should render current scene");
    }
    
    [TestMethod]
    public void Draw_DrawsPauseMenu_WhenPaused()
    {
        _orchestrator.Initialize();
        var mockScene = new Mock<IScene>();
        _orchestrator.LoadScene(mockScene.Object);
        _orchestrator.Pause();
        
        _orchestrator.Draw();
        
        // Verify pause menu was drawn (overlay)
        mockScene.Verify(s => s.Draw(), Times.Once());
        // Pause UI should be drawn on top
    }
}
```

**Test Count:** 15+ tests covering:
- Initialization
- Scene management
- Pause/resume states
- Update logic
- Draw logic

#### Step 2.2: Implement GameOrchestrator (GREEN)

```csharp
public class GameOrchestrator
{
    private readonly IInputStateManager _inputManager;
    private readonly IGraphicsAPI _graphics;
    private readonly IAudioAPI _audio;
    private readonly ISceneManager _sceneManager;
    private readonly IGameState _gameState;
    
    private IScene _currentScene;
    private bool _initialized;

    public IInputStateManager InputManager => _inputManager;
    public IGraphicsAPI Graphics => _graphics;
    public IAudioAPI Audio => _audio;
    public IGameState GameState => _gameState;
    public IScene CurrentScene => _currentScene;

    public GameOrchestrator(
        IInputStateManager inputManager,
        IGraphicsAPI graphics,
        IAudioAPI audio,
        ISceneManager sceneManager)
    {
        _inputManager = inputManager ?? throw new ArgumentNullException(nameof(inputManager));
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _audio = audio ?? throw new ArgumentNullException(nameof(audio));
        _sceneManager = sceneManager ?? throw new ArgumentNullException(nameof(sceneManager));
        _gameState = new GameStateContainer();
    }

    public void Initialize()
    {
        Pico8.Initialize(this);
        _initialized = true;
    }

    public void LoadScene(IScene scene)
    {
        scene.Init();
        _currentScene = scene;
    }

    public void Pause()
    {
        _gameState.IsPaused = true;
    }

    public void Resume()
    {
        _gameState.IsPaused = false;
    }

    public void Update()
    {
        if (!_initialized)
            throw new InvalidOperationException("GameOrchestrator must be initialized before Update");

        if (_gameState.IsPaused)
            return;

        _currentScene?.Update();
    }

    public void Draw()
    {
        if (!_initialized)
            throw new InvalidOperationException("GameOrchestrator must be initialized before Draw");

        _graphics.Cls(0);
        _currentScene?.Draw();
        
        if (_gameState.IsPaused)
        {
            DrawPauseMenu();
        }
    }

    private void DrawPauseMenu()
    {
        // Pause UI overlay - implement in Phase 3
    }
}
```

**Key Characteristics:**
- 400 LOC (with comments)
- Pure orchestration (no API methods)
- Manages scene lifecycle
- Coordinates pause state
- All tests pass (GREEN)

#### Step 2.3: Run Tests

```bash
dotnet test CSharpCraft.Tests/GameOrchestrator/ -v detailed
# Expected: 110+ tests passing (Phase 1 + Phase 2) ✓
```

---

### Phase 3: Orchestrator Classes (2 hours)

**Goal:** Encapsulate graphics and audio operations into sub-orchestrators

#### Overview

Create specialized orchestrator classes for graphics and audio operations:

```csharp
// GraphicsOrchestrator - Encapsulates all graphics operations
public class GraphicsOrchestrator
{
    private readonly IGraphicsAPI _graphics;
    private int _cameraX;
    private int _cameraY;
    
    public GraphicsOrchestrator(IGraphicsAPI graphics)
    {
        _graphics = graphics;
    }
    
    public void DrawMap(int cellX, int cellY, int screenX, int screenY, int w, int h)
        => _graphics.Map(cellX, cellY, screenX, screenY, w, h);
    
    public void SetCamera(int x, int y)
    {
        _cameraX = x;
        _cameraY = y;
        _graphics.Camera(x, y);
    }
    
    // ... additional graphics coordination
}

// AudioOrchestrator - Encapsulates audio operations
public class AudioOrchestrator
{
    private readonly IAudioAPI _audio;
    private int _currentMusicTrack;
    
    public AudioOrchestrator(IAudioAPI audio)
    {
        _audio = audio;
    }
    
    public void PlayMusic(int trackId, int fadeMs = 0)
    {
        _currentMusicTrack = trackId;
        _audio.PlayMusic(trackId, fadeMs);
    }
    
    // ... additional audio coordination
}
```

**Tests:** Create tests for each orchestrator class (10+ tests)

---

### Phase 4: Service Factory & Main.cs Update (1.5 hours)

**Goal:** Integrate GameOrchestrator into ServiceFactory and update Main.cs

```csharp
// In ServiceFactory
public GameOrchestrator CreateGameOrchestrator()
{
    var inputManager = CreateInputManager();
    var graphics = CreateGraphicsAPI();
    var audio = CreateAudioAPI();
    var sceneManager = CreateSceneManager();
    
    return new GameOrchestrator(inputManager, graphics, audio, sceneManager);
}

// In Main.cs
private GameOrchestrator _gameOrchestrator;

void Initialize()
{
    _gameOrchestrator = ServiceFactory.CreateGameOrchestrator();
    _gameOrchestrator.Initialize();
    
    var titleScreen = new TitleScreen();
    _gameOrchestrator.LoadScene(titleScreen);
}

void Update(GameTime gameTime)
{
    _gameOrchestrator.Update();
}

void Draw(GameTime gameTime)
{
    _gameOrchestrator.Draw();
}
```

---

### Phase 5: Scene Migration (2 hours)

**Goal:** Update all scenes to use static Pico8 API instead of Pico8Functions

#### Before:
```csharp
public class MyScene : IScene
{
    private Pico8Functions _pico8;
    
    public void Init(Pico8Functions pico8)
    {
        _pico8 = pico8;
    }
    
    public void Draw()
    {
        _pico8.Circ(64, 64, 8, 3);
    }
}
```

#### After:
```csharp
public class MyScene : IScene
{
    public void Init()
    {
        // No longer needs Pico8Functions injection
    }
    
    public void Draw()
    {
        Pico8.Circ(64, 64, 8, 3);  // Static API
    }
}
```

**Changes Needed:**
- Remove `Pico8Functions` parameter from all scene `Init()` methods
- Replace all `_pico8.Method()` calls with `Pico8.Method()`
- Update IScene interface to remove parameter
- Update all scene loading code

---

### Phase 6: Cleanup & Final Testing (1.5 hours)

**Goal:** Mark Pico8Functions as obsolete, finalize documentation, run full test suite

```csharp
[Obsolete("Use static Pico8 class instead. Pico8Functions will be removed in Phase 7.", false)]
public class Pico8Functions
{
    // ... existing code
}
```

**Documentation Updates:**
- Update ARCHITECTURE.md with new design
- Add Pico8 API reference guide
- Document GameOrchestrator pattern
- Update DEVELOPER_GUIDE.md with new workflow

**Final Verification:**
```bash
dotnet test  # All tests passing
dotnet build # No warnings (except obsolete)
git diff     # Review all changes
```

---

## TDD Discipline Requirements

### Mandatory RED-GREEN-REFACTOR Cycle

**For EVERY component:**

1. **RED Phase:**
   - Write failing test first
   - Test should compile
   - Test should fail (obvious failure)
   - Expected: `0/X tests passing`

2. **GREEN Phase:**
   - Write MINIMAL code to make test pass
   - No over-engineering
   - No extra features
   - No speculation
   - Expected: `X/X tests passing`

3. **REFACTOR Phase:**
   - Improve code while tests stay green
   - Extract methods
   - Improve naming
   - Remove duplication
   - Expected: `X/X tests still passing`

### FluentAssertions Usage (100% REQUIRED)

**Mandatory Format:**
```csharp
// ✅ REQUIRED
result.Should().BeTrue("because user clicked button");
actual.Should().Be(expected);
collection.Should().HaveCount(5);

// ❌ FORBIDDEN
Assert.IsTrue(result);
Assert.AreEqual(expected, actual);
Assert.AreEqual(5, collection.Count);
```

**Chain Assertions:**
```csharp
_mockInput
    .Verify(i => i.Btn(4, 0), Times.Once())
    .And.Verify(i => i.Btnp(4, 0), Times.Never());

result
    .Should()
    .BeTrue("verification context")
    .And.Be(expectedValue);
```

### Test Organization

```csharp
[TestClass]
public class ComponentTests
{
    private Mock<IDependency> _mockDependency;
    private Component _component;
    
    [TestInitialize]
    public void Setup()
    {
        _mockDependency = new Mock<IDependency>();
        _component = new Component(_mockDependency.Object);
    }
    
    // SECTION 1: INPUT TESTS
    [TestMethod]
    public void Input_Behavior_ProducesResult()
    {
        // Arrange
        var input = "value";
        
        // Act
        var result = _component.Process(input);
        
        // Assert
        result.Should().Be(expected, "reason for expectation");
    }
    
    // SECTION 2: STATE TESTS
    [TestMethod]
    public void State_Change_UpdatesProperty()
    {
        // Arrange-Act-Assert pattern strictly
    }
    
    // SECTION 3: INTERACTION TESTS
    [TestMethod]
    public void Dependency_IsInvoked_Correctly()
    {
        _mockDependency.Verify(...);
    }
}
```

---

## Success Criteria

### Test Success
- ✅ 115+ tests passing (110 existing + 5+ new)
- ✅ 100% using FluentAssertions
- ✅ Tests fail in RED phase
- ✅ Tests pass in GREEN phase
- ✅ Tests pass in REFACTOR phase

### Code Success
- ✅ Pico8: 200 LOC (pure API)
- ✅ GameOrchestrator: 400 LOC (pure orchestration)
- ✅ No classes with dual responsibility
- ✅ SRP 5/5 for both main classes

### Architecture Success
- ✅ SOLID: 4.1 → 4.6/5
- ✅ SRP: 4/5 → 5/5
- ✅ DIP: 5/5 → 5/5 (maintained)
- ✅ OCP: 3.5/5 → 4/5
- ✅ LSP: 4/5 → 4/5 (maintained)
- ✅ ISP: 4/5 → 4.5/5

### Production Success
- ✅ Game compiles without errors
- ✅ Game runs with all features working
- ✅ No breaking changes to player experience
- ✅ Performance equivalent to Phase 9

---

## Risk Mitigation

### High-Risk Points & Solutions

| Risk | Mitigation |
|------|-----------|
| Tests break during refactor | Commit after each phase, run full suite |
| Scenes need API changes | Automated refactoring with regex |
| Static API breaks DI tests | Mock Pico8.Initialize() in tests |
| Manager coordination issues | Detailed integration tests in Phase 3 |
| Game doesn't run | Integration tests + manual verification |

### Rollback Plan

If critical issues arise:
1. Commit frequently (after each phase)
2. Can rollback to last phase's commit
3. Can abandon at any phase without total loss
4. Version control is continuous safety net

---

## Schedule

| Phase | Task | Duration | Cumulative |
|-------|------|----------|-----------|
| 1 | API Design + TDD | 2.5h | 2.5h |
| 2 | GameOrchestrator | 2.5h | 5h |
| 3 | Sub-orchestrators | 2h | 7h |
| 4 | Factory + Main | 1.5h | 8.5h |
| 5 | Scene Migration | 2h | 10.5h |
| 6 | Cleanup + Testing | 1.5h | 12h |

**Total Effort:** 10-12 hours for core implementation
**Buffer:** 2-4 hours for debugging/fixes
**Grand Total:** 12-16 hours

---

## Continuation Checkpoint

After Phase 6 completion:
- Run full test suite: `dotnet test`
- Verify game runs: `dotnet run`
- Review SOLID metrics
- Decision: Continue to Phase 7 (further improvements) or stabilize

---

## Appendix: API Contract Specification

List of all 35+ methods that Pico8 static class must support:

### Input (2)
- `Btn(button, player=0)`
- `Btnp(button, player=0)`

### Graphics (12)
- `Cls(color=0)`
- `Circ(x, y, radius, color)`
- `Circfill(x, y, radius, color)`
- `Rect(x, y, w, h, color)`
- `Rectfill(x, y, w, h, color)`
- `Print(text, x, y, color)`
- `Spr(n, x, y, flipX=false, flipY=false)`
- `Map(cellX, cellY, screenX, screenY, w, h)`
- `Camera(x, y)`
- `Pal(c0, c1)`
- `Palt(c, transparent)`
- `Pget(x, y)`

### Audio (3)
- `Sfx(n, channel=0)`
- `Music(n, fadeout=0)`
- `Mute(muted)`

### Math (8+)
- `Cos(x)`, `Sin(x)`
- `Rnd(max)`
- `Min(x, y)`, `Max(x, y)`, `Mid(x, y, z)`
- `Flr(x)`, `Ceil(x)`
- `Abs(x)`, `Sgn(x)`

### Data (3+)
- `Setmetatable(table, metatable)`
- `Add(table, value)`
- `Del(table, index)`

---

**Ready to begin Phase 1? Confirm to proceed with Pico8StaticAPITests.cs creation.**
