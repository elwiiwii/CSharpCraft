using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftDeluxe;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase;

public sealed class DeluxeDataTests
{
    // --------------------------------------------------------------------------
    #region Inheritance
    // --------------------------------------------------------------------------

    [Fact]
    public void DeluxeData_IsSubclassOf_PcraftData()
    {
        _ = typeof(DeluxeData).Should().BeDerivedFrom<PcraftData>(
            because: "DeluxeData must extend the base data class to enable overrides");
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region SetData — dispatch to subclass
    // --------------------------------------------------------------------------

    [Fact]
    public void SetData_TileFor_ReturnsBaseResult_WhenDeluxeDataHasNoOverrides()
    {
        // Capture baseline before swapping.
        Tile baseline = PcraftData.TileFor(TileId.Grass);

        DeluxeData deluxe = new();
        PcraftData.SetData(deluxe);
        try
        {
            // DeluxeData has no overrides yet — must fall through to base result.
            Tile actual = PcraftData.TileFor(TileId.Grass);
            _ = actual.Type.Should().Be(baseline.Type,
                because: "DeluxeData with no overrides must produce the same tile as the base PcraftData");
        }
        finally
        {
            PcraftData.SetData(new PcraftData());
        }
    }

    [Fact]
    public void SetData_TileIdFor_ReturnsBaseResult_WhenDeluxeDataHasNoOverrides()
    {
        int baseline = PcraftData.TileIdFor(PcraftData.TileGrass);

        DeluxeData deluxe = new();
        PcraftData.SetData(deluxe);
        try
        {
            int actual = PcraftData.TileIdFor(PcraftData.TileGrass);
            _ = actual.Should().Be(baseline,
                because: "DeluxeData with no overrides must produce the same tile id as the base PcraftData");
        }
        finally
        {
            PcraftData.SetData(new PcraftData());
        }
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Constructor null guard on SetData
    // --------------------------------------------------------------------------

    [Fact]
    public void SetData_ThrowsArgumentNullException_WhenDataIsNull()
    {
        Action act = () => PcraftData.SetData(null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("d");
    }

    // --------------------------------------------------------------------------
    #endregion
}
