# Phase 7 Plan: Scene & Menu Management Extraction

## Objective
Extract scene and pause menu management concerns from Pico8Functions to achieve ~600-700 lines (currently 921).

## Current State
- **Pico8Functions:** 921 lines (facade handling too many concerns)
- **SOLID DIP Score:** 5/5 ✅
- **SOLID SRP Score:** 2/5 ❌ (handles 7+ distinct responsibilities)

## Responsibilities to Extract

### 1. Scene Management
**Current Implementation (Lines 262-298):**
- `ScheduleScene()` - Queue next scene
- `LoadCart()` - Load new scene, reset state, build pause menu

**Issues:**
- Single Responsibility Principle violation (scene loading mixed with state reset)
- Hard to test scene transitions without full Pico8Functions initialization
- Pause menu building tightly coupled to scene loading

**Solution:**
- Create `ISceneManager` interface
- Create `SceneManager` implementation
- Extract `LoadCart()` and `ScheduleScene()` logic
- Delegate scene transitions through `SceneManager`

**New Interface:**
```csharp
public interface ISceneManager
{
    IScene CurrentScene { get; }
    void ScheduleScene(Func<IScene> sceneFactory);
    void LoadScene(IScene scene);
    void UpdateScheduledScene();
}
```

### 2. Pause Menu Management
**Current Implementation (Lines 307-328):**
- `isPaused` - Pause state
- `menuSelected` - Selected menu item
- `mainMenuItems` - Main menu structure
- `curMenuItems` - Current menu items
- Button handling in Update()
- `PlaySound(bool)` - Pause audio

**Issues:**
- UI state mixed with game logic
- 4 related fields should be grouped
- Button handling logic scattered across Update()
- Hard to test menu interactions

**Solution:**
- Create `IPauseMenuManager` interface
- Create `PauseMenuManager` implementation
- Group pause state into single class
- Delegate menu updates through manager

**New Interface:**
```csharp
public interface IPauseMenuManager
{
    bool IsPaused { get; }
    void TogglePause();
    void HandleMenuInput(P8Btns buttons, Pico8Functions context);
    void Update();
}
```

### 3. Service Factory Updates
**Current IServiceFactory:**
- Doesn't include SceneManager or PauseMenuManager
- Will be updated to create all managers

**New Methods:**
```csharp
ISceneManager CreateSceneManager(IScene initialScene);
IPauseMenuManager CreatePauseMenuManager(PauseMenuBuilder builder);
```

## Implementation Order
1. ✅ Create ISceneManager interface
2. ✅ Create SceneManager implementation
3. ✅ Create IPauseMenuManager interface
4. ✅ Create PauseMenuManager implementation
5. ✅ Update IServiceFactory with new methods
6. ✅ Update ServiceFactory implementations
7. ✅ Refactor Pico8Functions to use managers
8. ✅ Run tests (expect 110/110 passing)

## Expected Outcomes

### Code Metrics
- **Lines of Code:** 921 → ~650-700
- **SOLID SRP Score:** 2/5 → 4/5 ✅
- **Test Pass Rate:** 110/110 ✅
- **Regressions:** 0 ✅

### Quality Improvements
- Scene loading logic independently testable
- Menu state management isolated
- Clear separation of concerns
- Easier to mock/replace implementations
- Reduced cognitive load on Pico8Functions

## Backward Compatibility
- Pico8Functions API unchanged (public methods/properties preserved)
- Optional IServiceFactory parameter continues to default to ServiceFactory
- All existing call sites continue to work

## Testing Strategy
- No new tests needed (existing 110 tests verify integration)
- All new managers tested through Pico8Functions behavior
- Focus on regression testing with full test suite

---
**Phase 7 Status:** 🚀 Ready to Implement
