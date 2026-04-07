using CSharpCraft.Pcraft;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.Pcraft;

[Collection("Fna")]
public sealed class PcraftGameInitTests(FnaFixture fixture) : IDisposable
{
    private TempMusicDirectory? _tempDir;

    // A test soundtrack with 5 tracks (indices 0–4), each backed by silent.ogg.
    // Track 4 is the one Init() plays via Pico8.Music(4, ...).
    private static readonly IReadOnlyList<Soundtrack> TestSoundtrack =
    [
        new Soundtrack("original",
        [
            new Track([new TrackPart("s0", loop: false)], channel: 0),
            new Track([new TrackPart("s1", loop: true)],  channel: 1),
            new Track([new TrackPart("s2", loop: true)],  channel: 2),
            new Track([new TrackPart("s3", loop: true)],  channel: 3),
            new Track([new TrackPart("s4", loop: true)],  channel: 4),
        ])
    ];

    private GameOrchestrator BuildOrchestratorWithMusic()
    {
        _tempDir = FnaFixture.CreateTempMusicDirectory(
            "s0.ogg", "s1.ogg", "s2.ogg", "s3.ogg", "s4.ogg");

        var orch = new GameOrchestrator(
            _tempDir.Path,
            ".",
            ".",
            new NullScene(),
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window);

        orch.LoadSoundtracks(TestSoundtrack, "original");
        return orch;
    }

    private sealed class NullScene : IScene
    {
        public string? Name => null;
        public void Init(ISceneSetup setup) { }
        public string? SpritesPath => null;
        public string? MapPath => null;
        public string? FlagData => null;
        public IReadOnlyList<Soundtrack> Music => [];
        public IReadOnlyList<SfxPack> Sfx => [];
    }

    public void Dispose() => _tempDir?.Dispose();

    // --------------------------------------------------------------------------
    #region CurMenu
    // --------------------------------------------------------------------------

    [Fact]
    public void Init_CurMenu_IsNullBeforeInit()
    {
        var sut = new PcraftGame();

        sut.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Init_SetsCurMenu_ToMainMenu()
    {
        using var orch = BuildOrchestratorWithMusic();
        Pico8.Initialize(orch);
        var sut = new PcraftGame();

        sut.Init();

        sut.CurMenu.Should().BeSameAs(PcraftData.MainMenu);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Recipes populated via InitRecipes
    // --------------------------------------------------------------------------

    [Fact]
    public void Init_PopulatesWorkbenchRecipe_WithTenEntries()
    {
        using var orch = BuildOrchestratorWithMusic();
        Pico8.Initialize(orch);
        var sut = new PcraftGame();

        sut.Init();

        sut.WorkbenchRecipe.Should().HaveCount(10);
    }

    [Fact]
    public void Init_PopulatesFurnaceRecipe_WithFourEntries()
    {
        using var orch = BuildOrchestratorWithMusic();
        Pico8.Initialize(orch);
        var sut = new PcraftGame();

        sut.Init();

        sut.FurnaceRecipe.Should().HaveCount(4);
    }

    // --------------------------------------------------------------------------
    #endregion
}
