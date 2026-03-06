# CSharpCraft

## Testing

Graphics-dependent tests in `CSharpCraft.Tests` create a real FNA `GraphicsDevice`.
On this Linux/Wayland setup they require FNA/SDL backend selection to be present
before the test host starts.

The supported way to run the test suite is:

```bash
dotnet test CSharpCraft.Tests/CSharpCraft.Tests.csproj -c Debug
```

This works because the test project points at the repository root `.runsettings`
file, which sets the required startup environment for the test host.

### VS Code

Running the tests from VS Code's Testing view may still fail even though the CLI
works. The likely reason is that the C# Dev Kit test runner uses a different
execution path from `dotnet test` and does not reliably apply project-level
`.runsettings` / `RunSettingsFilePath` configuration before the FNA test host is
started.

When that happens, FNA3D falls back to its default backend probing during test
startup, which can hit the Wayland/EGL/OpenGL path and abort before the Vulkan
backend is selected.

If you want to run tests from inside VS Code, use the provided task instead of
the Testing view:

1. Open the Command Palette.
2. Run `Tasks: Run Task`.
3. Select `test: CSharpCraft.Tests`.

### Test configuration files

- `.runsettings`: startup environment for the VSTest host
- `CSharpCraft.Tests/Infrastructure/GraphicsFixture.cs`: in-process fallback via
	SDL hints and environment variables

If the VS Code Testing view starts honoring project runsettings in a future
extension update, it should be able to use the same configuration as the CLI.