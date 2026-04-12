#nullable enable

using CSharpCraft.Pcraft.Data;
using CSharpCraft.Pcraft.Draw;
using CSharpCraft.Pcraft.Inventory;
using PSharp8;

namespace CSharpCraft.Pcraft.Menu;

internal sealed class InventoryMenu(List<ItemStack> list) : IMenu
{
    internal List<ItemStack> List { get; } = list ?? throw new ArgumentNullException(nameof(list));
    internal int Sel { get; set; } = 0;
    internal int Off { get; set; } = 0;

    public void Update(WorldState state, PcraftGame game)
    {
        if (List.Count > 0)
        {
            if (Pico8.Btnp(3)) { Sel += 1; Pico8.Sfx(18); }
            if (Pico8.Btnp(2)) { Sel -= 1; Pico8.Sfx(18); }
            Sel = InventoryOps.Loop(Sel, List.Count);

            if (Pico8.Btnp(5) && !state.Lb5)
            {
                state.CurItem = List[Sel];
                state.CurMenu = null;
                state.Block5  = true;
                Pico8.Sfx(16);
                return;
            }
        }

        if (Pico8.Btnp(4) && !state.Lb4)
        {
            state.CurMenu = null;
            Pico8.Sfx(17);
        }
    }

    public void Draw(WorldState state) => MenuOverlayDrawer.DrawInventoryMenu(this);
}
