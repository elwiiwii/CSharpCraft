using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Input;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Menu;

// --------------------------------------------------------------------------
// Pure tests — no graphics device needed
// --------------------------------------------------------------------------

public sealed class ChestMenuPureTests
{
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenChestItemsIsNull()
    {
        var playerItems = new List<InventorySlot>();
        var act = () => new ChestMenu(chestItems: null!, playerItems);
        act.Should().Throw<ArgumentNullException>().WithParameterName("chestItems");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPlayerItemsIsNull()
    {
        var chestItems = new List<InventorySlot>();
        var act = () => new ChestMenu(chestItems, playerItems: null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playerItems");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Initial state
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_InitializesTabToggleToZero()
    {
        var menu = new ChestMenu(new List<InventorySlot>(), new List<InventorySlot>());
        menu.TabToggle.Should().Be(0);
    }

    [Fact]
    public void Constructor_InitializesSelToZero()
    {
        var menu = new ChestMenu(new List<InventorySlot>(), new List<InventorySlot>());
        menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Constructor_ExposesChestItemsAndPlayerItemsReferences()
    {
        var chestItems  = new List<InventorySlot> { new StackableItem(PcraftData.Wood, 1) };
        var playerItems = new List<InventorySlot> { new StackableItem(PcraftData.Stone, 1) };
        var menu = new ChestMenu(chestItems, playerItems);
        menu.ChestItems.Should().BeSameAs(chestItems);
        menu.PlayerItems.Should().BeSameAs(playerItems);
    }

    // --------------------------------------------------------------------------
    #endregion
}

// --------------------------------------------------------------------------
// FNA tests — require real graphics/audio orchestrator for Pico8.Btnp/Sfx
// --------------------------------------------------------------------------

[Collection("Fna")]
public sealed class ChestMenuFnaTests(FnaFixture fixture) : IDisposable
{
    private TempMusicDirectory? _musicDir;
    private string? _sfxDir;

    public void Dispose()
    {
        _musicDir?.Dispose();
        if (_sfxDir is not null && Directory.Exists(_sfxDir))
            Directory.Delete(_sfxDir, recursive: true);
    }

    private sealed class NullScene : IScene
    {
        public string? Name => null;
        public void Init(ISceneSetup setup) { }
        public string? SpritesPath => null;
        public string? MapPath => null;
        public string? FlagData => null;
        public IReadOnlyList<Soundtrack> Music => [];
        public IReadOnlyList<SfxPack> Sfx => [new SfxPack("test", "pcraft_og_")];
    }

    private GameOrchestrator BuildOrchestrator(IInputManager? input = null)
    {
        _musicDir = FnaFixture.CreateTempMusicDirectory("s0.ogg");
        _sfxDir = FnaFixture.CreateTempSfxDirectory(
            "pcraft_og_16", "pcraft_og_17", "pcraft_og_18");

        var scene = new NullScene();
        var orch = new GameOrchestrator(
            _musicDir.Path, _sfxDir, ".", scene,
            fixture.GraphicsDevice, fixture.GraphicsDeviceManager, fixture.Window,
            inputManager: input);

        orch.LoadSfxPacks(scene.Sfx, "test");
        orch.Update(TimeSpan.Zero);
        return orch;
    }

    // --------------------------------------------------------------------------
    #region Tab toggle
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_IncrementsTabToggle_WhenBtnp1Pressed()
    {
        // btn1 = "right" — increments tab; mod 2 wraps 0→1
        var fake = new FakeInputManager();
        fake.PressOnce(1);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var menu = new ChestMenu(
            chestItems:  new List<InventorySlot> { new StackableItem(PcraftData.Wood, 1) },
            playerItems: new List<InventorySlot> { new StackableItem(PcraftData.Stone, 1) });
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player);

        menu.TabToggle.Should().Be(1);
    }

    [Fact]
    public void Update_DecrementsTabToggle_WhenBtnp0Pressed()
    {
        // btn0 = "left" — decrements tab; wraps 1→0
        var fake = new FakeInputManager();
        fake.PressOnce(0);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var menu = new ChestMenu(
            chestItems:  new List<InventorySlot> { new StackableItem(PcraftData.Wood, 1) },
            playerItems: new List<InventorySlot> { new StackableItem(PcraftData.Stone, 1) });
        menu.TabToggle = 1;
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player);

        menu.TabToggle.Should().Be(0);
    }

    [Fact]
    public void Update_WrapsTabToggle_Modulo2_WhenBtnp1AtLastTab()
    {
        // mod-2 wrap: 1 + 1 = 2 → 2 % 2 = 0
        var fake = new FakeInputManager();
        fake.PressOnce(1);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var menu = new ChestMenu(
            chestItems:  new List<InventorySlot> { new StackableItem(PcraftData.Wood, 1) },
            playerItems: new List<InventorySlot> { new StackableItem(PcraftData.Stone, 1) });
        menu.TabToggle = 1;
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player);

        menu.TabToggle.Should().Be(0);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Item transfer
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_TransfersItem_FromChestToPlayer_WhenBtnp5_OnChestTab()
    {
        // TabToggle=0 → chest is active; btn5 moves selected chest item into player invent
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var wood        = new StackableItem(PcraftData.Wood, 1);
        var chestItems  = new List<InventorySlot> { wood };
        var playerItems = new List<InventorySlot>();
        var menu = new ChestMenu(chestItems, playerItems); // TabToggle=0
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player);

        chestItems.Should().BeEmpty("item was transferred out of chest");
        playerItems.Should().ContainSingle(i => i.Type == PcraftData.Wood, "item arrived in player inventory");
    }

    [Fact]
    public void Update_TransfersItem_FromPlayerToChest_WhenBtnp5_OnPlayerTab()
    {
        // TabToggle=1 → player inventory is active; btn5 moves selected item into chest
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var stone       = new StackableItem(PcraftData.Stone, 1);
        var chestItems  = new List<InventorySlot>();
        var playerItems = new List<InventorySlot> { stone };
        var menu = new ChestMenu(chestItems, playerItems);
        menu.TabToggle = 1;
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player);

        playerItems.Should().BeEmpty("item was transferred out of player inventory");
        chestItems.Should().ContainSingle(i => i.Type == PcraftData.Stone, "item arrived in chest");
    }

    [Fact]
    public void Update_DoesNotTransfer_WhenActiveListIsEmpty_AndBtnp5()
    {
        // No items in active tab → btn5 is a no-op
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var chestItems  = new List<InventorySlot>();
        var playerItems = new List<InventorySlot> { new StackableItem(PcraftData.Wood, 1) };
        var menu = new ChestMenu(chestItems, playerItems); // TabToggle=0 → chest tab (empty)
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player);

        chestItems.Should().BeEmpty();
        playerItems.Should().ContainSingle(); // unchanged
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Close — btn4
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsCurMenuToNull_WhenBtnp4Pressed()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(4);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var menu = new ChestMenu(new List<InventorySlot>(), new List<InventorySlot>());
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player);

        player.CurMenu.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
}
