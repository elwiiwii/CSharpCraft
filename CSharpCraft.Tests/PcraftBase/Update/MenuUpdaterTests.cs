using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Menu;
using CSharpCraft.PcraftBase.Update;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Input;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.PcraftBase.Update;

public sealed class MenuUpdaterPureTests
{
    // --------------------------------------------------------------------------
    #region No menu active
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReturnsFalse_WhenCurMenuIsNull()
    {
        PlayerEntity player = new(F32.Zero, F32.Zero);

        (bool consumed, bool needsReset) result = MenuUpdater.Update(player);

        _ = result.consumed.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
}

[Collection("Fna")]
public sealed class MenuUpdaterFnaTests(FnaFixture fixture) : IDisposable
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
        _musicDir = FnaFixture.CreateTempMusicDirectory(
            "s0.ogg", "s1.ogg", "s2.ogg", "s3.ogg", "s4.ogg");
        _sfxDir = FnaFixture.CreateTempSfxDirectory(
            "pcraft_og_12", "pcraft_og_13", "pcraft_og_14", "pcraft_og_15",
            "pcraft_og_16", "pcraft_og_17", "pcraft_og_18", "pcraft_og_19", "pcraft_og_21");

        NullScene scene = new();
        GameOrchestrator orch = new(
            _musicDir.Path,
            _sfxDir,
            ".",
            scene,
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window,
            inputManager: input);

        orch.LoadSoundtracks([
            new Soundtrack("test", [
                new Track([new TrackPart("s0", false)], 0),
                new Track([new TrackPart("s1", true)],  1),
                new Track([new TrackPart("s2", true)],  2),
                new Track([new TrackPart("s3", true)],  3),
                new Track([new TrackPart("s4", true)],  4),
            ])
        ], "test");
        orch.LoadSfxPacks(scene.Sfx, "test");
        orch.Update(TimeSpan.Zero);
        return orch;
    }

    // --------------------------------------------------------------------------
    #region Splash menu (Spr != 0) — no button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReturnsTrue_WhenSplashMenuActive()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = PcraftData.MainMenu };

        _ = MenuUpdater.Update(player).consumed.Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotChangeCurMenu_WhenSplashAndNoBtnp4()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = PcraftData.MainMenu };

        _ = MenuUpdater.Update(player);

        _ = player.CurMenu.Should().BeSameAs(PcraftData.MainMenu);
    }

    [Fact]
    public void Update_SetsLb4_AfterSplash()
    {
        // Btn(4) held → lb4 becomes true; released → false
        FakeInputManager fake = new();
        fake.SetBtn(4, true);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = PcraftData.MainMenu };

        _ = MenuUpdater.Update(player);

        _ = player.Lb4.Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Splash menu — button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesToIntroMenu_WhenMainMenuAndBtnp4()
    {
        // Lua: if curmenu==mainmenu then curmenu=intromenu
        FakeInputManager fake = new();
        fake.PressOnce(4); // Btnp(4) fires once
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = PcraftData.MainMenu };

        _ = MenuUpdater.Update(player);

        _ = player.CurMenu.Should().BeSameAs(PcraftData.IntroMenu);
    }

    [Fact]
    public void Update_StartsGame_WhenNonMainSplashAndBtnp4()
    {
        // Lua: else resetlevel() ; curmenu=nil ; music(1)
        FakeInputManager fake = new();
        fake.PressOnce(4);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = PcraftData.IntroMenu };

        _ = MenuUpdater.Update(player);

        _ = player.CurMenu.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Interactive menu — no button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReturnsTrue_WhenInteractiveMenuActive()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.CurMenu = new InventoryMenu(player.Invent);

        _ = MenuUpdater.Update(player).consumed.Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotCloseCurMenu_WhenNoBtnp4()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        InventoryMenu menu = new(player.Invent);
        player.CurMenu = menu;

        _ = MenuUpdater.Update(player);

        _ = player.CurMenu.Should().BeSameAs(menu);
    }

    [Fact]
    public void Update_SetsLb4AndLb5_AfterInteractive()
    {
        FakeInputManager fake = new();
        fake.SetBtn(4, true);
        fake.SetBtn(5, true);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.CurMenu = new InventoryMenu(player.Invent);

        _ = MenuUpdater.Update(player);

        _ = player.Lb4.Should().BeTrue();
        _ = player.Lb5.Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotMoveSel_WhenNoNavButtons()
    {
        using GameOrchestrator orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.Invent.Add(new StackableItem(PcraftData.Wood, 1));
        player.Invent.Add(new StackableItem(PcraftData.Stone, 1));
        InventoryMenu menu = new(player.Invent);
        player.CurMenu = menu;
        menu.Sel = 0;

        _ = MenuUpdater.Update(player);

        _ = menu.Sel.Should().Be(0);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Interactive menu — button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ClosesCurMenu_WhenBtnp4()
    {
        // Lua: if btnp(4) and not lb4 then curmenu=nil
        FakeInputManager fake = new();
        fake.PressOnce(4);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.CurMenu = new InventoryMenu(player.Invent);

        _ = MenuUpdater.Update(player);

        _ = player.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Update_IncrementsSel_WhenBtnp3()
    {
        // Lua: if(btnp(3)) intmenu.sel+=1
        FakeInputManager fake = new();
        fake.PressOnce(3);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.Invent.Add(new StackableItem(PcraftData.Wood, 1));
        player.Invent.Add(new StackableItem(PcraftData.Stone, 1));
        InventoryMenu menu = new(player.Invent);
        player.CurMenu = menu;
        menu.Sel = 0;

        _ = MenuUpdater.Update(player);

        _ = menu.Sel.Should().Be(1);
    }

    [Fact]
    public void Update_DecrementsSel_WhenBtnp2()
    {
        // Lua: if(btnp(2)) intmenu.sel-=1
        FakeInputManager fake = new();
        fake.PressOnce(2);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.Invent.Add(new StackableItem(PcraftData.Wood, 1));
        player.Invent.Add(new StackableItem(PcraftData.Stone, 1));
        InventoryMenu menu = new(player.Invent);
        player.CurMenu = menu;
        menu.Sel = 1;

        _ = MenuUpdater.Update(player);

        _ = menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Update_WrapsSel_WhenBtnp3AtLastItem()
    {
        // Lua: intmenu.sel = loop(intmenu.sel, intmenu.list) — wraps 0-based
        FakeInputManager fake = new();
        fake.PressOnce(3);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        player.Invent.Add(new StackableItem(PcraftData.Wood, 1));
        player.Invent.Add(new StackableItem(PcraftData.Stone, 1));
        InventoryMenu menu = new(player.Invent);
        player.CurMenu = menu;
        menu.Sel = 1; // already at last (0-based, 2 items → max=1)

        _ = MenuUpdater.Update(player);

        _ = menu.Sel.Should().Be(0); // wraps back to 0
    }

    [Fact]
    public void Update_EquipsItem_WhenBtnp5_OnInventoryMenu()
    {
        // Lua: curitem = curmenu.list[curmenu.sel] ; curmenu=nil ; block5=true
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);
        ToolItem axe = new(PcraftData.Haxe, 1);
        player.Invent.Add(axe);
        InventoryMenu menu = new(player.Invent);
        player.CurMenu = menu;
        menu.Sel = 0;

        _ = MenuUpdater.Update(player);

        _ = player.CurItem.Should().BeSameAs(axe);
        _ = player.CurMenu.Should().BeNull();
        _ = player.Block5.Should().BeTrue();
    }

    [Fact]
    public void Update_CraftsItem_WhenBtnp5_OnCraftingMenu_WithIngredients()
    {
        // Lua: if cancraft(rec) then craft(rec)
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        PlayerEntity player = new(F32.Zero, F32.Zero);

        // Wood axe requires 5 wood
        player.Invent.Add(new StackableItem(PcraftData.Wood, 5));
        CraftingMenu craftMenu = new(PcraftData.Workbench, player.Invent)
        {
            Sel = 0 // first entry = wood haxe recipe
        };
        player.CurMenu = craftMenu;

        _ = MenuUpdater.Update(player);

        _ = player.Invent.Should().Contain(it => it.Type == PcraftData.Haxe);
    }

    // --------------------------------------------------------------------------
    #endregion
}
