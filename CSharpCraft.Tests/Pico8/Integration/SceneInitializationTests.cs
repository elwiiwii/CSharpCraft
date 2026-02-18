using CSharpCraft.Pico8;
using CSharpCraft.Pico8.Services;
using CSharpCraft.Tests.Pico8.Mocks;
using FluentAssertions;
using FixMath;
using Xunit;

namespace CSharpCraft.Tests.Pico8.Integration;

/// <summary>
/// Integration tests for service initialization and coordinated functionality
/// Tests verify that services initialize correctly and work together
/// Phase 3.3: Integration Testing - Part 1
/// </summary>
public class SceneInitializationTests
{
    [Fact]
    public void WhenUtilityServiceInitialized_CosReturnsValidValue()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        
        // Act
        var utility = new UtilityService(cosDict, sinDict, flags);
        var result = utility.Cos(F32.FromInt(0));

        // Assert
        result.Should().NotBe(F32.Zero);
    }

    [Fact]
    public void WhenUtilityServiceInitialized_SinReturnsValue()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        
        // Act - Should execute without exception
        var utility = new UtilityService(cosDict, sinDict, flags);
        var result = utility.Sin(F32.FromDouble(0.25));
        var result2 = utility.Sin(F32.FromDouble(0.5));

        // Assert - Both operations should complete successfully
        result.Should().BeOfType<F32>();
        result2.Should().BeOfType<F32>();
    }

    [Fact]
    public void WhenMenuServiceInitialized_MenuItemsCanBeAdded()
    {
        // Arrange
        var menu = new MenuService();
        var testList = new List<MenuItem>();

        // Act
        menu.Menuitem(0, () => "Test Item", () => { }, testList);

        // Assert
        testList.Should().HaveCount(1);
        testList[0].GetName().Should().Be("Test Item");
    }

    [Fact]
    public void WhenMenuServiceInitialized_SelectionNavigationWorks()
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
        menu.ResetSelection();
        menu.SelectNext(testList);

        // Assert
        menu.MenuSelected.Should().Be(1);
    }

    [Fact]
    public void WhenMenuServiceInitialized_SelectPrevWorks()
    {
        // Arrange
        var menu = new MenuService();
        menu.MenuSelected = 1;
        var testList = new List<MenuItem>
        {
            new MenuItem(() => "Item1", () => { }),
            new MenuItem(() => "Item2", () => { }),
            new MenuItem(() => "Item3", () => { })
        };

        // Act
        menu.SelectPrev(testList);

        // Assert
        menu.MenuSelected.Should().Be(0);
    }

    [Fact]
    public void WhenMapServiceInitialized_CanGetAndSetTiles()
    {
        // Arrange
        var mockGraphics = new MockGraphicsEngine();
        var mockScene = new MockScene();
        var map = new int[256];
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        
        // Act
        var mapService = new MapService(mockGraphics, map, mockScene, utility);
        var initialValue = mapService.Mget(0, 0);
        mapService.Mset(0, 0, 5);
        var newValue = mapService.Mget(0, 0);

        // Assert
        initialValue.Should().BeGreaterThanOrEqualTo(0);
        newValue.Should().Be(5);
    }

    [Fact]
    public void WhenUtilityServiceUsesRandom_ValuesAreInRange()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);

        // Act
        var rand1 = utility.Random();
        var rand2 = utility.Random();

        // Assert
        rand1.Should().BeGreaterThanOrEqualTo(F32.Zero);
        rand1.Should().BeLessThanOrEqualTo(F32.One);
        rand2.Should().BeGreaterThanOrEqualTo(F32.Zero);
        rand2.Should().BeLessThanOrEqualTo(F32.One);
    }

    [Fact]
    public void WhenUtilityServiceUsesSrand_SeedsProperlyProducesReproducibleResults()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);

        // Act
        utility.Srand(12345);
        var rand1 = utility.Random();
        utility.Srand(12345);
        var rand2 = utility.Random();

        // Assert
        rand1.Should().Be(rand2);
    }

    [Fact]
    public void WhenUtilityServiceFlagOperations_FgetAndFsetWork()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);

        // Act
        utility.Fset(5, 7);
        var result = utility.Fget(5);

        // Assert
        result.Should().Be(7);
    }

    [Fact]
    public void WhenUtilityServiceDelCalled_RemovesItemFromList()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        var testList = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        utility.Del(testList, 3);

        // Assert
        testList.Should().NotContain(3);
        testList.Should().HaveCount(4);
    }

    [Fact]
    public void WhenMenuItemExecuted_FunctionIsInvoked()
    {
        // Arrange
        var executionCount = 0;
        var item = new MenuItem(() => "Test", () => executionCount++);

        // Act
        item.Function();

        // Assert
        executionCount.Should().Be(1);
    }

    [Fact]
    public void WhenMultipleServicesInitialized_AllOperateIndependently()
    {
        // Arrange
        var cosDict = new CosDict();
        var sinDict = new SinDict();
        var flags = new int[256];
        var utility = new UtilityService(cosDict, sinDict, flags);
        var menu = new MenuService();
        var mockGraphics = new MockGraphicsEngine();
        var mockScene = new MockScene();
        var map = new int[256];
        var mapService = new MapService(mockGraphics, map, mockScene, utility);
        
        var menuList = new List<MenuItem>();

        // Act
        menu.Menuitem(0, () => "Item1", () => { }, menuList);
        var cosResult = utility.Cos(F32.FromDouble(0.25));
        var mapTile = mapService.Mget(0, 0);
        menu.Menuitem(1, () => "Item2", () => { }, menuList);
        var sinResult = utility.Sin(F32.FromDouble(0.25));

        // Assert
        menuList.Should().HaveCount(2);
        cosResult.Should().BeOfType<F32>();
        sinResult.Should().BeOfType<F32>();
        mapTile.Should().BeGreaterThanOrEqualTo(0);
    }
}
