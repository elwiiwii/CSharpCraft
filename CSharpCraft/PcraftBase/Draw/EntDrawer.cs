using CSharpCraft.PcraftBase.Data;

namespace CSharpCraft.PcraftBase.Draw;

internal static class EntDrawer
{
    internal static void DrawEnt(Level level)
    {
        foreach (Entity e in level.Ent)
        {
            if (e is PlacedItemEntity placed)
            {
                Pico8.Pal();
                if (placed.Type.Pal is not null) PcraftServices.SetPal(placed.Type.Pal);
                Pico8.Spr(placed.Type.BigSpr, e.X.Double - 8, e.Y.Double - 8, 2, 2);
            }
            else if (e is TextPopupEntity popup)
            {
                Pico8.Pal();
                PcraftServices.PrintB(popup.TextValue.Double.ToString("0"), e.X.Double - 2, e.Y.Double - 4, popup.TextColor, 1);
            }
            else if (e is DroppedItemEntity dropped)
            {
                Pico8.Pal();
                if (dropped.Type.Pal is not null) PcraftServices.SetPal(dropped.Type.Pal);
                if (dropped.Timer < 45 && dropped.Timer % 4 > 2)
                {
                    for (int i = 0; i <= 15; i++)
                        Pico8.Palt(i, true);
                }
                Pico8.Spr(dropped.Type.Spr, e.X.Double - 4, e.Y.Double - 4);
            }
        }
    }
}
