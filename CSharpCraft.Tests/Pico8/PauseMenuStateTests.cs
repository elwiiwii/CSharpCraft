using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for PauseMenuState.
/// Note: Tests that call InitializeMenuStructure() or HandleMenuInput() with selectPressed
/// require PauseMenuBuilder which has deeper dependencies. We test the state machine
/// behavior and leave menu construction integration to integration tests.
/// </summary>
public class PauseMenuStateTests
{
    private static GameOrchestrator CreateOrchestrator()
    {
        var input = new Mock<IInputStateManager>();
        var graphics = new Mock<IGraphicsAPI>();
        var audio = new Mock<IAudioAPI>();
        var scenes = new Mock<ISceneManager>();
        return new GameOrchestrator(
            input.Object, graphics.Object, audio.Object, scenes.Object);
    }

    private PauseMenuState CreateState() => new(CreateOrchestrator());

    // ── Constructor ──

    [Fact]
    public void Constructor_ThrowsOnNull()
    {
        var act = () => new PauseMenuState(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NotPaused()
    {
        var state = CreateState();
        state.IsPaused.Should().BeFalse();
    }

    [Fact]
    public void Constructor_SelectedIndexIsZero()
    {
        var state = CreateState();
        state.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void Constructor_MenuItemsEmpty()
    {
        var state = CreateState();
        state.CurrentMenuItems.Should().BeEmpty();
    }

    // ── TogglePause ──

    [Fact]
    public void TogglePause_Pauses()
    {
        var state = CreateState();
        state.TogglePause();
        state.IsPaused.Should().BeTrue();
    }

    [Fact]
    public void TogglePause_Twice_Unpauses()
    {
        var state = CreateState();
        state.TogglePause();
        state.TogglePause();
        state.IsPaused.Should().BeFalse();
    }

    [Fact]
    public void TogglePause_ThreeTimes_Paused()
    {
        var state = CreateState();
        state.TogglePause();
        state.TogglePause();
        state.TogglePause();
        state.IsPaused.Should().BeTrue();
    }

    [Fact]
    public void TogglePause_ResetsSelectedIndex()
    {
        var state = CreateState();
        state.TogglePause();
        state.SelectedIndex.Should().Be(0);
    }

    // ── Reset ──

    [Fact]
    public void Reset_ClearsPausedState()
    {
        var state = CreateState();
        state.TogglePause();
        state.Reset();
        state.IsPaused.Should().BeFalse();
    }

    [Fact]
    public void Reset_ClearsSelectedIndex()
    {
        var state = CreateState();
        state.TogglePause();
        state.Reset();
        state.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void Reset_ClearsMenuItems()
    {
        var state = CreateState();
        state.Reset();
        state.CurrentMenuItems.Should().BeEmpty();
    }

    [Fact]
    public void Reset_WhenNotPaused_StaysNotPaused()
    {
        var state = CreateState();
        state.Reset();
        state.IsPaused.Should().BeFalse();
    }

    // ── HandleMenuInput with manually added items ──

    [Fact]
    public void HandleMenuInput_DownPressed_IncrementsSelection()
    {
        var state = CreateState();
        // Manually add menu items for testing navigation
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 0", () => { }));
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 1", () => { }));
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 2", () => { }));

        state.HandleMenuInput(upPressed: false, downPressed: true, selectPressed: false);
        state.SelectedIndex.Should().Be(1);
    }

    [Fact]
    public void HandleMenuInput_UpPressed_DecrementsSelection()
    {
        var state = CreateState();
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 0", () => { }));
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 1", () => { }));
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 2", () => { }));

        // Start at 0, go up → should wrap to 2
        state.HandleMenuInput(upPressed: true, downPressed: false, selectPressed: false);
        state.SelectedIndex.Should().Be(2);
    }

    [Fact]
    public void HandleMenuInput_DownWrapsAround()
    {
        var state = CreateState();
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 0", () => { }));
        state.CurrentMenuItems.Add(new MenuItem(() => "Item 1", () => { }));

        // Go down twice from 0 → 1 → 0 (wraps)
        state.HandleMenuInput(false, true, false);
        state.HandleMenuInput(false, true, false);
        state.SelectedIndex.Should().Be(0);
    }

    [Fact]
    public void HandleMenuInput_SelectPressed_InvokesFunction()
    {
        var state = CreateState();
        bool invoked = false;
        state.CurrentMenuItems.Add(new MenuItem(() => "Test", () => invoked = true));

        state.HandleMenuInput(upPressed: false, downPressed: false, selectPressed: true);
        invoked.Should().BeTrue();
    }

    [Fact]
    public void HandleMenuInput_SelectPressed_InvokesCorrectItem()
    {
        var state = CreateState();
        int invokedIndex = -1;
        state.CurrentMenuItems.Add(new MenuItem(() => "A", () => invokedIndex = 0));
        state.CurrentMenuItems.Add(new MenuItem(() => "B", () => invokedIndex = 1));
        state.CurrentMenuItems.Add(new MenuItem(() => "C", () => invokedIndex = 2));

        // Move down to item 1, then select
        state.HandleMenuInput(false, true, false);
        state.HandleMenuInput(false, false, true);
        invokedIndex.Should().Be(1);
    }

    [Fact]
    public void HandleMenuInput_NoInput_NoChange()
    {
        var state = CreateState();
        state.CurrentMenuItems.Add(new MenuItem(() => "A", () => { }));
        state.CurrentMenuItems.Add(new MenuItem(() => "B", () => { }));

        state.HandleMenuInput(false, false, false);
        state.SelectedIndex.Should().Be(0);
    }
}
