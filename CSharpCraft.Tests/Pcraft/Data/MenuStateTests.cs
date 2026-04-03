using CSharpCraft.Pcraft.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Data;

public sealed class MenuStateTests
{
    private static readonly ItemDef Inventary = new("inventory", 89);

    // --------------------------------------------------------------------------
    #region Constructor
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_StoresType()
    {
        var sut = new MenuState(Inventary, list: null, spr: 128, text: "by nusan", text2: "2016");
        sut.Type.Should().BeSameAs(Inventary);
    }

    [Fact]
    public void Constructor_StoresSpr()
    {
        var sut = new MenuState(Inventary, list: null, spr: 128, text: null, text2: null);
        sut.Spr.Should().Be(128);
    }

    [Fact]
    public void Constructor_StoresText()
    {
        var sut = new MenuState(Inventary, list: null, spr: 128, text: "by nusan", text2: "2016");
        sut.Text.Should().Be("by nusan");
        sut.Text2.Should().Be("2016");
    }

    [Fact]
    public void Constructor_StoresList_WhenProvided()
    {
        var list = new List<IInventorySlot>();
        var sut = new MenuState(Inventary, list: list, spr: 128, text: null, text2: null);
        sut.List.Should().BeSameAs(list);
    }

    [Fact]
    public void Constructor_ListIsNull_WhenNotProvided()
    {
        var sut = new MenuState(Inventary, list: null, spr: 128, text: null, text2: null);
        sut.List.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Default mutable state
    // --------------------------------------------------------------------------

    [Fact]
    public void Sel_IsOne_AfterConstruction()
    {
        var sut = new MenuState(Inventary, list: null, spr: 128, text: null, text2: null);
        sut.Sel.Should().Be(1);
    }

    [Fact]
    public void Off_IsZero_AfterConstruction()
    {
        var sut = new MenuState(Inventary, list: null, spr: 128, text: null, text2: null);
        sut.Off.Should().Be(0);
    }

    [Fact]
    public void Sel_CanBeMutated()
    {
        var sut = new MenuState(Inventary, list: null, spr: 128, text: null, text2: null);
        sut.Sel = 3;
        sut.Sel.Should().Be(3);
    }

    // --------------------------------------------------------------------------
    #endregion
}
