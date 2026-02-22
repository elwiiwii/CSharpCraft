using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using FixMath;
using Color = Microsoft.Xna.Framework.Color;

namespace CSharpCraft.Tests.Pico8;

public class GameStateContainerTests
{
    private static Mock<IScene> CreateMockScene(string name = "TestScene")
    {
        var mock = new Mock<IScene>();
        mock.Setup(s => s.SceneName).Returns(name);
        mock.Setup(s => s.Fps).Returns(60.0);
        mock.Setup(s => s.Resolution).Returns((128, 128));
        mock.Setup(s => s.SpriteData).Returns("");
        mock.Setup(s => s.SpriteImage).Returns("");
        mock.Setup(s => s.FlagData).Returns("");
        mock.Setup(s => s.MapDimensions).Returns((0, 0));
        mock.Setup(s => s.MapData).Returns("");
        mock.Setup(s => s.Music).Returns(new Dictionary<string, List<SongInst>>());
        mock.Setup(s => s.Sfx).Returns(new Dictionary<string, Dictionary<int, string>>());
        return mock;
    }

    private GameStateContainer CreateContainer(
        IScene? scene = null,
        int[]? mapData = null,
        int[]? flagData = null,
        Color[]? sprites = null)
    {
        return new GameStateContainer(
            scene ?? CreateMockScene().Object,
            mapData ?? new int[64],
            flagData ?? new int[256],
            sprites ?? new Color[128 * 128],
            new Dictionary<string, List<SongInst>>(),
            new Dictionary<string, Dictionary<int, string>>());
    }

    // ── Constructor ──

    [Fact]
    public void Constructor_StoresScene()
    {
        var scene = CreateMockScene("TestScene").Object;
        var container = CreateContainer(scene: scene);
        container.CurrentScene.Should().BeSameAs(scene);
    }

    [Fact]
    public void Constructor_StoresMapData()
    {
        var mapData = new int[] { 1, 2, 3 };
        var container = CreateContainer(mapData: mapData);
        container.MapData.Should().BeSameAs(mapData);
    }

    [Fact]
    public void Constructor_StoresFlagData()
    {
        var flagData = new int[] { 10, 20, 30 };
        var container = CreateContainer(flagData: flagData);
        container.FlagData.Should().BeSameAs(flagData);
    }

    [Fact]
    public void Constructor_StoresSprites()
    {
        var sprites = new Color[] { Color.Red, Color.Blue };
        var container = CreateContainer(sprites: sprites);
        container.Sprites.Should().BeSameAs(sprites);
    }

    [Fact]
    public void Constructor_StoresMusic()
    {
        var music = new Dictionary<string, List<SongInst>> { ["track1"] = [] };
        var container = new GameStateContainer(
            CreateMockScene().Object, new int[1], new int[1],
            new Color[1], music, new Dictionary<string, Dictionary<int, string>>());
        container.Music.Should().BeSameAs(music);
    }

    [Fact]
    public void Constructor_StoresSfx()
    {
        var sfx = new Dictionary<string, Dictionary<int, string>> { ["sfx1"] = new() };
        var container = new GameStateContainer(
            CreateMockScene().Object, new int[1], new int[1],
            new Color[1], new Dictionary<string, List<SongInst>>(), sfx);
        container.Sfx.Should().BeSameAs(sfx);
    }

    // ── Constructor null guards ──

    [Fact]
    public void Constructor_ThrowsOnNullScene()
    {
        var act = () => new GameStateContainer(
            null!, new int[1], new int[1], new Color[1],
            new Dictionary<string, List<SongInst>>(),
            new Dictionary<string, Dictionary<int, string>>());
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("initialScene");
    }

    [Fact]
    public void Constructor_ThrowsOnNullMapData()
    {
        var act = () => new GameStateContainer(
            CreateMockScene().Object, null!, new int[1], new Color[1],
            new Dictionary<string, List<SongInst>>(),
            new Dictionary<string, Dictionary<int, string>>());
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("mapData");
    }

    [Fact]
    public void Constructor_ThrowsOnNullFlagData()
    {
        var act = () => new GameStateContainer(
            CreateMockScene().Object, new int[1], null!, new Color[1],
            new Dictionary<string, List<SongInst>>(),
            new Dictionary<string, Dictionary<int, string>>());
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("flagData");
    }

    [Fact]
    public void Constructor_ThrowsOnNullSprites()
    {
        var act = () => new GameStateContainer(
            CreateMockScene().Object, new int[1], new int[1], null!,
            new Dictionary<string, List<SongInst>>(),
            new Dictionary<string, Dictionary<int, string>>());
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("sprites");
    }

    [Fact]
    public void Constructor_ThrowsOnNullMusic()
    {
        var act = () => new GameStateContainer(
            CreateMockScene().Object, new int[1], new int[1], new Color[1],
            null!, new Dictionary<string, Dictionary<int, string>>());
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("music");
    }

    [Fact]
    public void Constructor_ThrowsOnNullSfx()
    {
        var act = () => new GameStateContainer(
            CreateMockScene().Object, new int[1], new int[1], new Color[1],
            new Dictionary<string, List<SongInst>>(), null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("sfx");
    }

    // ── Default values ──

    [Fact]
    public void Constructor_DefaultCameraOffset_IsZero()
    {
        var container = CreateContainer();
        container.CameraOffset.x.Should().Be(F32.Zero);
        container.CameraOffset.y.Should().Be(F32.Zero);
    }

    [Fact]
    public void Constructor_DefaultResolution_Is128x128()
    {
        var container = CreateContainer();
        container.Resolution.Should().Be((128, 128));
    }

    [Fact]
    public void Constructor_DefaultCell_Is1x1()
    {
        var container = CreateContainer();
        container.Cell.Should().Be((1, 1));
    }

    // ── CameraOffset ──

    [Fact]
    public void CameraOffset_CanBeSet()
    {
        var container = CreateContainer();
        container.CameraOffset = (F32.FromInt(10), F32.FromInt(20));
        container.CameraOffset.x.Should().Be(F32.FromInt(10));
        container.CameraOffset.y.Should().Be(F32.FromInt(20));
    }

    // ── Cell ──

    [Fact]
    public void Cell_CanBeSet()
    {
        var container = CreateContainer();
        container.Cell = (8, 8);
        container.Cell.Should().Be((8, 8));
    }

    // ── SetCurrentScene ──

    [Fact]
    public void SetCurrentScene_UpdatesCurrentScene()
    {
        var container = CreateContainer();
        var newScene = CreateMockScene("NewScene").Object;
        container.SetCurrentScene(newScene);
        container.CurrentScene.Should().BeSameAs(newScene);
    }

    [Fact]
    public void SetCurrentScene_ThrowsOnNull()
    {
        var container = CreateContainer();
        var act = () => container.SetCurrentScene(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetCurrentScene_CanBeCalledMultipleTimes()
    {
        var container = CreateContainer();
        var scene1 = CreateMockScene("Scene1").Object;
        var scene2 = CreateMockScene("Scene2").Object;
        container.SetCurrentScene(scene1);
        container.SetCurrentScene(scene2);
        container.CurrentScene.Should().BeSameAs(scene2);
    }
}
