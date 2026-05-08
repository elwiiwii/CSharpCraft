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

public sealed class CraftingMenuPureTests
{
    private static readonly BenchItemDef _testBench = new("test bench", 89, 104);
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBenchTypeIsNull()
    {
        List<InventorySlot> playerInvent = [];
        Func<CraftingMenu> act = () => new CraftingMenu(benchType: null!, playerInvent);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("benchType");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPlayerInventIsNull()
    {
        Func<CraftingMenu> act = () => new CraftingMenu(_testBench, playerInvent: null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("playerInvent");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Initial state
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_InitializesSelToZero()
    {
        CraftingMenu menu = new(_testBench, []);
        _ = menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Constructor_ExposesRecipesFromBench()
    {
        BenchItemDef bench = new("test", 0, 0);
        List<Recipe> recipes =
        [
            new Recipe(new StackableItem(PcraftData.Haxe, 1),
                [new StackableItem(PcraftData.Wood, 3)])
        ];
        bench.Recipes = recipes;
        CraftingMenu menu = new(bench, []);
        _ = menu.Recipes.Should().BeSameAs(recipes);
    }

    // --------------------------------------------------------------------------
    #endregion
}

// --------------------------------------------------------------------------
// FNA tests — require real graphics/audio orchestrator for Pico8.Btnp/Sfx
// --------------------------------------------------------------------------

[Collection("Fna")]
public sealed class CraftingMenuFnaTests(FnaFixture fixture) : IDisposable
{
    private TempMusicDirectory? _musicDir;
    private string? _sfxDir;

    public void Dispose()
    {
        _musicDir?.Dispose();
        if (_sfxDir is not null && Directory.Exists(_sfxDir))
            Directory.Delete(_sfxDir, recursive: true);
    }

    private static readonly BenchItemDef _testBench = new("test bench", 89, 104);

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

    /// <summary>A minimal recipe: 3x Wood → 1x Haxe.</summary>
    private static Recipe MakeHaxeRecipe()
    {
        return new(new StackableItem(PcraftData.Haxe, 1),
            [new StackableItem(PcraftData.Wood, 3)]);
    }

    private static CraftingMenu MakeBenchMenu(List<InventorySlot> invent, List<Recipe>? recipes = null)
    {
        BenchItemDef bench = new("test", 0, 0)
        {
            Recipes = recipes ?? [MakeHaxeRecipe()]
        };
        return new CraftingMenu(bench, invent);
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
        List<Recipe> recipes = [MakeHaxeRecipe(), MakeHaxeRecipe()];
        CraftingMenu menu = MakeBenchMenu([], recipes);
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
        List<Recipe> recipes = [MakeHaxeRecipe(), MakeHaxeRecipe()];
        CraftingMenu menu = MakeBenchMenu([], recipes);
        menu.Sel = 1;
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = menu.Sel.Should().Be(0);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Craft — btn5 with ingredients
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AddsResultItemToInventory_WhenBtnp5AndHasIngredients()
    {
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<InventorySlot> playerInvent = [new StackableItem(PcraftData.Wood, 3)];
        List<Recipe> recipes = [MakeHaxeRecipe()];
        CraftingMenu menu = MakeBenchMenu(playerInvent, recipes);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };
        player.Invent.Add(new StackableItem(PcraftData.Wood, 3));

        _ = menu.Update(player);

        _ = player.Invent.Should().Contain(i => i.Type == PcraftData.Haxe,
            "crafting should add the result to the player inventory");
    }

    [Fact]
    public void Update_RemovesIngredients_FromInventory_WhenBtnp5AndHasIngredients()
    {
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<Recipe> recipes = [MakeHaxeRecipe()];
        CraftingMenu menu = MakeBenchMenu([], recipes);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };
        player.Invent.Add(new StackableItem(PcraftData.Wood, 3));

        _ = menu.Update(player);

        _ = player.Invent.Should().NotContain(i => i.Type == PcraftData.Wood,
            "all 3 wood should be consumed by the recipe");
    }

    [Fact]
    public void Update_DoesNotCraft_WhenPlayerMissingIngredient_AndBtnp5()
    {
        // Missing ingredient → CanCraft returns false → no Haxe added
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        List<Recipe> recipes = [MakeHaxeRecipe()];
        CraftingMenu menu = MakeBenchMenu([], recipes);
        // player.Invent is empty — missing 3x Wood
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = player.Invent.Should().BeEmpty("no ingredients means nothing is crafted");
    }

    [Fact]
    public void Update_DoesNotCraft_WhenRecipeListIsEmpty_AndBtnp5()
    {
        FakeInputManager fake = new();
        fake.PressOnce(5);
        using GameOrchestrator orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        CraftingMenu menu = MakeBenchMenu([], []);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = player.Invent.Should().BeEmpty();
        _ = player.CurMenu.Should().BeSameAs(menu); // menu stays open
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
        CraftingMenu menu = MakeBenchMenu([], []);
        PlayerEntity player = new(F32.Zero, F32.Zero) { CurMenu = menu };

        _ = menu.Update(player);

        _ = player.CurMenu.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
}
