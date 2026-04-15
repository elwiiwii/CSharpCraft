using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Inventory;

internal static class InventoryOps
{
    internal static int HowMany(List<ItemStack> list, ItemStack query)
    {
        int? queryPower = query.Power;
        int count = 0;
        foreach (var slot in list)
        {
            if (slot.Type != query.Type) continue;
            if (queryPower is not null && queryPower != slot.Power) continue;
            count += slot.Count ?? 1;
        }
        return count;
    }

    internal static ItemStack? IsInList(List<ItemStack> list, ItemStack query)
    {
        int? queryPower = query.Power;
        foreach (var slot in list)
        {
            if (slot.Type != query.Type) continue;
            if (queryPower is not null && queryPower != slot.Power) continue;
            return slot;
        }
        return null;
    }

    internal static void RemInList(List<ItemStack> list, ItemStack elem)
    {
        var it = IsInList(list, elem);
        if (it is null) return;

        var itStack = it;
        if (itStack?.Count is not null)
        {
            itStack.Count -= elem.Count ?? 1;
            if (itStack.Count <= 0)
                list.Remove(it);
        }
        else
        {
            list.Remove(it);
        }
    }

    internal static void AddItemInList(List<ItemStack> list, ItemStack item, int pos)
    {
        var existing = IsInList(list, item);
        var existingStack = existing;
        if (existing is null || existingStack?.Count is null)
        {
            AddPlace(list, item, pos);
        }
        else
        {
            existingStack.Count += item.Count ?? 1;
        }
    }

    internal static void AddPlace(List<ItemStack> list, ItemStack item, int pos)
    {
        if (pos >= 0 && pos < list.Count)
            list.Insert(pos, item);
        else
            list.Add(item);
    }

    internal static int Loop(int sel, int count)
    {
        return ((sel % count) + count) % count;
    }
}
