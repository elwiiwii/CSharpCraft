# Migration Status

> Tracking the transition from legacy architecture to Plan D.
> Last updated after Phase 4 API surface completion (246 tests passing).

## Overview

| Metric | Value |
|--------|-------|
| Plan D LOC | ~1,400 (CSharpCraft.Pico8 + CSharpCraft.Tests) |
| Legacy LOC | ~2,400 (CSharpCraft.Pico8/Services + Pico8Functions.cs + Pico8Classes.cs) |
| Test count | 246 passing |
| CSharpCraft.Game build errors | 60 (58 CS0535 + 2 CS0108) |

## Phase Status

| Phase | Status | Notes |
|-------|--------|-------|
| 1: Pico8 Static API | ✅ Complete | 35 tests |
| 2: GameOrchestrator | ✅ Complete | ~50 tests |
| 3: Sub-orchestrators | ✅ Complete | ~20 tests |
| 4: API Expansion | ✅ Complete | Camera, Palette, Scene, Map, Audio, Data, Math, Rendering, Display Config |
| 5: Scene Migration | ⬜ Not Started | 29 direct + 9 inherited IScene implementations |
| 6: Cleanup & Final | ⬜ Not Started | Remove legacy code, update all docs |

## Scene Migration Checklist

### Error Summary

- **CS0535** (58 errors, 29 unique files): `IScene.Init()` — our redesign changed `Init(Pico8Functions)` to parameterless `Init()`. Each scene needs its `Init` signature updated and `p8.*` calls replaced with `Pico8.*` static calls.
- **CS0108** (2 errors, 1 file): `GenSeedCompetitive.worldSeed` hides `SpeedrunBase.worldSeed` — pre-existing, not caused by Plan D.

### Migration Order

Fix abstract bases first — each base fix cascades to all concrete subclasses.

#### Tier 0: Abstract Bases (fix first — cascading effect)

| # | File | Class | Fixes Subclasses |
|---|------|-------|------------------|
| | [ ] PcraftBase.cs | `PcraftBase` | PcraftSingleplayer, PcraftFilter, Visualiser, + SpeedrunBase chain |
| | [ ] DeluxeBase.cs | `DeluxeBase` | DeluxeSingleplayer |

> Fixing PcraftBase also fixes SpeedrunBase (inherits PcraftBase), which in turn fixes:
> PcraftSpeedrun, PcraftCompetitive, GenSeedCompetitive, LoadSeed

#### Tier 1: Simple Scenes (minimal logic, good for proving the pattern)

| # | File | Class | Area |
|---|------|-------|------|
| | [ ] ExitScene.cs | `ExitScene` | Root |
| | [ ] Template.cs | `Template` | Competitive |
| | [ ] MapTest.cs | `MapTest` | Root |
| | [ ] MapConversion.cs | `MapConversion` | Root |

#### Tier 2: Options Scenes (self-contained, similar structure)

| # | File | Class | Area |
|---|------|-------|------|
| | [ ] BackOptions1.cs | `BackOptions1` | OptionsMenu |
| | [ ] BackOptions2.cs | `BackOptions2` | OptionsMenu |
| | [ ] ControlsOptions.cs | `ControlsOptions` | OptionsMenu |
| | [ ] GeneralOptions.cs | `GeneralOptions` | OptionsMenu |
| | [ ] GeneralOptionsTitle.cs | `GeneralOptionsTitle` | OptionsMenu |
| | [ ] ControllerOptions.cs | `ControllerOptions` | OptionsMenu |
| | [ ] KeyboardOptions.cs | `KeyboardOptions` | OptionsMenu |

#### Tier 3: Core Scenes

| # | File | Class | Area |
|---|------|-------|------|
| | [ ] TitleScreen.cs | `TitleScreen` | Root |
| | [ ] CreditsScene.cs | `CreditsScene` | Credits |
| | [ ] SpeedrunScene.cs | `SpeedrunScene` | Competitive |

#### Tier 4: Competitive Scenes (most complex, many use Window/Viewport)

| # | File | Class | Area |
|---|------|-------|------|
| | [ ] CompetitiveScene.cs | `CompetitiveScene` | Competitive |
| | [ ] JoinRoomScene.cs | `JoinRoomScene` | Competitive |
| | [ ] LoginScene.cs | `LoginScene` | Competitive |
| | [ ] PickBanScene.cs | `PickBanScene` | Competitive |
| | [ ] PickBanSceneOld.cs | `PickBanSceneOld` | Competitive |
| | [ ] PrivateScene.cs | `PrivateScene` | Competitive |
| | [ ] ProfileScene.cs | `ProfileScene` | Competitive |
| | [ ] RankedScene.cs | `RankedScene` | Competitive |
| | [ ] ReplaysScene.cs | `ReplaysScene` | Competitive |
| | [ ] SearchScene.cs | `SearchScene` | Competitive |
| | [ ] SettingsScene.cs | `SettingsScene` | Competitive |
| | [ ] StatisticsScene.cs | `StatisticsScene` | Competitive |
| | [ ] UnrankedScene.cs | `UnrankedScene` | Competitive |

### Pre-existing Issues (not caused by Plan D)

- [ ] `GenSeedCompetitive.cs:19` — CS0108: `worldSeed` hides inherited member. Needs `new` keyword.

## API Coverage Summary

35 of 35 core PICO-8 methods matched between legacy and Plan D, plus 17 new additions
(Line, math helpers, display config, Initialize).

### Completed Abstractions

| Legacy Pattern | Plan D Replacement |
|----------------|-------------------|
| `p8.Btn(i)` / `p8.Btnp(i)` | `Pico8.Btn(i)` / `Pico8.Btnp(i)` |
| `p8.Cls/Circ/Circfill/Rect/Rectfill/Print/Spr/Sspr/Map/Pset/Memcpy/Reload` | `Pico8.*` static equivalents |
| `p8.Camera()` / `p8.Camera(x,y)` | `Pico8.Camera()` / `Pico8.Camera(x,y)` |
| `p8.Pal()` / `p8.Pal(c0,c1)` / `p8.Palt()` / `p8.Palt(col,t)` | `Pico8.Pal/Palt` |
| `p8.Mget/Mset/Fget` | `Pico8.Mget/Mset/Fget` |
| `p8.Sfx/Music/Mute` | `Pico8.Sfx/Music/Mute` |
| `p8.Add/Del/Srand/Mod` | `Pico8.Add/Del/Srand/Mod` |
| `p8.CartData/Cstore/Load/Dget/Dset` | `Pico8.CartData/Cstore/Load/Dget/Dset` (stubs) |
| `p8.ScheduleScene(...)` | `Pico8.ScheduleScene(...)` |
| `p8.Cell` | `Pico8.CellWidth` / `Pico8.CellHeight` ✅ |
| `p8.Resolution` | `Pico8.ResolutionWidth` / `Pico8.ResolutionHeight` ✅ |
| `p8.Colors[i]` | `Pico8.GetColor(i)` ✅ |

### Phase 5 Gaps (to address during scene migration)

#### High-Impact Infrastructure Gaps

| Legacy Pattern | Usages | Files | Plan D Replacement |
|----------------|:------:|:-----:|-------------------|
| `p8.Batch.Draw(p8.TextureDictionary[...])` | 169 + 84 | 28 + 20 | `Pico8.DrawTexture(name, ...)` — TBD |
| `p8.Window.ClientBounds` | ~30 | 16 | Viewport abstraction on GraphicsOrchestrator — TBD |
| `p8.Graphics` (GraphicsDeviceManager) | 10 | 1 | Display config methods — TBD |
| `p8.Pixel` | 4 | 1 | Likely unnecessary after DrawTexture — TBD |
| `p8.CameraOffset` | 6 | 2 | Already on GraphicsOrchestrator — wire to Pico8.cs |

#### Low-Impact API Method Gaps

| Method | Usages | Files | Notes |
|--------|:------:|:-----:|-------|
| `Menuitem(int, Func<string>, Action, ...)` | 5 | 2 | Pause menu customization |
| `PrintBig(string, int, int, Color)` | 3 | 2 | Large text rendering (non-PICO-8) |
| `Pal(Color, Color)` | 1 | 1 | Color overload (ProfileScene only) |
| `Rect(…, Color)` | 1 | 1 | Color overload (ProfileScene only) |
| `Rectfill(…, Color)` | 1 | 1 | Color overload (ProfileScene only) |
| `Sget` / `Sset` | 0 | 0 | Can be dropped |
| `Palt(Color, bool)` | 0 | 0 | Can be dropped |

#### Zero-Usage Legacy (safe to drop)

Track helpers (`SfxCount`, `MusicCount`, `LastMusicCall`, `GetCurrentSfxPackName`,
`DecrementSfxPack`, etc.), `PalColors`, `MusicDictionary`, `SoundEffectDictionary`,
`OptionsData`, `TitleSceneInstance`, `InputBindings`, `Settings`.

## Legacy Code to Remove (Phase 6)

| File | LOC | Purpose | Can Remove When |
|------|:---:|---------|----------------|
| Pico8Functions.cs | 928 | Legacy god class | All scenes migrated |
| Pico8Classes.cs | 527 | Legacy data classes | All scenes migrated |
| Services/*.cs | 1,455 | Legacy service layer | All scenes migrated |
| ReflectionAudioGraphicsSettings.cs | ~50 | Legacy reflection config | GameOrchestrator replaces it |

## Stale Documentation

These docs describe the legacy architecture or are outdated:

| File | Status | Action |
|------|--------|--------|
| ARCHITECTURE.md | Partially stale | Update with Plan D final architecture |
| DEVELOPER_GUIDE.md | Partially stale | Update after Phase 5 |
| IMPLEMENTATION_SUMMARY.md | Outdated | Rewrite or merge into this doc |
| SESSION_2_SUMMARY.md | Historical | Keep as-is or archive |
| TERMINAL_OUTPUT_SOLUTION.md | Narrow scope | Keep as-is |
| PHASE_3_PLAN.md | Completed | Archive or delete |
| VERIFICATION_CHECKLIST.md | Outdated | Update after Phase 5 |

## Migration Pattern

Each scene migration follows this pattern:

```csharp
// BEFORE (legacy)
public void Init(Pico8Functions p8) {
    this.p8 = p8;
    p8.Cls(0);
    p8.Print("hello", 10, 10, 7);
}

// AFTER (Plan D)
public void Init() {
    Pico8.Cls(0);
    Pico8.Print("hello", 10, 10, 7);
}
```

Key changes per scene:
1. Remove `Pico8Functions p8` parameter from `Init()`
2. Remove `this.p8 = p8;` field assignment
3. Replace all `p8.MethodName(...)` → `Pico8.MethodName(...)`
4. Replace `p8.Cell` → `Pico8.CellWidth` / `Pico8.CellHeight`
5. Replace `p8.Colors[i]` → `Pico8.GetColor(i)`
6. Replace `p8.Batch.Draw(p8.TextureDictionary[...])` → TBD (Phase 5 abstraction needed)
7. Replace `p8.Window.*` → TBD (Phase 5 abstraction needed)
