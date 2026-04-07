using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Draw;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Draw;

public sealed class EnemiesDrawerTests
{
    // --------------------------------------------------------------------------
    #region SortY
    // --------------------------------------------------------------------------

    [Fact]
    public void SortY_SwapsAdjacent_WhenFirstHasLargerY()
    {
        // bubble-sort pass: if enemies[i].Y > enemies[i+1].Y → swap
        var a = new PlayerEntity(F32.Zero, F32.FromInt(10));
        var b = new ZombieEntity(F32.Zero, F32.FromInt(5));
        var list = new List<CharacterEntity> { a, b };

        EnemiesDrawer.SortY(list);

        list[0].Y.Should().Be(F32.FromInt(5));
        list[1].Y.Should().Be(F32.FromInt(10));
    }

    [Fact]
    public void SortY_PreservesOrder_WhenAlreadySorted()
    {
        var a = new PlayerEntity(F32.Zero, F32.FromInt(3));
        var b = new ZombieEntity(F32.Zero, F32.FromInt(7));
        var list = new List<CharacterEntity> { a, b };

        EnemiesDrawer.SortY(list);

        list[0].Should().BeSameAs(a);
        list[1].Should().BeSameAs(b);
    }

    // --------------------------------------------------------------------------
    #endregion
}

