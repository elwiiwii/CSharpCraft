using CSharpCraft.PcraftBase;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase;

public sealed class PcraftMathTests
{
    private const float Eps = 0.0001f;

    // --------------------------------------------------------------------------
    #region GetLen
    // --------------------------------------------------------------------------

    [Fact]
    public void GetLen_IsSafe_ForZeroVector()
    {
        // Lua: sqrt(0+0+0.001) — must not divide by zero.
        // F32 represents 0.001 as 65/65536 ≈ 0.000991, so use wider tolerance.
        PcraftMath.GetLen(F32.Zero, F32.Zero).Float.Should().BeApproximately(MathF.Sqrt(0.001f), precision: 0.001f);
    }

    [Fact]
    public void GetLen_ReturnsCorrectLength_ForUnitVector()
    {
        // sqrt(1+0+0.001) ≈ 1.0005
        PcraftMath.GetLen(F32.One, F32.Zero).Float.Should().BeApproximately(MathF.Sqrt(1.001f), Eps);
    }

    [Fact]
    public void GetLen_ReturnsCorrectLength_ForDiagonalVector()
    {
        PcraftMath.GetLen(F32.FromInt(3), F32.FromInt(4)).Float.Should().BeApproximately(MathF.Sqrt(25.001f), Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetInvLen
    // --------------------------------------------------------------------------

    [Fact]
    public void GetInvLen_IsReciprocalOfGetLen()
    {
        F32 x = F32.FromInt(3), y = F32.FromInt(4);
        PcraftMath.GetInvLen(x, y).Should().Be(F32.One / PcraftMath.GetLen(x, y));
    }

    [Fact]
    public void GetInvLen_IsSafe_ForZeroVector()
    {
        // Should not throw
        var act = () => PcraftMath.GetInvLen(F32.Zero, F32.Zero);
        act.Should().NotThrow();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region GetRot — cardinal directions (PICO-8 fractions of circle)
    // --------------------------------------------------------------------------

    // PICO-8: rot=0 → right, rot=0.25 → up, rot=0.5 → left, rot=0.75 → down
    // getrot(dx,dy): dy>=0 → (dx+3)*0.25; dy<0 → (1-dx)*0.25

    [Theory]
    [InlineData( 1f,  0f, 1.0f)]     // right: dx=1,dy=0 → (1+3)*0.25 = 1.0
    [InlineData( 0f,  1f, 0.75f)]    // down: dx=0,dy=1 → (0+3)*0.25 = 0.75
    [InlineData(-1f,  0f, 0.5f)]     // left: dx=-1,dy=0 → (-1+3)*0.25 = 0.5
    [InlineData( 0f, -1f, 0.25f)]    // up: dx=0,dy=-1 → (1-0)*0.25 = 0.25
    public void GetRot_ReturnsExpected_ForCardinalDx_Dy(float dx, float dy, float expected)
    {
        PcraftMath.GetRot(F32.FromFloat(dx), F32.FromFloat(dy)).Float.Should().BeApproximately(expected, Eps);
    }

    [Fact]
    public void GetRot_TreatsZeroDy_AsDyGreaterOrEqualZero()
    {
        // dy==0 uses the dy>=0 branch: dy=0, dx=0 → (0+3)*0.25 = 0.75
        PcraftMath.GetRot(F32.Zero, F32.Zero).Float.Should().BeApproximately(0.75f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region NormGetRot
    // --------------------------------------------------------------------------

    [Fact]
    public void NormGetRot_ReturnsSameAsGetRot_ForAlreadyNormalizedVector()
    {
        // (1,0) is unit length (approx) — normgetrot should equal getrot(1,0).
        // The +0.001 epsilon inside GetLen shifts the normalised vector by ~0.0001,
        // so we use a looser tolerance here.
        PcraftMath.NormGetRot(F32.One, F32.Zero).Float.Should().BeApproximately(PcraftMath.GetRot(F32.One, F32.Zero).Float, precision: 0.002f);
    }

    [Fact]
    public void NormGetRot_IsLengthIndependent()
    {
        // (3,0) and (1,0) point in the same direction — results must be very close.
        // Epsilon epsilon causes a tiny difference (~0.0001) between short and long vectors.
        PcraftMath.NormGetRot(F32.FromInt(3), F32.Zero).Float.Should().BeApproximately(PcraftMath.NormGetRot(F32.One, F32.Zero).Float, precision: 0.002f);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Mirror — sprite flip for drawing
    // --------------------------------------------------------------------------

    // Rotation ranges (PICO-8 fractions):
    //   [0.000, 0.125) → (0,1)
    //   [0.125, 0.325) → (0,0)   ← empty elseif in Lua falls through to return 0,0
    //   [0.325, 0.625) → (1,0)
    //   [0.625, 0.825) → (1,1)
    //   [0.825, 1.0  ] → (0,1)

    [Theory]
    [InlineData(0.05f,  0, 1)]   // start of [0, 0.125)
    [InlineData(0.10f,  0, 1)]   // middle of [0, 0.125)
    [InlineData(0.20f,  0, 0)]   // [0.125, 0.325)
    [InlineData(0.30f,  0, 0)]   // near end of [0.125, 0.325)
    [InlineData(0.40f,  1, 0)]   // [0.325, 0.625)
    [InlineData(0.50f,  1, 0)]   // middle of [0.325, 0.625)
    [InlineData(0.70f,  1, 1)]   // [0.625, 0.825)
    [InlineData(0.85f,  0, 1)]   // [0.825, 1.0]
    [InlineData(0.99f,  0, 1)]   // near 1.0
    public void Mirror_ReturnsExpectedFlipPair(float rot, int expectedFlipX, int expectedFlipY)
    {
        var (flipX, flipY) = PcraftMath.Mirror(F32.FromFloat(rot));
        flipX.Should().Be(expectedFlipX);
        flipY.Should().Be(expectedFlipY);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region UpRot — smooth rotation with wrap handling
    // --------------------------------------------------------------------------

    [Fact]
    public void UpRot_SmoothlylLerps_WhenDifferenceIsSmall()
    {
        // target=0.8, current=0.5 → diff=0.3 <0.5, no wrap
        // lerp(0.5, 0.8, 0.4) = 0.5*0.6 + 0.8*0.4 = 0.3 + 0.32 = 0.62
        PcraftMath.UpRot(grot: F32.FromFloat(0.8f), rot: F32.FromFloat(0.5f)).Float.Should().BeApproximately(0.62f, Eps);
    }

    [Fact]
    public void UpRot_WrapsAroundSeam_WhenCurrentGreaterThanTarget()
    {
        // target=0.1, current=0.9 → diff=0.8>0.5; rot(0.9)>grot(0.1) → grot+=1 → grot=1.1
        // lerp(0.9, 1.1, 0.4) = 0.9*0.6 + 1.1*0.4 = 0.54 + 0.44 = 0.98
        // ((0.98%1)+1)%1 = 0.98
        PcraftMath.UpRot(grot: F32.FromFloat(0.1f), rot: F32.FromFloat(0.9f)).Float.Should().BeApproximately(0.98f, Eps);
    }

    [Fact]
    public void UpRot_WrapsAroundSeam_WhenCurrentLessThanTarget()
    {
        // target=0.9, current=0.1 → diff=0.8>0.5; rot(0.1)<grot(0.9) → grot-=1 → grot=-0.1
        // lerp(0.1, -0.1, 0.4) = 0.1*0.6 + (-0.1)*0.4 = 0.06 - 0.04 = 0.02
        // ((0.02%1)+1)%1 = 0.02
        PcraftMath.UpRot(grot: F32.FromFloat(0.9f), rot: F32.FromFloat(0.1f)).Float.Should().BeApproximately(0.02f, Eps);
    }

    [Fact]
    public void UpRot_ResultIsInZeroToOneRange()
    {
        var result = PcraftMath.UpRot(grot: F32.FromFloat(0.95f), rot: F32.FromFloat(0.05f));
        result.Float.Should().BeGreaterThanOrEqualTo(0f).And.BeLessThan(1f);
    }

    [Fact]
    public void UpRot_ReturnsTarget_WhenCurrentEqualsTarget()
    {
        // No movement needed — lerp(0.4, 0.4, 0.4) = 0.4
        PcraftMath.UpRot(grot: F32.FromFloat(0.4f), rot: F32.FromFloat(0.4f)).Float.Should().BeApproximately(0.4f, Eps);
    }

    // --------------------------------------------------------------------------
    #endregion
}
