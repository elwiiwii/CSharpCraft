using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for ServiceFactory — the default implementation of IServiceFactory.
/// Verifies that each factory method returns the correct concrete type.
/// Note: Some factory methods require FNA types (SpriteBatch, GraphicsDevice, SoundEffect)
/// that can't be instantiated without a graphics context. Those are excluded.
/// </summary>
public class ServiceFactoryTests
{
    private readonly ServiceFactory _factory = new();

    // ── CreateAudioChannels ──

    [Fact]
    public void CreateAudioChannels_ReturnsNonNull()
    {
        var result = _factory.CreateAudioChannels();
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateAudioChannels_ReturnsNewInstanceEachCall()
    {
        var a = _factory.CreateAudioChannels();
        var b = _factory.CreateAudioChannels();
        a.Should().NotBeSameAs(b);
    }

    // ── CreateSpriteCache ──

    [Fact]
    public void CreateSpriteCache_ReturnsNonNull()
    {
        var result = _factory.CreateSpriteCache();
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateSpriteCache_ReturnsNewInstanceEachCall()
    {
        var a = _factory.CreateSpriteCache();
        var b = _factory.CreateSpriteCache();
        a.Should().NotBeSameAs(b);
    }

    // ── CreatePaletteManager ──

    [Fact]
    public void CreatePaletteManager_ReturnsNonNull()
    {
        var colors = new List<Color> { Color.Black, Color.White };
        var result = _factory.CreatePaletteManager(colors);
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreatePaletteManager_ReturnsCorrectType()
    {
        var colors = new List<Color> { Color.Black, Color.White };
        var result = _factory.CreatePaletteManager(colors);
        result.Should().BeOfType<PaletteManager>();
    }

    [Fact]
    public void CreatePaletteManager_PassesPaletteThrough()
    {
        var colors = new List<Color> { Color.Black, Color.Red, Color.Blue };
        var result = _factory.CreatePaletteManager(colors);
        result.StaticPalette.Should().BeSameAs(colors);
    }

    // ── CreateTrackManager ──

    [Fact]
    public void CreateTrackManager_ReturnsNonNull()
    {
        var settings = new Mock<IAudioGraphicsSettings>();
        var result = _factory.CreateTrackManager(
            () => new Dictionary<string, List<SongInst>>(),
            () => new Dictionary<string, Dictionary<int, string>>(),
            settings.Object);
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateTrackManager_ReturnsITrackManager()
    {
        var settings = new Mock<IAudioGraphicsSettings>();
        var result = _factory.CreateTrackManager(
            () => new Dictionary<string, List<SongInst>>(),
            () => new Dictionary<string, Dictionary<int, string>>(),
            settings.Object);
        result.Should().BeAssignableTo<ITrackManager>();
    }

    // ── CreateMapManager ──

    [Fact]
    public void CreateMapManager_ReturnsNonNull()
    {
        var result = _factory.CreateMapManager(new int[64], new int[256], (8, 8));
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateMapManager_ReturnsIMapManager()
    {
        var result = _factory.CreateMapManager(new int[64], new int[256], (8, 8));
        result.Should().BeAssignableTo<IMapManager>();
    }

    // ── CreatePauseMenuState ──

    [Fact]
    public void CreatePauseMenuState_ReturnsNonNull()
    {
        var context = new Mock<IPauseMenuContext>();
        context.Setup(c => c.Settings).Returns(new Mock<IAudioGraphicsSettings>().Object);
        context.Setup(c => c.Scenes).Returns(new List<IScene>());
        var result = _factory.CreatePauseMenuState(context.Object);
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreatePauseMenuState_ReturnsCorrectType()
    {
        var context = new Mock<IPauseMenuContext>();
        context.Setup(c => c.Settings).Returns(new Mock<IAudioGraphicsSettings>().Object);
        context.Setup(c => c.Scenes).Returns(new List<IScene>());
        var result = _factory.CreatePauseMenuState(context.Object);
        result.Should().BeOfType<PauseMenuState>();
    }

    // ── CreateInputStateManager ──

    [Fact]
    public void CreateInputStateManager_ReturnsNonNull()
    {
        var result = _factory.CreateInputStateManager();
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateInputStateManager_ReturnsIInputStateManager()
    {
        var result = _factory.CreateInputStateManager();
        result.Should().BeAssignableTo<IInputStateManager>();
    }

    [Fact]
    public void CreateInputStateManager_ReturnsNewInstanceEachCall()
    {
        var a = _factory.CreateInputStateManager();
        var b = _factory.CreateInputStateManager();
        a.Should().NotBeSameAs(b);
    }

    // ── CreateGameState ──

    [Fact]
    public void CreateGameState_ReturnsNonNull()
    {
        var scene = new Mock<IScene>().Object;
        var result = _factory.CreateGameState(
            scene, new int[1], new int[1], new Color[1],
            new Dictionary<string, List<SongInst>>(),
            new Dictionary<string, Dictionary<int, string>>());
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateGameState_ReturnsIGameState()
    {
        var scene = new Mock<IScene>().Object;
        var result = _factory.CreateGameState(
            scene, new int[1], new int[1], new Color[1],
            new Dictionary<string, List<SongInst>>(),
            new Dictionary<string, Dictionary<int, string>>());
        result.Should().BeAssignableTo<IGameState>();
    }

    // ── CreateOutputFacade ──

    [Fact]
    public void CreateOutputFacade_ReturnsNonNull()
    {
        var result = _factory.CreateOutputFacade(
            new Mock<IGraphicsAPI>().Object,
            new Mock<IAudioAPI>().Object);
        result.Should().NotBeNull();
    }

    [Fact]
    public void CreateOutputFacade_ReturnsIOutputFacade()
    {
        var result = _factory.CreateOutputFacade(
            new Mock<IGraphicsAPI>().Object,
            new Mock<IAudioAPI>().Object);
        result.Should().BeAssignableTo<IOutputFacade>();
    }
}
