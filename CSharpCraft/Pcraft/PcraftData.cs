using CSharpCraft.Pcraft.Data;

namespace CSharpCraft.Pcraft;

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
    internal static readonly ItemDef Apple      = new("apple",       116);
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
    internal static readonly ItemDef Potion     = new("potion",      85, [1, 2, 8, 14]);
    internal static readonly ItemDef IronBar    = new("iron bar",    119, PIron);
    internal static readonly ItemDef GoldBar    = new("gold bar",    119, PGold);
    internal static readonly ItemDef Bread      = new("bread",       119, [1, 4, 15, 7]);
    internal static readonly ItemDef PickupTool = new("pickup tool", 73);

    // ------------------------------------------------------------------
    #endregion
    #region Benches
    // ------------------------------------------------------------------

    internal static readonly ItemDef Workbench  = MakeBigSpr(104, new("workbench",  89, [1, 4, 9], beCraft: true));
    internal static readonly ItemDef Stonebench = MakeBigSpr(104, new("stonebench", 89, [1, 6, 13], beCraft: true));
    internal static readonly ItemDef Furnace    = MakeBigSpr(106, new("furnace",    90, null, beCraft: true));
    internal static readonly ItemDef Anvil      = MakeBigSpr(108, new("anvil",      91, null, beCraft: true));
    internal static readonly ItemDef Factory    = MakeBigSpr(71,  new("factory",    74, null, beCraft: true));
    internal static readonly ItemDef Chem       = MakeBigSpr(78,  new("chem lab",   76, null, beCraft: true));
    internal static readonly ItemDef Chest      = MakeBigSpr(110, new("chest",      92));

    // ------------------------------------------------------------------
    #endregion
    #region Misc
    // ------------------------------------------------------------------

    internal static readonly ItemDef Inventary  = new("inventory",   89);
    internal static readonly ItemDef EText      = new("text",        103);

    // ------------------------------------------------------------------
    #endregion
    #region Entities
    // ------------------------------------------------------------------

    internal const int Player = 1;
    internal const int Zombi  = 2;

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

    internal static readonly GroundType LastGround = GrSand;

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

    internal static readonly MenuState MainMenu  = new(type: Inventary, list: null, spr: 128, text: "by nusan",              text2: "2016");
    internal static readonly MenuState IntroMenu = new(type: Inventary, list: null, spr: 136, text: "a storm leaved you",    text2: "on a deserted island");
    internal static readonly MenuState DeathMenu = new(type: Inventary, list: null, spr: 128, text: "you died",              text2: "alone ...");
    internal static readonly MenuState WinMenu   = new(type: Inventary, list: null, spr: 136, text: "you successfully escaped", text2: "from the island");

    // ------------------------------------------------------------------
    #endregion
    #region Helpers
    // ------------------------------------------------------------------

    static PcraftData()
    {
        Apple.GiveLife  = 20;
        Potion.GiveLife = 100;
        Bread.GiveLife  = 40;
    }

    private static ItemDef MakeBigSpr(int spr, ItemDef def)
    {
        def.BigSpr = spr;
        def.Drop   = true;
        return def;
    }

    // ------------------------------------------------------------------
    #endregion
    // ------------------------------------------------------------------
}
