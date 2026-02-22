# Phase 10 Plan: Input Handler Extraction & Pause State Consolidation

## Objective
Extract input handling logic and consolidate pause menu state management to achieve 5/5 SRP and further reduce Pico8Functions complexity.

## Current State (After Phase 9)
- **Pico8Functions:** ~950 lines (grew slightly due to facade integration)
- **Test suite:** 110/110 passing ✅
- **SOLID DIP Score:** 5/5 ✅
- **SOLID SRP Score:** 4/5 (target: 5/5)
- **Architecture:** Well-structured with IGameState, IOutputFacade, 10+ managers

## Analysis: Remaining SRP Violations

### 1. Input Handling Mixed with Orchestration (CURRENT)
**Current Responsibilities in Pico8Functions:**
- `Btn()` - Direct delegation to IInputStateManager
- `Btnp()` - Direct delegation to IInputStateManager
- `Btnv()` - Direct delegation to IInputStateManager
- Scene pause/unpause logic dispatched here
- Input state updates called directly

**Pattern:**
```csharp
public bool Btn(int b, int p = -1)
{
    return _inputStateManager?.Btn(b, p) ?? false;
}

public bool Btnp(int b, int p = -1)
{
    return _inputStateManager?.Btnp(b, p) ?? false;
}
```

**Issue:** Pico8Functions acts as pass-through for input queries instead of isolated concern

**Solution:** Create `IInputQueryFacade` to centralize input queries

### 2. Pause Menu State Management (CURRENT)
**Current Implementation:**
- `_pauseMenuState` field (creates pause menu instance)
- Pause menu controls itself in Draw
- Pause state checked in main update loop
- Pause toggling scattered across methods

**Pattern:**
```csharp
// In Pico8Functions
_pauseMenuState = serviceFactory.CreatePauseMenuState(this);

// In game loop
if (!_pauseMenuState?.IsPaused ?? false)
{
    // Update game
}
```

**Issues:**
- Pause state tightly coupled to main class
- Hard to test pause behavior independently
- PauseMenuState depends on Pico8Functions
- Pause logic not cohesive

**Solution:** Create `IPauseStateManager` to handle pause transitions

### 3. Scene Management Responsibilities (CURRENT)
**Current Workflow:**
- `IScene _cart` - current scene reference
- `_sceneStateManager` - manages scene transitions
- `LoadCart(IScene)` - initializes scene
- `Update(GameTime)` - yields to scene or pause menu
- Scene changes dispatched through scene state manager

**Issue:** Complex scene transition logic could be better organized

## Phase 10 Implementation Plan

### Milestone 1: Input Query Facade (1 hour)

**1. Create `IInputQueryFacade` interface**
```csharp
public interface IInputQueryFacade
{
    // Input state queries
    bool IsButtonPressed(int button, int player = -1);
    bool IsButtonJustPressed(int button, int player = -1);
    bool IsButtonReleased(int button, int player = -1);
    
    // Utility
    void UpdateInputState();
}
```

**2. Create `InputQueryFacade` implementation**
- Wraps `IInputStateManager`
- Provides clean input query API
- Handles player defaulting logic

**3. Update `IServiceFactory` + `ServiceFactory`**
- Add `CreateInputQueryFacade(IInputStateManager)` method

**4. Update Pico8Functions**
- Replace `Btn()`, `Btnp()`, `Btnv()` to use facade
- Remove direct `IInputStateManager` calls

**Tests:**
- InputQueryFacade queries match InputStateManager results
- All input methods still work
- No regression in input handling

### Milestone 2: Pause State Manager (1 hour)

**1. Create `IPauseStateManager` interface**
```csharp
public interface IPauseStateManager
{
    bool IsPaused { get; }
    void TogglePause();
    void SetPaused(bool paused);
    void Draw(IOutputFacade facade);
    void Update(GameTime gameTime, IInputQueryFacade inputFacade);
}
```

**2. Create `PauseStateManager` implementation**
- Tracks pause state
- Manages pause menu UI
- Handles pause input (Escape to unpause)
- Delegates rendering to menu builder

**3. Update `IServiceFactory` + `ServiceFactory`**
- Change `CreatePauseMenuState()` to `CreatePauseStateManager()`
- Return `IPauseStateManager` instead of `PauseMenuState`

**4. Update Pico8Functions**
- Replace `_pauseMenuState` with `_pauseStateManager`
- Call `_pauseStateManager?.Update()` in main loop
- Call `_pauseStateManager?.Draw()` when paused
- Check `_pauseStateManager?.IsPaused` for game loop control

**Tests:**
- Pause state toggles correctly
- Pause menu renders when paused
- Game doesn't update when paused
- Escape key unpauses game

### Milestone 3: Integration & Testing (1 hour)

**1. Update Pico8Functions Update() method**
```csharp
public void Update(GameTime gameTime)
{
    _inputQueryFacade?.UpdateInputState();
    
    if (_pauseStateManager?.IsPaused ?? false)
    {
        _pauseStateManager.Update(gameTime, _inputQueryFacade);
        return;
    }
    
    // Normal game update
    _cart?.Update(gameTime);
    // ... rest of update
}
```

**2. Update Pico8Functions Draw() method**
```csharp
public void Draw(GameTime gameTime)
{
    _cart?.Draw(gameTime);
    
    if (_pauseStateManager?.IsPaused ?? false)
    {
        _pauseStateManager.Draw(_outputFacade);
    }
}
```

**3. Run full test suite**
- Target: 110/110 tests passing
- Check for regressions
- Validate new managers work correctly

## Expected Metrics After Phase 10

| Metric | Before | After | Change |
|--------|--------|-------|--------|
| Pico8Functions lines | ~950 | ~900 | -50 |
| Public methods (delegating) | ~20 | ~18 | -2 |
| Private fields | ~14 | ~14 | 0 |
| Manager interfaces | 10 | 11 | +1 |
| IServiceFactory methods | ~20 | ~22 | +2 |
| DIP Score | 5/5 | 5/5 | — |
| SRP Score | 4/5 | 5/5 | ⬆️ |
| Test coverage | 110 | 110+ | +5-10 |

## Phase 10 Success Criteria

- ✅ All 110 existing tests pass (zero regressions)
- ✅ 5-10 new tests for input/pause managers
- ✅ `IInputQueryFacade` cleanly wraps input logic
- ✅ `IPauseStateManager` manages pause state independently
- ✅ Pico8Functions reduced to ~900 lines
- ✅ SRP Score: 5/5 (achieved!)
- ✅ All player input works correctly
- ✅ Pause/unpause transitions smooth
- ✅ Game update pauses correctly

## Continuation Roadmap

### Phase 11+: Future Improvements
1. **Scene Manager Extraction** - Move scene transition logic to dedicated manager
2. **Rendering Pipeline** - Create RenderQueue for batch operations
3. **State Serialization** - Enable save/load via IGameState
4. **Asset Management** - Create ResourceManager for texture/sound loading
5. **Configuration** - Move settings to config file instead of reflection

## Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Input regression | Low | High | Test all input combinations |
| Pause state bugs | Low | High | Test pause/resume transitions |
| Circular dependencies | Low | Medium | Keep facade dependencies one-way |
| Performance impact | Low | Low | No algorithmic changes |

---

**Ready to implement Phase 10? Features to extract:**
1. Input query facade (wraps IInputStateManager)
2. Pause state manager (independent pause logic)
3. Integration & full test suite validation

**Estimated time:** 2-3 hours total
**Difficulty:** Medium (facades are straightforward)
**Risk level:** Low (previous phases well-tested)
