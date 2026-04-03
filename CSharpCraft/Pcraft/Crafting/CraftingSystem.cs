using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Inventory;

namespace CSharpCraft.Pcraft.Crafting;

internal static class CraftingSystem
{
    internal static bool CanCraft(List<ItemStack> invent, Recipe recipe)
    {
        foreach (var req in recipe.Req)
        {
            if (InventoryOps.HowMany(invent, req) < (req.Count ?? 1))
                return false;
        }
        return true;
    }

    internal static void Craft(List<ItemStack> invent, Recipe recipe)
    {
        foreach (var req in recipe.Req)
            InventoryOps.RemInList(invent, req);

        var result = new ItemStack(recipe.Type, recipe.Count, recipe.List)
        {
            Power = recipe.Power
        };
        InventoryOps.AddItemInList(invent, result, 0);
    }
}
