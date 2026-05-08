using CSharpCraft.PcraftFilter.Zones;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftFilter.Zones;

public sealed class RectangleZoneTests
{
    // --------------------------------------------------------------------------
    #region Contains
    // --------------------------------------------------------------------------

    [Fact]
    public void Contains_ReturnsTrue_ForTopLeftCorner()
    {
        var zone = new RectangleZone(2, 3, 4, 5);
        zone.Contains(2, 3).Should().BeTrue();
    }

    [Fact]
    public void Contains_ReturnsTrue_ForBottomRightInterior()
    {
        var zone = new RectangleZone(2, 3, 4, 5);
        // Last included cell: (2+4-1, 3+5-1) = (5, 7)
        zone.Contains(5, 7).Should().BeTrue();
    }

    [Fact]
    public void Contains_ReturnsFalse_ForCellOneStepOutsideRight()
    {
        var zone = new RectangleZone(2, 3, 4, 5);
        zone.Contains(6, 3).Should().BeFalse();
    }

    [Fact]
    public void Contains_ReturnsFalse_ForCellOneStepOutsideBottom()
    {
        var zone = new RectangleZone(2, 3, 4, 5);
        zone.Contains(2, 8).Should().BeFalse();
    }

    [Fact]
    public void Contains_ReturnsFalse_ForCellBeforeOriginX()
    {
        var zone = new RectangleZone(2, 3, 4, 5);
        zone.Contains(1, 3).Should().BeFalse();
    }

    [Fact]
    public void Contains_ReturnsFalse_ForCellBeforeOriginY()
    {
        var zone = new RectangleZone(2, 3, 4, 5);
        zone.Contains(2, 2).Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Cells
    // --------------------------------------------------------------------------

    [Fact]
    public void Cells_ReturnsExactlyWidthTimesHeightCells()
    {
        var zone = new RectangleZone(0, 0, 3, 4);
        zone.Cells(64, 64).Should().HaveCount(12);
    }

    [Fact]
    public void Cells_ReturnsOnlyContainedCells()
    {
        var zone = new RectangleZone(1, 1, 2, 2);
        var cells = zone.Cells(64, 64).ToList();
        cells.Should().BeEquivalentTo(new[] { (1, 1), (2, 1), (1, 2), (2, 2) });
    }

    [Fact]
    public void Cells_ClampsToGridBounds()
    {
        // Zone extends beyond the 4×4 grid
        var zone = new RectangleZone(3, 3, 4, 4);
        var cells = zone.Cells(4, 4).ToList();
        cells.Should().OnlyContain(c => c.x >= 0 && c.x < 4 && c.y >= 0 && c.y < 4);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region HashString
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_IncludesAllFourParameters()
    {
        var zone = new RectangleZone(1, 2, 3, 4);
        zone.HashString.Should().Be("rect.1.2.3.4");
    }

    [Fact]
    public void HashString_DiffersForDifferentZones()
    {
        var a = new RectangleZone(0, 0, 4, 4);
        var b = new RectangleZone(0, 0, 4, 5);
        a.HashString.Should().NotBe(b.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
}

public sealed class RadiusZoneTests
{
    // --------------------------------------------------------------------------
    #region Contains
    // --------------------------------------------------------------------------

    [Fact]
    public void Contains_ReturnsTrue_ForCenter()
    {
        var zone = new RadiusZone(5, 5, 3);
        zone.Contains(5, 5).Should().BeTrue();
    }

    [Fact]
    public void Contains_ReturnsTrue_ForCellOnCircumference()
    {
        // Distance from (5,5) to (8,5) == 3 (== radius)
        var zone = new RadiusZone(5, 5, 3);
        zone.Contains(8, 5).Should().BeTrue();
    }

    [Fact]
    public void Contains_ReturnsFalse_ForCellJustOutsideRadius()
    {
        // Distance from (5,5) to (9,5) == 4 > 3
        var zone = new RadiusZone(5, 5, 3);
        zone.Contains(9, 5).Should().BeFalse();
    }

    [Fact]
    public void Contains_ReturnsFalse_ForDiagonalOutsideRadius()
    {
        // Distance from (5,5) to (8,8) == sqrt(18) > 3
        var zone = new RadiusZone(5, 5, 3);
        zone.Contains(8, 8).Should().BeFalse();
    }

    [Fact]
    public void Contains_ReturnsTrue_ForDiagonalInsideRadius()
    {
        // Distance from (5,5) to (7,6) == sqrt(5) < 3
        var zone = new RadiusZone(5, 5, 3);
        zone.Contains(7, 6).Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Cells
    // --------------------------------------------------------------------------

    [Fact]
    public void Cells_ContainsOnlyPointsWithinRadius()
    {
        var zone = new RadiusZone(5, 5, 2);
        var cells = zone.Cells(64, 64).ToList();
        cells.Should().OnlyContain(c =>
            (c.x - 5) * (c.x - 5) + (c.y - 5) * (c.y - 5) <= 4);
    }

    [Fact]
    public void Cells_ContainsAllPointsWithinRadius()
    {
        var zone = new RadiusZone(5, 5, 2);
        // Manually enumerate expected cells
        var expected = new List<(int, int)>();
        for (int x = 3; x <= 7; x++)
            for (int y = 3; y <= 7; y++)
                if ((x - 5) * (x - 5) + (y - 5) * (y - 5) <= 4)
                    expected.Add((x, y));
        zone.Cells(64, 64).Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Cells_ClampsToGridBounds()
    {
        var zone = new RadiusZone(1, 1, 5);
        var cells = zone.Cells(4, 4).ToList();
        cells.Should().OnlyContain(c => c.x >= 0 && c.x < 4 && c.y >= 0 && c.y < 4);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region HashString
    // --------------------------------------------------------------------------

    [Fact]
    public void HashString_IncludesAllThreeParameters()
    {
        var zone = new RadiusZone(3, 7, 5);
        zone.HashString.Should().Be("rad.3.7.5");
    }

    [Fact]
    public void HashString_DiffersForDifferentZones()
    {
        var a = new RadiusZone(5, 5, 3);
        var b = new RadiusZone(5, 5, 4);
        a.HashString.Should().NotBe(b.HashString);
    }

    // --------------------------------------------------------------------------
    #endregion
}
