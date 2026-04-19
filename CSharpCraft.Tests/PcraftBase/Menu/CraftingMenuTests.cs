using CSharpCraft.PcraftBase;
using CSharpCraft.PcraftBase.Crafting;
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
    // --------------------------------------------------------------------------
    #region Constructor argument validation
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenBenchTypeIsNull()
    {
        var recipes     = new List<Recipe>();
        var playerInvent = new List<ItemStack>();
        var act = () => new CraftingMenu(benchType: null!, recipes, playerInvent);
        act.Should().Throw<ArgumentNullException>().WithParameterName("benchType");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRecipesIsNull()
    {
        var playerInvent = new List<ItemStack>();
        var act = () => new CraftingMenu(PcraftData.Stonebench, recipes: null!, playerInvent);
        act.Should().Throw<ArgumentNullException>().WithParameterName("recipes");
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenPlayerInventIsNull()
    {
        var recipes = new List<Recipe>();
        var act = () => new CraftingMenu(PcraftData.Stonebench, recipes, playerInvent: null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playerInvent");
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Initial state
    // --------------------------------------------------------------------------

    [Fact]
    public void Constructor_InitializesSelToZero()
    {
        var menu = new CraftingMenu(
            PcraftData.Stonebench,
            new List<Recipe>(),
            new List<ItemStack>());
        menu.Sel.Should().Be(0);
    }

    [Fact]
    public void Constructor_ExposesRecipesReference()
    {
        var recipes = new List<Recipe>
        {
            new(PcraftData.Haxe, power: null, count: 1, list: null,
                req: [new ItemStack(PcraftData.Wood, count: 3)])
        };
        var menu = new CraftingMenu(PcraftData.Stonebench, recipes, new List<ItemStack>());
        menu.Recipes.Should().BeSameAs(recipes);
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

    /// <summary>A minimal recipe: 3x Wood → 1x Haxe.</summary>
    private static Recipe MakeHaxeRecipe() =>
        new(PcraftData.Haxe, power: null, count: 1, list: null,
            req: [new ItemStack(PcraftData.Wood, count: 3)]);

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
        var recipes = new List<Recipe> { MakeHaxeRecipe(), MakeHaxeRecipe() };
        var menu = new CraftingMenu(PcraftData.Stonebench, recipes, new List<ItemStack>());
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player, new PcraftGame());

        menu.Sel.Should().Be(1);
    }

    [Fact]
    public void Update_DecreasesSel_WhenBtnp2Pressed()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(2);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var recipes = new List<Recipe> { MakeHaxeRecipe(), MakeHaxeRecipe() };
        var menu = new CraftingMenu(PcraftData.Stonebench, recipes, new List<ItemStack>());
        menu.Sel = 1;
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player, new PcraftGame());

        menu.Sel.Should().Be(0);
    }

    // --------------------------------------------------------------------------
    #endregion
    #region Craft — btn5 with ingredients
    // --------------------------------------------------------------------------

    [Fact]
    public void Update_AddsResultItemToInventory_WhenBtnp5AndHasIngredients()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var playerInvent = new List<ItemStack> { new(PcraftData.Wood, count: 3) };
        var recipes = new List<Recipe> { MakeHaxeRecipe() };
        var menu = new CraftingMenu(PcraftData.Stonebench, recipes, playerInvent);
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };
        player.Invent.Add(new ItemStack(PcraftData.Wood, count: 3));

        menu.Update(player, new PcraftGame());

        player.Invent.Should().Contain(i => i.Type == PcraftData.Haxe,
            "crafting should add the result to the player inventory");
    }

    [Fact]
    public void Update_RemovesIngredients_FromInventory_WhenBtnp5AndHasIngredients()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var recipes = new List<Recipe> { MakeHaxeRecipe() };
        var menu = new CraftingMenu(PcraftData.Stonebench, recipes, new List<ItemStack>());
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };
        player.Invent.Add(new ItemStack(PcraftData.Wood, count: 3));

        menu.Update(player, new PcraftGame());

        player.Invent.Should().NotContain(i => i.Type == PcraftData.Wood,
            "all 3 wood should be consumed by the recipe");
    }

    [Fact]
    public void Update_DoesNotCraft_WhenPlayerMissingIngredient_AndBtnp5()
    {
        // Missing ingredient → CanCraft returns false → no Haxe added
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var recipes = new List<Recipe> { MakeHaxeRecipe() };
        var menu = new CraftingMenu(PcraftData.Stonebench, recipes, new List<ItemStack>());
        // player.Invent is empty — missing 3x Wood
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player, new PcraftGame());

        player.Invent.Should().BeEmpty("no ingredients means nothing is crafted");
    }

    [Fact]
    public void Update_DoesNotCraft_WhenRecipeListIsEmpty_AndBtnp5()
    {
        var fake = new FakeInputManager();
        fake.PressOnce(5);
        using var orch = BuildOrchestrator(fake);
        Pico8.Initialize(orch);
        var menu = new CraftingMenu(PcraftData.Stonebench, new List<Recipe>(), new List<ItemStack>());
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player, new PcraftGame());

        player.Invent.Should().BeEmpty();
        player.CurMenu.Should().BeSameAs(menu); // menu stays open
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
        var menu = new CraftingMenu(PcraftData.Stonebench, new List<Recipe>(), new List<ItemStack>());
        var player = new PlayerEntity(F32.Zero, F32.Zero) { CurMenu = menu };

        menu.Update(player, new PcraftGame());

        player.CurMenu.Should().BeNull();
    }

    // --------------------------------------------------------------------------
    #endregion
}
