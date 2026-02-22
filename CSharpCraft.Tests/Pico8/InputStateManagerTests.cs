using Xunit;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for InputStateManager.
/// Note: Btn() calls Pico8Utils.Ptn() which requires live keyboard/gamepad state,
/// so we test the state management logic (Btnp, Reset, Update flow) via P8Btns
/// rather than raw input.
/// </summary>
public class InputStateManagerTests
{
    private InputStateManager CreateManager() => new();

    // ── Constructor ──

    [Fact]
    public void Constructor_InitializesButtons()
    {
        var manager = CreateManager();
        manager.Buttons.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ButtonsHaveCorrectArraySizes()
    {
        var manager = CreateManager();
        manager.Buttons.Prev.Should().HaveCount(7);
        manager.Buttons.Lockout.Should().HaveCount(7);
        manager.Buttons.HeldCount.Should().HaveCount(6);
    }

    // ── Btnp ──

    [Fact]
    public void Btnp_ReturnsFalse_WhenHeldCountIsZero()
    {
        var manager = CreateManager();
        manager.Btnp(0).Should().BeFalse();
    }

    [Fact]
    public void Btnp_ReturnsFalse_WhenHeldCountGreaterThanOne()
    {
        var manager = CreateManager();
        // Simulate button being held for multiple frames
        manager.Buttons.HeldCount[0] = 2;
        manager.Buttons.Lockout[0] = false;
        manager.Btnp(0).Should().BeFalse();
    }

    [Fact]
    public void Btnp_ReturnsTrue_WhenHeldCountIsOne_AndNotLockedOut()
    {
        var manager = CreateManager();
        manager.Buttons.HeldCount[0] = 1;
        manager.Buttons.Lockout[0] = false;
        manager.Btnp(0).Should().BeTrue();
    }

    [Fact]
    public void Btnp_ReturnsFalse_WhenLockedOut()
    {
        var manager = CreateManager();
        manager.Buttons.HeldCount[0] = 1;
        manager.Buttons.Lockout[0] = true;
        manager.Btnp(0).Should().BeFalse();
    }

    [Fact]
    public void Btnp_ReturnsFalse_ForInvalidButtonIndex()
    {
        var manager = CreateManager();
        manager.Btnp(99).Should().BeFalse();
    }

    [Fact]
    public void Btnp_ThrowsForNegativeIndex()
    {
        var manager = CreateManager();
        // Note: Btnp doesn't guard against negative indices — this documents the actual behavior.
        var act = () => manager.Btnp(-1);
        act.Should().Throw<IndexOutOfRangeException>();
    }

    [Fact]
    public void Btnp_WorksForAllValidButtons()
    {
        var manager = CreateManager();
        for (int i = 0; i < 6; i++)
        {
            manager.Buttons.HeldCount[i] = 1;
            manager.Buttons.Lockout[i] = false;
            manager.Btnp(i).Should().BeTrue($"button {i} should be pressed");
        }
    }

    // ── Reset ──

    [Fact]
    public void Reset_SetsLockoutForAllButtons()
    {
        var manager = CreateManager();
        // Clear lockouts first
        for (int i = 0; i < 7; i++)
            manager.Buttons.Lockout[i] = false;

        manager.Reset();

        for (int i = 0; i < 7; i++)
            manager.Buttons.Lockout[i].Should().BeTrue($"lockout[{i}] should be true after reset");
    }

    [Fact]
    public void Reset_ClearsHeldCounts()
    {
        var manager = CreateManager();
        for (int i = 0; i < 6; i++)
            manager.Buttons.HeldCount[i] = 5;

        manager.Reset();

        for (int i = 0; i < 6; i++)
            manager.Buttons.HeldCount[i].Should().Be(0);
    }

    [Fact]
    public void Reset_MakesBtnpReturnFalse()
    {
        var manager = CreateManager();
        manager.Buttons.HeldCount[0] = 1;
        manager.Buttons.Lockout[0] = false;
        manager.Btnp(0).Should().BeTrue();

        manager.Reset();
        manager.Btnp(0).Should().BeFalse();
    }

    // ── SetPauseMode ──

    [Fact]
    public void SetPauseMode_DoesNotThrow()
    {
        var manager = CreateManager();
        var act = () => manager.SetPauseMode(true);
        act.Should().NotThrow();
    }

    [Fact]
    public void SetPauseMode_CanBeCalledMultipleTimes()
    {
        var manager = CreateManager();
        manager.SetPauseMode(true);
        manager.SetPauseMode(false);
        manager.SetPauseMode(true);
        // No exception means success
    }

    // ── IsPauseButtonPressed ──

    [Fact]
    public void IsPauseButtonPressed_ReturnsFalse_Initially()
    {
        var manager = CreateManager();
        manager.IsPauseButtonPressed().Should().BeFalse();
    }

    [Fact]
    public void IsPauseButtonPressed_ReturnsFalse_WhenHeldCountIsZero()
    {
        var manager = CreateManager();
        // Button 6 = pause, HeldCount only has 6 entries (0-5)
        // So HeldCount[6] would be out of bounds — the method checks length
        manager.IsPauseButtonPressed().Should().BeFalse();
    }

    // ── P8Btns direct tests ──

    [Fact]
    public void P8Btns_Reset_SetsAllLockoutsTrue()
    {
        var btns = new P8Btns();
        for (int i = 0; i < 7; i++)
            btns.Lockout[i] = false;

        btns.Reset();
        for (int i = 0; i < 7; i++)
            btns.Lockout[i].Should().BeTrue();
    }

    [Fact]
    public void P8Btns_Reset_ClearsAllHeldCounts()
    {
        var btns = new P8Btns();
        for (int i = 0; i < 6; i++)
            btns.HeldCount[i] = 10;

        btns.Reset();
        for (int i = 0; i < 6; i++)
            btns.HeldCount[i].Should().Be(0);
    }

    [Fact]
    public void P8Btns_InitialState_AllHeldCountsZero()
    {
        var btns = new P8Btns();
        for (int i = 0; i < 6; i++)
            btns.HeldCount[i].Should().Be(0);
    }

    [Fact]
    public void P8Btns_InitialState_AllPrevFalse()
    {
        var btns = new P8Btns();
        for (int i = 0; i < 7; i++)
            btns.Prev[i].Should().BeFalse();
    }

    [Fact]
    public void P8Btns_InitialState_AllLockoutFalse()
    {
        var btns = new P8Btns();
        for (int i = 0; i < 7; i++)
            btns.Lockout[i].Should().BeFalse();
    }
}
