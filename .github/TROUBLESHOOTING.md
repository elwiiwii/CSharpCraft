# Troubleshooting CSharpCraft & PSharp8

## FNA3D Backend Selection Errors (Linux/Wayland)

### Error: `FNA3D error: no suitable driver found`
- **Cause**: Vulkan not available or backend not pre-selected
- **Fix**: Ensure `FNA_PLATFORM_BACKEND=SDL3` before test starts; check Vulkan is installed

### Error: `Test times out in VS Code Testing view`
- **Cause**: Backend probe hangs on EGL path
- **Fix**: Use shell task or CLI instead (`dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug`)

### Error: `Native library not found (SDL3, FNA3D, FAudio)`
- **Cause**: `FNAlibs/*.so.0` missing from repo
- **Fix**: Verify `git lfs` is installed and files pulled: `git lfs ls-files`

### Error: `Argument null exception in manager constructor`
- **Cause**: Missing null guard
- **Fix**: Add `?? throw new ArgumentNullException(...)`

### Error: `AsyncLocal orchestrator is null`
- **Cause**: Static API called before orchestrator initialized
- **Fix**: Ensure `GameOrchestrator.SetCurrent()` called in entry point

## Debugging Backend Issues

```bash
# Check what backend FNA actually selected (only visible in failed test output)
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug --verbosity=diag | grep -i fna

# Manually trigger backend selection
export FNA_PLATFORM_BACKEND=SDL3 FNA3D_FORCE_DRIVER=SDLGPU SDL_GPU_DRIVER=vulkan
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```
