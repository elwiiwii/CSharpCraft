using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase;

internal static class PcraftData
{
    // ------------------------------------------------------------------
    #region Palettes
    // ------------------------------------------------------------------

    internal static readonly int[] PStone = [0, 1, 5, 13];
    internal static readonly int[] PIron  = [1, 5, 13, 6];
    internal static readonly int[] PGold  = [1, 9, 10, 7];

    // ------------------------------------------------------------------
    #endregion
    #region Tools
    // ------------------------------------------------------------------

    internal static readonly ItemDef Haxe   = new("haxe",   98);
    internal static readonly ItemDef Sword  = new("sword",  99);
    internal static readonly ItemDef Scythe = new("scythe", 100);
    internal static readonly ItemDef Shovel = new("shovel", 101);
    internal static readonly ItemDef Pick   = new("pick",   102);

    // ------------------------------------------------------------------
    #endregion
    #region Materials
    // ------------------------------------------------------------------

    internal static readonly ItemDef Wood       = new("wood",        103);
    internal static readonly ItemDef Sand       = new("sand",        114, [15]);
    internal static readonly ItemDef Seed       = new("seed",        115);
    internal static readonly ItemDef Wheat      = new("wheat",       118, [4, 9, 10, 9]);
    internal static readonly HealthItemDef Apple  = new("apple",     116) { GiveLife = 20 };
    internal static readonly ItemDef Glass      = new("glass",       117);
    internal static readonly ItemDef Stone      = new("stone",       118, PStone);
    internal static readonly ItemDef Iron       = new("iron",        118, PIron);
    internal static readonly ItemDef Gold       = new("gold",        118, PGold);
    internal static readonly ItemDef Gem        = new("gem",         118, [1, 2, 14, 12]);
    internal static readonly ItemDef Fabric     = new("fabric",      69);
    internal static readonly ItemDef Sail       = new("sail",        70);
    internal static readonly ItemDef Glue       = new("glue",        85, [1, 13, 12, 7]);
    internal static readonly ItemDef Boat       = new("boat",        86);
    internal static readonly ItemDef Ichor      = new("ichor",       114, [11]);
    internal static readonly HealthItemDef Potion = new("potion",    85, [1, 2, 8, 14]) { GiveLife = 100 };
    internal static readonly ItemDef IronBar    = new("iron bar",    119, PIron);
    internal static readonly ItemDef GoldBar    = new("gold bar",    119, PGold);
    internal static readonly HealthItemDef Bread  = new("bread",     119, [1, 4, 15, 7]) { GiveLife = 40 };
    internal static readonly ItemDef PickupTool = new("pickup tool", 73);

    // ------------------------------------------------------------------
    #endregion
    #region Benches
    // ------------------------------------------------------------------

    internal static readonly BenchItemDef Workbench;
    internal static readonly BenchItemDef Stonebench;
    internal static readonly BenchItemDef Furnace;
    internal static readonly BenchItemDef Anvil;
    internal static readonly BenchItemDef Factory;
    internal static readonly BenchItemDef Chem;
    internal static readonly ChestItemDef Chest = new("chest", 92) { BigSpr = 110 };

    // ------------------------------------------------------------------
    #endregion
    #region Misc
    // ------------------------------------------------------------------

    internal static readonly ItemDef Inventary  = new("inventory",   89);
    internal static readonly ItemDef EText      = new("text",        103);

    // ------------------------------------------------------------------
    #endregion
    #region Ground Types
    // ------------------------------------------------------------------

    internal static readonly GroundType GrWater = new(0, 0);
    internal static readonly GroundType GrSand  = new(1, 1);
    internal static readonly GroundType GrGrass = new(2, 2);
    internal static readonly GroundType GrRock  = new(3, 3) { Mat = Stone,  Tile = GrSand,  Life = 15 };
    internal static readonly GroundType GrTree  = new(4, 2) { Mat = Wood,   Tile = GrGrass, Life = 8, IsTree = true, Pal = [1, 5, 3, 11] };
    internal static readonly GroundType GrFarm  = new(5, 1);
    internal static readonly GroundType GrWheat = new(6, 1);
    internal static readonly GroundType GrPlant = new(7, 2);
    internal static readonly GroundType GrIron  = new(8, 1) { Mat = Iron,   Tile = GrSand,  Life = 45, IsTree = true, Pal = [1, 1, 13, 6] };
    internal static readonly GroundType GrGold  = new(9, 1) { Mat = Gold,   Tile = GrSand,  Life = 80, IsTree = true, Pal = [1, 2, 9, 10] };
    internal static readonly GroundType GrGem   = new(10, 1){ Mat = Gem,    Tile = GrSand,  Life = 160, IsTree = true, Pal = [1, 2, 14, 12] };
    internal static readonly GroundType GrHole  = new(11, 1);

    internal static readonly GroundType[] Grounds =
    [
        GrWater, GrSand, GrGrass, GrRock, GrTree,
        GrFarm,  GrWheat, GrPlant, GrIron, GrGold,
        GrGem,   GrHole
    ];

    // ------------------------------------------------------------------
    #endregion
    #region Power Data
    // ------------------------------------------------------------------

    internal static readonly string[] PwrNames = ["wood", "stone", "iron", "gold", "gem"];

    internal static readonly int[][] PwrPal =
    [
        [2, 2, 4, 4],
        [5, 2, 4, 13],
        [13, 5, 13, 6],
        [9, 2, 9, 10],
        [13, 2, 14, 12]
    ];

    // ------------------------------------------------------------------
    #endregion
    #region Menus
    // ------------------------------------------------------------------

    internal static readonly MenuState MainMenu  = new(type: Inventary, spr: 128, text: "by nusan",              text2: "2016");
    internal static readonly MenuState IntroMenu = new(type: Inventary, spr: 136, text: "a storm leaved you",    text2: "on a deserted island");
    internal static readonly MenuState DeathMenu = new(type: Inventary, spr: 128, text: "you died",              text2: "alone ...");
    internal static readonly MenuState WinMenu   = new(type: Inventary, spr: 136, text: "you successfully escaped", text2: "from the island");

    // ------------------------------------------------------------------
    #endregion
    #region Static Constructor
    // ------------------------------------------------------------------

    static PcraftData()
    {
        // Sub-phase A: create bench ItemDef objects (no Recipes yet)
        Workbench  = new BenchItemDef("workbench",  89, [1, 4, 9])    { BigSpr = 104 };
        Stonebench = new BenchItemDef("stonebench", 89, [1, 6, 13])   { BigSpr = 104 };
        Furnace    = new BenchItemDef("furnace",    90)                { BigSpr = 106 };
        Anvil      = new BenchItemDef("anvil",      91)                { BigSpr = 108 };
        Factory    = new BenchItemDef("factory",    74)                { BigSpr = 71  };
        Chem       = new BenchItemDef("chem lab",   76)                { BigSpr = 78  };

        // Sub-phase B: build recipe lists
        var furnaceRecipes = new List<Recipe>
        {
            MakeRecipe(IronBar, 1,    [(Iron,  3)]),
            MakeRecipe(GoldBar, 1,    [(Gold,  3)]),
            MakeRecipe(Glass,   1,    [(Sand,  3)]),
            MakeRecipe(Bread,   1,    [(Wheat, 5)]),
        };

        var factoryRecipes = new List<Recipe>
        {
            MakeRecipe(Sail,  1,    [(Fabric, 3), (Glue,  1)]),
            MakeRecipe(Boat,  null, [(Wood, 30),  (IronBar, 8), (Glue, 5), (Sail, 4)]),
        };

        var chemRecipes = new List<Recipe>
        {
            MakeRecipe(Glue,   1, [(Glass, 1), (Ichor, 3)]),
            MakeRecipe(Potion, 1, [(Glass, 1), (Ichor, 1)]),
        };

        var anvilRecipes    = new List<Recipe>();
        var stonebenchRecipes = new List<Recipe>();
        var workbenchRecipes  = new List<Recipe>();

        ItemDef[] toolTypes = [Haxe, Pick, Sword, Shovel, Scythe];
        int[]     quant     = [5, 5, 7, 7, 7];
        int[]     pows      = [1, 2, 3, 4, 5];
        ItemDef[] materials = [Wood, Stone, IronBar, GoldBar, Gem];
        int[]     mult      = [1, 1, 1, 1, 3];
        List<Recipe>[] crafterTable =
        [
            workbenchRecipes, stonebenchRecipes,
            anvilRecipes, anvilRecipes, anvilRecipes
        ];

        for (int j = 0; j < pows.Length; j++)
        {
            for (int i = 0; i < toolTypes.Length; i++)
            {
                var req = new List<ItemStack> { new(materials[j], count: quant[i] * mult[j]) };
                crafterTable[j].Add(new Recipe(toolTypes[i], pows[j], count: null, req));
            }
        }

        workbenchRecipes.Add(MakeRecipe(Workbench,  null, [(Wood,  15)]));
        workbenchRecipes.Add(MakeRecipe(Stonebench, null, [(Stone, 15)]));
        workbenchRecipes.Add(MakeRecipe(Factory,    null, [(Wood,  15), (Stone, 15)]));
        workbenchRecipes.Add(MakeRecipe(Chem,       null, [(Wood,  10), (Glass, 3), (Gem, 10)]));
        workbenchRecipes.Add(MakeRecipe(Chest,      null, [(Wood,  15), (Stone, 10)]));

        stonebenchRecipes.Add(MakeRecipe(Anvil,   null, [(Iron, 25), (Wood, 10), (Stone, 25)]));
        stonebenchRecipes.Add(MakeRecipe(Furnace, null, [(Wood, 10), (Stone, 15)]));

        // Sub-phase C: assign Recipes to each bench
        Workbench.Recipes  = workbenchRecipes;
        Stonebench.Recipes = stonebenchRecipes;
        Furnace.Recipes    = furnaceRecipes;
        Anvil.Recipes      = anvilRecipes;
        Factory.Recipes    = factoryRecipes;
        Chem.Recipes       = chemRecipes;
    }

    private static Recipe MakeRecipe(ItemDef type, int? count, (ItemDef def, int qty)[] reqPairs)
    {
        var req = new List<ItemStack>(reqPairs.Length);
        foreach (var (def, qty) in reqPairs)
            req.Add(new ItemStack(def, count: qty));
        return new Recipe(type, power: null, count, req);
    }

    // ------------------------------------------------------------------
    #endregion
    // ------------------------------------------------------------------
}
