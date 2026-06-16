using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase;

internal class PcraftData
{
    private static PcraftData _current = new();
    internal static void SetData(PcraftData d) => _current = d ?? throw new ArgumentNullException(nameof(d));

    // ------------------------------------------------------------------
    #region Palettes
    // ------------------------------------------------------------------

    internal static readonly int[] PStone = [0, 1, 5, 13];
    internal static readonly int[] PIron = [1, 5, 13, 6];
    internal static readonly int[] PGold = [1, 9, 10, 7];

    // ------------------------------------------------------------------
    #endregion
    #region Tools
    // ------------------------------------------------------------------

    internal static readonly ItemDef Haxe = new("haxe", 98);
    internal static readonly ItemDef Sword = new("sword", 99);
    internal static readonly ItemDef Scythe = new("scythe", 100);
    internal static readonly ItemDef Shovel = new("shovel", 101);
    internal static readonly ItemDef Pick = new("pick", 102);

    // ------------------------------------------------------------------
    #endregion
    #region Materials
    // ------------------------------------------------------------------

    internal static readonly ItemDef Wood = new("wood", 103);
    internal static readonly ItemDef Sand = new("sand", 114, [15]);
    internal static readonly ItemDef Seed = new("seed", 115);
    internal static readonly ItemDef Wheat = new("wheat", 118, [4, 9, 10, 9]);
    internal static readonly HealthItemDef Apple = new("apple", 116) { GiveLife = 20 };
    internal static readonly ItemDef Glass = new("glass", 117);
    internal static readonly ItemDef Stone = new("stone", 118, PStone);
    internal static readonly ItemDef Iron = new("iron", 118, PIron);
    internal static readonly ItemDef Gold = new("gold", 118, PGold);
    internal static readonly ItemDef Gem = new("gem", 118, [1, 2, 14, 12]);
    internal static readonly ItemDef Fabric = new("fabric", 69);
    internal static readonly ItemDef Sail = new("sail", 70);
    internal static readonly ItemDef Glue = new("glue", 85, [1, 13, 12, 7]);
    internal static readonly ItemDef Boat = new("boat", 86);
    internal static readonly ItemDef Ichor = new("ichor", 114, [11]);
    internal static readonly HealthItemDef Potion = new("potion", 85, [1, 2, 8, 14]) { GiveLife = 100 };
    internal static readonly ItemDef IronBar = new("iron bar", 119, PIron);
    internal static readonly ItemDef GoldBar = new("gold bar", 119, PGold);
    internal static readonly HealthItemDef Bread = new("bread", 119, [1, 4, 15, 7]) { GiveLife = 40 };
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
    internal static readonly ChestItemDef Chest = new("chest", 92, 110);

    // ------------------------------------------------------------------
    #endregion
    #region Misc
    // ------------------------------------------------------------------

    internal static readonly ItemDef Inventary = new("inventory", 89);
    internal static readonly ItemDef EText = new("text", 103);

    // ------------------------------------------------------------------
    #endregion
    #region Tile Types
    // ------------------------------------------------------------------

    internal static readonly TileType TileWater = new(BlendGroup.Water);
    internal static readonly TileType TileSand = new(BlendGroup.Sand);
    internal static readonly TileType TileGrass = new(BlendGroup.Grass);
    internal static readonly TileType TileFarm = new(BlendGroup.Sand);
    internal static readonly TileType TileWheat = new(BlendGroup.Sand);
    internal static readonly TileType TileHole = new(BlendGroup.Sand);
    internal static readonly WallTileType TileRock = new(BlendGroup.Rock, spritePal: null, Stone, TileSand, life: 15);
    internal static readonly WallTileType TileTree = new(BlendGroup.Grass, [1, 5, 3, 11], Wood, TileGrass, life: 8);
    internal static readonly WallTileType TileIron = new(BlendGroup.Sand, [1, 1, 13, 6], Iron, TileSand, life: 45);
    internal static readonly WallTileType TileGold = new(BlendGroup.Sand, [1, 2, 9, 10], Gold, TileSand, life: 80);
    internal static readonly WallTileType TileGem = new(BlendGroup.Sand, [1, 2, 14, 12], Gem, TileSand, life: 160);

    internal static Tile TileFor(TileId id) => _current.OnTileFor(id);

    protected virtual Tile OnTileFor(TileId id)
    {
        return id switch
        {
            TileId.Water => new Tile(TileWater),
            TileId.Sand => new Tile(TileSand),
            TileId.Grass => new Tile(TileGrass),
            TileId.Rock => new Tile(TileRock),
            TileId.Tree => new Tile(TileTree),
            TileId.Farm => new Tile(TileFarm),
            TileId.Wheat => new Tile(TileWheat),
            TileId.Iron => new Tile(TileIron),
            TileId.Gold => new Tile(TileGold),
            TileId.Gem => new Tile(TileGem),
            TileId.Hole => new Tile(TileHole),
            _ => new Tile(TileWater),
        };
    }

    internal static int TileIdFor(TileType type) => _current.OnTileIdFor(type);

    protected virtual int OnTileIdFor(TileType type)
    {
        if (type == TileWater) return (int)TileId.Water;
        if (type == TileSand) return (int)TileId.Sand;
        if (type == TileGrass) return (int)TileId.Grass;
        return type == TileRock
            ? (int)TileId.Rock
            : type == TileTree
            ? (int)TileId.Tree
            : type == TileFarm
            ? (int)TileId.Farm
            : type == TileWheat
            ? (int)TileId.Wheat
            : type == TileIron
            ? (int)TileId.Iron
            : type == TileGold ? (int)TileId.Gold : type == TileGem ? (int)TileId.Gem : type == TileHole ? (int)TileId.Hole : -1;
    }

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

    internal static readonly SplashScreen MainMenu = new(spr: 128, lines: ["by nusan", "2016"]);
    internal static readonly SplashScreen IntroMenu = new(spr: 136, lines: ["a storm leaved you", "on a deserted island"]);
    internal static readonly SplashScreen DeathMenu = new(spr: 128, lines: ["you died", "alone ..."]);
    internal static readonly SplashScreen WinMenu = new(spr: 136, lines: ["you successfully escaped", "from the island"]);

    // ------------------------------------------------------------------
    #endregion
    #region Static Constructor
    // ------------------------------------------------------------------

    static PcraftData()
    {
        // Sub-phase A: create bench ItemDef objects (no Recipes yet)
        Workbench = new BenchItemDef("workbench", 89, 104, [1, 4, 9]);
        Stonebench = new BenchItemDef("stonebench", 89, 104, [1, 6, 13]);
        Furnace = new BenchItemDef("furnace", 90, 106);
        Anvil = new BenchItemDef("anvil", 91, 108);
        Factory = new BenchItemDef("factory", 74, 71);
        Chem = new BenchItemDef("chem lab", 76, 78);

        // Sub-phase B: build recipe lists
        List<Recipe> furnaceRecipes =
        [
            MakeRecipe(IronBar, 1,    [(Iron,  3)]),
            MakeRecipe(GoldBar, 1,    [(Gold,  3)]),
            MakeRecipe(Glass,   1,    [(Sand,  3)]),
            MakeRecipe(Bread,   1,    [(Wheat, 5)]),
        ];

        List<Recipe> factoryRecipes =
        [
            MakeRecipe(Sail,  1,    [(Fabric, 3), (Glue,  1)]),
            MakeRecipe(Boat,  null, [(Wood, 30),  (IronBar, 8), (Glue, 5), (Sail, 4)]),
        ];

        List<Recipe> chemRecipes =
        [
            MakeRecipe(Glue,   1, [(Glass, 1), (Ichor, 3)]),
            MakeRecipe(Potion, 1, [(Glass, 1), (Ichor, 1)]),
        ];

        List<Recipe> anvilRecipes = [];
        List<Recipe> stonebenchRecipes = [];
        List<Recipe> workbenchRecipes = [];

        ItemDef[] toolTypes = [Haxe, Pick, Sword, Shovel, Scythe];
        int[] quant = [5, 5, 7, 7, 7];
        int[] pows = [1, 2, 3, 4, 5];
        ItemDef[] materials = [Wood, Stone, IronBar, GoldBar, Gem];
        int[] mult = [1, 1, 1, 1, 3];
        List<Recipe>[] crafterTable =
        [
            workbenchRecipes, stonebenchRecipes,
            anvilRecipes, anvilRecipes, anvilRecipes
        ];

        for (int j = 0; j < pows.Length; j++)
        {
            for (int i = 0; i < toolTypes.Length; i++)
            {
                List<StackableItem> req = [new(materials[j], quant[i] * mult[j])];
                crafterTable[j].Add(new Recipe(new ToolItem(toolTypes[i], pows[j]), req));
            }
        }

        workbenchRecipes.Add(MakeRecipe(Workbench, null, [(Wood, 15)]));
        workbenchRecipes.Add(MakeRecipe(Stonebench, null, [(Stone, 15)]));
        workbenchRecipes.Add(MakeRecipe(Factory, null, [(Wood, 15), (Stone, 15)]));
        workbenchRecipes.Add(MakeRecipe(Chem, null, [(Wood, 10), (Glass, 3), (Gem, 10)]));
        workbenchRecipes.Add(MakeRecipe(Chest, null, [(Wood, 15), (Stone, 10)]));

        stonebenchRecipes.Add(MakeRecipe(Anvil, null, [(Iron, 25), (Wood, 10), (Stone, 25)]));
        stonebenchRecipes.Add(MakeRecipe(Furnace, null, [(Wood, 10), (Stone, 15)]));

        // Sub-phase C: assign Recipes to each bench
        Workbench.Recipes = workbenchRecipes;
        Stonebench.Recipes = stonebenchRecipes;
        Furnace.Recipes = furnaceRecipes;
        Anvil.Recipes = anvilRecipes;
        Factory.Recipes = factoryRecipes;
        Chem.Recipes = chemRecipes;
    }

    private static Recipe MakeRecipe(ItemDef type, int? count, (ItemDef def, int qty)[] reqPairs)
    {
        List<StackableItem> req = new(reqPairs.Length);
        foreach ((ItemDef def, int qty) in reqPairs)
            req.Add(new StackableItem(def, qty));
        InventorySlot output = count.HasValue
            ? new StackableItem(type, count.Value)
            : new UnstackableItem(type);
        return new Recipe(output, req);
    }

    // ------------------------------------------------------------------
    #endregion
    // ------------------------------------------------------------------
}
