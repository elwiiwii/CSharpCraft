using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Inventory;

internal static class InventoryOps
{
    // Count total Stackable units of a given material type.
    // ToolItem and UnstackableItem slots are never counted (not stackable materials).
    internal static int HowMany(List<InventorySlot> list, InventorySlot query)
    {
        int total = 0;
        foreach (InventorySlot slot in list)
        {
            if (slot.Type == query.Type)
                total += slot is StackableItem s ? s.Count : 1;
        }
        return total;
    }

    // Return the first StackableItem slot matching query.Type, or null.
    internal static StackableItem? FindStackable(List<InventorySlot> list, StackableItem query)
    {
        foreach (InventorySlot slot in list)
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
        if (elem is StackableItem stackableElem)
        {
            StackableItem? found = PcraftServices.FindStackable(list, stackableElem);
            if (found is null) return;
            found.Count -= stackableElem.Count;
            if (found.Count <= 0)
                _ = list.Remove(found);
        }
        else
        {
            InventorySlot? found = PcraftServices.FindByType(list, elem.Type);
            if (found is not null)
                _ = list.Remove(found);
        }
    }

    // Add item to list. StackableItem merges onto an existing slot of the same type;
    // ToolItem and UnstackableItem always create a new slot.
    internal static void AddItemInList(List<InventorySlot> list, InventorySlot item, int pos)
    {
        if (item is StackableItem stackable)
        {
            StackableItem? existing = PcraftServices.FindStackable(list, stackable);
            if (existing is not null)
            {
                existing.Count += stackable.Count;
                return;
            }
        }
        list.Insert(pos, item);
    }

    internal static int Loop(int sel, int count)
    {
        return ((sel % count) + count) % count;
    }

    internal static InventorySlot? FindByType(List<InventorySlot> list, ItemDef type)
    {
        foreach (InventorySlot slot in list)
        {
            if (slot.Type == type)
                return slot;
        }
        return null;
    }
}

