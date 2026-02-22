# Phase 9 Plan: State Container & Output Facade

## Objective
Create a State Container to consolidate game state access and an Output Facade to simplify rendering calls. This will improve organization and reduce parameter passing.

## Current State
- **Pico8Functions:** 913 lines (still large)
- **State scattered:** Camera offset, resolution, scene, pause state in different places
- **SOLID DIP Score:** 5/5 ✅
- **SOLID SRP Score:** 4/5

## Responsibilities to Extract

### 1. Game State Container
**Current Implementation (Scattered):**
- `_sprites`, `_flags`, `_map`, `_music`, `_sfx` - Game data
- `CameraOffset` property - View state
- `Resolution` property - Display state
- `Cell` property - Viewport cell dimensions
- `_cart` field - Current scene

**Issues:**
- State scattered across Pico8Functions
- Hard to reason about state changes
- Difficult to save/load game state
- Parameter passing is unwieldy

**Solution:**
- Create `IGameState` interface
- Create `GameStateContainer` implementation
- Group related state together
- Provide clean state query interface

**New Interface:**
```csharp
public interface IGameState
{
    // View state
    (F32 x, F32 y) CameraOffset { get; set; }
    (int w, int h) Resolution { get; }
    (int Width, int Height) Cell { get; }
    
    // Game data
    int[] MapData { get; }
    int[] FlagData { get; }
    Color[] Sprites { get; }
    Dictionary<string, List<SongInst>> Music { get; }
    Dictionary<string, Dictionary<int, string>> Sfx { get; }
    
    // Scene reference
    IScene CurrentScene { get; }
    void SetCurrentScene(IScene scene);
}
```

### 2. Output Facade
**Current Implementation:**
- Scattered calls to graphics/audio APIs
- Direct calls: `_graphicsAPI.Pset()`, `_audioAPI.Sfx()`, etc.

**Solution:**
- Create `IOutputFacade` interface
- Create `OutputFacade` implementation
- Consolidate graphics and audio calls
- Simplify rendering coordination

**New Interface:**
```csharp
public interface IOutputFacade
{
    // Graphics shortcuts
    void SetPixel(F32 x, F32 y, int color);
    void DrawRect(F32 x, F32 y, F32 w, F32 h, int color);
    void FillRect(F32 x, F32 y, F32 w, F32 h, int color);
    void DrawCircle(F32 x, F32 y, double r, int color);
    void FillCircle(F32 x, F32 y, double r, int color);
    void ClearScreen(int color = 0);
    
    // Audio shortcuts
    void PlaySound(int sfx, int channel = 0, int offset = 0, int length = 32);
    void PlayMusic(int music, int fadems = 0);
    void StopMusic();
    void MuteAudio();
}
```

## Implementation Order
1. ✅ Create IGameState interface
2. ✅ Create GameStateContainer implementation
3. ✅ Create IOutputFacade interface
4. ✅ Create OutputFacade implementation
5. ✅ Update IServiceFactory with factory methods
6. ✅ Update ServiceFactory implementations
7. ✅ Refactor Pico8Functions to use facades
8. ✅ Run tests (expect 110/110 passing)

## Expected Outcomes

### Code Metrics
- **Lines of Code:** 913 → ~750 lines
- **SOLID SRP Score:** 4/5 → 4.5/5 ✅
- **Test Pass Rate:** 110/110 ✅
- **Regressions:** 0 ✅

### Quality Improvements
- State container isolated
- Rendering coordinated through facade
- Easier to reason about state
- Cleaner method signatures
- Better support for state serialization

---
**Phase 9 Status:** 🚀 Ready to Implement
