using CSharpCraft.Pcraft;
using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Map;
using CSharpCraft.Pcraft.Update;
using CSharpCraft.Tests.Infrastructure;
using FluentAssertions;
using PSharp8;
using PSharp8.Audio;
using PSharp8.Input;
using PSharp8.Scene;
using Xunit;

namespace CSharpCraft.Tests.Pcraft.Update;

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

        var result = MenuUpdater.Update(state, game, new Random(0));

        result.Should().BeFalse();
    }

    // --------------------------------------------------------------------------
    #endregion
}

[Collection("Fna")]
public sealed class MenuUpdaterFnaTests(FnaFixture fixture)
{
    private sealed class NullScene : IScene
    {
        public string? Name => null;
        public void Init(ISceneSetup setup) { }
        public string? SpritesPath => null;
        public string? MapPath => null;
        public string? FlagData => null;
        public IReadOnlyList<Soundtrack> Music => [];
        public IReadOnlyList<SfxPack> Sfx => [];
    }

    private GameOrchestrator BuildOrchestrator(IInputManager? input = null)
        => new(
            ".",
            ".",
            ".",
            new NullScene(),
            fixture.GraphicsDevice,
            fixture.GraphicsDeviceManager,
            fixture.Window,
            inputManager: input);

    private static MenuState MakeInvMenu(List<ItemStack> list)
        => new(PcraftData.Inventary, list, spr: 0, text: null, text2: null);

    // --------------------------------------------------------------------------
    #region Splash menu (Spr != 0) — no button pressed
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_ReturnsTrue_WhenSplashMenuActive()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { CurMenu = PcraftData.MainMenu };

        MenuUpdater.Update(state, new PcraftGame(), new Random(0)).Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotChangeCurMenu_WhenSplashAndNoBtnp4()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState { CurMenu = PcraftData.MainMenu };

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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

        MenuUpdater.Update(state, game, new Random(42));

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
        state.MenuInvent = MakeInvMenu(state.Invent);
        state.CurMenu    = MakeInvMenu(state.Invent);

        MenuUpdater.Update(state, new PcraftGame(), new Random(0)).Should().BeTrue();
    }

    [Fact]
    public void Update_DoesNotCloseCurMenu_WhenNoBtnp4()
    {
        using var orch = BuildOrchestrator();
        Pico8.Initialize(orch);
        var state = new WorldState();
        state.MenuInvent = MakeInvMenu(state.Invent);
        var menu = MakeInvMenu(state.Invent);
        state.CurMenu = menu;

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        state.MenuInvent = MakeInvMenu(state.Invent);
        state.CurMenu    = MakeInvMenu(state.Invent);

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        var menu = MakeInvMenu(state.Invent);
        state.MenuInvent = menu;
        state.CurMenu    = menu;
        menu.Sel = 0;

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        state.MenuInvent = MakeInvMenu(state.Invent);
        state.CurMenu    = MakeInvMenu(state.Invent);

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        var menu = MakeInvMenu(state.Invent);
        state.MenuInvent = menu;
        state.CurMenu    = menu;
        menu.Sel = 0;

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        var menu = MakeInvMenu(state.Invent);
        state.MenuInvent = menu;
        state.CurMenu    = menu;
        menu.Sel = 1;

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        var menu = MakeInvMenu(state.Invent);
        state.MenuInvent = menu;
        state.CurMenu    = menu;
        menu.Sel = 1; // already at last (0-based, 2 items → max=1)

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        var menu = MakeInvMenu(state.Invent);
        state.MenuInvent = menu;
        state.CurMenu    = menu;
        menu.Sel = 0;

        MenuUpdater.Update(state, new PcraftGame(), new Random(0));

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
        var craftMenu = new MenuState(PcraftData.Workbench, list: null, spr: 0, text: null, text2: null)
        {
            RecipeList = game.WorkbenchRecipe
        };
        state.MenuInvent = MakeInvMenu(state.Invent);
        craftMenu.Sel    = 0; // first entry = wood haxe recipe
        state.CurMenu    = craftMenu;

        MenuUpdater.Update(state, game, new Random(0));

        state.Invent.Should().Contain(it => it.Type == PcraftData.Haxe);
    }

    // --------------------------------------------------------------------------
    #endregion
}
