using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Menu;

/// <summary>
/// These tests verify the shape (contract) of the IMenu interface.
/// All tests fail to compile until IMenu is created.
/// </summary>
public sealed class IMenuContractTests
{
    // --------------------------------------------------------------------------
    #region Interface shape
    // --------------------------------------------------------------------------

    [Fact]
    public void Interface_HasUpdateMethod_AcceptingWorldStateAndPcraftGame()
    {
        typeof(IMenu)
            .GetMethod("Update", [typeof(WorldState), typeof(PcraftGame)])
            .Should().NotBeNull(
                because: "IMenu.Update(WorldState, PcraftGame) must be part of the contract");
    }

    [Fact]
    public void Interface_HasDrawMethod_AcceptingWorldState()
    {
        typeof(IMenu)
            .GetMethod("Draw", [typeof(WorldState)])
            .Should().NotBeNull(
                because: "IMenu.Draw(WorldState) must be part of the contract");
    }

    // --------------------------------------------------------------------------
    #endregion
}
