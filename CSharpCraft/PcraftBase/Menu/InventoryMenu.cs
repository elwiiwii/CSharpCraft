using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Menu;

internal sealed class InventoryMenu(List<InventorySlot> list) : IMenu
{
    internal List<InventorySlot> List { get; } = list ?? throw new ArgumentNullException(nameof(list));
    internal int Sel { get; set; } = 0;
    internal int Off { get; set; } = 0;

    public bool Update(PlayerEntity player)
    {
        if (List.Count > 0)
        {
            if (Pico8.Btnp(3)) { Sel += 1; Pico8.Sfx(18); }
            if (Pico8.Btnp(2)) { Sel -= 1; Pico8.Sfx(18); }
            Sel = PcraftServices.Loop(Sel, List.Count);

            if (Pico8.Btnp(5) && !player.Lb5)
            {
                player.CurItem = List[Sel];
                List.RemoveAt(Sel);
                PcraftServices.AddItemInList(player.Invent, player.CurItem, 0);
                player.CurMenu = null;
                player.Block5  = true;
                Pico8.Sfx(16);
                return false;
            }
        }

        if (Pico8.Btnp(4) && !player.Lb4)
        {
            player.CurMenu = null;
            Pico8.Sfx(17);
        }
        return false;
    }

    public void Draw(PlayerEntity player, Level level) => PcraftServices.DrawInventoryMenu(this);
}
