using CSharpCraft.Settings;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using Xunit;

namespace CSharpCraft.Tests.Settings;

public sealed class SettingsManagerTests
{
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenConfigDirectoryIsNull()
    {
        Func<SettingsManager> act = () => new SettingsManager(
            configDirectory: null!,
            orchestrator: null!,
            graphicsDeviceManager: null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("configDirectory");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenOrchestratorIsNull()
    {
        Func<SettingsManager> act = () => new SettingsManager(
            configDirectory: ".",
            orchestrator: null!,
            graphicsDeviceManager: null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("orchestrator");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
}

[Collection("Fna")]
public sealed class SettingsManagerFnaTests(FnaFixture fixture) : IDisposable
{
    private readonly string _tempDir =
        Path.Combine(Path.GetTempPath(), $"CSharpCraft.Tests_{Guid.NewGuid():N}");
    private GameOrchestrator? _lastOrchestrator;

    public void Dispose()
    {
        _lastOrchestrator?.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private SettingsManager CreateSut(string? configDir = null)
    {
        _lastOrchestrator?.Dispose();
        _lastOrchestrator = new GameOrchestrator(
            ".", ".", ".", new EmptyScene(),
            fixture.GraphicsDevice, fixture.GraphicsDeviceManager, fixture.Window);
        return new(
            configDirectory: configDir ?? _tempDir,
            orchestrator: _lastOrchestrator,
            graphicsDeviceManager: fixture.GraphicsDeviceManager);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenGraphicsDeviceManagerIsNull()
    {
        Action act = () =>
        {
            using GameOrchestrator orch = new(
                ".", ".", ".", new EmptyScene(),
                fixture.GraphicsDevice, fixture.GraphicsDeviceManager, fixture.Window);
            _ = new SettingsManager(".", orch, graphicsDeviceManager: null!);
        };
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("graphicsDeviceManager");
    }

    [Fact]
    public void Constructor_CreatesGeneralJson_WhenFileDoesNotExist()
    {
        using SettingsManager sut = CreateSut();

        _ = File.Exists(Path.Combine(_tempDir, "general.json")).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #region Load existing file
    // --------------------------------------------------------------------------

    [Fact]
    public void GeneralSettings_ReturnsDefaults_WhenCreatedFromEmptyDirectory()
    {
        using SettingsManager sut = CreateSut();

        _ = sut.GeneralSettings.MusicVolume.Should().Be(100);
        _ = sut.GeneralSettings.SfxVolume.Should().Be(100);
        _ = sut.GeneralSettings.WindowWidth.Should().Be(512);
        _ = sut.GeneralSettings.WindowHeight.Should().Be(512);
        _ = sut.GeneralSettings.Fullscreen.Should().BeFalse();
    }

    [Fact]
    public void Constructor_LoadsExistingGeneralJson_AndExposesSettings()
    {
        _ = Directory.CreateDirectory(_tempDir);
        File.WriteAllText(
            Path.Combine(_tempDir, "general.json"),
            """{"MusicVolume":60,"SfxVolume":40,"WindowWidth":1024,"WindowHeight":768,"Fullscreen":false}""");

        using SettingsManager sut = CreateSut();

        _ = sut.GeneralSettings.MusicVolume.Should().Be(60);
        _ = sut.GeneralSettings.SfxVolume.Should().Be(40);
        _ = sut.GeneralSettings.WindowWidth.Should().Be(1024);
        _ = sut.GeneralSettings.WindowHeight.Should().Be(768);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Apply settings to GraphicsDeviceManager
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_AppliesWindowWidth_ToGraphicsDeviceManager()
    {
        _ = Directory.CreateDirectory(_tempDir);
        File.WriteAllText(
            Path.Combine(_tempDir, "general.json"),
            """{"MusicVolume":100,"SfxVolume":100,"WindowWidth":800,"WindowHeight":600,"Fullscreen":false}""");

        using SettingsManager sut = CreateSut();

        _ = fixture.GraphicsDeviceManager.PreferredBackBufferWidth.Should().Be(800);
    }

    [Fact]
    public void Constructor_AppliesWindowHeight_ToGraphicsDeviceManager()
    {
        _ = Directory.CreateDirectory(_tempDir);
        File.WriteAllText(
            Path.Combine(_tempDir, "general.json"),
            """{"MusicVolume":100,"SfxVolume":100,"WindowWidth":800,"WindowHeight":600,"Fullscreen":false}""");

        using SettingsManager sut = CreateSut();

        _ = fixture.GraphicsDeviceManager.PreferredBackBufferHeight.Should().Be(600);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Hot reload settings from file
    // --------------------------------------------------------------------------

    [Fact]
    public async Task Update_AppliesNewWindowWidth_AfterFileChange()
    {
        using SettingsManager sut = CreateSut();

        File.WriteAllText(
            Path.Combine(_tempDir, "general.json"),
            """{"MusicVolume":100,"SfxVolume":100,"WindowWidth":640,"WindowHeight":480,"Fullscreen":false}""");
        await Task.Delay(500, TestContext.Current.CancellationToken); // wait > 300ms debounce

        sut.Update();

        _ = fixture.GraphicsDeviceManager.PreferredBackBufferWidth.Should().Be(640);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Dispose
    // --------------------------------------------------------------------------

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        SettingsManager sut = CreateSut();
        Action act = () => sut.Dispose();
        _ = act.Should().NotThrow();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
}
