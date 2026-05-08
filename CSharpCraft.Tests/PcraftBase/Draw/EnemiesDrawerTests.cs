using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Draw;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Draw;

public sealed class EnemiesDrawerTests
{
    // --------------------------------------------------------------------------
    #region SortY
    // --------------------------------------------------------------------------

    [Fact]
    public void SortY_SwapsAdjacent_WhenFirstHasLargerY()
    {
        // bubble-sort pass: if enemies[i].Y > enemies[i+1].Y → swap
        PlayerEntity a = new(F32.Zero, F32.FromInt(10));
        ZombieEntity b = new(F32.Zero, F32.FromInt(5));
        List<CharacterEntity> list = [a, b];

        EnemiesDrawer.SortY(list);

        _ = list[0].Y.Should().Be(F32.FromInt(5));
        _ = list[1].Y.Should().Be(F32.FromInt(10));
    }

    [Fact]
    public void SortY_PreservesOrder_WhenAlreadySorted()
    {
        PlayerEntity a = new(F32.Zero, F32.FromInt(3));
        ZombieEntity b = new(F32.Zero, F32.FromInt(7));
        List<CharacterEntity> list = [a, b];

        EnemiesDrawer.SortY(list);

        _ = list[0].Should().BeSameAs(a);
        _ = list[1].Should().BeSameAs(b);
    }

    // --------------------------------------------------------------------------
    #endregion
}

