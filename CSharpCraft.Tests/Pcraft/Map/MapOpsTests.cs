using CSharpCraft.Pcraft;
using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Map;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Map;

public sealed class MapOpsPureTests
{
    // --------------------------------------------------------------------------
    #region GetMCoord
    // --------------------------------------------------------------------------

    [Fact]
    public void GetMCoord_ReturnsZeroZero_ForOrigin()
    {
        var (i, j) = MapOps.GetMCoord(F32.Zero, F32.Zero);

        i.Should().Be(0);
        j.Should().Be(0);
    }

    [Fact]
    public void GetMCoord_ReturnsTileIndex_ForPixelCoordinates()
    {
        // pixel 32 = tile 2  (32/16 = 2)
        var (i, j) = MapOps.GetMCoord(F32.FromInt(32), F32.FromInt(48));

        i.Should().Be(2);
        j.Should().Be(3);
    }

    [Fact]
    public void GetMCoord_FloorsDown_ForFractionalPixels()
    {
        // 31 / 16 = 1.9375 → floor = 1
        var (i, j) = MapOps.GetMCoord(F32.FromInt(31), F32.FromInt(31));

        i.Should().Be(1);
        j.Should().Be(1);
    }

    [Fact]
    public void GetMCoord_ReturnsNegative_ForNegativeCoordinates()
    {
        // negative coords are allowed — bounds checks happen upstream
        var (i, j) = MapOps.GetMCoord(F32.FromInt(-16), F32.FromInt(-32));

        i.Should().Be(-1);
        j.Should().Be(-2);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region DirGetData / DirSetData
    // --------------------------------------------------------------------------

    [Fact]
    public void DirSetData_StoresValue_ByTileKey()
    {
        var state = MakeState(sx: 8, sy: 8);

        MapOps.DirSetData(2, 3, F32.FromInt(99), state);

        // key = i + j * sx = 2 + 3*8 = 26
        state.Data.Should().ContainKey(26)
            .WhoseValue.Should().Be(F32.FromInt(99));
    }

    [Fact]
    public void DirGetData_ReturnsStoredValue_WhenKeyExists()
    {
        var state = MakeState(sx: 8, sy: 8);
        MapOps.DirSetData(1, 1, F32.FromInt(7), state);

        var result = MapOps.DirGetData(1, 1, F32.FromInt(-1), state);

        result.Should().Be(F32.FromInt(7));
    }

    [Fact]
    public void DirGetData_ReturnsDefault_AndStoresIt_WhenKeyMissing()
    {
        var state = MakeState(sx: 8, sy: 8);
        var defaultVal = F32.FromInt(42);

        var result = MapOps.DirGetData(0, 0, defaultVal, state);

        result.Should().Be(defaultVal);
        state.Data.Should().ContainKey(0).WhoseValue.Should().Be(defaultVal);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetData / SetData / ClearData (via WorldState.Data, no Mget)
    // --------------------------------------------------------------------------

    [Fact]
    public void SetData_StoresValue_AtTileCoord()
    {
        var state = MakeState(sx: 8, sy: 8);

        MapOps.SetData(F32.FromInt(16), F32.FromInt(0), F32.FromFloat(3.5f), state);

        // tile (1, 0), key = 1 + 0*8 = 1
        state.Data.Should().ContainKey(1);
    }

    [Fact]
    public void SetData_IsNoOp_WhenCoordsOutOfBounds()
    {
        var state = MakeState(sx: 8, sy: 8);

        // negative x maps to tile -1 which is out of bounds
        MapOps.SetData(F32.FromInt(-1), F32.Zero, F32.FromInt(5), state);

        state.Data.Should().BeEmpty();
    }

    [Fact]
    public void SetData_IsNoOp_WhenCoordsAtOrBeyondLevelEdge()
    {
        var state = MakeState(sx: 8, sy: 8);

        // x=128 → tile 8 which is ≥ levelsx (8)
        MapOps.SetData(F32.FromInt(128), F32.Zero, F32.FromInt(5), state);

        state.Data.Should().BeEmpty();
    }

    [Fact]
    public void GetData_ReturnsDefault_WhenCoordsOutOfBounds()
    {
        var state = MakeState(sx: 8, sy: 8);
        var defaultVal = F32.FromInt(77);

        var result = MapOps.GetData(F32.FromInt(-1), F32.Zero, defaultVal, state);

        result.Should().Be(defaultVal);
    }

    [Fact]
    public void GetData_ReturnsDefault_WhenCoordsAtLevelEdge()
    {
        var state = MakeState(sx: 8, sy: 8);
        var defaultVal = F32.FromInt(10);

        // x=128 → tile 8 == levelsx (8) → out of bounds
        var result = MapOps.GetData(F32.FromInt(128), F32.Zero, defaultVal, state);

        result.Should().Be(defaultVal);
    }

    [Fact]
    public void GetData_RoundTrips_WithSetData()
    {
        var state = MakeState(sx: 8, sy: 8);
        var written = F32.FromFloat(1.5f);

        MapOps.SetData(F32.FromInt(16), F32.FromInt(16), written, state);
        var read = MapOps.GetData(F32.FromInt(16), F32.FromInt(16), F32.Zero, state);

        read.Should().Be(written);
    }

    [Fact]
    public void ClearData_RemovesEntry_FromData()
    {
        var state = MakeState(sx: 8, sy: 8);
        MapOps.SetData(F32.FromInt(0), F32.FromInt(0), F32.FromInt(5), state);

        MapOps.ClearData(F32.FromInt(0), F32.FromInt(0), state);

        state.Data.Should().BeEmpty();
    }

    [Fact]
    public void ClearData_IsNoOp_WhenCoordsOutOfBounds()
    {
        var state = MakeState(sx: 8, sy: 8);

        var act = () => MapOps.ClearData(F32.FromInt(-1), F32.Zero, state);

        act.Should().NotThrow();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------

    private static WorldState MakeState(int sx, int sy)
    {
        var state = new WorldState();
        var level = new Level(0, 0, sx, sy, false);
        state.SetLevel(level);
        return state;
    }
}

[Collection("Fna")]
public sealed class MapOpsFnaTests(FnaFixture fixture)
{
    private GameOrchestrator BuildOrchestrator()
        => new(
            ".",
            ".",
            ".",
            new NullScene(),
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window);

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
    #region GetDirectGr
    // --------------------------------------------------------------------------

    [Fact]
    public void GetDirectGr_ReturnsGrWater_WhenOutOfBounds_NegativeI()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);

        var result = MapOps.GetDirectGr(-1, 0, state);

        result.Should().Be(PcraftData.GrWater);
    }

    [Fact]
    public void GetDirectGr_ReturnsGrWater_WhenOutOfBounds_NegativeJ()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);

        var result = MapOps.GetDirectGr(0, -1, state);

        result.Should().Be(PcraftData.GrWater);
    }

    [Fact]
    public void GetDirectGr_ReturnsGrWater_WhenOutOfBounds_IAtLevelSx()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);

        var result = MapOps.GetDirectGr(8, 0, state);

        result.Should().Be(PcraftData.GrWater);
    }

    [Fact]
    public void GetDirectGr_ReturnsGrWater_WhenOutOfBounds_JAtLevelSy()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);

        var result = MapOps.GetDirectGr(0, 8, state);

        result.Should().Be(PcraftData.GrWater);
    }

    [Fact]
    public void GetDirectGr_ReturnsGroundFromMap_WhenInBounds()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);

        // Set tile (0,0) to GrSand sprite id=1
        Pico8.Mset(0, 0, PcraftData.GrSand.Id);

        var result = MapOps.GetDirectGr(0, 0, state);

        result.Should().Be(PcraftData.GrSand);
    }

    [Fact]
    public void GetDirectGr_RespectsLevelX_OffsetWhenReadingMap()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        // levelX = 10, so tile (0,0) reads from map cell (10, 0)
        var state = MakeState(levelX: 10, sx: 8, sy: 8);

        Pico8.Mset(10, 0, PcraftData.GrGrass.Id);

        var result = MapOps.GetDirectGr(0, 0, state);

        result.Should().Be(PcraftData.GrGrass);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetGr
    // --------------------------------------------------------------------------

    [Fact]
    public void GetGr_ReturnsGround_ForPixelCoordinates()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);

        // pixel (16, 0) → tile (1, 0)
        Pico8.Mset(1, 0, PcraftData.GrGrass.Id);

        var result = MapOps.GetGr(F32.FromInt(16), F32.Zero, state);

        result.Should().Be(PcraftData.GrGrass);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region SetGr
    // --------------------------------------------------------------------------

    [Fact]
    public void SetGr_WritesGroundId_ToMapTile()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);

        MapOps.SetGr(F32.Zero, F32.Zero, PcraftData.GrGrass, state);

        Pico8.Mget(0, 0).Should().Be(PcraftData.GrGrass.Id);
    }

    [Fact]
    public void SetGr_IsNoOp_WhenOutOfBounds()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);
        // Pre-set tile to GrSand
        Pico8.Mset(0, 0, PcraftData.GrSand.Id);

        // attempt write at negative coords
        MapOps.SetGr(F32.FromInt(-1), F32.Zero, PcraftData.GrGrass, state);

        Pico8.Mget(0, 0).Should().Be(PcraftData.GrSand.Id);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region IsFree / IsFreeEnem / IsCool
    // --------------------------------------------------------------------------

    [Theory]
    [InlineData(2)] // GrGrass — not a tree, not a rock → free
    [InlineData(1)] // GrSand  — not a tree, not a rock → free
    [InlineData(0)] // GrWater — not a tree, not a rock → free
    public void IsFree_ReturnsTrue_ForPassableTile(int groundId)
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);
        Pico8.Mset(0, 0, groundId);

        var result = MapOps.IsFree(F32.Zero, F32.Zero, state);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(4)] // GrTree (IsTree=true) → not free
    [InlineData(3)] // GrRock             → not free
    public void IsFree_ReturnsFalse_ForBlockingTile(int groundId)
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);
        Pico8.Mset(0, 0, groundId);

        var result = MapOps.IsFree(F32.Zero, F32.Zero, state);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(2)] // GrGrass → free for enemies
    [InlineData(1)] // GrSand  → free for enemies
    public void IsFreeEnem_ReturnsTrue_ForPassableTile(int groundId)
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);
        Pico8.Mset(0, 0, groundId);

        var result = MapOps.IsFreeEnem(F32.Zero, F32.Zero, state);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)] // GrWater → enemies cannot cross water
    [InlineData(3)] // GrRock  → not free
    [InlineData(4)] // GrTree  → not free
    public void IsFreeEnem_ReturnsFalse_ForBlockingTile(int groundId)
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);
        Pico8.Mset(0, 0, groundId);

        var result = MapOps.IsFreeEnem(F32.Zero, F32.Zero, state);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(4)] // GrTree → not free → iscool
    [InlineData(3)] // GrRock → not free → iscool
    public void IsCool_ReturnsTrue_WhenTileIsNotFree(int groundId)
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);
        Pico8.Mset(0, 0, groundId);

        var result = MapOps.IsCool(F32.Zero, F32.Zero, state);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsCool_ReturnsFalse_WhenTileIsFree()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = MakeState(levelX: 0, sx: 8, sy: 8);
        Pico8.Mset(0, 0, PcraftData.GrGrass.Id);

        var result = MapOps.IsCool(F32.Zero, F32.Zero, state);

        result.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    
    private static WorldState MakeState(int levelX, int sx, int sy)
    {
        var state = new WorldState();
        var level = new Level(levelX, 0, sx, sy, false);
        state.SetLevel(level);
        return state;
    }
}
