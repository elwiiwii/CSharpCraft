---
name: CSharpCraft Workspace Instructions
description: >-
  CSharpCraft is a competitive cross-platform game engine with multi-tier architecture:
  game logic (CSharpCraft) → engine abstraction (Pico8) → FNA framework → SDL3/native.
  Includes deterministic fixed-point math, scene management, graphics caching, and
  platform-specific graphics device handling for Linux/Wayland. Use when working on
  gameplay features, engine systems, graphics/audio, multiplayer, or tests.
---

# CSharpCraft Workspace Instructions

## Quick Reference: Build & Test

### Build
```bash
# Debug build (all projects)
dotnet build -c Debug

# Release build
dotnet build -c Release
```

### Test
```bash
# Recommended: Run via task (respects .runsettings FNA backend config)
# Command Palette → Tasks: Run Task → "test: CSharpCraft.Tests"

# Alternative: CLI (also uses .runsettings)
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```

**⚠️ Do NOT use VS Code Testing view** — it bypasses `.runsettings` and fails on Wayland.

---

## Project Structure

| Project | Type | Purpose |
|---------|------|---------|
| **CSharpCraft** | Game Exe (.NET 9) | Main game: scenes (Pcraft singleplayer, Competitive multiplayer), Options, Credits |
| **CSharpCraft.Game** | Exe (.NET 10) | FNA host layer: game loop setup, content loading |
| **Pico8** | Library (.NET 10) | **Core engine**: Graphics/Audio/Input/Scene managers, fixed-point math, memory management |
| **CSharpCraft.Tests** | xUnit Tests (.NET 10) | Integration tests with real `GraphicsDevice` on headless Linux |
| **RaceServer** | ASP.NET Web Service (.NET 9) | Backend: user auth, room/match management, competitive features |

---

## Architecture

### Layering
```
CSharpCraft (game logic)
    ↓ depends on
Pico8 (engine API + manager pattern)
    ↓ depends on
FNA (MonoGame-like framework)
    ↓ depends on
SDL3, FNA3D (native graphics/audio)
```

### Game Loop & Scenes
- **GameOrchestrator** (Pico8): Owns managers and runs main loop
- **IScene interface**: `SceneName`, `Fps`, `Resolution`; implement `Update()`, `Draw()`
- Examples:
  - `Pcraft/PcraftBase` — abstract base for singleplayer gameplay
  - `Competitive/CompetitiveScene` — multiplayer scene
  - `Credits/CreditsScene`, `OptionsMenu/GeneralOptions` — UI scenes

### Core Managers (Pico8)
| Manager | Purpose |
|---------|---------|
| **GraphicsManager** | 2D sprite rendering, camera, palette abstraction |
| **AudioManager** | Music/SFX playback via FNA SoundBank |
| **InputManager** | Button abstraction: `Btn(button)`, `Btnp(button)` API matching PICO-8 |
| **SceneManager** | Scene lifecycle: `Update()`, `Draw()`, transitions |
| **SpriteMapData** | Sprite asset management with LRU texture cache |
| **Fixed32/Fixed64** | Deterministic 16.16 fixed-point math for physics |

### Key Domains

**Pcraft/** — Singleplayer level/entity system
- `Level` — tilemap structure
- `Entity` — game objects (position, velocity, collision)
- `Material` — block types and properties
- `Ground` — terrain/floor system
- `GenSeedCompetitive`, `LoadSeed` — level generation and loading

**Competitive/** — Multiplayer features
- `RankedScene`, `UnrankedScene` — ranked/casual lobbies
- `PickBanScene` — character/level selection
- `VersusGame` — active match gameplay
- `RoomHandler`, `LobbyScene` — room/player management
- `ProfileScene`, `StatisticsScene` — player stats UI

**FixMath/** — Deterministic math (critical for multiplayer)
- `F32`, `F64` — Fixed-point value types (16.16 layout in 32-bit)
- All operators inlined (`[MethodImpl(AggressiveInlining)]`)
- Used for positions, velocities, physics—ensures identical simulation across clients

**Graphics Subsystem**
- `SpriteTextureManager` — GPU texture cache with LRU eviction
- `PaletteManager` — palette swapping and color management
- `SpriteBatchExtensions` — convenience helpers for drawing

---

## Platform-Specific: Linux/Wayland + FNA/SDL3

### The Issue
FNA3D native startup probes the backend (OpenGL → Vulkan). On this setup:
- Wayland requires Vulkan (not OpenGL)
- Environment variables must be set **before the test host launches**
- Setting them after startup or via fallback may be too late

### Solutions

**1. Use `.runsettings` (recommended for tests)**
- File: `.runsettings` (repo root)
- Configures the VSTest host to set environment variables early:
  ```xml
  <RunConfiguration>
    <EnvironmentVariables>
      <EnvironmentVariable name="FNA_PLATFORM_BACKEND" value="SDL3" />
      <EnvironmentVariable name="FNA3D_FORCE_DRIVER" value="SDLGPU" />
      <EnvironmentVariable name="SDL_GPU_DRIVER" value="vulkan" />
    </EnvironmentVariables>
  </RunConfiguration>
  ```

**2. Use SDL hints (fallback in-process)**
- File: `CSharpCraft.Tests/Infrastructure/GraphicsFixture.cs`
- Uses `SDL_SetHintWithPriority(SDL_HINT_GPU_DRIVER, "vulkan", High)` before device creation
- This is a fallback when `.runsettings` doesn't apply (e.g., certain test runners)

**3. Do NOT rely on `Environment.SetEnvironmentVariable` alone**
- Cannot reach native startup reliably
- See user memory: `debugging.md` for discussion

### How to Debug
If tests fail with "Failed to initialize graphics":
1. ✅ Run via task: `Tasks: Run Task` → `test: CSharpCraft.Tests` (uses .runsettings)
2. ✅ Run CLI: `dotnet test ... -c Debug` (uses .runsettings via CSharpCraft.Tests.csproj)
3. ❌ Do NOT use VS Code Testing view Explorer

---

## Code Patterns & Conventions

### Fixed-Point Math
Always use `Fixed32`/`Fixed64` for gameplay values that must be deterministic:
```csharp
// ✅ Correct: Deterministic physics
Fixed32 posX = Fixed32.FromInt(10);
Fixed32 velocityX = Fixed32.FromFraction(1, 2); // 0.5
posX += velocityX; // Exact same result every simulation

// ❌ Avoid: Float drift in multiplayer gameplay
float posX = 10f;
float velocityX = 0.5f;
posX += velocityX; // May differ microscopically across clients
```

### Scene Pattern
```csharp
public class MyScene : IScene
{
    public GameOrchestrator Pico8 { get; }
    
    public string SceneName => "MyScene";
    public int Fps => 60;
    public Vector2 Resolution => new(1280, 720);
    
    public void Update(GameTime gameTime) { /* ... */ }
    public void Draw(SpriteBatch spriteBatch) { /* ... */ }
}
```

### Graphics Caching
Sprites are cached with LRU eviction—no manual texture disposal needed:
```csharp
// First call: loads from disk, caches in GPU
Texture2D sprite = Pico8.Graphics.GetSpriteTexture("spriteName");

// Subsequent calls: returns cached instance
Texture2D same = Pico8.Graphics.GetSpriteTexture("spriteName");
```

### Input Pattern (PICO-8 API)
```csharp
// Per-frame button state (held)
if (Pico8.Input.Btn(Button.Left)) { /* moving */ }

// Button pressed this frame (edge-triggered)
if (Pico8.Input.Btnp(Button.A)) { /* action */ }
```

### Dependency Injection (Optional)
- RaceServer and tests use Microsoft.Extensions.DependencyInjection
- For game logic, use static `Pico8.Instance` (GameOrchestrator singleton) instead

---

## Development Workflow

### Adding a New Feature

1. **Determine scope:**
   - Gameplay logic? → `Pcraft/` or `Competitive/`
   - Engine behavior? → `Pico8/Graphics/` or `Pico8/Scene/`
   - Menu/UI? → `OptionsMenu/` or `Credits/`

2. **Create scene/entity or add to existing:**
   - Inherit from `IScene` or add logic to `Entity`
   - Use `Pico8.Graphics.DrawSprite(...)` for rendering
   - Use `Fixed32` for positions if multiplayer-relevant

3. **Build & test:**
   ```bash
   dotnet build -c Debug
   dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
   ```

4. **Wire into scene manager:**
   - Update `CSharpCraft/Main.cs` or relevant scene transition logic

### Testing Graphics-Dependent Code
- Create a test inheriting `IAsyncLifetime` using `GraphicsFixture`
- The fixture handles FNA device creation with proper SDL hints
- Example: `CSharpCraft.Tests/SpriteTextureManagerTests.cs`

---

## Common Conventions

| Pattern | Usage |
|---------|-------|
| `AsyncLocal<GameOrchestrator>` for static API | Pico8 managers accessed globally like PICO-8 API |
| `[MethodImpl(AggressiveInlining)]` | FixMath operators (performance-critical) |
| Scene lifecycle: `Update()` then `Draw()` | Separated logic and rendering phases |
| Sprite names as strings | SpriteTextureManager caches by name-based lookup |
| xUnit + Moq for tests | Mocking for unit tests; GraphicsFixture for integration |
| Single executable output | `PublishSingleFile=true` in .csproj |

---

## Quick Checklist: Before Committing

- [ ] `dotnet build -c Debug` succeeds
- [ ] `dotnet test` passes (or use task)
- [ ] Fixed-point math used for deterministic gameplay
- [ ] No `float` for physics values in competitive features
- [ ] Graphics accessed via managers (not direct FNA calls)
- [ ] New scenes properly hooked in SceneManager
- [ ] Code follows existing indentation/naming conventions

---

## Useful Files & References

| File | Purpose |
|------|---------|
| `.runsettings` | VSTest environment configuration for FNA backend |
| `CSharpCraft.Tests/Infrastructure/GraphicsFixture.cs` | FNA device setup with SDL hints (fallback) |
| `Pico8/GameOrchestrator.cs` | Central manager orchestration |
| `CSharpCraft/Main.cs` | Initial scene setup and main loop entry |
| `README.md` | Detailed testing and platform-specific issue guidance |
