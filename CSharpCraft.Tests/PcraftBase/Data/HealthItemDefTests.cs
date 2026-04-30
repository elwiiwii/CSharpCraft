using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class HealthItemDefTests
{
    [Fact]
    public void Constructor_InheritsNameSprPal()
    {
        var pal = new[] { 1, 2, 8, 14 };
        var sut = new HealthItemDef("potion", 85, pal);
        sut.Name.Should().Be("potion");
        sut.Spr.Should().Be(85);
        sut.Pal.Should().BeSameAs(pal);
    }

    [Fact]
    public void GiveLife_DefaultsToZero()
    {
        var sut = new HealthItemDef("apple", 116);
        sut.GiveLife.Should().Be(0);
    }

    [Fact]
    public void GiveLife_StoredViaInit()
    {
        var sut = new HealthItemDef("apple", 116) { GiveLife = 20 };
        sut.GiveLife.Should().Be(20);
    }
}
