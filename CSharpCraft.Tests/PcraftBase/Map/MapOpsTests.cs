using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Map;

public sealed class MapOpsPureTests
{
    // --------------------------------------------------------------------------
    #region GetMCoord
    // --------------------------------------------------------------------------

    [Fact]
    public void GetMCoord_ReturnsZeroZero_ForOrigin()
    {
        (int i, int j) = MapOps.GetMCoord(F32.Zero, F32.Zero);

        _ = i.Should().Be(0);
        _ = j.Should().Be(0);
    }

    [Fact]
    public void GetMCoord_ReturnsTileIndex_ForPixelCoordinates()
    {
        // pixel 32 = tile 2  (32/16 = 2)
        (int i, int j) = MapOps.GetMCoord(F32.FromInt(32), F32.FromInt(48));

        _ = i.Should().Be(2);
        _ = j.Should().Be(3);
    }

    [Fact]
    public void GetMCoord_FloorsDown_ForFractionalPixels()
    {
        // 31 / 16 = 1.9375 → floor = 1
        (int i, int j) = MapOps.GetMCoord(F32.FromInt(31), F32.FromInt(31));

        _ = i.Should().Be(1);
        _ = j.Should().Be(1);
    }

    [Fact]
    public void GetMCoord_ReturnsNegative_ForNegativeCoordinates()
    {
        // negative coords are allowed — bounds checks happen upstream
        (int i, int j) = MapOps.GetMCoord(F32.FromInt(-16), F32.FromInt(-32));

        _ = i.Should().Be(-1);
        _ = j.Should().Be(-2);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------

    private static Level MakeState(int sx, int sy)
    {
        return new(0, 0, sx, sy, LevelTheme.Surface);
    }
}

[Collection("Fna")]
public sealed class MapOpsTileTests(FnaFixture fixture)
{
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
        public string? SpritesPath => null;
        public string? MapPath => null;
        public string? FlagData => null;
        public IReadOnlyList<Soundtrack> Music => [];
        public IReadOnlyList<SfxPack> Sfx => [];
    }

    // --------------------------------------------------------------------------
    #region GetDirectTile
    // --------------------------------------------------------------------------

    [Fact]
    public void GetDirectTile_ReturnsWaterTile_WhenOutOfBounds_NegativeI()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);

        Tile result = MapOps.GetDirectTile(-1, 0, state);

        _ = result.Type.Should().BeSameAs(PcraftData.TileWater);
    }

    [Fact]
    public void GetDirectTile_ReturnsWaterTile_WhenOutOfBounds_NegativeJ()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);

        Tile result = MapOps.GetDirectTile(0, -1, state);

        _ = result.Type.Should().BeSameAs(PcraftData.TileWater);
    }

    [Fact]
    public void GetDirectTile_ReturnsWaterTile_WhenOutOfBounds_IAtLevelSx()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);

        Tile result = MapOps.GetDirectTile(8, 0, state);

        _ = result.Type.Should().BeSameAs(PcraftData.TileWater);
    }

    [Fact]
    public void GetDirectTile_ReturnsTileFromMap_WhenInBounds()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);
        state.Map[0, 0] = PcraftData.TileFor(TileId.Sand);

        Tile result = MapOps.GetDirectTile(0, 0, state);

        _ = result.Type.Should().BeSameAs(PcraftData.TileSand);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region IsFree / IsFreeEnem
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(0)]  // Water  — not wall -> free
    [InlineData(1)]  // Sand   — not wall -> free
    [InlineData(2)]  // Grass  — not wall -> free
    [InlineData(5)]  // Farm   — not wall -> free
    [InlineData(6)]  // Wheat  — not wall -> free
    [InlineData(11)] // Hole   — not wall -> free
    public void IsFree_ReturnsTrue_ForFloorOnlyTile(int tileId)
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);
        state.Map[0, 0] = PcraftData.TileFor((TileId)tileId);

        bool result = MapOps.IsFree(F32.Zero, F32.Zero, state);

        _ = result.Should().BeTrue();
    }

    [Theory]
    [InlineData(3)]  // Rock  — WallTileType -> blocked
    [InlineData(4)]  // Tree  — WallTileType -> blocked
    [InlineData(8)]  // Iron  — WallTileType -> blocked
    [InlineData(9)]  // Gold  — WallTileType -> blocked
    [InlineData(10)] // Gem   — WallTileType -> blocked
    public void IsFree_ReturnsFalse_ForSurfaceTile(int tileId)
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);
        state.Map[0, 0] = PcraftData.TileFor((TileId)tileId);

        bool result = MapOps.IsFree(F32.Zero, F32.Zero, state);

        _ = result.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)] // Sand  → free for enemies
    [InlineData(2)] // Grass → free for enemies
    public void IsFreeEnem_ReturnsTrue_ForPassableTile(int tileId)
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);
        state.Map[0, 0] = PcraftData.TileFor((TileId)tileId);

        bool result = MapOps.IsFreeEnem(F32.Zero, F32.Zero, state);

        _ = result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)] // Water → enemies cannot cross water
    [InlineData(3)] // Rock  → surface → blocked
    [InlineData(4)] // Tree  → surface → blocked
    public void IsFreeEnem_ReturnsFalse_ForBlockingTile(int tileId)
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        Level state = MakeState(levelX: 0, sx: 8, sy: 8);
        state.Map[0, 0] = PcraftData.TileFor((TileId)tileId);

        bool result = MapOps.IsFreeEnem(F32.Zero, F32.Zero, state);

        _ = result.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------

    private static Level MakeState(int levelX, int sx, int sy)
    {
        return new(levelX, 0, sx, sy, LevelTheme.Surface);
    }
}
