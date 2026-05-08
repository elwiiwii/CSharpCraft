using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Data;

public sealed class HealthItemDefTests
{
    [Fact]
    public void Constructor_InheritsNameSprPal()
    {
        int[] pal = new[] { 1, 2, 8, 14 };
        HealthItemDef sut = new("potion", 85, pal);
        _ = sut.Name.Should().Be("potion");
        _ = sut.Spr.Should().Be(85);
        _ = sut.Pal.Should().BeSameAs(pal);
    }

    [Fact]
    public void GiveLife_DefaultsToZero()
    {
        HealthItemDef sut = new("apple", 116);
        _ = sut.GiveLife.Should().Be(0);
    }

    [Fact]
    public void GiveLife_StoredViaInit()
    {
        HealthItemDef sut = new("apple", 116) { GiveLife = 20 };
        _ = sut.GiveLife.Should().Be(20);
    }
}
