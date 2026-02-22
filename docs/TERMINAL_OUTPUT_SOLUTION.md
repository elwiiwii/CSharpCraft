# Terminal Output Visibility Solution

## Problem
Build and test commands appear to hang or produce no visible output despite commands executing successfully.

## Root Cause
When using pipes/redirects (e.g., `| head`, `| tail`, `> /tmp/output.log`), the .NET Terminal Logger (TL) auto-detects and buffers output, causing it to appear frozen or invisible.

## Solution
**Use the `--tl:off` flag to disable Terminal Logger auto-detection**

This flag tells dotnet to bypass TL buffering and output directly to console.

## Recommended Commands

### Development Build with Full Output
```bash
cd /home/me/Documents/Source/CSharpCraft
dotnet build CSharpCraft.Pico8 -c Debug --tl:off -v:minimal
```

### Run Tests with Full Output
```bash
cd /home/me/Documents/Source/CSharpCraft
dotnet test CSharpCraft.Tests -c Debug --tl:off --logger "console;verbosity=minimal"
```

### Run Tests with Fluent Assertions
```bash
cd /home/me/Documents/Source/CSharpCraft
dotnet test CSharpCraft.Tests -c Debug --tl:off --logger "console;verbosity=detailed"
```

### Build Specific Project
```bash
cd /home/me/Documents/Source/CSharpCraft
dotnet build CSharpCraft.Pico8 -c Debug --tl:off --no-restore
```

## Why This Works

The `--tl:off` flag:
- ✅ Disables Terminal Logger auto-detection
- ✅ Prevents output buffering
- ✅ Works with pipes/redirects without hanging
- ✅ Produces visible console output immediately
- ✅ Works in all terminal environments

## Historical Note
**Discovered:** February 17, 2026 (Session 2)
**Applied to:** All Phase 3+ builds and test runs
**Status:** Stable and verified across full test suite

---

**If you cannot see terminal output even with `--tl:off`:**
1. Try running command WITHOUT pipes first: `dotnet build CSharpCraft.Pico8 -c Debug --tl:off`
2. If that works, pipes might be the issue - use `2>&1` instead
3. Check terminal encoding: Make sure terminal supports UTF-8
4. Try: `dotnet build CSharpCraft.Pico8 -c Debug --tl:off 2>&1 | cat` (cat removes buffering)
