# CSharpCraft

A PICO-8-style game engine built in C# with FNA, featuring a static API facade, orchestrator architecture, and competitive multiplayer via gRPC.

## Project Structure

```
CSharpCraft.slnx
├── CSharpCraft.Pico8/        # Static Pico8 API + orchestrator layer (43 files)
├── CSharpCraft.Game/          # Game scenes — competitive, speedrun, options (62 files)
├── CSharpCraft.FixMath/       # Fixed-point math library (F32, F64)
├── CSharpCraft.Tests/         # xUnit + Moq + FluentAssertions (419 tests)
├── RaceServer/                # gRPC multiplayer backend
└── docs/                      # Architecture docs, phase plans, analysis notebooks
```

## Quick Start

```bash
# Build everything
dotnet build CSharpCraft.slnx

# Run tests (419 tests, ~1s)
dotnet test CSharpCraft.Tests/

# Run the game
cd CSharpCraft && dotnet run
```

## Architecture

Scenes interact with a single **static facade** — no constructor injection needed:

```csharp
using static CSharpCraft.Pico8.Pico8;

public class MyScene : IScene
{
    public void Init()  { Cls(0); Print("Hello", 10, 10, 7); }
    public void Update() { if (Btnp(4)) ScheduleScene(() => new OtherScene()); }
    public void Draw()  { Spr(1, 64, 64); }
}
```

Under the hood, `Pico8` delegates to a `GameOrchestrator` stored in `AsyncLocal<T>` for thread-safe test isolation. See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for the full design.

## Testing

**419 tests** across 24 test files covering:

| Area | Tests | Files |
|------|-------|-------|
| Pico8 Static API | 60+ | Pico8StaticAPITests, Pico8APIExpansionTests, Pico8RenderingAPITests |
| Orchestrators | 40+ | GameOrchestratorTests, GraphicsOrchestratorTests, AudioOrchestratorTests |
| Services | 90+ | PaletteManagerTests, SceneManagerTests, InputStateManagerTests, MapManagerTests, TrackManagerTests |
| Utilities | 50+ | Pico8UtilsTests, Pico8DisplayConfigTests |
| Rendering | 40+ | TextureRendererTests, FnaTextureRendererTests, GraphicsAPITests, AudioAPITests |
| State | 40+ | GameStateContainerTests, PauseMenuStateTests, ServiceFactoryTests |
| Game | 36 | OptionsFileInterfaceTests, OptionsFileValidationTests |

```bash
# Run all tests
dotnet test CSharpCraft.Tests/

# Run a specific test class
dotnet test --filter "PaletteManagerTests"
```

## Dependencies

```
CSharpCraft.FixMath  (no dependencies)
       ↓
CSharpCraft.Pico8  →  FNA.Core
       ↓
CSharpCraft.Game  →  Protobuf, gRPC
       ↓
CSharpCraft (executable)
```

## Documentation

| Document | Description |
|----------|-------------|
| [Architecture](docs/ARCHITECTURE.md) | System design, orchestrator hierarchy, interface contracts |
| [Developer Guide](docs/DEVELOPER_GUIDE.md) | How to add scenes, write tests, work with the codebase |
| [Quick Reference](docs/QUICK_REFERENCE.md) | Commands, file locations, templates |
| [Testing Report](docs/TESTING_COVERAGE_REPORT.ipynb) | Coverage analysis with visualizations |

### Historical / Phase Documentation

Located in [`docs/`](docs/) — phase plans, completion reports, and analysis notebooks from the architectural migration.

## License

See [LICENSE.txt](LICENSE.txt).