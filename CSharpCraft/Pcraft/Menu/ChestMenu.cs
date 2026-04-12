#nullable enable

using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Draw;
using CSharpCraft.Pcraft.Inventory;
using PSharp8;

namespace CSharpCraft.Pcraft.Menu;

internal sealed class ChestMenu(List<ItemStack> chestItems, List<ItemStack> playerItems) : IMenu
{
    internal List<ItemStack> ChestItems  { get; } = chestItems  ?? throw new ArgumentNullException(nameof(chestItems));
    internal List<ItemStack> PlayerItems { get; } = playerItems ?? throw new ArgumentNullException(nameof(playerItems));
    internal int Sel       { get; set; } = 0;
    internal int Off       { get; set; } = 0;
    internal int TabToggle { get; set; } = 0;

    public void Update(WorldState state, PcraftGame game)
    {
        if (Pico8.Btnp(0)) { TabToggle = InventoryOps.Loop(TabToggle - 1, 2); Pico8.Sfx(18); }
        if (Pico8.Btnp(1)) { TabToggle = InventoryOps.Loop(TabToggle + 1, 2); Pico8.Sfx(18); }

        var activeList = TabToggle == 0 ? ChestItems : PlayerItems;
        var otherList  = TabToggle == 0 ? PlayerItems : ChestItems;

        if (activeList.Count > 0 && Pico8.Btnp(5))
        {
            var item = activeList[Sel];
            activeList.RemoveAt(Sel);
            otherList.Add(item);
            Pico8.Sfx(16);
        }

        if (Pico8.Btnp(4) && !state.Lb4)
        {
            state.CurMenu = null;
            Pico8.Sfx(17);
        }
    }

    public void Draw(WorldState state) => MenuOverlayDrawer.DrawChestPanels(this);
}
