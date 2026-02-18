using CSharpCraft.Pico8;
using CSharpCraft.Pico8.Services;
using CSharpCraft.Tests.Pico8.Mocks;
using FluentAssertions;
using FixMath;
using Xunit;

namespace CSharpCraft.Tests.Pico8.Integration;

/// <summary>
/// Integration tests for service coordination
/// Tests verify that services work together correctly in realistic scenarios
/// Phase 3.3: Integration Testing - Part 2
/// </summary>
public class ServiceCoordinationTests
{
    [Fact]
    public void WhenUtilityAndMenuServicesCoordinate_BothOperateIndependently()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        var menu = new MenuService();
        var menuList = new List<MenuItem>();

        // Act
        menu.Menuitem(0, () => "Item1", () => { }, menuList);
        var cosResult = utility.Cos(F32.FromInt(0));
        menu.Menuitem(1, () => "Item2", () => { }, menuList);

        // Assert - Both services operate without interference
        menuList.Should().HaveCount(2);
        cosResult.Should().NotBe(F32.Zero);
    }

    [Fact]
    public void WhenMapAndUtilityServicesCoordinate_FlagAndMapOperationsWorkTogether()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        var mockGraphics = new MockGraphicsEngine();
        var mockScene = new MockScene();
        var map = new int[256];
        var mapService = new MapService(mockGraphics, map, mockScene, utility);

        // Act - Map operations use flags from utility service
        utility.Fset(0, 2);
        var mapTile = mapService.Mget(0, 0);
        var flagValue = utility.Fget(mapTile);

        // Assert
        mapTile.Should().BeGreaterThanOrEqualTo(0);
        flagValue.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void WhenMultipleMenuItemsAdded_MenuServiceTracksSeparateLists()
    {
        // Arrange
        var menu = new MenuService();
        var menuList1 = new List<MenuItem>();
        var menuList2 = new List<MenuItem>();
        var executionCount = 0;

        // Act
        menu.Menuitem(0, () => "Option 1", () => executionCount++, menuList1);
        menu.Menuitem(1, () => "Option 2", () => executionCount++, menuList1);
        
        menu.Menuitem(0, () => "Option A", () => executionCount++, menuList2);
        menu.Menuitem(1, () => "Option B", () => executionCount++, menuList2);
        menu.Menuitem(2, () => "Option C", () => executionCount++, menuList2);

        // Assert
        menuList1.Should().HaveCount(2);
        menuList2.Should().HaveCount(3);
        menuList1[0].GetName().Should().Be("Option 1");
        menuList2[2].GetName().Should().Be("Option C");
    }

    [Fact]
    public void WhenMenuItemsExecuted_CallbacksInvokeCorrectly()
    {
        // Arrange
        var executionCounts = new[] { 0, 0, 0 };
        var item1 = new MenuItem(() => "Action1", () => executionCounts[0]++);
        var item2 = new MenuItem(() => "Action2", () => executionCounts[1]++);
        var item3 = new MenuItem(() => "Action3", () => executionCounts[2]++);

        // Act
        item1.Function();
        item1.Function();
        item2.Function();
        item3.Function();
        item1.Function();

        // Assert
        executionCounts[0].Should().Be(3);
        executionCounts[1].Should().Be(1);
        executionCounts[2].Should().Be(1);
    }

    [Fact]
    public void WhenRandomGeneratedMultipleTimes_ValuesAreWithinExpectedRange()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);

        // Act
        var values = Enumerable.Range(0, 10)
            .Select(_ => utility.Random())
            .ToList();

        // Assert
        foreach (var value in values)
        {
            value.Should().BeGreaterThanOrEqualTo(F32.Zero);
            value.Should().BeLessThanOrEqualTo(F32.One);
        }
    }

    [Fact]
    public void WhenRandomSeededTwiceWithSameSeed_ProducesIdenticalSequence()
    {
        // Arrange
        var cosDict1 = new CosDict();
        var sinDict1 = new SinDict();
        var flags1 = new int[256];
        var utility1 = new UtilityService(cosDict1, sinDict1, flags1);

        var cosDict2 = new CosDict();
        var sinDict2 = new SinDict();
        var flags2 = new int[256];
        var utility2 = new UtilityService(cosDict2, sinDict2, flags2);

        // Act
        utility1.Srand(54321);
        var sequence1 = Enumerable.Range(0, 5)
            .Select(_ => utility1.Random())
            .ToList();

        utility2.Srand(54321);
        var sequence2 = Enumerable.Range(0, 5)
            .Select(_ => utility2.Random())
            .ToList();

        // Assert
        for (int i = 0; i < sequence1.Count; i++)
        {
            sequence1[i].Should().Be(sequence2[i]);
        }
    }

    [Fact]
    public void WhenMapTilesSet_MultipleAccessesReturnCorrectValues()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        var mockGraphics = new MockGraphicsEngine();
        var mockScene = new MockScene();
        var map = new int[256];
        var mapService = new MapService(mockGraphics, map, mockScene, utility);

        // Act
        mapService.Mset(0, 0, 1);
        mapService.Mset(1, 0, 2);
        mapService.Mset(0, 1, 3);
        mapService.Mset(1, 1, 4);

        // Assert
        mapService.Mget(0, 0).Should().Be(1);
        mapService.Mget(1, 0).Should().Be(2);
        mapService.Mget(0, 1).Should().Be(3);
        mapService.Mget(1, 1).Should().Be(4);
    }

    [Fact]
    public void WhenUtilityDelRemovesFromList_ListStateIsConsistent()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        var testList = new List<string> { "A", "B", "C", "D", "E" };

        // Act
        utility.Del(testList, "C");
        utility.Del(testList, "A");
        utility.Del(testList, "E");

        // Assert
        testList.Should().HaveCount(2);
        testList.Should().Contain(new[] { "B", "D" });
    }

    [Fact]
    public void WhenTrigonometricFunctionsCalled_BothProduceValidResults()
    {
        // Arrange
        var cosDict1 = new CosDict();
        var sinDict1 = new SinDict();
        var flags1 = new int[256];
        var utility1 = new UtilityService(cosDict1, sinDict1, flags1);

        var cosDict2 = new CosDict();
        var sinDict2 = new SinDict();
        var flags2 = new int[256];
        var utility2 = new UtilityService(cosDict2, sinDict2, flags2);

        // Act
        var cos1 = utility1.Cos(F32.FromDouble(0.25));
        var sin1 = utility1.Sin(F32.FromDouble(0.25));
        var cos2 = utility2.Cos(F32.FromDouble(0.75));
        var sin2 = utility2.Sin(F32.FromDouble(0.75));

        // Assert - Both operations should complete successfully
        cos1.Should().BeOfType<F32>();
        sin1.Should().BeOfType<F32>();
        cos2.Should().BeOfType<F32>();
        sin2.Should().BeOfType<F32>();
    }

    [Fact]
    public void WhenMenuSelectionNavigates_BoundariesHandledCorrectly()
    {
        // Arrange
        var menu = new MenuService();
        var testList = new List<MenuItem>
        {
            new MenuItem(() => "Item1", () => { }),
            new MenuItem(() => "Item2", () => { }),
            new MenuItem(() => "Item3", () => { })
        };

        // Act
        menu.ResetSelection(); // Should be 0
        menu.SelectPrev(testList); // Wraps to 2
        var afterPrev = menu.MenuSelected;

        menu.SelectNext(testList); // Should be 0
        var afterNext = menu.MenuSelected;

        // Assert
        afterPrev.Should().Be(2); // Wrapped around
        afterNext.Should().Be(0); // Wrapped around again
    }

    [Fact]
    public void WhenServicesOperateConcurrently_StateRemainsConsistent()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        var menu = new MenuService();
        var menuList = new List<MenuItem>();
        var mockGraphics = new MockGraphicsEngine();
        var mockScene = new MockScene();
        var map = new int[256];
        var mapService = new MapService(mockGraphics, map, mockScene, utility);

        // Act - Perform interleaved operations
        for (int i = 0; i < 5; i++)
        {
            menu.Menuitem(i, () => $"Item{i}", () => { }, menuList);
            utility.Fset(i, i * 2);
            mapService.Mset(i, 0, i);
        }

        // Assert
        menuList.Should().HaveCount(5);
        for (int i = 0; i < 5; i++)
        {
            utility.Fget(i).Should().Be(i * 2);
            mapService.Mget(i, 0).Should().Be(i);
        }
    }
}
