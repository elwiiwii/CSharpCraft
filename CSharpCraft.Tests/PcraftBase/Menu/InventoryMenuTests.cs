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
        var act = () => new InventoryMenu(list: null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("list");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Initial state
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_InitializesSelToZero()
    {
        var list = new List<ItemStack> { new(PcraftData.Wood) };
        var menu = new InventoryMenu(list);
        menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Constructor_ExposesListReference()
    {
        var list = new List<ItemStack> { new(PcraftData.Wood) };
        var menu = new InventoryMenu(list);
        menu.List.Should().BeSameAs(list);
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
    #region Navigation — sel movement
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_IncreasesSel_WhenBtnp3Pressed()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(3);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var list = new List<ItemStack> { new(PcraftData.Wood), new(PcraftData.Stone) };
        var menu = new InventoryMenu(list);
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        menu.Sel.Should().Be(1);
    }

    [Fact]
    public void Update_DecreasesSel_WhenBtnp2Pressed()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(2);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var list = new List<ItemStack> { new(PcraftData.Wood), new(PcraftData.Stone) };
        var menu = new InventoryMenu(list);
        menu.Sel = 1;
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Update_WrapsSel_ToLast_WhenBtnp2AtFirstItem()
    {
        // Loop(-1, 2) = 1 — PICO-8 uses modular wrap, not clamp
        var fake = new FakeInputManager();
        fake.PressOnce(2);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var list = new List<ItemStack> { new(PcraftData.Wood), new(PcraftData.Stone) };
        var menu = new InventoryMenu(list); // Sel = 0
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        menu.Sel.Should().Be(1); // wraps to last
    }

    [Fact]
    public void Update_WrapsSel_ToFirst_WhenBtnp3AtLastItem()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(3);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var list = new List<ItemStack> { new(PcraftData.Wood), new(PcraftData.Stone) };
        var menu = new InventoryMenu(list);
        menu.Sel = 1; // at last (2 items, 0-based)
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        menu.Sel.Should().Be(0); // wraps to first
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
        var list = new List<ItemStack> { new(PcraftData.Wood) };
        var menu = new InventoryMenu(list);
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        state.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Update_DoesNotClose_WhenLb4IsTrue()
    {
        // lb4=true means btn4 was already held — guard prevents repeat-close
        var fake = new FakeInputManager();
        fake.PressOnce(4);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var list = new List<ItemStack> { new(PcraftData.Wood) };
        var menu = new InventoryMenu(list);
        var state = new WorldState { CurMenu = menu, Lb4 = true };

        menu.Update(state, new PcraftGame());

        state.CurMenu.Should().BeSameAs(menu);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Equip — btn5
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_SetsStateCurItemToSelectedItem_WhenBtnp5()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var axe = new ItemStack(PcraftData.Haxe);
        var list = new List<ItemStack> { axe };
        var menu = new InventoryMenu(list);
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        state.CurItem.Should().BeSameAs(axe);
    }

    [Fact]
    public void Update_SetsCurMenuToNull_WhenBtnp5Equips()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var list = new List<ItemStack> { new(PcraftData.Haxe) };
        var menu = new InventoryMenu(list);
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        state.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Update_DoesNotEquip_WhenListIsEmpty_AndBtnp5()
    {
        // No items → btn5 is a no-op; menu stays open
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var menu = new InventoryMenu(new List<ItemStack>());
        var state = new WorldState { CurMenu = menu };

        menu.Update(state, new PcraftGame());

        state.CurItem.Should().BeNull();
        state.CurMenu.Should().BeSameAs(menu);
    }

    // --------------------------------------------------------------------------
    #endregion
}
