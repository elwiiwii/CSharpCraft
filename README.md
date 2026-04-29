# C# Craft

## Development Setup

This project depends on two external repositories that must be cloned as siblings to your home directory (i.e. `~/FNA` and `~/FixPointCS`).

### 1. Clone dependencies

```bash
git clone https://github.com/FNA-XNA/FNA.git ~/FNA
git clone https://github.com/XMunkki/FixPointCS.git ~/FixPointCS
```

### 2. Initialize FNA submodules

FNA uses git submodules for its C# bindings (SDL2-CS, SDL3-CS, FAudio, Theorafile, dav1dfile). Without this step the build will fail with `CS2001: Source file could not be found` errors.

```bash
cd ~/FNA
git submodule update --init --recursive
```

### 3. Clone this repo and PSharp8

Both workspace folders must be checked out:

```bash
git clone https://github.com/elwiiwii/CSharpCraft.git ~/Documents/Source/CSharpCraft
git clone https://github.com/elwiiwii/PSharp8.git ~/Documents/Source/PSharp8
```

### 4. Build

```bash
cd ~/Documents/Source/CSharpCraft
dotnet build CSharpCraft.slnx -c Debug
```

### 5. Run tests

```bash
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```

> **Note:** The `.runsettings` file pre-configures required FNA backends (`SDL3`, `SDLGPU`, `vulkan`) for tests on Linux/Wayland. Always run tests via the CLI, not the VS Code Testing view.