using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Inventory;

internal static class InventoryOps
{
    // Count total Stackable units of a given material type.
    // ToolItem and UnstackableItem slots are never counted (not stackable materials).
    internal static int HowMany(List<InventorySlot> list, StackableItem query)
    {
        int total = 0;
        foreach (var slot in list)
        {
            if (slot is StackableItem s && s.Type == query.Type)
                total += s.Count;
        }
        return total;
    }

    // Return the first StackableItem slot matching query.Type, or null.
    internal static StackableItem? FindStackable(List<InventorySlot> list, StackableItem query)
    {
        foreach (var slot in list)
        {
            if (slot is StackableItem s && s.Type == query.Type)
                return s;
        }
        return null;
    }

    // Remove/decrement elem from list.
    // StackableItem: decrement by elem.Count; remove slot when count reaches zero.
    // ToolItem / UnstackableItem: remove the first slot whose Type matches.
    internal static void RemInList(List<InventorySlot> list, InventorySlot elem)
    {
        if (elem is StackableItem StackableElem)
        {
            var found = FindStackable(list, StackableElem);
            if (found is null) return;
            found.Count -= StackableElem.Count;
            if (found.Count <= 0)
                list.Remove(found);
        }
        else
        {
            var found = FindByType(list, elem.Type);
            if (found is not null)
                list.Remove(found);
        }
    }

    // Add item to list. StackableItem merges onto an existing slot of the same type;
    // ToolItem and UnstackableItem always create a new slot.
    internal static void AddItemInList(List<InventorySlot> list, InventorySlot item, int pos)
    {
        if (item is StackableItem Stackable)
        {
            var existing = FindStackable(list, Stackable);
            if (existing is not null)
            {
                existing.Count += Stackable.Count;
                return;
            }
        }
        AddPlace(list, item, pos);
    }

    internal static void AddPlace(List<InventorySlot> list, InventorySlot item, int pos)
    {
        if (pos >= 0 && pos < list.Count)
            list.Insert(pos, item);
        else
            list.Add(item);
    }

    internal static int Loop(int sel, int count)
        => ((sel % count) + count) % count;

    private static InventorySlot? FindByType(List<InventorySlot> list, ItemDef type)
    {
        foreach (var slot in list)
        {
            if (slot.Type == type)
                return slot;
        }
        return null;
    }
}

