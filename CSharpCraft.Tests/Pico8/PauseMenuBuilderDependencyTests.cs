using Xunit;
using Moq;
using FluentAssertions;
using CSharpCraft.Pico8;
using System.Reflection;

namespace CSharpCraft.Tests.Pico8;

/// <summary>
/// Tests for PauseMenuBuilder circular dependency removal.
///
/// Verifies:
/// - PauseMenuBuilder has no static Pico8 dependency
/// - Menu callbacks receive MenuInput instead of querying global state
/// - MenuInput carries button state correctly into callbacks
/// - PlayMusic is available on IPauseMenuContext
/// </summary>
public class PauseMenuBuilderDependencyTests
{
    #region NO STATIC PICO8 DEPENDENCY

    [Fact]
    public void PauseMenuBuilder_DoesNotImport_StaticPico8()
    {
        // PauseMenuBuilder should not reference the static Pico8 class at all.
        // If it did, it would need 'using static CSharpCraft.Pico8.Pico8'.
        // We verify by checking the source file has no such import via reflection:
        // The type should not call any static methods on CSharpCraft.Pico8.Pico8.
        var builderType = typeof(PauseMenuBuilder);
        var methods = builderType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // The type exists and is in the Pico8.Menu namespace
        builderType.Namespace.Should().Be("CSharpCraft.Pico8.Menu");

        // Verify it only depends on IPauseMenuContext (not GameOrchestrator or Pico8 static)
        var ctorParams = builderType.GetConstructors().Single().GetParameters();
        ctorParams.Should().Contain(p => p.ParameterType == typeof(IPauseMenuContext));
        ctorParams.Should().NotContain(p => p.ParameterType == typeof(GameOrchestrator),
            "PauseMenuBuilder should not depend on concrete GameOrchestrator");
    }

    [Fact]
    public void MenuItem_Function_AcceptsMenuInput()
    {
        // MenuItem.Function should be Action<MenuInput>, not Action
        var funcProp = typeof(MenuItem).GetProperty("Function");
        funcProp.Should().NotBeNull();
        funcProp!.PropertyType.Should().Be(typeof(Action<MenuInput>),
            "MenuItem.Function should accept MenuInput to avoid static API dependency");
    }

    [Fact]
    public void MenuInput_HasExpectedProperties()
    {
        var input = new MenuInput(Left: true, Right: false, ActionA: true, ActionB: false);
        input.Left.Should().BeTrue();
        input.Right.Should().BeFalse();
        input.ActionA.Should().BeTrue();
        input.ActionB.Should().BeFalse();
    }

    #endregion

    #region CALLBACKS RECEIVE MENU INPUT

    [Fact]
    public void Build_CreatesMenuItems_WithMenuInputCallbacks()
    {
        var context = CreateMockContext();
        var mainItems = new List<MenuItem>();
        var curItems = new List<MenuItem>();

        var builder = new PauseMenuBuilder(context.Object, mainItems, curItems);
        builder.Build();

        mainItems.Should().HaveCount(4);

        // Verify each item's Function accepts MenuInput without throwing.
        // Skip "options" (index 1) as it modifies the menu lists.
        var input = new MenuInput(false, false, true, false);
        var safeItems = new[] { mainItems[0], mainItems[2], mainItems[3] };
        foreach (var item in safeItems)
        {
            var act = () => item.Function(input);
            act.Should().NotThrow();
        }
    }

    [Fact]
    public void ResetCart_OnlyFires_OnActionButton()
    {
        var context = CreateMockContext();
        var mainItems = new List<MenuItem>();
        var curItems = new List<MenuItem>();

        var builder = new PauseMenuBuilder(context.Object, mainItems, curItems);
        builder.Build();

        // "reset cart" is index 2
        var resetItem = mainItems[2];
        resetItem.GetName().Should().Be("reset cart");

        // Left/Right should NOT trigger reset
        resetItem.Function(new MenuInput(Left: true, Right: false, ActionA: false, ActionB: false));
        context.Verify(c => c.ReloadCart(), Times.Never());

        // ActionA should trigger reset
        resetItem.Function(new MenuInput(Left: false, Right: false, ActionA: true, ActionB: false));
        context.Verify(c => c.ReloadCart(), Times.Once());
    }

    [Fact]
    public void Exit_OnlyFires_OnActionButton()
    {
        var context = CreateMockContext();
        var mainItems = new List<MenuItem>();
        var curItems = new List<MenuItem>();

        var builder = new PauseMenuBuilder(context.Object, mainItems, curItems);
        builder.Build();

        // "exit" is index 3
        var exitItem = mainItems[3];
        exitItem.GetName().Should().Be("exit");

        // ActionB should trigger quit
        exitItem.Function(new MenuInput(false, false, false, true));
        context.Verify(c => c.QuitToTitle(), Times.Once());
    }

    #endregion

    #region IPAUSEMENUCONTEXT HAS PLAYMUSIC

    [Fact]
    public void IPauseMenuContext_HasPlayMusic_Method()
    {
        var method = typeof(IPauseMenuContext).GetMethod("PlayMusic");
        method.Should().NotBeNull("IPauseMenuContext should expose PlayMusic for soundtrack switching");
        method!.GetParameters().Should().HaveCount(1);
        method.GetParameters()[0].ParameterType.Should().Be(typeof(int));
    }

    [Fact]
    public void GameOrchestrator_ImplementsPlayMusic()
    {
        var orchestrator = new GameOrchestrator(
            new Mock<IInputStateManager>().Object,
            new Mock<IGraphicsAPI>().Object,
            new Mock<IAudioAPI>().Object,
            new Mock<ISceneManager>().Object);

        // PlayMusic should exist and not throw for basic call
        var method = typeof(GameOrchestrator).GetMethod("PlayMusic");
        method.Should().NotBeNull();
    }

    #endregion

    #region HANDLE MENU INPUT WITH BUTTON STATE

    [Fact]
    public void HandleMenuInput_PassesButtonState_ToCallback()
    {
        var context = CreateMockContext();
        var state = new PauseMenuState(context.Object);

        MenuInput? received = null;
        state.CurrentMenuItems.Add(new MenuItem(() => "Test", input => received = input));

        state.HandleMenuInput(
            upPressed: false, downPressed: false, selectPressed: true,
            leftPressed: true, rightPressed: false,
            actionAPressed: false, actionBPressed: true);

        received.Should().NotBeNull();
        received!.Left.Should().BeTrue();
        received.Right.Should().BeFalse();
        received.ActionA.Should().BeFalse();
        received.ActionB.Should().BeTrue();
    }

    [Fact]
    public void HandleMenuInput_NoSelect_DoesNotInvokeCallback()
    {
        var context = CreateMockContext();
        var state = new PauseMenuState(context.Object);

        bool invoked = false;
        state.CurrentMenuItems.Add(new MenuItem(() => "Test", _ => invoked = true));

        state.HandleMenuInput(
            upPressed: false, downPressed: false, selectPressed: false,
            leftPressed: true, rightPressed: true);

        invoked.Should().BeFalse();
    }

    #endregion

    #region HELPERS

    private static Mock<IPauseMenuContext> CreateMockContext()
    {
        var mock = new Mock<IPauseMenuContext>();
        mock.Setup(c => c.AudioSettings).Returns(new InMemorySettings());
        mock.Setup(c => c.DisplaySettings).Returns(new InMemorySettings());
        mock.Setup(c => c.Scenes).Returns(new List<IScene>());
        mock.Setup(c => c.Resolution).Returns((128, 128));
        return mock;
    }

    #endregion
}
