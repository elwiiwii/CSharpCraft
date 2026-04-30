using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Crafting;

internal static class CraftingSystem
{
    internal static bool CanCraft(List<ItemStack> invent, Recipe recipe)
    {
        foreach (var req in recipe.Req)
        {
            if (PcraftServices.HowMany(invent, req) < (req.Count ?? 1))
                return false;
        }
        return true;
    }

    internal static void Craft(List<ItemStack> invent, Recipe recipe)
    {
        foreach (var req in recipe.Req)
            PcraftServices.RemInList(invent, req);

        var result = new ItemStack(recipe.Type, recipe.Count)
        {
            Power = recipe.Power
        };
        PcraftServices.AddItemInList(invent, result, 0);
    }
}
