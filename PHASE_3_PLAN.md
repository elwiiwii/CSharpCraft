# Phase 3.2 & 3.3 Implementation Plan

**Target Scope:** Full decomposition of Pico8Functions into service-oriented architecture  
**Estimated Timeline:** 3-4 hours total  
**Success Criteria:** Pico8Functions <150 lines + 18-22 integration tests passing

---

## Phase 3.2: Service Extraction & Orchestrator Refactoring

### Overview
Extract remaining Pico8 utilities and domain logic into 3 new services, then refactor Pico8Functions.cs to become a clean orchestrator that delegates to 7 services.

**Current State:**
- Pico8Functions.cs: **1,310 lines** (bloated god class)
- 4 core services: Already created and tested ✅
- 29 unit tests: All passing ✅

**Target State:**
- Pico8Functions.cs: **~120-150 lines** (pure orchestrator)
- 7 total services: 4 core + 3 utility
- 29 unit tests: Still all passing (backward compatible)

---

## Task 3.2.1: Create UtilityService

**File:** `CSharpCraft.Pico8/Services/UtilityService.cs`

**Purpose:** Centralize all mathematical and memory utilities from Pico8Functions

**Methods to Extract:**
```csharp
// Math utilities
public F32 Cos(F32 angle)      // Uses CosDict lookup
public F32 Sin(F32 angle)      // Uses SinDict lookup
public F32 Sqrt(F32 x)
public F32 Abs(F32 x)
public int Ceil(F32 x)
public int Floor(F32 x)

// Random
public F32 Random()
public F32 Random(F32 min, F32 max)

// Memory operations
public void Memcpy(int destAddr, int sourceAddr, int len)  // Memory copy
public void Memset(int addr, int length, int value)        // Memory fill

// Flag operations
public int Fget(int n)         // Get flag value
public void Fset(int n, int v) // Set flag value

// List operations
public void Del<T>(List<T> table, T value)  // Delete from list

// Type conversions
public F32 ToFixed(int value)
public int ToInt(F32 value)
```

**Dependencies:**
- CosDict, SinDict (for trigonometry)
- Random (for number generation)
- Flags array (for Fget/Fset)

**Size Estimate:** 250-300 lines

**Implementation Steps:**
1. Create class inheriting from no interface (utility only)
2. Copy methods from Pico8Functions (lines ~670-760, ~760-800, ~810-880)
3. Update internal field references
4. Add XML documentation
5. No test changes needed (backward compatible)

---

## Task 3.2.2: Create MenuService

**File:** `CSharpCraft.Pico8/Services/MenuService.cs`

**Purpose:** Manage UI menu rendering and interaction

**Methods to Extract:**
```csharp
// Menu management
public void Menuitem(int pos, Func<string> getName, Action function, List<MenuItem>? list = null)
public void DrawMenu(List<MenuItem> items, int selected)  // Render menu
public void SelectNext(List<MenuItem> items, ref int selected)
public void SelectPrev(List<MenuItem> items, ref int selected)
public void ExecuteSelected(List<MenuItem> items, int selected)

// Properties
public List<MenuItem> MainMenuItems { get; set; }
public List<MenuItem> CurrentMenuItems { get; set; }
public int MenuSelected { get; set; }
```

**Dependencies:**
- GraphicsService (for rendering)
- MenuItem struct/class definition
- Input state (from InputService, passed as parameter)

**Integration Points:**
- Called from Pico8Functions.Update() for menu input handling
- Called from Pico8Functions.Draw() for menu rendering
- Receives input state from InputService

**Size Estimate:** 150-200 lines

**Implementation Steps:**
1. Create class with dependency injection in constructor
2. Copy menu-related methods from Pico8Functions (lines ~743-850)
3. Create public methods for menu rendering/navigation
4. Integrate with GraphicsService.Print() for rendering
5. Keep MenuItem struct in same file or reference existing

---

## Task 3.2.3: Create MapService

**File:** `CSharpCraft.Pico8/Services/MapService.cs`

**Purpose:** Handle map rendering and tile management

**Methods to Extract:**
```csharp
// Map rendering
public void Map(double celx, double cely, double sx, double sy, double celw, double celh, int flags = 0)
public void Mset(double x, double y, int n)  // Set map tile
public int Mget(double x, double y)          // Get map tile

// Properties
public (int x, int y) MapDimensions { get; set; }
public string MapData { get; set; }

// Helper
private int GetMapTile(int x, int y)
private void RenderMapTile(int x, int y, int screenX, int screenY, int spriteId)
```

**Dependencies:**
- GraphicsService (for rendering map tiles)
- Sprite/texture data (from Pico8Functions)
- Map array data (loaded from scene)

**Integration Points:**
- Called from Pico8Functions.Draw() to render visible map
- Uses GraphicsService.Spr() internally for tile rendering
- Reads map/sprite data from current scene (IScene.MapData)

**Size Estimate:** 200-250 lines

**Implementation Steps:**
1. Create class with GraphicsService dependency
2. Copy map methods from Pico8Functions (lines ~714-750)
3. Extract tile rendering logic into private helper
4. Add public Map() as main render method
5. Coordinate with GraphicsService for actual sprite rendering

---

## Task 3.2.4: Refactor Pico8Functions as Orchestrator

**File:** `CSharpCraft.Pico8/Pico8Functions.cs` (MAJOR REFACTORING)

**Current:** 1,310 lines → **Target:** ~120-150 lines

### Structure of New Pico8Functions

```csharp
public class Pico8Functions : IDisposable
{
    // === PUBLIC PROPERTIES (data holders) ===
    // Kept from original
    public SpriteBatch Batch { get; }
    public (F32 x, F32 y) CameraOffset { get; internal set; }
    public (int Width, int Height) Cell { get; internal set; }
    public GraphicsDeviceManager Graphics { get; }
    public GraphicsDevice GraphicsDevice { get; }
    public (int w, int h) Resolution { get; private set; }
    public GameWindow Window { get; }
    public IInputBindingProvider InputBindings { get; set; }
    public IAudioGraphicsSettings Settings { get; set; }
    
    // Pico-8 data (colors, sprites, audio, etc.)
    public List<Color> Colors { get; }
    public List<IScene> Scenes { get; }
    public Dictionary<string, SoundEffect> MusicDictionary { get; }
    public Dictionary<string, SoundEffect> SoundEffectDictionary { get; }
    public Dictionary<string, Texture2D> TextureDictionary { get; }
    public Texture2D Pixel { get; }

    // === SERVICE PROPERTIES (new) ===
    // Injected services - PUBLIC so tests/external code can access
    public IGraphicsEngine? Graphics { get; private set; }
    public IAudioManager? Audio { get; private set; }
    public IInputManager? Input { get; private set; }
    public ISceneManager? SceneManager { get; private set; }
    public IUtilityService? Utilities { get; private set; }
    public IMenuService? Menu { get; private set; }
    public IMapService? Map { get; private set; }

    // === CONSTRUCTOR ===
    // Initialize all services with dependencies
    public Pico8Functions(IScene cart, ...)
    {
        // Store passed data
        Batch = _batch;
        Graphics = _graphics;
        GraphicsDevice = _graphicsDevice;
        // ... other fields

        // Create services
        _graphicsService = new GraphicsService(...);
        _audioService = new AudioService(...);
        _inputService = new InputService(...);
        _sceneManagerService = new SceneManagerService(...);
        _utilityService = new UtilityService(...);
        _menuService = new MenuService(...);
        _mapService = new MapService(...);
    }

    // === MAIN LIFECYCLE METHODS ===
    public void Init()
    {
        _graphicsService.Cls(0);
        _cart?.Init(_graphicsService, _audioService, _inputService, _sceneManagerService, null);
        _sceneManagerService.LoadScene(_cart);
    }

    public void Update()
    {
        _sceneManagerService.ProcessPendingTransitions();
        _inputService.Update();
        _sceneManagerService.GetCurrentScene()?.Update();
        _audioService.Update();
    }

    public void Draw()
    {
        _sceneManagerService.GetCurrentScene()?.Draw();
    }

    // === BACKWARD COMPATIBILITY METHODS ===
    // These delegate to services for old code compatibility
    public void Cls(int col = 0) => _graphicsService?.Cls(col);
    public void Circle(F32 x, F32 y, double r, int c) => _graphicsService?.Circle(x, y, r, c);
    public bool Btn(int i, int p = 0) => _inputService?.Btn(i, p) ?? false;
    public bool Btnp(int i, int p = 0) => _inputService?.Btnp(i, p) ?? false;
    public void Music(int n, double fade = 0) => _audioService?.Music(n, fade);
    public void Sfx(int n, int channel = -1) => _audioService?.PlaySfx(n, channel);
    public F32 Cos(F32 angle) => _utilityService?.Cos(angle) ?? F32.Zero;
    public F32 Sin(F32 angle) => _utilityService?.Sin(angle) ?? F32.Zero;
    public void ScheduleScene(Func<IScene> sceneFactory) => _sceneManagerService?.ScheduleScene(sceneFactory);
    public void Map(...) => _mapService?.Map(...);

    // === CLEANUP ===
    public void Dispose()
    {
        _graphicsService?.Dispose();
        _audioService?.Dispose();
        _inputService?.Dispose();
        _sceneManagerService?.Dispose();
    }
}
```

### Implementation Steps

1. **Identify all methods to extract** (already mapped above to services)
2. **Delete 1,100+ lines** of implementation code
3. **Add 7 service fields** (private IXxxService _xxxService)
4. **Initialize services in constructor**
5. **Add backward compatibility method stubs** (~40 methods)
6. **Test extensively** - Run all 29 unit tests (should still pass)

---

## Task 3.2.5: Create Service Interfaces (if needed)

Check if these interfaces exist; create if missing:
- `IUtilityService` - Math, memory, utilities
- `IMenuService` - Menu management
- `IMapService` - Map rendering

These enable dependency injection testing and service mocking.

---

## Phase 3.3: Integration Testing (18-22 tests)

### Task 3.3.1: SceneInitializationTests.cs

**File:** `CSharpCraft.Tests/Pico8/Integration/SceneInitializationTests.cs`

**Purpose:** Verify services initialize correctly and scenes receive dependencies

**Test 1: Services Initialize with Pico8Functions**
```csharp
[Fact]
public void WhenPico8FunctionsCreated_AllServicesInitialize()
{
    // Arrange & Act
    var pico8 = new Pico8Functions(...);

    // Assert
    pico8.Graphics.Should().NotBeNull();
    pico8.Audio.Should().NotBeNull();
    pico8.Input.Should().NotBeNull();
    pico8.SceneManager.Should().NotBeNull();
    // etc for all 7 services
}
```

**Test 2-3: Graphics/Audio/Input Services Initialize via Injection**
```csharp
// Verify each service received correct dependencies in constructor
```

**Test 4: Scene.Init() Receives All Services**
```csharp
[Fact]
public void WhenSceneInitialized_ReceivesAllServiceDependencies()
{
    var mockScene = new MockScene();
    var pico8 = new Pico8Functions(...);
    
    pico8.Init();
    
    mockScene.ReceivedGraphics.Should().NotBeNull();
    mockScene.ReceivedAudio.Should().NotBeNull();
    // etc
}
```

**Test 5-8: Property Access and State**
```csharp
// CameraOffset, Resolution, MenuItems, MapData all accessible
// Services don't overwrite each other's state
```

**Total:** 8-10 tests

---

### Task 3.3.2: ServiceCoordinationTests.cs

**File:** `CSharpCraft.Tests/Pico8/Integration/ServiceCoordinationTests.cs`

**Purpose:** Verify services work together correctly in realistic scenarios

**Test 1: Graphics + Audio Coordination**
```csharp
[Fact]
public void WhenDrawingWhilePlaying Audio_BothOperateCorrectly()
{
    var pico8 = new Pico8Functions(...);
    pico8.Init();
    pico8.Audio.Music(0);  // Start music
    
    // Draw multiple frames
    for (int i = 0; i < 5; i++)
    {
        pico8.Graphics.Cls(0);
        pico8.Graphics.Circle(...);
        pico8.Draw();
    }
    
    // Audio should still be playing
    pico8.Audio.CurrentTrack.Should().Be(0);
}
```

**Test 2: Input + Scene Transition**
```csharp
[Fact]
public void WhenButtonPressedAndSceneScheduled_TransitionsCorrectly()
{
    var pico8 = new Pico8Functions(...);
    pico8.Init();
    
    var mockScene2 = new MockScene();
    pico8.SceneManager.ScheduleScene(() => mockScene2);
    
    pico8.Input.SetButtonState(0, true);  // Press button
    pico8.Update();  // Should trigger transition
    
    pico8.SceneManager.GetCurrentScene().Should().Be(mockScene2);
}
```

**Test 3: Map + Graphics Rendering**
```csharp
[Fact]
public void WhenMapRenderCalled_UsesGraphicsServiceForTiles()
{
    var mockGraphics = new MockGraphicsService();
    var pico8 = new Pico8Functions(...) { Graphics = mockGraphics };
    
    pico8.Map(0, 0, 0, 0, 16, 16);
    
    // Should have called graphics.Sprite() multiple times
    mockGraphics.SpriteCallCount.Should().BeGreaterThan(0);
}
```

**Test 4-6: Full Game Loop Simulation**
```csharp
// Init → 5 Updates → 5 Draws → State consistency checks
```

**Test 7-10: Complex Multi-Service Workflows**
```csharp
// Realistic scenes using all services together
```

**Total:** 10-12 tests

**Expected Result:**
- Total test count post-Phase-3.3: **47-51 tests** (up from 29)
- All tests passing
- Services properly mocked/tested in isolation and together

---

## Validation Checklist

### Phase 3.2 Post-Completion
- [ ] UtilityService created (250-300 lines)
- [ ] MenuService created (150-200 lines)
- [ ] MapService created (200-250 lines)
- [ ] Pico8Functions refactored (1,310 → ~120 lines)
- [ ] Pico8Functions has backward compatibility methods
- [ ] All 29 unit tests still pass
- [ ] Zero compilation errors
- [ ] Services properly initialize in Pico8Functions constructor
- [ ] Code read-able in single screen view

### Phase 3.3 Post-Completion
- [ ] SceneInitializationTests.cs created (8-10 tests)
- [ ] ServiceCoordinationTests.cs created (10-12 tests)
- [ ] All new integration tests pass
- [ ] Total test count: 47-51 tests
- [ ] Test execution time: <100ms total
- [ ] Services properly mocked in tests
- [ ] Integration scenarios validated

---

## Key Design Principles to Maintain

1. **Dependency Injection** - All services receive dependencies via constructor
2. **Interface-based** - Services implement interfaces for testing/mocking
3. **Backward Compatibility** - Pico8Functions delegates to services but maintains old API
4. **Single Responsibility** - Each service has clear, focused purpose
5. **Testability** - Services can be unit tested independently and integration tested together
6. **No Circular Dependencies** - Services depend on interfaces, not concrete implementations

---

## Risk Mitigation

**Risk:** Pico8Functions refactoring breaks existing scenes  
**Mitigation:** Keep backward compatibility method stubs, run full test suite after each service extraction

**Risk:** Service initialization order matters  
**Mitigation:** Document initialization sequence, validate in tests

**Risk:** Menu/Map services incomplete  
**Mitigation:** Test early with mock data to validate extraction

---

## References for Implementation

**Existing Code to Reference:**
- `/CSharpCraft.Pico8/Services/GraphicsService.cs` - Pattern for new services
- `/CSharpCraft.Pico8/Pico8Functions.cs` - Lines to extract (~670-880 for utilities, ~743-850 for menu, ~714-750 for map)
- `/CSharpCraft.Tests/Pico8/GraphicsPrimitivesTests.cs` - Pattern for new integration tests

**Build Commands:**
```bash
# Build Pico8 only
dotnet build CSharpCraft.Pico8 -c Debug --tl:off

# Build & run tests
dotnet test CSharpCraft.Tests -c Debug --tl:off --logger "console;verbosity=minimal"

# Check compilation only
dotnet build CSharpCraft.Tests -c Debug --tl:off -v quiet
```

---

**Document Created:** February 17, 2026  
**Ready for:** Next session continuation  
**Estimated Total Time:** 3-4 hours  
**Status:** Ready to implement
