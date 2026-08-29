using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftFilter;
using CSharpCraft.PcraftFilter.Filters;
using CSharpCraft.PcraftSeeded;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter;

// --------------------------------------------------------------------------
// Verifies that FilteredSeededServices produces a level whose spawn and tiles
// are consistent with FilteredWorldSampler for the same seed + FilterSet.
// Requires FNA (Pico8.Mget/Mset).
// --------------------------------------------------------------------------

[Collection("Fna")]
public sealed class FilteredSeededServicesTests(FnaFixture fixture)
{
    private const long Seed = 12345L;
    private const int Radius = 4;
    private static FilterSet Empty => new([]);

    // -----------------------------------------------------------------------
    #region Helpers
    // -----------------------------------------------------------------------

    private GameOrchestrator BuildOrchestrator()
    {
        return new(
            ".",
            ".",
            ".",
            new NullScene(),
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window);
    }

    private sealed class NullScene : IScene
    {
        public string? Name => null;
        public void Init(ISceneSetup setup) { }
        public void Update() { }
        public void Draw() { }
        public string? SpritesPath => null;
        public string? MapPath => null;
        public string? FlagData => null;
        public IReadOnlyList<Soundtrack> Music => [];
        public IReadOnlyList<SfxPack> Sfx => [];
    }

    // -----------------------------------------------------------------------
    #endregion
    // -----------------------------------------------------------------------
    #region Constructor argument validation
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenFiltersIsNull()
    {
        Action act = () => _ = new FilteredSeededServices(Seed, filters: null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("filters");
    }

    // -----------------------------------------------------------------------
    #endregion
    // -----------------------------------------------------------------------
    #region Spawn consistency with FilteredWorldSampler
    // -----------------------------------------------------------------------

    [Fact]
    public void ResetLevel_SpawnTile_MatchesFilteredWorldSampler_WithEmptyFilterSet()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        _ = new FilteredSeededServices(Seed, Empty);
        try
        {
            PlayerEntity player = new(F32.Zero, F32.Zero);
            PcraftSession.SetCurrent(new PcraftSession(player));
            PcraftServices.ResetLevel(player);

            SampleResult sample = FilteredWorldSampler.Sample(Seed, Radius, Empty);

            int spawnTileX = F32.FloorToInt(player.X / F32.FromInt(16));
            int spawnTileY = F32.FloorToInt(player.Y / F32.FromInt(16));

            _ = spawnTileX.Should().Be(sample.SpawnTileX,
                because: "island spawn X must match FilteredWorldSampler for the same seed and empty filters");
            _ = spawnTileY.Should().Be(sample.SpawnTileY,
                because: "island spawn Y must match FilteredWorldSampler for the same seed and empty filters");
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }
    }

    // -----------------------------------------------------------------------
    #endregion
    // -----------------------------------------------------------------------
    #region Tile consistency with FilteredWorldSampler
    // -----------------------------------------------------------------------

    [Fact]
    public void ResetLevel_IslandTiles_MatchFilteredWorldSampler_WithEmptyFilterSet()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);

        _ = new FilteredSeededServices(Seed, Empty);
        try
        {
            PlayerEntity player = new(F32.Zero, F32.Zero);
            PcraftSession.SetCurrent(new PcraftSession(player));
            PcraftServices.ResetLevel(player);
            Level island = PcraftSession.Current.Island!;

            SampleResult sample = FilteredWorldSampler.Sample(Seed, Radius, Empty,
                forceCenterX: 32, forceCenterY: 32);

            // SeededMapGenerator.CreateMap writes a 3×3 ladder hole at (32, 32). Skip it.
            const int holeX = 32;
            const int holeY = 32;

            int side = (2 * Radius) + 1;
            for (int i = 0; i < side; i++)
                for (int j = 0; j < side; j++)
                {
                    int tileX = sample.CenterTileX - Radius + i;
                    int tileY = sample.CenterTileY - Radius + j;

                    bool isHoleArea = Math.Abs(tileX - holeX) <= 1 && Math.Abs(tileY - holeY) <= 1;
                    if (isHoleArea) continue;

                    int expected = sample.Tiles[i, j];
                    int actual = PcraftData.TileIdFor(island.Map[tileX, tileY].Type);
                    _ = actual.Should().Be(expected,
                        because: $"tile ({tileX},{tileY}) must match FilteredWorldSampler output for seed {Seed}");
                }
        }
        finally
        {
            PcraftServices.SetServices(new PcraftServices());
        }
    }

    // -----------------------------------------------------------------------
    #endregion
    // -----------------------------------------------------------------------
    #region Service inheritance — DeluxeMapStep not overridden
    // -----------------------------------------------------------------------

    [Fact]
    public void FilteredSeededServices_IsSubclassOf_SeededServices()
    {
        _ = typeof(FilteredSeededServices).Should().BeDerivedFrom<SeededServices>(
            because: "FilteredSeededServices must inherit seeded level creation from SeededServices");
    }

    // -----------------------------------------------------------------------
    #endregion
}
