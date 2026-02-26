# Migration Status

> Tracking the transition from legacy architecture to the current static facade + orchestrator design.
> Last updated after Phase 16 completion (544 tests passing).

## Overview

| Metric | Value |
|--------|-------|
| Pico8 API Layer | 59 source files in 7 domain folders, ~4,900 LOC (excluding lookup tables) |
| GameOrchestrator | 373 lines (down from 1,072) |
| Interfaces | 17 public interfaces |
| Test count | 544 passing |
| Test files | 35 |
| Game build errors | 0 |
| Scene migration | 100% complete |
| Sub-namespaces | 7 (Audio, Graphics, Input, Scene, Menu, Models, Utilities) |

## Phase Status

| Phase | Status | Tests | Key Changes |
|-------|--------|-------|-------------|
| 1 | ✅ Complete | 35 | Pico8 Static API design + TDD tests |
| 2 | ✅ Complete | ~50 | GameOrchestrator tests & implementation |
| 3 | ✅ Complete | ~70 | Sub-orchestrators (Graphics, Audio) |
| 4 | ✅ Complete | ~246 | API expansion: Camera, Palette, Scene, Map, Audio, Data, Math, Rendering, Display Config |
| 5 | ✅ Complete | — | Scene migration — all 29 IScene implementations migrated to `using static` |
| 6 | ✅ Complete | — | Cleanup — Pico8Functions.cs (875 lines) and Services/ directory removed |
| 7 | ✅ Complete | 528 | PopupService, PauseMenuRenderer, hotkey extraction |
| 8 | ✅ Complete | 528 | GameOrchestrator reduction (1,072 → 328 lines) |
| 9 | ✅ Complete | 548 | Notifications static accessor, IPauseMenuContext, exception boundary |
| 10 | ✅ Complete | 568 | Constructor consolidation — NullScene, InMemorySettings, DefaultInputBindings |
| 11 | ✅ Complete | 578 | PauseMenuBuilder circular dependency removal (MenuInput record) |
| 12 | ✅ Complete | 569 | YAGNI cleanup — dead code removal from GameOrchestrator and Main.cs |
| 13 | ✅ Complete | 569 | Content loading — GameHostContext expanded with audio dictionaries, music pipeline wired |
| 14 | ✅ Complete | 544 | ISP cleanup — split IAudioGraphicsSettings → IAudioSettings + IDisplaySettings, removed IServiceFactory/IOutputFacade |
| 15 | ✅ Complete | 544 | Architecture consistency — 9-sub-phase cleanup (constructors, null guards, readonly, Initialize pattern, IDisposable, records, bug fixes) |
| 16 | ✅ Complete | 544 | Folder reorganization — 53 flat files → 7 domain folders with matching sub-namespaces, Pico8Classes.cs split into 6 files |

## Architecture Milestones

### Legacy Removal (Phase 6)
- **Pico8Functions.cs** (875 lines) — removed, replaced by GameOrchestrator (373 lines)
- **Services/ directory** — removed, services organized into domain folders within CSharpCraft.Pico8/
- All scenes migrated from `p8.Method()` → `Pico8.Method()` via `using static`

### Overlay Split (Phase 9)
- `IOverlayRenderer` split into `IPopupService`/`PopupService` + `IPauseMenuRenderer`/`PauseMenuRenderer`
- `Notifications` static accessor (AsyncLocal pattern, same as `Pico8` and `GameRendering`)
- `PopupSeverity` enum for notification severity-based coloring

### Constructor Consolidation (Phase 10)
- Three Null Object types: `NullScene`, `InMemorySettings`, `DefaultInputBindings`
- Unified constructor: 4 required params + optional defaults (no null! fragility)
- Production path uses `GameHostContext` record; test path bypasses it entirely

### Music Pipeline Fix (Phase 13)
- `GameHostContext` expanded from 9 to 11 properties (added `MusicDictionary`, `SoundEffectDictionary`)
- Audio wiring: `AudioChannels` → `MusicManager` → `AudioAPI` (resolved chicken-and-egg via captured reference)
- Fixed broken music playback (AudioAPI previously received `null` for audioChannels and musicManager)

### ISP Cleanup (Phase 14)
- Split `IAudioGraphicsSettings` into `IAudioSettings` + `IDisplaySettings` (single-responsibility)
- Removed `IServiceFactory`/`ServiceFactory` and `IOutputFacade`/`OutputFacade` (unused after earlier extractions)
- Deleted corresponding test files

### Architecture Consistency (Phase 15)
9 sub-phases ensuring 100% consistent patterns across all Pico8 source files:
1. Traditional constructors (9 primary constructors converted)
2. `?? throw new ArgumentNullException()` null guards (standardized from `ThrowIfNull`)
3. `readonly` on 3 mutable fields
4. GameRendering and Notifications: property-setter → `Initialize()` method pattern
5. AudioOrchestrator: `IDisposable` implementation
6. CosDict/SinDict: public readonly field → property with `{ get; }`
7. MusicInst, SongInst → immutable records; PalCol → mutable record
8. Bug fixes: IntArrayEqualityComparer null handling, MapManager validated accessor, DefaultColors caching, Ptn decoupling

### Folder Reorganization (Phase 16)
- Split 53 flat files into 7 domain folders with matching namespaces
- `Pico8Classes.cs` split into 6 individual per-type files (P8Btns, MenuInput, MenuItem, MusicInst, SongInst, PalCol)
- Interfaces co-located with implementations
- `GlobalUsings.cs` added to Pico8, Game, and Tests projects
- 4 root files kept: Pico8.cs, GameOrchestrator.cs, GameRendering.cs, Notifications.cs

## API Coverage

35 of 35 core PICO-8 methods implemented, plus extensions:

| Legacy Pattern | Current Replacement |
|----------------|-------------------|
| `p8.Btn(i)` / `p8.Btnp(i)` | `Pico8.Btn(i)` / `Pico8.Btnp(i)` |
| `p8.Cls/Circ/Circfill/Rect/Rectfill/Print/Spr/Sspr/Map/Pset` | `Pico8.*` static equivalents |
| `p8.Camera()` / `p8.Camera(x,y)` | `Pico8.Camera()` / `Pico8.Camera(x,y)` |
| `p8.Pal()` / `p8.Pal(c0,c1)` / `p8.Palt()` | `Pico8.Pal/Palt` |
| `p8.Mget/Mset/Fget` | `Pico8.Mget/Mset/Fget` |
| `p8.Sfx/Music/Mute` | `Pico8.Sfx/Music/Mute` |
| `p8.CartData/Cstore/Load/Dget/Dset` | `Pico8.CartData/Cstore/Load/Dget/Dset` |
| `p8.ScheduleScene(...)` | `Pico8.ScheduleScene(...)` |
| `p8.Cell` | `Pico8.CellWidth` / `Pico8.CellHeight` |
| `p8.Resolution` | `Pico8.ResolutionWidth` / `Pico8.ResolutionHeight` |
| `p8.Colors[i]` | `Pico8.GetColor(i)` |
