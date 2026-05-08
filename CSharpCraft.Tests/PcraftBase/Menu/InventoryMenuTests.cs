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

public sealed class InventoryMenuPureTests
{
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenListIsNull()
    {
        Func<InventoryMenu> act = () => new InventoryMenu(list: null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("list");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Initial state
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_InitializesSelToZero()
    {
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1)];
        InventoryMenu menu = new(list);
        _ = menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Constructor_ExposesListReference()
    {
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1)];
        InventoryMenu menu = new(list);
        _ = menu.List.Should().BeSameAs(list);
    }

    // --------------------------------------------------------------------------
    #endregion
}

// --------------------------------------------------------------------------
// FNA tests — require real graphics/audio orchestrator for Pico8.Btnp/Sfx
// --------------------------------------------------------------------------

[Collection("Fna")]
public sealed class InventoryMenuFnaTests(FnaFixture fixture) : IDisposable
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

        NullScene scene = new();
        GameOrchestrator orch = new(
            _musicDir.Path, _sfxDir, ".", scene,
            fixture.GraphicsDevice, fixture.GraphicsDeviceManager, fixture.Window,
            inputManager: input);

        orch.LoadSfxPacks(scene.Sfx, "test");
        orch.Update(TimeSpan.Zero);
        return orch;
    }

    // --------------------------------------------------------------------------
    #region Navigation — sel movement
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_IncreasesSel_WhenBtnp3Pressed()
    {
        FakeInputManager fake = new();
        fake.PressOnce(3);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1), new StackableItem(PcraftData.Stone, 1)];
        InventoryMenu menu = new(list);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = menu.Sel.Should().Be(1);
    }

    [Fact]
    public void Update_DecreasesSel_WhenBtnp2Pressed()
    {
        FakeInputManager fake = new();
        fake.PressOnce(2);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1), new StackableItem(PcraftData.Stone, 1)];
        InventoryMenu menu = new(list)
        {
            Sel = 1
        };
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Update_WrapsSel_ToLast_WhenBtnp2AtFirstItem()
    {
        // Loop(-1, 2) = 1 — PICO-8 uses modular wrap, not clamp
        FakeInputManager fake = new();
        fake.PressOnce(2);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1), new StackableItem(PcraftData.Stone, 1)];
        InventoryMenu menu = new(list); // Sel = 0
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = menu.Sel.Should().Be(1); // wraps to last
    }

    [Fact]
    public void Update_WrapsSel_ToFirst_WhenBtnp3AtLastItem()
    {
        FakeInputManager fake = new();
        fake.PressOnce(3);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1), new StackableItem(PcraftData.Stone, 1)];
        InventoryMenu menu = new(list)
        {
            Sel = 1 // at last (2 items, 0-based)
        };
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = menu.Sel.Should().Be(0); // wraps to first
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Close — btn4
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsCurMenuToNull_WhenBtnp4Pressed()
    {
        FakeInputManager fake = new();
        fake.PressOnce(4);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1)];
        InventoryMenu menu = new(list);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = player.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Update_DoesNotClose_WhenLb4IsTrue()
    {
        // lb4=true means btn4 was already held — guard prevents repeat-close
        FakeInputManager fake = new();
        fake.PressOnce(4);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> list = [new StackableItem(PcraftData.Wood, 1)];
        InventoryMenu menu = new(list);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu, Lb4 = true };

        _ = menu.Update(player);

        _ = player.CurMenu.Should().BeSameAs(menu);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Equip — btn5
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsStateCurItemToSelectedItem_WhenBtnp5()
    {
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        UnstackableItem axe = new(PcraftData.Haxe);
        List<InventorySlot> list = [axe];
        InventoryMenu menu = new(list);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = player.CurItem.Should().BeSameAs(axe);
    }

    [Fact]
    public void Update_SetsCurMenuToNull_WhenBtnp5Equips()
    {
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> list = [new UnstackableItem(PcraftData.Haxe)];
        InventoryMenu menu = new(list);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = player.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Update_DoesNotEquip_WhenListIsEmpty_AndBtnp5()
    {
        // No items → btn5 is a no-op; menu stays open
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        InventoryMenu menu = new([]);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = player.CurItem.Should().BeNull();
        _ = player.CurMenu.Should().BeSameAs(menu);
    }

    // --------------------------------------------------------------------------
    #endregion
}
