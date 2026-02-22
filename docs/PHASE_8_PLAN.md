# Phase 8 Plan: Input Handling Extraction

## Objective
Extract input state management from Pico8Functions to achieve ~400-500 lines and improve SRP score from 3/5 → 4/5.

## Current State
- **Pico8Functions:** 913 lines
- **Input-related code:**
  - `P8Btns buttons` field
  - Button methods: `buttons.Reset()`, `buttons.UpPause()`, `buttons.UpLockout()`, `buttons.Update()`
  - Query methods: `Btn()`, `Btnp()`
  - `InputBindings` property - input configuration
- **SOLID DIP Score:** 5/5 ✅
- **SOLID SRP Score:** 3/5

## Responsibilities to Extract

### Input State Management
**Current Implementation (Lines ~87, ~128, ~273, ~300-334, ~394-431):**
- `P8Btns buttons` - Button state object
- Button lifecycle: `Reset()`, `UpPause()`, `UpLockout()`, `Update()`
- Button query: `Btn(i, p)`, `Btnp(i, p)`
- Pause button detection: Check for button 6 press

**Issues:**
- Input state management mixed with game logic
- Button query methods require access to P8Btns
- Lifecycle methods (Reset, Update) scattered across methods
- Hard to test input behavior in isolation

**Solution:**
- Create `InputStateManager` implementation
- Create `IInputStateManager` interface
- Extract button state and management
- Delegate button queries through manager
- Encapsulate P8Btns internally

**New Interface:**
```csharp
public interface IInputStateManager
{
    bool Btn(int buttonIndex, int player = 0);
    bool Btnp(int buttonIndex, int player = 0);
    void Reset(Pico8Functions context);
    void Update(Pico8Functions context);
    void SetPauseMode(bool isPaused);
}
```

**New Implementation:**
```csharp
public class InputStateManager : IInputStateManager
{
    private P8Btns _buttons;
    private bool _isPauseMode;
    
    // Delegate to _buttons, manage state
}
```

## Implementation Order
1. ✅ Create IInputStateManager interface
2. ✅ Create InputStateManager implementation
3. ✅ Update IServiceFactory with factory method
4. ✅ Update ServiceFactory implementation
5. ✅ Refactor Pico8Functions to use manager
6. ✅ Run tests (expect 110/110 passing)

## Expected Outcomes

### Code Metrics
- **Lines of Code:** 913 → ~700 lines
- **Input-related fields:** 1 (`P8Btns buttons`) → manager encapsulation
- **SOLID SRP Score:** 3/5 → 4/5 ✅
- **Test Pass Rate:** 110/110 ✅
- **Regressions:** 0 ✅

### Quality Improvements
- Input state management isolated
- Button queries independently available
- Input lifecycle clearly defined
- Easier to mock input for testing
- Cleaner Pico8Functions

## Backward Compatibility
- Public `Btn()` and `Btnp()` methods unchanged
- `InputBindings` property unchanged
- All existing call sites continue to work

---
**Phase 8 Status:** 🚀 Ready to Implement
