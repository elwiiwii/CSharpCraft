using CSharpCraft.MenuShared;
using FluentAssertions;
using Microsoft.Xna.Framework;
using Xunit;

namespace CSharpCraft.Tests.MainMenu;

public sealed class MainMenuHoverTests
{
    private const float Radius = 4f;

    // --------------------------------------------------------------------------
    #region Rect corners helper
    // --------------------------------------------------------------------------

    [Fact]
    public void GetRectCorners_ReturnsFourCorners_InClockwiseOrder()
    {
        Vector2[] corners = MenuButtonGeometry.GetRectCorners(10, 20, 30, 40);

        corners.Should().HaveCount(4);
        corners[0].Should().Be(new Vector2(10, 20));     // top-left
        corners[1].Should().Be(new Vector2(40, 20));     // top-right
        corners[2].Should().Be(new Vector2(40, 60));     // bottom-right
        corners[3].Should().Be(new Vector2(10, 60));     // bottom-left
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Label bounds helper
    // --------------------------------------------------------------------------

    [Fact]
    public void GetLabelBounds_ReturnsCorrectBounds_ForNonEmptyLabel()
    {
        // P8SCII default: 4 px per character wide, 6 px tall
        var bounds = MenuButtonGeometry.GetLabelBounds("hello", 37, 30);
        bounds.Should().NotBeNull();
        bounds.Value.X.Should().Be(37);
        bounds.Value.Y.Should().Be(30);
        bounds.Value.W.Should().Be(20); // 5 chars * 4 px
        bounds.Value.H.Should().Be(6);
    }

    [Fact]
    public void GetLabelBounds_ReturnsNull_WhenLabelIsEmpty()
    {
        MenuButtonGeometry.GetLabelBounds("", 0, 0).Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Convex hull
    // --------------------------------------------------------------------------

    [Fact]
    public void ConvexHull_ReturnsFourCorners_ForSingleRectangle()
    {
        Vector2[] corners = MenuButtonGeometry.GetRectCorners(0, 0, 10, 10);

        Vector2[] hull = MenuButtonGeometry.ConvexHull(corners);

        hull.Should().HaveCount(4);
        hull.Should().BeEquivalentTo(corners);
    }

    [Fact]
    public void ConvexHull_ExcludesInteriorPoints()
    {
        // A rectangle with extra point inside — hull should still have 4 corners
        Vector2[] points =
        [
            new(0, 0), new(20, 0), new(20, 10), new(0, 10),
            new(10, 5), // interior point
        ];

        Vector2[] hull = MenuButtonGeometry.ConvexHull(points);

        hull.Should().HaveCount(4);
    }

    [Fact]
    public void ConvexHull_CombinesSpriteAndLabelRectCorners()
    {
        // Sprite: (39, 11, 19, 17), Label: (37, 30, 6*4=24, 6)
        Vector2[] spriteCorners = MenuButtonGeometry.GetRectCorners(39, 11, 19, 17);
        Vector2[] labelCorners = MenuButtonGeometry.GetRectCorners(37, 30, 24, 6);

        Vector2[] hull = MenuButtonGeometry.ConvexHull([.. spriteCorners, .. labelCorners]);

        // The convex hull of two adjacent rectangles should have at least 4 vertices
        hull.Should().HaveCount(count => count >= 4);
        // Every hull vertex should be one of the input corner points
        hull.Should().OnlyContain(v =>
            v == spriteCorners[0] || v == spriteCorners[1] ||
            v == spriteCorners[2] || v == spriteCorners[3] ||
            v == labelCorners[0] || v == labelCorners[1] ||
            v == labelCorners[2] || v == labelCorners[3]);
    }

    // --------------------------------------------------------------------------
    #endregion
    // --------------------------------------------------------------------------
    #region Point in rounded hull
    // --------------------------------------------------------------------------

    [Fact]
    public void IsPointInRoundedHull_ReturnsTrue_WhenPointInsideSprite()
    {
        // Sprite rect: (39, 11, 19, 17) — center is ~(48.5, 19.5)
        Vector2[] hull = MenuButtonGeometry.ConvexHull(
            MenuButtonGeometry.GetRectCorners(39, 11, 19, 17));

        MenuButtonGeometry.IsPointInRoundedHull(new Vector2(48, 19), hull, Radius)
            .Should().BeTrue();
    }

    [Fact]
    public void IsPointInRoundedHull_ReturnsTrue_WhenPointInsideLabelArea()
    {
        // Label for "ranked": (37, 30, 24, 6) — center is ~(49, 33)
        Vector2[] spriteCorners = MenuButtonGeometry.GetRectCorners(39, 11, 19, 17);
        Vector2[] labelCorners = MenuButtonGeometry.GetRectCorners(37, 30, 24, 6);
        Vector2[] hull = MenuButtonGeometry.ConvexHull([.. spriteCorners, .. labelCorners]);

        MenuButtonGeometry.IsPointInRoundedHull(new Vector2(49, 33), hull, Radius)
            .Should().BeTrue();
    }

    [Fact]
    public void IsPointInRoundedHull_ReturnsTrue_WhenPointInRoundedCorner()
    {
        // A point just outside the sharp corner of the hull but within radius
        Vector2[] hull = MenuButtonGeometry.ConvexHull(
            MenuButtonGeometry.GetRectCorners(0, 0, 20, 20));

        // Top-left corner — point 2px outside (within radius 4)
        MenuButtonGeometry.IsPointInRoundedHull(new Vector2(-2, -2), hull, Radius)
            .Should().BeTrue();
    }

    [Fact]
    public void IsPointInRoundedHull_ReturnsFalse_WhenPointFarOutside()
    {
        Vector2[] hull = MenuButtonGeometry.ConvexHull(
            MenuButtonGeometry.GetRectCorners(39, 11, 19, 17));

        MenuButtonGeometry.IsPointInRoundedHull(new Vector2(-50, -50), hull, Radius)
            .Should().BeFalse();
    }

    [Fact]
    public void IsPointInRoundedHull_ReturnsFalse_WhenPointOutsideRadius()
    {
        Vector2[] hull = MenuButtonGeometry.ConvexHull(
            MenuButtonGeometry.GetRectCorners(0, 0, 20, 20));

        // Point 6px beyond the top-left corner — outside radius 4
        MenuButtonGeometry.IsPointInRoundedHull(new Vector2(-6, -6), hull, Radius)
            .Should().BeFalse();
    }

    [Fact]
    public void IsPointInRoundedHull_ReturnsTrue_WhenPointNearEdge()
    {
        Vector2[] hull = MenuButtonGeometry.ConvexHull(
            MenuButtonGeometry.GetRectCorners(0, 0, 20, 20));

        // Above the top edge by 3px — inside the 4px radius zone
        MenuButtonGeometry.IsPointInRoundedHull(new Vector2(10, -3), hull, Radius)
            .Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
}
