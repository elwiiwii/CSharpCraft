using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Menu;

internal sealed class ChestMenu(List<InventorySlot> chestItems, List<InventorySlot> playerItems) : IMenu
{
    internal List<InventorySlot> ChestItems { get; } = chestItems ?? throw new ArgumentNullException(nameof(chestItems));
    internal List<InventorySlot> PlayerItems { get; } = playerItems ?? throw new ArgumentNullException(nameof(playerItems));
    internal int Sel { get; set; } = 0;
    internal int Off { get; set; } = 0;
    internal int TabToggle { get; set; } = 0;

    public bool Update(PlayerEntity player)
    {
        if (Pico8.Btnp(0)) { TabToggle = PcraftServices.Loop(TabToggle - 1, 2); Pico8.Sfx(18); }
        if (Pico8.Btnp(1)) { TabToggle = PcraftServices.Loop(TabToggle + 1, 2); Pico8.Sfx(18); }

        List<InventorySlot> activeList = TabToggle == 0 ? ChestItems : PlayerItems;
        List<InventorySlot> otherList = TabToggle == 0 ? PlayerItems : ChestItems;

        if (activeList.Count > 0 && Pico8.Btnp(5))
        {
            InventorySlot item = activeList[Sel];
            activeList.RemoveAt(Sel);
            otherList.Add(item);
            Pico8.Sfx(16);
        }

        if (Pico8.Btnp(4) && !player.Lb4)
        {
            player.CurMenu = null;
            Pico8.Sfx(17);
        }
        return false;
    }

    public void Draw(PlayerEntity player, Level level)
    {
        PcraftServices.DrawChestPanels(this);
    }
}
