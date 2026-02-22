# Agile Refactoring Plan for Pico8Functions

**Date:** February 22, 2026  
**Objective:** Restore Agile principles (KISS, DRY, YAGNI, TDD) to Pico8Functions class  
**Current State:** 1,072 lines, 2/5 SOLID score, 2/5 Agile score  
**Target State:** ~400-500 lines, 4/5 SOLID score, 4/5 Agile score  
**Total Effort:** 5-7 hours aggressive refactoring  

---

## Decision Summary

| Decision | Status | Rationale |
|----------|--------|-----------|
| Remove Phase 1 DI entirely | ✅ APPROVED | Services unused, managers work better, simpler codebase |
| Aggressive extraction | ✅ APPROVED | Extract 4-5 subsystems progressively, visible improvement each phase |
| Test-driven approach | ✅ APPROVED | Tests define contracts, validate refactoring, prevent regressions |
| Priority order | ✅ APPROVED | YAGNI → SRP → DRY → Testing → Docs (bottom-up approach) |

---

## Problem Statement

### Current Violations of Agile Principles

**❌ KISS (Keep It Simple, Stupid)**
- 1,072-line god object doing everything
- 12 constructor parameters (overwhelming)
- 32+ private fields (scattered responsibilities)
- 100+ public methods (not hierarchical)
- Complex initialization: 154 lines in constructor just to set up fields

**❌ SRP (Single Responsibility Principle)**
- Pico8Functions responsible for:
  - Graphics primitives (Circle, Rect, Pset, Spr, Sspr)
  - Audio playback (Music, Sfx, Mute)
  - Scene management (LoadCart, ScheduleScene)
  - Menu rendering (Draw, pause menu logic)
  - Input handling (Btn, Btnp delegation)
  - Map management (Map, Mget, Mset)
  - Palette management (Pal, Palt)
  - Sprite caching (Spr/Sspr texture caching)
  - Utility functions (Cos, Sin, Rnd)
- **10+ distinct responsibilities** in single class

**❌ YAGNI (You Aren't Gonna Need It)**
- Phase 1 DI services created but unused:
  - `_graphicsService` - only used in 4 methods as fallback
  - `_audioService` - only used as fallback in Music/Mute
  - `_inputService` - never used directly
  - `_sceneManager` - never used directly
  - `_utilityService` - never used directly
  - `_menuService` - never used directly
  - `_mapService` - never used directly
- ServiceConfiguration creates but adds 50+ lines of initialization noise
- All services null-checked everywhere (`?.` throughout code)
- **Result:** Phase 1 adds complexity without solving problems

**❌ DRY (Don't Repeat Yourself)**
- Audio track selection duplicated:
  - `curSoundtrack` field (local state)
  - `curSfxPack` field (local state)
  - `Settings.CurrentSoundtrack` property (authoritative)
  - `Settings.CurrentSfxPack` property (authoritative)
  - Requires keeping both in sync (error-prone)
- Reflection patterns scattered:
  - `GetOptionPropertyValue<T>()` method
  - `GetOptionPropertyValueStruct<T>()` method
  - Both delegate to ReflectionHelper (redundant wrappers)
  - Only used by PauseMenuBuilder

**⚠️ Testing Coverage**
- ✅ 52 tests passing
- ⚠️ Mostly integration tests (depend on full Pico8Functions instantiation)
- ❌ Few unit tests for individual methods
- ❌ No tests for constructor behavior
- ❌ No tests for property initialization
- ❌ Hard to test Pico-8 API methods independently

---

## Solution Strategy: 4-Phase Extraction Plan

### PHASE 0: Remove Unused Code (YAGNI) - 1 hour

**Goal:** Eliminate Phase 1 DI, reduce initialization noise, simplify constructor

**Changes:**
1. Delete `ServiceConfiguration.cs` entirely
2. Remove all DI-related fields:
   - `_serviceProvider`
   - `_graphicsService`, `_audioService`, `_inputService`, `_sceneManager`, `_utilityService`, `_menuService`, `_mapService`
3. Remove DI initialization code from constructor (20 lines)
4. Remove null-checks from all methods
5. Delete unused `InputBindings` and `Settings` wrappers

**New Constructor:**
```csharp
public Pico8Functions(
    IScene cart,
    object? titleScreen,
    List<IScene> scenes,
    Dictionary<string, Texture2D> textureDictionary,
    Dictionary<string, SoundEffect> soundEffectDictionary,
    Dictionary<string, SoundEffect> musicDictionary,
    Texture2D pixel,
    SpriteBatch batch,
    GraphicsDeviceManager graphics,
    GraphicsDevice graphicsDevice,
    GameWindow window,
    object? optionsData)
{
    // Initialize properties (simple assignment)
    Batch = batch;
    Graphics = graphics;
    // ... etc (simplified from 154 lines to ~60 lines)
    
    // Initialize managers (Phase 2)
    _audioChannels = new AudioChannels();
    _musicManager = new MusicManager(...);
    _paletteManager = new PaletteManager(Colors);
    _spriteCache = new SpriteCache();
    
    LoadCart(cart);
}
```

**Tests:**
- Constructor completes without exception
- All managers initialized
- All properties assigned
- No null fields

**Result:**
- ✅ YAGNI violation fixed
- ✅ Removes ~70 lines of complexity
- ✅ Clearer constructor intent

---

### PHASE 1: Consolidate State (DRY) - 30 minutes

**Goal:** Remove duplicate state, use single source of truth

**Changes:**
1. Delete `curSoundtrack` field - use `Settings.CurrentSoundtrack` directly
2. Delete `curSfxPack` field - use `Settings.CurrentSfxPack` directly
3. Delete redundant `InitializeSettings()` logic
4. Update all methods to read from Settings:
   - `DecrementSoundtrack()` → uses Settings directly
   - `IncrementSoundtrack()` → uses Settings directly
   - `DecrementSfxPack()` → uses Settings directly
   - `IncrementSfxPack()` → uses Settings directly
5. Delete reflection wrapper methods:
   - Remove `GetOptionPropertyValue<T>()`
   - Remove `GetOptionPropertyValueStruct<T>()`
6. Have all reflection go through `ReflectionHelper` directly

**Tests:**
- Track selection delegates to Settings correctly
- No local state conflicts
- All track methods work with Settings

**Result:**
- ✅ DRY violation fixed
- ✅ Single source of truth
- ✅ 8-10 fewer lines

---

### PHASE 2: Graphics Extraction (SRP Part 1) - 2-3 hours

**Goal:** Extract all graphics methods to service, improve testability, begin SRP

**Architecture:**
```
IGraphicsAPI (interface)
├── Primitives: Pset, Line, Rect, Rectfill, Circ, Circfill
├── Sprites: Spr, Sspr, Sget, Sset
├── Palettes: Pal, Palt, ResetPalette
├── Text: Print, PrintBig
├── Camera: Camera, CameraOffset
└── Colors: Color list, palette management

GraphicsEngine (implementation)
├── Uses: SpriteBatch, GraphicsDevice, TextureDictionary, Pixel
├── Has: _spriteCache, _paletteManager
└── No: Logic for audio, scenes, menus, input

Pico8Functions (remains)
├── Delegates graphics to IGraphicsAPI
├── Handles: scenes, menus, game loop, audio
└── Reduced from 1,072 to ~700 lines
```

**Implementation Steps:**
1. **Write GraphicsAPI tests first** (TDD):
   - Test Pset draws pixel at correct position
   - Test Circle draws circular outline
   - Test Rectfill fills rectangle
   - Test Spr caches textures correctly
   - Test palette remapping applied to sprites
   - Test camera offset affects rendering
   - ~15-20 graphics-specific tests

2. **Create IGraphicsAPI interface:**
```csharp
public interface IGraphicsAPI
{
    void Pset(F32 x, F32 y, int color);
    void Line(F32 x1, F32 y1, F32 x2, F32 y2, int color);
    void Rect(F32 x1, F32 y1, F32 x2, F32 y2, int color);
    void Rectfill(F32 x1, F32 y1, F32 x2, F32 y2, int color);
    void Circ(F32 x, F32 y, F32 radius, int color);
    void Circfill(F32 x, F32 y, F32 radius, int color);
    void Spr(int spriteIndex, F32 x, F32 y, F32 w = 1, F32 h = 1, bool flipX = false, bool flipY = false);
    void Sspr(F32 sx, F32 sy, F32 sw, F32 sh, F32 dx, F32 dy, F32 dw = -1, F32 dh = -1, bool flipX = false, bool flipY = false);
    // ... etc
}
```

3. **Create GraphicsEngine class:**
   - Copy all graphics methods from Pico8Functions
   - Update to use passed dependencies instead of properties
   - Add constructor injection for SpriteBatch, GraphicsDevice, etc.

4. **Update Pico8Functions:**
   - Create `_graphicsAPI` field of type `IGraphicsAPI`
   - Initialize in constructor with new GraphicsEngine(...)
   - Have all graphics methods delegate: `public void Pset(...) => _graphicsAPI.Pset(...);`
   - Remove 300+ lines of graphics implementation

5. **Update tests:**
   - Run GraphicsAPI tests against new implementation
   - Verify all 52 original tests still pass

**Tests Added:**
- GraphicsAPI unit tests (graphics primitives)
- Integration tests (graphics + palette manager)
- Regression tests (existing Pico-8 API methods)

**Result:**
- ✅ SRP violation reduced (graphics now separate concern)
- ✅ Graphics testable independently
- ✅ Pico8Functions reduced to ~700 lines (~35% reduction)
- ✅ GraphicsEngine ~350 lines (focused, testable)
- ✅ KISS improved (clear separation)

---

### PHASE 3: Audio Architecture (SRP Part 2) - 1-2 hours

**Goal:** Wrap AudioChannels in proper service, consolidate audio logic

**Architecture:**
```
IAudioAPI (interface)
├── Sfx(channel, sprite, offset, length)
├── Music(track, fadeMs)
├── Mute(level)
├── Pause/Resume
└── SetVolume

AudioManager (implementation)
├── Has: AudioChannels, MusicManager
├── Coordinates: Audio channel + music playback
└── No: Graphics, scenes, menus

Pico8Functions (remains)
├── Delegates audio to IAudioAPI
└── Handles: scenes, menus, game loop
```

**Implementation Steps:**
1. **Write AudioAPI tests:**
   - Sfx on channel N plays sound
   - Music fades in
   - Mute silences all audio
   - Pause/Resume work correctly

2. **Create IAudioAPI interface:**
```csharp
public interface IAudioAPI
{
    void PlaySfx(int channel, int sfxIndex);
    void PlayMusic(int trackIndex, float fadeMs = 0);
    void Mute(float volume = 0);
    void Pause();
    void Resume();
    void SetSfxVolume(float volume);
    void SetMusicVolume(float volume);
}
```

3. **Create AudioManager class:**
   - Wraps AudioChannels and MusicManager
   - Coordinates both systems
   - Exposes single unified audio API

4. **Update Pico8Functions:**
   - Create `_audioAPI` field
   - Initialize in constructor
   - Have Sfx/Music/Mute delegate
   - Remove ~100 lines of audio code

**Tests Added:**
- AudioAPI unit tests
- AudioChannels integration tests
- MusicManager integration tests
- Regression tests

**Result:**
- ✅ Audio now proper service
- ✅ SRP violation further reduced
- ✅ Pico8Functions reduced to ~600 lines
- ✅ AudioManager ~200 lines

---

### PHASE 4: Final Cleanup (SRP Completion) - 1-2 hours

**Goal:** Finalize SRP by moving remaining concerns, reduce god object to ~100 lines

**Remaining Concerns:**
1. **Menu System** (~80 lines)
   - Already extracted to PauseMenuBuilder
   - Move LoadCart logic to MenuService

2. **Scene Management** (~50 lines)
   - ScheduleScene, Init, Update game loop
   - Move to SceneManager service

3. **Map Management** (~30 lines)
   - Mget, Mset, Map already delegated
   - Consolidate in MapService

4. **Utility Functions** (~30 lines)
   - Cos, Sin, Rnd already delegated
   - Consolidate in UtilityService

**Architecture After Phase 4:**
```
Pico8Functions (~100 lines)
├── Properties (public API surface)
├── Dependencies (GraphicsAPI, AudioAPI, etc.)
├── Constructor (simple initialization)
└── Main Game Loop (Update, Draw)
    └── Delegates to services

IGraphicsAPI + GraphicsEngine (350 lines)
IAudioAPI + AudioManager (200 lines)
ISceneAPI + SceneManager (150 lines)
IMenuAPI + MenuService (100 lines)
IMapAPI + MapService (100 lines)
IUtilityAPI + UtilityService (100 lines)
```

**Result:**
- ✅ SRP fully satisfied
- ✅ KISS achieved (clear separation)
- ✅ Pico8Functions is thin facade (~100 lines)
- ✅ 6-7 focused service classes (~1,000 lines total, distributed)
- ✅ All testable independently

---

## Testing Strategy: Test-Driven Approach

### Test Pyramid
```
         ___
        /   \  Unit Tests (80%)
       /     \ └─ Service-specific behavior
      /_______\
      /       \  Integration Tests (15%)
     /         \ └─ Service-to-service
    /_________\
    /         \   Acceptance Tests (5%)
   /           \  └─ Full game loop
  /___________\
```

### Writing Tests (Before Implementation)

For each phase:
1. **Identify behavior to test**
   - What should this service do?
   - What inputs should it accept?
   - What outputs should it produce?

2. **Write unit tests** (TDD Red phase):
   - Test individual method behavior
   - Mock dependencies
   - Write failing tests first

3. **Write integration tests:**
   - Test service with real dependencies
   - Test interaction between services
   - Verify no regressions

4. **Implement** (TDD Green phase):
   - Make tests pass
   - Refactor to improve code quality

5. **Verify all tests pass** (TDD Refactor phase):
   - Check 52 original tests still pass
   - Check new tests pass
   - Commit if clean

### Test File Structure
```
CSharpCraft.Tests/
├── Pico8/
│   ├── GraphicsAPITests.cs          (Phase 2)
│   ├── AudioAPITests.cs             (Phase 3)
│   ├── SceneAPITests.cs             (Phase 4)
│   ├── MenuAPITests.cs              (Phase 4)
│   ├── MapAPITests.cs               (Phase 4)
│   ├── UtilityAPITests.cs           (Phase 4)
│   ├── Pico8FunctionsTests.cs       (All phases)
│   └── Integration/
│       └── FullGameLoopTests.cs     (Phase 4)
```

---

## Metrics & Success Criteria

### Before Refactoring
| Metric | Current | Target |
|--------|---------|--------|
| Pico8Functions lines | 1,072 | <300 |
| Constructor params | 12 | 12 (justified) |
| Public methods | 100+ | 20-30 (delegating) |
| Private fields | 32+ | 8-10 (DI + state) |
| Unit tests (Pico8Functions) | 5 | 25+ |
| SOLID score | 2/5 | 4/5 |
| Agile score | 2/5 | 4/5 |

### Success Criteria
- ✅ All 52 original tests pass with new architecture
- ✅ 25+ new unit tests added
- ✅ No modifications to public Pico8Functions API (backward compatible)
- ✅ Pico8Functions reduced to <300 lines
- ✅ 6+ service classes, each <200 lines
- ✅ KISS: Clear separation of concerns
- ✅ SRP: Each class has one job
- ✅ DRY: No duplication
- ✅ YAGNI: All code is used
- ✅ TDD: Tests guide design

---

## Implementation Order

### Priority 1: Phase 0 (Remove YAGNI)
- **Effort:** 1 hour
- **Risk:** Low (removing unused code)
- **Benefit:** Clearer codebase immediately
- **Start:** NOW

### Priority 2: Phase 1 (Consolidate State)
- **Effort:** 30 mins
- **Risk:** Low (consolidating duplicates)
- **Benefit:** Single source of truth
- **Start:** After Phase 0 ✅

### Priority 3: Phase 2 (Graphics Extraction)
- **Effort:** 2-3 hours
- **Risk:** Medium (large refactor, well-tested)
- **Benefit:** ~35% size reduction, testable graphics
- **Start:** After Phase 1 ✅

### Priority 4: Phase 3 (Audio Service)
- **Effort:** 1-2 hours
- **Risk:** Medium (audio tested)
- **Benefit:** Proper audio architecture
- **Start:** After Phase 2 ✅

### Priority 5: Phase 4 (Final Cleanup)
- **Effort:** 1-2 hours
- **Risk:** Medium (multiple services)
- **Benefit:** Complete SRP, thin facade
- **Start:** After Phase 3 ✅

---

## Risk Mitigation

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Tests fail after Phase 0 | Medium | High | Keep Phase 1 DI code temporarily, remove after Phase 0 tests pass |
| Graphics API breaks existing code | Medium | High | Write tests for all graphics before extraction; test during refactoring |
| Audio refactoring breaks music/sfx | Medium | High | AudioChannels/MusicManager already tested; wrap carefully |
| Forgotten dependency in extraction | Low | Medium | Systematic review of each method before moving |
| Performance regression | Low | Medium | Profile after Phase 2; compare frame times |
| Scene initialization breaks | Low | High | SceneManager carefully designed; extensive testing |

---

## Next Steps

1. **Review this plan** - Adjust priorities if needed
2. **Approve Phase 0** - Remove unused DI code
3. **Implement Phase 0** - Commit after tests pass
4. **Continue to Phase 1** - After Phase 0 complete
5. **Document as we go** - Update architecture docs parallel to code

---

**Ready to proceed with Phase 0? Or would you like adjustments to the plan?**
