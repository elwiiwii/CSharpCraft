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

        PcraftServices.AddItemInList(invent, recipe.Output, invent.Count);
    }
}

