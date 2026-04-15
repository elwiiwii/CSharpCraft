using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase;

internal sealed class PcraftGame
{
    internal List<Recipe> FurnaceRecipe    { get; } = [];
    internal List<Recipe> WorkbenchRecipe  { get; } = [];
    internal List<Recipe> StonebenchRecipe { get; } = [];
    internal List<Recipe> AnvilRecipe      { get; } = [];
    internal List<Recipe> FactoryRecipe    { get; } = [];
    internal List<Recipe> ChemRecipe       { get; } = [];

    internal void InitRecipes()
    {
        FurnaceRecipe.Clear();
        WorkbenchRecipe.Clear();
        StonebenchRecipe.Clear();
        AnvilRecipe.Clear();
        FactoryRecipe.Clear();
        ChemRecipe.Clear();

        FactoryRecipe.Add(MakeRecipe(PcraftData.Sail,  1,    null, [(PcraftData.Fabric, 3), (PcraftData.Glue,  1)]));
        FactoryRecipe.Add(MakeRecipe(PcraftData.Boat,  null, null, [(PcraftData.Wood, 30),  (PcraftData.IronBar, 8), (PcraftData.Glue, 5), (PcraftData.Sail, 4)]));

        ChemRecipe.Add(MakeRecipe(PcraftData.Glue,   1, null, [(PcraftData.Glass, 1), (PcraftData.Ichor, 3)]));
        ChemRecipe.Add(MakeRecipe(PcraftData.Potion, 1, null, [(PcraftData.Glass, 1), (PcraftData.Ichor, 1)]));

        FurnaceRecipe.Add(MakeRecipe(PcraftData.IronBar, 1, null, [(PcraftData.Iron,  3)]));
        FurnaceRecipe.Add(MakeRecipe(PcraftData.GoldBar, 1, null, [(PcraftData.Gold,  3)]));
        FurnaceRecipe.Add(MakeRecipe(PcraftData.Glass,   1, null, [(PcraftData.Sand,  3)]));
        FurnaceRecipe.Add(MakeRecipe(PcraftData.Bread,   1, null, [(PcraftData.Wheat, 5)]));

        ItemDef[] toolTypes   = [PcraftData.Haxe, PcraftData.Pick, PcraftData.Sword, PcraftData.Shovel, PcraftData.Scythe];
        int[]     quant       = [5, 5, 7, 7, 7];
        int[]     pows        = [1, 2, 3, 4, 5];
        ItemDef[] materials   = [PcraftData.Wood, PcraftData.Stone, PcraftData.IronBar, PcraftData.GoldBar, PcraftData.Gem];
        int[]     mult        = [1, 1, 1, 1, 3];
        List<Recipe>[] crafterTable =
        [
            WorkbenchRecipe, StonebenchRecipe,
            AnvilRecipe, AnvilRecipe, AnvilRecipe
        ];

        for (int j = 0; j < pows.Length; j++)
        {
            for (int i = 0; i < toolTypes.Length; i++)
            {
                var req = new List<ItemStack> { new(materials[j], count: quant[i] * mult[j]) };
                crafterTable[j].Add(new Recipe(toolTypes[i], pows[j], count: null, list: null, req));
            }
        }

        WorkbenchRecipe.Add(MakeRecipe(PcraftData.Workbench,  null, WorkbenchRecipe,  [(PcraftData.Wood,  15)]));
        WorkbenchRecipe.Add(MakeRecipe(PcraftData.Stonebench, null, StonebenchRecipe, [(PcraftData.Stone, 15)]));
        WorkbenchRecipe.Add(MakeRecipe(PcraftData.Factory,    null, FactoryRecipe,    [(PcraftData.Wood,  15), (PcraftData.Stone, 15)]));
        WorkbenchRecipe.Add(MakeRecipe(PcraftData.Chem,       null, ChemRecipe,       [(PcraftData.Wood,  10), (PcraftData.Glass, 3), (PcraftData.Gem, 10)]));
        WorkbenchRecipe.Add(MakeRecipe(PcraftData.Chest,      null, null,             [(PcraftData.Wood,  15), (PcraftData.Stone, 10)]));

        StonebenchRecipe.Add(MakeRecipe(PcraftData.Anvil,    null, AnvilRecipe,    [(PcraftData.Iron, 25), (PcraftData.Wood, 10), (PcraftData.Stone, 25)]));
        StonebenchRecipe.Add(MakeRecipe(PcraftData.Furnace,  null, FurnaceRecipe,  [(PcraftData.Wood, 10), (PcraftData.Stone, 15)]));
    }

    internal void Init()
    {
        Pico8.Music(4, 10000);
        InitRecipes();
    }

    private static Recipe MakeRecipe(ItemDef type, int? count, List<Recipe>? list, (ItemDef def, int qty)[] reqPairs)
    {
        var req = new List<ItemStack>(reqPairs.Length);
        foreach (var (def, qty) in reqPairs)
            req.Add(new ItemStack(def, count: qty));
        return new Recipe(type, power: null, count, list, req);
    }
}
