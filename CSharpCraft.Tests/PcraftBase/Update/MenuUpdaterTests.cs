using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Data;
using CSharpCraft.PcraftBase.Map;
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
        var state = new WorldState { CurMenu = null };
        var game  = new PcraftGame();

        var result = MenuUpdater.Update(state, game);

        result.Should().BeFalse();
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

        var scene = new NullScene();
        var orch = new GameOrchestrator(
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
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { CurMenu = PcraftData.MainMenu };

        MenuUpdater.Update(state, new PcraftGame()).Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotChangeCurMenu_WhenSplashAndNoBtnp4()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { CurMenu = PcraftData.MainMenu };

        MenuUpdater.Update(state, new PcraftGame());

        state.CurMenu.Should().BeSameAs(PcraftData.MainMenu);
    }

    [Fact]
    public void Update_SetsLb4_AfterSplash()
    {
        // Btn(4) held → lb4 becomes true; released → false
        var fake = new FakeInputManager();
        fake.SetBtn(4, true);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState { CurMenu = PcraftData.MainMenu };

        MenuUpdater.Update(state, new PcraftGame());

        state.Lb4.Should().BeTrue();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Splash menu — button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AdvancesToIntroMenu_WhenMainMenuAndBtnp4()
    {
        // Lua: if curmenu==mainmenu then curmenu=intromenu
        var fake = new FakeInputManager();
        fake.PressOnce(4); // Btnp(4) fires once
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState { CurMenu = PcraftData.MainMenu };

        MenuUpdater.Update(state, new PcraftGame());

        state.CurMenu.Should().BeSameAs(PcraftData.IntroMenu);
    }

    [Fact]
    public void Update_StartsGame_WhenNonMainSplashAndBtnp4()
    {
        // Lua: else resetlevel() ; curmenu=nil ; music(1)
        var fake = new FakeInputManager();
        fake.PressOnce(4);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState { CurMenu = PcraftData.IntroMenu };
        var game  = new PcraftGame();
        game.InitRecipes();

        MenuUpdater.Update(state, game);

        state.CurMenu.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Interactive menu — no button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReturnsTrue_WhenInteractiveMenuActive()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.CurMenu = new InventoryMenu(state.Invent);

        MenuUpdater.Update(state, new PcraftGame()).Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotCloseCurMenu_WhenNoBtnp4()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        var menu = new InventoryMenu(state.Invent);
        state.CurMenu = menu;

        MenuUpdater.Update(state, new PcraftGame());

        state.CurMenu.Should().BeSameAs(menu);
    }

    [Fact]
    public void Update_SetsLb4AndLb5_AfterInteractive()
    {
        var fake = new FakeInputManager();
        fake.SetBtn(4, true);
        fake.SetBtn(5, true);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.CurMenu = new InventoryMenu(state.Invent);

        MenuUpdater.Update(state, new PcraftGame());

        state.Lb4.Should().BeTrue();
        state.Lb5.Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotMoveSel_WhenNoNavButtons()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.Invent.Add(new ItemStack(PcraftData.Wood,  count: 1));
        state.Invent.Add(new ItemStack(PcraftData.Stone, count: 1));
        var menu = new InventoryMenu(state.Invent);
        state.CurMenu = menu;
        menu.Sel = 0;

        MenuUpdater.Update(state, new PcraftGame());

        menu.Sel.Should().Be(0);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Interactive menu — button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ClosesCurMenu_WhenBtnp4()
    {
        // Lua: if btnp(4) and not lb4 then curmenu=nil
        var fake = new FakeInputManager();
        fake.PressOnce(4);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.CurMenu = new InventoryMenu(state.Invent);

        MenuUpdater.Update(state, new PcraftGame());

        state.CurMenu.Should().BeNull();
    }

    [Fact]
    public void Update_IncrementsSel_WhenBtnp3()
    {
        // Lua: if(btnp(3)) intmenu.sel+=1
        var fake = new FakeInputManager();
        fake.PressOnce(3);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.Invent.Add(new ItemStack(PcraftData.Wood,  count: 1));
        state.Invent.Add(new ItemStack(PcraftData.Stone, count: 1));
        var menu = new InventoryMenu(state.Invent);
        state.CurMenu = menu;
        menu.Sel = 0;

        MenuUpdater.Update(state, new PcraftGame());

        menu.Sel.Should().Be(1);
    }

    [Fact]
    public void Update_DecrementsSel_WhenBtnp2()
    {
        // Lua: if(btnp(2)) intmenu.sel-=1
        var fake = new FakeInputManager();
        fake.PressOnce(2);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.Invent.Add(new ItemStack(PcraftData.Wood,  count: 1));
        state.Invent.Add(new ItemStack(PcraftData.Stone, count: 1));
        var menu = new InventoryMenu(state.Invent);
        state.CurMenu = menu;
        menu.Sel = 1;

        MenuUpdater.Update(state, new PcraftGame());

        menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Update_WrapsSel_WhenBtnp3AtLastItem()
    {
        // Lua: intmenu.sel = loop(intmenu.sel, intmenu.list) — wraps 0-based
        var fake = new FakeInputManager();
        fake.PressOnce(3);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.Invent.Add(new ItemStack(PcraftData.Wood,  count: 1));
        state.Invent.Add(new ItemStack(PcraftData.Stone, count: 1));
        var menu = new InventoryMenu(state.Invent);
        state.CurMenu = menu;
        menu.Sel = 1; // already at last (0-based, 2 items → max=1)

        MenuUpdater.Update(state, new PcraftGame());

        menu.Sel.Should().Be(0); // wraps back to 0
    }

    [Fact]
    public void Update_EquipsItem_WhenBtnp5_OnInventoryMenu()
    {
        // Lua: curitem = curmenu.list[curmenu.sel] ; curmenu=nil ; block5=true
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState();
        var axe = new ItemStack(PcraftData.Haxe) { Power = 1 };
        state.Invent.Add(axe);
        var menu = new InventoryMenu(state.Invent);
        state.CurMenu = menu;
        menu.Sel = 0;

        MenuUpdater.Update(state, new PcraftGame());

        state.CurItem.Should().BeSameAs(axe);
        state.CurMenu.Should().BeNull();
        state.Block5.Should().BeTrue();
    }

    [Fact]
    public void Update_CraftsItem_WhenBtnp5_OnCraftingMenu_WithIngredients()
    {
        // Lua: if cancraft(rec) then craft(rec)
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var state = new WorldState();
        var game  = new PcraftGame();
        game.InitRecipes();

        // Wood axe requires 5 wood
        state.Invent.Add(new ItemStack(PcraftData.Wood, count: 5));
        var craftMenu = new CraftingMenu(PcraftData.Workbench, game.WorkbenchRecipe, state.Invent);
        craftMenu.Sel = 0; // first entry = wood haxe recipe
        state.CurMenu = craftMenu;

        MenuUpdater.Update(state, game);

        state.Invent.Should().Contain(it => it.Type == PcraftData.Haxe);
    }

    // --------------------------------------------------------------------------
    #endregion
}
