using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using FluentAssertions;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase;

public sealed class WorldStateTests
{
    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var action = () => new WorldState();
        action.Should().NotThrow();
    }

    [Fact]
    public void Constructor_InitializesPlayerPositionToZero()
    {
        var state = new WorldState();

        state.Plx.Should().Be(F32.Zero);
        state.Ply.Should().Be(F32.Zero);
    }

    [Fact]
    public void Constructor_InitializesPlayerStatsToZero()
    {
        var state = new WorldState();

        state.Prot.Should().Be(F32.Zero);
        state.Lrot.Should().Be(F32.Zero);
        state.Panim.Should().Be(F32.Zero);
        state.Banim.Should().Be(F32.Zero);
        state.Pstam.Should().Be(F32.Zero);
        state.Lstam.Should().Be(F32.Zero);
        state.Plife.Should().Be(F32.Zero);
        state.Llife.Should().Be(F32.Zero);
    }

    [Fact]
    public void Constructor_InitializesCameraToZero()
    {
        var state = new WorldState();

        state.Clx.Should().Be(F32.Zero);
        state.Cly.Should().Be(F32.Zero);
        state.Cmx.Should().Be(F32.Zero);
        state.Cmy.Should().Be(F32.Zero);
        state.Coffx.Should().Be(F32.Zero);
        state.Coffy.Should().Be(F32.Zero);
    }

    [Fact]
    public void Constructor_InitializesInventoryToNonNull()
    {
        var state = new WorldState();

        state.Invent.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesInventoryToEmpty()
    {
        var state = new WorldState();

        state.Invent.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesCurItemToNull()
    {
        var state = new WorldState();

        state.CurItem.Should().BeNull();
    }

    [Fact]
    public void WorldState_DoesNotHave_MenuInventProperty()
    {
        // ChestMenu owns chest/player item lists — WorldState should not hold a
        // separate MenuInvent reference. This test enforces the Phase 4 contract.
        var prop = typeof(WorldState).GetProperty(
            "MenuInvent",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        prop.Should().BeNull("ChestMenu owns dual-panel state — WorldState should not carry MenuInvent");
    }

    [Fact]
    public void Constructor_InitializesCurrentLevelToNull()
    {
        var state = new WorldState();

        state.CurrentLevel.Should().BeNull();
    }

    [Fact]
    public void Constructor_InitializesCaveToNull()
    {
        var state = new WorldState();

        state.Cave.Should().BeNull();
    }

    [Fact]
    public void Constructor_InitializesIslandToNull()
    {
        var state = new WorldState();

        state.Island.Should().BeNull();
    }

    [Fact]
    public void Constructor_InitializesLevelProxiesWhenNoCurrentLevel()
    {
        var state = new WorldState();

        state.LevelX.Should().Be(0);
        state.LevelY.Should().Be(0);
        state.LevelSx.Should().Be(0);
        state.LevelSy.Should().Be(0);
        state.LevelUnder.Should().BeFalse();
    }

    [Fact]
    public void Constructor_InitializesEntitiesToNonNull()
    {
        var state = new WorldState();

        state.Entities.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesEntitiesToEmpty()
    {
        var state = new WorldState();

        state.Entities.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesEnemiesToNonNull()
    {
        var state = new WorldState();

        state.Enemies.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesEnemiesToEmpty()
    {
        var state = new WorldState();

        state.Enemies.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesDataDictToNonNull()
    {
        var state = new WorldState();

        state.Data.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesDataDictToEmpty()
    {
        var state = new WorldState();

        state.Data.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesTimeToZero()
    {
        var state = new WorldState();

        state.Time.Should().Be(F32.Zero);
    }

    [Fact]
    public void Constructor_InitializesSwitchLevelToFalse()
    {
        var state = new WorldState();

        state.SwitchLevel.Should().BeFalse();
    }

    [Fact]
    public void Constructor_InitializesCanSwitchLevelToFalse()
    {
        var state = new WorldState();

        state.CanSwitchLevel.Should().BeFalse();
    }

    [Fact]
    public void Constructor_InitializesCurMenuToNull()
    {
        var state = new WorldState();

        state.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Constructor_InitializesNearEnemiesToNonNull()
    {
        var state = new WorldState();

        state.NearEnemies.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesNearEnemiesToEmpty()
    {
        var state = new WorldState();

        state.NearEnemies.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_InitializesRndWatToNonNull()
    {
        var state = new WorldState();

        state.RndWat.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesRndWatToProperSize()
    {
        var state = new WorldState();

        state.RndWat.Should().HaveCount(16);
        for (int i = 0; i < 16; i++)
        {
            state.RndWat[i].Should().HaveCount(16);
        }
    }

    [Fact]
    public void Constructor_InitializesInputHelpersToFalse()
    {
        var state = new WorldState();

        state.Lb4.Should().BeFalse();
        state.Lb5.Should().BeFalse();
        state.Block5.Should().BeFalse();
    }

    [Fact]
    public void SetLevel_UpdatesLevelProxies_WhenCurrentLevelSet()
    {
        var state = new WorldState();
        var level = new Level(10, 20, 64, 64, false);

        state.SetLevel(level);

        state.CurrentLevel.Should().Be(level);
        state.LevelX.Should().Be(10);
        state.LevelY.Should().Be(20);
        state.LevelSx.Should().Be(64);
        state.LevelSy.Should().Be(64);
        state.LevelUnder.Should().BeFalse();
    }

    [Fact]
    public void SetLevel_UpdatesEntitiesToLevelEntities()
    {
        var state = new WorldState();
        var level = new Level(0, 0, 64, 64, false);
        var item = new ItemEntity(PcraftData.Wood, F32.Zero, F32.Zero);
        level.Ent.Add(item);

        state.SetLevel(level);

        state.Entities.Should().ContainSingle().Which.Should().Be(item);
    }

    [Fact]
    public void SetLevel_UpdatesEnemiesToLevelEnemies()
    {
        var state = new WorldState();
        var level = new Level(0, 0, 64, 64, false);
        var player = new PlayerEntity(F32.Zero, F32.Zero);
        level.Ene.Add(player);

        state.SetLevel(level);

        state.Enemies.Should().ContainSingle().Which.Should().Be(player);
    }

    [Fact]
    public void SetLevel_UpdatesDataToLevelData()
    {
        var state = new WorldState();
        var level = new Level(0, 0, 64, 64, false);
        var testValue = F32.FromFloat(3.14f);
        level.Dat[42] = testValue;

        state.SetLevel(level);

        state.Data.Should().ContainKey(42);
        state.Data[42].Should().Be(testValue);
    }

    [Fact]
    public void PlayerStats_CanBeModified()
    {
        var state = new WorldState();

        state.Plx = F32.FromInt(5);
        state.Ply = F32.FromInt(10);
        state.Prot = F32.FromInt(1);

        state.Plx.Should().Be(F32.FromInt(5));
        state.Ply.Should().Be(F32.FromInt(10));
        state.Prot.Should().Be(F32.FromInt(1));
    }

    [Fact]
    public void CameraStats_CanBeModified()
    {
        var state = new WorldState();

        state.Clx = F32.FromInt(3);
        state.Cly = F32.FromInt(7);

        state.Clx.Should().Be(F32.FromInt(3));
        state.Cly.Should().Be(F32.FromInt(7));
    }
}
