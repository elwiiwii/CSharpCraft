---
name: best-practices
description: |-
  Enforce uniform industry best practices and codebase conventions for
  CSharpCraft and PSharp8. Covers C#, FNA, FixPointCS, PSharp8, xUnit,
  and project-specific patterns.
  Use when writing new code, reviewing changes, or refactoring.
  Use proactively when creating new files, classes, scenes, entities,
  filters, or when modifying existing patterns.

  Examples:
  - user writes new entity type → check Entity hierarchy, F32 math, PcraftServices integration
  - user creates new scene → check IScene implementation, PSharp8 conventions, resolution
  - user adds filter → follow Filter pipeline, FilterSet, deterministic HashString
  - user adds test → check xUnit + FluentAssertions, naming convention, sealed class
---

## Before Writing New Code

Find 2-3 existing implementations of similar functionality in the codebase. Follow their patterns exactly. Uniformity over personal preference.

## C# Conventions
- `sealed` classes by default
- Minimum necessary privilage
- File-scoped namespaces
- Explicit types in public API and tests; `var` only when the type is obvious from the right side
- Use `#region` blocks for grouping in longer files
- XML doc comments on public API and test classes

## Codebase-Specific Patterns

### Facade / Template Method (PcraftServices)
- All game logic is accessed through `PcraftServices.*` static methods
- Each static method delegates to `protected virtual On*` on the current instance
- Variants subclass and override: `SeededServices`, `DeluxeServices`, `FilteredSeededServices`
- Same pattern for `PcraftData` (with `On*` hooks) and `PcraftSession` (singleton)
- When adding new functionality, follow this pattern: static method → virtual hook → subclass override

### Entity System
- Base: `Entity(F32 x, F32 y, F32 vx, F32 vy)`
- Character: `CharacterEntity(..., Life, Prot, Panim, Banim, Step, ...)` → `PlayerEntity(..., Stam, invent, CurItem, ...)`
- World items: `DroppedItemEntity`, `PlacedItemEntity`, `TextPopupEntity`
- All game state in F32. All positions/velocities are F32.

### Scene System (PSharp8)
- Implement `IScene` with `Init(ISceneSetup)`, `SpritesPath`, `MapPath`, `Music`, `Sfx`
- Register via `SceneManager` (stack-based with push/pop/schedule)
- Resolution declared in PICO-8 convention pixels in `Init()`
- Base class `PcraftSceneBase` for common gameplay scene behavior

### Filter Pipeline (PcraftFilter)
- Each filter extends `MapFilter` with specific tile count / spawn / concentration constraints
- `FilterSet` holds `IReadOnlyList<MapFilter>`
- Each filter has a `HashString` for deterministic caching
- Pipeline: noise layers → MapClassifier → BiasLayers → BiasedMapClassifier → SpawnFinder → tile slice

### Fixed-Point Math
- All game logic uses `F32` from FixMath (a fixed-point 32-bit type matching PICO-8 Lua float semantics)
- Never use `float`, `double`, or `decimal` for game logic
- For constants: `F32.FromInt(n)`, `F32.Zero`, `F32.One`

## Testing (xUnit + FluentAssertions)
- Naming: `MethodOrBehavior_ExpectedOutcome_GivenCondition`
- Classes: `public sealed class [Subject]Tests`
- `[Fact]` for single tests, `[Theory]` + `[InlineData]` for parameterized
- Assertions via FluentAssertions `Should()` chain
- Tests needing FNA graphics: `[Collection("Fna")]` with `FnaFixture`
- Test patterns from the codebase: determinism tests, independence tests, invariant tests, edge-case tests, smoke tests for static data

## External Libraries
- FNA: graphics, audio, input. Native backends: SDL3 + SDLGPU + Vulkan.
- FixPointCS: F32 fixed-point math via `~/FixPointCS` project reference.
- PSharp8: PICO-8 emulation layer. `Pico8.*` static API, `GameOrchestrator`, `SceneManager`.

## When Patterns Conflict
If a best practice conflicts with the codebase convention, follow the codebase convention. Uniformity within the project takes priority over external standards, unless the convention is causing active problems — in which case, invoke `proactive-design` to discuss with the user.
