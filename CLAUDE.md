# CSharpCraft — Project Rules

## Build & Test
- Build: `dotnet build CSharpCraft.slnx -c Debug`
- Test: `dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug`
- Always run tests via CLI, not the VS Code Testing view — FNA backends require `.runsettings`
- Run tests after every code change

## Stack
- C# 13 / .NET 10 — primary language and runtime
- FNA (reimplementation of XNA) — graphics/audio/input via SDL3 + Vulkan
- FixPointCS — fixed-point math via `F32` type for deterministic numerics
- PSharp8 (sibling project at `../PSharp8`) — PICO-8 fantasy-console API emulation layer

## External Dependencies
- `~/FNA` — FNA.Core.csproj project reference
- `~/FixPointCS` — FixMath.csproj and FixPointCS.csproj project references
- `../PSharp8` — sibling project reference

## Architecture
- **Facade/Template Method pattern**: `PcraftServices` exposes `internal static` API methods. Each delegates to `protected virtual On*` methods on a current instance. Subclasses (`SeededServices`, `DeluxeServices`, `FilteredSeededServices`) override hooks to change behavior. Same pattern for `PcraftData` / `PcraftSession`.
- **Entity hierarchy**: `Entity(F32 x, F32 y, F32 vx, F32 vy)` → `CharacterEntity(..., Life, Prot)` → `PlayerEntity(..., Stam, invent)` / `ZombieEntity`. Also: `DroppedItemEntity`, `PlacedItemEntity`, `TextPopupEntity`.
- **Scene system**: `IScene` from PSharp8. `PcraftSceneBase` abstract base. Scenes register via `SceneManager` stack.
- **Filter pipeline**: `MapFilter` → `FilterSet` → deterministic via `HashString`. Filter pipeline: noise → classifier → bias → biased classifier → spawn finder → tile slice.
- **Settings**: `HotReloadableSettings<GeneralSettings>` watches `~/.config/CSharpCraft/general.json`, auto-applies on next Update tick.

## Testing Conventions
- xUnit v3 with `[Fact]` / `[Theory]`
- FluentAssertions for assertions (`result.Should().Be(expected)`)
- Moq for mocking
- Test naming: `MethodOrBehavior_ExpectedOutcome_GivenCondition`
- Test classes: `public sealed class [Subject]Tests`
- Organization: `#region` blocks grouping related tests
- Arrange-Act-Assert structure with descriptive `because:` params in FluentAssertions
- Tests requiring FNA graphics: `[Collection("Fna")]` with `FnaFixture`

## Fixed-Point Math
- All positions, velocities, and game math use `F32` from FixMath
- Never use `float` or `double` for game logic
- For F32 comparison in tests: `.Should().Be(f32Value)` or `.Float.Should().BeApproximately(...)`

## Code Conventions
- `sealed` by default
- File-scoped namespaces
- `#region` blocks for test organization
- Explicit types in test code, `var` only when type is obvious
- Follow existing patterns: check 2-3 similar files before creating new ones
