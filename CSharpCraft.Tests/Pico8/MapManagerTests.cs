using Xunit;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8;

public class MapManagerTests
{
    private MapManager CreateMapManager(int width = 8, int height = 8)
    {
        var mapData = new int[width * height];
        var flagData = new int[256]; // Standard sprite limit
        return new MapManager(mapData, flagData, (width, height));
    }

    // Tests for Mget()
    [Fact]
    public void Mget_WithValidCoordinates_ReturnsTileValue()
    {
        var mapData = new int[64];
        mapData[5 + 3 * 8] = 42; // Set tile at (5, 3)
        var manager = new MapManager(mapData, new int[256], (8, 8));

        int value = manager.Mget(5, 3);
        value.Should().Be(42);
    }

    [Fact]
    public void Mget_WithFloatCoordinates_FloorsToInteger()
    {
        var mapData = new int[64];
        mapData[5 + 3 * 8] = 99;
        var manager = new MapManager(mapData, new int[256], (8, 8));

        // 5.9 should floor to 5, 3.8 should floor to 3
        int value = manager.Mget(5.9, 3.8);
        value.Should().Be(99);
    }

    [Fact]
    public void Mget_WithNegativeCoordinates_Returns_Zero()
    {
        var manager = CreateMapManager();
        int value = manager.Mget(-1, 5);
        value.Should().Be(0);
    }

    [Fact]
    public void Mget_WithCoordinatesOutOfBounds_Returns_Zero()
    {
        var manager = CreateMapManager(8, 8);
        int value = manager.Mget(8, 5); // Beyond 8x8 grid
        value.Should().Be(0);
    }

    [Fact]
    public void Mget_WithBoundaryCoordinates_Returns_CorrectValue()
    {
        var mapData = new int[64];
        mapData[7 + 7 * 8] = 123; // Last cell in 8x8 grid
        var manager = new MapManager(mapData, new int[256], (8, 8));

        int value = manager.Mget(7, 7);
        value.Should().Be(123);
    }

    [Fact]
    public void Mget_WithZeroCoordinates_Returns_FirstTile()
    {
        var mapData = new int[64];
        mapData[0] = 55;
        var manager = new MapManager(mapData, new int[256], (8, 8));

        int value = manager.Mget(0, 0);
        value.Should().Be(55);
    }

    // Tests for Mset()
    [Fact]
    public void Mset_WithValidCoordinates_SetsValue()
    {
        var mapData = new int[64];
        var manager = new MapManager(mapData, new int[256], (8, 8));

        manager.Mset(3, 4, 77);
        mapData[3 + 4 * 8].Should().Be(77);
    }

    [Fact]
    public void Mset_WithNegativeCoordinates_DoesNothing()
    {
        var mapData = new int[64];
        var manager = new MapManager(mapData, new int[256], (8, 8));

        mapData[0] = 0;
        manager.Mset(-1, 5, 42);
        mapData[0].Should().Be(0); // Unchanged
    }

    [Fact]
    public void Mset_WithOutOfBoundsCoordinates_DoesNothing()
    {
        var mapData = new int[64];
        var manager = new MapManager(mapData, new int[256], (8, 8));

        // This should not crash and should not modify the array
        manager.Mset(10, 10, 42);
        mapData.Should().AllSatisfy(x => x.Should().Be(0));
    }

    [Fact]
    public void Mset_WithFloatCoordinates_FloorsValues()
    {
        var mapData = new int[64];
        var manager = new MapManager(mapData, new int[256], (8, 8));

        manager.Mset(3.9, 4.1, 88.7); // All should floor
        mapData[3 + 4 * 8].Should().Be(88);
    }

    [Fact]
    public void Mset_WithZeroAsDefault_SetsZero()
    {
        var mapData = new int[64];
        mapData[2 + 3 * 8] = 99;
        var manager = new MapManager(mapData, new int[256], (8, 8));

        manager.Mset(2, 3); // Default snum = 0
        mapData[2 + 3 * 8].Should().Be(0);
    }

    [Fact]
    public void Mset_WithBoundaryCoordinates_SetsValue()
    {
        var mapData = new int[64];
        var manager = new MapManager(mapData, new int[256], (8, 8));

        manager.Mset(7, 7, 200); // Last cell
        mapData[7 + 7 * 8].Should().Be(200);
    }

    // Tests for Fget()
    [Fact]
    public void Fget_WithValidSpriteIndex_ReturnsFlags()
    {
        var flagData = new int[256];
        flagData[42] = 0b00001111; // Set some flags
        var manager = new MapManager(new int[64], flagData, (8, 8));

        int flags = manager.Fget(42);
        flags.Should().Be(0b00001111);
    }

    [Fact]
    public void Fget_WithZeroIndex_ReturnsFlagsForFirstSprite()
    {
        var flagData = new int[256];
        flagData[0] = 0b10101010;
        var manager = new MapManager(new int[64], flagData, (8, 8));

        int flags = manager.Fget(0);
        flags.Should().Be(0b10101010);
    }

    [Fact]
    public void Fget_WithMaxSpriteIndex_ReturnsFlagsForLastSprite()
    {
        var flagData = new int[256];
        flagData[255] = 0b11110000;
        var manager = new MapManager(new int[64], flagData, (8, 8));

        int flags = manager.Fget(255);
        flags.Should().Be(0b11110000);
    }

    [Fact]
    public void Fget_WithNegativeIndex_Returns_Zero()
    {
        var manager = new MapManager(new int[64], new int[256], (8, 8));
        int flags = manager.Fget(-1);
        flags.Should().Be(0);
    }

    [Fact]
    public void Fget_WithIndexOutOfBounds_Returns_Zero()
    {
        var manager = new MapManager(new int[64], new int[256], (8, 8));
        int flags = manager.Fget(256); // Beyond 256 sprites
        flags.Should().Be(0);
    }

    [Fact]
    public void Fget_WithUnsetFlags_Returns_Zero()
    {
        var manager = new MapManager(new int[64], new int[256], (8, 8));
        int flags = manager.Fget(50);
        flags.Should().Be(0);
    }

    // Integration tests
    [Fact]
    public void MultipleOperations_Maintain_DataIntegrity()
    {
        var mapData = new int[64];
        var flagData = new int[256];
        var manager = new MapManager(mapData, flagData, (8, 8));

        // Set multiple tiles
        manager.Mset(0, 0, 10);
        manager.Mset(1, 1, 20);
        manager.Mset(2, 2, 30);

        // Verify all are set correctly
        manager.Mget(0, 0).Should().Be(10);
        manager.Mget(1, 1).Should().Be(20);
        manager.Mget(2, 2).Should().Be(30);

        // Modify one and verify others unchanged
        manager.Mset(1, 1, 99);
        manager.Mget(0, 0).Should().Be(10);
        manager.Mget(1, 1).Should().Be(99);
        manager.Mget(2, 2).Should().Be(30);
    }

    [Fact]
    public void LargeMapDimensions_WorkCorrectly()
    {
        var mapData = new int[1024]; // 32x32 map
        var flagData = new int[256];
        var manager = new MapManager(mapData, flagData, (32, 32));

        manager.Mset(31, 31, 128);
        manager.Mget(31, 31).Should().Be(128);
    }
}
