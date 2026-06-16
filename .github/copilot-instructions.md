# CSharpCraft & PSharp8 Context

Multi-root workspace: **CSharpCraft** (game executable on FNA) + **PSharp8** (reusable emulator library on .NET 10.0, C# 13).

## Essential Commands

Use **CLI** (not VS Code Testing view):
```bash
dotnet build CSharpCraft.slnx -c Debug
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```
`.runsettings` pre-configures FNA backends before tests start (required on Linux/Wayland).

## Code Rules

- **Access**: Default `internal`. Only `public` for PSharp8 ↔ CSharpCraft boundary.
- **Nullability**: Constructor guards: `param ?? throw new ArgumentNullException(nameof(param))`
- **Tests**: xUnit `[Fact]`, FluentAssertions, Moq. Pattern: `Subject_Expected_Context()`
- **Namespaces**: `CSharpCraft.*`, `PSharp8.*`, tests in `.Tests` projects

## Architecture Overview

PSharp8 uses manager-based architecture:
- **GameOrchestrator** creates and wires managers (Graphics, Audio, Input, Memory, Scene)
- **Pico8.cs** static facade provides thread-safe access via `AsyncLocal<GameOrchestrator>`
- CSharpCraft entry: **Main.cs** configures FNA backend, initializes orchestrator

See [GameOrchestrator.cs](../../PSharp8/PSharp8/GameOrchestrator.cs), [Pico8.cs](../../PSharp8/PSharp8/Pico8.cs) for implementation details.

## FNA Backend Configuration (Linux/Wayland Critical)

`.runsettings` file pre-configures:
```xml
<EnvironmentVariables>
  <FNA_PLATFORM_BACKEND>SDL3</FNA_PLATFORM_BACKEND>
  <FNA3D_FORCE_DRIVER>SDLGPU</FNA3D_FORCE_DRIVER>
  <SDL_GPU_DRIVER>vulkan</SDL_GPU_DRIVER>
</EnvironmentVariables>
```

- **Applied by**: `dotnet test` CLI and VS Code shell task
- **NOT by**: VS Code Testing view (C# Dev Kit limitation)

Troubleshooting: See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for FNA3D backend issues.

## References

- **Testing patterns**: [TDD-WORKFLOW.md](TDD-WORKFLOW.md)
- **Doc maintenance**: [DOCUMENTATION-MAINTENANCE.md](DOCUMENTATION-MAINTENANCE.md)
- **Design reviews**: [.github/skills/proactive-design/SKILL.md](skills/proactive-design/SKILL.md)

## Accumulator & Fixed Timestep

The game loop follows the **"Fix Your Timestep"** pattern (Glenn Fiedler): fixed-dt logic steps with input captured once per step, while rendering runs at a potentially different rate.

### Architecture: Poll-at-frame-rate, Consume-at-logic-rate

```
Display loop (60fps, driven by FNA):
  InputManager.Update(frameDt)       ← poll hardware every frame
  SceneManager.InternalUpdate(frameDt) ← accumulator loop
    for each scene → for each registration:
      accumulator += frameDt
      while accumulator >= fixedDt:
        accumulator -= fixedDt
        reg.Callback()               ← logic at fixed rate
        if isTop && reg.ReceivesInput:
          InputManager.ConsumePressedFlags()  ← consume once per step
  SceneManager.InternalDraw(frameDt) ← draw at display rate
```

### Key design points

- **Input polling** (`InputManager.Update`) runs every display frame — no transient key states missed.
- **Edge detection** (`_pressedThisFrame`) **latches** across frames until `ConsumePressedFlags()` clears it — a press on frame N is visible to a 30fps callback on frame N+2.
- **Consumption boundary** (`ConsumePressedFlags`) is called by `SceneManager` after the primary registration's callback fires — ensures one `Btnp()` hit per press per logic step.
- **Per-registration accumulators** — each `FunctionRegistration` tracks its own `Accumulator` and `Fps`, allowing different update/draw rates within the same scene.
- **Spiral-of-death guard** — max 5 accumulator steps per frame (`MaxStepsPerFrame`). If hit, excess time is discarded and resumes fresh next frame.
- **Auto-repeat** (`BtnpConfig.InitialRepeatMs`/`SubsequentRepeatMs`) is ms-based and framerate-independent — consistent across all logic rates.

### Primary registration (input receiver)

Only **one registration per scene** receives input — the first `RegisterUpdate` call. Its `ReceivesInput` property is set to `true` by `SceneSetup`. Only the **top scene's** primary triggers `ConsumePressedFlags`. Background scenes with `ContinueWithoutInputs` still have `InputBlocked = true` during their callbacks.

Key classes:
- `InputManager` — `IInputManager` implementation with latched `_pressedThisFrame` and `_consumedSincePress`
- `SceneManager` — owns the accumulator loop, calls `ConsumePressedFlags` after primary callback
- `FunctionRegistration` — per-callback accumulator, FPS, PauseBehavior, ReceivesInput flag
- `IFunctionHandle` — public API for runtime mutation (Fps, PauseBehavior, Enabled, ReceivesInput)

---

*Last updated: 16 June 2026*
