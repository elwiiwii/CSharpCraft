# Migration Status

> Tracking the transition from legacy architecture to the current static facade + orchestrator design.
> Last updated after Phase 13 completion (569 tests passing).

## Overview

| Metric | Value |
|--------|-------|
| Pico8 API Layer | 56 source files, ~4,900 LOC (excluding lookup tables) |
| GameOrchestrator | 373 lines (down from 1,072) |
| Interfaces | 18 public interfaces |
| Test count | 569 passing |
| Test files | 36 |
| Game build errors | 0 |
| Scene migration | 100% complete |

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

## Architecture Milestones

### Legacy Removal (Phase 6)
- **Pico8Functions.cs** (875 lines) — removed, replaced by GameOrchestrator (373 lines)
- **Services/ directory** — removed, services live at top level of CSharpCraft.Pico8/
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
