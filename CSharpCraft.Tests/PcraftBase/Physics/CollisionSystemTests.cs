using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Physics;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Physics;

public sealed class CollisionSystemTests
{
    // --------------------------------------------------------------------------
    #region EntColFree
    // --------------------------------------------------------------------------

    [Fact]
    public void EntColFree_ReturnsTrue_WhenChebyshevDistanceIsGreaterThan8()
    {
        // entity at (0,0), check point at (9,0) → max(9,0)=9 > 8 → free
        PlayerEntity e = new(F32.Zero, F32.Zero);

        bool result = CollisionSystem.EntColFree(F32.FromInt(9), F32.Zero, e);

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void EntColFree_ReturnsFalse_WhenChebyshevDistanceIsExactly8()
    {
        // entity at (0,0), check point at (8,3) → max(8,3)=8 — NOT >8 → not free
        PlayerEntity e = new(F32.Zero, F32.Zero);

        bool result = CollisionSystem.EntColFree(F32.FromInt(8), F32.FromInt(3), e);

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void EntColFree_ReturnsFalse_WhenEntitiesCoincide()
    {
        // same position → max=0 ≤ 8
        PlayerEntity e = new(F32.FromInt(5), F32.FromInt(5));

        bool result = CollisionSystem.EntColFree(F32.FromInt(5), F32.FromInt(5), e);

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void EntColFree_UsesChebyshevDistance_NotEuclidean()
    {
        // entity at (0,0), point at (7,7) → Euclidean≈9.9 (>8) but Chebyshev=7 (≤8) → not free
        PlayerEntity e = new(F32.Zero, F32.Zero);

        bool result = CollisionSystem.EntColFree(F32.FromInt(7), F32.FromInt(7), e);

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void EntColFree_UsesAbsoluteValue_ForNegativeXOffset()
    {
        // entity at (10,10), check at (1,10) → max(9,0)=9 > 8 → free
        PlayerEntity e = new(F32.FromInt(10), F32.FromInt(10));

        bool result = CollisionSystem.EntColFree(F32.FromInt(1), F32.FromInt(10), e);

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void EntColFree_ReturnsFalse_WhenInsideBox_LessThan8OnBothAxes()
    {
        // entity at (0,0), check at (5,6) → max(5,6)=6 ≤ 8
        PlayerEntity e = new(F32.Zero, F32.Zero);

        bool result = CollisionSystem.EntColFree(F32.FromInt(5), F32.FromInt(6), e);

        _ = result.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region ReflectCol
    // --------------------------------------------------------------------------

    [Fact]
    public void ReflectCol_ReturnsUnchanged_WhenCurrentPositionIsBlocked()
    {
        // ccur=false → reflection logic skipped entirely, velocity returned as-is
        F32 dx = F32.FromInt(2), dy = F32.FromInt(3);

        (F32 rdx, F32 rdy) = CollisionSystem.ReflectCol(
            F32.Zero, F32.Zero,
            dx, dy,
            check: (_, _) => false,   // everything blocked including current pos
            dp: F32.FromInt(1));

        _ = rdx.Should().Be(dx);
        _ = rdy.Should().Be(dy);
    }

    [Fact]
    public void ReflectCol_ReturnsUnchanged_WhenAllPositionsAreFree()
    {
        // ccur=true, ctotal=true → move is unobstructed, nothing to reflect
        F32 dx = F32.FromInt(2), dy = F32.FromInt(3);

        (F32 rdx, F32 rdy) = CollisionSystem.ReflectCol(
            F32.Zero, F32.Zero,
            dx, dy,
            check: (_, _) => true,    // everything free
            dp: F32.FromInt(1));

        _ = rdx.Should().Be(dx);
        _ = rdy.Should().Be(dy);
    }

    [Fact]
    public void ReflectCol_FlipsDy_WhenHorizontalMoveIsFreeButTotalIsBlocked()
    {
        // x=0, y=0, dx=2, dy=3 → newx=2, newy=3
        // ccur(0,0)=true, chor(2,0)=true, cver(0,3)=false, ctotal(2,3)=false
        // → chor is true → dy = -dy*dp
        F32 x = F32.Zero, y = F32.Zero;
        F32 dx = F32.FromInt(2), dy = F32.FromInt(3);

        (F32 rdx, F32 rdy) = CollisionSystem.ReflectCol(
            x, y, dx, dy,
            check: (cx, cy) =>
                (cx == F32.Zero && cy == F32.Zero) ||  // ccur
                (cx == F32.FromInt(2) && cy == F32.Zero),     // chor only
            dp: F32.FromInt(1));

        _ = rdx.Should().Be(dx);            // dx unchanged
        _ = rdy.Should().Be(-dy);           // dy flipped (dp=1 → -dy*1 = -dy)
    }

    [Fact]
    public void ReflectCol_FlipsDx_WhenVerticalMoveIsFreeButTotalIsBlocked()
    {
        // x=0, y=0, dx=2, dy=3 → newx=2, newy=3
        // ccur(0,0)=true, chor(2,0)=false, cver(0,3)=true, ctotal(2,3)=false
        // → chor is false, cver is true → dx = -dx*dp
        F32 x = F32.Zero, y = F32.Zero;
        F32 dx = F32.FromInt(2), dy = F32.FromInt(3);

        (F32 rdx, F32 rdy) = CollisionSystem.ReflectCol(
            x, y, dx, dy,
            check: (cx, cy) =>
                (cx == F32.Zero && cy == F32.Zero) ||  // ccur
                (cx == F32.Zero && cy == F32.FromInt(3)), // cver only
            dp: F32.FromInt(1));

        _ = rdx.Should().Be(-dx);           // dx flipped
        _ = rdy.Should().Be(dy);            // dy unchanged
    }

    [Fact]
    public void ReflectCol_FlipsBoth_WhenNeitherAxisMoveIsFree()
    {
        // x=0, y=0, dx=2, dy=3 → newx=2, newy=3
        // ccur(0,0)=true, chor(2,0)=false, cver(0,3)=false
        // → neither free → dx=-dx*dp, dy=-dy*dp
        F32 x = F32.Zero, y = F32.Zero;
        F32 dx = F32.FromInt(2), dy = F32.FromInt(3);

        (F32 rdx, F32 rdy) = CollisionSystem.ReflectCol(
            x, y, dx, dy,
            check: (cx, cy) =>
                cx == F32.Zero && cy == F32.Zero,   // only ccur is free
            dp: F32.FromInt(1));

        _ = rdx.Should().Be(-dx);
        _ = rdy.Should().Be(-dy);
    }

    [Fact]
    public void ReflectCol_ScalesFlippedComponent_ByDp()
    {
        // same horizontal-free scenario but dp=0.5: flipped dy = -dy*0.5
        // dx=2(unchanged), dy=4 → flipped rdy = -4*0.5 = -2
        F32 x = F32.Zero, y = F32.Zero;
        F32 dx = F32.FromInt(2), dy = F32.FromInt(4);
        F32 dp = F32.FromFloat(0.5f);

        (F32 rdx, F32 rdy) = CollisionSystem.ReflectCol(
            x, y, dx, dy,
            check: (cx, cy) =>
                (cx == F32.Zero && cy == F32.Zero) ||  // ccur
                (cx == F32.FromInt(2) && cy == F32.Zero),     // chor only
            dp: dp);

        _ = rdx.Should().Be(dx);
        _ = rdy.Should().Be(F32.FromInt(-2));   // -4 * 0.5 = -2
    }

    [Fact]
    public void ReflectCol_ZerosDampedVelocity_WhenDpIsZero()
    {
        // dp=0 — Lua equivalent: sticking into a wall kills velocity (isfree collision)
        // horizontal free, total blocked → dy = -dy*0 = 0
        F32 x = F32.Zero, y = F32.Zero;
        F32 dx = F32.FromInt(2), dy = F32.FromInt(3);

        (F32 rdx, F32 rdy) = CollisionSystem.ReflectCol(
            x, y, dx, dy,
            check: (cx, cy) =>
                (cx == F32.Zero && cy == F32.Zero) ||
                (cx == F32.FromInt(2) && cy == F32.Zero),
            dp: F32.Zero);

        _ = rdx.Should().Be(dx);
        _ = rdy.Should().Be(F32.Zero);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region IsIn
    // --------------------------------------------------------------------------

    [Fact]
    public void IsIn_ReturnsTrue_WhenEntityIsAtCenter()
    {
        // clx=64, cly=64, size=100 → box (−36..164) × (−36..164); entity at (64,64) → strictly inside
        PlayerEntity e = new(F32.FromInt(64), F32.FromInt(64));

        bool result = CollisionSystem.IsIn(e, F32.FromInt(100), F32.FromInt(64), F32.FromInt(64));

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenEntityIsToTheRight()
    {
        // entity.x = clx+size → NOT strictly less than clx+size
        PlayerEntity e = new(F32.FromInt(164), F32.FromInt(64));

        bool result = CollisionSystem.IsIn(e, F32.FromInt(100), F32.FromInt(64), F32.FromInt(64));

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenEntityIsBelow()
    {
        // entity.y = clx+size → NOT strictly less than cly+size
        PlayerEntity e = new(F32.FromInt(64), F32.FromInt(164));

        bool result = CollisionSystem.IsIn(e, F32.FromInt(100), F32.FromInt(64), F32.FromInt(64));

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenEntityIsToTheLeft()
    {
        // entity.x = clx-size → NOT strictly greater than clx-size
        PlayerEntity e = new(F32.FromInt(-36), F32.FromInt(64));

        bool result = CollisionSystem.IsIn(e, F32.FromInt(100), F32.FromInt(64), F32.FromInt(64));

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenEntityIsAbove()
    {
        // entity.y = cly-size → NOT strictly greater than cly-size
        PlayerEntity e = new(F32.FromInt(64), F32.FromInt(-36));

        bool result = CollisionSystem.IsIn(e, F32.FromInt(100), F32.FromInt(64), F32.FromInt(64));

        _ = result.Should().BeFalse();
    }

    [Fact]
    public void IsIn_ReturnsTrue_WhenEntityIsJustInsideBoundary()
    {
        // entity at clx+size-1, cly+size-1 → just inside the exclusive bounds
        PlayerEntity e = new(F32.FromInt(163), F32.FromInt(163));

        bool result = CollisionSystem.IsIn(e, F32.FromInt(100), F32.FromInt(64), F32.FromInt(64));

        _ = result.Should().BeTrue();
    }

    [Fact]
    public void IsIn_ReturnsFalse_WhenEntityIsFarOutside()
    {
        // entity far away from camera
        PlayerEntity e = new(F32.FromInt(500), F32.FromInt(500));

        bool result = CollisionSystem.IsIn(e, F32.FromInt(100), F32.FromInt(64), F32.FromInt(64));

        _ = result.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
}
