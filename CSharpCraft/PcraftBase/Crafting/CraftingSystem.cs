using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Crafting;

internal static class CraftingSystem
{
    internal static bool CanCraft(List<InventorySlot> invent, Recipe recipe)
    {
        foreach (var req in recipe.Req)
        {
            if (PcraftServices.HowMany(invent, req) < req.Count)
                return false;
        }
        return true;
    }

    internal static void Craft(List<InventorySlot> invent, Recipe recipe)
    {
        foreach (var req in recipe.Req)
            PcraftServices.RemInList(invent, req);

        InventorySlot result = recipe.Power.HasValue
            ? new ToolItem(recipe.Type, recipe.Power.Value)
            : recipe.Count.HasValue
                ? new StackableItem(recipe.Type, recipe.Count.Value)
                : new UnstackableItem(recipe.Type);
        PcraftServices.AddItemInList(invent, result, 0);
    }
}

