using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftSeeded;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftSeeded;

public sealed class SeededCaveTests
{
    // --------------------------------------------------------------------------
    #region Determinism
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateCaveMap_IsDeterministic_GivenSameSeed()
    {
        Level level1 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        PlayerEntity player1 = new(F32.Zero, F32.Zero);
        SeededMapGenerator.CreateCaveMap(level1, player1, 42L);

        Level level2 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        PlayerEntity player2 = new(F32.Zero, F32.Zero);
        SeededMapGenerator.CreateCaveMap(level2, player2, 42L);

        for (int i = 0; i < SeededMapGenerator.CaveGridSx; i++)
            for (int j = 0; j < SeededMapGenerator.CaveGridSy; j++)
                level1.Map[i, j].Should().Be(level2.Map[i, j],
                    because: $"same cave seed must produce identical tile at [{i},{j}]");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Seed independence
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateCaveMap_DifferentSeed_ProducesDifferentLayout()
    {
        Level level1 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(level1, new PlayerEntity(F32.Zero, F32.Zero), 42L);

        Level level2 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(level2, new PlayerEntity(F32.Zero, F32.Zero), 99L);

        bool anyDiffers = false;
        for (int i = 0; i < SeededMapGenerator.CaveGridSx && !anyDiffers; i++)
            for (int j = 0; j < SeededMapGenerator.CaveGridSy && !anyDiffers; j++)
                if (level1.Map[i, j] != level2.Map[i, j])
                    anyDiffers = true;

        anyDiffers.Should().BeTrue(because: "different cave seeds must produce different tile layouts");
    }

    [Fact]
    public void CreateCaveMap_IsIndependentFrom_IslandSeed()
    {
        // Generating an island map with different seeds must not affect a subsequent
        // cave map generated with the same cave seed.
        long caveSeed = 42L;

        Level island1 = new(0, 0, SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy, LevelTheme.Surface);
        SeededMapGenerator.CreateMap(island1, new PlayerEntity(F32.Zero, F32.Zero), 1L);

        Level cave1 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(cave1, new PlayerEntity(F32.Zero, F32.Zero), caveSeed);

        Level island2 = new(0, 0, SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy, LevelTheme.Surface);
        SeededMapGenerator.CreateMap(island2, new PlayerEntity(F32.Zero, F32.Zero), 999L);

        Level cave2 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(cave2, new PlayerEntity(F32.Zero, F32.Zero), caveSeed);

        for (int i = 0; i < SeededMapGenerator.CaveGridSx; i++)
            for (int j = 0; j < SeededMapGenerator.CaveGridSy; j++)
                cave1.Map[i, j].Should().Be(cave2.Map[i, j],
                    because: $"cave map must not be affected by island seed at [{i},{j}]");
    }

    [Fact]
    public void CreateIslandMap_IsIndependentFrom_CaveSeed()
    {
        // Generating cave maps with different seeds must not affect a subsequent
        // island map generated with the same island seed.
        long islandSeed = 42L;

        Level cave1 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(cave1, new PlayerEntity(F32.Zero, F32.Zero), 1L);

        Level island1 = new(0, 0, SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy, LevelTheme.Surface);
        SeededMapGenerator.CreateMap(island1, new PlayerEntity(F32.Zero, F32.Zero), islandSeed);

        Level cave2 = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(cave2, new PlayerEntity(F32.Zero, F32.Zero), 999L);

        Level island2 = new(0, 0, SeededMapGenerator.IslandGridSx, SeededMapGenerator.IslandGridSy, LevelTheme.Surface);
        SeededMapGenerator.CreateMap(island2, new PlayerEntity(F32.Zero, F32.Zero), islandSeed);

        for (int i = 0; i < SeededMapGenerator.IslandGridSx; i++)
            for (int j = 0; j < SeededMapGenerator.IslandGridSy; j++)
                island1.Map[i, j].Should().Be(island2.Map[i, j],
                    because: $"island map must not be affected by cave seed at [{i},{j}]");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Expected tile types
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateCaveMap_ContainsOnlyCaveTileTypes()
    {
        Level level = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(level, new PlayerEntity(F32.Zero, F32.Zero), 42L);

        TileType[] validTypes =
        [
            PcraftData.TileRock,
            PcraftData.TileIron,
            PcraftData.TileSand,
            PcraftData.TileGold,
            PcraftData.TileGem,
            PcraftData.TileHole,
        ];

        for (int i = 0; i < SeededMapGenerator.CaveGridSx; i++)
            for (int j = 0; j < SeededMapGenerator.CaveGridSy; j++)
                validTypes.Should().Contain(level.Map[i, j].Type,
                    because: $"cave tile at [{i},{j}] must be a valid cave type (no Water/Grass/Tree)");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Ladder placement
    // --------------------------------------------------------------------------

    [Fact]
    public void CreateCaveMap_LadderHole_IsAtGridCenter()
    {
        Level level = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(level, new PlayerEntity(F32.Zero, F32.Zero), 42L);

        int cx = SeededMapGenerator.CaveGridSx / 2;
        int cy = SeededMapGenerator.CaveGridSy / 2;

        level.Map[cx, cy].Type.Should().BeSameAs(PcraftData.TileHole,
            because: "cave ladder hole must be placed at the grid centre");
    }

    [Fact]
    public void CreateCaveMap_LadderSurround_IsSand()
    {
        Level level = new(0, 0, SeededMapGenerator.CaveGridSx, SeededMapGenerator.CaveGridSy, LevelTheme.Cave);
        SeededMapGenerator.CreateCaveMap(level, new PlayerEntity(F32.Zero, F32.Zero), 42L);

        int cx = SeededMapGenerator.CaveGridSx / 2;
        int cy = SeededMapGenerator.CaveGridSy / 2;

        for (int di = -1; di <= 1; di++)
            for (int dj = -1; dj <= 1; dj++)
                if (di != 0 || dj != 0)
                    level.Map[cx + di, cy + dj].Type.Should().BeSameAs(PcraftData.TileSand,
                        because: $"cave ladder tile at [{cx + di},{cy + dj}] must be Sand");
    }

    // --------------------------------------------------------------------------
    #endregion
}
